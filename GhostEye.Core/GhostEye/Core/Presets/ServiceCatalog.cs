using System.Collections.Generic;
using GhostEye.Core.Models;

namespace GhostEye.Core.Presets;

public static class ServiceCatalog
{
	public static IReadOnlyList<ServiceTweakDefinition> Services { get; } = new _003C_003Ez__ReadOnlyArray<ServiceTweakDefinition>(new ServiceTweakDefinition[17]
	{
		new ServiceTweakDefinition("DiagTrack", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("dmwappushservice", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("RetailDemo", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("MapsBroker", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("Fax", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("RemoteRegistry", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("WerSvc", RiskLevel.Low, Disable: false),
		new ServiceTweakDefinition("wisvc", RiskLevel.Low, Disable: true),
		new ServiceTweakDefinition("WpcMonSvc", RiskLevel.Low, Disable: false),
		new ServiceTweakDefinition("lfsvc", RiskLevel.Medium, Disable: false),
		new ServiceTweakDefinition("PhoneSvc", RiskLevel.Medium, Disable: false),
		new ServiceTweakDefinition("XblAuthManager", RiskLevel.Medium, Disable: false),
		new ServiceTweakDefinition("XblGameSave", RiskLevel.Medium, Disable: false),
		new ServiceTweakDefinition("XboxNetApiSvc", RiskLevel.Medium, Disable: false),
		new ServiceTweakDefinition("SysMain", RiskLevel.Medium, Disable: true),
		new ServiceTweakDefinition("WSearch", RiskLevel.High, Disable: true),
		new ServiceTweakDefinition("Spooler", RiskLevel.High, Disable: true)
	});

	public static IReadOnlyList<TaskTweakDefinition> Tasks { get; } = new _003C_003Ez__ReadOnlyArray<TaskTweakDefinition>(new TaskTweakDefinition[10]
	{
		new TaskTweakDefinition("\\Microsoft\\Windows\\Application Experience\\Microsoft Compatibility Appraiser", "appraiser", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Application Experience\\ProgramDataUpdater", "programDataUpdater", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator", "ceipConsolidator", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip", "usbCeip", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Autochk\\Proxy", "autochkProxy", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector", "diskDiagnostic", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Feedback\\Siuf\\DmClient", "siufDmClient", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload", "siufScenario", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Windows Error Reporting\\QueueReporting", "werQueue", RiskLevel.Low),
		new TaskTweakDefinition("\\Microsoft\\Windows\\Maps\\MapsUpdateTask", "mapsUpdate", RiskLevel.Low)
	});
}
