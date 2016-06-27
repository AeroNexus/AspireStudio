using System;

namespace Aspire.Studio.Plugin
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class SettingsContentAttribute : Attribute
	{
		public string Content { get; set; }

		public SettingsContentAttribute(string settingsContent)
		{
			this.Content = settingsContent;
		}
	}
}