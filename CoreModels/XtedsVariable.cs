using System;
//using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;
using Aspire.Core.Messaging;
//using Aspire.Core.Utilities;
//using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	/// <summary>
	/// 
	/// </summary>
	public class XtedsVariableItem
	{
		Blackboard.Item blackboardItem;
		XtedsVariable xtedsVariable;
		internal TypeCode typeCode = TypeCode.Empty;
		internal bool needsCoercion, valid;

		public XtedsVariableItem( XtedsVariable xVar, Blackboard.Item bi, TypeCode tc )
		{
			xtedsVariable = xVar;
			blackboardItem = bi;
			typeCode = tc;
		}

		public bool SetBytes( byte[] bytes, int start )
		{
			if ( blackboardItem == null ) return true;
			try
			{
				switch ( typeCode )
				{
					case TypeCode.Byte:
					case TypeCode.SByte:
						blackboardItem.Value = bytes[start];
						break;
					case TypeCode.Int16:
						blackboardItem.Value = GetNative.SHORT(bytes,start);
						break;
					case TypeCode.UInt16:
						blackboardItem.Value = GetNative.USHORT(bytes,start);
						break;
					case TypeCode.Int32:
						blackboardItem.Value = GetNative.INT(bytes, start);
						break;
					case TypeCode.UInt32:
						blackboardItem.Value = GetNative.UINT(bytes, start);
						break;
					case TypeCode.Single:
						if (xtedsVariable.LogToConsole)
							Log.WriteLine("{0} {1}",xtedsVariable.Path,BitConverter.ToString(bytes,start,4));
						float f = GetNative.FLOAT(bytes, start);
						if (float.IsNaN(f))
						{
							f = 0;
							nanCount++;
							Log.WriteLine("{0} is NaN", xtedsVariable.Path);
						}
						blackboardItem.Value = f;
						break;
					case TypeCode.Double:
						if (xtedsVariable.LogToConsole)
							Log.WriteLine("{0} {1}", xtedsVariable.Path, BitConverter.ToString(bytes, start, 8));
						double d = GetNative.DOUBLE(bytes, start);
						if (double.IsNaN(d))
						{
							d = 0;
							nanCount++;
							Log.WriteLine("{0} is NaN", xtedsVariable.Path);
						}
						blackboardItem.Value = d;
						break;
				}
			}
			catch (System.ArgumentOutOfRangeException)
			{
				return false;
			}
			catch (System.Exception){}

			//	Timing.SectionTimeTag("HWIL",Name+" SetBytes");
			return true;
		}

		public int NanCount
		{
			get { return nanCount; }
			set { nanCount = value; }
		} int nanCount;
		#region Conversion

		public byte ToByte()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (byte)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (byte)blackboardItem.Value;
			}
			else
				return 0;
		}

		public SByte ToSByte()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (SByte)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (SByte)blackboardItem.Value;
			}
			else
				return 0;
		}

		public short ToShort()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (short)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (short)blackboardItem.Value;
			}
			else
				return 0;
		}

		public int ToInt()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (int)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (int)blackboardItem.Value;
			}
			else
				return 0;
		}

		public float ToFloat()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (float)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (float)blackboardItem.Value;
			}
			else
				return 0;
		}

		public double ToDouble()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (double)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (double)blackboardItem.Value;
			}
			else
				return 0;
		}

		public ushort ToUShort()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (ushort)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (ushort)blackboardItem.Value;
			}
			else
				return 0;
		}

		public uint ToUInt()
		{
			if ( valid )
			{
				if ( needsCoercion )
					return (uint)System.Convert.ChangeType(blackboardItem.Value, typeCode );
				else
					return (uint)blackboardItem.Value;
			}
			else
				return 0;
		}

		#endregion

		public override string ToString()
		{
			return xtedsVariable.ToString(blackboardItem);
		}

		public object Value
		{
			get { return blackboardItem.Value; }
			set { blackboardItem.Value = value; }
		}

	};

	/// <summary>
	/// 
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class XtedsVariable
	{
		IUseXteds mXtedsUser;

		public void Bind( Xteds.Interface.Variable xiVar, IUseXteds xtedsUser )
		{
			// keep a reference to the Xteds.Interface.Variable
			m_Variable = xiVar;
			mXtedsUser = xtedsUser;

			typeCode = TypeCode.Empty;
			if ( xiVar.IsNotified && update == UpdateType.Event )
				this.update = UpdateType.Polled;
			byte len;
			if ( xiVar.Format == Xteds.Interface.Variable.FormatType.Unknown )
			{
				if (dictionaryName.Length > 0)
				{
					boundItem = xtedsUser.BlackboardSubscribe(dictionaryName);
					typeCode = Type.GetTypeCode(boundItem.Type);
					xiVar.Format = Xteds.Interface.Variable.GetFormatType(typeCode, out len);
					xiVar.Length = boundItem.Items.Count;
				}
				else
					len = 0;
			}
			else
				typeCode = Xteds.Interface.Variable.GetTypeCode(xiVar.Format, out len);

			length = len;
			format = xiVar.Format;
			arrayLength = xiVar.Length;

			if (boundItem == null && dictionaryName.Length > 0)
				boundItem = xtedsUser.BlackboardSubscribe(dictionaryName);

			if (boundItem.IsArray)
			{
				items = new XtedsVariableItem[boundItem.Items.Count];
				int i = 0;
				foreach (Blackboard.Item di in boundItem.Items)
					items[i++] = new XtedsVariableItem( this, di, typeCode );
			}
			else
			{
				items = new XtedsVariableItem[1];
				items[0] = new XtedsVariableItem(this, boundItem, typeCode);
			}
		}

		public bool IsNonXteds
		{
			get { return m_Variable.Kind == "NonXteds"; }
		}

		public void ResetFormat(Xteds.Interface.Variable.FormatType newFormat, XtedsVariableMap xvMap)
		{
			byte len;
			var typeCode = Xteds.Interface.Variable.GetTypeCode(newFormat, out len);
			format = newFormat;
			foreach (var item in items)
				item.typeCode = typeCode;
			Validate(xvMap.ParentModel);
			length = len;
		}

		public XtedsVariableItem Item( int index )
		{
			return items[index];
		}
		XtedsVariableItem[] items;

		[XmlIgnore]
		public bool ShowAsHex
		{
			get { return showAsHex; }
			set { showAsHex = value; }
		} bool showAsHex;

		[XmlIgnore]
		public bool LogToConsole
		{
			get { return logToConsole; }
			set { logToConsole = value; }
		} bool logToConsole;

		#region Properties

		private Xteds.Interface.Variable m_Variable = null;

		/// <summary>
		/// Gets the <see cref="Xteds.Interface.Variable"/> to which this is attached.
		/// </summary>
		[XmlIgnore]
		public Xteds.Interface.Variable Variable
		{
			get { return(m_Variable); }
		}
		
		[XmlIgnore]
		public int ArrayLength{ get { return arrayLength; } } int arrayLength;

		public Blackboard.Item BoundItem { get { return boundItem; } }

		[XmlAttribute("name")]
		public string Name
		{
			get { return name; }
			set { name = value; }
		} string name = string.Empty;

		[XmlAttribute("count")]
		[DefaultValue(1)]
		public int Count
		{
			get { return count; }
			set { count = value; }
		} int count = 1;

		[XmlAttribute("dictionary")]
		public string DictionaryName
		{
			get { return dictionaryName; }
			set { dictionaryName = value; }
		} string dictionaryName = string.Empty;
		Blackboard.Item boundItem;

		[XmlAttribute("dir")]
		[DefaultValue(0)]
		public byte Dir
		{
			get { return dir; }
			set { dir = value; dirIsSet = true; }
		} byte dir;

		internal bool IsDirSet{ get { return dirIsSet; } } bool dirIsSet;
		[XmlAttribute("reg")]
		[DefaultValue(0)]
		public byte Reg
		{
			get { return reg; }
			set { reg = value; }
		} byte reg;

		[XmlAttribute("reply")]
		[DefaultValue("")]
		public string Reply
		{
			get { return reply; }
			set { reply = value; }
		} string reply = string.Empty;

		[XmlAttribute("epsilon")]
		[DefaultValue(Double.Epsilon)]
		public double Epsilon
		{
			get { return epsilon; }
			set
			{
				if ( value < 0 ) return;
				epsilon = value;
			}
		} double epsilon = Double.Epsilon;

		[XmlAttribute("expected")]
		[DefaultValue("")]
		public string Expected
		{
			get { return expected; }
			set { expected = value; }
		} string expected = string.Empty;


		public bool IsArray{ get { return arrayLength > 1&& Name.IndexOf('[')<0; } }

		public bool IsBound { get { return boundItem != null; } }

		public int Length { get { return length; } } int length;

		[XmlAttribute("inXteds"),DefaultValue(true)]
		public bool InXteds
		{
			get { return inXteds; }
			set { inXteds = value; }
		} bool inXteds = true;

//		public TypeCode TypeCode { get { return typeCode; } }
		internal TypeCode typeCode = TypeCode.Empty;
		internal Xteds.Interface.Variable.FormatType format;

		public enum UpdateType
		{
			Event,
			Polled,
			Bidirectional
		}

		[XmlAttribute("update")]
		[DefaultValue(UpdateType.Event)]
		public UpdateType Update
		{
			get { return update; }
			set { update = value; }
		} UpdateType update = UpdateType.Event;

		public string Path
		{
			get
			{
				if (IsBound && mXtedsUser.Parent != null)
					return mXtedsUser.Parent.Name+'.'+Name;
				else
					return Name;
			}
		}

		public override string ToString()
		{
			if ( IsBound )
			{
				if (boundItem.IsArray)
				{
					string values = string.Empty;
					foreach (Blackboard.Item item in boundItem.Items)
					{
						if ( values.Length > 0 ) values += ",";
						values += ToString(item);
					}
					return values;
				}
				else
					return ToString(boundItem);
			}
			else
				return GetType().Name;
		}

		internal string ToString( Blackboard.Item item )
		{
			if ( showAsHex )
			{
				switch ( items[0].typeCode )
				{
					case TypeCode.Byte:
						return String.Format("{0:X2}",Convert.ChangeType(item.Value,typeof(byte) ) );
					case TypeCode.Int16:
						return String.Format("{0:X4}",Convert.ChangeType(item.Value,typeof(short) ) );
					case TypeCode.UInt16:
						return String.Format("{0:X4}",Convert.ChangeType(item.Value,typeof(ushort) ) );
					case TypeCode.Int32:
						return String.Format("{0:X8}",Convert.ChangeType(item.Value,typeof(int) ) );
					case TypeCode.UInt32:
						return String.Format("{0:X8}",Convert.ChangeType(item.Value,typeof(uint) ) );
					case TypeCode.Single:
						byte[] buff = new byte[4];
						PutNative.FLOAT(buff,0,(float)Convert.ChangeType(item.Value,typeof(float) ));
						int val = GetNative.INT(buff,0);
						return String.Format("{0:X8}",val );
					case TypeCode.Double:
						byte[] bufd = new byte[8];
						PutNative.DOUBLE(bufd, 0, (double)item.Value);
						int val1 = GetNative.INT(bufd,0);
						int val2 = GetNative.INT(bufd,4);
						return String.Format("{0:X8}{1:X8}",val2,val1 );
					default:
						return item.Value.ToString();
				}
			}
			else
				return item.Value==null?string.Empty:item.Value.ToString();
		}

		[XmlIgnore]
		public object Value
		{
			get
			{
				if (boundItem != null)
					return boundItem.Value;
				else
					return null;
			}
			set
			{
				if (boundItem != null)
					boundItem.Value = value;
			}
		}

		[XmlIgnore]
		public Array Values
		{
			get
			{
				if (boundItem != null && boundItem.IsArray)
					return boundItem.Value as Array;
				else
					return null;
			}
			set
			{
				if (boundItem != null && boundItem.IsArray)
					value.CopyTo(boundItem.Value as Array, 0);
			}
		}

		#endregion Properties

		public void Validate(Model parent)
		{
			bool needsCoercion = false;
			if (boundItem != null && boundItem.Value != null)
			{
				if (boundItem.IsPrimitive)
				{
					if ( IsArray )
					{
						Log.WriteLine("xTEDSVar", Log.Severity.Warning,
						"{0}.{1} is dimensioned [{2}] {3} is primitive. Edit XtedsVarMap in \n{4}",
							parent.Name, Name, arrayLength, boundItem, parent.FileNamePath);
						return;
					}
					if (Type.GetTypeCode(boundItem.Type) != items[0].typeCode)
						needsCoercion = true;
				}
				else if (boundItem.Type.IsEnum)
					needsCoercion = true;
				else if (boundItem.IsArray && !IsArray)
				{
					Log.WriteLine("xTEDSVar", Log.Severity.Warning,
					"{0}.{1} is primitive, {2} is dimensioned [{3}]. Edit XtedsVarMap in \n{4}",
						parent.Name, Name, boundItem, boundItem.GetArray().Length, parent.FileNamePath);
					return;
				}
				else if (boundItem.Items.Count == arrayLength && arrayLength > 0)
				{
					if (Type.GetTypeCode(boundItem.Items[0].Type) != items[0].typeCode)
						needsCoercion = true;
				}
				foreach ( XtedsVariableItem item in items )
				{
					item.needsCoercion = needsCoercion;
					item.valid = true;
				}
			}
			else
				Log.WriteLine("xTEDSVar", Log.Severity.Warning,
				"{0}.{1} {2} is not published. Edit XtedsVarMap in \n{3}",
				parent.Name, Name, boundItem==null?dictionaryName:boundItem.ToString(), parent.FileNamePath);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class XtedsVariableList : List<XtedsVariable>
	{
		public override string ToString() { return GetType().Name; }
	}

	/// <summary>
	/// 
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class XtedsVariableMap
	{
		XtedsVariableList list = new XtedsVariableList ();
		Dictionary<string,XtedsVariable> dict;
		IUseXteds mXtedsUser;

		public void Add(XtedsVariable variable)
		{
			list.Add(variable);
			if ( dict != null )
				dict.Add(variable.Name, variable);
		}

		[Description("Array of XtedsVariables")]
		public XtedsVariableList Variables
		{
			get { return list; }
			set { list = value; }
		}
	
		[XmlAttribute("mustBeInXteds"),DefaultValue(true)]
		public bool MustBeInXteds
		{
			get { return mustBeInXteds; }
			set { mustBeInXteds = value; }
		} bool mustBeInXteds = true;

		public XtedsVariable this[string name]
		{
			get
			{
				if ( dict == null )
				{
					dict = new Dictionary<string,XtedsVariable>();
					foreach (XtedsVariable xVar in Variables)
					{
						if (dict.ContainsKey(xVar.Name))
							Log.WriteLine("{0} already contains a mapped xteds variable {1}",
								mXtedsUser.Parent.Name, xVar.Name);
						else
							dict.Add(xVar.Name, xVar);
					}
				}
				string searchName;
				if ( name.EndsWith("]") )
					searchName = name.Substring(0,name.IndexOf('['));
				else
					searchName = name;
				XtedsVariable value;
				dict.TryGetValue(searchName, out value);
				return value; 
			}
		}

		public void Bind(IUseXteds xtedsUser)
		{
			mXtedsUser = xtedsUser;
			foreach ( XtedsVariable xVar in Variables )
			{
				Xteds.Interface.Variable xiVar;
				if (xVar.InXteds)
					xiVar = mXtedsUser.Xteds.FindVariable(xVar.Name);
				else
					xiVar = null;
				if ( xiVar != null )
					xVar.Bind(xiVar, mXtedsUser);
				else if (mustBeInXteds)
					Log.WriteLine("{0}'s xTEDS variable {1} not in {2} xTEDS",
						mXtedsUser.Path, xVar.Name, mXtedsUser.XtedsFile);
				else
				{
					xiVar = new Xteds.Interface.Variable(); // Xteds.NewVariable();
					xiVar.ParentInterface = null;
					xiVar.Kind = "NonXteds";
					xVar.Bind(xiVar, mXtedsUser);
				}
			}
		}

		public Model ParentModel
		{
			get { return mXtedsUser.ParentModel; }
		}

		public void Print()
		{
			foreach ( XtedsVariable xVar in list )
				Log.WriteLine(String.Empty, Log.Severity.Warning, "\t{0}", xVar.Name);
		}

		[XmlIgnore]
		public bool ShowAsHex
		{
			get { return showAsHex; }
			set
			{
				showAsHex = value;
				foreach ( XtedsVariable xVar in list )
					xVar.ShowAsHex = value;
				mXtedsUser.ChangeProperties("TestBypassMap");
			}
		} bool showAsHex;

		public override string ToString() { return GetType().Name; }

		public void Validate()
		{
			if (mXtedsUser == null) return;
			var parent = mXtedsUser.Parent as Model;
			foreach ( XtedsVariable xVar in list )
				xVar.Validate(parent);
		}
	}
}
