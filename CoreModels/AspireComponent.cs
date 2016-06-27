using System;
//using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

using System.Windows.Forms;

using Aspire.Framework;
using Aspire.Utilities;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	/// <summary>
	/// The NameSource enum is used to specify the source of an <see cref="BrowserComponent"/>'s <see cref="BrowserComponent.Name"/> property.
	/// </summary>
	public enum NameSource { Address, ComponentName, Enumerated, Aliased, MissionInformationService }

	/// <summary>
	/// Summary description for AspireComponent.
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class AspireComponent : IPublishable, IHostXteds, INotifyPropertyChanged, IPropertyCategoryInitializer
	{
		int publishes;
		internal int instances=1;
		List<Xteds.Interface.DataMessage> subscribers = new List<Xteds.Interface.DataMessage>();
		public byte AppDev { set { appDev = value; } }
		byte appDev;

		internal bool xtedsInProgress;
		internal NameSource nameSource = NameSource.Address;

		public int placeHolder;
		public event EventHandler ComponentIdChanged;
		public event EventHandler XtedsAvailable;

		//public int KeyRequests{ get { return keyRequests; } }
		//internal int keyRequests;

		public AspireComponent() { }

		public AspireComponent(IPnPBrowser browser, Address address)
		{
			mAddress = address;
			mBrowser = browser;
			name = String.Format("{0}", address.Hash);
		}

		public void Publish()
		{
			publishes++;
			var item = Blackboard.Publish(this);
			if (xteds == null)
			{
				item.Expanded += new EventHandler(item_ItemExpanded);
				Blackboard.Publish(this, mBrowser.Path+"."+Name+".$", "placeHolder");
				//Log.WriteLine("{0}.Publish placeholder", this);
			}
			else
				PublishInterfaces();
		}

		public int NumPublishes { get { return publishes; } }

		public int NumOnComponentInfos { get; set; }
		Aspire.Framework.Timer compInfoTimer;

		void OnGetCompInfoTimeout()
		{
			compInfoTimer = null;
			Browser.ResendGetComponentInfo(this);
		}
		public bool CompInfoTimer
		{
			set
			{
				if (value)
					compInfoTimer = Aspire.Framework.Timer.SetWallTimer(TimerOccurance.Periodic, 5, new Aspire.Framework.Timer.Callback(OnGetCompInfoTimeout));
				else
				{
					if ( compInfoTimer != null )
						compInfoTimer.Cancel();
					compInfoTimer = null;
				}
			}
		}

		void item_ItemExpanded(object sender, EventArgs e)
		{
			try
			{
				var item = sender as Blackboard.Item;

				GetXteds();
				if ( item != null )
					item.Expanded -= new EventHandler(item_ItemExpanded);
			}
			catch (Exception ex)
			{
				Log.ReportException(ex, "AspireComponent.Expand data item:");
			}
		}

		private void GetXteds()
		{
			if (xtedsInProgress) return;
			if (xtedsText.Length == 0)
			{
				xtedsInProgress = true;
				mBrowser.Cache.RequestXteds(this, mBrowser);
			}
			else if (xteds == null)
			{
				xteds = Xteds.Parse(xtedsText, "", this);
				if (xteds != null)
				{
					publishes++;
					PublishInterfaces();
					Blackboard.Unpublish(mBrowser.Path + "." + Name + ".$");
				}
			}
		}

		/// <summary>
		/// This is used by the Browser to avoid getting a new xTEDS
		/// </summary>
		/// <param name="value"></param>
		internal void SetCached(bool value)
		{
			cached = value;
			mBrowser.Cache.SetEnable(XtedsUid, cached);
		}

		public override string ToString() { return name+".AspireComponent"; }

		public void Unpublish()
		{
			if (xteds == null)
			{
				var item = Blackboard.Unpublish(mBrowser.Path+"."+Name+".$");
				item.Expanded -= new EventHandler(item_ItemExpanded);
			}
			else
				Xteds.UnpublishInterfaces(mBrowser.Path);
			Blackboard.Unpublish(mBrowser.Path+"."+Name);
		}

		#region Properties

		[Category("Connectivity")]
		[XmlIgnore]
		public bool Active
		{
			get { return active; }
			set
			{
				var di = Blackboard.GetExistingItem(mBrowser.Path+"."+Name);
				var props = new BlackboardDisplayProperties();
				active = value;
				if (value)
				{
					props.FontItalic = false;
					props.ForeColor = System.Drawing.Color.Black;
					foreach (var msg in subscribers)
						if (msg.Subscribed > 0)
						{
							msg.Subscribed = 0;
							msg.Subscribe();
						}
				}
				else
				{
					props.FontItalic = true;
					props.ForeColor = System.Drawing.Color.Gray;
				}
				if (di != null)
					di.DisplayProperties = props;

			}
		} bool active = true;

		[Category("Connectivity")]
		public Address Address
		{
			get { return mAddress; }
			set
			{
				value.CopyTo(mAddress);
				RaisePropertyChanged("ComponentId");
				if (ComponentIdChanged != null)
					ComponentIdChanged(this, EventArgs.Empty);
			}
		} Address mAddress;

		[XmlAttribute("address"),Browsable(false)]
		public string xmlAddress
		{
			get { return mAddress.ToString(); }
			set { mAddress = AspireBrowser.The.Address.Clone(); mAddress.Parse(value); }
		}

		[Category("Xteds")]
		public bool Cached
		{
			get
			{
				mBrowser.Cache.Enabled(xtedsUid, ref cached);
				return cached;
			}
			set
			{
				cached = value;
				mBrowser.Cache.SetEnable(XtedsUid, cached);
				if (!cached) // get a new xTEDS from Directory
				{
					xteds.UnpublishInterfaces(mBrowser.Path);
					xtedsText = string.Empty;
					xteds = null;
					xtedsInProgress = true;
					mBrowser.Cache.RequestXteds(this, mBrowser);
				}
			}
		} bool cached;

		[Category("Component")]
		public int Debug
		{
			get { return debugLevel; }
			set
			{
				debugLevel = value;
				bool unmarshal = debugLevel > 1;
				if (xteds == null || xteds.Interfaces == null) return;
				foreach (var iface in xteds.Interfaces)
					iface.UnmarshalOnMonitor = unmarshal;
			}
		} int debugLevel;

		[Category("Connectivity")]
		public string Domain
		{
			get { return mDomainName; }
			set { mDomainName = value; }
		} string mDomainName;

		[Browsable(false)]
		public int PublishableByteArrayLength { get { return mBrowser.PublishableByteArrayLength; } }

		void IHostXteds.Publish(IDataMessage dataMsg){}

		[Category("Component")]
		public string Kind
		{
			get { return kind; }
			set
			{
				kind = value;
				RaisePropertyChanged("Kind");
			}
		} string kind = string.Empty;

		[Category("Connectivity"),XmlAttribute("compId")]
		public string CompUidString
		{
			get
			{
				if (compUidIsValid)
					return compUid.ToString();
				else
					return string.Empty;
			}
			set
			{
				compUid = new Uuid(value);
				compUidIsValid = true;
				RaisePropertyChanged("CompUid");
			}
		}

		[Category("Connectivity")]
		public Uuid CompUid
		{
			get { return compUid; }
			set
			{
				compUid = new Uuid(value);
				compUidIsValid = true;
				RaisePropertyChanged("CompUid");
			}
		}
		Uuid compUid;
		bool compUidIsValid;

		[Browsable(false),XmlIgnore]
		public bool NeedsAddressTranslation { get; set; }

		[Category("Component")]
		[XmlIgnore]
		public object Tag
		{
			get { return tag; }
			set { tag = value; }
		} object tag;

		[Category("Component")]
		[XmlIgnore]
		public string Type
		{
			get
			{
				if (xteds != null)
				{
					if (xteds.Application == null)
					{
						if (xteds.Device == null)
							return "Component";
						else
							return "Device";
					}
					else
						return "Application";
				}
				else
				{
					if (appDev == (byte)'A')
						return "Application";
					else if (appDev == (byte)'D')
						return "Device";
					else
						return "Component";
				}
			}
		}

		[Category("Xteds")]
		public Xteds Xteds
		{
			get { return xteds; }
			set
			{
				xteds = value;
				if (XtedsAvailable != null)
					XtedsAvailable(this, EventArgs.Empty);
			}
		} Xteds xteds;

		[Category("Xteds")]
		public string XtedsText
		{
			get { return xtedsText; }
			set
			{
				//Log.WriteLine("{0}.XtedsText= xip {1}, {2}", this, xtedsInProgress, publishes);
				xtedsText = value;
				xteds = Xteds.Parse(value, mBrowser.Cache.Directory+name+".xteds", this);
				if (XtedsAvailable != null)
					XtedsAvailable(this, EventArgs.Empty);
				if (xteds != null)
				{
					if (publishes == 0)
						Publish();
					else
					{
						if (xtedsInProgress)
						{
							publishes++;
							PublishInterfaces();
							Blackboard.Unpublish(mBrowser.Path + "." + Name + ".$");
							//Log.WriteLine("{0}.un-publish placeholder", this);
						}
					}
				}
			}
		} string xtedsText = string.Empty;

		void PublishInterfaces()
		{
			if (Blackboard.ViewHasPopulated)
				xteds.PublishInterfaces(mBrowser.Path);
			else
				Blackboard.ViewPopulated += Blackboard_ViewPopulated;
		}

		void Blackboard_ViewPopulated(object sender, EventArgs e)
		{
			if (publishes == 0)
				Publish();
			else
				PublishInterfaces();
			Blackboard.ViewPopulated -= Blackboard_ViewPopulated;
		}

		[Category("Xteds")]
		public string XtedsUidString
		{
			get
			{
				if (xtedsUidIsValid)
					return xtedsUid.ToString();
				else
					return string.Empty;
			}
			set
			{
				xtedsUid = new Uuid(value);
				xtedsUidIsValid = true;
				RaisePropertyChanged("XtedsUid");
			}
		}

		[Category("Xteds")]
		public Uuid XtedsUid
		{
			get { return xtedsUid; }
			set
			{
				xtedsUid = new Uuid(value);
				xtedsUidIsValid = true;
				RaisePropertyChanged("XtedsUid");
			}
		}
		Uuid xtedsUid;
		bool xtedsUidIsValid;

		[Category("Component")]
		[XmlIgnore, Browsable(false)]
		public BrowserComponentState State
		{
			get { return state; }
			set { state = value; }
		} BrowserComponentState state;

		#endregion

		#region IPublisher Members

		[Category("Component"),XmlAttribute("name")]
		public string Name
		{
			get { return name; }
			set
			{
				bool republish = false;
				if (name.Length > 0)
				{
					Unpublish();
					republish = true;
				}
				name = value;
				if (Address == null || Address.Hash == 0) return;
				if (xtedsName.Length == 0)
					xtedsName = name;
				if (republish)
					Publish();
			}
		} string name = string.Empty;

		[XmlIgnore, Browsable(false)]
		public IPublishable Parent
		{
			get { return mBrowser; }
			set { } 
		} IPnPBrowser mBrowser;

		public string Path { get; set; }

		[Browsable(false),XmlIgnore]
		public IPnPBrowser Browser
		{
			get { return mBrowser; }
			internal set { mBrowser = value; }
		}

		[Category("Xteds"),XmlAttribute("xteds")]
		public string XtedsName
		{
			get { return xtedsName; }
			set { xtedsName = value; }
		}
		string xtedsName = string.Empty;

		//internal bool reconnecting;

		#endregion

		#region IHostXteds Members

		[Category("Component")]
		[XmlIgnore]
		public IAdapterState AdapterState { get { return null; } }

		[XmlIgnore, Browsable(false)]
		public bool CanParse
		{
			get
			{
				return false;
			}
		}

		[Category("Component")]
		[XmlIgnore]
		public HostType HostType { get { return HostType.Consumer; } }

		[Category("Component")]
		[XmlIgnore]
		public int AllowableLeasePeriod { get { return 0; } }

		public void HookNotification(XtedsMessage msg)
		{
			(msg as Xteds.Interface.Message).WhenMessageArrives += msg_Notification;
		}

		public void NeedSubscription(IDataMessage dataMessage)
		{
			if ( pendingSubscriptions == null )
				pendingSubscriptions = new List<IDataMessage>();
			if (!pendingSubscriptions.Contains(dataMessage))
				pendingSubscriptions.Add(dataMessage);
		} List<IDataMessage> pendingSubscriptions;

		internal bool HasPendingSubscriptions { get { return pendingSubscriptions != null; } }

		internal void SatisfyPendingSubscriptions()
		{
			foreach (var dataMessage in pendingSubscriptions)
				Subscribe(dataMessage as Xteds.Interface.DataMessage);
		}

		void msg_Notification(XtedsMessage msg)
		{
			var sb = new StringBuilder();
			var action = (msg as Xteds.Interface.Message).ArrivalNotification;
			switch (action)
			{
				case "console":
				case "messageBox":
					sb.AppendFormat("{0}:", msg);
					int i=0;
					foreach (var v in msg.IVariables)
						sb.AppendFormat("{0}{1}:{2}", ++i == 1 ? " " : ", ", v.Name,v.Value);

					if (action == "console")
						Log.WriteLine(sb.ToString());
					else
						MessageBox.Show(sb.ToString());
					break;
			}
		}

		public VariableMarshaler KnownMarshaler(IVariable variable)
		{
			return null;
		}

		public void NotifyCancelled(Xteds.Interface.DataMessage msg)
		{
		}

		public void ParseAspireMessage(byte[] buf, int length)
		{
		}

		[Category("Component")]
		public string ProviderName { get { return name; } }

		/// <summary>
		/// Send a Aspire message
		/// </summary>
		public void Send(Xteds.Interface.Message msg)
		{
			int n = mBrowser.Send(msg, mAddress);
			if (debugLevel > 1)
				Log.WriteLine("Sent {0}: {1} bytes", msg.Name, n);
		}

		void AspireMessageLogged(byte[] buffer, int length)
		{
			string hex = BitConverter.ToString(buffer, 0, length);
			Log.WriteLine(hex);
		}

		/// <summary>
		/// Subscribe to a Aspire message
		/// </summary>
		public void Subscribe(Xteds.Interface.DataMessage dataMsg)
		{
			if (!subscribers.Contains(dataMsg)) subscribers.Add(dataMsg);
			mBrowser.Subscribe(dataMsg, mAddress);
		}

		[XmlIgnore, Browsable(false)]
		public bool Testable { get { return false; } }

		public void Unmarshal(byte[] buf, int length, Xteds.Interface.Message msg)
		{
			if (debugLevel > 1)
				Log.WriteLine("Msg {0}, transferred {1} bytes", msg.Name, length);
		}

		/// <summary>
		/// Unsubscribe to a Aspire message
		/// </summary>
		public void Unsubscribe(Xteds.Interface.DataMessage dataMsg)
		{
			if (subscribers.Contains(dataMsg)) subscribers.Remove(dataMsg);
			mBrowser.CancelSubscription(dataMsg, mAddress);
		}

		public XtedsVariable XtedsVariable(string name)
		{
			return null;
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		void RaisePropertyChanged(string info)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(info));
		}

		#endregion

		#region IPropertyCategoryInitializer Members

		/// <summary>
		/// Even though we are not manipulating categories here, we use this to detect that our properties are being browsed.
		/// If so, we request the xTEDS for this component.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, bool> GetCategoryStates()
		{
			GetXteds();
			return new Dictionary<string, bool>();
		}

		#endregion
	}

	public enum BrowserComponentState { Empty, Opened, Registered }

}
