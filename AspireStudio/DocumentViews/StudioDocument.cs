using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Text;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
//using System.Design;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DocumentViews
{
	public abstract class StudioDocument : Solution.ProjectItem, INotifyPropertyChanged
	{
		const string category = "StudioDocument";
		int pendingItems;

		public enum CaptionRule { Field245, First, First2, First3, FullPath, Last2, Last3, Leaf };

		public StudioDocument()
		{
			GeneratedCaptionRule = (CaptionRule)Enum.Parse(typeof(CaptionRule), StudioSettings.Default.BlackboardGeneratedCaptionRule);
		}

		protected StudioDocument(ItemType type) : this()
		{
			Type = type;
		}

		public static void Add(StudioDocument document)
		{
			if (ActiveProject != null)
				ActiveProject.AddItem(document);
		}

		public Item AddItem(Item item, Blackboard.Item bbItem)
		{
			item.Document = this;
			item.Caption = BuildCaption(bbItem);
			item.BlackboardItem = bbItem;
			Items.Add(item);
			IsDirty = true;
			return item;
		}

		[XmlIgnore]
		public static Solution.Project ActiveProject { get; set; }

		public static void Bind(StudioDocumentView docView)
		{
			if (ActiveProject == null) return;
			var doc = ActiveProject.StudioDocument(docView.Name,docView.ItemType);
			if (doc != null)
			{
				docView.Document = doc;
				doc.View = docView;
				if ( doc.pendingItems > 0 )
					Blackboard.ItemPublished += doc.Blackboard_ItemPublished;
			}
		}

		void Blackboard_ItemPublished(object sender, EventArgs e)
		{
			var bbItem = sender as Blackboard.Item;
			if (bbItem == null) return;
			foreach ( var pi in mItems )
				if (bbItem.Path == pi.BlackboardPath)
				{
					pi.BlackboardItem = bbItem;
					if ( --pendingItems <= 0 )
						Blackboard.ItemPublished -= Blackboard_ItemPublished;
				}
		}

		public string BuildCaption(Blackboard.Item bbItem)
		{
			var captionRule = TypedRule(bbItem.Owner.GetType());
			string caption = null;
			switch (captionRule)
			{
				case CaptionRule.Leaf: caption = bbItem.LeafName; break;
				case CaptionRule.FullPath: caption = bbItem.Path; break;
			}

			if (caption == null)
			{
				string[] tokens = bbItem.Path.Split(Blackboard.PathSeparator);
				int m = tokens.Length - 1;

				switch (captionRule)
				{
					case CaptionRule.First: caption = tokens[0]; break;
					case CaptionRule.First2: caption = BuildCaption(tokens, 0, 1); break;
					case CaptionRule.First3: caption = BuildCaption(tokens, 0, 1, 2); break;
					case CaptionRule.Last2: caption = BuildCaption(tokens, m - 1, m); break;
					case CaptionRule.Last3: caption = BuildCaption(tokens, m - 2, m - 1, m); break;
					case CaptionRule.Field245: caption = BuildCaption(tokens, 1, 3, 4); break;
					default: caption = bbItem.Path; break;
				}
			}

			int suffix=0;
			bool found;
			string root = caption;
			do
			{
				found = false;
				foreach (var eItem in mItems)
					if (caption == eItem.Caption)
					{
						found = true;
						caption = root + ++suffix;
						break;
					}
			} while (found);


			return caption;
		}

		private static string BuildCaption(string[] tokens, params int[] indexes)
		{
			var sb = new StringBuilder();

			sb.Append(tokens[indexes[0]]);

			for (int i = 1; i < indexes.Length; i++)
				if (indexes[i] < tokens.Length)
					sb.Append('.' + tokens[indexes[i]]);

			return sb.ToString();
		}

		static List<Rule> rules;
		static int monitorRulesLength;
		static void BuildCaptionRules()
		{
			if ( rules == null ) rules = new List<Rule>();
			var monitorRules = StudioSettings.Default.MonitorCaptionRules;
			monitorRulesLength = monitorRules.Length;
			if ( monitorRules == null || monitorRules.Length == 0 ) return;
			var lines = monitorRules.Split('\n');
			foreach (var line in lines)
			{
				var tokens = line.Split(',');
				if ( tokens.Length != 2 ) continue;
				rules.Add(new Rule(tokens[0],tokens[1]));
			}
		}

		class Rule
		{
			internal Rule(string typeName, string rule)
			{
				Type = typeName;
				CaptionRule = (StudioDocument.CaptionRule)Enum.Parse(typeof(StudioDocument.CaptionRule),rule);
			}
			internal StudioDocument.CaptionRule CaptionRule { get; set; }
			internal string Type { get; set; }
		}

		StudioDocument.CaptionRule TypedRule(Type type)
		{
			//if (type.IsPrimitive || type.IsEnum) return GeneratedCaptionRule;
			if (StudioSettings.Default.MonitorCaptionRules.Length != monitorRulesLength) BuildCaptionRules();
			var typeName = type.Name;
			foreach (var rule in rules)
				if (rule.Type == typeName) return rule.CaptionRule;
			return GeneratedCaptionRule;
		}

		public StudioDocumentView CreateView()
		{
			return null;
		}

		public void Display()
		{
			if (DocumentMgr.FindDocumentView(Name) == null)
			{
				View = NewView(Name);
				View.Document = this;
				DocumentMgr.AddDocumentView(View);
				DocumentMgr.Add(View);
			}
			else
				Log.WriteLine("Can't display {0}; its already displayed", Name);
		}

		[Category(category), DefaultValue(true)]
		public bool Enabled { get { return mEnabled; } set { mEnabled = value; Notify(); } } bool mEnabled = true;

		public static StudioDocument Find(string name)
		{
			if (ActiveProject != null)
			{
				foreach (var item in ActiveProject.Items)
					if (item is StudioDocument && (item as StudioDocument).Name == name)
						return item as StudioDocument;
			}
			return null;
		}

		[Category(category),DefaultValue(CaptionRule.Leaf)]
		public CaptionRule GeneratedCaptionRule { get { return mGeneratedCaptionRule; } set { mGeneratedCaptionRule = value; Notify(); } } CaptionRule mGeneratedCaptionRule = CaptionRule.Leaf;

		[Category(category),
		XmlElement("Item", typeof(Item)),
		Editor(typeof(ItemsEditor),typeof(UITypeEditor))]
		public ObservableCollection<Item> Items
		{
			get { return mItems; }
			set
			{
				mItems = value;
				mItems.CollectionChanged += mItems_CollectionChanged;
			}
		} ObservableCollection<Item> mItems;

		void mItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
			{
				IsDirty = true;
				foreach (INotifyPropertyChanged item in e.NewItems)
					item.PropertyChanged += (o, ea) => { IsDirty = true; };
			}
		}

		[XmlAttribute("name"), Browsable(false)]
		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				if (View != null) View.Name = value;
				Notify();
			}
		}

		[Category(category),DefaultValue(1.0), XmlAttribute("period")]
		public double Period { get { return mPeriod; } set { mPeriod = value; Notify(); } } double mPeriod = 1.0;

		public virtual void RemoveItem(Item item)
		{
		}

		/// <summary>
		/// Subscribes all items to the Blackboard, accounting for pending items
		/// </summary>
		public void Subscribe()
		{
			foreach (var item in mItems)
			{
				item.Document = this;
				if (item.BlackboardItem == null)
				{
					var bbItem = Blackboard.Subscribe(item.BlackboardPath);
					if (bbItem != null)
					{
						if (item.Caption == null) item.Caption = bbItem.LeafName;
						if (bbItem.Type != null)
							item.BlackboardItem = bbItem;
						else
							pendingItems++;
					}
				}
				else
					item.BlackboardItem = item.BlackboardItem;
			}

		}

		public override void Unload()
		{
			foreach (var item in mItems)
				item.BlackboardItem = null;
		}

		[XmlIgnore, Browsable(false)]
		public StudioDocumentView View { get; set; }

		public abstract StudioDocumentView NewView(string name);

		public class Item : INotifyPropertyChanged
		{
			protected Blackboard.Item bbItem;
			[XmlIgnore]
			public StudioDocument Document;

			[XmlIgnore, Browsable(false)]
			public virtual Blackboard.Item BlackboardItem
			{
				get { return bbItem; }
				set
				{
					bbItem = value;
					if (value == null) return;

					if (bbItem.IsArrayProxy)
					{
						IsArrayProxy = true;
						Length = (bbItem.Value as IArrayProxy).Length;
					}
					else if (bbItem.IsArray)
					{
						IsArray = true;
						Length = (bbItem.Value as Array).Length;
					}
						//Todo: Revisit logic on Length=0/1 vis a vis StripCharts
					else
						Length = 1;

					if (BlackboardPath == null)
						BlackboardPath = bbItem.Path;
				}
			}

			[XmlAttribute("blackboardPath")]
			public string BlackboardPath { get; set; }

			[XmlAttribute("caption")]
			public virtual string Caption { get { return mCaption; } set { mCaption = value; Notify(); }  } string mCaption;

			//[XmlAttribute("isArrayProxy"), DefaultValue(false)]
			[XmlIgnore, Browsable(false)]
			public bool IsArrayProxy { get; set; }

			//[XmlAttribute("isArray"), DefaultValue(false)]
			[XmlIgnore, Browsable(false)]
			public bool IsArray { get; set; }

			[XmlAttribute("length"), DefaultValue(1)]
			public int Length { get { return mLength; } set { mLength = value; } } int mLength;

			public override string ToString()
			{
				return BlackboardPath;
			}

			public virtual void Update() { }

			#region INotifyPropertyChanged Members

			public event PropertyChangedEventHandler PropertyChanged;

			protected void Notify([CallerMemberName] string propertyName = "")
			{
				if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			#endregion
		}

		#region Items UI Editor

		public class ItemsEditor : CollectionEditor
		{
			public ItemsEditor(Type type)
				: base(type)
			{
			}

			protected override void DestroyInstance(object instance)
			{
				var item = instance as StudioDocument.Item;
				item.Document.RemoveItem(item);
				base.DestroyInstance(instance);
			}
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void Notify([CallerMemberName]string propertyName="")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
