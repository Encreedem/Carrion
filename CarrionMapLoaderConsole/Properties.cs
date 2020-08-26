using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	class Properties
	{
		public enum Alignment
		{
			Horizontal,
			Vertical,
		}

		public enum Command
		{
			// Default
			Confirm,
			Cancel,
			NavigateUp,
			NavigateRight,
			NavigateDown,
			NavigateLeft,
			PageUp,
			PageDown,
			// Navigation
			ShowLauncher,
			ShowNavigationWindow,
			ShowMapInstaller,
			ShowSaveManager,
			ShowBackupsWindow,
		}

		public enum GameLaunchMethod
		{
			Directly,
			Steam,
		}

		public enum HorizontalAlignment
		{
			Left,
			Center,
			Right,
		}

		public enum SelectionStatus
		{
			None,
			Selected,
			Highlighted,
		}
	}

	public class MenuColor
	{
		public const ConsoleColor
			ErrorBG = ConsoleColor.Black, ErrorFG = ConsoleColor.Red,
			EmptyBG = ConsoleColor.Black, EmptyText = ConsoleColor.White,
			MajorHeaderBG = ConsoleColor.Cyan, MajorHeaderFG = ConsoleColor.Black,
			MinorHeaderBG = ConsoleColor.Gray, MinorHeaderFG = ConsoleColor.Black,
			SeparatorBG = ConsoleColor.Gray, SeparatorFG = ConsoleColor.Black,
			ContentBG = ConsoleColor.Black, ContentFG = ConsoleColor.White,
			SelectedBG = ConsoleColor.DarkGray, SelectedText = ConsoleColor.White,
			HighlightBG = ConsoleColor.Black, HighlightText = ConsoleColor.White,
			DisabledBG = ConsoleColor.Black, DisabledFG = ConsoleColor.DarkGray,
			SelectedDisabledBG = ConsoleColor.White, SelectedDisabledFG = ConsoleColor.DarkGray,
			ControlsBG = ConsoleColor.Black, ControlsFG = ConsoleColor.White,
			ScrollBarBG = ConsoleColor.DarkGray, ScrollBarFG = ConsoleColor.White,
			NavigationWindowTitleBG = ConsoleColor.DarkBlue, NavigationWindowTitleFG = ConsoleColor.White,
			LauncherWindowTitleBG = ConsoleColor.DarkGreen, LauncherWindowTitleFG = ConsoleColor.White,
			MapInstallerWindowTitleBG = ConsoleColor.DarkCyan, MapInstallerWindowTitleFG = ConsoleColor.White,
			SaveMangerWindowTitleBG = ConsoleColor.DarkYellow, SaveMangerWindowTitleFG = ConsoleColor.White,
			BackupsWindowTitleBG = ConsoleColor.DarkMagenta, BackupsWindowTitleFG = ConsoleColor.White;
	}

	public class Text
	{
		public const string
			SelectedLeftSymbol = "[", SelectedRightSymbol = "]",
			HighlightedLeftSymbol = "[", HighlightedRightSymbol = "]",
			UnselectedLeftSymbol = " ", UnselectedRightSymbol = " ",

			// Selections
			Enabled = "Enabled",
			Disabled = "Disabled",
			Cancel = "Cancel",
			Install = "Install",
			Uninstall = "Uninstall",
			Reinstall = "Reinstall",
			Overwrite = "Overwrite",
			BackupAndInstall = "Backup & Install",
			SetStartupLevel = "Set Startup Level",
			Launch = "Launch",
			Continue = "Continue",
			NewGame = "New Game",
			MainGame = "Main Game",
			Unknown = "Unknown",

			// Errors
			PressAnyKeyToContinue = "Press any key to continue...",
			PressAnyKeyToQuit = "Press any key to quit...",
			UnexpectedErrorOccured = "An unexpected error occured!",
			InvalidAlignment = "Invalid Alignment!",

			// Saves & Backups
			LoadBackup = "Load Backup",
			LoadedBackup = "Loaded backup \"{0}\".",
			BackUpCurrentSave = "Backup current save",
			ViewBackups = "View backups...",
			ToggleAutoBackups = "Toggle Auto-Backups",
			BackedUpMap = "Backed up map \"{0}\"",
			PreparingSaveFile = "Preparing save files...",
			BackingUpFiles = "Backing up file(s)...",
			BackupFinished = "backed up!",
			BackingUpCurrentMap = "Backing up current map...",
			BackingUpCurrentSave = "Backing up current save files...",
			BackedUpFilesRestored = "Backed up files restored!",
			NoBackedUpSaves = "No backups of save files available.",
			BackedUpLevels = "Backed up Levels",
			BackedUpSaves = "Backed up Saves",
			BackedUpScripts = "Backed up Scripts",
			LevelBackupsCount = "Number of backed up levels:",
			ScriptBackupsCount = "Number of backed up scripts:",

			// Map issues
			MapHasIssuesIndicator = "[!] ",
			ShowIssues = "Show Issues",
			SoManyMoreIssues = "[!] {0} more issues...",
			MapIssueNoLevelsFolder = "Map doesn't contain \"Levels\" folder!",
			MapIssueNoScriptsFolder = "Map doesn't contain \"Scripts\" folder!",
			StartupLevelInvalid = "Startup Level \"{0}\" is invalid!",

			// Window Titles
			NavigationWindowTitle = "Navigation Window",
			NavigationWindowListHeader = "Windows",
			LauncherWindowTitle = "Launcher",
			MapInstallerWindowTitle = "Map Installer",
			SaveManagerWindowTitle = "Save File Manager",
			BackupsWindowTitle = "Backups",

			// Map Installer Window
			MapInstallerInstalledMapsHeader = "Installed Maps",
			MapInstallerAvailableMapsHeader = "Available Custom Maps",
			PromptReinstall = "Map \"{0}\" is already installed. Reinstall?",
			UninstallingMap = "Uninstalling map {0}...",

			// Save Manager Window
			SaveManagerWindowCurrentSave = "Current saved map:",
			SaveManagerWindowBackupsCount = "Number of backups:",
			SaveManagerWindowAutoBackupStatus = "Auto-backup & -load saves:",
			DisabledManageSaves = "Disabled Auto-backup & -loading of save files.",
			EnabledManageSaves = "Enabled Auto-backup & -loading of save files.",

			// Save Info File
			SaveInfoMapName = "MapName",

			// Map Info File
			MapInfoSeparator = " | ",
			MapInfoMapName = "Map: ",
			MapInfoFileMapName = "MapName",
			MapInfoAuthor = "Author: ",
			MapInfoFileAuthorName = "Author",
			MapInfoNoAuthor = "Unknown",
			MapInfoVersion = "Version: ",
			MapInfoFileVersion = "Version",
			MapInfoNoVersion = "-",
			MapInfoShortDescription = "Description: ",
			MapInfoFileShortDescription = "ShortDescription",
			MapInfoLongDescription = "Description: ",
			MapInfoFileLongDescription = "LongDescription",
			MapInfoNoDescription = "",
			MapInfoStartupLevel = "Startup Level: ",
			MapInfoFileStartupLevel = "StartupLevel",
			MapInfoNoStartupLevel = "-not set-",

			//Settings
			True = "true",
			False = "false",
			ConfigLaunchMethod = "LaunchMethod",
			ConfigLaunchMethodSteam = "steam",
			ConfigLaunchMethodDirectly = "directly",
			ConfigSteamPath = "SteamPath",
			ConfigGamePath = "GamePath",
			ConfigBackupsPath = "BackupsPath",
			ConfigCustomMapsPath = "CustomMapsPath",
			ConfigAppDataPath = "AppDataPath",
			ConfigZippedMapsPath = "ZippedMapsPath",
			ConfigManageSaves = "ManageSaves",
			ConfigLaunchMethodDescription = "LaunchMethod:   If Carrion is installed via steam, enter \"Steam\", otherwise \"Directly\".",
			ConfigSteamPathDescription = "SteamPath:      The full path to steam.exe",
			ConfigGamePathDescription = "GamePath:       The full path to the folder containing carrion.exe (i.e. where Carrion is installed)",
			ConfigBackupsPathDescription = "BackupsPath:    The full path to the folder where saves and files will be backed up.",
			ConfigCustomMapsPathDescription = "CustomMapsPath: The full path to the folder containing all your extracted custom maps",
			ConfigAppDataPathDescription = "AppDataPath:    The full path to the folder containing Carrion's saves folder and settings.json",
			ConfigZippedMapsPathDescription = "ZippedMapsPath: The full path to the folder where this program will look for zipped maps.",
			ConfigManageSavesDescription = "ManageSaves:    Whether saves should automatically be backed up and loaded when you launch a map.",
			ConfigInvalidValue = "ERROR: Setting \"{0}\" has invalid value \"{1}\"!",
			ConfigInvalidDirectoryPath = "ERROR: Setting \"{0}\": Path is not a valid directory or does not exist:\n{1}",
			ConfigInvalidFilePath = "ERROR: Setting \"{0}\": Path is not a valid file or does not exist:\n{1}",
			ConfigAllowedValues = "Allowed values:",
			OneOrMoreSettingsAreMissing = "One or more settings are missing:",
			SettingsInvalid = "One or more settings are invalid! See previous messages for details.",
			SettingsCouldNotBeLoaded = "Settings could not be loaded! See previous messages for details.",

			// Path
			PathCurrentDirectoryIndicator = @".\",
			PathUserDirectoryIndicator = @"[user]\",

			// Controls
			DefaultControls = "Arrow Keys/PgUp/PgDown: Navigate    Enter/Space: Confirm    Escape: Back/Cancel    1-9: Switch Window",
			DefaultControlsShort = "Arrow Keys: Navigate   Enter: Confirm   Escape: Back/Cancel   1-9: Switch Window";
	}
}
