using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection.Emit;
using System.Text;

namespace CarrionManagerConsole
{
	class MapInstallerWindow : IWindow
	{
		private readonly GUI.Label title;
		private readonly GUI.ListMenu mapsMenu;
		private readonly GUI.Box menuCommandSeparator;
		private readonly GUI.SelectionPrompt selectionPrompt;
		private readonly GUI.Box commandTextSeparator;
		private readonly GUI.TextBox textBox;

		private bool windowQuit;

		public const int
			CommandTextLeft = 0, CommandTextTop = 27,
			CommandItemsLeft = 0, CommandItemsTop = 28;

		public MapInstallerWindow() {
			int width = Console.WindowWidth;
			int height = Console.WindowHeight;
			title = new GUI.Label(0, 0, width, 1, MenuColor.MajorHeaderBG, MenuColor.MajorHeaderFG, Text.MapInstallerWindowTitle);
			mapsMenu = new GUI.ListMenu(0, 1, width, height - 10, 2, MenuColor.ContentBG, MenuColor.ContentFG);
			mapsMenu.SetColumnContent(0, Text.InstalledMaps, Program.MapListToStringArray(Program.installedMaps));
			mapsMenu.SetColumnContent(1, Text.AvailableMaps, Program.MapListToStringArray(Program.availableMaps));
			menuCommandSeparator = new GUI.Box(0, height - 9, width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			selectionPrompt = new GUI.SelectionPrompt(0, height - 8, width, 1, MenuColor.ContentBG, MenuColor.ContentFG);
			commandTextSeparator = new GUI.Box(0, height - 7, width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			textBox = new GUI.TextBox(0, height - 6, width, 4, MenuColor.ContentBG, MenuColor.ContentFG);

		}

		public void DrawAll() {
			GUI.Reset();
			title.Draw();
			mapsMenu.Draw();
			menuCommandSeparator.Clear();
			selectionPrompt.Clear();
			commandTextSeparator.Clear();
			textBox.Draw();
			Program.controlsLabel.Draw();
		}

		public void InstallMap(LoadableMap map, bool overwrite) {
			if (FindInstalledMap(map.Name) != null) {
				textBox.WriteLine(String.Format("ERROR: Map {0} is already installed!", map.Name));
				return;
			}

			textBox.WriteLine(String.Format("Installing map {0}...", map.Name));

			string levelSourcePath = Path.Combine(map.MapPath, Program.LevelFolderName);
			string scriptSourcePath = Path.Combine(map.MapPath, Program.ScriptFolderName);

			foreach (var level in map.Levels) {
				string sourcePath = Path.Combine(levelSourcePath, level);
				string destinationPath = Path.Combine(Program.installedLevelsPath, level);
				File.Copy(sourcePath, destinationPath, overwrite);
			}
			foreach (var script in map.Scripts) {
				string sourcePath = Path.Combine(scriptSourcePath, script);
				string destinationPath = Path.Combine(Program.installedScriptsPath, script);
				File.Copy(sourcePath, destinationPath, overwrite);
			}

			var installedMap = new Map {
				Name = map.Name,
				Levels = new string[map.Levels.Length],
				Scripts = new string[map.Scripts.Length]
			};
			if (installedMap.Levels.Length == 1) {
				installedMap.startupLevel = Program.RemoveLevelExtension(map.Levels[0]);
			}
			map.Levels.CopyTo(installedMap.Levels, 0);
			map.Scripts.CopyTo(installedMap.Scripts, 0);
			Program.installedMaps.Add(installedMap);
			mapsMenu.SetColumnContent(0, Text.InstalledMaps, Program.MapListToStringArray(Program.installedMaps));
			Program.SaveInstalledMaps();
			mapsMenu.Draw();
			textBox.AppendLastLine(" installed!");
		}

		public void ReinstallMap(Map installedMap, LoadableMap toInstall) {
			UninstallMap(installedMap);
			InstallMap(toInstall, false);
		}

		public void Show() {
			mapsMenu.SelectFirstItem();
			DrawAll();
			windowQuit = false;
			while (!windowQuit) {
				GUI.Selection selection = mapsMenu.PromptInput();
				switch (selection.command) {
					case Properties.Command.Confirm:
						if (selection.list.IsEmpty) {
							continue;
						}

						var selectedItem = selection.list.SelectedItem;
						selectedItem.Highlight();

						if (selection.columnIndex == 0) { // Installed Maps -> Uninstall
							textBox.Clear();
							int response = selectionPrompt.PromptSelection(new string[] { Text.Uninstall }, new GUI.SelectionPrompt.Options() { cancel = true });
							if (response == 0) {
								var currentRow = mapsMenu.CurrentList.selectedItemIndex;
								UninstallMap(Program.installedMaps[selection.rowIndex]);
								if (mapsMenu.CurrentList.IsEmpty) {
									mapsMenu.SelectFirstItem();
								}
								else {
									mapsMenu.CurrentList.Select(currentRow);
								}
							}
						}
						else if (selection.columnIndex == 1) { // Available Maps -> Install/Reinstall
							int response;
							var selectedLoadableMap = Program.availableMaps[selection.rowIndex];
							var alreadyInstalledMap = FindInstalledMap(selection.Text);
							if (alreadyInstalledMap != null) { // If the map is already installed
								textBox.Clear();
								textBox.WriteLine(String.Format("Map \"{0}\" is already installed. Reinstall?", selection.Text));
								response = selectionPrompt.PromptSelection(new string[] { Text.Reinstall }, true);
								if (response == 0) {
									ReinstallMap(alreadyInstalledMap, selectedLoadableMap);
								}
								else {
									textBox.Clear();
								}
							}
							else if (VerifyNothingOverwritten(selectedLoadableMap)) {
								textBox.Clear();
								response = selectionPrompt.PromptSelection(new string[] { Text.Install }, true);
								if (response == 0) {
									InstallMap(Program.availableMaps[selection.rowIndex], false);
								}
								else {
									textBox.Clear();
								}
							}
							selection.list.Select(selection.rowIndex);
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

		public void UninstallMap(Map map) {
			textBox.WriteLine(String.Format("Uninstalling map {0}...", map.Name));
			foreach (var level in map.Levels) {
				var levelPath = Path.Combine(Program.installedLevelsPath, level);
				File.Delete(levelPath);
			}
			foreach (var script in map.Scripts) {
				var scriptPath = Path.Combine(Program.installedScriptsPath, script);
				File.Delete(scriptPath);
			}
			Program.installedMaps.Remove(map);
			Program.SaveInstalledMaps();
			mapsMenu.SetColumnContent(0, "Installed Maps", Program.MapListToStringArray(Program.installedMaps));

			if (!CheckMapsExist()) {
				return;
			}

			mapsMenu.Clear();
			mapsMenu.Draw();
			textBox.AppendLastLine(" uninstalled!");
		}

		public Map FindInstalledMap(string mapName) {
			foreach (var map in Program.installedMaps) {
				if (map.Name.Equals(mapName)) {
					return map;
				}
			}

			return null;
		}

		private bool CheckMapsExist() {
			return (Program.installedMaps.Count > 0 || Program.availableMaps.Count > 0);
		}

		private bool VerifyNothingOverwritten(LoadableMap map) {
			// Check which files already exist
			var existingLevels = new List<string>();
			foreach (var level in map.Levels) {
				string correspondingLevel = Path.Combine(Program.installedLevelsPath, level);
				if (File.Exists(correspondingLevel)) {
					existingLevels.Add(level);
				}
			}
			var existingScripts = new List<string>();
			foreach (var script in map.Scripts) {
				string correspondingScript = Path.Combine(Program.installedScriptsPath, script);
				if (File.Exists(correspondingScript)) {
					existingScripts.Add(script);
				}
			}
			if (existingLevels.Count > 0 || existingScripts.Count > 0) {
				textBox.Clear();
				textBox.WriteLine("Map is not marked as installed, but one or more files already exist. Overwrite?");
				textBox.WriteLine(String.Format("Levels: {0}", string.Join(',', existingLevels.ToArray())));
				textBox.WriteLine(String.Format("Scripts: {0}", string.Join(',', existingLevels.ToArray())));
				textBox.WriteLine("It is recommended to backup your original files before overwriting them.");
				var options = new GUI.SelectionPrompt.Options() {
					cancel = true,
					index = 1,
				};
				int response = selectionPrompt.PromptSelection(new string[] { Text.Overwrite }, options);
				if (response == 0) {
					InstallMap(map, true);
				}
				else textBox.Clear();

				return false;
			}
			else {
				return true;
			}
		}
	}
}
