using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Xml;
using System.Xml.Serialization;

using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// A configuration of models bound in an executable framework
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class Scenario
	{
		static List<IManager> managers = new List<IManager>();
		/// <summary>
		/// 
		/// </summary>
		public static Scenario The { get; private set; }
		static List<Type> types = new List<Type>();

		/// <summary>
		/// The Clock
		/// </summary>
		protected Clock mClock = new Clock();
		Dynamics mDynamics = new Dynamics();
		Executive mExecutive;
		ModelList executors = new ModelList();
		ModelList models = new ModelList();
		XmlSerializer mSerializer;
		string mFileName;

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler IsDirty;
		internal static void RaiseIsDirty()
		{
			if ( The != null && The.IsDirty != null )
				The.IsDirty(The,EventArgs.Empty);
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public Scenario()
		{
			The = this;
			mExecutive = new Executive(this);
			mExecutive.ModeChanged += mExecutive_ModeChanged;
			Models.Add(mClock);
			Name = GetType().Name;
			BlackboardBindings = new List<Blackboard.Binding>();
		}

		void mExecutive_ModeChanged(ExecutiveMode previousMode, ExecutiveMode mode)
		{
			if (mode != ExecutiveMode.Executing)
				logFile.Flush();
		}

		/// <summary>
		/// This scenario is the active one
		/// </summary>
		public static Scenario Active
		{
			get { return mActive; }
			set
			{
				mActive = value;
				ModelMgr.Scenario = value;
			}
		} static Scenario mActive;

		/// <summary>
		/// Add a manager to the list. Managers are singletons that are notified when a scenario is unloaded
		/// </summary>
		/// <param name="manager"></param>
		public static void AddManager(IManager manager)
		{
			managers.Add(manager);
		}

		// Keep Parameters in front of Clock so they are available during loading of subsequent models
		/// <summary>
		/// Scenario level name/values
		/// </summary>
		[XmlElement("Parameter", typeof(Parameter))]
		public List<Parameter> Parameters { get; set; }

		/// <summary>
		/// The Clock singleton
		/// </summary>
		public Clock Clock
		{
			get { return mClock; }
			set
			{
				mClock = value;
				Models[0] = mClock;
			}
		}

    static void CloseLogFile()
    {
      if (logFileIsOpen)
      {
        logFile.Close();
        logFileIsOpen = false;
      }
    }
    
    /// <summary>
		/// Create a Scenario from a file or an assembly
		/// </summary>
		/// <param name="name"></param>
		/// <param name="assemblyName"></param>
		/// <returns></returns>
		public static Scenario Create(string name, string assemblyName)
		{
			OpenLogFile(name);

			Scenario scenario = null;
			try
			{
				if ( assemblyName.EndsWith(".dll") || assemblyName.EndsWith(".DLL") )
					assemblyName = assemblyName.Substring(0,assemblyName.LastIndexOf('.'));
				Assembly.ReflectionOnlyLoad(assemblyName);

				var assembly = AppDomain.CurrentDomain.Load(assemblyName);
				if (assembly != null)
				{
					mDirectory = System.IO.Path.GetDirectoryName(assembly.Location) + '\\';
					FileUtilities.ScenarioDirectory = mDirectory;
					foreach (var type in assembly.GetTypes())
						if (type.IsSubclassOf(typeof(Scenario)) && type.Name.StartsWith(name) )
						{
							scenario = type.InvokeMember("",
								BindingFlags.Instance | BindingFlags.CreateInstance | BindingFlags.Public,
								null, null, null) as Scenario;
							break;
						}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				var text = Log.ExceptionText(e.LoaderExceptions[0]);
				for (int i = 1; i < e.LoaderExceptions.Length; i++)
					text = Environment.NewLine + Log.ExceptionText(e.LoaderExceptions[i]);
				Log.ReportException(e, text);
			}
			catch (System.IO.FileNotFoundException e)
			{
				var text = e.FileName + Environment.NewLine + "FusionLog: " + e.FusionLog;
				Log.ReportException(e, text);
			}
			catch (Exception e)
			{
				Log.ReportException(e);
			}
			if (scenario == null)
			{
				Log.WriteLine("Can't create scenario {0} from {1}; defaulting",name,assemblyName);
				scenario = new Scenario();
			}
			return scenario;
		}

		/// <summary>
		/// The home directory for this scenario
		/// </summary>
		public static string Directory
		{
			get { return mDirectory; }
			set
			{
				if (value.EndsWith("\\") || value.EndsWith("/"))
					mDirectory = value;
				else
					mDirectory = value + '\\';
				FileUtilities.ScenarioDirectory = mDirectory;
			} 
		}
		static string mDirectory;

		/// <summary>
		/// Model discovery phase
		/// </summary>
		public virtual void Discover()
		{
			mDynamics.Discover(this);
			foreach (var model in models)
				model.Discover(this);
		}

		/// <summary>
		/// Access the Dynamics engine
		/// </summary>
		public Dynamics Dynamics
		{
			get { return mDynamics; }
			//set
			//{
			//	mDynamics = value;
			//}
		}

		/// <summary>
		/// Model execute phase
		/// </summary>
		public virtual void Execute()
		{
			if (mReschedule)
			{
				executors.Clear();
				if (mDynamics.States.Count > 0)
					executors.Add(mDynamics);
				foreach (var model in models)
					if (model.Enabled)
						executors.Add(model);
				mReschedule = false;
			}
			foreach (var model in executors)
				model.Execute();
			foreach (var binding in BlackboardBindings)
				binding.Update();
		} bool mReschedule = true;

		/// <summary>
		/// Get the Executive
		/// </summary>
		public Executive Executive { get { return mExecutive; } }

		/// <summary>
		/// Find type information using its name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Type FindType(string name)
		{
			foreach (var type in types)
				if (type.Name == name)
					return type;
			return null;
		}
		/// <summary>
		/// Reset internal state
		/// </summary>
		public virtual void Initialize()
		{
			foreach (var model in models)
				model.Initialize();
	
			foreach (var binding in BlackboardBindings)
				binding.Update();
		}

		/// <summary>
		/// After initialization, should the scenario start the time base
		/// </summary>
		[XmlAttribute("running")]
		public bool InitiallyRunning { get; set; }

		/// <summary>
		/// Initial date/time for the scenario. Unspecified uses Now.
		/// </summary>
		[XmlIgnore]
		public string InitialTime { get { return mClock.InitialTime; } set { mClock.InitialTime = value; } }

		/// <summary>
		/// Load a scenario from a file
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="references"></param>
		/// <returns></returns>
		public static Scenario Load(string fileName, ObservableCollection<AssemblyReference> references)
		{
			OpenLogFile(Path.GetFileNameWithoutExtension(fileName));

			types.Clear();

			types.Add(typeof(Model));

			try
			{
				foreach ( var reference in references )
				{
					string referenceName = reference.Name;
					if (reference.Name.EndsWith(".dll") || reference.Name.EndsWith(".DLL"))
						referenceName = reference.Name.Substring(0, reference.Name.LastIndexOf('.'));
					Assembly.ReflectionOnlyLoad(referenceName);

					var assembly = AppDomain.CurrentDomain.Load(referenceName);
					if (assembly != null)
					{
						reference.Assembly = assembly;
						reference.AssemblyTypes = assembly.GetTypes();
						foreach (var type in reference.AssemblyTypes)
							if (type.IsSubclassOf(typeof(Model)))
								types.Add(type);
							else if (reference.Types != null)
							{
								foreach (var refType in reference.Types)
									if (refType.Name == type.Name)
									{
										types.Add(type);
										break;
									}
							}
					}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				var text = Log.ExceptionText(e.LoaderExceptions[0]);
				for (int i = 1; i < e.LoaderExceptions.Length; i++)
					text = Environment.NewLine + Log.ExceptionText(e.LoaderExceptions[i]);
				Log.ReportException(e, text);
			}
			catch (System.IO.FileNotFoundException e)
			{
				var text = e.FileName + Environment.NewLine + "FusionLog: " + e.FusionLog;
				Log.ReportException(e, text);
			}
			catch (Exception e)
			{
				Log.ReportException(e);
			}

			var serializer = new XmlSerializer(typeof(Scenario),types.ToArray());

			Scenario scenario = null;
			Scenario.Directory = Path.GetDirectoryName(fileName);

			if (File.Exists(fileName))
			{
				var reader = new StreamReader(fileName);
				try
				{
					scenario = serializer.Deserialize(reader) as Scenario;
				}
				catch (InvalidOperationException ex)
				{
					Log.ReportException(ex, "Perhaps you are missing a reference in the scenario ?");
				}
				catch (Exception ex)
				{
					Log.ReportException(ex);
				}
				finally
				{
					reader.Close();
				}
			}
			if (scenario == null)
			{
				scenario = new Scenario();
				Log.WriteLine("Can't load scenario from {0}/{1}; defaulting",
					Environment.CurrentDirectory, fileName);
			}
			else
			{
				scenario.mFileName = fileName;
				scenario.mSerializer = serializer;
				foreach (var reference in references)
					if (reference.Register != null && reference.Assembly != null)
					{
						foreach (var type in reference.AssemblyTypes)
							if (type.Name == reference.Register)
								type.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
					}
			}
			return scenario;
		}

		/// <summary>
		/// List of models. All are not necessarily executing.
		/// </summary>
		[XmlElement("Model",typeof(Model))]
		public ModelList Models { get { return models; } set { models = value; } }

		/// <summary>
		/// <see cref="Blackboard.Binding"/>s used to wire together <see cref="Blackboard.Items"/>
		/// </summary>
		[XmlElement("Binding",typeof(Blackboard.Binding))]
		public List<Blackboard.Binding> BlackboardBindings { get; set; }

		/// <summary>
		/// Moniker
		/// </summary>
		[XmlIgnore]
		public string Name { get; set; }

		/// <summary>
		/// Opens a log file with a unique name to avoid clashes with other AspireStudio instances running the same scenario
		/// </summary>
		/// <param name="name"></param>
		static void OpenLogFile(string name)
		{
			string fileName = name;
			int count = 1;
			do
			{
				try
				{
					logFile = new StreamWriter(fileName + ".log");
				}
				catch (System.IO.IOException)
				{
					fileName = name + ';'+count++;
				}
				catch (Exception ex)
				{
					Log.ReportException(ex,"Can't open scenario log file {0}", name);
				}
			} while (logFile == null);
			logFileIsOpen = true;
			Log.NewText += Log_NewText;
		}
		static TextWriter logFile;
		static bool logFileIsOpen;

		/// <summary>
		/// File logger event handler
		/// </summary>
		/// <param name="text"></param>
		/// <param name="severity"></param>
		static void Log_NewText(string text, Log.Severity severity)
		{
			if ( logFileIsOpen )
				logFile.WriteLine(text);
		}

		/// <summary>
		/// Get a Parameter's value by name. null is not found, "" is found, but no value
		/// </summary>
		/// <param name="parameterName"></param>
		/// <returns></returns>
		public string this[string parameterName]
		{
			get
			{
				if (Parameters == null) return null;

				var param = Parameters.FirstOrDefault<Parameter>(p => p.Name.Equals(parameterName));
				if (param == null) return null;
				return param.Value;
			}
		}

		/// <summary>
		/// The Model list has been modified and we need to rebuild the execution list
		/// </summary>
		public static void Reschedule()
		{
			if ( Active != null )
				Active.mReschedule = true;
		}

		/// <summary>
		/// Save the scenario to disk.
		/// </summary>
		public void Save()
		{
			if (mFileName == null)
			{
				Log.WriteLine("Can't save scenario because it wasn't loaded from a file");
				return;
			}

			string extension = Path.GetExtension(mFileName);
			string fileName = Path.Combine(Path.GetDirectoryName(mFileName), Path.GetFileNameWithoutExtension(mFileName));
			var backup = fileName + ".bck" + extension;
			var newFileName = fileName + ".new" + extension;

			// Remove the Clock for serialization, since it has its own Property
			//lock(this) // need to add lock to Execute
			var orig = models;
			models = new ModelList();
			foreach (var m in orig)
				if ( m != mClock && m != mDynamics)
					models.Add(m);
			Model.IsSaving = true;
			var settings = new XmlWriterSettings(){Indent = true};
			try
			{
				using (XmlWriter writer = XmlWriter.Create(newFileName, settings))
					mSerializer.Serialize(writer, this);
				File.Delete(backup);
				File.Move(mFileName, backup);
				File.Move(newFileName, mFileName);
			}
			catch (Exception ex)
			{
				Log.ReportException(ex, "Trying to save scenario {0}", Name);
			}
			Model.IsSaving = false;
			models.Clear();
			models = orig;
		}

		/// <summary>
		/// Printable text moniker
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name;
		}

		/// <summary>
		/// Unload a scenario in preparation to load a new one.
		/// </summary>
		public void Unload()
		{
			mSerializer = null;
			mClock.ClearTimers();
			foreach (var manager in managers)
				manager.Unload();
			foreach (var model in Models)
				model.Unload();
			mExecutive.ModeChanged -= mExecutive_ModeChanged;
			logFile.Flush();
			Log.NewText -= Log_NewText;
			CloseLogFile();
			mExecutive.Scheduler.Reset(true);
		}

		/// <summary>
		/// Verify that each assembly used in a scenario is valid
		/// </summary>
		/// <param name="scenarioName"></param>
		/// <returns></returns>
		public static bool Valid(string scenarioName)
		{
			try
			{
				var assy = Assembly.ReflectionOnlyLoad(scenarioName);
				var attr = assy.GetCustomAttribute<AssemblyDescriptionAttribute>();
				if ( attr != null && attr.Description.StartsWith("AspireStudio scenario"))
					return true;
			}
			catch (Exception)
			{
				return false;
			}
			return false;
		}

		/// <summary>
		/// Scenario level name/values
		/// </summary>
		public class Parameter
		{
			/// <summary>
			/// Moniker
			/// </summary>
			[XmlAttribute("name")]
			public string Name { get; set; }
			/// <summary>
			/// Value
			/// </summary>
			[XmlAttribute("value"),DefaultValue("")]
			public string Value { get { return mValue; } set { mValue = value; } } string mValue = string.Empty;
		}
	}



}
