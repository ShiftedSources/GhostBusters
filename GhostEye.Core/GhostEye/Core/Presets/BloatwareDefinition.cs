namespace GhostEye.Core.Presets;

public sealed record BloatwareDefinition(string PackageName, string Key)
{
	public string DescriptionKey => "bloat." + Key + ".desc";
}
