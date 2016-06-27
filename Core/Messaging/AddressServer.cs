using System;
using System.Collections.Generic;

using Aspire.Core.Utilities;

namespace Aspire.Core.Messaging
{
	public class CoreAddress
	{
		Address mAddress;
		internal SecTime mTime = new SecTime();

		public CoreAddress() { }

		public CoreAddress(Address address, string type, string scope, string domainName)
		{
			mAddress = address.Clone();
			Type = type;
			Scope = scope;
			DomainName = domainName;
		}

		public Address Address { get { return mAddress; } }

		public string DomainName { get; set; }

		internal bool Equals(Address address, string type, string scope, string domainName)
		{
			if (mAddress.EqualsNetwork(address))
				mAddress.Hash = address.Hash;

			bool test = mAddress.Equals(address) && Type == type;
			if (scope != null) test = test && Scope == scope;
			if (domainName != null ) test = test && DomainName == domainName;
			return test;
		}

		public string Scope { get; set; }

		public SecTime Time { get { return mTime; } }

		public override string ToString()
		{
			return string.Format("{0} {1}, ({2})",Scope,DomainName,mAddress.ToString());
		}

		public string Type { get; set; }

		public object Tag { get; set; }
	}

	public class CoreAddressList : IEnumerable<CoreAddress>
	{
		List<CoreAddress> mItems = new List<CoreAddress>();

		public CoreAddress this[int index]
		{
			get { return mItems[index]; }
			set { mItems[index] = value; }
		}

		public int Add(CoreAddress coreAddress)
		{
			mItems.Add(coreAddress);
			return mItems.Count-1;
		}

		public void Clear() { mItems.Clear(); }

		public CoreAddressList Clone()
		{
			var newList = new CoreAddressList();
			foreach (var ca in mItems)
				newList.mItems.Add(ca);
			return newList;
		}

		public int Last { get { return mItems.Count-1; } }

		public int Length { get { return mItems.Count; } }

		public int Remove(Address address, string type)
		{
			int i = 0;
			foreach (var ca in mItems)
			{
				if (ca.Address == address && ca.Type == type)
				{
					mItems.Remove(ca);
					return i;
				}
				i++;
			}
			return -1;
		}

		public int Remove(CoreAddress coreAddress)
		{
			int i = mItems.IndexOf(coreAddress);
			if ( i >= 0 ) mItems.RemoveAt(i);
			return i;
		}

		internal void Squeeze()
		{
			List<CoreAddress> newItems = new List<CoreAddress>(); ;
			for (int i=0; i<mItems.Count; i++)
				if (mItems[i] != null)
					newItems.Add(mItems[i]);

			mItems.Clear();

			for (int i=0; i<mItems.Count; i++)
				mItems.Add(newItems[i]);
		}

		#region IEnumerable<CoreAddress> Members

		public IEnumerator<CoreAddress> GetEnumerator()
		{
			return mItems.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return mItems.GetEnumerator();
		}

		#endregion
	}

	public class AddressServer
	{
		static CoreAddressList
			addresses = new CoreAddressList(),
			directories = new CoreAddressList();

		public static event EventHandler DirectoryAdded;

		public static CoreAddress AddAddress(string type, Address address, string scope, string domainName)
		{
			bool directory = false;
			CoreAddress ai = null;
			bool added = true;
			SecTime now = new SecTime();
			Clock.GetTime(ref now);

			lock (addresses)
			{
				foreach (var addr in addresses)
				{
					if (addr.Equals(address, type, scope, domainName))
					{
						added = false;
						ai = addr;
						ai.mTime = now;
						break;
					}
				}
				if (added)
				{
					ai = new CoreAddress(address, type, scope, domainName);
					ai.mTime = now;
					addresses.Add(ai);

					if (type == "DIRECTORY")
					{
						directories.Add(ai);
						directory = true;
						//if (DirectoryAdded != null)
						//    DirectoryAdded(ai, EventArgs.Empty);
					}
				}
			}

			if (directory && DirectoryAdded != null)
				DirectoryAdded(ai, EventArgs.Empty);
			return ai;
		}

		public static CoreAddress FindCoreAddress(Address address)
		{
			lock(addresses)
				foreach ( var coreAddress in addresses)
					if (coreAddress.Address.Equals(address))
						return coreAddress;
			return null;
		}

		public static CoreAddressList GetAddresses(string type)
		{
			return GetAddresses(type,null,null);
		}
		public static CoreAddressList GetAddresses(string type, string domainName)
		{
			return GetAddresses(type,domainName,null);
		}
		public static CoreAddressList GetAddresses(string type, string domainName, string scope)
		{
			bool match;
			CoreAddressList getAddresses;

			if (type=="DIRECTORY")
			{
				if (domainName==null && scope==null)
					return directories;
				else
				{
					getAddresses = new CoreAddressList();
					lock (directories)
						foreach ( var dir in directories )
						{
							match = true;
							if (domainName.Length > 0) match = match && domainName == dir.DomainName;
							if (scope.Length > 0) match = match && scope == dir.Scope;
							if (match) getAddresses.Add(dir);
						}
					return getAddresses;
				}
			}
			getAddresses = new CoreAddressList();
			lock (addresses)
				foreach (var addr in addresses)
				{
					match = type == addr.Type;
					if (domainName != null) match = match && domainName == addr.DomainName;
					if (scope != null) match = match && scope == addr.Scope;
					if (match) getAddresses.Add(addr);
				}
			return getAddresses;
		}

		public Address LastAddress { get { return directories[directories.Last].Address; } }

		public static int NumDirectories
		{
			get { return directories.Length; }
		}

		public void PreloadRegistrar(Address address, string domainName)
		{
			AddAddress("DIRECTORY",address,"Local",domainName);
		}

		public static void RemoveAddress(string type, Address address)
		{
			lock (addresses)
			{
				int i = addresses.Remove(address, type);
				if (i >= 0) addresses.Squeeze();

				if (type == "DIRECTORY")
				{
					i = directories.Remove(address, type);
					// Keep directories cleaned of NULL slots created when removing an item.
					if (i >= 0) directories.Squeeze();
				}
			}
		}
	}
}
