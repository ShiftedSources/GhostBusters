using GhostEye.Core.Models;

namespace GhostEye.Core.Presets;

public sealed record TaskTweakDefinition(string Path, string Key, RiskLevel Risk)
{
	public string NameKey => "task." + Key + ".name";

	public string DescriptionKey => "task." + Key + ".desc";
}
