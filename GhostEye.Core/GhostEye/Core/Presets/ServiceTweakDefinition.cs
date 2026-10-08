using GhostEye.Core.Models;

namespace GhostEye.Core.Presets;

public sealed record ServiceTweakDefinition(string Name, RiskLevel Risk, bool Disable)
{
	public string NameKey => "svc." + Name + ".name";

	public string DescriptionKey => "svc." + Name + ".desc";
}
