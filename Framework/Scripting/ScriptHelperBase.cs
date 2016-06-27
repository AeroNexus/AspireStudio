using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace Aspire.Framework.Scripting
{
  /// <summary>
  /// Summary description for ScriptHelperBase.
  /// </summary>
  public abstract class ScriptHelperBase : ILanguageSpecificScriptHelper
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public ScriptHelperBase()
    {
    }

    /// <summary>
    /// Gets the header text that appears before the code when wrapped
    /// </summary>
    public virtual string HeaderText
    {
      get { return string.Empty; }
    }

    /// <summary>
    /// Gets the header text that appears after the code when wrapped
    /// </summary>
    public virtual string FooterText
    {
      get { return string.Empty; }
    }

    /// <summary>
    /// String containing the compiler parameters
    /// </summary>
    protected string m_CompilerParameters = string.Empty;

    /// <summary>
    /// Number of lines appearing prior to actual code
    /// </summary>
    protected int m_PreambleLineCount = 0;

    /// <summary>
    /// Number of #reference lines removed
    /// </summary>
    protected int m_ReferenceLinesRemoved = 0;

    /// <summary>
    /// The strings that signifies a "using" or "Imports" statement
    /// </summary>
    protected string m_UsingStatement = "using ";

    /// <summary>
    /// The number of using/imports lines found in the script code
    /// </summary>
    protected int m_UsingLineCount = 0;

    /// <summary>
    /// Gets the parameters used when compiling the script code
    /// </summary>
    public virtual string CompilerParameters
    {
      get { return m_CompilerParameters; }
    }

    /// <summary>
    /// Returns the ICodeCompiler which is used by the script object to compile the script code.
    /// </summary>
    /// <returns>An ICodeCompiler.</returns>
    public abstract CodeDomProvider GetCodeCompiler();

    /// <summary>
    /// Gets the wrapped code based on default UsingLines and Code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public virtual string WrapCode(string code)
    {
      return GetWrappedCode(GetUsingLines(code), GetCleanLines(code));
    }

    /// <summary>
    /// Gets the wrapped code given a set of using lines and the original code. This will wrap the code with the code necessary to create the class and compile it.
    /// </summary>
    /// <param name="usingLines">The set of using lines.</param>
    /// <param name="code">The code as provided.</param>
    /// <returns></returns>
    public abstract string GetWrappedCode(string usingLines, string code);

    /// <summary>
    /// Gets the number of lines that occur prior to the user's code
    /// </summary>
    public virtual int PreambleLineCount
    {
      get { return m_PreambleLineCount; }
    }

    /// <summary>
    /// Given a set of code, returns the code without including any recognized pre-processor directives.
    /// </summary>
    /// <param name="code">The code to evaluate</param>
    /// <returns>The "cleaned" code, with pre-processor directives removed.</returns>
    public virtual string GetCleanLines(string code)
    {
      m_ReferenceLinesRemoved = 0;

      // strip out the #reference lines
      string[] lines = code.Split('\n');
      StringBuilder sb = new StringBuilder();
      foreach (string line in lines)
      {
        if (!LineContainsDirective(line))
        {
          sb.Append(line);
          sb.Append('\n');
        }
        else
        {
          m_ReferenceLinesRemoved++;
        }
      }
      return sb.ToString();
    }

    /// <summary>
    /// Extracts the lines containing <b>using</b> directives.
    /// </summary>
    /// <param name="code">The code to evaluate.</param>
    /// <returns>The lines that contain the <b>using</b> directive.</returns>
    public virtual string GetUsingLines(string code)
    {
      // strip out the #reference lines
      string[] lines = code.Split('\n');
      StringBuilder usingLines = new StringBuilder();
      m_UsingLineCount = 0;

      foreach (string line in lines)
      {
        if (line.TrimStart().StartsWith(m_UsingStatement))
        {
          usingLines.Append(line);
          usingLines.Append('\n');
          m_UsingLineCount++;
        }
      }

      return usingLines.ToString();
    }

    /// <summary>
    /// Specifies whether or not the line provided contains a pre-processor directive as recognized by the script object. The pre-processor directives recognized are <b>#reference</b> and <b>using</b>.
    /// </summary>
    /// <param name="line">The line of code to evaluate.</param>
    /// <returns>True if the line contains a recognized pre-processor directive; false otherwise.</returns>
    public virtual bool LineContainsDirective(string line)
    {
      if (ScriptObject.PreProcessorDirectives != null && ScriptObject.PreProcessorDirectives.Length > 0)
      {
        string[] preprocessors = ScriptObject.PreProcessorDirectives.Split(',');
        foreach (string s in preprocessors)
        {
          if (line.TrimStart().StartsWith(s))
          {
            return true;
          }
        }
      }
      return line.TrimStart().StartsWith(m_UsingStatement);
    }

    /// <summary>
    /// Gets the full class name (namespace name plus class name)
    /// </summary>
    public virtual string FullClassName
    {
      get { return string.Format("MyNamespace.MyClass"); }
    }

    /// <summary>
    /// Gets the default language type
    /// </summary>
    public virtual LanguageType Language
    {
      get { return LanguageType.Unknown; }
    }

    /// <summary>
    /// Override this property if additional assemblies are required for the helper to compile. The default implementation returns a zero-length array.
    /// </summary>
    /// <example>public override string[] ReferencedAssemblies
    /// get { return new string[] { "Aspire.Training.dll" }; }
    /// </example>
    public virtual string[] ReferencedAssemblies
    {
      get { return new string[0]; }
    }
  }

  /// <summary>
	/// A Factory class to get script helper objects
	/// </summary>
	public sealed class ScriptHelperFactory
	{
		/// <summary>
		/// The type of available helpers
		/// </summary>
		public enum HelperType
		{
			/// <summary>
			/// C#
			/// </summary>
			CSharp,
			/// <summary>
			/// VB.net
			/// </summary>
			VBnet,
			/// <summary>
			/// JScript
			/// </summary>
			JScript,
			/// <summary>
			/// Unknown
			/// </summary>
			Unknown
		}

		/// <summary>
		/// Gets a helper for the given HelperType
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILanguageSpecificScriptHelper GetHelper( HelperType type )
		{
			switch( type )
			{
				case HelperType.CSharp:
					return new CSharpScriptHelper();

        //case HelperType.VBnet:
        //  return new VBScriptHelper();

        //case HelperType.JScript:
        //  return new JScriptScriptHelper();
			}

			return null;
		}
		
		/// <summary>
		/// Gets the <see cref="HelperType"/> given a filename.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static HelperType GetHelperType( string filename )
		{
			switch( System.IO.Path.GetExtension( filename ).ToLower() )
			{
				case ".cs":
					return HelperType.CSharp;

				case ".vb":
					return HelperType.VBnet;

				case ".js":
					return HelperType.JScript;

				default:
					return HelperType.Unknown;
			}

		}

		/// <summary>
		/// Gets a script helper for the given file name. Currently based solely on the file extension.
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static ILanguageSpecificScriptHelper GetHelper( string filename )
		{
			switch( System.IO.Path.GetExtension( filename ).ToLower() )
			{
				case ".cs":
					return new CSharpScriptHelper();

        //case ".vb":
        //  return new VBScriptHelper();

        //case ".js":
        //  return new JScriptScriptHelper();

				default:
					return new CSharpScriptHelper();
			}
		}

		/// <summary>
		/// Gets the HelperType of the provided script helper
		/// </summary>
		/// <param name="helper"></param>
		/// <returns></returns>
		public static HelperType GetTypeOfHelper( ILanguageSpecificScriptHelper helper )
		{
			if( helper is CSharpScriptHelper )
			{
				return HelperType.CSharp;
			}
      //else if( helper is VBScriptHelper )
      //{
      //  return HelperType.VBnet;
      //}
      //else if( helper is JScriptScriptHelper )
      //{
      //  return HelperType.JScript;
      //}
			return HelperType.Unknown;
		}

		/// <summary>
		/// Returns a string suitable for a File Open dialog's Filter property
		/// </summary>
		/// <returns></returns>
		public static string GetFileDialogFilter()
		{
			return "All Script Files (*.cs, *.vb)|*.cs;*.vb|C# files (*.cs)|*.cs|VB.NET files (*.vb)|*.vb|JScript files (*.js)|*.js|All files (*.*)|*.*" ;
		}
	}
}
