﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CarrionManagerConsole
{
	class GUI
	{
		public static void DrawHorizontalLine(int left, int top, int width, char c, ConsoleColor background, ConsoleColor foreground) {
			Console.BackgroundColor = background;
			Console.ForegroundColor = foreground;
			Console.SetCursorPosition(left, top);
			Console.Write(new string(c, width));
		}

		public static void DrawVerticalLine(int left, int top, int height, char c, ConsoleColor background, ConsoleColor foreground) {
			Console.BackgroundColor = background;
			Console.ForegroundColor = foreground;
			for (int i = 0; i < height; ++i) {
				Console.SetCursorPosition(left, top + i);
				Console.Write(c);
			}
		}

		public static String FixedWidth(string s, int width, Properties.HorizontalAlignment horizontalAlignment) {
			if (width <= 0) {
				throw new Exception(String.Format("StringToFixedWidth: Parameter \"width\" has invalid value \"{0}\"!", width.ToString()));
			}
			if (string.IsNullOrEmpty(s)) {
				return new string(' ', width);
			}
			if (s.Length > width) {
				return s.Substring(0, width);
			}
			else {
				if (horizontalAlignment == Properties.HorizontalAlignment.Center) {
					double half = width / 2;
					return s.PadLeft((int)Math.Floor(half)).PadRight((int)Math.Ceiling(half));
				}
				else if (horizontalAlignment == Properties.HorizontalAlignment.Right) {
					return s.PadLeft(width);
				}
				else {
					return s.PadRight(width);
				}
			}
		}

		public static String FixedWidth(string s, int width) {
			return FixedWidth(s, width, Properties.HorizontalAlignment.Left);
		}

		public static void Reset() {
			Console.BackgroundColor = MenuColor.ContentBG;
			Console.ForegroundColor = MenuColor.ContentFG;
			Console.Clear();
			Console.SetCursorPosition(0, 0);
		}

		public static void Write(int left, int top, string text, ConsoleColor backgroundColor, ConsoleColor textColor) {
			Console.SetCursorPosition(left, top);
			Console.BackgroundColor = backgroundColor;
			Console.ForegroundColor = textColor;
			Console.Write(text);
		}

		public static void Write(int left, int top, string text) {
			Write(left, top, text, MenuColor.ContentBG, MenuColor.ContentFG);
		}

		public class Box
		{
			public int left, top;
			public int width, height;
			public ConsoleColor background, foreground;

			public Box(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) {
				this.left = left;
				this.top = top;
				this.width = width;
				this.height = height;
				this.background = background;
				this.foreground = foreground;
			}

			public int Right {
				get {
					return this.left + this.width - 1;
				}
			}

			public void Clear() {
				for (int i = 0; i < this.height; ++i) {
					GUI.DrawHorizontalLine(this.left, this.top + i, this.width, ' ', this.background, this.foreground);
				}
			}
		}
		public class ListBox : Box
		{
			public List<SelectableText> items;
			public int selectedItemIndex;
			public int scroll;

			public ListBox(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				Init();
			}

			public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

			public bool IsEmpty => this.items.Count == 0;
			public int VisibleItemCount => (items.Count <= height) ? items.Count : height;
			public List<SelectableText> VisibleItems {
				get {
					if (this.items.Count <= this.height) {
						return this.items;
					}
					var visibleItems = new List<SelectableText>();
					for (int i = scroll; i < height + scroll; ++i) {
						visibleItems.Add(items[i]);
					}
					return visibleItems;
				}
			}
			public SelectableText SelectedItem => RowContainsItem(selectedItemIndex) ? items[selectedItemIndex] : null;

			public int ClampRow(int row) {
				if (row < 0) {
					return 0;
				}
				else if (row >= items.Count - 1) {
					return items.Count - 1;
				}
				else {
					return row;
				}
			}

			public void Deselect() {
				if (SelectedItem != null) {
					SelectedItem.Deselect();
				}
				selectedItemIndex = -1;
			}

			public void Draw() {
				foreach (var item in VisibleItems) {
					item.Draw();
				}
			}

			public void Init() {
				selectedItemIndex = 0;
				scroll = 0;
				items = new List<SelectableText>();
			}

			public void Navigate(int rows) {
				int newRow = ClampRow(selectedItemIndex + rows);
				if (selectedItemIndex == newRow) {
					return;
				}
				else {
					Select(newRow);
				}

				ScrollToSelectedItem();
			}

			public void NavigateDown() {
				Navigate(1);
			}

			public void NavigateUp() {
				Navigate(-1);
			}

			public void PageDown() {
				Navigate(height);
			}

			public void PageUp() {
				Navigate(-height);
			}

			public Selection PromptInput() {
				if (SelectedItem == null && !IsEmpty) {
					items[0].Select();
				}

				while (true) {
					var input = Console.ReadKey(true).Key;
					if (Program.keybindings.ContainsKey(input)) {
						switch (Program.keybindings[input]) {
							case Properties.Command.NavigateUp:
								NavigateUp();
								break;
							case Properties.Command.PageUp:
								PageUp();
								break;
							case Properties.Command.NavigateDown:
								NavigateDown();
								break;
							case Properties.Command.PageDown:
								PageDown();
								break;
							default:
								return new Selection(this, 0, selectedItemIndex, Program.keybindings[input]);
						}
					}
				}
			}

			public virtual void OnSelectionChanged(SelectionChangedEventArgs e) {
				SelectionChanged?.Invoke(this, e);
			}

			public bool RowContainsItem(int row) {
				return row >= 0 && row < this.items.Count;
			}

			public void Scroll(int offset) {
				if (offset == 0) {
					return;
				}
				else if (scroll + offset < 0) {
					offset = scroll * -1;
				}
				else if (scroll + offset > items.Count) {
					offset = items.Count - (height + scroll);
				}

				foreach (var item in items) {
					item.top += offset;
					SetItemVisibility(item);
				}
				Draw();
			}

			public void ScrollToSelectedItem() {
				if (selectedItemIndex < scroll) {
					Scroll((scroll - selectedItemIndex) * -1); // TODO: Test (-(
				}
				else if (selectedItemIndex > height + scroll) {
					Scroll((selectedItemIndex - (height + scroll)));
				}
			}

			public void Select(int row) {
				if (IsEmpty) {
					return;
				}
				var previousItem = SelectedItem;
				Deselect();
				selectedItemIndex = ClampRow(row);
				SelectedItem.Select();

				var args = new SelectionChangedEventArgs() {
					PreviousItem = previousItem,
					SelectedItem = SelectedItem,
					SelectedItemIndex = selectedItemIndex,
				};
				OnSelectionChanged(args);
			}

			public void SetContent(string[] content) {
				Init();
				for (int i = 0; i < content.Length; ++i) {
					var item = new SelectableText(left, top + i, width, content[i]);
					items.Add(item);
					SetItemVisibility(item);
				}
			}

			public void SetItemVisibility(SelectableText item) {
				item.visible = (item.top >= top && item.top < top + height);
			}
		}
		public class Label : Box
		{
			public string text;
			public Label(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, string text) : base(left, top, width, height, background, foreground) {
				this.text = text;
			}

			public Label(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				this.text = String.Empty;
			}

			public void Draw() {
				this.Clear();
				GUI.Write(this.left, this.top, this.text, this.background, this.foreground);
			}
		}
		public class ListMenu : Box
		{
			public readonly Label[] headers;
			public readonly ListBox[] lists;
			public int currentListIndex;

			public ListMenu(int left, int top, int width, int height, int columnCount, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				this.headers = new Label[columnCount];
				this.lists = new ListBox[columnCount];
				this.Init();
			}

			public ListBox CurrentList => IsListSelected ? lists[currentListIndex] : null;

			public bool IsEmpty {
				get {
					foreach (var list in lists) {
						if (list.items.Count > 0) {
							return false;
						}
					}
					return true;
				}
			}

			public bool IsListSelected => currentListIndex >= 0 && currentListIndex < lists.Length;

			public void Draw() {
				for (int i = 0; i < lists.Length; ++i) {
					headers[i].Draw();
					lists[i].Draw();
				}
				DrawSeparators();
			}

			public Selection PromptInput() {
				if (!SelectionValid() && !IsEmpty) {
					SelectFirstItem();
				}

				while (true) {
					var input = Console.ReadKey(true).Key;
					if (Program.keybindings.ContainsKey(input)) {
						switch (Program.keybindings[input]) {
							case Properties.Command.NavigateUp:
								if (IsListSelected) {
									CurrentList.NavigateUp();
								}
								break;
							case Properties.Command.PageUp:
								if (IsListSelected) {
									CurrentList.PageUp();
								}
								break;
							case Properties.Command.NavigateDown:
								if (IsListSelected) {
									CurrentList.NavigateDown();
								}
								break;
							case Properties.Command.PageDown:
								if (IsListSelected) {
									CurrentList.PageDown();
								}
								break;
							case Properties.Command.NavigateLeft:
								if (IsListSelected) {
									NavigateLeft();
								}
								break;
							case Properties.Command.NavigateRight:
								if (IsListSelected) {
									NavigateRight();
								}
								break;
							default:
								int selectedItemIndex = IsListSelected ? CurrentList.selectedItemIndex : -1;
								return new Selection(CurrentList, currentListIndex, selectedItemIndex, Program.keybindings[input]);
						}
					}
				}
			}

			public void SetColumnContent(int columnNumber, string header, string[] content) {
				headers[columnNumber].text = header;
				lists[columnNumber].SetContent(content);
			}

			private void DrawSeparators() {
				for (int i = 0; i < this.lists.Length - 1; ++i) {
					DrawVerticalLine(this.lists[i].Right + 1, this.top + 1, this.height - 1, ' ', MenuColor.SeparatorBG, MenuColor.SeparatorFG);
				}
			}

			private void Init() {
				currentListIndex = 0;
				int columnWidth = (width / lists.Length) - 1;
				int columnHeight = height - 1;
				for (int columnNumber = 0; columnNumber < lists.Length; ++columnNumber) {
					int columnLeft = left + ((columnWidth + 1) * columnNumber);
					headers[columnNumber] = new Label(columnLeft, top, columnWidth + 1, 1, MenuColor.MinorHeaderBG, MenuColor.MinorHeaderFG);
					lists[columnNumber] = new ListBox(columnLeft, top + 1, columnWidth, columnHeight, background, foreground);
				}
			}

			private void NavigateLeft() {
				if (currentListIndex > 0) {
					for (int c = currentListIndex - 1; c >= 0; --c) {
						if (!lists[c].IsEmpty) {
							CurrentList.Deselect();
							currentListIndex = c;
							int currentRow = CurrentList.selectedItemIndex;
							CurrentList.Select(currentRow);
						}
					}
				}
			}

			private void NavigateRight() {
				if (currentListIndex < this.lists.Length - 1) {
					for (int c = currentListIndex + 1; c < this.lists.Length; ++c) {
						if (!lists[c].IsEmpty) {
							CurrentList.Deselect();
							currentListIndex = c;
							int currentRow = CurrentList.selectedItemIndex;
							CurrentList.Select(currentRow);
						}
					}
				}
			}

			public bool SelectFirstItem() {
				if (CurrentList != null) {
					CurrentList.Deselect();
				}
				for (int c = 0; c < lists.Length; ++c) {
					if (!lists[c].IsEmpty) {
						currentListIndex = c;
						CurrentList.Select(0);
						return true;
					}
				}

				currentListIndex = -1;
				return false;
			}

			private bool SelectionValid() {
				return !((currentListIndex < 0 || currentListIndex >= this.lists.Length) ||
					(!CurrentList.RowContainsItem(CurrentList.selectedItemIndex)));
			}
		}
		public class TextBox : Box
		{
			public string[] content;

			private int nextEmptyLine;
			public TextBox(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				this.content = new string[this.height];
				this.background = background;
				this.foreground = foreground;
				this.nextEmptyLine = 0;
			}

			public void AppendLastLine(string text) {
				int lastWrittenLine = nextEmptyLine == -1 ? this.height - 1 : nextEmptyLine - 1;
				this.content[lastWrittenLine] += text;
				GUI.Write(this.left, this.top + lastWrittenLine, FixedWidth(this.content[lastWrittenLine], this.width));
			}

			public new void Clear() {
				for (int i = 0; i < this.content.Length; ++i) {
					this.content[i] = String.Empty;
				}
				this.nextEmptyLine = 0;
				this.Draw();
			}

			public void Draw() {
				for (int i = 0; i < this.content.Length; ++i) {
					GUI.Write(this.left, this.top + i, FixedWidth(this.content[i], this.width), this.background, this.foreground);
				}
			}

			public void WriteLine(string text) {
				if (nextEmptyLine == -1) {
					for (int i = 0; i < this.content.Length - 1; ++i) {
						this.content[i] = this.content[i + 1];
					}
					this.content[^1] = text;
					this.Draw();
				}
				else {
					this.content[nextEmptyLine] = text;
					GUI.Write(this.left, this.top + nextEmptyLine, FixedWidth(text, this.width), this.background, this.foreground);
					nextEmptyLine++;
					if (nextEmptyLine >= this.height) {
						nextEmptyLine = -1; // No empty lines remain
					}
				}
			}
		}
		public class SelectableText : Box
		{
			public bool enabled;
			public Properties.SelectionStatus selectionStatus;
			public string text;
			public bool visible;

			public SelectableText(int left, int top, int width, string text) : base(left, top, width, 1, MenuColor.ContentBG, MenuColor.ContentFG) {
				this.left = left;
				this.top = top;
				this.text = text;
				this.enabled = true;
				this.visible = true;
				this.selectionStatus = Properties.SelectionStatus.None;
			}

			public SelectableText(int left, int top, string text) : base(left, top, text.Length + 2, 1, MenuColor.ContentBG, MenuColor.ContentFG) {
				this.left = left;
				this.top = top;
				this.text = text;
				this.enabled = true;
				this.visible = true;
				this.selectionStatus = Properties.SelectionStatus.None;
			}

			public void Deselect() {
				this.selectionStatus = Properties.SelectionStatus.None;
				Draw();
			}

			public void Draw() {
				if (visible) {
					Console.SetCursorPosition(left, top);
					SetConsoleColor();
					Console.Write(this.ToString());
				}
			}

			public void Highlight() {
				this.selectionStatus = Properties.SelectionStatus.Highlighted;
				Draw();
			}

			public void Select() {
				this.selectionStatus = Properties.SelectionStatus.Selected;
				Draw();
			}

			public void SetConsoleColor() {
				if (enabled) {
					switch (selectionStatus) {
						case Properties.SelectionStatus.None:
							Console.BackgroundColor = MenuColor.ContentBG;
							Console.ForegroundColor = MenuColor.ContentFG;
							break;
						case Properties.SelectionStatus.Selected:
							Console.BackgroundColor = MenuColor.SelectedBG;
							Console.ForegroundColor = MenuColor.SelectedText;
							break;
						case Properties.SelectionStatus.Highlighted:
							Console.BackgroundColor = MenuColor.HighlightBG;
							Console.ForegroundColor = MenuColor.HighlightText;
							break;
						default:
							Console.BackgroundColor = MenuColor.ContentBG;
							Console.ForegroundColor = MenuColor.ContentFG;
							break;
					}
				}
				else {
					if (selectionStatus == Properties.SelectionStatus.Selected || selectionStatus == Properties.SelectionStatus.Highlighted) {
						Console.BackgroundColor = MenuColor.SelectedDisabledBG;
						Console.ForegroundColor = MenuColor.SelectedDisabledFG;
					}
					else {
						Console.BackgroundColor = MenuColor.DisabledBG;
						Console.ForegroundColor = MenuColor.DisabledFG;
					}
				}
			}

			public override string ToString() {
				string fixedWidthText = FixedWidth(text, width - 2);
				if (this.selectionStatus == Properties.SelectionStatus.None) {
					return Text.UnselectedLeftSymbol + fixedWidthText + Text.UnselectedRightSymbol;
				}
				else if (this.selectionStatus == Properties.SelectionStatus.Selected) {
					return Text.SelectedLeftSymbol + fixedWidthText + Text.SelectedRightSymbol;
				}
				else if (this.selectionStatus == Properties.SelectionStatus.Highlighted) {
					return Text.HighlightedLeftSymbol + fixedWidthText + Text.HighlightedRightSymbol;
				}
				else {
					throw new Exception(String.Format("Unsupported SelectionOption \"{0}\"", this.selectionStatus.ToString()));
				}
			}
		}
		public class Selection
		{
			public ListBox list;
			public int rowIndex, columnIndex;
			public Properties.Command command;

			public Selection(ListBox list, int columnIndex, int rowIndex, Properties.Command command) {
				this.list = list;
				this.columnIndex = columnIndex;
				this.rowIndex = rowIndex;
				this.command = command;
			}

			public string Text => list.items[rowIndex].text;
		}
		public class SelectionPrompt : Box
		{
			public SelectionPrompt(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) { }

			public int PromptSelection(string[] selectableItems, Options options) {
				this.Clear();
				var items = new List<SelectableText>();
				int textLeft = this.left;
				for (int i = 0; i < selectableItems.Length; ++i) {
					var item = selectableItems[i];
					var selectableText = new SelectableText(textLeft, this.top, item);
					if (options.disabledItems.Contains(i)) {
						selectableText.enabled = false;
					}
					items.Add(selectableText);
					textLeft = selectableText.Right + 1;
				}

				if (options.cancel) {
					items.Add(new SelectableText(textLeft, top, Text.Cancel));
				}

				int selected = options.index;
				items[selected].selectionStatus = Properties.SelectionStatus.Selected;

				bool finished = false;
				while (!finished) {
					foreach (var text in items) {
						text.Draw();
					}
					bool inputValid = false;
					while (!inputValid) {
						var key = Console.ReadKey(true).Key;
						if (!Program.keybindings.ContainsKey(key))
							continue;

						var command = Program.keybindings[key];
						switch (command) {
							case Properties.Command.NavigateLeft:
								if (selected > 0) {
									inputValid = true;
									items[selected].selectionStatus = Properties.SelectionStatus.None;
									selected--;
									items[selected].selectionStatus = Properties.SelectionStatus.Selected;
								}
								break;
							case Properties.Command.NavigateRight:
								if (selected < items.Count - 1) {
									inputValid = true;
									items[selected].selectionStatus = Properties.SelectionStatus.None;
									selected++;
									items[selected].selectionStatus = Properties.SelectionStatus.Selected;
								}
								break;
							case Properties.Command.Confirm:
								if (items[selected].enabled) {
									if (options.cancel && selected == (items.Count - 1)) {
										selected = -1;
									}
									inputValid = true;
									finished = true;
								}
								break;
							case Properties.Command.Cancel:
								if (options.cancel) {
									selected = -1;
									inputValid = true;
									finished = true;
								}
								break;
						}
					}
				}
				this.Clear();
				return selected;
			}

			public int PromptSelection(string[] selectableItems, bool cancel) {
				var options = new Options() {
					cancel = cancel,
				};
				return PromptSelection(selectableItems, options);
			}

			public class Options
			{
				public bool cancel; // Adds the "Cancel" option to the end of the list. Accepts Escape-key.
				public int index;
				public int[] disabledItems;

				public Options() {
					cancel = false;
					index = 0;
					disabledItems = new int[0];
				}
			}
		}

		#region Event Arguments
		public class SelectionChangedEventArgs : EventArgs
		{
			public int SelectedItemIndex { get; set; }
			public SelectableText SelectedItem { get; set; }
			public SelectableText PreviousItem { get; set; }
		}
		#endregion
	}
}
