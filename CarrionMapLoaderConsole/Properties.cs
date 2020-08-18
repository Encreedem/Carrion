﻿using System;
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
			MapInstallerWindowTitleBG = ConsoleColor.DarkCyan, MapInstallerWindowTitleFG = ConsoleColor.White;
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
			Continue = "Continue",
			NewGame = "New Game",
			MainGame = "Main Game",
			Unknown = "Unknown",
			BackedUpMap = "Backed up map \"{0}\"",
			PreparingSaveFile = "Preparing save files...",
			BackUpCurrentSave = "Backing up current save files...",

			NavigationWindowTitle = "Navigation Window",
			LauncherWindowTitle = "Launcher",
			MapInstallerWindowTitle = "Map Installer",
			SaveManagerTitle = "Save File Manager",

			SaveInfoMapName = "MapName",

			//Settings
			True = "true",
			False = "false",
			ConfigLaunchMethod = "LaunchMethod",
			ConfigLaunchMethodSteam = "steam",
			ConfigLaunchMethodDirectly = "directly",
			ConfigSteamPath = "SteamPath",
			ConfigGamePath = "GamePath",
			ConfigCustomMapsPath = "CustomMapsPath",
			ConfigAppDataPath = "AppDataPath",
			ConfigManageSaves = "ManageSaves",
			ConfigLaunchMethodDescription = "LaunchMethod:    If Carrion is installed via steam, enter \"Steam\", otherwise \"Directly\".",
			ConfigSteamPathDescription = "SteamPath:      The full path to steam.exe",
			ConfigGamePathDescription = "GamePath:       The full path to the folder containing carrion.exe (i.e. where Carrion is installed)",
			ConfigCustomMapsPathDescription = "CustomMapsPath: The full path to the folder containing all your extracted custom maps",
			ConfigAppDataPathDescription = "AppDataPath:    The full path to the folder containing Carrion's saves folder and settings.json",
			ConfigManageSavesDescription = "ManageSaves:    Whether saves should automatically be backed up and loaded when you launch a map.",

			DefaultControls = "Arrow Keys/PgUp/PgDown: Navigate    Enter/Space: Confirm    Escape: Back/Cancel    1-9: Switch Window",
			DefaultControlsShort = "Arrow Keys: Navigate   Enter: Confirm   Escape: Back/Cancel   1-9: Switch Window";
	}
}