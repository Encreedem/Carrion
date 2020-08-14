using System;
using System.Collections.Generic;
using System.Text;

namespace CarrionManagerConsole
{
	/*TODO:
	 * Maybe support more than just loading levels and scripts.
	 * */
	
	class Map
	{
		public string Name;
		public string[] Levels;
		public string[] Scripts;
		public string startupLevel;

		public Map() {
			/*this.Name = String.Empty;
			this.Levels = new string[0];
			this.Scripts = new string[0];*/
		}

		public Map(string name, string[] levels, string[] scripts) {
			this.Name = name;
			this.Levels = levels;
			this.Scripts = scripts;
			if (this.Levels.Length == 1) {
				this.startupLevel = this.Levels[0].Substring(this.Levels[0].Length - Program.LevelFileExtension.Length);
			}
			else {
				this.startupLevel = string.Empty;
			}
		}

		public override string ToString() {
			return this.Name;
		}
	}
}
