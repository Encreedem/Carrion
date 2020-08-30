using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using CarrionManagerConsole.Windows;

namespace CarrionManagerConsole
{
	class Program
	{
		/* Carrion Manager Console
		 * Author: Encreedem
		 * 
		 * ### TODO ###
		 * Defensive programming when handling files
		 * Verify before overwriting backups
		 * Show how to prepare custom maps: Add a folder + map to this program's root folder.
		 * Check whether GOG needs a custom way to launch Carrion
		 * Check whether other platforms exist for Carrion
		 * Logger
		 * "WIP" tag for levels that shouldn't be uninstalled.
		 * Add a proper Readme.txt.
		 * 
		 * ### To consider ###
		 * Setup Wizard/Extractor
		 * Settings Window
		 * Map Extractor (maybe with File Explorer)
		 * 
		 * ### Mapping Tools ###
		 * Add installed map manually
		 * - Would require checkbox + container for checkboxes
		 * Window to configure map info and add/remove levels
		 * Verify map
		 * Export map
		 * */
		public const string
			ProgramName = "Carrion Manager Console",
			ProgramVersion = "v0.2 Alpha";

		public const int
			MinConsoleWidth = 120,
			MinConsoleHeight = 13;

		#region Files and Paths
		public const string
			LevelFolderName = "Levels", LevelFileExtension = ".json",
			ScriptFolderName = "Scripts", ScriptFileExtension = ".cgs",
			ContentFolderName = "Content",
			SaveFolderName = "Saves", SaveFileExtension = ".crn",
			BackupFolderName = "Backups",
			SavesBackupsFolderName = "Saves", SaveInfoFileName = "SaveInfo.txt",
			LevelBackupsFolderName = "Levels",
			ScriptBackupsFolderName = "Scripts",
			ZippedFileExtension = ".zip";
		public const string
			SteamRegistryPath = @"Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",
			SteamRegistryKey = "InstallPath",
			GameExeName = "Carrion.exe",
			ConfigFileName = "CarrionManagerConsole.cfg",
			InstalledMapsFileName = "InstalledMaps.json",
			MapInfoFileName = "MapInfo.txt";
		public const string
			LaunchSteamGameArgument = "-applaunch 953490",
			CustomLevelArgument = " -level {0}";
		public const int MaxSubfolderIterations = 3; // The maximum number of nested subfolders this program will look through to find a custom map.

		public static string
			appDataPath,
			backupsPath,
			configFilePath,
			customMapsPath,
			gameContentPath,
			gameRootPath,
			gameExePath,
			installedLevelsPath,
			installedMapsPath,
			installedScriptsPath,
			levelBackupsPath,
			scriptBackupsPath,
			steamPath,
			saveBackupsPath,
			saveInfoFilePath,
			saveFolderPath,
			zippedMapsFolder;
		#endregion

		public static Dictionary<ConsoleKey, Properties.Command> navigationKeybindings;
		public static Dictionary<ConsoleKey, Properties.Command> textInputKeybindings;
		public static List<LoadableMap> availableMaps;
		public static List<Map> installedMaps;
		public static bool quit;
		public static bool manageSaves;
		public static Properties.GameLaunchMethod gameLaunchMethod;
		public static bool mappingToolsEnabled;

		// Windows
		public static string[] windowNames;
		public static IWindow currentWindow;
		public static LauncherWindow launcherWindow;
		public static NavigationWindow navigationWindow;
		public static MapInstallerWindow mapInstallerWindow;
		public static SaveManagerWindow saveManagerWindow;
		public static BackupsWindow backupsWindow;
		public static MappingToolsWindow mappingToolsWindow;

		public static string GameLaunchMethodToString(Properties.GameLaunchMethod gameLaunchMethod) {
			return gameLaunchMethod switch
			{
				Properties.GameLaunchMethod.Directly => Text.ConfigLaunchMethodDirectly,
				Properties.GameLaunchMethod.Steam => Text.ConfigLaunchMethodSteam,
				_ => throw new Exception(string.Format("Invalid game launch method \"{0}\"", gameLaunchMethod.ToString())),
			};
		}

		private static void Init() {
			try {
				Console.WriteLine("Initializing...");
				InitConsole();
				InitFields();
				LoadDefaultSettings();
				LoadSettings();
				LoadInstalledMaps();
				LoadAvailableMaps();
				InitWindows();
				Console.Clear();
			} catch (Exception e) {
				Console.BackgroundColor = MenuColor.ErrorBG;
				Console.ForegroundColor = MenuColor.ErrorFG;
				Console.WriteLine("\nInitialization failed! Error message:");
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
				Console.WriteLine("\nPress any key to exit...");
				Console.ReadKey();
				Environment.Exit(1);
			}
		}

		private static void InitConsole() {
			if (Console.WindowWidth <= MinConsoleWidth) {
				Console.WindowWidth = MinConsoleWidth;
			}
			if (Console.WindowHeight <= MinConsoleHeight) {
				Console.WindowHeight = MinConsoleHeight;
			}
			Console.CursorVisible = false;
			Console.Title = string.Format("{0} {1}", ProgramName, ProgramVersion);
		}

		private static void InitFields() {
			quit = false;
			navigationKeybindings = new Dictionary<ConsoleKey, Properties.Command> {
				[ConsoleKey.UpArrow] = Properties.Command.NavigateUp,
				[ConsoleKey.RightArrow] = Properties.Command.NavigateRight,
				[ConsoleKey.DownArrow] = Properties.Command.NavigateDown,
				[ConsoleKey.LeftArrow] = Properties.Command.NavigateLeft,
				[ConsoleKey.PageUp] = Properties.Command.PageUp,
				[ConsoleKey.PageDown] = Properties.Command.PageDown,
				[ConsoleKey.Enter] = Properties.Command.Confirm,
				[ConsoleKey.Spacebar] = Properties.Command.Confirm,
				[ConsoleKey.Escape] = Properties.Command.Cancel,
				[ConsoleKey.NumPad0] = Properties.Command.Cancel,
				[ConsoleKey.D1] = Properties.Command.ShowLauncher,
				[ConsoleKey.NumPad1] = Properties.Command.ShowLauncher,
				[ConsoleKey.D2] = Properties.Command.ShowMapInstaller,
				[ConsoleKey.NumPad2] = Properties.Command.ShowMapInstaller,
				[ConsoleKey.D3] = Properties.Command.ShowSaveManager,
				[ConsoleKey.NumPad3] = Properties.Command.ShowSaveManager,
				[ConsoleKey.D4] = Properties.Command.ShowBackupsWindow,
				[ConsoleKey.NumPad4] = Properties.Command.ShowBackupsWindow,
				[ConsoleKey.D5] = Properties.Command.ShowMappingToolsWindow,
				[ConsoleKey.NumPad5] = Properties.Command.ShowMappingToolsWindow,
			};
			textInputKeybindings = new Dictionary<ConsoleKey, Properties.Command>() {
				[ConsoleKey.Escape] = Properties.Command.Cancel,
				[ConsoleKey.Enter] = Properties.Command.Confirm,
				[ConsoleKey.LeftArrow] = Properties.Command.NavigateLeft,
				[ConsoleKey.RightArrow] = Properties.Command.NavigateRight,
				[ConsoleKey.UpArrow] = Properties.Command.NavigateUp,
				[ConsoleKey.DownArrow] = Properties.Command.NavigateDown,
				[ConsoleKey.Home] = Properties.Command.GoToStart,
				[ConsoleKey.End] = Properties.Command.GoToEnd,
				[ConsoleKey.Delete] = Properties.Command.DeleteCurrentCharacter,
				[ConsoleKey.Backspace] = Properties.Command.DeletePreviousCharacter,
			};
		}

		private static void InitWindows() {
			Console.WriteLine("Initializing Windows...");
			if (mappingToolsEnabled) {
				windowNames = new string[] {
					Text.LauncherWindowTitle,
					Text.MapInstallerWindowTitle,
					Text.SaveManagerWindowTitle,
					Text.BackupsWindowTitle,
					Text.MappingToolsWindowTitle,
				};
			} else {
				windowNames = new string[] {
					Text.LauncherWindowTitle,
					Text.MapInstallerWindowTitle,
					Text.SaveManagerWindowTitle,
					Text.BackupsWindowTitle,
				};
			}

			navigationWindow = new NavigationWindow();
			launcherWindow = new LauncherWindow();
			mapInstallerWindow = new MapInstallerWindow();
			saveManagerWindow = new SaveManagerWindow();
			backupsWindow = new BackupsWindow();
			mappingToolsWindow = new MappingToolsWindow();

			currentWindow = navigationWindow;
		}

		private static void LoadAvailableMaps() {
			Console.WriteLine("Loading available custom maps...");
			Console.WriteLine(customMapsPath);
			if (Directory.Exists(customMapsPath)) {
				availableMaps = ProcessCustomMapDirectory(customMapsPath, 0);
			} else {
				availableMaps = new List<LoadableMap>();
			}
		}

		private static void LoadDefaultSettings() {
			gameLaunchMethod = Properties.GameLaunchMethod.Steam;
			steamPath = string.Format("C:{0}Program Files (x86){0}Steam{0}steam.exe", Path.DirectorySeparatorChar);
			gameRootPath = string.Format("C:{0}Program Files (x86){0}Steam{0}SteamApps{0}common{0}Carrion{0}", Path.DirectorySeparatorChar);
			backupsPath = string.Format(".{0}Backups{0}", Path.DirectorySeparatorChar);
			customMapsPath = string.Format(".{0}Custom Maps{0}", Path.DirectorySeparatorChar);
			appDataPath = string.Format("C:{0}Users{0}your_username{0}AppData{0}LocalLow{0}Phobia{0}Carrion{0}_steam_xxxxxxxxxxxxxxxxx{0}", Path.DirectorySeparatorChar);
			zippedMapsFolder = string.Format("[user]{0}Downloads", Path.DirectorySeparatorChar);
			mappingToolsEnabled = false;
			manageSaves = true;
		}

		private static void LoadInstalledMaps() {
			Console.WriteLine("Loading installed maps...");
			// Check whether game root folder is valid
			if (!Directory.Exists(gameContentPath)) {
				Console.WriteLine("ERROR: Carrion Content path \"{0}\" is invalid.", gameContentPath);
				Console.WriteLine(string.Format("Specify the correct path in \"{0}\".", ConfigFileName));
				Console.WriteLine("e.g.: GameContentPath = \"C:\\Program Files\\Steam\\steamapps\\common\\Carrion\\Content\"");
				Console.WriteLine();
				throw new Exception("Installed maps couldn't be loaded. See previous errors for details.");
			}
			if (File.Exists(installedMapsPath)) {
				string installedMapsJson = File.ReadAllText(installedMapsPath);
				installedMaps = JsonConvert.DeserializeObject<List<Map>>(installedMapsJson);
			} else {
				installedMaps = new List<Map>();
			}
		}

		private static void LoadSettings() {
			Console.WriteLine("Loading settings file...");

			Setting.Init();
			var thisPath = Directory.GetCurrentDirectory();
			configFilePath = Path.Combine(thisPath, ConfigFileName);

			Console.WriteLine();
			if (!File.Exists(configFilePath)) {
				Console.WriteLine("ERROR: Config file not found. New one with default settings was created:");
				Console.WriteLine(configFilePath);
				Console.WriteLine("\nOpen this file and adjust all settings:");
				Setting.WriteAllSettingDescriptions();
				Console.WriteLine();
				LoadDefaultSettings();
				SaveSettings();
				throw new Exception(Text.SettingsInvalid);
			}
			Console.WriteLine(configFilePath);
			var settings = ReadInfoFile(configFilePath);

			gameLaunchMethod = Setting.Convert(settings, Text.ConfigLaunchMethod, Setting.ConversionTable.GameLaunchMethod);
			if (!Setting.MissingSettings.Contains(Text.ConfigLaunchMethod) &&
				gameLaunchMethod == Properties.GameLaunchMethod.Steam) {
				steamPath = Setting.GetFilePath(settings, Text.ConfigSteamPath);
			}
			gameRootPath = Setting.GetDirectoryPath(settings, Text.ConfigGamePath);
			backupsPath = Setting.GetDirectoryPath(settings, Text.ConfigBackupsPath);
			customMapsPath = Setting.GetDirectoryPath(settings, Text.ConfigCustomMapsPath);
			appDataPath = Setting.GetDirectoryPath(settings, Text.ConfigAppDataPath);
			manageSaves = Setting.Convert(settings, Text.ConfigManageSaves, Setting.ConversionTable.TrueFalse);
			zippedMapsFolder = Setting.GetDirectoryPath(settings, Text.ConfigZippedMapsPath);
			mappingToolsEnabled = Setting.Convert(settings, Text.ConfigMappingTools, Setting.ConversionTable.TrueFalse);

			if (Setting.MissingSettings.Count > 0) {
				Console.WriteLine(Text.OneOrMoreSettingsAreMissing);
				foreach (var setting in Setting.MissingSettings) {
					if (Setting.SettingDescriptions.ContainsKey(setting)) {
						Console.WriteLine(Setting.SettingDescriptions[setting]);
					} else {
						Console.WriteLine(setting);
					}
				}
				throw new Exception(Text.SettingsCouldNotBeLoaded);
			}

			gameExePath = Path.Combine(gameRootPath, GameExeName);
			gameContentPath = Path.Combine(gameRootPath, ContentFolderName);
			installedMapsPath = Path.Combine(Directory.GetCurrentDirectory(), InstalledMapsFileName);
			installedLevelsPath = Path.Combine(gameContentPath, LevelFolderName);
			installedScriptsPath = Path.Combine(gameContentPath, ScriptFolderName);
			levelBackupsPath = Path.Combine(backupsPath, LevelBackupsFolderName);
			scriptBackupsPath = Path.Combine(backupsPath, ScriptBackupsFolderName);
			saveFolderPath = Path.Combine(appDataPath, SaveFolderName);
			saveInfoFilePath = Path.Combine(saveFolderPath, SaveInfoFileName);
			saveBackupsPath = Path.Combine(backupsPath, SavesBackupsFolderName);
			Directory.CreateDirectory(levelBackupsPath);
			Directory.CreateDirectory(saveBackupsPath);
			Directory.CreateDirectory(scriptBackupsPath);
		}

		public static string[] MapListToStringArray(List<Map> maps) {
			var ret = new string[maps.Count];
			for (int i = 0; i < ret.Length; ++i) {
				var map = maps[i];
				if (map.IsValid) {
					ret[i] = map.Name;
				} else {
					ret[i] = Text.MapHasIssuesIndicator + map.Name;
				}
			}
			return ret;
		}

		public static string[] MapListToStringArray(List<LoadableMap> maps) {
			var ret = new string[maps.Count];
			for (int i = 0; i < ret.Length; ++i) {
				var map = maps[i];
				if (map.Issues.Count > 0) {
					ret[i] = Text.MapHasIssuesIndicator + map.Name;
				} else {
					ret[i] = map.Name;
				}
			}
			return ret;
		}

		// Returns all maps in the specified folder.
		private static List<LoadableMap> ProcessCustomMapDirectory(string folderPath, int iteration) {
			var ret = new List<LoadableMap>();
			try {
				// Check if current directory is a map folder.
				if (Directory.Exists(Path.Combine(folderPath, LevelFolderName))) {
					ret.Add(new LoadableMap(folderPath));
				} else if (iteration < MaxSubfolderIterations) {
					foreach (var directory in Directory.GetDirectories(folderPath)) {
						var subMaps = ProcessCustomMapDirectory(directory, iteration + 1);
						ret.AddRange(subMaps);
					}
				}
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}

			return ret;
		}

		public static Dictionary<string, string> ReadInfoFile(string path, List<string> allowNewLine) {
			var ret = new Dictionary<string, string>();
			string fileText = File.ReadAllText(path);
			fileText = fileText.Replace("\r", "");
			Regex regex = new Regex(@"^(.+?)[ ]*=[ ]*""?(.+?)""?[ ]*$|^" + Text.InfoFileNewLine + "(.+)$", RegexOptions.Multiline);
			var lastKey = string.Empty;
			foreach (Match match in regex.Matches(fileText)) {
				if (match.Groups[0].Value.StartsWith(Text.InfoFileNewLine)) {
					if (allowNewLine.Contains(lastKey)) {
						ret[lastKey] += '\n' + match.Groups[3].Value;
					}
				} else {
					lastKey = match.Groups[1].Value;
					ret.Add(lastKey, match.Groups[2].Value);
				}
			}
			return ret;
		}

		public static Dictionary<string, string> ReadInfoFile(string path) {
			return ReadInfoFile(path, new List<string>());
		}

		public static string RemoveLevelExtension(string levelName) {
			if (!levelName.EndsWith(LevelFileExtension)) {
				throw new Exception(string.Format("RemoveLevelExtension: \"{0}\" is not a valid file name!", levelName));
			}
			return levelName.Substring(0, levelName.Length - LevelFileExtension.Length);
		}

		public static void SaveInstalledMaps() {
			string json = JsonConvert.SerializeObject(installedMaps.ToArray(), Formatting.Indented);
			File.WriteAllText(installedMapsPath, json);
		}

		public static void SaveSettings() {
			var settings = new Dictionary<string, string> {
				[Text.ConfigLaunchMethod] = GameLaunchMethodToString(gameLaunchMethod),
				[Text.ConfigSteamPath] = steamPath,
				[Text.ConfigGamePath] = gameRootPath,
				[Text.ConfigBackupsPath] = backupsPath,
				[Text.ConfigCustomMapsPath] = customMapsPath,
				[Text.ConfigAppDataPath] = appDataPath,
				[Text.ConfigZippedMapsPath] = zippedMapsFolder,
				[Text.ConfigManageSaves] = manageSaves ? Text.True : Text.False,
				[Text.ConfigMappingTools] = mappingToolsEnabled ? Text.True : Text.False,
			};
			var adjustedSettings = new Dictionary<string, string>();
			var currentDirectory = Directory.GetCurrentDirectory();
			var userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			foreach (var setting in settings) {
				if (setting.Value.StartsWith(currentDirectory)) {
					adjustedSettings[setting.Key] = Text.PathCurrentDirectoryIndicator + setting.Value.Substring(currentDirectory.Length + 1);
				} else if (setting.Value.StartsWith(userDirectory)) {
					adjustedSettings[setting.Key] = Text.PathUserDirectoryIndicator + setting.Value.Substring(userDirectory.Length + 1);
				} else {
					adjustedSettings[setting.Key] = setting.Value;
				}
			}
			SaveInfoFile(configFilePath, adjustedSettings);
		}

		public static void SaveInfoFile(string path, Dictionary<string, string> settings) {
			string fileText = string.Empty;
			foreach (var setting in settings) {
				var value = setting.Value.Replace("\n", Environment.NewLine + Text.InfoFileNewLine);
				fileText += string.Format("{0}={1}{2}", setting.Key, setting.Value, Environment.NewLine);
			}
			File.WriteAllText(path, fileText);
		}

		public static List<string> SplitIntoLines(string text, int maxWidth) {
			List<string> ret = new List<string>();
			string[] lines = text.Split("\n");
			var regex = new Regex(@"(\S+)\s*");
			foreach (var line in lines) {
				string currentLine = string.Empty;
				foreach (Match match in regex.Matches(line)) {
					string currentWord = match.Groups[1].Value;

					while (currentWord != string.Empty) {
						if (currentLine == string.Empty) {
							if (currentWord.Length < maxWidth) {
								currentLine = currentWord;
								currentWord = string.Empty;
							} else if (currentWord.Length == maxWidth) {
								ret.Add(currentWord);
								currentWord = string.Empty;
							} else {
								ret.Add(currentWord.Substring(0, maxWidth));
								currentWord = currentWord.Substring(maxWidth + 1);
							}
						} else {
							int remainingCharacters = maxWidth - currentLine.Length;
							if (currentWord.Length + 1 <= remainingCharacters) {
								currentLine += " " + currentWord;
								currentWord = string.Empty;
							} else {
								ret.Add(currentLine);
								currentLine = string.Empty;
							}
						}
					}
				}
				if (currentLine != string.Empty) {
					ret.Add(currentLine);
				}
			}

			return ret;
		}

		static void Main() {
			Init();

			while (!quit) {
				//try {
				currentWindow.Show();
				/*} catch (Exception e) {
					quit = true;
					GUI.Reset();
					Console.BackgroundColor = MenuColor.ErrorBG;
					Console.ForegroundColor = MenuColor.ErrorFG;
					Console.WriteLine(Text.UnexpectedErrorOccured);
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
					Console.WriteLine();
					Console.WriteLine(Text.PressAnyKeyToQuit);
					Console.ReadKey();
				}*/
			}
			Console.SetCursorPosition(0, 0);
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Clear();
		}
	}
}
