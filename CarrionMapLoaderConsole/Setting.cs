using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CarrionManagerConsole
{
	class Setting
	{
		public static List<string> MissingSettings { get; set; }
		public static Dictionary<string, string> SettingDescriptions { get; set; }

		public static bool Convert(Dictionary<string, string> settings, string settingKey, Dictionary<string, bool> conversionTable) {
			if (!VerifySettingContained(settings, settingKey)) {
				return false;
			}
			var settingValue = settings[settingKey];
			if (conversionTable.ContainsKey(settingValue)) {
				return conversionTable[settingValue];
			} else {
				WriteInvalidValue(settingKey, settingValue, DictionaryKeysToStringArray(conversionTable));
				throw new Exception(Text.SettingsInvalid);
			}
		}

		public static Properties.GameLaunchMethod Convert(Dictionary<string, string> settings, string settingKey, Dictionary<string, Properties.GameLaunchMethod> conversionTable) {
			if (!VerifySettingContained(settings, settingKey)) {
				return Properties.GameLaunchMethod.Steam;
			}
			var settingValue = settings[settingKey];
			if (conversionTable.ContainsKey(settingValue)) {
				return conversionTable[settingValue];
			} else {
				WriteInvalidValue(settingKey, settingValue, DictionaryKeysToStringArray(conversionTable));
				throw new Exception(Text.SettingsInvalid);
			}
		}

		public static string[] DictionaryKeysToStringArray(Dictionary<string, bool> dictionary) {
			string[] ret = new string[dictionary.Count];
			dictionary.Keys.CopyTo(ret, 0);
			return ret;
		}

		public static string[] DictionaryKeysToStringArray(Dictionary<string, Properties.GameLaunchMethod> dictionary) {
			string[] ret = new string[dictionary.Count];
			dictionary.Keys.CopyTo(ret, 0);
			return ret;
		}

		public static string GetDirectoryPath(Dictionary<string, string> settings, string settingKey) {
			var path = GetPath(settings, settingKey);
			if (MissingSettings.Contains(settingKey)) {
				return path;
			}
			if (Directory.Exists(path)) {
				return path;
			} else {
				GUI.SetErrorColors();
				Console.WriteLine(Text.ConfigInvalidDirectoryPath, settingKey, path);
				throw new Exception(Text.SettingsInvalid);
			}
		}

		public static string GetPath(Dictionary<string, string> settings, string settingKey) {
			if (!VerifySettingContained(settings, settingKey)) {
				return string.Empty;
			}
			var path = settings[settingKey];
			if (path.StartsWith(Text.PathCurrentDirectoryIndicator)) {
				path = Path.Combine(
					Directory.GetCurrentDirectory(),
					path.Substring(Text.PathCurrentDirectoryIndicator.Length));
			} else if (path.ToLower().StartsWith(Text.PathUserDirectoryIndicator)) {
				path = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
					path.Substring(Text.PathUserDirectoryIndicator.Length));
			}
			return path;
		}

		public static string GetFilePath(Dictionary<string, string> settings, string settingKey) {
			var path = GetPath(settings, settingKey);
			if (MissingSettings.Contains(settingKey)) {
				return path;
			}
			if (File.Exists(path)) {
				return path;
			} else {
				GUI.SetErrorColors();
				Console.WriteLine(Text.ConfigInvalidFilePath, settingKey, path);
				throw new Exception(Text.SettingsInvalid);
			}
		}

		public static void Init() {
			MissingSettings = new List<string>();
			SettingDescriptions = new Dictionary<string, string>() {
				[Text.ConfigLaunchMethod] = Text.ConfigLaunchMethodDescription,
				[Text.ConfigSteamPath] = Text.ConfigSteamPathDescription,
				[Text.ConfigGamePath] = Text.ConfigGamePathDescription,
				[Text.ConfigBackupsPath] = Text.ConfigBackupsPathDescription,
				[Text.ConfigCustomMapsPath] = Text.ConfigCustomMapsPathDescription,
				[Text.ConfigAppDataPath] = Text.ConfigAppDataPathDescription,
				[Text.ConfigManageSaves] = Text.ConfigManageSavesDescription,
				[Text.ConfigZippedMapsPath] = Text.ConfigZippedMapsPathDescription,
			};
			ConversionTable.GameLaunchMethod = new Dictionary<string, Properties.GameLaunchMethod>() {
				[Text.ConfigLaunchMethodDirectly] = Properties.GameLaunchMethod.Directly,
				[Text.ConfigLaunchMethodSteam] = Properties.GameLaunchMethod.Steam,
			};
			ConversionTable.TrueFalse = new Dictionary<string, bool>() {
				[Text.True] = true,
				[Text.False] = false,
			};

		}

		public static void WriteAllSettingDescriptions() {
			foreach (var setting in SettingDescriptions) {
				Console.WriteLine(setting.Value);
			}
		}

		public static void WriteInvalidValue(string settingKey, string settingValue, string[] allowedValues) {
			GUI.SetErrorColors();
			Console.WriteLine(Text.ConfigInvalidValue, settingKey, settingValue);
			Console.WriteLine(Text.ConfigAllowedValues);
			foreach (var allowedValue in allowedValues) {
				Console.WriteLine(allowedValue);
			}
		}

		public static bool VerifySettingContained(Dictionary<string, string> settings, string settingKey) {
			if (settings.ContainsKey(settingKey)) {
				return true;
			} else {
				MissingSettings.Add(settingKey);
				return false;
			}
		}

		public static class ConversionTable
		{
			public static Dictionary<string, Properties.GameLaunchMethod> GameLaunchMethod { get; set; }
			public static Dictionary<string, bool> TrueFalse { get; set; }
		}
	}
}
