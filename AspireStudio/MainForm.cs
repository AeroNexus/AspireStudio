using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Framework;
using Aspire.Studio.Dashboard;
using Aspire.Studio.Dialogs;
using Aspire.Studio.DockedViews;
using Aspire.Studio.DocumentViews;
using Aspire.Studio.Plugin;
using Aspire.Utilities;

namespace Aspire.Studio
{
	public partial class MainForm : Form
	{
		const string DefaultDockingConfigFile = "AspireStudio.docking.xml";

		AppState mAppState;
		BlackboardView mBlackboardView;
		Clock clock;
		DashboardManager dashboardMgr;
		DateTime startDateTime;
		DeserializeDockContent mDeserializeDockContent;
		DocumentMgr documentMgr;
		Executive executive;
		ModelsView mModelsView = new ModelsView();
		OutputView mOutputView = new OutputView();
		PropertiesView mPropertiesView;
		Scenario mScenario;
		Solution mSolution;
		SolutionExplorer mSolutionExplorer = new SolutionExplorer();
		TaskList mTaskList;
		TaskListView mTaskListView = new TaskListView();
		Toolbox mToolbox = new Toolbox();

		string rootDirectory;
		bool executing;
		bool mSaveLayout = true;

		public MainForm()
		{
			mAppState = new AppState(this);
			mBlackboardView = new BlackboardView(this);

			GetVersion();

			AppState.InstallDirectory = rootDirectory;
			Aspire.Framework.ApplicationInfo.InstallDirectory = rootDirectory;
			Environment.SetEnvironmentVariable("FindComponentWithWholeAddress",
				StudioSettings.Default.FindComponentWithWholeAddress.ToString());

			InitializeComponent();

			var size = StudioSettings.Default.Size;
			if (size.Width > 100 && size.Height > 100)
				Size = size;

			InitializePlugins();

			dashboardMgr = new DashboardManager(mPropertiesView, mToolbox);
			documentMgr = new DocumentMgr(this, dockPanel);
			mPropertiesView = new PropertiesView();

			mOutputView.DockHandler.DockStateChanged += outputDockStateChanged;
			mTaskListView.DockHandler.DockStateChanged += taskListDockStateChanged;
			mBlackboardView.DockHandler.DockStateChanged += blackboardDockStateChanged;
			mModelsView.DockHandler.DockStateChanged += modelsDockStateChanged;
			mPropertiesView.DockHandler.DockStateChanged += propertiesDockStateChanged;
			mSolutionExplorer.DockHandler.DockStateChanged += solutionExplorerDockStateChanged;
			mToolbox.FileName = Path.Combine(Program.RootDirectory, StudioSettings.Default.ToolBoxFileName);
			mToolbox.DockHandler.DockStateChanged += toolboxDockStateChanged;
			
			toolStripStatusLabel1.Text = "Loading";

			mDeserializeDockContent = new DeserializeDockContent(GetContentFromPersistString);

			mBlackboardView.ObjectIsBrowsable += ObjectIsBrowsable;
			mModelsView.ObjectIsBrowsable += ObjectIsBrowsable;

			var args = Environment.GetCommandLineArgs();
			//foreach (var arg in args) Log.WriteLine(arg);
			if (args.Length > 1)
				LoadSolution(args[1], args.Length > 2 ? args[2] : null);
			else if (StudioSettings.Default.LoadLastSolution && StudioSettings.Default.LastSolutionFileName.Length > 0)
			{
				var fileName = StudioSettings.Default.LastSolutionFileName;
				if (Path.IsPathRooted(fileName))
					LoadSolution(fileName);
				else
					LoadSolution(Path.Combine(FileUtilities.TranslateSpecialPath(StudioSettings.Default.DefaultSolutionDirectory), fileName));
			}
			else
			{
				clock = new Clock();
				ConfigureTimeDisplays();
			}

			startDateTime = DateTime.Now;

			toolStripStatusLabel1.Text = mSolution == null ? "Failed to Load" : "Loaded";
			SettingsDirty = false;
			BuildNewTool();
		}


		private void CloseAllContents()
		{
			// we don't want to create another instance of tool window, set DockPanel to null
			mPropertiesView.DockPanel = null;
			mSolutionExplorer.DockPanel = null;
			mTaskListView.DockPanel = null;
			mToolbox.DockPanel = null;
			mOutputView.DockPanel = null;
			mBlackboardView.DockPanel = null;
			mModelsView.DockPanel = null;

			// Close all other document windows
			documentMgr.CloseAllDocuments();
		}

		void ConfigureTimeDisplays()
		{
			timeDisplayFormatBtn.DropDownItems.Clear();
			foreach (var formatter in clock.TimeDisplay.Formatters)
			{
				foreach (var name in formatter.Names)
				{
					var item = new ToolStripMenuItem(name) { Tag = formatter, Size = timeDisplayFormatBtn.Size };
					item.Size = new Size(item.Size.Width + tsTbTimeDisplay.Size.Width, item.Size.Height);
					item.Click += timeDisplayFormatBtnItem_Click;
					timeDisplayFormatBtn.DropDownItems.Add(item);
				}
			}
			IEnumerable<ToolStripMenuItem> sorted =
				from ToolStripMenuItem item in timeDisplayFormatBtn.DropDownItems
				orderby item.Text
				select item;
			timeDisplayFormatBtn.DropDownItems.AddRange( sorted.ToArray() );
		}

		void timeDisplayFormatBtnItem_Click(object sender, EventArgs e)
		{
			var item = sender as ToolStripMenuItem;
			clock.TimeDisplay.SetFormatter(item.Tag as ITimeFormatter, item.Text);
			UpdateDisplays();
		}

		private IDockContent GetContentFromPersistString(string persistString)
		{
			if (persistString == typeof(SolutionExplorer).ToString())
				return mSolutionExplorer;
			else if (persistString == typeof(PropertiesView).ToString())
				return mPropertiesView;
			else if (persistString == typeof(Toolbox).ToString())
				return mToolbox;
			else if (persistString == typeof(OutputView).ToString())
				return mOutputView;
			else if (persistString == typeof(TaskListView).ToString())
				return mTaskListView;
			else if (persistString == typeof(BlackboardView).ToString())
				return mBlackboardView;
			else if (persistString == typeof(ModelsView).ToString())
				return mModelsView;
			else
			{
				// StudioDocumentView overrides GetPersistString to add extra information into persistString.
				// Any DockContent may override this value to add any needed information for deserialization.

				string[] parsedStrings = persistString.Split(new char[] { ',' });
				if (parsedStrings.Length != 3)
					return null;

				var studioDocView = DocumentMgr.DocViewFactory(parsedStrings[0]);
				if (studioDocView == null) return null;

				if (parsedStrings[1] != string.Empty)
					studioDocView.FileName = parsedStrings[1];
				if (parsedStrings[2] != string.Empty)
					studioDocView.Text = studioDocView.Name = parsedStrings[2];

				DocumentMgr.Add(studioDocView);

				return studioDocView;
			}
		}

		void GetVersion()
		{
			mVersion = "1.0";
			var assys = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assy in assys)
			{
				if (assy.FullName.StartsWith("AspireStudio"))
				{
					rootDirectory = Path.GetDirectoryName(assy.Location);
					int start = assy.FullName.IndexOf("Version");
					int end = assy.FullName.IndexOf(',', start);
					mVersion = assy.FullName.Substring(start + 8, end - (start + 8));
					break;
				}
			}
		} string mVersion;

		private void LoadDockingConfig()
		{
			string configPath;
			if (mSolution != null && mSolution.ActiveProject != null)
			{
				if (mSolution.ActiveProject.DockPanelConfigFile.Length > 0)
				{
					try
					{
						configPath = Path.Combine(mSolution.DirectoryName, mSolution.ActiveProject.DockPanelConfigFile);

						if (File.Exists(configPath))
							dockPanel.LoadFromXml(configPath, mDeserializeDockContent);
						else
						{
							var srcFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), DefaultDockingConfigFile);
							File.Copy(srcFile, configPath);
							dockPanel.LoadFromXml(configPath, mDeserializeDockContent);
						}
					}
					catch (Exception ex)
					{
						Log.ReportException(ex, "MainForm.LoadDockingConfig fromDockingPanelConfigFile");
					}
				}
				else
				{
					try
					{
						var ms = new MemoryStream(Encoding.ASCII.GetBytes(mSolution.ActiveProject.DockPanelConfig));
						dockPanel.LoadFromXml(ms, mDeserializeDockContent, true);
					}
					catch (Exception ex)
					{
						Log.ReportException(ex, "MainForm.LoadDockingConfig from ActiveProject");
					}
				}
			}
			else
			{
				try
				{
					configPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), DefaultDockingConfigFile);

					if (File.Exists(configPath))
						dockPanel.LoadFromXml(configPath, mDeserializeDockContent);
					else
						Log.WriteLine("Default docking config file, '{0}', has been lost. Defaulting",
							DefaultDockingConfigFile);
				}
				catch (Exception ex)
				{
					Log.ReportException(ex, "MainForm.LoadDockingConfig from Default");
				}
			}
		}

		private void LoadScenario(Solution.Scenario slnScenario)
		{
			//Blackboard.Publish(mAppState);
			Environment.CurrentDirectory = mSolution.DirectoryName;

			if (slnScenario.FileName != null)
			{
				mScenario = Scenario.Load(
					Path.Combine(mSolution.DirectoryName, slnScenario.FileName), slnScenario.References);
				Scenario.Directory = mSolution.DirectoryName;
			}
			else
				mScenario = Scenario.Create(slnScenario.Name, slnScenario.Assembly);

			if (mScenario != null)
			{
				mScenario.Name = slnScenario.Name;
				this.Text = "Aspire Studio " + mVersion + " - " + slnScenario.Name;
				mScenario.IsDirty += mScenario_IsDirty;
			}

			clock = mScenario.Clock;
			clock.SecondsChanged += clock_SecondsChanged;

			ConfigureTimeDisplays();

			executive = mScenario.Executive;
			executive.ModeChanged += Executive_ModeChanged;
			executive.Scheduler.TimerPeriodChanged += Scheduler_TimerPeriodChanged;

			documentMgr.OnNewScenario(clock,executive);
			Scenario.Active = mScenario;

			mScenario.Discover();

			mModelsView.Populate(mScenario.Models);
			mBlackboardView.Populate();

			executive.Mode = ExecutiveMode.Reset;

			CloseAllContents();

			LoadDockingConfig();

			Blackboard.Publish(mAppState);
			mBlackboardView.SelectFirst();

			timer1.Interval = clock.StepSizeMilliSeconds;
			timer1.Enabled = true;

			if (mScenario.InitiallyRunning)
				executive.Mode = ExecutiveMode.Executing;
		}

		void mScenario_IsDirty(object sender, EventArgs e)
		{
			mSolution.IsDirty = true;
		}

		void Scheduler_TimerPeriodChanged(object sender, EventArgs e)
		{
			timer1.Interval = (Math.Abs(executive.Scheduler.TimerPeriod)+999) / 1000;
		}

		private void LoadSolution(string fileName,string scenarioName=null)
		{
			if (mSolution != null)
				mSolution.ActiveProjectChanged -= mSolution_ActiveProjectChanged;

			Log.WriteLine("Loading {0} solution", fileName);

			mSolution = Solution.Load(fileName);
			if (mSolution != null)
			{
				mSolution.ActiveProjectChanged += mSolution_ActiveProjectChanged;
				mSolutionExplorer.Populate(mSolution);
				mTaskListView.TaskList = mTaskList = mSolution.TaskList;
				mSolution.InitActiveProject(scenarioName);
				StudioSettings.Default.LastSolutionFileName = mSolution.FileName;
				SettingsDirty = true;
			}
		}

		void ObjectIsBrowsable(object sender, string name)
		{
			mPropertiesView.Browse(sender, name);
		}

		private void SaveDockingConfig()
		{
			string configPath;
			if (mSolution != null && mSolution.ActiveProject != null)
			{
				if (mSolution.ActiveProject.DockPanelConfigFile.Length > 0)
				{
					try
					{
						configPath = Path.Combine(mSolution.DirectoryName, mSolution.ActiveProject.DockPanelConfigFile);
						dockPanel.SaveAsXml(configPath);
					}
					catch (Exception ex)
					{
						Log.ReportException(ex, "MainForm.SaveDockingConfig to docking config file");
					}
				}
				else
				{
					try
					{
						var ms = new MemoryStream();
						dockPanel.SaveAsXml(ms, Encoding.ASCII, true);
						mSolution.ActiveProject.DockPanelConfig = Encoding.ASCII.GetString(ms.GetBuffer(), 0, (int)ms.Length);
					}
					catch (Exception ex)
					{
						Log.ReportException(ex, "MainForm.SaveDockingConfig to ActiveProject");
					}
				}
			}
			else
			{
				try
				{
					configPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), DefaultDockingConfigFile);
					dockPanel.SaveAsXml(configPath);
				}
				catch (Exception ex)
				{
					Log.ReportException(ex, "MainForm.SaveDockingConfig to Default");
				}
			}
		}

		private void SaveSettings()
		{
			if (SettingsDirty)
			{
				var treeExpansions = mBlackboardView.TreeExpansionsText;
				StudioSettings.Default.BlackboardExpand = treeExpansions;
				StudioSettings.Default.Save();
				SettingsDirty = false;
			}
		}

		internal Scenario Scenario { get { return mScenario; } }
		internal bool SettingsDirty { get; set; }

		private void ShowHide(ToolStripMenuItem menuItem, ToolWindow toolWindow)
		{
			menuItem.Checked = !menuItem.Checked;
			if (menuItem.Checked && toolWindow.DockState == DockState.Hidden || toolWindow.DockState == DockState.Unknown)
				toolWindow.Show(dockPanel);
			else if (!menuItem.Checked && toolWindow.DockState != DockState.Hidden)
				toolWindow.DockHandler.Hide();
		}

		internal Solution Solution { get { return mSolution; } }

		void UnloadScenario()
		{
			if (mSolution.IsDirty)
				mSolution.Save();
			timer1.Enabled = false;
			clock.SecondsChanged -= clock_SecondsChanged;
			executive.ModeChanged -= Executive_ModeChanged;
			if ( mSolution.ActiveProject != null )
				mSolution.ActiveProject.Unload();
			mScenario.Unload();
			documentMgr.CloseAllDocuments(DockState.Document);
			mModelsView.Clear();
			documentMgr.Clear();
			Blackboard.Clear();
		}

		private void UpdateDisplays(bool topOfSecond = false)
		{
			tsTbTimeDisplay.Text = clock.TimeDisplay.Display;
			if ( !topOfSecond )
				documentMgr.UpdateViews();
			mBlackboardView.Update();
			mPropertiesView.Refresh(executing, topOfSecond);
		}

		private void aboutToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			var about = new AboutBox();
			about.ShowDialog(this);
		}

		void blackboardDockStateChanged(object sender, EventArgs e)
		{
			blackboardToolStripMenuItem.Checked = mBlackboardView.DockState != DockState.Hidden;
		}

		private void blackboardToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(blackboardToolStripMenuItem, mBlackboardView);
		}

		void clock_SecondsChanged(object sender, EventArgs e)
		{
			UpdateDisplays(true);
		}

		private void closeSolutionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (mSolution == null) return;

			if (mSolution.FileName == null)
			{
				saveFileDialog1.Title = "Save Aspire Solution";
				saveFileDialog1.AddExtension = true;
				saveFileDialog1.CheckPathExists = true;
				if (mSolution.DirectoryName != null)
					saveFileDialog1.InitialDirectory = mSolution.DirectoryName;
				else
					saveFileDialog1.InitialDirectory = FileUtilities.TranslateSpecialPath(StudioSettings.Default.DefaultSolutionDirectory);
				saveFileDialog1.DefaultExt = "Asln";
				saveFileDialog1.Filter = "AspireStudio solutions (*.Asln)|*.Asln|All files (*.*)|*.*";
				saveFileDialog1.FileName = mSolution.Name;
				if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					mSolution.FileName = saveFileDialog1.FileName;
					StudioSettings.Default.LastSolutionFileName = mSolution.FileName;
					SettingsDirty = true;
				}
			}
			if (mScenario != null)
				UnloadScenario();
			mSolution.Close();
			mSolution.ActiveProjectChanged -= mSolution_ActiveProjectChanged;
			mSolution = null;
			StudioSettings.Default.LastSolutionFileName = string.Empty;
			SettingsDirty = true;
			mSolutionExplorer.Populate(mSolution);
		}

		private void dashboardToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DocumentMgr.NewDocumentView("DashboardView");
		}

		void Executive_ModeChanged(ExecutiveMode previousMode, ExecutiveMode mode)
		{
			if (previousMode == ExecutiveMode.Executing)
				executing = false;
			switch (mode)
			{
				case ExecutiveMode.Stop:
					stopVcrBtn.Enabled = false;
					playVcrBtn.Enabled = true;
					resetVcrBtn.Enabled = true;
					stepForwardVcrBtn.Enabled = true;
					stepBackVcrBtn.Enabled = clock.ElapsedSeconds > 0;
					UpdateDisplays();
					break;
				case ExecutiveMode.Executing:
					stopVcrBtn.Enabled = true;
					playVcrBtn.Enabled = false;
					resetVcrBtn.Enabled = false;
					stepForwardVcrBtn.Enabled = false;
					stepBackVcrBtn.Enabled = false;
					executing = true;
					break;
				case ExecutiveMode.Reset:
					stopVcrBtn.Enabled = false;
					playVcrBtn.Enabled = false;
					resetVcrBtn.Enabled = false;
					stepForwardVcrBtn.Enabled = false;
					stepBackVcrBtn.Enabled = false;
					UpdateDisplays();
					startDateTime = DateTime.Now;
					executive.Scheduler.Reset();
					break;
				case ExecutiveMode.Increment:
					stopVcrBtn.Enabled = false;
					playVcrBtn.Enabled = false;
					resetVcrBtn.Enabled = false;
					stepForwardVcrBtn.Enabled = false;
					stepBackVcrBtn.Enabled = true;
					UpdateDisplays();
					break;
				case ExecutiveMode.Decrement:
					stopVcrBtn.Enabled = false;
					playVcrBtn.Enabled = false;
					resetVcrBtn.Enabled = false;
					stepForwardVcrBtn.Enabled = true;
					stepBackVcrBtn.Enabled = clock.ElapsedSeconds > 0;
					UpdateDisplays();
					break;
			}
		}

		private void exitToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void fileToolBarToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			fileToolStrip.Visible = fileToolBarToolStripMenuItem.Checked = !fileToolBarToolStripMenuItem.Checked;
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (mSaveLayout)
				SaveDockingConfig();
			if (mSolution != null)
			{
				if (mSolution.FileName == null)
					closeSolutionToolStripMenuItem_Click(this, EventArgs.Empty);
				else if (mSolution.IsDirty) mSolution.Save();
			}
			SaveSettings();
			if (mToolbox.IsDirty)
				mToolbox.Save();
			ClosePlugins();
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
			if ( mSolution == null)
				LoadDockingConfig();
		}

		private void MainForm_SizeChanged(object sender, EventArgs e)
		{
			StudioSettings.Default.Size = Size;
			SettingsDirty = true;
		}

		void modelsDockStateChanged(object sender, EventArgs e)
		{
			modelsToolStripMenuItem.Checked = mModelsView.DockState != DockState.Hidden;
		}

		private void modelsToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(modelsToolStripMenuItem, mModelsView);
		}

		private void monitorToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DocumentMgr.NewDocumentView("Monitor");
		}

		private void newSolutionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mSolution = new Solution();
			mSolutionExplorer.Populate(mSolution);
			mBlackboardView.Populate();
			Blackboard.Publish(mAppState);
			//StudioSettings.Default.LastSolutionFileName = mSolution.FileName;
			//SettingsDirty = true;
		}

		private void openSolutionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Open Aspire Studion Solution";
			openFileDialog1.DefaultExt = "Asln";
			openFileDialog1.InitialDirectory = FileUtilities.TranslateSpecialPath(StudioSettings.Default.DefaultSolutionDirectory);
			openFileDialog1.FileName = string.Empty;
			openFileDialog1.Filter = "AspireStudio solutions (*.Asln)|*.Asln|All files (*.*)|*.*";
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				LoadSolution(openFileDialog1.FileName);
		}

		void outputDockStateChanged(object sender, EventArgs e)
		{
			outputToolStripMenuItem.Checked = mOutputView.DockState != DockState.Hidden;
		}

		private void outputToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(outputToolStripMenuItem, mOutputView);
		}

		private void playVcrBtn_Click(object sender, EventArgs e)
		{
			if ( executive != null )
				executive.Mode = ExecutiveMode.Executing;
		}

		private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var preferences = new Preferences();
			preferences.ShowDialog();
			SettingsDirty = true;
		}

		void propertiesDockStateChanged(object sender, EventArgs e)
		{
			propertiesToolStripMenuItem.Checked = mPropertiesView.DockState != DockState.Hidden;
		}

		private void propertiesToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(propertiesToolStripMenuItem, mPropertiesView);
		}

		private void resetVcrBtn_Click(object sender, EventArgs e)
		{
			if (executive != null)
				executive.Mode = ExecutiveMode.Reset;
		}

		void mSolution_ActiveProjectChanged(object sender, EventArgs e)
		{
			if (mScenario != null)
				UnloadScenario();
			var sln = sender as Solution;
			StudioDocument.ActiveProject = sln.ActiveProject;
			LoadScenario(sln.ActiveProject.Scenario);
			mModelsView.Executive = executive;
		}

		void solutionExplorerDockStateChanged(object sender, EventArgs e)
		{
			solutionExplorerToolStripMenuItem.Checked = mSolutionExplorer.DockState != DockState.Hidden;
		}

		private void solutionExplorerToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(solutionExplorerToolStripMenuItem, mSolutionExplorer);
		}

		private void statusBarMenuItem_Click(object sender, System.EventArgs e)
		{
			statusStrip1.Visible = StatusBarMenuItem.Checked = !StatusBarMenuItem.Checked;
		}

		private void stepBackVcrBtn_Click(object sender, EventArgs e)
		{
			if (executive != null)
				executive.Mode = ExecutiveMode.Decrement;
		}

		private void stepForwardVcrBtn_Click(object sender, EventArgs e)
		{
			if (executive != null)
				executive.Mode = ExecutiveMode.Increment;
		}

		private void stopVcrBtn_Click(object sender, EventArgs e)
		{
			if (executive != null)
				executive.Mode = ExecutiveMode.Stop;
		}

		private void stripChartToolStripMenuItem_Click(object sender, EventArgs e)
		{
			DocumentMgr.NewDocumentView("StripChart");
		}

		void taskListDockStateChanged(object sender, EventArgs e)
		{
			taskListToolStripMenuItem.Checked = mTaskListView.DockState != DockState.Hidden;
		}

		private void taskListToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(taskListToolStripMenuItem, mTaskListView);
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			if (executive != null)
				executive.Scheduler.Tick((DateTime.Now - startDateTime).TotalSeconds);
		}

		void toolboxDockStateChanged(object sender, EventArgs e)
		{
			toolboxToolStripMenuItem.Checked = mToolbox.DockState != DockState.Hidden;
		}

		private void toolboxToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			ShowHide(toolboxToolStripMenuItem, mToolbox);
		}

		private void toolStripSplitButton1_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			Log.WriteLine("toolStripSplitButton1_DropDownItemClicked {0}", e.ClickedItem.Name);
		}

		private void vcrToolBarToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			vcrToolStrip.Visible = vcrToolBarToolStripMenuItem.Checked = !vcrToolBarToolStripMenuItem.Checked;
		}

		private void apiReferenceToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(Path.Combine(rootDirectory,"ApiReference.chm"));
		}

		private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start(Path.Combine(rootDirectory, "UserGuide.mht"));
		}

		private void saveScenarioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			mScenario.Save();
		}

		private void managePluginsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var form = new AvailablePlugins();
			if (form.ShowDialog() == DialogResult.OK)
				LoadPlugins(form.SelectedPlugins);
		}
		private PluginInfoCollection mPlugins = null;

		private void pluginSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var form = new Aspire.Studio.Plugin.Settings(mPlugins);

			if (!form.IsDisposed)
				form.ShowDialog();
		}
	}
}
