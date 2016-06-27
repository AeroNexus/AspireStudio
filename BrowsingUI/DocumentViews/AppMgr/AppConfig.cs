using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Serialization;

using Aspire.Core.Utilities;
using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.BrowsingUI.DocumentViews.AppMgr
{
	#region AppInfo
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class AppInfo : INotifyPropertyChanged
	{
		public enum AppState { Running, Unresponsive, Inactive, Restarting, DidNotStart };
		public enum ChangeReason { Started, Stopped, StateChange };
		const string category = "AppInfo";

		internal AppInfo Clone()
		{
			var ai = new AppInfo();
			ai.mName = mName;
			ai.mArgs = mArgs;
			ai.mConsole = mConsole;
			ai.mId = mId;
			ai.mLog = mLog;
			ai.mMaxHeapSize = mMaxHeapSize;
			ai.mPid = mPid;
			ai.mPriority = mPriority;
			ai.mRestart = mRestart;
			ai.mStackSize = mStackSize;
			ai.mState = mState;
			ai.mUid = mUid;
			ai.mUnresponsiveRestartPeriod = mUnresponsiveRestartPeriod;
			return ai;
		}

		internal void CopyState(AppInfo dst)
		{
			dst.mPid = mPid;
			dst.mState = mState;
			dst.mPriority = mPriority;
		}

		// Keep this as the first property so the XML looks better
		[Category(category),XmlAttribute("name"),Description("Moniker. Also the root filename of the executable. ApplicationManager will add a platform-required suffix as required")]
		[EditorAttribute(typeof(AspireAppEditor), typeof(UITypeEditor))]
		public string Name { get { return mName; } set { mName = value; Notify(); } } string mName;

		[Category(category), XmlAttribute("args"), DefaultValue(""),Description("Command line arguments, as they would appear on the command line")]
		public string Args { get { return mArgs; } set { mArgs = value; Notify(); } } string mArgs;

		[Category(category), XmlAttribute("console"), DefaultValue(AppMgrDoc.ConsoleState.Inherit),
		 Description("When the process is created, should it have a console and should that console be open or minimized")]
		public AppMgrDoc.ConsoleState Console { get { return mConsole; } set { mConsole = value; Notify("Console"); } }
		AppMgrDoc.ConsoleState mConsole = AppMgrDoc.ConsoleState.Inherit;

		[Category(category), XmlAttribute("heartbeat"), DefaultValue(true),Description("Should a heartbeat be used with the application")]
		public bool Heartbeat { get { return mHeartbeat; } set { mHeartbeat = value; Notify(); } } bool mHeartbeat = true;

		[Category(category), XmlAttribute("id"), DefaultValue(0), Description("Ordinal identifier of multiple instances of an app")]
		public int Id { get { return mId; } set { mId = value; Notify(); } } int mId;

		[Category(category), XmlAttribute("log"), DefaultValue(false),Description("Should a log file be created to record standard output")]
		public bool Log { get { return mLog; } set { mLog = value; Notify(); } } bool mLog;

		[Category(category), XmlAttribute("maxHeapSize"), DefaultValue(0),Description("Maximum heap size [bytes]. (Future)")]
		public uint MaxHeapSize { get { return mMaxHeapSize; } set { mMaxHeapSize = value; Notify(); } } uint mMaxHeapSize;

		[Category(category), XmlIgnore,Description("OS process ID")]
		public int Pid { get { return mPid; } set { mPid = value; Notify(); } } int mPid;

		[Category(category), XmlAttribute("priority"), DefaultValue(0),Description("Normalized priority")]
		public float Priority { get { return mPriority; } set { mPriority = value; Notify(); } } float mPriority;

		[Category(category), XmlAttribute("restart"), DefaultValue(true),Description("Should the app be restarted if it dies")]
		public bool Restart { get { return mRestart; } set { mRestart = value; Notify(); } } bool mRestart = true;

		[Category(category), XmlAttribute("stackSize"), DefaultValue(0),Description("Maximum stack size [bytes]. (Future)")]
		public uint StackSize { get { return mStackSize; } set { mStackSize = value; Notify(); } } uint mStackSize;

		[Category(category), XmlIgnore,Description("Run state of the app.")]
		public AppState State { get { return mState; } set { mState = value; Notify(); } } AppState mState = AppState.Inactive;

		[Category(category), XmlIgnore,Description("Unique identifier of the process. If not specified, use the internal one. Typically used for multiple instances of the same app.")]
		public Uuid Uid { get { return mUid; } set { mUid = value; Notify(); } } Uuid mUid;

		[XmlAttribute("uid"), DefaultValue(""), Browsable(false)]
		public string xmlUid
		{
			get { return Uid.IsEmpty ? "" :  Uid.ToString(); }
			set { Uid = new Uuid(value); }
		}

		[Category(category), XmlAttribute("unresponsiveRestartPeriod"), DefaultValue(0)]
		[Description("Amount of time after detecting an app's unresponsiveness to restart the app. Restart must also be true")]
		public int UnresponsiveRestartPeriod { get { return mUnresponsiveRestartPeriod; } set { mUnresponsiveRestartPeriod = value; Notify(); } }
		int mUnresponsiveRestartPeriod;

		public override string ToString()
		{
			return Name;
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		void Notify([CallerMemberName]string name="")
		{
			if (PropertyChanged != null)
				try
				{
					PropertyChanged(this, new PropertyChangedEventArgs(name));
				}
				catch (InvalidOperationException) { }
				catch (Exception) { }
		}

		#endregion
	}
	#endregion

	#region AppConfig
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[DefaultProperty("Name")]
	public class AppConfig : INotifyPropertyChanged
	{
		public enum ConfigState { Quiescent, NewConfig, StateChanged };

		SortableBindingList<AppInfo> apps = new SortableBindingList<AppInfo>();
		List<AppInfo> old = new List<AppInfo>();

		public AppConfig()
		{
			apps.ListChanged += apps_ListChanged;
			//.CollectionChanged += apps_CollectionChanged;
		}

		[Description("List of applications")]
		[XmlElement("App",typeof(AppInfo))]
		public SortableBindingList<AppInfo> Apps
		{
			get { return apps; }
			set { apps = value; }
		}

		internal AppConfig Clone()
		{
			var ac = new AppConfig();
			ac.Name = Name;
			foreach (var app in apps)
			{
				var newApp = app.Clone();
				ac.Apps.Add(newApp);
				old.Add(newApp);
			}
			return ac;
		}

		[XmlIgnore]
		internal IAppInfoClient Client { get; set; }

		[Description("Default ConsoleState for all apps that don't specify their own")]
		[XmlAttribute("console"), DefaultValue(AppMgrDoc.ConsoleState.None)]
		public AppMgrDoc.ConsoleState Console { get { return mConsole; } set { mConsole = value; IsDirty = true; } }
		AppMgrDoc.ConsoleState mConsole;

		internal int FindNextAppId(string name, AppInfo ignore = null)
		{
			int id = 0;
			foreach (var app in Apps)
				if (app != ignore && app.Name == name && app.Id >= id) id = app.Id+1;
			return id;
		}

		[Description("Master heartbeat enable. Must be true to send heartbeats to all apps.")]
		[XmlAttribute("heartbeat"), DefaultValue(false)]
		public bool Heartbeat { get { return mHeartbeat; } set { mHeartbeat = value; IsDirty = true; } }
		bool mHeartbeat;

		[XmlIgnore, Browsable(false)]
		internal bool IsDirty { get; set; }

		[Description("Friendly name and root file name ({name}.apps.xml)")]
		[XmlAttribute("name")]
		public string Name
		{
			get { return mName; }
			set
			{
				mName = value;
				IsDirty = true;
				if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Name"));
			}
		} string mName;

		internal void Save(string directory)
		{
			if (IsDirty)
			{
				this.XmlSerialize(Path.Combine(directory, Name + ".apps.xml"));
				IsDirty = false;
			}
		}

		internal event EventHandler StateChanged;

		internal ConfigState State
		{
			get { return mState; }
			set
			{
				mState = value;
				if ( value != ConfigState.Quiescent && StateChanged != null )
					StateChanged(this,EventArgs.Empty);
			}
		} ConfigState mState;

		public override string ToString()
		{
			return Name;
		}

		void apps_ListChanged(object sender, ListChangedEventArgs e)
		{
			AppInfo ai;
			switch (e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
					IsDirty = true;
					ai = apps[e.NewIndex];
					ai.PropertyChanged += item_PropertyChanged;
					ai.Id = FindNextAppId(ai.Name,ai);
					if (ai.Id > 0)
						ai.Uid = Uuid.NewUuid();
					old.Add(ai);
					break;
				case ListChangedType.ItemDeleted:
					ai = old[e.NewIndex];
					if (ai.State == AppInfo.AppState.Running)
						Client.AppMgrProxy.StopApp(ai.Pid);
					old.Remove(ai);
					break;
			}
		}

		void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var ai = sender as AppInfo;
			switch (e.PropertyName)
			{
				case "Args":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Arguments, 0, ai.Args);
					break;
				case "Console":
					IsDirty = true;
					//ToDo:Changing console state affects the orderly shutdown . Need to accomodate this in AM/PI
					//if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Console, (int)ai.Console);
					break;
				case "Id":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Id, ai.Id);
					break;
				case "Log":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Log, ai.Log ? 1 : 0);
					break;
				case "MaxHeapSize":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.MaxHeapSize, (int)ai.MaxHeapSize);
					break;
				case "Name":
					IsDirty = true;
					//What about changing the name remotely ?
					break;
				case "Priority":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Priority, (int)(ai.Priority * 1e7f));
					break;
				case "Restart":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Restart, ai.Restart ? 1 : 0);
					break;
				case "StackSize":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.StackSize, (int)ai.StackSize);
					break;
				case "Uid":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.Uid, 0, ai.Uid.ToString());
					break;
				case "UnresponsiveRestartPeriod":
					IsDirty = true;
					if (Client != null) Client.SyncProperty(ai, AppMgrProxy.Property.UnresponsiveRestartPeriod, ai.UnresponsiveRestartPeriod);
					break;
				case "Pid":
				case "State":
					State = ConfigState.StateChanged;
					break;
			}
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		void Notify([CallerMemberName]string name = "")
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}
	#endregion

	#region App UI Editor
	class AspireAppEditor : UITypeEditor
	{
		IWindowsFormsEditorService editorService;

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			if (provider != null)
			{
				editorService = provider.GetService(typeof(IWindowsFormsEditorService))
					as IWindowsFormsEditorService;
				var ai = context.Instance as AppInfo;
				Log.WriteLine("editing {0}", ai.Name);
			}

			if (editorService != null)
			{
				var lb = new ListBox();
				lb.SelectedIndexChanged += lb_SelectedIndexChanged;
				lb.Items.AddRange(ApplicationManager.The.AspireApps);

				editorService.DropDownControl(lb);

				value = lb.SelectedItem;
			}
			return value;
		}

		void lb_SelectedIndexChanged(object sender, EventArgs e)
		{
			editorService.CloseDropDown();
		}

		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}
	}
	#endregion

	#region SortableBindingList

	public class SortableBindingList<T> : BindingList<T>
	{
		bool isSorted;
		ListSortDirection sortDirection;
		PropertyDescriptor sortProperty;

		// Core sort methods
		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			// Get list to sort
			var items = Items as List<T>;

			// Apply and set the sort, if items to sort
			if( items != null )
			{
				var pc = new PropertyComparer<T>(property.Name, direction);
				items.Sort(pc);
				isSorted = true;
			}
			else
				isSorted = false;

			sortDirection = direction;
			sortProperty = property;

			// Let bound controls know they should refresh their views
			OnListChanged( new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
		protected override void RemoveSortCore()
		{
		}

		// Core sort properties
		protected override bool SupportsSortingCore { get { return true; } }
		protected override bool IsSortedCore { get { return isSorted; } }
		protected override ListSortDirection SortDirectionCore { get { return sortDirection; } }
		protected override PropertyDescriptor SortPropertyCore { get { return sortProperty; } }
	}

	public class PropertyComparer<T> : IComparer<T>
	{
		private PropertyInfo property;
		private ListSortDirection sortDirection;

		public PropertyComparer(string sortProperty, ListSortDirection sortDirection)
		{
			property = typeof(T).GetProperty(sortProperty);
			if ( property == null )
				Log.WriteLine("Property {0} not found on type {1}", sortProperty, typeof(T).FullName);
			this.sortDirection = sortDirection;
		}

		public int Compare(T x, T y)
		{
			var valueX = property.GetValue(x, null);
			var valueY = property.GetValue(y, null);

			if (property.PropertyType == typeof(string))
			{
				if (sortDirection == ListSortDirection.Ascending)
					return String.Compare(valueX as string, valueY as string);
				else
					return String.Compare(valueY as string, valueX as string);
			}
			else if (property.PropertyType == typeof(int) ||
				property.PropertyType.IsEnum)
			{
				if (sortDirection == ListSortDirection.Ascending)
					return Comparer<int>.Default.Compare((int)valueX, (int)valueY);
				else
					return Comparer<int>.Default.Compare((int)valueY, (int)valueX);
			}
			else return 0;
		}
	}

	#endregion

}