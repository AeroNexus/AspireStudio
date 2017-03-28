using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;

using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.CoreModels
{
	public class BrowserFactory : Model, IApplicationLite, IAsyncItemProvider
	{
		BrowserFactoryProtocol mBrowserFactoryProtocol;
		List<string> providedItems = new List<string>();
		//ProtocolMux mProtocolMux;
		Scenario mScenario;
		Thread mThread;
		Transport mTransport;

        readonly string transportName;
		public BrowserFactory()
		{
            this.transportName = Config.TransportName;
			CleanPublish = true;
			//ExecutionPeriod = new SecTime(1);
			Prefix = "__";
			Blackboard.RegisterAsyncItemProvider(this);
			AspireBrowser.CreateActions();
		}

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			mScenario = scenario;
			//mProtocolMux = new ProtocolMux(this);
			//mProtocolMux.DirectoryAdded += new EventHandler(mProtocolMux_DirectoryAdded);

			base.Discover(scenario);
			mThread = new Thread(new ThreadStart(Run)) { Name = Name, IsBackground = true };
			mThread.Start();
		}

		//void FindLocalManager(object state)
		//{
		//	mProtocolMux.FindLocalManager(false);
		//}

		//void mProtocolMux_DirectoryAdded(object sender, EventArgs e)
		//{
		//	OnDirectoryAdded(sender as CoreAddress);
		//}

		[XmlAttribute("name")]
		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				Prefix = Name + ".";
			}
		}

		public override void Unload()
		{
			IsClosing = true;
		}

		#endregion

		[DefaultValue(0)]
		[Description("Level of diagnostic printouts.")]
		[Category("Diagnostics")]
		[XmlAttribute("debug")]
		public virtual int DebugLevel
		{
			get { return mDebugLevel; }
			set
			{
				mDebugLevel = value;
				if (mBrowserFactoryProtocol != null)
					mBrowserFactoryProtocol.DebugLevel = value;
				Logger.LogLevel = mDebugLevel;
			}
		} int mDebugLevel;

		[DefaultValue(0)]
		[Description("Created Browser's level of diagnostic printouts.")]
		[Category("Diagnostics")]
		[XmlAttribute("debugBrowsers")]
		public virtual int DebugLevelBrowsers { get; set; }

		bool InitializeInternal()
		{
			int FixedPort = 0;
			mTransport = TransportFactory.Create(transportName, FixedPort);

			if (IoLoggingEnabled)
				mTransport = new LoggingTransportDecorator(mTransport, this);

			if (mTransport.Open() < 0)
			{
				Logger.Log(1, "Could not open transport. Exiting");
				Thread.CurrentThread.Abort();
				return false;
			}

			var xtedsString = new MarshaledString("BrowserFactory");
			var compUid = Uuid.NewUuid();
			mBrowserFactoryProtocol = new BrowserFactoryProtocol(this, mTransport, xtedsString, compUid);
			mBrowserFactoryProtocol.DebugLevel = DebugLevel;

			MessageHandler = new MessageHandler(this, mTransport);
			MessageHandler.AddProtocol(ProtocolId.Aspire, mBrowserFactoryProtocol);

			return true;
		}

		[Category("Diagnostics"),DefaultValue(false), XmlAttribute("ioLog")]
		[Description("Log incoming and outgoing packets.")]
		public bool IoLoggingEnabled { get; set; }

		[Category("Diagnostics"),DefaultValue(false), XmlAttribute("ioLogBrowsers")]
		[Description("Created Browser's log incoming and outgoing packets.")]
		public bool IoLogBrowsers { get; set; }

		public void OnDirectory(CoreAddress coreAddress, bool added)
		{
			if (added)
			{
				if (PnPBrowsers.ByDomain(coreAddress.DomainName) == null)
				{
					var browser = new AspireBrowser(Path + '.' + coreAddress.DomainName,DebugLevelBrowsers)
					{
						Name = coreAddress.DomainName,
						ComponentName = "Browser",
						DomainName = coreAddress.DomainName,
						IoLoggingEnabled = IoLogBrowsers
					};
					AddChild(browser, mScenario);
          PnPBrowsers.Add(browser,true);
				}
			}
			else
			{
				Write("Directory ({0}) removed", coreAddress.DomainName);
			}
		}

		public void OnLocalManager(CoreAddress coreAddress, bool up)
		{
			//Write("LocalManager {0}", up?"up":"down");
		}

		void Run()
		{
			Setup();

			MessageHandler.FieldMessages();

			Teardown();
		}

		void Setup()
		{
			InitializeInternal();

			mBrowserFactoryProtocol.Start();
		}

		void Teardown()
		{
			mBrowserFactoryProtocol.Stop();
		}

		#region IApplicationLite

		/// <summary>
		/// The execution period used to dispatch periodic processing
		/// </summary>
		[Category("Application"),XmlIgnore]
		public SecTime ExecutionPeriod { get; private set; }

		/// <summary>
		/// Is the application in the midst of closing
		/// </summary>
		[Browsable(false), XmlIgnore]
		public bool IsClosing { get; private set; }

		/// <summary>
		/// The MessageHandler
		/// </summary>
		[Category("Application"),XmlIgnore]
		public MessageHandler MessageHandler { get; private set; }

		/// <summary>
		/// The application name used in error messages
		/// </summary>
		//string Name { get; }
		/// <summary>
		/// The periodic processing method
		/// </summary>
		public void Perform() { }

		#endregion

		#region IAsyncItemProvider Members

		public bool Matches(string path)
		{
			return path.StartsWith(Prefix);
		}

		[Category("Application"),XmlIgnore]
		public string Prefix { get; private set; }

		public void Provide(string path)
		{
			providedItems.Add(path);
		}

		public void TransferItemsTo(IAsyncItemProvider provider)
		{
			List<string> transferred = new List<string>();
			foreach (var item in providedItems)
				if (provider.Matches(item))
				{
					provider.Provide(item);
					transferred.Add(item);
				}
			foreach (var item in transferred)
				providedItems.Remove(item);
			transferred.Clear();
		}

		public void Verify(string path)
		{
		}

		#endregion
	}
}
