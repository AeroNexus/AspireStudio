using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
	public class Application : Model, IApplication, IHostXtedsLite
	{
		const string category = "Application";
		const string connectivity = "Connectivity";

		protected ControlProtocol mControlProtocol;
		protected int mDebugLevel;
		protected Thread mThread;

		MarshaledString mXtedsString = string.Empty;
		Message mSyncParseHeader;
		MessageHandler mMessageHandler;
		Aspire.Core.Utilities.Timer mExecutionTimer;
		Transport mTransport, mSynchronousReplyReader;
		Uuid mCompUid;
		Xteds mXteds;
		XtedsProtocol mXtedsProtocol;

        private string mTransportName;
        string mDomainName = "Default";
		int mLogLength=32, mPort;
		bool mDontRegister, mRun = true, mStandaloneFromParameter;


        public Application(string compIdText)
        {
            if(!String.IsNullOrEmpty(compIdText))
                mCompUid = new Uuid(compIdText);
            mTransportName = Config.TransportName;
            CleanPublish = true;
            ExecutionPeriod = new SecTime(1, 0);

            mExecutionTimer = Core.Utilities.Timer.Set(mExecutionPeriod,
                Core.Utilities.Timer.Trigger.Periodic, PerformInternal, false);

            
            // The rest is in Discover so we have access to properties set in the XML
        }

        public Application() : this(null)
		{
		}


		#region Component implementation

		public override void Discover(Scenario scenario)
		{
			if (LocalPort > 0)
			{
			}
			if (RemotePort > 0)
			{
			}
			if (scenario["AspireStandalone"] != null)
			{
				Standalone = true;
				mStandaloneFromParameter = true;
			}
			if (DomainName == "Default" && Parent != null && Parent is Application)
				DomainName = (Parent as Application).DomainName;
			int n = Xteds.maxInterfaceId; // trigger static Xteds()
			if (xtedsFileName.Length == 0)
			{
				BuildXteds(ComponentName==null?Name:ComponentName);
			}

			Executive.Exit += new EventHandler(Exiting);

			base.Discover(scenario);

			var thread = new Thread(new ThreadStart(Run))
			{
				Name = Name,
				IsBackground = true,
			};
			mThread = thread;
			thread.Start();
		}

		void Exiting(object sender, EventArgs e)
		{
			Close(0);
		}

		public override Dictionary<string, bool> GetCategoryStates()
		{
			var dict = base.GetCategoryStates();
			dict.Add(category, false);
			dict.Add(connectivity, false);
			dict.Add("Diagnostics", false);
			return dict;
		}

		bool InitializeInternal()
		{
			mTransport = TransportFactory.Create(mTransportName, FixedPort);

			if (IoLoggingEnabled)
				mTransport = new LoggingTransportDecorator(mTransport,this,mLogLength);

			if (mTransport.Open() < 0)
			{
				Logger.Log(1, "Could not open transport. Exiting");
				Thread.CurrentThread.Abort();
				return false;
			}

			mMessageHandler = new MessageHandler(this, mTransport);

			mXtedsProtocol = new XtedsProtocol(mXteds, mTransport);
			mXtedsProtocol.AppName = Path;
			mXtedsProtocol.FindComponentWithWholeAddress = AspireManager.FindComponentWithWholeAddress;

			mControlProtocol = CreateControlProtocol();
			mControlProtocol.XtedsProtocol = mXtedsProtocol;
			mControlProtocol.DebugLevel = DebugLevel;
			OnControlProtocol(mControlProtocol);
			mExecutionTimer.Enable = true;
			mQueryMgr.ControlProtocol = mControlProtocol;
			OnXtedsIdValid();

			mMessageHandler.AddProtocol(ProtocolId.Aspire,mControlProtocol);
			mMessageHandler.AddProtocol(ProtocolId.XtedsCommand, mXtedsProtocol);
			mMessageHandler.AddProtocol(ProtocolId.XtedsData, mXtedsProtocol);
			return true;
		}

		public override void Unload()
		{
			//mProtocolMux.CancelLocalManagerSubscriptions();
			Close(0);
			Unregister();
			mThread.Abort();
			base.Unload();
		}

		#endregion

		#region Protected

		protected virtual void Initialize(bool dummy)
		{
		}

		/// <summary>
		/// Finds a data message in our own xTEDS using interface and message name.
		/// IDataMessages are used for subscribing or publishing and to reply to requests.
		/// </summary>
		/// <param name="interfaceName">Name of the interface containing the message.</param>
		/// <param name="messageName">Name of the message.</param>
		/// <returns>null if not found, otherwise the IDataMessage</returns>
		protected IDataMessage OwnDataMessage(string interfaceName, string messageName)
		{
			IDataMessage dmsg = Xteds.FindDataMessage(interfaceName, messageName);
			if (dmsg == null)
			{
				Logger.Log(1, "{0}: Can't find data message {1}.{2} in own xTEDS", Path, interfaceName, messageName);
				var xmsg = Xteds.FindXtedsMessage(interfaceName, messageName);
				if (xmsg != null)
					Logger.Log(1, "However, it is defined in the xTEDS as an XtedsMessage");
			}
			else
			{
				if ( DefaultLeasePeriod != 0 )
					dmsg.AllowableLeasePeriod = DefaultLeasePeriod;
				(dmsg as XtedsMessage).Application = this;
			}

			return dmsg;
		}

		/// <summary>
		/// When a Directory is added/removed
		/// </summary>
		/// <param name="coreAddress"></param>
		/// <param name="added">true if Added, false if removed</param>
		public virtual void OnDirectory(CoreAddress coreAddress, bool added)
		{
		}

		/// <summary>
		/// When the LocalManager starts or shutsdown
		/// </summary>
		/// <param name="coreAddress"></param>
		/// <param name="up">true if LM started, false if shutdown</param>
		public virtual void OnLocalManager(CoreAddress coreAddress, bool up)
		{
		}

		/// <summary>
		/// Finds a message in our own xTEDS using interface and message name. It can be
		/// used in variable maps and to send with.
		/// </summary>
		/// <param name="interfaceName">Name of the interface containing the message.</param>
		/// <param name="messageName">Name of the message.</param>
		/// <returns>null if not found, otherwise the IDataMessage</returns>
		protected XtedsMessage OwnMessage(string interfaceName, string messageName)
		{
			XtedsMessage xmsg = Xteds.FindXtedsMessage(interfaceName, messageName);
			if (xmsg == null)
			{
				if ( !Xteds.QuietFind)
					Logger.Log(1, "Can't find message {0}.{1} in own xTEDS", interfaceName, messageName);
				if (xmsg is IDataMessage)
					Logger.Log(1, "{0} is a IDataMessage, not just an XtedsMessage", messageName);
			}
			else
				xmsg.Application = this;
			return xmsg;
		}

		protected void Register() { } // For legacy apps

		protected void Unregister()
		{
			Close(0);
		}

		#endregion

		#region Properties

		[Category(connectivity),XmlIgnore]
		public Uuid CompUid
		{
			get { return mCompUid; }
			set
			{
				mCompUid = value;
				if (mControlProtocol is ApplicationProtocol)
					(mControlProtocol as ApplicationProtocol).CompUid = mCompUid;
			}
		}

		[Category(connectivity)]
		[Instanced, XmlAttribute("compId"),DefaultValue("")]
		public string CompUidString
		{
			get { return mCompUid.ToString(); }
			set
			{
				mCompUid = new Uuid(value);
				if (mControlProtocol is ApplicationProtocol)
					(mControlProtocol as ApplicationProtocol).CompUid = mCompUid; 
			}
		}

		[Category(category),XmlIgnore]
		public string ComponentName { get; set; }

		[Category(category),XmlIgnore]
		public virtual ControlProtocol ControlProtocol
		{
			get { return mControlProtocol; }
			set { mControlProtocol = value; }
		}

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
				if (mControlProtocol != null) mControlProtocol.DebugLevel = value;

				// Not so much changing the transport dynamically
				//if (mTransport != null)
				//{
				//	if (mDebugLevel > 1 && !(mTransport is LoggingTransportDecorator))
				//		mTransport = new LoggingTransportDecorator(mTransport,this);
				//	else if (mDebugLevel <= 1 && mTransport is LoggingTransportDecorator)
				//		mTransport = (mTransport as LoggingTransportDecorator).Transport;
				//}
				Logger.LogLevel = mDebugLevel;
			}
		}

		[Category(category)]
		[XmlAttribute("defaultLeasePeriod"), DefaultValue(0)]
		public int DefaultLeasePeriod { get; set ; }

		[Category(connectivity)]
		[XmlAttribute("domain"), DefaultValue("Default"), Instanced]
		public virtual string DomainName
		{
			get { return mDomainName; }
			set
			{
				mDomainName = value;
			}
		}

		protected bool DontRegister { get { return mDontRegister; } set { mDontRegister = value; } }

		[Browsable(false)]
		public uint ElapsedSeconds
		{
			get { return mTiming.ElapsedSeconds; }
		}
		Timing mTiming = new Timing();

		[Category(category),XmlIgnore]
		public SecTime ExecutionPeriod
		{
			get { return mExecutionPeriod; }
			set
			{
				mExecutionPeriod = value;
				if ( mExecutionTimer != null )
					mExecutionTimer.Change(mExecutionPeriod);
			}
		} SecTime mExecutionPeriod;

		[Category(category),XmlIgnore]
		protected double ExecutionPeriodSeconds
		{
			set
			{
				var period = new SecTime(value);
				ExecutionPeriod = period;
			}
		}

		[Category(connectivity), XmlAttribute("fixedPort"), DefaultValue(0)]
		public int FixedPort { get { return mPort; } set { mPort = value; } }

		[Category("Diagnostics"),XmlAttribute("ioLog")]
		public bool IoLoggingEnabled { get; set; }

		[Browsable(false), XmlIgnore]
		public bool IsClosing
		{
			get { return !mRun; }
		}

		/// <summary>
		/// Is the debug level at least 'level'
		/// </summary>
		/// <param name="level">The level to compare against</param>
		/// <returns>true if it is.</returns>
		public bool IsDebugLevel(int level) { return mDebugLevel >= level; }

		[Category(category),XmlAttribute("logLength")]
		public int LogLength { get { return mLogLength; } set { mLogLength = value; } }

		[Category(category)]
		public IKnownMarshaler IKnownMarshaler
		{
			get
			{
				return AspireClock.The;
			}
		}

		[Category(connectivity)]
		public Address LocalManagerAddress
		{
			get { return null; }// mControlProtocol != null ? mControlProtocol.LocalManagerAddress : null; }
		}

		//[Category(connectivity),XmlIgnore]
		//public bool LocalManagerIsPresent
		//{
		//	get { return false; }// mControlProtocol != null ? mControlProtocol.LocalManagerIsPresent : mLmIsPresent; }
		//	set
		//	{
		//		//if ( mControlProtocol!=null )
		//		//	mControlProtocol.LocalManagerIsPresent = value;
		//		mLmIsPresent = value;
		//	}
		//} bool mLmIsPresent;

		[Category(connectivity)]
		public Address OwnAddress
		{
			get { return mControlProtocol.OwnAddress; }
		}

		public QueryMgr Query()
		{
			return Query(DomainScope.Specifically,null);
		}
		public QueryMgr Query(DomainScope scope)
		{
			return Query(scope,null);
		}

		public QueryMgr Query(DomainScope scope, string domainName)
		{
			mQueryMgr.Scope = scope;
			mQueryMgr.DomainName = domainName;
			return mQueryMgr;
		} QueryMgr mQueryMgr = new QueryMgr();

		public QueryMgr Query(Application provider)
		{
			mQueryMgr.QueryProvider = provider;
			return mQueryMgr;
		}

		[Category(connectivity),XmlIgnore]
		public CoreAddressList Registrars
		{
			//todo: what about registrars ?
			get { return null; }// return mControlProtocol != null ? mControlProtocol.Registrars : null; }
		}

		[Category(connectivity), XmlAttribute("standalone"), DefaultValue(false)]
		public bool Standalone
		{
			get
			{
				if (IsSaving && mStandaloneFromParameter) return false;
				return mStandalone;
			}
			set
			{
				mStandalone = value;
				DontRegister = true;
			}
		} bool mStandalone;

		[Category(connectivity), XmlIgnore]
		public virtual Transport Transport { get { return mTransport; } set { mTransport = value; } }

		[Category(connectivity)]
		[Instanced]
		[XmlAttribute("transport")]
		public string TransportName
        {
            get { return mTransportName; }
            set { mTransportName = value; }
        }

		[Category(category), XmlAttribute("commonXtedsDirectory"), DefaultValue("./xTEDS/")]
		public static string CommonXtedsDirectory
		{
			get
			{
				if (mCommonXtedsDirectory == null)
				{
					Log.WriteLine("The first Aspire application or Asim or AsimMgr needs to set commonXtedsDirectory. Defaulting to ./xTEDS/");
					mCommonXtedsDirectory = "./xTEDS/";
				}
				return mCommonXtedsDirectory;
			}
			set { mCommonXtedsDirectory = value; }
		} static string mCommonXtedsDirectory;

		[Category(category), XmlAttribute("xtedsDirectory")]
		public string XtedsDirectory
		{
			get { return mXtedsDirectory; }
			set { mXtedsDirectory = value; }
		} string mXtedsDirectory;

		/// <summary>
		/// The parsed xTEDS.
		/// </summary>
		[Browsable(false),XmlIgnore]
		public IXteds IXteds
		{
			get { return mXteds; }
			set { mXteds = value as Xteds; }
		}

		/// <summary>
		/// The parsed xTEDS.
		/// </summary>
		[Category(category),XmlIgnore]
		public Xteds Xteds
		{
			get { return mXteds; }
			set { mXteds = value; }
		}

		[Category(category), XmlAttribute("xtedsFile"), DefaultValue("")]
		public virtual string XtedsFile
		{
			get { return xtedsFileName; }
			set
			{
				xtedsFileName = value;
				string dir, rootDir = Scenario.Directory, path;
				if (rootDir == null) rootDir = string.Empty;

				if (XtedsDirectory != null)
					dir = XtedsDirectory;
				else
					dir = CommonXtedsDirectory;

				if (dir != null)
					path = System.IO.Path.Combine(rootDir,dir,xtedsFileName);
				else
					path = System.IO.Path.Combine(rootDir,"xTEDS",xtedsFileName);

				var test = System.IO.Path.GetFullPath((new Uri(path)).LocalPath);

				try
				{
					using (StreamReader sr = new StreamReader(path))
					{
						XtedsText = sr.ReadToEnd() + '\0';
					}
				}
				catch (System.Exception e)
				{
					Log.ReportException(e,"Application.Opening xTEDS file {0}", xtedsFileName);
					return;
				}

				try
				{
					Xteds = Xteds.Parse(XtedsText, path, this);
				}
				catch (System.Exception e)
				{
					Log.ReportException(e,"set_XtedsFile");
					if (mXteds == null)
						Logger.Log(1, "set_XtedsFile", MsgLevel.Warning,
							"Tried to load {0}'s xTEDS file, {1} and failed", Path, path);
				}
				OnPropertyChanged("XtedsFile");
			}
		} string xtedsFileName = string.Empty;

		/// <summary>
		/// Access the xTEDS text.
		/// </summary>
		/// <value>The xTEDS text.</value>
		[Category(category),XmlIgnore]
		[DefaultValue("")]
		public string XtedsText
		{
			get { return mXtedsText; }
			set
			{
				mXtedsText = value;
				mXtedsString = new MarshaledString(mXtedsText);
			}
		} string mXtedsText = string.Empty;

		protected XtedsProtocol XtedsProtocol { get { return mXtedsProtocol; } }

		[Category(connectivity), XmlIgnore]
		public Uuid XtedsUid
		{
			get { return mControlProtocol.XtedsUid; }
			set { mControlProtocol.XtedsUid = value; }
		}

		#endregion

		#region IHostXtedsLite implementation

		[Category(category),XmlIgnore]
		public HostType HostType { get { return HostType.Provider; } }

		[Category(category),XmlIgnore]
		public int AllowableLeasePeriod { get { return 0; } }

		#endregion

		#region Methods

		void BuildXteds(string name)
		{
		  XtedsText = XtedsHelper.ConstructXteds(name);
			try
			{
				Xteds = IXteds.Parse(this, XtedsText, "internal") as Xteds;
			}
			catch (System.Exception e)
			{
				Log.ReportException(e,"BuildXteds");
				if (mXteds == null)
					Logger.Log(1, "BuildXteds", MsgLevel.Warning, 
						"Tried to build {0}'s xTEDS and failed", Path);
			}
		}

	  public void CancelSubscription(IDataMessage message, Address provider=null)
		{
			Publication pub = message.Publication;
			int dialog;
			if (pub != null)
			{
				pub.CancelLeaseRenewal();
				dialog = pub.Dialog;
			}
			else
				dialog = 0;
			mControlProtocol.SendSubscriptionCancel(provider!=null ? provider : message.Provider,
				message.XtedsMessage.MessageId, null, dialog);
		}

		public void CancelSubscription(MessageId messageId, Address provider)
		{
			mControlProtocol.SendSubscriptionCancel(provider, messageId, null, 0);
		}

		public void Close(int reason)
		{
			mRun = false;
		}

		ControlProtocol CreateControlProtocol()
		{
			// This can be extended to return a (control) protocol mux if it also derives from ControlProtocol
			return new ApplicationProtocol(this, mTransport, mXtedsString, mCompUid, mQueryMgr);

			//legacy code
			//todo: What about protocol mux ?
			//mProtocolMux = new ProtocolMux(this);
			//mProtocolMux.DirectoryAdded += new EventHandler(mProtocolMux_DirectoryAdded);
			//if (mStandalone)
			//{
			//	var cp = mMessageHandler.Protocols(ProtocolId.Aspire) as ControlProtocol;
			//	mProtocolMux.StandaloneProtocol = cp;
			//	cp.AddressServer.AddAddress("DIRECTORY", OwnAddress, "Local", "Default");
			//}
		}

		public bool DonePolling(SecTime waitPeriod)
		{
			mMessageHandler.PollMessagesAndTimers(waitPeriod);
			if ( IsClosing )
			{
				Teardown();
				return true;
			}
			return false;
		}

		public void InitRegister()
		{
			try
			{
				Initialize(true);
			}
			catch(System.Threading.ThreadAbortException){}
			catch ( Exception e )
			{
				Log.ReportException(e, "{0}.Initialize", Name);
			}
		}

		[Category(connectivity),XmlAttribute("localPort"),DefaultValue(0)]
		public int LocalPort { get; set; }

		/// <summary>
		/// Marshal a message for delivery to the transport at a later time
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="destination">The destination component's address. If unspecified, sent
		/// to the message's provider if we found it from a query response.</param>
		/// <param name="tag">Some tag used for identification or sequencing.</param>
		/// <returns>The DeferredMsg ready to Send later.</returns>
		public DeferredMsg Marshal(XtedsMessage message, Address destination, int tag)
		{
			return mXtedsProtocol.Marshal(message,destination,tag);
		}

		[Category(category)]
		public virtual MessageHandler MessageHandler { get { return mMessageHandler; } }

		protected virtual void OnClosing()
		{
		}

		protected virtual void OnControlProtocol(ControlProtocol controlProtocol)
		{
		}

		protected virtual void OnDirectoryAdded(CoreAddress coreAddress)
		{
		}

		/// <summary>
		/// If overridden in your derived class, executes when a response to one of your
		/// query for message's arrives.
		/// </summary>
		/// <param name="queryId"> Identifier from the original query.</param>
		/// <param name="msg">The message satisfying the query.</param>
		public virtual void OnQueryForMessage(int queryId, XtedsMessage msg)
		{
			Logger.Log(1,"OnQueryForMessage stub:");
			Logger.Log(1,"{0} needs to implement this virtual method to handle a query response.", Path);
		}

		public virtual void OnQueryFound(int queryId, int responseId, StructuredResponse response)
		{
			Logger.Log(1, "OnQueryFound stub. You need to override this in your class for {0}", Path);
		}

		//todo: How do we trigger RegistrarChanged ?
		void mControlProtocol_RegistrarChanged(object sender, EventArgs e)
		{
			OnRegistrarChanged();
		}

		public virtual void OnRegistrarChanged()
		{
		}

		protected virtual void OnXtedsIdValid()
		{
		}

		public virtual void OnSubscriptionReply(XtedsMessage xmsg)
		{
			//Logger::Log(1, "OnSubscriptionReply stub");
		}

		public virtual void OnXtedsDelivered()
		{
			//Logger.Log(1,"OnXtedsDelivered stub");
		}

		// Note: 'Perform' is a rename of 'Execute' found in C++. Execute was taken by the Model framework
		public virtual void Perform()
		{
		}

		// Note: 'Perform' is a rename of 'Execute' found in C++. Execute was taken by the Model framework
		void PerformInternal()
		{
			if (mControlProtocol.IsOperational)
				Perform();
		}

		//todo: is there still a need for PreloadRegistrar ? in Standalone ?
		//public void PreloadRegistrar()
		//{
		//	mPreloadRegistrar = true;
		//} bool mPreloadRegistrar;

		public void Publish(IDataMessage message)
		{
			Publication pub = message.Publication;
			if (pub != null) pub.Publish(message, mXtedsProtocol);
		}

		//todo: Is IsRegistered still required w/ Directory FSM ?
		[Category(category)]
		public bool IsRegistered { get { return mControlProtocol.IsOperational; } }

		// Allow an IQueryClient to register with ApplicationProtocol. Example IQueryClients are: DataStream, etc
		public int RegisterQueryClient(IQueryClient client)
		{
			if (mControlProtocol == null)
			{
				Logger.Log(1, "Can't register query client: registration not finished");
				return 0;
			}
			return mControlProtocol.RegisterQueryClient(client);
		}

		[Category(connectivity), XmlAttribute("remotePort"), DefaultValue(0)]
		public int RemotePort { get; set; }

		/// <summary>
		/// Reply to a request message. Same as request.ReplyMessage().Send(), with error checking
		/// </summary>
		/// <param name="request">The request to send a reply to.</param>
		/// <param name="sequence">The sequence number to send with. 0 used the requestt's sequence.</param>
		public void ReplyTo(XtedsMessage request, int sequence=0)
		{
			if (sequence == 0) sequence = request.Header.Sequence;
			mXtedsProtocol.Send(request.ReplyMessage, request.Header.Source, sequence);
		}

		/// <summary>
		///  Synchronously send a request and await its reply reply. Will block for timeout
		/// duration. If timeout not specified, blocks forever.
		/// </summary>
		/// <param name="request">The request message to send.</param>
		/// <param name="timeoutSec">The timeout seconds to wait. If not specified, wait forever</param>
		/// <param name="timeoutUsec">The timeout micro-seconds to wait for. Defaults to 0</param>
		/// <returns>true if it received the reply, false if it timed out.</returns>
		bool RequestAwaitReply(XtedsMessage request, int timeoutSec, int timeoutUsec)
		{
			if ( request.IsSynchronous )
			{
				if (mSynchronousReplyReader == null)
				{
					mSynchronousReplyReader = TransportFactory.Create(mTransportName, 0, true);
					mSyncParseHeader = mXtedsProtocol.CloneHeader();
				}

				request.Header = mXtedsProtocol.CloneHeader();
				uint id = request.Header.Source.Hash;
				request.Header.Source = mSynchronousReplyReader.ListenAddress.Clone();
				request.Header.Source.Hash = id;
				request.IsSynchronous = true;
			}

			var timeout = new SecTime(timeoutSec,timeoutUsec);
			request.Header.NextSequence();

			mXtedsProtocol.Send(request,null,0);

			byte[] buffer = new byte[64*1024];

			//mSynchronousReplyReader.Lock();
			//mSynchronousReplyReader.Clear();

			bool delivered = false;

			while (!delivered)
			{
				//SendHeartbeat();
				int len = mSynchronousReplyReader.Read(buffer,timeout);
				if ( len > 0 )
				{
					if ( mMessageHandler.Parse(buffer,len,mSyncParseHeader) )
					{
						delivered = true;
						break;
					}
					else
					{
						// Old data in the reader, or out of sequence response
						continue;
					}
				}
				else
				{
					// Don't think we want to throw exceptions for timeouts.... Maybe maybe not
					return false;
				}
			}
			//mSynchronousReplyReader.Unlock();

			return delivered;
		}

		/// <summary>
		/// Resumption of earlier instance of this app
		/// </summary>
		public virtual void Resume()
		{
			// Override in your derived app as required.
		}

		/// <summary>
		/// Runs this application by initializing and receiving messages. If you overrode
		/// Perform(), it will be called every ExecutionPeriod(). If you set timers,
		/// they will be serviced. If you started other threads, they will run. You can also
		/// Start() this applications message handler on a separate thread in order to keep
		/// your own processing as the main thread of execution.
		/// </summary>
		void Run()
		{
			Setup();

			mMessageHandler.FieldMessages();   // blocks until told to shutdown
			
			Teardown();
		}

		/// <summary>
		/// Send a message to another component via the XtedsProtocol's transport.
		/// </summary>
		/// <param name="deferredMessage">The DeferredMsg to send.</param>
		/// <returns>The number of bytes transferred.</returns>
		int Send(DeferredMsg deferredMessage)
		{
			return mXtedsProtocol.Send(deferredMessage);
		}

		/// <summary>
		/// Send a message to another component.
		/// </summary>
		/// <param name="message">The message to send.</param>
		/// <param name="destination">The destination component's address. If unspecified, sent
		/// to the message`s provider if we found it from a query response.</param>
		/// <param name="tag">Some tag used for identification or sequencing.</param>
		/// <returns></returns>
		public int Send(XtedsMessage message, Address destination=null, int tag=0)
		{
			return mXtedsProtocol.Send(message, destination, tag);
		}

		/// <summary>
		/// Sets the disposition of an XtedsMessage. Use this to change a message's disposition to
		/// Deliver. All other dispositions can be set with msg.setDisposition()

		/// </summary>
		/// <param name="msg"> The message to set its disposition of</param>
		/// <param name="disposition">The disposition of the message.</param>  <seealso>XtedsMessage.Desposition</seealso>
		public void SetDisposition( XtedsMessage msg, XtedsMessage.DispositionType disposition )
		{
			const int deliveriesPerSec = 50;

			var old = msg.Disposition;
			msg.Disposition = disposition;

			if ( old != disposition && disposition == XtedsMessage.DispositionType.Deliver )
				mXtedsProtocol.InitiateDelivery((int)(deliveriesPerSec*ExecutionPeriod.ToDouble));
		}

		/// <summary>
		/// Sets the default notification message subscription lease policy. If
		/// leasePeriod is not forever (0), each of our OwnDataMessage()'s will have its
 		/// AllowableLeasePeriod() set to this leasePeriod. You can still set
 		/// AllowableLeasePeriod for each message as required.
		/// </summary>
		/// <remarks>AllowableLeasePeriod is ignored on command and data reply messages as
		/// well as messages returned from queries. You may only set policy on your own
		/// notifications.
		/// </remarks>
		/// <param name="leasePeriod">(optional) the lease period, 0 is forever.</param>
		public void SetSubscriptonPolicy(int leasePeriod = 0)
		{
			DefaultLeasePeriod = leasePeriod;
		}

		void Setup()
		{
			DomainName = mDomainName;
			InitializeInternal();

			InitRegister();

      if (mXteds == null) return;
			mXteds.VerifyVariableMapping();
			mControlProtocol.Start();
		}

		XtedsMessage SubscribeTo(XtedsMessage message, int Ith=1, Address client=null)
		{
			var dataMsg = message as IDataMessage;
			if (dataMsg != null && Ith > 0)
			{
				var pub = dataMsg.Publication;
				if (pub == null)
				{
					pub = new Publication(dataMsg as IPublisher);
					dataMsg.Publication = pub;
				}
				pub.Ith = Ith;
				Logger.Log(2, "{2}: Subscribing to {0}'s {1} message.",
					message.Provider, message.Name, Name);
				pub.resumeCanceled = mControlProtocol.SendSubscriptionRequest(message.Provider,
					message.MessageId, (byte)Ith, 0, 0, client, pub.resumeCanceled);
			}
			return message;
		}

		void Teardown()
		{
			OnClosing();

			mControlProtocol.Stop();
			mMessageHandler.Close();
			mTransport.Close();

			// Control will resume in main after the Run() command.
		}

		public XtedsMessage WhenMessageArrives(XtedsMessage message, XtedsMessage.Handler handler, int Ith=1)
		{
			if (message.Verified) return message;
			message.WhenMessageArrives += handler;
			if (message is IDataMessage && Ith > 0)
				SubscribeTo(message, Ith);
			return message;
		}

		Xteds.Interface.Message emptyMessage = new Xteds.Interface.Message();

		protected XtedsMessage WhenMessageReplyArrives(XtedsMessage msg, XtedsMessage.Handler handler)
		{
			XtedsMessage reply = msg.ReplyMessage;
			if (reply != null)
			{
				if (reply.Verified) return reply;
				reply.WhenMessageArrives += handler;
			}
			else
			{
				Logger.Log(1, "Reply not defined for {0}", msg);
				reply = emptyMessage;
			}
			return reply; 
		}

		public void WhenSubscribedTo(IDataMessage message, SubscribedToHandler handler)
		{
			if (message.Verified) return;
			var ipub = message as IPublisher;
			if (ipub != null)
				ipub.SubscribedTo += handler;
		}

		#endregion

		public void QueuePublisher(IPublisher publisher,bool cancel) { }


		#region IApplication Members


		#endregion
	}
}
