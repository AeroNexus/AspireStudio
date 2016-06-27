namespace Aspire.BrowsingUI.TestBench
{
  /// <summary>
  /// An SDT DocumentWindow. Implements functionality to host test scripts for Aspire components
  /// </summary>
  partial class TestBenchView
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    //protected override void Dispose(bool disposing)
    //{
    //    if (disposing && (components != null))
    //    {
    //        components.Dispose();
    //    }
    //    base.Dispose(disposing);
    //}

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      Aspire.Framework.Scripting.ScriptObject scriptObject1 = new Aspire.Framework.Scripting.ScriptObject();
      this.btnRunSelected = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.treeAvailableTests = new Aspire.UiToolbox.Controls.TreeControl();
      this.contextMenuStripTree = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.publishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.printPreviousResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.node1 = new Aspire.UiToolbox.Controls.Node();
      this.node2 = new Aspire.UiToolbox.Controls.Node();
      this.panel1 = new System.Windows.Forms.Panel();
      this.linkSelectNone = new System.Windows.Forms.LinkLabel();
      this.linkSelectAll = new System.Windows.Forms.LinkLabel();
      this.btnRunAll = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.lblRunResults = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.lblTestResultsHeader = new System.Windows.Forms.Label();
      this.brwsrHelp = new System.Windows.Forms.WebBrowser();
      this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.ctxScriptingHelp = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.ctxTestResults = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuSaveResults = new System.Windows.Forms.ToolStripMenuItem();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainerTop = new System.Windows.Forms.SplitContainer();
      this.tableTopLeft = new System.Windows.Forms.TableLayoutPanel();
      this.flowPanelButtons = new System.Windows.Forms.FlowLayoutPanel();
      this.btnClearResults = new System.Windows.Forms.Button();
      this.tableTopRight = new System.Windows.Forms.TableLayoutPanel();
      this.toolTips = new System.Windows.Forms.ToolTip(this.components);
      this.tabControl1 = new System.Windows.Forms.TabControl();
      this.tpExecution = new System.Windows.Forms.TabPage();
      this.tpScript = new System.Windows.Forms.TabPage();
      this.editor = new Aspire.Studio.DocumentViews.CodeEditor();
      this.exportResultsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.selectedNodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.allCheckedNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.contextMenuStripTree.SuspendLayout();
      this.panel1.SuspendLayout();
      this.contextMenuStrip1.SuspendLayout();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.splitContainerTop.Panel1.SuspendLayout();
      this.splitContainerTop.Panel2.SuspendLayout();
      this.splitContainerTop.SuspendLayout();
      this.tableTopLeft.SuspendLayout();
      this.flowPanelButtons.SuspendLayout();
      this.tableTopRight.SuspendLayout();
      this.tabControl1.SuspendLayout();
      this.tpExecution.SuspendLayout();
      this.tpScript.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnRunSelected
      // 
      this.btnRunSelected.Location = new System.Drawing.Point(5, 3);
      this.btnRunSelected.Margin = new System.Windows.Forms.Padding(5, 3, 3, 3);
      this.btnRunSelected.Name = "btnRunSelected";
      this.btnRunSelected.Size = new System.Drawing.Size(89, 23);
      this.btnRunSelected.TabIndex = 0;
      this.btnRunSelected.Text = "&Run Selected";
      this.btnRunSelected.UseVisualStyleBackColor = true;
      this.btnRunSelected.Click += new System.EventHandler(this.button1_Click);
      // 
      // textBox1
      // 
      this.tableTopRight.SetColumnSpan(this.textBox1, 2);
      this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox1.Location = new System.Drawing.Point(5, 34);
      this.textBox1.Margin = new System.Windows.Forms.Padding(5);
      this.textBox1.Multiline = true;
      this.textBox1.Name = "textBox1";
      this.textBox1.ReadOnly = true;
      this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.textBox1.Size = new System.Drawing.Size(351, 174);
      this.textBox1.TabIndex = 2;
      this.textBox1.Click += new System.EventHandler(this.window_click);
      // 
      // treeAvailableTests
      // 
      this.treeAvailableTests.AutoEdit = false;
      this.treeAvailableTests.BoxDrawStyle = Aspire.UiToolbox.Controls.DrawStyle.Plain;
      this.treeAvailableTests.CheckStates = Aspire.UiToolbox.Controls.CheckStates.TwoStateCheck;
      this.tableTopLeft.SetColumnSpan(this.treeAvailableTests, 2);
      this.treeAvailableTests.ColumnWidth = 10;
      this.treeAvailableTests.ContextMenuStrip = this.contextMenuStripTree;
      this.treeAvailableTests.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeAvailableTests.FocusNode = null;
      this.treeAvailableTests.GroupArrows = true;
      this.treeAvailableTests.HotBackColor = System.Drawing.Color.Empty;
      this.treeAvailableTests.HotForeColor = System.Drawing.Color.Empty;
      this.treeAvailableTests.ImageGapRight = 0;
      this.treeAvailableTests.Indicators = Aspire.UiToolbox.Controls.Indicators.AtGroup;
      this.treeAvailableTests.LabelEdit = false;
      this.treeAvailableTests.LineDashStyle = Aspire.UiToolbox.Controls.LineDashStyle.Solid;
      this.treeAvailableTests.LineVisibility = Aspire.UiToolbox.Controls.LineBoxVisibility.OnlyBelowRoot;
      this.treeAvailableTests.Location = new System.Drawing.Point(5, 40);
      this.treeAvailableTests.Margin = new System.Windows.Forms.Padding(5);
      this.treeAvailableTests.Name = "treeAvailableTests";
      this.treeAvailableTests.Nodes.AddRange(new Aspire.UiToolbox.Controls.Node[] {
            this.node1,
            this.node2});
      this.treeAvailableTests.SelectedNode = null;
      this.treeAvailableTests.SelectedNoFocusBackColor = System.Drawing.SystemColors.Highlight;
      this.treeAvailableTests.SelectMode = Aspire.UiToolbox.Controls.SelectMode.Single;
      this.treeAvailableTests.Size = new System.Drawing.Size(391, 142);
      this.treeAvailableTests.TabIndex = 3;
      this.treeAvailableTests.Text = "treeControl1";
      this.treeAvailableTests.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeAvailableTests_MouseDoubleClick);
      this.treeAvailableTests.Click += new System.EventHandler(this.window_click);
      this.treeAvailableTests.AfterCheck += new Aspire.UiToolbox.Controls.NodeEventHandler(this.treeAvailableTests_AfterCheck);
      this.treeAvailableTests.AfterSelect += new Aspire.UiToolbox.Controls.NodeEventHandler(this.treeAvailableTests_AfterSelect);
      // 
      // contextMenuStripTree
      // 
      this.contextMenuStripTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.publishToolStripMenuItem,
            this.exportResultsMenuItem,
            this.printPreviousResultsToolStripMenuItem});
      this.contextMenuStripTree.Name = "contextMenuStripTree";
      this.contextMenuStripTree.Size = new System.Drawing.Size(188, 92);
      this.contextMenuStripTree.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripTree_Opening);
      // 
      // publishToolStripMenuItem
      // 
      this.publishToolStripMenuItem.Name = "publishToolStripMenuItem";
      this.publishToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
      this.publishToolStripMenuItem.Text = "&Publish Results";
      this.publishToolStripMenuItem.Click += new System.EventHandler(this.publishToolStripMenuItem_Click);
      // 
      // printPreviousResultsToolStripMenuItem
      // 
      this.printPreviousResultsToolStripMenuItem.Name = "printPreviousResultsToolStripMenuItem";
      this.printPreviousResultsToolStripMenuItem.Size = new System.Drawing.Size(187, 22);
      this.printPreviousResultsToolStripMenuItem.Text = "Print Pre&vious Results";
      this.printPreviousResultsToolStripMenuItem.Click += new System.EventHandler(this.printPreviousResultsToolStripMenuItem_Click);
      // 
      // node1
      // 
      this.node1.Checked = false;
      this.node1.Expanded = true;
      // 
      // node2
      // 
      this.node2.Checked = false;
      this.node2.Expanded = true;
      // 
      // panel1
      // 
      this.panel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.panel1.Controls.Add(this.linkSelectNone);
      this.panel1.Controls.Add(this.linkSelectAll);
      this.panel1.Location = new System.Drawing.Point(236, 16);
      this.panel1.Margin = new System.Windows.Forms.Padding(0);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(158, 19);
      this.panel1.TabIndex = 6;
      this.panel1.Click += new System.EventHandler(this.window_click);
      // 
      // linkSelectNone
      // 
      this.linkSelectNone.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.linkSelectNone.AutoSize = true;
      this.linkSelectNone.Location = new System.Drawing.Point(54, 3);
      this.linkSelectNone.Margin = new System.Windows.Forms.Padding(5, 0, 3, 0);
      this.linkSelectNone.Name = "linkSelectNone";
      this.linkSelectNone.Size = new System.Drawing.Size(62, 13);
      this.linkSelectNone.TabIndex = 4;
      this.linkSelectNone.TabStop = true;
      this.linkSelectNone.Text = "select none";
      this.linkSelectNone.Click += new System.EventHandler(this.linkSelectNone_Click);
      // 
      // linkSelectAll
      // 
      this.linkSelectAll.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.linkSelectAll.AutoSize = true;
      this.linkSelectAll.Location = new System.Drawing.Point(5, 3);
      this.linkSelectAll.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
      this.linkSelectAll.Name = "linkSelectAll";
      this.linkSelectAll.Size = new System.Drawing.Size(48, 13);
      this.linkSelectAll.TabIndex = 4;
      this.linkSelectAll.TabStop = true;
      this.linkSelectAll.Text = "select all";
      this.linkSelectAll.Click += new System.EventHandler(this.linkSelectAll_Click);
      // 
      // btnRunAll
      // 
      this.btnRunAll.Location = new System.Drawing.Point(100, 3);
      this.btnRunAll.Name = "btnRunAll";
      this.btnRunAll.Size = new System.Drawing.Size(83, 23);
      this.btnRunAll.TabIndex = 0;
      this.btnRunAll.Text = "Run &All";
      this.btnRunAll.UseVisualStyleBackColor = true;
      this.btnRunAll.Click += new System.EventHandler(this.btnRunAll_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.tableTopLeft.SetColumnSpan(this.label1, 2);
      this.label1.Dock = System.Windows.Forms.DockStyle.Top;
      this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(5, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(393, 16);
      this.label1.TabIndex = 7;
      this.label1.Text = "Available Tests";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.label1.Click += new System.EventHandler(this.window_click);
      // 
      // lblRunResults
      // 
      this.lblRunResults.AutoSize = true;
      this.tableTopRight.SetColumnSpan(this.lblRunResults, 2);
      this.lblRunResults.Dock = System.Windows.Forms.DockStyle.Fill;
      this.lblRunResults.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblRunResults.Location = new System.Drawing.Point(3, 213);
      this.lblRunResults.Name = "lblRunResults";
      this.lblRunResults.Size = new System.Drawing.Size(355, 14);
      this.lblRunResults.TabIndex = 9;
      this.lblRunResults.Text = "No tests run";
      this.lblRunResults.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.lblRunResults.Click += new System.EventHandler(this.window_click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.tableTopRight.SetColumnSpan(this.label2, 2);
      this.label2.Dock = System.Windows.Forms.DockStyle.Top;
      this.label2.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label2.Location = new System.Drawing.Point(5, 0);
      this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(353, 16);
      this.label2.TabIndex = 8;
      this.label2.Text = "Test Results";
      this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      this.label2.Click += new System.EventHandler(this.window_click);
      // 
      // label3
      // 
      this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
      this.label3.AutoSize = true;
      this.label3.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label3.Location = new System.Drawing.Point(8, 22);
      this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 3, 0);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(216, 13);
      this.label3.TabIndex = 7;
      this.label3.Text = "(grouped by Device Under Test)";
      this.label3.Click += new System.EventHandler(this.window_click);
      // 
      // lblTestResultsHeader
      // 
      this.lblTestResultsHeader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.lblTestResultsHeader.AutoSize = true;
      this.tableTopRight.SetColumnSpan(this.lblTestResultsHeader, 2);
      this.lblTestResultsHeader.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.lblTestResultsHeader.Location = new System.Drawing.Point(5, 16);
      this.lblTestResultsHeader.Margin = new System.Windows.Forms.Padding(5, 0, 3, 0);
      this.lblTestResultsHeader.Name = "lblTestResultsHeader";
      this.lblTestResultsHeader.Size = new System.Drawing.Size(121, 13);
      this.lblTestResultsHeader.TabIndex = 7;
      this.lblTestResultsHeader.Text = "For selected test:";
      this.lblTestResultsHeader.Click += new System.EventHandler(this.window_click);
      // 
      // brwsrHelp
      // 
      this.brwsrHelp.ContextMenuStrip = this.contextMenuStrip1;
      this.brwsrHelp.Dock = System.Windows.Forms.DockStyle.Fill;
      this.brwsrHelp.IsWebBrowserContextMenuEnabled = false;
      this.brwsrHelp.Location = new System.Drawing.Point(5, 5);
      this.brwsrHelp.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
      this.brwsrHelp.MinimumSize = new System.Drawing.Size(20, 20);
      this.brwsrHelp.Name = "brwsrHelp";
      this.brwsrHelp.Size = new System.Drawing.Size(768, 220);
      this.brwsrHelp.TabIndex = 6;
      // 
      // contextMenuStrip1
      // 
      this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ctxScriptingHelp,
            this.toolStripSeparator1,
            this.ctxTestResults,
            this.mnuSaveResults});
      this.contextMenuStrip1.Name = "contextMenuStrip1";
      this.contextMenuStrip1.Size = new System.Drawing.Size(178, 76);
      // 
      // ctxScriptingHelp
      // 
      this.ctxScriptingHelp.Checked = true;
      this.ctxScriptingHelp.CheckOnClick = true;
      this.ctxScriptingHelp.CheckState = System.Windows.Forms.CheckState.Checked;
      this.ctxScriptingHelp.Name = "ctxScriptingHelp";
      this.ctxScriptingHelp.Size = new System.Drawing.Size(177, 22);
      this.ctxScriptingHelp.Text = "Scripting &Help ";
      this.ctxScriptingHelp.Click += new System.EventHandler(this.ctxScriptingHelp_Click);
      // 
      // toolStripSeparator1
      // 
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new System.Drawing.Size(174, 6);
      // 
      // ctxTestResults
      // 
      this.ctxTestResults.CheckOnClick = true;
      this.ctxTestResults.Name = "ctxTestResults";
      this.ctxTestResults.Size = new System.Drawing.Size(177, 22);
      this.ctxTestResults.Text = "Display &Test Results";
      this.ctxTestResults.Click += new System.EventHandler(this.ctxTestResults_Click);
      // 
      // mnuSaveResults
      // 
      this.mnuSaveResults.Name = "mnuSaveResults";
      this.mnuSaveResults.Size = new System.Drawing.Size(177, 22);
      this.mnuSaveResults.Text = "&Save Results...";
      this.mnuSaveResults.Click += new System.EventHandler(this.mnuSaveResults_Click);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(3, 3);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.splitContainerTop);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.brwsrHelp);
      this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(5);
      this.splitContainer1.Size = new System.Drawing.Size(778, 467);
      this.splitContainer1.SplitterDistance = 233;
      this.splitContainer1.TabIndex = 5;
      // 
      // splitContainerTop
      // 
      this.splitContainerTop.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainerTop.Location = new System.Drawing.Point(0, 0);
      this.splitContainerTop.Margin = new System.Windows.Forms.Padding(5);
      this.splitContainerTop.Name = "splitContainerTop";
      // 
      // splitContainerTop.Panel1
      // 
      this.splitContainerTop.Panel1.Controls.Add(this.tableTopLeft);
      this.splitContainerTop.Panel1.Padding = new System.Windows.Forms.Padding(3);
      // 
      // splitContainerTop.Panel2
      // 
      this.splitContainerTop.Panel2.Controls.Add(this.tableTopRight);
      this.splitContainerTop.Panel2.Padding = new System.Windows.Forms.Padding(3);
      this.splitContainerTop.Size = new System.Drawing.Size(778, 233);
      this.splitContainerTop.SplitterDistance = 407;
      this.splitContainerTop.TabIndex = 5;
      // 
      // tableTopLeft
      // 
      this.tableTopLeft.ColumnCount = 2;
      this.tableTopLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 57.38095F));
      this.tableTopLeft.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42.61905F));
      this.tableTopLeft.Controls.Add(this.treeAvailableTests, 0, 2);
      this.tableTopLeft.Controls.Add(this.label1, 0, 0);
      this.tableTopLeft.Controls.Add(this.label3, 0, 1);
      this.tableTopLeft.Controls.Add(this.panel1, 1, 1);
      this.tableTopLeft.Controls.Add(this.flowPanelButtons, 0, 3);
      this.tableTopLeft.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableTopLeft.Location = new System.Drawing.Point(3, 3);
      this.tableTopLeft.Name = "tableTopLeft";
      this.tableTopLeft.RowCount = 4;
      this.tableTopLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableTopLeft.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableTopLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableTopLeft.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.tableTopLeft.Size = new System.Drawing.Size(401, 227);
      this.tableTopLeft.TabIndex = 0;
      // 
      // flowPanelButtons
      // 
      this.tableTopLeft.SetColumnSpan(this.flowPanelButtons, 2);
      this.flowPanelButtons.Controls.Add(this.btnRunSelected);
      this.flowPanelButtons.Controls.Add(this.btnRunAll);
      this.flowPanelButtons.Controls.Add(this.btnClearResults);
      this.flowPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
      this.flowPanelButtons.Location = new System.Drawing.Point(3, 190);
      this.flowPanelButtons.Name = "flowPanelButtons";
      this.flowPanelButtons.Size = new System.Drawing.Size(395, 34);
      this.flowPanelButtons.TabIndex = 8;
      // 
      // btnClearResults
      // 
      this.btnClearResults.Location = new System.Drawing.Point(189, 3);
      this.btnClearResults.Name = "btnClearResults";
      this.btnClearResults.Size = new System.Drawing.Size(90, 23);
      this.btnClearResults.TabIndex = 1;
      this.btnClearResults.Text = "R&eset";
      this.toolTips.SetToolTip(this.btnClearResults, "Clear all test results");
      this.btnClearResults.UseVisualStyleBackColor = true;
      this.btnClearResults.Click += new System.EventHandler(this.btnClearResults_Click);
      // 
      // tableTopRight
      // 
      this.tableTopRight.ColumnCount = 2;
      this.tableTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableTopRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableTopRight.Controls.Add(this.lblRunResults, 0, 3);
      this.tableTopRight.Controls.Add(this.textBox1, 0, 2);
      this.tableTopRight.Controls.Add(this.label2, 0, 0);
      this.tableTopRight.Controls.Add(this.lblTestResultsHeader, 0, 1);
      this.tableTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableTopRight.Location = new System.Drawing.Point(3, 3);
      this.tableTopRight.Name = "tableTopRight";
      this.tableTopRight.RowCount = 4;
      this.tableTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableTopRight.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableTopRight.Size = new System.Drawing.Size(361, 227);
      this.tableTopRight.TabIndex = 0;
      // 
      // tabControl1
      // 
      this.tabControl1.Controls.Add(this.tpExecution);
      this.tabControl1.Controls.Add(this.tpScript);
      this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl1.Location = new System.Drawing.Point(5, 5);
      this.tabControl1.Margin = new System.Windows.Forms.Padding(10);
      this.tabControl1.Name = "tabControl1";
      this.tabControl1.SelectedIndex = 0;
      this.tabControl1.Size = new System.Drawing.Size(792, 499);
      this.tabControl1.TabIndex = 6;
      // 
      // tpExecution
      // 
      this.tpExecution.Controls.Add(this.splitContainer1);
      this.tpExecution.Location = new System.Drawing.Point(4, 22);
      this.tpExecution.Name = "tpExecution";
      this.tpExecution.Padding = new System.Windows.Forms.Padding(3);
      this.tpExecution.Size = new System.Drawing.Size(784, 473);
      this.tpExecution.TabIndex = 0;
      this.tpExecution.Text = "Execution";
      this.tpExecution.UseVisualStyleBackColor = true;
      // 
      // tpScript
      // 
      this.tpScript.Controls.Add(this.editor);
      this.tpScript.Location = new System.Drawing.Point(4, 22);
      this.tpScript.Name = "tpScript";
      this.tpScript.Padding = new System.Windows.Forms.Padding(3);
      this.tpScript.Size = new System.Drawing.Size(784, 473);
      this.tpScript.TabIndex = 1;
      this.tpScript.Text = "Test Script";
      this.tpScript.UseVisualStyleBackColor = true;
      // 
      // editor
      // 
      this.editor.Code = "";
      this.editor.CommentString = "//";
      this.editor.CompilerErrors = null;
      this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
      this.editor.FileName = "";
      this.editor.HostComponent = null;
      this.editor.IsDirty = false;
      this.editor.IsSaving = false;
      this.editor.LanguageType = Aspire.Framework.Scripting.LanguageType.Csharp;
      this.editor.Location = new System.Drawing.Point(3, 3);
      this.editor.Name = "editor";
      this.editor.SaveFileDlg = null;
      this.editor.SaveOnCompile = false;
      scriptObject1.Code = "";
      scriptObject1.DontExecute = false;
      scriptObject1.Filename = "";
      scriptObject1.Tag = -1;
      this.editor.ScriptObject = scriptObject1;
      this.editor.Size = new System.Drawing.Size(778, 467);
      this.editor.StatusBarVisible = false;
      this.editor.TabIndex = 0;
      this.editor.WatchesPaneVisible = false;
      // 
      // exportResultsMenuItem
      // 
      this.exportResultsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectedNodeToolStripMenuItem,
            this.allCheckedNodesToolStripMenuItem});
      this.exportResultsMenuItem.Name = "exportResultsMenuItem";
      this.exportResultsMenuItem.Size = new System.Drawing.Size(187, 22);
      this.exportResultsMenuItem.Text = "E&xport Results...";
      this.exportResultsMenuItem.Click += new System.EventHandler(this.exportResultsMenuItem_Click);
      // 
      // selectedNodeToolStripMenuItem
      // 
      this.selectedNodeToolStripMenuItem.Name = "selectedNodeToolStripMenuItem";
      this.selectedNodeToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
      this.selectedNodeToolStripMenuItem.Text = "Selected Node";
      this.selectedNodeToolStripMenuItem.Click += new System.EventHandler(this.selectedNodeToolStripMenuItem_Click);
      // 
      // allCheckedNodesToolStripMenuItem
      // 
      this.allCheckedNodesToolStripMenuItem.Name = "allCheckedNodesToolStripMenuItem";
      this.allCheckedNodesToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
      this.allCheckedNodesToolStripMenuItem.Text = "All Checked Nodes";
      this.allCheckedNodesToolStripMenuItem.Click += new System.EventHandler(this.allCheckedNodesToolStripMenuItem_Click);
      // 
      // TestBenchDocumentWindow
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(802, 509);
      this.Controls.Add(this.tabControl1);
      this.Location = new System.Drawing.Point(0, 0);
      this.Name = "TestBenchDocumentWindow";
      this.Padding = new System.Windows.Forms.Padding(5);
      this.Text = "TestBenchDocumentWindow";
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.TestBenchDocumentWindow_DragDrop);
      this.DragOver += new System.Windows.Forms.DragEventHandler(this.TestBenchDocumentWindow_DragOver);
      this.contextMenuStripTree.ResumeLayout(false);
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.contextMenuStrip1.ResumeLayout(false);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      this.splitContainer1.ResumeLayout(false);
      this.splitContainerTop.Panel1.ResumeLayout(false);
      this.splitContainerTop.Panel2.ResumeLayout(false);
      this.splitContainerTop.ResumeLayout(false);
      this.tableTopLeft.ResumeLayout(false);
      this.tableTopLeft.PerformLayout();
      this.flowPanelButtons.ResumeLayout(false);
      this.tableTopRight.ResumeLayout(false);
      this.tableTopRight.PerformLayout();
      this.tabControl1.ResumeLayout(false);
      this.tpExecution.ResumeLayout(false);
      this.tpScript.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button btnRunSelected;
    private System.Windows.Forms.TextBox textBox1;
    private Aspire.UiToolbox.Controls.TreeControl treeAvailableTests;
    private System.Windows.Forms.LinkLabel linkSelectAll;
    private System.Windows.Forms.LinkLabel linkSelectNone;
    private System.Windows.Forms.Button btnRunAll;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.WebBrowser brwsrHelp;
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label lblRunResults;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label lblTestResultsHeader;
    private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    private System.Windows.Forms.ToolStripMenuItem ctxScriptingHelp;
    private System.Windows.Forms.ToolStripMenuItem ctxTestResults;
    private Aspire.UiToolbox.Controls.Node node1;
    private Aspire.UiToolbox.Controls.Node node2;
    private System.Windows.Forms.SplitContainer splitContainerTop;
    private System.Windows.Forms.TableLayoutPanel tableTopLeft;
    private System.Windows.Forms.TableLayoutPanel tableTopRight;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    private System.Windows.Forms.ToolStripMenuItem mnuSaveResults;
    private System.Windows.Forms.ContextMenuStrip contextMenuStripTree;
    private System.Windows.Forms.ToolStripMenuItem publishToolStripMenuItem;
    private System.Windows.Forms.FlowLayoutPanel flowPanelButtons;
    private System.Windows.Forms.Button btnClearResults;
    private System.Windows.Forms.ToolTip toolTips;
    private System.Windows.Forms.TabControl tabControl1;
    private System.Windows.Forms.TabPage tpExecution;
    private System.Windows.Forms.TabPage tpScript;
    private Aspire.Studio.DocumentViews.CodeEditor editor;
    private System.Windows.Forms.ToolStripMenuItem printPreviousResultsToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem exportResultsMenuItem;
    private System.Windows.Forms.ToolStripMenuItem selectedNodeToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem allCheckedNodesToolStripMenuItem;
  }
}