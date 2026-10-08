using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Tweaks;

public abstract class RegistryTweakBase : ITweakAction
{
	public string Id { get; }

	protected IRegistryService Registry { get; }

	protected ILocalizer Loc { get; }

	protected abstract IReadOnlyList<RegistryWrite> Writes { get; }

	protected virtual string SuccessKey => "result.applied";

	protected virtual bool RequiresRestart => false;

	protected RegistryTweakBase(string id, IRegistryService registry, ILocalizer loc)
	{
		Id = id;
		Registry = registry;
		Loc = loc;
	}

	public virtual Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(TweakApplicability.Applicable);
	}

	public Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(RegistrySnapshotCodec.Encode(Capture()));
	}

	public Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(Writes.All(IsAtDesiredValue));
	}

	public Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		string previousValue = RegistrySnapshotCodec.Encode(Capture());
		if (!TryGuarded(() =>
		{
			foreach (RegistryWrite write in Writes)
			{
				Registry.WriteValue(write.Key, write.Name, write.DesiredValue, write.Kind);
			}
			AfterApply();
		}, out string error))
		{
			return Task.FromResult(TweakResult.Fail(Id, error));
		}
		return Task.FromResult(TweakResult.Ok(Id, previousValue, Loc[SuccessKey], RequiresRestart));
	}

	public Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		IReadOnlyList<RegistryValueSnapshot> snapshots = RegistrySnapshotCodec.TryDecode(previousValue);
		if (snapshots == null)
		{
			return Task.FromResult(TweakResult.Fail(Id, Loc["result.noSnapshot"]));
		}
		if (!TryGuarded(() =>
		{
			foreach (RegistryValueSnapshot item in snapshots.Reverse())
			{
				if (!item.Existed || item.Value == null)
				{
					Registry.DeleteValue(item.Key, item.Name);
				}
				else
				{
					RegistryValueKind kind = RegistrySnapshotCodec.ParseKind(item.Kind);
					Registry.WriteValue(item.Key, item.Name, RegistrySnapshotCodec.DecodeValue(item.Value, kind), kind);
				}
			}
			AfterRevert();
		}, out string error))
		{
			return Task.FromResult(TweakResult.Fail(Id, error));
		}
		return Task.FromResult(TweakResult.Ok(Id, previousValue, Loc["result.reverted"], RequiresRestart));
	}

	protected virtual void AfterApply()
	{
	}

	protected virtual void AfterRevert()
	{
		AfterApply();
	}

	private IReadOnlyList<RegistryValueSnapshot> Capture()
	{
		return Writes.Select((RegistryWrite write) =>
		{
			object obj = Registry.ReadValue(write.Key, write.Name);
			string kind = ((obj == null) ? null : Registry.GetValueKind(write.Key, write.Name)?.ToString());
			return new RegistryValueSnapshot
			{
				Key = write.Key,
				Name = write.Name,
				Kind = kind,
				Value = RegistrySnapshotCodec.EncodeValue(obj)
			};
		}).ToList();
	}

	private bool IsAtDesiredValue(RegistryWrite write)
	{
		object obj = Registry.ReadValue(write.Key, write.Name);
		if (obj != null)
		{
			return Matches(write, obj);
		}
		return false;
	}

	protected virtual bool Matches(RegistryWrite write, object current)
	{
		return RegistrySnapshotCodec.EncodeValue(current) == RegistrySnapshotCodec.EncodeValue(write.DesiredValue);
	}

	protected bool TryGuarded(Action operation, out string error)
	{
		try
		{
			operation();
			error = string.Empty;
			return true;
		}
		catch (UnauthorizedAccessException)
		{
			error = Loc["result.registry.accessDenied"];
			return false;
		}
		catch (SecurityException)
		{
			error = Loc["result.registry.insufficient"];
			return false;
		}
	}
}
