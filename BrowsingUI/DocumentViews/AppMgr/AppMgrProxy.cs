using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;
using Aspire.Utilities;

namespace Aspire.BrowsingUI.DocumentViews.AppMgr
{
	public class AppMgrProxy : Aspire.CoreModels.Application
	{
		enum AddRemoveStatus { Added, Removed, NotFound };
		enum LoadStatus { Loaded, FileNotFound, Malformed, PartialLoad, Unloaded,
			PreviousConfigDeactivated };
		public enum Property { Arguments, Console, Heartbeat, Id, Log, MaxHeapSize, Name, Priority,
			Restart, StackSize, Uid, UnresponsiveRestartPeriod };
		enum QueryId { AddApp=1, GetConfigFiles, GetProperty, List, Load, PropertyChanged, RemoveApp,
			Restart, SetHeartbeat, SetProperty, Start, StatusChange, Stop };
		enum Result { OK, InvalidPID, OsError, ValueDifferent };
		enum StartStatus { OK, PathInvalid, ArgStringInvalid, InsufficientMemory, NotInConfiguration };

		IAppInfoClient mClient;
		XtedsMessage mAddApp, mGetConfigFiles, mGetProperty, mSetHeartbeat, mList, mLoad,
			mRemoveApp, mRestart, mSetProperty, mStart, mStop;
		string mArgs, mConfigName, mConfigFileName, mExecutableName, mName = null,
			mPreviousConfigName, mStringValue = null, mUid;
		AddRemoveStatus mAddRemoveStatus = AddRemoveStatus.Added;
		AppInfo.AppState mState = AppInfo.AppState.Running;
		AppInfo.ChangeReason mChangeReason = AppInfo.ChangeReason.Started;
		AppInfo
			pm = new AppInfo() { Name = "ProcessorManager" },
			dir = new AppInfo() { Name = "Directory" };
		AppMgrDoc.ConsoleState mConsole;
		LoadStatus mLoadStatus = LoadStatus.Loaded;
		Property mProperty = Property.Id;
		Result mResult = Result.OK;
		StartStatus mStartStatus = StartStatus.OK;
		float mPriority;
		int mId=0, mPid, mValue = 0;
		uint mMaxHeapSize, mStackSize;
		bool mConnected, mHeartbeat, mHeartbeatReply=false, mAppRestart, mLog, settingProperty;

		public AppMgrProxy()
		{
		}

		public AppMgrProxy(IAppInfoClient client)
			: base("{04F74C38-B42B-4017-91F9-8A5C3EA31B09}")
		{
			mClient = client;
			//DontRegister = true;
			Dummy(mArgs, mExecutableName, mPriority, mStackSize);
		}

		void Dummy(string x, string y, float z, uint w) { }

		internal void AddAppToConfig(AppInfo appInfo)
		{
			mName = appInfo.Name;
			mArgs = appInfo.Args;
			mConsole = appInfo.Console;
			mId = appInfo.Id;
			mLog = appInfo.Log;
			mMaxHeapSize = appInfo.MaxHeapSize;
			mPriority = appInfo.Priority;
			mAppRestart = appInfo.Restart;
			mStackSize = appInfo.StackSize;
			mUid = mId == 0 ? null : appInfo.Uid.ToString();
			Send(mAddApp);
		}

		public void Disconnect()
		{
			mConnected = false;
		}

		internal bool Heartbeat
		{
			set
			{
				mHeartbeat = value;
				if (mSetHeartbeat != null)
					Send(mSetHeartbeat);
			}
		}

		//todo: How to use FSMs
		internal bool IsConnected { get { return IsRegistered && mConnected && mStart != null; } }

		protected override void Initialize(bool dummy)
		{
			Register();

			Query().ForMessage((int)QueryId.GetProperty, "Interface name=IApplicationMgmt Request(CommandMsg name=GetProperty)");
			Query().ForMessage((int)QueryId.List, "Interface name=IApplicationMgmt Request(CommandMsg name=List)");
			Query().ForMessage((int)QueryId.PropertyChanged, "Interface name=IApplicationMgmt Notification(DataMsg name=PropertyChanged)");
			Query().ForMessage((int)QueryId.Restart, "Interface name=IApplicationMgmt Request(CommandMsg name=Restart)");
			Query().ForMessage((int)QueryId.SetProperty, "Interface name=IApplicationMgmt Request(CommandMsg name=SetProperty)");
			Query().ForMessage((int)QueryId.Start, "Interface name=IApplicationMgmt Request(CommandMsg name=Start)");
			Query().ForMessage((int)QueryId.StatusChange, "Interface name=IApplicationMgmt Notification(DataMsg name=StatusChange)");
			Query().ForMessage((int)QueryId.Stop, "Interface name=IApplicationMgmt Request(CommandMsg name=Stop)");

			Query().ForMessage((int)QueryId.Load, "Interface name=IApplicationConfiguration Request(CommandMsg name=Load)");
			Query().ForMessage((int)QueryId.AddApp, "Interface name=IApplicationConfiguration Request(CommandMsg name=AddApp)");
			Query().ForMessage((int)QueryId.GetConfigFiles, "Interface name=IApplicationConfiguration Request(CommandMsg name=GetConfigFiles)");
			Query().ForMessage((int)QueryId.RemoveApp, "Interface name=IApplicationConfiguration Request(CommandMsg name=RemoveApp)");
			Query().ForMessage((int)QueryId.SetHeartbeat, "Interface name=IApplicationConfiguration Request(CommandMsg name=SetHeartbeat)");
		}

		internal void LoadConfiguration(string name)
		{
			if (mLoad == null) return;
			mPreviousConfigName = mConfigName;
			mConfigName = name;
			mConfigFileName = name;
			Send(mLoad);
		}

		void OnGetPropertyReply(XtedsMessage msg)
		{
			if (mResult == Result.OK)
			{
				settingProperty = true;
				mClient.PropertyChanged(mPid, mProperty, mValue, mStringValue);
				settingProperty = false;
			}
		}

		void OnSetHeartbeatReply(XtedsMessage msg)
		{
			if (mHeartbeat != mHeartbeatReply)
				Log.WriteLine("Heartbeat could not be set to {0}", mHeartbeat);
		}

		public override void OnQueryForMessage(int queryId, XtedsMessage msg)
		{
			switch ((QueryId)queryId)
			{
				case QueryId.AddApp:
					mConnected = true;
					mAddApp = msg;
					msg.
						MapVariable("Name", "mName").
						MapVariable("Arguments", "mArgs").
						MapVariable("Console", "mConsole").
						MapVariable("Id", "mId").
						MapVariable("Log", "mLog").
						MapVariable("MaxHeapSize", "mMaxHeapSize").
						MapVariable("Priority", "mPriority").
						MapVariable("Restart", "mAppRestart").
						MapVariable("StackSize", "mStackSize").
						MapVariable("Uid", "mUid");
					msg.ReplyMessage.
						MapVariable("Name", "mName").
						MapVariable("Id", "mName").
						MapVariable("AddRemoveStatus", "mAddRemoveStatus");
					WhenMessageReplyArrives(msg, OnAddAppReply);
					break;
				case QueryId.GetConfigFiles:
					mGetConfigFiles = msg;
					msg.ReplyMessage.
						MapVariable("Count","mValue").
						MapVariable("FilesCsv","mStringValue");
					WhenMessageReplyArrives(msg,OnConfigFiles);
					break;
				case QueryId.GetProperty:
					mGetProperty = msg;
					msg.
						MapVariable("PID", "mPid").
						MapVariable("Property", "mProperty");
					msg.ReplyMessage.
						MapVariable("PID", "mPid").
						MapVariable("Result", "mResult").
						MapVariable("Property", "mProperty").
						MapVariable("Value", "mValue").
						MapVariable("StringValue", "mStringValue");
					WhenMessageReplyArrives(msg, OnGetPropertyReply);
					break;
				case QueryId.List:
					mList = msg;
					msg.ReplyMessage.
						MapVariable("Name", "mName").
						MapVariable("PID", "mPid").
						MapVariable("State", "mState");
					WhenMessageReplyArrives(msg, OnListReply);
					break;
				case QueryId.Load:
					mLoad = msg;
					msg.
						MapVariable("Name", "mConfigFileName");
					msg.ReplyMessage.
						MapVariable("Name", "mName").
						MapVariable("Status", "mLoadStatus");
					WhenMessageReplyArrives(msg, OnLoadReply);
					break;
				case QueryId.PropertyChanged:
					msg.
						MapVariable("PID", "mPid").
						MapVariable("Property", "mProperty").
						MapVariable("Value", "mValue").
						MapVariable("StringValue", "mStringValue");
					WhenMessageArrives(msg, OnPropertyChanged);
					break;
				case QueryId.RemoveApp:
					mRemoveApp = msg;
					msg.
						MapVariable("Name", "mName").
						MapVariable("Id", "mId");
					msg.ReplyMessage.
						MapVariable("Name", "mName").
						MapVariable("Id", "mName").
						MapVariable("AddRemoveStatus", "mAddRemoveStatus");
					WhenMessageReplyArrives(msg, OnRemoveAppReply);
					break;
				case QueryId.Restart:
					mRestart = msg;
					msg.
						MapVariable("PID", "mPid");
					msg.ReplyMessage.
						MapVariable("PID", "mPid").
						MapVariable("Result", "mResult");
					WhenMessageReplyArrives(msg, OnRestartReply);
					break;
				case QueryId.SetHeartbeat:
					mSetHeartbeat = msg;
					msg.
						MapVariable("Heartbeat", "mHeartbeat");
					msg.ReplyMessage.
						MapVariable("Heartbeat", "mHeartbeatReply");
					WhenMessageReplyArrives(msg, OnSetHeartbeatReply);
					break;
				case QueryId.SetProperty:
					mSetProperty = msg;
					msg.
						MapVariable("PID", "mPid").
						MapVariable("Property", "mProperty").
						MapVariable("Value", "mValue").
						MapVariable("StringValue", "mStringValue");
					msg.ReplyMessage.
						MapVariable("PID", "mPid").
						MapVariable("Result", "mResult").
						MapVariable("Property", "mProperty").
						MapVariable("Value", "mValue");
					WhenMessageReplyArrives(msg, OnSetPropertyReply);
					break;
				case QueryId.Start:
					mStart = msg;
					msg.
						MapVariable("ExecutableName", "mExecutableName").
						MapVariable("Arguments", "mArgs").
						MapVariable("Console", "mConsole").
						MapVariable("MaxHeapSize", "mMaxHeapSize").
						MapVariable("Priority", "mPriority").
						MapVariable("StackSize", "mStackSize");
					msg.ReplyMessage.
						MapVariable("Name", "mName").
						MapVariable("Id", "mId").
						MapVariable("PID", "mPid").
						MapVariable("StartStatus", "mStartStatus");
					WhenMessageReplyArrives(msg, OnStartReply);
					break;
				case QueryId.StatusChange:
					msg.MapVariable("Name", "mName").
						MapVariable("Id", "mId").
						MapVariable("PID", "mPid").
						MapVariable("State", "mState").
						MapVariable("ChangeReason", "mChangeReason");
					WhenMessageArrives(msg, OnStatusChange);
					break;
				case QueryId.Stop:
					mStop = msg;
					msg.
						MapVariable("PID", "mPid");
					msg.ReplyMessage.
						MapVariable("PID", "mPid").
						MapVariable("Result", "mResult");
					WhenMessageReplyArrives(msg, OnStopReply);
					break;
			}
		}

		void OnAddAppReply(XtedsMessage msg)
		{
			if ( mAddRemoveStatus != AddRemoveStatus.Added )
				Log.WriteLine("AddApp {0} {1} {2}", mName, mId, mAddRemoveStatus);
		}

		void OnConfigFiles(XtedsMessage msg)
		{
			int count = mValue;
			mConfigFiles = mStringValue.Split(',');
		} string[] mConfigFiles;

		void OnRemoveAppReply(XtedsMessage msg)
		{
			if (mAddRemoveStatus != AddRemoveStatus.Removed)
				Log.WriteLine("RemoveApp {0} {1} {2}", mName, mId, mAddRemoveStatus);
		}

		void OnListReply(XtedsMessage msg)
		{
			if (mPid > -1)
				Log.WriteLine("{0} {1} {2}", mName, mPid, mState);
		}

		void OnLoadReply(XtedsMessage msg)
		{
			switch (mLoadStatus)
			{
				case LoadStatus.Loaded:
				case LoadStatus.Unloaded:
					break;
				case LoadStatus.PreviousConfigDeactivated:
					mClient.ConfigChanged(mName,mConfigName);
					break;
				case LoadStatus.FileNotFound:
				case LoadStatus.Malformed:
				case LoadStatus.PartialLoad:
					Log.WriteLine("configuration {0} failed to load: {1}", mName, mLoadStatus);
					break;
			}
		}

		void OnPropertyChanged(XtedsMessage msg)
		{
			settingProperty = true;
			mClient.PropertyChanged(mPid, mProperty,mValue,mStringValue);
			settingProperty = false;
		}

		void OnRestartReply(XtedsMessage msg)
		{
			//Log.WriteLine("restarted {0} {1}", mPid, mResult);
		}

		void OnSetPropertyReply(XtedsMessage msg)
		{
			if (mResult == Result.OK) return;
			else if (mResult == Result.OsError)
				Send(mGetProperty);
			else if (mResult == Result.ValueDifferent)
				mClient.PropertyChanged(mPid, mProperty, mValue,null);
		}

		void OnStartReply(XtedsMessage msg)
		{
			if (mName == "ProcessorManager")
				pm.Pid = mPid;
			else if (mName == "Directory")
				dir.Pid = mPid;
			else if ( mStartStatus != StartStatus.OK )
				Log.WriteLine("Couldn't start {0} {1} {2}", mName, mPid, mStartStatus);
		}

		void OnStopReply(XtedsMessage msg)
		{
			if (mPid == dir.Pid) dir.Pid = 0;
			else if (mPid == pm.Pid) pm.Pid = 0;
			else if ( mResult != Result.OK )
			{
				//mClient.PropertyChanged(mPid, Property.Pid, 0);
				Log.WriteLine("Couldn't stop {0} {1}", mPid, mResult);
			}
		}

		void OnStatusChange(XtedsMessage msg)
		{
			mConnected = true;
			//Log.WriteLine("status {0} {1} {2} {3}", mName, mPid, mState, mChangeReason);
			//if (mPid == -1) return;
			if (mName == "ProcessorManager")
			{
				pm.Pid = mPid;
				mClient.ProcessStatusChanged(mName, -1, mPid, mState, mChangeReason);
			}
			else if (mName == "Directory")
				dir.Pid = mPid;
			else
				mClient.ProcessStatusChanged(mName, mId, mPid, mState, mChangeReason);
		}

		internal void RestartApp(int pid)
		{
			if (mStop == null || pid == 0) return;

			mPid = pid;
			Send(mRestart);
		}

		internal void SetProperty(AppInfo appInfo, Property property, int value, string stringValue = null)
		{
			if (settingProperty) return;
			mPid = appInfo.Pid;
			mProperty = property;
			mValue = value;
			mStringValue = stringValue;
			Send(mSetProperty);
		}

		internal void StartApp(AppInfo appInfo)
		{
			if (mStart == null) return;

			mExecutableName = appInfo.Name;
			mId = appInfo.Id;
			mArgs = appInfo.Args;
			if ( mId > 0 )
				mArgs += " -u" + appInfo.Uid.ToString();
			mConsole = appInfo.Console;
			mMaxHeapSize = mStackSize = 0;
			mPriority = 0;
			Send(mStart);
		}

		internal void StopApp(int pid)
		{
			if (mStop == null || pid == 0) return;

			mPid = pid;
			Send(mStop);
		}

		internal void StartAspire(AppMgrDoc.ConsoleState console)
		{
			pm.Console = console;
			StartApp(pm);
			dir.Console = console;
			StartApp(dir);
			//mAspireStopped = false;
			//LoadConfiguration("*", "");	// Either implement '*' to reactivate the active config or AppMgr might
											// detect Aspire's restart
		}

		internal void StopAspire()
		{
			LoadConfiguration("");
			StopApp(pm.Pid);
			StopApp(dir.Pid);
			//mAspireStopped = true;
		}
	}

	public interface IAppInfoClient
	{
		AppMgrProxy AppMgrProxy { get; }
		void ConfigChanged(string prevConfigName, string newConfigName);
		void PropertyChanged(int pid, AppMgrProxy.Property property, int value, string stringValue=null);
		void ProcessStatusChanged(string name, int id, int pid, AppInfo.AppState state, AppInfo.ChangeReason reason);
		void SyncProperty(AppInfo appInfo, AppMgrProxy.Property property, int value, string stringValue=null);
	}
}