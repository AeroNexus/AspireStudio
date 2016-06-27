using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Studio.DocumentViews;
using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
    public partial class SolutionExplorer : ToolWindow
    {
		Solution mSolution;
		Solution.Project mActiveProject;

		enum Images { Solution, OpenFolder, ClosedFolder, CsProject, ResourceFolder, IconGrid, Resource, CsFile, Form, Image,
			XmlFile, ScenarioProject, References };
		[Flags]
		enum Tool { NewProject=1, NewFolder=2, NewItem=4, Delete=8, Rename=0x10, SetAsActive=0x20, NewScenario=0x40, Add=0x80 };

        public SolutionExplorer()
        {
            InitializeComponent();
        }

		private void AddItem(Solution.ProjectItem item, TreeNode parentNode)
		{// Need to find a way to use the doc.View.Icon, not just this image list
			int index = ImageIndex(item), selectedIndex = index;
			if (index == (int)Images.ClosedFolder) selectedIndex = (int)Images.OpenFolder;
			var node = new TreeNode(item.Name, index, selectedIndex)
			{
				ContextMenuStrip = contextMenuStrip1,
				Tag = item
			};
			parentNode.Nodes.Add(node);

			if (item.Type == Solution.ProjectItem.ItemType.Folder)
			{
				var folder = item as Solution.Folder;
				foreach (var subItem in folder.Items)
					AddItem(subItem, node);
			}
		}

		private TreeNode AddProject(Solution.Project proj)
		{
			var scenarioProj = proj as Solution.ScenarioProject;

			int imageIndex = scenarioProj != null ? (int)Images.ScenarioProject : (int)Images.CsProject;
			var node = new TreeNode(proj.Name, imageIndex, imageIndex)
			{
				ContextMenuStrip = contextMenuStrip1,
				Tag = proj
			};
			proj.Tag = node;
			treeView1.Nodes.Add(node);

			PopulateProject(proj, node);
			return node;
		}

		void EnableTools(Tool toolMask,bool rename=true)
		{
			deleteToolStripMenuItem.Visible = (toolMask & Tool.Delete) == Tool.Delete;
			newProjectToolStripMenuItem.Visible = (toolMask & Tool.NewProject) == Tool.NewProject;
			newScenarioToolStripMenuItem.Visible = (toolMask & Tool.NewScenario) == Tool.NewScenario;
			newItemToolStripMenuItem.Visible = (toolMask & Tool.NewItem) == Tool.NewItem;
			setAsActiveProjectToolStripMenuItem.Visible = (toolMask & Tool.SetAsActive) == Tool.SetAsActive;
			newFolderToolStripMenuItem.Visible = (toolMask & Tool.NewFolder) == Tool.NewFolder;
			addToolStripMenuItem.Visible = (toolMask & Tool.Add) == Tool.Add;
			renameToolStripMenuItem.Visible = rename;
		}

		int ImageIndex(Solution.ProjectItem item)
		{
			switch ( item.Type)
			{
				case Solution.ProjectItem.ItemType.CsFile: return (int)Images.CsFile;
				case Solution.ProjectItem.ItemType.Dashboard: return (int)Images.Form;
				case Solution.ProjectItem.ItemType.Folder: return (int)Images.ClosedFolder;
				case Solution.ProjectItem.ItemType.Monitor: return (int)Images.Form;
				case Solution.ProjectItem.ItemType.StripChart: return (int)Images.Form;
				case Solution.ProjectItem.ItemType.XmlFile: return (int)Images.XmlFile;
				default: return (int)Images.Resource;
			}
		}

		public void Populate(Solution solution)
		{
			mSolution = solution;
			treeView1.Nodes.Clear();
			if (solution == null)
			{
				treeView1.Nodes.Add(new TreeNode("No solution loaded"));
				return;
			}

			solution.ActiveProjectChanged += solution_ActiveProjectChanged;
			solution.ProjectEdited += solution_ProjectEdited;
			var label = string.Format("Solution '{0}' ({1} projects)", mSolution.Name, mSolution.Projects.Count);
			var node = new TreeNode(label, (int)Images.Solution, (int)Images.Solution)
			{
				ContextMenuStrip = contextMenuStrip1,
				Tag = solution
			};
			treeView1.Nodes.Add(node);

			foreach (var proj in mSolution.Projects)
				node = AddProject( proj);
		}

		void PopulateProject(Solution.Project proj, TreeNode projectNode)
		{
			var scenarioProj = proj as Solution.ScenarioProject;
			if (scenarioProj != null)
			{
				var scenario = scenarioProj.Scenario;
				if (scenario == null)
				{
					scenario = new Solution.Scenario()
					{
						Name = proj.Name,
						FileName = proj.Name+".xml"
					};
					scenarioProj.Scenario = scenario;
				}
				else if (mPreviousProjectName != null)
				{
					scenario.Name = proj.Name;
					scenario.FileName = proj.Name + ".xml";
				}
				string label = "Scenario: " + scenario.Name;
				if (scenario.FileName != null)
					label += ", file=" + scenario.FileName;
				else
					label += ", assembly=" + scenario.Assembly;

				var node = new TreeNode(label, (int)Images.Resource, (int)Images.Resource)
				{
					ContextMenuStrip = contextMenuStrip1,
					Tag = scenarioProj.Scenario
				};
				projectNode.Nodes.Add(node);

				var refNode = new TreeNode("References", (int)Images.References, (int)Images.References)
				{
					//ContextMenuStrip = contextMenuStrip1,
					Tag = scenarioProj.Scenario.References
				};
				projectNode.Nodes.Add(refNode);

				if ( scenarioProj.Scenario.References != null )
					foreach ( var reference in scenarioProj.Scenario.References )
					{
						node = new TreeNode(reference.Name,(int)Images.Resource, (int)Images.Resource)
						{
							ContextMenuStrip = contextMenuStrip1,
							Tag = reference
						};
						refNode.Nodes.Add(node);
					}
			}

			foreach (var item in proj.Items)
				AddItem(item, projectNode);
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (contextMenuNode.Parent == null)
			{
				mSolution.Remove(contextMenuNode.Tag, null);
				treeView1.Nodes.Remove(contextMenuNode);
			}
			else
			{
				mSolution.Remove(contextMenuNode.Tag, contextMenuNode.Parent.Tag as Solution.IItemListHolder);
				contextMenuNode.Parent.Nodes.Remove(contextMenuNode);
			}
		}

		private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var project = contextMenuNode.Tag as Solution.Project;
			if (project == null) return;
			var item = mSolution.NewFolder(project);
			Populate(mSolution);
		}

		private void newItemToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var itemListHolder = contextMenuNode.Tag as Solution.IItemListHolder;
			if (itemListHolder == null) return;
			var item = mSolution.NewItem(itemListHolder);
			Populate(mSolution);
		}

		private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!(contextMenuNode.Tag is Solution)) return;
			var proj = mSolution.NewProject();
			AddProject(proj);
		}

		private void newScenarioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!(contextMenuNode.Tag is Solution)) return;
			var proj = mSolution.NewScenarioProject();
			AddProject(proj);
		}
		private void renameToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (contextMenuNode.Tag == mSolution)
				contextMenuNode.Text = mSolution.Name;
			contextMenuNode.BeginEdit();
		}

		void solution_ActiveProjectChanged(object sender, EventArgs e)
		{
			if (mActiveProject != null)
				(mActiveProject.Tag as TreeNode).ForeColor = Color.Black;

			mActiveProject = mSolution.ActiveProject;

			(mActiveProject.Tag as TreeNode).ForeColor = Color.Blue;
		}

		void solution_ProjectEdited(object sender, EventArgs e)
		{
			var proj = sender as Solution.Project;
			TreeNode parentNode = null;
			foreach (TreeNode node in treeView1.Nodes)
				if (node.Tag == proj)
				{
					parentNode = node;
					break;
				}
			if (parentNode != null)
			{
				parentNode.Nodes.Clear();
				PopulateProject(proj, parentNode);
			}
		}

		private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			var node = e.Node;
			switch (e.Button)
			{
				case MouseButtons.Left:
					PropertiesView.The.Browse(node.Tag);
					break;
				case MouseButtons.Right:
					contextMenuNode = e.Node;
					if (node.Tag is Solution)
						EnableTools(Tool.NewProject|Tool.NewScenario);
					else if (node.Tag is Solution.ScenarioProject)
						EnableTools(Tool.NewItem | Tool.NewFolder | Tool.Delete | Tool.SetAsActive);
					else if (node.Tag is Solution.Project)
						EnableTools(Tool.NewItem | Tool.NewFolder | Tool.Delete);
					else if (node.Tag is Solution.Folder)
						EnableTools(Tool.NewItem|Tool.NewFolder|Tool.Delete);
					else if (node.Tag is Solution.ProjectItem)
						EnableTools(Tool.Delete);
					else if (node.Tag is AssemblyReference)
						EnableTools(Tool.Delete,false);
					break;
			}
		} TreeNode contextMenuNode;

		private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Label == null || e.Label.Length == 0) return;
			var node = e.Node;
			//node.EndEdit(false);
			if (node.Tag is Solution)
			{
				mSolution.Name = e.Label;
				node.Text = string.Format("Solution '{0}' ({1} Projects)", mSolution.Name, mSolution.Projects.Count);
				Populate(mSolution);
			}
			else if (node.Tag is Solution.Project)
			{
				var proj = node.Tag as Solution.Project;
				mPreviousProjectName = proj.Name;
				proj.Name = e.Label;
				node.Text = e.Label;
				if (proj is Solution.ScenarioProject)
				{
					node.Nodes.Clear();
					PopulateProject(proj, node);
				}
				mPreviousProjectName = null;
			}
			else if (node.Tag is Solution.ProjectItem)
			{
				var item = node.Tag as Solution.ProjectItem;
				item.Name = e.Label;
				node.Text = e.Label;
			}
			else
				return;
		}

		string mPreviousProjectName;

		private void treeView1_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			if (e.Node.Tag is Solution)
			{
				e.Node.Text = mSolution.Name;
			}
		}

		private void setAsActiveProjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (contextMenuNode.Tag is Solution.ScenarioProject)
				mSolution.ActiveProject = (contextMenuNode.Tag as Solution.ScenarioProject);
		}

		private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
		{
			if (!(e.Item is TreeNode)) return;

			if (e.Button == MouseButtons.Left)
				DoDragDrop((e.Item as TreeNode).Tag, DragDropEffects.All);
		}

		private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (e.Node.Tag is Solution.ProjectItem)
				{
					var item = e.Node.Tag as Solution.ProjectItem;
					if (item is StudioDocument)
						(item as StudioDocument).Display();
				}
			}

		}
    }

}