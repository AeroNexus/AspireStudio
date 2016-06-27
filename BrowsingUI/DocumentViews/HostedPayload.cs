using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

using Aspire.Core.xTEDS;
using Aspire.Framework;
using Aspire.Studio;
using Aspire.Studio.DocumentViews;
using Aspire.Utilities;

namespace Aspire.BrowsingUI.DocumentViews
{
	public partial class HostedPayload : StudioDocumentView
	{
		HostedPayloadDoc myDoc;

		public HostedPayload()
			: base(Solution.ProjectItem.ItemType.Monitor)
		{
			InitializeComponent();
		}

		public override void AddingToDocManager()
		{
			if (Document == null)
			{
				myDoc = new HostedPayloadDoc();
				BrowseProperties(myDoc);
				MyDocument(myDoc);
			}
		}

		internal bool Changed
		{
			set
			{
				greenLbl.Visible = myDoc.ShowGreen;
				greenKeyTb.Visible = myDoc.ShowGreen;
				redLbl.Visible = myDoc.ShowRed;
				redKeyTb.Visible = myDoc.ShowRed;
				brownLbl.Visible = myDoc.ShowBrown;
				brownKeyTb.Visible = myDoc.ShowBrown;
			}
		}

		protected override void OnDocumentBind()
		{
			myDoc = mDocument as HostedPayloadDoc;
			Changed = true;
		}

		public static void Register(bool unregister = false)
		{
			if (unregister)
				DocumentMgr.UndefineDocView(typeof(HostedPayload), typeof(HostedPayloadDoc) );
			else
				DocumentMgr.DefineDocView(typeof(HostedPayload), typeof(HostedPayloadDoc));
		}

		void StartClient()
		{
			client = new Process();
			client.StartInfo.UseShellExecute = false;
			string dir = Scenario.Directory;
			client.StartInfo.FileName = Path.Combine(dir,"CcsdsClient.exe");
			var sb = new StringBuilder();
			sb.AppendFormat("-A={0} -G={1} -T={2}",
				myDoc.ApId, myDoc.GroundPortRx, myDoc.GroundPortTx);
			if (myDoc.RouterIp != null && myDoc.RouterIp.Length > 0)
				sb.Append(" -R=" + myDoc.RouterIp);
			if (myDoc.RouterPort != 0)
				sb.Append(" -P=" + myDoc.RouterPort);
			if (myDoc.GreenKey != null && myDoc.GreenKey.Length > 0)
				sb.Append(" -I=" + myDoc.GreenKey);
			if (myDoc.RedKey != null && myDoc.RedKey.Length > 0)
				sb.Append(" -J=" + myDoc.RedKey);
			if (myDoc.BrownKey != null && myDoc.BrownKey.Length > 0)
				sb.Append(" -K=" + myDoc.BrownKey);
			client.StartInfo.Arguments = sb.ToString();
			client.StartInfo.CreateNoWindow = !myDoc.Window;

			try
			{
				client.Exited += client_Exited;
				client.Disposed += client_Disposed;
				client.Start();
			}
			catch (Exception ex)
			{
				Log.ReportException(ex,"Trying to start {0}",client.StartInfo.FileName);
			}
		}

		void client_Disposed(object sender, EventArgs e)
		{
			client = null;
		}

		void client_Exited(object sender, EventArgs e)
		{
			client = null;
		}

		Process client;
		void StopClient()
		{
			try
			{
				client.Kill();
				client.WaitForExit();
				client.Close();
			}
			catch (InvalidOperationException) { }
			catch (Exception ex)
			{
				Log.ReportException(ex, "Trying to kill CcsdsClient");
			}
			client = null;
		}

		private void connectBtn_Click(object sender, EventArgs e)
		{
			if (client != null) StopClient();
			StartClient();
		}

		private void greenKeyTb_TextChanged(object sender, EventArgs e)
		{
			myDoc.GreenKey = (sender as TextBox).Text;
			myDoc.IsDirty = true;
		}

		private void redKeyTb_TextChanged(object sender, EventArgs e)
		{
			myDoc.RedKey = (sender as TextBox).Text;
			myDoc.IsDirty = true;
		}

		private void brownKeyTb_TextChanged(object sender, EventArgs e)
		{
			myDoc.BrownKey = (sender as TextBox).Text;
			myDoc.IsDirty = true;
		}
	}

	public class HostedPayloadDoc : StudioDocument
	{
		const string category = "HostedPayload";

		public HostedPayloadDoc()
			: base(ItemType.Monitor)
		{
		}

		[XmlAttribute("apId"), Description("The CCSDS ap id of the client (-A)")]
		public int ApId { get { return mApId; } set { mApId = value; IsDirty = true; } } int mApId;

		[XmlAttribute("groundPortRx"), Description("The client UDP port connected to the operator console  (-G)")]
		public int GroundPortRx { get { return mGroundPortRx; } set { mGroundPortRx = value; IsDirty = true; } } int mGroundPortRx;

		[XmlAttribute("groundPortTx"), Description("The operator console UDP port (-T)")]
		public int GroundPortTx { get { return mGroundPortTx; } set { mGroundPortTx = value; IsDirty = true; } } int mGroundPortTx;

		[XmlAttribute("routerIp"), DefaultValue(""), Description("The IPv4 address of the CCSDS router (-R)")]
		public string RouterIp { get { return mRouterIp; } set { mRouterIp = value; IsDirty = true; } } string mRouterIp;

		[XmlAttribute("routerPort"), DefaultValue(0), Description("The UDP port of the CCSDS router (-P)")]
		public int RouterPort { get { return mRouterPort; } set { mRouterPort = value; IsDirty = true; } } int mRouterPort;

		[XmlAttribute("green"), DefaultValue(""), Description("The green authentication key (-I)")]
		public string GreenKey { get { return mGreenKey; } set { mGreenKey = value; IsDirty = true; } } string mGreenKey;

		[XmlAttribute("red"), DefaultValue(""), Description("The red authentication key (-J)")]
		public string RedKey { get { return mRedKey; } set { mRedKey = value; IsDirty = true; } } string mRedKey;

		[XmlAttribute("brown"), DefaultValue(""), Description("The brown authentication key (-K)")]
		public string BrownKey { get { return mBrownKey; } set { mBrownKey = value; IsDirty = true; } } string mBrownKey;

		[XmlAttribute("showBrown"), DefaultValue(true), Description("Show the brown authentication key on the page")]
		public bool ShowBrown
		{
			get { return mShowBrown; }
			set
			{
				mShowBrown = value;
				IsDirty = true;
				if (View != null) (View as HostedPayload).Changed = true;
			}
		} bool mShowBrown = true;

		[XmlAttribute("showGreen"), DefaultValue(true), Description("Show the green authentication key on the page")]
		public bool ShowGreen
		{
			get { return mShowGreen; }
			set
			{
				mShowGreen = value;
				IsDirty = true;
				if (View != null) (View as HostedPayload).Changed = true;
			}
		} bool mShowGreen = true;

		[XmlAttribute("showRed"), DefaultValue(true), Description("Show the red authentication key on the page")]
		public bool ShowRed
		{
			get { return mShowRed; }
			set
			{
				mShowRed = value;
				IsDirty = true;
				if (View != null) (View as HostedPayload).Changed = true;
			}
		} bool mShowRed = true;

		[XmlAttribute("window"), DefaultValue(true),Description("Run the client in a console window.")]
		public bool Window { get { return mWindow; } set { mWindow = value; IsDirty = true; } } bool mWindow = true;

		public override StudioDocumentView NewView(string name)
		{
			var view = new HostedPayload() { Name = name };
			return view;
		}

	}

}
