using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;
using Aspire.Utilities.Extensions;

namespace Aspire.Studio.DocumentViews
{
	public partial class Monitor : StudioDocumentView
	{
		delegate void SetRowStyleDelegate(DataGridViewRow row, DataGridViewCellStyle style,string units);

		MonitorDoc myDoc;

		public Monitor() : base(Solution.ProjectItem.ItemType.Monitor)
		{
			InitializeComponent();
			//myDoc = new MonitorDoc();
			//BrowseProperties(myDoc);
		}

		private int AddRow(MonitorItem item, Blackboard.Item bbItem)
		{
			mValues[0] = item.Caption;
			if (bbItem != null)
			{
				if ( bbItem.Units != null )
					mValues[1] = bbItem.Units;
				else if ( bbItem.ParentItem != null && bbItem.ParentItem.IsArray )
					mValues[1] = bbItem.ParentItem.Units;
				for (int i = 0; i < myDoc.NumValues; i++) mValues[i + 2] = null;
				if (item.Length == 1)
					mValues[2] = bbItem.Value;
				else if ( bbItem != null )
				{
					var ap = bbItem.Value as IArrayProxy;
					for (int j = 0; j < item.Length; j++)
						mValues[2 + j] = ap[j];
				}
			}

			int row = dataGridView1.Rows.Add(mValues);
			if (bbItem == null)
				dataGridView1.Rows[row].Cells[0].Style.Font = new Font(dataGridView1.DefaultCellStyle.Font,FontStyle.Italic);
			return row;
		}

		private void AdjustColumns()
		{
			dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
			int numValueCols = dataGridView1.Columns.Count - 2;
			if (numValueCols < myDoc.NumValues)
			{
				for (int icol = 1; icol < myDoc.NumValues; icol++)
				{
					var col = new DataGridViewColumn(ValueColumn.CellTemplate) { Name = "Value" + icol };
					col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
					dataGridView1.Columns.Add(col);
				}
			}
			else if (numValueCols > myDoc.NumValues)
				for (int n = numValueCols - myDoc.NumValues; n > 0; n--)
					dataGridView1.Columns.RemoveAt(dataGridView1.Columns.Count - 1);

			dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			mValues = new object[myDoc.NumValues + 2];
		} object[] mValues;

		public bool AlternatingRowColoring
		{
			set
			{
				dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = value ? Color.PaleTurquoise : Color.White;
			}
		}

		protected override void NewItem(Blackboard.Item bbItem, DragEventArgs e)
		{
			if ( bbItem.Items.Count > 0 ) return;

			if (Document == null)
			{
				if (myDoc == null) myDoc = new MonitorDoc();
				MyDocument(myDoc);
				myDoc.DefaultCellStyle = dataGridView1.DefaultCellStyle;
			}

			var item = new MonitorItem();

			myDoc.AddItem(item, bbItem);

			if (bbItem.IsArrayProxy)
			{
				var ap = bbItem.Value as IArrayProxy;
				item.Length = ap.Length;
				item.Format = "G8";
				myDoc.NumValues = Math.Max(myDoc.NumValues, ap.Length);
				AdjustColumns();
			}
			else
			{
				switch (Type.GetTypeCode(bbItem.Type))
				{
					case TypeCode.Byte:
					case TypeCode.SByte:
					case TypeCode.Int16:
					case TypeCode.UInt16:
					case TypeCode.Int32:
					case TypeCode.UInt32:
					case TypeCode.Int64:
					case TypeCode.UInt64:
						if ( bbItem.Type.IsPrimitive )
							item.Format = "F0";
						break;
					case TypeCode.Single:
					case TypeCode.Double:
						item.Format = "G8";
						break;
				}
				AdjustColumns();
			}

			int i = AddRow(item, bbItem);
			item.Row = dataGridView1.Rows[i];
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as MonitorDoc;
			myDoc.DefaultCellStyle = dataGridView1.DefaultCellStyle;
			AlternatingRowColoring = myDoc.AlternatingRowColoring;
			AdjustColumns();
			foreach (var item in mItems)
			{
				var bbItem = item.BlackboardItem;
				int i = AddRow(item as MonitorItem, bbItem);
				(item as MonitorItem).Row = dataGridView1.Rows[i];
			}
		}

		public static void Register(bool unregister = false)
		{
			if ( unregister )
				DocumentMgr.UndefineDocView(typeof(Monitor), typeof(MonitorDoc), typeof(Monitor.MonitorItem));
			else
				DocumentMgr.DefineDocView(typeof(Monitor), typeof(MonitorDoc), typeof(Monitor.MonitorItem));
		}

		void SortItemsByRow()
		{
			var items = new ObservableCollection<StudioDocument.Item>();
			foreach (DataGridViewRow row in dataGridView1.Rows)
				items.Add(row.Tag as StudioDocument.Item);
			lock(mItems)
				mItems = items;
			Document.Items = items;
		}

		public override void UpdateDisplay(Clock clock)
		{
			lock (mItems)
				foreach (var item in mItems)
					item.Update();
		}

		void UpdateDisplay()
		{
			lock (mItems)
				foreach (var item in mItems)
					item.Update();
		}

		delegate void UpdateDisplayDelegate();

		internal void InvokeUpdateDisplay()
		{
			dataGridView1.Invoke(new UpdateDisplayDelegate(UpdateDisplay), null);
		}


		private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			var item = dataGridView1.Rows[e.RowIndex].Tag as MonitorItem;
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(item);
		}

		private void dataGridView1_SelectionChanged(object sender, EventArgs e)
		{
			if (dataGridView1.SelectedRows.Count == 0) return;
			// deal with multiline selection someday
		}

		private void dataGridView1_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			var array = mItems.ToArray();
			int maxIndex = e.RowIndex + e.RowCount;
			for (int row = e.RowIndex; row < maxIndex; row++)
			{
				if (row >= array.Length) continue;
				var item = array[row]; // Might need to insert items @ index
				if (item != null)
				{
					mItems.Remove(item);
					if ((item as MonitorItem).Length > 1)
					{
						myDoc.NumValues = 1;
						foreach (MonitorItem mItem in mItems)
							myDoc.NumValues = Math.Max(myDoc.NumValues, mItem.Length);
						AdjustColumns();
					}
					mDocument.IsDirty = true;
				}
			}
		}

		Rectangle dragBox;
		int moveRowIndex;

		private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				// If the mouse moves outside the rectangle, start the drag.
				if (dragBox != Rectangle.Empty && !dragBox.Contains(e.X, e.Y) && moveRowIndex < dataGridView1.Rows.Count)
				{
					// Proceed with the drag and drop, passing in the list item.                    
					DragDropEffects dropEffect = dataGridView1.DoDragDrop(
						  dataGridView1.Rows[moveRowIndex],
						  DragDropEffects.Move);
				}
			}
		}

		private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
		{
			// Get the index of the item the mouse is below.
			var ht = dataGridView1.HitTest(e.X, e.Y);
			moveRowIndex = dataGridView1.HitTest(e.X, e.Y).RowIndex;

			if (moveRowIndex != -1)
			{
				// Remember the point where the mouse down occurred. 
				// The DragSize indicates the size that the mouse can move 
				// before a drag event should be started.                
				Size dragSize = SystemInformation.DragSize;

				// Create the drag box using the DragSize, centered on the current mouse
				dragBox = new Rectangle(
						  new Point(
							e.X - (dragSize.Width / 2),
							e.Y - (dragSize.Height / 2)),
					  dragSize);
			}
			else
				// Reset the drag box if the mouse is not over an item in the ListBox.
				dragBox = Rectangle.Empty;
		}

		private void dataGridView1_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.Move;
		}

		private void dataGridView1_DragDrop(object sender, DragEventArgs e)
		{
			var obj = e.Data.GetData(typeof(Blackboard.Item));
			if (obj != null)
				base.OnDragDrop(sender, e);
			else
			{
				// Convert the mouse location from screen to client coordinates
				Point clientPoint = dataGridView1.PointToClient(new Point(e.X, e.Y));

				// Get the row index of the item under the mouse. 
				int rowIndexOfItemUnderMouseToDrop = dataGridView1.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

				// If the drag operation was a move then remove and insert the row.
				if (e.Effect == DragDropEffects.Move)
				{
					DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
					dataGridView1.Rows.RemoveAt(moveRowIndex);
					dataGridView1.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
					SortItemsByRow();
					Document.IsDirty = true;
				}
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class MonitorItem : StudioDocument.Item
		{
			DataGridViewRow mRow;

			[XmlIgnore, Browsable(false)]
			public override Blackboard.Item BlackboardItem
			{
				get { return base.BlackboardItem; }
				set
				{
					base.BlackboardItem = value;

					if (value == null)
						mRow = null;
					else
					{
						if (mRefreshOnChange)
							BlackboardItem.ValueChanged += BlackboardItem_ValueChanged;
						else
							BlackboardItem.ValueChanged -= BlackboardItem_ValueChanged;
					}
          if (mRow != null && mRow.Index >= 0 && mRow.Cells[0].Style.Font != null && mRow.Cells[0].Style.Font.Italic)
					{
						if (Format != null && BlackboardItem.Type.IsEnum)
						{
							Format = null;
							Notify();
						}
						if (Document.View.InvokeRequired)
							Document.View.Invoke(new SetRowStyleDelegate(SetRowStyle),
								new object[] { mRow, (Document as MonitorDoc).DefaultCellStyle, bbItem.Units });
						else
							SetRowStyle(mRow, (Document as MonitorDoc).DefaultCellStyle, bbItem.Units);
					}
				}
			}

			void BlackboardItem_ValueChanged(object sender, EventArgs e)
			{
				if ( Document.View != null )
					(Document.View as Monitor).InvokeUpdateDisplay();
			}

			void SetRowStyle(DataGridViewRow row, DataGridViewCellStyle style, string units)
			{
				row.Cells[0].Style = style;
				row.Cells[1].Value = units;
			}

			[XmlAttribute("caption")]
			public override string Caption
			{
				get { return base.Caption; }
				set
				{
					base.Caption = value;
					if (mRow != null)
						mRow.Cells[0].Value = Caption;
				}
			}

			[XmlAttribute("format")]
			public string Format
			{
				get { return mFormat; }
				set
				{
					mFormat = value;
					formatSpec = "{0:" + mFormat + '}';
					if (mRow != null)
						Update();
					Notify();
				}
			} string mFormat, formatSpec;

			[XmlAttribute("refreshOnChange"),DefaultValue(false)]
			public bool RefreshOnChange
			{
				get { return mRefreshOnChange; }
				set
				{
					mRefreshOnChange = value;
					if (BlackboardItem != null)
					{
						if ( mRefreshOnChange )
							BlackboardItem.ValueChanged += BlackboardItem_ValueChanged;
						else
							BlackboardItem.ValueChanged -= BlackboardItem_ValueChanged;
					}
					Notify();
				}
			} bool mRefreshOnChange;

			[Browsable(false)]
			internal DataGridViewRow Row
			{
				get { return mRow; }
				set { mRow = value; mRow.Tag = this; }
			}

			public override void Update()
			{
				if (bbItem == null) return;

				string str;
				if (Length == 1)
				{
					if (Format != null)
						str = string.Format(formatSpec, bbItem.Value);
					else
					{
						var val = bbItem.Value;
						if (val != null)
							str = val.ToString();
						else
							str = string.Empty;
					}
					mRow.Cells[2].Value = str;
				}
				else
				{
					var ap = bbItem.Value as IArrayProxy;
					for (int j = 0; j < Length; j++)
					{
						if (Format != null)
							str = string.Format(formatSpec, ap[j]);
						else
							str = ap[j].ToString();
						mRow.Cells[2 + j].Value = str;
					}

				}

			}
		}
	}

	public class MonitorDoc : StudioDocument
	{
		const string category = "Monitor";

		public MonitorDoc() : base(ItemType.Monitor)
		{
			NumValues = 1;
		}

		[Category(category),DefaultValue(false)]
		public bool AlternatingRowColoring
		{
			get { return mAlternatingRowColoring; }
			set
			{
				mAlternatingRowColoring = value;
				if (View != null) (View as Monitor).AlternatingRowColoring = value;
				IsDirty = true;
			}
		} bool mAlternatingRowColoring;

		[Category(category), DefaultValue("")]
		[EditorAttribute(typeof(BlackboardPathSelection),typeof(UITypeEditor))]
		public string BlackboardRefresher
		{
			get { return mBlackboardRefresher; }
			set
			{
				mBlackboardRefresher = value;
				IsDirty = true;
			}
		} string mBlackboardRefresher;

		[XmlIgnore, Browsable(false)]
		internal DataGridViewCellStyle DefaultCellStyle { get; set; }

		public override StudioDocumentView NewView(string name)
		{
			var view = new Monitor() { Name = name };
			return view;
		}

		[XmlAttribute("values"),DefaultValue(1)]
		public int NumValues { get; set; }
	}
}
