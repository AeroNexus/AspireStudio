using System;
using System.ComponentModel;

using Aspire.Framework;

namespace Aspire.CoreModels
{
	public interface IUseXteds : IPublishable
	{
		void ChangeProperties(string propertyName);
		Blackboard.Item BlackboardSubscribe(string itemName);
		//string Path { get; }
		Model ParentModel { get; set; }
		string XtedsFile { get; }
		Xteds Xteds { get; }
	}
}
