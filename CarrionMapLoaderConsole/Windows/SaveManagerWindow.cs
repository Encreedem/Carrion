using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarrionManagerConsole
{
	class SaveManagerWindow : IWindow
	{
		private readonly GUI.Label title;
		private readonly GUI.ListBox commandsList;
		//private readonly GUI.Box menuDetailsSeparator;
		private readonly GUI.TextBox detailsTextBox;
		private readonly GUI.ListBox backedUpSavesList;
		private readonly GUI.Box menuCommandSeparator;
		private readonly GUI.SelectionPrompt selectionPrompt;
		private readonly GUI.Box commandTextSeparator;
		private readonly GUI.TextBox logTextBox;

		private bool windowQuit;
		private string currentSavedMapName;
		private List<string> backedUpSaves;

		public SaveManagerWindow() {
			int width = Console.WindowWidth;
			int halfWidth = width / 2;
			int height = Console.WindowHeight;
			title = new GUI.Label(0, 0, width, 1, MenuColor.SaveMangerWindowTitleBG, MenuColor.SaveMangerWindowTitleFG, Text.SaveManagerWindowTitle) {
				HorizontalAlignment = Properties.HorizontalAlignment.Center
			};
			commandsList = new GUI.ListBox(0, 1, halfWidth, height - 10, MenuColor.ContentBG, MenuColor.ContentFG);
			commandsList.SetContent(new string[] { Text.BackUpCurrentSave, Text.ViewBackups, Text.ToggleAutoBackups });
			//menuDetailsSeparator = new GUI.Box(halfWidth - 1, 1, 1, height - 10, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			detailsTextBox = new GUI.TextBox(halfWidth, 1, halfWidth - 1, height - 10, MenuColor.ContentBG, MenuColor.ContentFG);
			backedUpSavesList = new GUI.ListBox(halfWidth, 1, halfWidth - 1, height - 10, MenuColor.ContentBG, MenuColor.ContentFG);
			menuCommandSeparator = new GUI.Box(0, height - 9, width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			selectionPrompt = new GUI.SelectionPrompt(0, height - 8, width, 1, MenuColor.ContentBG, MenuColor.ContentFG);
			commandTextSeparator = new GUI.Box(0, height - 7, width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			logTextBox = new GUI.TextBox(0, height - 6, width, 4, MenuColor.ContentBG, MenuColor.ContentFG);

			currentSavedMapName = string.Empty;
			backedUpSaves = new List<string>();
		}

		public string BackupCurrentSave() {
			Dictionary<string, string> saveSettings;
			if (File.Exists(Program.saveInfoFilePath)) {
				saveSettings = Program.ReadInfoFile(Program.saveInfoFilePath);
			} else {
				saveSettings = GenerateSaveInfo(Text.MainGame);
				Program.SaveInfoFile(Program.saveInfoFilePath, saveSettings);
			}
			if (!saveSettings.ContainsKey(Text.SaveInfoMapName)) {
				saveSettings[Text.SaveInfoMapName] = string.Format("{0} - {1}", DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"), Text.Unknown);
			}

			var mapName = saveSettings[Text.SaveInfoMapName];
			var destinationFolder = Path.Combine(Program.saveBackupsPath, mapName);
			Directory.CreateDirectory(destinationFolder);
			foreach (var file in Directory.GetFiles(Program.saveFolderPath, "*" + Program.SaveFileExtension)) {
				var fileName = Path.GetFileName(file);
				var destinationFilePath = Path.Combine(destinationFolder, fileName);
				File.Copy(file, destinationFilePath, true);
			}
			File.Copy(Program.saveInfoFilePath, Path.Combine(destinationFolder, Program.SaveInfoFileName), true);

			return string.Format(Text.BackedUpMap, mapName);
		}

		public bool BackupSavesContainMap(Map map) {
			var checkFolderPath = Path.Combine(Program.saveBackupsPath, map.Name);
			var checkInfoFilePath = Path.Combine(checkFolderPath, Program.SaveInfoFileName);
			return File.Exists(checkInfoFilePath);
		}

		private void DrawAll() {
			//GUI.Reset();
			title.Draw();
			commandsList.Draw();
			//menuDetailsSeparator.Clear();
			WriteSummary();
			menuCommandSeparator.Clear();
			selectionPrompt.Clear();
			commandTextSeparator.Clear();
			logTextBox.Draw();
			Program.controlsLabel.Draw();
		}

		public List<string> GetBackedUpMapSaveNames() {
			var backedUpSaves = new List<string>();
			foreach (var directory in Directory.GetDirectories(Program.saveBackupsPath)) {
				var saveInfoPath = Path.Combine(directory, Program.SaveInfoFileName);
				if (File.Exists(saveInfoPath)) {
					var saveInfo = Program.ReadInfoFile(saveInfoPath);
					if (saveInfo.ContainsKey(Text.SaveInfoMapName)) {
						backedUpSaves.Add(saveInfo[Text.SaveInfoMapName]);
					}
				}
			}

			return backedUpSaves;
		}

		public Dictionary<string, string> GenerateSaveInfo(string mapName) {
			return new Dictionary<string, string>() {
				[Text.SaveInfoMapName] = mapName,
			};
		}

		public string GetCurrentSavedMapName() {
			if (!File.Exists(Program.saveInfoFilePath)) {
				return Text.MainGame;
			}
			var saveInfo = Program.ReadInfoFile(Program.saveInfoFilePath);
			return saveInfo[Text.SaveInfoMapName];
		}

		public void RefreshInfo() {
			currentSavedMapName = GetCurrentSavedMapName();
			backedUpSaves = GetBackedUpMapSaveNames();
			backedUpSavesList.SetContent(backedUpSaves.ToArray());
		}

		public void LoadBackedUpSave(string mapName) {
			var sourcePath = Path.Combine(Program.saveBackupsPath, mapName);
			if (Directory.Exists(sourcePath)) {
				foreach (var filePath in Directory.GetFiles(sourcePath, "*" + Program.SaveFileExtension)) {
					var fileName = Path.GetFileName(filePath);
					var destinationFilePath = Path.Combine(Program.saveFolderPath, fileName);
					File.Copy(filePath, destinationFilePath, true);
				}
				File.Copy(Path.Combine(sourcePath, Program.SaveInfoFileName), Program.saveInfoFilePath, true);
			} else {
				var saveSettings = GenerateSaveInfo(mapName);
				Program.SaveInfoFile(Program.saveInfoFilePath, saveSettings);
			}
		}

		public bool MapHasSave(Map map) {
			return ((GetCurrentSavedMapName() == map.Name) || (BackupSavesContainMap(map)));
		}

		public void SetCurrentSave(string mapName) {
			var saveInfo = GenerateSaveInfo(mapName);
			Program.SaveInfoFile(Program.saveInfoFilePath, saveInfo);
		}

		public void Show() {
			GUI.Reset();
			RefreshInfo();
			DrawAll();
			commandsList.Select(0);

			windowQuit = false;
			while (!windowQuit) {
				GUI.Selection selection = commandsList.PromptInput();
				switch (selection.command) {
					case Properties.Command.Confirm:
						if (selection.list.IsEmpty) {
							return;
						}
						logTextBox.Clear();

						switch (selection.rowIndex) {
							case 0: // Backup current map
								logTextBox.WriteLine(Text.BackingUpCurrentSave);
								BackupCurrentSave();
								RefreshInfo();
								WriteSummary();
								logTextBox.WriteLine(string.Format(Text.BackedUpMap, currentSavedMapName));
								break;
							case 1: // View backups...
								commandsList.HighlightCurrentItem();
								ShowBackups();
								commandsList.SelectCurrentItem();
								break;
							case 2:
								if (Program.manageSaves) {
									Program.manageSaves = false;
									logTextBox.WriteLine(Text.DisabledManageSaves);
								}
								else {
									Program.manageSaves = true;
									logTextBox.WriteLine(Text.EnabledManageSaves);
								}
								Program.SaveSettings();
								WriteSummary();
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

		public void ShowBackups() {
			if (backedUpSaves.Count == 0) {
				logTextBox.WriteLine(Text.NoBackedUpSaves);
				return;
			}
			backedUpSavesList.Clear();
			backedUpSavesList.Draw();
			backedUpSavesList.Select(0);
			bool promptQuit = false;
			while (!promptQuit) {
				var selection = backedUpSavesList.PromptInput();
				if (selection.command == Properties.Command.Confirm) {
					backedUpSavesList.HighlightCurrentItem();
					var response = selectionPrompt.PromptSelection(new string[] { Text.LoadBackup }, true);
					if (response == 0) { // Load Backup
						SwapSaves(selection.Text);
						RefreshInfo();
						logTextBox.WriteLine(string.Format(Text.LoadedBackup, selection.Text));
						promptQuit = true;
					}
					else {
						backedUpSavesList.SelectCurrentItem();
					}
				} else if (selection.command == Properties.Command.Cancel) {
					promptQuit = true;
				}
			}
			
			WriteSummary();
		}

		// Swaps the current save with the specified one.
		public void SwapSaves(string mapName) {
			BackupCurrentSave();
			LoadBackedUpSave(mapName);
		}

		public void WriteSummary() {
			detailsTextBox.Clear();
			detailsTextBox.WriteLine(Text.SaveManagerWindowCurrentSave);
			detailsTextBox.WriteLine(currentSavedMapName);
			detailsTextBox.WriteLine();
			detailsTextBox.WriteLine(Text.SaveManagerWindowBackupsCount);
			detailsTextBox.WriteLine(backedUpSaves.Count.ToString());
			detailsTextBox.WriteLine();
			detailsTextBox.WriteLine(Text.SaveManagerWindowAutoBackupStatus);
			detailsTextBox.WriteLine(Program.manageSaves ? Text.Enabled : Text.Disabled);
		}
	}
}
