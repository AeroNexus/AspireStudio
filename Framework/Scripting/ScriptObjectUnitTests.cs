using System;
using System.CodeDom.Compiler;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Aspire.Framework;

namespace Aspire.Framework.Scripting
{
	/// <summary>
	/// Summary description for ScriptObjectUnitTests.
	/// </summary>
	//[TestFixture]
	public class ScriptObjectUnitTests
	{
		
		private ScriptObject m_ScriptObject;

		/// <summary>
		/// Setup method
		/// </summary>
		//[SetUp]
		public void Setup()
		{
			Blackboard.Clear();
			m_ScriptObject = new ScriptObject();
			scriptCompiledSuccessfullyReceived = false;
			codeChangedReceived = false;
			m_ScriptObject.CodeChanged += new EventHandler(m_ScriptObject_CodeChanged);
			m_ScriptObject.ScriptCompiledSuccessfully += new EventHandler(m_ScriptObject_ScriptCompiledSuccessfully);
		}
	
		#region Code Snippets Used in Tests

		private string sampleCodeThatWontCompile = @"public void Execute() { 
// comment
test++; 
}";

		private string sampleCodeThatPublishesAnItem = @"[DataDictionaryEntry(""testing"")] public double test = 2; public void Execute() { test++; }";

		private string sampleCodeThatUsesNestedType = @"public class NestedClass { public void SetIt() { SetValue( ""Tester.FirstBoolean"", true ); } } public void Execute() { NestedClass n = new NestedClass(); n.SetIt(); }";

		private string sampleCodeThatUsesSetValueObject = @"public void Execute() { SetValue( ""Tester.FirstBoolean"", true ); }";

		private string sampleCodeThatUsesSetValueArray = @"public void Execute() { double[] arr = new double[3]; arr[0]=10; SetValue( ""Tester.Array"", arr ); }";

		private string sampleCodeThatUsesSetValue = @"public void Execute() { SetValue( ""Tester.FirstDouble"", 44 ); }";
		
		private string sampleCodeThatUsesGetValue = @"public void Execute() { double val = GetDbl( ""Tester.FirstDouble"" ); this.Tag = val; }";

		private string sampleCodeThatUsesGetItem = @"public void Execute() { DataItem item = GetItem( ""Tester.FirstDouble"" ); this.Tag = item; }";

		private string sampleCodeThatHasErrors = @"public void Execute() { this.bogus = 12; }";

		private string sampleCode = @"public void Execute() { System.Console.WriteLine(""Hello world""); }";
		
		private string sampleCodeWithReference = @"
		#reference System.Drawing.dll
		#reference System.Windows.Forms.dll
     #reference SceneGraph.dll

public void Execute() { System.Console.WriteLine(""Hello world""); }";

		private string sampleCodeWithNoDebug = @"
#reference System.Windows.Forms.dll
#nodebug
#reference System.Drawing.dll
public void Execute() { double x = 12; bool test = false; if( (x == 1) || (x == 12 && !test )) { System.Console.WriteLine(""Hello world!"" ); this.Tag = ""Hokies Win!""; } else System.Console.WriteLine( ""Oops"" ); }";

		private string sampleCodeWithDebug = @"
#reference System.Windows.Forms.dll
#debug
#reference System.Drawing.dll
public void Execute() { double x = 12; bool test = false; if( (x == 1) || (x == 12 && !test )) { System.Console.WriteLine(""Hello world!"" ); this.Tag = ""Hokies Win!""; } else System.Console.WriteLine( ""Oops"" ); }";

		private string sampleCodeForExecute = @"public void Execute() { double x = 12; bool test = false; if( (x == 1) || (x == 12 && !test )) { System.Console.WriteLine(""Hello world!"" ); this.Tag = ""Hokies Win!""; } else System.Console.WriteLine( ""Oops"" ); }";

//		private string sampleCodeForExecute = @"#reference System.Drawing.dll
//#reference System.Windows.Forms.dll
//
//using System.Drawing;
//
//public void Execute() { Color c = Color.Red; System.Console.WriteLine(""Hello world""); this.Tag = ""Hokies Win!""; }";

		private string sampleCodeForExecuteOtherMethod = @"#reference System.Drawing.dll
#reference System.Windows.Forms.dll

public void ExecuteOther() { System.Console.WriteLine(""Hello world""); this.Tag = ""Hokies Win!""; }
public void Execute() { System.Console.WriteLine(""Hello world""); this.Tag = ""Hokies!""; }

";
		
		private string sampleCodeForTestGetUsingLines = @"#reference System.Drawing.dll
#reference System.Windows.Forms.dll

using System.Drawing;
usingTest;
";
		
		#endregion

		private string WriteToTempFile( string code )
		{
			string path = System.IO.Path.GetTempFileName();

			try
			{
				using( System.IO.TextWriter tw = new System.IO.StreamWriter( path, false ) )
				{
					tw.Write( code );
				}
				return path;
			}
			catch
			{}

			return string.Empty;
		}
		
		#region XML Snippets Used In Tests

		private string xmlStringWithInvalidCode = @"<ScriptObject xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
		<Code>public void Execute() { ssss; System.Console.WriteLine(""Hello world""); }</Code>
		<Filename>{0}</Filename>
		<IncludeDebugInformation>false</IncludeDebugInformation>
		</ScriptObject>";
		
		private string xmlStringWithValidCode = @"<ScriptObject xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
		<Code><![CDATA[{0}]]></Code>
		<Filename />
		<IncludeDebugInformation>false</IncludeDebugInformation>
		</ScriptObject>";

		#endregion

		private bool codeChangedReceived = false;
		
		private void ExecuteScriptObject()
		{
			try
			{
				m_ScriptObject.Execute();
			}
			catch( ScriptCompilerException ex )
			{
				foreach( CompilerError error in ex.Errors )
				{
					System.Console.Out.WriteLine( "Compiler Error in TestExecute: {0} ({1})", error.ErrorText, error.Line );
				}
				throw ex;
			}
		}
		
		bool m_SaveRequestReceived = false;
		bool m_SavePerformed = false;

		/// <summary>
		/// Tests that the line numbers of the script compile errors are correct for the source.
		/// </summary>
		//[Test]
    public void TestCodeThatWontCompile()
		{
			m_ScriptObject.Code = sampleCodeThatWontCompile;
			try
			{
				m_ScriptObject.Compile();
				Assert.Fail();
			}
			catch( ScriptCompilerException ex )
			{
				Assert.AreEqual( 1, ex.Errors.Count );
				Console.WriteLine(ex.Errors[0].ErrorText);
				Console.WriteLine(m_ScriptObject.LanguageHelper.PreambleLineCount);
				Assert.AreEqual( 3, ex.Errors[0].Line );
			}
		}

		/// <summary>
		/// Tests that a <see cref="System.IO.FileNotFoundException"/> is thrown when <see cref="ScriptObject.LoadFromFile(string)"/> is called with a non-existent file.
		/// </summary>
		//[ExpectedException( typeof( System.IO.FileNotFoundException ) )]
		//[Test]
    public void AttemptToLoadNonExistentFile()
		{
			m_ScriptObject.LoadFromFile( @"c:\totallybogus\pathisbogus\nowaythisexists\test.cs" );
		}

		/// <summary>
		/// Verifies that #nodebug directive does what's intended
		/// </summary>
		//[Test]
    public void NoDebugDirectiveRecognized()
		{
			Assert.IsFalse( m_ScriptObject.IncludeDebugInformation );
			m_ScriptObject.IncludeDebugInformation = true;
			m_ScriptObject.Code = sampleCodeWithNoDebug;
			ExecuteScriptObject();
			Assert.IsFalse( m_ScriptObject.IncludeDebugInformation, "IncludeDebugInformation was true even though the code had #nodebug directive." );
		}

		/// <summary>
		/// Verifies that the #debug pre-processor directive does what's intended
		/// </summary>
		//[Test]
    public void DebugDirectiveRecognized()
		{
			Assert.IsFalse( m_ScriptObject.IncludeDebugInformation );
			m_ScriptObject.Code = sampleCodeWithDebug;
			ExecuteScriptObject();
			Assert.IsTrue( m_ScriptObject.IncludeDebugInformation, "IncludeDebugInformation was false even though the code had #debug directive." );
		}

		/// <summary>
		/// Tests that GetHelper( filename ) returns the proper helper given the file
		/// </summary>
		//[Test]
    public void ScriptHelperGetHelperForFilename()
		{
			ILanguageSpecificScriptHelper helper = ScriptHelperFactory.GetHelper( "test.cs" );
			Assert.IsTrue( helper is CSharpScriptHelper );

      //helper = ScriptHelperFactory.GetHelper( "test.vb" );
      //Assert.IsTrue( helper is VBScriptHelper );
		}

		/// <summary>
		/// Tests saving to a file
		/// </summary>
		//[Test]
    public void SaveToFile()
		{
			string path = System.IO.Path.GetTempFileName(); 
			m_SavePerformed = false;
			if( System.IO.File.Exists( path ) )
			{
				System.IO.File.Delete( path );
			}

			m_ScriptObject.SavePerformed += new EventHandler(m_ScriptObject_SavePerformed);
			m_ScriptObject.Filename = path;
			m_ScriptObject.Code = sampleCodeThatPublishesAnItem;
			m_ScriptObject.SaveToFile();

			Assert.IsTrue( System.IO.File.Exists( path ) );
			System.IO.File.Delete( path );

			Assert.IsTrue( m_SavePerformed );
		}

		/// <summary>
		/// Test that we receive the SaveRequested event
		/// </summary>
		//[Test]
    public void ExpectTheSaveRequestedEvent()
		{
			m_SaveRequestReceived = false;
			m_ScriptObject.SaveRequested += new EventHandler(m_ScriptObject_SaveRequested);
			Assert.IsTrue( m_ScriptObject.HasSaveRequestedHandler );
			m_ScriptObject.Code = sampleCodeThatPublishesAnItem;
			m_ScriptObject.SaveToFile();
			m_ScriptObject.SaveRequested -= new EventHandler(m_ScriptObject_SaveRequested);
			Assert.IsTrue( m_SaveRequestReceived );
		}

		/// <summary>
		/// Tests that the InvalidOperationException is received if the SaveToFile method is called when the script object is not properly set up
		/// </summary>
		//[ExpectedException( typeof( InvalidOperationException ) )]
		//[Test]
    public void SaveWithNoFilenameOrListeners()
		{
			m_ScriptObject.Code = sampleCodeThatPublishesAnItem;
			Assert.IsFalse( m_ScriptObject.HasSaveRequestedHandler );
			m_ScriptObject.SaveToFile();
		}

		/// <summary>
		/// Verify that the C# language helper identifies itself as having LanguageType = Csharp
		/// </summary>
		//[Test]
    public void LanguageTypeSetting()
		{
			Assert.AreEqual( LanguageType.Csharp, m_ScriptObject.LanguageHelper.Language );
		}

		/// <summary>
		/// Tests using a [DataDictionaryEntry] within script code
		/// </summary>
		//[Test]
    public void PublishScriptItems()
		{
			m_ScriptObject.Code = sampleCodeThatPublishesAnItem;
			m_ScriptObject.Compile();
			
			ScriptOtherTester ot = new ScriptOtherTester( "other" );
			(m_ScriptObject.CompiledObject as IScriptObjectImplementation).Parent = ot as IPublishable;

			var publisher = m_ScriptObject.CompiledObject as IPublishable;
			publisher.Name = "test";
			Blackboard.Publish( publisher );
			Assert.IsNotNull( Blackboard.GetExistingItem( "other.test.testing" ) );
		}

		/// <summary>
		/// Tests the HelperType for the C# script object
		/// </summary>
		//[Test]
    public void DefaultHelperType()
		{
			Assert.AreEqual( ScriptHelperFactory.HelperType.CSharp, m_ScriptObject.HelperType );
		}

		/// <summary>
		/// Tests that the default Namespace and ClassName properties are correct
		/// </summary>
		//[Test]
    public void DefaultNamespaceAndClassName()
		{
			Assert.AreEqual( "MyNamespace.MyClass", m_ScriptObject.LanguageHelper.FullClassName );
		}

		/// <summary>
		/// Tests that a nested class can access the helper methods provided by CSharpScriptHelper
		/// </summary>
		//[Test]
    public void TestNestedClassAccessToHelperMethods()
		{
			ScriptOtherTester t = new ScriptOtherTester( "Tester" );
			Assert.IsFalse( t.FirstBoolean );
			
			m_ScriptObject.Code = sampleCodeThatUsesNestedType;
			ExecuteScriptObject();
			Assert.IsTrue( t.FirstBoolean );

		}

		/// <summary>
		/// Tests the GetItem and other helper methods
		/// </summary>
		//[Test]
    public void TestGetItemInScript()
		{
			ScriptOtherTester t = new ScriptOtherTester( "Tester" );
			
			m_ScriptObject.Code = sampleCodeThatUsesGetItem;
			ExecuteScriptObject();

			var item = Blackboard.GetExistingItem( "Tester.FirstDouble" );
			Assert.IsNotNull( item );

			Assert.AreSame( item, (m_ScriptObject.CompiledObject as IScriptObjectImplementation).Tag );
			
			m_ScriptObject.Code = sampleCodeThatUsesGetValue;
			ExecuteScriptObject();
			Assert.AreEqual( t.FirstDouble, (m_ScriptObject.CompiledObject as IScriptObjectImplementation).Tag );

			m_ScriptObject.Code = sampleCodeThatUsesSetValue;
			ExecuteScriptObject();
			Assert.AreEqual( 44, t.FirstDouble );

			m_ScriptObject.Code = sampleCodeThatUsesSetValueArray;
			ExecuteScriptObject();
			Assert.AreEqual( 10, t.DblArray[0] );
      var itemElement0 = Blackboard.GetExistingItem("Tester.Array[0]");
      Assert.IsNotNull(itemElement0);
      Assert.AreEqual(10, itemElement0.Value);

			m_ScriptObject.Code = sampleCodeThatUsesSetValueObject;
			ExecuteScriptObject();
			Assert.IsTrue( t.FirstBoolean );
		}

		/// <summary>
		/// Tests the LanguageHelper.PreambleLineCount method.
		/// </summary>
		//[Test]
		public void TestPreambleLineCount()
		{
			m_ScriptObject.Code	= sampleCode;
			Assert.AreEqual( 13, m_ScriptObject.LanguageHelper.PreambleLineCount );

			m_ScriptObject.Code = sampleCodeWithReference;
			Assert.AreEqual( 10, m_ScriptObject.LanguageHelper.PreambleLineCount );

			m_ScriptObject.Code = sampleCodeForExecute;
			Assert.AreEqual( 13, m_ScriptObject.LanguageHelper.PreambleLineCount );
		}

		/// <summary>
		/// Tests the CodeChanged event
		/// </summary>
		//[Test]
		public void TestCodeChangedEvent()
		{
			m_ScriptObject.Code	= sampleCode;
			Assert.IsTrue( codeChangedReceived );
		}

		/// <summary>
		/// Tests the ScriptCompiledSuccessfully event
		/// </summary>
		//[Test]
		public void TestCompileSuccessfulEvent()
		{
			m_ScriptObject.Code = sampleCode;
			m_ScriptObject.Compile();
			Assert.IsTrue( scriptCompiledSuccessfullyReceived );

			scriptCompiledSuccessfullyReceived = false;
			m_ScriptObject.Code = sampleCodeThatHasErrors;
			try
			{
				m_ScriptObject.Compile();
			}
			catch {}

			Assert.IsFalse( scriptCompiledSuccessfullyReceived );

		}

		/// <summary>
		/// Test deserializing an XML string with the code in the XML
		/// </summary>
		//[Test]
		public void TestSerializeStringWithCode()
		{
			string xmlCode = xmlStringWithValidCode.Replace( "{0}", sampleCodeForExecute );

			ScriptObject script2 = ScriptObject.LoadScriptObjectFromXmlString( xmlCode, false );
			Assert.IsNotNull( script2 );
			Assert.AreEqual( sampleCodeForExecute, script2.Code );

			ExecuteSampleCodeForExecute(script2);
		}

		/// <summary>
		/// Test deserializing an XML string with the filename in the XML
		/// </summary>
		//[Test]
		public void TestSerializeStringWithFilename()
		{
			string path = WriteToTempFile( sampleCodeForExecute );
			string xml = xmlStringWithInvalidCode.Replace( "{0}", path );

			ScriptObject script2 = ScriptObject.LoadScriptObjectFromXmlString( xml, true );
			Assert.IsNotNull( script2 );
			Assert.AreEqual( path, script2.Filename );
			ExecuteSampleCodeForExecute(script2);
		}

		/// <summary>
		/// Tests serializing the script object with the Filename property loaded.
		/// </summary>
		//[Test]
		public void TestSerializeFilename()
		{
			string path = WriteToTempFile( sampleCodeForExecute );
			
			m_ScriptObject.Filename = path;

			string pathXML = SerializeScriptObject( m_ScriptObject );

			System.IO.FileInfo fi = new System.IO.FileInfo( path );
			Assert.IsTrue( fi.Length > 0 );

			ScriptObject script2 = ScriptObject.LoadScriptObjectFromXmlFile( pathXML, true );
			Assert.IsNotNull( script2 );
			Assert.AreEqual( path, script2.Filename );

			ExecuteSampleCodeForExecute(script2);
		}

		private string SerializeScriptObject( ScriptObject scriptObject )
		{
			string path = System.IO.Path.GetTempFileName();
			
			System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer( typeof(ScriptObject) );
			using( System.IO.TextWriter writer = new System.IO.StreamWriter(path) )
			{
				serializer.Serialize( writer, scriptObject );
			}
			
			return path;
		}

		private void SerializeDeserializeAndTest( string code )
		{
			m_ScriptObject.Code = code;
			string path = SerializeScriptObject( m_ScriptObject );

			System.IO.FileInfo fi = new System.IO.FileInfo( path );
			Assert.IsTrue( fi.Length > 0 );

			ScriptObject script2 = ScriptObject.LoadScriptObjectFromXmlFile( path, false );

			ExecuteSampleCodeForExecute(script2);

			if( System.IO.File.Exists( path ) )
			{
				System.IO.File.Delete( path );
			}
		}

//		/// <summary>
//		/// This test ensures that we can serialize/deserialize code containing characters that might mess up XML serialization but are valid code.
//		/// </summary>
//		[Test] public void SerializeCodeContainingSpecialCharacters()
//		{
//			SerializeDeserializeAndTest( sampleCodeWithSpecialCharacters );
//		}
//
		/// <summary>
		/// Test serializing the script object with code loaded
		/// </summary>
		//[Test]
		public void TestSerializeCode()
		{
			SerializeDeserializeAndTest( sampleCodeForExecute );
		}

		/// <summary>
		/// Test loading the code from an external file by setting the Filename property and calling LoadFile()
		/// </summary>
		//[Test]
		public void TestLoadFromFile2()
		{
			string path = WriteToTempFile( sampleCodeForExecute );
			
			m_ScriptObject.Filename = path;
			m_ScriptObject.LoadFromFile();
			Assert.AreEqual( path, m_ScriptObject.Filename );
			Assert.IsTrue( m_ScriptObject.IsDirty );
			
			ExecuteSampleCodeForExecute(m_ScriptObject);

			if( System.IO.File.Exists( path ) )
			{
				System.IO.File.Delete( path );
			}
		}

		/// <summary>
		/// Test loading the code from an external file by calling LoadFile( path )
		/// </summary>
		//[Test]
		public void TestLoadFromFile()
		{
			string path = WriteToTempFile( sampleCodeForExecute );

			m_ScriptObject.LoadFromFile( path );
			Assert.AreEqual( path, m_ScriptObject.Filename );
			Assert.IsTrue( m_ScriptObject.IsDirty );
			ExecuteSampleCodeForExecute(m_ScriptObject);

			if( System.IO.File.Exists( path ) )
			{
				System.IO.File.Delete( path );
			}
		}

		/// <summary>
		/// Tests the IsDirty flag
		/// </summary>
		//[Test]
		public void TestIsDirtyFlag()
		{
			m_ScriptObject.Code = sampleCode;
			Assert.IsTrue( m_ScriptObject.IsDirty );
			
			m_ScriptObject.Compile();
			Assert.IsFalse( m_ScriptObject.IsDirty );
		}

		/// <summary>
		/// Tests compiling the script code and the validity of the CompiledObject after compile
		/// </summary>
		//[Test]
		public void TestCompile()
		{
			m_ScriptObject.Code = sampleCode;
			Assert.IsTrue( m_ScriptObject.CompleteCode.Length > 0 );

			Assert.IsNull( m_ScriptObject.CompiledAssembly );
			Assert.IsNull( m_ScriptObject.CompiledObject );

			m_ScriptObject.Compile();

			Assert.IsFalse( m_ScriptObject.IsDirty );
			Assert.IsNotNull( m_ScriptObject.CompiledAssembly );
			Assert.IsNotNull( m_ScriptObject.CompiledObject );
			Assert.IsNotNull( m_ScriptObject.CompiledObject as IScriptObjectImplementation );
		}

		/// <summary>
		/// Tests adding to the referenced assemblies using the #reference directive. Also makes sure items with
		/// #reference that already exist in the reference assemblies are not added again
		/// </summary>
		//[Test]
		public void TestReferenceAssemblyDirective()
		{
			int referenceAssemblyCount = m_ScriptObject.ReferencedAssemblies.Count;
            referenceAssemblyCount += m_ScriptObject.LanguageHelper.ReferencedAssemblies.Length;

			m_ScriptObject.Code = sampleCodeWithReference;

			Assert.IsTrue( m_ScriptObject.ReferencedAssemblies.Contains( "System.Drawing.dll" ) );	
			Assert.IsTrue( m_ScriptObject.ReferencedAssemblies.Contains( "System.Windows.Forms.dll" ) );	
			Assert.IsTrue( m_ScriptObject.ReferencedAssemblies.Contains( "SceneGraph.dll" ) );	
			Assert.AreEqual( referenceAssemblyCount + 2, m_ScriptObject.ReferencedAssemblies.Count );
		}	

		private void ExecuteSampleCodeForExecute(ScriptObject script)
		{
			try
			{
				script.Execute();
			}
			catch( ScriptCompilerException ex )
			{
				foreach( CompilerError error in ex.Errors )
				{
					System.Console.Out.WriteLine( "Compiler Error in TestExecute: {0}", error.ErrorText );
				}
				throw ex;
			}
			Assert.AreEqual("Hokies Win!", (script.CompiledObject as IScriptObjectImplementation).Tag );
		}

		/// <summary>
		/// Tests the Execute method with the sampleCodeForExecute
		/// </summary>
		//[Test]
		public void TestExecute()
		{
			m_ScriptObject.Code = sampleCodeForExecute;
			ExecuteSampleCodeForExecute(m_ScriptObject);
		}

		/// <summary>
		/// Tests execute a method by name
		/// </summary>
		//[Test]
		public void TestExecuteSomeOtherMethod()
		{

			m_ScriptObject.Code = sampleCodeForExecuteOtherMethod;
			m_ScriptObject.Execute("ExecuteOther");

			Assert.AreEqual("Hokies Win!", (m_ScriptObject.CompiledObject as IScriptObjectImplementation).Tag );

			m_ScriptObject.Execute();
			Assert.AreEqual("Hokies!", (m_ScriptObject.CompiledObject as IScriptObjectImplementation).Tag );

		}

		/// <summary>
		/// Tests get UsingLines
		/// </summary>
		//[Test]
		public void TestGetUsingLines()
		{
			Assert.IsFalse( new CSharpScriptHelper().LineContainsDirective( "usingThis" ) );
			Assert.IsTrue( new CSharpScriptHelper().LineContainsDirective( "using Test" ) );

			Assert.IsFalse( new CSharpScriptHelper().LineContainsDirective( "#referenceTest" ) );
			Assert.IsTrue( new CSharpScriptHelper().LineContainsDirective( "#reference Test" ) );

			string lines = new CSharpScriptHelper().GetUsingLines( sampleCodeForTestGetUsingLines );
			Assert.AreEqual( @"using System.Drawing;
", lines );

		}

		private void m_ScriptObject_CodeChanged(object sender, EventArgs e)
		{
			codeChangedReceived = true;
		}

		bool scriptCompiledSuccessfullyReceived = false;

		private void m_ScriptObject_ScriptCompiledSuccessfully(object sender, EventArgs e)
		{
			scriptCompiledSuccessfullyReceived = true;
		}

		private void m_ScriptObject_SaveRequested(object sender, EventArgs e)
		{
			m_SaveRequestReceived = true;
		}

		private void m_ScriptObject_SavePerformed(object sender, EventArgs e)
		{
			m_SavePerformed = true;
		}
	}

	/// <summary>
	/// OtherTester is a test class used by DataDictionaryUnitTests
	/// </summary>
	internal class ScriptOtherTester: IPublishable
	{
		#region IModel Members
		private string m_Name = string.Empty;
		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; } 
		}

		public IPublishable Parent
		{
			get
			{
				// TODO:  Add OtherTester.Parent getter implementation
				return null;
			}
		  set {}
		}

	  public string Path { get; set; }

		#endregion
		/// <summary>
		/// 
		/// </summary>
		public ScriptOtherTester()
		{
      Blackboard.Publish(this, "FirstBoolean", "FirstBoolean");
      Blackboard.Publish(this, "FirstDouble", "FirstDouble");
      Blackboard.Publish(this, "SecondBoolean", "SecondBoolean");
		}
		

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		public ScriptOtherTester( string name )
		{
      Blackboard.Publish(this, name + Blackboard.PathSeparator + "TestAttribute", "TestAttributedBool");
      Blackboard.Publish(this, name + Blackboard.PathSeparator + "FirstBoolean", "FirstBoolean");
      Blackboard.Publish(this, name + Blackboard.PathSeparator + "FirstDouble", "FirstDouble");
      Blackboard.Publish(this, name + Blackboard.PathSeparator + "Array", "DblArray");
			m_Name = name;

			this.DblArray[0] = 1;
			this.DblArray[1] = 2;
			this.DblArray[2] = 3;

		}	

		[Blackboard("TestAttribute")]
		private bool TestAttributedBool = true;
		
		public bool TestAttrBool
		{
			get { return TestAttributedBool; }
		}
		
		internal double[] DblArray = new double[3];

		internal bool FirstBoolean = false;
		internal bool SecondBoolean = false;
		internal double FirstDouble = 1;
	}

}
