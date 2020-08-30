using System;
using System.Collections.Generic;
using System.Data;
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
		public class CheckBox : SelectableText
		{
			public CheckBox(int left, int top, int width, string text, bool isChecked) : base(left, top, width, text) {
				Checked = isChecked;
			}

			public CheckBox(int left, int top, string text, bool isChecked) : base(left, top, text) {
				Checked = isChecked;
			}

			public bool Checked { get; set; }

			public override void Draw() {
				if (!Visible) {
					return;
				}
				Write(Left, Top, LeftSymbol + Text.CheckBoxLeftSybmol, Background, Foreground);
				Console.BackgroundColor = Checked ? MenuColor.CheckBoxCheckedBG : MenuColor.CheckBoxUncheckedBG;
				Console.ForegroundColor = Checked ? MenuColor.CheckBoxCheckedFG : MenuColor.CheckBoxUncheckedFG;
				Console.Write(Checked ? Text.CheckBoxChecked : Text.CheckBoxUnchecked);
				Write(Left + 3, Top, FixedWidth(Text.CheckBoxRightSymbol + " " + Value, Width - 4), Background, Foreground);
				Console.Write(RightSymbol);
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
				Items = new Drawable[columnCount];
				Navigables = new Navigable[columnCount];
				SelectableTextContainers = new SelectableCollection[columnCount];
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
			public Navigable CurrentNavigable => CurrentColumnValid ? Navigables[CurrentColumn] : null;
			public SelectableCollection CurrentSelectableTextContainer => SelectionValid() ? SelectableTextContainers[CurrentColumn] : null;
			public Label[] Headers { get; set; }
			public override bool IsActive => SelectionValid();
			public Drawable[] Items { get; set; }
			public Navigable[] Navigables { get; set; }
			public SelectableCollection[] SelectableTextContainers { get; set; }

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

			public void AddNavigable(int column, Navigable navigable) {
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
		public abstract class Drawable
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
			public virtual ConsoleColor Background { get; set; }
			public virtual ConsoleColor Foreground { get; set; }

			public void Clear() {
				DrawRectangle(Left, Top, Width, Height, Background, Foreground);
			}

			public abstract void Draw();
		}
		public class Input
		{
			public Input(Navigable list, int columnIndex, int rowIndex, Properties.Command command) {
				List = list;
				ColumnIndex = columnIndex;
				RowIndex = rowIndex;
				Command = command;
			}

			public int RowIndex { get; set; }
			public int ColumnIndex { get; set; }
			public Properties.Command Command { get; set; }
			public Navigable List { get; set; }
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
		public class ListBox : SelectableCollection
		{
			public int scroll;
			private readonly ScrollBar scrollBar;

			public ListBox(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, bool forceShowScrollBar) :
				base(left, top, width, height, background, foreground, Properties.Alignment.Vertical) {
				scrollBar = new ScrollBar(Right, Top, 1, Height, MenuColor.ScrollBarBG, MenuColor.ScrollBarFG) {
					MaxScroll = Items.Count - Height,
					Scroll = scroll,
					ForceShow = forceShowScrollBar,
				};
			}

			public int NextItemTop => Items.Count == 0 ? Top : Items[^1].Top + 1;
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

			public CheckBox AddCheckBox(string text, bool isChecked) {
				int top = NextItemTop;
				var checkBox = new CheckBox(Left, top, Width - 1, text, isChecked);
				Items.Add(checkBox);
				SetItemVisibility(checkBox);
				return checkBox;
			}

			public void AddItem(string text) {
				var item = new SelectableText(Left, NextItemTop, Width - 1, text);
				Items.Add(item);
				SetItemVisibility(item);
			}

			public void AddItems(string[] items) {
				int top = NextItemTop;
				int textWidth = Width - 1;
				for (int i = 0; i < items.Length; ++i) {
					var item = new SelectableText(Left, top + i, textWidth, items[i]);
					Items.Add(item);
					SetItemVisibility(item);
				}
				RefreshContentInfo();
			}

			public void AddMaps(List<Map> maps) {
				int top = NextItemTop;
				int textWidth = Width - 1;
				for (int i = 0; i < maps.Count; ++i) {
					var item = new SelectableMap(Left, top + i, textWidth, maps[i]);
					Items.Add(item);
					SetItemVisibility(item);
				}
				RefreshContentInfo();
			}

			public override void Draw() {
				foreach (var item in VisibleItems) {
					item.Draw();
				}
				scrollBar.Draw();
			}

			public override void Init() {
				base.Init();
				scroll = 0;
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

			public override void Select(int index) {
				base.Select(index);
				ScrollToSelectedItem();
			}

			public void SelectCurrentItem() {
				Select(SelectedItemIndex);
			}

			public override void SetItems(string[] content) {
				Items.Clear();
				AddItems(content);
			}

			public void SetItemVisibility(SelectableText item) {
				item.Visible = (item.Top >= Top && item.Top < Top + Height);
			}

			private void RefreshContentInfo() {
				scrollBar.MaxScroll = Items.Count - Height;
			}
		}
		public abstract class Navigable : Drawable
		{
			public Navigable(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {

			}

			public abstract bool CanNavigate { get; }
			public abstract bool CanNavigateDown { get; }
			public abstract bool CanNavigateLeft { get; }
			public abstract bool CanNavigateRight { get; }
			public abstract bool CanNavigateUp { get; }
			public abstract int CurrentColumn { get; }
			public abstract int CurrentRow { get; }
			public abstract bool IsActive { get; }

			public abstract void Deactivate();
			public override abstract void Draw();
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
					if (!Program.navigationKeybindings.ContainsKey(input)) {
						continue;
					}

					Properties.Command command = Program.navigationKeybindings[input];
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
		public class SelectableMap : SelectableText
		{
			public SelectableMap(int left, int top, Map map) : base(left, top, map.Name) {
				Map = map;
			}

			public SelectableMap(int left, int top, int width, Map map) : base(left, top, width, map.Name) {
				Map = map;
			}

			public Map Map { get; set; }
		}
		public class SelectableText : Drawable
		{
			public SelectableText(int left, int top, int width, string text) : base(left, top, width, 1, MenuColor.ContentBG, MenuColor.ContentFG) {
				Value = text;
				Enabled = true;
				Visible = true;
				SelectionStatus = Properties.SelectionStatus.None;
			}

			public SelectableText(int left, int top, string text) : base(left, top, text.Length + 2, 1, MenuColor.ContentBG, MenuColor.ContentFG) {
				Value = text;
				Enabled = true;
				Visible = true;
				SelectionStatus = Properties.SelectionStatus.None;
			}

			public override ConsoleColor Background => SelectionStatus switch
			{
				Properties.SelectionStatus.None => Enabled ? MenuColor.ContentBG : MenuColor.DisabledBG,
				Properties.SelectionStatus.Selected => Enabled ? MenuColor.SelectedBG : MenuColor.SelectedDisabledBG,
				Properties.SelectionStatus.Highlighted => Enabled ? MenuColor.HighlightBG : MenuColor.SelectedDisabledBG,
				_ => Enabled ? MenuColor.ContentBG : MenuColor.DisabledBG,
			};
			public override ConsoleColor Foreground => SelectionStatus switch
			{
				Properties.SelectionStatus.None => Enabled ? MenuColor.ContentFG : MenuColor.DisabledFG,
				Properties.SelectionStatus.Selected => Enabled ? MenuColor.SelectedFG : MenuColor.SelectedDisabledFG,
				Properties.SelectionStatus.Highlighted => Enabled ? MenuColor.HighlightFG : MenuColor.SelectedDisabledFG,
				_ => Enabled ? MenuColor.ContentFG : MenuColor.DisabledFG,
			};
			public bool Enabled { get; set; }
			public Properties.SelectionStatus SelectionStatus { get; set; }
			public string Value { get; set; }
			public bool Visible { get; set; }

			protected string LeftSymbol => SelectionStatus switch
			{
				Properties.SelectionStatus.None => CarrionManagerConsole.Text.UnselectedLeftSymbol,
				Properties.SelectionStatus.Selected => CarrionManagerConsole.Text.SelectedLeftSymbol,
				Properties.SelectionStatus.Highlighted => CarrionManagerConsole.Text.HighlightedLeftSymbol,
				_ => CarrionManagerConsole.Text.UnselectedLeftSymbol,
			};
			protected string RightSymbol => SelectionStatus switch
			{
				Properties.SelectionStatus.None => CarrionManagerConsole.Text.UnselectedRightSymbol,
				Properties.SelectionStatus.Selected => CarrionManagerConsole.Text.SelectedRightSymbol,
				Properties.SelectionStatus.Highlighted => CarrionManagerConsole.Text.HighlightedRightSymbol,
				_ => CarrionManagerConsole.Text.UnselectedRightSymbol,
			};

			public void Deselect() {
				SelectionStatus = Properties.SelectionStatus.None;
				Draw();
			}

			public override void Draw() {
				if (Visible) {
					Write(Left, Top, LeftSymbol + FixedWidth(Value, Width - 2) + RightSymbol, Background, Foreground);
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
		}
		public abstract class SelectableCollection : Navigable
		{
			private Properties.Alignment alignment;
			private bool canNavigateHorizontally;
			private bool canNavigateVertically;

			public SelectableCollection(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, Properties.Alignment alignment) :
				base(left, top, width, height, background, foreground) {
				Alignment = alignment;
				Init();
			}

			#region Navigable
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

			public abstract void SetItems(string[] content);

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

			#region SelectableCollection
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

			public void OnSelectionChanged(SelectionChangedEventArgs e) {
				SelectionChanged?.Invoke(this, e);
			}

			public void HighlightCurrentItem() {
				if (IndexValid(SelectedItemIndex)) {
					SelectedItem.Highlight();
				}
			}

			public virtual void Init() {
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

			public virtual void Select(int index) {
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
				OnSelectionChanged(args);
			}

			public virtual void SelectFirstItem() {
				if (!IsEmpty) {
					Select(0);
				}
			}

			public virtual void SelectLastItem() {
				if (!IsEmpty) {
					Select(Items.Count - 1);
				}
			}
			#endregion
		}
		public class Selection
		{
			public Selection(SelectableCollection list, int columnIndex, int rowIndex, Properties.Command command) {
				List = list;
				ColumnIndex = columnIndex;
				RowIndex = rowIndex;
				Command = command;
			}

			public Properties.Command Command { get; set; }
			public int ColumnIndex { get; set; }
			public SelectableCollection List { get; set; }
			public int RowIndex { get; set; }
			public SelectableText SelectedItem => List.Items[(List.Alignment == Properties.Alignment.Horizontal ? ColumnIndex : RowIndex)];
			public string Text => SelectedItem.Value;
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
					if (options.DisabledItems.Contains(i)) {
						selectableText.Enabled = false;
					}
					items.Add(selectableText);
					textLeft = selectableText.Right + 1;
				}

				if (options.AllowCancel) {
					items.Add(new SelectableText(textLeft, Top, Text.Cancel));
				}

				int selected = options.Index;
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
						if (!Program.navigationKeybindings.ContainsKey(key))
							continue;

						var command = Program.navigationKeybindings[key];
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
									if (options.AllowCancel && selected == (items.Count - 1)) {
										selected = -1;
									}
									inputValid = true;
									finished = true;
								}
								break;
							case Properties.Command.Cancel:
								if (options.AllowCancel) {
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
					AllowCancel = cancel,
				};
				return PromptSelection(selectableItems, options);
			}

			public class Options
			{
				/// <summary>
				/// Adds the "Cancel" option to the end of the list. Prompt accepts Cancel (e.g. Esc-Key) command.
				/// </summary>
				public bool AllowCancel { get; set; }
				/// <summary>
				/// The zero-based index of the initially selected item.
				/// </summary>
				public int Index;
				/// <summary>
				/// The zero-based list of all disabled items' indexes.
				/// </summary>
				public List<int> DisabledItems;

				public Options() {
					AllowCancel = false;
					Index = -1;
					DisabledItems = new List<int>();
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

			public void WriteAllMapInfo(Map map) {
				ClearContent();
				WriteLine(Text.MapInfoMapName + map.Name);
				WriteLine(Text.MapInfoVersion + (string.IsNullOrEmpty(map.Version) ? Text.MapInfoNoVersion : map.Version));
				WriteLine(Text.MapInfoAuthor + (string.IsNullOrEmpty(map.Author) ? Text.MapInfoNoAuthor : map.Author));
				WriteLine(Text.MapInfoStartupLevel + (string.IsNullOrEmpty(map.StartupLevel) ? Text.MapInfoNoStartupLevel : map.StartupLevel));
				WriteLine(Text.MapInfoIsWIP + map.IsWIP.ToString());
				WriteLine(Text.MapInfoShortDescription + (string.IsNullOrEmpty(map.ShortDescription) ? Text.MapInfoNoDescription : map.ShortDescription));
				if (string.IsNullOrEmpty(map.LongDescription)) {
					WriteLine(Text.MapInfoLongDescription + Text.MapInfoNoDescription);
				} else {
					WriteLine(Text.MapInfoLongDescription);
					List<string> longDescription = Program.SplitIntoLines(map.LongDescription, Width);
					for (int i = 0; i < longDescription.Count && nextEmptyLine != -1; ++i) {
						WriteLine(longDescription[i]);
					}
				}
			}

			public void WriteShortMapInfo(Map map) {
				ClearContent();
				string firstLine = Text.MapInfoMapName + map.Name;
				if (map.Version != null) {
					firstLine += Text.MapInfoSeparator + Text.MapInfoVersion + map.Version;
				}
				if (map.Author != null) {
					firstLine += Text.MapInfoSeparator + Text.MapInfoAuthor + map.Author;
				}
				WriteLine(firstLine);

				if (map.ShortDescription != null) {
					WriteLine(Text.MapInfoShortDescription + map.ShortDescription);
				}

				if (!map.IsValid) {
					for (int currentIssue = 0; currentIssue < map.Issues.Count; ++currentIssue) {
						if (RemainingFreeLines > 1 || map.Issues.Count - currentIssue == 1) {
							WriteLine(Text.MapHasIssuesIndicator + map.Issues[currentIssue]);
						} else {
							WriteLine(string.Format(Text.SoManyMoreIssues, map.Issues.Count - currentIssue));
							break;
						}
					}
				}
			}

			public void WriteLongMapInfo(Map map) {
				ClearContent();
				WriteLine(Text.MapInfoMapName + map.Name);
				WriteLine(Text.MapInfoVersion + (string.IsNullOrEmpty(map.Version) ? Text.MapInfoNoVersion : map.Version));
				WriteLine(Text.MapInfoAuthor + (string.IsNullOrEmpty(map.Author) ? Text.MapInfoNoAuthor : map.Author));
				WriteLine(Text.MapInfoStartupLevel + (string.IsNullOrEmpty(map.StartupLevel) ? Text.MapInfoNoStartupLevel : map.StartupLevel));
				if (string.IsNullOrEmpty(map.LongDescription)) {
					WriteLine(Text.MapInfoLongDescription + Text.MapInfoNoDescription);
				} else {
					WriteLine(Text.MapInfoLongDescription);
					List<string> longDescription = Program.SplitIntoLines(map.LongDescription, Width);
					for (int i = 0; i < longDescription.Count && nextEmptyLine != -1; ++i) {
						WriteLine(longDescription[i]);
					}
				}
			}
		}
		public class TextInput : Drawable
		{
			private int totalColumnIndex, cursorColumnIndex, cursorRowIndex;
			private int scroll;
			private List<string> textLines;

			public TextInput(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground) : base(left, top, width, height, background, foreground) {
				Text = string.Empty;
				PreviewText = string.Empty;
			}

			public TextInput(int left, int top, int width, int height, ConsoleColor background, ConsoleColor foreground, string text, string previewText, PromptOptions defaultOptions) : base(left, top, width, height, background, foreground) {
				Text = text ?? string.Empty;
				PreviewText = previewText ?? string.Empty;
				DefaultOptions = defaultOptions;
			}

			public PromptOptions DefaultOptions;
			public string PreviewText { get; set; }
			public string Text { get; set; }

			public override void Draw() {
				if (Height > 1) {
					for (int i = 0; i < textLines.Count; i++) {
						Write(Left, Top + i, FixedWidth(textLines[i], Width), Background, Foreground);
					}
				} else {
					Write(Left, Top, FixedWidth(Text, Width), Background, Foreground);
				}
			}

			/// <summary>
			/// Prompts the user to input text and stores the result it this instance's <see cref="Text"/> property.
			/// </summary>
			/// <param name="promptOptions">How the prompt will behave (e.g. whether to show preview text or whether the user can cancel).</param>
			/// <returns>
			/// <para>true if the user confirmed the input.</para>
			/// <para>false if they cancelled it (in which case the Text will revert to its string before the input)</para>
			/// </returns>
			public bool PromptText(PromptOptions promptOptions) {
				Console.CursorVisible = true;
				string initialText = Text;
				cursorColumnIndex = 0;
				scroll = 0;

				if (promptOptions == null) {
					promptOptions = new PromptOptions();
				}

				if (!string.IsNullOrEmpty(promptOptions.PreWrittenText)) {
					Text = promptOptions.PreWrittenText;
				} else if (promptOptions.PreWriteCurrentText) {
					// Keep current text
				} else {
					Text = string.Empty;
				}
				cursorColumnIndex = Text.Length;

				while (true) {
					bool textChanged = true;
					if (ScrollToCursorHorizontally() || textChanged) {
						textChanged = false;
						Console.CursorVisible = false;
						DrawPrompt();
						Console.CursorVisible = true;
					}
					Console.SetCursorPosition(Left + cursorColumnIndex - scroll, Top);

					var input = Console.ReadKey(true);
					if (Program.textInputKeybindings.ContainsKey(input.Key)) {
						switch (Program.textInputKeybindings[input.Key]) {
							case Properties.Command.Confirm:
								if (Text != string.Empty || promptOptions.AllowEmpty) {
									Console.CursorVisible = false;
									Draw();
									return true;
								}
								break;
							case Properties.Command.Cancel:
								if (promptOptions.CanCancel) {
									Console.CursorVisible = false;
									Text = initialText;
									Draw();
									return false;
								}
								break;
							case Properties.Command.NavigateLeft:
								if (cursorColumnIndex > 0) {
									cursorColumnIndex--;
								}
								break;
							case Properties.Command.NavigateRight:
								if (cursorColumnIndex < Text.Length) {
									cursorColumnIndex++;
								}
								break;
							case Properties.Command.GoToStart:
								cursorColumnIndex = 0;
								break;
							case Properties.Command.GoToEnd:
								cursorColumnIndex = Text.Length;
								break;
							case Properties.Command.DeletePreviousCharacter:
								if (cursorColumnIndex > 0) {
									Text = Text.Remove(--cursorColumnIndex, 1);
									textChanged = true;
								}
								break;
							case Properties.Command.DeleteCurrentCharacter:
								if (cursorColumnIndex <= Text.Length - 1) {
									Text = Text.Remove(cursorColumnIndex);
									textChanged = true;
								}
								break;
						}
					} else {
						char charInput = input.KeyChar;
						if (cursorColumnIndex == Text.Length) {
							Text += charInput;
							cursorColumnIndex++;
							textChanged = true;
						} else {
							Text = Text.Insert(cursorColumnIndex++, charInput.ToString());
							textChanged = true;
						}
					}
				}
			}

			/// <summary>
			/// Prompts the user to input text and stores the result it this instance's <see cref="Text"/> property.
			/// </summary>
			/// <remarks>
			/// <para>This method will use the default <see cref="PromptOptions"/>.</para>
			/// <para>Use <see cref="PromptText(PromptOptions)"/> to customize them.</para>
			/// </remarks>
			/// <returns>
			/// <para>true if the user confirmed the input.</para>
			/// <para>false if they cancelled it (in which case the Text will revert to its string before the input)</para>
			/// </returns>
			public bool PromptText() {
				return PromptText(DefaultOptions);
			}

			public bool PromptTextMultiline(PromptOptions promptOptions) {
				Console.CursorVisible = true;
				string initialText = Text;

				if (promptOptions == null) {
					promptOptions = new PromptOptions();
				}

				if (!string.IsNullOrEmpty(promptOptions.PreWrittenText)) {
					Text = promptOptions.PreWrittenText;
				} else if (promptOptions.PreWriteCurrentText) {
					// Keep current text
				} else {
					Text = string.Empty;
				}
				textLines = Program.SplitIntoLines(Text, Width);
				cursorRowIndex = textLines.Count - 1;
				cursorColumnIndex = textLines[cursorRowIndex].Length;
				totalColumnIndex = -1;
				foreach (string line in textLines) {
					totalColumnIndex += line.Length + 1;
				}

				while (true) {
					bool textChanged = true;
					if (ScrollToCursorVertically() || textChanged) {
						textChanged = false;
						Console.CursorVisible = false;
						DrawPromptMultiline();
						Console.CursorVisible = true;
					}
					Console.SetCursorPosition(Left + cursorColumnIndex, Top + cursorRowIndex - scroll);

					ConsoleKeyInfo input = Console.ReadKey(true);
					if (Program.textInputKeybindings.ContainsKey(input.Key) &&
						!(input.Key == ConsoleKey.Enter && input.Modifiers == ConsoleModifiers.Shift)) {
						switch (Program.textInputKeybindings[input.Key]) {
							case Properties.Command.Confirm:
								if (Text != string.Empty || promptOptions.AllowEmpty) {
									Console.CursorVisible = false;
									Draw();
									return true;
								}
								break;
							case Properties.Command.Cancel:
								if (promptOptions.CanCancel) {
									Console.CursorVisible = false;
									Text = initialText;
									Draw();
									return false;
								}
								break;
							case Properties.Command.NavigateDown:
								if (cursorRowIndex == textLines.Count - 2) {
									totalColumnIndex += textLines[cursorRowIndex].Length - cursorColumnIndex + 1;
									cursorColumnIndex = Math.Min(textLines[++cursorRowIndex].Length, cursorColumnIndex);
									totalColumnIndex += cursorColumnIndex;
								} else if (cursorRowIndex < textLines.Count - 2) {
									totalColumnIndex += textLines[cursorRowIndex].Length - cursorColumnIndex + 1;
									cursorColumnIndex = Math.Min(textLines[++cursorRowIndex].Length - 1, cursorColumnIndex);
									totalColumnIndex += cursorColumnIndex;
								}
								break;
							case Properties.Command.NavigateLeft:
								if (cursorColumnIndex > 0) {
									cursorColumnIndex--;
									totalColumnIndex--;
								} else if (cursorRowIndex > 0) {
									cursorColumnIndex = textLines[--cursorRowIndex].Length - 1;
									totalColumnIndex--;
								}
								break;
							case Properties.Command.NavigateRight:
								if (cursorRowIndex == textLines.Count - 1) { // Last row
									if (cursorColumnIndex < textLines[cursorRowIndex].Length) {
										cursorColumnIndex++;
										totalColumnIndex++;
									}
								} else {
									if (cursorColumnIndex == textLines[cursorRowIndex].Length - 1) {
										cursorRowIndex++; // End of line => go to next line
										cursorColumnIndex = 0;
									} else {
										cursorColumnIndex++;
									}

									totalColumnIndex++;
								}
								break;
							case Properties.Command.NavigateUp:
								if (cursorRowIndex > 0) {
									totalColumnIndex -= cursorColumnIndex + 1; // Get to end of previous line
									cursorColumnIndex = Math.Min(textLines[--cursorRowIndex].Length - 1, cursorColumnIndex);
									totalColumnIndex -= textLines[cursorRowIndex].Length - cursorColumnIndex; // Adjust total column index to cursor column index
								}
								break;
							case Properties.Command.GoToStart:
								totalColumnIndex -= cursorColumnIndex;
								cursorColumnIndex = 0;
								break;
							case Properties.Command.GoToEnd:
								if (cursorRowIndex == textLines.Count - 1) { // Last row
									totalColumnIndex += (textLines[cursorRowIndex].Length - cursorColumnIndex);
									cursorColumnIndex = textLines[cursorRowIndex].Length;
								} else {
									totalColumnIndex += (textLines[cursorRowIndex].Length - cursorColumnIndex - 1);
									cursorColumnIndex = textLines[cursorRowIndex].Length - 1;
								}
								break;
							case Properties.Command.DeletePreviousCharacter:
								if (totalColumnIndex > 0) {
									Text = Text.Remove(--totalColumnIndex, 1);
									if (cursorRowIndex == textLines.Count - 1 && cursorColumnIndex > 0) {
										// If the cursor stays in the last row after the character gets deleted
										textLines[cursorRowIndex] = textLines[cursorRowIndex].Remove(--cursorColumnIndex, 1);
										Write(Left, Top + cursorRowIndex, FixedWidth(textLines[cursorRowIndex], Width), Background, Foreground);
									} else {
										RefreshMultiLineText();
										textChanged = true;
									}
								}
								break;
							case Properties.Command.DeleteCurrentCharacter:
								if (totalColumnIndex < Text.Length) {
									Text = Text.Remove(totalColumnIndex, 1);
									if (cursorRowIndex == textLines.Count - 1 && cursorColumnIndex > 0) {
										// If the cursor stays in the last row after the character gets deleted
										textLines[cursorRowIndex] = textLines[cursorRowIndex].Remove(cursorColumnIndex, 1);
										Write(Left, Top + cursorRowIndex, FixedWidth(textLines[cursorRowIndex], Width), Background, Foreground);
									} else {
										RefreshMultiLineText();
										textChanged = true;
									}
								}
								break;
						}
					} else if (input.Key == ConsoleKey.Enter && input.Modifiers == ConsoleModifiers.Shift) {
						Text = Text.Insert(totalColumnIndex++, "\n");
						RefreshMultiLineText();
						textChanged = true;
					} else {
						char charInput = input.KeyChar;
						if (totalColumnIndex == Text.Length) {
							Text += charInput;
							if (++cursorColumnIndex <= Width) { // Only current line changed
								textLines[cursorRowIndex] += charInput;
								Write(Left, Top + cursorRowIndex, FixedWidth(textLines[cursorRowIndex], Width), Background, Foreground);
							} else {
								RefreshMultiLineText();
								textChanged = true;
							}
						} else {
							Text = Text.Insert(totalColumnIndex++, charInput.ToString());
							RefreshMultiLineText();
							textChanged = true;
						}
					}
				}
			}

			public bool PromptTextMultiline() {
				return PromptTextMultiline(DefaultOptions);
			}

			private void DrawPrompt() {
				if (string.IsNullOrEmpty(Text)) {
					Write(Left, Top, FixedWidth(PreviewText, Width), MenuColor.PreviewTextBG, MenuColor.PreviewTextFG);
					return;
				}

				string visibleText;
				if (scroll > 0) {
					if (Text.Length - scroll > Width) {
						visibleText = Text.Substring(scroll, Width);
					} else {
						visibleText = Text.Substring(scroll).PadRight(Width);
					}
				} else {
					visibleText = Text.PadRight(Width);
				}

				Write(Left, Top, visibleText, Background, Foreground);
			}

			private void DrawPromptMultiline() {
				if (string.IsNullOrEmpty(Text)) {
					string[] previewLines = PreviewText.Split('\n');
					for (int i = 0; i < previewLines.Length; i++) {
						Write(Left, Top + i, FixedWidth(previewLines[i], Width), MenuColor.PreviewTextBG, MenuColor.PreviewTextFG);
					}
					return;
				}

				int maxHeigth = Math.Min(textLines.Count - scroll, Height);
				for (int i = 0; i < maxHeigth; i++) {
					Write(Left, Top + i, FixedWidth(textLines[i + scroll], Width), Background, Foreground);
				}
				if (maxHeigth < Height) { // Clear the free line below just to be sure
					Write(Left, Top + maxHeigth, new string(' ', Width), Background, Foreground);
				}
			}

			private void RefreshMultiLineText() {
				textLines = Program.SplitIntoLines(Text, Width);
				int currentColumnIndex = 0;
				cursorColumnIndex = 0;
				cursorRowIndex = 0;
				foreach (string line in textLines) {
					if (currentColumnIndex + line.Length > totalColumnIndex) {
						cursorColumnIndex = totalColumnIndex - currentColumnIndex;
						return;
					} else {
						currentColumnIndex += line.Length + 1;
						cursorRowIndex++;
					}
				}
				// Reached end of text. Reverting last row increase.
				cursorColumnIndex = textLines[--cursorRowIndex].Length;
			}

			private bool ScrollToCursorHorizontally() {
				bool scrolled = false;
				if (scroll > 0 && Text.Length - scroll + 1 < Width) { // If there's unused space to the right
					scroll = Text.Length - Width + 1;
					scrolled = true;
				}
				if (cursorColumnIndex - scroll >= Width) { // If the cursor is too far to the right
					scroll = cursorColumnIndex - Width + 1;
					scrolled = true;

				} else if (cursorColumnIndex < scroll) { // If the cursor is too far to the left
					scroll = cursorColumnIndex;
					scrolled = true;
				}

				return scrolled;
			}

			private bool ScrollToCursorVertically() {
				bool scrolled = false;
				if (scroll > 0 && textLines.Count - scroll + 1 < Height) { // If there's unused space at the bottom
					scroll = textLines.Count - Height + 1;
					scrolled = true;
				}
				if (cursorRowIndex - scroll >= Height) { // If the cursor is too far to the bottom
					scroll = cursorRowIndex - Height + 1;
					scrolled = true;

				} else if (cursorRowIndex < scroll) { // If the cursor is too far to the top
					scroll = cursorRowIndex;
					scrolled = true;
				}

				return scrolled;
			}

			public class PromptOptions
			{
				public PromptOptions() {
					AllowEmpty = true;
					CanCancel = false;
					PreWriteCurrentText = false;
					PreWrittenText = string.Empty;
				}

				public PromptOptions(bool allowEmpty, bool canCancel, bool preWriteCurrentText, string preWrittenText) {
					AllowEmpty = allowEmpty;
					CanCancel = canCancel;
					PreWriteCurrentText = preWriteCurrentText;
					PreWrittenText = preWrittenText;
				}

				public bool AllowEmpty { get; set; }
				public bool CanCancel { get; set; }
				public bool PreWriteCurrentText { get; set; }
				public string PreWrittenText { get; set; }
			}
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
