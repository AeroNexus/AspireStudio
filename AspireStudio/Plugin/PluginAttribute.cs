using System;

namespace Aspire.Studio.Plugin
{
	[AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Class)]
	public class PluginAttribute : Attribute
	{
		public PluginAttribute()
		{
		}
	}
}