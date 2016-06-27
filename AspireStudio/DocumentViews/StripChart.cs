using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Windows.Forms.DataVisualization.Charting;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Studio.DocumentViews
{
	public partial class StripChart : StudioDocumentView
	{
		StripChartDoc myDoc;
		SeriesChartType chartType;
		int lineWidth;

		public StripChart() : base(Solution.ProjectItem.ItemType.StripChart)
		{
			InitializeComponent();

			//someday, look in Docking to see how to do this
			//var menuItem = MainMenuStrip.Items.Add("TestA");
			//menuItem.Click += (s, e) =>
			//	{ Log.WriteLine("Strip Chart test"); };

			chart1.ContextMenuStrip = ContextMenuStrip;
			chart1.Legends[0].BackColor = Color.Transparent;
		}

		private Legend AddChartArea(string name, out ChartArea chartArea, out int chartAreaId)
		{
			if (myDoc == null) NewDoc();

			chartArea = name == null ? new ChartArea() : new ChartArea(name);
			chart1.ChartAreas.Add(chartArea);
			chartAreaId = chart1.ChartAreas.Count-1;
			chartArea.AxisY.IsStartedFromZero = false;
			var legend = new Legend();
			legend.Docking = Docking.Top;
			legend.DockedToChartArea = chartArea.Name;
			legend.BackColor = Color.Transparent;
			legend.Enabled = myDoc.ShowLegend;
			chart1.Legends.Add(legend);
			return legend;
		}

		internal delegate void ItemBind(StripChartItem item);

		private void BindItem(StripChartItem item)
		{
			ChartArea chartArea = null;
			Legend legend = null;
			foreach (var ca in chart1.ChartAreas)
				if (ca.Name == item.ChartArea)
				{
					chartArea = ca;
					foreach (var legn in chart1.Legends)
						if (legn.DockedToChartArea == ca.Name)
						{
							legend = legn;
							break;
						}
				}

			if (legend == null)
			{
				int id;
				legend = AddChartArea(item.ChartArea, out chartArea, out id);
				myDoc.SetChartProperties(chart1, id);
			}

			if (item.BlackboardItem == null)
			{
				item.Pending = true;
				return;
			}

			item.ShowYAxisTitle(chartArea, myDoc.ShowYAxisTitle);

			for (int i = 0; i < item.Length; i++)
			{
				var series = item.Series[i];
				series.ChartType = chartType;
				series.BorderWidth = lineWidth;

				series.Legend = legend.Name;
				chart1.Series.Add(series);
			}

			if (Manager != null && Manager.Clock != null)
				item.Update(Manager.Clock.ElapsedSeconds);
		}

		private void BuildContextMenu()
		{
			ContextMenuStrip.Items.Clear();

			var menuItem = ContextMenuStrip.Items.Add("Add chart area");
			menuItem.Click += AddChartArea_Clicked;

			switch ( contextHit.ChartElementType )
			{
				case ChartElementType.Axis:
				case ChartElementType.AxisLabels:
					menuItem = ContextMenuStrip.Items.Add("Show title");
					menuItem.Click += HideShowAxisTitle_Clicked;
					break;
				case ChartElementType.AxisTitle:
					menuItem = ContextMenuStrip.Items.Add("Hide title");
					menuItem.Click += HideShowAxisTitle_Clicked;
					break;
				case ChartElementType.DataPoint:
					menuItem = ContextMenuStrip.Items.Add(
						string.Format("Hide {0}", contextHit.Series.Name));
					menuItem.Click += HideItem_Clicked;
					break;
				case ChartElementType.LegendArea:
				case ChartElementType.LegendHeader:
				case ChartElementType.LegendItem:
				case ChartElementType.LegendTitle:
					menuItem = ContextMenuStrip.Items.Add("Hide legend");
					menuItem.Click += HideShowLegend_Clicked;
					break;
				case ChartElementType.PlottingArea:
					menuItem = ContextMenuStrip.Items.Add("Show legend");
					menuItem.Click += HideShowLegend_Clicked;
					break;
				case ChartElementType.Annotation:
				case ChartElementType.AxisLabelImage:
				case ChartElementType.DataPointLabel:
				case ChartElementType.Gridlines:
				case ChartElementType.Nothing:
				case ChartElementType.TickMarks:
				case ChartElementType.Title:
					menuItem = ContextMenuStrip.Items.Add(
						string.Format("{0} clicked",contextHit.ChartElementType));
					menuItem.Enabled = false;
				break;
			}

		}

		private void InitChart()
		{
			myDoc.Chart = chart1;
			var series = chart1.Series[0];
			chartType = series.ChartType;
			lineWidth = series.BorderWidth;
			chart1.Series.Clear();
			chart1.Legends[0].Enabled = myDoc.ShowLegend;
			var ca = chart1.ChartAreas[0];
			ca.AxisX.LabelStyle.Format = "G2";
			ca.AxisY.IsStartedFromZero = false;

			if (myDoc.ChartAreas != null)
				foreach (var docChartArea in myDoc.ChartAreas)
					myDoc.SetChartProperties(chart1, docChartArea);
		}

		private void NewDoc()
		{
			myDoc = MyDocument(new StripChartDoc()) as StripChartDoc;
			myDoc.Period = Manager.Clock.StepSize;

			InitChart();
		}

		protected override void NewItem(Blackboard.Item bbItem, DragEventArgs e)
		{
			var clientPoint = chart1.PointToClient(new Point(e.X, e.Y));
			var hit = chart1.HitTest(clientPoint.X, clientPoint.Y);

			if (hit.ChartArea == null)
			{
				Log.WriteLine("StripChart.NewItem: Hit test missed");
				return;
			}

			if (myDoc == null) NewDoc();

			var newItem = new StripChartItem { ChartArea = hit.ChartArea.Name };

			myDoc.AddItem(newItem, bbItem);

			BindItem(newItem);
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as StripChartDoc;
			InitChart();
			foreach (StripChartItem item in mItems)
				BindItem(item);
			NeedUpdate();
		}

		public override void OnResetTime()
		{
			foreach (StripChartItem item in mItems)
			{
				item.Reset();
				item.Update(Manager.Clock.ElapsedSeconds);
			}
			foreach (var chartArea in chart1.ChartAreas)
			{
				chartArea.AxisX.LabelStyle.Format = "G2";
				chartArea.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Seconds;
				chartArea.AxisX.IntervalOffset = 0.5;
			}
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(StripChart), typeof(StripChartDoc), typeof(StripChart.StripChartItem));
			else
				DocumentMgr.DefineDocView(typeof(StripChart), typeof(StripChartDoc), typeof(StripChart.StripChartItem));
		}

		double prevTime;

		public override void UpdateDisplay(Clock clock)
		{
			if (myDoc == null) return;
			
			double time = clock.ElapsedSeconds;

			if (prevTime < 1 && time >= 1)
				chart1.ChartAreas[0].AxisX.LabelStyle.Format = "F1";
			else if (prevTime < 3.5 && time >= 3.5)
				chart1.ChartAreas[0].AxisX.LabelStyle.Format = "F0";
			prevTime = time;

			double trimTime = time - myDoc.TimeWindow;

			foreach (StripChartItem item in mItems)
				item.Update(time, trimTime);

			if (time > myDoc.TimeWindow)
			{
				double interval = (int)(myDoc.TimeWindow / 5);
				double offset = interval - (time - ((int)(time / interval)) * interval);
				//if (offset >= interval) offset = 0;

				foreach (var chartArea in chart1.ChartAreas)
				{
					chartArea.AxisX.Maximum = time;
					chartArea.AxisX.Minimum = time - myDoc.TimeWindow;
					chartArea.AxisX.IntervalOffset = offset;
					//Log.WriteLine("{0}", offset);
				}
			}

			foreach ( var ca in chart1.ChartAreas)
				ca.RecalculateAxesScale();


			//if (time > myDoc.TimeWindow)
			//{
			//	// remove all points from the source series older than 1.5 minutes.
			//	double removeBefore = time - myDoc.TimeWindow;
			//	//remove oldest values to maintain a constant number of data points
			//	while (series.Points[0].XValue < removeBefore)
			//		series.Points.RemoveAt(0);

			//	chart1.ChartAreas[0].AxisX.Minimum = series.Points[0].XValue;
			//	chart1.ChartAreas[0].AxisX.Maximum = time+1;
			//}
		}

		public class StripChartItem : StudioDocument.Item
		{
			//Comment out
			List<Series> mSeriesList = new List<Series>();
			PropertyList<Element> elements = new PropertyList<Element>(); //Move Series to element
			bool mCastAsInt;

			[XmlIgnore, Browsable(false)]
			public override Blackboard.Item BlackboardItem
			{
				get { return base.BlackboardItem; }
				set
				{
					if (base.BlackboardItem == value)
						mSeriesList.Clear();

					base.BlackboardItem = value;
					if (value == null) return;

					if (bbItem.IsEnum)
						mCastAsInt = true;

					if (bbItem.IsArrayProxy)
					{
						//Comment out
						var ap = bbItem.Value as IArrayProxy;
						for (int i = 0; i < ap.Length; i++)
						{
							string name = Caption + ap.Suffix(i);

							var series = new Series(name) { Tag = this };
							series.ChartArea = ChartArea;
							mSeriesList.Add(series);
						}

						if (elements.Count == 0)
							for (int i = 0; i < ap.Length; i++)
								elements.Add(new Element());
						int index = 0;
						foreach (var elem in elements)
						{
							elem.Item = this;
							elem.Index = index++;
						}
					}
					else
					{
						//Comment out
						string name = Caption;

						var series = new Series(name) { Tag = this };
						if (!mColor.IsEmpty)
							series.Color = mColor;
						series.ChartArea = ChartArea;
						mSeriesList.Add(series);

						if ( elements.Count == 0 )
							elements.Add(new Element());
						elements[0].Item = this;
					}
					if (Pending)
					{
						Pending = false;
						
						var stripChart = Document.View as StripChart;
						if (stripChart.InvokeRequired)
							stripChart.Invoke(new ItemBind(stripChart.BindItem), new object[] { this });
						else
							stripChart.BindItem(this);
					}
				}
			}

			[XmlAttribute("caption")]
			public override string Caption
			{
				get { return base.Caption; }
				set
				{
					base.Caption = value;
					if (Length == 1)
						mSeriesList[0].Name = value;
					Notify();
				}
			}

			[XmlAttribute("chart")]
			public string ChartArea { get { return mChartArea; } set { mChartArea = value; Notify(); } } string mChartArea;

			[XmlIgnore]
			public Color Color
			{
				get { return mColor; }
				set
				{
					mColor = value.Name=="Transparent" ? Color.Empty : value;
					if (Length == 1 && mSeriesList.Count > 0)
						mSeriesList[0].Color = mColor;
					Notify();
				}
			} Color mColor;

			[XmlAttribute("color"),DefaultValue("0:0:0:0"),Browsable(false)]
			public string xmlColor
			{
				get { return mColor.Formatted(); }
				set { mColor = Color.Parse(value); Notify(); }
			}

			public void DisableSeries(Series series)
			{
				series.Enabled = false;
				foreach (var ser in mSeriesList)
					if (ser.Enabled) return;
				Document.RemoveItem(this);
			}

			[XmlElement("Element", typeof(Element))]
			public PropertyList<Element> Elements { get { return elements; } set { elements = value; } }

			internal bool Pending { get; set; }

			public void Reset()
			{
				for (int i = 0; i < Length; i++)
					mSeriesList[i].Points.Clear();
			}

			//Comment out
			[XmlIgnore]
			internal List<Series> Series { get { return mSeriesList; } }

			internal void ShowYAxisTitle(ChartArea chartArea, bool enabled)
			{
				if (enabled)
				{
					if (chartArea.AxisY.Title.Length == 0)
						chartArea.AxisY.Title = Caption;
					if (BlackboardItem.Units != null && BlackboardItem.Units.Length > 0)
						chartArea.AxisY.Title += " [" + BlackboardItem.Units + ']';
				}
				else
					chartArea.AxisY.Title = string.Empty;
			}

			public void Trim(double trimTime)
			{
				foreach (var series in mSeriesList)
					series.Points.RemoveAt(0); ;
			}

			public void Update(double time, double trimTime=-1e-10)
			{
				if (mSeriesList.Count == 0) return;
				if (IsArrayProxy)
				{
					var ap = BlackboardItem.Value as IArrayProxy;
					int length = ap.Length;

					for (int i = 0; i < length; i++)
					{
						var series = mSeriesList[i];
						series.Points.AddXY(time, ap[i]);
						if (series.Points[0].XValue < trimTime)
							series.Points.RemoveAt(0);
					}
				}
				else if (IsArray)
				{
					try
					{
						var a = BlackboardItem.Value as Array;
						int length = a.Length;

						for (int i = 0; i < length; i++)
						{
							var series = mSeriesList[i];
							object o = a.GetValue(i);
							if ( o is double || o is float )
							{
								series.Points.AddXY(time, (double)o);
								if (series.Points[0].XValue < trimTime)
									series.Points.RemoveAt(0);
							}
						}
					}
					catch (Exception e)
					{
						Log.ReportException(e, "Updating strip chart item {0}", bbItem.Path);
					}
				}
				else
				{
					var series = mSeriesList[0];
					if ( mCastAsInt )
						series.Points.AddXY(time, (int)bbItem.Value);
					else
						series.Points.AddXY(time, bbItem.Value);
					if (series.Points[0].XValue < trimTime)
						series.Points.RemoveAt(0);
				}
			}
			/// <summary>
			/// Element is a sub-item of item. Initialize it with (w/o Index if len==1):
			/// var elem = new Element(){Item = this, Index = i}
			/// </summary>
			[TypeConverter(typeof(ExpandableObjectConverter))]
			public class Element : INamed
			{
				[XmlIgnore]
				public Color Color
				{
					get { return mColor; }
					set
					{
						mColor = value.Name == "Transparent" ? Color.Empty : value;
						if (Item != null)
						{
							Series.Color = mColor;
							Item.Notify();
						}
					}
				} Color mColor;

				[XmlAttribute("color"), DefaultValue("0:0:0:0"), Browsable(false)]
				public string xmlColor
				{
					get { return mColor.Formatted(); }
					set { mColor = Color.Parse(value); }
				}

				internal int Index
				{
					set
					{
						var ap = Item.BlackboardItem.Value as IArrayProxy;
						if (ap != null)
						{
							Name = ap.Suffix(value);
							NewSeries(Item.Caption + Name);
						}
					}
				}
				[XmlIgnore]
				public StripChartItem Item
				{
					get { return mItem; }
					set
					{
						mItem = value;
						Name = string.Empty;
						if (Item.Length > 1) return;
						NewSeries(Item.Caption);
					}
				} StripChartItem mItem;
				[XmlIgnore,Browsable(false)]
				public string Name { get; set; }
				private void NewSeries(string name)
				{
					// Uncomment 
					//Series = new Series(name);
					//Series.Tag = this;
					//Series.ChartArea = Item.ChartArea;
					//if (!mColor.IsEmpty)
					//	Series.Color = mColor;
				}
				internal Series Series { get; set; }
				public override string ToString()
				{
					return Name == string.Empty ? Item.Caption : Name;
				}
			}
		}

		void AddChartArea_Clicked(object sender, EventArgs e)
		{
			ChartArea chartArea;
			int id;
			AddChartArea(null, out chartArea, out id);
			myDoc.SetChartProperties(chart1, id);
		}

		void HideShowLegend_Clicked(object sender, EventArgs e)
		{
			myDoc.ShowLegend = !myDoc.ShowLegend;
		}

		void HideShowAxisTitle_Clicked(object sender, EventArgs e)
		{
			myDoc.ShowYAxisTitle = !myDoc.ShowYAxisTitle;
		}

		void HideItem_Clicked(object sender, EventArgs e)
		{
			var item = contextHit.Series.Tag as StripChartItem;
			item.DisableSeries(contextHit.Series);
		}

		private void chart1_DoubleClick(object sender, EventArgs e)
		{
			StudioDocumentView_DoubleClick(sender, e);
		}

		private void chart1_MouseDown(object sender, MouseEventArgs e)
		{
			var hit = chart1.HitTest(e.X, e.Y);
			//Log.WriteLine("{0} hit {1},{2}", Name, hit.ChartElementType, hit.ChartArea);
			switch (e.Button)
			{
				case MouseButtons.Right:
					contextHit = hit;
					break;
				case MouseButtons.Left:
					//Log.WriteLine("Hit {0}", hit.ChartElementType);
					switch (hit.ChartElementType)
					{
						case ChartElementType.LegendItem:
							var li = hit.Object as LegendItem;
							foreach ( var series in chart1.Series)
								if ( series.Name == li.Name )
									BrowseProperties(series.Tag);
							break;
					}
					break;
			}
		} HitTestResult contextHit;

		private void chart1_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == System.Windows.Forms.MouseButtons.Right)
				BuildContextMenu();
		}
	}

	public class StripChartDoc : StudioDocument
	{
		const string category = "StripChart";

		public StripChartDoc()
		{
			Type = ItemType.StripChart;
		}

		[Category(category), XmlIgnore,
		Description("The running chart; not persisted. Use ChartAreas. If you need to persist one of these properties, contact the developer")]
		public Chart Chart { get; set; }

		[Category(category),Description("Persisted chart area properties")]
		public ChartArea[] ChartAreas
		{
			get { return mChartAreas; }
			set
			{
				mChartAreas = value;
				foreach (var chart in mChartAreas)
					chart.PropertyChanged += chart_PropertyChanged;
			}
		} ChartArea[] mChartAreas;

		void chart_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			IsDirty = true;

			SetChartProperties(Chart, sender as ChartArea, e.PropertyName);
		}

		public void SetChartProperties(Chart chart, int chartAreaId)
		{
			if ( ChartAreas != null )
				foreach ( var chartArea in ChartAreas )
					if (chartArea.Id == chartAreaId)
					{
						SetChartProperties(chart, chartArea);
						return;
					}
		}

		public void SetChartProperties(Chart chart, ChartArea docChartArea, string propertyName=null)
		{
			if (docChartArea.Id >= Chart.ChartAreas.Count) return;
			var chartArea = Chart.ChartAreas[docChartArea.Id];

			if (docChartArea.Axes != null)
				foreach (var dAxis in docChartArea.Axes)
				{
					if (dAxis.Id >= chartArea.Axes.Length) return;
					var axis = chartArea.Axes[dAxis.Id];
					if (propertyName == "Axis.Scaling")
					{
						if (dAxis.Scaling == AxisScaling.Fixed)
						{
							dAxis.Maximum = axis.Maximum;
							dAxis.Minimum = axis.Minimum;
							axis.Enabled = AxisEnabled.True;
						}
						else
						{
							axis.Maximum = double.NaN;
							axis.Minimum = double.NaN;
							axis.Enabled = AxisEnabled.Auto;
						}
						continue;
					}
					axis.LabelStyle.Format = dAxis.LabelStyleFormat;
					if (dAxis.Scaling == AxisScaling.Fixed)
					{
						axis.Maximum = dAxis.Maximum;
						axis.Minimum = dAxis.Minimum;
						axis.Enabled = AxisEnabled.True;
					}
				}
		}

		public override StudioDocumentView NewView(string name)
		{
			var view = new StripChart { Name = name };
			return view;
		}

		public override void RemoveItem(Item item)
		{
			if (Chart != null)
			{
				var scItem = item as StripChart.StripChartItem;

				foreach (var series in scItem.Series)
					foreach ( var cSeries in Chart.Series)
						if (series == cSeries)
						{
							Chart.Series.Remove(cSeries);
							break;
						}

				bool found = false;
				foreach ( var series in Chart.Series )
					if ( series.ChartArea == scItem.ChartArea )
						found = true;
				if (!found)
					foreach ( var ca in Chart.ChartAreas)
						if (ca.Name == scItem.ChartArea)
						{
							Chart.ChartAreas.Remove(ca);
							break;
						}
			}
		}

		[Category(category), DefaultValue(10)]
		public double TimeWindow
		{
			get { return mTimeWindow; }
			set { mTimeWindow = value; Notify(); }
		} double mTimeWindow = 10;

		[Category(category)]
		public bool ShowLegend
		{
			get { return mShowLegend; }
			set
			{
				mShowLegend = value;
				if (Chart != null)
					foreach (var legend in Chart.Legends)
						legend.Enabled = value;
				Notify();
			}
		} bool mShowLegend = true;

		[Category(category)]
		public bool ShowYAxisTitle
		{
			get { return mShowYAxisTitle; }
			set
			{
				mShowYAxisTitle = value;
				if (Chart != null)
				{
					foreach (var chartArea in Chart.ChartAreas)
					{
						chartArea.Tag = false;
						foreach (StripChart.StripChartItem item in Items)
						{
							if (item.ChartArea == chartArea.Name && !(bool)chartArea.Tag)
							{
								item.ShowYAxisTitle(chartArea, value);
								chartArea.Tag = true;
							}
						}
					}
				}
				Notify();
			}
		} bool mShowYAxisTitle = true;

		public enum AxisScaling { Auto, Fixed };

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class Axis : INotifyPropertyChanged
		{
			[XmlAttribute("id"),Description("Which Chart.ChartAreas[i].Axes does this correspond to")]
			public int Id
			{
				get { return mId; }
				set { mId = value; ChangeProperty("Id"); }
			} int mId;

			[XmlAttribute("labelStyleFormat"), DefaultValue("G3"), Description("Format used for axis labels")]
			public string LabelStyleFormat
			{
				get { return mLabelStyleFormat; }
				set { mLabelStyleFormat = value; ChangeProperty("LabelStyleFormat"); }
			}
			string mLabelStyleFormat = "G3";

			[XmlAttribute("maximum"), DefaultValue(0), Description("Maximum value of the axis. Must be > minimum")]
			public double Maximum
			{
				get { return mMaximum; }
				set { mMaximum = value; ChangeProperty("Maximum"); }
			}
			double mMaximum;

			[XmlAttribute("minimum"), DefaultValue(0), Description("Minimum value of the axis. Must be < maximum")]
			public double Minimum
			{
				get { return mMinimum; }
				set { mMinimum = value; ChangeProperty("Minimum"); }
			}
			double mMinimum;

			[XmlAttribute("scaling"), DefaultValue(AxisScaling.Auto), Description("Fixed or Auto scaling")]
			public AxisScaling Scaling
			{
				get { return mScaling; }
				set { mScaling = value; ChangeProperty("Scaling"); }
			}
			AxisScaling mScaling;

			#region INotifyPropertyChanged Members

			int depth;
			void ChangeProperty(string propertyName)
			{
				if (++depth == 1 && PropertyChanged != null )
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				depth--;
			}

			public event PropertyChangedEventHandler PropertyChanged;

			#endregion

			public override string ToString()
			{
				return GetType().Name;
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class ChartArea : INotifyPropertyChanged
		{
			[Description("Persisted axis properties")]
			public Axis[] Axes
			{
				get { return mAxes; }
				set
				{
					mAxes = value;
					foreach (var axis in Axes )
						axis.PropertyChanged += axis_PropertyChanged;
					ChangeProperty("Axes");
				}
			} Axis[] mAxes;

			void axis_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				ChangeProperty("Axis."+e.PropertyName);
			}

			[XmlAttribute("id"),Description("Which Chart.ChartAreas does this correspond to")]
			public int Id
			{
				get { return mId; }
				set { mId = value; ChangeProperty("Id"); }
			} int mId;

			#region INotifyPropertyChanged Members

			void ChangeProperty(string propertyName)
			{
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}

			public event PropertyChangedEventHandler PropertyChanged;

			#endregion

			public override string ToString()
			{
				return GetType().Name;
			}
		}
	}
}
