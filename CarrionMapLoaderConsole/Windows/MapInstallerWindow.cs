﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection.Emit;
using System.Text;

namespace CarrionManagerConsole
{
	class MapInstallerWindow : DefaultWindow
	{
		private bool ignoreNextSelectionChangedEvent;

		public MapInstallerWindow() : base(Text.MapInstallerWindowTitle, MenuColor.MapInstallerWindowTitleBG, MenuColor.MapInstallerWindowTitleFG) {
			InstalledMapsList = Menu.AddListBox(0, Text.MapInstallerInstalledMapsHeader, true);
			InstalledMapsList.SetContent(Program.MapListToStringArray(Program.installedMaps));
			InstalledMapsList.SelectionChanged += InstalledMapSelectionChanged;
			AvailableMapsList = Menu.AddListBox(1, Text.MapInstallerAvailableMapsHeader, true);
			AvailableMapsList.SetContent(Program.MapListToStringArray(Program.availableMaps));
			AvailableMapsList.SelectionChanged += AvailableMapSelectionChanged;
			ignoreNextSelectionChangedEvent = false;
		}

		public GUI.ListBox InstalledMapsList;
		public GUI.ListBox AvailableMapsList;

		public void AvailableMapSelectionChanged(object sender, GUI.SelectionChangedEventArgs args) {
			if (ignoreNextSelectionChangedEvent) {
				ignoreNextSelectionChangedEvent = false;
			} else {
				var selectedMap = Program.availableMaps[args.SelectedItemIndex];
				WriteMapInfo(selectedMap);
			}
		}

		public void InstalledMapSelectionChanged(object sender, GUI.SelectionChangedEventArgs args) {
			if (ignoreNextSelectionChangedEvent) {
				ignoreNextSelectionChangedEvent = false;
			} else {
				var selectedMap = Program.installedMaps[args.SelectedItemIndex];
				WriteMapInfo(selectedMap);
			}
		}

		public void InstallMap(LoadableMap map, bool overwrite) {
			if (FindInstalledMap(map.Name) != null) {
				LogTextBox.WriteLine(string.Format("ERROR: Map {0} is already installed!", map.Name));
				return;
			}

			LogTextBox.WriteLine(string.Format("Installing map {0}...", map.Name));

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

			var installedMap = new Map(
				map.Name,
				map.Author,
				map.Version,
				map.ShortDescription,
				map.LongDescription,
				new string[map.Levels.Length],
				new string[map.Scripts.Length],
				map.StartupLevel,
				map.Issues);
			if (installedMap.Levels.Length == 1) {
				installedMap.StartupLevel = Program.RemoveLevelExtension(map.Levels[0]);
			}
			map.Levels.CopyTo(installedMap.Levels, 0);
			map.Scripts.CopyTo(installedMap.Scripts, 0);
			Program.installedMaps.Add(installedMap);
			InstalledMapsList.SetContent(Program.MapListToStringArray(Program.installedMaps));
			Program.SaveInstalledMaps();
			Menu.Draw();
			LogTextBox.AppendLastLine(" installed!");
			ignoreNextSelectionChangedEvent = true;
		}

		public void ReinstallMap(Map installedMap, LoadableMap toInstall) {
			UninstallMap(installedMap);
			InstallMap(toInstall, false);
		}

		public override void Selected(GUI.Selection selection) {
			selection.SelectedItem.Highlight();

			if (selection.ColumnIndex == 0) { // Installed Maps -> Uninstall
				var selectedMap = Program.installedMaps[selection.RowIndex];
				LogTextBox.ClearContent();
				string[] selections;
				if (selectedMap.IsValid) {
					selections = new string[] { Text.Uninstall };
				} else {
					selections = new string[] { Text.Uninstall, Text.ShowIssues };
				}
				int response = SelectionPrompt.PromptSelection(selections, true);
				switch (response) {
					case 0:
						var currentRow = Menu.CurrentRow;
						UninstallMap(selectedMap);
						if (InstalledMapsList.CanNavigate) {
							InstalledMapsList.Select(currentRow);
						} else {
							Menu.NavigateToDefault();
						}
						break;
					case 1:
						selectedMap.ShowIssues();
						DrawAll();
						break;
				}
			} else if (selection.ColumnIndex == 1) { // Available Maps -> Install/Reinstall/Overwrite
				var selectedLoadableMap = Program.availableMaps[selection.RowIndex];
				var alreadyInstalledMap = FindInstalledMap(selectedLoadableMap.Name);
				if (alreadyInstalledMap != null) { // If the map is already installed
					LogTextBox.ClearContent();
					LogTextBox.WriteLine(string.Format(Text.PromptReinstall, alreadyInstalledMap.Name));
					string[] selections;
					if (selectedLoadableMap.IsValid) {
						selections = new string[] { Text.Reinstall };
					} else {
						selections = new string[] { Text.Reinstall, Text.ShowIssues };
					}
					int response = SelectionPrompt.PromptSelection(selections, true);
					switch (response) {
						case 0:
							ReinstallMap(alreadyInstalledMap, selectedLoadableMap);
							break;
						case 1:
							selectedLoadableMap.ShowIssues();
							DrawAll();
							break;
						default:
							WriteMapInfo(selectedLoadableMap);
							break;
					}
				} else if (VerifyNothingOverwritten(selectedLoadableMap)) {
					LogTextBox.ClearContent();
					string[] selections;
					if (selectedLoadableMap.IsValid) {
						selections = new string[] { Text.Install };
					} else {
						selections = new string[] { Text.Install, Text.ShowIssues };
					}
					int response = SelectionPrompt.PromptSelection(selections, true);
					switch (response) {
						case 0:
							InstallMap(selectedLoadableMap, false);
							break;
						case 1:
							selectedLoadableMap.ShowIssues();
							DrawAll();
							break;
						default:
							WriteMapInfo(selectedLoadableMap);
							break;
					}
				}
				selection.List.Select(selection.RowIndex);
			}
		}

		public void UninstallMap(Map map) {
			LogTextBox.WriteLine(string.Format(Text.UninstallingMap, map.Name));
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
			InstalledMapsList.SetContent(Program.MapListToStringArray(Program.installedMaps));
			InstalledMapsList.Clear();
			InstalledMapsList.Draw();
			LogTextBox.AppendLastLine(" uninstalled!");
			if (Program.backupsWindow.RestoreInstalledMapFiles(map)) {
				LogTextBox.WriteLine(Text.BackedUpFilesRestored);
			}
			ignoreNextSelectionChangedEvent = true;
		}

		public Map FindInstalledMap(string mapName) {
			foreach (var map in Program.installedMaps) {
				if (map.Name.Equals(mapName)) {
					return map;
				}
			}

			return null;
		}

		public override void PreShow() {

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
				LogTextBox.ClearContent();
				LogTextBox.WriteLine("Map is not marked as installed, but one or more files already exist. Overwrite?");
				LogTextBox.WriteLine(string.Format("Levels: {0}", string.Join(',', existingLevels.ToArray())));
				LogTextBox.WriteLine(string.Format("Scripts: {0}", string.Join(',', existingLevels.ToArray())));
				LogTextBox.WriteLine("Backup & Install: Listed files will be backed up and restored when uninstalling this map.");
				var options = new GUI.SelectionPrompt.Options() {
					cancel = true,
					index = 2,
				};
				int response = SelectionPrompt.PromptSelection(new string[] { Text.Overwrite, Text.BackupAndInstall }, options);
				LogTextBox.ClearContent();
				switch (response) {
					case 0:
						InstallMap(map, true);
						break;
					case 1:
						// TODO: Verify in case backups would be overwritten.
						LogTextBox.WriteLine(Text.BackingUpFiles);
						Program.backupsWindow.BackUpInstalledMapFiles(map);
						LogTextBox.AppendLastLine(" " + Text.BackupFinished);
						InstallMap(map, true);
						break;
					default:
						WriteMapInfo(map);
						break;
				}

				return false;
			} else {
				return true;
			}
		}

		private void WriteMapInfo(Map map) {
			LogTextBox.ClearContent();
			string firstLine = Text.MapInfoMapName + map.Name;
			if (map.Version != null) {
				firstLine += Text.MapInfoSeparator + Text.MapInfoVersion + map.Version;
			}
			if (map.Author != null) {
				firstLine += Text.MapInfoSeparator + Text.MapInfoAuthor + map.Author;
			}
			LogTextBox.WriteLine(firstLine);

			if (map.ShortDescription != null) {
				LogTextBox.WriteLine(Text.MapInfoShortDescription + map.ShortDescription);
			}

			if (!map.IsValid) {
				for (int currentIssue = 0; currentIssue < map.Issues.Count; ++currentIssue) {
					if (LogTextBox.RemainingFreeLines > 1 || map.Issues.Count - currentIssue == 1) {
						LogTextBox.WriteLine(Text.MapHasIssuesIndicator + map.Issues[currentIssue]);
					} else {
						LogTextBox.WriteLine(string.Format(Text.SoManyMoreIssues, map.Issues.Count - currentIssue));
						break;
					}
				}
			}
		}
	}
}
