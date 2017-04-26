using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;

using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	public class AspireBrowser : Application, IPnPBrowser, IAsyncItemProvider
	{
		internal static AspireBrowser The;
		const string components = "Components";
		const string connectivity = "Connectivity";
		const string diagnostics = "Diagnostics";

		static AspireBrowser()
		{
			CreateActions();
		}

		internal static void CreateActions()
		{
			if (registered) return;
			registered = true;
			new AspireShellAction();
			//new ManifestAction();
			new MessageHelper();
			new PendingSubscriptionsAction();
		} static bool registered;

		MarshaledBuffer mGetInfoAddressBuf = new MarshaledBuffer();
		MarshaledBuffer mInfoAddressBuf = new MarshaledBuffer();
		MarshaledBuffer mAddressBuf = new MarshaledBuffer();
		MarshaledBuffer mXtedsIdBuf = new MarshaledBuffer();
		MarshaledBuffer mCompInfoIdBuf = new MarshaledBuffer();
		MarshaledBuffer mCompIdBuf = new MarshaledBuffer();
		MarshaledBuffer mRequestXtedsBuf = new MarshaledBuffer();
		MarshaledString mRequestXtedsStr = new MarshaledString();
		Address mInfoAddress, mAddress;
		List<AspireComponent> mComponents = new List<AspireComponent>();
		Dictionary<uint, AspireComponent> mComponentByAddress = new Dictionary<uint, AspireComponent>();
		Dictionary<string, AspireComponent> mComponentByName = new Dictionary<string, AspireComponent>();
		Dictionary<Uuid, AspireComponent> mComponentByCompId = new Dictionary<Uuid, AspireComponent>();
		MarshaledString mCompNameStr = new MarshaledString(), mCompKindStr = new MarshaledString();
		XtedsCache mXtedsCache;
		XtedsMessage mGetAddressWithRespectTo, mGetComponentInfo, mRequestXteds;
		byte mAppDev = (byte)'C';

		public enum CompInfoStatus { uninitialized, Valid=1, BadAddress, UnknownAddress, UnknownXteds, UnknownName, UnknownKind }
		CompInfoStatus mCompInfoStatus = CompInfoStatus.Valid;

		[Category(connectivity),XmlAttribute("browsableDomains")]
		public string BrowsableDomains{get;set;}
		[Category(connectivity),XmlAttribute("restrictedDomains")]
		public string RestrictedDomains { get; set; }
		[Browsable(false),XmlAttribute("dontRegister")]
		public bool DontRegisterAttribute
		{
			get { return DontRegister; }
			set { DontRegister = value; }
		}

		Dictionary<string, List<string>> subscriptionRequests = new Dictionary<string, List<string>>();

		public AspireBrowser() : this(null,0)
		{
		}

		public AspireBrowser(string parentPath, int debugLevel)
			: base(null)
		{
			mXtedsCache = XtedsCache.The;
			mDebugLevel = debugLevel;

			if (parentPath != null)
				Path = parentPath;
			Prefix = Path+'.';
			Blackboard.RegisterAsyncItemProvider(this); // Move to Discover after base.Discover
			Enabled = false;
			The = this;
		}

		internal Address Address { get { return mAddress; } }

		protected override void OnControlProtocol(ControlProtocol controlProtocol)
		{
			base.OnControlProtocol(controlProtocol);
			mInfoAddress = OwnAddress.Clone();
			mAddress = mInfoAddress.Clone();
			mGetInfoAddressBuf.Allocate = mInfoAddress.Length;
		}

		protected override void OnDirectoryAdded(CoreAddress coreAddress)
		{
			if (!DontRegister) return;
			if (ControlProtocol != null)
			{
				if (BrowsableDomains != null && BrowsableDomains.Contains(coreAddress.DomainName))
					QueryDirectory(coreAddress.DomainName);
				else if (RestrictedDomains != null && !RestrictedDomains.Contains(coreAddress.DomainName))
					QueryDirectory(coreAddress.DomainName);
			}
		}

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			if (Transport != null)
			{
				Transport.Open();
			}

			PnPBrowsers.Add(this,DontRegister);

			foreach (var comp in Components)
				AddInitialComponent(comp);

			base.Discover(scenario);
		}

		void MessageHandler_HandleUnknownProtocol(object sender, EventArgs e)
		{
			var buffer = sender as byte[];
			if (buffer[0] == (byte)'&')
			{
				Log.WriteLine("Client: {0}", ASCIIEncoding.ASCII.GetString(buffer, 1, buffer.Length - 1));
			}
		}

		public override void Unload()
		{
			if (Transport != null) Transport.Close();
			base.Unload();
		}

		void OnUnknownComponent(Message message, MarshaledBuffer sbuf)
		{
			AspireComponent comp;
			if ( mComponentByAddress.TryGetValue(message.Source.Hash,out comp) && comp.Xteds != null)
			{
				XtedsMessage xMsg = comp.Xteds.FindMessage(message.Selector);
				if (xMsg != null)
				{
					if (comp.Debug >= 2)
					{
						string text = BitConverter.ToString(sbuf.Bytes, sbuf.Offset, sbuf.Length);
						Log.WriteLine("{0}.{1}: {2}",
							comp.Name, xMsg.Name, text);
					}
					if (message.Sequence  == 255)
						xMsg.LeaseHasExpired();
					else
						xMsg.Unmarshal(sbuf, message);
				}
			}
		}

		#endregion

		#region Messaging

		enum QueryId { GetAddressWithRespectTo=1, GetComponentInfo, ModifyComponent, RegisterComponent,
			RequestXteds, UnregisterComponent, AdHocQuery };

    public int AdHocQueryId { get { return (int)QueryId.AdHocQuery; } }

		protected override void Initialize(bool dummy)
		{
			MessageHandler.HandleUnknownProtocol += MessageHandler_HandleUnknownProtocol;
			XtedsProtocol.WhenUnknownComponent(new XtedsProtocol.UnknownComponentHandler(OnUnknownComponent));

			if (Standalone) return;

			Query().ForExistingMessage((int)QueryId.RegisterComponent,
				"Interface name=IDirectory,Notification(DataMsg name=RegisterComponent)");
			Query().ForExistingMessage((int)QueryId.ModifyComponent,
				"Interface name=IDirectory,Notification(DataMsg name=ModifyComponent)");
			Query().ForExistingMessage((int)QueryId.UnregisterComponent,
				"Interface name=IDirectory,Notification(DataMsg name=UnregisterComponent)");
			Query().ForExistingMessage((int)QueryId.GetComponentInfo,
				"Interface name=IDirectory,Request(CommandMsg name=GetComponentInfo)");
			Query().ForExistingMessage((int)QueryId.RequestXteds,
				"Interface name=IDirectory,Request(CommandMsg name=RequestXteds)");
			Query().ForExistingMessage((int)QueryId.GetAddressWithRespectTo,
				"Interface name=IDirectory,Request(CommandMsg name=GetAddressWithRespectTo)");
		}

		bool mAllowQuery;
		public bool AllowQuery { get { return mAllowQuery; } set { mAllowQuery = value; } }

		void QueryDirectory(string domainName)
		{
			if (!AllowQuery) return;
			Query(DomainScope.Specifically, domainName).ForExistingMessage((int)QueryId.RegisterComponent,
				"Interface name=IDirectory,Notification(DataMsg name=RegisterComponent)");
			Query(DomainScope.Specifically, domainName).ForExistingMessage((int)QueryId.ModifyComponent,
				"Interface name=IDirectory,Notification(DataMsg name=ModifyComponent)");
			Query(DomainScope.Specifically, domainName).ForExistingMessage((int)QueryId.UnregisterComponent,
				"Interface name=IDirectory,Notification(DataMsg name=UnregisterComponent)");
			Query(DomainScope.Specifically, domainName).ForExistingMessage((int)QueryId.GetComponentInfo,
				"Interface name=IDirectory,Request(CommandMsg name=GetComponentInfo)");
			Query(DomainScope.Specifically, domainName).ForExistingMessage((int)QueryId.RequestXteds,
				"Interface name=IDirectory,Request(CommandMsg name=RequestXteds)");
		}

		void AddInitialComponent(AspireComponent comp)
		{
			comp.Browser = this;
			comp.AppDev = (byte)'A';
			comp.Kind = "Initial";
			comp.Domain = DomainName;
			mComponentByName[comp.Name] = comp;
			mComponentByAddress[comp.Address.Hash] = comp;

			CacheEntry ce;
			var text = mXtedsCache.Find(System.IO.Path.GetFileNameWithoutExtension(comp.XtedsName), out ce);
			if (text != null)
			{
				comp.XtedsUid = ce.uid;
				comp.XtedsText = text;
			}
			else
				Log.WriteLine("Can't find {0} in the XtedsCache", comp.Name);

			if ( !comp.CompUid.IsEmpty )
				mComponentByCompId[comp.CompUid] = comp;
		}

		AspireComponent AddComponent(Address address,Uuid compId,XtedsMessage msg)
		{
			var comp = new AspireComponent(this, address.Clone()) { CompUid = compId };
			var registrar = AddressServer.FindCoreAddress(msg.Header.Source);
			if ( registrar != null )
				comp.Domain = registrar.DomainName;
			comp.Publish();
			mComponentByCompId[compId] = comp;
			lock (mComponentByAddress)
				mComponentByAddress[address.Hash] = comp;
			mComponents.Add(comp);
			comp.NeedsAddressTranslation = !msg.Header.Source.IsLocal && (comp.Address.IsLoopback || !comp.Address.IsLocal);

			System.Threading.Thread.Sleep(10);
			comp.CompInfoTimer = true;
			address.Marshal(mGetInfoAddressBuf);
			Send(mGetComponentInfo);
			return comp;
		}

		public void ResendGetComponentInfo(AspireComponent comp)
		{
			Logger.Log(2,"Re-sending GetComponentInfo for {0}",comp.Address.ToString());
			comp.Address.Marshal(mGetInfoAddressBuf);
			Send(mGetComponentInfo);
		}

		void ModifyComponent(AspireComponent comp, Address address)
		{
			lock (mComponentByAddress)
				mComponentByAddress.Remove(address.Hash);
			//Need to go thru QueuedMessages and fixup addresses,
			//Might be able to utilize ProviderChanged for subscriptions
			address.CopyTo(comp.Address);
			lock (mComponentByAddress)
				mComponentByAddress[address.Hash] = comp;
		}

		void OnComponentRegistered(XtedsMessage msg)
		{
			Uuid compId = new Uuid(mCompIdBuf.Bytes, mCompIdBuf.Offset);
			if (mAddressBuf.Length > 0)
			{
				mAddress.Unmarshal(mAddressBuf);
				if(mDebugLevel > 1)
					Write("{2}: {0}.OnComponentRegistered({1})",
						mAddress.ToString(), compId.ToString(),Name);
			}
			else
				Write("RegisterComponent bogus address");
			AspireComponent comp;
			if (!mComponentByCompId.TryGetValue(compId, out comp))
				comp = AddComponent(mAddress,compId,msg);
			else
			{
				lock(mComponentByAddress)
					mComponentByAddress.Remove(comp.Address.Hash);
				//Need to go thru QueuedMessages and fixup addresses,
				//Might be able to utilize ProviderChanged for subscriptions
				mAddress.CopyTo(comp.Address);
				var registrar = AddressServer.FindCoreAddress(msg.Header.Source);
				if (registrar != null)
					comp.Domain = registrar.DomainName;
				lock (mComponentByAddress)
					mComponentByAddress[mAddress.Hash] = comp;
				comp.Active = true;
				//Need to get xTEDSUid so we can test for an xTEDS change.
				//However, this creates a lot of issues so we'll wait a bit
				//System.Threading.Thread.Sleep(10);
				//comp.Address.Marshal(mInfoAddressBuf);
				//Send(mGetComponentInfo);

			}
			// The ComponentRegistered address is wrt/ Directory. If Directory is local, that's OK
			// If Directory is remote, we need the address wrt/ this Browser
			if (!msg.Header.Source.IsLocal && (comp.Address.IsLoopback || !comp.Address.IsLocal))
			{
				var bytes = comp.CompUid.ToByteArray();
				mCompIdBuf.Set(bytes.Length, bytes, 0);
				Send(mGetAddressWithRespectTo);
			}
			mChangedActives = true;
		}

		void OnComponentModified(XtedsMessage msg)
		{
			Uuid compId = new Uuid(mCompIdBuf.Bytes, mCompIdBuf.Offset),
				xtedsId = new Uuid(mXtedsIdBuf.Bytes, mXtedsIdBuf.Offset);
			if (mAddressBuf.Length > 0)
			{
				mAddress.Unmarshal(mAddressBuf);
				if (mDebugLevel > 1)
					Write("{2}: {0}.OnComponentModified({1})",
						mAddress.ToString(), compId.ToString(),Name);
			}
			else
				Write("ModifyComponent bogus address");

			AspireComponent comp;
			if (mComponentByCompId.TryGetValue(compId, out comp))
			{
				ModifyComponent(comp,mAddress);

				var registrar = AddressServer.FindCoreAddress(msg.Header.Source);
				if (registrar != null)
					comp.Domain = registrar.DomainName;

				comp.Active = true;
				mChangedActives = true;

				// Check xtedsUid for changes
				if (comp.XtedsUid.IsEmpty || !comp.XtedsUid.Equals(xtedsId) )
				{
					comp.XtedsUid = xtedsId;
					comp.xtedsInProgress = true;
					mXtedsCache.RequestXteds(comp, this);
				}

			}
			else
				comp = AddComponent(mAddress,compId,msg);

			// Need to handle new xtedsId indicating an xteds change

			// The ComponentRegistered address is wrt/ Directory. If Directory is local, that's OK
			// If Directory is remote, we need the address wrt/ this Browser
			if (!msg.Header.Source.IsLocal && (comp.Address.IsLoopback || !comp.Address.IsLocal))
			{
				var bytes = compId.ToByteArray();
				mCompIdBuf.Set(bytes.Length, bytes, 0);
				Send(mGetAddressWithRespectTo);
				return;
			}
		}

		void OnComponentUnregistered(XtedsMessage msg)
		{
			if ( mAddressBuf.Length > 0 )
				mAddress.Unmarshal(mAddressBuf);
			var comp = Component(mAddress.Hash);
			if (comp == null) return;
			comp.Active = false;
			mChangedActives = true;
		}

		void OnComponentInfo(XtedsMessage msg)
		{
			if (mCompInfoStatus == CompInfoStatus.BadAddress ||
				mCompInfoStatus == CompInfoStatus.UnknownAddress)
			{
				Logger.Log(1,"{2}: {0}.OnComponentInfo: request had {1}",
					Path, mCompInfoStatus, Name);
				return;
			}
			mInfoAddress.Unmarshal(mInfoAddressBuf);
			if (mCompInfoStatus != CompInfoStatus.Valid)
				Logger.Log(1,"{3}: {0}.OnComponentInfo: request {1} had {2}",
					Path, mInfoAddress.ToString(), mCompInfoStatus,Name);

			if ( mCompInfoIdBuf.Length == 0 )
			{
				Logger.Log(1,"{1}: OnComponentInfo: Bad CompId or XtedsId from {0}\n",
					msg.Header.Source.ToString(),Name);
				return;
			}
			Uuid compId = new Uuid(mCompInfoIdBuf.Bytes, mCompInfoIdBuf.Offset),
				xtedsId = new Uuid(mXtedsIdBuf.Bytes, mXtedsIdBuf.Offset);
			if (mCompInfoStatus == CompInfoStatus.UnknownXteds || mXtedsIdBuf.Length == 0)
			{
				Logger.Log(1,"{2}: {0}.OnComponentInfo: requested comp {1} didn't have an xTEDS",
					Path,mInfoAddress.ToString(),Name);
				return;
			}
			AspireComponent comp;
			lock (mComponentByAddress)
				if (!mComponentByAddress.TryGetValue(mInfoAddress.Hash, out comp))
				{
					Logger.Log(1,"{3}: {0}.OncomponentInfo: new comp {1} {2}",
						Path, mInfoAddress.ToString(), mCompNameStr.String,Name);

					comp = new AspireComponent(this, mInfoAddress.Clone());
					var registrar = AddressServer.FindCoreAddress(msg.Header.Source);
					if (registrar != null)
						comp.Domain = registrar.DomainName;
					comp.Publish();
					mComponentByAddress[mInfoAddress.Hash] = comp;
					mComponents.Add(comp);
				}
			else if (!comp.XtedsUid.IsEmpty && comp.XtedsUid.Equals(xtedsId))
			{
				// 2nd reply detected
				Logger.Log(1,"{3}: {0}.OnComponentInfo: 2nd reply: {1}:{2}",
					Path, comp.Name, comp.NumPublishes,Name);
				comp.CompInfoTimer = false;
				comp.Publish();
				return;
			}

			comp.CompInfoTimer = false;
			comp.NumOnComponentInfos++;

			if (mCompInfoStatus == CompInfoStatus.Valid)
			{

				comp.Kind = mCompKindStr.String;
				comp.XtedsUid = xtedsId;
				comp.XtedsName = mCompNameStr.String;
				comp.AppDev = mAppDev;
			}
			else
			{
				Logger.Log(1,"{2}: {0}.OnComponentInfo: {1}", Path, mCompInfoStatus,Name);
				return;
			}

			bool requestXteds = false;

			string name;
			if (mAppDev == (byte)'D')
				name = comp.Kind;
			else
				name = comp.XtedsName;

			if (comp.nameSource == NameSource.Aliased || comp.nameSource == NameSource.MissionInformationService)
				// if MissionInformationService responded quicker than Directory
				return;
			// Check for this being the second occurance of DeviceName
			AspireComponent compByName;
			lock (mComponentByName)
				if (!mComponentByName.TryGetValue(name, out compByName))
				{
					if (!mComponentByName.TryGetValue(name+'1', out compByName))
						mComponentByName.TryGetValue(name+"-1", out compByName);
				}
			if (compByName != null && compByName.Address != mInfoAddress)
			{
				//comp.XtedsName = name; // Prevent setName from setting XtedsName
				char last = name[name.Length-1];
				bool separate = '0' <= last && last <= '9';
				if (separate)
					comp.Name = name + '-' + (++compByName.instances).ToString();
				else
					comp.Name = name + (++compByName.instances).ToString();
				if (compByName.instances == 2)
				{
					lock (mComponentByName)
					{
						mComponentByName.Remove(name);
						if (separate)
							compByName.Name = name+"-1";
						else
							compByName.Name = name+"1";
						mComponentByName.Add(compByName.Name, compByName);
					}
				}
			}
			else
			{
				//if (!AsimMgr.ComponentIsAliased(mInfoAddress.Hash, comp))
				comp.Name = String.Copy(name);
				lock (mComponentByName)
					mComponentByName.Add(comp.Name, comp);
				if (comp.Name == "MissionInformationService")
					requestXteds = true;
				else if (PnPBrowsers.UsageRequestsContain(Path + '.' + comp.Name))
					requestXteds = true;
				if (!requestXteds)
					SatisfyPending(comp, false);
			}
			//if ( !mComponentByCompId.ContainsKey(comp.CompUid) )
			//    mComponentByCompId.Add(comp.CompUid,comp);
			//if (!deferredXteds || requestXteds)
			if (requestXteds)
			{
				comp.xtedsInProgress = true;
				mXtedsCache.RequestXteds(comp,this);
			}
		}

		void OnAddressWithRespectTo(XtedsMessage msg)
		{
			if (mAddressBuf.Length == 0)
			{
				Write("OnAddressWithRespectTo bogus address");
				return;
			}

			mAddress.Unmarshal(mAddressBuf);
			Uuid compId = new Uuid(mCompIdBuf.Bytes, mCompIdBuf.Offset);
			AspireComponent comp;
			if (mComponentByCompId.TryGetValue(compId, out comp))
			{
				ModifyComponent(comp, mAddress);
				if (mDebugLevel > 1)
					Write("{0}'s address wrt Browser {1}", comp.Name, mAddress);
			}
			else if (mDebugLevel > 1)
				Write("{0}'s address wrt Browser {1}, but comp uid not found", compId, mAddress);
			if (comp.NeedsAddressTranslation)
			{
				comp.NeedsAddressTranslation = false;
				SatisfyPending(comp, false);
			}
		}

		public override void OnLocalManager(CoreAddress coreAddress, bool up)
		{
			base.OnLocalManager(coreAddress, up);

			if (up)
			{
				if (IsRegistered || DontRegister)
				{
					if (mComponents.Count > 0) mPmDirRestarts++;
					//ClearComponents = true;
					foreach (var comp in mComponents)
						comp.Active = false;
					mChangedActives = true;
				}
			}
		} int mPmDirRestarts;

		public override void OnQueryForMessage(int queryId, XtedsMessage msg)
		{
			//Write("Found message: {0}",msg.Name);
			switch ((QueryId)queryId)
			{
				case QueryId.GetAddressWithRespectTo:
					mGetAddressWithRespectTo = msg;
					msg.MapVariable("ComponentId", "mCompIdBuf", PrimitiveType.BUFFER);
					WhenMessageReplyArrives(msg, new XtedsMessage.Handler(OnAddressWithRespectTo)).
						MapVariable("ComponentId", "mCompIdBuf", PrimitiveType.BUFFER).
						MapVariable("Address", "mAddressBuf", PrimitiveType.BUFFER);
					break;
				case QueryId.GetComponentInfo:
					mGetComponentInfo = msg;
						msg.MapVariable("Address", "mGetInfoAddressBuf", PrimitiveType.BUFFER);
					WhenMessageReplyArrives(msg, new XtedsMessage.Handler(OnComponentInfo)).
						MapVariable("Address", "mInfoAddressBuf", PrimitiveType.BUFFER).
						MapVariable("AppDev", "mAppDev", PrimitiveType.UINT8).
						MapVariable("Name", "mCompNameStr", PrimitiveType.STRING).
						MapVariable("Kind", "mCompKindStr", PrimitiveType.STRING).
						MapVariable("ComponentId", "mCompInfoIdBuf", PrimitiveType.BUFFER).
						MapVariable("XtedsId", "mXtedsIdBuf", PrimitiveType.BUFFER).
						MapVariable("CompInfoStatus", "mCompInfoStatus");
					break;
				case QueryId.ModifyComponent:
					msg.MapVariable("Address", "mAddressBuf", PrimitiveType.BUFFER).
						MapVariable("ComponentId", "mCompIdBuf", PrimitiveType.BUFFER).
						MapVariable("XtedsId", "mXtedsIdBuf", PrimitiveType.BUFFER);
					WhenMessageArrives(msg, new XtedsMessage.Handler(OnComponentModified));
					break;
				case QueryId.RegisterComponent:
					msg.MapVariable("Address", "mAddressBuf", PrimitiveType.BUFFER).
						MapVariable("ComponentId", "mCompIdBuf", PrimitiveType.BUFFER);
					WhenMessageArrives(msg, new XtedsMessage.Handler(OnComponentRegistered));
					break;
				case QueryId.RequestXteds:
					mRequestXteds = msg;
					msg.MapVariable("XtedsId", "mRequestXtedsBuf", PrimitiveType.BUFFER);
					WhenMessageReplyArrives(msg, new XtedsMessage.Handler(OnXtedsReply)).
						MapVariable("XtedsId", "mRequestXtedsBuf", PrimitiveType.BUFFER).
						MapVariable("Text", "mRequestXtedsStr", PrimitiveType.STRING);
					break;
				case QueryId.UnregisterComponent:
					msg.MapVariable("Address", "mAddressBuf", PrimitiveType.BUFFER);
					WhenMessageArrives(msg, new XtedsMessage.Handler(OnComponentUnregistered));
					break;
			}
		}

		public void RequestXteds(AspireComponent component)
		{
			mRequestXtedsBuf.Set(Uuid.size, component.XtedsUid.ToByteArray(), 0);
			Send(mRequestXteds);
		}

		public void OnXtedsReply(XtedsMessage msg)
		{
			Uuid xtedsId = new Uuid(mRequestXtedsBuf.Bytes, mRequestXtedsBuf.Offset);

			mXtedsCache.OnXtedsReply(xtedsId, mRequestXtedsStr.String);
		}

		#endregion

		#region Properties

		[Category(components)]
		public XtedsCache Cache { get { return mXtedsCache; } }

		/// <summary>
		/// 
		/// </summary>
		[Description("Enable caching of a component's xTEDS.")]
		[Category(components)]
		[DefaultValue(true)]
		public bool CacheXteds
		{
			get { return mXtedsCache.CachingEnabled; }
			set
			{
				mXtedsCache.CachingEnabled = value;
				mXtedsCache.BlockSave = true;
				foreach (var comp in Components)
					comp.SetCached(value);
				mXtedsCache.SaveIndex();
			}
		}

		/// <summary>
		/// Double clicking clears out all registered components
		/// </summary>
		[Description("Double clicking clears out all registered components")]
		[Category(components)]
		[XmlIgnore]
		public bool ClearComponents
		{
			get { return false; }
			set
			{
				foreach (var comp in mComponents)
					comp.Unpublish();
				mComponents.Clear();
				mComponentByAddress.Clear();
				mComponentByName.Clear();
				mComponentByCompId.Clear();
				XtedsProtocol.Clear();
			}
		}

		[XmlElement("Component",typeof(AspireComponent))]
		[Description("All registered Aspire components.")]
		[Category(components)]
		public List<AspireComponent> Components
		{
			get { return mComponents; }
			set { mComponents = value; }
		}

		[DefaultValue(0)]
		[Description("Level of diagnostic printouts.")]
		[Category(diagnostics)]
		[XmlAttribute("debug")]
		public override int DebugLevel
		{
			get { return mDebugLevel; }
			set
			{
				base.DebugLevel = value;
				foreach (var comp in mComponents)
					comp.Debug = mDebugLevel;
			}
		}

		[Description("Domain id is the upper byte of the logical address for now.")]
		[Category(connectivity)]
		[XmlIgnore]
		public string DomainId
		{
			get { return string.Format("{0,2:X}",(OwnAddress.Hash&0xFF00)>>8); }
		}

		[XmlAttribute("name")]
		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				Prefix = (Path != null ? Path : Name) + ".";
			}
		}

		/// <summary>
		/// Returns the number of registered components
		/// </summary>
		[Description("Number of registered components."), XmlIgnore]
		[Category(components)]
		public int NumActive
		{
			get
			{
				if (mChangedActives)
				{
					mNumActive = 0;
					foreach (var comp in mComponents)
						if (comp.Active) mNumActive++;
					mChangedActives = false;
				}
				return mNumActive;
			}
		}
		int mNumActive;
		bool mChangedActives;

		/// <summary>
		/// Returns the number of registered components
		/// </summary>
		[Description("Number of components."), XmlIgnore]
		[Category(components)]
		public int NumComponents { get { return mComponents.Count; } set { } }

		/// <summary>
		/// Returns the number of registered components
		/// </summary>
		[Description("Number of unregistered components."), XmlIgnore]
		[Category(components)]
		public int NumInactive { get { return mComponents.Count - mNumActive; } }

		[DefaultValue(16)]
		[Description("Maximum dimension of byte arrays that will be published.")]
		public int PublishableByteArrayLength
		{
			get { return publishableByteArrayLength; }
			set { publishableByteArrayLength = value; }
		} int publishableByteArrayLength=16;

		#endregion

		#region IAsyncItemProvider

		public bool Matches(string path)
		{
			return path.StartsWith(Prefix);
		}

		[XmlIgnore]
		public string Prefix { get; private set; }

		public void Provide(string path)
		{
			string compName, ifaceName, msgName;
			if ( !NeededSubscription(path, out compName, out ifaceName, out msgName) )
			{
				UseComponent(compName);
				PendSubscription(compName, ifaceName, msgName);
			}
		}

		public void TransferItemsTo(IAsyncItemProvider provider){}

		bool NeededSubscription(string path, out string compName, out string ifaceName, out string msgName)
		{
			var tp = new TokenParser(path.Substring(Prefix.Length), ".");
			compName = tp.Token();
			ifaceName = tp.Token();
			if (ifaceName.Length == 0)
			{
				msgName = string.Empty;
				return true;
			}
			msgName = tp.Token();
			if (msgName.Length == 0) return true;
			bool needsSubscription = false;
			var comp = Component(compName);
			if (comp != null && comp.Xteds != null)
			{
				var msg = comp.Xteds.FindMessage(ifaceName, msgName);
				if (msg != null)
					needsSubscription = msg.MessageType is Xteds.Interface.Notification;
			}
			if (needsSubscription)
				Subscribe(comp, ifaceName, msgName);
			return needsSubscription;
		}

		public void Verify(string path)
		{
			string compName, ifaceName, msgName;
			NeededSubscription(path, out compName, out ifaceName, out msgName);
		}

		#endregion

		#region Helpers

		public AspireComponent this[string name]
		{
			get
			{
				AspireComponent comp;
				lock (mComponentByName)
					if (mComponentByName.TryGetValue(name, out comp))
						return comp;
				return null;
			}
		}

		public AspireComponent Component(string name)
		{
			AspireComponent comp;
			lock (mComponentByName)
				if (!mComponentByName.TryGetValue(name, out comp))
					return null;
			return comp;
		}

		public AspireComponent Component(uint hash)
		{
			AspireComponent comp;
			lock (mComponentByAddress)
				if (mComponentByAddress.TryGetValue(hash, out comp))
					return comp;
				else
					return null;
		}

		public void XtedsIsAvailable(AspireComponent comp, string xtedsText)
		{
			comp.XtedsText = xtedsText;
			SatisfyPending(comp, false);
		}

		void PendSubscription(string componentName, string interfaceName, string messageName)
		{
			List<string> list;
			int count;
			lock (subscriptionRequests)
			{
				if (!subscriptionRequests.TryGetValue(Path+'.'+componentName, out list))
				{
					list = new List<string>();
					subscriptionRequests[Path+'.'+componentName] = list;
					if (mDebugLevel > 1)
						Log.WriteLine("Adding {0} to pending subscriptions", componentName);
				}
				var fullMsg = interfaceName+'.'+messageName;
				count = list.Count;
				if ( !list.Contains(fullMsg) )
					list.Add(fullMsg);
			}
			if (mDebugLevel > 1 && list.Count > count)
				Log.WriteLine("{0}, pending {1}.{2} ({3}) subscription",
					componentName, interfaceName, messageName, list.Count);
		}

		public Address RoutedAddress(Address logicalAddress)
		{
			return logicalAddress;
		}

		bool SatisfyPending(AspireComponent comp, bool requestNow)
		{
			if (comp != null)
			{
				if (comp.NeedsAddressTranslation) return false;
				//Write("SatisfyPending {0}, xip {1}", comp, comp.xtedsInProgress);
				comp.xtedsInProgress = false;
				if (comp.Xteds != null && !comp.xtedsInProgress)
				{
					PnPBrowsers.RemoveUsage(Path + '.' + comp.Name);
					if (comp.Name != "MissionInformationService" )
					{
						// Let clients know their component is available
						PnPBrowsers.RaiseComponentAvailable(comp);
					}

					if (requestNow) return true;
				}
				else if (requestNow || PnPBrowsers.UsageRequestsContain(Path + '.' + comp.Name))
					if (mXtedsCache.RequestXteds(comp,this))
						comp.xtedsInProgress = true;
			}

			if (requestNow) return false;

			if (comp == null || comp.Xteds == null) return false;

			lock (subscriptionRequests)
			{
				List<string> list;
				if ( !subscriptionRequests.TryGetValue(Path+'.'+comp.Name,out list) )
					return false;

				mIsPending = true;
				foreach (string ifaceMsg in list)
				{
					string[] names = ifaceMsg.Split('.');
					Logger.Log(2, "{0}: Subscribing to pending {1}", Name, ifaceMsg);
					Subscribe(comp, names[0], names[1]);
				}
				mIsPending = false;
				subscriptionRequests.Remove(Path+'.'+comp.Name);
			}

			return true;
		}

		bool mIsPending;

		internal void Subscribe(AspireComponent comp, string interfaceName, string messageName)
		{
			string name = comp.Name+'.'+interfaceName+'.'+messageName;
			XtedsMessage msg = comp.Xteds.FindMessage(interfaceName, messageName);
			if (msg != null)
			{
				if (msg is Xteds.Interface.DataMessage)
				{
					//comp.Subscribe(msg as Xteds.Interface.DataMessage);
					(msg as Xteds.Interface.DataMessage).Subscribe();
					// Let clients know their component is available
					PnPBrowsers.RaiseComponentAvailable(comp);
				}
				else if ( !mIsPending )
					Log.WriteLine(Name+".Subscribe", MsgLevel.Warning,
						"Can't subscribe to {0} {1}", msg.GetType().Name, name);
			}
			else
				Log.WriteLine(Name+".Subscribe", MsgLevel.Warning,
					"Can't find message {0}", name);
		}

		public void Subscribe(string componentName, string interfaceName, string messageName)
		{
			string name = componentName+'.'+interfaceName+'.'+messageName;
			AspireComponent comp = null;
			comp = Component(componentName);
			if (comp != null && comp.Xteds != null)
			{
				Subscribe(comp, interfaceName, messageName);
				return;
			}
			else
			{
				UseComponent(componentName);
				PendSubscription(componentName, interfaceName, messageName);
			}
		}

		public void Subscribe(IDataMessage message, Address provider)
		{
			// How do we do this now ?
			//AddMessage(message, provider);
			mControlProtocol.SendSubscriptionRequest(provider, message.XtedsMessage.MessageId, 1, 0, 0, null);
		}

		#endregion

		#region Manifest

		public void QueueManifest()
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(ShowManifest));
		}

		void ShowManifest(object state)
		{
            System.IO.TextWriter writer = null;
			ManifestProgress progress = null;
            try
            {
                writer = FileUtilities.OpenScenarioTextWriter("Manifest.csv");
            }
            catch( Exception )
            {
                Log.WriteLine("Unable to create the manifest CSV file. Perhaps manifest.csv is already opened in another application?");
            }
            
            bool createdFile = false;

            if (writer != null)
            {
                try
                {
				var sb = new StringBuilder(DateTime.Now.ToString("F"));
				sb.Replace(',', ' ');
				writer.WriteLine("Date,{0}", sb.ToString());
				writer.WriteLine();
				progress = new ManifestProgress(2*Components.Count);
				progress.Show();
				foreach (var comp in Components)
				{
					int count = 0;
					if (comp.Xteds == null)
						comp.GetCategoryStates();

					while (comp.Xteds == null && count++ < 30)
					{
						System.Threading.Thread.Sleep(100);
					}
					progress.Increment();
				}
				writer.WriteLine("Devices,Name,Dev name,XtedsVersion,Address,"+
					"CompUid,XtedsUid,CRCProgCode,CRCxTEDS,SWCoreLibRev,HWFPGAFirmwareRev");
				foreach (var comp in Components)
				{
					if (comp.Xteds == null || comp.Xteds.Device == null) continue;
					writer.Write(",{0},{1},{2},{3},{4},{5}",
						comp.Name, comp.Name, comp.Xteds.Version, comp.Address.ToString(),
						comp.CompUid,comp.XtedsUid);
					foreach (var iface in comp.Xteds.Interfaces)
					{
						if (iface.Name.Contains("ASIM"))
						{
							var msg = iface.FindMessage("GetVersionInfo") as Xteds.Interface.CommandMessage;
							if (msg == null) break;
							Xteds.Interface.Message reply = iface.FindMessage("VersionInfoReply");
							if (reply == null) break;
							int received = reply.Received;
							comp.Send(msg);
							for (int i=0; i<30; i++)
							{
								if (reply.Received > received) break;
								System.Threading.Thread.Sleep(100);
							}
							if (reply.Received > received)
							{
								//var compUid = new Guid(reply.Variable("GUID").Value as Buffer);
								//var xtedsUid = new Guid();//reply.Variable("GUID").Value as byte[]);
								writer.Write(",x{0:X4},x{1:X4},x{2:X2},x{3:X2}",
								   reply.Variable("CRCProgCode").Value,
								   reply.Variable("CRCxTEDS").Value,
								   reply.Variable("SWCoreLibRev").Value,
								   reply.Variable("HWFPGAFirmwareRev").Value
								   );
							}
						}
					}
					writer.WriteLine();
					progress.Increment();
				}
				writer.WriteLine("Applications,Name,App name,XtedsVersion,Address,CompUid,XtedsUid");
				foreach (var comp in Components)
				{
					if (comp.Xteds == null || comp.Xteds.Application == null) continue;
					writer.Write(",{0},{1},{2},{3},{4},{5}",
						comp.Name, comp.Name, comp.Xteds.Version, comp.Address.ToString(),
						comp.CompUid,comp.XtedsUid);
					writer.WriteLine();
					progress.Increment();
				}

				createdFile = true;
				}
				catch (Exception ex)
				{
					Log.ReportException(ex, Name+".ShowManifest");
				}
				finally
				{
					if (progress != null) progress.Close();

					writer.Close();

					if (createdFile)
					{
						Process.Start("Excel", string.Format(@"/p ""{0}\Manifest.csv""", Scenario.Directory));
					}
				}
			}
		}

		#endregion

    public override void OnRegistrarChanged()
		{
			//Write("Browser registrar changed to {0}",Registrars[0].ToString());
			//Initialize(true);
		}

		/// <summary>
		/// A client wants to insure that a component is loaded and available for messaging
		/// </summary>
		/// <param name="componentName">The component name</param>
		public void UseComponent(string componentName)
		{
			var comp = Component(componentName);
			if (comp == null) comp = Components.FirstOrDefault<AspireComponent>(c => c.Name.Equals(componentName));

			if (SatisfyPending(comp, true))
				return;

			PnPBrowsers.AddUsage(Path + '.' + componentName, componentName);
		}

		public void Unsubscribe(IDataMessage message, Address provider)
		{
			CancelSubscription(message, provider);
		}

	}

  ///// <summary>
  ///// Summary description for Manifest BlackboardAction.
  ///// </summary>
  //public class ManifestAction : BlackboardAction
  //{
  //  public ManifestAction() : base("Aspire Manifest") { }

  //  public override bool Visible(Blackboard.Item item, out bool enabled)
  //  {
  //    enabled = item.Value is AspireBrowser;
  //    return enabled;
  //  }

  //  public override void Execute(Blackboard.Item item)
  //  {
  //    (item.Value as AspireBrowser).QueueManifest();
  //  }
  //}

  /// <summary>
	/// Summary description for AspireShell BlackboardAction.
	/// </summary>
	public class PendingSubscriptionsAction : BlackboardAction
	{
		public PendingSubscriptionsAction() : base("Subscribe to pending") { }

		public override bool Visible(Blackboard.Item item, out bool enabled)
		{
			var browser = item.Value as AspireBrowser;
			bool visible = browser != null;
			enabled = false;
			if ( visible )
			{
				foreach ( var comp in browser.Components)
					if ( comp.HasPendingSubscriptions )
						enabled = true;
			}
			return visible;
		}

		public override void Execute(Blackboard.Item item)
		{
			var browser = item.Value as AspireBrowser;
			foreach (var comp in browser.Components)
				comp.SatisfyPendingSubscriptions();
		}
	}
}
