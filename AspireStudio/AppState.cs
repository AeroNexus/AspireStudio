using System;
using System.Collections.Generic;
using System.Reflection;

using Aspire.Framework;
using Aspire.Studio.Plugin;

namespace Aspire.Studio
{
	public class AppState : IPublishable
	{
		MainForm mForm;

		public AppState(MainForm form)
		{
			mForm = form;
		}

		[Blackboard("Blackboard.Count")]
		public int BlackboardCount { get { return Blackboard.ItemsCount; } }

		//[Blackboard("Blackboard.MemberInfoCache")]
		//public Dictionary<Type,MemberInfo[]> BlackboardMemInfoCache { get { return Blackboard.MemberInfoCache; } }

		public static ConfigurationFile ConfigurationFile { get; set; }

		[Blackboard]
		public static string InstallDirectory { get; set; }

		[Blackboard]
		public string ScenarioDirectory { get { return Environment.CurrentDirectory; } }

		[Blackboard]
		public Scenario Scenario { get { return mForm.Scenario; } }

		[Blackboard]
		public Solution Solution { get { return mForm.Solution; } }

		#region IPublishable Members

		public string Path
		{
			get { return Name; }
			set {}
		}

		public string Name
		{
			get { return "Application"; }
			set { }
		}

		public IPublishable Parent
		{
			get { return null; }
			set { }
		}

		#endregion
	}
}
