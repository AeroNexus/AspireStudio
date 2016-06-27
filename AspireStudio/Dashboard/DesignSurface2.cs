using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

using Silver.UI;

using Aspire.Studio.DockedViews;

namespace Aspire.Studio.Dashboard
{
	public class DesignSurface2 : DesignSurface, IDesignSurface2
	{
		private const string _Name_ = "DesignSurface2";

//- this class adds to a .NET 
//-     DesignSurface instance
//- the following facilities:
//-     * TabOrder
//-     * UndoEngine
//-     * Cut/Copy/Paste/Delete commands
//-
//- DesignSurface2
//-     |
//-     +--|ContextMenu|
//-     |
//-     +--|DesignSurface|
//-     |
//-     +--|TabOrder|
//-     |
//-     +--|UndoEngine|
//-     |
//-     +--|Cut/Copy/Paste/Delete commands|

    #region IDesignSurface2 Members

		public void SwitchTabOrder()
		{
			if (false == IsTabOrderMode)
			{
				InvokeTabOrder();
			}
			else
			{
				DisposeTabOrder();
			}
		}

		public void UseSnapLines()
		{
        IServiceContainer serviceProvider = GetService ( typeof ( IServiceContainer ) ) as IServiceContainer;
        DesignerOptionService opsService = serviceProvider.GetService ( typeof ( DesignerOptionService ) ) as DesignerOptionService;
        if ( null != opsService ) {
            serviceProvider.RemoveService ( typeof ( DesignerOptionService ) );
        }
        DesignerOptionService opsService2 = new DesignerOptionService4SnapLines();
        serviceProvider.AddService ( typeof ( DesignerOptionService ), opsService2 );
    }

    public void UseGrid ( Size gridSize ) {
        IServiceContainer serviceProvider = GetService ( typeof ( IServiceContainer ) ) as IServiceContainer;
        DesignerOptionService opsService = serviceProvider.GetService ( typeof ( DesignerOptionService ) ) as DesignerOptionService;
        if ( null != opsService ) {
            serviceProvider.RemoveService ( typeof ( DesignerOptionService ) );
        }
        DesignerOptionService opsService2 = new DesignerOptionService4Grid ( gridSize );
        serviceProvider.AddService ( typeof ( DesignerOptionService ), opsService2 );
    }

    public void UseGridWithoutSnapping ( Size gridSize ) {
        IServiceContainer serviceProvider = GetService ( typeof ( IServiceContainer ) ) as IServiceContainer;
        DesignerOptionService opsService = serviceProvider.GetService ( typeof ( DesignerOptionService ) ) as DesignerOptionService;
        if ( null != opsService ) {
            serviceProvider.RemoveService ( typeof ( DesignerOptionService ) );
        }
        DesignerOptionService opsService2 = new DesignerOptionService4GridWithoutSnapping ( gridSize );
        serviceProvider.AddService ( typeof ( DesignerOptionService ), opsService2 );
    }

    public void UseNoGuides() {
        IServiceContainer serviceProvider = GetService ( typeof ( IServiceContainer ) ) as IServiceContainer;
        DesignerOptionService opsService = serviceProvider.GetService ( typeof ( DesignerOptionService ) ) as DesignerOptionService;
        if ( null != opsService ) {
            serviceProvider.RemoveService ( typeof ( DesignerOptionService ) );
        }
        DesignerOptionService opsService2 = new DesignerOptionService4NoGuides();
        serviceProvider.AddService ( typeof ( DesignerOptionService ), opsService2 );
    }

    public UndoEngine2 GetUndoEngine() {
        return _undoEngine;
    }

    private IComponent CreateRootComponentCore ( Type controlType, Size controlSize, DesignerLoader loader ) {
        const string _signature_ = _Name_ + @"::CreateRootComponentCore()";
        try {
            //- step.1
            //- get the IDesignerHost
            //- if we are not not able to get it 
            //- then rollback (return without do nothing)
            IDesignerHost host = GetIDesignerHost();
            if ( null == host ) return null;
            //- check if the root component has already been set
            //- if so then rollback (return without do nothing)
            if ( null != host.RootComponent ) return null;
            //-
            //-
            //- step.2
            //- create a new root component and initialize it via its designer
            //- if the component has not a designer
            //- then rollback (return without do nothing)
            //- else do the initialization
            if ( null != loader ) {
                BeginLoad( loader );
                if ( LoadErrors.Count > 0 )
                    throw new Exception( _signature_ + " - Exception: the BeginLoad(loader) failed!" );
            }
            else {
                BeginLoad( controlType );
                if ( LoadErrors.Count > 0 )
                    throw new Exception( _signature_ + " - Exception: the BeginLoad(Type) failed! Some error during " + controlType.ToString() + " loading" );
            }
            //-
            //-
            //- step.3
            //- try to modify the Size of the object just created
            IDesignerHost ihost = GetIDesignerHost();
            //- Set the backcolor and the Size
            Control ctrl = null;
            if ( host.RootComponent is  Form ) {
                ctrl = View as Control;
                ctrl.BackColor = Color.LightGray;
                //- set the Size
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties( ctrl );
                //- Sets a PropertyDescriptor to the specific property
                PropertyDescriptor pdS = pdc.Find( "Size", false );
                if ( null != pdS )
                    pdS.SetValue( ihost.RootComponent, controlSize );
            }
            else if ( host.RootComponent is UserControl ) {
                ctrl = View as Control;
                ctrl.BackColor = Color.Gray;
                //- set the Size
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties( ctrl );
                //- Sets a PropertyDescriptor to the specific property
                PropertyDescriptor pdS = pdc.Find( "Size", false );
                if ( null != pdS )
                    pdS.SetValue( ihost.RootComponent, controlSize );
            }
            else if (  host.RootComponent is  Control ) {
                ctrl = View as Control;
                ctrl.BackColor = Color.LightGray;
                //- set the Size
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties( ctrl );
                //- Sets a PropertyDescriptor to the specific property
                PropertyDescriptor pdS = pdc.Find( "Size", false );
                if ( null != pdS )
                    pdS.SetValue( ihost.RootComponent, controlSize );
            }
            else if (  host.RootComponent is  Component ) {
                ctrl = View as Control;
                ctrl.BackColor = Color.White;
                //- don't set the Size
            }
            else {
                //- Undefined Host Type
                ctrl = View as Control;
                ctrl.BackColor = Color.Red;
            }

            return ihost.RootComponent;
        }
        catch( Exception exx ) {
            Debug.WriteLine( exx.Message );
            if ( null != exx.InnerException )
                Debug.WriteLine( exx.InnerException.Message );
            
            throw;
        }
    }

    public IComponent CreateRootComponent( Type controlType, Size controlSize ) {
        return CreateRootComponentCore( controlType, controlSize, null );
    }

    public IComponent CreateRootComponent( DesignerLoader loader, Size controlSize ) {
        return CreateRootComponentCore( null, controlSize, loader );
    }
    
    public Control CreateControl ( Type controlType, Size controlSize, Point controlLocation ) {
        try {
            //- step.1
            //- get the IDesignerHost
            //- if we are not able to get it 
            //- then rollback (return without do nothing)
            IDesignerHost host = GetIDesignerHost();
            if ( null == host ) return null;
            //- check if the root component has already been set
            //- if not so then rollback (return without do nothing)
            if ( null == host.RootComponent ) return null;
            //-
            //-
            //- step.2
            //- create a new component and initialize it via its designer
            //- if the component has not a designer
            //- then rollback (return without do nothing)
            //- else do the initialization
            IComponent newComp = host.CreateComponent ( controlType );
            if ( null == newComp ) return null;
            IDesigner designer = host.GetDesigner ( newComp );
            if ( null == designer ) return null;
            if ( designer is IComponentInitializer )
                ( ( IComponentInitializer ) designer ).InitializeNewComponent ( null );
            //-
            //-
            //- step.3
            //- try to modify the Size/Location of the object just created
            PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties ( newComp );
            //- Sets a PropertyDescriptor to the specific property.
            PropertyDescriptor pdS = pdc.Find ( "Size", false );
            if ( null != pdS )
                pdS.SetValue ( newComp, controlSize );
            PropertyDescriptor pdL = pdc.Find ( "Location", false );
            if ( null != pdL )
                pdL.SetValue ( newComp, controlLocation );
            //-
            //-
            //- step.4
            //- commit the Creation Operation
            //- adding the control to the DesignSurface's root component
            //- and return the control just created to let further initializations
            ( ( Control ) newComp ).Parent = host.RootComponent as Control;
            return newComp as Control;
        }
        catch( Exception exx ) {
            Debug.WriteLine( exx.Message );
            if ( null != exx.InnerException )
                Debug.WriteLine( exx.InnerException.Message );
            
            throw;
        }
    }

    public IDesignerHost GetIDesignerHost() {
        return ( IDesignerHost ) ( GetService ( typeof ( IDesignerHost ) ) );
    }

    public Control GetView() {
        Control ctrl = View as Control;
        if ( null == ctrl )
            return null;
        ctrl.Dock = DockStyle.Fill;
        return ctrl;
    }

    #endregion

	#region TabOrder

	private TabOrderHooker _tabOrder = null;
	private bool _tabOrderMode = false;

	public bool IsTabOrderMode
	{
		get { return _tabOrderMode; }
	}

	public TabOrderHooker TabOrder
	{
		get
		{
			if (_tabOrder == null)
				_tabOrder = new TabOrderHooker();
			return _tabOrder;
		}
		set { _tabOrder = value; }
	}

	public void InvokeTabOrder()
	{
		TabOrder.HookTabOrder(GetIDesignerHost());
		_tabOrderMode = true;
	}

	public void DisposeTabOrder()
	{
		TabOrder.HookTabOrder(GetIDesignerHost());
		_tabOrderMode = false;
	}
	#endregion

	#region  UndoEngine

    private UndoEngine2 _undoEngine = null;
    private NameCreationService _nameCreationService = null;
    private DesignerSerializationService _designerSerializationService = null;
    private CodeDomComponentSerializationService _codeDomComponentSerializationService = null;

    #endregion



    #region ctors

    //- ctors
    //- Summary:
    //-     Initializes a new instance of the System.ComponentModel.Design.DesignSurface
    //-     class.
    //-
    //- Exceptions:
    //-   System.ObjectDisposedException:
    //-     The System.ComponentModel.Design.IDesignerHost attached to the System.ComponentModel.Design.DesignSurface
    //-     has been disposed.
    public DesignSurface2() : base() { InitServices(); }

    //-
    //- Summary:
    //-     Initializes a new instance of the System.ComponentModel.Design.DesignSurface
    //-     class.
    //-
    //- Parameters:
    //-   parentProvider:
    //-     The parent service provider, or null if there is no parent used to resolve
    //-     services.
    //-
    //- Exceptions:
    //-   System.ObjectDisposedException:
    //-     The System.ComponentModel.Design.IDesignerHost attached to the System.ComponentModel.Design.DesignSurface
    //-     has been disposed.
    public DesignSurface2( IServiceProvider parentProvider ) : base( parentProvider ) { InitServices(); }

    //-
    //- Summary:
    //-     Initializes a new instance of the System.ComponentModel.Design.DesignSurface
    //-     class.
    //-
    //- Parameters:
    //-   rootComponentType:
    //-     The type of root component to create.
    //-
    //- Exceptions:
    //-   System.ArgumentNullException:
    //-     rootComponent is null.
    //-
    //-   System.ObjectDisposedException:
    //-     The System.ComponentModel.Design.IDesignerHost attached to the System.ComponentModel.Design.DesignSurface
    //-     has been disposed.
    public DesignSurface2( Type rootComponentType ) : base( rootComponentType ) { InitServices(); }
 
    //-
    //- Summary:
    //-     Initializes a new instance of the System.ComponentModel.Design.DesignSurface
    //-     class.
    //-
    //- Parameters:
    //-   parentProvider:
    //-     The parent service provider, or null if there is no parent used to resolve
    //-     services.
    //-
    //-   rootComponentType:
    //-     The type of root component to create.
    //-
    //- Exceptions:
    //-   System.ArgumentNullException:
    //-     rootComponent is null.
    //-
    //-   System.ObjectDisposedException:
    //-     The System.ComponentModel.Design.IDesignerHost attached to the System.ComponentModel.Design.DesignSurface
    //-     has been disposed.
    public DesignSurface2( IServiceProvider parentProvider, Type rootComponentType ) : base( parentProvider, rootComponentType ) { InitServices(); }

    //- The DesignSurface class provides several design-time services automatically.
    //- The DesignSurface class adds all of its services in its constructor.
    //- Most of these services can be overridden by replacing them in the
    //- protected ServiceContainer property.To replace a service, override the constructor,
    //- call base, and make any changes through the protected ServiceContainer property.
    private void InitServices() {
        //- each DesignSurface has its own default services
        //- We can leave the default services in their present state,
        //- or we can remove them and replace them with our own.
        //- Now add our own services using IServiceContainer
        //-
        //-
        //- Note
        //- before loading the root control in the design surface
        //- we must add an instance of naming service to the service container.
        //- otherwise the root component did not have a name and this caused
        //- troubles when we try to use the UndoEngine
        //-
        //-
        //- 1. NameCreationService
        _nameCreationService = new NameCreationService();
        if ( _nameCreationService != null ) {
            ServiceContainer.RemoveService( typeof( INameCreationService ), false );
            ServiceContainer.AddService( typeof( INameCreationService ), _nameCreationService );
        }
        //-
        //-
        //- 2. CodeDomComponentSerializationService
        _codeDomComponentSerializationService = new CodeDomComponentSerializationService( ServiceContainer );
        if ( _codeDomComponentSerializationService != null ) {
            //- the CodeDomComponentSerializationService is ready to be replaced
            ServiceContainer.RemoveService( typeof( ComponentSerializationService ), false );
            ServiceContainer.AddService( typeof( ComponentSerializationService ), _codeDomComponentSerializationService );
        }
        //-
        //-
        //- 3. IDesignerSerializationService
        _designerSerializationService = new DesignerSerializationService( ServiceContainer );
        if ( _designerSerializationService != null ) {
            //- the IDesignerSerializationService is ready to be replaced
            ServiceContainer.RemoveService( typeof( IDesignerSerializationService ), false );
            ServiceContainer.AddService( typeof( IDesignerSerializationService ), _designerSerializationService );
        }
        //-
        //-
        //- 4. UndoEngine
		_undoEngine = new UndoEngine2(ServiceContainer) { Enabled = false };
        if ( _undoEngine != null ) {
            //- the UndoEngine is ready to be replaced
            ServiceContainer.RemoveService( typeof( UndoEngine ), false );
            ServiceContainer.AddService( typeof( UndoEngine ), _undoEngine );
        }
        //-
        //-
        //- 5. IMenuCommandService
        ServiceContainer.AddService( typeof( IMenuCommandService ), new MenuCommandService( this ) );

		_menuCommandService = new MenuCommandService2(this);
		if (_menuCommandService != null)
		{
			//- remove the old Service, i.e. the DesignsurfaceExt service
			ServiceContainer.RemoveService(typeof(IMenuCommandService), false);
			//- add the new IMenuCommandService
			ServiceContainer.AddService(typeof(IMenuCommandService), _menuCommandService);
		}
		//-
		//-
		//- IToolboxService - use parent provider
		//_toolboxService = new ToolboxService(GetIDesignerHost());
		//if (_toolboxService != null)
		//{
		//	ServiceContainer.RemoveService(typeof(IToolboxService), false);
		//	ServiceContainer.AddService(typeof(IToolboxService), _toolboxService);
		//}
	}
    
    #endregion

    //- do some Edit menu command using the MenuCommandServiceImp
    public void DoAction( string command ) {
        if ( string.IsNullOrEmpty( command ) ) return;

        IMenuCommandService ims = GetService( typeof( IMenuCommandService ) ) as IMenuCommandService;
        if ( null == ims ) return;


        try {
            switch( command.ToUpper() ) {
                case "CUT" :
                    ims.GlobalInvoke( StandardCommands.Cut );
                    break;
                case "COPY" :
                    ims.GlobalInvoke( StandardCommands.Copy );
                    break;
                case "PASTE":
                    ims.GlobalInvoke( StandardCommands.Paste );
                    break;
                case "DELETE":
                    ims.GlobalInvoke( StandardCommands.Delete );
                    break;
				//default:
				//	// do nothing;
				//	break;
            }
        }
        catch( Exception exx ) {
            Debug.WriteLine( exx.Message );
            if ( null != exx.InnerException )
                Debug.WriteLine( exx.InnerException.Message );

            throw;
        }
    }

	#region  IToolboxService

    //private ToolboxService _toolboxService = null;

    public ToolboxService GetIToolboxService() {
        return GetService( typeof( IToolboxService ) ) as ToolboxService;
    }

    #region drag&Drop
    public void EnableDragandDrop() {
        // For the management of the drag and drop of the toolboxItems
        var ctrl = GetView();
        if ( null==ctrl )
            return;

        ctrl.AllowDrop = true;
        ctrl.DragDrop += new DragEventHandler( OnDragDrop );
		ctrl.DragEnter += ctrl_DragEnter;

        //- enable the Dragitem inside the our Toolbox
        var tbs = GetIToolboxService();
        if ( null == tbs )
            return;
        if ( null == tbs.Toolbox )
            return;
        tbs.Toolbox.MouseDown += new MouseEventHandler( OnListboxMouseDown );
    }


    //- Management of the Drag&Drop of the toolboxItems contained inside our Toolbox
    private void OnListboxMouseDown( object sender, MouseEventArgs e ) {
        var tbs = GetIToolboxService();
        if ( null == tbs )
            return;
        if ( null == tbs.Toolbox ) 
            return;
        if ( null == tbs.Toolbox.SelectedItem ) 
            return;

        tbs.Toolbox.DoDragDrop( tbs.Toolbox.SelectedItem, DragDropEffects.Copy | DragDropEffects.Move );
    }

    //- Management of the drag and drop of the toolboxItems
    public void OnDragDrop( object sender, DragEventArgs e ) {
        //- if the user don't drag a ToolboxItem 
        //- then do nothing
		//if ( !e.Data.GetDataPresent( typeof( ToolboxItem ) ) ) {
		//	e.Effect = DragDropEffects.None;
		//	return;
		//}
        //- now retrieve the data node
        var item = e.Data.GetData( typeof( ToolBoxItem ) ) as ToolBoxItem;
        e.Effect = DragDropEffects.Copy;

		var info = item.Object as ToolBoxConfig.ToolInfo;
		var tbItem = new ToolboxItem(info.Type);
        tbItem.CreateComponents( GetIDesignerHost() );

    }
	private void ctrl_DragEnter(object sender, DragEventArgs e)
	{
		var item = e.Data.GetData(typeof(ToolBoxItem)) as ToolBoxItem;
		if (item != null )//&& item.Object is ToolBoxConfig.ToolInfo )
			e.Effect = e.AllowedEffect;
	}
	#endregion

    #endregion


    #region  MenuCommandService
    private MenuCommandService2 _menuCommandService = null;
    #endregion
	}
}
