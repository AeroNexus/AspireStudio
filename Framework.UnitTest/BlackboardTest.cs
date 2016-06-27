using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Aspire.Framework;
using Aspire.Utilities;

namespace Framework.UnitTest
{
	[TestClass]
	public class BlackboardTest
	{
		int one = 1;
		float two = 2.0f;
		static TestContext context;
		static ConsoleLogger consoleLog;

		class Thing : Model
		{
			[Blackboard]
			double value = 3;

			public Thing(string name) : base(name) { }

			[Blackboard]
			string Label { get; set; }

			[Blackboard]
			public event EventHandler Changed;

			void Junk()
			{
				if (value == 3)
				{
					if (Changed != null) Changed(this, EventArgs.Empty);
					value++;
				}
			}
		}

		[ClassInitialize]
		public static void Init(TestContext ctx)
		{
			context = ctx;
			consoleLog = new ConsoleLogger();
		}

		void AssertPrint(Blackboard.Item item)
		{
			Log.WriteLine("{0}",item.Owner);
			Assert.IsNotNull(item);
		}

		[TestMethod]
		public void PublishTest()
		{
			AssertPrint(Blackboard.Publish("One",one));

			var parent = new Model("Parent");
			parent.Path = "Parent";
			AssertPrint(Blackboard.Publish(parent));

			var child = new Model("Child");
			child.Parent = parent;
			AssertPrint(Blackboard.Publish(child));

			AssertPrint(Blackboard.Publish(child, "Two", new ObjectValueInfo(two)));

			var thing = new Thing("aThing");
			AssertPrint(Blackboard.Publish(thing));
		}
	}
}
