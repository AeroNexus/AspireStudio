using System;
using System.Collections.Generic;
using System.ComponentModel;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core
{
	class Route
	{
		internal Uuid uid;
		internal Address address;
		internal UInt32 logicalAddress;
		internal MonarchTypes.ComponentType componentType;
	}

	class Vector<T>
	{
		List<T> list = new List<T>();

		internal T this[int index]
		{
			get
			{
				while (index >= list.Count)
					list.Add(default(T));
				return list[index];
			}
			set
			{
				while (index >= list.Count)
					list.Add(default(T));
				list[index] = value;
			}
		}

		internal int Length { get { return list.Count; } }
	}


	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class RoutingTable
	{
		class RouteTable : Vector<Route>{}
		Vector<RouteTable> mSubNets = new Vector<RouteTable>();
		Address mUnknownAddress;
		int mCount;

		public RoutingTable()
		{
			mUnknownAddress = ProtocolFactory.CreateAddress(ProtocolId.Monarch);
			mUnknownAddress.Set(Address.Type.Unknown,0);
			Uuid uuid = new Uuid();
			Add(0,uuid,mUnknownAddress,MonarchTypes.ComponentType.Unknown);
		}

		public Address Add(UInt32 logicalAddress, Uuid uuid, Address address, MonarchTypes.ComponentType componentType)
		{
			int subNet = (int)(logicalAddress>>16);
			RouteTable table = mSubNets[subNet];
			if (table == null)
			{
				table = new RouteTable();
				mSubNets[subNet] = table;
			}
			int id = (int)(logicalAddress&0xffff);
			Route route = table[id];
			if (route != null)
				route.address = address;
			else
			{
				route = new Route();
				table[id] = route;
				mCount++;
				route.address = address.Clone();
			}
			route.logicalAddress = logicalAddress;
			route.uid = uuid;
			route.componentType = componentType;
			if ( mCount > 1 )
				Print();
			return route.address;
		}

		void Print()
		{
			MsgConsole.WriteLine("Routing table updated [{0}]",mCount);
			int i=1;
			for(int sn=0; sn<mSubNets.Length; sn++ )
			{
				RouteTable table = mSubNets[sn];
				for( int ri=0; ri<table.Length; ri++)
				{
					Route r = table[ri];
					if ( r != null )
						MsgConsole.WriteLine("{0} {1,6} {2}.{3} {4} {5,-7} {6}",
							i++,r.logicalAddress,sn,ri,r.uid.ToString(),
							r.componentType,r.address.ToString());
				}
			}
		}

		void Remove(UInt32 logicalAddress)
		{
			int subNet = (int)(logicalAddress>>16);
			RouteTable table = mSubNets[subNet];
			if (table != null)
			{
				int id = (int)(logicalAddress&0xffff);
				table[id] = null;
			}
			mCount--;
			if ( mCount > 1 )
				Print();
		}

		public Address RoutedAddress(Address logicalAddress)
		{
			int depth = 0;
			Route route = null;
			Address addr = logicalAddress;
			do
			{
				uint address = addr.Hash;
				int subNet = (int)(address>>16);
				if ( subNet >= mSubNets.Length ) break;
				RouteTable table = mSubNets[subNet];
				if ( table == null ) break;
				int id = (int)(address&0xffff);
				if ( id >= table.Length ) break;
				route = table[id];
				if ( route == null ) break;
				addr = route.address;
			} while(addr.AddressType == Address.Type.Logical && depth++ < mCount);

			if ( route == null )
				return mUnknownAddress;

			return route.address;
		}
	}
}
