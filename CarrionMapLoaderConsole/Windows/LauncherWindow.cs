using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CarrionManagerConsole
{
	class LauncherWindow : DefaultWindow
	{
		public LauncherWindow() : base(Text.LauncherWindowTitle, MenuColor.LauncherWindowTitleBG, MenuColor.LauncherWindowTitleFG) {
			LaunchableMapsList = Menu.AddListBox(0, Text.MapInstallerInstalledMapsHeader, true);
			LaunchableMapsList.SelectionChanged += MapSelectionChanged;
			DetailsTextBox = Menu.AddTextBox(1, null);
			StartupLevelList = new GUI.ListBox(DetailsTextBox.Left, DetailsTextBox.Top, DetailsTextBox.Width, DetailsTextBox.Height, MenuColor.ContentBG, MenuColor.ContentFG, true);
		}

		private GUI.ListBox LaunchableMapsList { get; set; }
		private GUI.ListBox StartupLevelList { get; set; }
		private GUI.TextBox DetailsTextBox { get; set; }

		public void LaunchGame(Map customMap) {
			string args = string.Empty;
			ProcessStartInfo info;
			switch (Program.gameLaunchMethod) {
				case Properties.GameLaunchMethod.Directly:
					if (customMap != null) {
						args = string.Format(Program.CustomLevelArgument, customMap.StartupLevel);
					}
					info = new ProcessStartInfo(Program.gameExePath, args);
					LogTextBox.WriteLine(string.Format("Started: carrion.exe {0}", info.Arguments));
					break;
				case Properties.GameLaunchMethod.Steam:
					args = Program.LaunchSteamGameArgument;
					if (customMap != null) {
						args += string.Format(Program.CustomLevelArgument, customMap.StartupLevel);
					}
					info = new ProcessStartInfo(Program.steamPath, args);
					LogTextBox.WriteLine(string.Format("Started: steam.exe {0}", info.Arguments));
					break;
				default:
					throw new Exception(string.Format("LaunchGame ERROR: Invalid launch method \"{0}\"", Program.gameLaunchMethod.ToString()));
			}

			LogTextBox.WriteLine("This might take a few seconds...");
			Process.Start(info);
		}

		public void MapSelectionChanged(object sender, GUI.SelectionChangedEventArgs e) {
			if (e.SelectedItemIndex < 1) {
				DetailsTextBox.ClearContent();
				return;
			}
			var selectedMap = Program.installedMaps[e.SelectedItemIndex - 1];
			DetailsTextBox.WriteLongMapInfo(selectedMap);
		}

		public override void Selected(GUI.Selection selection) {
			if (selection.List.IsEmpty) {
				return;
			}
			LogTextBox.ClearContent();
			LaunchableMapsList.HighlightCurrentItem();
			Map map = null;
			string mapName;
			if (selection.RowIndex == 0) {
				mapName = Text.MainGame;
			} else {
				map = ((GUI.SelectableMap)selection.SelectedItem).Map;
				mapName = map.Name;
			}
			var options = new GUI.SelectionPrompt.Options() { AllowCancel = true };
			if (selection.RowIndex == 0) { // main map
				options.DisabledItems.Add(2); // Disable "Set Startup Level"
			} else {
				if (string.IsNullOrEmpty(map.StartupLevel)) {
					options.DisabledItems.Add(1); // Disable "New Game"
				}
				if (!Program.saveManagerWindow.MapHasSave(map)) {
					options.DisabledItems.Add(0); // Disable "Continue"
				}
			}
			int response = SelectionPrompt.PromptSelection(
				new string[] { Text.Continue, Text.NewGame, Text.SetStartupLevel },
				options);
			switch (response) {
				case 0: // Continue
					try {
						if (Program.manageSaves &&
							Program.saveManagerWindow.GetCurrentSavedMapName() != mapName) {
							LogTextBox.WriteLine(Text.PreparingSaveFile);
							Program.saveManagerWindow.SwapSaves(mapName);
						}
						LaunchGame(null);
					} catch (Exception e) {
						LogTextBox.WriteLine(string.Format(Text.ErrorWithMessage, e.Message));
					}
					break;
				case 1: // New Game
					try {
						if (Program.manageSaves) {
							LogTextBox.WriteLine(Text.BackingUpCurrentSave);
							Program.saveManagerWindow.BackupCurrentSave();
							Program.saveManagerWindow.SetCurrentSave(mapName);
						}
						LaunchGame((selection.RowIndex == 0) ? null : map);
					} catch (Exception e) {
						LogTextBox.WriteLine(string.Format(Text.ErrorWithMessage, e.Message));
					}
					break;
				case 2: // Set Startup Level
					SetStartupLevel(map);
					DetailsTextBox.WriteLongMapInfo(map);
					break;
			}
			LaunchableMapsList.SelectCurrentItem();
		}

		public void SetStartupLevel(Map map) {
			StartupLevelList.SetItems(map.GetLevelsWithoutExtension());
			StartupLevelList.Clear();
			StartupLevelList.Draw();
			StartupLevelList.Select(0);

			GUI.Selection selection = StartupLevelList.PromptSelection();
			switch (selection.Command) {
				case Properties.Command.Confirm:
					map.StartupLevel = selection.Text;
					Program.SaveInstalledMaps();
					break;
				case Properties.Command.Cancel:
					break;
			}

			StartupLevelList.Clear();
		}

		public override void Show() {
			LaunchableMapsList.SetItems(new string[] { Text.MainGame });
			LaunchableMapsList.AddMaps(Program.installedMaps);
			base.Show();
		}
	}
}
