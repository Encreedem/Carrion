using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole.Windows
{
	class MappingToolsWindow : DefaultWindow
	{
		private readonly string[] commands, commandsWithIssues;
		private readonly GUI.ListBox mapInfoList;
		private readonly GUI.TextInput
			mapInfoNameInput,
			mapInfoVersionInput,
			mapInfoAuthorInput,
			mapInfoStartupLevelInput,
			mapInfoShortDescriptionInput,
			mapInfoLongDescriptionInput;

		public MappingToolsWindow() : base(Text.MappingToolsWindowTitle, MenuColor.MappingToolsWindowBG, MenuColor.MappingToolsWindowFG) {
			commands = new string[] {
				Text.MappingToolsCommandEditMapInfo,
				Text.MappingToolsCommandEditLevels,
			};
			commandsWithIssues = new string[] {
				Text.MappingToolsCommandEditMapInfo,
				Text.MappingToolsCommandEditLevels,
				Text.ShowIssues,
			};
			MapList = Menu.AddListBox(0, null, true);
			MapList.SelectionChanged += MapListSelectionChanged;
			DetailsTextBox = Menu.AddTextBox(1, null);
			DetailsTextBox.Width--;
			NewMapNameInput = new GUI.TextInput(MapList.Left + 1, 0, MapList.Width - 2, 1, MenuColor.TextInputBG, MenuColor.TextInputFG) {
				PreviewText = Text.MapName,
				DefaultOptions = new GUI.TextInput.PromptOptions(false, true, false, null)
			};
			var optionsNoEmpty = new GUI.TextInput.PromptOptions(false, true, true, null);
			var optionsWithEmpty = new GUI.TextInput.PromptOptions(true, true, true, null);
			mapInfoNameInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoMapName.Length + 1, DetailsTextBox.Top,
				DetailsTextBox.Width - Text.MapInfoMapName.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = optionsNoEmpty,
			};
			mapInfoVersionInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoVersion.Length + 1, DetailsTextBox.Top + 1,
				DetailsTextBox.Width - Text.MapInfoVersion.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = optionsWithEmpty,
			};
			mapInfoAuthorInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoAuthor.Length + 1, DetailsTextBox.Top + 2,
				DetailsTextBox.Width - Text.MapInfoAuthor.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = optionsWithEmpty,
			};
			mapInfoStartupLevelInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoStartupLevel.Length + 1, DetailsTextBox.Top + 3,
				DetailsTextBox.Width - Text.MapInfoStartupLevel.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = optionsWithEmpty,
			};
			mapInfoShortDescriptionInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoShortDescription.Length + 1, DetailsTextBox.Top + 5,
				DetailsTextBox.Width - Text.MapInfoShortDescription.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = optionsWithEmpty,
			};
			mapInfoLongDescriptionInput = new GUI.TextInput(
				DetailsTextBox.Left, DetailsTextBox.Top + 7,
				DetailsTextBox.Width, DetailsTextBox.Height - 5,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = optionsWithEmpty,
			};

			mapInfoList = new GUI.ListBox(
				DetailsTextBox.Left, DetailsTextBox.Top,
				DetailsTextBox.Width + 1, DetailsTextBox.Height,
				MenuColor.ContentBG, MenuColor.ContentBG,
				true);

			RefreshMapList();
		}

		public GUI.CheckBox ShowOnlyWIPCheckBox { get; set; }
		public GUI.ListBox MapList { get; set; }
		public GUI.TextBox DetailsTextBox { get; set; }
		public GUI.TextInput NewMapNameInput { get; set; }

		public void EditMapInfo(Map map) {
			mapInfoNameInput.Text = map.Name;
			mapInfoVersionInput.Text = map.Version ?? string.Empty;
			mapInfoAuthorInput.Text = map.Author ?? string.Empty;
			mapInfoStartupLevelInput.Text = map.StartupLevel ?? string.Empty;
			mapInfoShortDescriptionInput.Text = map.ShortDescription ?? string.Empty;
			mapInfoLongDescriptionInput.Text = map.LongDescription ?? string.Empty;

			mapInfoList.SetItems(new string[] {
				Text.MapInfoMapName + map.Name,
				Text.MapInfoVersion + (string.IsNullOrEmpty(map.Version) ? Text.MapInfoNoVersion : map.Version),
				Text.MapInfoAuthor + (string.IsNullOrEmpty(map.Author) ? Text.MapInfoNoAuthor : map.Author),
				Text.MapInfoStartupLevel + (string.IsNullOrEmpty(map.StartupLevel) ? Text.MapInfoNoStartupLevel : map.StartupLevel),
				Text.MapInfoIsWIP + map.IsWIP.ToString(),
				Text.MapInfoShortDescription + (string.IsNullOrEmpty(map.ShortDescription) ? Text.MapInfoNoDescription : map.ShortDescription),
				Text.MapInfoLongDescription,
			}); ;
			mapInfoList.Draw();
			mapInfoList.NavigateToDefault();

			bool promptQuit = false;
			while (!promptQuit) {
				var result = mapInfoList.PromptSelection();
				bool infoChanged = true;
				switch (result.Command) {
					case Properties.Command.Confirm:
						switch (result.RowIndex) {
							case 0: // Map Name
								if (mapInfoNameInput.PromptText()) {
									map.Name = mapInfoNameInput.Text;
									mapInfoList.Items[0].Value = Text.MapInfoMapName + map.Name;
								}
								break;
							case 1: // Version
								if (mapInfoVersionInput.PromptText()) {
									map.Version = mapInfoVersionInput.Text;
									mapInfoList.Items[1].Value = Text.MapInfoVersion + (string.IsNullOrEmpty(map.Version) ? Text.MapInfoNoVersion : map.Version);
								}
								break;
							case 2: // Author
								if (mapInfoAuthorInput.PromptText()) {
									map.Author = mapInfoAuthorInput.Text;
									mapInfoList.Items[2].Value = Text.MapInfoAuthor + (string.IsNullOrEmpty(map.Author) ? Text.MapInfoNoAuthor : map.Author);
								}
								break;
							case 3: // Startup Level
								if (mapInfoStartupLevelInput.PromptText()) {
									map.StartupLevel = mapInfoStartupLevelInput.Text;
									mapInfoList.Items[3].Value = Text.MapInfoStartupLevel + (string.IsNullOrEmpty(map.StartupLevel) ? Text.MapInfoNoStartupLevel : map.StartupLevel);
									map.VerifyMap();
								}
								break;
							case 4: // Is WIP
								map.IsWIP = !map.IsWIP;
								mapInfoList.Items[4].Value = Text.MapInfoIsWIP + map.IsWIP.ToString();
								break;
							case 5: // Short Description
								if (mapInfoShortDescriptionInput.PromptText()) {
									map.ShortDescription = mapInfoShortDescriptionInput.Text;
									mapInfoList.Items[5].Value = (string.IsNullOrEmpty(map.ShortDescription) ? Text.MapInfoNoDescription : map.ShortDescription);
								}
								break;
							case 6: // Long Description
								if (mapInfoLongDescriptionInput.PromptTextMultiline()) {
									map.LongDescription = mapInfoLongDescriptionInput.Text;
								}
								break;
							default:
								infoChanged = false;
								break;
						}

						if (infoChanged) {
							Program.SaveInstalledMaps();
						}
						break;
					case Properties.Command.Cancel:
						promptQuit = true;
						break;
				}
				mapInfoList.SelectCurrentItem();
			}

			mapInfoList.Clear();
		}

		public List<Map> GetWipMaps() {
			List<Map> maps = new List<Map>();
			foreach (Map map in Program.installedMaps) {
				if (map.IsWIP) {
					maps.Add(map);
				}
			}
			return maps;
		}

		public void MapListSelectionChanged(object Sender, GUI.SelectionChangedEventArgs eventArgs) {
			if (eventArgs.SelectedItemIndex == 0 || eventArgs.SelectedItemIndex == MapList.Items.Count - 1) {
				DetailsTextBox.ClearContent();
			} else {
				DetailsTextBox.WriteAllMapInfo(((GUI.SelectableMap)eventArgs.SelectedItem).Map);
			}
		}

		public override void PreShow() {

		}

		public void RefreshMapList() {
			MapList.Clear();
			bool showOnlyWIP = ShowOnlyWIPCheckBox != null && ShowOnlyWIPCheckBox.Checked;
			MapList.Items.Clear();
			ShowOnlyWIPCheckBox = MapList.AddCheckBox(Text.ShowOnlyWipMaps, showOnlyWIP);
			MapList.AddMaps(ShowOnlyWIPCheckBox.Checked ? GetWipMaps() : Program.installedMaps);
			MapList.AddItem(Text.MappingToolsAddMap);
			MapList.Draw();
		}

		public override void Selected(GUI.Selection selection) {
			if (selection.RowIndex == 0) { // Show only WIP
				ShowOnlyWIPCheckBox.Checked = !ShowOnlyWIPCheckBox.Checked;
				RefreshMapList();
				MapList.NavigateToDefault();
			} else if (selection.RowIndex == MapList.Items.Count - 1) { // Add Map...
				NewMapNameInput.Top = MapList.Top + selection.RowIndex;
				if (NewMapNameInput.PromptText()) {
					var newMap = new Map(
						NewMapNameInput.Text,
						null,
						null,
						null,
						null,
						new string[0],
						new string[0],
						true);
					Program.installedMaps.Add(newMap);
					RefreshMapList();
				}
				MapList.Select(selection.RowIndex);
			} else {
				MapList.HighlightCurrentItem();
				var selectedMap = (GUI.SelectableMap)(selection.SelectedItem);
				var map = selectedMap.Map;
				string[] selections;
				var options = new GUI.SelectionPrompt.Options() {
					AllowCancel = true,
				};
				if (map.IsValid) {
					selections = commands;
				} else {
					selections = commandsWithIssues;
					options.Index = 4;
				}

				int response = SelectionPrompt.PromptSelection(selections, options);
				switch (response) {
					case 0: // Edit Map Info
						EditMapInfo(map);
						break;
					case 1: // Edit Levels
						throw new NotImplementedException();
					case 2: // Show Issues
						map.ShowIssues();
						DrawAll();
						break;
				}

				MapList.SelectCurrentItem();
			}
		}
	}
}
