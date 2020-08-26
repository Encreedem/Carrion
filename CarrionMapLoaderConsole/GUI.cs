using System;
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

		public static void DrawRectangle(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) {
			Console.BackgroundColor = background;
			Console.ForegroundColor = foreground;
			string s = new string(' ', width);

			for (int i = 0; i < height; ++i) {
				Console.SetCursorPosition(left, top + i);
				Console.Write(s);
			}
		}

		public static void DrawVerticalLine(int left, int top, int height, char c, ConsoleColor background, ConsoleColor foreground) {
			Console.BackgroundColor = background;
			Console.ForegroundColor = foreground;
			for (int i = 0; i < height; ++i) {
				Console.SetCursorPosition(left, top + i);
				Console.Write(c);
			}
		}

		public static string FixedWidth(string s, int width, Properties.HorizontalAlignment horizontalAlignment) {
			if (width <= 0) {
				throw new Exception(string.Format("StringToFixedWidth: Parameter \"width\" has invalid value \"{0}\"!", width.ToString()));
			}
			if (string.IsNullOrEmpty(s)) {
				return new string(' ', width);
			}
			if (s.Length > width) {
				return s.Substring(0, width);
			} else {
				if (horizontalAlignment == Properties.HorizontalAlignment.Center) {
					int padLeft = (width / 2) + (s.Length / 2);
					return s.PadLeft(padLeft).PadRight(width);
				} else if (horizontalAlignment == Properties.HorizontalAlignment.Right) {
					return s.PadLeft(width);
				} else {
					return s.PadRight(width);
				}
			}
		}

		public static string FixedWidth(string s, int width) {
			return FixedWidth(s, width, Properties.HorizontalAlignment.Left);
		}

		public static void Reset() {
			Console.BackgroundColor = MenuColor.ContentBG;
			Console.ForegroundColor = MenuColor.ContentFG;
			Console.Clear();
			Console.SetCursorPosition(0, 0);
		}

		public static void SetErrorColors() {
			Console.BackgroundColor = MenuColor.ErrorBG;
			Console.ForegroundColor = MenuColor.ErrorFG;
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

		public class Box : Drawable
		{
			public Box(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) :
				base(left, top, width, height, background, foreground) {

			}

			public override void Draw() {
				Clear();
			}
		}
		public class ColumnView : Navigable
		{
			private int currentColumn;
			private Box[] dimensions;

			public ColumnView(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, int columnCount) :
				base(left, top, width, height, background, foreground) {
				ColumnCount = columnCount;
				Headers = new Label[columnCount];
				Items = new IDrawable[columnCount];
				Navigables = new INavigable[columnCount];
				SelectableTextContainers = new SelectableTextContainer[columnCount];
				currentColumn = -1;
				InitDimensions();
			}

			public override bool CanNavigate {
				get {
					foreach (var item in Navigables) {
						if (item != null && item.CanNavigate) {
							return true;
						}
					}

					return false;
				}
			}
			public override bool CanNavigateDown => true;
			public override bool CanNavigateLeft => true;
			public override bool CanNavigateRight => true;
			public override bool CanNavigateUp => true;
			public int ColumnCount { get; private set; }
			public override int CurrentColumn => currentColumn;
			public bool CurrentColumnValid => CurrentColumn >= 0 && CurrentColumn < Items.Length;
			public override int CurrentRow => SelectionValid() ? CurrentNavigable.CurrentRow : -1;
			public INavigable CurrentNavigable => CurrentColumnValid ? Navigables[CurrentColumn] : null;
			public SelectableTextContainer CurrentSelectableTextContainer => SelectionValid() ? SelectableTextContainers[CurrentColumn] : null;
			public Label[] Headers { get; set; }
			public override bool IsActive => SelectionValid();
			public IDrawable[] Items { get; set; }
			public INavigable[] Navigables { get; set; }
			public SelectableTextContainer[] SelectableTextContainers { get; set; }

			public ListBox AddListBox(int column, string title, bool forceShowScrollBar) {
				SetHeader(column, title);

				var dimension = dimensions[column];
				ListBox listBox;
				if (Headers[column] == null) {
					listBox = new ListBox(
						dimension.Left, dimension.Top,
						dimension.Width, dimension.Height,
						Background, Foreground,
						forceShowScrollBar);
				} else {
					listBox = new ListBox(
						dimension.Left, dimension.Top + 1,
						dimension.Width, dimension.Height - 1,
						Background, Foreground,
						forceShowScrollBar);
				}

				AddNavigable(column, listBox);
				SelectableTextContainers[column] = listBox;
				return listBox;
			}

			public TextBox AddTextBox(int column, string title) {
				SetHeader(column, title);

				var dimension = dimensions[column];
				TextBox textBox;
				if (Headers[column] == null) {
					textBox = new TextBox(
						dimension.Left, dimension.Top,
						dimension.Width, dimension.Height,
						Background, Foreground);
				} else {
					textBox = new TextBox(
						dimension.Left, dimension.Top + 1,
						dimension.Width, dimension.Height - 1,
						Background, Foreground);
				}

				Items[column] = textBox;
				return textBox;
			}

			public void AddNavigable(int column, INavigable navigable) {
				Items[column] = navigable;
				Navigables[column] = navigable;
			}

			public override void Deactivate() {
				var currentNavigable = CurrentNavigable;
				if (currentNavigable != null) {
					currentNavigable.Deactivate();
				}
			}

			public override void Draw() {
				for (int i = 0; i < Items.Length; ++i) {
					if (Headers[i] != null) {
						Headers[i].Draw();
					}
					Items[i].Draw();
				}
			}

			public void InitDimensions() {
				dimensions = new Box[ColumnCount];
				int columnLeft = Left;
				int columnWidth = Width / ColumnCount;
				for (int i = 0; i < ColumnCount; ++i) {
					dimensions[i] = new Box(columnLeft, Top, columnWidth, Height, Background, Foreground);
					columnLeft += columnWidth;
				}
			}

			public override void NavigateDown() {
				var currentNavigable = CurrentNavigable;
				if (currentNavigable != null && currentNavigable.IsActive && currentNavigable.CanNavigateDown) {
					currentNavigable.NavigateDown();
				}
			}

			public override void NavigateLeft() {
				for (int c = CurrentColumn - 1; c >= 0; --c) {
					if (Navigables[c] != null && Navigables[c].CanNavigate) {
						int row = CurrentRow;
						Deactivate();
						currentColumn = c;
						CurrentNavigable.NavigateToLastColumn(row);
					}
				}
			}

			public override void NavigateRight() {
				for (int c = CurrentColumn + 1; c < Navigables.Length; ++c) {
					if (Navigables[c] != null && Navigables[c].CanNavigate) {
						int row = CurrentRow;
						Deactivate();
						currentColumn = c;
						CurrentNavigable.NavigateToFirstColumn(row);
					}
				}
			}

			public override void NavigateToDefault() {
				var currentNavigable = CurrentNavigable;
				if (currentNavigable != null) {
					currentNavigable.Deactivate();
				}
				for (int c = 0; c < Navigables.Length; ++c) {
					if (Navigables[c] != null && Navigables[c].CanNavigate) {
						currentColumn = c;
						Navigables[c].NavigateToDefault();
						return;
					}
				}

				currentColumn = -1;
			}

			public override void NavigateToFirstColumn(int row) {
				throw new NotImplementedException();
			}

			public override void NavigateToFirstRow(int column) {
				throw new NotImplementedException();
			}

			public override void NavigateToLastColumn(int row) {
				throw new NotImplementedException();
			}

			public override void NavigateToLastRow(int column) {
				throw new NotImplementedException();
			}

			public override void NavigateUp() {
				var currentNavigable = CurrentNavigable;
				if (currentNavigable != null && currentNavigable.IsActive && currentNavigable.CanNavigateUp) {
					currentNavigable.NavigateUp();
				}
			}

			public override void PageDown() {
				var currentNavigable = CurrentNavigable;
				if (currentNavigable != null && currentNavigable.IsActive && currentNavigable.CanNavigateDown) {
					currentNavigable.PageDown();
				}
			}

			public override void PageUp() {
				var currentNavigable = CurrentNavigable;
				if (currentNavigable != null && currentNavigable.IsActive && currentNavigable.CanNavigateUp) {
					currentNavigable.PageUp();
				}
			}

			public Selection PromptSelection() {
				while (true) {
					var input = PromptInput();
					var container = CurrentSelectableTextContainer;
					if (input.Command != Properties.Command.Confirm || container != null) {
						return new Selection(container, CurrentColumn, CurrentRow, input.Command);
					}
				}
			}

			public bool SelectionValid() {
				return (
					CurrentColumn >= 0 &&
					CurrentColumn < Navigables.Length &&
					Navigables[CurrentColumn] != null &&
					Navigables[CurrentColumn].IsActive);
			}
			public void SetHeader(int column, string text) {
				if (string.IsNullOrEmpty(text)) {
					Headers[column] = null;
				} else {
					var dimension = dimensions[column];
					Headers[column] = new Label(dimension.Left, dimension.Top, dimension.Width, 1, MenuColor.MinorHeaderBG, MenuColor.MinorHeaderFG, text) {
						HorizontalAlignment = Properties.HorizontalAlignment.Center,
					};
				}
			}
		}
		public abstract class Drawable : IDrawable
		{
			public Drawable(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) {
				Left = left;
				Top = top;
				Width = width;
				Height = height;
				Background = background;
				Foreground = foreground;
			}

			public int Left { get; set; }
			public int Top { get; set; }
			public int Bottom => Top + Height - 1;
			public int Right => Left + Width - 1;
			public int Width { get; set; }
			public int Height { get; set; }
			public ConsoleColor Background { get; set; }
			public ConsoleColor Foreground { get; set; }

			public void Clear() {
				DrawRectangle(Left, Top, Width, Height, Background, Foreground);
			}

			public abstract void Draw();
		}
		public class Input
		{
			public Input(INavigable list, int columnIndex, int rowIndex, Properties.Command command) {
				List = list;
				ColumnIndex = columnIndex;
				RowIndex = rowIndex;
				Command = command;
			}

			public int RowIndex { get; set; }
			public int ColumnIndex { get; set; }
			public Properties.Command Command { get; set; }
			public INavigable List { get; set; }
		}
		public class Label : Drawable
		{
			public Properties.HorizontalAlignment HorizontalAlignment { get; set; }
			public string Text { get; set; }

			public Label(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, string text) : base(left, top, width, height, background, foreground) {
				Text = text;
				HorizontalAlignment = Properties.HorizontalAlignment.Left;
			}

			public Label(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				Text = string.Empty;
				HorizontalAlignment = Properties.HorizontalAlignment.Left;
			}

			public override void Draw() {
				string fixedText = FixedWidth(Text, Width, HorizontalAlignment);
				Write(Left, Top, fixedText, Background, Foreground);
			}
		}
		public class ListBox : SelectableTextContainer
		{
			public int scroll;
			private ScrollBar scrollBar;

			public ListBox(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, bool forceShowScrollBar) :
				base(left, top, width, height, background, foreground, Properties.Alignment.Vertical) {
				ForceShowScrollBar = forceShowScrollBar;
				Init();
			}

			public bool ForceShowScrollBar { get; set; }
			public int VisibleItemCount => (Items.Count <= Height) ? Items.Count : Height;
			public List<SelectableText> VisibleItems {
				get {
					if (Items.Count <= Height) {
						return Items;
					}
					var visibleItems = new List<SelectableText>();
					for (int i = scroll; i < Height + scroll; ++i) {
						visibleItems.Add(Items[i]);
					}
					return visibleItems;
				}
			}

			public override void Draw() {
				foreach (var item in VisibleItems) {
					item.Draw();
				}
				scrollBar.Draw();
			}

			public void Init() {
				scroll = 0;
			}

			public void InitScrollBar() {
				scrollBar = new ScrollBar(Right, Top, 1, Height, MenuColor.ScrollBarBG, MenuColor.ScrollBarFG) {
					MaxScroll = Items.Count - Height,
					Scroll = scroll,
					ForceShow = ForceShowScrollBar,
				};
			}

			public override void Navigate(int offset) {
				int newRow = ClampIndex(SelectedItemIndex + offset);
				if (newRow == SelectedItemIndex) {
					return;
				} else {
					Select(newRow);
				}

				ScrollToSelectedItem();
			}

			public void Scroll(int scrollCount) {
				if (scrollCount == 0) {
					return;
				} else if (scroll + scrollCount < 0) {
					scrollCount = scroll * -1;
				} else if (scroll + scrollCount > Items.Count) {
					scrollCount = Items.Count - (Height + scroll);
				}
				scroll += scrollCount;
				scrollBar.Scroll = scroll;
				int offset = -scrollCount;

				foreach (var item in Items) {
					item.Top += offset;
					SetItemVisibility(item);
				}
				Draw();
			}

			public void ScrollToSelectedItem() {
				if (SelectedItemIndex < scroll) {
					Scroll(-(scroll - SelectedItemIndex));
				} else if (SelectedItemIndex + 1 > Height + scroll) {
					Scroll(SelectedItemIndex + 1 - (Height + scroll));
				}
			}

			public void SelectCurrentItem() {
				Select(SelectedItemIndex);
			}

			public void SetContent(string[] content) {
				InitItems();
				Init();
				int textWidth = Width - 1;
				for (int i = 0; i < content.Length; ++i) {
					var item = new SelectableText(Left, Top + i, textWidth, content[i]);
					Items.Add(item);
					SetItemVisibility(item);
				}
				InitScrollBar();
			}

			public void SetItemVisibility(SelectableText item) {
				item.Visible = (item.Top >= Top && item.Top < Top + Height);
			}
		}
		public abstract class Navigable : INavigable
		{
			public Navigable(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) {
				Left = left;
				Top = top;
				Width = width;
				Height = height;
				Background = background;
				Foreground = foreground;
			}

			#region IBox
			public int Left { get; set; }
			public int Top { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public ConsoleColor Background { get; set; }
			public ConsoleColor Foreground { get; set; }
			public int Bottom => Top + Height - 1;
			public int Right => Left + Width - 1;

			public void Clear() {
				DrawRectangle(Left, Top, Width, Height, Background, Foreground);
			}
			#endregion

			#region INavigable
			public abstract bool CanNavigate { get; }
			public abstract bool CanNavigateDown { get; }
			public abstract bool CanNavigateLeft { get; }
			public abstract bool CanNavigateRight { get; }
			public abstract bool CanNavigateUp { get; }
			public abstract int CurrentColumn { get; }
			public abstract int CurrentRow { get; }
			public abstract bool IsActive { get; }

			public abstract void Deactivate();
			public abstract void Draw();
			public abstract void NavigateDown();
			public abstract void NavigateLeft();
			public abstract void NavigateRight();
			public abstract void NavigateToDefault();
			public abstract void NavigateToFirstColumn(int row);
			public abstract void NavigateToFirstRow(int column);
			public abstract void NavigateToLastColumn(int row);
			public abstract void NavigateToLastRow(int column);
			public abstract void NavigateUp();
			public abstract void PageDown();
			public abstract void PageUp();
			public Input PromptInput() {
				if (!IsActive) {
					NavigateToDefault();
				}

				while (true) {
					var input = Console.ReadKey(true).Key;
					if (!Program.keybindings.ContainsKey(input)) {
						continue;
					}

					Properties.Command command = Program.keybindings[input];
					switch (command) {
						case Properties.Command.NavigateUp:
							if (CanNavigateUp) {
								NavigateUp();
							}
							break;
						case Properties.Command.PageUp:
							if (CanNavigateUp) {
								PageUp();
							}
							break;
						case Properties.Command.NavigateDown:
							if (CanNavigateDown) {
								NavigateDown();
							}
							break;
						case Properties.Command.PageDown:
							if (CanNavigateDown) {
								PageDown();
							}
							break;
						case Properties.Command.NavigateLeft:
							if (CanNavigateLeft) {
								NavigateLeft();
							}
							break;
						case Properties.Command.NavigateRight:
							if (CanNavigateRight) {
								NavigateRight();
							}
							break;
						default:
							return new Input(this, CurrentColumn, CurrentRow, command);
					}
				}
			}
			#endregion
		}
		public class ScrollBar : Drawable
		{
			private double maxScroll;
			private double scroll;
			private double scrollBarHeight;

			public ScrollBar(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				Scroll = 0;
				MaxScroll = 0;
				ForceShow = false;
			}

			public bool ForceShow { get; set; }

			public double MaxScroll {
				get { return maxScroll; }
				set {
					maxScroll = Math.Max(0, value);
					Init();
				}
			}
			public double Scroll {
				get { return scroll; }
				set { scroll = Math.Clamp(value, 0, MaxScroll); }
			}

			public override void Draw() {
				if (MaxScroll == 0) {
					if (ForceShow) {
						DrawVerticalLine(Left, Top, Height, ' ', Background, Background);
					}
					return;
				}

				double scrollBarTop = Math.Floor((Scroll / MaxScroll) * (Height - scrollBarHeight + 1));
				if (Scroll > 0 && MaxScroll > 0) { // When scrolled, put bar at least one away from top.
					scrollBarTop = Math.Max(1, scrollBarTop);
				}
				if (scrollBarTop + scrollBarHeight >= Height - 1) { // Only put scroll bar at bottom when scrolled all the way to bottom.
					scrollBarTop--;
				}

				if (scrollBarTop > 0) {
					DrawVerticalLine(Left, Top, (int)scrollBarTop, ' ', Background, Background);
				}

				DrawVerticalLine(Left, Top + (int)scrollBarTop, (int)scrollBarHeight, ' ', Foreground, Foreground);

				if (Scroll < MaxScroll) {
					DrawVerticalLine(
						Left,
						Top + (int)(scrollBarTop + scrollBarHeight),
						(int)(Height - (scrollBarTop + scrollBarHeight)),
						' ',
						Background,
						Background);
				}
			}

			public void Init() {
				scrollBarHeight = Math.Floor(Height / (Height + MaxScroll) * Height);
			}
		}
		public class SelectableText : Drawable
		{
			public SelectableText(int left, int top, int width, string text) : base(left, top, width, 1, MenuColor.ContentBG, MenuColor.ContentFG) {
				Left = left;
				Top = top;
				Text = text;
				Enabled = true;
				Visible = true;
				SelectionStatus = Properties.SelectionStatus.None;
			}

			public SelectableText(int left, int top, string text) : base(left, top, text.Length + 2, 1, MenuColor.ContentBG, MenuColor.ContentFG) {
				Left = left;
				Top = top;
				Text = text;
				Enabled = true;
				Visible = true;
				SelectionStatus = Properties.SelectionStatus.None;
			}

			public bool Enabled { get; set; }
			public Properties.SelectionStatus SelectionStatus { get; set; }
			public string Text { get; set; }
			public bool Visible { get; set; }

			public void Deselect() {
				SelectionStatus = Properties.SelectionStatus.None;
				Draw();
			}

			public override void Draw() {
				if (Visible) {
					Console.SetCursorPosition(Left, Top);
					SetConsoleColor();
					Console.Write(ToString());
				}
			}

			public void Highlight() {
				SelectionStatus = Properties.SelectionStatus.Highlighted;
				Draw();
			}

			public void Select() {
				SelectionStatus = Properties.SelectionStatus.Selected;
				Draw();
			}

			public void SetConsoleColor() {
				if (Enabled) {
					switch (SelectionStatus) {
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
				} else {
					if (SelectionStatus == Properties.SelectionStatus.Selected || SelectionStatus == Properties.SelectionStatus.Highlighted) {
						Console.BackgroundColor = MenuColor.SelectedDisabledBG;
						Console.ForegroundColor = MenuColor.SelectedDisabledFG;
					} else {
						Console.BackgroundColor = MenuColor.DisabledBG;
						Console.ForegroundColor = MenuColor.DisabledFG;
					}
				}
			}

			public override string ToString() {
				string fixedWidthText = FixedWidth(Text, Width - 2);
				if (SelectionStatus == Properties.SelectionStatus.None) {
					return CarrionManagerConsole.Text.UnselectedLeftSymbol + fixedWidthText + CarrionManagerConsole.Text.UnselectedRightSymbol;
				} else if (SelectionStatus == Properties.SelectionStatus.Selected) {
					return CarrionManagerConsole.Text.SelectedLeftSymbol + fixedWidthText + CarrionManagerConsole.Text.SelectedRightSymbol;
				} else if (SelectionStatus == Properties.SelectionStatus.Highlighted) {
					return CarrionManagerConsole.Text.HighlightedLeftSymbol + fixedWidthText + CarrionManagerConsole.Text.HighlightedRightSymbol;
				} else {
					throw new Exception(string.Format("Unsupported SelectionOption \"{0}\"", SelectionStatus.ToString()));
				}
			}
		}
		public abstract class SelectableTextContainer : Navigable
		{
			private Properties.Alignment alignment;
			private bool canNavigateHorizontally;
			private bool canNavigateVertically;

			public SelectableTextContainer(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, Properties.Alignment alignment) :
				base(left, top, width, height, background, foreground) {
				Alignment = alignment;
				InitItems();
			}

			#region INavigable
			public override bool CanNavigate => !IsEmpty;
			public override bool CanNavigateDown => canNavigateVertically;
			public override bool CanNavigateLeft => canNavigateHorizontally;
			public override bool CanNavigateRight => canNavigateHorizontally;
			public override bool CanNavigateUp => canNavigateVertically;
			public override int CurrentColumn {
				get {
					if (Alignment == Properties.Alignment.Horizontal) {
						return SelectedItemIndex;
					} else if (Alignment == Properties.Alignment.Vertical) {
						return 1;
					} else {
						throw new Exception(Text.InvalidAlignment);
					}
				}
			}
			public override int CurrentRow {
				get {
					if (Alignment == Properties.Alignment.Horizontal) {
						return 1;
					} else if (Alignment == Properties.Alignment.Vertical) {
						return SelectedItemIndex;
					} else {
						throw new Exception(Text.InvalidAlignment);
					}
				}
			}
			public override bool IsActive => IndexValid(SelectedItemIndex);

			public override void Deactivate() {
				Deselect();
			}

			public abstract override void Draw();

			public override void NavigateDown() {
				if (CanNavigateDown) {
					Navigate(1);
				}
			}

			public override void NavigateLeft() {
				if (CanNavigateLeft) {
					Navigate(-1);
				}
			}

			public override void NavigateRight() {
				if (CanNavigateRight) {
					Navigate(1);
				}
			}

			public override void NavigateToDefault() {
				SelectFirstItem();
			}

			public override void NavigateToFirstColumn(int row) {
				if (Alignment == Properties.Alignment.Horizontal) {
					SelectLastItem();
				} else if (Alignment == Properties.Alignment.Vertical) {
					Select(row);
				}
			}
			public override void NavigateToFirstRow(int column) {
				if (Alignment == Properties.Alignment.Horizontal) {
					Select(column);
				} else if (Alignment == Properties.Alignment.Vertical) {
					SelectLastItem();
				}
			}

			public override void NavigateToLastColumn(int row) {
				NavigateToFirstColumn(row);
			}

			public override void NavigateToLastRow(int column) {
				NavigateToFirstRow(column);
			}

			public override void NavigateUp() {
				if (CanNavigateUp) {
					Navigate(-1);
				}
			}

			public override void PageDown() {
				if (CanNavigateDown) {
					Navigate(Height);
				}
			}

			public override void PageUp() {
				if (CanNavigateUp) {
					Navigate(-Height);
				}
			}

			#endregion

			#region SelectableTextContainer
			public Properties.Alignment Alignment {
				get { return alignment; }
				set {
					alignment = value;
					canNavigateHorizontally = alignment == Properties.Alignment.Horizontal;
					canNavigateVertically = alignment == Properties.Alignment.Vertical;
				}
			}
			public List<SelectableText> Items { get; set; }
			public bool IsEmpty => Items == null || Items.Count == 0;
			public SelectableText SelectedItem => IndexValid(SelectedItemIndex) ? Items[SelectedItemIndex] : null;
			public int SelectedItemIndex { get; set; }

			public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

			public int ClampIndex(int index) {
				if (IsEmpty) {
					return 0;
				} else {
					return Math.Clamp(index, 0, Items.Count - 1);
				}
			}

			public void Deselect() {
				if (IndexValid(SelectedItemIndex)) {
					SelectedItem.Deselect();
				}
				SelectedItemIndex = -1;
			}

			public virtual void HandleSelectionChanged(SelectionChangedEventArgs e) {
				SelectionChanged?.Invoke(this, e);
			}

			public void HighlightCurrentItem() {
				if (IndexValid(SelectedItemIndex)) {
					SelectedItem.Highlight();
				}
			}

			public void InitItems() {
				SelectedItemIndex = 0;
				Items = new List<SelectableText>();
			}

			public bool IndexValid(int index) {
				return !IsEmpty && index >= 0 && index < Items.Count;
			}

			public abstract void Navigate(int offset);

			public Selection PromptSelection() {
				var input = PromptInput();
				return new Selection(this, CurrentColumn, CurrentRow, input.Command);
			}

			public void Select(int index) {
				if (IsEmpty) {
					return;
				}
				var previousItemIndex = SelectedItemIndex;
				var previousItem = SelectedItem;
				Deselect();
				SelectedItemIndex = ClampIndex(index);
				SelectedItem.Select();

				var args = new SelectionChangedEventArgs() {
					PreviousItem = previousItem,
					PreviousItemIndex = previousItemIndex,
					SelectedItem = SelectedItem,
					SelectedItemIndex = SelectedItemIndex,
				};
				HandleSelectionChanged(args);
			}

			public void SelectFirstItem() {
				if (!IsEmpty) {
					Select(0);
				}
			}

			public void SelectLastItem() {
				if (!IsEmpty) {
					Select(Items.Count - 1);
				}
			}
			#endregion
		}
		public class Selection
		{
			public Selection(SelectableTextContainer list, int columnIndex, int rowIndex, Properties.Command command) {
				List = list;
				ColumnIndex = columnIndex;
				RowIndex = rowIndex;
				Command = command;
			}

			public Properties.Command Command { get; set; }
			public int ColumnIndex { get; set; }
			public SelectableTextContainer List { get; set; }
			public int RowIndex { get; set; }
			public SelectableText SelectedItem => List.Items[(List.Alignment == Properties.Alignment.Horizontal ? ColumnIndex : RowIndex)];
			public string Text => SelectedItem.Text;
		}
		public class SelectionPrompt : Drawable
		{
			public SelectionPrompt(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) { }

			public override void Draw() {
				throw new NotImplementedException();
			}

			public int PromptSelection(string[] selectableItems, Options options) {
				this.Clear();
				var items = new List<SelectableText>();
				int textLeft = this.Left;
				for (int i = 0; i < selectableItems.Length; ++i) {
					var item = selectableItems[i];
					var selectableText = new SelectableText(textLeft, this.Top, item);
					if (options.disabledItems.Contains(i)) {
						selectableText.Enabled = false;
					}
					items.Add(selectableText);
					textLeft = selectableText.Right + 1;
				}

				if (options.cancel) {
					items.Add(new SelectableText(textLeft, Top, Text.Cancel));
				}

				int selected = options.index;
				if (selected == -1) {
					for (int i = 0; i < items.Count; ++i) {
						if (items[i].Enabled) {
							selected = i;
							break;
						}
					}
					if (selected == -1) {
						throw new Exception("Prompted selection without valid items!");
					}
				}
				items[selected].SelectionStatus = Properties.SelectionStatus.Selected;

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
									items[selected].SelectionStatus = Properties.SelectionStatus.None;
									selected--;
									items[selected].SelectionStatus = Properties.SelectionStatus.Selected;
								}
								break;
							case Properties.Command.NavigateRight:
								if (selected < items.Count - 1) {
									inputValid = true;
									items[selected].SelectionStatus = Properties.SelectionStatus.None;
									selected++;
									items[selected].SelectionStatus = Properties.SelectionStatus.Selected;
								}
								break;
							case Properties.Command.Confirm:
								if (items[selected].Enabled) {
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
				Clear();
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
				public List<int> disabledItems;

				public Options() {
					cancel = false;
					index = -1;
					disabledItems = new List<int>();
				}
			}
		}
		public class TextBox : Drawable
		{
			private int nextEmptyLine;

			public TextBox(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				Content = new string[Height];
				nextEmptyLine = 0;
			}

			public string[] Content { get; set; }
			public int RemainingFreeLines => nextEmptyLine == -1 ? 0 : Height - nextEmptyLine;

			public void AppendLastLine(string text) {
				int lastWrittenLine = nextEmptyLine == -1 ? Height - 1 : nextEmptyLine - 1;
				Content[lastWrittenLine] += text;
				Write(Left, Top + lastWrittenLine, FixedWidth(Content[lastWrittenLine], Width));
			}

			public void ClearContent() {
				for (int i = 0; i < Content.Length; ++i) {
					Content[i] = string.Empty;
				}
				nextEmptyLine = 0;
				Draw();
			}

			public override void Draw() {
				for (int i = 0; i < Content.Length; ++i) {
					Write(Left, Top + i, FixedWidth(Content[i], Width), Background, Foreground);
				}
			}

			public void WriteLine(string text) {
				if (nextEmptyLine == -1) {
					for (int i = 0; i < Content.Length - 1; ++i) {
						Content[i] = Content[i + 1];
					}
					Content[^1] = text;
					Draw();
				} else {
					Content[nextEmptyLine] = text;
					Write(Left, Top + nextEmptyLine, FixedWidth(text, Width), Background, Foreground);
					nextEmptyLine++;
					if (nextEmptyLine >= Height) {
						nextEmptyLine = -1; // No empty lines remain
					}
				}
			}

			public void WriteLine() {
				WriteLine(string.Empty);
			}
		}

		public interface IDrawable
		{
			public int Left { get; set; }
			public int Top { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public ConsoleColor Background { get; set; }
			public ConsoleColor Foreground { get; set; }

			public int Bottom { get; }
			public int Right { get; }

			public abstract void Clear();
			public abstract void Draw();
		}
		public interface INavigable : IDrawable
		{
			public bool CanNavigate { get; }
			public bool CanNavigateDown { get; }
			public bool CanNavigateLeft { get; }
			public bool CanNavigateRight { get; }
			public bool CanNavigateUp { get; }
			public int CurrentColumn { get; }
			public int CurrentRow { get; }
			public bool IsActive { get; }

			public abstract void Deactivate();
			public abstract void NavigateDown();
			public abstract void NavigateLeft();
			public abstract void NavigateRight();
			public abstract void NavigateToDefault();
			public abstract void NavigateToFirstColumn(int row);
			public abstract void NavigateToFirstRow(int column);
			public abstract void NavigateToLastColumn(int row);
			public abstract void NavigateToLastRow(int column);
			public abstract void NavigateUp();
			public abstract void PageDown();
			public abstract void PageUp();
			public abstract Input PromptInput();
		}

		#region Event Arguments
		public class SelectionChangedEventArgs : EventArgs
		{
			public int SelectedItemIndex { get; set; }
			public int PreviousItemIndex { get; set; }
			public SelectableText SelectedItem { get; set; }
			public SelectableText PreviousItem { get; set; }
		}
		#endregion
	}
}
