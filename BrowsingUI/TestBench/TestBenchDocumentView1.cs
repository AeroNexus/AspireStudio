using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Aspire.Studio.DocumentViews;
using Aspire.CoreModels;
using Aspire.BrowsingUI.Properties;
using Aspire.Utilities;

#pragma warning disable 1591

namespace Aspire.BrowsingUI.TestBench
{
  public partial class TestBenchView1 : StudioDocumentView//, IPrinterSupport
    {
        const Indicator DeviceReadyIndicator = Indicator.Lightning;
        const Indicator DeviceNotReadyIndicator = Indicator.Lock;
        const Indicator NoSuchDeviceIndicator = Indicator.QuestionBlack;

        public TestBenchView1()
        {
            InitializeComponent();
            CreateNewDocument();
            ShowTestResults();
        }

        private void ShowScriptHelp()
        {
            ctxScriptingHelp.Checked = true;
            ctxTestResults.Checked = false;

            brwsrHelp.DocumentText = @"<HTML><HEAD></HEAD><BODY><H2>Script Attributes</H2>
<B>TestFixtureSetUp</B>: Method invoked at the start of a test run; only invoked once regardless of the number of test methods executed.<p>
<B>TestFixtureTearDown</B>: Method invoked at the end of a test run; only invoked once regardless of the number of test methods executed.<p>
<B>SetUp</B>: Method invoked prior to each test method being invoked.<p>
<B>TearDown</B>: Method invoked after each test method has been invoked. The TearDown method is invoked regardless of whether the test method fails or throws an exception.<p>
<B>Test</B>: Marks a method as being a test method. The method should have the signature <i>void MethodName()</i><p>
<B>DeviceUnderTest</B>: An optional attribute applied to a test method to specify which device is being tested. Each method can have multiple [DeviceUnderTest] attributes. Specify IsSpecifiedAsDeviceKind = true to indicate that the Name refers to a KIND.<p>
</BODY></HTML>";

        }

        //protected override void OnGotFocus(EventArgs e)
        //{
        //    base.OnGotFocus(e);
        //    RaiseItemSelectedEvent( Document );
        //}

        //protected override void OnClick(EventArgs e)
        //{
        //    base.OnClick(e);
        //    RaiseItemSelectedEvent(Document);
        //}

        //public override void WindowActivated()
        //{
        //    RaiseItemSelectedEvent(Document);
        //}

        //public override void LoadContents()
        //{
        //    Document = Deserialize() as TestBenchDocument;
        //}

        public TestBenchDocument Document
        {
            get
            {
                return m_Document;
            }

            private set
            {
                if (m_Document != null && m_Document.ScriptObject != null)
                {
                    if (m_Document.ScriptObject.CompiledObject is IDisposable)
                    {
                        (m_Document.ScriptObject.CompiledObject as IDisposable).Dispose();
                    }

                    m_Document.PropertyChanged -= new PropertyChangedEventHandler(Document_PropertyChanged);

                    m_Document.ScriptObject.ScriptCompiledSuccessfully -= new EventHandler(ScriptObject_ScriptCompiledSuccessfully);
                }

                editor.ScriptObject = null;

                m_Document = value;

                if (m_Document != null)
                {
                    editor.ScriptObject = m_Document.ScriptObject;

                    if (!string.IsNullOrEmpty(m_Document.Caption))
                    {
                        this.Text = m_Document.Caption;
                    }

                    m_Document.PropertyChanged += new PropertyChangedEventHandler(Document_PropertyChanged);
                    
                    if (m_Document.ScriptObject != null)
                    {
                        m_Document.ScriptObject.ScriptCompiledSuccessfully += new EventHandler(ScriptObject_ScriptCompiledSuccessfully);
                        try
                        {
                            m_Document.ScriptObject.Compile();
                        }
                        catch (Exception ex)
                        {
                            Log.ReportException(ex, "TestBench: Error compiling script");
                        }
                    }
                }
            }
        }

        void Document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Document.Caption))
            {
                this.Text = Document.Caption;
            }
            IsDirty = true;
        }

        private TestBenchDocument m_Document;

        //public override StudioDocument GetDocument()
        //{
        //    return m_Document;
        //}

        //public override StudioDocument CreateNewDocument()
        //{
        //    Document = new TestBenchDocument();
        //    return m_Document;
        //}

        const string UnspecifiedNodeLabel = "Unspecified";

        private Dictionary<string, Node> m_NodesByDevice = new Dictionary<string, Node>();
        
        // each test script has a single setup, teardown, test fixture setup, and test fixture teardown method...
        private System.Reflection.MethodInfo m_SetupMethod = null;

        private System.Reflection.MethodInfo m_TearDownMethod = null;

        private System.Reflection.MethodInfo m_TestFixtureSetupMethod = null;

        private System.Reflection.MethodInfo m_TestFixtureTearDownMethod = null;
        
        private void CreateNodesForMethod(System.Reflection.MethodInfo method)
        {
            CreateNodesForMethod(method, string.Empty);
        }

        private void SubscribeToDataMessages(AspireComponent component)
        {
            List<string> notificationsNeeded = null;
            if (m_SubscriptionsToProcess.TryGetValue(component, out notificationsNeeded))
            {
                notificationsNeeded.ForEach(notification =>
                    {
                    var dataMsg = component.Xteds.FindMessage(notification) as Xteds.Interface.DataMessage;
                    if (dataMsg != null)
                    {
                        dataMsg.Subscribe();
                    }
                    else
                        Log.WriteLine(Log.Severity.Warning, "TestBench: Unable to find data message {0} for subscription.", notification);
                    }
                );
            }
        }

        Dictionary<AspireComponent, List<string>> m_SubscriptionsToProcess = new Dictionary<AspireComponent, List<string>>();

        private void CreateNodesForMethod(System.Reflection.MethodInfo method, string className)
        {
            DeviceUnderTestAttribute[] deviceNames = method.GetCustomAttributes(typeof(DeviceUnderTestAttribute), false).
                Cast<DeviceUnderTestAttribute>().ToArray();

            List<Node> nodesForMethod = new List<Node>();

            // if the attribute isn't applied to the method, then it's "unspecified"
            if (deviceNames.Length == 0)
            {
                Node deviceNode = null;
                if (m_NodesByDevice.TryGetValue(UnspecifiedNodeLabel, out deviceNode))
                {
                    nodesForMethod.Add(deviceNode);
                }
            }
            else
            {
                deviceNames.ForEach(attr =>
                    {
                        if (attr.Name == "*")
                        {
                            var list = AspireBrowser.AllComponents.Select(c => c);

                            // and add nodes to the tree for each
                            list.ForEach(c => nodesForMethod.Add(
                                        GetDeviceNodeForComponent(attr, c.Name, c)));
                        }
                        // if the test is for a device/app KIND...
                        else if (attr.IsSpecifiedAsDeviceKind)
                        {
                            // find all AspireComponent instances of that KIND
                            string kind = attr.Name;
                            var list = AspireBrowser.AllComponents.Where(
                                c => SafeDeviceKindGetter(c).Equals(kind, StringComparison.InvariantCultureIgnoreCase));

                            // and add nodes to the tree for each
                            list.ForEach(c => nodesForMethod.Add(
                                        GetDeviceNodeForComponent(attr, c.Name, c)));
                        }
                        else     // otherwise, a device name is specified
                        {
                            string deviceName = attr.Name;
                            AspireComponent comp = AspireBrowser.AllComponent(deviceName);

                            Node deviceNode = GetDeviceNodeForComponent(attr, deviceName, comp);

                            nodesForMethod.Add(deviceNode);
                        }
                    }
                );
            }

            nodesForMethod.ForEach(n =>
                {
                    string methodName = string.IsNullOrEmpty(className) ? method.Name :
                                                string.Format("{0}.{1}", className, method.Name);
                    Node node = new Node(methodName);
                    //node.Indicator = Indicator.QuestionGreen;
                    node.Image = PnPInnovations.Aspire.BrowsingUserInterface.Properties.Resources.unknown;
                    node.Tag = new TestResultInfo(node) { MethodInfo = method, Component = (n.Tag as AspireComponent) };
                    node.Checked = false;
                    n.Nodes.Add(node);
                }
            );
        }

        private Node GetDeviceNodeForComponent(DeviceUnderTestAttribute attr, string deviceName, AspireComponent comp)
        {
            Node deviceNode = null;
            if (comp != null)
            {
                List<string> notifications = null;
                if (!m_SubscriptionsToProcess.TryGetValue(comp, out notifications))
                {
                    notifications = new List<string>();
                    m_SubscriptionsToProcess.Add(comp, notifications);
                }

                if (!string.IsNullOrEmpty(attr.NotificationsRequired))
                {
                    string[] attrNotifications = attr.NotificationsRequired.Split(';');
                    attrNotifications.ForEach(noti =>
                    { if (!notifications.Contains(noti)) notifications.Add(noti); });
                }
            }

            // do we already have a node for this device?
            if (!m_NodesByDevice.TryGetValue(deviceName, out deviceNode))
            {
                // no...
                // make a new node
                deviceNode = new Node(deviceName);
                deviceNode.CheckStates = NodeCheckStates.TwoStateCheck;
                treeAvailableTests.Nodes.Add(deviceNode);
                m_NodesByDevice.Add(deviceName, deviceNode);

                // make sure the device's AspireComponent is ready
                if (comp != null)
                {
                    deviceNode.Tag = comp;
                    deviceNode.Indicator = DeviceNotReadyIndicator;
                }
                else
                {
                    // no such component found...
                    deviceNode.Indicator = NoSuchDeviceIndicator;
                }
            }

            return deviceNode;
        }

        void xcomp_ComponentIdChanged(object sender, EventArgs e)
        {
            AspireComponent comp = sender as AspireComponent;
            comp.ComponentIdChanged -= new EventHandler(xcomp_ComponentIdChanged);
            
            SubscribeToDataMessages(comp);

            Node node = null;
            if (m_NodesByDevice.TryGetValue(comp.Name, out node))
            {
                node.Indicator = DeviceReadyIndicator;
            }
        }

        void comp_XtedsAvailable(object sender, EventArgs e)
        {
            AspireComponent comp = sender as AspireComponent;
            if (comp != null)
            {
                SubscribeToDataMessages(comp);

                Node node = null;
                if (m_NodesByDevice.TryGetValue(comp.Name, out node))
                {
                    node.Indicator = DeviceReadyIndicator;
                }
            }
        }

        private void LoadTestMethods()
        {
            lblRunResults.Text = "No test run.";
            
            m_SubscriptionsToProcess.Clear();

            treeAvailableTests.Nodes.Clear();
            m_NodesByDevice.Clear();

            Node unspecified = new Node(UnspecifiedNodeLabel);
            treeAvailableTests.Nodes.Add(unspecified);
            m_NodesByDevice.Add(UnspecifiedNodeLabel, unspecified);
            unspecified.CheckStates = NodeCheckStates.TwoStateCheck;

            System.Type type = m_Document.ScriptObject.CompiledObject.GetType();
            System.Reflection.MethodInfo[] methods = type.GetMethods();
            
            if (methods != null)
            {
                methods.Where(method => method.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false).Length > 0).
                    ForEach(method => CreateNodesForMethod(method));

                m_SetupMethod = methods.FirstOrDefault(method =>
                    method.GetCustomAttributes(typeof(NUnit.Framework.SetUpAttribute), false).Length > 0);

                m_TearDownMethod = methods.FirstOrDefault(method =>
                    method.GetCustomAttributes(typeof(NUnit.Framework.TearDownAttribute), false).Length > 0);

                m_TestFixtureSetupMethod = methods.FirstOrDefault(method =>
                    method.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureSetUpAttribute), false).Length > 0);

                m_TestFixtureTearDownMethod = methods.FirstOrDefault(method =>
                    method.GetCustomAttributes(typeof(NUnit.Framework.TestFixtureTearDownAttribute), false).Length > 0);
            }

            // if there are no methods in the "Unspecified" group, remove the group
            if (unspecified.Nodes.Count == 0)
            {
                unspecified.Remove();
                m_NodesByDevice.Remove(UnspecifiedNodeLabel);
            }

            MakeAllTheAspireStuffWork();

            // nested types have creation issues when invoking the methods: there is no object of the nested type in existence
            //type.GetNestedTypes().ForEach(nestedType =>
            //    {
            //        methods = nestedType.GetMethods();
            //        if (methods != null)
            //        {
            //            methods.Where(method => method.GetCustomAttributes(typeof(NUnit.Framework.TestAttribute), false).Length > 0).
            //                ForEach(method => CreateNodesForMethod(method, nestedType.Name));
            //        }
            //    }
            //);
        }

        private void MakeAllTheAspireStuffWork()
        {
            m_SubscriptionsToProcess.Keys.ForEach(comp =>
                {
                    if (comp.Xteds == null)
                    {
                        // listen for the component to become available
                        comp.XtedsAvailable += new EventHandler(comp_XtedsAvailable);

                        // inform the AspireBrowser that we want to use this device
                        comp.Browser.UseComponent(comp.Name);                        
                    }
                    else
                    {
                        Node node = null;
                        if (m_NodesByDevice.TryGetValue(comp.Name, out node))
                        {
                            node.Indicator = DeviceReadyIndicator;
                        }

                        SubscribeToDataMessages(comp);
                    }
                }
            );
        }

        void ScriptObject_ScriptCompiledSuccessfully(object sender, EventArgs e)
        {
            LoadTestMethods();
        }

        //public override object[] GetSelectedItems()
        //{
        //    object[] objects = new object[1];
        //    objects[0] = m_Document;
        //    return objects;
        //}

        private void PerformTestTearDown()
        {
            // Call the [TearDown] method, if present
            if (m_TearDownMethod != null)
            {
                try
                {
                    Log.WriteLine(Log.Severity.Debug, "Calling TearDown method.");
                    
                    m_TearDownMethod.Invoke(m_Document.ScriptObject.CompiledObject, null);

                    Log.WriteLine(Log.Severity.Debug, "TearDown method succeeded.");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(NUnit.Framework.AssertionException))
                    {
                        Log.WriteLine("TestBench", Log.Severity.Error, "TearDown failed:{1}{0}",
                            ex.InnerException.Message, System.Environment.NewLine);
                    }
                    else
                    {
                        Log.ReportException( ex, "TestBenchDocumentWindow", "TearDown failed");
                    }

                    if (Log.IsErrorEnabled) Log.Error("Error calling TearDown", ex);
                }
            }
        }

        private bool PerformTestFixtureSetUp()
        {
            if (m_TestFixtureSetupMethod != null)
            {
                try
                {
                  Log.WriteLine(Log.Severity.Debug, "Calling TestFixtureSetup method.");

                    m_TestFixtureSetupMethod.Invoke(m_Document.ScriptObject.CompiledObject, null);

                    Log.WriteLine(Log.Severity.Debug, "TestFixtureSetup method succeeded.");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(NUnit.Framework.AssertionException))
                    {
                        Log.WriteLine("TestBench", Severity.Error, "TestFixtureSetUp failed:{1}{0}",
                            ex.InnerException.Message, System.Environment.NewLine);
                    }
                    else
                    {
                        Log.ReportException("TestBenchDocumentWindow", Severity.Error, "TestFixtureSetUp failed", ex);
                    }

                    if (logging.IsErrorEnabled) logging.Error("Error calling TestFixtureSetUp method", ex);

                    return false;
                }
            }
            return true;
        }

        private void PerformTestFixtureTearDown()
        {
            if (m_TestFixtureTearDownMethod != null)
            {
                try
                {
                  Log.WriteLine(Log.Severity.Debug, "Calling TestFixtureTearDown method.");

                    m_TestFixtureTearDownMethod.Invoke(m_Document.ScriptObject.CompiledObject, null);

                    Log.WriteLine(Log.Severity.Debug, "TestFixtureTearDown method succeeded.");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(AssertionException))
                    {
                        Log.WriteLine(Log.Severity.Error, "TestBench", "TestFixtureTearDown failed:{1}{0}",
                            ex.InnerException.Message, System.Environment.NewLine);
                    }
                    else
                    {
                        Log.ReportException(ex, "TestBenchDocumentWindow", "TestFixtureTearDown failed");
                    }
                    
                    if (logging.IsErrorEnabled) logging.Error("Error calling TestFixtureSetUp method", ex);
                }
            }
        }

       // private Thread m_ExecutionThread = null;
        //private bool m_IsExecuting = false;

        private string SafeDeviceUidGetter(AspireComponent comp)
        {
            string uid = string.Empty;
            new Action(() => { uid = comp != null ? comp.CompUidString : string.Empty; }).ExecuteAndIgnoreException();
            return uid;
        }

        private string SafeDeviceKindGetter(AspireComponent comp)
        {
            string kind = string.Empty;
            new Action(() => { kind = comp != null ? comp.Kind : string.Empty; }).ExecuteAndIgnoreException();
            return kind;

        }

        private void ExecuteMethodForNode(Node deviceNode, Node node)
        {
            TestResultInfo method = (node.Tag as TestResultInfo);
            AspireComponent comp = (deviceNode.Tag as AspireComponent);
            
            m_Results.ResultInfos.Add(method);
            method.Component = comp;
            method.DeviceName = deviceNode.Text;

            TestBenchScriptObject scriptObject = (m_Document.ScriptObject.CompiledObject as TestBenchScriptObject);

            if (scriptObject != null)
            {
                scriptObject.DeviceName = deviceNode.Text;
                scriptObject.DeviceUid = SafeDeviceUidGetter(comp);
                scriptObject.DeviceKind = SafeDeviceKindGetter(comp);
            }

            if ( method != null && method.MethodInfo != null )
            {
                try
                {
                    //textBox1.Text = string.Empty;
                    m_TestMessages = string.Empty;
                    Log.NewText += new TextHandler(Log_NewText);

                    // Call the [SetUp] method, if present
                    if (m_SetupMethod != null)
                    {
                        m_SetupMethod.Invoke(m_Document.ScriptObject.CompiledObject, null);
                    }

                    m_Results.m_TestsRun++;
                    
                    // Call the test method
                    method.MethodInfo.Invoke(m_Document.ScriptObject.CompiledObject, null);

                    Log.WriteLine(Log.Severity.Info,"Test {0} succeeded.", method.MethodInfo.Name);

                    // Mark the test as passed
                    m_Results.m_TestsPassed++;
                    method.Passed = true;
                    //node.Indicator = Indicator.FlagGreen;
                    node.Image = Resources.check;
                }
                catch (Exception ex)
                {
                    method.Passed = false;

                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(NUnit.Framework.AssertionException))
                    {
                        //node.Indicator = Indicator.FlagRed;
                        node.Image = PnPInnovations.Aspire.BrowsingUserInterface.Properties.Resources.error;
                        m_Results.m_TestsFailed++;
                        //method.Exception = ex.InnerException;
                        Log.WriteLine(Log.Severity.Error, "TestBench", "{0} failed:{2}{1}", 
                            method.MethodInfo.Name, ex.InnerException.Message, System.Environment.NewLine);

                        Log.WriteLine(Log.Severity.Info,"Test {0} filed:{1}", method.MethodInfo.Name, ex.InnerException.Message);
                    }
                    else
                    {
                        //node.Indicator = Indicator.Exclamation;
                        node.Image = PnPInnovations.Aspire.BrowsingUserInterface.Properties.Resources.stop;
                        m_Results.m_TestsExceptionThrown++;
                        method.Exception = ex;
                        Log.ReportException(ex, "TestBenchDocumentWindow");
                        Log.WriteLine(Log.Severity.Error,"Error calling method {0}", method.MethodInfo.Name);
                    }
                }
                finally
                {
                    // invoke the per-test TearDown method
                    PerformTestTearDown();

                    lblRunResults.Text = m_Results.ToString();

                    method.OutputText = m_TestMessages;

                    Log.NewText -= new Log.TextHandler(Log_NewText);

                    node.Select();
                }
            }
        }

        private void RunTests(IEnumerable<Node> nodesToRun, Node deviceNode)
        {
            if (nodesToRun.Count() > 0 && m_Document != null && m_Document.ScriptObject != null && m_Document.ScriptObject.CompiledObject != null)
            {
                Node selected = treeAvailableTests.SelectedNode;
                if (selected != null) selected.Deselect();

                Log.WriteLine(Log.Severity.Info,"Starting tests...");

                if (PerformTestFixtureSetUp())
                {
                    nodesToRun.ForEach(node => ExecuteMethodForNode(deviceNode, node));

                    PerformTestFixtureTearDown();
                }
                
                //if (selected != null) selected.Select();

                Log.WriteLine(Info, m_Results.ToString());

                if (ctxTestResults.Checked)
                {
                    ShowTestResults();
                }
            }

        }

        private TestResults m_Results = new TestResults();

        private void ToggleButtons(bool state)
        {
            btnRunSelected.Enabled = state;
            btnRunAll.Enabled = state;
            treeAvailableTests.Enabled = state;
            linkSelectAll.Enabled = state;
            linkSelectNone.Enabled = state;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RaiseItemSelectedEvent(Document);
            ToggleButtons(false);
            m_Results = new TestResults();
            
            lblRunResults.Text = m_Results.ToString();

            try
            {
                treeAvailableTests.Nodes.Cast<Node>().ForEach( deviceNode => 
                    RunTests(deviceNode.Nodes.Cast<Node>().Where(node => node.Checked), deviceNode));
            }
            catch (Exception ex)
            {
                Log.ReportException("TestBench", Log.Severity.Error, "Error running tests", ex);
            }
            finally
            {
                lblRunResults.Text = m_Results.ToString();
                ToggleButtons(true);
            }
        }

        private void btnRunAll_Click(object sender, EventArgs e)
        {
            RaiseItemSelectedEvent(Document);
            ToggleButtons(false);
            m_Results = new TestResults();
            lblRunResults.Text = m_Results.ToString();

            try
            {
                treeAvailableTests.Nodes.Cast<Node>().ForEach(deviceNode =>
                    RunTests(deviceNode.Nodes.Cast<Node>(), deviceNode));
            }
            catch (Exception ex)
            {
                Log.ReportException("TestBench", Log.Severity.Error, "Error running tests", ex);
            }
            finally
            {
                lblRunResults.Text = m_Results.ToString();
                ToggleButtons(true);
            }
        }

        void AppendText(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new AppendTextDelegate(AppendText), text);

            }
            else
            {
                textBox1.Text += text + System.Environment.NewLine;
            }
        }

        private delegate void AppendTextDelegate(string text);

        private string m_TestMessages = string.Empty;

        void Log_NewText(string text, Log.Severity severity)
        {
            m_TestMessages += text + System.Environment.NewLine;
        }

        private class TestResultInfo
        {
            public TestResultInfo(Node node)
            {
                Node = node;
            }

            public AspireComponent Component { get; set; }

            public string DeviceName { get; set; }

            public Node Node { get; private set; }

            /// <summary>
            /// Pass or fail; null indicates that the test has not been executed
            /// </summary>
            public bool? Passed { get; set; }

            public Exception Exception { get; set; }

            public string OutputText { get; set; }

            public System.Reflection.MethodInfo MethodInfo { get; set; }

            public override string ToString()
            {
                return MethodInfo != null ? MethodInfo.Name : GetType().Name;
            }
        }

        private class TestResults
        {
            public int m_TestsRun = 0;
            public int m_TestsPassed = 0;
            public int m_TestsFailed = 0;
            public int m_TestsExceptionThrown = 0;

            public List<TestResultInfo> ResultInfos = new List<TestResultInfo>();

            public void Reset()
            {
                m_TestsRun = m_TestsPassed = m_TestsFailed = m_TestsExceptionThrown = 0;
                ResultInfos.Clear();
            }

            public override string ToString()
            {
                return string.Format("{0} tests run; {1} passed, {2} failed, {3} with exceptions",
                    m_TestsRun, m_TestsPassed, m_TestsFailed, m_TestsExceptionThrown);
            }
        }


        void treeAvailableTests_AfterCheck(TreeControl tc, NodeEventArgs e)
        {
            e.Node.Nodes.Cast<Node>().ForEach(node => node.Checked = e.Node.Checked);
        }

        private void treeAvailableTests_AfterSelect(TreeControl tc, NodeEventArgs e)
        {
            if (treeAvailableTests.SelectedNode != null && treeAvailableTests.SelectedNode.Tag is TestResultInfo)
            {
                lblTestResultsHeader.Text = 
                    string.Format("For {0}.{1}", treeAvailableTests.SelectedNode.Parent.Text,
                        treeAvailableTests.SelectedNode.Text );

                textBox1.Text = (treeAvailableTests.SelectedNode.Tag as TestResultInfo).OutputText;
            }
        }

        private void linkSelectAll_Click(object sender, EventArgs e)
        {
            treeAvailableTests.Nodes.Cast<Node>().ForEach(group => group.Nodes.Cast<Node>().ForEach(n => n.Checked = true));
        }

        private void linkSelectNone_Click(object sender, EventArgs e)
        {
            treeAvailableTests.Nodes.Cast<Node>().ForEach(group => group.Nodes.Cast<Node>().ForEach(n => n.Checked = false));
        }

        private void window_click(object sender, EventArgs e)
        {
            RaiseItemSelectedEvent(Document);
        }

        private void ctxTestResults_Click(object sender, EventArgs e)
        {
            ShowTestResults();
        }

        private void ShowTestResults()
        {
            ctxScriptingHelp.Checked = false;
            ctxTestResults.Checked = true;

            this.brwsrHelp.DocumentText = GetResultsAsHtml();
        }

        private string GetResultsAsHtml()
        {
            string HTML = @"<HTML><BODY><H1>Results</H1>";
            HTML += m_Results.ToString();
            HTML += "<hr><p>";

            // construct the HTML for tests
            var list = m_Results.ResultInfos.
                Where(info => info.DeviceName == UnspecifiedNodeLabel);

            if (list.Count() > 0)
            {
                HTML += PrintResultsForDevice(list, UnspecifiedNodeLabel);
            }

            var devices = from info in m_Results.ResultInfos
                          where info.DeviceName != UnspecifiedNodeLabel
                          group info by info.DeviceName;

            devices.ForEach(group =>
            {
                string deviceName = group.Key;
                list = m_Results.ResultInfos.Where(info => info.DeviceName == deviceName);
                HTML += PrintResultsForDevice(list, deviceName);
            }
            );

            HTML += @"</BODY></HTML>";
            return HTML;
        }

        private string GetResultsAsText()
        {
            string text = @"Results" + Environment.NewLine;
            text += m_Results.ToString() + Environment.NewLine;
            text += string.Empty.PadRight(80, '-') + Environment.NewLine;

            // construct the HTML for tests
            var list = m_Results.ResultInfos.
                Where(info => info.DeviceName == UnspecifiedNodeLabel);

            if (list.Count() > 0)
            {
                text += Environment.NewLine + UnspecifiedNodeLabel + Environment.NewLine;
                list.ForEach(info =>
                    {
                        text += string.Format("{0}: {1}", info.MethodInfo.Name, TestResult(info));
                        text += Environment.NewLine;
                    });
            }

            var devices = from info in m_Results.ResultInfos
                          where info.DeviceName != UnspecifiedNodeLabel
                          group info by info.DeviceName;

            devices.ForEach(group =>
            {
                text += Environment.NewLine;

                string deviceName = group.Key;
                text += Environment.NewLine + deviceName + Environment.NewLine;
                list = m_Results.ResultInfos.Where(info => info.DeviceName == deviceName);
                list.ForEach(info =>
                {
                    text += string.Format("{0}: {1}", info.MethodInfo.Name, TestResult(info));
                    text += Environment.NewLine;
                });
            }
            );

            return text;
        }

        private string TestResult(TestResultInfo info)
        {
            if (info.Exception != null) return "Threw Exception";
            if (info.Passed.HasValue)
            {
                return info.Passed.Value ? "Passed" : "Failed";
            }
            return "Unknown";
        }

        private string PrintResultsForDevice(IEnumerable<TestResultInfo> list, string deviceName)
        {
            string HTML = string.Format("<H2>{0}</H2>", deviceName);
            list.ForEach(info =>
            {
                HTML += string.Format("<H4>{0}: {1}</H4>", info.MethodInfo.Name, TestResult(info));
                HTML += info.OutputText.Replace(System.Environment.NewLine, "<BR>");
            }
            );
            HTML += "<HR>";
            return HTML;
        }

        #region IPrinterSupport Members

        /// <summary>
        /// 
        /// </summary>
        public bool HasPrintPreview
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPrintable
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Print()
        {
            brwsrHelp.ShowPrintDialog();
        }

        /// <summary>
        /// 
        /// </summary>
        public void PrintPreview()
        {
            brwsrHelp.ShowPrintPreviewDialog();
        }

        #endregion

        private void ctxScriptingHelp_Click(object sender, EventArgs e)
        {
            ctxTestResults.Checked = false;
            ShowScriptHelp();
        }

        private void mnuSaveResults_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = GuiController.CurrentScenarioFolder;
            dlg.RestoreDirectory = true;
            dlg.Filter = "HTML File (*.htm)|*.htm|Text File (*.txt)|*.txt";
            dlg.Title = "Save Results To File";
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (dlg.FilterIndex == 1)   // the FilterIndex seems to be 1-based
                {
                    System.IO.File.WriteAllText(dlg.FileName, GetResultsAsHtml());
                }
                else
                {
                    System.IO.File.WriteAllText(dlg.FileName, GetResultsAsText());
                }
            }
        }

        private void allCheckedNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = GuiController.CurrentScenarioFolder;
            dlg.Filter = "XML File (*.xml)|*.xml|Any File Type (*.*)|*.*";
            dlg.Title = "Export Results To...";

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                List<TestResult_WS> resultsForExport = new List<TestResult_WS>();
                m_Results.ResultInfos.ForEach(ri =>
                    PublishTestResultForMethod((r) => resultsForExport.Add(r), ri));
                Utilities.FileUtilities.SerializeToXml(resultsForExport, dlg.FileName, true);
            }
        }

        private void treeAvailableTests_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        private void selectedNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.InitialDirectory = GuiController.CurrentScenarioFolder;
            dlg.Filter = "XML File (*.xml)|*.xml|Any File Type (*.*)|*.*";
            dlg.Title = "Export Results To...";

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                ExportTestResultsForSelectedNode(dlg.FileName);
            }
        }

        private void exportResultsMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ExportTestResultsForSelectedNode(string filename)
        {
            List<TestResult_WS> resultsForExport = new List<TestResult_WS>();

            TestResultInfo result = (treeAvailableTests.SelectedNode.Tag as TestResultInfo);
            if (result != null)
            {
                PublishTestResultForMethod((r2) => resultsForExport.Add(r2), result);
            }
            else              // this was potentially a component/application node
            {
                AspireComponent comp = (treeAvailableTests.SelectedNode.Tag as AspireComponent);
                if (comp != null)
                {
                    // get a list of the results for tests that have been executed
                    var tests = treeAvailableTests.SelectedNode.Nodes.Cast<Node>().
                        Where(node => (node.Tag is TestResultInfo) && (node.Tag as TestResultInfo).Passed.HasValue).
                        Select(node => node.Tag as TestResultInfo);

                    tests.ForEach(r => PublishTestResultForMethod((r2) => resultsForExport.Add(r2), r));
                }
            }

            FileUtilities.SerializeToXml(resultsForExport, filename, true);
        }

        private void PublishResultsForSelectedNode()
        {
            RepositoryWebServiceSoapClient client = GetRepositoryClient();

            if (client != null)
            {
                TestResultInfo result = (treeAvailableTests.SelectedNode.Tag as TestResultInfo);
                if (result != null)
                {
                    PublishTestResultForMethod((r2) => client.PublishTestResults(r2), result);
                }
                else              // this was potentially a component/application node
                {
                    AspireComponent comp = (treeAvailableTests.SelectedNode.Tag as AspireComponent);
                    if (comp != null)
                    {
                        // get a list of the results for tests that have been executed
                        var tests = treeAvailableTests.SelectedNode.Nodes.Cast<Node>().
                            Where(node => (node.Tag is TestResultInfo) && (node.Tag as TestResultInfo).Passed.HasValue).
                            Select( node => node.Tag as TestResultInfo);

                        tests.ForEach( r => PublishTestResultForMethod( (r2) => client.PublishTestResults(r2), r ));
                    }
                }
            }
        }

        //private void PublishTestResultForMethod(Action<TestResult_WS> action, TestResultInfo result)
        //{
        //    if (result != null)
        //    {
        //        AspireComponent comp = result.Component;
        //        PublishTestResultAttribute[] publishedAsList =
        //            result.MethodInfo.GetCustomAttributes(typeof(PublishTestResultAttribute), false).
        //                Cast<PublishTestResultAttribute>().ToArray();

        //        publishedAsList.ForEach(test =>
        //        {
        //            new Action(() =>
        //            {
        //                TestResult_WS tr = new TestResult_WS()
        //                {
        //                    DeviceUid = SafeDeviceUidGetter(comp),
        //                    TestGuid = test.TestIdentifier,
        //                    TestName = result.MethodInfo.Name,
        //                    Passed = result.Passed.Value,
        //                    Comments = string.Empty,
        //                    Operator = Environment.UserName,
        //                    LoggedText = result.OutputText
        //                };

        //                action(tr);

        //                //client.PublishTestResultForComponent(SafeDeviceUidGetter(comp), test.TestIdentifier, result.MethodInfo.Name,
        //                //    result.Passed.Value, string.Empty, Environment.UserName, result.OutputText);

        //                //TestResult[] results = client.GetTestResultsByTestGuid(test.TestIdentifier);
        //                //Log.WriteLine("Previous Results for {0} ({1})",
        //                //    result.MethodInfo.Name, test.TestIdentifier);

        //                //results.ForEach( r =>
        //                //    Log.WriteLine("{0}: {1} Component {2} - {3} {4}", result.Created, result.TestId, result.ComponentId, result.Passed, result.Comments));

        //            }).ExecuteAndReportException();
        //        });
        //    }
        //}

        private void printPreviousResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RepositoryWebServiceSoapClient client = GetRepositoryClient();

            if (client != null)
            {
                try
                {
                    TestResultInfo method = (treeAvailableTests.SelectedNode.Tag as TestResultInfo);
                    if (method != null)
                    {
                        AspireComponent comp = method.Component;

                        PublishTestResultAttribute[] publishedAsList =
                            method.MethodInfo.GetCustomAttributes(typeof(PublishTestResultAttribute), false).
                                Cast<PublishTestResultAttribute>().ToArray();

                        publishedAsList.ForEach(test =>
                            {
                                TestResult[] results = client.GetTestResultsForComponentByTestId(SafeDeviceUidGetter(comp), test.TestIdentifier);
                                Log.WriteLine("Previous Results for {0} ({1})",
                                    method.MethodInfo.Name, test.TestIdentifier);

                                results.ForEach(result =>
                                    Log.WriteLine("{0}: {1} Component {2} - {3} {4}", result.Created, result.TestId, result.ComponentId, result.Passed, result.Comments));
                            });
                    }
                    else
                    {
                        AspireComponent comp = (treeAvailableTests.SelectedNode.Tag as AspireComponent);
                        if (comp != null)
                        {

                            TestResult[] results = client.GetTestResultsForComponentByDeviceUid(SafeDeviceUidGetter( comp ));
                            Log.WriteLine("Previous Results for {0} ({1})", comp.Name, SafeDeviceUidGetter( comp ));
                            results.ForEach(result =>
                                Log.WriteLine("{0}: {1} {2} {3}", result.Created, result.TestId, result.Passed, result.Comments));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLine("Error accessing test results: {0}", ex.Message);
                }
            }
        }

//        private RepositoryWebServiceSoapClient GetRepositoryClient()
//        {
//            RepositoryWebServiceSoapClient client = null;

//            try
//            {
//                client = new PnPInnovations.Aspire.BrowsingUserInterface.RepositoryWebService.RepositoryWebServiceSoapClient();
//            }
//            catch (InvalidOperationException)
//            {
//                string msg = @"Unable to create a client to connect to the repository. Most likely this means that the SDT.exe.config file is missing the necessary configuration information. Please add the following to your SDT.exe.config file and restart SDT:
//    <system.serviceModel>
//        <bindings>
//            <basicHttpBinding>
//                <binding name=""RepositoryWebServiceSoap"" closeTimeout=""00:01:00""
//                    openTimeout=""00:01:00"" receiveTimeout=""00:10:00"" sendTimeout=""00:01:00""
//                    allowCookies=""false"" bypassProxyOnLocal=""false"" hostNameComparisonMode=""StrongWildcard""
//                    maxBufferSize=""65536"" maxBufferPoolSize=""524288"" maxReceivedMessageSize=""65536""
//                    messageEncoding=""Text"" textEncoding=""utf-8"" transferMode=""Buffered""
//                    useDefaultWebProxy=""true"">
//                    <readerQuotas maxDepth=""32"" maxStringContentLength=""8192"" maxArrayLength=""16384""
//                        maxBytesPerRead=""4096"" maxNameTableCharCount=""16384"" />
//                    <security mode=""None"">
//                        <transport clientCredentialType=""None"" proxyCredentialType=""None""
//                            realm="""">
//                            <extendedProtectionPolicy policyEnforcement=""Never"" />
//                        </transport>
//                        <message clientCredentialType=""UserName"" algorithmSuite=""Default"" />
//                    </security>
//                </binding>
//            </basicHttpBinding>
//        </bindings>
//        <client>
//            <endpoint address=""http://<SERVER_URL>/RepositoryWebService.asmx""
//                binding=""basicHttpBinding"" bindingConfiguration=""RepositoryWebServiceSoap""
//                contract=""RepositoryWebService.RepositoryWebServiceSoap"" name=""RepositoryWebServiceSoap"" />
//        </client>
//    </system.serviceModel>
//";
//                Log.WriteLine("TestBench", Log.Severity.Error, msg);
//            }
//            catch (Exception ex)
//            {
//                Log.ReportException("TestBench", Log.Severity.Error, "Error creating a client to connect to the repository.", ex);
//            }

//            return client;
//        }

        private void publishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PublishResultsForSelectedNode();
        }

        private void contextMenuStripTree_Opening(object sender, CancelEventArgs e)
        {
            publishToolStripMenuItem.Enabled = false;
            printPreviousResultsToolStripMenuItem.Enabled = false;

            TestResultInfo method = (treeAvailableTests.SelectedNode.Tag as TestResultInfo);
            if (method != null)
            {
                AspireComponent comp = method.Component;
                PublishTestResultAttribute[] publishedAsList =
                    method.MethodInfo.GetCustomAttributes(typeof(PublishTestResultAttribute), false).
                        Cast<PublishTestResultAttribute>().ToArray();

                publishToolStripMenuItem.Enabled = (publishedAsList != null && publishedAsList.Length > 0 && comp != null);
                printPreviousResultsToolStripMenuItem.Enabled = (publishedAsList != null && publishedAsList.Length > 0 && comp != null);
            }
            else
            {
                publishToolStripMenuItem.Enabled = (treeAvailableTests.SelectedNode.Tag is AspireComponent);
                printPreviousResultsToolStripMenuItem.Enabled = (treeAvailableTests.SelectedNode.Tag is AspireComponent);
            }
        }

        #region Clear Results

        private void btnClearResults_Click(object sender, EventArgs e)
        {

            treeAvailableTests.Nodes.Cast<Node>().ForEach(deviceNode =>
                ClearResults(deviceNode.Nodes.Cast<Node>(), deviceNode));
            
            m_Results.Reset();
            textBox1.Text = string.Empty;
            lblTestResultsHeader.Text = string.Empty;
            lblRunResults.Text = m_Results.ToString();
            ShowTestResults();
        }

        private void ClearResults(IEnumerable<Node> nodeList, Node deviceNode)
        {
            nodeList.ForEach(node =>
                {
                    TestResultInfo info = node.Tag as TestResultInfo;
                    if (info != null)
                    {
                        info.Passed = null;
                        info.OutputText = string.Empty;
                        node.Image = Resources.unknown;
                    }
                }
            );

        }

        #endregion

        #region Drag/Drop

        private void TestBenchDocumentWindow_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void TestBenchDocumentWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(string)))
            {
                if (editor.Editor.SelectedView.Selection != null && editor.Editor.SelectedView.Selection.Length == 0)
                {
                    Point p = editor.Editor.PointToClient(new Point(e.X, e.Y));
                    editor.Editor.SelectedView.Selection.SelectRange
                        (editor.Editor.SelectedView.LocationToOffset(p, LocationToPositionAlgorithm.BestFit), 0);
                }

                editor.Editor.SelectedView.SelectedText = e.Data.GetData(typeof(string)).ToString(); // item.Path;
                editor.Editor.Focus();
            }
        }

        #endregion





    }

}
