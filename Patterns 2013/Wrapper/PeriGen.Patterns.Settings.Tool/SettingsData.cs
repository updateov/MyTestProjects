namespace PeriGen.Patterns.Settings.Tool
{
	internal class SettingData
	{
		public string Type
		{
			get;
			set;
		}
		public string Key
		{
			get;
			set;
		}
		public string Value
		{
			get;
			set;
		}
		public string ResetValue
		{
			get;
			set;
		}
		public string Info
		{
			get;
			set;
		}
		public override string ToString()
		{
			return Key;
		}
		public bool Edited
		{
			get;
			set;
		}
	}
}
