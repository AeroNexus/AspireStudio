using System.Windows.Forms;

using WeifenLuo.WinFormsUI.Docking;

using Aspire.Studio.Dialogs;
using Aspire.Utilities;

namespace Aspire.Studio.DockedViews
{
    public partial class TaskListView : ToolWindow
    {
        public TaskListView()
        {
            InitializeComponent();
			descriptionColumn.Width = -2;
        }

		public TaskList TaskList
		{
			get { return mTaskList; }
			set
			{
				mTaskList = value;

				foreach (var task in mTaskList.Tasks)
				{
					var item = listView.Items.Add(task.Name);
					item.SubItems.Add(string.Empty);
					item.SubItems.Add(task.Description);
					item.Tag = task;
					task.Tag = item;
				}
			}
		} TaskList mTaskList;

		private void newTaskToolStripMenuItem_Click(object sender, System.EventArgs e)
		{
			var task = mTaskList.NewTask();
			task.Description = task.Name;
			var item = listView.Items.Add(task.Name);
			item.SubItems.Add(string.Empty);
			item.SubItems.Add(task.Description);
			item.Tag = task;
			task.Tag = item;
		}

		private void listView_ItemActivate(object sender, System.EventArgs e)
		{
			var item = listView.SelectedItems[0];
			var editor = new TaskEdit();
			(item.Tag as TaskList.Task).CopyTo(editor.Task);
			if ( editor.ShowDialog() == DialogResult.OK )
			{
				editor.Task.CopyTo(item.Tag as TaskList.Task);
				var task = item.Tag as TaskList.Task;
				item.Text = task.Name;
				item.SubItems[0].Text = task.Name;
				item.SubItems[1].Text = task.State.ToString();
				item.SubItems[2].Text = task.Description;
			}
		}

    }
}