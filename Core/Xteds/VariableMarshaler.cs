using System;
using System.Reflection;
using System.Text;

using Aspire.Framework;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
    public class VariableMarshaler : IVariableMarshaler
    {
        IVariable mVariable;
        IValueInfo mValueInfo;
        Type mType;
        object mContainer;
        int mLength, mSize;
        PrimitiveType mPrimType;
        bool mNeedsCoercion;
        TypeCode mTypeCode, mXVarTypeCode;
        enum MarshalType { Array, Atomic, Buffer, MarshaledString, String };
        MarshalType mMarshalType = MarshalType.Atomic;
        byte[] swapBuf;
        byte[] scratchBuf = new byte[8];

        public VariableMarshaler(string msgName, IVariable ivar)
        {
            mContainer = ivar;

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var pinfo = mContainer.GetType().GetProperty("Value", flags);
            mValueInfo = new PropertyValueInfo(pinfo);
            mType = mValueInfo.GetValue(ivar).GetType();

            Common(msgName, ivar, ivar.Name, ivar.Type);
        }

        public VariableMarshaler(IVariable ivariable, object container, string fieldName)
        {
            mContainer = container;

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var info = mContainer.GetType().GetField(fieldName, flags);
            mValueInfo = new FieldValueInfo(info);
            mType = mValueInfo.GetValue(container).GetType();
            Common(ivariable.Name, ivariable, ivariable.Name, ivariable.Type);
        }

        public VariableMarshaler(string msgName, IVariable ivar, Blackboard.Item item)
        {
            mContainer = ivar;

            mValueInfo = new DataItemValueInfo(item);
            object value = mValueInfo.GetValue(ivar);
            if (value != null)
            {
                mType = value.GetType();
                Common(msgName, ivar, ivar.Name, ivar.Type);
            }
        }

        public VariableMarshaler(string msgName, object container, IVariable ivar, string localName, PrimitiveType primType, int length)
        {
            if (ivar == null) return;
            if (container == null)
            {
                MsgConsole.WriteLine("Can't map {0}.{1} to {2}: container is null.",
                    msgName, ivar.Name, localName);
                return;
            }
            mContainer = container;

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var finfo = mContainer.GetType().GetField(localName, flags);
            if (finfo == null)
            {
                var pinfo = mContainer.GetType().GetProperty(localName, flags);
                if (pinfo != null)
                {
                    mType = pinfo.PropertyType;
                    mValueInfo = new PropertyValueInfo(pinfo);
                }
                else
                {
                    MsgConsole.WriteLine("VariableMarshaler: can't find {0} in app's fields or properties", localName);
                    return;
                }
            }
            else
            {
                mType = finfo.FieldType;
                mValueInfo = new FieldValueInfo(finfo);
            }

            if (primType != PrimitiveType.unknownType)
            {
                mPrimType = primType;
                mLength = length;
            }

            if (length > 1 && length != ivar.Length && primType != PrimitiveType.STRING && primType != PrimitiveType.BUFFER)
                MsgConsole.WriteLine("VariableMarshaler: {0}.{1} array incompatible with {2}",
                    msgName, mVariable.Name, localName);
            else
                Common(msgName, ivar, localName, primType);
        }

        void Common(string msgName, IVariable ivar, string localName, PrimitiveType localPrimType)
        {
            mVariable = ivar;

            if (mLength == 0)
            {
                mPrimType = localPrimType = mVariable.Type;
                mLength = mVariable.Length;
            }

            if (mPrimType == PrimitiveType.BUFFER)
                mMarshalType = MarshalType.Buffer;
            else if (mPrimType == PrimitiveType.STRING)
                mMarshalType = mType == typeof(String) ? MarshalType.String : MarshalType.MarshaledString;
            if (localPrimType != mVariable.Type)
            {
                if (mPrimType == PrimitiveType.ENUM32)
                    mNeedsCoercion = true;
                else if (localPrimType == PrimitiveType.STRING || localPrimType == PrimitiveType.BUFFER)
                {
                    //if ( mMarshalForSend )
                    mNeedsCoercion = true;
                }
                else if (mPrimType == PrimitiveType.OBJECT)
                {
                    //if type has IMarshal
                    //item.imarshal = def.value as IMarshal;
                    //if (item.imarshal == null)
                    MsgConsole.WriteLine("VariableMarshaler: msg {0}, var {1}, OBJECTs must implement IMarshal",
                    msgName, localName);
                }
                else
                    MsgConsole.WriteLine("VariableMarshaler: {0}.{1} incompatible with {2}",
                        msgName, mVariable.Name, localName);
            }
            mSize = mVariable.Size;
            swapBuf = new byte[mSize];

            var vi = mValueInfo.GetValue(mContainer);
            if (mType.IsArray || vi is IHostArray || vi is IArrayProxy)
            {
                mMarshalType = MarshalType.Array;
                if (mType.IsArray)
                    mType = mType.GetElementType();
                else if (vi is IHostArray)
                    mType = (vi as IHostArray).HostedArray.GetValue(0).GetType();
                else if (vi is IArrayProxy)
                    mType = (vi as IArrayProxy)[0].GetType();
            }
            if (mType.IsEnum)
                mTypeCode = TypeCode.Object;
            else
                mTypeCode = Type.GetTypeCode(mType);

            if (mXVarTypeCode == TypeCode.Empty)
                mXVarTypeCode = mVariable.TypeCode;
            if (mTypeCode != TypeCode.Empty)
                mNeedsCoercion = mTypeCode != mXVarTypeCode;
        }

        public IVariable IVariable { get { return mVariable; } }

        public int Deserialize(byte[] buffer, int offset)
        {
            int len = 0, start = offset;
            switch (mMarshalType)
            {
                case MarshalType.Array:
                    object value = mValueInfo.GetValue(mContainer);
                    if (mTypeCode == TypeCode.Byte && !mNeedsCoercion)
                    {
                        Buffer.BlockCopy(buffer, offset, value as byte[], 0, mLength);
                        len = mLength;
                    }
                    else
                    {
                        Array array = null;
                        if (value is IHostArray)
                            array = (value as IHostArray).HostedArray;
                        else if (value is IArrayProxy)
                        {
                            var ap = value as IArrayProxy;
                            for (int index = 0; index < mLength; index++)
                                ap[index] = (double)DeserializeValue(buffer, ref offset);
                            mValueInfo.SetValue(mContainer, ap);
                            len += offset - start;
                            break;
                        }
                        else
                            array = value as Array;

                        for (int index = 0; index < mLength; index++)
                            array.SetValue(DeserializeValue(buffer, ref offset), index);
                        len += offset - start;
                    }
                    break;
                case MarshalType.Atomic:
                    mValueInfo.SetValue(mContainer, DeserializeValue(buffer, ref offset));
                    len = offset - start;
                    break;
                case MarshalType.Buffer:
                    MarshaledBuffer buf = mValueInfo.GetValue(mContainer) as MarshaledBuffer;
                    len = buf.Unmarshal(buffer, offset);
                    break;
                case MarshalType.MarshaledString:
                    MarshaledString mstr = mValueInfo.GetValue(mContainer) as MarshaledString;
                    len = mstr.Unmarshal(buffer, offset);
                    mstr.String = mstr.String; // Make a copy of what's in the parse buffer
                    break;
                case MarshalType.String:
                    MarshaledString serializeString = new MarshaledString();
                    len = serializeString.Unmarshal(buffer, offset);
                    mValueInfo.SetValue(mContainer, serializeString.String);
                    break;
            }
            mVariable.NotifyOnChange();
            return len;
        }

        object DeserializeValue(byte[] buffer, ref int offset)
        {
            object value;
            if (mNeedsCoercion)
            {
                int size = mSize;
                switch (mXVarTypeCode)
                {
                    case TypeCode.Double:
                        value = System.Convert.ChangeType(GetNetwork.DOUBLE(buffer, offset, swapBuf), mTypeCode);
                        break;
                    case TypeCode.Single:
                        value = System.Convert.ChangeType(GetNetwork.FLOAT(buffer, offset, swapBuf), mTypeCode);
                        break;
                    case TypeCode.UInt64:
                        value = System.Convert.ChangeType(GetNetwork.UINT64(buffer, offset, swapBuf), mTypeCode);
                        break;
                    case TypeCode.Int64:
                        value = System.Convert.ChangeType(GetNetwork.INT64(buffer, offset, swapBuf), mTypeCode);
                        break;
                    case TypeCode.UInt32:
                        value = System.Convert.ChangeType(GetNetwork.UINT(buffer, offset, swapBuf), mTypeCode);
                        break;
                    case TypeCode.Int32:
                        value = System.Convert.ChangeType(GetNetwork.INT(buffer, offset, swapBuf), mTypeCode);
                        break;
                    case TypeCode.UInt16:
                        value = System.Convert.ChangeType(GetNetwork.USHORT(buffer, offset), mTypeCode);
                        break;
                    case TypeCode.Int16:
                        value = System.Convert.ChangeType(GetNetwork.SHORT(buffer, offset), mTypeCode);
                        break;
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        if (mTypeCode == TypeCode.String)
                        {
                            int i, len = mLength;
                            for (i = 0; i < len; i++) if (buffer[offset + i] == 0) break;
                            char[] chars = Encoding.ASCII.GetChars(buffer, offset, i);
                            value = new string(chars);
                            size = len;
                        }
                        else if (mTypeCode == TypeCode.Object)
                        {
                            if (mType.IsEnum)
                            {
                                int ival = buffer[offset]; // fixme do enums ever come in as > 1 byte?

                                // int constant to string, string to value. ugh.
                                string name = Enum.GetName(mType, ival);
                                value = Enum.Parse(mType, name);
                            }
                            else
                                value = (int)buffer[offset];
                        }
                        else
                            value = System.Convert.ChangeType(buffer[offset], mTypeCode);
                        break;
                    default:
                        throw new Exception("Variable.Deserialize: Unhandled typecode: " + mTypeCode.ToString());
                }
                offset += size;
            }
            else
            {
                switch (mXVarTypeCode)
                {
                    case TypeCode.Double:
                        value = GetNetwork.DOUBLE(buffer, offset, swapBuf); break;
                    case TypeCode.Single:
                        value = GetNetwork.FLOAT(buffer, offset, swapBuf); break;
                    case TypeCode.UInt64:
                        value = GetNetwork.UINT64(buffer, offset, swapBuf); break;
                    case TypeCode.Int64:
                        value = GetNetwork.INT64(buffer, offset, swapBuf); break;
                    case TypeCode.UInt32:
                        value = GetNetwork.UINT(buffer, offset, swapBuf); break;
                    case TypeCode.Int32:
                        value = GetNetwork.INT(buffer, offset, swapBuf); break;
                    case TypeCode.UInt16:
                        value = GetNetwork.USHORT(buffer, offset); break;
                    case TypeCode.Int16:
                        value = GetNetwork.SHORT(buffer, offset); break;
                    case TypeCode.Byte:
                        value = buffer[offset]; break;
                    case TypeCode.SByte:
                        value = (SByte)buffer[offset]; break;
                    case TypeCode.Object:
                        value = buffer[offset]; break;
                    default:
                        throw new Exception("Variable.Deserialize: Unhandled typecode: " + mXVarTypeCode.ToString());
                }
                offset += mSize;
            }
            return value;
        }

        public int Serialize(byte[] buffer, int offset, int size)
        {
            int len = 0;
            switch (mMarshalType)
            {
                case MarshalType.Array:
                    if (mTypeCode == TypeCode.Byte && !mNeedsCoercion)
                    {
                        if (mLength > size) return mLength;
                        Buffer.BlockCopy(mValueInfo.GetValue(mContainer) as byte[], 0, buffer, offset, mLength);
                        len = mLength;
                    }
                    else
                    {
                        Array array = null;
                        object obj = mValueInfo.GetValue(mContainer);
                        if (obj is IHostArray)
                            array = (obj as IHostArray).HostedArray;
                        else if (obj is IArrayProxy)
                        {
                            var ap = obj as IArrayProxy;
                            for (int index = 0; index < mLength; index++)
                                len += SerializeValue(buffer, offset + len, ap[index]);
                            break;
                        }
                        else
                            array = obj as Array;

                        for (int index = 0; index < mLength; index++)
                            len += SerializeValue(buffer, offset + len, array.GetValue(index));
                    }
                    break;
                case MarshalType.Atomic:
                    len = SerializeValue(buffer, offset, mValueInfo.GetValue(mContainer));
                    break;
                case MarshalType.Buffer:
                    MarshaledBuffer value = mValueInfo.GetValue(mContainer) as MarshaledBuffer;
                    if (value.Length > size)
                    {
                        int n = value.VarLength.Marshal(scratchBuf, 0);
                        return value.Length + n - 1;
                    }
                    len = value.Marshal(buffer, offset);
                    break;
                case MarshalType.MarshaledString:
                    MarshaledString mstr = mValueInfo.GetValue(mContainer) as MarshaledString;
                    if (mstr.Length > size)
                    {
                        int n = mstr.VarLength.Marshal(scratchBuf, 0);
                        return mstr.Length + n - 1;
                    }
                    len = mstr.Marshal(buffer, offset);
                    break;
                case MarshalType.String:
                    String str = mValueInfo.GetValue(mContainer) as String;
                    MarshaledString serializeString = new MarshaledString();
                    serializeString.Set(str);
                    if (serializeString.Length > size)
                    {
                        int n = serializeString.VarLength.Marshal(scratchBuf, 0);
                        return serializeString.Length + n - 1;
                    }
                    len = serializeString.Marshal(buffer, offset);
                    break;
            }
            return len;
        }

        int SerializeValue(byte[] buffer, int offset, object value)
        {
            int length = mSize;
            if (mNeedsCoercion)
            {
                switch (mXVarTypeCode)
                {
                    case TypeCode.Double:
                        PutNetwork.DOUBLE(buffer, offset, (double)System.Convert.ChangeType(value, mXVarTypeCode), swapBuf);
                        break;
                    case TypeCode.Single:
                        PutNetwork.FLOAT(buffer, offset, (float)System.Convert.ChangeType(value, mXVarTypeCode), swapBuf);
                        break;
                    case TypeCode.UInt64:
                        PutNetwork.UINT64(buffer, offset, (UInt64)System.Convert.ChangeType(value, mXVarTypeCode), swapBuf);
                        break;
                    case TypeCode.Int64:
                        PutNetwork.INT64(buffer, offset, (Int64)System.Convert.ChangeType(value, mXVarTypeCode), swapBuf);
                        break;
                    case TypeCode.UInt32:
                        PutNetwork.UINT(buffer, offset, (uint)System.Convert.ChangeType(value, mXVarTypeCode));
                        break;
                    case TypeCode.Int32:
                        PutNetwork.INT(buffer, offset, (int)System.Convert.ChangeType(value, mXVarTypeCode));
                        break;
                    case TypeCode.UInt16:
                        PutNetwork.USHORT(buffer, offset, (ushort)System.Convert.ChangeType(value, mXVarTypeCode));
                        break;
                    case TypeCode.Int16:
                        PutNetwork.SHORT(buffer, offset, (short)System.Convert.ChangeType(value, mXVarTypeCode));
                        break;
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                        if (mTypeCode == TypeCode.String)
                        {
                            char[] chars = (value as string).ToCharArray();
                            byte[] bytes = Encoding.ASCII.GetBytes(chars);
                            if (bytes.Length + 1 <= mLength)
                            {
                                Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
                                buffer[offset + bytes.Length] = 0;
                            }
                            else
                            {
                                Buffer.BlockCopy(bytes, 0, buffer, offset, mLength - 1);
                                buffer[offset + mLength - 1] = 0;
                            }
                            length *= mLength;
                        }
                        //else if (value.GetType().IsEnum)
                        //    PutNetwork.INT(buffer,offset,(int)value);
                        else
                            buffer[offset] = (byte)System.Convert.ChangeType(value, mXVarTypeCode);
                        break;
                    default:
                        throw new Exception("Variable.Serialize: Unhandled typecode: " + mXVarTypeCode.ToString());
                }
            }
            else
            {
                switch (mXVarTypeCode)
                {
                    case TypeCode.Double:
                        PutNetwork.DOUBLE(buffer, offset, (double)value, swapBuf);
                        break;
                    case TypeCode.Single:
                        PutNetwork.FLOAT(buffer, offset, (float)value, swapBuf);
                        break;
                    case TypeCode.UInt64:
                        PutNetwork.UINT64(buffer, offset, (UInt64)value, swapBuf);
                        break;
                    case TypeCode.Int64:
                        PutNetwork.INT64(buffer, offset, (Int64)value, swapBuf);
                        break;
                    case TypeCode.UInt32:
                        PutNetwork.UINT(buffer, offset, (uint)value);
                        break;
                    case TypeCode.Int32:
                        PutNetwork.INT(buffer, offset, (int)value);
                        break;
                    case TypeCode.UInt16:
                        PutNetwork.USHORT(buffer, offset, (ushort)value);
                        break;
                    case TypeCode.Int16:
                        PutNetwork.SHORT(buffer, offset, (short)value);
                        break;
                    case TypeCode.Byte:
                        buffer[offset] = (byte)value;
                        break;
                    case TypeCode.SByte:
                        buffer[offset] = (byte)((sbyte)value);
                        break;
                    default:
                        throw new Exception("Variable.Serialize: Unhandled typecode: " + mXVarTypeCode.ToString());
                }
            }
            return length;
        }

        public override string ToString()
        {
            if (mVariable == null)
                return GetType().ToString();
            return mVariable.Name;
        }
    }
    internal interface IValueInfo
    {
        object GetValue(object content);
        object GetValue(object content, object[] index);
        void SetValue(object content, object value);
        void SetValue(object content, object value, int index);
        bool IsValueType { get; set; }
        IArrayProxy GetArrayProxy(object content);
    }
    internal class FieldValueInfo : IValueInfo
    {
        FieldInfo info;
        internal FieldValueInfo(FieldInfo fi) { info = fi; if (fi.FieldType.IsValueType) IsValueType = true; }
        public object GetValue(object content) { return info.GetValue(content); }
        public object GetValue(object content, object[] index) { return info.GetValue(content); }
        public void SetValue(object content, object value) { info.SetValue(content, value); }
        public void SetValue(object content, object value, int index) { info.SetValue(content, value); }
        public bool IsValueType { get; set; }
        public IArrayProxy GetArrayProxy(object content) { return info.GetValue(content) as IArrayProxy; }
    }
    internal class PropertyValueInfo : IValueInfo
    {
        PropertyInfo info;
        object[] indexes = new object[1];

        internal PropertyValueInfo(PropertyInfo pi) { info = pi; if (pi.PropertyType.IsValueType) IsValueType = true; }
        public object GetValue(object content) { return info.GetValue(content, null); }
        public object GetValue(object content, object[] index) { return info.GetValue(content, index); }
        public void SetValue(object content, object value) { info.SetValue(content, value, null); }
        public void SetValue(object content, object value, int index)
        {
            indexes[0] = index;
            info.SetValue(content, value, indexes);
        }
        public bool IsValueType { get; set; }
        public IArrayProxy GetArrayProxy(object content) { return null; }
    }
    internal class DataItemValueInfo : IValueInfo
    {
        Blackboard.Item mBlackboardItem;
        internal DataItemValueInfo(Blackboard.Item item)
        {
            mBlackboardItem = item;
        }
        public object GetValue(object content) { return mBlackboardItem.Value; }
        public object GetValue(object content, object[] index) { return mBlackboardItem.Value; }
        public void SetValue(object content, object value) { mBlackboardItem.Value = value; }
        public void SetValue(object content, object value, int index) { mBlackboardItem.Value = value; }
        public bool IsValueType { get; set; }
        public IArrayProxy GetArrayProxy(object content) { return null; }
    }
}
