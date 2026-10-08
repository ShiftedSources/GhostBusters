using GhostEye.Core.Presets;

namespace GhostEye.Infrastructure.Apps;

public sealed record StoreApp(BloatwareDefinition Definition, string PackageFullName, string DisplayName, string Version);
