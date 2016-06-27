using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Aspire.Utilities
{
	public static class FileUtilities
	{

		public static string ScenarioDirectory;

		/// <summary>
		/// Open a text file for reading in the current scenario
		/// </summary>
		/// <param name="relativeFileName"></param>
		/// <returns></returns>
		public static TextReader OpenScenarioTextReader(string relativeFileName)
		{
			try
			{
				TextReader tw = new StreamReader(ScenarioDirectory + relativeFileName);
				return tw;
			}
			catch (System.Exception e)
			{
				Log.ReportException(e,"OpenScenarioTextReader({0})", relativeFileName);
			}
			return null;
		}

		/// <summary>
		/// Open a text file for writing in the current scenario
		/// </summary>
		/// <param name="relativeFileName"></param>
		/// <returns></returns>
		public static TextWriter OpenScenarioTextWriter(string relativeFileName)
		{
			try
			{
				TextWriter tw = new StreamWriter(ScenarioDirectory + relativeFileName);
				return tw;
			}
			catch (System.Exception e)
			{
				Log.ReportException(e, "OpenScenarioTextWriteder({0})", relativeFileName);
			}
			return null;
		}

		/// <summary>
		/// Deserialize an object of the specified type from the specified file
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="type"></param>
		/// <param name="additionalTypes"></param>
		/// <returns></returns>
		public static object ReadFromXml(string filename, Type type, Type[] additionalTypes = null)
		{
			XmlSerializer serializer = null;

			if (additionalTypes != null)
				serializer = new XmlSerializer(type, additionalTypes);
			else
				serializer = new XmlSerializer(type);

			using (var reader = new StreamReader(filename))
			{
				return serializer.Deserialize(reader);
			}
		}

		/// <summary>
		/// Find a relative directory wrt/ the Scenario folder
		/// </summary>
		/// <param name="rootedPath">A fully rooted folder path</param>
		/// <returns> the relative path or null if not common</returns>
		public static string ScenarioRelativePath(string rootedPath)
		{
			return RelativePath(rootedPath, ScenarioDirectory);
		}

		/// <summary>
		/// Find a relative directory wrt/ the Scenario folder
		/// </summary>
		/// <param name="rootedPath">A fully rooted folder path representing the t'o' path</param>
		/// <param name="referencePath">The path used as the reference to find the relative path 'from' </param>
		/// <returns> the relative path or null if not common</returns>
		public static string RelativePath(string rootedPath, string referencePath)
		{
			Uri scenarioDir = new Uri(referencePath);
			Uri path = new Uri(rootedPath);
			Uri relativeUri = scenarioDir.MakeRelativeUri(path);
			var relativePath = relativeUri.ToString();
			return relativePath.Replace("%20", " ");
		}

		public static string TranslateSpecialPath(string path)
		{
			string translated = path;
			var start = translated.IndexOf('$');
			if (start >= 0)
			{
				var end = translated.IndexOf(')');
				var special = translated.Substring(start + 2, end - start - 2);
				switch (special)
				{
					case "MyDocuments": special = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); break;
					case "Recent": special = Environment.GetFolderPath(Environment.SpecialFolder.Recent); break;
					default: Log.WriteLine("FileUtilities.TranslateSpecialFolder: unsupported {0}", special); break;
				}
				translated = special + translated.Substring(end + 1);
			}
			return translated;
		}

		/// <summary>
		/// Serialize an object instance to an XML file using the given XSLT file as the xml-stylesheet property. If the xsltFilename parameter is an empty string, the xml-stylesheet property is not written.
		/// </summary>
		/// <param name="objectToSerialize"></param>
		/// <param name="filename"></param>
		/// <param name="xsltFilename"></param>
		/// <param name="formatIndented">If true, uses Indented formating on the xml file.</param>
		/// <param name="additionalTypes"></param>
		public static void WriteToXml(object objectToSerialize, string filename, bool formatIndented=false, string xsltFilename=null, Type[] additionalTypes=null)
		{
			XmlSerializer serializer;

			if (additionalTypes != null)
				serializer = new XmlSerializer(objectToSerialize.GetType(), additionalTypes);
			else
				serializer = new XmlSerializer(objectToSerialize.GetType());

			var writer = new XmlTextWriter(filename, null);
			try
			{
				writer.Formatting = formatIndented ? Formatting.Indented : Formatting.None;

				if (xsltFilename != null)
					writer.WriteProcessingInstruction("xml-stylesheet", string.Format("type='text/xsl' href='{0}'", xsltFilename));

				// Serializes the items, and closes the TextWriter.
				serializer.Serialize(writer, objectToSerialize);
			}
			finally
			{
				writer.Close();
			}
		}

	}
}
