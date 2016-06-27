using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// Base class for all configurable, executable models
	/// </summary>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[XmlInclude(typeof(Clock)),XmlInclude(typeof(Blackboard.Binding))]
	public class Model : IPublishable, IPropertyCategoryInitializer, INotifyPropertyChanged
	{
		const string modelCategory = "Model";

		ModelList models = new ModelList();
		ModelList executors = new ModelList();
		bool mReschedule = true;

		/// <summary>
		/// IsSaving indicates that the model is being serialized to persistent store. Derived classes can adjust Properies
		/// to serialize properly
		/// </summary>
		[XmlIgnore]
		public static bool IsSaving { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public Model()
		{
			Id = ModelMgr.DefaultId;
		}

		/// <summary>
		/// Construct with a name
		/// </summary>
		/// <param name="name"></param>
		public Model(string name) : this()
		{
			mName = name;
		}

		/// <summary>
		/// Add a child model to an existing model at the end
		/// </summary>
		/// <param name="child"></param>
		/// <param name="scenario"></param>
		/// <returns></returns>
		public Model AddChild(Model child, Scenario scenario)
		{
			child.Parent = this;
			Models.Add(child);
			if (scenario != null)
				child.Discover(scenario);
			return child;
		}

		/// <summary>
		/// Add a child to the front of the component's sub-models list
		/// </summary>
		/// <param name="child"></param>
		/// <param name="scenario"></param>
		/// <returns></returns>
		public Model AddChildAtFront(Model child, Scenario scenario)
		{
			var old = Models.ToArray();
			Models.Clear();
			child.Parent = this;
			Models.Add(child);
			foreach (var model in old)
				Models.Add(model);
			if ( scenario != null )
				child.Discover(scenario);
			return child;
		}

		/// <summary>
		/// When a model is published, do not publish base class variables
		/// </summary>
		[Category(modelCategory), XmlIgnore]
		public bool CleanPublish { get; set; }

		/// <summary>
		/// Discovery phase. Must be called from inherited implementations
		/// </summary>
		/// <param name="scenario"></param>
		public virtual void Discover(Scenario scenario)
		{
			if (Name == null) Name = GetType().Name;

			ModelMgr.TagModel(this);

			if ( !InhibitPublish ) Blackboard.Publish(this);

			foreach (var model in models)
			{
				model.Parent = this;
				model.Discover(scenario);
			}
		}

		/// <summary>
		/// Execution is enabled.
		/// </summary>
		[Category("Scheduling"),XmlIgnore]
		public bool Enabled
		{
			get { return mEnabled; }
			set
			{
				mEnabled = value;
				if ( Parent == null )
					Scenario.Reschedule();
				else
					ParentModel.mReschedule = true;
			}
		} bool mEnabled = true;

		/// <summary>
		/// Execute every major frame. Must be called from base implementations
		/// </summary>
		public virtual void Execute()
		{
			if (mReschedule)
			{
				executors.Clear();
				foreach (var model in models)
					if (model.mEnabled)
						executors.Add(model);
				mReschedule = false;
			}
			foreach (var model in executors)
				model.Execute();
		}

		/// <summary>
		/// The filename that defined this model's characteristics
		/// </summary>
		[XmlIgnore]
		protected string FileName { get; set; }

		/// <summary>
		/// The full path of the characteristic file
		/// </summary>
		[XmlIgnore,Browsable(false)]
		public string FileNamePath
		{
			get
			{
				return System.IO.Path.Combine(Scenario.Directory, FileName);
			}
		}

		/// <summary>
		/// Allow a model to prepare itself for saving to persistent store
		/// </summary>
		protected virtual void FinishedSaving() { }

		/// <summary>
		/// Broker the sub-models as services. Might need to define IServiceProvider.GetService for non-Models
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual object GetService(Type type)
		{
			foreach (var model in Models)
				if (model.GetType() == type)
					return model;
			return null;
		}

		/// <summary>
		/// Entity relationship test used for embedded models
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual Model HasA(Type type)
		{
			if (type == typeof(Model))
				return this;
			else if (type.BaseType == typeof(Attribute))
			{
				object[] attrs = GetType().GetCustomAttributes(type, false);
				if (attrs.Length > 0)
					return this;
			}
			Type iface = this.GetType().GetInterface(type.ToString());
			return iface != null ? this : null;
		}

		/// <summary>
		/// Index within the root component's componentById table
		/// </summary>
		[Category(modelCategory), ReadOnly(true), XmlIgnore]
		[Description("Index within the root component's componentById table.")]
		public int Id { get; set; }

		/// <summary>
		/// Don't publish this model to the Blackboard
		/// </summary>
		[Category(modelCategory), XmlIgnore]
		public bool InhibitPublish { get; set; }

		/// <summary>
		/// Reset internal state. Must be called from base implementations
		/// </summary>
		public virtual void Initialize()
		{
			foreach (var model in models)
				model.Initialize();
		}

		/// <summary>
		/// Has the model changed since creation
		/// </summary>
		[Category(modelCategory),XmlIgnore]
		public bool IsDirty
		{
			get { return mIsDirty; }
			set
			{
				mIsDirty = value;
				Scenario.RaiseIsDirty();
			}
		} bool mIsDirty;

		/// <summary>
		/// List of Sub-models
		/// </summary>
		[Category(modelCategory),XmlElement("Model",typeof(Model))]
		public ModelList Models { get { return models; } }

		string mName;
		/// <summary>
		/// The display name of the model
		/// </summary>
		[Category(modelCategory),XmlAttribute("name"),DefaultValue("")]
		public virtual string Name
		{
			get
			{
				if (IsSaving && mName == GetType().Name) return string.Empty;
				return mName;
			}
			set { mName = value; }
		}

		Model mParent;
		/// <summary>
		/// The IPublishable parent
		/// </summary>
		[Browsable(false), XmlIgnore]
		public IPublishable Parent { get { return mParent; } set { mParent = value as Model; } }

		/// <summary>
		/// The parent model
		/// </summary>
		[Category(modelCategory),XmlIgnore]
		public Model ParentModel { get { return mParent; } set { mParent = value; } }

		/// <summary>
		/// The full path name of the model, including all antecedants
		/// </summary>
		[Category(modelCategory),XmlIgnore]
		public string Path { get; set; }

		/// <summary>
		/// Allow a model to prepare itself for saving to persistent store
		/// </summary>
		protected virtual void PrepareToSave() { }

		/// <summary>
		/// Save the model to a file
		/// </summary>
		public void Save()
		{
			IsSaving = true;
			FileUtilities.WriteToXml(this, FileNamePath, true);
			IsSaving = false;
		}

		/// <summary>
		/// String Representation
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return Name + ':' + GetType().Name;
		}

		/// <summary>
		/// The fully qualified type name
		/// </summary>
		[Category(modelCategory)]
		public string Type { get { return GetType().FullName; } }

		/// <summary>
		/// Unload a model. Must be called from base implementations
		/// </summary>
		public virtual void Unload()
		{
			foreach (var model in Models)
				model.Unload();
		}

		/// <summary>
		/// Convenience method: write text to the Output view
		/// </summary>
		/// <param name="text"></param>
		/// <param name="args"></param>
		protected void Write(string text, params object[] args)
		{
			Log.WriteLine(text,args);
		}

		#region IPropertyCategoryInitializer Members

		/// <summary>
		/// Gets a collection of category names to set expanded or collapsed on the Property view
		/// </summary>
		/// <returns></returns>
		public virtual Dictionary<string, bool> GetCategoryStates()
		{
			var dict = new Dictionary<string, bool>();
			dict.Add(modelCategory, false);
			dict.Add("Scheduling", false);
			return dict;
		}

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// A dynamic property has changed. Used by the Property view
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Whenever a dynamic property changes its value and its important to show that on the Property view, 
		/// call this method from the property's setter.
		/// </summary>
		/// <param name="propertyName"></param>
		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region IXmlSerializable Members

		/// <summary>
		/// Get the schema for a specific Model sub type
		/// </summary>
		/// <returns></returns>
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Parse the inner XML
		/// </summary>
		/// <param name="reader"></param>
		public void ReadXml(System.Xml.XmlReader reader)
		{
			//Model model = null;

			reader.MoveToElement();
			string innerXml = reader.ReadInnerXml();
			// Need to parse innerXml for typeName
			//var typeName = 
			//if (typeName.Length > 0)
			//{
			//	var type = Scenario.FindType(typeName);
			//	if (innerXml.Length > 0)
			//	{
			//		string xml = "<?xml version='1.0' encoding='utf-8'?>" + Environment.NewLine +
			//			"<" + typeName + Environment.NewLine +
			//			//	"xmlns:xsd='http://www.w3.org/2001/XMLSchema' "+Environment.NewLine+
			//			"xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " + Environment.NewLine +
			//			">" + Environment.NewLine +
			//			innerXml + Environment.NewLine +
			//			"</" + typeName + ">";
			//		TextReader textReader = new StringReader(xml);
			//		var serializer = new XmlSerializer(type);
			//		model = serializer.Deserialize(textReader) as Model;
			//	}
			//	else
			//		model = Activator.CreateInstance(type) as Model;

			//	if (model != null)
			//	{
			//		model.Name = Name;
			//	}
			//}
		}

		/// <summary>
		/// Format the model as XML in situ
		/// </summary>
		/// <param name="writer"></param>
		public void WriteXml(System.Xml.XmlWriter writer)
		{
		}

		#endregion
	}
}
