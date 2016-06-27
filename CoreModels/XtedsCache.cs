using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
//using System.Threading;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;
using Aspire.Core.Utilities;

namespace Aspire.CoreModels
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class XtedsCache
	{
		//AspireBrowser mBrowser;
		string mDirectoryPath;
		Dictionary<string, CacheEntry> mEntryByName = new Dictionary<string, CacheEntry>();
		Dictionary<Uuid, CacheEntry> mEntryByUid = new Dictionary<Uuid, CacheEntry>();
		List<CacheEntry> mCacheList = new List<CacheEntry>();
		RequestTable requests = new RequestTable();
		bool mChanged;
		const string xtedsIndexFile = "xtedsIndex.xml";

		static XtedsCache theCache;

		public static XtedsCache The
		{
			get
			{
				if (theCache == null)
					theCache = new XtedsCache();
				return theCache;
			}
		}

		private XtedsCache()
		{
			mDirectoryPath = Scenario.Directory + "\\BrowserCache\\";
			DirectoryInfo dir = new DirectoryInfo(mDirectoryPath);
			if (!dir.Exists)
				dir.Create();
			string path = mDirectoryPath + xtedsIndexFile;
			try
			{
				if (!File.Exists(path))
					return;
				mEntries = FileUtilities.ReadFromXml(path, typeof(CacheEntry[])) as CacheEntry[];
			}
			catch (System.IO.DirectoryNotFoundException)
			{
				System.IO.Directory.CreateDirectory(mDirectoryPath);
			}
			catch (System.Exception)
			{
			}
			if (mEntries != null)
				foreach (CacheEntry ce in mEntries)
					Add(ce, true);
		}

		#region Properties

		[XmlIgnore, Browsable(false)]
		public bool BlockSave
		{
			set { mBlockSave = value; }
		} bool mBlockSave;

		[XmlIgnore]
		public bool CachingEnabled
		{
			get { return cachingEnabled; }
			set { cachingEnabled = value; }
		} bool cachingEnabled = true;

		[XmlIgnore]
		public string Directory { get { return mDirectoryPath; } }

		public void Enabled(Uuid uid, ref bool cached)
		{
			CacheEntry ce;
			if (mEntryByUid.TryGetValue(uid, out ce))
				cached = ce.enabled;
		}

		[XmlArray]
		public CacheEntry[] Entries
		{
			get
			{
				if (mChanged)
				{
					mEntries = mCacheList.ToArray();
					mChanged = false;
				}
				return mEntries;
			}
			set { mEntries = value; }
		} CacheEntry[] mEntries;

		#endregion

		void Add(CacheEntry ce, bool addByName)
		{
			mCacheList.Add(ce);
			if (addByName)
				mEntryByName.Add(ce.name, ce);
			mEntryByUid.Add(ce.uid, ce);
		}

		public string Find(string name, out CacheEntry cacheEntry)
		{
			string searchName;
			int version = 0, numVersions = 1;

			do 	{
				if (version > 0)
					searchName = name + ';' + version;
				else
					searchName = name;
				if (mEntryByName.TryGetValue(name, out cacheEntry))
				{
					if (version == 0) numVersions = cacheEntry.version;
					try
					{
						StreamReader sr = new StreamReader(mDirectoryPath + cacheEntry.fileName);
						cacheEntry.text = sr.ReadToEnd();
					}
					catch (FileNotFoundException ex)
					{
						Log.ReportException(ex, "Maybe the cache entry for {0} is wrong", name);
						return null;
					}
					return cacheEntry.text;
				}
				version++;
			} while (version < numVersions);
			return null;
		}

		public void SaveIndex()
		{
			mCacheList.Sort();
			CacheEntry[] sorted = mCacheList.ToArray();
			FileUtilities.WriteToXml(sorted, mDirectoryPath + xtedsIndexFile, true);
			mBlockSave = false;
		}

		public void SetEnable(Uuid xtedsUid, bool cached)
		{
			CacheEntry ce;
			if ( mEntryByUid.TryGetValue(xtedsUid, out ce) )
			{
				ce.enabled = cached;
				if (!mBlockSave)
					SaveIndex();
			}
		}

		void Remove(CacheEntry ce)
		{
			lock (mCacheList)
			{
				mCacheList.Remove(ce);
				mChanged = true;
				mEntryByName.Remove(ce.name);
				mEntryByUid.Remove(ce.uid);
			}
		}

		public bool RequestXteds(AspireComponent comp, IPnPBrowser browser)
		{
			CacheEntry ce;
			//Log.WriteLine("Requesting {0}[{1:X}]'s xteds",name,sensorId);
			lock (mCacheList)
			{
				if (!mEntryByUid.TryGetValue(comp.XtedsUid, out ce) || !ce.enabled || !cachingEnabled)
				{
					// not known yet. Get it from the component
					requests.Add(comp);
					browser.RequestXteds(comp);

					return true;
				}
			}
			//var list = requests[comp.XtedsUid];
			//list.Clear();
			if (ce.text != null) // already in the in-memory cache
			{
				browser.XtedsIsAvailable(comp, ce.text);
			}
			else // in the index but not yet read in
				try
				{
					StreamReader sr = new StreamReader(mDirectoryPath+ce.fileName);
					ce.text = sr.ReadToEnd();
					browser.XtedsIsAvailable(comp, ce.text);
				}
				catch (System.IO.FileNotFoundException)
				{
					Remove(ce);
					// Save the index just in case the component is no longer available
					// and we need to start at this point next invocation
					SaveIndex();
					requests.Add(comp);
					browser.RequestXteds(comp);

					return true;
				}
				catch (Exception e)
				{
					Log.WriteLine("AspireBrowser: Exception Requesting {0}[{1:X}]'s xteds\n{2}",
						comp.Name, comp.Address.Hash, e.Message);
				}
			return false;
		}

		public void OnXtedsReply(Uuid xtedsUid, string text)
		{
			//Log.WriteLine("OnXtedsReply({0}",xtedsUid);
			var list = requests[xtedsUid];
			if (list == null || list.Count == 0)
			{
				Log.WriteLine("AspireBrowser: Request not found for {0}", xtedsUid.ToString());
				return;
			}
			var comp = list.First.Value;
			CacheEntry baseEntry, ce = null;
			StreamWriter sw;

			string fileName = null;
			// If this was in the cache, but is uncached, just save the xteds and deliver the text to the user
			if (!cachingEnabled || (mEntryByUid.TryGetValue(xtedsUid, out ce) && !ce.enabled) )
			{
				if (ce == null)
				{
					int start = text.IndexOf("name=");
					if (start > 0)
					{
						char delim = text[start + 5];
						if ( delim == '\'' || delim == '\"' )
						{
							int stop = text.IndexOf(delim, start + 6);
							fileName = text.Substring(start+6,stop-start-6) + ".xteds";
						}
					}
					if ( fileName == null )
						fileName = xtedsUid.ToString() + ".xteds";
				}
				else
					fileName = ce.fileName;
				//using (
				try	
				{
					sw = new StreamWriter(mDirectoryPath + fileName);
				//{
					sw.Write(text);
					sw.Close();
				}
				catch ( Exception ex )
				{
					Log.ReportException(ex);
				}
				comp.Browser.XtedsIsAvailable(comp, text);
				lock (list)
				{
					foreach (var bcomp in list)
						bcomp.Browser.XtedsIsAvailable(bcomp, text);
					list.Clear();
				}
				return;
			}

			// caching is enabled, so update it in the cache
			string version;
			if (!mEntryByName.TryGetValue(comp.XtedsName, out baseEntry))
				version = string.Empty;
			else
				version = ';'+(baseEntry.version++).ToString();
			if (text == string.Empty) // wasn't found in Directory
			{
				if (mEntryByUid.TryGetValue(xtedsUid, out ce))
					Remove(ce);
				list.Clear();
				return;
			}
			//int start = text.IndexOf("name")+5;
			//int stop = text.IndexOf("\"", start+1);
			//string name = text.Substring(start, stop-start);
			fileName = comp.XtedsName+version+".xteds";
			sw = new StreamWriter(mDirectoryPath+fileName);
			sw.Write(text);
			sw.Close();
			lock (mCacheList)
			{
				if (!mEntryByUid.TryGetValue(xtedsUid, out ce))
				{
					ce = new CacheEntry();
					ce.uid = xtedsUid;
					ce.text = text;
					ce.name = comp.XtedsName + version;
					ce.fileName = fileName;
					ce.enabled = cachingEnabled;
					Add(ce, !mEntryByName.ContainsKey(ce.name));
					mChanged = true;
				}
				else
				{
					ce.text = text;
					ce.fileName = fileName;
					//mChanged = true;
				}
			}
			SaveIndex();
			lock (list)
			{
				foreach (var bcomp in list)
					bcomp.Browser.XtedsIsAvailable(bcomp, ce.text);
				list.Clear();
			}
		}
	}

	public class CacheEntry : IComparable
	{
		[XmlAttribute]
		public string name;
		[XmlAttribute]
		public string fileName;
		[XmlAttribute, DefaultValue(true)]
		public bool enabled = true;
		[XmlAttribute, DefaultValue(1)]
		public int version=1;
		[XmlAttribute("uid")]
		public string UidString
		{
			get { return uid.ToString(); }
			set { uid = new Uuid(value); }
		}
		internal Uuid uid;
		internal string text;

		public override string ToString()
		{
			return name;
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return String.Compare(name, (obj as CacheEntry).name);
		}

		#endregion
	}

	internal class RequestTable
	{
		Dictionary<Uuid, LinkedList<AspireComponent>> listByUid = new Dictionary<Uuid, LinkedList<AspireComponent>>();
		internal void Add(AspireComponent component)
		{
			LinkedList<AspireComponent> list;
			lock (listByUid)
			{
				if (!listByUid.TryGetValue(component.XtedsUid, out list))
				{
					list = new LinkedList<AspireComponent>();
					listByUid.Add(component.XtedsUid, list);
				}
				if ( !list.Contains(component) )
					lock (list)
						list.AddLast(component);
			}
		}
		internal bool Contains(Uuid uid)
		{
			LinkedList<AspireComponent> list;
			lock (listByUid)
			{
				return listByUid.TryGetValue(uid, out list);
			}
		}
		internal LinkedList<AspireComponent> this[Uuid uid]
		{
			get
			{
				LinkedList<AspireComponent> list;
				lock (listByUid)
				{
					if (listByUid.TryGetValue(uid, out list))
						return list;
				}
				return null;
			}
		}
		internal void Remove(AspireComponent comp)
		{
			LinkedList<AspireComponent> list;
			lock (listByUid)
			{
				if (listByUid.TryGetValue(comp.XtedsUid, out list))
				{
					lock (list)
						list.Remove(comp);
				}
			}
		}
	}
}
