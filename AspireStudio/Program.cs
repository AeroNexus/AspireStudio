using System;
using System.Windows.Forms;

namespace Aspire.Studio
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			RootDirectory = Environment.CurrentDirectory;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		public static string RootDirectory { get; set; }
	}
}
