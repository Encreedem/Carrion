using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	abstract class DefaultWindow : IWindow
	{
		public const int
			TitleHeight = 1,
			MenuCommandSeparatorHeight = 1,
			SelectionPromptHeight = 1,
			CommandLogSeparatorHeight = 1,
			LowerTextBoxesHeight = 4,
			InfoLogSeparatorWidth = 1,
			TextBoxesCommandsDistance = 1,
			ControlsLabelHeight = 1;

		private readonly string title;
		private readonly ConsoleColor titleBG, titleFG;

		public DefaultWindow(string title, ConsoleColor titleBG, ConsoleColor titleFG) {
			this.title = title;
			this.titleBG = titleBG;
			this.titleFG = titleFG;
			Init();
		}

		private int BottomAlignedHeight =>
			MenuCommandSeparatorHeight +
			SelectionPromptHeight +
			CommandLogSeparatorHeight +
			LowerTextBoxesHeight +
			TextBoxesCommandsDistance +
			ControlsLabelHeight;

		public GUI.Label Title { get; set; }
		public GUI.ColumnView Menu { get; set; }
		public GUI.Box MenuCommandSeparator { get; set; }
		public GUI.SelectionPrompt SelectionPrompt { get; set; }
		public GUI.Box CommandLogSeparator { get; set; }
		//public GUI.TextBox InfoTextBox { get; set; }
		//public GUI.Box InfoLogSeparator { get; set; }
		public GUI.TextBox LogTextBox { get; set; }
		//public GUI.Box DecorativeLine { get; set; }
		public GUI.Label ControlsLabel { get; set; }

		public bool WindowQuit { get; set; }

		public static bool ChangeWindow(Properties.Command command) {
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
				case Properties.Command.ShowBackupsWindow:
					nextWindow = Program.backupsWindow;
					break;
			}

			if (nextWindow == null || nextWindow == Program.currentWindow) {
				return false;
			} else {
				Program.currentWindow = nextWindow;
				return true;
			}
		}

		public void DrawAll() {
			GUI.Reset();
			Title.Draw();
			Menu.Draw();
			MenuCommandSeparator.Draw();
			CommandLogSeparator.Draw();
			//InfoTextBox.Draw();
			//InfoLogSeparator.Draw();
			LogTextBox.Draw();
			//DecorativeLine.Draw();
			ControlsLabel.Draw();
		}

		public void Init() {
			int width = Console.WindowWidth;
			//int halfWidth = width / 2;
			int height = Console.WindowHeight;
			Title = new GUI.Label(0, 0, width, TitleHeight, titleBG, titleFG, title) {
				HorizontalAlignment = Properties.HorizontalAlignment.Center
			};
			ControlsLabel = new GUI.Label(
				0, height - ControlsLabelHeight,
				width, ControlsLabelHeight,
				MenuColor.ControlsBG, MenuColor.ControlsFG,
				Text.DefaultControls);
			/*DecorativeLine = new GUI.Box(
				0, ControlsLabel.Top - TextBoxesCommandsDistance - 1,
				halfWidth, 1,
				MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			InfoTextBox = new GUI.TextBox(
				0, ControlsLabel.Top - TextBoxesCommandsDistance - LowerTextBoxesHeight,
				halfWidth - 1, LowerTextBoxesHeight - 1,
				MenuColor.ContentBG, MenuColor.ContentFG);
			InfoLogSeparator = new GUI.Box(
				halfWidth - 1, InfoTextBox.Top,
				InfoLogSeparatorWidth, LowerTextBoxesHeight,
				MenuColor.SeparatorBG, MenuColor.SeparatorFG);*/
			LogTextBox = new GUI.TextBox(
				0, ControlsLabel.Top - TextBoxesCommandsDistance - LowerTextBoxesHeight,
				width, LowerTextBoxesHeight,
				MenuColor.ContentBG, MenuColor.ContentFG);
			CommandLogSeparator = new GUI.Box(
				0, LogTextBox.Top - CommandLogSeparatorHeight,
				width, CommandLogSeparatorHeight,
				MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			SelectionPrompt = new GUI.SelectionPrompt(
				0, CommandLogSeparator.Top - SelectionPromptHeight,
				width, SelectionPromptHeight,
				MenuColor.ContentBG, MenuColor.ContentFG);
			MenuCommandSeparator = new GUI.Box(
				0, SelectionPrompt.Top - MenuCommandSeparatorHeight,
				width, 1, MenuColor.SeparatorBG, MenuColor.SeparatorFG);
			Menu = new GUI.ColumnView(
				0, TitleHeight,
				width, height - TitleHeight - BottomAlignedHeight,
				MenuColor.ContentBG, MenuColor.ContentFG,
				2);
		}

		public abstract void PreShow();

		public abstract void Selected(GUI.Selection selection);

		public void Show() {
			PreShow();
			Menu.NavigateToDefault();
			DrawAll();
			WindowQuit = false;
			while (!WindowQuit) {
				GUI.Selection selection = Menu.PromptSelection();
				switch (selection.Command) {
					case Properties.Command.Confirm:
						if (selection.List != null && selection.SelectedItem != null) {
							Selected(selection);
						}
						break;
					case Properties.Command.Cancel:
						Program.currentWindow = Program.navigationWindow;
						WindowQuit = true;
						break;
				}

				if (!WindowQuit) {
					WindowQuit = ChangeWindow(selection.Command);
				}
			}
		}
	}
}
