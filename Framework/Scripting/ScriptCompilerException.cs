using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using Microsoft.CSharp;
using Microsoft.VisualBasic;

using Aspire.Utilities;

namespace Aspire.Framework.Scripting
{
	/// <summary>
	/// Exception class thrown by the ScriptObject.Compile method if a compiler error occurs.
	/// </summary>
	public class ScriptCompilerException: System.Exception
	{
		/// <summary>
		/// Create a new exception specifying the offending code and the errors.
		/// </summary>
		/// <param name="code">The code causing the compiler errors.</param>
		/// <param name="errors">The CompilerErrorCollection.</param>
		public ScriptCompilerException( string code, CompilerErrorCollection errors ): base("An error occurred compiling the code provided.")
		{
			m_code = code;
			m_errors = errors;
		}

		private string m_code = string.Empty;

		///<summary>
		///Gets the code that caused the compile errors
		///</summary>
		public string Code
		{
			get { return m_code; }
		}

		private CompilerErrorCollection m_errors = null;

		///<summary>
		///Gets the CompilerErrorCollection containing the compile errors
		///</summary>
		public CompilerErrorCollection Errors
		{
			get { return m_errors; }
		}

	}

}
