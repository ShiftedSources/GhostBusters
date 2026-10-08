using GhostEye.App.Services;
using GhostEye.Infrastructure.Health;

namespace GhostEye.App.ViewModels;

public sealed class SecurityCheckViewModel(SecurityCheck check)
{
	public SecurityCheck Check { get; } = check;

	public string Name => AppServices.Localizer["health.security." + Check.Key + ".name"];

	public string Description => AppServices.Localizer["health.security." + Check.Key + ".desc"];

	public HealthLevel Level => Check.Level;

	public string LevelLabel => AppServices.Localizer[$"health.level.{Check.Level}"];

	public string Detail => Check.Detail;
}
