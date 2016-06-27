using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
//using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Aspire.Framework;
using Aspire.Framework.Scripting;
using Aspire.Utilities;
using Aspire.UiToolbox.SyntaxEditor;

namespace Aspire.Studio.DocumentViews
{
  public partial class CodeEditor : StudioDocumentView
  {
    private CodeDoc myDoc;
    private SyntaxEditor editor;
    private static List<string> addedReferences = new List<string>();
    private static IntelliSenseResolver intelliSenseResolver = new IntelliSenseResolver();


    public CodeEditor()
      : base(Solution.ProjectItem.ItemType.CsFile)
    {
      InitializeComponent();
      Name = "CodeEditor"; // simplify
      LogDrop = true;
      editor = new SyntaxEditor();
    }

    private static void AddReferencesFromCollection(ICollection collection)
    {
      foreach (string s in collection)
      {
        try
        {
          string assemblyPath = Path.GetFileNameWithoutExtension(s);
          if (!addedReferences.Contains(assemblyPath))
          {
            intelliSenseResolver.AddExternalReference(System.Reflection.Assembly.Load(assemblyPath));
            addedReferences.Add(assemblyPath);
          }
        }
        catch (System.Exception e)
        {
          Log.WriteLine("Error adding reference assembly {0}: {1}", s, e.Message);
        }
      }
    }

    private void AddScriptObjectAssembliesAndWrapper()
    {
      if (ScriptObject != null)
      {
        using (new WaitCursor())
        {
          AddReferencesFromCollection(ScriptObject.ReferencedAssemblies);

          if (ScriptObject.LanguageHelper != null)
          {
            editor.Document.HeaderText = ScriptObject.LanguageHelper.HeaderText;
            editor.Document.FooterText = ScriptObject.LanguageHelper.FooterText;

            AddReferencesFromCollection(ScriptObject.LanguageHelper.ReferencedAssemblies);
          }
        }
      }

    }

    /// <summary>
    /// Gets and sets the text in the code editor
    /// </summary>
    public string Code
    {
      get { return editor.Text; }
      set
      {
        editor.Text = value;

        if (ScriptObject != null && ScriptObject.LanguageHelper != null)
        {
          ScriptObject.LanguageHelper.WrapCode(value);
          AddScriptObjectAssembliesAndWrapper();
        }

      }
    }

    public string CommentString { get; set; }
    public object CompilerErrors { get; set; }
    public SyntaxEditor Editor { get; set; }
    public object HostComponent { get; set; }
    public bool IsDirty { get; set; }
    public bool IsSaving { get; set; }
    public LanguageType LanguageType { get; set; }
    public Form SaveFileDlg { get; set; }
    public bool SaveOnCompile { get; set; }
    public bool StatusBarVisible { get; set; }
    public bool WatchesPaneVisible { get; set; }

    //protected override void NewItem(Blackboard.Item bbItem, DragEventArgs e)
    //{
    //  if (myDoc == null) myDoc = MyDocument(new DashboardDoc()) as DashboardDoc;

    //  var item = new DashboardItem();
    //  myDoc.AddItem(item, bbItem);
    //}

    protected override void OnDocumentBind()
    {
      myDoc = mDocument as CodeDoc;
    }

    ////- When the selection changes this sets the PropertyGrid's selected component
    //private void OnSelectionChanged(object sender, EventArgs e)
    //{
    //  if (mPropertiesView == null) return;
    //  ISelectionService selectionService = selectionService = mSurface.GetIDesignerHost().GetService(typeof(ISelectionService)) as ISelectionService;
    //  try
    //  {
    //    mPropertiesView.SelectedObject = (sender as ISelectionService).PrimarySelection;
    //  }
    //  catch (Exception ex)
    //  {
    //    Log.ReportException(ex, "OnSelectionChanged");
    //  }
    //}

    public static void Register(bool unregister = false)
    {
      if (unregister)
        DocumentMgr.UndefineDocView(typeof (CodeEditor), typeof (CodeDoc));
      else
        DocumentMgr.DefineDocView(typeof (CodeEditor), typeof (CodeDoc));
    }

    public ScriptObject ScriptObject { get; set; }

    public override void UpdateDisplay(Clock clock)
    {
    }

  }

  public class CodeDoc : StudioDocument
  {
    private const string category = "Code";

    public CodeDoc()
      : base(ItemType.CsFile)
    {
    }

    public override StudioDocumentView NewView(string name)
    {
      var view = new CodeEditor() {Name = name};
      return view;
    }

  }
}


