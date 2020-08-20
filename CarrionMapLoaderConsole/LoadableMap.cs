using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CarrionManagerConsole
{
	class LoadableMap : Map
	{
		public string MapPath;
		public List<string> Issues;

		public LoadableMap(string name, string[] levels, string[] scripts, string path) : base(name, levels, scripts) {
			this.Issues = new List<string>();
			this.MapPath = path;
			this.VerifyMap();
		}

		// Get map info from folder
		public LoadableMap(string folderPath) : base() {
			Issues = new List<string>();
			MapPath = folderPath;

			var directoryInfo = new DirectoryInfo(folderPath);
			Name = directoryInfo.Name;

			string levelFolderPath = Path.Combine(folderPath, Program.LevelFolderName);
			if (Directory.Exists(levelFolderPath)) {
				string[] levelPaths = Directory.GetFiles(levelFolderPath);
				Levels = new string[levelPaths.Length];
				for (int i = 0; i < levelPaths.Length; ++i) {
					Levels[i] = Path.GetFileName(levelPaths[i]);
				}
			} else {
				Levels = new string[0];
				Issues.Add(Text.MapIssueNoLevelsFolder);
			}

			string scriptFolderPath = Path.Combine(folderPath, Program.ScriptFolderName);
			if (Directory.Exists(scriptFolderPath)) {
				string[] scriptPaths = Directory.GetFiles(scriptFolderPath);
				Scripts = new string[scriptPaths.Length];
				for (int i = 0; i < scriptPaths.Length; ++i) {
					Scripts[i] = Path.GetFileName(scriptPaths[i]);
				}
			} else {
				Scripts = new string[0];
				Issues.Add(Text.MapIssueNoScriptsFolder);
			}
			
			VerifyMap();
		}

		public bool IsValid {
			get {
				return (Issues == null || Issues.Count == 0);
			}
		}

		// Returns the map's path relative to the path passed as the argument.
		public string GetRelativePath(string rootPath) {
			if (MapPath.StartsWith(rootPath)) {
				string relativePath = MapPath.Substring(rootPath.Length);
				return relativePath;
			}
			else {
				return MapPath;
			}
		}

		// Checks whether there are any issues with the map and stores any in its Issues list.
		public void VerifyMap() {
			if (Levels != null && Scripts != null) {
				// Check whether script files are missing.
				foreach (string level in Levels) {
					string baseName = level.Substring(0, level.Length - Program.LevelFileExtension.Length);
					string correspondingScriptName = baseName + Program.ScriptFileExtension;
					if (!Scripts.Contains(correspondingScriptName)) {
						Issues.Add(string.Format("Map contains level {0} but not corresponding script {1}", level, correspondingScriptName));
					}
				}
				// Check whether level files are missing.
				foreach (string script in Scripts) {
					string baseName = script.Substring(0, script.Length - Program.ScriptFileExtension.Length);
					string correspondingLevelName = baseName + Program.LevelFileExtension;
					if (!Levels.Contains(correspondingLevelName)) {
						Issues.Add(string.Format("Map contains script {0} but not corresponding level {1}", script, correspondingLevelName));
					}
				}
			}
		}
	}
}
