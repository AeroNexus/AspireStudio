using System;
using System.ComponentModel;
//using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

using Aspire.Framework;
//using GuiUtilities.DataDictionaryTreeFilter;
using Aspire.Primitives;
using au=Aspire.Utilities;

using Aspire.Core;
using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;

namespace Aspire.CoreModels
{
	/// <summary>
	/// Xteds holds a parsed xTEDS.
	/// </summary>
	[XmlRoot("xTEDS")]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Xteds : IXteds
	{
		static object mLoaderLock = new object();

		public Xteds()
		{
			maxInterfaceId = 0;
		}

		private Xteds(IHostXtedsLite host)
			: this()
		{
			mLiteHost = host;
		}

		static Xteds()
		{
			aspireSerializer.UnknownAttribute += new XmlAttributeEventHandler(UnknownAttribute);
			aspireSerializer.UnknownElement += new XmlElementEventHandler(UnknownElement);
			aspireSerializer.UnknownNode += new XmlNodeEventHandler(UnknownNode);
			aspireSerializer.UnreferencedObject += new UnreferencedObjectEventHandler(UnreferencedObject);
			IXteds.SetParseHandler(new ParseHandler(ParseTextCheck));
		}
		internal static int maxInterfaceId;
		static XmlSerializer aspireSerializer = new XmlSerializer(typeof(Xteds), "http://www.PnPInnovations.com/Aspire/xTEDS");
		static XmlSerializer ssmSerializer;

		[XmlIgnore]
		public IHostXteds Host{ get { return mLiteHost as IHostXteds; } }
		IHostXtedsLite mLiteHost;

		void AddTestBypass(Interface.Variable var)
		{
			if (testBypassVariables == null)
				testBypassVariables = new List<Interface.Variable>();
			testBypassVariables.Add(var);
		} List<Interface.Variable> testBypassVariables;
		public List<Interface.Variable> TestBypassVariables { get { return testBypassVariables; } }

		void Bind()
		{
			interfaceById = new Interface[maxInterfaceId+1];
			if (interfaces == null) return;
			Dictionary<string, int> duplicateNameCount = new Dictionary<string, int>();
			foreach ( Interface iface in interfaces )
			{
				if ( interfaceById[iface.Id] != null )
					MsgConsole.WriteLine("Xteds.Bind", MsgLevel.Warning,
						"file, {0}, interface {1}, has the same id as interface {2}",
						filePath, iface.Name, interfaceById[iface.Id].Name);
				interfaceById[iface.Id] = iface;
				interfaceByName[iface.Name] = iface;
				
				iface.Bind(this);
				iface.DistinguishDuplicates(duplicateNameCount);
			}
		}

		Interface[] interfaceById = new Interface[0];

		public override void ChangeAllCommandProviders(Address newAddress)
		{
			foreach (var iface in interfaces)
			{
				if (iface.CommandMessages == null) continue;
				foreach (var msg in iface.CommandMessages)
					msg.Provider = newAddress;
			}
		}

		public Interface FindInterface(int id)
		{
			return id < interfaceById.Length?interfaceById[id]:null;
		}

		public Interface FindOrAddInterface(int id, string interfaceName)
		{
			Interface iface = FindInterface(id);
			lock (mLoaderLock)
			{
				if (iface == null)
				{
					iface = new Interface(this);
					iface.Id = (byte)id;
					iface.Name = interfaceName != null ? interfaceName : string.Format("interface{0}", iface.Id);
					interfaceByName.Add(iface.Name, iface);
					Interface[] newInterfaces = new Interface[interfaces.Length+1];
					interfaces.CopyTo(newInterfaces, 0);
					newInterfaces[interfaces.Length] = iface;
					interfaces = newInterfaces;
					if (maxInterfaceId < iface.Id)
						maxInterfaceId = iface.Id;
					Interface[] newInterfaceById = new Interface[maxInterfaceId+1];
					interfaceById.CopyTo(newInterfaceById, 0);
					newInterfaceById[iface.Id] = iface;
					interfaceById = newInterfaceById;
				}
			}
			return iface;
		}

		public override IDataMessage FindDataMessage(string interfaceName, string messageName)
		{
			return FindXtedsMessage(interfaceName, messageName) as IDataMessage;
		}

		public Interface.Message FindMessage( MessageId messageId )
		{
			if (messageId.Interface < interfaceById.Length)
			{
				var iface = interfaceById[messageId.Interface];
				if (iface != null)
					return iface.FindMessage(messageId.Message);
				else
					return null;
			}
			else
				return null;
		}

		public Interface.Message FindMessage(string messageName)
		{
			foreach (var iface in interfaces)
			{
				var msg = iface.FindMessage(messageName);
				if (msg != null)
					return msg;
			}
			if (!QuietFind)
				MsgConsole.WriteLine("Message {0}, is not in xteds {1}",
					messageName, Name);
			return null;
		}

		public Interface.Message FindMessage(string interfaceName, string messageName)
		{
			Interface iface;

			if ( interfaceByName.TryGetValue(interfaceName, out iface) )
			{
				var msg = iface.FindMessage(messageName);
				if (msg == null && !QuietFind)
					MsgConsole.WriteLine("Message {0}, is not in xteds {1}, interface {2}",
						messageName, Name, interfaceName);
				return msg;
			}
			else
			{
				if(!QuietFind)
					MsgConsole.WriteLine("Interface {0} is not in xTEDS {1}",
						interfaceName, Name);
				return null;
			}
		}
		Dictionary<string, Interface> interfaceByName = new Dictionary<string, Interface>();

		public override XtedsMessage FindMessage(int selector, bool nag=true)
		{
			int interfaceId = selector>>8;
			if (interfaceId > 0 && interfaceId < interfaceById.Length)
			{
				Interface iface = FindInterface(interfaceId);
				if (iface != null)
					return iface.FindMessage((byte)(selector&0xff));
				MsgConsole.WriteLine("Xteds::FindMessage: Bad message id {0}", selector & 0xff);
			}
			else if (interfaceId > 0 && nag)
				MsgConsole.WriteLine("Xteds::FindMessage: Bad interface id {0}", interfaceId);
			return null;
		}
		public override void AddMessage(XtedsMessage xMsg)
		{
			var msg = xMsg as Interface.Message;
			var iface = FindOrAddInterface(xMsg.MessageId.Interface,msg.OwnerInterface.Name);
			iface.AddMessage(msg);
		}

		public override XtedsMessage FindMessage(MarshaledBuffer messageSpec)
		{
			int offset = messageSpec.Offset, notificationOffset=0;
			byte[] bytes = messageSpec.Bytes;
			//StructuredResponse.PrintOpStr(messageSpec);
			for (int loc=offset; bytes[loc]>0; loc++)
			{
			Reparse:
				int op = bytes[loc+notificationOffset];
				switch (op)
				{
					case (int)StructuredQuery.sq.Interface:
						if (bytes[loc+notificationOffset+1] == (int)StructuredQuery.sq.Id)
						{
							lock (mLoaderLock)
							{
								int nameLen = 0;
								var iface = FindOrAddInterface(bytes[loc+notificationOffset+2],null);
								if (iface == null)
								{
									iface = new Interface(this);
									iface.Id = bytes[loc+notificationOffset+2];
									if (bytes[loc + notificationOffset + 3] == (int)StructuredQuery.sq.Name)
									{
										int len = 0;
										while (bytes[loc + notificationOffset + 4 + len] != 0) len++;
										iface.Name = new string(ASCIIEncoding.ASCII.GetChars(bytes, loc + notificationOffset + 4, len));
										nameLen = iface.Name.Length + 1 + 1;
									}
									else
										iface.Name = string.Format("interface{0}", iface.Id);
									interfaceByName.Add(iface.Name, iface);
									Interface[] newInterfaces = new Interface[interfaces.Length+1];
									interfaces.CopyTo(newInterfaces, 0);
									newInterfaces[interfaces.Length] = iface;
									interfaces = newInterfaces;
									if (maxInterfaceId < iface.Id)
										maxInterfaceId = iface.Id;
									Interface[] newInterfaceById = new Interface[maxInterfaceId+1];
									interfaceById.CopyTo(newInterfaceById, 0);
									newInterfaceById[iface.Id] = iface;
									interfaceById = newInterfaceById;
								}
								else if (bytes[loc + notificationOffset + 3] == (int)StructuredQuery.sq.Name)
								{
									int len = 0;
									while (bytes[loc + notificationOffset + 4 + len] != 0) len++;
									nameLen = len + 1 + 1;
								}
								var xmsg = iface.FindMessage(bytes, loc+notificationOffset+3+nameLen);
								if (xmsg is Interface.DataMessage)
									(xmsg as Interface.DataMessage).Parse(bytes, loc);
								Interface.currentInterface = null;
								Interface.Message.currentMessage = null;
								return xmsg;
							}
						}
						break;
					case (int)StructuredQuery.sq.MsgArrival:
						notificationOffset = bytes[loc+1] == (byte)Interface.Message.Arrival.PERIODIC ? 4 + bytes[loc+3]: 2;
						goto Reparse;
				}
			}

			return null;
		}

		public override XtedsMessage FindXtedsMessage(string interfaceName, string messageName)
		{
			return FindMessage(interfaceName, messageName) as XtedsMessage;
		}

		public Interface.Variable FindVariable(string name)
		{
			string searchName;
			if ( name.EndsWith("]") )
				searchName = name.Substring(0,name.IndexOf('['));
			else
				searchName = name;
			if ( varDict == null )
			{
				varDict = new Dictionary<string, Interface.Variable>();
				if ( interfaces != null )
					foreach ( Interface iface in Interfaces )
						foreach ( Interface.Variable var in iface.Variables )
							if ( !varDict.ContainsKey(var.Name) )
								varDict.Add(var.Name,var);
			}
			Interface.Variable iv;
			varDict.TryGetValue(searchName, out iv);
			return iv;
		} Dictionary<string,Interface.Variable> varDict;

		static void InitializeSsm()
		{
			if (ssmSerializer != null) return;
			ssmSerializer = new XmlSerializer(typeof(Xteds), "https://pnpsoftware.sdl.usu.edu/redmine/projects/xtedsschema");
			ssmSerializer.UnknownAttribute += new XmlAttributeEventHandler(UnknownAttribute);
			ssmSerializer.UnknownElement += new XmlElementEventHandler(UnknownElement);
			ssmSerializer.UnknownNode += new XmlNodeEventHandler(UnknownNode);
			ssmSerializer.UnreferencedObject += new UnreferencedObjectEventHandler(UnreferencedObject);
		}

		public override List<IDataMessage> NotificationMessages()
		{
			var list = new List<IDataMessage>();
			if ( Interfaces != null )
				foreach (var iface in Interfaces)
					if ( iface.Messages != null )
						foreach (var msg in iface.Messages)
							if ( msg is IDataMessage && msg.MessageType is Interface.Notification)
								list.Add(msg as IDataMessage);
			return list;
		}

		private static IXteds ParseTextCheck(string text, string filePath, IHostXtedsLite host)
		{
			if ( text == null )
				lock (mLoaderLock)
					return new Xteds(host);
			return Parse(text, filePath, host);
		}

		static bool useSsm;

		public static Xteds Parse( string text, string filePath, IHostXtedsLite liteHost )
		{
			if (text == null)
				lock (mLoaderLock)
					return new Xteds(liteHost);
			string text1;
			if (text[text.Length-1] == '\0')
				text1 = text.Substring(0, text.Length-1);
			else
				text1 = text;
			bool unexpectedNull = false;
			Xteds xteds = null;

			lock (mLoaderLock)
			{
			Again:
				try
				{
					var sr = new StringReader(text1);
					if (useSsm)
					{
						useSsm = false;
						xteds = ssmSerializer.Deserialize(sr) as Xteds;
					}
					else
						xteds = aspireSerializer.Deserialize(sr) as Xteds;
				}
				catch (XmlException e)
				{
					if (e.Message.Contains("hexadecimal value 0x00"))
					{
						text1 = text1.Substring(0, text1.IndexOf('\0'));
						unexpectedNull = true;
						goto Again;
					}
					MsgConsole.ReportException("Xteds.Parse", MsgLevel.Warning, e);
					return null;
				}
				catch (InvalidOperationException e)
				{
					if ( e.InnerException.Message.Contains("https://pnpsoftware.sdl.usu.edu/redmine/projects/xtedsschema") )
					{
						InitializeSsm();
						useSsm = true;
						goto Again;
					}
					else
						MsgConsole.ReportException(string.Format("Xteds.Parse file:{0}", filePath), MsgLevel.Warning, e);
					return null;
				}
				catch (Exception e)
				{
					MsgConsole.ReportException(string.Format("Xteds.Parse file:{0}", filePath), MsgLevel.Warning, e);
					return null;
				}

				xteds.FilePath = filePath;
				xteds.mLiteHost = liteHost;
				xteds.Bind();
			}
			Interface.currentInterface = null;
			Interface.Message.currentMessage = null;
			if (unexpectedNull)
				MsgConsole.WriteLine("Xteds.Parse({0}) encountered unexpected null at [{1}]",
					xteds.Component.Name,text1.Length);
			return xteds;
		}

		public void Publish(string parentPath)
		{
			string path = parentPath+'.'+Component.Name+" "+Host.Name;
			var attr = new BlackboardAttribute();
			attr.Description = this.Description;
			var di = Blackboard.Publish(this, path, new ObjectValueInfo(this));
			di.AddAttributeInfo(attr);
			if ( interfaces != null )
				foreach ( Interface iface in interfaces )
					iface.Publish(path);
		}

		public void PublishInterfaces(string parentPath)
		{
			if (interfaces == null) return;
			string path;
			if (parentPath.Length == 0)
			{
				path = parentPath = Host.Name;
			}
			else
				path = parentPath != Host.Name ? parentPath+'.'+Host.Name : Host.Name;
			Blackboard.CollectSubItems = parentPath;
			foreach ( Interface iface in interfaces )
				iface.Publish(path);
			Blackboard.CollectSubItems = null;
		}

		[XmlIgnore]
		public bool QuietFind { get; set; }

		public void Unpublish(string parentPath)
		{
			string path = parentPath + '.' + Component.Name + " " + Host.Name;
			foreach (Interface iface in interfaces)
				iface.Unpublish(path);
			Blackboard.Unpublish(path);
		}

		public void UnpublishInterfaces(string parentPath)
		{
			string path = parentPath+'.'+Host.Name;
			if ( interfaces != null )
				foreach ( Interface iface in interfaces )
					iface.Unpublish(path);
		}

		public override void VisitAllPublishers(ChangePublisherState OnChangePublisherState, bool active, Address newAddress=null)
		{
			foreach ( var iface in interfaces )
				if ( iface.DataMessages != null )
					foreach (Interface.DataMessage msg in iface.DataMessages)
					{
						if ( newAddress != null )
							msg.Provider = newAddress;
						OnChangePublisherState(msg, active);
					}
		}

		public void Save( string fileName )
		{
			au.FileUtilities.WriteToXml(this,fileName,true,null );
		}

		public void VerifyVariableMapping()
		{
			if (interfaces == null) return;

			foreach ( var iface in interfaces)
				foreach ( var msg in iface.messages )
					if ( msg != null ) 
						msg.VerifyVariableMapping();
		}

		#region Properties

		[XmlAttribute("name")]
		public string Name
		{
			get { return name; }
			set { name = value; }
		} string name;

		[XmlIgnore]
		public string FilePath
		{
			get { return filePath; }
			set { filePath = value; }
		} string filePath;

		[XmlAttribute("description")]
		public string Description
		{
			get { return description;}
			set { description = value;}
		} string description;

		[XmlAttribute("version")]
		public string Version
		{
			get { return version; }
			set { version = value; }
		} string version;

		public AppInfo Application
		{
			get { return application; }
			set { component = application = value; }
		} AppInfo application;

		public DevInfo Device
		{
			get { return device; }
			set { component = device = value; }
		} DevInfo device;

		[XmlIgnore]
		public CompInfo Component
		{
			get { return component; }
			set { component = value; }
		} CompInfo component;

		public override IXtedsComponent XtedsComponent { get { return component; } }

		[XmlElement("Interface", typeof(Interface))]
		public Interface[] Interfaces
		{
			get { return interfaces; }
			set { interfaces = value; }
		} Interface[] interfaces = new Interface[0];

		public override string ToString()
		{
			if ( Component != null )
				return Component.Name+" xTEDS";
			else
				return GetType().ToString();
		}
		#endregion 

		#region Qualifier
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Qualifier
		{
			public static Qualifier Parse(byte[] messageSpec, ref int loc, bool construct)
			{
				Qualifier qual=null;
				string str;
				int i;
				if (construct) qual = new Qualifier();
				for (; ; loc++)
				{
					switch (messageSpec[loc])
					{
						case (int)StructuredQuery.sq.Name:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								qual.Name = str;
							}
							loc += i;
							break;
						case (int)StructuredQuery.sq.Value:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								qual.Value = str;
							}
							loc += i;
							break;
						case (int)StructuredQuery.sq.Units:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								qual.Units = str;
							}
							loc += i;
							break;
						default:
							loc--;
							return null;
					}
				}
			}

			[XmlAttribute("name")]
			public string Name
			{
				get { return name; }
				set { name = value; }
			} string name;

			[XmlAttribute("qualifierValue")]
			public string QualifierValue
			{
				get { return qualValue; }
				set { qualValue = value; }
			}

			[XmlAttribute("value")]
			public string Value
			{
				get { return qualValue; }
				set { qualValue = value; }
			} string qualValue;

			[XmlAttribute("units")]
			public string Units
			{
				get { return units; }
				set { units = value; }
			} string units;
			public override string ToString()
			{
				return string.Format("{0}={1}{2}", name, qualValue, units);
			}
		}
		#endregion

		#region Location
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Location
		{
			public enum LengthUnits { meter, cm, inch };
			public static Location Parse(byte[] messageSpec, ref int loc, bool construct)
			{
				Location loctn=null;
				string str;
				int i;
				if (construct) loctn = new Location();
				for (; ; loc++)
				{
					switch (messageSpec[loc])
					{
						case (int)StructuredQuery.sq.X:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								loctn.XLocation = double.Parse(str);
							}
							loc += i;
							break;
						case (int)StructuredQuery.sq.Y:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								loctn.YLocation = double.Parse(str);
							}
							loc += i;
							break;
						case (int)StructuredQuery.sq.Z:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								loctn.ZLocation = double.Parse(str);
							}
							loc += i;
							break;
						case (int)StructuredQuery.sq.Units:
							if (construct)
								loctn.Units = (LengthUnits)messageSpec[loc+1];
							loc++;
							break;
						default:
							loc--;
							return loctn;
					}
				}
			}

			[XmlAttribute("x")]
			public double XLocation
			{
				get { return xLocation; }
				set { xLocation = value; }
			} double xLocation;

			[XmlAttribute("y")]
			public double YLocation
			{
				get { return yLocation; }
				set { yLocation = value; }
			} double yLocation;

			[XmlAttribute("z")]
			public double ZLocation {
				get { return zLocation; }
				set { zLocation = value; }
			} double zLocation;

			[XmlAttribute("units")]
			public LengthUnits Units
			{
				get { return units; }
				set { units = value; }
			} LengthUnits units = LengthUnits.meter;
			public override string ToString()
			{
				return string.Format("{0},{1},{2} {3}",xLocation,yLocation,zLocation,units);
			}
		}
		#endregion

		#region Orientation
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Orientation
		{
			public enum AxisType {unknownAxis,X,Y,Z};
			public enum AngleUnits { degrees, radians };
			public static Orientation Parse(byte[] messageSpec, ref int loc, bool construct)
			{
				Orientation orient=null;
				string str;
				int i;
				if (construct) orient = new Orientation();
				for (; ; loc++)
				{
					switch (messageSpec[loc])
					{
						case (int)StructuredQuery.sq.Axis:
							if (construct)
								orient.Axis = (AxisType)messageSpec[loc+1];
							loc++;
							break;
						case (int)StructuredQuery.sq.Angle:
							for (i=1; messageSpec[i+loc]>0; i++) ;
							if (construct)
							{
								str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								orient.Angle = double.Parse(str);
							}
							loc += i;
							break;
						case (int)StructuredQuery.sq.Units:
							if (construct)
								orient.Units = (AngleUnits)messageSpec[loc+1];
							loc++;
							break;
						default:
							loc--;
							return orient;
					}
				}
			}

			[XmlAttribute("axis")]
			public AxisType Axis
			{
				get { return axis; }
				set { axis = value; }
			} AxisType axis = AxisType.unknownAxis;

			[XmlAttribute("angle")]
			public double Angle
			{
				get { return angle; }
				set { angle = value; }
			} double angle;

			[XmlAttribute("units")]
			public AngleUnits Units
			{
				get { return units; }
				set { units = value; }
			} AngleUnits units = AngleUnits.degrees;
			public override string ToString()
			{
				return string.Format("{0},{1} {2}", axis, angle, units);
			}
		}
		#endregion

		#region XtedsComponent

		// Device or Application

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class CompInfo : IXtedsComponent
		{
			[XmlAttribute("name")]
			public string Name
			{
				get { return name; }
				set { name = value; }
			} string name;

			[XmlAttribute("componentKey")]
			public string ComponentKey
			{
				get { return componentKey; }
				set { componentKey = value; }
			} string componentKey;

			[XmlAttribute("description")]
			public string Description
			{
				get { return descr; }
				set { descr = value; }
			} string descr;

			[XmlAttribute("id")]
			public short Id
			{
				get { return id; }
				set { id = value; }
			} short id;

			[XmlAttribute("kind")]
			public string Kind
			{
				get { return kind; }
				set { kind= value; }
			} string kind;

			[XmlAttribute("manufacturerId")]
			public string Manufacturer
			{
				get { return manufacturer; }
				set { manufacturer = value; }
			} string manufacturer;

			[XmlElement("Qualifier", typeof(Qualifier))]
			public Qualifier[] Qualifiers
			{
				get { return qualifiers; }
				set { qualifiers = value; }
			} Qualifier[] qualifiers;

			public override string ToString(){return name;}
		}

		public enum Architecture { unknown, SPAU, Intel, PPC7404, PPC755, PPC405, WSSP, MODAS }
		public enum OperatingSystem { unknown, Linux24, Linux26, Win32, VXWorks, ucLinux, RTEMS, dotNet, Java }
 
		public class AppInfo : CompInfo
		{
			[XmlAttribute("architecture")]
			public Architecture Architecture { get; set; }

			[XmlAttribute("compilerUsed")]
			public string CompilerUsed { get; set; }

			[XmlAttribute("dataMemoryRequired")]
			public uint DataMemoryRequired { get; set; }

			[XmlAttribute("memoryMinimum")]
			public uint MemoryMinimum { get; set; }

			[XmlAttribute("operatingSystem")]
			public OperatingSystem OperatingSystem { get; set; }

			[XmlAttribute("pathForAssembly")]
			public string PathForAssembly { get; set; }

			[XmlAttribute("pathOnSpacecraft")]
			public string PathOnSpacecraft { get; set; }

			[XmlAttribute("programMemoryRequired")]
			public uint ProgramMemoryRequired { get; set; }

			[XmlAttribute("version")]
			public double Version { get; set; }

		}
		
		public class DevInfo : CompInfo
		{
			[XmlElement("Location")]
			public Location Location
			{
				get { return location; }
				set { location = value; }
			} Location location;

			[XmlElement("Orientation", typeof(Orientation))]
			public Orientation[] Orientations
			{
				get { return orientations; }
				set { orientations = value; }
			} Orientation[] orientations;

			[XmlAttribute("calDueDate")]
			public string CalDueDate
			{
				get { return calDueDate; }
				set { calDueDate = value; }
			} string calDueDate;

			[XmlAttribute("calibrationDate")]
			public string CalibrationDate
			{
				get { return calibrationDate; }
				set { calibrationDate = value; }
			} string calibrationDate;

			[XmlAttribute("directionXYZ")]
			public string DirectionXYZ
			{
				get { return directionXYZ; }
				set { directionXYZ = value; }
			} string directionXYZ;

			[XmlAttribute("electricalOutput")]
			public string ElectricalOutput
			{
				get { return electricalOutput; }
				set { electricalOutput = value; }
			} string electricalOutput;

			[XmlAttribute("measurementRange")]
			public string MeasurementRange
			{
				get { return measurementRange; }
				set { measurementRange = value; }
			} string measurementRange;

			[XmlAttribute("modelId")]
			public string Model
			{
				get { return model; }
				set { model = value; }
			} string model;

			[XmlAttribute("powerRequirements")]
			public float PowerRequirements
			{
				get { return power; }
				set { power = value; }
			} float power;

			[XmlAttribute("qualityFactor")]
			public string QualityFactor
			{
				get { return qualityFactor; }
				set { qualityFactor = value; }
			} string qualityFactor;

			[XmlAttribute("referenceFrequency")]
			public string ReferenceFrequency
			{
				get { return referenceFrequency; }
				set { referenceFrequency = value; }
			} string referenceFrequency;

			[XmlAttribute("referenceTemperature")]
			public string ReferenceTemperature
			{
				get { return referenceTemperature; }
				set { referenceTemperature = value; }
			} string referenceTemperature;

			[XmlAttribute("sensitivityAtReference")]
			public string SensitivityAtReference
			{
				get { return sensitivityAtReference; }
				set { sensitivityAtReference = value; }
			} string sensitivityAtReference;

			[XmlAttribute("serialNumber")]
			public string SerialNumber
			{
				get { return serialNumber; }
				set { serialNumber = value; }
			} string serialNumber;

			[XmlAttribute("temperatureCoefficient")]
			public string TemperatureCoefficient
			{
				get { return temperatureCoefficient; }
				set { temperatureCoefficient = value; }
			} string temperatureCoefficient;

			[XmlAttribute("versionLetter")]
			public string VersionLetter
			{
				get { return versionLetter; }
				set { versionLetter = value; }
			} string versionLetter;

		}

		#endregion 

		#region Interface
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Interface
		{
			internal static Interface currentInterface;
			internal int maxMessageId = -1;

			internal List<Message> dataMessageList = new List<Message>();
			internal List<Message> dataReplyMessageList = new List<Message>();
			internal List<Message> commandMessageList = new List<Message>();
			internal List<Message> faultMessageList = new List<Message>();

			Message[] messageById = new Message[0];
			internal Dictionary<string, Message> messageByName = new Dictionary<string, Message>();
			internal Dictionary<string, Variable> variableByName = new Dictionary<string, Variable>();

			[XmlIgnore]
			public List<Message> Messages { get { return messages; } }

			internal List<Message> messages = new List<Message>();

			public Interface()
			{
				currentInterface = this;
			}

			public Interface(Xteds xteds)
			{
				currentInterface = this;
				this.xteds = xteds;
			}

			static Interface()
			{
				NodeDisplayProperties = new BlackboardDisplayProperties() { Image = Properties.Resources.Interface };
			}

			internal static BlackboardDisplayProperties NodeDisplayProperties;

			internal void Add(Message msg)
			{
				if (msg == null) return;
				if (msg.Id >= messageById.Length)
				{
					Message[] newMessageById = new Message[msg.Id+1];
					messageById.CopyTo(newMessageById, 0);
					messageById = newMessageById;
				}
				messageById[msg.Id] = msg;
				messages.Add(msg);
			}

			internal void AddMessage(Message msg)
			{
				Add(msg);
				currentInterface = this;
				Message.currentMessage = msg;
				msg.OwnerInterface = this;
				if (!(msg is FaultMessage))
				{
					if (msg.messageType is Command)
					{
						var cmd = new Command();
						if (msg.messageType.FaultMsg != null)
						{
							cmd.FaultMsg = msg.messageType.FaultMsg.Clone() as FaultMessage;
							Add(cmd.FaultMsg);
						}
						cmd.CommandMsg = msg as CommandMessage;
					}
					else if (msg.messageType is Notification)
					{
						var not = new Notification();
						if (msg.messageType.FaultMsg != null)
						{
							not.FaultMsg = msg.messageType.FaultMsg.Clone() as FaultMessage;
							Add(not.FaultMsg);
						}
						not.DataMsg = msg as DataMessage;
					}
					else if (msg.messageType is Request)
					{
						var req = new Request();
						if (msg.messageType.FaultMsg != null)
						{
							req.FaultMsg = msg.messageType.FaultMsg.Clone() as FaultMessage;
							Add(req.FaultMsg);
						}
						if ((msg.messageType as Request).DataReplyMsg != null)
						{
							req.DataReplyMsg = (msg.messageType as Request).DataReplyMsg.Clone() as DataReplyMessage;
							Add(req.DataReplyMsg);
						}
						req.CommandMsg = msg as CommandMessage;
					}
				}

				Message.currentMessage = null;
				currentInterface = null;
			}

			public void AddVariable(Variable var)
			{
				if (variableList == null)
					variableList = new List<Variable>();
				variableList.Clear();
				variableList.Add(var);
				variableByName.Add(var.Name, var);
			}

			internal void Bind(Xteds xteds)
			{
				this.xteds = xteds;
				commandMessages = commandMessageList.ToArray();
				dataMessages = dataMessageList.ToArray();
				replyMessages   = dataReplyMessageList.ToArray();
				//				requests        = requestList.ToArray();
				faultMessages   = faultMessageList.ToArray();
				messageById     = new Message[maxMessageId+1];

				//int location = 0;
				//foreach ( Variable var in variables )
				//    location = var.SetOffset(location);

				currentInterface = this;

				foreach (Message msg in dataMessages)
					BindAndAdd(msg);
				foreach (Message msg in commandMessages)
					BindAndAdd(msg);
				foreach (Message msg in replyMessages)
					BindAndAdd(msg);
				foreach (Message msg in faultMessages)
					BindAndAdd(msg);
			}

			private void BindAndAdd(Message msg)
			{
				msg.Bind();
				if (messageById[msg.Id] != null)
					MsgConsole.WriteLine("Xteds.Interface.Bind", MsgLevel.Warning,
						"file, {0}, interface {1}, message {2}, has the same id as message {3}",
						xteds.filePath, Name, msg.Name, messageById[msg.Id].Name);
				messageById[msg.Id] = msg;
				messages.Add(msg);
			}

			internal void DistinguishDuplicates(Dictionary<string, int> duplicateNameCount)
			{
				int nameCount;
				if (duplicateNameCount.TryGetValue(name, out nameCount))
					name = name + "_" + nameCount;
				else
					duplicateNameCount[name] = nameCount + 1;
			}

			public Message FindMessage(byte id)
			{
				if (id < messageById.Length)
					return messageById[id];
				else
					return null;
			}

			public Message FindMessage(string msgName)
			{
				return messageByName[msgName] as Message;
			}

			internal Message FindMessage(byte[] messageSpec, int loc)
			{
				currentInterface = this;
				int msgLoc;
				Message msg=null;
				for(; messageSpec[loc]>0; loc++)
				{
					msg = FindMessage(messageSpec[loc+3]);
					if (msg == null)
					{
						msgLoc = loc+1;
						switch (messageSpec[loc++])
						{
							case (int)StructuredQuery.sq.Command:
								Command cmd = new Command(messageSpec,ref loc, this);
								Add(cmd.CommandMsg);
								Add(cmd.FaultMsg);
								if (messageSpec[msgLoc] == (int)StructuredQuery.sq.CommandMsg)
									return cmd.CommandMsg;
								else if (messageSpec[msgLoc] == (int)StructuredQuery.sq.FaultMsg)
									return cmd.FaultMsg;
								break;
							case (int)StructuredQuery.sq.Notification:
								Notification noti = new Notification(messageSpec, ref loc, this);
								Add(noti.DataMsg);
								Add(noti.FaultMsg);
								if (messageSpec[msgLoc] == (int)StructuredQuery.sq.DataMsg)
									return noti.DataMsg;
								else if (messageSpec[msgLoc] == (int)StructuredQuery.sq.FaultMsg)
									return noti.FaultMsg;
								break;
							case (int)StructuredQuery.sq.Request:
								Request req = new Request(messageSpec, ref loc, this);
								Add(req.CommandMsg);
								Add(req.DataReplyMsg);
								Add(req.FaultMsg);
								if (messageSpec[msgLoc] == (int)StructuredQuery.sq.CommandMsg)
									return req.CommandMsg;
								else if (messageSpec[msgLoc] == (int)StructuredQuery.sq.DataReplyMsg)
									return req.DataReplyMsg;
								else if (messageSpec[msgLoc] == (int)StructuredQuery.sq.FaultMsg)
									return req.FaultMsg;
								break;
						}
					}
				}
				return msg;
			}
			List<Variable> variableList;
			public Variable FindVariable(byte[] messageSpec, ref int loc)
			{
				int id = messageSpec[loc];
				Variable var = null;
				if ( variableList == null )
					variableList = new List<Variable>();

				if (id > 0 && id < this.variableList.Count)
					var = variableList[id];
				loc++;
				if ( var != null )
					Variable.Parse(messageSpec,ref loc,false);
				else
				{
					var = Variable.Parse(messageSpec,ref loc,true);
					var.Id = (short)id;
					while ( variableList.Count <= id )
						variableList.Add(null);
					variableList[id] = var;
				}
				return var;
			}

			internal Variable FindVariable(string name)
			{
				return variableByName[name] as Variable;
			}

			internal void MergeVariables()
			{
				Variable[] newVars = new Variable[variables.Length+variableList.Count];
				int i = 0;
				foreach ( var var in variables )
					newVars[i++] = var;
				foreach ( var var in variableList )
					newVars[i++] = var;
				variables = newVars;
			}

			public void Publish(string parentPath)
			{
				string path = parentPath+'.'+Name;
				//if (!path.Contains("nterface"))
				//    path = path + " interface";
				var attr = new BlackboardAttribute();
				attr.Description = this.Description;
				var di = Blackboard.Publish(this, path, new ObjectValueInfo(this));
				di.DisplayProperties = NodeDisplayProperties;
				di.AddAttributeInfo(attr);

				if ( commands != null )
					foreach (var cmd in commands)
						cmd.Publish(path);
				if (notifications != null)
					foreach (var notify in notifications)
						notify.Publish(path);
				if (requests != null)
					foreach (var req in requests)
						req.Publish(path);
			}

			public void Unpublish(string parentPath)
			{
				string path = parentPath+'.'+Name;

				if (commands != null)
					foreach (var cmd in commands)
						cmd.Unpublish(path);
				if (notifications != null)
					foreach (var notify in notifications)
						notify.Unpublish(path);
				if (requests != null)
					foreach (var req in requests)
						req.Unpublish(path);

				Blackboard.Unpublish(path);
			}

			[XmlIgnore]
			public bool UnmarshalOnMonitor
			{
				get { return unmarshalOnMonitor; }
				set { unmarshalOnMonitor = value; }
			} internal bool unmarshalOnMonitor;

			[XmlAttribute("name")]
			public string Name
			{
				get { return name; }
				set
				{
					if (origName == null)
						origName = value;
					int dot = value.LastIndexOf('.'); // Not sure why interfaces can have . separators
					if (dot > 0)
						name = value.Substring(0, dot + 1) + origName;
					else
						name = value;
				}
			} string name, origName;

			[XmlAttribute("id")]
			public byte Id
			{
				get { return id; }
				set
				{ 
					id = value; 
					if ( id > maxInterfaceId )
						maxInterfaceId = id;
				}
			} byte id;

			[XmlAttribute("description")]
			public string Description
			{
				get { return description; }
				set { description = value; }
			} string description;

			public enum ScopeType { Local, Private, Public };

			[XmlAttribute("scope")]
			public ScopeType Scope
			{
				get { return scope; }
				set { scope = value; }
			} ScopeType scope;

			[XmlElement("Variable", typeof(Variable))]
			public Variable[] Variables
			{
				get { return variables; }
				set { variables = (value == null ? new Variable[0] : value); }
			} Variable[] variables;

			[XmlElement("Command", typeof(Command))]
			public Command[] Commands
			{
				get { return commands; }
				set { commands = value; }
			} Command[] commands;

			[XmlElement("Notification", typeof(Notification))]
			public Notification[] Notifications
			{
				get { return notifications; }
				set { notifications = value; }
			} Notification[] notifications;

			[XmlElement("Request", typeof(Request))]
			public Request[] Requests
			{
				get { return requests; }
				set { requests = value; }
			} Request[] requests;

			[XmlElement("Qualifier", typeof(Qualifier))]
			public Qualifier[] Qualifiers
			{
				get { return qualifiers; }
				set { qualifiers = value; }
			} Qualifier[] qualifiers;

			[XmlIgnore]
			public Message[] CommandMessages
			{
				get { return commandMessages; }
				set { commandMessages = value; }
			} Message[] commandMessages;

			[XmlIgnore]
			public Message[] DataMessages
			{
				get { return dataMessages; }
				set { dataMessages = value; }
			} Message[] dataMessages;

			[XmlIgnore]
			public Message[] ReplyMessages
			{
				get { return replyMessages; }
				set { replyMessages = value; }
			} Message[] replyMessages;

			[XmlIgnore]
			public Message[] FaultMessages
			{
				get { return faultMessages; }
				set { faultMessages = value; }
			} Message[] faultMessages;

			public override string ToString(){return name;}

			public Xteds Xteds
			{
				get { return xteds; }
			} Xteds xteds;

			#region Message

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Message : XtedsMessage
			{
				internal static Message currentMessage;
				internal bool marshalerDefined;
				internal MessageType messageType;

				public Message()
				{
					currentMessage = this;
					iface = currentInterface;
					if ( iface != null )
						mMessageId.Interface = iface.Id;
				}

				//protected override void OnMessageArrival()
				//{
				//    if (Arrived != null)
				//    {
				//        Arrived(this, EventArgs.Empty);
				//    }
				//}

				/// <summary>
				/// Event that is raised whenever this message is arrived
				/// </summary>
				public event EventHandler Arrived;

				public string ArrivalNotification { get; set; }

				public enum Arrival
				{
					EVENT=1,
					PERIODIC
				};

				internal void Bind()
				{
					if ( qualifiers != null )
						foreach ( var q in qualifiers )
							if (q.Name == "Notify")
							{
								ArrivalNotification = q.Value;
								OwnerInterface.Xteds.Host.HookNotification(this);
								break;
							}
					if (VariableList.Count == 0 && mVariableDefs != null && mVariableDefs.Length > 0)
					{
						//foreach (var varDef in VariableDefs)
						//{
						//    var existingVar = currentInterface.FindVariable(varDef.Name);
						//    if (existingVar != null)
						//    {
						//        MsgConsole.WriteLine("Xteds.Interface.Bind: duplicate variable {0}", varDef.Name);
						//        continue;
						//    }
						//    currentInterface.AddVariable(varDef);
						//}
						//currentInterface.MergeVariables();
						variableRefList.Clear();
						currentMessage = this;
						foreach (var varDef in VariableDefs)
						{
							var varRef = new VariableRef();
							varRef.Name = varDef.Name;
							variableRefList.Add(varRef);
						}
						
						variableRefs = variableRefList.ToArray();
					}
					variables = VariableList.ToArray();
					int count = 0;
					length = 0;
					foreach ( VariableValue var in variables )
					{
						if ( var == null ) break;
						length += (short)var.Bind(this,length);
						count++;
						
					}
					if ( count != variables.Length )
					{
						foreach ( VariableRef vRef in VariableRefs )
							if ( vRef.VariableDef == null )
							{
								valid = false;
								string devName = iface.xteds.Component != null ?
									iface.xteds.Component.Name : iface.xteds.Name;
								MsgConsole.WriteLine("Aspire.Message.Bind", MsgLevel.Warning,
									"vref {0} not found in {1}.{2}.{3}'s variables",
									vRef.Name, devName,
									Interface.currentInterface.Name, Name );
								MsgConsole.WriteLine("Aspire.Message.Bind", MsgLevel.Warning,
									"  file: {0}",
									iface.xteds.FilePath);
							}
					}
				}

				public override XtedsMessage Clone()
				{
					Message clone;
					if (this is CommandMessage) clone = new CommandMessage();
					else if (this is DataMessage) clone = new DataMessage();
					else if (this is DataReplyMessage) clone = new DataReplyMessage();
					else if (this is FaultMessage) clone = new FaultMessage();
					else clone = new Message();
					clone.descr = descr;
					clone.Disposition = Disposition;
					clone.id = id;
					clone.iface = iface;
					clone.IsSynchronous = IsSynchronous;
					clone.mMessageId = mMessageId;
					clone.name = name;
					clone.qualifiers = qualifiers;
					clone.length = length;
					clone.location = location;
					clone.valid = valid;
					clone.variableRefs = variableRefs;
					clone.variableList = new List<VariableValue>();
					foreach ( var varValue in variableList )
						clone.variableList.Add(varValue.Clone(clone));
					clone.variables = variableList.ToArray();
					clone.messageType = messageType;
					return clone;
				}

				List<VariableRef> variableRefList = new List<VariableRef>();
				protected void ParseMessageSpecForAttributes(byte[] messageSpec, ref int loc, MessageType type)
				{
					messageType = type;
					variableRefList.Clear();
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.Id:
								Id = messageSpec[loc+1];
								loc++;
								break;
							case (int)StructuredQuery.sq.Name:
								int i;
								for (i=1; messageSpec[i+loc]>0; i++);
								Name = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
								loc += i;
								break;
							case (int)StructuredQuery.sq.Variable:
								loc++;
								VariableRef var = new VariableRef(messageSpec,ref loc,iface);
								variableRefList.Add(var);
								break;
							default:
								variableRefs = variableRefList.ToArray();
								Bind();
								loc--;
								return;
						}
					}
				}

				public override string ProviderName
				{
					get
					{
						return OwnerInterface.Xteds.Host.ProviderName;
					}
				}

				public override IVariable IVariable(string name)
				{
					if ( variables == null )
						return null;
					return Variable(name);
				}
				public VariableValue Variable(string name)
				{
					foreach (var vv in variables)
					{
						if (vv.Name == name)
							return vv;
					}
					return null;
				}
				/// <summary>
				/// Gets the <see cref="Blackboard.Item"/> that represents this <see cref="Message"/> if it is published to the <see cref="DataDictionary"/>
				/// </summary>
				[XmlIgnore]
				public Blackboard.Item PublishedAs
				{
					get { return mItem; }
				}
				
				private Blackboard.Item mItem = null;

				public virtual void Publish(string parentPath, string suffix)
				{
					var attr = new BlackboardAttribute();
					attr.Description = this.Description;
					string path = parentPath+'.'+name + suffix;
					mItem = Blackboard.Publish(this, path, new ObjectValueInfo(this));
					mItem.AddAttributeInfo(attr);
					foreach (VariableValue vv in variableList)
						vv.Publish(path);
				}

				public virtual void Unpublish(string parentPath, string suffix)
				{
					string path = parentPath + '.' + name + suffix;
					foreach ( VariableValue vv in variableList )
						Blackboard.Unpublish(path+'.'+vv.Variable.Name);
					Blackboard.Unpublish(path);
				}

				public override int GetHashCode()
				{
					return Hash(OwnerInterface.Id, Id);
				}

				public static int Hash(int interfaceId, int messageId)
				{
					return (interfaceId << 8) | messageId;
				}

				#region Properties

				[XmlIgnore]
				public override string InterfaceName
				{
					get { return iface.name; }
				}

				public override IMessageType MessageType { get { return messageType; } }
				internal virtual MessageType SetMessageType { set { messageType = value; } }

				MessageTypeCode mMessageTypeCode;
				public override MessageTypeCode MessageTypeCode { get { return mMessageTypeCode; } }
				internal virtual MessageTypeCode SetMessageTypeCode { set { mMessageTypeCode = value; } }

				[XmlAttribute("name")]
				public override string Name
				{
					get { return name; }
					set
					{ 
						name = value;
						currentInterface.messageByName[value] = this;
					}
				} protected string name;

				[XmlAttribute("description")]
				public string Description
				{
					get { return descr; }
					set { descr = value; }
				} string descr;

				[Browsable(false)]
				[XmlAttribute("id")]
				public short xmlId
				{
					get { return Id; }
					set { Id = (byte)(value&0xFF); }
				}
				public byte Id
				{
					get { return id; }
					set
					{ 
						id = value; 
						if (id > Interface.currentInterface.maxMessageId)
							Interface.currentInterface.maxMessageId = id;
						mMessageId.Message = id;
					}
				} byte id;

				[XmlAttribute("standardImplemented")]
				public string StandardImplemented { get; set; }

				[XmlElement("Qualifier", typeof(Qualifier))]
				public Qualifier[] Qualifiers
				{
					get { return qualifiers; }
					set { qualifiers = value; }
				} Qualifier[] qualifiers;

				[XmlIgnore]
				public short Length
				{
					get { return length; }
				} short length;

				public override int Size { get { return length; } }

				[XmlIgnore]
					// FIXME location never set
				public int location;

				[XmlIgnore]
				public bool Valid
				{
					get { return valid; }
				} bool valid = true;

				[XmlElement("VariableRef", typeof(VariableRef))]
				public VariableRef[] VariableRefs
				{
					get { return variableRefs; }
					set { variableRefs = value; }
				} VariableRef[] variableRefs;

				[XmlElement("Variable", typeof(Variable))]
				public Variable[] VariableDefs
				{
					get { return mVariableDefs; }
					set { mVariableDefs = value; }
				} Variable[] mVariableDefs;

				[XmlIgnore]
				public VariableValue[] Variables
				{
					get { return variables; }
				} VariableValue[] variables;

				[XmlIgnore]
				public override IVariable[] IVariables
				{
					get { return variables; }
				}

				public override int NumVariables
				{
					get { return variables==null?0:variables.Length; }
				}

				[XmlIgnore]
				[Description("Number of times this message has been received.")]
				public int Received
				{
					get { return received; }
					set { received = value; }
				}

				[XmlIgnore]
				[Description("Number of times this message has been sent.")]
				public int Sent
				{
					get { return sent; }
					set { sent = value; }
				}

				[XmlIgnore]
				public List<VariableValue> VariableList 
				{
					get { return variableList; }
					set { variableList = value; }
				} List<VariableValue> variableList = new List<VariableValue>();

				[XmlIgnore]
				public Interface OwnerInterface
				{
					get { return iface; }
					internal set { iface = value; }
				} Interface iface;

				#endregion 

				public override void LeaseHasExpired()
				{
				}

				public void MonitorReceive(byte[] buf,int length )
				{
					received++;
					if ( iface.unmarshalOnMonitor )
						iface.xteds.Host.Unmarshal(buf,length,this);
					if (Arrived != null)
						Arrived(this,EventArgs.Empty);
				}

				public void MonitorReceive()
				{
					received++;
					//	iface.xteds.Host.Unmarshal(buf,this);
					if (Arrived != null)
						Arrived(this,EventArgs.Empty);
				}

				public void MonitorSend(byte[] buf)
				{
					sent++;
					if ( iface.unmarshalOnMonitor )
						iface.xteds.Host.Unmarshal(buf,buf.Length,this);
				}

				public override string ToString()
				{ 
					return name; 
				}

				public override IVariableMarshaler XtedsVariableMarshaler(IVariable ivar)
				{
					Variable var = (ivar as VariableValue).Variable;
					if (var.marshaler == null || (var.marshaler.IVariable as VariableValue).OwningMessage != this)
					{
						var host = OwnerInterface.Xteds.Host;
						if (host != null)
						{

							var xVar = host.XtedsVariable(var.Name);
							if (xVar != null)
								var.marshaler = new VariableMarshaler(var.Name, ivar, xVar.BoundItem);
							else
								var.marshaler = host.KnownMarshaler(ivar);
							if (var.marshaler == null)
								var.marshaler = new VariableMarshaler(var.Name, ivar);
						}
						else
							var.marshaler = new VariableMarshaler(var.Name, ivar);
					}
					return var.marshaler;
				}

			}

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class CommandMessage : Message
			{
				public CommandMessage()
				{
					if (Interface.currentInterface != null)
						Interface.currentInterface.commandMessageList.Add(this);
				}

				internal CommandMessage(byte[] messageSpec, ref int loc, MessageType type)
				{
					ParseMessageSpecForAttributes(messageSpec, ref loc, type);
				}

				internal static BlackboardDisplayProperties NodeDisplayConsumerCommand;
				internal static BlackboardDisplayProperties NodeDisplayProviderCommand;
				internal static BlackboardDisplayProperties NodeDisplayConsumerRequest;
				internal static BlackboardDisplayProperties NodeDisplayProviderRequest;

				static CommandMessage()
				{
					NodeDisplayConsumerCommand = new BlackboardDisplayProperties() { Image = Properties.Resources.export1 };
					NodeDisplayProviderCommand = new BlackboardDisplayProperties() { Image = Properties.Resources.import1 };
					NodeDisplayConsumerRequest = new BlackboardDisplayProperties() { Image = Properties.Resources.mail_exchange };
					NodeDisplayProviderRequest = new BlackboardDisplayProperties() { Image = Properties.Resources.mail_forward };
				}

				public Request Request
				{
					get { return messageType as Request; }
				}

				public void Send()
				{
					OwnerInterface.Xteds.Host.Send(this);
					//Sent++;
				}

				void reply_WhenMessageArrives(XtedsMessage msg)
				{
					(msg as Message).MonitorReceive();
				}

				public override void Publish(string parentPath,string suffix)
				{
					base.Publish(parentPath,suffix);
                    PublishedAs.AddToCategory("Command");

					if (messageType is Command)
					{
						var prop = this.OwnerInterface.Xteds.Host.HostType == HostType.Provider ?
						NodeDisplayProviderCommand : NodeDisplayConsumerCommand;
						PublishedAs.DisplayProperties = prop;

					}
					else if (messageType is Request)
					{
						var prop = this.OwnerInterface.Xteds.Host.HostType == HostType.Provider ?
						NodeDisplayProviderRequest : NodeDisplayConsumerRequest;
						PublishedAs.DisplayProperties = prop;
					}

                    if ( OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer )
						Blackboard.Publish(this, parentPath + '.' + name + suffix + ".Sent", "Sent");
					else
						Blackboard.Publish(this, parentPath + '.' + name+suffix + ".Received", "Received");
				}

				public override void Unpublish(string parentPath, string suffix)
				{
					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Unpublish(parentPath + '.' + name + suffix + ".Sent");
					else
						Blackboard.Unpublish(parentPath + '.' + name+suffix + ".Received");
					base.Unpublish(parentPath, suffix);
				}
			}

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class DataMessage : Message, IDataMessage, IPublisher
			{
				public DataMessage()
				{
					if (Interface.currentInterface != null)
						Interface.currentInterface.dataMessageList.Add(this);
				}

				internal static BlackboardDisplayProperties NodeDisplayPeriodicUnsubscribed;
				internal static BlackboardDisplayProperties NodeDisplayEventConsumerUnsubscribed;
				internal static BlackboardDisplayProperties NodeDisplayEventProviderUnsubscribed;
				internal static BlackboardDisplayProperties NodeDisplaySubscribed;

				static DataMessage()
				{
					NodeDisplaySubscribed = new BlackboardDisplayProperties() { Image = Properties.Resources.mail_preferences };
					NodeDisplayPeriodicUnsubscribed = new BlackboardDisplayProperties() { Image = Properties.Resources.window_time };
					// Need to find two matched event icons
					NodeDisplayEventConsumerUnsubscribed = new BlackboardDisplayProperties() { Image = Properties.Resources.flash };
					NodeDisplayEventProviderUnsubscribed = new BlackboardDisplayProperties() { Image = Properties.Resources.flashUp };
				}

				internal DataMessage(byte[] messageSpec, ref int loc, MessageType type)
				{
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
						case (int)StructuredQuery.sq.Id:
								Id = messageSpec[loc+1];
							loc++;
							break;
						case (int)StructuredQuery.sq.Name:
							ParseMessageSpecForAttributes(messageSpec, ref loc, type);
							break;
						default:
							loc--;
							return;
						}
					}
				}

				public void Parse(byte[] messageSpec, int loc)
				{
					for (; ; loc++)
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.MsgArrival:
								MsgArrival = (Arrival)messageSpec[loc+1];
								loc++;
								break;
							case (int)StructuredQuery.sq.MsgRate:
								MarshaledString ms = new MarshaledString(messageSpec[loc+1], messageSpec, loc+2);
								MsgRate = Double.Parse(ms.String);
								loc += ms.Length+1;
								break;
							default:
								return;
						}
				}

				public void Publish()
				{
					OwnerInterface.Xteds.Host.Publish(this as IDataMessage);
					//Sent++;
				}

				/// <summary>
				/// Subscribe to the data message if not already subscribed. Increments the reference count.
				/// </summary>
				public bool Subscribe()
				{
					try
					{
						if( subscribed == 0 ) OwnerInterface.Xteds.Host.Subscribe(this);

                        subscribed++;
                        UpdateDisplayNode();

						return true;
					}
					catch { return false; }
				}

				/// <summary>
				/// Decrements the subscription reference count and un-subscribes from the data message when the count reaches 0.
				/// </summary>
				public void Unsubscribe()
				{
					if ( subscribed <= 0 ) return;
					subscribed--;
					if( subscribed == 0 ) 
					{
                        ForceUnsubscribe();
					}
                    UpdateDisplayNode();
                }

                /// <summary>
                /// Performs <see cref="IHostXteds.Unsubscribe"/> regardless of the value of <see cref="subscribed"/>
                /// </summary>
                internal void ForceUnsubscribe()
                {
                    try
                    {
                        OwnerInterface.Xteds.Host.Unsubscribe(this);
                        subscribed = 0;
                        UpdateDisplayNode();
                    }
                    catch
                    {
                        subscribed = 1;
                    }
                }

                internal void UpdateDisplayNode()
                {
                    if (Subscribed == 0)
                    {
						var prop = NodeDisplayPeriodicUnsubscribed;
						if (MsgArrival == Arrival.EVENT)
							prop = OwnerInterface.Xteds.Host.HostType == HostType.Provider ?
							NodeDisplayEventProviderUnsubscribed : NodeDisplayEventConsumerUnsubscribed;
						PublishedAs.DisplayProperties = prop;
                    }
                    else
                    {
						PublishedAs.DisplayProperties = NodeDisplaySubscribed;
                    }
                }

				internal int Subscribed
				{
					get { return subscribed; }
					set { subscribed = value; }
				} int subscribed;

				public override void Publish(string parentPath, string suffix)
				{
					base.Publish(parentPath, suffix);
					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Publish(this, parentPath + '.' + name+suffix + ".Received", "Received");
					else
						Blackboard.Publish(this, parentPath + '.' + name + suffix + ".Sent", "Sent");
                    UpdateDisplayNode();
				}

				public override void Unpublish(string parentPath, string suffix)
				{
					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Unpublish(parentPath + '.' + name+suffix + ".Received");
					else
						Blackboard.Unpublish(parentPath + '.' + name + suffix + ".Sent");
					base.Unpublish(parentPath, suffix);
				}

				public override void LeaseHasExpired()
				{
					subscribed = 0;
					UpdateDisplayNode();
				}

				public event EventHandler Unpublished;
				public void RaiseUnpublished(Address subscriber)
				{
					if ( Unpublished != null )
						Unpublished(subscriber,EventArgs.Empty);
				}

				#region Properties

				[XmlAttribute("msgArrival")]
				public Arrival MsgArrival
				{
					get { return msgArrival; }
					set { msgArrival = value; }
				} Arrival msgArrival = Arrival.EVENT;

				[XmlAttribute("msgRate")]
				[DefaultValue(1.0)]
				public double MsgRate
				{
					get { return msgRate; }
					set { msgRate = value; }
				} double msgRate = 1;

				#endregion

				public XtedsMessage XtedsMessage { get { return this; } }

				#region IPublisher Members

				public bool IsEvent
				{
					get { return msgArrival == Arrival.EVENT; }
				}

				public bool IsProvider
				{
					get
					{
						return OwnerInterface.Xteds.mLiteHost.HostType == HostType.Provider;
					}
				}

				[XmlIgnore]
				public Publication Publication
				{
					get { return mPublication; }
					set { mPublication = value; }
				} Publication mPublication;

				[XmlIgnore]
				public int AllowableLeasePeriod
				{
					get
					{
						if (mAllowableLeasePeriod == -1)
						{
							if ( OwnerInterface.Xteds.mLiteHost.HostType == HostType.Provider)
								mAllowableLeasePeriod = OwnerInterface.Xteds.mLiteHost.AllowableLeasePeriod;
							else
								mAllowableLeasePeriod = 0;
						}
						return mAllowableLeasePeriod;
					}
					set
					{
						mAllowableLeasePeriod = value;
					}
				} int mAllowableLeasePeriod = -1;

				public void RaiseSubscription(Subscription subscription, bool cancel)
				{
					if ( cancel )
					{
						if ( subscribed > 0 ) subscribed--;
					}
					else
						subscribed++;
					UpdateDisplayNode();
					if (SubscribedTo != null)
						SubscribedTo(this, subscription, cancel);
				}

				public event SubscribedToHandler SubscribedTo;

				#endregion
			}

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class DataReplyMessage : Message, IDataMessage
			{
				public DataReplyMessage()
				{
					if ( Interface.currentInterface != null )
						Interface.currentInterface.dataReplyMessageList.Add(this);
				}

				internal DataReplyMessage(byte[] messageSpec, ref int loc, MessageType type)
				{
					ParseMessageSpecForAttributes(messageSpec, ref loc, type);
				}

				internal static BlackboardDisplayProperties NodeDisplayConsumerReply;
				internal static BlackboardDisplayProperties NodeDisplayProviderReply;

				static DataReplyMessage()
				{
					NodeDisplayConsumerReply = new BlackboardDisplayProperties() { Image = Properties.Resources.import1 };
					NodeDisplayProviderReply = new BlackboardDisplayProperties() { Image = Properties.Resources.export1 };
				}

				public int AllowableLeasePeriod
				{
					get { return 0; }
					set { }
				}

				[XmlIgnore]
				public Publication Publication
				{
					get { return null; }
					set { }
				}

				public override void Publish(string parentPath, string suffix)
				{
					var prop = this.OwnerInterface.Xteds.Host.HostType == HostType.Provider ?
						NodeDisplayProviderReply : NodeDisplayConsumerReply;
					base.Publish(parentPath, suffix);
					PublishedAs.DisplayProperties = prop;

					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Publish(this, parentPath + '.' + name+suffix + ".Received", "Received");
					else
						Blackboard.Publish(this, parentPath + '.' + name + suffix + ".Sent", "Sent");
				}

				public override void Unpublish(string parentPath, string suffix)
				{
					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Unpublish(parentPath + '.' + name+suffix + ".Received");
					else
						Blackboard.Unpublish(parentPath + '.' + name + suffix + ".Sent");
					base.Unpublish(parentPath, suffix);
				}
				public XtedsMessage XtedsMessage { get { return this; } }
			}

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class FaultMessage : Message
			{
				public FaultMessage()
				{
					if (Interface.currentInterface != null)
						Interface.currentInterface.faultMessageList.Add(this);
				}

				static FaultMessage()
				{
					NodeDisplayProperties = new BlackboardDisplayProperties()
					{
						Image = Properties.Resources.window_warning
					};
				}

				internal FaultMessage(byte[] messageSpec, ref int loc, MessageType type)
				{
					ParseMessageSpecForAttributes(messageSpec, ref loc, type);
				}

				internal static BlackboardDisplayProperties NodeDisplayProperties;

				public override void Publish(string parentPath, string suffix)
				{
					base.Publish(parentPath, suffix);
					PublishedAs.DisplayProperties = NodeDisplayProperties;

					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Publish(this, parentPath + '.' + name+suffix + ".Received", "Received");
					else
						Blackboard.Publish(this, parentPath + '.' + name + suffix + ".Sent", "Sent");
				}

				public override void Unpublish(string parentPath, string suffix)
				{
					if (OwnerInterface.xteds.mLiteHost.HostType==HostType.Consumer)
						Blackboard.Unpublish(parentPath + '.' + name+suffix + ".Received");
					else
						Blackboard.Unpublish(parentPath + '.' + name + suffix + ".Sent");
					base.Unpublish(parentPath, suffix);
				}
			}

			#endregion 

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class MessageType : IMessageType
			{
				[XmlElement("FaultMsg")]
				public FaultMessage FaultMsg
				{
					get { return faultMsg; }
					set
					{
						faultMsg = value;
						faultMsg.messageType = this;
					}
				} FaultMessage faultMsg;

				protected string faultSuffix = "";//" (fault)";

				public override string ToString()
				{
					return GetType().Name;
				}
			}

			#region Command
			public class Command : MessageType
			{
				public Command()
				{
				}
				internal Command(byte[] messageSpec, ref int loc, Interface iface)
				{
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.CommandMsg:
								loc++;
								commandMsg = new CommandMessage(messageSpec, ref loc, this);
								break;
							case (int)StructuredQuery.sq.FaultMsg:
								loc++;
								FaultMsg = new FaultMessage(messageSpec, ref loc, this);
								break;
							default:
								loc--;
								return;
						}
					}
				}

				[XmlElement("CommandMsg")]
				public CommandMessage CommandMsg
				{
					get { return commandMsg; }
					set
					{
						commandMsg = value;
						commandMsg.messageType = this;
					}
				} CommandMessage commandMsg;

				const string suffix = "";//" (command)";

				public void Publish(string path)
				{
					commandMsg.Publish(path, suffix);
					if (FaultMsg != null)
						FaultMsg.Publish(path + '.' + commandMsg.Name + suffix, faultSuffix);
				}
				public void Unpublish(string path)
				{
					commandMsg.Unpublish(path, suffix);
					if (FaultMsg != null)
						FaultMsg.Unpublish(path + '.' + commandMsg.Name + suffix, faultSuffix);
				}
				public override string  ToString()
				{
					return base.ToString() + '(' + commandMsg.ToString() + ')'; 
				}
			}
			#endregion

			#region Notification
			public class Notification : MessageType
			{
				public Notification()
				{
				}
				internal Notification(byte[] messageSpec, ref int loc, Interface iface)
				{
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.DataMsg:
								loc++;
								dataMsg = new DataMessage(messageSpec,ref loc, this);
								break;
							case (int)StructuredQuery.sq.FaultMsg:
								loc++;
								FaultMsg = new FaultMessage(messageSpec,ref loc, this);
								break;
							default:
								loc--;
								return;
						}
					}
				}

				[XmlElement("DataMsg")]
				public DataMessage DataMsg
                {
					get { return dataMsg; }
					set
					{
						dataMsg = value;
						dataMsg.messageType = this;
						dataMsg.SetMessageTypeCode = MessageTypeCode.Notification;					}
				} DataMessage dataMsg;

				const string suffix = "";//" (notification)";

				public void Publish(string path)
				{
                    if (dataMsg != null)
                    {
                        dataMsg.Publish(path, suffix);
                        dataMsg.PublishedAs.AddToCategory("Notification");

                        //dataMsg.UpdateDisplayNode();
                    }

					if (FaultMsg != null)
						FaultMsg.Publish(path + '.' + dataMsg.Name + suffix, faultSuffix);
				}
				public void Unpublish(string path)
				{
					dataMsg.Unpublish(path, suffix);
					if (FaultMsg != null)
						FaultMsg.Unpublish(path + '.' + dataMsg.Name + suffix, faultSuffix);
				}
				public override string ToString()
				{
					return base.ToString() + '(' + dataMsg.ToString() + ')';
				}
			}
			#endregion

			#region Request

			public class Request : MessageType, IXtedsRequest
			{
				public Request()
				{
				}
				internal Request(byte[] messageSpec, ref int loc, Interface iface)
				{
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.CommandMsg:
								loc++;
								commandMsg = new CommandMessage(messageSpec, ref loc, this);
								break;
							case (int)StructuredQuery.sq.DataReplyMsg:
								loc++;
								dataReplyMsg = new DataReplyMessage(messageSpec, ref loc, this);
								break;
							case (int)StructuredQuery.sq.FaultMsg:
								loc++;
								FaultMsg = new FaultMessage(messageSpec, ref loc, this);
								break;
							default:
								loc--;
								return;
						}
					}
				}

				[XmlElement("CommandMsg")]
				public CommandMessage CommandMsg
				{
					get { return commandMsg; }
					set
					{
						commandMsg = value;
						commandMsg.messageType = this;
						commandMsg.SetMessageTypeCode = MessageTypeCode.Request;
					}
				} CommandMessage commandMsg;

				public XtedsMessage CommandXtedsMsg
				{
					get { return commandMsg; }
				}

				public XtedsMessage DataReplyXtedsMsg
				{
					get { return dataReplyMsg; }
				}

				[XmlElement("DataReplyMsg")]
				public DataReplyMessage DataReplyMsg
				{
					get { return dataReplyMsg; }
					set
					{
						dataReplyMsg = value;
						dataReplyMsg.messageType = this;
						dataReplyMsg.SetMessageTypeCode = MessageTypeCode.Reply;
					}
				} DataReplyMessage dataReplyMsg;

				[XmlIgnore]
				public string Name
				{
					get { return commandMsg.Name; }
					set { commandMsg.Name = value; }
				}

				//public override string ToString(){return Name;}

				const string suffix = "";//" (request)";

				public void Publish(string path)
				{
					commandMsg.Publish(path, suffix);
                    commandMsg.PublishedAs.AddToCategory("Request");

                    dataReplyMsg.Publish(path + '.' + commandMsg.Name + suffix, string.Empty);
                    dataReplyMsg.PublishedAs.AddToCategory("Reply");

					if (FaultMsg != null)
						FaultMsg.Publish(path + '.' + commandMsg.Name + suffix, faultSuffix);
				}

				public void Unpublish(string path)
				{
					commandMsg.Unpublish(path, suffix);
					dataReplyMsg.Unpublish(path + '.' + commandMsg.Name + suffix, string.Empty);
					if (FaultMsg != null)
						FaultMsg.Unpublish(path + '.' + commandMsg.Name + suffix, faultSuffix);
				}

				public override string ToString()
				{
					return string.Format("{0}({1},{2})",base.ToString(),commandMsg,dataReplyMsg);
				}
			}

			#endregion 

			#region Variable

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Variable
			{
				public Variable()
				{
					ParentInterface = currentInterface;
				}

				public enum FormatType
				{
					Unknown,
					Length1=1,
					Length2=2,
					Length4=4,
					Length8=8,
					Length16=16,
					UINT8 = 17,
					UINT08 = UINT8,
					INT8 = 18,
					INT08 = INT8,
					UINT16,
					INT16,
					UINT32,
					INT32,
					FLOAT32,
					FLOAT64,
					UINT64,
					INT64,
					STRING,
					BUFFER,
					// not currently serialized on the wire
					ENUM32,
					FLOAT128,
					OBJECT,
					LastType
				};

				public static TypeCode GetTypeCode(FormatType formatType, out byte length)
				{
					TypeCode typeCode;
					switch (formatType)
					{
						case FormatType.INT8:
							typeCode = TypeCode.SByte;
							length = 1;
							break;
						case FormatType.INT16:
							typeCode = TypeCode.Int16;
							length = 2;
							break;
						case FormatType.INT32:
							typeCode = TypeCode.Int32;
							length = 4;
							break;
						case FormatType.INT64:
							typeCode = TypeCode.Int64;
							length = 8;
							break;
						case FormatType.UINT8:
							typeCode = TypeCode.Byte;
							length = 1;
							break;
						case FormatType.UINT16:
							typeCode = TypeCode.UInt16;
							length = 2;
							break;
						case FormatType.UINT32:
							typeCode = TypeCode.UInt32;
							length = 4;
							break;
						case FormatType.UINT64:
							typeCode = TypeCode.UInt64;
							length = 8;
							break;
						case FormatType.FLOAT32:
							typeCode = TypeCode.Single;
							length = 4;
							break;
						case FormatType.FLOAT64:
							typeCode = TypeCode.Double;
							length = 8;
							break;
						case FormatType.Length1:
							typeCode = TypeCode.Empty;
							length = 1;
							break;
						case FormatType.Length2:
							typeCode = TypeCode.Empty;
							length = 2;
							break;
						case FormatType.Length4:
							typeCode = TypeCode.Empty;
							length = 4;
							break;
						case FormatType.Length8:
							typeCode = TypeCode.Empty;
							length = 8;
							break;
						case FormatType.Length16:
							typeCode = TypeCode.Empty;
							length = 16;
							break;
						default:
							typeCode = TypeCode.Empty;
							length = 0;
							break;
					}
					return typeCode;
				}

				public static FormatType GetFormatType(TypeCode typeCode, out byte length)
				{
					FormatType format;
					switch (typeCode)
					{
						case TypeCode.Byte:
							format = FormatType.UINT8;
							length = 1;
							break;
						case TypeCode.SByte:
							format = FormatType.INT8;
							length = 1;
							break;
						case TypeCode.Int16:
							format = FormatType.INT16;
							length = 2;
							break;
						case TypeCode.UInt16:
							format = FormatType.UINT16;
							length = 2;
							break;
						case TypeCode.Int32:
							format = FormatType.INT32;
							length = 4;
							break;
						case TypeCode.UInt32:
							format = FormatType.UINT32;
							length = 4;
							break;
						case TypeCode.Single:
							format = FormatType.FLOAT32;
							length = 4;
							break;
						case TypeCode.Int64:
							format = FormatType.INT64;
							length = 8;
							break;
						case TypeCode.UInt64:
							format = FormatType.UINT64;
							length = 8;
							break;
						case TypeCode.Double:
							format = FormatType.FLOAT64;
							length = 8;
							break;
						default:
							format = FormatType.Unknown;
							length = 0;
							break;
					}
					return format;
				}

				[XmlIgnore]
				[Browsable(false)]
				public IVariableMarshaler Marshaler
				{
					get { return marshaler; }
					set { marshaler = value; }
				} internal IVariableMarshaler marshaler;

				public static Variable Parse(byte[] messageSpec, ref int loc, bool construct)
				{
					Variable var=null;
					Orientation orient;
					Qualifier qual;
					List<Orientation> orientations=null;
					List<Qualifier> qualifiers=null;
					string str;
					int i;
					if (construct)
					{
						var = new Variable();
						orientations = new List<Orientation>();
						qualifiers = new List<Qualifier>();
					}
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.Name:
								for (i=1; messageSpec[i+loc]>0; i++) ;
								if (construct)
								{
									str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
									var.Name = str;
									currentInterface.variableByName[str] = var;
								}
								loc += i;
								break;
							case (int)StructuredQuery.sq.Kind:
								for (i=1; messageSpec[i+loc]>0; i++) ;
								if (construct)
								{
									str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
									var.Kind = str;
								}
								loc += i;
								break;
							case (int)StructuredQuery.sq.Format:
								if (construct)
									var.Format = (Variable.FormatType)messageSpec[loc+1];
								loc++;
								break;
							case (int)StructuredQuery.sq.Length:
								if (construct)
									var.Length = messageSpec[loc+1];
								loc++;
								break;
							case (int)StructuredQuery.sq.LengthStr:
								for (i=1; messageSpec[i+loc]>0; i++) ;
								if (construct)
								{
									str = new string(Encoding.ASCII.GetChars(messageSpec, loc+1, i-1));
									var.Length = int.Parse(str);
								}
								loc += i;
								break;
							case (int)StructuredQuery.sq.Location:
								loc++;
								Location location = Location.Parse(messageSpec, ref loc, construct);
								if (construct)
									var.location = location;
								break;
							case (int)StructuredQuery.sq.Orientation:
								loc++;
								orient = Orientation.Parse(messageSpec, ref loc, construct);
								if (construct)
									orientations.Add(orient);
								break;
							case (int)StructuredQuery.sq.Qualifier:
								loc++;
								qual = Qualifier.Parse(messageSpec, ref loc, construct);
								if (construct)
									qualifiers.Add(qual);
								break;
							default:
								if (construct)
								{
									if (orientations.Count > 0)
										var.orientations = orientations.ToArray();
									if (qualifiers.Count > 0)
										var.qualifiers = qualifiers.ToArray();
								}
								loc--;
								return var;
						}
					}
				}
/*	{
		switch ( *op )
		{
		case sqQualifier:
			qual = Qualifier::Parse(++op,construct);
			if ( construct)
			{
				XmlSerializableNode* node = (XmlSerializableNode*)vdef->mQualifiers;
				if ( node == NULL )
					vdef->mQualifiers = qual;
				else
				{
					while ( node->mNext )
						node = node->mNext;
					node->mNext = (XmlSerializableNode*)qual;
				}
			}
			break;
		default:
			messageSpec = op-1;
			return vdef;
		}
	}
}
*/
				#region Properties

				[XmlAttribute("accuracy")]
				public string Accuracy
				{
					get { return accuracy; }
					set { accuracy = value; }
				} string accuracy;

				public Curve Curve
				{
					get { return curve; }
					set { curve = value; }
				} Curve curve;

				[XmlAttribute("dataType")]
				public FormatType DataType
				{
					get { return format; }
					set { format = value; }
				}

				[XmlAttribute("defaultValue")]
				public string DefaultValue
				{
					get { return defaultValue; }
					set { defaultValue = value; }
				} string defaultValue;

				[XmlAttribute("description")]
				public string Description
				{
					get { return descr; }
					set { descr = value; }
				} string descr;

				//[XmlElement("Drange")]
				public Drange Drange
				{
					get { return drange; }
					set { drange = value; }
				} Drange drange;

				[XmlElement("Enumeration")]
				public Drange Enumeration
				{
					get { return drange; }
					set { drange = value; }
				}

				[XmlAttribute("format")]
				public FormatType Format
				{
					get { return format; }
					set { format = value; }
				} FormatType format = FormatType.Unknown;

				[XmlAttribute("id")]
				public short Id
				{
					get { return id; }
					set { id = value; }
				} short id;

				[XmlAttribute("invalidValue")]
				[DefaultValue("")]
				public string InvalidValue
				{
					get { return invalidValue; }
					set { invalidValue = value; }
				} string invalidValue;

				public bool IsPublishableLength
				{
					get
					{
						return length <= parentInterface.Xteds.Host.PublishableByteArrayLength;
					}
				}

				[XmlAttribute("kind")]
				public string Kind
				{
					get { return kind; }
					set { kind= value; }
				} string kind;

				[XmlAttribute("length")]
				[DefaultValue(1)]
				public int Length
				{
					get { return length; }
					set { length = value; }
				} int length = 1;

				[XmlElement("Location")]
				public Location Location {
					get { return location; }
					set { location = value; }
				} Location location;

				[XmlAttribute("name")]
				public string Name
				{
					get { return name; }
					set
					{ 
						name = value;
                        if (currentInterface != null)
                            currentInterface.variableByName[name] = this;
					}
				} string name;

				[XmlAttribute("numberOfArrayElements")]
				[DefaultValue(1)]
				public int NumberOfArrayElements
				{
					get { return length; }
					set { length = value; }
				}

				[XmlElement("Orientation", typeof(Orientation))]
				public Orientation[] Orientations
				{
					get { return orientations; }
					set { orientations = value; }
				} Orientation[] orientations;

				[XmlAttribute("precision")]
				public int Precision
				{
					get { return precision; }
					set { precision = value; }
				} int precision;

				[XmlElement("Qualifier", typeof(Qualifier))]
				public Qualifier[] Qualifiers
				{
					get { return qualifiers; }
					set { qualifiers = value; }
				} Qualifier[] qualifiers;

				[XmlAttribute("rHigh")]
				public double RedHigh {
					get { return redHigh; }
					set { redHigh = value; }
				} double redHigh;

				[XmlAttribute("rLow")]
				public double RedLow {
					get { return redLow; }
					set { redLow = value; }
				} double redLow;

				[XmlAttribute("rangeMax")]
				public double RangeMax
				{
					get { return rangeMax; }
					set { rangeMax = value; }
				} double rangeMax;

				[XmlAttribute("rangeMin")]
				public double RangeMin
				{
					get { return rangeMin; }
					set { rangeMin = value; }
				} double rangeMin;

				[XmlAttribute("scaleFactor")]
				public double ScaleFactor
				{
					get { return scaleFactor; }
					set { scaleFactor = value; }
				} double scaleFactor = 1;

				[XmlAttribute("scaleUnits")]
				public string ScaleUnits
				{
					get { return scaleUnits; }
					set { scaleUnits = value; }
				} string scaleUnits;

				[XmlAttribute("timeModel")]
				public string TimeModel { get; set; }

				[XmlAttribute("testBypassTag")]
				public string xmlTestBypassTag
				{
					get { return string.Format("0x{0:X}", TestBypassTag); }
					set
					{
						if (value.StartsWith("0x"))
							TestBypassTag = Int32.Parse(value.Substring(2), NumberStyles.HexNumber);
						else
							TestBypassTag = Int32.Parse(value);
						ParentInterface.Xteds.AddTestBypass(this);
					}
				}

				public int TestBypassTag { get; set; }

				[XmlAttribute("units")]
				public string Units
				{
					get { return units; }
					set { units = value; }
				} string units;

				[XmlAttribute("yHigh")]
				public double YellowHigh {
					get { return yellowHigh; }
					set { yellowHigh = value; }
				} double yellowHigh;

				[XmlAttribute("yLow")]
				public double YellowLow {
					get { return yellowLow; }
					set { yellowLow = value; }
				} double yellowLow;

				[XmlIgnore]
				public bool IsNotified
				{
					get
					{
						lock(mLoaderLock)
						{
							if (parentInterface == null ||
								parentInterface.dataMessages == null)
								return false;

							foreach (var msg in ParentInterface.dataMessages)
								foreach (VariableRef vref in msg.VariableRefs)
									if (vref.VariableDef == this)
										return true;

							if ( parentInterface.replyMessages == null ) return false;

							foreach (var msg in ParentInterface.replyMessages)
								foreach (VariableRef vref in msg.VariableRefs)
									if (vref.VariableDef == this)
										return true;
						}
						return false;
					}
				}

				[XmlIgnore]
				public int Size
				{
					get
					{
						if ( size == 0 )
							switch ( format )
							{
								case FormatType.UINT8: size = 1; break;
								case FormatType.INT8: size = 1; break;
								case FormatType.UINT16: size = 2; break;
								case FormatType.INT16: size = 2; break;
								case FormatType.UINT32: size = 4; break;
								case FormatType.INT32: size = 4; break;
								case FormatType.FLOAT32: size = 4; break;
								case FormatType.FLOAT64: size = 8; break;
								case FormatType.UINT64: size = 8; break;
								case FormatType.INT64: size = 8; break;
								case FormatType.BUFFER: size = 1; break;
								case FormatType.STRING: size = 1; break;
								case FormatType.FLOAT128: size = 16; break;
								case FormatType.OBJECT: size = 4; break;
								case FormatType.ENUM32: size = 4; break;
							}
						return size;
					}
				} int size;

				[XmlIgnore]
				public Interface ParentInterface
				{
					get { return parentInterface; }
					set { parentInterface = value; }
				} Interface parentInterface;

				#endregion 

				public Type EnumType
				{
					get
					{
						if (enumType == null)
						{
							if (drange == null) return enumType;
							AppDomain domain = Thread.GetDomain();
							AssemblyName name = new AssemblyName();
							name.Name = "EnumAssembly";
							AssemblyBuilder asmBuilder = domain.DefineDynamicAssembly(
								name, AssemblyBuilderAccess.Run);
							ModuleBuilder modBuilder =
								asmBuilder.DefineDynamicModule("EnumModule");
							EnumBuilder enumBuilder = modBuilder.DefineEnum(string.Format("{0}Drange", Name),
								TypeAttributes.Public,
								typeof(System.Int32));

							if ( drange.Options[0].Value != 0 )
								enumBuilder.DefineLiteral("unset", 0);

							foreach (var opt in drange.Options)
								enumBuilder.DefineLiteral(opt.Name, opt.Value);
                            enumBuilder.DefineLiteral("__INVALID__", drange.Options.Select(x => x.Value).Max() + 1);
							enumType = enumBuilder.CreateType();
						}
						return enumType;
					}
				} Type enumType;


				public override string ToString(){return name;}
			}

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Curve
			{
				[XmlAttribute("description")]
				public string Description
				{
					get { return descr; }
					set { descr = value; }
				} string descr;

				[XmlAttribute("name")]
				public string Name
				{
					get { return name; }
					set { name = value; }
				} string name;

				[XmlElement("Coefficient", typeof(Coefficient))]
				public Coefficient[] Coefficients
				{
					get { return coefficients; }
					set { coefficients = value; }
				} Coefficient[] coefficients;

				public override string ToString() { return name; }
			}

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Coefficient
			{
				[XmlAttribute("description")]
				public string Description
				{
					get { return description; }
					set { description = value; }
				} string description;

				[XmlAttribute("exponent")]
				public int Exponent
				{
					get { return exponent; }
					set { exponent = value; }
				} int exponent;

				[XmlAttribute("value")]
				public double Value
				{
					get { return val; }
					set { val = value; }
				} double val;

				public override string ToString() { return exponent.ToString(); }
			}

            /// <summary>
            /// Discrete range of values
            /// </summary>
			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Drange
			{
                /// <summary>
                /// Name of the discrete value collection
                /// </summary>
				[XmlAttribute("name")]
				public string Name
				{
					get { return name; }
					set { name = value; }
				} string name;

                /// <summary>
                /// Description of the discrete value collection
                /// </summary>
				[XmlAttribute("description")]
				public string Description
				{
					get { return descr; }
					set { descr = value; }
				} string descr;

                /// <summary>
                /// List of options/discrete values
                /// </summary>
				[XmlElement("Option", typeof(Option))]
				public Option[] Options
				{
					get { return options; }
					set { options = value; }
				} Option[] options;

                /// <summary>
                /// return the string equivalent
                /// </summary>
                /// <returns></returns>
				public override string ToString(){return name;}
			}

            /// <summary>
            /// A discrete value used in a Drange
            /// </summary>
			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Option
			{
                /// <summary>
                /// The symbolic name of the value
                /// </summary>
				[XmlAttribute("name")]
				public string Name
				{
					get { return name; }
					set { name = value; }
				} string name;

                /// <summary>
                /// The ordinal value used by SSM
                /// </summary>
				[XmlAttribute("optionValue")]
				public int OptionValue
				{
					get { return val; }
					set { val = value; }
				}

                /// <summary>
                /// The integer ordinal value of the discrete value
                /// </summary>
				[XmlAttribute("value")]
				public int Value
				{
					get { return val; }
					set { val = value; }
				} int val;

                /// <summary>
                /// Description of the discrete value
                /// </summary>
				[XmlAttribute("description")]
				public string Description
				{
					get { return descr; }
					set { descr = value; }
				} string descr;

                /// <summary>
                /// The string equivalent
                /// </summary>
                /// <returns></returns>
				public override string ToString(){return name;}
			}

			#endregion 

			#region VariableRef

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class VariableRef
			{
				public VariableRef()
				{
				}

				internal VariableRef(byte[] messageSpec, ref int loc, Interface iface)
				{
					for (; ; loc++)
					{
						switch (messageSpec[loc])
						{
							case (int)StructuredQuery.sq.Id:
								loc++;
								variableDef = iface.FindVariable(messageSpec,ref loc);
								name = variableDef.Name;
								Message.currentMessage.VariableList.Add(new VariableValue(variableDef));
								break;
							default:
								loc--;
								return;
						}
					}
				}

				[XmlAttribute("name")]
				public string Name
				{
					get { return name; }
					set
					{ 
						name = value; 
						variableDef = currentInterface.variableByName[name] as Variable;
						if (variableDef == null)
							MsgConsole.WriteLine("Can't find variable {0} on {1}.{2}.{3}", name, currentInterface.Xteds, currentInterface, Message.currentMessage);
						else
							Message.currentMessage.VariableList.Add(new VariableValue(variableDef));
					}
				} string name;
				public override string ToString(){return "@"+name;}

				[XmlIgnore]
				public Variable VariableDef
				{
					get { return variableDef; }
				} Variable variableDef;
			}

			#endregion 

			#region VariableValue

			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class VariableValue : IVariable
			{
				public enum ValueFormat { None, BufferAsUuid, String, Uuid };
				Blackboard.Item blackboardItem;

                /// <summary>
                /// Gets the <see cref="Blackboard.Item"/> that is associated with this <see cref="VariableValue"/>.
                /// </summary>
                /// <remarks>Created when the <see cref="Publish"/> method is called.</remarks>
                public Blackboard.Item PublishedAs
                {
                    get { return blackboardItem; }
                }
				int tag;
				static int tags=1;
				public VariableValue(Variable var)
				{
					variable = var;
					mMessage = Message.currentMessage;
					tag = tags++;

					switch ( variable.Format )
					{
						case Variable.FormatType.UINT8:
							if (variable.Length == 1)
							{
								if (variable.Kind == "boolean")
									val = new Boolean();
								else if (variable.Drange == null)
									val = new Byte();
								else
									val = EnumValue(true);//OwningMessage is CommandMessage);
							}
							else
							{
								if (variable.Drange == null &&
								 (variable.Kind == "string" || variable.Kind == "String" ||
								  variable.Units == "string"))
								{
									val = new string(' ', 0);
									mValueFormat = ValueFormat.String;
								}
								else if (variable.Kind=="boolean")
									val = new Boolean[variable.Length];
								else if (variable.Units=="uuid" || variable.Kind.Contains("UID"))
								{
									val = new Uuid();
									mValueFormat = ValueFormat.Uuid;
								}
								else
									val = new Byte[variable.Length];
							}
							break;
						case Variable.FormatType.INT8:
							if (variable.Length == 1)
							{
								if (variable.Kind=="boolean")
									val = new Boolean();
								else if (variable.Drange == null)
									val = new SByte();
								else
									val = EnumValue(true);//OwningMessage is CommandMessage);
							}
							else
							{
								if (variable.Drange == null &&
									(variable.Kind == "string" || variable.Kind == "String") ||
									 variable.Units == "string")
								{
									val = new string(' ', 0);
									mValueFormat = ValueFormat.String;
								}
								else if (variable.Kind=="boolean")
									val = new Boolean[variable.Length];
								else if (variable.Units=="uuid" || variable.Kind.Contains("UID"))
								{
									val = new Uuid();
									mValueFormat = ValueFormat.Uuid;
								}
								else
									val = new SByte[variable.Length];
							}
							break;
						case Variable.FormatType.UINT16:
							if (variable.Length == 1)
							{
								if (variable.Drange == null)
									val = new UInt16();
								else
									val = EnumValue(true);//OwningMessage is CommandMessage);
							}
							else
								val = new UInt16[variable.Length];
							break;
						case Variable.FormatType.INT16:
							if ( variable.Length == 1 )
								val = new Int16();
							else
								val = new Int16[variable.Length];
							break;
						case Variable.FormatType.UINT32:
							if (variable.Length == 1)
								val = new UInt32();
							else
								val = new UInt32[variable.Length];
							break;
						case Variable.FormatType.INT32:
							if (variable.Length == 1)
								val = new Int32();
							else
								val = new Int32[variable.Length];
							break;
						case Variable.FormatType.UINT64:
							if (variable.Length == 1)
								val = new UInt64();
							else
								val = new UInt64[variable.Length];
							break;
						case Variable.FormatType.INT64:
							if (variable.Length == 1)
								val = new Int64();
							else
								val = new Int64[variable.Length];
							break;
						case Variable.FormatType.FLOAT32:
							if ( variable.Length == 1 )
								val = new Single();
							else
							{
								if ( variable.Length == 3 && variable.Qualifiers !=null )
								{
									foreach ( Qualifier qual in variable.Qualifiers )
										if ( qual.Name.EndsWith("epresentation") && qual.Value == "vector" )
										{
											val = new Vector3();
											break;
										}
									if ( val == null )
										val = new Single[variable.Length];
								}
								else if ( variable.Length == 4 && variable.Qualifiers !=null )
								{
									foreach ( Qualifier qual in variable.Qualifiers )
										if ( qual.Name.EndsWith("epresentation") && qual.Value == "quaternion" )
										{
											val = new Quaternion();
											break;
										}
									if ( val == null )
										val = new Single[variable.Length];
								}
								else
									val = new Single[variable.Length];
							}
							break;
						case Variable.FormatType.FLOAT64:
							if ( variable.Length == 1 )
								val = new Double();
							else
							{
								if ( variable.Length == 3 && variable.Qualifiers !=null )
								{
									foreach ( Qualifier qual in variable.Qualifiers )
										if ( qual.Name.EndsWith("epresentation") && qual.Value == "vector" )
										{
											val = new Vector3();
											break;
										}
									if ( val == null )
										val = new Double[variable.Length];
								}
								else if ( variable.Length == 4 && variable.Qualifiers !=null )
								{
									foreach ( Qualifier qual in variable.Qualifiers )
										if ( qual.Name.EndsWith("epresentation") && qual.Value == "quaternion" )
										{
											val = new Quaternion();
											break;
										}
									if ( val == null )
										val = new Double[variable.Length];
								}
								else
									val = new Double[variable.Length];
							}
							break;
						case Variable.FormatType.BUFFER:
							if (variable.Kind.Contains("UID"))
								mValueFormat = ValueFormat.BufferAsUuid;
							if (variable.Length == 1)
								val = new MarshaledBuffer();
							else
								val = new MarshaledBuffer[variable.Length];
							break;
						case Variable.FormatType.STRING:
							mValueFormat = ValueFormat.String;
							if (variable.Length == 1)
								val = new MarshaledString();
							else
								val = new MarshaledString[variable.Length];
							//val = new string(' ', variable.Length);
							break;
						default:
							MsgConsole.WriteLine("variable {0}: unknown format {1}", variable, variable.Format);
							break;
					}
					if (variable.DefaultValue != null && !val.GetType().IsEnum)
					{
						var value = DefaultValue();
						if (value != null)
							val = value;
					}
				}

				object DefaultValue()
				{
					if (variable.Drange != null && variable.Drange.Options != null)
					{
						return EnumValue(true);
						//int defaultVal = 0;
						//if (int.TryParse(variable.DefaultValue, out defaultVal))
						//{
						//	Option opt = variable.Drange.Options.FirstOrDefault(op => op.Value == defaultVal);
						//	if (opt != null)
						//		return defaultVal;
						//}
						//Option o = variable.Drange.Options.FirstOrDefault(op => op.Name == variable.DefaultValue);
						//if (o != null)
						//	return o.Value;
					}
					float fValue; double dValue;
					sbyte sbValue; byte bValue;
					short sValue; ushort usValue;
					int iValue; uint uiValue;
					long lValue; ulong ulValue;
					switch (Variable.Format)
					{
						case Variable.FormatType.FLOAT32:
							if (float.TryParse(variable.DefaultValue, out fValue))
								return fValue;
							else break;
						case Variable.FormatType.FLOAT64:
							if (double.TryParse(variable.DefaultValue, out dValue))
								return dValue;
							else break;
						case Variable.FormatType.INT8:
							if (sbyte.TryParse(variable.DefaultValue, out sbValue))
								return sbValue;
							else break;
						case Variable.FormatType.INT16:
							if (short.TryParse(variable.DefaultValue, out sValue))
								return sValue;
							else break;
						case Variable.FormatType.INT32:
							if (int.TryParse(variable.DefaultValue, out iValue))
								return iValue;
							else break;
						case Variable.FormatType.INT64:
							if (long.TryParse(variable.DefaultValue, out lValue))
								return lValue;
							else break;
						case Variable.FormatType.UINT8:
							if (byte.TryParse(variable.DefaultValue, out bValue))
								return bValue;
							else break;
						case Variable.FormatType.UINT16:
							if (ushort.TryParse(variable.DefaultValue, out usValue))
								return usValue;
							else break;
						case Variable.FormatType.UINT32:
							if (uint.TryParse(variable.DefaultValue, out uiValue))
								return uiValue;
							else break;
						case Variable.FormatType.UINT64:
							if (ulong.TryParse(variable.DefaultValue, out ulValue))
								return ulValue;
							else break;
						case Variable.FormatType.STRING:
							return variable.DefaultValue;
					}
					return null;
				}

				object EnumValue(bool initialize)
				{
					var type = variable.EnumType;
					if (!initialize)
						return Activator.CreateInstance(type);
					else if (variable.DefaultValue != null && variable.DefaultValue.Length > 0)
						return Enum.Parse(type, variable.DefaultValue);
					else
						return Enum.Parse(type, variable.Drange.Options[0].Name);
				}

				ValueFormat mValueFormat = ValueFormat.None;
				public bool IsFormatted { get { return mValueFormat != ValueFormat.None; } }
				public string FormattedValue
				{
					get
					{
						switch (mValueFormat)
						{
							case ValueFormat.BufferAsUuid:
								var buf = val as MarshaledBuffer;
								if (buf.Bytes == null)
									return new Uuid().ToString();
								else
									return new Uuid(buf.Bytes,buf.Offset).ToString();
							case ValueFormat.String:
								return val.ToString();
							case ValueFormat.Uuid:
								return ((Uuid)val).ToString();
						}
						return string.Empty;
					}
					set
					{
						switch (mValueFormat)
						{
							case ValueFormat.BufferAsUuid:
								var buf = val as MarshaledBuffer;
								var uuid = new Uuid(value);
								buf.Set(16, uuid.ToByteArray(), 0);
								break;
							case ValueFormat.String:
								var str = val as MarshaledString;
								str.Set(value);
								break;
							case ValueFormat.Uuid:
								val = new Uuid(value);
								break;
						}
					}
				}

				public void Get(byte[] buffer, int offset, MemberInfo info)
				{
				}

				public string Kind { get { return variable.Kind; } }

				public int Size { get { return variable.Size; } }

				public TypeCode TypeCode
				{
					get
					{
						switch (Variable.Format)
						{
							case Variable.FormatType.FLOAT32:
								return TypeCode.Single;

							case Variable.FormatType.FLOAT64:
								return TypeCode.Double;

							case Variable.FormatType.INT8:
								return TypeCode.SByte;

							case Variable.FormatType.INT16:
								return TypeCode.Int16;

							case Variable.FormatType.INT32:
								return TypeCode.Int32;

							case Variable.FormatType.INT64:
								return TypeCode.Int64;

							case Variable.FormatType.UINT8:
								return TypeCode.Byte;

							case Variable.FormatType.UINT16:
								return TypeCode.UInt16;

							case Variable.FormatType.UINT32:
								return TypeCode.UInt32;

							case Variable.FormatType.UINT64:
								return TypeCode.UInt64;
						}

						return TypeCode.Object;
					}
				}

				public string Name { get { return variable.Name; } }
				public override string ToString() { return variable.ToString();} 


				public Blackboard.Item Publish(string path)
				{
					Type type = val.GetType();
					if (
						(type == typeof(Byte[]) || type == typeof(SByte[])) && !variable.IsPublishableLength && type != typeof(string))
						return blackboardItem;

					var attr = new BlackboardAttribute();
					if ( variable.Description != null ) attr.Description = variable.Description;
					if ( variable.Units != null ) attr.Units = variable.Units;
					blackboardItem = Blackboard.Publish(this,path+'.'+variable.Name,"Value",null,false);
					if ( mMessage.messageType is Notification)
						blackboardItem.SubscribedTo += blackboardItem_SubscribedTo;
					blackboardItem.AddAttributeInfo( attr);

					if (variable.DefaultValue != null)
					{
						var value = DefaultValue();
						if ( value != null )
							blackboardItem.Value = value;
					}

                    return blackboardItem;
				}

				void blackboardItem_SubscribedTo(object sender, EventArgs e)
				{
					mMessage.OwnerInterface.Xteds.Host.NeedSubscription(mMessage as IDataMessage);
				}

				public void NotifyOnChange()
				{
					if ( blackboardItem != null )
						blackboardItem.RaiseValueChanged();
				}

				[XmlIgnore]
				[TypeConverter(typeof(ExpandableObjectConverter))]
                [Editor(typeof(DRangeTypeEditor),typeof(System.Drawing.Design.UITypeEditor))]
				public object Value
				{
					get { return val; }
					set { val = value; }
				} internal object val;

				[XmlIgnore]
				public int Length
				{
					get { return variable.Length; }
				}

				public int Offset { get { return offset; } } int offset;

				[XmlIgnore]
				public PrimitiveType Type
				{
					get { return (PrimitiveType)(int)variable.Format; }
				}

				[XmlIgnore]
				public Variable Variable
				{
					get { return variable; }
				} Variable variable;

				/// <summary>
				/// Gets the <see cref="Message"/> that holds this <see cref="VariableValue"/>
				/// </summary>
				[XmlIgnore]
				[TypeConverter(typeof(ExpandableObjectConverter))]
				public Message OwningMessage
				{
					get { return mMessage; }
				} Message mMessage = null;
				
				/// <summary>
				/// Binds this <see cref="VariableValue"/> to the given <see cref="Message"/>
				/// </summary>
				/// <param name="owner"></param>
				internal int Bind(Message owner, int offset)
				{
					mMessage = owner;
					this.offset = offset;
					return Variable.Length*Variable.Size;

				}

				public VariableValue Clone(Message message)
				{
					var clone = new VariableValue(variable);
					clone.mValueFormat = mValueFormat;
					clone.offset = offset;
					clone.mMessage = message;
					return clone;
				}
			}

			#endregion 
		}
		#endregion

		#region Diagnostics

		static string where = "xteds";
		static void UnknownAttribute(object sender, XmlAttributeEventArgs e)
		{
			if(e.Attr.Name != "xsi:schemaLocation")
			{
				MsgConsole.WriteLine("{0} UnknownAttribute, {1}", where, e.Attr.Name);
			}
		}
		static void UnknownElement(object sender, XmlElementEventArgs e)
		{
			MsgConsole.WriteLine("{0} UnknownElement, {1}", where, e.Element.Name);
		}
		static void UnknownNode(object sender, XmlNodeEventArgs e)
		{
			if(e.LocalName != "schemaLocation")
			{
				MsgConsole.WriteLine("{0} UnknownNode, n {1} ln {2} nt {3} obj {4} t {5}",
					where, e.Name, e.LocalName, e.NodeType, e.ObjectBeingDeserialized, e.Text);
			}
		}
		static void UnreferencedObject(object sender, UnreferencedObjectEventArgs e)
		{
			MsgConsole.WriteLine("{0} UnreferencedObject, {1} {2}", where, e.UnreferencedId, e.UnreferencedObject);
		}

		#endregion 

	}
}
