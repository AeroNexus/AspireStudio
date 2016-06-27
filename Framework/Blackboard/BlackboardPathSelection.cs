using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Aspire.Framework
{
	/// <summary>
	///  <see cref="System.Drawing.Design.UITypeEditor"/> to display a list of <see cref="Blackboard.Item"/> paths that are currently published
	/// </summary>
	public class BlackboardSelection : UITypeEditor
	{
		#region Statics 

		private class LockedItems
		{
			public TreeNodeCollection m_DataDictionaryTreeNodes = null;
		}

		private static LockedItems m_LockedItems = new LockedItems();

		///<summary>
		///Gets and sets the DataDictionaryTreeNodes value
		///</summary>
		public static TreeNodeCollection SourceTreeNodes
		{
			get { return m_LockedItems.m_DataDictionaryTreeNodes; }
			set 
			{ 
				lock(m_LockedItems)
				{
					m_LockedItems.m_DataDictionaryTreeNodes = value;	
				}
			}
		}

		#endregion

		TreeView tree = new TreeView();

		IWindowsFormsEditorService edSvc;

		/// <summary>
		/// Default constructor
		/// </summary>
		public BlackboardSelection()
		{
			tree.BorderStyle = BorderStyle.None;
			tree.DoubleClick += new EventHandler(OnTreeDoubleClick);
		}

        /// <summary>
        /// Allow the user resize the dropdown list when selecting long paths.
        /// </summary>
        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }

		/// <summary>
		/// Gets the style of the editor
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
		{
			return System.Drawing.Design.UITypeEditorEditStyle.DropDown;
		}

		/// <summary>
		/// Displays the UI for value selection.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="provider"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object EditValue
			(System.ComponentModel.ITypeDescriptorContext 
			context, System.IServiceProvider provider, 
			object value)
		{
			using( new WaitCursor() )
			{
				// because the contents of the TreeNodeCollection referenced by SourceTreeNodes can be changed at any time
				// we need to refresh our local tree's nodes each time EditValue is called
				//Node[] nodes = new Node[ SourceTreeNodes.Count ];
				//SourceTreeNodes. .CopyTo(nodes,0);
			
				tree.Nodes.Clear();
				tree.PathSeparator = Blackboard.PathSeparator.ToString();

				foreach( TreeNode node in SourceTreeNodes )
				{
					var clonedNode = node.Clone() as TreeNode;
					tree.Nodes.Add(clonedNode);
					OnNodeAdded(tree, clonedNode);
				}
			

				TreeNode nodeToSelect = null;
				
				if ( value != null )
				{
					nodeToSelect = GetNodeForPath( tree.Nodes, value.ToString() );
					if (nodeToSelect != null)
						//tree.EnsureDisplayed(nodeToSelect);
						nodeToSelect.EnsureVisible();
				}
				tree.SelectedNode = nodeToSelect;
			
				tree.Height = 100;
			}

			// Uses the IWindowsFormsEditorService to 
			// display a drop-down UI in the Properties 
			// window.
			edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if ( edSvc != null )
			{
				edSvc.DropDownControl( tree );
				if ( tree.SelectedNode != null && tree.SelectedNode.Tag != null && tree.SelectedNode.Tag is Blackboard.Item )
				{
					return tree.SelectedNode.Tag as Blackboard.Item;
				}
				return value;
			}
			return value;
		}

		/// <summary>
		/// Called after each Node from the DataDictionaryTree is added to this TypeEditor's TreeControl
		/// </summary>
		/// <param name="tree"></param>
		/// <param name="node"></param>
		protected virtual void OnNodeAdded(TreeView tree, TreeNode node)
		{
		}

		/// <summary>
		/// Called when the tree control is double-clicked.
		/// </summary>
		protected virtual void OnTreeDoubleClick(object sender, EventArgs e)
		{
			edSvc.CloseDropDown();
		}

		private TreeNode GetNodeForPath( TreeNodeCollection nodes, string path )
		{
			if ( path == null || path.Length == 0 )
				return null;

			foreach( TreeNode node in nodes )
			{
				if ( node.FullPath == path )
					return node;
				if ( node.Nodes != null && node.Nodes.Count > 0 )
				{
					var nodeFromChildNodes = GetNodeForPath( node.Nodes, path );
					if ( nodeFromChildNodes != null )
						return nodeFromChildNodes;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// <see cref="System.Drawing.Design.UITypeEditor"/> to display a list of <see cref="Blackboard.Item"/> paths that are currently published
	/// </summary>
	public class BlackboardPathSelection : BlackboardSelection
	{
		/// <summary>
		/// Displays the UI for value selection.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="provider"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public override object EditValue( ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			object o = base.EditValue(context, provider, value);
			var item = o as Blackboard.Item;
			if (item != null)
				return item.Path;
			else
				return value;			
		}
	}

	/// <summary>
	/// Utility class used to display a wait cursor
	/// while a long operation takes place and
	/// guarantee that it will be removed on exit.
	/// 
	/// Use as follows:
	/// 
	///		using ( new WaitCursor() )
	///		{
	///			// Long running operation goes here
	///		}
	///		
	/// </summary>
	public class WaitCursor : IDisposable
	{
		private Cursor cursor;

    /// <summary>
    /// Default constructor
    /// </summary>
		public WaitCursor()
		{
			cursor = Cursor.Current;
			Cursor.Current = Cursors.WaitCursor;
		}

    /// <summary>
    /// DIspose
    /// </summary>
		public void Dispose()
		{
			Cursor.Current = cursor;
		}
	}

	/// <summary>
	/// Contains types allowed to be assigned to the property that this attribute is on.
	/// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class AllowedTypesAttribute : Attribute
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="otherAllowedTypes"></param>
		public AllowedTypesAttribute(Type type, params Type[] otherAllowedTypes)
		{
			if (type == null)
				throw new ArgumentNullException("type");


			this.types = new Type[otherAllowedTypes.Length + 1];
			types[0] = type;
			for (int i = 1; i < types.Length; ++i)
			{
				Type t = otherAllowedTypes[i - 1];
				if (t == null)
					throw new ArgumentNullException("otherAllowedTypes");
				types[i] = t;
			}
		}

		/// <summary>
		/// The types which are allowed.
		/// </summary>
		public Type[] Types
		{
			get { return types; }
		}
		private readonly Type[] types;
	}
}

