using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Aspire.Studio
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class TaskList
	{
		static int count;

		List<Task> mTasks = new List<Task>();

		public TaskList()
		{
			mName = "TaskList" + ++count;
		}

		internal void Initialize()
		{
			foreach (var task in mTasks)
				task.mTaskList = this;

			IsDirty = false;
		}

		[XmlIgnore]
		public bool IsDirty { get; set; }

		public string Name
		{
			get { return mName; }
			set { mName = value; IsDirty = true; }
		} string mName;

		public Task NewTask()
		{
			var task = new Task() { mTaskList = this };
			mTasks.Add(task);
			IsDirty = true;
			return task;
		}

		[XmlElement("Task", typeof(Task))]
		public List<Task> Tasks { get { return mTasks; } set { mTasks = value; } }

		public enum TaskState { Pending, Done, InWork };
		public class Task
		{
			static int count;
			internal TaskList mTaskList;

			public Task()
			{
				Name = "Task" + ++count;
			}

			public void CopyTo(Task dest)
			{
				dest.Name = Name;
				dest.Description = Description;
				dest.State = State;
			}

			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlAttribute("description")]
			public string Description
			{ get { return mDescription; } set { mDescription = value; IsDirty = true; } } string mDescription;

			[XmlIgnore, Browsable(false)]
			public bool IsDirty
			{
				get { return mIsDirty; }
				set
				{
					mIsDirty = value;
					if (value && mTaskList != null)
						mTaskList.IsDirty = value;
				}
			} bool mIsDirty;

			[XmlAttribute("state")]
			public TaskState State { get; set; }

			[XmlIgnore]
			public object Tag { get; set; }
		}

	}
}
