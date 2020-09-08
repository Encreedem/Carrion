using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CarrionManagerConsole
{
	class MapEditorWindow : DefaultWindow
	{
		private bool ignoreNextSelectionChangedEvent;

		private readonly string[]
			mapCommands,
			mapCommandsWithIssues,
			levelCommands,
			confirmDeleteCommands,
			exportCommands;
		private readonly GUI.ListBox
			mapList,
			mapInfoList,
			levelList,
			assignedLevelsList;
		private readonly GUI.TextInput
			mapInfoNameInput,
			mapInfoVersionInput,
			mapInfoAuthorInput,
			mapInfoStartupLevelInput,
			mapInfoShortDescriptionInput,
			mapInfoLongDescriptionInput,
			levelNameInput,
			levelRenameInput;

		public MapEditorWindow() : base(Text.MapEditorWindowTitle, MenuColor.MapEditorWindowBG, MenuColor.MapEditorWindowFG) {
			ignoreNextSelectionChangedEvent = false;
			mapCommands = new string[] {
				Text.MapEditorCommandEditMapInfo,
				Text.MapEditorCommandEditLevels,
				Text.Export,
			};
			mapCommandsWithIssues = new string[] {
				Text.MapEditorCommandEditMapInfo,
				Text.MapEditorCommandEditLevels,
				Text.Export,
				Text.ShowIssues,
			};
			levelCommands = new string[] {
				Text.Rename,
				Text.Delete,
			};
			confirmDeleteCommands = new string[] {
				Text.Delete,
			};
			exportCommands = new string[] {
				Text.Overwrite,
				Text.ExportWithTimestamp,
			};

			mapList = Menu.AddListBox(0, null, true);
			mapList.SelectionChanged += MapListSelectionChanged;
			DetailsTextBox = Menu.AddTextBox(1, null);
			DetailsTextBox.Width--;
			NewMapNameInput = new GUI.TextInput(mapList.Left + 1, mapList.Top + 1, mapList.Width - 2, 1, MenuColor.TextInputBG, MenuColor.TextInputFG) {
				PreviewText = Text.MapName,
				DefaultOptions = new GUI.TextInput.PromptOptions(false, true, false, null)
			};
			var infoOptionsNoEmpty = new GUI.TextInput.PromptOptions(false, true, true, null);
			var infoOptionsWithEmpty = new GUI.TextInput.PromptOptions(true, true, true, null);
			var nameInputOptions = new GUI.TextInput.PromptOptions(false, true, false, null);
			mapInfoNameInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoMapName.Length + 1, DetailsTextBox.Top,
				DetailsTextBox.Width - Text.MapInfoMapName.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = infoOptionsNoEmpty,
			};
			mapInfoVersionInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoVersion.Length + 1, DetailsTextBox.Top + 1,
				DetailsTextBox.Width - Text.MapInfoVersion.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = infoOptionsWithEmpty,
			};
			mapInfoAuthorInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoAuthor.Length + 1, DetailsTextBox.Top + 2,
				DetailsTextBox.Width - Text.MapInfoAuthor.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = infoOptionsWithEmpty,
			};
			mapInfoStartupLevelInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoStartupLevel.Length + 1, DetailsTextBox.Top + 3,
				DetailsTextBox.Width - Text.MapInfoStartupLevel.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = infoOptionsWithEmpty,
			};
			mapInfoShortDescriptionInput = new GUI.TextInput(
				DetailsTextBox.Left + Text.MapInfoShortDescription.Length + 1, DetailsTextBox.Top + 5,
				DetailsTextBox.Width - Text.MapInfoShortDescription.Length - 1, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = infoOptionsWithEmpty,
			};
			mapInfoLongDescriptionInput = new GUI.TextInput(
				DetailsTextBox.Left, DetailsTextBox.Top + 7,
				DetailsTextBox.Width, DetailsTextBox.Height - 5,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				DefaultOptions = infoOptionsWithEmpty,
			};

			mapInfoList = new GUI.ListBox(
				DetailsTextBox.Left, DetailsTextBox.Top,
				DetailsTextBox.Width + 1, DetailsTextBox.Height,
				MenuColor.ContentBG, MenuColor.ContentBG,
				true);

			levelList = new GUI.ListBox(
				mapList.Left, mapList.Top,
				mapList.Width, mapList.Height,
				MenuColor.ContentBG, MenuColor.ContentFG,
				true);
			assignedLevelsList = new GUI.ListBox(
				DetailsTextBox.Left, DetailsTextBox.Top,
				DetailsTextBox.Width + 1, DetailsTextBox.Height,
				MenuColor.ContentBG, MenuColor.ContentBG,
				true);
			levelNameInput = new GUI.TextInput(
				levelList.Left + 1, levelList.Top,
				levelList.Width - 2, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				PreviewText = Text.LevelName,
				DefaultOptions = nameInputOptions,
			};
			levelNameInput = new GUI.TextInput(
				levelList.Left + 1, levelList.Top,
				levelList.Width - 2, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				PreviewText = Text.LevelName,
				DefaultOptions = nameInputOptions,
			};
			levelRenameInput = new GUI.TextInput(
				levelList.Left + 1, levelList.Top,
				levelList.Width - 2, 1,
				MenuColor.TextInputBG, MenuColor.TextInputFG) {
				PreviewText = Text.LevelName,
				DefaultOptions = infoOptionsNoEmpty,
			};
		}

		public GUI.CheckBox ShowOnlyWIPCheckBox { get; set; }
		public GUI.TextBox DetailsTextBox { get; set; }
		public GUI.TextInput NewMapNameInput { get; set; }

		public override void Selected(GUI.Selection selection) {
			switch (selection.RowIndex) {
				case 0:
					ShowOnlyWIPCheckBox.Checked = !ShowOnlyWIPCheckBox.Checked;
					RefreshMapList();
					mapList.NavigateToDefault();
					break;
				case 1:
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
						Program.SaveInstalledMaps();
						RefreshMapList();
					}
					mapList.Select(mapList.Items.Count - 1);
					break;
				case 2: // Separator
					break;
				default:
					mapList.HighlightCurrentItem();
					var selectedMap = (GUI.SelectableMap)(selection.SelectedItem);
					var map = selectedMap.Map;
					string[] selections;
					var options = new GUI.SelectionPrompt.Options() {
						AllowCancel = true,
					};
					if (map.IsValid) {
						selections = mapCommands;
					} else {
						selections = mapCommandsWithIssues;
						options.Index = 3;
					}

					int response = SelectionPrompt.PromptSelection(selections, options);
					switch (response) {
						case 0: // Edit Map Info
							EditMapInfo(map);
							break;
						case 1: // Edit Levels
							EditLevels(map);
							mapList.Clear();
							mapList.Draw();
							break;
						case 2: // Export
							PromptExportMap(map);
							break;
						case 3: // Show Issues
							map.ShowIssues();
							DrawAll();
							break;
					}

					mapList.SelectCurrentItem();
					break;
			}
		}

		public override void Show() {
			RefreshMapList();
			base.Show();
		}

		private void AssignExistingLevels(Map map) {
			assignedLevelsList.Items.Clear();
			string[] levels = Program.GetInstalledLevelNames();
			string[] levelsWithoutExtension = levels.Select(levelName => Program.RemoveLevelExtension(levelName)).ToArray();
			string[] scripts = levelsWithoutExtension.Select(levelName => levelName + Program.ScriptFileExtension).ToArray();

			for (int i = 0; i < levels.Length; ++i) {
				string levelWithExtension = levels[i];
				string levelWithoutExtension = levelsWithoutExtension[i];
				bool thisMapIncludesLevel = map.Levels.Contains(levelWithExtension);
				var checkBox = assignedLevelsList.AddCheckBox(levelWithoutExtension, thisMapIncludesLevel);
				checkBox.Enabled = thisMapIncludesLevel || !Program.InstalledMapsContainLevel(levelWithExtension);
			}

			assignedLevelsList.Draw();
			assignedLevelsList.NavigateToDefault();
			bool promptQuit = false;
			while (!promptQuit) {
				var selection = assignedLevelsList.PromptSelection();
				switch (selection.Command) {
					case Properties.Command.Confirm:
						var selectedCheckBox = (GUI.CheckBox)selection.SelectedItem;
						string levelName = selectedCheckBox.Value;
						if (selectedCheckBox.Enabled) {
							if (selectedCheckBox.Checked) { // Unassign level
								selectedCheckBox.Checked = false;
								map.RemoveLevel(levelName);
								RefreshLevelList(map);
							} else { // Assign level
								selectedCheckBox.Checked = true;
								map.AddLevel(levelName);
								RefreshLevelList(map);
							}
						}

						assignedLevelsList.SelectedItem.Draw();
						break;
					case Properties.Command.Cancel:
						promptQuit = true;
						break;
				}
			}
		}

		private void EditLevels(Map map) {
			RefreshLevelList(map);
			levelList.NavigateToDefault();
			bool quitPrompt = false;
			while (!quitPrompt) {
				GUI.Selection selection = levelList.PromptSelection();
				switch (selection.Command) {
					case Properties.Command.Confirm:
						switch (selection.RowIndex) {
							case 0: // Add New Level
								if (levelNameInput.PromptText()) {
									LogTextBox.ClearContent();
									string levelName = levelNameInput.Text;
									// Check if level already exists
									string levelDestination = Path.Combine(Program.installedLevelsPath, levelName + Program.LevelFileExtension);
									string scriptDestination = Path.Combine(Program.installedScriptsPath, levelName + Program.ScriptFileExtension);
									if (File.Exists(levelDestination) || File.Exists((scriptDestination))) {
										LogTextBox.WriteLine(string.Format(Text.LevelOrScriptAlreadyExists, levelName + Program.LevelFileExtension, levelName + Program.ScriptFileExtension));
									} else {
										File.Copy(Program.emptyLevelTemplatePath, levelDestination);
										File.Copy(Program.emptyScriptTemplatePath, scriptDestination);
										map.AddLevel(levelName);
										Program.SaveInstalledMaps();
										RefreshLevelList(map);
										LogTextBox.WriteLine(string.Format(Text.AddedLevel, levelName));
									}
								}
								break;
							case 1: // Assign Existing Levels
								LogTextBox.Clear();
								AssignExistingLevels(map);
								assignedLevelsList.Clear();
								LogTextBox.Draw();
								break;
							case 2: // Separator
								break;
							default: // Level
								LogTextBox.ClearContent();
								string selectedLevelName = selection.Text;
								string currentLevelName = selectedLevelName + Program.LevelFileExtension;
								string currentScriptName = selectedLevelName + Program.ScriptFileExtension;
								string levelSource = Path.Combine(Program.installedLevelsPath, currentLevelName);
								string scriptSource = Path.Combine(Program.installedScriptsPath, currentScriptName);

								switch (SelectionPrompt.PromptSelection(levelCommands, true)) {
									case 0: // Rename
										levelRenameInput.Top = levelList.Top + selection.RowIndex;
										bool renamed = levelRenameInput.PromptText(new GUI.TextInput.PromptOptions(false, true, false, selectedLevelName));
										if (renamed) {
											string newLevelName = levelRenameInput.Text;
											string levelDestination = Path.Combine(Program.installedLevelsPath, newLevelName + Program.LevelFileExtension);
											string scriptDestination = Path.Combine(Program.installedScriptsPath, newLevelName + Program.ScriptFileExtension);
											if (File.Exists(levelDestination) || File.Exists((scriptDestination))) {
												LogTextBox.WriteLine(string.Format(Text.LevelOrScriptAlreadyExists, newLevelName + Program.LevelFileExtension, newLevelName + Program.ScriptFileExtension));
											} else {
												try {
													map.RenameLevel(selectedLevelName, newLevelName);
													File.Move(levelSource, levelDestination);
													File.Move(scriptSource, scriptDestination);
													Program.SaveInstalledMaps();
													RefreshLevelList(map);
												} catch (Exception e) {
													LogTextBox.WriteLine(string.Format(Text.ErrorWithMessage, e.Message));
													if (File.Exists(levelDestination)) {
														LogTextBox.WriteLine(string.Format(Text.RevertLevelRename, currentLevelName));
														File.Move(levelDestination, levelSource);
														LogTextBox.AppendLastLine(Text.Renamed);
													}
													if (File.Exists(scriptDestination)) {
														LogTextBox.WriteLine(string.Format(Text.RevertScriptRename, currentScriptName));
														File.Move(scriptDestination, scriptSource);
														LogTextBox.AppendLastLine(Text.Renamed);
													}
												}
											}
										}
										break;
									case 1: // Delete
										LogTextBox.WriteLine(string.Format(Text.ConfirmDelete, currentLevelName, currentScriptName));
										LogTextBox.WriteLine(Text.AreYouSureYouWantToContinue);
										LogTextBox.WriteLine(Text.UnassignInsteadOfDelteInstructions);
										int confirmation = SelectionPrompt.PromptSelection(confirmDeleteCommands, new GUI.SelectionPrompt.Options() { AllowCancel = true, Index = 1 });
										if (confirmation == 0) { // Confirmed deletion
											try {
												LogTextBox.ClearContent();
												map.RemoveLevel(selectedLevelName);
												Program.SaveInstalledMaps();
												LogTextBox.WriteLine(string.Format(Text.Deleting, currentLevelName));
												File.Delete(levelSource);
												LogTextBox.AppendLastLine(Text.Deleted);
												LogTextBox.WriteLine(string.Format(Text.Deleting, currentScriptName));
												File.Delete(scriptSource);
												LogTextBox.AppendLastLine(Text.Deleted);
											} catch (Exception e) {
												LogTextBox.WriteLine(string.Format(Text.ErrorWithMessage, e.Message));
											}
											RefreshLevelList(map);
										}
										break;
								}
								break;
						}

						levelList.SelectCurrentItem();
						break;
					case Properties.Command.Cancel:
						quitPrompt = true;
						break;
				}
			}
		}

		private void EditMapInfo(Map map) {
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
			});
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
									RefreshMapList();
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
									mapInfoList.Items[5].Value = Text.MapInfoShortDescription + (string.IsNullOrEmpty(map.ShortDescription) ? Text.MapInfoNoDescription : map.ShortDescription);
								}
								break;
							case 6: // Long Description
								LogTextBox.ClearContent();
								LogTextBox.WriteLine("Press Shift + Enter to go to the next line.");
								LogTextBox.WriteLine("Note: The multiline text editor is still in its early stages.");
								LogTextBox.WriteLine("If you experience any issues, please adjust the description with a text editor after exporting your map.");
								if (mapInfoLongDescriptionInput.PromptTextMultiline()) {
									map.LongDescription = mapInfoLongDescriptionInput.Text;
								}
								LogTextBox.Clear();
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

		private void ExportMap(Map map, string destinationFolderPath, bool overrideExisting) {
			LogTextBox.WriteLine(string.Format(Text.ExportingMap, map.Name));
			string destinationLevelPath = Path.Combine(destinationFolderPath, Program.LevelFolderName);
			string destinationScriptPath = Path.Combine(destinationFolderPath, Program.ScriptFolderName);

			Directory.CreateDirectory(destinationLevelPath);
			foreach (var level in map.Levels) {
				string levelSource = Path.Combine(Program.installedLevelsPath, level);
				string levelDestination = Path.Combine(destinationLevelPath, level);
				File.Copy(levelSource, levelDestination, overrideExisting);
			}
			Directory.CreateDirectory(destinationScriptPath);
			foreach (var script in map.Scripts) {
				string scriptSource = Path.Combine(Program.installedScriptsPath, script);
				string scriptDestination = Path.Combine(destinationScriptPath, script);
				File.Copy(scriptSource, scriptDestination, overrideExisting);
			}
			var loadableMap = new LoadableMap(map, destinationFolderPath);
			Program.availableMaps.Add(loadableMap);
			loadableMap.SaveMapInfo();
			LogTextBox.AppendLastLine(Text.Exported);
			if (map.IsWIP) {
				LogTextBox.WriteLine(Text.MapStillWipWarning);
			}
			ignoreNextSelectionChangedEvent = true;
		}

		private List<Map> GetWipMaps() {
			List<Map> maps = new List<Map>();
			foreach (Map map in Program.installedMaps) {
				if (map.IsWIP) {
					maps.Add(map);
				}
			}
			return maps;
		}

		private void MapListSelectionChanged(object Sender, GUI.SelectionChangedEventArgs eventArgs) {
			if (ignoreNextSelectionChangedEvent) {
				ignoreNextSelectionChangedEvent = false;
			} else {
				if (eventArgs.SelectedItemIndex <= 2) {
					DetailsTextBox.ClearContent();
				} else {
					Map map = ((GUI.SelectableMap)eventArgs.SelectedItem).Map;
					DetailsTextBox.WriteAllMapInfo(map);
					LogTextBox.WriteMapIssues(map);
				}
			}
		}

		private void PromptExportMap(Map map) {
			LogTextBox.ClearContent();
			string destinationFolderPath = Path.Combine(Program.customMapsPath, map.Name);
			try {
				if (Directory.Exists(destinationFolderPath)) {
					LogTextBox.WriteLine(string.Format(Text.ConfirmExportAndOverwrite, destinationFolderPath));
					int response = SelectionPrompt.PromptSelection(exportCommands, new GUI.SelectionPrompt.Options() { AllowCancel = true, Index = 2 });
					switch (response) {
						case 0: // Overwrite
							ExportMap(map, destinationFolderPath, true);
							break;
						case 1: // Export with Timestamp
							string timestamp = DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss.fffffff");
							//string timestamp = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss.fffffffK");
							ExportMap(map, destinationFolderPath + timestamp, false);
							break;
					}
				} else {
					ExportMap(map, destinationFolderPath, false);
				}
			} catch (Exception e) {
				ignoreNextSelectionChangedEvent = true;
				LogTextBox.WriteLine(string.Format(Text.ErrorWithMessage, e.Message));
				
			}
		}

		private void RefreshLevelList(Map map) {
			levelList.Clear();
			levelList.Items.Clear();
			levelList.AddItems(new string[] {
				Text.MapEditorCommandAddNewLevel,
				Text.MapEditorCommandAssignLevels,
			});
			levelList.AddSeparator();
			levelList.AddItems(map.GetLevelsWithoutExtension());
			levelList.Draw();
		}

		private void RefreshMapList() {
			mapList.Clear();
			bool showOnlyWIP = ShowOnlyWIPCheckBox != null && ShowOnlyWIPCheckBox.Checked;
			mapList.Items.Clear();
			ShowOnlyWIPCheckBox = mapList.AddCheckBox(Text.ShowOnlyWipMaps, showOnlyWIP);
			mapList.AddItem(Text.MapEditorAddMap);
			mapList.AddSeparator();
			mapList.AddMaps(ShowOnlyWIPCheckBox.Checked ? GetWipMaps() : Program.installedMaps);
			mapList.Draw();
		}

	}
}
