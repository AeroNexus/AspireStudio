using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
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
	/// Summary description for ScriptObject.
	/// </summary>
	public class ScriptObject //: Aspire.Framework.ISchedulable
	{
		#region Constructors

		/// <summary>
		/// Create a new script object.
		/// </summary>
		public ScriptObject()
		{
			SetupReferenceAssemblies();
			languageHelper = new CSharpScriptHelper();
		}

		/// <summary>
		/// Create a new script object using the code provided.
		/// </summary>
		/// <param name="code">The script code to base the object upon.</param>
		public ScriptObject( string code ): this()
		{
			Code = code;
		}

		/// <summary>
		/// Create a new script object using a ScriptHelper.
		/// </summary>
		/// <param name="helper"></param>
		public ScriptObject( ILanguageSpecificScriptHelper helper ): this()
		{
			languageHelper = helper;
		}

		/// <summary>
		/// Create a new script object using the code and ILanguageSpecificScriptHelper provided.
		/// </summary>
		/// <param name="code">The script code to base the object upon.</param>
		/// <param name="helper">The language-specific helper for the ScriptObject to use</param>
		public ScriptObject( string code, ILanguageSpecificScriptHelper helper ): this( code )
		{
			languageHelper = helper;
			Code = code;
		}

		#endregion

		#region Constants

		/// <summary>
		/// The pre-processor directive for specifying additional reference assemblies
		/// </summary>
		public const string ReferencePreProcessorDirective = "#reference";

		/// <summary>
		/// The name of the Execute method
		/// </summary>
		public const string DefaultExecuteMethodName = "Execute";

		/// <summary>
		/// A comma-separated list of pre-processor directives recognized by the ScriptObject parser
		/// </summary>
		// #reference requires a space separating the directive from the reference assembly file.
		public const string PreProcessorDirectives = "#debug,#nodebug,#reference ,#define ";

		/// <summary>
		/// The #debug pre-processor directive
		/// </summary>
		public const string DebugPreprocessorDirective = "#debug";

		/// <summary>
		/// The #nodebug pre-processor directive
		/// </summary>
		public const string NoDebugPreprocessorDirective = "#nodebug";

        /// <summary>
        /// The #define pre-processor directive
        /// </summary>
        public const string PoundDefineDirective = "#define";

		#endregion

		#region Statics
		
		/// <summary>
		/// Executes all of the scripts found in the specified folder. 
		/// </summary>
		/// <param name="folder">Full path to the folder from which scripts should be loaded and executed.</param>
		/// <returns>Number of scripts successfully executed.</returns>
		public static int ExecuteAllInFolder( string folder )
		{
			if( !Directory.Exists( folder ) )
			{
				Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Debug, "Folder specified ({0}) does not exist.", folder );
				return 0;
			}

			string[] files = Directory.GetFiles(folder);
			int successful = 0;

			foreach( string file in files )
			{
				ScriptObject script = new ScriptObject();
				try
				{
					Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Debug, "Loading file {0}", file );
					script.LoadFromFile( file );
          Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Debug, "Executing file {0}", file);
					script.Execute();
          Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Debug, "Execution of file {0} was successful", file);
					successful++;
				}
				catch( ScriptCompilerException ex )
				{
          Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Warning, string.Format("Exception compiling {0}", file));
					//SimulationConsole.ReportException("ScriptObject.ExecuteAllInFolder", Severity.Debug, string.Format("Exception compiling {0}", file), ex );
					foreach( CompilerError error in ex.Errors )
					{
            Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Debug, error.ToString());
					}
				}
				catch( Exception )
				{
          Log.WriteLine("ScriptObject.ExecuteAllInFolder", Log.Severity.Warning, string.Format("Exception executing {0}", file));
				}
			}

			return successful;
		}

		private static string m_ReferencePaths = string.Empty;

		/// <summary>
		/// Set the reference path that the code compiler will use to find referenced assemblies.
		/// </summary>
		/// <param name="path">Path, separate multiple directories with a semicolon.</param>
		public static void SetReferencePath( string path )
		{
			m_ReferencePaths = path;
		}
		
		/// <summary>
		/// Gets the current ReferencePath setting
		/// </summary>
		public static string ReferencePath { get { return m_ReferencePaths; } }

		/// <summary>
		/// Creates a ScriptObject from the specified xml string. If the loadScriptFileSpecifiedInXml parameter is true, will call LoadFromFile to load the script from the file specified in the Filename property.
		/// <seealso cref="Filename"/>
		/// <seealso cref="LoadFromFile(string)"/>
		/// </summary>
		/// <param name="xmlString">The xml representing the ScriptObject to be loaded.</param>
		/// <param name="loadScriptFileSpecifiedInXml">If true, will call LoadFromFile to load the script code from the file specified in the Filename property.</param>
		/// <returns>A ScriptObject deserialized from the xml string.</returns>
		public static ScriptObject LoadScriptObjectFromXmlString( string xmlString, bool loadScriptFileSpecifiedInXml )
		{
			return DeserializeScriptObject( new System.IO.StringReader( xmlString ), loadScriptFileSpecifiedInXml );
		}

		/// <summary>
		/// Loads a ScriptObject from the specified xml file. If the loadScriptFileSpecifiedInXml parameter is true, will call LoadFromFile to load the script from the file specified in the Filename property.
		/// <seealso cref="Filename"/>
		/// <seealso cref="LoadFromFile(string)"/>
		/// </summary>
		/// <param name="xmlFilename">The xml file from which to deserialize the ScriptObject instance</param>
		/// <param name="loadScriptFileSpecifiedInXml">If true, will call LoadFromFile to load the script code from the file specified in the Filename property.</param>
		/// <returns>A ScriptObject deserialized from the specified xml file.</returns>
		public static ScriptObject LoadScriptObjectFromXmlFile( string xmlFilename, bool loadScriptFileSpecifiedInXml )
		{
			using( System.IO.TextReader reader = new System.IO.StreamReader( xmlFilename ) )
			{
				return DeserializeScriptObject( reader, loadScriptFileSpecifiedInXml );
			}
		}

		private static ScriptObject DeserializeScriptObject( System.IO.TextReader reader, bool loadScriptFileSpecifiedInXml )
		{
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer( typeof(ScriptObject) );
			ScriptObject script = serializer.Deserialize( reader as System.IO.TextReader ) as ScriptObject;

			if( script != null && loadScriptFileSpecifiedInXml )
			{
				script.LoadFromFile();
			}
			return script;
		}

		#endregion

		private ILanguageSpecificScriptHelper languageHelper = null;
		
		/// <summary>
		/// Gets the ILanguageSpecificScriptHelper for the ScriptObject
		/// </summary>
		[XmlIgnore]
		public ILanguageSpecificScriptHelper LanguageHelper 
		{
			get { return languageHelper; }
		}

		private CompilerResults compilerResults = null;

		#region Protected Methods

		/// <summary>
		/// Creates the initial set of referenced assemblies
		/// </summary>
		protected void SetupReferenceAssemblies()
		{
			referenceAssemblies.Clear();
			referenceAssemblies.Add("System.dll");
            referenceAssemblies.Add("System.Windows.Forms.dll");
			referenceAssemblies.Add("System.Xml.dll");
            referenceAssemblies.Add("Aspire.Body.dll");
			referenceAssemblies.Add("Aspire.Framework.dll");
			referenceAssemblies.Add("Aspire.Primitives.dll");
			referenceAssemblies.Add("Aspire.Utilities.dll");

			if( LanguageHelper != null && LanguageHelper.ReferencedAssemblies.Length > 0 )
			{
				foreach( string s in LanguageHelper.ReferencedAssemblies )
				{
					if( !referenceAssemblies.Contains( s ) )
					{
						referenceAssemblies.Add( s );
					}
				}
			}
		}
		
        private List<string> m_PreprocessorDirectives = new List<string>();

		/// <summary>
		/// Creates a set of referenced assemblies based on the code provided. The <b>#reference</b> pre-processor directive indicates an assembly that should be added to the list.
		/// </summary>
		/// <param name="code">The code to evaluate for #reference directives</param>
		/// <example>SetupReferenceAssemblies("#reference System.Windows.Forms.dll");</example>
		protected void StripKnownPreProcessorDirectives( string code )
		{
			// strip out the #reference lines
			string[] lines = code.Split( '\n' );
            m_PreprocessorDirectives.Clear();
            IncludeDebugInformation = false;

			foreach( string line in lines )
			{
				if( line.TrimStart().StartsWith( string.Format( "{0} ", ScriptObject.ReferencePreProcessorDirective ) ) )
				{
                    string referenceName = line.TrimStart().Substring(ScriptObject.ReferencePreProcessorDirective.Length + 1).Trim();
                    if (referenceName.Length == 0) continue;
                    if (!referenceAssemblies.Contains( referenceName ))
                    {
                        referenceAssemblies.Add( referenceName );
                    }

                    //string[] referenceLine = line.Split( ' ' );
                    //if( referenceLine.Length == 0 )
                    //{
                    //    continue;
                    //}
                    //if( !referenceAssemblies.Contains( referenceLine[ referenceLine.Length - 1 ].Trim() ) )
                    //{
                    //    referenceAssemblies.Add( referenceLine[ referenceLine.Length - 1 ].Trim() );
                    //}
				}
				else if( line.TrimStart().StartsWith( ScriptObject.DebugPreprocessorDirective ) )
				{
					IncludeDebugInformation = true;
				}
				else if( line.TrimStart().StartsWith( ScriptObject.NoDebugPreprocessorDirective ) )
				{
					IncludeDebugInformation = false;
				}
                else if (line.TrimStart().StartsWith(ScriptObject.PoundDefineDirective))
                {
                    m_PreprocessorDirectives.Add(
                        line.Substring(ScriptObject.PoundDefineDirective.Length + 1).Trim());
                }
			}
		}

		#endregion

		#region Public Properties
		
		/// <summary>
		/// Returns a boolean indicating whether anything is listening to the <see cref="SaveRequested"/> event.
		/// </summary>
		public bool HasSaveRequestedHandler
		{
			get { return SaveRequested != null; }
		}

		/// <summary>
		/// Returns a boolean indicating that the Execute() method is defined.
		/// </summary>
		public bool HasExecute
		{
			get { return m_HasExecute; }
		} bool m_HasExecute;
		

		/// <summary>
		/// Gets and sets the HelperType of the script object. This is used when deserializing the script to create the proper script helper.
		/// </summary>
		[System.ComponentModel.DefaultValue(ScriptHelperFactory.HelperType.CSharp)]
		public ScriptHelperFactory.HelperType HelperType
		{
			get { return ScriptHelperFactory.GetTypeOfHelper( LanguageHelper ); }
			set
			{
				languageHelper = ScriptHelperFactory.GetHelper( value );
				m_CompleteCode = WrapCode( originalCode );
				m_IsDirty = true;
			}
		}

		/// <summary>
		/// Gets and sets an XML element representing the code of the script object. This mechanism is used to wrap the script code within a CDATA section to avoid encoding special characters.
		/// </summary>
		[System.Xml.Serialization.XmlAnyElement("Code")]
		public System.Xml.XmlElement CodeElement
		{
			get
			{
				System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
				System.Xml.XmlElement elem = doc.CreateElement( "Code" );
				System.Xml.XmlCDataSection cdata = doc.CreateCDataSection( Code );
				elem.AppendChild( cdata );
				return elem;
			}

			set
			{
				Code = value.InnerText;
			}
		}

		private string originalCode = string.Empty;
		
		/// <summary>
		/// Gets and sets the code for the script
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public string Code
		{
			get { return(originalCode); }
			set
			{
				originalCode = value;
				m_CompleteCode = WrapCode( originalCode );
				m_IsDirty = true;

				if( CodeChanged != null )
				{
					CodeChanged( this, EventArgs.Empty );
				}
			}
		}
		
		private string m_CompleteCode = string.Empty;
		
		/// <summary>
		/// Gets the "complete" code. This is the code from the script wrapped with the class wrappers necessary to compile the script.
		/// </summary>
		[XmlIgnore()]
		public string CompleteCode
		{
			get { return(m_CompleteCode); }
		}
		
		private string m_Filename = string.Empty;

		/// <summary>
		/// Gets and sets the filename for the script object's code.
		/// <seealso cref="LoadFromFile(string)"/>
		/// </summary>
		public string Filename
		{
			get { return(m_Filename); }
			set { m_Filename = value; }
		}

		private bool m_IncludeDebugInformation = false;

		///<summary>
		///Gets and sets whether or not debug information is included in the compiled assembly.
		///<seealso cref="CompilerParameters.IncludeDebugInformation"/>
		///</summary>
		[System.ComponentModel.DefaultValue(false)]
		public bool IncludeDebugInformation
		{
			get { return m_IncludeDebugInformation; }
			set { m_IncludeDebugInformation = value; }
		}

		private bool m_IsDirty = false;

		/// <summary>
		/// Gets whether the code has been updated since it was last compiled
		/// </summary>
		[XmlIgnore()]
		public bool IsDirty
		{
			get { return(m_IsDirty); }
		}

		private string m_AssemblyPath = string.Empty;

		///<summary>
		///Gets and sets the value for AssemblyPath
		///</summary>
		public string AssemblyPath
		{
			get { return m_AssemblyPath; }
		}

		private Assembly compiledAssembly = null;

		/// <summary>
		/// Gets the compiled assembly
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		public Assembly CompiledAssembly
		{
			get { return(compiledAssembly); }
		}

		/// <summary>
		/// The compiled object instance. This is the instance on which methods are invoked.
		/// </summary>
		private object compiledObject = null;
		
		/// <summary>
		/// Gets the compiled script as an object. The object will implement IScriptObjectImplementation.
		/// <seealso cref="IScriptObjectImplementation"/>
		/// </summary>
		[XmlIgnore()]
		public object CompiledObject
		{
			get { return(compiledObject); }
		}

		/// <summary>
		/// Collection of referenced assemblies
		/// </summary>
		private StringCollection referenceAssemblies = new StringCollection();

		/// <summary>
		/// Gets the set of assemblies referenced by the script. This is set up with an initial set of assemblies and assemblies are added when the Code property is set. To add an assembly in the script's code, use the #reference pre-compiler directive.
		/// </summary>
		/// <example>#reference System.Drawing.dll</example>
		[XmlIgnore()]
		public StringCollection ReferencedAssemblies
		{
			get { return(referenceAssemblies); }
		}
		

		#endregion

		#region Public Methods

		/// <summary>
		/// Load the code from an external file. Sets the Code property to the text contained in the file.
		/// <seealso cref="Code"/>
		/// </summary>
		/// <param name="filename">The full path to the code file.</param>
		public void LoadFromFile( string filename )
		{
			m_Filename = filename;
			LoadFromFile();
		}
		
		/// <summary>
		/// Load the code from the external file specified by the Filename property. Sets the Code property to the text contained in the file.
		/// <seealso cref="Code"/>
		/// </summary>
		public void LoadFromFile()
		{
			if( !File.Exists(Filename) )
				throw new FileNotFoundException( string.Format("Script file {0} does not exist.", Filename), Filename );

			using( System.IO.TextReader reader = new System.IO.StreamReader( Filename ) )
			{
				Code = reader.ReadToEnd();
			}
		}

		/// <summary>
		/// Saves the code to the external file specified by the Filename property. Raises the SaveRequested event if Filename is empty; raises the SavePerformed event when save is successful
		/// </summary>
		public void SaveToFile()
		{
			if( Filename.Length == 0 )
			{
				if( SaveRequested != null )
				{
					SaveRequested( this, EventArgs.Empty );
					return;
				}
				else
				{
					throw new InvalidOperationException( "Cannot save the ScriptObject Code because Filename is empty and there are no save request handlers." );
				}
			}

			using( System.IO.TextWriter writer = new System.IO.StreamWriter( Filename, false ) )
			{
				writer.Write( Code );
				if( SavePerformed != null )
				{
					SavePerformed( this, EventArgs.Empty );
				}
			}		
		}

		/// <summary>
		/// Wrap the script code with the code required to completely compile the script. This also populates the referenceAssemblies collection.
		/// </summary>
		/// <param name="lcCode">The code to wrap.</param>
		/// <returns>The script code wrapped by the code necessary to compile.</returns>
		/// <exception cref="InvalidOperationException">InvalidOperationException is thrown if the language-specific helper object has not been initialized.</exception>
		public string WrapCode( string lcCode )
		{
			if( languageHelper == null )
			{
				throw new InvalidOperationException( "The language-specific helper instance has not been initialized." );
			}
			SetupReferenceAssemblies();			
			StripKnownPreProcessorDirectives( lcCode );
			return languageHelper.WrapCode( lcCode );
		}
		
		// Loads the content of a file to a byte array. 
		private byte[] LoadRawFile(string filename) 
		{
			FileStream fs = new FileStream(filename, FileMode.Open);
			byte[] buffer = new byte[(int) fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			fs.Close();
   
			return buffer;
		}   

		/// <summary>
		/// Compiles the code.
		/// </summary>
		/// <exception cref="ScriptCompilerException">ScriptCompilerException is thrown if the compilation generates errors</exception>
		/// <exception cref="InvalidOperationException">InvalidOperationException is thrown if the language-specific helper object has not been initialized.</exception>
		public void Compile()
		{
			m_Methods.Clear();

			compilerResults = Compile( CompleteCode );
			if( compilerResults != null )
			{
				if( IncludeDebugInformation && AssemblyPath.Length != 0 )
				{
					string pdbPath = Path.GetFileNameWithoutExtension( AssemblyPath );
					pdbPath = Path.Combine( Path.GetDirectoryName( AssemblyPath ), pdbPath ) + ".pdb";
					
					byte[] rawAssembly = LoadRawFile(AssemblyPath);

                    byte[] rawSymbolStore = null;
                    try
                    {
                        rawSymbolStore = LoadRawFile(pdbPath);
                    }
                    catch 
                    {
                      Log.WriteLine("ScriptObject", Log.Severity.Warning, "Unable to load symbols file for this script.");
                    }
					compiledAssembly = AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
				}
				else
				{
					compiledAssembly = compilerResults.CompiledAssembly;
				}
				if( compiledAssembly != null )
				{
					compiledObject = compiledAssembly.CreateInstance( languageHelper.FullClassName );		
					if( compiledObject is ScriptObjectImplementationBase )
					{
						ScriptObjectImplementationBase baseObj = compiledObject as ScriptObjectImplementationBase;
						baseObj.Debuggable = IncludeDebugInformation;
					}

                    m_HasExecute = compiledObject != null && compiledObject.GetType() != null &&
                                    compiledObject.GetType().GetMethod("Execute") != null;
				}
				m_IsDirty = false;

				if( ScriptCompiledSuccessfully != null )
				{
					ScriptCompiledSuccessfully( this, EventArgs.Empty );
				}
			}
		}

		/// <summary>
		/// Destructor. Removes any temporary files used when compiling with <see cref="IncludeDebugInformation"/> set to true.
		/// </summary>
		~ScriptObject()
		{
			if( AssemblyPath != null && AssemblyPath.Length != 0 )
			{
				string path = Path.GetDirectoryName( AssemblyPath );
				string pdbPath = Path.GetFileNameWithoutExtension( AssemblyPath );
				string codePath = Path.Combine( path, pdbPath );
				pdbPath = Path.Combine( path, pdbPath ) + ".pdb";
				try
				{
					File.Delete( pdbPath );
				}
				catch {}
				try
				{
					File.Delete( AssemblyPath );
				}
				catch {}
				try
				{
					File.Delete( codePath );
				}
				catch {}
				try
				{
					Directory.Delete( path );
				}
				catch {}
			}
		}

        private CompilerResults Compile(string lcCode)
        {
            return Compile(lcCode, false);
        }

		/// <summary>
		/// Compile a piece of code using the reference assemblies collection
		/// </summary>
		/// <param name="lcCode">The code to compile.</param>
		/// <param name="alreadyFixedReferences">WHether or not the references have already been satisfied.</param>
		/// <returns>The CompilerResults returned by the ICodeCompiler.</returns>
		/// <exception cref="ScriptCompilerException">ScriptCompilerException is thrown if the compilation generates errors</exception>
		/// <exception cref="InvalidOperationException">InvalidOperationException is thrown if the language-specific helper object has not been initialized.</exception>
		private CompilerResults Compile( string lcCode, bool alreadyFixedReferences )
		{
			if( languageHelper == null )
			{
				throw new InvalidOperationException( "The language-specific helper instance has not been initialized." );
			}
			
			CodeDomProvider loCompiler = languageHelper.GetCodeCompiler();
			CompilerParameters loParameters = new CompilerParameters();

			//loParameters.Evidence = Assembly.GetCallingAssembly().Evidence;

			//loParameters.CompilerOptions = languageHelper.CompilerParameters;

            m_PreprocessorDirectives.ForEach(pound_define =>
                { loParameters.CompilerOptions += string.Format(" /D:{0}", pound_define); });

			// *** Start by adding any referenced assemblies
			
			foreach( string s in referenceAssemblies )
			{
				loParameters.ReferencedAssemblies.Add( s );
			}

			// *** Load the resulting assembly into memory
			loParameters.GenerateInMemory = true;
			loParameters.IncludeDebugInformation = IncludeDebugInformation;

			string currentDir = System.Environment.CurrentDirectory;
			// *** Now compile the whole thing
			CompilerResults loCompiled = null;

			try {

                // cre 06/02/2010
                // the .Net compilers will use the Environment.CurrentDirectory folder as the
                // first place to look for referenced assemblies, so we need to make sure that
                // this points to the current bin folder, and not potentially an older SDT bin folder...
				System.Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

				if (IncludeDebugInformation) {
					string fullname = m_AssemblyPath;
					string path = fullname.Length != 0 ? Path.GetDirectoryName(fullname) : string.Empty;
					if (path.Length == 0) {
						path = Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
						if (!Directory.Exists(path)) {
							Directory.CreateDirectory(path);
						}
						fullname = Path.Combine(path, "script.tmp");
					}

					string filename = System.IO.Path.GetFileNameWithoutExtension(fullname);
					using (System.IO.TextWriter tw = new System.IO.StreamWriter(System.IO.Path.Combine(path, filename))) {
						tw.Write(lcCode);
					}

					loParameters.CompilerOptions += " /D:DEBUG /debug+ /optimize- /w:4  /unsafe ";
					m_AssemblyPath = System.IO.Path.Combine(path, filename) + ".dll";
					loParameters.OutputAssembly = m_AssemblyPath;
					loParameters.GenerateInMemory = false;
					loCompiled = loCompiler.CompileAssemblyFromFile(loParameters, System.IO.Path.Combine(path, filename));
				} else {
					loCompiled = loCompiler.CompileAssemblyFromSource(loParameters, lcCode);
				}
			} finally 
			{
				System.Environment.CurrentDirectory = currentDir;
			}

			if (loCompiled != null && loCompiled.Errors != null && loCompiled.Errors.HasErrors) 
			{
				CompilerErrorCollection errorCollection = new CompilerErrorCollection(loCompiled.Errors);
                
                bool hasReferenceError = false;

				// offset the line numbers
				foreach( CompilerError error in errorCollection )
				{
                    if( !alreadyFixedReferences && 
                        (HelperType == ScriptHelperFactory.HelperType.CSharp && error.ErrorNumber == "CS0006" ) ||
                        (HelperType == ScriptHelperFactory.HelperType.VBnet && error.ErrorNumber == "BC2017"))
                    {
                        hasReferenceError = true;
                        string assembly =
                            loCompiled.Errors[0].ErrorText.Substring(
                                loCompiled.Errors[0].ErrorText.IndexOf("'") + 1,
                                loCompiled.Errors[0].ErrorText.LastIndexOf("'") - (loCompiled.Errors[0].ErrorText.IndexOf("'") + 1));
                        referenceAssemblies.Remove(assembly);
                        referenceAssemblies.Add(GetFullReference(assembly));
                    }

					error.Line -= (LanguageHelper.PreambleLineCount - 1);
					if( error.Line <= 0 )
					{
						error.Line = 1;
					}
				}
                if (!alreadyFixedReferences && hasReferenceError)
                {
                    loCompiled = Compile(lcCode, true);
                }
                else
                {
                    throw new ScriptCompilerException(lcCode, errorCollection);
                }
			}

			return loCompiled;
		}

        private string GetFullReference(string relativeReference)
        {
            // First, get the path for this executing assembly.
            Assembly a = Assembly.GetCallingAssembly();

            string path = Path.GetDirectoryName(a.Location);
            // if the file exists in this Path - prepend the path
            string fullReference = Path.Combine(path, relativeReference);
            if (File.Exists(fullReference))
                return fullReference;
            else
            {
                // Strip off any trailing ".dll" if present.
                if
                (string.Compare(relativeReference.Substring(relativeReference.Length - 4),
                ".dll", true) == 0)
                    fullReference = relativeReference.Substring(0,
                    relativeReference.Length - 4);
                else
                    fullReference = relativeReference;

                // See if the required assembly is already present in our current AppDomain
                foreach (Assembly currAssembly in
                AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (string.Compare(currAssembly.GetName().Name, fullReference,
                    true) == 0)
                    {
                        // Found it, return the location as the full reference.
                        return currAssembly.Location;
                    }
                }

                // The assembly isn't present in our current application, so attempt to
                // load it from the GAC, using the partial name.
                try
                {
                    Assembly tempAssembly =
                    //Assembly.LoadWithPartialName(fullReference);
                    Assembly.Load(fullReference);
                    return tempAssembly.Location;
                }
                catch
                {
                    // If we cannot load or otherwise access the assembly from the GAC then just
                    // return the relative reference and hope for the best.
                    return relativeReference;
                }
            }

        }

		#region ISchedulable

		/// <summary>
		/// Access the flag that prevents execution.
		/// </summary>
		[XmlIgnore]
		public bool DontExecute
		{
			get { return dontExecute; }
			set { dontExecute = value; }
		} bool dontExecute;

		/// <summary>
		/// Calls the Execute method on the script
		/// </summary>
		/// <exception cref="MissingMethodException">MissingMethodException is thrown if the script code does not contain an Execute() method.</exception>
		public void Execute()
		{
			Execute( ScriptObject.DefaultExecuteMethodName );
		}
		
		/// <summary>
		/// Alternate form of Execute that is passed in its stepSize
		/// </summary>
		public virtual void Execute( double deltaTime ){}
		/// <summary>
		/// ExecutePeriodic is called from the scheduler at regular 
		/// simulation time intervals
		/// </summary>
		public void ExecutePeriodic(){ Execute(schedulePeriod); }
		double schedulePeriod = 0;

		///<summary>
		///Access the tag
		///</summary>
		[XmlIgnore()]
		public int Tag
		{
			get{ return tag; }
			set{ tag = value; }
		} int tag = -1;

		#endregion

		private Hashtable m_Methods = new Hashtable();

		/// <summary>
		/// Executes the method specified by methodName. If the method does not exist, MissingMethodException is thrown.
		/// </summary>
		/// <param name="methodName">The name of the method to execute.</param>
		/// <param name="args">Optional arguments to pass to the method.</param>
		/// <exception cref="MissingMethodException">MissingMethodException is thrown if the method specified by the methodName parameter does not exist in the script code.</exception>
		public object Execute( string methodName, params object[] args )
		{
			if( compilerResults == null || compiledObject == null || IsDirty )
			{
				Compile();
			}
			
			if (compiledObject == null) 
			{
				throw new InvalidOperationException( "Compiled script is not valid." );
			}
			
			MethodInfo mi = m_Methods[methodName] as MethodInfo;

			if( mi == null )
			{
				mi = compiledObject.GetType().GetMethod( methodName );
				m_Methods[methodName] = mi;
			}
			if ( mi == null ) return null;
			if ( args == null || args.Length == 0 )
				return mi.Invoke( compiledObject, null );
			else
			{
				return mi.Invoke( compiledObject, args );
			}
//			object loResult = compiledObject.GetType().InvokeMember( methodName,
//				BindingFlags.InvokeMethod,null,compiledObject,null);
//			return loResult;
		}

		#endregion

		#region Events Raised

		/// <summary>
		/// Event raised when the Code property value changes
		/// </summary>
		public event EventHandler CodeChanged;

		/// <summary>
		/// Event raised when the script was successfully compiled
		/// </summary>
		public event EventHandler ScriptCompiledSuccessfully;

		/// <summary>
		/// Event raised when the SaveToFile method is called but <see cref="Filename"/> is not set. This is the case if some other persisted object contains a reference to a ScriptObject.
		/// </summary>
		public event EventHandler SaveRequested;

		/// <summary>
		/// Event raised when the ScriptObject has saved the code to the file specified by the <see cref="Filename"/> property.
		/// </summary>
		public event EventHandler SavePerformed;

		#endregion
	}

	/// <summary>
	/// IScriptObjectImplementation is the interface that will be implemented by script objects when they are compiled.
	/// The implementation will be built for the script code; the script writer does not have to enter the code. The properties
	/// will be available to the script code using the names provided.
	/// </summary>
	public interface IScriptObjectImplementation
	{
		/// <summary>
		/// The currently-loaded Configuration object
		/// </summary>
		Scenario LoadedScenario
		{ get; set; }

		/// <summary>
		/// A generic object instance the script can use for whatever.
		/// </summary>
		object Tag
		{ get; set; }

		/// <summary>
		/// Gets and sets the publisher
		/// </summary>
		IPublishable Parent
		{ get; set; }
	}
	
	/// <summary>
	/// Enumerates the available language types
	/// </summary>
	public enum LanguageType
	{
		/// <summary>
		/// XML
		/// </summary>
		Xml,
		/// <summary>
		/// C#
		/// </summary>
		Csharp,
		/// <summary>
		/// VB.net
		/// </summary>
		VB,
		/// <summary>
		/// JScript
		/// </summary>
		JScript,
		/// <summary>
		/// Unknown/other
		/// </summary>
		Unknown
	}

	/// <summary>
	/// Indicates that the class contains a ScriptObject
	/// </summary>
	/// <seealso cref="ScriptObject"/>
	public interface IHasScriptObject
	{
		/// <summary>
		/// Gets and sets the ScriptObject
		/// </summary>
		ScriptObject ScriptObject { get; set; }
	}
}
