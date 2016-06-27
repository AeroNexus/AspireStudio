using System.CodeDom.Compiler;

namespace Aspire.Framework.Scripting
{
  /// <summary>
  /// ILanguageSpecificScriptHelper is implemented by ScriptObject language helpers. These are language-specific classes which provide language-specific functionality.
  /// </summary>
  public interface ILanguageSpecificScriptHelper
  {
    /// <summary>
    /// Gets the header text that appears before the code when wrapped
    /// </summary>
    string HeaderText { get; }

    /// <summary>
    /// Gets the header text that appears after the code when wrapped
    /// </summary>
    string FooterText { get; }

    /// <summary>
    /// Wrap the provided code with the code necessary to compile and execute the code.
    /// </summary>
    /// <param name="code">The code to wrap</param>
    /// <returns>Wrapped, compilable code.</returns>
    string WrapCode(string code);

    /// <summary>
    /// Gets the number of lines prior to the user's actual code. This will be the number of lines added to the script
    /// </summary>
    int PreambleLineCount { get; }

    /// <summary>
    /// Given a set of code, returns the code without including any pre-processor directives recognized by the language-specific helper.
    /// </summary>
    /// <param name="code">The code to evaluate</param>
    /// <returns>The "cleaned" code, with pre-processor directives removed.</returns>
    string GetCleanLines(string code);

    /// <summary>
    /// Returns the ICodeCompiler which is used by the script object to compile the script code.
    /// </summary>
    /// <returns>An ICodeCompiler.</returns>
    CodeDomProvider GetCodeCompiler();

    /// <summary>
    /// Gets the CompilerParameters for the script object to use when compiling
    /// </summary>
    string CompilerParameters { get; }

    /// <summary>
    /// Extracts the lines containing <b>using</b> directives.
    /// </summary>
    /// <param name="code">The code to evaluate.</param>
    /// <returns>The lines that contain the <b>using</b> directive.</returns>
    string GetUsingLines(string code);

    /// <summary>
    /// Specifies whether or not the line provided contains a pre-processor directive as recognized by the language-specific helper.
    /// </summary>
    /// <param name="line">The line of code to evaluate.</param>
    /// <returns>True if the line contains a recognized pre-processor directive; false otherwise.</returns>
    bool LineContainsDirective(string line);

    /// <summary>
    /// Gets the full class name of the class created
    /// </summary>
    string FullClassName { get; }

    /// <summary>
    /// Gets the LanguageType that the helper provides
    /// </summary>
    LanguageType Language { get; }

    /// <summary>
    /// Gets any additional assembly references the script helper requires
    /// </summary>
    string[] ReferencedAssemblies { get; }
  }
}
