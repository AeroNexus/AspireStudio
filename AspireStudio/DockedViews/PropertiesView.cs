using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Studio.Dashboard;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
    public partial class PropertiesView : ToolWindow, IPropertiesView
    {
		public delegate void PropertyBrowsableHandler(object property, string name);
		public static PropertiesView The { get; private set; }

		//bool mSuppressEvents;
		string mComboBoxText;

		public PropertiesView()
        {
            InitializeComponent();
			The = this;
			mComboBox.Visible = false;

 			var des = DashboardManager.SurfaceManager.GetService(typeof(IDesignerEventService)) as IDesignerEventService;

			if (null == des) return;
			des.ActiveDesignerChanged += des_ActiveDesignerChanged;
       }

		void des_ActiveDesignerChanged(object sender, ActiveDesignerEventArgs e)
		{
			//Log.WriteLine("designer:{0}=>{1}", e.OldDesigner, e.NewDesigner);
			mComboBox.Visible = e.NewDesigner != null;
			PerformLayout();
		}

		public void Browse(object obj, string name=null)
		{
			if (mPropertyGrid.SelectedObject is INotifyPropertyChanged)
				(mPropertyGrid.SelectedObject as INotifyPropertyChanged).PropertyChanged -= PropertiesView_PropertyChanged;

			mPropertyGrid.SelectedObject = obj;

			if (obj is INotifyPropertyChanged)
				(obj as INotifyPropertyChanged).PropertyChanged += PropertiesView_PropertyChanged;

			if (obj is IPropertyCategoryInitializer)
			{
				var categories = (obj as IPropertyCategoryInitializer).GetCategoryStates();
				foreach (var entry in categories)
					SetCategory(entry.Key, entry.Value);
			}

			if (name != null) mPropertyGrid.ParentForm.Text = name + " Properties";
			else if (mPropertyGrid.ParentForm.Text[0] != 'P') mPropertyGrid.ParentForm.Text = "Properties";
		}

		void SetCategory(string categoryName, bool expanded)
		{
			GridItem root = mPropertyGrid.SelectedGridItem;
			//Get the parent
			while (root.Parent != null)
				root = root.Parent;

			if (root != null)
			{
				foreach (GridItem g in root.GridItems)
				{
					if (g.GridItemType == GridItemType.Category && g.Label == categoryName)
					{
						g.Expanded = expanded;
						break;
					}
				}
			}
		}

		void PropertiesView_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			mPropertyGrid.Refresh();
		}

		bool mHasFocus;

		public void Refresh(bool executing, bool topOfSecond)
		{
			if (!executing || (topOfSecond && !mHasFocus))
				mPropertyGrid.Refresh();
		}

		private void mPropertyGrid_Enter(object sender, EventArgs e)
		{
			mHasFocus = true;
		}

		private void mPropertyGrid_Leave(object sender, EventArgs e)
		{
			mHasFocus = false;
		}

		#region IPropertiesView Members

		public void ReloadSelectableObjects()
		{
			//mSuppressEvents = true;
			try
			{
				//- IDesignerEventService provides a global eventing mechanism for designer events. With this mechanism,
				//- an application is informed when a designer becomes active. The service provides a collection of
				//- designers and a single place where global objects, such as the Properties window, can monitor selection
				//- change events.
				var des =DashboardManager.SurfaceManager.GetService(typeof(IDesignerEventService)) as IDesignerEventService;
				if (null == des)
					return;
				var host = des.ActiveDesigner;

				var selectedObj = mPropertyGrid.SelectedObject;
				if (null == selectedObj)
					return; //- don't reload at all

				//- get the name of the control selected from the comboBox
				//- and if we are not able to get it then it's better to exit
				string sName = string.Empty;
				if (selectedObj is Form)
					sName = (selectedObj as Form).Name;
				else if (selectedObj is Control)
					sName = (selectedObj as Control).Site.Name;
				if (string.IsNullOrEmpty(sName))
					return;

				//- prepare the data for reloading the combobox (begin)
				List<object> ctrlsToAdd = new List<object>();
				string pgrdComboBox_Text = string.Empty;
				try
				{
					ComponentCollection ctrlsExisting = host.Container.Components;
					Debug.Assert(0 != ctrlsExisting.Count);

					foreach (Component comp in ctrlsExisting)
					{
						string sItemText = TranslateComponentToName(comp);
						ctrlsToAdd.Add(sItemText);
						if (sName == comp.Site.Name)
							mComboBoxText = sItemText;
					}//end_foreach
				}
				catch (Exception)
				{
					return; //- (rollback)
				}
				//- update the combobox (commit)
				mComboBox.Items.Clear();
				mComboBox.Items.AddRange(ctrlsToAdd.ToArray());
				mComboBox.Text = mComboBoxText;
			}
			finally
			{
				//mSuppressEvents = false;
			}
		}

		public object SelectedObject
		{
			get { return mPropertyGrid.SelectedObject; }
			set { Browse(value); }
		}

		public object[] SelectableObjects
		{
			get { return mSelectableObjects; }
			set { mSelectableObjects = value; }
		} object[] mSelectableObjects;

		public bool SelectableObjectsVisible { get; set; }

		private string TranslateComponentToName(Component comp)
		{
			string sType = comp.GetType().ToString();
			if (string.IsNullOrEmpty(sType))
				return string.Empty;
			if (string.IsNullOrEmpty(comp.Site.Name))
				return string.Empty;

			sType = sType.Substring(sType.LastIndexOf(".") + 1);
			return String.Format("({0}) {1}", sType, comp.Site.Name);
		}

		#endregion

		private void mPropertyGrid_SelectedObjectsChanged(object sender, EventArgs e)
		{
			ReloadSelectableObjects();
		}
	}
}