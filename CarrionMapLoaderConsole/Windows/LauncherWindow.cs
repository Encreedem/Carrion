using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CarrionManagerConsole
{
	class LauncherWindow : IWindow
	{
		private readonly GUI.Label title;
		private readonly GUI.ListMenu mapsMenu;
		private readonly GUI.Box menuDetailsSeparator;
		private readonly GUI.TextBox detailsTextBox;
		private readonly GUI.ListBox startupLevelList;
		private readonly GUI.Box menuCommandSeparator;
		private readonly GUI.SelectionPrompt selectionPrompt;
		private readonly GUI.Box commandTextSeparator;
		private readonly GUI.TextBox logTextBox;

		private bool windowQuit;

		public LauncherWindow() {
			int width = Console.WindowWidth;
			int halfWidth = width / 2;
			int height = Console.WindowHeight;
			title = new GUI.Label(0, 0, width, 1, MenuColor.LauncherWindowTitleBG, MenuColor.LauncherWindowTitleFG, Text.LauncherWindowTitle);
			mapsMenu = new GUI.ListMenu(0, 1, halfWidth, height - 10, 1, MenuColor.ContentBG, MenuColor.ContentFG);
			menuDetailsSeparator = new GUI.Box(halfWidth - 1, 1, 1, height - 10, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			detailsTextBox = new GUI.TextBox(halfWidth, 1, halfWidth - 1, height - 10, MenuColor.ContentBG, MenuColor.ContentFG);
			startupLevelList = new GUI.ListBox(halfWidth, 1, halfWidth - 1, height - 10, MenuColor.ContentBG, MenuColor.ContentFG);
			menuCommandSeparator = new GUI.Box(0, height - 9, width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			selectionPrompt = new GUI.SelectionPrompt(0, height - 8, width, 1, MenuColor.ContentBG, MenuColor.ContentFG);
			commandTextSeparator = new GUI.Box(0, height - 7, width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			logTextBox = new GUI.TextBox(0, height - 6, width, 4, MenuColor.ContentBG, MenuColor.ContentFG);
		}

		public void LaunchGame(Map customMap) {
			var args = Program.LaunchGameArgument;
			if (customMap != null) {
				args += string.Format(Program.CustomLevelArgument, customMap.startupLevel);
			}
			var info = new ProcessStartInfo(Program.steamPath, args);
			logTextBox.WriteLine(String.Format("Started: steam.exe {0}", info.Arguments));
			logTextBox.WriteLine("This might take a few seconds...");
			Process.Start(info);
		}

		public void SetStartupLevel(Map map) {
			var levelsWithoutExtension = new string[map.Levels.Length];
			for (int i = 0; i < map.Levels.Length; ++i) {
				levelsWithoutExtension[i] = Program.RemoveLevelExtension(map.Levels[i]);
			}
			startupLevelList.SetContent(levelsWithoutExtension);
			startupLevelList.Clear();
			startupLevelList.Draw();
			startupLevelList.Select(0);

			GUI.Selection selection = startupLevelList.PromptInput();
			switch (selection.command) {
				case Properties.Command.Confirm:
					map.startupLevel = selection.Text;
					Program.SaveInstalledMaps();
					break;
				case Properties.Command.Cancel:
					break;
			}

			startupLevelList.Clear();
		}

		public void Show() {
			var launchableMaps = new string[Program.installedMaps.Count + 1];
			launchableMaps[0] = Text.MainGame;
			Program.MapListToStringArray(Program.installedMaps).CopyTo(launchableMaps, 1);
			mapsMenu.SetColumnContent(0, Text.InstalledMaps, launchableMaps);
			mapsMenu.SelectFirstItem();
			GUI.Reset();
			DrawAll();
			windowQuit = false;
			while (!windowQuit) {
				GUI.Selection selection = mapsMenu.PromptInput();
				switch (selection.command) {
					case Properties.Command.Confirm:
						if (selection.list.IsEmpty) {
							return;
						}
						logTextBox.Clear();
						var map = Program.mapInstallerWindow.FindInstalledMap(selection.Text);
						var options = new GUI.SelectionPrompt.Options() { cancel = true };
						if (selection.rowIndex == 0) { // main map
							options.disabledItems = new int[] { 1 };
							options.index = 0;
						}
						else if (map.startupLevel == null) {
							options.disabledItems = new int[] { 0 };
							options.index = 1;
						}
						int response = selectionPrompt.PromptSelection(new string[] { Text.Launch, Text.SetStartupLevel }, options);
						switch (response) {
							case 0:
								if (selection.rowIndex == 0) {
									LaunchGame(null);
								}
								else {
									LaunchGame(map);
								}
								break;
							case 1:
								SetStartupLevel(map);
								break;
						}
						break;
					case Properties.Command.Cancel:
						Program.currentWindow = Program.navigationWindow;
						this.windowQuit = true;
						break;
				}

				if (!windowQuit) {
					windowQuit = Program.navigationWindow.ChangeWindow(selection.command);
				}
			}
		}

		private void DrawAll() {
			//GUI.Reset();
			title.Draw();
			mapsMenu.Draw();
			menuDetailsSeparator.Clear();
			detailsTextBox.Draw();
			menuCommandSeparator.Clear();
			selectionPrompt.Clear();
			commandTextSeparator.Clear();
			logTextBox.Draw();
			Program.controlsLabel.Draw();
		}
	}
}
