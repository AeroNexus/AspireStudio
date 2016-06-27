using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;

using Aspire.Core.Messaging;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	public class AspireShell: Application, IHostXteds, INotifyPropertyChanged, IUseXteds
	{
		const string shell = "Shell";
		static BlackboardDisplayProperties nodeDisplayShell;
		static AspireShell()
		{
			AspireBrowser.CreateActions();
			nodeDisplayShell = new BlackboardDisplayProperties() { Image = Properties.Resources.shell };
		}

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			Enabled = false;


			if ( Blackboard.ViewHasPopulated )
				Publish();
			else
				Blackboard.ViewPopulated += Blackboard_ViewPopulated;

			InhibitPublish = true;
			base.Discover(scenario);

			//if (Parent is IHostComponent2)
			//	(Parent as IHostComponent2).DiscoverHostedComponent();

			//Bind();
		}

		[Category("Model"), XmlAttribute("name"), DefaultValue("")]
		public override string Name
		{
			get
			{
				if (IsSaving && base.Name.StartsWith(Xteds.Component.Name + " shell")) return string.Empty;
				return base.Name;
			}
			set
			{
				bool overriding = base.Name != null;
				if (overriding)
				{
					Unpublish();
					base.Name = value;
					Publish();
				}
				else
					base.Name = value;
			}
		}
		public override void Unload()
		{
			if (Transport != null) Transport.Close();
			base.Unload();
		}

		void Blackboard_ViewPopulated(object sender, EventArgs e)
		{
			if (publishes == 0)
				Publish();
			Blackboard.ViewPopulated -= Blackboard_ViewPopulated;
		}

		List<PeriodicMessage> periodicMessages = new List<PeriodicMessage>();
		PeriodicMessage[] periodicMsgs;
		[Category(shell),XmlIgnore]
		public PeriodicMessage[] PeriodicMessages { get { return periodicMsgs; } }

		//public override void Execute()
		//{
		//}

		#endregion

		bool hasRegistered;

		protected override void OnControlProtocol(Core.ControlProtocol controlProtocol)
		{
			if ( Standalone ) EnablePeriodicMessages = true;

		}
		public override void OnRegistrarChanged()
		{
			hasRegistered = true;
			EnablePeriodicMessages = true;
		}

		void Bind()
		{
			foreach (var pMsg in periodicMessages)
				pMsg.Dispose();
			periodicMessages.Clear();

			if (xtedsVariableMap != null && Xteds != null)
				xtedsVariableMap.Bind(this);

			foreach (var iface in Xteds.Interfaces)
			{
				if ( iface.commandMessageList != null )
					foreach (var msg in iface.CommandMessages)
						if (msg.ReplyMessage != null)
							msg.WhenMessageArrives +=new XtedsMessage.Handler(msg_WhenMessageArrives);
				foreach (var msg in iface.DataMessages)
				{
					var dataMsg = msg as Xteds.Interface.DataMessage;
					if (dataMsg.MsgArrival == Xteds.Interface.Message.Arrival.PERIODIC)
						periodicMessages.Add(new PeriodicMessage(dataMsg, this));
					foreach (var vv in dataMsg.Variables)
						if (vv.Variable.Name == "Time" || vv.Variable.Name == "SubS")
						{
							dataMsg.Application = this;
							dataMsg.MapVariable(vv.Variable.Name);
						}
				}
				foreach (var msg in iface.ReplyMessages)
				{
					var dataReplyMsg = msg as Xteds.Interface.DataReplyMessage;
					foreach (var vv in dataReplyMsg.Variables)
						if (vv.Variable.Name == "Time" || vv.Variable.Name == "SubS")
						{
							dataReplyMsg.Application = this;
							dataReplyMsg.MapVariable(vv.Variable.Name);
						}
				}
				periodicMsgs = periodicMessages.ToArray();
			}
		}

		void msg_WhenMessageArrives(XtedsMessage request)
		{
			ReplyTo(request, 0);
		}

		[Category(shell),XmlIgnore]
		public int Debug
		{
			get { return debugLevel; }
			set
			{
				debugLevel = value;
				bool unmarshal = debugLevel > 1;
				if (Xteds == null || Xteds.Interfaces == null) return;
				foreach (var iface in Xteds.Interfaces)
					iface.UnmarshalOnMonitor = unmarshal;
			}
		} int debugLevel;

		[Category("Connectivity")]
		[XmlAttribute("domain"), DefaultValue("Default"), Instanced]
		public override string DomainName
		{
			get
			{
				return base.DomainName;
			}
			set
			{
				base.DomainName = value;
				if (hasRegistered)
				{
					EnablePeriodicMessages = false;
				}
			}
		}

		[XmlAttribute("dontRegister"),Browsable(false),DefaultValue(false)]
		public bool xmlDontRegister
		{
			get { return base.DontRegister; }
			set { base.DontRegister = value; }
		}

		bool EnablePeriodicMessages
		{
			set
			{
				foreach (var pMsg in periodicMessages)
					pMsg.Enabled = value;
			}
		}

		void item_ItemExpanded(object sender, EventArgs e)
		{
			try
			{
				var item = sender as Blackboard.Item;

				PublishFirst();
				item.Expanded -= new EventHandler(item_ItemExpanded);
			}
			catch (Exception ex)
			{
				Log.ReportException(ex, "AspireShell.Expand data item:");
			}
		}

		int publishes;

		[Category(shell)]
		public int NumPublishes { get { return publishes; } }

		protected override void OnXtedsIdValid()
		{
			if ( CompUid.IsEmpty )
				CompUid = XtedsUid;
		}

		[XmlIgnore]
		public int placeHolder;

		public virtual void Publish()
		{
			publishes++;
			var item = Blackboard.Publish(this);
			item.DisplayProperties = nodeDisplayShell;
			string path = string.Empty;
			if (Parent != null) path = Parent.Path;
			if (Xteds == null)
			{
				item.Expanded += new EventHandler(item_ItemExpanded);
				Blackboard.Publish(this, path+'.'+Name+". ", "placeHolder");
			}
			else
				Xteds.PublishInterfaces(path);
		}

		void PublishFirst()
		{
			if (Xteds != null)
			{
				publishes++;
				string path = string.Empty;
				if (Parent != null) path = Parent.Path;
				Xteds.PublishInterfaces(path+'.'+Name);
				Blackboard.Unpublish(path);
			}
		}

		public void Unpublish()
		{
			string path = string.Empty;
			if (Parent != null)
				path = Parent.Path;
			else
				path = Path;
			if (Xteds == null)
				Blackboard.Unpublish(path+'.'+Name+". ");
			else
				Xteds.UnpublishInterfaces(path);
			Blackboard.Unpublish(path);
		}

		bool Unique(string name)
		{
			return ModelMgr.Model(name) == null;
		}

		[Category("Application"), XmlAttribute("xtedsFile")]
		[EditorAttribute(typeof(XtedsFileBrowser), typeof(System.Drawing.Design.UITypeEditor))]
		public override string XtedsFile
		{
			get
			{
				return base.XtedsFile;
			}
			set
			{
				EnablePeriodicMessages = false;

				if (System.IO.Path.IsPathRooted(value))
				{
					base.XtedsDirectory = FileUtilities.ScenarioRelativePath(System.IO.Path.GetDirectoryName(value));
					base.XtedsFile = System.IO.Path.GetFileName(value);
				}
				else
					base.XtedsFile = value;

				string suffix = string.Empty;
				int count = 0;
				if (Xteds != null)
				{
					if ( Name == null )
						do {
							Name = Xteds.Component.Name + " shell" + suffix;
							suffix = (++count).ToString();
						} while (ModelMgr.Model(Name) != null);
					FileName = '.' + Name + ".xml";

					Bind();
				}
				ChangeProperties("XtedsFile");
			}
		}

		[Category(shell),Description("xTEDS to SDT variable map")]
		public XtedsVariableMap XtedsVarMap
		{
			get { return xtedsVariableMap; }
			set { xtedsVariableMap = value; }
		} XtedsVariableMap xtedsVariableMap;

		#region IHostXteds Members

		[Browsable(false),XmlIgnore]
		public IAdapterState AdapterState { get { return null; } }

		[XmlIgnore, Browsable(false)]
		public bool CanParse { get { return true; } }

		public void HookNotification(XtedsMessage xmsg)
		{
		}

		public void NeedSubscription(IDataMessage dataMessage)
		{
		}

		HostType IHostXtedsLite.HostType { get { return HostType.Provider; } }

		int IHostXtedsLite.AllowableLeasePeriod { get { return 0; } }

		public VariableMarshaler KnownMarshaler(IVariable variable)
		{
			return AspireClock.The.KnownMarshaler(variable);
		}

		//public void NotifyCancelled(Xteds.Interface.DataMessage msg){}

		public void ParseAspireMessage(byte[] buf, int length) { }

		[Category(shell),Browsable(false)]
		public string ProviderName { get { return Name; } }

		/// <summary>
		/// Send a Aspire message
		/// </summary>
		public void Send(Xteds.Interface.Message msg)
		{
			if (msg is Xteds.Interface.DataReplyMessage)
				ReplyTo((msg as Xteds.Interface.DataReplyMessage).RequestMessage, 0);
			else
				Send(msg as XtedsMessage);
		}

		void AspireMessageLogged(byte[] buffer, int length)
		{
			string hex = BitConverter.ToString(buffer, 0, length);
			Log.WriteLine(hex);
		}

		[Category(shell),Browsable(false)]
		public int PublishableByteArrayLength { get { return 16; } }

		/// <summary>
		/// Subscribe to a Aspire message
		/// </summary>
		public void Subscribe(Xteds.Interface.DataMessage dataMsg) { }

		[XmlIgnore, Browsable(false)]
		public bool Testable { get { return false; } }

		public void Unmarshal(byte[] buf, int length, Xteds.Interface.Message msg)
		{
			MessageHelper.Unmarshal(buf, length, msg);
			if (debugLevel > 1)
				Log.WriteLine("Msg {0}, transferred {1} bytes", msg.Name, length);
		}

		/// <summary>
		/// Unsubscribe to a Aspire message
		/// </summary>
		public void Unsubscribe(Xteds.Interface.DataMessage dataMsg) { }

		public XtedsVariable XtedsVariable(string name)
		{
			return xtedsVariableMap == null ? null : xtedsVariableMap[name];
		}

		#endregion

		#region INotifyPropertyChanged Members

		//public event PropertyChangedEventHandler PropertyChanged; inherited

		#endregion

		#region IUseXteds Members

		public void ChangeProperties(string propertyName)
		{
			OnPropertyChanged(propertyName);
		}

		public Blackboard.Item BlackboardSubscribe(string itemName){ return Blackboard.Subscribe(itemName); }

		#endregion

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class PeriodicMessage : IDisposable
		{
			Xteds.Interface.DataMessage dataMsg;
			public Xteds.Interface.DataMessage Message { get { return dataMsg; } }

			Application app;
			Aspire.Framework.Timer timer;

			internal PeriodicMessage(Xteds.Interface.DataMessage msg, Application app)
			{
				dataMsg = msg;
				this.app = app;
			}

			public bool Enabled
			{
				get { return timer != null; }
				set
				{
					if (!value)
					{
						timer.Cancel();
						timer = null;
					}
					else
						timer = Aspire.Framework.Timer.SetSimulationTimer(TimerOccurance.Periodic,
							1.0/dataMsg.MsgRate, new Aspire.Framework.Timer.Callback(OnPeriodicPublish));
				}
			}

			void OnPeriodicPublish()
			{
				app.Publish(dataMsg);
			}

			#region IDisposable Members

			public void Dispose()
			{
				timer.Cancel();
			}

			public override string ToString()
			{
				return dataMsg.Name;
			}
			#endregion
		}
	}

	/// <summary>
	/// XtedsFileBrowser is a ui type editor for initialization files.
	/// </summary>
	internal class XtedsFileBrowser : System.Windows.Forms.Design.FileNameEditor
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public XtedsFileBrowser()
		{
		}

		/// <summary>
		/// <see cref="System.Windows.Forms.Design.FileNameEditor.InitializeDialog"/>
		/// </summary>
		/// <param name="openFileDialog"></param>
		protected override void InitializeDialog(System.Windows.Forms.OpenFileDialog openFileDialog)
		{
			base.InitializeDialog(openFileDialog);
			var dlg = openFileDialog;
			dlg.InitialDirectory = Application.CommonXtedsDirectory;
			dlg.CheckFileExists = true;		// file doesn't have to exist
			dlg.Filter = "xTEDS Files (*.xteds)|*.xteds|All Files (*.*)|*.*";
			dlg.DefaultExt = ".xteds";
			dlg.AddExtension = true;
			dlg.FileName = string.Empty;
			dlg.Title = "Select xTEDS File";
			dlg.FileOk += new CancelEventHandler(dlg_FileOk);
		}

		void dlg_FileOk(object sender, CancelEventArgs e)
		{
			var dlg = sender as System.Windows.Forms.OpenFileDialog;
		}
	}

	/// <summary>
	/// Summary description for AspireShell BlackboardAction.
	/// </summary>
	public class AspireShellAction : BlackboardAction
	{
		public AspireShellAction() : base("Aspire Shell") { }

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			enabled = item.Value is AspireBrowser || item.Value is BrowserFactory;
			return enabled;
		}

		public override void Execute(Blackboard.Item item)
		{
			var browser = item.Value as AspireBrowser;
			string domainName = "Default";
			if (browser != null) domainName = browser.DomainName;
			var ui = new AspireShellUI(domainName);
			if (ui.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				var shell = new AspireShell();
				shell.DomainName = ui.Domain;
				var uri = new Uri(System.IO.Path.Combine(Scenario.Directory, Application.CommonXtedsDirectory));
				if (!ui.XtedsPath.StartsWith(uri.LocalPath))
					shell.XtedsFile = ui.XtedsPath;
				else
					shell.XtedsFile = ui.XtedsFile;
				ModelMgr.Add(shell);
				shell.Save();
			}
		}
	}

}
