using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace CarrionManagerConsole
{
	class Program
	{
		/* Carrion Map Loader by Encreedem
		 * 
		 * Application that let's the user easily install/uninstall custom maps for Phobia's game "Carrion".
		 * 
		 * TODO:
		 * Selector for single column (e.g. options) and two columns (e.g. install/uninstall window).
		 * - Mandatory: Display key/values. Return selected key.
		 * - Optional: Folders that can be collapsed/expanded.
		 * - Optional: Checkbox for boolean variables
		 * 
		 * Store installed levels and their components.
		 * 
		 * ### Windows ###
		 * Setup
		 * Install/Uninstall Maps
		 * Backups Saves
		 * Backup Maps
		 * Settings
		 * 
		 * ### Settings ###
		 * Game Path
		 * Custom Levels Path
		 * Override Prompt: Confirm before files would be overridden.
		 * 
		 * Install/Uninstall Maps window:
		 * ------------------------------------------------------------------------------------------------------------------------
		 * Install/Uninstall Maps
		 * 
		 * Installed Maps				Available Maps
		 * map1							Cuni - somemap
		 * map2							also cuni - other map
		 * map3							always cuni - third map
		 * map4
		 * 
		 * [Current status]
		 * 
		 * Arrow Keys/PgUp/PgDown: Navigate    Enter/Space: Confirm    Esc: Quit
		 * */
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
			SavesBackupsFolderName = "Saves", SaveInfoFileName = "SaveInfo.txt";
		public const string
			SteamRegistryPath = @"Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam",
			SteamRegistryKey = "InstallPath",
			GameExeName = "Carrion.exe",
			ConfigFileName = "config.txt",
			InstalledMapsFileName = "installed.txt";
		public const string
			LaunchSteamGameArgument = "-applaunch 953490",
			CustomLevelArgument = " -level {0}";
		public const int MaxSubfolderIterations = 10; // The maximum number of nested subfolders this program will look through to find a custom map.

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
			steamPath,
			saveBackupsPath,
			saveInfoFilePath,
			saveFolderPath;
		#endregion

		public static Dictionary<ConsoleKey, Properties.Command> keybindings;
		public static List<LoadableMap> availableMaps;
		public static List<Map> installedMaps;
		public static bool quit;
		public static bool manageSaves;
		public static Properties.GameLaunchMethod gameLaunchMethod;

		public static readonly GUI.Label controlsLabel;
		// Windows
		public static string[] windowNames;
		public static IWindow currentWindow;
		public static LauncherWindow launcherWindow;
		public static NavigationWindow navigationWindow;
		public static MapInstallerWindow mapInstallerWindow;
		public static SaveManagerWindow saveManagerWindow;

		static Program() {
			controlsLabel = new GUI.Label(0, Console.WindowHeight - 1, Console.WindowWidth - 1, 1, MenuColor.ControlsBG, MenuColor.ControlsFG, Text.DefaultControls);
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
				Console.WriteLine("\nPress any key to exit...");
				Console.ReadKey();
				Environment.Exit(1);
			}
		}

		private static void InitFields() {
			quit = false;
			Console.CursorVisible = false;
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
			};
			installedMapsPath = Path.Combine(Directory.GetCurrentDirectory(), InstalledMapsFileName);
		}

		private static void InitWindows() {
			if (Console.WindowWidth < Text.DefaultControls.Length) {
				controlsLabel.Text = Text.DefaultControlsShort;
			}
			windowNames = new string[] {
				Text.LauncherWindowTitle,
				Text.MapInstallerWindowTitle,
				Text.SaveManagerTitle,
			};

			navigationWindow = new NavigationWindow();
			launcherWindow = new LauncherWindow();
			mapInstallerWindow = new MapInstallerWindow();
			saveManagerWindow = new SaveManagerWindow();

			currentWindow = navigationWindow;
		}

		private static void LoadAvailableMaps() {
			Console.WriteLine("Loading available custom maps...");
			// Check whether custom map folder is valid
			if (!Directory.Exists(customMapsPath)) {
				Console.WriteLine("ERROR: Custom map path \"{0}\" is invalid.", customMapsPath);
				Console.WriteLine(String.Format("Specify the correct path in \"{0}\".", ConfigFileName));
				Console.WriteLine();
				throw new Exception("Custom maps couldn't be loaded. See previous errors for details.");
			}
			availableMaps = ProcessCustomMapDirectory(customMapsPath, 0);
		}

		private static void LoadDefaultSettings() {
			gameLaunchMethod = Properties.GameLaunchMethod.Steam;
			steamPath = @"C:\Program Files (x86)\Steam\steam.exe";
			gameRootPath = @"C:\Program Files (x86)\Steam\SteamApps\common\Carrion";
			customMapsPath = @"C:\Games\Modding\Carrion\Custom Maps";
			appDataPath = @"C:\Users\your_username\AppData\LocalLow\Phobia\Carrion\_steam_xxxxxxxxxxxxxxxxx";
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
			var thisPath = Directory.GetCurrentDirectory();
			configFilePath = Path.Combine(thisPath, ConfigFileName);

			Console.WriteLine();
			if (!File.Exists(configFilePath)) {
				Console.WriteLine("ERROR: Config file not found. New one with default settings was created:");
				Console.WriteLine(configFilePath);
				Console.WriteLine("\nOpen this file and adjust all settings:");
				Console.WriteLine(Text.ConfigLaunchMethodDescription);
				Console.WriteLine(Text.ConfigSteamPathDescription);
				Console.WriteLine(Text.ConfigGamePathDescription);
				Console.WriteLine(Text.ConfigCustomMapsPathDescription);
				Console.WriteLine(Text.ConfigAppDataPathDescription);
				Console.WriteLine(Text.ConfigManageSavesDescription);
				Console.WriteLine();
				LoadDefaultSettings();
				SaveSettings();
				throw new Exception("Settings could not be loaded! See previous messages for details.");
			}
			Console.WriteLine(configFilePath);
			var settings = ReadInfoFile(configFilePath);
			List<string> missingSettings = new List<string>();

			if (!settings.ContainsKey(Text.ConfigLaunchMethod)) {
				missingSettings.Add(Text.ConfigLaunchMethodDescription);
			} else if (settings[Text.ConfigLaunchMethod].ToLower() == Text.ConfigLaunchMethodDirectly) {
				gameLaunchMethod = Properties.GameLaunchMethod.Directly;
			} else if (settings[Text.ConfigLaunchMethod].ToLower() == Text.ConfigLaunchMethodSteam) {
				gameLaunchMethod = Properties.GameLaunchMethod.Steam;
			} else {
				Console.WriteLine("Setting \"{0}\", value \"{1}\" is invalid! Allowed values:",
					Text.ConfigLaunchMethod,
					settings[Text.ConfigLaunchMethod].ToString());
				Console.WriteLine(Text.ConfigLaunchMethodDirectly);
				Console.WriteLine(Text.ConfigLaunchMethodSteam);
				Console.WriteLine();
				throw new Exception("Settings could not be loaded! See previous messages for details.");
			}

			if (gameLaunchMethod == Properties.GameLaunchMethod.Steam &&
				!settings.ContainsKey(Text.ConfigSteamPath)) {
				missingSettings.Add(Text.ConfigSteamPathDescription);
			}
			if (!settings.ContainsKey(Text.ConfigGamePath)) {
				missingSettings.Add(Text.ConfigGamePathDescription);
			}
			if (!settings.ContainsKey(Text.ConfigCustomMapsPath)) {
				missingSettings.Add(Text.ConfigCustomMapsPathDescription);
			}
			if (!settings.ContainsKey("AppDataPath")) {
				missingSettings.Add(Text.ConfigAppDataPathDescription);
			}
			if (!settings.ContainsKey(Text.ConfigManageSaves)) {
				missingSettings.Add(Text.ConfigManageSavesDescription);
			}

			if (missingSettings.Count > 0) {
				Console.WriteLine("One or more settings are missing:");
				foreach (var setting in missingSettings) {
					Console.WriteLine(setting);
				}
				throw new Exception("Settings could not be loaded! See previous messages for details.");
			}

			if (gameLaunchMethod == Properties.GameLaunchMethod.Steam) {
				steamPath = settings[Text.ConfigSteamPath];
			}
			gameRootPath = settings[Text.ConfigGamePath];
			customMapsPath = settings[Text.ConfigCustomMapsPath];
			appDataPath = settings["AppDataPath"];
			if (settings[Text.ConfigManageSaves].ToLower() == Text.True) {
				manageSaves = true;
			} else if (settings[Text.ConfigManageSaves].ToLower() == Text.False) {
				manageSaves = false;
			} else {
				Console.WriteLine("Setting \"{0}\", value \"{1}\" is invalid! Allowed values:",
					Text.ConfigManageSaves,
					settings[Text.ConfigManageSaves].ToString());
				Console.WriteLine(Text.True);
				Console.WriteLine(Text.False);
				Console.WriteLine();
				throw new Exception("Settings could not be loaded! See previous messages for details.");
			}

			gameExePath = Path.Combine(gameRootPath, GameExeName);
			gameContentPath = Path.Combine(gameRootPath, ContentFolderName);
			installedLevelsPath = Path.Combine(gameContentPath, LevelFolderName);
			installedScriptsPath = Path.Combine(gameContentPath, ScriptFolderName);
			saveFolderPath = Path.Combine(appDataPath, SaveFolderName);
			saveInfoFilePath = Path.Combine(Program.saveFolderPath, Program.SaveInfoFileName);
			backupsPath = Path.Combine(thisPath, BackupFolderName);
			saveBackupsPath = Path.Combine(backupsPath, SavesBackupsFolderName);
			Directory.CreateDirectory(saveBackupsPath);
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
				ret[i] = maps[i].ToString();
			}
			return ret;
		}

		// Returns all maps in the specified folder.
		private static List<LoadableMap> ProcessCustomMapDirectory(string folderPath, int iteration) {
			var ret = new List<LoadableMap>();
			// Check if current directory is a map folder.
			if (Directory.Exists(Path.Combine(folderPath, LevelFolderName))) {
				ret.Add(new LoadableMap(folderPath));
			} else if (iteration < MaxSubfolderIterations) {
				foreach (var directory in Directory.GetDirectories(folderPath)) {
					var subMaps = ProcessCustomMapDirectory(directory, iteration + 1);
					ret.AddRange(subMaps);
				}
			}

			return ret;
		}

		public static Dictionary<string, string> ReadInfoFile(string path) {
			var ret = new Dictionary<string, string>();
			string fileText = File.ReadAllText(path);
			fileText = fileText.Replace("\r", "");
			Regex regex = new Regex(@"^(.+?)=[ ]*""?(.+?)""?$", RegexOptions.Multiline);
			foreach (Match match in regex.Matches(fileText)) {
				ret.Add(match.Groups[1].Value, match.Groups[2].Value);
			}
			return ret;
		}

		public static string RemoveLevelExtension(string levelName) {
			if (!levelName.EndsWith(LevelFileExtension)) {
				throw new Exception(String.Format("RemoveLevelExtension: \"{0}\" is not a valid file name!", levelName));
			}
			return levelName.Substring(0, levelName.Length - LevelFileExtension.Length);
		}

		public static void SaveInstalledMaps() {
			string json = JsonConvert.SerializeObject(installedMaps.ToArray());
			File.WriteAllText(installedMapsPath, json);
		}

		public static void SaveSettings() {
			var settings = new Dictionary<string, string> {
				[Text.ConfigSteamPath] = steamPath,
				[Text.ConfigGamePath] = gameRootPath,
				[Text.ConfigCustomMapsPath] = customMapsPath,
				[Text.ConfigAppDataPath] = appDataPath,
				[Text.ConfigManageSaves] = manageSaves ? Text.True : Text.False,
			};
			SaveInfoFile(configFilePath, settings);
		}

		public static void SaveInfoFile(string path, Dictionary<string, string> settings) {
			string fileText = string.Empty;
			foreach (var setting in settings) {
				fileText += String.Format("{0}={1}{2}", setting.Key, setting.Value, Environment.NewLine);
			}
			File.WriteAllText(path, fileText);
		}

		static void Main() {
			Init();

			while (!quit) {
				currentWindow.Show();
			}
			Console.SetCursorPosition(0, 0);
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Clear();
		}
	}
}
