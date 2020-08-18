using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarrionManagerConsole
{
	class SaveManagerWindow : IWindow
	{
		public SaveManagerWindow() {

		}

		public string BackupCurrentSave() {
			Dictionary<string, string> saveSettings;
			if (File.Exists(Program.saveInfoFilePath)) {
				saveSettings = Program.ReadInfoFile(Program.saveInfoFilePath);
			}
			else {
				saveSettings = GenerateSaveInfo(Text.MainGame);
				Program.SaveInfoFile(Program.saveInfoFilePath, saveSettings);
			}
			if (!saveSettings.ContainsKey(Text.SaveInfoMapName)) {
				saveSettings[Text.SaveInfoMapName] = String.Format("{0} - {1}", DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"), Text.Unknown);
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

		public bool BackupSavesContainMap(Map map)
		{
			var checkFolderPath = Path.Combine(Program.saveBackupsPath, map.Name);
			var checkInfoFilePath = Path.Combine(checkFolderPath, Program.SaveInfoFileName);
			return File.Exists(checkInfoFilePath);
		}

		public Dictionary<string, string> GenerateSaveInfo(string mapName) {
			return new Dictionary<string, string>() {
				[Text.SaveInfoMapName] = mapName,
			};
		}

		public string GetCurrentSavedMapName()
		{
			if (!File.Exists(Program.saveInfoFilePath)) {
				return Text.MainGame;
			}
			var saveInfo = Program.ReadInfoFile(Program.saveInfoFilePath);
			return saveInfo[Text.SaveInfoMapName];
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
			}
			else {
				var saveSettings = GenerateSaveInfo(mapName);
				Program.SaveInfoFile(Program.saveInfoFilePath, saveSettings);
			}
		}

		public bool MapHasSave(Map map)
		{
			return ((GetCurrentSavedMapName() == map.Name) || (BackupSavesContainMap(map)));
		}

		public void SetCurrentSave(string mapName) {
			var saveInfo = GenerateSaveInfo(mapName);
			Program.SaveInfoFile(Program.saveInfoFilePath, saveInfo);
		}

		public void Show() {
			GUI.Reset();
			Console.WriteLine("Not yet implemented!");
			Console.WriteLine("Press any key to go back...");
			Console.ReadKey();
			Program.currentWindow = Program.navigationWindow;
		}

		// Swaps the current save with the specified one.
		public void SwapSaves(string mapName) {
			BackupCurrentSave();
			LoadBackedUpSave(mapName);
		}
	}
}
