using System;
using System.Windows.Forms;

using Aspire.Studio;

namespace Aspire.Studio.Dialogs
{
	public partial class TaskEdit : Form
	{
		public TaskEdit()
		{
			InitializeComponent();
			stateComboBox.Items.AddRange(Enum.GetNames(typeof(TaskList.TaskState)));
		}

		public TaskList.Task Task
		{
			get { return mTask; }
			set { mTask = value; }
		} TaskList.Task mTask = new TaskList.Task();

		private void TaskEdit_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!e.Cancel)
			{
				mTask.Name = nameTextBox.Text;
				mTask.Description = descriptionTextBox.Text;
				mTask.State = (TaskList.TaskState)Enum.Parse(typeof(TaskList.TaskState), stateComboBox.Text);
			}
		}

		private void TaskEdit_Activated(object sender, EventArgs e)
		{
			nameTextBox.Text = mTask.Name;
			descriptionTextBox.Text = mTask.Description;
			stateComboBox.Text = mTask.State.ToString();
		}
	}
}
