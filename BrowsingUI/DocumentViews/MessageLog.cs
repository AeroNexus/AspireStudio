using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Studio;
using Aspire.Studio.DocumentViews;
using Aspire.Utilities.Extensions;

namespace Aspire.BrowsingUI.DocumentViews
{
	public partial class MessageLog : StudioDocumentView
	{
		MessageLogDoc myDoc;

		public MessageLog()
			: base(Solution.ProjectItem.ItemType.Monitor)
		{
			InitializeComponent();
		}

		private int AddRow(MessageLogItem item, Blackboard.Item bbItem)
		{
			mValues[0] = item.Caption;
			if (bbItem != null)
			{
				mValues[1] = bbItem.Units;
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
				//for (int icol = 1; icol < myDoc.NumValues; icol++)
				//{
				//	var col = new DataGridViewColumn(ValueColumn.CellTemplate) { Name = "Value" + icol };
				//	col.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
				//	dataGridView1.Columns.Add(col);
				//}
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
				MyDocument(myDoc);
				myDoc.DefaultCellStyle = dataGridView1.DefaultCellStyle;
			}

			var item = new MessageLogItem();

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
			myDoc = mDocument as MessageLogDoc;
			myDoc.DefaultCellStyle = dataGridView1.DefaultCellStyle;
			AlternatingRowColoring = myDoc.AlternatingRowColoring;
			AdjustColumns();
			foreach (var item in mItems)
			{
				var bbItem = item.BlackboardItem;
				int i = AddRow(item as MessageLogItem, bbItem);
				(item as MessageLogItem).Row = dataGridView1.Rows[i];
			}
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(MessageLog), typeof(MessageLogDoc), typeof(MessageLog.MessageLogItem));
			else
				DocumentMgr.DefineDocView(typeof(MessageLog), typeof(MessageLogDoc), typeof(MessageLog.MessageLogItem));
		}

		public override void UpdateDisplay(Clock clock)
		{
			lock(mItems)
				foreach (var item in mItems)
					item.Update();
		}

		private void dataGridView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Aspire.Studio.DockedViews.PropertiesView.The.Browse(myDoc);
		}

		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			var item = dataGridView1.Rows[e.RowIndex].Tag as MessageLogItem;
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
				var item = array[row]; // Might need to insert items @ index
				if (item != null)
				{
					mItems.Remove(item);
					if ((item as MessageLogItem).Length > 1)
					{
						myDoc.NumValues = 1;
						foreach (MessageLogItem mItem in mItems)
							myDoc.NumValues = Math.Max(myDoc.NumValues, mItem.Length);
						AdjustColumns();
					}
					mDocument.IsDirty = true;
				}
			}
		}
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class MessageLogItem : StudioDocument.Item
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
					if (mRow != null && mRow.Index >= 0 && mRow.Cells[0].Style.Font.Italic)
					{
						if (Format != null && BlackboardItem.Type.IsEnum)
						{
							Format = null;
							Notify();
						}
						mRow.Cells[0].Style = (Document as MessageLogDoc).DefaultCellStyle;
						mRow.Cells[1].Value = bbItem.Units;
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
					if ( mRow != null )
						Update();
					Notify();
				}
			} string mFormat, formatSpec;

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
						str = bbItem.Value.ToString();
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

		private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{

		}

	}

	public class MessageLogDoc : StudioDocument
	{
		const string category = "MessageLog";

		public MessageLogDoc()
			: base(ItemType.Monitor)
		{
			NumValues = 1;
		}

		[Category(category)]
		public bool AlternatingRowColoring
		{
			get { return mAlternatingRowColoring; }
			set
			{
				mAlternatingRowColoring = value;
				if (View != null) (View as MessageLog).AlternatingRowColoring = value;
				IsDirty = true;
			}
		} bool mAlternatingRowColoring;

		[XmlIgnore, Browsable(false)]
		internal DataGridViewCellStyle DefaultCellStyle { get; set; }

		public override StudioDocumentView NewView(string name)
		{
			var view = new MessageLog() { Name = name };
			return view;
		}

		[XmlAttribute("values"),DefaultValue(1)]
		public int NumValues { get; set; }
	}
}
