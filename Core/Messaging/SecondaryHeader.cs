using System;
using System.Collections.Generic;

namespace Aspire.Core.Messaging
{
	public class SecondaryHeader
	{
		public enum SecondaryHdrType { AddressResolution=1, Continuation, MaxHdrType };

		protected const int MarshaledLength = 2;

		protected byte mLength;	///< Length of this secondary header [bytes]
		protected byte mTypeId;	///< Secondary header type. Either SecondaryHdrType or user defined

		private SecondaryHeader mNext; ///< Next header in list

		public SecondaryHeader(byte typeId, byte length)
		{
			mLength = length;
			mTypeId = typeId;
		}

		public void AddLast(SecondaryHeader header)
		{
			SecondaryHeader hdr = this;
			while (hdr.mNext != null)
				hdr = hdr.mNext;
			hdr.mNext = header;
		}

		public virtual SecondaryHeader CopyTo(SecondaryHeader dst)
		{
			dst.mLength = mLength;
			dst.mTypeId = mTypeId;

			return this;
		}

		public virtual int Marshal(byte[] buffer, int offset)
		{
			buffer[offset] = (byte)(mLength + 2);
			buffer[offset+1] = mTypeId;
			return 2;
		}

		public virtual int MarshalSize()
		{
			return 2;
		}

		public SecondaryHeader Next { get { return mNext; } }

		static List<byte> reservedTypes;
		static byte isPresent = 0xA5;

		public static void Reserve(int typeId)
		{
			if (reservedTypes == null)
			{
				reservedTypes = new List<byte>();
				reservedTypes.Insert((int)SecondaryHdrType.AddressResolution, isPresent);
				reservedTypes.Insert((int)SecondaryHdrType.Continuation, isPresent);
			}
			if (typeId > 255 || reservedTypes[typeId] == isPresent)
				throw new InUseException("Secondary header is already reserved. Use another", typeId);
			else
				reservedTypes[typeId] = isPresent;
		}

		//public static bool operator==(SecondaryHeader lhs, SecondaryHeader rhs)
		//{
		//    return lhs.mTypeId == rhs.mTypeId && lhs.mLength == rhs.mLength;
		//}

		//public static bool operator!=(SecondaryHeader lhs, SecondaryHeader rhs)
		//{
		//    return lhs.mTypeId != rhs.mTypeId || lhs.mLength != rhs.mLength;
		//}

		public override bool Equals(object obj)
		{
			SecondaryHeader rhs = obj as SecondaryHeader;
			return mTypeId == rhs.mTypeId && mLength == rhs.mLength;
		}

		public override int GetHashCode()
		{
			return mTypeId;
		}

		public int TypeId { get { return mTypeId; } }

		public override string ToString()
		{
			return string.Format("<{0}>[{1}]",mTypeId,mLength);
		}

		public virtual int Unmarshal(byte[] buffer, int offset)
		{
			mLength = buffer[offset];
			mTypeId = buffer[offset+1];
			return 2;
		}

	}

	public class SecondaryHeaderIterator
	{
		private Message mHeader;
		private int mLoc, mEnd;
		private byte[] mBuffer;

		public SecondaryHeaderIterator()
		{
		}

		public SecondaryHeaderIterator(Message header)
		{
			SetHeader(header);
		}

		public void Begin()
		{
			if (mHeader==null)
				mLoc = mEnd = -1;
			else
			{
				mBuffer = mHeader.Buffer;
				mLoc = mHeader.Size+mHeader.Offset;
				mEnd = (mHeader.HeaderSize<<2)+mHeader.Offset;
			}
		}

		public SecondaryHeader GetSecondaryHeader(SecondaryHeader src)
		{
			if (mLoc < mEnd && mBuffer!=null && mBuffer[mLoc+1] == src.TypeId)
			{
				src.Unmarshal(mBuffer,mLoc);
				return src;
			}
			return null;
		}

		public SecondaryHeader GetNextSecondaryHeader(SecondaryHeader src)
		{
			while (mLoc < mEnd && mBuffer[mLoc] != 0)
			{
				if (mBuffer!=null && mBuffer[mLoc+1] == src.TypeId)
				{
					src.Unmarshal(mBuffer,mLoc);
					mLoc += mBuffer[mLoc];
					return src;
				}
				else
					mLoc += mBuffer[mLoc];
			}
			return null;
		}

		public bool Has(SecondaryHeader.SecondaryHdrType type)
		{
			while (mLoc < mEnd && mBuffer[mLoc] != 0)
			{
				if (mBuffer!=null && mBuffer[mLoc+1] == (byte)type)
					return true;
				else
					mLoc += mBuffer[mLoc];
			}
			return false;
		}

		public static SecondaryHeaderIterator operator++(SecondaryHeaderIterator hdr)
		{
			hdr.mLoc += hdr.mBuffer[hdr.mLoc];
			if (hdr.mLoc >= hdr.mEnd || hdr.mBuffer[hdr.mLoc]==0) return null;
			return hdr;
		}

		public void SetHeader( Message header)
		{
			if (header.HaveSecondaryHeaders)
				mHeader = header;
			else
				mHeader = null;
			Begin();
		}

		/**
		 Gets the type of the current secondary header pointer to by this iterator
		
		 @return the type id.
		 */
		public byte Type()
		{
			return mLoc == -1 ? (byte)0 : mBuffer[mLoc+1];
		}
	}

	public class SecondaryHeaderHost
	{
		SecondaryHeader mSecondaryHeaders;

		public void AddSecondaryHeader(SecondaryHeader header)
		{
			if (mSecondaryHeaders == null)
				mSecondaryHeaders = header;
			else
				mSecondaryHeaders.AddLast(header);
		}

		public void ClearHeaders() { mSecondaryHeaders = null; }

		public SecondaryHeader GetSecondaryHeader(int typeId)
		{
			for (SecondaryHeader sh = mSecondaryHeaders; sh != null; sh = sh.Next)
				if (sh.TypeId == typeId)
					return sh;
			return null;
		}

		public int MarshalSecondaryHeaders(byte[] buffer, int offset)
		{
			var secHdr = mSecondaryHeaders;
			for (var sh=secHdr; sh != null; sh = sh.Next)
				offset += sh.Marshal(buffer, offset);
			return offset;
		}

		public SecondaryHeader SecondaryHeaders { get { return mSecondaryHeaders; } }
	}

	public class InUseException : Exception
	{
		string mText;
		int mIndex;

		public InUseException(string text, int index)
		{
			mText = text;
			mIndex = index;
		}

		public override string Message
		{
			get
			{
				return string.Format(mText+" {0}",mIndex);
			}
		}
	}
}
