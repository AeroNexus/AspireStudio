using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Framework;
using Aspire.Studio.Plugin;
using Aspire.Utilities;

namespace Aspire.Studio.DocumentViews
{
    public partial class StudioDocumentView : DockContent, ITypePlugin
    {
		protected Solution.ProjectItem.ItemType mType;

		protected StudioDocument mDocument;
		protected ObservableCollection<StudioDocument.Item> mItems = new ObservableCollection<StudioDocument.Item>();

		public StudioDocumentView()
        {
            InitializeComponent();
        }

		protected StudioDocumentView(Solution.ProjectItem.ItemType type) : this()
		{
			mType = type;
		}

		public virtual void AddingToDocManager() { }

		protected virtual bool AllowedType(Type type) { return true; }

		public virtual StudioDocument Document
		{
			get { return mDocument; }
			set
			{
				mDocument = value;
				if (value.Items == null)
					value.Items = mItems;
				else
					mItems = value.Items;

				mDocument.Subscribe();

				try
				{
					OnDocumentBind();
				}
				catch (Exception e)
				{
					Log.ReportException(e, "Binding {0}'sview {0} to its document", Name);
				}
			}
		}

		private string m_fileName = string.Empty;
		public string FileName
		{
			get	{	return m_fileName;	}
			set
			{
				if (value != string.Empty)
				{
					var reader = new StreamReader(value);

					FileInfo efInfo = new FileInfo(value);

					reader.Close();
				}

				m_fileName = value;
				this.ToolTipText = value;
			}
		}

		public static bool Find(string docName)
		{
			return false;
		}

		protected override string GetPersistString()
		{
			// Add extra information into the persist string for this document
			// so that it is available when deserialized.
			return GetType().ToString() + "," + FileName + "," + Text;
		}

		public void InhibitDrop(Type type)
		{
			mInhibitedType = type;
		} Type mInhibitedType;

		delegate void Updater(Clock clock);
		protected void UpdateNow()
		{
			this.Invoke(new Updater(UpdateDisplay),Manager.Clock);
		}

		public Solution.ProjectItem.ItemType ItemType { get { return mType; } }
 
		internal double LastDisplayed { get; set; }

		protected bool LogDrop { get; set; }

		public virtual DocumentMgr Manager { get;  set; }

		protected StudioDocument MyDocument(StudioDocument document)
		{
			document.Name = Name;
			document.View = this;
			document.Items = mItems;
			mDocument = document;
			StudioDocument.Add(document);
			DocumentMgr.Add(this);
			return document;
		}

		public new string Name
		{
			get { return base.Name;  }
			set
			{
				var prev = base.Name;
				if (prev != null) // Subsequent name changes change the title bar
					Text = value;

				base.Name = value;
			}
		}

		protected virtual void NewItem(Blackboard.Item bbItem, DragEventArgs e)
		{
		}

		void NewItems(Blackboard.ItemList items, DragEventArgs e)
		{
			foreach (var item in items)
			{
				if (!item.IsPrimitive && !item.IsArrayProxy && !item.IsArray && !item.IsEnum)
					Blackboard.ProviderVerify(item);

				if (item.Items.Count > 0)
					NewItems(item.Items, e);
				else
					NewItem(item, e);
			}
		}

		protected virtual void OnDocumentBind() { }

		public void OnFileContents(string content, string extension)
		{
			Log.WriteLine("{0}.OnFileCOntents({1},{2})", GetType().Name, content, extension);
		}

		protected virtual void OnItemDropped(Blackboard.Item bbItem, DragEventArgs e)
		{
			if ( LogDrop )
				Log.WriteLine("{0}.OnItemDropped:{1},{2},{3},{4}", GetType().Name,
					bbItem.Path, bbItem.Description, bbItem.Units, bbItem.Value);

			Blackboard.ProviderVerify(bbItem);

			lock (mItems)
			{
				if (bbItem.Items.Count > 0)
					NewItems(bbItem.Items, e);
				else
					NewItem(bbItem, e);
			}
		}

		public virtual void OnResetTime()
		{
		}

		protected virtual void OnObjectDropped(object obj, DragEventArgs e)
		{
			Log.WriteLine("{0}.OnObjectDropped:{1}", GetType().Name, obj);
		}

		// workaround of RichTextbox control's bug:
		// If load file before the control showed, all the text format will be lost
		// re-load the file after it get showed.
		private bool m_resetText = true;
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (m_resetText)
			{
				m_resetText = false;
				FileName = FileName;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged (e);
			//if (FileName == string.Empty)
			//	this.richTextBox1.Text = Text;
		}

    #region Events

    /// <summary>
    /// Event raised when the window's selected item changes.
    /// </summary>
    public event ItemSelectedEventHandler OnItemSelected;

    private object mSelectedObject = null;

    /// <summary>
    /// Call to set the selected object and raise the selected item event
    /// </summary>
    /// <param name="selectedItem">The window's currenlty-selected item.</param>
    protected void RaiseItemSelectedEvent(object selectedItem)
    {
      mSelectedObject = selectedItem;

      RaiseItemSelectedEvent();
    }

    private void SdtDocumentWindow_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
    {
      if (e.KeyCode == Keys.S && e.Control)
      {
        //SaveContents();
      }
    }

    /// <summary>
    /// Raises the OnItemSelected event
    /// <seealso cref="OnItemSelected"/>
    /// </summary>
    protected void RaiseItemSelectedEvent()
    {
      if (OnItemSelected != null)
      {
        OnItemSelected(this, new ItemSelectedEventArgs(mSelectedObject));
      }
    }

    #endregion

    public virtual void UpdateDisplay(Clock clock)
		{
		}

		public void NeedUpdate()
		{
			LastDisplayed = 0;
		}

		internal void OnDragDrop(object sender, DragEventArgs e)
		{
			var obj = e.Data.GetData(typeof(Blackboard.Item));
			if (obj is Blackboard.Item)
				OnItemDropped(obj as Blackboard.Item, e);
			else
				OnObjectDropped(obj, e);
		}

		private void StudioDoc_DragEnter(object sender, DragEventArgs e)
		{
			var item = e.Data.GetData(typeof(Blackboard.Item)) as Blackboard.Item;
			if ( item != null )
			{
				var type = item.Value.GetType();
				if (
					AllowedType(type) || mInhibitedType == null ||
					(type != mInhibitedType && !type.IsSubclassOf(mInhibitedType)) )
					e.Effect = e.AllowedEffect;
			}				
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show("This is to demostrate menu item has been successfully merged into the main form. Form Text=" + Text);
		}

		public virtual void StudioDocumentView_DoubleClick(object sender, EventArgs e)
		{
			BrowseProperties(Document);
		}

		public void BrowseProperties(object obj)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(obj);
		}
	}

}