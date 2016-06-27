using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace Aspire.Studio.Dashboard
{
	public class ToolboxService : IToolboxService
	{

		//- It is the gateway between the toolbox user interface 
		//- in the development environment and the designers. 
		//- The designers constantly query the toolbox when the 
		//- cursor is  over them to get feedback about the selected control
		//-
		//- NOTE:
		//- this is a lightweight class!
		//- this class implements the interface IToolboxService 
		//- it does NOT create a ListBox, it merely links one
		//- which is created by user and then referenced by 
		//- the ToolboxServiceImp::Toolbox property

		public IDesignerHost DesignerHost { get; private set; }

        //- our real Toolbox
        public ListBox Toolbox { get; set; }


        //- ctor
        public ToolboxService( IDesignerHost host ){
			this.DesignerHost = host;

			//- Our MainForm adds our ToolboxPane to the DesignerHost's services.
            Toolbox = null;
		}

        #region IToolboxService Members

        //- Add a creator that will convert non-standard tools in the specified format into ToolboxItems, to be associated with a host.
        void IToolboxService.AddCreator( ToolboxItemCreatorCallback creator, string format, IDesignerHost host ) {
            // UNIMPLEMENTED - We aren't handling any non-standard tools here. Our toolset is constant.
            //throw new NotImplementedException();
        }

        //- Add a creator that will convert non-standard tools in the specified format into ToolboxItems.
        void IToolboxService.AddCreator( ToolboxItemCreatorCallback creator, string format ) {
            // UNIMPLEMENTED - We aren't handling any non-standard tools here. Our toolset is constant.
            //throw new NotImplementedException();
        }

        //- Add a ToolboxItem to our toolbox, in a specific category, bound to a certain host.
        void IToolboxService.AddLinkedToolboxItem( ToolboxItem toolboxItem, string category, IDesignerHost host ) {
            // UNIMPLEMENTED - We didn't end up doing a project system, so there's no need
            // to add custom tools (despite that we do have a tab for such tools).
            //throw new NotImplementedException();
        }

        //- Add a ToolboxItem to our toolbox, bound to a certain host.
        void IToolboxService.AddLinkedToolboxItem( ToolboxItem toolboxItem, IDesignerHost host ) {
            // UNIMPLEMENTED - We didn't end up doing a project system, so there's no need
            // to add custom tools (despite that we do have a tab for such tools).
            //throw new NotImplementedException();
        }

        //- Add a ToolboxItem to our toolbox under the specified category.
        void IToolboxService.AddToolboxItem( ToolboxItem toolboxItem, string category ) {
            //- we have no category 
            ((IToolboxService) this).AddToolboxItem( toolboxItem );
        }

        //- Add a ToolboxItem to our Toolbox.
        void IToolboxService.AddToolboxItem( ToolboxItem toolboxItem ) {
            Toolbox.Items.Add( toolboxItem );
        }


        //- Our toolbox has categories akin to those of Visual Studio, but you
        //- could group them any which way. Just make sure your IToolboxService knows.
        CategoryNameCollection IToolboxService.CategoryNames {
            get {
                return null;
            }
        }

        //- necessary for the Drag&Drop
        //- We deserialize a ToolboxItem when we drop it onto our design surface.
        //- The ToolboxItem comes packaged in a DataObject. We're just working
        //- with standard tools and one host, so the host parameter is ignored.
        ToolboxItem IToolboxService.DeserializeToolboxItem( object serializedObject, IDesignerHost host ) {
            return ((IToolboxService)this).DeserializeToolboxItem( serializedObject );
        }

        //- We deserialize a ToolboxItem when we drop it onto our design surface.
        //- The ToolboxItem comes packaged in a DataObject.
        ToolboxItem IToolboxService.DeserializeToolboxItem( object serializedObject ) {
            return (ToolboxItem) ((DataObject) serializedObject).GetData( typeof( ToolboxItem ) );
        }

        //- Return the selected ToolboxItem in our toolbox if it is associated with this host.
        //- Since all of our tools are associated with our only host, the host parameter
        //- is ignored.
        ToolboxItem IToolboxService.GetSelectedToolboxItem( IDesignerHost host ) {
            return ((IToolboxService)this).GetSelectedToolboxItem();
        }

        //- Return the selected ToolboxItem in our Toolbox
        ToolboxItem IToolboxService.GetSelectedToolboxItem() {

            if ( null == Toolbox || null == Toolbox.SelectedItem )
                return null;

            ToolboxItem tbItem = (ToolboxItem) Toolbox.SelectedItem;
            if ( tbItem.DisplayName.ToUpper().Contains( "POINTER") ) 
                return null;


            return tbItem;
        }

        //-  Get all the tools in a category
        ToolboxItemCollection IToolboxService.GetToolboxItems( string category, IDesignerHost host ) {
            //- we have no category
            return ((IToolboxService) this).GetToolboxItems();
        }


        //- Get all of the tools.
        ToolboxItemCollection IToolboxService.GetToolboxItems( string category ) {
            //- we have no category
            return ((IToolboxService) this).GetToolboxItems();
        }


        //- Get all of the tools. We're always using our current host though.
        ToolboxItemCollection IToolboxService.GetToolboxItems( IDesignerHost host ) {
            return ((IToolboxService) this).GetToolboxItems();
        }


        //- Get all of the tools
        ToolboxItemCollection IToolboxService.GetToolboxItems() {
            if ( null == Toolbox ) return null;


            ToolboxItem[] arr = new ToolboxItem[Toolbox.Items.Count];
            Toolbox.Items.CopyTo( arr, 0 );

            return new ToolboxItemCollection( arr );
        }

        //- We are always using standard ToolboxItems, so they are always supported
        bool IToolboxService.IsSupported( object serializedObject, ICollection filterAttributes ) {
            return true;
        }

        //- We are always using standard ToolboxItems, so they are always supported
        bool IToolboxService.IsSupported( object serializedObject, IDesignerHost host ) {
            return true;
        }

        //- Check if a serialized object is a ToolboxItem. In our case, all of our tools
        //- are standard and from a constant set, and they all extend ToolboxItem, so if
        //- we can deserialize it in our standard-way, then it is indeed a ToolboxItem
        //- The host is ignored
        bool IToolboxService.IsToolboxItem( object serializedObject, IDesignerHost host ) {
            return ((IToolboxService) this).IsToolboxItem( serializedObject );
        }



        //- Check if a serialized object is a ToolboxItem. In our case, all of our tools
        //- are standard and from a constant set, and they all extend ToolboxItem, so if
        //- we can deserialize it in our standard-way, then it is indeed a ToolboxItem
        bool IToolboxService.IsToolboxItem( object serializedObject ) {
            //- If we can deserialize it, it's a ToolboxItem.
            if ( ((IToolboxService) this).DeserializeToolboxItem( serializedObject ) != null )
                return true;

            return false;
        }


        //- Refreshes the Toolbox
        void IToolboxService.Refresh() {
            Toolbox.Refresh();
        }

        //- Remove the creator for the specified format, associated with a particular host.
        void IToolboxService.RemoveCreator( string format, IDesignerHost host ) {
            // UNIMPLEMENTED - We aren't handling any non-standard tools here. Our toolset is constant.
            //throw new NotImplementedException();
        }

        //- Remove the creator for the specified format.
        void IToolboxService.RemoveCreator( string format ) {
            // UNIMPLEMENTED - We aren't handling any non-standard tools here. Our toolset is constant.
            //throw new NotImplementedException();
        }

        //- Remove a ToolboxItem from the specified category in our Toolbox.
        void IToolboxService.RemoveToolboxItem( ToolboxItem toolboxItem, string category ) {
            ((IToolboxService) this).RemoveToolboxItem( toolboxItem );
        }

        //- Remove a ToolboxItem from our Toolbox.
        void IToolboxService.RemoveToolboxItem( ToolboxItem toolboxItem ) {
            if ( null == Toolbox ) return ;

            Toolbox.SelectedItem = null;
            Toolbox.Items.Remove( toolboxItem );
        }

       
        //- If your toolbox is categorized, then it's good for others to know
        //- which category is selected.
        string IToolboxService.SelectedCategory {
            get {
                return null;
            }
            set {
                // UNIMPLEMENTED 
            }
        }


        //- This gets called after our IToolboxUser (the designer) ToolPicked method is called.
        //- In our case, we select the pointer. 
        void IToolboxService.SelectedToolboxItemUsed() {
            if ( null == Toolbox ) return;

            Toolbox.SelectedItem = null;
        }


        //- Serialize the toolboxItem necessary for the Drag&Drop
        //- We serialize a toolbox by packaging it in a DataObject
        object IToolboxService.SerializeToolboxItem( ToolboxItem toolboxItem ) {
            //return new DataObject(typeof( ToolboxItem), toolboxItem );
            DataObject dataObject = new DataObject();
            dataObject.SetData( typeof( ToolboxItem ), toolboxItem );
            return dataObject;
        }

        //- If we've got a tool selected, then perhaps we want to set our cursor to do
        //- something interesting when its over the design surface. If we do, then
        //- we do it here and return true. Otherwise we return false so the caller
        //- can set the cursor in some default manor.
        bool IToolboxService.SetCursor() {

            if (null == Toolbox || null == Toolbox.SelectedItem)
                return false;


            //- <Pointer> is not a tool
            ToolboxItem tbItem = (ToolboxItem) Toolbox.SelectedItem;
            if ( tbItem.DisplayName.ToUpper().Contains( "POINTER" ) )
                return false;


            if ( null != Toolbox.SelectedItem ) {
                Cursor.Current = Cursors.Cross;
                return true;
            }

            return false;
        }

        //- Set the selected ToolboxItem in our Toolbox.
        void IToolboxService.SetSelectedToolboxItem( ToolboxItem toolboxItem ) {
            if ( null == Toolbox ) 
                return;
            
            Toolbox.SelectedItem = toolboxItem;
        }

        #endregion
	}
}
