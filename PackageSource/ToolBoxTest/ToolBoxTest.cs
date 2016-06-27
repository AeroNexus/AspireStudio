//----------------------------------------------------------------------------
// File    : ToolBoxTest.cs
// Date    : 17/09/2004
// Author  : Aju George
// Email   : aju_george_2002@yahoo.co.in ; george.aju@gmail.com
//----------------------------------------------------------------------------
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using Silver.UI;

namespace ToolBoxTest
{
	public class TestForm : Form
	{
		#region Static Helper Methods
		public static Stream GetResource(string resourceName)
		{
			Stream stream = null;
			try
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				stream = asm.GetManifestResourceStream("ToolBoxTest.Resources."+resourceName);
			}
			catch(Exception)
			{
				stream = null;
			}
			return stream;
		}

		public static Image GetImage(string resouceName)
		{
			Image  image  = null;
			Stream stream = null;
			try
			{
				stream = GetResource("Images."+resouceName);
				image  = Image.FromStream(stream);
			}
			catch(Exception)
			{
				image = null;
			}
			return image;
		}
		#endregion //Static Helper Methods

		#region Attributes
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ToolWindow toolWindow;
		private System.Windows.Forms.PropertyGrid propGrid;


		private System.Windows.Forms.GroupBox gbItems;
		private System.Windows.Forms.GroupBox gbTabs;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown tabImgUpDn;
		private System.Windows.Forms.Button addTab;
		private System.Windows.Forms.Button delSelTab;
		private System.Windows.Forms.Button editSelTab;
		private System.Windows.Forms.TextBox tabName;
		private System.Windows.Forms.Button addTabEx;
		private System.Windows.Forms.NumericUpDown tabIndex;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox selTabEnabled;
		private System.Windows.Forms.NumericUpDown itemTabIndex;
		private System.Windows.Forms.NumericUpDown itemImgUpDn;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox itemName;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown itemIndex;
		private System.Windows.Forms.CheckBox selItemEnabled;
		private System.Windows.Forms.Button addItem;
		private System.Windows.Forms.Button addItemEx;
		private System.Windows.Forms.Button editSelItem;
		private System.Windows.Forms.Button delSelItem;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox itemAllowDrag;
        private System.Windows.Forms.GroupBox gbDrag;
        private System.Windows.Forms.Label dragContainer;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.GroupBox grpPropType;
        private System.Windows.Forms.ComboBox cmbPropType;
        private System.Windows.Forms.Label lblPropType;

		private ToolBoxItem     newItem;
		private ToolBoxTab      newTab;
		private ToolBoxItem     editedItem;
        private ToolBoxTab      editedTab;
        private bool            dirSet;

		#endregion //Attributes

		#region Constructor
		public TestForm()
		{
			InitializeComponent();
            InitializeInternal();
		}

		#endregion //Constructor

		#region Overrides
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
            propGrid.SelectedObject = toolWindow.ToolBox;
		}

		protected override void OnMove(EventArgs e)
		{
			Point ptLocation  = Point.Empty;;
			base.OnMove (e);

			if(null != toolWindow)
			{
				ptLocation.Y        = this.Top;
				ptLocation.X        = this.Right;
				toolWindow.Location = ptLocation;
			}
		}

        protected override void OnSizeChanged(EventArgs e)
        {
            Point propLoc;

            base.OnSizeChanged (e);

            propLoc   = grpPropType.Location;
            propLoc.Y = grpPropType.Bottom;

            propLoc = this.PointToScreen(propLoc);

            this.propGrid.Height = (this.Bottom- propLoc.Y )-16;
        }

		#endregion //Overrides

		#region Private Methods

        private void InitializeInternal()
        {

            this.cmbPropType.SelectedIndex = 0;
            this.CreateToolWindow();

            this.itemTabIndex.ValueChanged          += new EventHandler(this.itemTabIndex_ValueChanged);
            this.selItemEnabled.CheckedChanged      += new EventHandler(this.selItemEnabled_CheckedChanged);
            this.addItem.Click                      += new EventHandler(this.addItem_Click);
            this.addItemEx.Click                    += new EventHandler(this.addItemEx_Click);
            this.editSelItem.Click                  += new EventHandler(this.editSelItem_Click);
            this.delSelItem.Click                   += new EventHandler(this.delSelItem_Click);
            this.itemAllowDrag.CheckedChanged       += new EventHandler(this.itemAllowDrag_CheckedChanged);
            this.btnLoad.Click                      += new EventHandler(this.btnLoad_Click);
            this.btnSave.Click                      += new EventHandler(this.btnSave_Click);
            this.selTabEnabled.CheckedChanged       += new EventHandler(this.selTabEnabled_CheckedChanged);
            this.addTabEx.Click                     += new EventHandler(this.addTabEx_Click);
            this.addTab.Click                       += new EventHandler(this.addTab_Click);
            this.delSelTab.Click                    += new EventHandler(this.delSelTab_Click);
            this.editSelTab.Click                   += new EventHandler(this.editSelTab_Click);
            this.dragContainer.DragDrop             += new DragEventHandler(this.dragContainer_DragDrop);
            this.dragContainer.DragOver             += new DragEventHandler(this.dragContainer_DragOver);
            this.cmbPropType.SelectedIndexChanged   += new EventHandler(this.cmbPropType_SelectedIndexChanged);

        }

		private void CreateToolWindow()
		{
			Point       ptLocation  = this.Location;
            TreeView    treeView    = null;


            if(null == toolWindow)
            {
                toolWindow = new ToolWindow();

                toolWindow.ToolBox.SetImageList(GetImage("ToolBox_Small.bmp"),new Size(16,16),Color.Magenta,true);
                toolWindow.ToolBox.SetImageList(GetImage("ToolBox_Large.bmp"),new Size(32,32),Color.Magenta,false);
            
                toolWindow.ToolBox.RenameFinished       += new RenameFinishedHandler(ToolBox_RenameFinished);
                toolWindow.ToolBox.TabSelectionChanged  += new TabSelectionChangedHandler(ToolBox_TabSelectionChanged);
                toolWindow.ToolBox.ItemSelectionChanged += new ItemSelectionChangedHandler(ToolBox_ItemSelectionChanged);
                toolWindow.ToolBox.TabMouseUp           += new TabMouseEventHandler(ToolBox_TabMouseUp);
                toolWindow.ToolBox.ItemMouseUp          += new ItemMouseEventHandler(ToolBox_ItemMouseUp);
                toolWindow.ToolBox.OnDeSerializeObject  += new XmlSerializerHandler(ToolBox_OnDeSerializeObject);
                toolWindow.ToolBox.OnSerializeObject    += new XmlSerializerHandler(ToolBox_OnSerializeObject);
                toolWindow.ToolBox.ItemKeyPress         += new ItemKeyPressEventHandler(ToolBox_ItemKeyPress);
            }

            toolWindow.ToolBox.DeleteAllTabs(false);

			toolWindow.ToolBox.AddTab("Tab 1",-1);
			toolWindow.ToolBox.AddTab("Tab 2 (TreeView)",-1);
			toolWindow.ToolBox.AddTab("Tab 3",-1);
			toolWindow.ToolBox.AddTab("Tab 4",-1);
            toolWindow.ToolBox.AddTab("Tab 5 (TreeView)",-1);

            treeView = new TreeView();
            treeView.BorderStyle = BorderStyle.None;

            FillTreeView(treeView);
            toolWindow.ToolBox[1].Control = treeView;

            treeView = new TreeView();
            treeView.BorderStyle = BorderStyle.None;
            treeView.Dock = DockStyle.Fill;

            FillTreeView(treeView);

            toolWindow.ToolBox[4].Control = treeView;

            toolWindow.ToolBox[0].View = ViewMode.LargeIcons;
			toolWindow.ToolBox[0].AddItem("Pointer",0,0,false,null);
			toolWindow.ToolBox[0].AddItem("Tab Item 1",1,1,true,1);
			toolWindow.ToolBox[0].AddItem("Tab Item 2",2,2,true,2);

            //toolWindow.ToolBox[0].AddItem("Pointer",0,0,false,null);
            toolWindow.ToolBox[2].AddItem("Pointer",0,0,false,null);
            toolWindow.ToolBox[2].AddItem("Tab Item 1",3,3,true,3);
            toolWindow.ToolBox[2].AddItem("Tab Item 2",4,4,true,4);

			toolWindow.ToolBox[3].AddItem("Pointer",0,0,false,null);
            toolWindow.ToolBox[3].AddItem("Tab Item 1",5,5,true,5);
            toolWindow.ToolBox[3].AddItem("Tab Item 2",6,6,true,6);

			toolWindow.ToolBox[0].Deletable = false;
			toolWindow.ToolBox[0].Renamable = false;
			toolWindow.ToolBox[0].Movable   = false;

			toolWindow.ToolBox[0][0].Renamable = false;
			//toolWindow.ToolBox[1][0].Renamable = false;
			toolWindow.ToolBox[2][0].Renamable = false;
			toolWindow.ToolBox[3][0].Renamable = false;

			toolWindow.ToolBox[0][0].Deletable = false;
			//toolWindow.ToolBox[1][0].Deletable = false;
			toolWindow.ToolBox[2][0].Deletable = false;
			toolWindow.ToolBox[3][0].Deletable = false;

			toolWindow.ToolBox[0][0].Movable = false;
			//toolWindow.ToolBox[1][0].Movable = false;
			toolWindow.ToolBox[2][0].Movable = false;
			toolWindow.ToolBox[3][0].Movable = false;

			toolWindow.ToolBox[0].Selected = true;

			tabIndex.Value      = toolWindow.ToolBox.SelectedTabIndex;
			itemTabIndex.Value  = toolWindow.ToolBox.SelectedTabIndex;

			ptLocation.X        = this.Right;
			toolWindow.Visible  = true;
			toolWindow.Height   = Height;
			toolWindow.Location = ptLocation;

            this.tabImgUpDn.Minimum  = -1;
            this.itemImgUpDn.Minimum = -1;
            this.tabImgUpDn.Maximum  = toolWindow.ToolBox.SmallImageList.Images.Count-1;
            this.itemImgUpDn.Maximum = toolWindow.ToolBox.SmallImageList.Images.Count-1;

		}

        private void FillTreeView(TreeView t)
        {
            t.ImageList = toolWindow.ToolBox.SmallImageList;

            t.Nodes.Add("Musical Instruments");

            /*t.Nodes[0].ImageIndex = 1;
            t.Nodes[0].SelectedImageIndex = 1;*/

            t.Nodes[0].Nodes.Add("Piano");

            /*t.Nodes[0].Nodes[0].ImageIndex = 2;
            t.Nodes[0].Nodes[0].SelectedImageIndex = 2;*/

            t.Nodes[0].Nodes[0].Nodes.Add("Acoustic grand piano");
            t.Nodes[0].Nodes[0].Nodes.Add("Bright acoustic piano");
            t.Nodes[0].Nodes[0].Nodes.Add("Electric grand piano");
            t.Nodes[0].Nodes[0].Nodes.Add("Honky-tonk piano");
            t.Nodes[0].Nodes[0].Nodes.Add("Rhodes piano");
            t.Nodes[0].Nodes[0].Nodes.Add("Chorused piano");
            t.Nodes[0].Nodes[0].Nodes.Add("Harpsichord");
            t.Nodes[0].Nodes[0].Nodes.Add("Clavinet");

            t.Nodes[0].Nodes.Add("Chromatic Percussion");

            t.Nodes[0].Nodes[1].Nodes.Add("Celesta");
            t.Nodes[0].Nodes[1].Nodes.Add("Glockenspiel");
            t.Nodes[0].Nodes[1].Nodes.Add("Music box");
            t.Nodes[0].Nodes[1].Nodes.Add("Vibraphone");
            t.Nodes[0].Nodes[1].Nodes.Add("Marimba");
            t.Nodes[0].Nodes[1].Nodes.Add("Xylophone");
            t.Nodes[0].Nodes[1].Nodes.Add("Tubular bells");
            t.Nodes[0].Nodes[1].Nodes.Add("Dulcimer");

            t.Nodes[0].Nodes.Add("Organ");
            t.Nodes[0].Nodes[2].Nodes.Add("Hammond organ");
            t.Nodes[0].Nodes[2].Nodes.Add("Percussive organ");
            t.Nodes[0].Nodes[2].Nodes.Add("Rock organ");
            t.Nodes[0].Nodes[2].Nodes.Add("Church organ");
            t.Nodes[0].Nodes[2].Nodes.Add("Reed organ");
            t.Nodes[0].Nodes[2].Nodes.Add("Accordion");
            t.Nodes[0].Nodes[2].Nodes.Add("Harmonica");
            t.Nodes[0].Nodes[2].Nodes.Add("Tango accordion");

            t.Nodes[0].Nodes.Add("Guitar");
            t.Nodes[0].Nodes[3].Nodes.Add("Acoustic guitar (nylon)");
            t.Nodes[0].Nodes[3].Nodes.Add("Acoustic guitar (steel)");
            t.Nodes[0].Nodes[3].Nodes.Add("Electric guitar (jazz)");
            t.Nodes[0].Nodes[3].Nodes.Add("Electric guitar (clean)");
            t.Nodes[0].Nodes[3].Nodes.Add("Electric guitar (muted)");
            t.Nodes[0].Nodes[3].Nodes.Add("Overdriven guitar");
            t.Nodes[0].Nodes[3].Nodes.Add("Distortion guitar");
            t.Nodes[0].Nodes[3].Nodes.Add("Guitar harmonics");

            t.Nodes[0].Nodes.Add("Bass");
            t.Nodes[0].Nodes[4].Nodes.Add("Acoustic bass");
            t.Nodes[0].Nodes[4].Nodes.Add("Electric bass (finger)");
            t.Nodes[0].Nodes[4].Nodes.Add("Electric bass (pick)");
            t.Nodes[0].Nodes[4].Nodes.Add("Fretless bass");
            t.Nodes[0].Nodes[4].Nodes.Add("Slap bass 1");
            t.Nodes[0].Nodes[4].Nodes.Add("Slap bass 2");
            t.Nodes[0].Nodes[4].Nodes.Add("Synth bass 1");
            t.Nodes[0].Nodes[4].Nodes.Add("Synth bass 2");

            t.Nodes[0].Nodes.Add("Strings");
            t.Nodes[0].Nodes[5].Nodes.Add("Violin");
            t.Nodes[0].Nodes[5].Nodes.Add("Viola");
            t.Nodes[0].Nodes[5].Nodes.Add("Cello");
            t.Nodes[0].Nodes[5].Nodes.Add("Contrabass");
            t.Nodes[0].Nodes[5].Nodes.Add("Tremolo strings");
            t.Nodes[0].Nodes[5].Nodes.Add("Pizzicato strings");
            t.Nodes[0].Nodes[5].Nodes.Add("Orchestral harp");
            t.Nodes[0].Nodes[5].Nodes.Add("Timpani");

            t.Nodes[0].Nodes.Add("Ensemble");
            t.Nodes[0].Nodes[6].Nodes.Add("String ensemble 1");
            t.Nodes[0].Nodes[6].Nodes.Add("String ensemble 2");
            t.Nodes[0].Nodes[6].Nodes.Add("Synth. strings 1");
            t.Nodes[0].Nodes[6].Nodes.Add("Synth. strings 2");
            t.Nodes[0].Nodes[6].Nodes.Add("Choir Aahs");
            t.Nodes[0].Nodes[6].Nodes.Add("Voice Oohs");
            t.Nodes[0].Nodes[6].Nodes.Add("Synth voice");
            t.Nodes[0].Nodes[6].Nodes.Add("Orchestra hit");

            t.Nodes[0].Nodes.Add("Brass");
            t.Nodes[0].Nodes[7].Nodes.Add("Trumpet");
            t.Nodes[0].Nodes[7].Nodes.Add("Trombone");
            t.Nodes[0].Nodes[7].Nodes.Add("Tuba");
            t.Nodes[0].Nodes[7].Nodes.Add("Muted trumpet");
            t.Nodes[0].Nodes[7].Nodes.Add("French horn");
            t.Nodes[0].Nodes[7].Nodes.Add("Brass section");
            t.Nodes[0].Nodes[7].Nodes.Add("Synth. brass 1");
            t.Nodes[0].Nodes[7].Nodes.Add("Synth. brass 2");

            t.Nodes[0].Nodes.Add("Reed");
            t.Nodes[0].Nodes[8].Nodes.Add("Soprano sax");
            t.Nodes[0].Nodes[8].Nodes.Add("Alto sax");
            t.Nodes[0].Nodes[8].Nodes.Add("Tenor sax");
            t.Nodes[0].Nodes[8].Nodes.Add("Baritone sax");
            t.Nodes[0].Nodes[8].Nodes.Add("Oboe");
            t.Nodes[0].Nodes[8].Nodes.Add("English horn");
            t.Nodes[0].Nodes[8].Nodes.Add("Bassoon");
            t.Nodes[0].Nodes[8].Nodes.Add("Clarinet");

            t.Nodes[0].Nodes.Add("Pipe");
            t.Nodes[0].Nodes[9].Nodes.Add("Piccolo");
            t.Nodes[0].Nodes[9].Nodes.Add("Flute");
            t.Nodes[0].Nodes[9].Nodes.Add("Recorder");
            t.Nodes[0].Nodes[9].Nodes.Add("Pan flute");
            t.Nodes[0].Nodes[9].Nodes.Add("Bottle blow");
            t.Nodes[0].Nodes[9].Nodes.Add("Shakuhachi");
            t.Nodes[0].Nodes[9].Nodes.Add("Whistle");
            t.Nodes[0].Nodes[9].Nodes.Add("Ocarina");

            t.Nodes[0].Nodes.Add("Synth Lead");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 1 (square)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 2 (sawtooth)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 3 (calliope lead)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 4 (chiff lead)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 5 (charang)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 6 (voice)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 7 (fifths)");
            t.Nodes[0].Nodes[10].Nodes.Add("Lead 8 (brass + lead)");

            t.Nodes[0].Nodes.Add("Synth Pad");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 1 (new age)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 2 (warm)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 3 (polysynth)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 4 (choir)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 5 (bowed)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 6 (metallic)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 7 (halo)");
            t.Nodes[0].Nodes[11].Nodes.Add("Pad 8 (sweep)");

            t.Nodes[0].Nodes.Add("Sound Effects");
            t.Nodes[0].Nodes[12].Nodes.Add("Guitar fret noise");
            t.Nodes[0].Nodes[12].Nodes.Add("Breath noise");
            t.Nodes[0].Nodes[12].Nodes.Add("Seashore");
            t.Nodes[0].Nodes[12].Nodes.Add("Bird tweet");
            t.Nodes[0].Nodes[12].Nodes.Add("Telephone ring");
            t.Nodes[0].Nodes[12].Nodes.Add("Helicopter");
            t.Nodes[0].Nodes[12].Nodes.Add("Applause");
            t.Nodes[0].Nodes[12].Nodes.Add("Gunshot");

        }

        private ContextMenu CreateContextMenu(bool forTab, ToolBoxItem item)
        {

            ToolBoxTab  theTab   = null;
            ContextMenu cm       = new ContextMenu();

            cm.MenuItems.Add(forTab ? "ToolBox Tab Menu" : "ToolBox Item Menu");
            cm.MenuItems.Add(item.Caption);

            cm.MenuItems.Add("-");
            cm.MenuItems.Add("Rename " + (forTab ? "Tab" : "Item"));         //3 - Rename
            cm.MenuItems.Add("Move "   + (forTab ? "Tab" : "Item") + " up"); //4 - Move up
            cm.MenuItems.Add("Move "   + (forTab ? "Tab" : "Item") +" down");//5 - Move down
            cm.MenuItems.Add("Delete " + (forTab ? "Tab" : "Item"));         //6 - Delete

            cm.MenuItems[0].Enabled = false;

            if(forTab)
            {
                theTab   = (ToolBoxTab)item;
            }
            else
            {
                theTab    = (ToolBoxTab)item.ParentItem;
            }

            cm.MenuItems[3].Enabled = item.Renamable;
            cm.MenuItems[4].Enabled = item.CanMoveUp;
            cm.MenuItems[5].Enabled = item.CanMoveDown;
            cm.MenuItems[6].Enabled = item.Deletable;

            if(!forTab)
            {
                cm.MenuItems[3].Click += new EventHandler(OnItemRenameClick);
                cm.MenuItems[4].Click += new EventHandler(OnItemMoveUpClick);
                cm.MenuItems[5].Click += new EventHandler(OnItemMoveDownClick);
                cm.MenuItems[6].Click += new EventHandler(OnItemDeleteClick);

            }
            else
            {
                cm.MenuItems[3].Click += new EventHandler(OnTabRenameClick);
                cm.MenuItems[4].Click += new EventHandler(OnTabMoveUpClick);
                cm.MenuItems[5].Click += new EventHandler(OnTabMoveDownClick);
                cm.MenuItems[6].Click += new EventHandler(OnTabDeleteClick);
            }

            if(!forTab)
            {
                editedItem = item;
            }
            else
            {
            }

            if(null != theTab)
            {
                MenuItem[] subMenus = new MenuItem[3];

                subMenus[0] = new MenuItem("List");
                subMenus[1] = new MenuItem("Small Icons");
                subMenus[2] = new MenuItem("Large Icons");


                cm.MenuItems.Add("-");
                cm.MenuItems.Add("View",subMenus);

                switch(theTab.View)
                {
                    case ViewMode.List:
                        subMenus[0].Checked = true;
                        break;
                    case ViewMode.SmallIcons:
                        subMenus[1].Checked = true;
                        break;
                    case ViewMode.LargeIcons:
                        subMenus[2].Checked = true;
                        break;
                }

                subMenus[0].Click += new EventHandler(OnItemViewModeChange);
                subMenus[1].Click += new EventHandler(OnItemViewModeChange);
                subMenus[2].Click += new EventHandler(OnItemViewModeChange);

            }

            editedTab = theTab;

            cm.MenuItems.Add("-");

            cm.MenuItems.Add("Sample Menu Item 1");
            cm.MenuItems.Add("Sample Menu Item 2");


            return cm;
        }

        private string GetFileBrowseFilter()
        {
            return "Supported Files|*.xml;*.config|Xml files (*.xml)|*.xml|Configuration Files (*.config)|*.config|All Files (*.*)|*.*";
        }

		#endregion //Private Methods

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.propGrid = new System.Windows.Forms.PropertyGrid();
            this.gbItems = new System.Windows.Forms.GroupBox();
            this.itemTabIndex = new System.Windows.Forms.NumericUpDown();
            this.itemImgUpDn = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.itemName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.itemIndex = new System.Windows.Forms.NumericUpDown();
            this.selItemEnabled = new System.Windows.Forms.CheckBox();
            this.addItem = new System.Windows.Forms.Button();
            this.addItemEx = new System.Windows.Forms.Button();
            this.editSelItem = new System.Windows.Forms.Button();
            this.delSelItem = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.itemAllowDrag = new System.Windows.Forms.CheckBox();
            this.gbTabs = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.selTabEnabled = new System.Windows.Forms.CheckBox();
            this.addTabEx = new System.Windows.Forms.Button();
            this.addTab = new System.Windows.Forms.Button();
            this.tabImgUpDn = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.tabName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.delSelTab = new System.Windows.Forms.Button();
            this.editSelTab = new System.Windows.Forms.Button();
            this.tabIndex = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.gbDrag = new System.Windows.Forms.GroupBox();
            this.dragContainer = new System.Windows.Forms.Label();
            this.grpPropType = new System.Windows.Forms.GroupBox();
            this.lblPropType = new System.Windows.Forms.Label();
            this.cmbPropType = new System.Windows.Forms.ComboBox();
            this.gbItems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.itemTabIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemImgUpDn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemIndex)).BeginInit();
            this.gbTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabImgUpDn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabIndex)).BeginInit();
            this.gbDrag.SuspendLayout();
            this.grpPropType.SuspendLayout();
            this.SuspendLayout();
            // 
            // propGrid
            // 
            this.propGrid.CommandsVisibleIfAvailable = true;
            this.propGrid.HelpVisible = false;
            this.propGrid.LargeButtons = false;
            this.propGrid.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.propGrid.Location = new System.Drawing.Point(0, 288);
            this.propGrid.Name = "propGrid";
            this.propGrid.Size = new System.Drawing.Size(456, 176);
            this.propGrid.TabIndex = 4;
            this.propGrid.Text = "PropertyGrid";
            this.propGrid.ToolbarVisible = false;
            this.propGrid.ViewBackColor = System.Drawing.SystemColors.Window;
            this.propGrid.ViewForeColor = System.Drawing.SystemColors.WindowText;
            // 
            // gbItems
            // 
            this.gbItems.Controls.Add(this.itemTabIndex);
            this.gbItems.Controls.Add(this.itemImgUpDn);
            this.gbItems.Controls.Add(this.label4);
            this.gbItems.Controls.Add(this.label5);
            this.gbItems.Controls.Add(this.itemName);
            this.gbItems.Controls.Add(this.label6);
            this.gbItems.Controls.Add(this.itemIndex);
            this.gbItems.Controls.Add(this.selItemEnabled);
            this.gbItems.Controls.Add(this.addItem);
            this.gbItems.Controls.Add(this.addItemEx);
            this.gbItems.Controls.Add(this.editSelItem);
            this.gbItems.Controls.Add(this.delSelItem);
            this.gbItems.Controls.Add(this.label7);
            this.gbItems.Controls.Add(this.itemAllowDrag);
            this.gbItems.Location = new System.Drawing.Point(232, 0);
            this.gbItems.Name = "gbItems";
            this.gbItems.Size = new System.Drawing.Size(225, 184);
            this.gbItems.TabIndex = 1;
            this.gbItems.TabStop = false;
            this.gbItems.Text = "Tab Items";
            // 
            // itemTabIndex
            // 
            this.itemTabIndex.Location = new System.Drawing.Point(48, 24);
            this.itemTabIndex.Maximum = new System.Decimal(new int[] {
                                                                         1024,
                                                                         0,
                                                                         0,
                                                                         0});
            this.itemTabIndex.Name = "itemTabIndex";
            this.itemTabIndex.Size = new System.Drawing.Size(88, 20);
            this.itemTabIndex.TabIndex = 1;
            // 
            // itemImgUpDn
            // 
            this.itemImgUpDn.Location = new System.Drawing.Point(48, 56);
            this.itemImgUpDn.Minimum = new System.Decimal(new int[] {
                                                                        1,
                                                                        0,
                                                                        0,
                                                                        -2147483648});
            this.itemImgUpDn.Name = "itemImgUpDn";
            this.itemImgUpDn.Size = new System.Drawing.Size(88, 20);
            this.itemImgUpDn.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(16, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "Tab";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(8, 56);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 16);
            this.label5.TabIndex = 3;
            this.label5.Text = "Image";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // itemName
            // 
            this.itemName.Location = new System.Drawing.Point(48, 88);
            this.itemName.Name = "itemName";
            this.itemName.Size = new System.Drawing.Size(88, 20);
            this.itemName.TabIndex = 7;
            this.itemName.Text = "Item";
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(8, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 16);
            this.label6.TabIndex = 6;
            this.label6.Text = "Name";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // itemIndex
            // 
            this.itemIndex.Location = new System.Drawing.Point(48, 120);
            this.itemIndex.Maximum = new System.Decimal(new int[] {
                                                                      1024,
                                                                      0,
                                                                      0,
                                                                      0});
            this.itemIndex.Name = "itemIndex";
            this.itemIndex.Size = new System.Drawing.Size(88, 20);
            this.itemIndex.TabIndex = 10;
            // 
            // selItemEnabled
            // 
            this.selItemEnabled.Checked = true;
            this.selItemEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.selItemEnabled.Location = new System.Drawing.Point(16, 152);
            this.selItemEnabled.Name = "selItemEnabled";
            this.selItemEnabled.Size = new System.Drawing.Size(88, 24);
            this.selItemEnabled.TabIndex = 12;
            this.selItemEnabled.Text = "Enabled";
            this.selItemEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // addItem
            // 
            this.addItem.Location = new System.Drawing.Point(144, 24);
            this.addItem.Name = "addItem";
            this.addItem.Size = new System.Drawing.Size(72, 24);
            this.addItem.TabIndex = 2;
            this.addItem.Text = "Add New";
            // 
            // addItemEx
            // 
            this.addItemEx.Location = new System.Drawing.Point(144, 56);
            this.addItemEx.Name = "addItemEx";
            this.addItemEx.Size = new System.Drawing.Size(72, 24);
            this.addItemEx.TabIndex = 5;
            this.addItemEx.Text = "Add + Edit";
            // 
            // editSelItem
            // 
            this.editSelItem.Location = new System.Drawing.Point(144, 88);
            this.editSelItem.Name = "editSelItem";
            this.editSelItem.Size = new System.Drawing.Size(72, 24);
            this.editSelItem.TabIndex = 8;
            this.editSelItem.Text = "Rename";
            // 
            // delSelItem
            // 
            this.delSelItem.Location = new System.Drawing.Point(144, 120);
            this.delSelItem.Name = "delSelItem";
            this.delSelItem.Size = new System.Drawing.Size(72, 24);
            this.delSelItem.TabIndex = 11;
            this.delSelItem.Text = "Delete";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 120);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(32, 16);
            this.label7.TabIndex = 9;
            this.label7.Text = "Item";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // itemAllowDrag
            // 
            this.itemAllowDrag.Checked = true;
            this.itemAllowDrag.CheckState = System.Windows.Forms.CheckState.Checked;
            this.itemAllowDrag.Location = new System.Drawing.Point(120, 152);
            this.itemAllowDrag.Name = "itemAllowDrag";
            this.itemAllowDrag.Size = new System.Drawing.Size(88, 24);
            this.itemAllowDrag.TabIndex = 13;
            this.itemAllowDrag.Text = "Allow Drag";
            this.itemAllowDrag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // gbTabs
            // 
            this.gbTabs.Controls.Add(this.btnLoad);
            this.gbTabs.Controls.Add(this.btnSave);
            this.gbTabs.Controls.Add(this.selTabEnabled);
            this.gbTabs.Controls.Add(this.addTabEx);
            this.gbTabs.Controls.Add(this.addTab);
            this.gbTabs.Controls.Add(this.tabImgUpDn);
            this.gbTabs.Controls.Add(this.label2);
            this.gbTabs.Controls.Add(this.tabName);
            this.gbTabs.Controls.Add(this.label1);
            this.gbTabs.Controls.Add(this.delSelTab);
            this.gbTabs.Controls.Add(this.editSelTab);
            this.gbTabs.Controls.Add(this.tabIndex);
            this.gbTabs.Controls.Add(this.label3);
            this.gbTabs.Location = new System.Drawing.Point(0, 0);
            this.gbTabs.Name = "gbTabs";
            this.gbTabs.Size = new System.Drawing.Size(225, 184);
            this.gbTabs.TabIndex = 0;
            this.gbTabs.TabStop = false;
            this.gbTabs.Text = "Tabs";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(48, 152);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(72, 23);
            this.btnLoad.TabIndex = 11;
            this.btnLoad.Text = "Load...";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(144, 152);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(72, 23);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "Save...";
            // 
            // selTabEnabled
            // 
            this.selTabEnabled.Checked = true;
            this.selTabEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.selTabEnabled.Location = new System.Drawing.Point(48, 120);
            this.selTabEnabled.Name = "selTabEnabled";
            this.selTabEnabled.Size = new System.Drawing.Size(88, 16);
            this.selTabEnabled.TabIndex = 9;
            this.selTabEnabled.Text = "Enabled";
            this.selTabEnabled.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // addTabEx
            // 
            this.addTabEx.Location = new System.Drawing.Point(144, 56);
            this.addTabEx.Name = "addTabEx";
            this.addTabEx.Size = new System.Drawing.Size(72, 24);
            this.addTabEx.TabIndex = 5;
            this.addTabEx.Text = "Add + Edit";
            // 
            // addTab
            // 
            this.addTab.Location = new System.Drawing.Point(144, 24);
            this.addTab.Name = "addTab";
            this.addTab.Size = new System.Drawing.Size(72, 24);
            this.addTab.TabIndex = 2;
            this.addTab.Text = "Add New";
            // 
            // tabImgUpDn
            // 
            this.tabImgUpDn.Location = new System.Drawing.Point(48, 56);
            this.tabImgUpDn.Minimum = new System.Decimal(new int[] {
                                                                       1,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
            this.tabImgUpDn.Name = "tabImgUpDn";
            this.tabImgUpDn.Size = new System.Drawing.Size(88, 20);
            this.tabImgUpDn.TabIndex = 4;
            this.tabImgUpDn.Value = new System.Decimal(new int[] {
                                                                     1,
                                                                     0,
                                                                     0,
                                                                     -2147483648});
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Image";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabName
            // 
            this.tabName.Location = new System.Drawing.Point(48, 24);
            this.tabName.Name = "tabName";
            this.tabName.Size = new System.Drawing.Size(88, 20);
            this.tabName.TabIndex = 1;
            this.tabName.Text = "Tab";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // delSelTab
            // 
            this.delSelTab.Location = new System.Drawing.Point(144, 120);
            this.delSelTab.Name = "delSelTab";
            this.delSelTab.Size = new System.Drawing.Size(72, 24);
            this.delSelTab.TabIndex = 10;
            this.delSelTab.Text = "Delete";
            // 
            // editSelTab
            // 
            this.editSelTab.Location = new System.Drawing.Point(144, 88);
            this.editSelTab.Name = "editSelTab";
            this.editSelTab.Size = new System.Drawing.Size(72, 24);
            this.editSelTab.TabIndex = 8;
            this.editSelTab.Text = "Rename";
            // 
            // tabIndex
            // 
            this.tabIndex.Location = new System.Drawing.Point(48, 88);
            this.tabIndex.Name = "tabIndex";
            this.tabIndex.Size = new System.Drawing.Size(88, 20);
            this.tabIndex.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Tab";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gbDrag
            // 
            this.gbDrag.Controls.Add(this.dragContainer);
            this.gbDrag.Location = new System.Drawing.Point(0, 186);
            this.gbDrag.Name = "gbDrag";
            this.gbDrag.Size = new System.Drawing.Size(456, 54);
            this.gbDrag.TabIndex = 2;
            this.gbDrag.TabStop = false;
            // 
            // dragContainer
            // 
            this.dragContainer.AllowDrop = true;
            this.dragContainer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.dragContainer.ForeColor = System.Drawing.Color.Blue;
            this.dragContainer.Location = new System.Drawing.Point(8, 16);
            this.dragContainer.Name = "dragContainer";
            this.dragContainer.Size = new System.Drawing.Size(440, 32);
            this.dragContainer.TabIndex = 0;
            this.dragContainer.Text = "Drag Items Here";
            this.dragContainer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpPropType
            // 
            this.grpPropType.Controls.Add(this.lblPropType);
            this.grpPropType.Controls.Add(this.cmbPropType);
            this.grpPropType.Location = new System.Drawing.Point(0, 240);
            this.grpPropType.Name = "grpPropType";
            this.grpPropType.Size = new System.Drawing.Size(456, 38);
            this.grpPropType.TabIndex = 3;
            this.grpPropType.TabStop = false;
            // 
            // lblPropType
            // 
            this.lblPropType.Location = new System.Drawing.Point(128, 16);
            this.lblPropType.Name = "lblPropType";
            this.lblPropType.Size = new System.Drawing.Size(80, 14);
            this.lblPropType.TabIndex = 0;
            this.lblPropType.Text = "Property Type";
            // 
            // cmbPropType
            // 
            this.cmbPropType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPropType.Items.AddRange(new object[] {
                                                             "ToolBox",
                                                             "Selected Tab",
                                                             "Selected Tab Item"});
            this.cmbPropType.Location = new System.Drawing.Point(224, 10);
            this.cmbPropType.Name = "cmbPropType";
            this.cmbPropType.Size = new System.Drawing.Size(216, 21);
            this.cmbPropType.TabIndex = 1;
            // 
            // TestForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(464, 469);
            this.Controls.Add(this.grpPropType);
            this.Controls.Add(this.gbDrag);
            this.Controls.Add(this.propGrid);
            this.Controls.Add(this.gbTabs);
            this.Controls.Add(this.gbItems);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(472, 760);
            this.MinimumSize = new System.Drawing.Size(472, 448);
            this.Name = "TestForm";
            this.Text = "ToolBox Test Application";
            this.gbItems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.itemTabIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemImgUpDn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.itemIndex)).EndInit();
            this.gbTabs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tabImgUpDn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tabIndex)).EndInit();
            this.gbDrag.ResumeLayout(false);
            this.grpPropType.ResumeLayout(false);
            this.ResumeLayout(false);

        }
		#endregion //Windows Form Designer generated code

		#region Mapped Events

		private void addTab_Click(object sender, System.EventArgs e)
		{
			toolWindow.ToolBox.AddTab(tabName.Text,(int)tabImgUpDn.Value);
		}

		private void addTabEx_Click(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = new ToolBoxTab(tabName.Text,(int)tabImgUpDn.Value);

			newTab = tab;
			toolWindow.ToolBox.AddTab(tab);
			tab.Rename();

		}

		private void editSelTab_Click(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)tabIndex.Value];

			if(null != tab)
			{
				tab.Rename();
			}
		}

		private void delSelTab_Click(object sender, System.EventArgs e)
		{
			toolWindow.ToolBox.DeleteTab((int)tabIndex.Value,true);
		}

		private void selTabEnabled_CheckedChanged(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)tabIndex.Value];
		
			if(null != tab)
			{
				tab.Enabled = selTabEnabled.Checked;
			}

		}

		private void addItem_Click(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)itemTabIndex.Value];

			if(null != tab)
			{
				tab.AddItem(itemName.Text,(int)itemImgUpDn.Value,itemAllowDrag.Checked,"Hello");
			}
		}

		private void addItemEx_Click(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)itemTabIndex.Value];
			int index;

			if(null != tab)
			{
				index   = tab.AddItem(itemName.Text,(int)itemImgUpDn.Value,itemAllowDrag.Checked,new Rectangle(10,10,100,100));
				newItem = tab[index];
			}

			if(null != newItem)
			{
				newItem.Rename();
			}
		}

		private void editSelItem_Click(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)itemTabIndex.Value];

			try
			{
				newItem = null;
				if(null != tab)
				{
					tab[(int)itemIndex.Value].Rename();
				}
			}
			catch(Exception)
			{
			}
		}

		private void delSelItem_Click(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)itemTabIndex.Value];
		
			try
			{
				if(null != tab)
				{
					tab.DeleteItem((int)itemIndex.Value);
				}
			}
			catch(Exception)
			{
			}

		}

        private void itemTabIndex_ValueChanged(object sender, EventArgs e)
        {
            addItemEx.Enabled = (itemTabIndex.Value == toolWindow.ToolBox.SelectedTabIndex);
        }

		private void selItemEnabled_CheckedChanged(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)itemTabIndex.Value];
		
			try
			{
				if(null != tab)
				{
					tab[(int)itemIndex.Value].Enabled = selItemEnabled.Checked;
				}
			}
			catch(Exception)
			{
			}
		}

		private void itemAllowDrag_CheckedChanged(object sender, System.EventArgs e)
		{
			ToolBoxTab tab = toolWindow.ToolBox[(int)itemTabIndex.Value];
		
			try
			{
				if(null != tab)
				{
					tab[(int)itemIndex.Value].AllowDrag = itemAllowDrag.Checked;
				}
			}
			catch(Exception)
			{
			}
		}

        private void ToolBox_RenameFinished(ToolBoxItem sender, RenameFinishedEventArgs e)
        {
            if(sender == newTab)
            {
                if(e.EscapeKeyPressed || null == e.NewCaption || 0 >= e.NewCaption.Length)
                {
                    toolWindow.ToolBox.DeleteTab(newTab);
                    newTab = null;
                }
            }
            else if(sender == newItem)
            {
                if(e.EscapeKeyPressed || null == e.NewCaption || 0 >= e.NewCaption.Length)
                {
                    toolWindow.ToolBox.SelectedTab.DeleteItem(newItem);
                    newItem = null;
                }
            }
            else
            {
                if(null == e.NewCaption || 0 >= e.NewCaption.Length)
                {
                    e.Cancel = true;
                }
            }
        }

		private void dragContainer_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(e.Data.GetDataPresent(typeof(Silver.UI.ToolBoxItem)))
			{
				e.Effect = DragDropEffects.Copy;
			}
		}

		private void dragContainer_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{
			ToolBoxItem dragItem = null;
			string strItem = "";
		
			if(e.Data.GetDataPresent(typeof(Silver.UI.ToolBoxItem)))
			{
				dragItem = e.Data.GetData(typeof(Silver.UI.ToolBoxItem)) as ToolBoxItem;

				if(null != dragItem && null != dragItem.Object)
				{
					strItem = dragItem.Object.ToString();
					MessageBox.Show(this,strItem,"Drag Drop");

                    //toolWindow.ToolBox.Focus();
				}
			}
		}

		private void ToolBox_TabSelectionChanged(ToolBoxTab sender, EventArgs e)
		{
			tabIndex.Value     = toolWindow.ToolBox.IndexOfTab(sender);
			itemTabIndex.Value = tabIndex.Value;
			tabImgUpDn.Value   = sender.SmallImageIndex;
			tabName.Text       = sender.Caption;

			selTabEnabled.Checked = sender.Enabled;
		}

        private void ToolBox_ItemSelectionChanged(ToolBoxItem sender, EventArgs e)
        {
            itemIndex.Value        = ((ToolBoxTab)sender.ParentItem).IndexOfItem(sender);
            itemImgUpDn.Value      = sender.SmallImageIndex;
            itemTabIndex.Value     = toolWindow.ToolBox.IndexOfTab((ToolBoxTab)sender.ParentItem);
            itemName.Text          = sender.Caption;
            selItemEnabled.Checked = sender.Enabled;
            itemAllowDrag.Checked  = sender.AllowDrag;

        }

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            FileDialog fd;

            try
            {
                fd = new SaveFileDialog();

                if(!dirSet)
                {
                    fd.InitialDirectory = Environment.CurrentDirectory;
                    dirSet = true;
                }

                fd.Filter     = GetFileBrowseFilter();
                fd.DefaultExt = "xml";

                if(DialogResult.OK == fd.ShowDialog(this))
                {
                    toolWindow.ToolBox.XmlSerialize(fd.FileName);
                }
            }
            catch
            {
            }
        
        }

        private void btnLoad_Click(object sender, System.EventArgs e)
        {
            FileDialog fd;

            try
            {
                fd = new OpenFileDialog();

                if(!dirSet)
                {
                    fd.InitialDirectory = Environment.CurrentDirectory;
                    dirSet = true;
                }

                fd.Filter = GetFileBrowseFilter();

                if(DialogResult.OK == fd.ShowDialog(this))
                {
                    toolWindow.ToolBox.XmlDeSerialize(fd.FileName);
                }
            }
            catch
            {
            }
        
        }

        private void ToolBox_TabMouseUp(ToolBoxTab sender, MouseEventArgs e)
        {
            Point ptPos = Point.Empty;
            ContextMenu cm = null;

            if(e.Button == MouseButtons.Right)
            {
                ptPos.X = e.X;
                ptPos.Y = e.Y;
                cm = CreateContextMenu(true,sender);
                cm.Show(toolWindow.ToolBox,ptPos);
            }

        }

        private void ToolBox_ItemMouseUp(ToolBoxItem sender, MouseEventArgs e)
        {
            Point ptPos = Point.Empty;
            ContextMenu cm = null;

            if(e.Button == MouseButtons.Right)
            {
                ptPos.X = e.X;
                ptPos.Y = e.Y;
                cm = CreateContextMenu(false,sender);
                cm.Show(toolWindow.ToolBox,ptPos);
            }

        }

        private void OnItemViewModeChange(object sender, EventArgs e)
        {
            try
            {
                switch(((MenuItem)sender).Index)
                {
                    case 0:
                        editedTab.View = ViewMode.List;
                        break;
                    case 1:
                        editedTab.View = ViewMode.SmallIcons;
                        break;
                    case 2:
                        editedTab.View = ViewMode.LargeIcons;
                        break;
                }
            }
            catch
            {
            }
        }

        private void ToolBox_OnDeSerializeObject(ToolBoxItem sender, XmlSerializationEventArgs e)
        {
            int        objData = 10;
            XmlElement element = null;

            try
            {
                element = (XmlElement)e.Node.SelectSingleNode("Object");
                objData = Convert.ToInt32(element.InnerText);
            }
            catch
            {
            }

            e.Object = objData;

        }

        private void ToolBox_OnSerializeObject(ToolBoxItem sender, XmlSerializationEventArgs e)
        {
            XmlElement element;

            element = e.Node.OwnerDocument.CreateElement("Object");

            if(null != e.Object)
            {
                element.AppendChild(e.Node.OwnerDocument.CreateTextNode(e.Object.ToString()));
            }

            e.Node.AppendChild(element);

        }

        private void cmbPropType_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                switch(cmbPropType.SelectedIndex)
                {
                    case 0:
                        propGrid.SelectedObject = toolWindow.ToolBox;
                        break;
                    case 1:
                        propGrid.SelectedObject = toolWindow.ToolBox.SelectedTab;
                        break;
                    case 2:
                        propGrid.SelectedObject = toolWindow.ToolBox.SelectedTab.SelectedItem;
                        break;
                }
            }
            catch
            {
                propGrid.SelectedObject = toolWindow.ToolBox;
            }
        }

        private void ToolBox_ItemKeyPress(ToolBoxItem sender, KeyPressEventArgs e)
        {
            string strItem = "";

            if(13 == e.KeyChar && null != sender && null != sender.Object)
            {
                strItem = sender.Object.ToString();
                MessageBox.Show(this,strItem,"Enter Key");
                toolWindow.ToolBox.Focus();
            }
        }


		#endregion //Mapped Events

		#region Context Menu Handlers
		private void OnItemRenameClick(object sender, EventArgs e)
		{
			editedItem.Rename();
		}

		private void OnItemMoveUpClick(object sender, EventArgs e)
		{
			ToolBoxTab tab = editedItem.ParentItem as ToolBoxTab;
			tab.MoveItemUp(editedItem);
		}

		private void OnItemMoveDownClick(object sender, EventArgs e)
		{
			ToolBoxTab tab = editedItem.ParentItem as ToolBoxTab;
			tab.MoveItemDown(editedItem);
		}

		private void OnItemDeleteClick(object sender, EventArgs e)
		{
			ToolBoxTab tab = editedItem.ParentItem as ToolBoxTab;
			tab.DeleteItem(editedItem);
		}

		private void OnTabRenameClick(object sender, EventArgs e)
		{
			editedTab.Rename();
		}

		private void OnTabMoveUpClick(object sender, EventArgs e)
		{
			toolWindow.ToolBox.MoveTabUp(editedTab);
		}

		private void OnTabMoveDownClick(object sender, EventArgs e)
		{
			toolWindow.ToolBox.MoveTabDown(editedTab);
		}

		private void OnTabDeleteClick(object sender, EventArgs e)
		{
			toolWindow.ToolBox.DeleteTab(editedTab,true);
		}
		#endregion //Context Menu Handlers

		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
            try
            {
                Application.Run(new TestForm());
            }
            catch
            {
                MessageBox.Show("Sorry,\nSomething went wrong!\nSorry that I got you into this mess.\nMay be I should stop doing this kinda coding.", "Whoops!@", MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
		}
        #endregion //Main
    }
}
//----------------------------------------------------------------------------
