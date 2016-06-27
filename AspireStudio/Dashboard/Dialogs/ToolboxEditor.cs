using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

using Aspire.Framework;
using Aspire.Studio.DockedViews;
using Aspire.Utilities;

namespace Aspire.Studio.Dialogs
{
	public partial class ToolboxEditor : Form
	{
		List<Type> models = new List<Type>();
		static AppDomain appDomain;

		public ToolboxEditor()
		{
			InitializeComponent();
		}

		void Add(Assembly assembly, List<Type> models)
		{
			int comma = assembly.FullName.IndexOf(',');
			var name = assembly.FullName.Substring(0, comma);
			int start = assembly.FullName.IndexOf("Version", comma);
			comma = assembly.FullName.IndexOf(',',start);
			var version = assembly.FullName.Substring(start+8, comma-(start+8));

			if ( config.AddAssembly(assembly, name, version) )
				dataGridView1.Rows.Add(name, version);
		}

		void Scan(string[] fileNames)
		{
			models.Clear();

			for(int j=0; j<fileNames.Length; j++)
			{
				string fileName = fileNames[j];
				try
				{
					if (fileName.EndsWith(".dll") || fileName.EndsWith(".DLL"))
						fileName = fileName.Substring(0, fileName.LastIndexOf('.'));
					var assyName = Path.GetFileName(fileName);
					Assembly assembly = null;
					foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
						if (assy.FullName.StartsWith(assyName))
						{
							assembly = assy;
							break;
						}
					if (assembly == null)
					{
						if (appDomain == null)
							appDomain = AppDomain.CreateDomain("ToolboxEditor");
						foreach (var assy in appDomain.GetAssemblies())
							if (assy.FullName.StartsWith(assyName))
							{
								assembly = assy;
								break;
							}
						if (assembly == null)
							assembly = appDomain.Load(fileName);
					}

					if (assembly != null)
					{
						foreach (var type in assembly.GetTypes())
							if (type.IsSubclassOf(typeof(Model)))
								models.Add(type);
						if (models.Count > 0)
							Add(assembly, models);
					}
				}
				catch (ReflectionTypeLoadException e)
				{
					var text = Log.ExceptionText(e.LoaderExceptions[0]);
					for (int i = 1; i < e.LoaderExceptions.Length; i++)
						text = Environment.NewLine + Log.ExceptionText(e.LoaderExceptions[i]);
					Log.ReportException(e, text);
				}
				catch (Exception e)
				{
					Log.ReportException(e);
				}
			}
		}

		public ToolBoxConfig ToolConfig
		{
			get { return config; }
			set { config = value.Clone(); }
		} ToolBoxConfig config;

		private void addBtn_Click(object sender, EventArgs e)
		{
			openFileDialog1.Title = "Select .Net assembly with Models";
			openFileDialog1.InitialDirectory = Program.RootDirectory;
			openFileDialog1.CheckFileExists = true;
			openFileDialog1.DefaultExt = ".dll";
			openFileDialog1.FileName = string.Empty;
			openFileDialog1.Filter = ".Net Assembly (*.dll)|*.dll|All files (*.*)|*.*";
			openFileDialog1.Multiselect = true;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
				Scan(openFileDialog1.FileNames);
		}

		private void ToolboxEditor_Activated(object sender, EventArgs e)
		{
			dataGridView1.Rows.Clear();
			foreach (var assy in config.Assemblies)
				dataGridView1.Rows.Add(assy.Name, assy.Version);
		}

		private void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			dataGridView1.Rows.Clear();
			dataGridView1.Refresh();
			config.Assemblies.Clear();
		}
	}
}
