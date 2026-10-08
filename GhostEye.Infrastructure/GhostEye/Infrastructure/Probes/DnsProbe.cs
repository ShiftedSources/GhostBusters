using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.Infrastructure.Probes;

public sealed class DnsProbe(ILocalizer loc) : IAnalysisProbe
{
	public string Id => "net.dns";

	public AnalysisGroup Group => AnalysisGroup.Network;

	public async Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		List<string> list = (from a in (from n in NetworkInterface.GetAllNetworkInterfaces()
				where n.OperationalStatus == OperationalStatus.Up
				where n.NetworkInterfaceType != NetworkInterfaceType.Loopback
				select n).SelectMany((NetworkInterface n) => n.GetIPProperties().DnsAddresses)
			where a.AddressFamily == AddressFamily.InterNetwork
			select a.ToString()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		if (list.Count == 0)
		{
			return new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.dns.name"],
				Description = loc["probe.dns.none"],
				Status = AnalysisStatus.Warning
			};
		}
		string primary = list[0];
		long? num = await MeasureLatencyAsync(primary, ct).ConfigureAwait(continueOnCapturedContext: false);
		bool flag = primary.StartsWith("192.168.", StringComparison.Ordinal) || primary.StartsWith("10.", StringComparison.Ordinal) || primary.StartsWith("172.16.", StringComparison.Ordinal);
		long? num2 = num;
		AnalysisStatus analysisStatus = ((!num2.HasValue) ? AnalysisStatus.Warning : ((num2.GetValueOrDefault() > 60) ? AnalysisStatus.Warning : (flag ? AnalysisStatus.Warning : AnalysisStatus.Ok)));
		AnalysisStatus analysisStatus2 = analysisStatus;
		num2 = num;
		string text;
		if (num2.HasValue)
		{
			if (num2.GetValueOrDefault() <= 60)
			{
				text = ((!flag) ? loc.Format("probe.dns.ok", primary, num) : loc.Format("probe.dns.router", primary));
			}
			else
			{
				text = loc.Format("probe.dns.high", primary, num);
			}
		}
		else
		{
			text = loc.Format("probe.dns.noPing", primary);
		}
		string description = text;
		return new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.dns.name"],
			Measurement = ((!num.HasValue) ? loc["status.none"] : $"{num} ms"),
			Status = analysisStatus2,
			Description = description,
			SuggestedTweakId = ((analysisStatus2 == AnalysisStatus.Ok) ? null : "network.dns-optimized")
		};
	}

	private static async Task<long?> MeasureLatencyAsync(string address, CancellationToken ct)
	{
		List<long> samples = new List<long>();
		using Ping ping = new Ping();
		for (int i = 0; i < 3; i++)
		{
			ct.ThrowIfCancellationRequested();
			try
			{
				PingReply pingReply = await ping.SendPingAsync(address, 1000).ConfigureAwait(continueOnCapturedContext: false);
				if (pingReply.Status == IPStatus.Success)
				{
					samples.Add(pingReply.RoundtripTime);
				}
			}
			catch (PingException)
			{
			}
		}
		if (samples.Count == 0)
		{
			return null;
		}
		samples.Sort();
		return samples[samples.Count / 2];
	}
}
