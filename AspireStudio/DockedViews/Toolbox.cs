using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using Silver.UI;
using WeifenLuo.WinFormsUI.Docking;

using Aspire.Framework;
using Aspire.Studio.Dialogs;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
    public partial class Toolbox : ToolWindow, IToolboxService
    {
		ToolBoxConfig toolConfig;

        public Toolbox()
        {
            InitializeComponent();
			DockHandler.DockStateChanged += Toolbox_DockStateChanged;
        }

		private void AddListBoxTools()
		{
			// Used to manually fill the toolbox. Be sure to extend the tool picker
			// to allow choice of particular types and understand System.Drawing.Design.ToolboxItem 
			// Tried to derive Silver.ToolboxItem from S.D.D.ToolboxItem, but it broke reordering

			var toolPointer = new ToolboxItem()
			{
				DisplayName = "<Pointer>",
				Bitmap = new System.Drawing.Bitmap(16, 16)
			};
			AddToolboxItem(toolPointer);
			//- the control
			AddToolboxItem(new ToolboxItem(typeof(Button)));
			AddToolboxItem(new ToolboxItem(typeof(CheckBox)));
			AddToolboxItem(new ToolboxItem(typeof(ComboBox)));
			AddToolboxItem(new ToolboxItem(typeof(GroupBox)));
			AddToolboxItem(new ToolboxItem(typeof(ImageList)));
			AddToolboxItem(new ToolboxItem(typeof(Label)));
			AddToolboxItem(new ToolboxItem(typeof(ListView)));
			AddToolboxItem(new ToolboxItem(typeof(OpenFileDialog)));
			AddToolboxItem(new ToolboxItem(typeof(Panel)));
			AddToolboxItem(new ToolboxItem(typeof(ProgressBar)));
			AddToolboxItem(new ToolboxItem(typeof(RadioButton)));
			AddToolboxItem(new ToolboxItem(typeof(SplitContainer)));
			AddToolboxItem(new ToolboxItem(typeof(StatusBar)));
			AddToolboxItem(new ToolboxItem(typeof(TabControl)));
			AddToolboxItem(new ToolboxItem(typeof(TextBox)));
			AddToolboxItem(new ToolboxItem(typeof(TreeView)));
			AddToolboxItem(new ToolboxItem(typeof(ToolBar)));
			AddToolboxItem(new ToolboxItem(typeof(ToolTip)));
		}

		private void Bind()
		{
			toolConfig.Bind();

			foreach (var category in toolConfig.Categories)
			{
				var tab = toolBox1[category.Name];
				if (tab != null)
				{
					category.Tab = tab;
					tab.Object = category;
				}
			}

			foreach (var tool in toolConfig.Tools)
			{
				var tab = toolBox1[tool.CategoryName];
				if (tab != null)
				{
					var item = tab[tool.ItemName];
					if (item != null)
					{
						item.Object = tool;
						tool.Item = item;
						tool.Type = toolConfig.FindType(tool.TypeName);
						if (tool.Type != null)
						{
							//item.AssemblyName = tool.Type.Assembly.GetName();
							//item.TypeName = tool.Type.FullName;
						}
					}
				}
			}
		}

		public string FileName { get; set; }

		public bool IsDirty { get; set; }

		public ListBox ListBox { get { return listBox1; } }

		public void Save()
		{
			toolBox1.XmlSerialize(FileName+".xml");
			toolConfig.Save(FileName + ".config.xml");
			IsDirty = false;
		}

		void Toolbox_DockStateChanged(object sender, EventArgs e)
		{
			switch (DockState)
			{
				case DockState.DockBottom:
				case DockState.DockLeft:
				case DockState.DockRight:
				case DockState.DockTop:
				case DockState.Float:
					if (toolConfig == null)
					{
						toolBox1.XmlDeSerialize(FileName + ".xml");
						toolConfig = ToolBoxConfig.Load(FileName + ".config.xml");
						if (toolConfig == null)
							toolConfig = new ToolBoxConfig();
						Bind();
					}
					if (listBox1.Items.Count == 0)
						AddListBoxTools();
					break;
			}
		}

		private void toolBox1_RenameFinished(ToolBoxItem sender, RenameFinishedEventArgs e)
		{
			if (contextTab != null)
			{
				var category = new ToolBoxConfig.Category(e.NewCaption) { Tab = contextTab };
				contextTab.Object = category;
				toolConfig.Categories.Add(category);
				contextTab = null;
			}
			else if (contextItem != null)
			{
				// ??
				contextItem = null;
			}
			//Log.WriteLine("toolBox1_RenameFinished");
		}

		private void toolBox1_ItemSelectionChanged(ToolBoxItem sender, System.EventArgs e)
		{
			//Log.WriteLine("toolBox1_ItemSelectionChanged");
		}

		private void toolBox1_TabSelectionChanged(ToolBoxTab sender, System.EventArgs e)
		{
			//Log.WriteLine("toolBox1_TabSelectionChanged");
		}

		private void toolBox1_TabMouseUp(ToolBoxTab sender, MouseEventArgs e)
		{
			contextTab = sender;
			//Log.WriteLine("toolBox1_TabMouseUp");
		} ToolBoxTab contextTab;

		private void toolBox1_ItemMouseUp(ToolBoxItem sender, MouseEventArgs e)
		{
			contextItem = sender;
			//Log.WriteLine("toolBox1_ItemMouseUp");
		} ToolBoxItem contextItem;

		private void toolBox1_OnDeSerializeObject(ToolBoxItem sender, XmlSerializationEventArgs e)
		{
			//Log.WriteLine("toolBox1_OnDeSerializeObject");
		}

		private void toolBox1_OnSerializeObject(ToolBoxItem sender, XmlSerializationEventArgs e)
		{
			//Log.WriteLine("toolBox1_OnSerializeObject");
		}

		private void toolBox1_ItemKeyPress(ToolBoxItem sender, KeyPressEventArgs e)
		{
			//Log.WriteLine("toolBox1_ItemKeyPress");
		}

		private void addItemsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var dlg = new ToolboxEditor() { ToolConfig = toolConfig };
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var newConfig = dlg.ToolConfig;

				var tab = toolBox1["General"];
				var category = tab.Object as ToolBoxConfig.Category;
				if (tab != null)
				{
					tab.DeleteAllItems();

					foreach (var assy in newConfig.Assemblies)
					{
						toolConfig.AddOrReplaceAssembly(assy);
						foreach (var typeInfo in assy.Types)
						{
							int i = tab.AddItem(typeInfo.Name);
							var item = tab[i];
							//item.TypeName = typeInfo.Type.FullName;
							var tool = new ToolBoxConfig.ToolInfo(typeInfo.Name)
							{
								CategoryName = category.Name,
								Item = item,
								ItemName = item.Caption,
								TypeName = typeInfo.Name,
								Type = typeInfo.Type
							};

							item.Object = tool;

							toolConfig.Tools.Add(tool);
						}
					}

					IsDirty = true;
				}
			}
		}

		private void addCategoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			toolBox1.AddTab("<category>", -1);
			IsDirty = true;
		}

		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (contextTab != null)
				contextTab.Rename();
			else if (contextItem != null)
				contextItem.Rename();
			IsDirty = true;
		}

		private void toolBox1_MouseUp(object sender, MouseEventArgs e)
		{
			//Log.WriteLine("toolBox1_MouseUp tab:{0} item:{1}", contextTab, contextItem);
		}

		private void toolBox1_DragDropFinished(ToolBoxItem sender, DragDropEffects e)
		{
			//Log.WriteLine("toolBox1_DragDropFinished {0}",e);
		}

		private void toolBox1_OnBeginDragDrop(ToolBoxItem sender, PreDragDropEventArgs e)
		{
			//var tbi = new ToolboxItem(typeof(System.Windows.Forms.Label));

			//Log.WriteLine("toolBox1_OnBeginDragDrop({0})",(e.DragObject as ToolBoxConfig.ToolInfo).TypeName);
		}


		#region IToolboxService Members

		public void AddCreator(ToolboxItemCreatorCallback creator, string format, IDesignerHost host)
		{
		}

		public void AddCreator(ToolboxItemCreatorCallback creator, string format)
		{
		}

		public void AddLinkedToolboxItem(ToolboxItem toolboxItem, string category, IDesignerHost host)
		{
		}

		public void AddLinkedToolboxItem(ToolboxItem toolboxItem, IDesignerHost host)
		{
		}

		public void AddToolboxItem(ToolboxItem toolboxItem, string category)
		{
			//- we have no category 
			AddToolboxItem(toolboxItem);
		}

		public void AddToolboxItem(ToolboxItem toolboxItem)
		{
			listBox1.Items.Add(toolboxItem);
		}

		public CategoryNameCollection CategoryNames
		{
			get { return null; }
		}

		public ToolboxItem DeserializeToolboxItem(object serializedObject, IDesignerHost host)
		{
			return (serializedObject as DataObject).GetData(typeof(ToolboxItem)) as ToolboxItem;
		}

		public ToolboxItem DeserializeToolboxItem(object serializedObject)
		{
			return GetSelectedToolboxItem();
		}

		public ToolboxItem GetSelectedToolboxItem(IDesignerHost host)
		{
			return GetSelectedToolboxItem();
		}

		public ToolboxItem GetSelectedToolboxItem()
		{
			if (null == listBox1.SelectedItem)
				return null;

			var tbItem = listBox1.SelectedItem as ToolboxItem;
			if (tbItem.DisplayName.ToUpper().Contains("POINTER"))
				return null;

			return tbItem;
		}

		//-  Get all the tools in a category
		public ToolboxItemCollection GetToolboxItems(string category, IDesignerHost host)
		{
			//- we have no category
			return GetToolboxItems();
		}

		//-  Get all the tools in a category
		public ToolboxItemCollection GetToolboxItems(string category)
		{
			//- we have no category
			return GetToolboxItems();
		}

		//- Get all of the tools. We're always using our current host though.
		public ToolboxItemCollection GetToolboxItems(IDesignerHost host)
		{
			return GetToolboxItems();
		}

		public ToolboxItemCollection GetToolboxItems()
		{
			var items = new ToolboxItem[listBox1.Items.Count];
			listBox1.Items.CopyTo(items, 0);

			return new ToolboxItemCollection(items);
		}

		public bool IsSupported(object serializedObject, ICollection filterAttributes)
		{
			return true;
		}

		public bool IsSupported(object serializedObject, IDesignerHost host)
		{
			return true;
		}

		public bool IsToolboxItem(object serializedObject, IDesignerHost host)
		{
			return IsToolboxItem(serializedObject);
		}

		public bool IsToolboxItem(object serializedObject)
		{
			//- If we can deserialize it, it's a ToolboxItem.
			if (DeserializeToolboxItem(serializedObject) != null)
				return true;

			return false;
		}

		void IToolboxService.Refresh()
		{
			listBox1.Refresh();
		}

		public void RemoveCreator(string format, System.ComponentModel.Design.IDesignerHost host)
		{
		}

		public void RemoveCreator(string format)
		{
		}

		public void RemoveToolboxItem(ToolboxItem toolboxItem, string category)
		{
			RemoveToolboxItem(toolboxItem);
		}

		public void RemoveToolboxItem(ToolboxItem toolboxItem)
		{
			listBox1.SelectedItem = null;
			listBox1.Items.Remove(toolboxItem);
		}

		public string SelectedCategory
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public void SelectedToolboxItemUsed()
		{
			listBox1.SelectedItem = null;
		}

        //- Serialize the toolboxItem necessary for the Drag&Drop
        //- We serialize a toolbox by packaging it in a DataObject
		public object SerializeToolboxItem(ToolboxItem toolboxItem)
		{
			var dataObject = new DataObject();
			dataObject.SetData(typeof(ToolboxItem), toolboxItem);
			return dataObject;
		}

		public bool SetCursor()
		{
			if (null == listBox1.SelectedItem)
				return false;

			//- <Pointer> is not a tool
			ToolboxItem tbItem = listBox1.SelectedItem as ToolboxItem;
			if (tbItem.DisplayName.ToUpper().Contains("POINTER"))
				return false;


			if (null != listBox1.SelectedItem)
			{
				Cursor.Current = Cursors.Cross;
				return true;
			}

			return false;
		}

		public void SetSelectedToolboxItem(ToolboxItem toolboxItem)
		{
			listBox1.SelectedItem = toolboxItem;
		}

		#endregion
	}

	public class ToolBoxConfig
	{
		static XmlSerializer serializer;
		List<AssemblyInfo> assemblies = new List<AssemblyInfo>();
		List<Category> categories = new List<Category>();
		List<ToolInfo> toolInfos = new List<ToolInfo>();

		[XmlElement("Category", typeof(Category))]
		public List<Category> Categories { get { return categories; } set { categories = value; } }

		[XmlElement("Assembly", typeof(AssemblyInfo))]
		public List<AssemblyInfo> Assemblies { get { return assemblies; } set { assemblies = value; } }

		[XmlElement("Tool", typeof(ToolInfo))]
		public List<ToolInfo> Tools { get { return toolInfos; } set { toolInfos = value; } }

		public bool AddAssembly(Assembly assembly, string name, string version)
		{
			foreach (var assy in assemblies)
				if (assy.Name == name) return false;

			var assyInfo = new AssemblyInfo()
			{
				Name = name,
				Assembly = assembly,
				Version = version
			};
			assyInfo.ExtractTypeInfo();
			assemblies.Add(assyInfo);
			return true;
		}

		public void AddOrReplaceAssembly(AssemblyInfo assembly)
		{
			foreach ( var assy in assemblies)
				if (assy.Name == assembly.Name)
				{
					assemblies.Remove(assy);
					break;
				}
			assemblies.Add(assembly);
		}

		public void Bind()
		{
			var assys = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assy in assemblies)
				assy.Bind(assys);
		}

		static void CheckSerializer()
		{
			if (serializer == null)
				try
				{
					serializer = new XmlSerializer(typeof(ToolBoxConfig));
				}
				catch (Exception e)
				{
					Log.ReportException(e, "ToolBoxConfig.CheckSerializer");
				}
		}

		public ToolBoxConfig Clone()
		{
			var cfg = new ToolBoxConfig();

			foreach (var assy in assemblies)
				cfg.assemblies.Add(assy.Clone());

			foreach (var category in categories)
				cfg.categories.Add(category.Clone());

			foreach (var tool in toolInfos)
				cfg.toolInfos.Add(tool.Clone());

			return cfg;
		}

		public Type FindType(string name)
		{
			foreach (var assy in assemblies)
			{
				foreach (var info in assy.Types)
					if (info.Name == name)
						return info.Type;
			}
			return null;
		}

		public static ToolBoxConfig Load(string fileName)
		{
			CheckSerializer();
			ToolBoxConfig cfg = null;

			StreamReader reader = null;
			try
			{
				reader = new StreamReader(fileName);
				cfg = serializer.Deserialize(reader) as ToolBoxConfig;
			}
			catch (Exception e)
			{
				Log.ReportException(e, "ToolBoxConfig.Load({0})", fileName);
			}
			finally
			{
				if ( reader != null )
					reader.Close();
			}
			return cfg;
		}

		public void Save(string fileName)
		{
			CheckSerializer();

			var writer = new XmlTextWriter(fileName, null);
			try
			{
				writer.Formatting = Formatting.Indented;
				serializer.Serialize(writer, this);
			}
			finally
			{
				writer.Close();
			}
		}

		public class AssemblyInfo
		{
			List<TypeInfo> mTypes = new List<TypeInfo>();

			public AssemblyInfo() { }
			public AssemblyInfo(string name) { Name = name; }

			[XmlIgnore]
			public Assembly Assembly { get; set; }

			public void Bind(Assembly[] assemblies)
			{
				if ( Assembly == null )
				{
					foreach ( var assy in assemblies )
						if ( assy.FullName.StartsWith(Name) )
						{
							Assembly = assy;
							break;
						}
					if ( Assembly != null )
					{
						var types = Assembly.GetTypes();
						foreach ( var info in mTypes )
						{
							foreach ( var type in types )
								if ( info.Name == type.Name )
								{
									info.Type = type;
									break;
								}
						}
					}
				}
			}

			public AssemblyInfo Clone()
			{
				var assy = new AssemblyInfo()
				{
					Name = Name,
					Version = Version,
					Tab = Tab
				};
				foreach (var type in mTypes)
					assy.mTypes.Add(type.Clone());
				return assy;
			}

			public void ExtractTypeInfo()
			{
				var types = Assembly.GetTypes();
				foreach ( var type in types)
					if (type.IsSubclassOf(typeof(Model)))
					{
						var typeInfo = new TypeInfo()
						{
							Name = type.Name,
							Type = type
						};
						mTypes.Add(typeInfo);
					}
			}

			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlAttribute("version")]
			public string Version { get; set; }

			[XmlIgnore]
			internal ToolBoxTab Tab { get; set; }

			public override string ToString()
			{
				return Name;
			}

			[XmlElement("Type",typeof(TypeInfo))]
			public List<TypeInfo> Types { get { return mTypes; } set { mTypes = value; } }

		}

		public class Category
		{
			public Category() { }
			public Category(string name) { Name = name; }

			public Category Clone()
			{
				var cat = new Category() { Name = Name, Tab = Tab };
				return cat;
			}

			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlIgnore]
			internal ToolBoxTab Tab { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}

		public class ToolInfo
		{
			public ToolInfo() { }
			public ToolInfo(string name) { Name = name; }

			public ToolInfo Clone()
			{
				var info = new ToolInfo()
				{
					Name = Name,
					CategoryName = CategoryName,
					TypeName = TypeName,
					ItemName = ItemName,
					Category = Category,
					Item = Item,
					Type = Type
				};
				return info;
			}

			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlAttribute("category")]
			public string CategoryName { get; set; }

			[XmlAttribute("type")]
			public string TypeName { get; set; }

			[XmlAttribute("item")]
			public string ItemName { get; set; }

			[XmlIgnore]
			public Category Category { get; set; }

			[XmlIgnore]
			public ToolBoxItem Item { get; set; }

			[XmlIgnore]
			public Type Type { get; set; }

			[XmlIgnore]
			internal ToolBoxTab Tab { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}

		public class TypeInfo
		{
			public TypeInfo Clone()
			{
				var info = new TypeInfo() { Name = Name, Type = Type };
				return info;
			}

			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlIgnore]
			public Type Type { get; set; }
		}
	}
}