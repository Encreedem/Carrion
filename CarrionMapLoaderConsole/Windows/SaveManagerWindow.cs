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
				saveSettings = new Dictionary<string, string>() {
					["MapName"] = Text.MainGame,
				};
				Program.SaveInfoFile(Program.saveInfoFilePath, saveSettings);
			}
			if (!saveSettings.ContainsKey("MapName")) {
				saveSettings["MapName"] = String.Format("{0} - {1}", DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss"), Text.Unknown);
			}

			var mapName = saveSettings["MapName"];
			var destinationFolder = Path.Combine(Program.saveBackupsPath, mapName);
			Directory.CreateDirectory(destinationFolder);
			foreach (var file in Directory.GetFiles(Program.saveFolderPath, "*" + Program.SaveFileExtension)) {
				var destinationFilePath = Path.Combine(destinationFolder, file);
				File.Copy(file, destinationFilePath, true);
			}
			File.Copy(Program.saveInfoFilePath, Path.Combine(destinationFolder, Program.SaveInfoFileName), true);

			return string.Format(Text.BackedUpMap, mapName);
		}

		public void LoadBackedUpSave(string mapName) {
			var sourcePath = Path.Combine(Program.saveBackupsPath, mapName);
			if (Directory.Exists(sourcePath)) {
				foreach (var file in Directory.GetFiles(sourcePath, "*" + Program.SaveFileExtension)) {
					var destinationFilePath = Path.Combine(Program.saveFolderPath, file);
					File.Copy(file, destinationFilePath, true);
				}
				File.Copy(Path.Combine(sourcePath, Program.SaveInfoFileName), Program.saveInfoFilePath, true);
			}
			else {
				var saveSettings = new Dictionary<string, string>() {
					["MapName"] = mapName,
				};
				Program.SaveInfoFile(Program.saveInfoFilePath, saveSettings);
			}
		}

		public void Show() {
			GUI.Reset();
			Console.WriteLine("Not yet implemented!");
			Console.WriteLine("Press any key to go back...");
			Console.ReadKey();
			Program.currentWindow = Program.navigationWindow;
		}
	}
}
