using System.ComponentModel;
using System.Xml.Serialization;

namespace Aspire.BrowsingUI.TestBench
{
  public partial class TestResult_WS : object, INotifyPropertyChanged
  {

    private string commentsField;

    private string loggedTextField;

    private string deviceUidField;

    private string testGuidField;

    private string testNameField;

    private bool passedField;

    private string operatorField;

    /// <remarks/>
    [XmlElement(Order = 0)]
    public string Comments
    {
      get
      {
        return commentsField;
      }
      set
      {
        commentsField = value;
        RaisePropertyChanged("Comments");
      }
    }

    /// <remarks/>
    [XmlElement(Order = 1)]
    public string LoggedText
    {
      get
      {
        return loggedTextField;
      }
      set
      {
        loggedTextField = value;
        RaisePropertyChanged("LoggedText");
      }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string DeviceUid
    {
      get
      {
        return deviceUidField;
      }
      set
      {
        deviceUidField = value;
        RaisePropertyChanged("DeviceUid");
      }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string TestGuid
    {
      get
      {
        return testGuidField;
      }
      set
      {
        testGuidField = value;
        RaisePropertyChanged("TestGuid");
      }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string TestName
    {
      get
      {
        return testNameField;
      }
      set
      {
        testNameField = value;
        RaisePropertyChanged("TestName");
      }
    }

    /// <remarks/>
    [XmlAttribute()]
    public bool Passed
    {
      get
      {
        return passedField;
      }
      set
      {
        passedField = value;
        RaisePropertyChanged("Passed");
      }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string Operator
    {
      get
      {
        return operatorField;
      }
      set
      {
        operatorField = value;
        RaisePropertyChanged("Operator");
      }
    }
    /// <summary>
    /// 
    /// </summary>
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    protected void RaisePropertyChanged(string propertyName)
    {
      System.ComponentModel.PropertyChangedEventHandler propertyChanged = PropertyChanged;
      if (propertyChanged != null)
      {
        propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
      }
    }
  }
}
