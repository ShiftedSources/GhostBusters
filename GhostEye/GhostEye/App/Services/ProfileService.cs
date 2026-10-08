using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.Services;

public sealed class ProfileService(IOptimizationEngine engine, ISystemOptimizerService services, AppSettingsService settings, ILocalizer loc, IAppLogger logger)
{
	private const long MaxFileBytes = 1048576L;

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	public async Task<OptimizationProfile> CaptureCurrentAsync(string name, CancellationToken ct = default(CancellationToken))
	{
		IReadOnlyList<string> active = await AppServices.Drift.GetActiveTweakIdsAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		List<string> list = await Task.Run(() => (from s in services.GetServices()
			where s.IsInstalled && s.IsOptimized
			select s.Definition.Name).ToList(), ct).ConfigureAwait(continueOnCapturedContext: false);
		return new OptimizationProfile
		{
			Name = name,
			Tweaks = active,
			Services = list,
			DnsPrimary = (active.Contains("network.dns-optimized") ? settings.Current.PrimaryDns : null),
			DnsSecondary = (active.Contains("network.dns-optimized") ? settings.Current.SecondaryDns : null)
		};
	}

	public void Save(OptimizationProfile profile, string path)
	{
		File.WriteAllText(path, JsonSerializer.Serialize(profile, Options));
	}

	public OptimizationProfile Load(string path, out string error)
	{
		error = string.Empty;
		try
		{
			if (new FileInfo(path).Length > 1048576)
			{
				error = loc["profile.error.invalid"];
				return new OptimizationProfile();
			}
			OptimizationProfile optimizationProfile = JsonSerializer.Deserialize<OptimizationProfile>(File.ReadAllText(path));
			if ((object)optimizationProfile == null || optimizationProfile.Format != "ghosteye-profile")
			{
				error = loc["profile.error.invalid"];
				return new OptimizationProfile();
			}
			if (optimizationProfile.Version > 1)
			{
				error = loc["profile.error.newer"];
				return new OptimizationProfile();
			}
			return optimizationProfile with
			{
				Tweaks = optimizationProfile.Tweaks.Where((string id) => (object)TweakCatalog.Find(id) != null).Distinct(StringComparer.Ordinal).ToList(),
				Services = optimizationProfile.Services.Where((string n) => ServiceCatalog.Services.Any((ServiceTweakDefinition s) => s.Name == n)).Distinct(StringComparer.Ordinal).ToList()
			};
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? true : false)
		{
			logger.Error("Profile '" + path + "' could not be read", ex);
			error = loc["profile.error.invalid"];
			return new OptimizationProfile();
		}
	}

	public async Task<ProfileApplyReport> ApplyAsync(OptimizationProfile profile, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		List<string> errors = new List<string>();
		string primary = profile.DnsPrimary;
		if (primary != null && primary.Length > 0)
		{
			settings.Update((AppSettings s) => s with
			{
				PrimaryDns = primary,
				SecondaryDns = (profile.DnsSecondary ?? s.SecondaryDns)
			});
		}
		IReadOnlyDictionary<string, TweakApplicability> applicability = await engine.GetApplicabilityAsync(profile.Tweaks, ct).ConfigureAwait(continueOnCapturedContext: false);
		IReadOnlyDictionary<string, bool> applied = await engine.GetAppliedStatesAsync(profile.Tweaks, ct).ConfigureAwait(continueOnCapturedContext: false);
		List<string> source = (from id in profile.Tweaks
			where applicability.TryGetValue(id, out TweakApplicability value) && value.IsApplicable
			where !applied.GetValueOrDefault(id)
			select id).ToList();
		int tweaksOk = 0;
		int tweaksFailed = 0;
		bool flag = true;
		foreach (IGrouping<TweakCategory, string> item in from id in source
			group id by TweakCatalog.Get(id).Category)
		{
			ApplyOutcome applyOutcome = await engine.ApplyAsync(new ApplyRequest
			{
				TweakIds = item.ToList(),
				Title = loc.Format("profile.historyTitle", string.IsNullOrWhiteSpace(profile.Name) ? "—" : profile.Name),
				Category = item.Key,
				CreateRestorePoint = (flag && settings.Current.AutoRestorePoint)
			}, progress, ct).ConfigureAwait(continueOnCapturedContext: false);
			flag = false;
			tweaksOk += applyOutcome.SucceededCount;
			tweaksFailed += applyOutcome.FailedCount;
			errors.AddRange(from r in applyOutcome.Results
				where !r.Success
				select r.TweakId + ": " + r.Message);
		}
		int servicesOk = 0;
		int servicesFailed = 0;
		foreach (OptionalServiceState state in (await Task.Run(() => services.GetServices(), ct).ConfigureAwait(continueOnCapturedContext: false)).Where((OptionalServiceState s) => s.IsInstalled && !s.IsOptimized && profile.Services.Contains(s.Definition.Name)))
		{
			progress?.Report(state.DisplayName);
			OperationResult operationResult = await Task.Run(() => services.SetServiceOptimized(state.Definition, optimized: true), ct).ConfigureAwait(continueOnCapturedContext: false);
			if (operationResult.Success)
			{
				servicesOk++;
				continue;
			}
			servicesFailed++;
			errors.Add(state.Definition.Name + ": " + operationResult.Error);
		}
		logger.Info($"Profile '{profile.Name}' applied: tweaks {tweaksOk} ok / {tweaksFailed} failed, services {servicesOk} ok / {servicesFailed} failed.");
		return new ProfileApplyReport(tweaksOk, tweaksFailed, servicesOk, servicesFailed, errors);
	}
}
