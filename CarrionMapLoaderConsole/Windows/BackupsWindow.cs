using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CarrionManagerConsole
{
	class BackupsWindow : DefaultWindow
	{
		private string[] backedUpLevels, backedUpScripts;

		public BackupsWindow() : base(Text.BackupsWindowTitle, MenuColor.BackupsWindowTitleBG, MenuColor.BackupsWindowTitleFG) {
			CommandsList = Menu.AddListBox(0, null, true);
			CommandsList.SetItems(new string[] {
				Text.BackedUpLevels,
				Text.BackedUpScripts,
			});
			DetailsTextBox = Menu.AddTextBox(1, null);
			BackedUpLevelsList = new GUI.ListBox(
				DetailsTextBox.Left, DetailsTextBox.Top,
				DetailsTextBox.Width, DetailsTextBox.Height,
				MenuColor.ContentBG, MenuColor.ContentFG,
				true);
			BackedUpScriptsList = new GUI.ListBox(
				DetailsTextBox.Left, DetailsTextBox.Top,
				DetailsTextBox.Width, DetailsTextBox.Height,
				MenuColor.ContentBG, MenuColor.ContentFG,
				true);
		}

		private GUI.ListBox CommandsList { get; set; }
		private GUI.TextBox DetailsTextBox { get; set; }
		private GUI.ListBox BackedUpLevelsList { get; set; }
		private GUI.ListBox BackedUpScriptsList { get; set; }

		public void BackUpInstalledMapFiles(Map map) {
			if (map.Levels != null) {
				foreach(var level in map.Levels) {
					string sourcePath = Path.Combine(Program.installedLevelsPath, level);
					string destinationPath = Path.Combine(Program.levelBackupsPath, level);
					File.Copy(sourcePath, destinationPath, true);
				}
			}
			if (map.Scripts != null) {
				foreach (var script in map.Scripts) {
					string sourcePath = Path.Combine(Program.installedScriptsPath, script);
					string destinationPath = Path.Combine(Program.scriptBackupsPath, script);
					File.Copy(sourcePath, destinationPath, true);
				}
			}
		}

		public string[] GetBackedUpLevelNames() {
			if (!Directory.Exists(Program.levelBackupsPath)) {
				return new string[0];
			}
			var files = Directory.GetFiles(Program.levelBackupsPath, "*" + Program.LevelFileExtension);
			string[] onlyFileNames = files.Select(file => Path.GetFileName(file)).ToArray();
			return onlyFileNames;
		}

		public string[] GetBackedUpScriptNames() {
			if (!Directory.Exists(Program.scriptBackupsPath)) {
				return new string[0];
			}
			var files = Directory.GetFiles(Program.scriptBackupsPath, "*" + Program.ScriptFileExtension);
			string[] onlyFileNames = files.Select(file => Path.GetFileName(file)).ToArray();
			return onlyFileNames;
		}

		public void RefreshInfo() {
			backedUpLevels = GetBackedUpLevelNames();
			BackedUpLevelsList.SetItems(backedUpLevels);
			backedUpScripts = GetBackedUpScriptNames();
			BackedUpScriptsList.SetItems(backedUpScripts);
		}

		/// <summary>
		/// Restores the files overwritten by the given map.
		/// </summary>
		/// <param name="map">The map whose files should be restored.</param>
		/// <returns>Returns whether there were any files to be restored.</returns>
		public bool RestoreInstalledMapFiles(Map map) {
			bool restored = false;

			if (map.Levels != null) {
				foreach(var level in map.Levels) {
					string sourcePath = Path.Combine(Program.levelBackupsPath, level);
					if (!File.Exists(sourcePath))
						continue;
					string destinationPath = Path.Combine(Program.installedLevelsPath, level);
					File.Move(sourcePath, destinationPath, false);
					restored = true;
				}
			}
			if (map.Scripts != null) {
				foreach (var script in map.Scripts) {
					string sourcePath = Path.Combine(Program.scriptBackupsPath, script);
					if (!File.Exists(sourcePath))
						continue;
					string destinationPath = Path.Combine(Program.installedScriptsPath, script);
					File.Move(sourcePath, destinationPath, false);
					restored = true;
				}
			}

			return restored;
		}

		public override void Selected(GUI.Selection selection) {
			bool promptQuit = false;
			GUI.ListBox listBox = selection.RowIndex switch
			{
				0 => BackedUpLevelsList,
				1 => BackedUpScriptsList,
				_ => throw new NotImplementedException(),
			};
			CommandsList.HighlightCurrentItem();
			listBox.Clear();
			listBox.Draw();
			listBox.NavigateToDefault();
			while (!promptQuit) {
				var subSelection = BackedUpLevelsList.PromptSelection();
				if (subSelection.Command == Properties.Command.Cancel) {
					promptQuit = true;
				}
			}

			CommandsList.SelectCurrentItem();
			DetailsTextBox.Clear();
			WriteSummary();
		}

		public override void Show() {
			RefreshInfo();
			WriteSummary();
			base.Show();
		}

		public void WriteSummary() {
			DetailsTextBox.ClearContent();
			DetailsTextBox.WriteLine(Text.LevelBackupsCount);
			DetailsTextBox.WriteLine(backedUpLevels.Length.ToString());
			DetailsTextBox.WriteLine();
			DetailsTextBox.WriteLine(Text.ScriptBackupsCount);
			DetailsTextBox.WriteLine(backedUpScripts.Length.ToString());
		}
	}

}
