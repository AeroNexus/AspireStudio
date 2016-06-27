using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Framework.Scripting;
//using StarTechnologies.GuiUtilities.UiTypeEditors;
//using StarTechnologies.GuiUtilities;

namespace Aspire.BrowsingUI.TestBench
{
    /// <summary>
    /// TestBenchScriptObjectHelper is an ILanguageSpecificScriptHelper.
    /// </summary>
    public class TestBenchScriptObjectHelper : CSharpScriptHelper
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public TestBenchScriptObjectHelper(): base()
        {
        }

        private string[] m_ReferencedAssemblies = new string[] 
                { "System.Core.dll", "StarTechnologies.GuiUtilities.dll", "NUnit.Framework.dll", 
                    "AeroNexus.Aspire.BrowsingUserInterface.dll", "AeroNexus.Aspire.Core.dll", "AeroNexus.Aspire.Components.dll" };

        /// <summary>
        /// Get the assemblies referenced by this script
        /// </summary>
        public override string[] ReferencedAssemblies
        {
            get
            {
                return m_ReferencedAssemblies;
            }
        }

		/// <summary>
		/// Access the class name
		/// </summary>
        public override string FullClassName
        {
            get
            {
                return "TestBench.TestFixture";
            }
        }

        /// <summary>
        /// Returns the ICodeCompiler which is used by the script object to compile the script code.
        /// </summary>
        /// <returns>An ICodeCompiler.</returns>
        public override System.CodeDom.Compiler.CodeDomProvider GetCodeCompiler()
        {
            Dictionary<string, string> providerOptions = new Dictionary<string, string>();
            providerOptions.Add("CompilerVersion", "v3.5");  // Requires a prefix of 'v'?
            return new Microsoft.CSharp.CSharpCodeProvider(providerOptions);
            //return new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
        }

        //protected string m_HeaderText = string.Empty;

        /// <summary>
        /// Gets the header text used to wrap the script into a usable object
        /// </summary>
        public override string HeaderText
        {
            get
            {
                return m_HeaderText;
            }
        }

        /// <summary>
        /// Gets the footer text used to wrap the script into a usable object
        /// </summary>
        public override string FooterText
        {
            get
            {
                return "}    }";
            }
        }

        /// <summary>
        /// Gets the wrapped code given a set of using lines and the original code. This will wrap the code with the code necessary to create the class and compile it.
        /// </summary>
        /// <param name="usingLines">The set of using lines.</param>
        /// <param name="code">The code as provided.</param>
        /// <returns></returns>
        public override string GetWrappedCode(string usingLines, string code)
        {
            string codePreamble = @"
using System;
using System.IO;
using System.Linq;
using StarTechnologies.Components;
using StarTechnologies.Framework;
using StarTechnologies.Framework.Scripting;
using StarTechnologies.GuiUtilities;
using StarTechnologies.Primitives;
using StarTechnologies.Utilities;
using StarTechnologies.MathLib;
using NUnit.Framework;
using AeroNexus.Aspire;
using AeroNexus.Aspire.Components;
using AeroNexus.Aspire.TestBench;
";
            codePreamble += usingLines;
            codePreamble += @"
namespace TestBench {
public class TestFixture : TestBenchScriptObject {

";
            m_PreambleLineCount = codePreamble.Split('\n').Length - m_ReferenceLinesRemoved;// - m_UsingLineCount;
            m_HeaderText = codePreamble;

            string lcCode = codePreamble + code + @"
			}    }";

            return lcCode;

        }

        /// <summary>
        /// Gets the LanguageType for this helper
        /// </summary>
        public override LanguageType Language
        {
            get { return LanguageType.Csharp; }
        }
    }


}
