namespace GhostEye.Infrastructure.Tweaks;

public interface IDnsPreferenceProvider
{
	string PrimaryDns { get; }

	string SecondaryDns { get; }
}
