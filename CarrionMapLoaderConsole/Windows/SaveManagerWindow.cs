using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	class SaveManagerWindow : IWindow
	{
		public SaveManagerWindow() {

		}

		public void Show() {
			Console.Clear();
			Console.SetCursorPosition(0, 0);
			Console.WriteLine("Not yet implemented");
			Console.WriteLine("Press any key to go back...");
			Console.ReadKey();
			Program.currentWindow = Program.navigationWindow;
		}
	}
}
