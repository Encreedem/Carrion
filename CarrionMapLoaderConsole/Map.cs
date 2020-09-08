using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace CarrionManagerConsole
{
	/*TODO:
	 * Maybe support more than just loading levels and scripts.
	 * */
	
	class Map
	{
		public Map() {
			
		}

		public Map(string name, string author, string version, string shortDescription, string longDescription, string[] levels, string[] scripts, bool isWIP) {
			Name = name;
			Author = author;
			Version = version;
			ShortDescription = shortDescription;
			LongDescription = longDescription;
			Levels = levels;
			Scripts = scripts;
			IsWIP = isWIP;
			VerifyMap();
		}

		public Map(string name, string author, string version, string shortDescription, string longDescription, string[] levels, string[] scripts, string startupLevel, bool isWIP, List<string> issues) {
			Name = name;
			Author = author;
			Version = version;
			ShortDescription = shortDescription;
			LongDescription = longDescription;
			Levels = levels;
			Scripts = scripts;
			StartupLevel = startupLevel;
			IsWIP = isWIP;
			Issues = issues;
		}

		public string Name { get; set; }
		public string Author { get; set; }
		public string Version { get; set; }
		public string ShortDescription { get; set; }
		public string LongDescription { get; set; }
		public string[] Levels { get; set; }
		public string[] Scripts { get; set; }
		public string StartupLevel { get; set; }
		public bool IsWIP { get; set; }
		[JsonIgnore] public List<string> Issues { get; set; }
		[JsonIgnore]
		public bool IsValid => Issues == null || Issues.Count == 0;

		/// <summary>
		/// Adds the level to the map's Levels and Scripts.
		/// </summary>
		/// <param name="levelName">The level name without an extension.</param>
		public void AddLevel(string levelName) {
			string[] levels = new string[Levels.Length + 1];
			string[] scripts = new string[Scripts.Length + 1];
			Levels.CopyTo(levels, 0);
			Scripts.CopyTo(scripts, 0);
			levels[^1] = levelName + Program.LevelFileExtension;
			scripts[^1] = levelName + Program.ScriptFileExtension;
			Array.Sort(levels);
			Array.Sort(scripts);
			Levels = levels;
			Scripts = scripts;
		}

		public string[] GetLevelsWithoutExtension() {
			var levelsWithoutExtension = new string[Levels.Length];
			for (int i = 0; i < Levels.Length; ++i) {
				levelsWithoutExtension[i] = Program.RemoveLevelExtension(Levels[i]);
			}
			return levelsWithoutExtension;
		}

		/// <summary>
		/// Removes the level from the map's Levels and Scripts without deleting the file.
		/// </summary>
		/// <param name="levelName">The level name without an extension.</param>
		public void RemoveLevel(string levelName) {
			string fullLevelName = levelName + Program.LevelFileExtension;
			string fullScriptName = levelName + Program.ScriptFileExtension;
			Levels = Levels.Where(name => name != fullLevelName).ToArray();
			Scripts = Scripts.Where(name => name != fullScriptName).ToArray();
		}

		/// <summary>
		/// Renames the entries in the map's Levels and Scripts.
		/// </summary>
		/// <param name="currentName">The current name without an extension.</param>
		/// <param name="newName">The new name without an extension.</param>
		public void RenameLevel(string currentName, string newName) {
			string currentLevelName = currentName + Program.LevelFileExtension;
			string currentScriptName = currentName + Program.ScriptFileExtension;
			for (int i = 0; i < Levels.Length; i++) {
				if (Levels[i] == currentLevelName) {
					Levels[i] = newName + Program.LevelFileExtension;
					break;
				}
			}
			for (int i = 0; i < Scripts.Length; i++) {
				if (Scripts[i] == currentScriptName) {
					Scripts[i] = newName + Program.ScriptFileExtension;
					break;
				}
			}
		}

		public void ShowIssues() {
			GUI.Reset();
			Console.WriteLine(Text.MapInfoMapName + Name);
			foreach (string issue in Issues) {
				Console.WriteLine(issue);
			}
			Console.WriteLine();
			Console.WriteLine(Text.PressAnyKeyToContinue);
			Console.ReadKey();
		}

		/// <summary>
		/// Checks whether there are any issues with the map and stores any in its Issues list.
		/// </summary>
		public void VerifyMap() {
			Issues = new List<string>();
			if (Levels != null && Levels.Length > 0 && Scripts != null && Levels.Length > 0) {
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
			} else {
				if (Levels == null || Levels.Length == 0) {
					Issues.Add(Text.MapContainsNoLevels);
				}
				if (Scripts == null || Scripts.Length == 0) {
					Issues.Add(Text.MapContainsNoScripts);
				}
			}

			if (Levels != null && !string.IsNullOrEmpty(StartupLevel)) {
				if (!Levels.Contains(StartupLevel + Program.LevelFileExtension)) {
					Issues.Add(string.Format(Text.StartupLevelInvalid, StartupLevel));
				}
			}
		}
	}
}
