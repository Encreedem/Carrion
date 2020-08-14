using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	class NavigationWindow : IWindow
	{
		private const string windowName = "Window Navigator";
		private readonly GUI.Label titleLabel;
		private readonly GUI.ListMenu windowsMenu;
		private readonly GUI.TextBox windowNumberTextBox;

		private bool windowQuit;

		public NavigationWindow() {
			var width = Console.WindowWidth;
			//var height = Console.WindowHeight;
			titleLabel = new GUI.Label(0, 0, width, 1, MenuColor.NavigationWindowTitleBG, MenuColor.NavigationWindowTitleFG, windowName);
			windowsMenu = new GUI.ListMenu(2, 3, 40, 10, 1, MenuColor.ContentBG, MenuColor.ContentFG);
			windowsMenu.SetColumnContent(0, "Windows", Program.windowNames);

			windowNumberTextBox = new GUI.TextBox(0, 4, 1, 10, MenuColor.ContentBG, MenuColor.ContentFG) {
				content = new string[] { "1", "2", "3" }
			};
		}

		public bool ChangeWindow(Properties.Command command) {
			IWindow nextWindow = null;
			switch (command) {
				case Properties.Command.ShowLauncher:
					nextWindow = Program.launcherWindow;
					break;
				case Properties.Command.ShowMapInstaller:
					nextWindow = Program.mapInstallerWindow;
					break;
				case Properties.Command.ShowSaveManager:
					nextWindow = Program.saveManagerWindow;
					break;
			}

			if (nextWindow == null || nextWindow == Program.currentWindow) {
				return false;
			}
			else {
				Program.currentWindow = nextWindow;
				return true;
			}
		}

		public void Show() {
			GUI.Reset();
			windowsMenu.SelectFirstItem();
			DrawAll();
			windowQuit = false;
			while (!windowQuit) {
				GUI.Selection selection = windowsMenu.PromptInput();
				switch (selection.command) {
					case Properties.Command.Confirm:
						switch (selection.rowIndex) {
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
						}
						break;
					case Properties.Command.Cancel:
						windowQuit = true;
						Program.quit = true;
						break;
				}

				if (!windowQuit) {
					windowQuit = ChangeWindow(selection.command);
				}
			}
		}

		private void DrawAll() {
			titleLabel.Draw();
			windowsMenu.Draw();
			windowNumberTextBox.Draw();
			Program.controlsLabel.Draw();
		}
	}
}
