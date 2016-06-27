//----------------------------------------------------------------------------
// File    : ToolWindow.cs
// Date    : 17/09/2004
// Author  : Aju George
// Email   : aju_george_2002@yahoo.co.in ; george.aju@gmail.com
//----------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Silver.UI;

namespace ToolBoxTest
{
	public class ToolWindow : Form
	{
		#region Attributes
		private ToolBox _toolBox;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		#endregion //Attributes

		#region Constructor
		public ToolWindow()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}
		#endregion //Constructor

		#region Properties
		public ToolBox ToolBox
		{
			get{return _toolBox;}
		}
		#endregion //Properties

		#region Overrides
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion //Overrides

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._toolBox = new ToolBox();
			this.SuspendLayout();
			// 
			// _toolBox
			// 
			this._toolBox.AllowDrop = true;
			this._toolBox.BackColor = System.Drawing.SystemColors.Control;
			this._toolBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._toolBox.ItemHeight = 20;
			this._toolBox.ItemHoverColor = System.Drawing.Color.BurlyWood;
			this._toolBox.ItemNormalColor = System.Drawing.SystemColors.Control;
			this._toolBox.ItemSelectedColor = System.Drawing.Color.Linen;
			this._toolBox.ItemSpacing = 1;
			this._toolBox.Location = new System.Drawing.Point(0, 0);
			this._toolBox.Name = "_toolBox";
			this._toolBox.Size = new System.Drawing.Size(208, 405);
			this._toolBox.TabHeight = 18;
			this._toolBox.TabIndex = 2;

			// 
			// ToolBoxTest
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(208, 405);
			this.Controls.Add(this._toolBox);
			this.ShowInTaskbar = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ToolBoxTest";
			this.Text = "ToolBox";
			this.ResumeLayout(false);

		}
		#endregion
	}

}
//----------------------------------------------------------------------------
