using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Aspire.Studio.DocumentViews;
using Aspire.Framework.Scripting;
using Aspire.Utilities;

namespace Aspire.BrowsingUI.TestBench
{
	/// <summary>
	/// Define the contents of a TestBench contents file
	/// </summary>
  public class TestBenchDocument : StudioDocumentEx, INotifyPropertyChanged, IPropertyBagProvider
    {
		/// <summary>
		/// Default constructor
		/// </summary>
        public TestBenchDocument()
        {
            InitializeProperties();
            ScriptObject = new ScriptObject(GetScriptHelper());
            ScriptObject.Code = @"
[Test]
public void TestMethod()
{

}
";
        }

    	  public ScriptObject ScriptObject{ get; set; }

        public override StudioDocumentView NewView(string name)
        {
          return new CodeEditor();
        }

		/// <summary>
		/// Get the language script helper
		/// </summary>
		/// <returns></returns>
        //public override Aspire.Framework.Scripting.ILanguageSpecificScriptHelper GetScriptHelper()
        public Aspire.Framework.Scripting.ILanguageSpecificScriptHelper GetScriptHelper()
        {
          return new TestBenchScriptObjectHelper();
        }

        #region IPropertyBagProvider Members
        
        private const string ScriptCodeProp = "Script Code";

        private void InitializeProperties()
        {
            if (m_PropertyBag != null)
            {
                m_PropertyBag.GetValue -= new PropertySpecEventHandler(m_PropertyBag_GetValue);
                m_PropertyBag.SetValue -= new PropertySpecEventHandler(m_PropertyBag_SetValue);
            }

            m_PropertyBag = new PropertyBag();
           
            //PropertySpec scriptProp = new PropertySpec(ScriptCodeProp, typeof(ScriptObject), "Misc",
            //            "The script code associated with the test bench.", ScriptObject, 
            //            typeof(DocumentWindowScriptCodeTypeEditor), typeof(ClickToEditNonExpandableTypeConverter));

            //m_PropertyBag.Properties.Add(scriptProp);

            m_PropertyBag.Properties.Add( new PropertySpec("Test Suite ID", typeof(string), "Misc", "The identifier for the test suite. Used in storing results for the test."));
            m_PropertyBag.Properties.Add(new PropertySpec("Caption", typeof(string), "Misc", "The caption/title for the testbench window."));
            
            m_PropertyBag.GetValue += new PropertySpecEventHandler(m_PropertyBag_GetValue);
            m_PropertyBag.SetValue += new PropertySpecEventHandler(m_PropertyBag_SetValue);
        }
        
        private void m_PropertyBag_GetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Caption":
                    e.Value = this.Caption;
                    break;

                case "Test Suite ID":
                    e.Value = this.DocumentId;
                    break;

                case ScriptCodeProp:
                    e.Value = this.ScriptObject;
                    break;
            }
        }

        private void m_PropertyBag_SetValue(object sender, PropertySpecEventArgs e)
        {
            switch (e.Property.Name)
            {
                case "Caption":
                    this.Caption = e.Value.ToString();
                    break;

                case "Test Suite ID":
                    this.DocumentId = e.Value.ToString();
                    RaisePropChangedEvent("Test Suite ID");
                    break;

                case ScriptCodeProp:

                    if (!this.Code.Equals((e.Value as ScriptObject).Code))
                    {
                        this.Code = (e.Value as ScriptObject).Code;
                        RaisePropChangedEvent("Code");
                    }
                    break;
            }
        }

        private string m_Caption = string.Empty;

        /// <summary>
        /// Gets and sets the caption for the window.
        /// </summary>
        public string Caption
        {
            get { return m_Caption; }
            set
            {
                if (!m_Caption.Equals(value))
                {
                    m_Caption = value;
                    RaisePropChangedEvent("Caption");
                }
            }
        }

        public string Code { get; set; }

        private void RaisePropChangedEvent(string propName)
        {
          Notify(propName);
        }

        private PropertyBag m_PropertyBag = new PropertyBag();

		/// <summary>
		/// Access the property bag
		/// </summary>
        public PropertyBag ThePropertyBag
        {
            get { return m_PropertyBag; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        #endregion
    }
}
