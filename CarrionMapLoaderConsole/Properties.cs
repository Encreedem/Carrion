using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	class Properties
	{
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
			NavigationWindowTitleBG = ConsoleColor.White, NavigationWindowTitleFG = ConsoleColor.Black,
			LauncherWindowTitleBG = ConsoleColor.DarkGreen, LauncherWindowTitleFG = ConsoleColor.White;
	}

	public class Text
	{
		public const string
			SelectedLeftSymbol = "[", SelectedRightSymbol = "]",
			HighlightedLeftSymbol = "[", HighlightedRightSymbol = "]",
			UnselectedLeftSymbol = " ", UnselectedRightSymbol = " ",

			Cancel = "Cancel",
			Install = "Install",
			Uninstall = "Uninstall",
			Reinstall = "Reinstall",
			Overwrite = "Overwrite",
			InstalledMaps = "Installed Maps",
			AvailableMaps = "Available Custom Maps",
			SetStartupLevel = "Set Startup Level",
			Launch = "Launch",
			MainGame = "Main Game",
			Unknown = "Unknown",
			BackedUpMap = "Backed up map \"{0}\"",

			NavigationWindowTitle = "Navigation Window",
			LauncherWindowTitle = "Launcher",
			MapInstallerWindowTitle = "Install/Uninstall Custom Maps",
			SaveManagerTitle = "Save File Manager",

			ConfigSteamPathDescription = "SteamPath:      The full path to steam.exe",
			ConfigGamePathDescription = "GamePath:       The full path to the folder containing carrion.exe (i.e. where Carrion is installed)",
			ConfigCustomMapsPathDescription = "CustomMapsPath: The full path to the folder containing all your extracted custom maps",
			ConfigAppDataPathDescription = "AppDataPath:    The full path to the folder containing Carrion's saves folder and settings.json",

			DefaultControls = "Arrow Keys/PgUp/PgDown: Navigate    Enter/Space: Confirm    Escape: Back/Cancel    1-9: Switch Window";
	}
}
