using System.IO;

namespace Aspire.Utilities
{
	public static class FlatFile
	{
		public static string ReadFile(string fileName)
		{
			FileStream fs = null;
			StreamReader sr = null;

			try
			{
				if (!File.Exists(fileName))
					return string.Empty;

				using (fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
				{
					using (sr = new StreamReader(fs))
					{
						return sr.ReadToEnd();
					}
				}
			}
			catch
			{
				return string.Empty;
			}
			finally
			{
				if (sr != null)
					sr.Close();

				if (fs != null)
				{
					fs.Close();
					fs.Dispose();
				}
			}
		}
	}
}