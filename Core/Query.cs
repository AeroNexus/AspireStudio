using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.Core
{
	#region Query

	class Query
	{
		internal string mDomainName, mSpec;
		internal int mId;
		//int mRegistrarsLength;
		internal DomainScope mScope;
		internal QueryMgr.When mWhen;
		internal byte mOp;
		string mExtraDeliveries;

		internal Query(DomainScope scope, string domainName, int id, string spec,
			QueryMgr.When when, byte op, string extraDeliveries=null)
		{
			mDomainName = domainName;
			mSpec = spec;
			mId = id;
			mScope = scope;
			mWhen = when;
			mOp = op;
			mExtraDeliveries = extraDeliveries;
		}

		internal Query(Query rhs)
		{
			mDomainName = rhs.mDomainName;
			mSpec = rhs.mSpec;
			mId = rhs.mId;
			mScope = rhs.mScope;
			mWhen = rhs.mWhen;
			mOp = rhs.mOp;
		}

		internal string DomainName { get { return mDomainName; } }

		internal string ExtraDeliveries { get { return mExtraDeliveries; } }

		internal int Id { get { return mId; } }

		internal byte Op { get { return mOp; } }

		internal DomainScope Scope { get { return mScope; } }

		internal string Spec { get { return mSpec; } }

		internal QueryMgr.When When { get { return mWhen; } }

		internal bool Matches(CoreAddress registrar, bool registered)
		{
			string scope;

			string domain = DomainName;
			switch (Scope)
			{
				case DomainScope.Specifically:
					scope = null;
					break;
				case DomainScope.Locally:
					scope = "Local";
					break;
				case DomainScope.Remotely:
					scope = "Remote";
					break;
				case DomainScope.All:
				default:
					scope = null;
					break;
			}

			if (domain != null && domain[0] == '*') domain = null;

			// NULL domain + Specifically only matches the registrar we are registered with
			if (domain == null && Scope == DomainScope.Specifically && !registered) return false;

			if (domain != null && domain == registrar.DomainName) return false;
			if (scope != null && scope == registrar.Scope) return false;
			return true;
		}
	}

	#endregion

	#region QueryMgr

	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class QueryMgr
	{
		public enum When { Existing, Future }

		ASCIIEncoding mAsciiEncoding = new ASCIIEncoding();
		List<Byte> mByteList = new List<Byte>();
		List<Query> queries = new List<Query>();
		MarshaledBuffer sbuf = new MarshaledBuffer();
		string mExtraDeliveries;
		uint mAddressHash;

		public QueryMgr()
		{
			mParser = new QueryParser();
		}

		public AddressServer AddressServer { get; set; }

		public ControlProtocol ControlProtocol
		{
			get { return mControlProtocol; }
			set { mControlProtocol = value; }
		} ControlProtocol mControlProtocol;

		public string DomainName
		{
			get { return mDomainName; }
			set { mDomainName = value; }
		} string mDomainName;

		public string ExtraDeliveries { set { mExtraDeliveries = value; } }

		public void ForExistingMessage(int queryId, string searchSpec, UInt32 addressHash = 0)
		{
			mAddressHash = addressHash;
			ForMessage(queryId, searchSpec, When.Existing);
		}

		public bool ForMessage(int queryId, string searchSpec, When when = When.Future, byte op = 0)
		{
			return ForMessage(queryId, searchSpec, null, when, op);
		}

		internal bool ForMessage(Query q)
		{
			return ForMessage(q.Id, q.Spec, q, q.When,q.Op );
		}

		bool ForMessage(int queryId, string searchSpec, Query query, When when = When.Future, byte op = 0)
		{
			if (mControlProtocol == null) return false;

			query = new Query(Scope, mDomainName, queryId, searchSpec, when, op, mExtraDeliveries);
			mExtraDeliveries = null;
			return mControlProtocol.Query(query);
		}

		//byte[] forMessageBuffer = new byte[8192];

		//void forMessage(int queryId, string searchSpec, When when, byte op, MarshaledBuffer sbuf)
		//{
		//	if (queryId == 0)
		//	{
		//		MsgConsole.WriteLine("Query tags must be > 0");
		//		return;
		//	}

		//	int levelBeyondMessage;
		//	mParser.Parse(searchSpec, this, out levelBeyondMessage);

		//	mDeliverOps.Add(new OpStr(StructuredQuery.sq.MessageSpec));
		//	mDeliverOps.Add(new OpStr(StructuredQuery.sq.End));
		//	SetDeliver(mDeliverOps,0,2);
		//	Futures = when==When.Future;

		//	int payload = mControlProtocol.MessageSize, offset = payload;
		//	offset += Marshal(forMessageBuffer, payload, offset);
		//	sbuf.Set(offset-payload,forMessageBuffer, payload);
		//	//mControlProtocol.SendQuery(queryId, ControlProtocol.QueryType.Structured, sbuf, null);
		//}

		public bool Futures
		{
			get { return mFutures; }
			set
			{
				mFutures = value;
				mAddressHash = 0;
			}
		} bool mFutures;

		void IssuePendingQueries(object sender, EventArgs e)
		{
			//MsgConsole.WriteLine("Issueing pending queries");
			int length = queries.Count; // Might increase when calling ForMessage
			for(int i=0;i<length;i++)
			{
				Query q = queries[i];
				if (q==null) continue;
				//MsgConsole.WriteLine("{0}? {1}",q.mDomainName,q.mSpec);
				Scope = q.mScope;
				mDomainName = q.mDomainName;
				if ( ForMessage(q) )
				{
					queries.RemoveAt(i);
				}
			}
		}

		int Marshal(byte[] buffer, int offset, int length)
		{
			if (mMatch == null || mMatch.Length == 0) return 0;
			int start = offset; PutNetwork.UINT(buffer, offset, mAddressHash);
			buffer[offset+4] = (byte)(mFutures?1:0);
			Buffer.BlockCopy(mMatch, 0, buffer, offset+5, mMatch.Length);
			offset += 5+mMatch.Length-1;
			buffer[offset+0] = (byte)StructuredQuery.sq.Deliver;
			Buffer.BlockCopy(mDeliver, 0, buffer, offset+1, mDeliver.Length);
			offset += mDeliver.Length+1;

			return offset - start;
		}

		object[] mDeliverOps = new object[16];

		int Parse(int queryId, string searchSpec, When when,
							byte op, byte[] buffer, string extraDeliveries)
		{
			if (queryId == 0)
			{
				MsgConsole.WriteLine("Query tags must be > 0");
				return -1;
			}
			int levelBeyondMessage;
			mParser.Parse(searchSpec, this, out levelBeyondMessage);

			int di = 0;
			while (levelBeyondMessage-- > 0)
				mDeliverOps[di++] = StructuredQuery.sq.Up;

			mDeliverOps[di++] = StructuredQuery.sq.MessageSpec;

			if (op != (byte)StructuredQuery.sq.End)
			{
				mDeliverOps[di++] = (StructuredQuery.sq)op;
				mDeliverOps[di++] = StructuredQuery.sq.End;

				SetDeliver(mDeliverOps,0,di);
			}
			else
			{
				int pushes = mParser.Pushes;
				if (pushes > 0)
				{
					for (int i = 0; i < pushes; i++)
						mDeliverOps[di++] = StructuredQuery.sq.Pop;
				}
				//int len = pushes+2;
				if (extraDeliveries != null)
					di = mParser.ParseOps(extraDeliveries, mDeliverOps, di, mDeliverOps.Length-di, out levelBeyondMessage);
				mDeliverOps[di++] = StructuredQuery.sq.End;
				SetDeliver(mDeliverOps,0,di);
			}
			Futures = when == When.Future;

			int len = Marshal(buffer, 0, buffer.Length);
			return len;
		}

		internal int Parse(Query query, byte[] buffer)
		{
			return Parse(query.Id, query.Spec, query.When, query.Op, buffer, query.ExtraDeliveries);
		}

		public QueryParser Parser
		{
			get { return mParser; }
		} QueryParser mParser;

		public IApplication QueryProvider { get; set; }

		public DomainScope Scope { get; set; }

		void SetDeliver(object[] ops, int offset, int length)
		{
			FillByteList(ops,offset,length);
			mDeliver = mByteList.ToArray();
		} byte[] mDeliver;

		private void FillByteList(object[] ops, int offset, int length)
		{
			mByteList.Clear();
			for(int i=offset; i<length; i++)
			{
				object os = ops[i];
				if (os.GetType() == typeof(string))
				{
					Byte[] bytes = mAsciiEncoding.GetBytes(os as string);
					foreach (var _byte in bytes)
						mByteList.Add(_byte);
					mByteList.Add((byte)0);
				}
				else if (os.GetType() == typeof(StructuredQuery.sq))
				{
					if (((StructuredQuery.sq)os) == StructuredQuery.sq.End)
						break;
					mByteList.Add((byte)(StructuredQuery.sq)os);
				}
				else
					break;
			}
			mByteList.Add((byte)StructuredQuery.sq.End);
		}

		public void SetMatch(object[] ops, int offset, int length)
		{
			FillByteList(ops,offset,length);
			mMatch = mByteList.ToArray();
		} byte[] mMatch;

    public bool Validate(string searchSpec)
    {
      object[] ops = new object[32];
      int levelBeyond, offset=0, opLength=0;
      offset = mParser.ParseOps(searchSpec,ops,offset,opLength,out levelBeyond);
      return levelBeyond == 0 && mParser.Errors == 0;
    }
	}

#endregion
}
