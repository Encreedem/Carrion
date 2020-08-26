using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	class NavigationWindow : IWindow
	{
		private readonly GUI.Label titleLabel;
		private readonly GUI.ColumnView windowsMenu;
		private readonly GUI.ListBox windowList;
		private readonly GUI.TextBox windowNumberTextBox;
		private readonly GUI.Label controlsLabel;

		private bool windowQuit;

		public NavigationWindow() {
			var width = Console.WindowWidth;
			var height = Console.WindowHeight;
			titleLabel = new GUI.Label(0, 0, width, 1, MenuColor.NavigationWindowTitleBG, MenuColor.NavigationWindowTitleFG, Text.NavigationWindowTitle) {
				HorizontalAlignment = Properties.HorizontalAlignment.Center
			};

			windowsMenu = new GUI.ColumnView(2, 3, 40, 10, MenuColor.ContentBG, MenuColor.ContentFG, 1);
			windowList = windowsMenu.AddListBox(0, Text.NavigationWindowListHeader, false);
			windowList.SetContent(Program.windowNames);

			windowNumberTextBox = new GUI.TextBox(0, 4, 1, 10, MenuColor.ContentBG, MenuColor.ContentFG) {
				Content = new string[] { "1", "2", "3", "4" }
			};

			controlsLabel = new GUI.Label(0, height - 1, width, 1, MenuColor.ControlsBG, MenuColor.ControlsFG, Text.DefaultControls);
		}

		public void Show() {
			GUI.Reset();
			windowsMenu.NavigateToDefault();
			DrawAll();
			windowQuit = false;
			while (!windowQuit) {
				GUI.Selection selection = windowsMenu.PromptSelection();
				switch (selection.Command) {
					case Properties.Command.Confirm:
						switch (selection.RowIndex) {
							case 0:
								Program.currentWindow = Program.launcherWindow;
								windowQuit = true;
								break;
							case 1:
								Program.currentWindow = Program.mapInstallerWindow;
								windowQuit = true;
								break;
							case 2:
								Program.currentWindow = Program.saveManagerWindow;
								windowQuit = true;
								break;
							case 3:
								Program.currentWindow = Program.backupsWindow;
								windowQuit = true;
								break;
						}
						break;
					case Properties.Command.Cancel:
						windowQuit = true;
						Program.quit = true;
						break;
				}

				if (!windowQuit) {
					windowQuit = DefaultWindow.ChangeWindow(selection.Command);
				}
			}
		}

		private void DrawAll() {
			titleLabel.Draw();
			windowsMenu.Draw();
			windowNumberTextBox.Draw();
			controlsLabel.Draw();
		}
	}
}
