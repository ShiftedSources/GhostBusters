# GhostBusters

**Windows optimization, gaming tools, system diagnostics, and maintenance in one desktop application.**

GhostBusters contains the **GhostEye** application: a Windows desktop utility for understanding your PC, adjusting its configuration, managing background activity, and reclaiming disk space. The application, executable, solution projects, and local data folder currently use the GhostEye name.

Start with a read-only system analysis, review the findings, and choose the changes that suit your machine. The program combines individual tweaks and ready-made presets with change history, registry backups, optional Windows restore points, and tools for measuring the results.

## Contents

- [Highlights](#highlights)
- [Requirements](#requirements)
- [Build and run](#build-and-run)
- [Getting started](#getting-started)
- [Application guide](#application-guide)
- [Optimization presets](#optimization-presets)
- [Gaming and performance measurements](#gaming-and-performance-measurements)
- [Profiles and command-line usage](#profiles-and-command-line-usage)
- [Background operation and maintenance](#background-operation-and-maintenance)
- [History, backups, and recovery](#history-backups-and-recovery)
- [Settings and local data](#settings-and-local-data)
- [Troubleshooting](#troubleshooting)
- [Project structure](#project-structure)
- [Third-party components](#third-party-components)

## Highlights

- **13 analysis probes** covering hardware, resource usage, Windows configuration, startup activity, and networking.
- **39 optimization actions** across performance, gaming, networking, privacy, and the Windows interface.
- **Six presets:** Balanced, Performance, Extreme, Gaming, Laptop, and Safe.
- **Gaming tools:** temporary Game Boost, automatic game detection, and FPS capture with bundled Intel PresentMon.
- **Network tools:** DNS presets and benchmarking, DNS over HTTPS configuration, speed tests, ping, and network repair.
- **Storage tools:** selectable cleanup categories, folder size analysis, large-file discovery, and content-based duplicate detection.
- **System management:** startup programs, installed apps, optional services, scheduled tasks, and software installation or updates through WinGet.
- **Recovery tools:** recorded tweak changes, previous values, registry exports, service backups, and optional restore points.
- **Portable configuration profiles** that can be exported, imported, or applied from a script.
- **Seven interface languages:** English, Italian, German, French, Spanish, Brazilian Portuguese, and Polish.

Feature availability depends on the Windows version, hardware, installed components, and account permissions. The interface checks applicability and shows current states, risk levels, and restart requirements where relevant.

## Requirements

| Component | Requirement |
| --- | --- |
| Platform | Windows desktop, targeting Windows 10 and Windows 11 |
| Architecture | x64 executable and `win-x64` publication |
| Build toolchain | .NET 10 SDK or a compiler supporting C# 14 |
| Application runtime | .NET 8 Windows Desktop Runtime for framework-dependent builds |
| Published executable | The supplied publish profile includes the runtime in a self-contained build |
| Permissions | Standard-user access for browsing and many diagnostics; administrator access for protected system changes and FPS capture |
| Optional software | WinGet for the Software page |
| Connectivity | Needed for package downloads, online network tests, and some Windows repair operations |

The projects explicitly select C# 14 while targeting .NET 8. A .NET 8 SDK alone does not provide the required compiler; see Microsoft's [C# 14 documentation](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14).

The Windows projects use the target framework `net8.0-windows7.0`. That platform suffix is not a promise of Windows 7 compatibility: the application manifest targets Windows 10/11, and .NET 8 does not support Windows 7 or Windows 8.1. Check Microsoft's [Windows installation and supported-platform guidance](https://learn.microsoft.com/en-us/dotnet/core/install/windows) for current OS and runtime support.

## Build and run

Run these commands in PowerShell on Windows.

### Get the source

```powershell
git clone https://github.com/ShiftedSources/GhostBusters.git
cd GhostBusters
```

### Restore and build

```powershell
dotnet restore .\entire-ghosteye.sln
dotnet build .\entire-ghosteye.sln -c Release --no-restore
```

The solution maps the main application's Release configuration to x64 and builds it under:

```text
GhostEye/bin/x64/Release/net8.0-windows7.0/
```

### Run from source

```powershell
dotnet run --project .\GhostEye\GhostEye.csproj -c Release
```

A framework-dependent build requires the .NET 8 Windows Desktop Runtime. Installing only a newer runtime does not necessarily supply that runtime version; the self-contained publish option below bundles it instead.

### Publish a self-contained executable

```powershell
dotnet publish .\GhostEye\GhostEye.csproj -c Release -p:PublishProfile=FolderProfile
.\dist\win-x64\GhostEye.exe
```

The included [publish profile](GhostEye/Properties/PublishProfiles/FolderProfile.pubxml) produces a compressed, self-contained x64 single-file application, includes native libraries for extraction, enables ReadyToRun, and omits debug symbols. Trimming is disabled. Publication can require additional runtime packages to be restored from NuGet.

The executable can be copied from `dist/win-x64/` to another compatible Windows PC. Its settings, logs, and recovery data are stored in the current user's local application data folder rather than beside the executable.

## Getting started

1. Open `GhostEye.exe` and review the **Dashboard**.
2. Run the **System Analyzer** to inspect the current configuration without applying tweaks.
3. Record a starting state in the Dashboard's **Benchmark** panel if you want a before/after comparison.
4. Open **Optimization Center**, choose a preset or individual settings, and review their descriptions, risk labels, and applicability.
5. Apply the selected changes. The application requests elevation when an operation needs administrator rights.
6. Review the outcome and restart Windows if the selected changes require it.
7. Run the analysis or measurements again. Use **Change History** to restore supported recorded settings if you want to undo them.

For a first pass, Safe and Balanced contain narrower selections than Performance or Extreme. Presets are configuration choices; improvements vary with the workload and the machine.

## Application guide

| Page | What it provides |
| --- | --- |
| **Dashboard** | System score from analysis findings, CPU/RAM/GPU information, recent changes, configuration drift notices, and before/after measurements. |
| **System Analyzer** | Read-only probes for CPU, GPU, RAM, storage, power plan, startup programs, background-app settings, services, Windows Update, DNS, TCP settings, adapter power management, and NIC buffers. |
| **Optimization Center** | Presets and individual performance tweaks, with current values, risk levels, applicability, and advanced options. |
| **Gaming** | Gaming configuration tweaks, manual and automatic Game Boost, game library discovery, and FPS measurements. |
| **Network** | DNS selection and benchmarking, TCP/IP and adapter tweaks, speed testing, ping measurements, and network reset. |
| **Privacy** | Controls for Windows telemetry policies, advertising ID, activity history, suggestions, feedback, Copilot, Recall, widgets, Start web search and ads, Edge policies, input personalization, tailored experiences, and location. |
| **Interface** | Classic context menu, visible file extensions, taskbar End Task, opening Explorer to This PC, taskbar alignment, and accessibility shortcut prompts. |
| **Startup Manager** | Inspect startup entries and enable or disable individual programs. |
| **Apps** | Inspect installed programs, estimated sizes and last-use information; run their uninstallers; remove selected preinstalled Store apps for the current account. |
| **Software** | Browse a curated WinGet package catalog, install software, inspect available updates, and upgrade individual packages or all detected upgrades. |
| **Services & Tasks** | Review 17 optional service definitions and 10 scheduled-task definitions, change supported states, and create or restore service configuration backups. |
| **Cleanup** | Scan temporary files and caches, inspect estimated recoverable space, select categories, and delete the selected contents. |
| **Disk Space** | Browse folder sizes and identify duplicate files by size and SHA-256 content fingerprints. |
| **PC Health** | Live resource monitoring, disk-health information, security checks, installed-driver information, vendor download links, and SFC/DISM repair tools. |
| **Performance Tools** | Free memory, inspect heavy processes, end selected programs, flush DNS, and find large files in user folders. |
| **Change History** | Inspect recorded optimization batches, before/after values, and restoration results; delete entries or clear history. |
| **Settings** | Language, appearance, notifications, restore-point preference, Windows startup, background operation, maintenance schedule, profiles, and third-party notices. |

### Performance and Windows configuration

Performance actions include High Performance and Ultimate Performance power plans, foreground scheduling preferences, visual effects, startup cleanup, memory compression, and enabling storage TRIM when it is disabled. Some options are advanced or unavailable on a particular system.

Gaming settings cover Windows Game Mode, Game Bar and background recording, hardware-accelerated GPU scheduling, fullscreen optimization behavior, mouse acceleration settings, and selected networking policies.

Privacy actions configure specific Windows settings and policies. Their effect depends on the Windows edition and build; they do not provide complete anonymity or remove every network connection made by Windows or other applications.

### Networking

Choose from Cloudflare, Google, Quad9, AdGuard, and OpenDNS presets, or configure preferred DNS addresses. The DNS benchmark compares the current resolver with the built-in providers using ordinary DNS queries and reports response times and unanswered queries. It leaves the DNS configuration unchanged until you select and apply a choice.

Additional options include DNS over HTTPS, TCP autotuning, adapter power management, gaming-related network settings, and Delivery Optimization policies. The online speed test uses Cloudflare's speed-test endpoints; ping reports latency, variation, and packet loss for the available targets.

Replacing DNS can affect corporate domains, local names, or VPN resolution. The network reset tool changes system networking and may require a restart. Use these tools when their specific configuration change is appropriate for your connection.

### Cleanup and disk space

Manual cleanup scans categories including:

- User and Windows temporary files.
- Crash dumps, stale logs, and Windows error reports.
- Supported browser caches.
- Windows Update and Delivery Optimization caches.
- Explorer thumbnail caches.
- DirectX and supported NVIDIA/AMD shader caches.
- The Recycle Bin.

The scan reports categories and sizes before deletion. Open browsers, locked files, and insufficient permissions can prevent some items from being cleaned. Shader and thumbnail caches are rebuilt by the applications that use them; clearing shader caches can cause compilation stutter during subsequent game sessions.

The Disk Space page measures folder contents and supports navigating into large folders. Duplicate scanning compares file sizes and SHA-256 fingerprints in user folders, skips AppData, and starts with files of at least 1 MiB. The newest copy is kept by default, and the interface requires at least one copy to remain selected for retention. Selected duplicate copies are moved to the Recycle Bin.

Performance Tools also finds files larger than 500 MB in common user folders. Its file-removal action moves the selected file to the Recycle Bin.

### Apps, startup, and services

The Apps page delegates ordinary program removal to each program's own uninstaller, which may display its own prompts. Last-use estimates come from Windows launch history; an unknown date does not prove that a program is unused. Store-app removal applies to the current account.

Startup Manager exposes individual startup switches. Services & Tasks presents a curated catalog with risk labels, rather than assuming every background component should be disabled. Changes to search, printing, Xbox components, or other optional services can affect the features that depend on them.

The Software page uses the locally installed `winget` command. Its curated catalog covers browsers, gaming, media, utilities, developer tools, and runtimes. Package installation and updates depend on WinGet availability, network access, package sources, and the installers themselves.

### PC health and repair

PC Health checks areas including antivirus registration, firewall state, Windows updates, disk encryption, Secure Boot, and User Account Control. Disk and driver information depends on what Windows and the hardware expose. Driver actions open vendor download pages; the page does not automatically replace every installed driver.

Windows repair actions run the built-in tools:

```text
sfc /scannow
dism /Online /Cleanup-Image /RestoreHealth
```

These are repair operations that can change Windows system files. They require administrator access and can take time to complete.

## Optimization presets

| Preset | Actions | Selection |
| --- | ---: | --- |
| **Balanced** | 5 | High Performance plan, startup cleanup, storage TRIM, Game Mode, and adapter power management. |
| **Performance** | 9 | Balanced-style changes plus foreground scheduling, visual effects, GPU scheduling, and TCP autotuning. |
| **Extreme** | 20 | A broader selection across performance, gaming, network, and privacy; requires additional confirmation. |
| **Gaming** | 10 | Power plan, foreground scheduling, Game Mode, recording/Game Bar controls, GPU scheduling, fullscreen behavior, mouse settings, and networking. |
| **Laptop** | 5 | Visual effects, startup cleanup, storage TRIM, telemetry policies, and Windows suggestions. |
| **Safe** | 5 | Startup cleanup, storage TRIM, Game Mode, advertising ID, and Windows suggestions. |

Counts describe the preset definitions. The number of changes actually applied depends on availability, current state, and individual outcomes. The Laptop preset contains no dedicated battery power-plan action, and the Safe label does not make its changes universally appropriate.

## Gaming and performance measurements

### Manual Game Boost

Game Boost provides a temporary session with selectable actions:

- Switch to the preferred performance power plan.
- Attempt to pause Windows Update activity by stopping relevant running services.
- Close selected supported background programs.
- Free memory.

The boost state is saved locally. **Stop and restore** attempts to restore the previous power plan and restart the services stopped by the boost. You can also choose to reopen the programs it closed. Relaunching a program cannot recover unsaved work or its exact previous session.

### Automatic Game Boost

The game library reads installed-game records from Steam, Epic Games, GOG, Ubisoft Connect, and the Xbox app. Other games can be added by executable, and individual games can be excluded.

When enabled, automatic boost detects a running supported game and stops the boost when that game closes. It uses performance-plan selection and eligible background-program closure; pausing update services requires administrator rights. It deliberately skips automatic memory trimming while the game is loading and excludes known launchers from automatic closure.

Detection and automatic restoration require GhostEye to remain open or running in the notification area.

### FPS capture

The bundled Intel PresentMon executable measures frames presented by a selected running process through Windows event tracing. Capture reports average FPS, low-FPS statistics, and average frame time, and saves measurement summaries for later comparison. This uses an external capture process rather than injecting code into the game.

Run capture with administrator privileges and keep the selected game rendering during the measurement. Repeat the same scene and settings before and after a change for a useful comparison. Capture availability depends on the rendering workload and system permissions.

### Before/after system snapshots

The Dashboard benchmark records boot duration, RAM usage, enabled startup-program count, and sampled CPU usage. Take a baseline, apply your chosen changes, and measure again. Boot duration comes from Windows' recorded boot event, so a meaningful new boot comparison requires a subsequent restart. These measurements describe the observed state rather than a guaranteed performance gain.

## Profiles and command-line usage

Use **Settings → Profiles** to export the active supported tweak and service choices, or import a profile on another machine. DNS preferences are included when the DNS tweak is active. A profile stores configuration choices and metadata rather than copying the source machine's backup history or previous values.

The JSON format uses `format: "ghosteye-profile"` and version `1`. For example:

```json
{
  "format": "ghosteye-profile",
  "version": 1,
  "name": "Desktop configuration",
  "tweaks": [
    "gaming.game-mode",
    "ui.file-extensions"
  ],
  "services": []
}
```

Import validates the format, rejects files larger than 1 MiB and newer unsupported profile versions, filters unknown catalog entries, and avoids reapplying already-active tweaks. Only applicable tweaks and installed eligible services are applied. Results can include partial failures.

### Apply from PowerShell

```powershell
# Apply with a result dialog
Start-Process .\GhostEye.exe -ArgumentList '--apply-profile "C:\Profiles\desktop.json"' -Wait

# Apply without result dialogs and obtain the exit code
$result = Start-Process .\GhostEye.exe `
    -ArgumentList '--apply-profile "C:\Profiles\desktop.json" --silent' `
    -Wait -PassThru
$result.ExitCode
```

Run scripts from an administrator PowerShell session for unattended use. An unpackaged application can request UAC elevation and relaunch itself when necessary. `--silent` suppresses profile result dialogs; it does not bypass UAC.

| Exit code | Meaning |
| --- | --- |
| `0` | Profile operation completed successfully. |
| `1` | One or more attempted changes failed. |
| `2` | Profile could not be read or validated. |
| `4` | Required elevation could not be obtained. |

Profile tweaks use the optimization engine and its history. Service choices are applied through the service-management path; use service backups for restoring an exact earlier service configuration.

## Background operation and maintenance

Enable **Keep running in the background** to leave GhostEye in the notification area when the main window closes. Exit from the tray menu to stop the application. The **Start with Windows** setting controls startup separately.

```powershell
.\GhostEye.exe --tray
```

`--tray` starts with the main window hidden when background operation is enabled.

Automatic maintenance is opt-in. Its initial interval is seven days, with weekly, fortnightly, and monthly choices in the interface. It cleans a narrower set of categories: temporary files, stale logs, crash dumps, and error reports. It excludes browser caches, the Recycle Bin, and update caches from automatic deletion.

The background agent also checks whether previously recorded tweaks have been changed back. When drift is found, the Dashboard lets you reapply those choices or keep the current state. A Windows update is one possible cause; drift detection itself does not establish what changed the setting.

Maintenance and automatic Game Boost operate while the application is running. They are not installed as a separate Windows service.

## History, backups, and recovery

The optimization engine records successful tweak changes in batches, including previous values and readable before/after states when available. Restoration invokes each tweak's restore action in reverse order and reports individual failures. Registry keys listed by selected tweaks can be exported before application, and the default settings enable an attempt to create a Windows restore point before a batch.

Keep these recovery boundaries in mind:

- **Restore points are best-effort.** Windows System Protection, permissions, and system policy can prevent their creation. A failed restore-point attempt does not automatically prevent the tweak batch from proceeding.
- **History is for recorded tweak actions.** App uninstallers, manual cleanup, process termination, Windows repairs, and every other tool are not covered by a universal undo operation.
- **Cleanup deletes files directly.** Emptying the Recycle Bin is permanent. Disk Space and the large-file tool instead move their selected files to the Recycle Bin when possible.
- **Some restore actions intentionally retain a setting.** The storage optimization action keeps TRIM enabled rather than switching it back off.
- **Deleting history does not restore settings.** It removes records and associated registry backup files, reducing the information available for later restoration and drift detection.
- **Services have their own backups.** Create a service configuration backup before changing a set of service states if you need to restore the exact prior configuration.
- **Game Boost has separate saved state.** Use its Stop action to restore the session settings; reopening closed programs cannot restore unsaved documents.

Restore relevant changes before deleting their history or removing the local application data folder.

## Settings and local data

Language changes apply immediately. Settings also control animated particles, notifications, automatic restore-point attempts, Windows startup, background operation, automatic boost, maintenance, and custom games. The interface includes seasonal theme behavior and reduces animations according to its rendering budget.

| Setting | Initial value |
| --- | --- |
| Language | English (`en`) |
| Animated particles | Enabled |
| Notifications | Enabled |
| Automatic restore-point attempts | Enabled |
| Start with Windows | Disabled |
| Keep running in the background | Disabled |
| Automatic maintenance | Disabled; seven-day interval when enabled |
| Automatic Game Boost | Disabled |
| Preferred DNS addresses | `1.1.1.1` and `1.0.0.1`; stored preferences alone do not change Windows DNS |

Application data is stored under:

```text
%LOCALAPPDATA%\GhostEye\
├── settings.json       Preferences, custom games, and scheduling state
├── history.json        Recorded optimization batches
├── baseline.json       Before/after system baseline
├── gameboost.json      Saved Game Boost state when present
├── fps.json            FPS measurement history
├── backups\            Registry, startup, and service backup data
├── logs\               Application and operation logs
├── cache\              Local cache directory
└── tools\              Extracted PresentMon executable
```

The extracted PresentMon binary is checked against the expected SHA-256 hash before use. Raw frame-capture CSV files are created temporarily and cleaned up after capture.

Core preferences and history are local files. Online features contact external services: speed testing uses Cloudflare, DNS benchmarking queries the selected resolvers, ping contacts its target hosts, and WinGet downloads software through its configured sources. Logs and diagnostic records can contain file paths, process names, or configuration details; inspect them before sharing.

## Troubleshooting

| Symptom | What to check |
| --- | --- |
| Build rejects `LangVersion` or newer C# syntax | Run `dotnet --list-sdks` and use a .NET 10 SDK with C# 14 support. |
| A framework-dependent build will not launch | Install the .NET 8 Windows Desktop Runtime for x64, or use the self-contained publication. |
| A tweak is unavailable or already enabled | Review its applicability message and current state. Windows edition, build, hardware, and existing settings affect availability. |
| Changes or repairs fail with access errors | Accept the elevation request or launch from an administrator session. |
| A restore point was not created | Check Windows System Protection and the operation warning or logs. |
| Software installation or update is unavailable | Confirm `winget --version` works and its sources and network connection are available. |
| DNS benchmark gets no responses | Check connectivity and whether DNS queries on UDP port 53 are permitted. |
| A speed test or ping fails | Check network access, firewall rules, VPN behavior, and the target's availability; some hosts do not answer ICMP. |
| FPS capture records no frames | Use administrator rights, select the rendering game process, and keep it active rather than paused or minimized. |
| Automatic boost or maintenance stops after closing the window | Enable background operation and keep the notification-area process running. |
| Cleanup skips files | Close the relevant applications and inspect locked-file or permission messages. |
| An app has an unknown last-use date | Windows launch history does not cover every way of running a program. |
| A restore succeeds only partially | Review the per-action results and logs; some actions cannot reproduce the previous state exactly. |

Open `%LOCALAPPDATA%\GhostEye\logs\` for operation details. Keep the associated history and backups while investigating restoration issues.

## Project structure

| Project | Responsibility |
| --- | --- |
| [GhostEye](GhostEye/GhostEye.csproj) | WPF desktop interface, MVVM view models, navigation, dialogs, tray integration, settings, profiles, and background coordination. |
| [GhostEye.Core](GhostEye.Core/GhostEye.Core.csproj) | Models, abstractions, tweak and preset catalogs, analysis and optimization engines, drift detection, and localization tables. |
| [GhostEye.Infrastructure](GhostEye.Infrastructure/GhostEye.Infrastructure.csproj) | Windows registry and process integration, tweak implementations, probes, networking, cleanup, app/service management, health tools, disk analysis, game discovery, and frame capture. |
| [GhostEye.Backup](GhostEye.Backup/GhostEye.Backup.csproj) | Application data paths, JSON change history, file logging, registry exports, and Windows restore-point integration. |

The desktop project uses WPF and Windows Forms integration, including notification-area functionality, with CommunityToolkit.Mvvm. Windows infrastructure includes System.Management, performance counters, and service-controller APIs.

Generated `bin/` and `obj/` directories, Visual Studio state, `.build-check/`, and `dist/` publication output are excluded by [`.gitignore`](.gitignore). The embedded PresentMon executable, localization files, fonts, icons, and publish profile are project inputs.

## Third-party components

Bundled components include Intel PresentMon, CommunityToolkit.Mvvm, .NET/WPF/Windows Forms components, and the Inter, JetBrains Mono, and Space Grotesk fonts. Their license texts and notices are included in [GhostEye/assets/licenses](GhostEye/assets/licenses/) and are exposed through Settings.

This repository currently has no root project license file. The bundled third-party notices describe their respective components.
