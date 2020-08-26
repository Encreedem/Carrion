using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace CarrionManagerConsole
{
	class Program
	{
		/* Carrion Manager Console
		 * Author: Encreedem
		 * 
		 * ### TODO ###
		 * Backup files before overwriting them
		 * Restore files after uninstalling a map
		 * Show how to prepare custom maps: Add a folder + map to this program's root folder.
		 * Check whether GOG needs a custom way to launch Carrion
		 * Check whether other platforms exist for Carrion
		 * Logger
		 * 
		 * ### To consider ###
		 * --- Windows ---
		 * Setup
		 * Backup Maps/other content files
		 * Settings
		 * Map Extractor (maybe with File Explorer)
		 * 
		 * ### Mapping Tools ###
		 * Add installed map manually
		 * Window to configure map info and add/remove levels
		 * Verify map
		 * Export map
		 * 
		 * --- Settings ---
		 * Override Prompt: Confirm before files would be overridden.
		 * */
		public const string
			ProgramName = "Carrion Manager Console",
			ProgramVersion = "v0.2 alpha";

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
			InstalledMapsFileName = "InstalledMaps.txt",
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

		public static Dictionary<ConsoleKey, Properties.Command> keybindings;
		public static List<LoadableMap> availableMaps;
		public static List<Map> installedMaps;
		public static bool quit;
		public static bool manageSaves;
		public static Properties.GameLaunchMethod gameLaunchMethod;

		// Windows
		public static string[] windowNames;
		public static IWindow currentWindow;
		public static LauncherWindow launcherWindow;
		public static NavigationWindow navigationWindow;
		public static MapInstallerWindow mapInstallerWindow;
		public static SaveManagerWindow saveManagerWindow;
		public static BackupsWindow backupsWindow;

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
				if (Console.WindowWidth <= MinConsoleWidth) {
					Console.WindowWidth = MinConsoleWidth;
				}
				if (Console.WindowHeight <= MinConsoleHeight) {
					Console.WindowHeight = MinConsoleHeight;
				}
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

		private static void InitFields() {
			quit = false;
			Console.CursorVisible = false;
			Console.Title = string.Format("{0} {1}", ProgramName, ProgramVersion);
			Console.Title = "Carrion Manager Console v0.1 Alpha";
			keybindings = new Dictionary<ConsoleKey, Properties.Command> {
				[ConsoleKey.UpArrow] = Properties.Command.NavigateUp,
				[ConsoleKey.RightArrow] = Properties.Command.NavigateRight,
				[ConsoleKey.DownArrow] = Properties.Command.NavigateDown,
				[ConsoleKey.LeftArrow] = Properties.Command.NavigateLeft,
				[ConsoleKey.PageUp] = Properties.Command.PageUp,
				[ConsoleKey.PageDown] = Properties.Command.PageDown,
				[ConsoleKey.Enter] = Properties.Command.Confirm,
				[ConsoleKey.Spacebar] = Properties.Command.Confirm,
				[ConsoleKey.Escape] = Properties.Command.Cancel,
				[ConsoleKey.D1] = Properties.Command.ShowLauncher,
				[ConsoleKey.NumPad1] = Properties.Command.ShowLauncher,
				[ConsoleKey.D2] = Properties.Command.ShowMapInstaller,
				[ConsoleKey.NumPad2] = Properties.Command.ShowMapInstaller,
				[ConsoleKey.D3] = Properties.Command.ShowSaveManager,
				[ConsoleKey.NumPad3] = Properties.Command.ShowSaveManager,
				[ConsoleKey.D4] = Properties.Command.ShowBackupsWindow,
				[ConsoleKey.NumPad4] = Properties.Command.ShowBackupsWindow,
			};
		}

		private static void InitWindows() {
			Console.WriteLine("Initializing Windows...");
			windowNames = new string[] {
				Text.LauncherWindowTitle,
				Text.MapInstallerWindowTitle,
				Text.SaveManagerWindowTitle,
				Text.BackupsWindowTitle,
			};

			navigationWindow = new NavigationWindow();
			launcherWindow = new LauncherWindow();
			mapInstallerWindow = new MapInstallerWindow();
			saveManagerWindow = new SaveManagerWindow();
			backupsWindow = new BackupsWindow();

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
				ret[i] = maps[i].ToString();
			}
			return ret;
		}

		public static string[] MapListToStringArray(List<LoadableMap> maps) {
			var ret = new string[maps.Count];
			for (int i = 0; i < ret.Length; ++i) {
				var map = maps[i];
				if (map.Issues.Count > 0) {
					ret[i] = Text.MapHasIssuesIndicator + map.ToString();
				} else {
					ret[i] = map.ToString();
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

		public static Dictionary<string, string> ReadInfoFile(string path) {
			var ret = new Dictionary<string, string>();
			string fileText = File.ReadAllText(path);
			fileText = fileText.Replace("\r", "");
			Regex regex = new Regex(@"^(.+?)[ ]*=[ ]*""?(.+?)""?[ ]*$", RegexOptions.Multiline);
			foreach (Match match in regex.Matches(fileText)) {
				ret.Add(match.Groups[1].Value, match.Groups[2].Value);
			}
			return ret;
		}

		public static string RemoveLevelExtension(string levelName) {
			if (!levelName.EndsWith(LevelFileExtension)) {
				throw new Exception(string.Format("RemoveLevelExtension: \"{0}\" is not a valid file name!", levelName));
			}
			return levelName.Substring(0, levelName.Length - LevelFileExtension.Length);
		}

		public static void SaveInstalledMaps() {
			string json = JsonConvert.SerializeObject(installedMaps.ToArray());
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
				fileText += string.Format("{0}={1}{2}", setting.Key, setting.Value, Environment.NewLine);
			}
			File.WriteAllText(path, fileText);
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
