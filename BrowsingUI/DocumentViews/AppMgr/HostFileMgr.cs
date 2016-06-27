using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Aspire.Utilities;

namespace Aspire.BrowsingUI.DocumentViews.AppMgr
{
	public class HostFileMgr : FileMgr
	{
		Process mAppMgr;

		public HostFileMgr()
		{
			mBinDirectory = Environment.GetEnvironmentVariable("AspireBin");
		}

		public override void CloseAppMgr()
		{
			mAppMgr.WaitForExit(1000);
			mAppMgr.Close();
			mAppMgr = null;
			IsAppMgrStarted = false;
		}

		public override void Delete(string configName)
		{
			File.Delete(ApplicationManager.CombinePath(mDataDirectory, configName,".apps.xml"));
		}

		public override AppMgrDoc Document
		{
			get { return base.Document; }
			set
			{
				base.Document = value;

				if (mBinDirectory == null) mBinDirectory = ".\\";
				if (mDataDirectory == null)
				{
					Uri dd = new Uri(Path.Combine(mBinDirectory, @"..\..\..\dat"));
					mDataDirectory = dd.LocalPath;
				}
			}
		}

		public override string[] GetConfigFiles()
		{
			if (mDataDirectory == null)
				return null;
			return Directory.GetFiles(mDataDirectory);
		}

		public override string[] GetApplications()
		{
			var di = new DirectoryInfo(mBinDirectory);
			if (di.LastWriteTime > mLastBinWriteTime)
			{
				List<string> apps = new List<string>();
				mLastBinWriteTime = di.LastWriteTime;
				var files = Directory.GetFiles(mBinDirectory);
				foreach (var file in files)
					if (Path.GetExtension(file).ToLower() == ".exe")
					{
						var fileName = Path.GetFileNameWithoutExtension(file);
						switch (fileName)
						{
							case "ApplicationManager":
							case "Directory":
							case "ProcessorManager":
								break;
							default:
								apps.Add(fileName);
								break;
						}
					}
				mApplications = apps.ToArray();
			}
			return mApplications;
		} string[] mApplications = new string[0];

		public override void KillAppMgr(bool hasConsole)
		{
			if (mAppMgr == null) return;

			try
			{
				if (hasConsole)
					mAppMgr.CloseMainWindow();
				else
					mAppMgr.Kill();
				CloseAppMgr();
			}
			catch (InvalidOperationException) { }
			catch (Exception ex)
			{
				Log.ReportException(ex, "Trying to kill AppMgr");
			}
		}

		public override string ReadConfig(string configFileName)
		{
			string fileName = ApplicationManager.CombinePath(mDataDirectory, configFileName, ".apps.xml");

			return FlatFile.ReadFile(fileName);
		}

		public override void Save(AppConfig config)
		{
			config.Save(mDataDirectory);
		}

		public override void StartAppMgr(bool startAspire, string configName)
		{
			if (mAppMgr != null) return;

			mAppMgr = new Process();
			mAppMgr.StartInfo.FileName = Path.Combine(mBinDirectory, "ApplicationManager.exe");
			mAppMgr.StartInfo.WorkingDirectory = mBinDirectory;
			if (startAspire)
			{
				var args = string.Format("-P{0} -D{0} -g{1}", (int)Document.StartAspireConsole, Document.DebugLevel);
				if (Document.ActiveConfig != null)
					args += " -c" + configName + ".apps.xml";
				mAppMgr.StartInfo.Arguments = args;
				Client.AspireState = AspireState.UpArmed;
			}
			switch (Document.Console)
			{
				case AppMgrDoc.ConsoleState.None:
					mAppMgr.StartInfo.UseShellExecute = false;
					mAppMgr.StartInfo.CreateNoWindow = true;
					break;
				case AppMgrDoc.ConsoleState.Stowed:
				default:
					mAppMgr.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
					break;
				case AppMgrDoc.ConsoleState.Open:
					mAppMgr.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					break;
			}

			try
			{
				mAppMgr.Exited += appMgr_Exited;
				mAppMgr.Disposed += appMgr_Disposed;
				mAppMgr.Start();
				IsAppMgrStarted = true;
			}
			catch (Exception ex)
			{
				Log.ReportException(ex, "Trying to start {0}", mAppMgr.StartInfo.FileName);
			}

		}

		void appMgr_Disposed(object sender, EventArgs e)
		{
			mAppMgr = null;
			IsAppMgrStarted = false;
			Client.AspireState = AspireState.DownArmed;
		}

		void appMgr_Exited(object sender, EventArgs e)
		{
			mAppMgr = null;
			IsAppMgrStarted = false;
			Client.AspireState = AspireState.DownArmed;
		}

	}
}