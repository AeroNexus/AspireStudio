using System;
using System.Collections.Generic;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.CoreModels
{
	public class AspireManager : IManager
	{
		static AspireManager()
		{
			Scenario.AddManager(new AspireManager());
			var text = Environment.GetEnvironmentVariable("FindAspireComponentWithWholeAddress");
			if (text != null)
				FindComponentWithWholeAddress = bool.Parse(text);
		}

		public static bool FindComponentWithWholeAddress { get; set; }

		#region IManager Members

		public void Unload()
		{
			PnPBrowsers.Unload();
		}

		#endregion
	}
}
