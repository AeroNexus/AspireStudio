using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;
using Aspire.Studio.DocumentViews;

namespace Aspire.Studio
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Solution
	{
		static int count;

		public event EventHandler ActiveProjectChanged;
		public event EventHandler ProjectEdited;

		List<Project> mProjects = new List<Project>();
		static XmlSerializer serializer;

		public Solution()
		{
			mName = "Solution" + ++count;
		}

		// Name is here so it serializes first
		public string Name
		{
			get { return mName; }
			set { mName = value; IsDirty = true; }
		} string mName;

		[XmlIgnore]
		public ScenarioProject ActiveProject
		{
			get { return mActiveProject; }
			set
			{
				PreviousProject = mActiveProject;
				mActiveProject = value;
				ActiveProjectName = mActiveProject.Name;
				if (ActiveProjectChanged != null)
					ActiveProjectChanged(this, EventArgs.Empty);
				IsDirty = true;
			}
		} ScenarioProject mActiveProject;

		public string ActiveProjectName { get; set; }

		static void CheckSerializer()
		{
			if (serializer == null || DocumentMgr.DocTypesChanged)
			{
				List<Type> types = new List<Type>();
				types.AddRange(DocumentMgr.DocTypes);
				types.Add(typeof(ScenarioProject));
				types.Add(typeof(Folder));

				serializer = new XmlSerializer(typeof(Solution), types.ToArray());
				DocumentMgr.DocTypesChanged = false;
			}
		}

		public void Close()
		{
		}

		[XmlIgnore]
		public string DirectoryName { get; set; }

		[XmlIgnore]
		public string FileName
		{
			get { return mFileName; }
			set
			{
				mFileName = value;
				if ( Name.StartsWith("Solution"))
					Name = Path.GetFileNameWithoutExtension(mFileName);
				IsDirty = true;
			}
		} string mFileName;

		void Init(string fileName)
		{
			FileName = fileName;
			DirectoryName = Path.GetDirectoryName(fileName);

			foreach (var proj in mProjects)
				proj.Init(this);
			mTaskList.Initialize();
			IsDirty = false;
		}

		public void InitActiveProject(string scenarioName=null)
		{
			if ( scenarioName != null )
				SetActiveProject(scenarioName);
			else if (ActiveProjectName != null)
				SetActiveProject(ActiveProjectName);
			else if (Projects.Count > 0)
				foreach (var proj in Projects)
					if (proj is ScenarioProject)
						ActiveProject = Projects[0] as ScenarioProject;
			IsDirty = false;
		}

		void InitItems(IItemListHolder listHolder)
		{
			foreach (var item in listHolder.Items)
			{
				item.mParent = listHolder;
				item.Initialize();
				item.IsDirty = false;
				if (item is Folder)
					InitItems(item as Folder);
				if ( item is INotifyPropertyChanged )
					(item as INotifyPropertyChanged).PropertyChanged += item_PropertyChanged;
			}
		}

		void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IsDirty = true;
		}

		[XmlIgnore]
		public bool IsDirty { get; set; }

		public static Solution Load(string fileName)
		{
			CheckSerializer();
			if (!File.Exists(fileName))
			{
				Log.WriteLine("File, {0}, not found",fileName);
				return null;
			}

			var reader = new StreamReader(fileName);
			var text = reader.ReadToEnd();
			reader.BaseStream.Position = 0;
			Solution sln = null;
			try
			{
				sln = serializer.Deserialize(reader) as Solution;
			}
			catch (Exception e)
			{
				Log.ReportException(e, "Trying to load {0}", fileName);
			}
			finally
			{
				reader.Close();
			}
			if (sln != null) sln.Init(fileName);
			return sln;
		}

		public ProjectItem NewFolder(Project project)
		{
			var folder = new Folder();
			project.Items.Add(folder);
			IsDirty = true;
			return folder;
		}

		public ProjectItem NewItem(IItemListHolder listHolder)
		{
			var item = new ProjectItem();
			listHolder.Items.Add(item);
			IsDirty = true;
			return item;
		}

		public Project NewProject()
		{
			var proj = new Project() { mSolution = this };
			mProjects.Add(proj);
			IsDirty = true;
			return proj;
		}

		public Project NewScenarioProject()
		{
			var proj = new ScenarioProject() { mSolution = this };
			mProjects.Add(proj);
			IsDirty = true;
			return proj;
		}

		[XmlIgnore]
		public ScenarioProject PreviousProject { get; set; }

		[XmlElement("Project", typeof(Project))]
		public List<Project> Projects { get { return mProjects; } set { mProjects = value; } }

		public void Remove(object obj, IItemListHolder parent)
		{
			if (obj is Project)
			{
				var proj = obj as Project;
				if (proj == ActiveProject)
					ActiveProject = null;
				foreach (var item in proj.Items)
					RemoveItem(item,proj.Items);
				mProjects.Remove(proj);
				IsDirty = true;
			}
			else if (obj is Folder)
			{
				var folder = obj as Folder;
				foreach (var item in folder.Items)
					RemoveItem(item,folder.Items);
				if (parent != null) parent.Items.Remove(folder);
			}
			else if (obj is ProjectItem)
			{
				if (parent != null) parent.Items.Remove(obj as ProjectItem);
				if (obj is StudioDocument)
					DocumentMgr.Remove(obj as StudioDocument);
				IsDirty = true;
			}
		}

		void RemoveItem(ProjectItem item,ItemList list)
		{
			if (item is Folder)
			{
				var folder = item as Folder;
				foreach (var subItem in folder.Items)
					RemoveItem(subItem,folder.Items);
			}
			list.Remove(item);
			IsDirty = true;

		}
		public void Save()
		{
			if (FileName == null) return;
			CheckSerializer();

			var writer = new XmlTextWriter(FileName, null);
			try
			{
				writer.Formatting = Formatting.Indented;
				serializer.Serialize(writer, this);
			}
			catch (Exception e)
			{
				Log.ReportException(e, "Saving solution {0}", FileName);
			}
			finally
			{
				writer.Close();
				IsDirty = false;
				StudioSettings.Default.LastSolutionFileName = FileName;
			}
		}

		public bool SetActiveProject(string name)
		{
			foreach (var proj in mProjects)
				if (proj.Name == name)
				{
					ActiveProject = proj as ScenarioProject;
					return true;
				}
			Log.WriteLine("Can't find {0} project to set as active.", name);
			return false;
		}

		public TaskList TaskList
		{
			get { return mTaskList; }
			set { mTaskList = value; IsDirty = true; }
		} TaskList mTaskList = new TaskList();

		public override string ToString()
		{
			return Name;
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Project : IItemListHolder
		{
			static int count;
			ItemList mItems = new ItemList();
			internal Solution mSolution;

			public Project()
			{
				mName = "Project" + ++count;
			}

			public void AddItem(ProjectItem item)
			{
				item.mParent = this;
				Items.Add(item);
				if (mSolution.ProjectEdited != null) mSolution.ProjectEdited(this, EventArgs.Empty);
				IsDirty = true;
			}

			[XmlIgnore]
			public string DockPanelConfig
			{
				get { return mDockPanelConfig; }
				set { mDockPanelConfig = value; if (mSolution != null) mSolution.IsDirty = true; }
			} string mDockPanelConfig;

			public string DockPanelConfigFile { get { return mSolution.Name + '.' + Name + ".docking.xml"; } }

			public virtual void Init(Solution solution)
			{
				mSolution = solution;
				solution.InitItems(this);
				IsDirty = false;
			}

			[XmlIgnore]
			public bool IsDirty
			{
				get { return mIsDirty; }
				set
				{
					mIsDirty = value;
					if (value && mSolution != null)
						mSolution.IsDirty = value;
				}
			} bool mIsDirty;

			[XmlElement("Item", typeof(ProjectItem))]
			public ItemList Items { get { return mItems; } set { mItems = value; } }

			[XmlAttribute("name")]
			public virtual string Name { get { return mName; } set { mName = value; IsDirty = true; } }
			protected string mName;

			public StudioDocument StudioDocument(string name, ProjectItem.ItemType type)
			{
				foreach (var item in mItems)
					if (item.Name == name && item.Type == type)
						return item as StudioDocument;
				return null;
			}

			[XmlIgnore]
			public object Tag;

			public override string ToString()
			{
				return Name;
			}

			[XmlElement("DockPanelConfig"),DefaultValue(null)]
			public XmlCDataSection xmlDockPanelConfig
			{
				get
				{
					if (mDockPanelConfig == null) return null;
					XmlDocument doc = new XmlDocument();
					return doc.CreateCDataSection(DockPanelConfig);
				}
				set
				{
					DockPanelConfig = value.Value;
				}
			}
		}

		public class ScenarioProject : Project
		{
			public Scenario Scenario
			{
				get { return mScenario; }
				set { mScenario = value; }
			} Scenario mScenario;

			[XmlAttribute("name")]
			public override string Name
			{
				get { return mName; }
				set { mName = value; }
			}

			public override void Init(Solution solution)
			{
				if (mScenario == null)
				{
					mScenario = new Scenario() { Name = mName };
				}
				if ( mScenario.FileName == null && mScenario.Assembly == null )
					mScenario.Assembly = mName + ".dll";
				mScenario.mParent = this;
				base.Init(solution);
			}

			public void Unload()
			{
				foreach (var item in Items)
					item.Unload();
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class ProjectItem
		{
			public enum ItemType { CsFile, Folder, Dashboard, Monitor, StripChart, XmlFile }
			static int count;
			internal IItemListHolder mParent;

			public ProjectItem()
			{
				mName = "Item" + ++count;
			}

			public virtual void Initialize() { }

			[XmlIgnore, Browsable(false)]
			public bool IsDirty
			{
				get { return mIsDirty; } 
				set
				{
					mIsDirty = value;
					if (value && mParent != null)
						mParent.IsDirty = value;
				}
			} bool mIsDirty;

			[XmlAttribute("name"),Browsable(false)]
			public virtual string Name { get { return mName; } set { mName = value; IsDirty = true; } }
			protected string mName;

			[Category("ProjectItem")]
			public ItemType Type { get; set; }

			public virtual void Unload() { }
		}

		public class ItemList : List<ProjectItem>
		{
		}

		public interface IItemListHolder
		{
			ItemList Items { get; }
			bool IsDirty { set; }
		}

		public class Folder : ProjectItem, IItemListHolder
		{
			ItemList mItems = new ItemList();

			public Folder()
			{
				Type = ItemType.Folder;
				mName = "Folder" + Name.Substring(4);
			}

			[XmlElement("Item", typeof(ProjectItem))]
			public ItemList Items { get { return mItems; } set { mItems = value; } }
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Scenario
		{
			static int count;
			internal ScenarioProject mParent;

			public Scenario()
			{
				mName = "Scenario" + ++count;
			}

			void mReferences_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				if ( mParent != null )
					IsDirty = true;
				//sender as AssemblyReference;
			}

			void mScenario_IsDirty(object sender, EventArgs e)
			{
				IsDirty = true;
			}

			[XmlAttribute("name")]
			public string Name
			{
				get { return mName; }
				set { mName = value; IsDirty = true; }
			} string mName;

			[XmlAttribute("file")]
			public string FileName
			{
				get { return mFileName; }
				set { mFileName = value; IsDirty = true; }
			} string mFileName;

			[XmlAttribute("assembly")]
			public string Assembly
			{
				get { return mAssembly; }
				set { mAssembly = value; IsDirty = true; }
			} string mAssembly;

			[XmlElement("Reference", typeof(AssemblyReference))]
			public ObservableCollection<AssemblyReference> References
			{
				get { return mReferences; }
				set
				{
					mReferences = value;
					IsDirty = true;
					mReferences.CollectionChanged += mReferences_CollectionChanged;
				}
			} ObservableCollection<AssemblyReference> mReferences;

			[XmlIgnore, Browsable(false)]
			public bool IsDirty
			{
				get { return mIsDirty; }
				set
				{
					mIsDirty = value;
					if (value && mParent != null)
						mParent.IsDirty = value;
				}
			} bool mIsDirty;


			public override string ToString()
			{
				return Name;
			}
		}
	}
}
