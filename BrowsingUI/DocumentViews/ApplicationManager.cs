using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Studio;
using Aspire.Studio.DocumentViews;
using Aspire.Utilities;
using Aspire.Utilities.Extensions;

using Aspire.BrowsingUI.DocumentViews.AppMgr;

namespace Aspire.BrowsingUI.DocumentViews
{
	public enum AspireState { Down, DownArmed, Up, UpArmed };

	public partial class ApplicationManager : StudioDocumentView, IAppInfoClient, IFileMgrClient
	{
		AppConfig mActiveConfig, mSelectedConfig;
		AppMgrDoc myDoc;
		AppMgrProxy mAppMgrProxy;
		AspireState aspireState;
		Dictionary<int, AppInfo> appsByPid = new Dictionary<int, AppInfo>();
		FileMgr mFileMgr;

		int activeConfigIndex, inactiveConfigIndex=-1;

		public static ApplicationManager The { get; private set; }

		public ApplicationManager()
			: base(Solution.ProjectItem.ItemType.Monitor)
		{
			InitializeComponent();
			mAppMgrProxy = new AppMgrProxy(this);
			mAppMgrProxy.Name = "AppMgrUI";
			mAppMgrProxy.InhibitPublish = true;
		}

		internal void ActivateConfig(string name)
		{
			if (!mFileMgr.IsAppMgrStarted)
				mFileMgr.StartAppMgr(true, name);
			else if (mAppMgrProxy.IsConnected)
				mAppMgrProxy.LoadConfiguration(name);
		}

		public override void AddingToDocManager()
		{
			if (Document == null)
			{
				myDoc = new AppMgrDoc();
				BrowseProperties(myDoc);
				MyDocument(myDoc);
			}
		}

		[XmlIgnore]
		public AppMgrProxy AppMgrProxy { get { return mAppMgrProxy; } }

		public string[] AspireApps
		{
			get
			{
				var appNames = mFileMgr.GetApplications();
				List<string> apps = new List<string>();
				foreach (var app in appNames)
					switch (app)
					{
						case "ApplicationManager":
						case "Directory":
						case "ProcessorManager":
							break;
						default:
							apps.Add(app);
							break;
					}
				aspireApps = apps.ToArray();

				return aspireApps;
			}
		} static string[] aspireApps = new string[0];

		ToolStripMenuItem[] AspireAppsToolStripItems
		{
			get
			{
				var apps = AspireApps;
				if (apps.Length > appsMenuItems.Length)
				{
					appsMenuItems = new ToolStripMenuItem[apps.Length];
					for (int i = 0; i < apps.Length; i++)
					{
						var item = new ToolStripMenuItem(apps[i], null, OnAddApp);
						appsMenuItems[i] = item;
					}
					appsMenuItemsModified = true;
				}
				return appsMenuItems;
			}
		} ToolStripMenuItem[] appsMenuItems = new ToolStripMenuItem[0];
		bool appsMenuItemsModified;

		public AspireState AspireState { get { return aspireState; } set { aspireState = value; } }

		public static string CombinePath(string directory, string fileName, string suffix)
		{
			if ( directory.EndsWith("/") )
				return directory + fileName + suffix;
			else
				return directory + '/' + fileName + suffix;
		}

		private void ConfigItemChecked(int i, bool value)
		{
			enableCheck = true;
			configsLb.SetItemChecked(i, value);
		}

		public void ConfigChanged(string prevConfigName, string newConfigName)
		{
Again:
			int i = 0;
			foreach (object item in configsLb.Items)
			{
				string name;
				if ( item is string )
					name = item as string;
				else
					name = (item as AppConfig).Name;
				if (name == newConfigName)
				{
					var old = mActiveConfig;
					if (item is string)
					{
						ReplaceConfig(name, i);
						goto Again;
					}
					else
					{
						mActiveConfig = item as AppConfig;
						mActiveConfig.State = AppConfig.ConfigState.NewConfig;
						activeConfigIndex = i;
					}
					foreach (var ai in old.Apps)
					{
						foreach (var nai in mActiveConfig.Apps)
							if (ai.Name == nai.Name && ai.Id == nai.Id)
							{
								ai.CopyState(nai);
								break;
							}
					}
				}
				else if (name == prevConfigName)
					inactiveConfigIndex = i;
				i++;
			}
			UpdateNow();
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as AppMgrDoc;
			BrowseProperties(myDoc);
			mFileMgr = myDoc.FileMgr;
			mFileMgr.Client = this;
			mAppMgrProxy.Discover(Scenario.The);
		}

		void OnAddApp(object sender, EventArgs e)
		{
			var item = sender as ToolStripMenuItem;

			var ai = new AppInfo()
			{
				Name = item.Text,
				//Console = myDoc.StartAspireConsole // leave it Inherit
			};

			if (mConfigsLbRow == -1)
			{
				mActiveConfig.Apps.Add(ai);

				if (mAppMgrProxy.IsConnected)
					mAppMgrProxy.AddAppToConfig(ai);
			}
			else
			{
				AppConfig appCfg;
				var obj = configsLb.Items[mConfigsLbRow];
				if (obj is string)
				{
					ReplaceConfig(obj as string, mConfigsLbRow);
					appCfg = configsLb.Items[mConfigsLbRow] as AppConfig;
				}
				else
					appCfg = obj as AppConfig;
				appCfg.Apps.Add(ai);
				appCfg.IsDirty = true;
				mConfigsLbRow = -1;
			}
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(ApplicationManager), typeof(AppMgrDoc));
			else
				DocumentMgr.DefineDocView(typeof(ApplicationManager), typeof(AppMgrDoc));
		}

		int ReplaceConfig(string name, int index )
		{
			var text = mFileMgr.ReadConfig(name);
			if (text == null) return -1;

			var nac = text.XmlDeserialize<AppConfig>();
			if (nac == null) return -1;

			nac.IsDirty = false;
			int checkedIndex = configsLb.CheckedIndices.Count == 0 ? -1 : configsLb.CheckedIndices[0];
			configsLb.Items.RemoveAt(index);
			int i = configsLb.Items.Add(nac);
			if (i == checkedIndex)
				ConfigItemChecked(i,true);
			return i;
		}

		public AppConfig SelectedConfig { get { return mSelectedConfig; } }

		void SetActiveConfig()
		{
			if (mActiveConfig != null)
			{
				mActiveConfig.StateChanged -= mActiveConfig_StateChanged;
				mActiveConfig.Client = null;
			}

			int i = 0;
			foreach (var obj in configsLb.Items)
			{
				if (obj is AppConfig && (obj as AppConfig).Name == myDoc.ActiveConfig)
				{
					mActiveConfig = obj as AppConfig;
					break;
				}
				else if (obj is string && (obj as string) == myDoc.ActiveConfig)
				{
					int j = ReplaceConfig(obj as string, i);
					if (j < 0) return;
					mActiveConfig = configsLb.Items[j] as AppConfig;
					break;
				}
				i++;
			}
			if (mActiveConfig == null) return;
			mActiveConfig.StateChanged += mActiveConfig_StateChanged;
			mActiveConfig.Client = this;
			applicationsTab.Text = mActiveConfig.Name;
		}

		void mActiveConfig_StateChanged(object sender, EventArgs e)
		{
			NeedUpdate();
		}

		void StartAppMgr(bool startAspire, string configName)
		{
			mFileMgr.StartAppMgr(startAspire, configName);

		}

		public override void UpdateDisplay(Clock clock)
		{
			switch (aspireState)
			{
				case AspireState.UpArmed:
					aspireTab.Text = "Aspire Up";
					aspireState = AspireState.Up;
					break;
				case AspireState.DownArmed:
					aspireTab.Text = "Aspire Down";
					aspireState = AspireState.Down;
					break;
			}
			if (mActiveConfig != null)
			{
				switch (mActiveConfig.State)
				{
					case AppConfig.ConfigState.StateChanged:
						dataGridView1.Refresh();
						mActiveConfig.State = AppConfig.ConfigState.Quiescent;
						break;
					case AppConfig.ConfigState.NewConfig:
						bindingSource1.DataSource = mActiveConfig.Apps;
						bindingSource1.Sort = "Name";
						mActiveConfig.State = AppConfig.ConfigState.Quiescent;

						ConfigItemChecked(activeConfigIndex, true);
						if ( inactiveConfigIndex >= 0 )
							ConfigItemChecked(inactiveConfigIndex, false);

						applicationsTab.Text = mActiveConfig.Name;
						myDoc.ActiveConfig = mActiveConfig.Name;

						break;
				}
			}
		}

		enum AppTool { Restart, Start, Stop, Remove };

		private void ApplicationManager_Load(object sender, EventArgs e)
		{
			Application.ApplicationExit += Application_ApplicationExit;
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Restart", null, restartToolStripMenuItem_Click));
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Start", null, startToolStripMenuItem_Click));
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Stop", null, stopToolStripMenuItem_Click));
			contextMenuStrip.Items.Add(new ToolStripMenuItem("Remove", null, removeToolStripMenuItem_Click));
			NameColumn.DataPropertyName = "Name";
			PidColumn.DataPropertyName = "Pid";
			StateColumn.DataPropertyName = "State";
			PriorityColumn.DataPropertyName = "Priority";
			IdColumn.DataPropertyName = "Id";

			var files = mFileMgr.GetConfigFiles();

			if ( files != null )
        foreach (var file in files)
				  if (file.ToLower().EndsWith(".apps.xml"))
				  {
					  var name = Path.GetFileNameWithoutExtension(file);
					  int index = name.IndexOf('.');
					  name = name.Substring(0, index);
					  int i = configsLb.Items.Add(name);
					  if (myDoc != null && name == myDoc.ActiveConfig)
						  ConfigItemChecked(i, true);
				  }
			SetActiveConfig();
			if (mActiveConfig != null)
			{
				bindingSource1.DataSource = mActiveConfig.Apps;
				bindingSource1.Sort = "Name";
			}
		}

		void addAppToConfig_DropDownOpening(object sender, EventArgs e)
		{
			var items = AspireAppsToolStripItems;
			var item = sender as ToolStripDropDownItem;

			if (appsMenuItemsModified || item.DropDownItems.Count == 1)
			{
				item.DropDownItems.Clear();
				item.DropDownItems.AddRange(items);
				appsMenuItemsModified = false;
			}
		}

		void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			if (e.Exception is IndexOutOfRangeException)
			{
			}
			else
				Log.WriteLine("DataError [{0},{1}]", e.RowIndex, e.ColumnIndex);
		}

		void Application_ApplicationExit(object sender, EventArgs e)
		{
			if ( (myDoc != null && myDoc.LeaveAppMgrRunningOnExit) || !mFileMgr.IsAppMgrStarted) return;
			mFileMgr.KillAppMgr(myDoc.Console != AppMgrDoc.ConsoleState.None);
		}

		void mAppMgrProxy_ProcessStatusChanged(object sender, EventArgs e)
		{
		}

		private void startAspireToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (mFileMgr.IsAppMgrStarted && mAppMgrProxy.IsConnected)
				mAppMgrProxy.StartAspire(myDoc.StartAspireConsole);
			else
			{
				if (mFileMgr.IsAppMgrStarted)
					stopAspireToolStripMenuItem_Click(null, null);
				StartAppMgr(true,myDoc.ActiveConfig);
			}
		}

		private void stopAspireToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// For now, defer this until the state machines are working in .Net
			//if (mAppMgrProxy.IsConnected)
			//	mAppMgrProxy.StopAspire();
			mAppMgrProxy.Disconnect();
			mFileMgr.KillAppMgr(true);
			aspireState = AspireState.DownArmed;
		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mActiveConfig.Apps.Remove(dataGridView1.Rows[activeRowIndex].Tag as AppInfo);
		}

		private void restartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mAppMgrProxy.RestartApp((dataGridView1.Rows[contextMenuRow].Tag as AppInfo).Pid);
		}

		private void startToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var ai = dataGridView1.Rows[contextMenuRow].Tag as AppInfo;
			mAppMgrProxy.StartApp(ai);
		}

		private void stopToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mAppMgrProxy.StopApp((dataGridView1.Rows[contextMenuRow].Tag as AppInfo).Pid);
		}

		private void configurationsTab_DoubleClick(object sender, EventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void dataGridView1_DoubleClick(object sender, EventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void newBtn_Click(object sender, EventArgs e)
		{
			var ac = new AppConfig() { Console = myDoc.Console };
			ac.PropertyChanged += ac_PropertyChanged;
			The = this;
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(ac);
		}

		void ac_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!configsLb.Items.Contains(sender))
			{
				int i = configsLb.Items.Add(sender as AppConfig);
				configsLb.SelectedIndex = i;
				mFileMgr.Save(sender as AppConfig);
			}
			configsLb.Refresh();
		}

		private void removeBtn_Click(object sender, EventArgs e)
		{
			if (configsLb.SelectedItem != null)
			{
				string name = null;
				if (configsLb.SelectedItem is string)
					name = configsLb.SelectedItem as string;
				else if (configsLb.SelectedItem is AppConfig)
					name = (configsLb.SelectedItem as AppConfig).Name;
				if (name != null)
					mFileMgr.Delete(name);
				configsLb.Items.Remove(configsLb.SelectedItem);
			}
		}

		private void configsLb_MouseDown(object sender, MouseEventArgs e)
		{
			var listBox = sender as CheckedListBox;
			var child = listBox.GetChildAtPoint(new Point(e.X, e.Y));
			mConfigsLbRow = e.Y / listBox.ItemHeight;
			if (mConfigsLbRow > listBox.Items.Count)
			{
				mConfigsLbRow = -1;
				addAppToConfigToolStripMenuItem.Visible = false;
			}
			else
				addAppToConfigToolStripMenuItem.Visible = true;
		} int mConfigsLbRow = -1;

		private void configsLb_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (configsLb.SelectedItem is string)
			{
				int i = ReplaceConfig(configsLb.SelectedItem as string, configsLb.SelectedIndex);
				if (i < 0) return;
				configsLb.SelectedIndex = i;
			}
			var ac = configsLb.SelectedItem;
			if (ac != null)
			{
				The = this;
				Aspire.Studio.DockedViews.PropertiesView.The.Browse(ac);
				mSelectedConfig = ac as AppConfig;
			}
		}

		private void activateBtn_Click(object sender, EventArgs e)
		{
			AppConfig ac;
			if (configsLb.SelectedItem != null)
			{
				ac = configsLb.SelectedItem as AppConfig;
				mFileMgr.Save(ac);
				if (!mFileMgr.IsAppMgrStarted)
				{
					//myDoc.ActiveConfig = ac.Name;
					ac.State = AppConfig.ConfigState.NewConfig;
					if (mActiveConfig != ac)
					{
						for (int i = 0; i < configsLb.Items.Count; i++ )
						{
							AppConfig apc = configsLb.Items[i] as AppConfig;
							if (apc == mActiveConfig)
							{
								inactiveConfigIndex = i;
								break;
							}
						}
					} 
					mActiveConfig = ac;
					activeConfigIndex = configsLb.SelectedIndex;
					UpdateDisplay(null);
					StartAppMgr(true, ac.Name);
				}
				else if (mAppMgrProxy.IsConnected)
					mAppMgrProxy.LoadConfiguration(ac.Name);
			}
		}

		private void cloneBtn_Click(object sender, EventArgs e)
		{
			var ac = (configsLb.SelectedItem as AppConfig).Clone();
			ac.Name = null;
			ac.PropertyChanged += ac_PropertyChanged;
			The = this;
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(ac);
		}

		private void configsLb_DoubleClick(object sender, EventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void addAppToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			var items = AspireAppsToolStripItems;
			var item = sender as ToolStripDropDownItem;

			if (appsMenuItemsModified || item.DropDownItems.Count == 1)
			{
				item.DropDownItems.Clear();
				item.DropDownItems.AddRange(items);
				appsMenuItemsModified = false;
			}
		}

		private void configsLb_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if ( !enableCheck )
				e.NewValue = e.CurrentValue;
			enableCheck = false;
		} bool enableCheck;


		#region IAppInfoClient Members

		public void PropertyChanged(int pid, AppMgrProxy.Property property, int value, string stringValue=null)
		{
			AppInfo ai;
			if (appsByPid.TryGetValue(pid, out ai))
			{
				//Log.WriteLine("{0}({1},{2},{3})", ai.Name, property, value, stringValue);
				switch (property)
				{
					case AppMgrProxy.Property.Arguments: ai.Args = stringValue; break;
					case AppMgrProxy.Property.Console: ai.Console = (AppMgrDoc.ConsoleState)value; break;
					case AppMgrProxy.Property.Id: ai.Id = value; break;
					//case AppMgrProxy.Property.Pid: ai.Pid = value; break;
					case AppMgrProxy.Property.Log: ai.Log = value != 0; break;
					case AppMgrProxy.Property.MaxHeapSize: ai.MaxHeapSize = (uint)value; break;
					case AppMgrProxy.Property.Name: ai.Name = stringValue; break;
					case AppMgrProxy.Property.Priority: ai.Priority = (float)value * 1e-7f; break;
					case AppMgrProxy.Property.Restart: ai.Restart = value != 0; break;
					case AppMgrProxy.Property.StackSize: ai.StackSize = (uint)value; break;
					case AppMgrProxy.Property.Uid: ai.Uid.Parse(stringValue); break;
					case AppMgrProxy.Property.UnresponsiveRestartPeriod: ai.UnresponsiveRestartPeriod = value; break;
					default:
						Log.WriteLine("PropertyChanged: Unhandled property {0}", property);
						break;
				}
				System.Threading.Thread.Sleep(10); // Try to avoid the popup
			}
		}

		public void ProcessStatusChanged(string name, int id, int pid, AppInfo.AppState state, AppInfo.ChangeReason reason)
		{
			if (id == -1)
			{
				if ( reason != AppInfo.ChangeReason.Stopped && aspireState != AspireState.Up )
					aspireState = AspireState.UpArmed;
				return;
			}

			AppInfo appInfo;
			if (!appsByPid.TryGetValue(pid, out appInfo))
			{
				if (mActiveConfig == null)
					SetActiveConfig();
				foreach ( var ai in mActiveConfig.Apps )
					if (ai.Name == name && ai.Id == id)
					{
						if ( state != AppInfo.AppState.Inactive )
							ai.Pid = pid;
						appsByPid.Add(pid, ai);
						appInfo = ai;
						break;
					}
			}
			else if (state == AppInfo.AppState.Inactive)
			{
				appInfo.Pid = 0;
				appsByPid.Remove(pid);
			}
			if ( appInfo != null )
				appInfo.State = state;
		}

		public void SyncProperty(AppInfo appInfo, AppMgrProxy.Property property, int value, string stringValue=null)
		{
			mAppMgrProxy.SetProperty(appInfo, property, value, stringValue);
		}

		#endregion

		private void dataGridView1_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1) return;
			activeRowIndex = e.RowIndex;
			dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight);
		} int activeRowIndex;

		private void dataGridView1_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1) return;
			dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromKnownColor(KnownColor.Window);
		}

		private void dataGridView1_SelectionChanged(object sender, EventArgs e)
		{
			if ( dataGridView1.SelectedRows.Count > 0 )
				selectedRow = dataGridView1.SelectedRows[0];
		} DataGridViewRow selectedRow;

		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex == -1) return;
			if ( dataGridView1.Rows[e.RowIndex].Tag != null && e.ColumnIndex != 3)
				BrowseProperties(dataGridView1.Rows[e.RowIndex].Tag);
		}

		private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			if (mActiveConfig == null) return;
			for (int r = 0; r < e.RowCount; r++)
			{
				DataGridViewRow row = dataGridView1.Rows[r + e.RowIndex];
				if (row.Cells.Count > 0)
				{
					var name = row.Cells[0].Value as string;
					foreach (var ai in mActiveConfig.Apps)
					{
						if (ai.Name == name && ai.Id == (int)row.Cells[4].Value)
						{
							row.Tag = ai;
							break;
						}
					}
				}
			}
		}

		private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex != 3 || e.RowIndex == -1) return;
			if (dataGridView1.Rows[e.RowIndex].Tag == null) return;

			var ai = dataGridView1.Rows[e.RowIndex].Tag as AppInfo;
			mAppMgrProxy.SetProperty(ai,AppMgrProxy.Property.Priority,(int)(ai.Priority*1e7f));
		}

		private void dataGridView1_RowContextMenuStripNeeded(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
		{
			var ai = dataGridView1.Rows[e.RowIndex].Tag as AppInfo;
			if (ai.Pid > 0)
			{
				e.ContextMenuStrip = contextMenuStrip;
				contextMenuStrip.Items[(int)AppTool.Restart].Visible = true;
				contextMenuStrip.Items[(int)AppTool.Start].Visible = false;
				contextMenuStrip.Items[(int)AppTool.Stop].Visible = true;
				contextMenuStrip.Items[(int)AppTool.Restart].Enabled = ai.Restart ? false : true;
				contextMenuRow = e.RowIndex;
			}
			else if (e.ContextMenuStrip != null)
			{
				e.ContextMenuStrip.Items[(int)AppTool.Restart].Visible = false;
				e.ContextMenuStrip.Items[(int)AppTool.Start].Visible = true;
				e.ContextMenuStrip.Items[(int)AppTool.Stop].Visible = false;
			}
		} int contextMenuRow;

		private void ApplicationManager_FormClosing(object sender, FormClosingEventArgs e)
		{
			foreach (var item in configsLb.Items)
				if (item is AppConfig)
					mFileMgr.Save(item as AppConfig);
			//if ( mActiveConfig != null )
			//	mActiveConfig.Save(aspireDataDirectory);
		}

		private void heartbeatToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
		{
			if (mActiveConfig != null)
			{
				mActiveConfig.Heartbeat = heartbeatToolStripMenuItem.Checked;
				mAppMgrProxy.Heartbeat = mActiveConfig.Heartbeat;
			}
		}
	}

	#region AppMgrDoc
	public class AppMgrDoc : StudioDocument
	{
		const string category = "AppMgr";
		public enum ConsoleState { None, Stowed, Open, Inherit };

		public AppMgrDoc()
			: base(ItemType.Monitor)
		{
		}

		[Category(category), DefaultValue(null)]
		public string ActiveConfig { get { return mActiveConfig; } set { mActiveConfig = value; Notify(); } } string mActiveConfig;

		public override void Initialize()
		{
			mFileMgr.Document = this;
		}

		[Category(category), XmlIgnore]
		public string NewConfig
		{
			get { return newConfig; }
			set
			{
				newConfig = value;
				(View as ApplicationManager).ActivateConfig(newConfig);

			}
		} string newConfig;

		[Category(category), DefaultValue(null)]
		public string AspireBinDirectory { get { return mAspireBinDirectory; } set { mAspireBinDirectory = value; IsDirty = true; } } string mAspireBinDirectory;

		[Category(category), DefaultValue(null)]
		public string AspireDataDirectory { get { return mAspireDataDirectory; } set { mAspireDataDirectory = value; IsDirty = true; } } string mAspireDataDirectory;

		[Category(category), XmlAttribute("console"), DefaultValue(ConsoleState.Stowed), Description("The console state of the Aspire Infrastructure.")]
		public ConsoleState Console { get { return mConsole; } set { mConsole = value; IsDirty = true; } } ConsoleState mConsole = ConsoleState.Stowed;

		[Category(category), XmlAttribute("debug"), DefaultValue(1), Description("The debug level to start ApplicationManager with")]
		public int DebugLevel { get { return mDebugLevel; } set { mDebugLevel = value; IsDirty = true; } } int mDebugLevel = 1;

		[Category(category),DefaultValue(FileMgrType.Target),
		 Description("The class name of the file manager. Changing requires a restart")]
		public FileMgrType FileManagerClass {
			get { return mFileMgrType; }
			set
			{
				mFileMgrType = value;
				switch (mFileMgrType)
				{
					case FileMgrType.Host: mFileMgr = new HostFileMgr(); break;
					case FileMgrType.Target: mFileMgr = new TargetFileMgr(); break;
				}
				IsDirty = true;
			}
		} FileMgrType mFileMgrType = FileMgrType.Target;

		[Browsable(false),XmlIgnore]
		public FileMgr FileMgr
		{
			get
			{
				//if (mFileMgr == null)
				//	FileManagerClass = FileManagerClass;
				return mFileMgr;
			}
		} FileMgr mFileMgr = new TargetFileMgr();

		[Category(category), XmlAttribute("leaveAppMgrRunningOnExit"), DefaultValue(false), Description("If ApplicationManager was started locally, should it be left running when the UI exits ?")]
		public bool LeaveAppMgrRunningOnExit { get { return mLeaveAppMgrRunningOnExit; } set { mLeaveAppMgrRunningOnExit = value; IsDirty = true; } } bool mLeaveAppMgrRunningOnExit;

		[Category(category), XmlAttribute("startAspireConsole"), DefaultValue(ConsoleState.Stowed), Description("The console state of the Aspire Infrastructure.")]
		public ConsoleState StartAspireConsole { get { return mStartAspireConsole; } set { mStartAspireConsole = value; IsDirty = true; } } ConsoleState mStartAspireConsole = ConsoleState.Stowed;

		public override StudioDocumentView NewView(string name)
		{
			var view = new ApplicationManager() { Name = name };
			return view;
		}
	}
	#endregion
}
