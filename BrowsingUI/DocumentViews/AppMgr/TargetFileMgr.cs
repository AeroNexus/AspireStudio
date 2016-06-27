namespace Aspire.BrowsingUI.DocumentViews.AppMgr
{
	public class TargetFileMgr : FileMgr
	{
		public override void CloseAppMgr()
		{
		}

		public override void Delete(string configName)
		{
		}

		public override string[] GetApplications()
		{
			return null;
		}

		public override string[] GetConfigFiles()
		{
			return null;
		}

		public override void KillAppMgr(bool hasConsole)
		{
		}

		public override string ReadConfig(string configFileName)
		{
			return string.Empty;
		}

		public override void Save(AppConfig config)
		{
		}

		public override void StartAppMgr(bool startAspire, string configName)
		{
		}

	}
}
