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
			this.Issues = new List<string>();
			this.MapPath = folderPath;

			var directoryInfo = new DirectoryInfo(folderPath);
			this.Name = directoryInfo.Name;

			string levelFolderPath = Path.Combine(folderPath, Program.LevelFolderName);
			if (Directory.Exists(levelFolderPath)) {
				string[] levelPaths = Directory.GetFiles(levelFolderPath);
				this.Levels = new string[levelPaths.Length];
				for (int i = 0; i < levelPaths.Length; ++i) {
					this.Levels[i] = Path.GetFileName(levelPaths[i]);
				}
			} else {
				this.Issues.Add("Map doesn't contain Levels folder.");
			}

			string scriptFolderPath = Path.Combine(folderPath, Program.ScriptFolderName);
			if (Directory.Exists(scriptFolderPath)) {
				string[] scriptPaths = Directory.GetFiles(scriptFolderPath);
				this.Scripts = new string[scriptPaths.Length];
				for (int i = 0; i < scriptPaths.Length; ++i) {
					this.Scripts[i] = Path.GetFileName(scriptPaths[i]);
				}
			} else {
				this.Issues.Add("Map doesn't contain Scripts folder.");
			}
			
			VerifyMap();
		}

		public bool IsValid {
			get {
				return (this.Issues == null || this.Issues.Count == 0);
			}
		}

		// Returns the map's path relative to the path passed as the argument.
		public string GetRelativePath(string rootPath) {
			if (this.MapPath.StartsWith(rootPath)) {
				string relativePath = this.MapPath.Substring(rootPath.Length);
				return relativePath;
			}
			else {
				return this.MapPath;
			}
		}

		// Checks whether there are any issues with the map and stores any in its Issues list.
		public void VerifyMap() {
			// Check whether script files are missing.
			foreach (string level in this.Levels) {
				string baseName = level.Substring(0, level.Length - Program.LevelFileExtension.Length);
				string correspondingScriptName = baseName + Program.ScriptFileExtension;
				if (!this.Scripts.Contains(correspondingScriptName)) {
					this.Issues.Add(String.Format("Map contains level {0} but not corresponding script {1}", level, correspondingScriptName));
				}
			}
			foreach (string script in this.Scripts) {
				string baseName = script.Substring(0, script.Length - Program.ScriptFileExtension.Length);
				string correspondingLevelName = baseName + Program.LevelFileExtension;
				if (!this.Levels.Contains(correspondingLevelName)) {
					this.Issues.Add(String.Format("Map contains script {0} but not corresponding level {1}", script, correspondingLevelName));
				}
			}
		}
	}
}
