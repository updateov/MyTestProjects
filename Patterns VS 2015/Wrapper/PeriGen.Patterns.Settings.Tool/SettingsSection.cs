using System.Collections.Generic;

namespace PeriGen.Patterns.Settings.Tool
{
	internal class SettingsSection
	{
		public SettingsSection()
		{
			Settings = new List<SettingData>();
		}
		public int Level
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}
		public List<SettingData> Settings
		{
			get;
			private set;
		}
		public override string ToString()
		{
			return Name;
		}
	}
}
