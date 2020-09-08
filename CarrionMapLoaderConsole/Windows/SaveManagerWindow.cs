using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarrionManagerConsole
{
	class SaveManagerWindow : DefaultWindow
	{

		private string currentSavedMapName;
		private List<string> backedUpSaves;

		public SaveManagerWindow() : base(Text.SaveManagerWindowTitle, MenuColor.SaveMangerWindowTitleBG, MenuColor.SaveMangerWindowTitleFG) {
			CommandsList = Menu.AddListBox(0, null, true);
			CommandsList.SetItems(new string[] {
				Text.BackUpCurrentSave,
				Text.ViewBackups,
				Text.ToggleAutoBackups });
			DetailsTextBox = Menu.AddTextBox(1, null);
			BackedUpSavesList = new GUI.ListBox(DetailsTextBox.Left, DetailsTextBox.Top, DetailsTextBox.Width, DetailsTextBox.Height, MenuColor.ContentBG, MenuColor.ContentFG, true);
			currentSavedMapName = string.Empty;
			backedUpSaves = new List<string>();
		}

		private GUI.ListBox CommandsList { get; set; }
		private GUI.TextBox DetailsTextBox { get; set; }
		private GUI.ListBox BackedUpSavesList { get; set; }

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
			BackedUpSavesList.SetItems(backedUpSaves.ToArray());
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

		public override void Selected(GUI.Selection selection) {
			if (selection.List.IsEmpty) {
				return;
			}
			LogTextBox.ClearContent();

			switch (selection.RowIndex) {
				case 0: // Backup current map
					LogTextBox.WriteLine(Text.BackingUpCurrentSave);
					BackupCurrentSave();
					RefreshInfo();
					WriteSummary();
					LogTextBox.WriteLine(string.Format(Text.BackedUpMap, currentSavedMapName));
					break;
				case 1: // View backups...
					CommandsList.HighlightCurrentItem();
					ShowBackups();
					CommandsList.SelectCurrentItem();
					break;
				case 2:
					if (Program.manageSaves) {
						Program.manageSaves = false;
						LogTextBox.WriteLine(Text.DisabledManageSaves);
					} else {
						Program.manageSaves = true;
						LogTextBox.WriteLine(Text.EnabledManageSaves);
					}
					Program.SaveSettings();
					WriteSummary();
					break;
			}
		}

		public override void Show() {
			RefreshInfo();
			WriteSummary();
			base.Show();
		}

		public void ShowBackups() {
			if (backedUpSaves.Count == 0) {
				LogTextBox.WriteLine(Text.NoBackedUpSaves);
				return;
			}
			BackedUpSavesList.Clear();
			BackedUpSavesList.Draw();
			BackedUpSavesList.Select(0);
			bool promptQuit = false;
			while (!promptQuit) {
				var selection = BackedUpSavesList.PromptSelection();
				if (selection.Command == Properties.Command.Confirm) {
					BackedUpSavesList.HighlightCurrentItem();
					var response = SelectionPrompt.PromptSelection(new string[] { Text.LoadBackup }, true);
					if (response == 0) { // Load Backup
						SwapSaves(selection.Text);
						RefreshInfo();
						LogTextBox.WriteLine(string.Format(Text.LoadedBackup, selection.Text));
						promptQuit = true;
					} else {
						BackedUpSavesList.SelectCurrentItem();
					}
				} else if (selection.Command == Properties.Command.Cancel) {
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
			DetailsTextBox.ClearContent();
			DetailsTextBox.WriteLine(Text.SaveManagerWindowCurrentSave);
			DetailsTextBox.WriteLine(currentSavedMapName);
			DetailsTextBox.WriteLine();
			DetailsTextBox.WriteLine(Text.SaveManagerWindowBackupsCount);
			DetailsTextBox.WriteLine(backedUpSaves.Count.ToString());
			DetailsTextBox.WriteLine();
			DetailsTextBox.WriteLine(Text.SaveManagerWindowAutoBackupStatus);
			DetailsTextBox.WriteLine(Program.manageSaves ? Text.Enabled : Text.Disabled);
		}
	}
}
