using System;
using System.Xml.Serialization;

namespace Aspire.Core.Utilities
{
	public struct Uuid : IXmlSerializable
	{
		public const int size = 16;
		enum Type { TimeBased=0x10, DCE=0x20, MD5=0x30, Random=0x40, SHA1=0x50 };

		byte[] bytes;

		public Uuid(int x)
		{
			bytes = new byte[size];
		}

		public Uuid(Uuid src)
		{
			bytes = new byte[size];
			src.bytes.CopyTo(bytes,0);
		}

		public Uuid(byte[] bytes, int offset)
		{
			this.bytes = new byte[size];
			int n = Math.Min(bytes.Length, this.bytes.Length);
		  if (bytes == null) return;
			Buffer.BlockCopy(bytes, offset, this.bytes, 0, n);
		}

		static byte[] swap = new byte[]{3,2,1,0,5,4,7,6,9,8,10,11,12,13,14,15};

		public Uuid(string value)
		{
			bytes = new byte[size];
			Parse(value);
		}

		public override bool Equals(object obj)
		{
            if (obj is Uuid)
            {
                byte[] rhsBytes = ((Uuid)obj).bytes;
                for (int i = 0; i < size; i++)
                    if (bytes[i] != rhsBytes[i]) return false;
                return true;
            }
            return false;
		}

		public override int GetHashCode()
		{
			int hash = 0;
			foreach (var b in bytes)
				hash += b;
			return hash;
		}

		public bool IsEmpty
		{
			get
			{
				if (bytes == null) return true;
				foreach (var b in bytes)
					if (b != 0)
						return false;
				return true;
			}
		}

		public int Marshal(byte[] buffer, int offset)
		{
			Buffer.BlockCopy(bytes, 0, buffer, offset, size);
			return size;
		}

		public int Marshal(MarshaledBuffer buffer)
		{
			Buffer.BlockCopy(bytes, 0, buffer.Bytes, buffer.Offset, size);
			return size;
		}

		public static Uuid NewUuid()
		{
			var guid = Guid.NewGuid();
			return new Uuid(guid.ToByteArray(),0);
		}

		public static bool operator<(Uuid lhs, Uuid rhs)
		{
			if (lhs.GetHashCode() < rhs.GetHashCode())
				return true;
			return false;
		}

		public static bool operator>(Uuid lhs, Uuid rhs)
		{
			if (lhs.GetHashCode() > rhs.GetHashCode())
				return true;
			return false;
		}

		public void Parse(string value)
		{
			if ( bytes == null ) bytes = new byte[size];
			if (value == null || value.Length > 38)
				return;
			if (value.Length < 36)
			{
				char[] template = "00000000-0000-0000-0000-000000000000".ToCharArray();
				for (int i = 0; i < value.Length; i++)
					template[i] = value[i];
				value = new string(template);
			}
			var guid = new Guid(value);
			var gbytes = guid.ToByteArray();
			for (int i = 0; i < size; i++)
				bytes[i] = gbytes[swap[i]];
		}

		void SetBytes(Type type, byte[] otherBytes)
		{
			// Per RFC 4122
			Buffer.BlockCopy(otherBytes, 0, bytes, 0, 16);
			bytes[6] = (byte)((int)type | (bytes[6]&0xf));
			bytes[8] = (byte)(0x80 | (bytes[8]&0x3f));
		}
		/// <summary>
		/// Convert 16 bytes of MD5 into 16 bytes of UUID
		/// </summary>
		/// <param name="md5Bytes"></param>
		public void SetFromMd5(byte[] md5Bytes)
		{
			SetBytes(Type.MD5, md5Bytes);
		}

		/// <summary>
		/// Convert the SHA-1 20 byte value to 16 bytes of UUID
		/// </summary>
		/// <param name="shaBytes"></param>
		public void SetFromSha1(byte[] shaBytes)
		{
			SetBytes(Type.SHA1, shaBytes);
		}

		public byte[] ToByteArray()
		{
			return bytes;
		}

		override public string ToString()
		{
			if (IsEmpty) return string.Empty;
			var gbytes = new byte[size];
			for (int i=0; i<size; i++)
				gbytes[i] = bytes[swap[i]];
			var guid = new Guid(gbytes);
			return guid.ToString();
		}

		#region IXmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.Value == null || reader.Value.Length < 36 || reader.Value.Length > 38)
			{
				bytes = new byte[size];
				return;
			}
			var uuid = new Uuid(reader.Value);
			uuid.ToByteArray().CopyTo(bytes,0);
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteValue(ToString());
		}

		#endregion
	}

}
