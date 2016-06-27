using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Aspire.Framework.Scripting
{
	/// <summary>
	/// CSharpScriptHelper is the ILanguageSpecificScriptHelper used by ScriptObject when dealing with C# scripts.
	/// </summary>
	public class CSharpScriptHelper: ScriptHelperBase
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public CSharpScriptHelper()
		{
			if( ScriptObject.ReferencePath.Length != 0 )
			{
                string m_AdditionalLibPaths = string.Empty;
                //PlugIn.Manager.Mgr().ManifestList.ForEach( manifest =>
                //    {
                //        m_AdditionalLibPaths += string.Format(@"""/lib:{0}"" ",
                //            System.IO.Path.GetFullPath( manifest.Path ) );
                //    }
                //);

				m_CompilerParameters = string.Format( @"""/lib:{0}"" ""/lib:{0}\Project"" {1} /optimize- /w:4  /unsafe",
					ScriptObject.ReferencePath, m_AdditionalLibPaths );
			}
			m_UsingStatement = "using ";
            
		}

        /// <summary>
        /// Gets the parameters to use with the compiler
        /// </summary>
        public override string CompilerParameters
        {
            get
            {
                if (ScriptObject.ReferencePath.Length != 0)
                {
                    string m_AdditionalLibPaths = string.Empty;
                    //PlugIn.Manager.Mgr().ManifestList.ForEach(manifest =>
                    //{
                    //    m_AdditionalLibPaths += string.Format(@"""/lib:{0}"" ",
                    //        System.IO.Path.GetFullPath(manifest.Path));
                    //}
                    //);

                    m_CompilerParameters = string.Format(@"""/lib:{0}"" ""/lib:{0}\Project"" {1} /optimize- /w:4  /unsafe",
                        ScriptObject.ReferencePath, m_AdditionalLibPaths);
                }

                return m_CompilerParameters;
            }
        }
        private string[] m_ReferencedAssemblies = new string[] { "System.Core.dll", "WindowsBase.dll" };

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
		/// Returns the ICodeCompiler which is used by the script object to compile the script code.
		/// </summary>
		/// <returns>An ICodeCompiler.</returns>
		public override CodeDomProvider GetCodeCompiler()
		{
            Dictionary<string,string> providerOptions = new Dictionary<string,string>();
            providerOptions.Add ("CompilerVersion", "v3.5");  // Requires a prefix of 'v'?
            return new CSharpCodeProvider(providerOptions);
            //return new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
		}
		
		/// <summary>
		/// Gets the header text used to wrap the script into a usable object
		/// </summary>
		protected string m_HeaderText = string.Empty;

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
		public override string GetWrappedCode( string usingLines, string code )
		{
			string codePreamble = @"
using System;
using System.IO;
using Aspire.Framework;
using Aspire.Framework.Scripting;
using Aspire.Primitives;
using Aspire.Utilities;
";
			codePreamble += usingLines;
			codePreamble += @"
namespace MyNamespace {
public class MyClass : ScriptObjectImplementationBase {
";
			m_PreambleLineCount = codePreamble.Split( '\n' ).Length - m_ReferenceLinesRemoved;// - m_UsingLineCount;
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
