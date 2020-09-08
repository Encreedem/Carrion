using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CarrionManagerConsole
{
	class LoadableMap : Map
	{
		public string MapPath { get; set; }

		public LoadableMap(
			string name,
			string author,
			string version,
			string shortDescription,
			string longDescription,
			string[] levels,
			string[] scripts,
			bool isWIP,
			string path) :
			base(name, author, version, shortDescription, longDescription, levels, scripts, isWIP) {
			MapPath = path;
		}

		public LoadableMap(Map map, string folderPath) : base(
			map.Name,
			map.Author,
			map.Version,
			map.ShortDescription,
			map.LongDescription,
			map.Levels,
			map.Scripts,
			map.StartupLevel,
			map.IsWIP,
			map.Issues) {
			MapPath = folderPath;
		}

		// Get map info from folder
		public LoadableMap(string folderPath) : base() {
			LoadMap(folderPath);
		}

		public void LoadMap(string folderPath) {
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
			}

			LoadMapInfo();
			VerifyMap();
		}

		public void LoadMapInfo() {
			var mapInfoPath = Path.Combine(MapPath, Program.MapInfoFileName);
			Author = null;
			Version = null;
			ShortDescription = null;
			LongDescription = null;
			StartupLevel = null;
			IsWIP = false;
			if (File.Exists(mapInfoPath)) {
				Dictionary<string, string> mapInfo = Program.ReadInfoFile(mapInfoPath, new List<string>() { Text.MapInfoFileLongDescription });
				if (mapInfo.ContainsKey(Text.MapInfoFileMapName)) {
					Name = mapInfo[Text.MapInfoFileMapName];
				}
				if (mapInfo.ContainsKey(Text.MapInfoFileVersion)) {
					Version = mapInfo[Text.MapInfoFileVersion];
				}
				if (mapInfo.ContainsKey(Text.MapInfoFileAuthorName)) {
					Author = mapInfo[Text.MapInfoFileAuthorName];
				}
				if (mapInfo.ContainsKey(Text.MapInfoFileShortDescription)) {
					ShortDescription = mapInfo[Text.MapInfoFileShortDescription];
				}
				if (mapInfo.ContainsKey(Text.MapInfoFileLongDescription)) {
					LongDescription = mapInfo[Text.MapInfoFileLongDescription];
				}
				if (mapInfo.ContainsKey(Text.MapInfoFileStartupLevel)) {
					StartupLevel = mapInfo[Text.MapInfoFileStartupLevel];
				}
				if (mapInfo.ContainsKey(Text.MapInfoFileIsWIP)) {
					IsWIP = Setting.Convert(mapInfo, Text.MapInfoFileIsWIP, Setting.ConversionTable.TrueFalse);
				}
			}
		}

		// Returns the map's path relative to the path passed as the argument.
		public string GetRelativePath(string rootPath) {
			if (MapPath.StartsWith(rootPath)) {
				string relativePath = MapPath.Substring(rootPath.Length);
				return relativePath;
			} else {
				return MapPath;
			}
		}

		public void SaveMapInfo() {
			var mapInfo = new Dictionary<string, string>() {
				[Text.MapInfoFileMapName] = Name,
				[Text.MapInfoFileVersion] = Version,
				[Text.MapInfoFileAuthorName] = Author,
				[Text.MapInfoFileShortDescription] = ShortDescription,
				[Text.MapInfoFileLongDescription] = LongDescription,
				[Text.MapInfoFileStartupLevel] = StartupLevel,
				[Text.MapInfoFileIsWIP] = IsWIP.ToString(),
			};
			string infoFilePath = Path.Combine(MapPath, Program.MapInfoFileName);
			Program.SaveInfoFile(infoFilePath, mapInfo);
		}
	}
}
