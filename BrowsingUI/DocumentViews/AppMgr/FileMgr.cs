using System;

namespace Aspire.BrowsingUI.DocumentViews.AppMgr
{
	public enum FileMgrType { Target, Host };

	public abstract class FileMgr
	{
		protected AppMgrDoc mDocument;
		protected DateTime mLastBinWriteTime = new DateTime();
		protected string
			mBinDirectory,
			mDataDirectory;

		public abstract void CloseAppMgr();

		public IFileMgrClient Client { get; set; }

		public abstract void Delete(string configName);

		public virtual AppMgrDoc Document
		{
			get { return mDocument; }
			set
			{
				mDocument = value;
				if (mDocument.AspireBinDirectory != null)
					mBinDirectory = mDocument.AspireBinDirectory;
				if (mDocument.AspireDataDirectory != null)
					mDataDirectory = mDocument.AspireDataDirectory;
			}
		}

		public abstract string[] GetApplications();

		public abstract string[] GetConfigFiles();

		public bool IsAppMgrStarted { get; set; }

		public abstract void KillAppMgr(bool hasConsole);

		public DateTime LastBinWriteTime
		{
			get { return mLastBinWriteTime; }
			set { mLastBinWriteTime = value;}
		}

		public abstract string ReadConfig(string configFileName);

		public abstract void Save(AppConfig config);

		public abstract void StartAppMgr(bool startAspire, string configName);
	}

	public interface IFileMgrClient
	{
		AspireState AspireState { get; set; }
	}

}
