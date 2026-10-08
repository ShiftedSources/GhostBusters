using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GhostEye.Backup;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.App.Services;

public static class HistoryActions
{
	public static async Task<bool> ConfirmAndDeleteAsync(ChangeSet changeSet)
	{
		ILocalizer localizer = AppServices.Localizer;
		int active = changeSet.Changes.Count((TweakChangeRecord c) => !c.Reverted);
		if (!(await AppServices.Dialogs.ConfirmAsync(localizer["history.delete.title"], (active > 0) ? localizer.Format("history.delete.bodyActive", changeSet.Title, active) : localizer.Format("history.delete.body", changeSet.Title), localizer["common.delete"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true)))
		{
			return false;
		}
		await AppServices.History.DeleteAsync(changeSet.Id).ConfigureAwait(continueOnCapturedContext: true);
		DeleteBackupFiles(changeSet);
		AppServices.Logger.Info($"History entry {changeSet.Id} deleted ({active} changes still applied).");
		BackgroundAgent.RefreshAfterHistoryChange();
		return true;
	}

	public static async Task<bool> ConfirmAndClearAsync(IReadOnlyList<ChangeSet> all)
	{
		if (all.Count == 0)
		{
			return false;
		}
		ILocalizer localizer = AppServices.Localizer;
		int active = all.Sum((ChangeSet c) => c.Changes.Count((TweakChangeRecord r) => !r.Reverted));
		if (!(await AppServices.Dialogs.ConfirmAsync(localizer["history.clear.title"], (active > 0) ? localizer.Format("history.clear.bodyActive", all.Count, active) : localizer.Format("history.clear.body", all.Count), localizer["history.clear"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true)))
		{
			return false;
		}
		await AppServices.History.ClearAsync().ConfigureAwait(continueOnCapturedContext: true);
		foreach (ChangeSet item in all)
		{
			DeleteBackupFiles(item);
		}
		AppServices.Logger.Info($"History cleared: {all.Count} entries, {active} changes still applied.");
		BackgroundAgent.RefreshAfterHistoryChange();
		return true;
	}

	private static void DeleteBackupFiles(ChangeSet changeSet)
	{
		string value = Path.GetFullPath(AppPaths.Backups) + Path.DirectorySeparatorChar;
		foreach (string registryBackupFile in changeSet.RegistryBackupFiles)
		{
			try
			{
				string fullPath = Path.GetFullPath(registryBackupFile);
				if (fullPath.StartsWith(value, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
				{
					File.Delete(fullPath);
				}
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException) ? true : false)
			{
				AppServices.Logger.Info("Could not delete backup file " + registryBackupFile + ": " + ex.Message);
			}
		}
	}
}
