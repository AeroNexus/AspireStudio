using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

using Aspire.Studio.DockedViews;
using Aspire.Utilities;

namespace Aspire.Studio.Dashboard
{
	public class DesignSurfaceManager2 : DesignSurfaceManager
	{
		private const string _Name_ = "DesignSurfaceManager2";

		//- this List<> is necessary to be able to delete the DesignSurfaces previously created
		//- Note: 
		//-     the DesignSurfaceManager.DesignSurfaces Property is a collection of design surfaces 
		//-     that are currently hosted by the DesignSurfaceManager but is readonly
		private List<DesignSurface2> DesignSurface2Collection = new List<DesignSurface2>();

		private IPropertiesView propertiesView;

		#region ctors
		//- ctors
		// Summary:
		//     Initializes a new instance of the System.ComponentModel.Design.DesignSurfaceManager
		//     class.
		public DesignSurfaceManager2() : base() { Init();  }
		//
		// Summary:
		//     Initializes a new instance of the System.ComponentModel.Design.DesignSurfaceManager
		//     class.
		//
		// Parameters:
		//   parentProvider:
		//     A parent service provider. Service requests are forwarded to this provider
		//     if they cannot be resolved by the design surface manager.
		public DesignSurfaceManager2( IServiceProvider parentProvider ) : base( parentProvider ) { Init(); }

		//-
		private void Init(){

			ActiveDesignSurfaceChanged += ( object sender, ActiveDesignSurfaceChangedEventArgs e ) =>
			{
				DesignSurface2 surface = e.NewSurface as DesignSurface2;
				if ( null == surface )
					return;

				propertiesView = GetService(typeof(IPropertiesView)) as IPropertiesView;

				UpdatePropertiesView( surface );
			};
		}

		public void UpdatePropertiesView(DesignSurface2 surface)
		{
			IDesignerHost host = (IDesignerHost) surface.GetService( typeof( IDesignerHost ) );
			if ( null == host)
				return;
			if ( null == host.RootComponent ) 
				return;

			//- sync the PropertyGridHost
			propertiesView.SelectedObject = host.RootComponent;
			propertiesView.ReloadSelectableObjects();
		}

		#endregion

		//- The CreateDesignSurfaceCore method is called by both CreateDesignSurface methods
		//- It is the implementation that actually creates the design surface
		//- The default implementation just returns a new DesignSurface, we override 
		//- this method to provide a custom object that derives from the DesignSurface class
		//- i.e. a new DesignSurface2 instance
		protected override DesignSurface CreateDesignSurfaceCore( IServiceProvider parentProvider ) {
			return new DesignSurface2( parentProvider );
		}

		//- Gets a new DesignSurface2 
		//- and loads it with the appropriate type of root component. 
		public DesignSurface2 CreateDesignSurface2() {
			//- with a DesignSurfaceManager class, is useless to add new services 
			//- to every design surface we are about to create,
			//- because of the "IServiceProvider" parameter of CreateDesignSurface(IServiceProvider) Method.
			//- This param let every design surface created 
			//- to use the services of the DesignSurfaceManager.
			//- A new merged service provider will be created that will first ask 
			//- this provider for a service, and then delegate any failures 
			//- to the design surface manager object. 
			//- Note:
			//-     the following line of code create a brand new DesignSurface which is added 
			//-     to the Designsurfeces collection, 
			//-     i.e. the property "DesignSurfaces" ( the .Count in incremented by one)
			var surface = CreateDesignSurface( ServiceContainer ) as DesignSurface2;

			//- each time a brand new DesignSurface is created,
			//- subscribe our handler to its SelectionService.SelectionChanged event
			//- to sync the PropertyGridHost
			ISelectionService selectionService = (ISelectionService)(surface.GetService(typeof(ISelectionService)));
			if ( null != selectionService ) {
				selectionService.SelectionChanged += ( object sender, EventArgs e ) =>
				{
					ISelectionService selectService = sender as ISelectionService;
					if ( null == selectService )
						return;

					if ( 0 == selectService.SelectionCount )
						return;

					//- Sync the PropertyGridHost
					PropertyGrid propertyGrid = (PropertyGrid) GetService( typeof( PropertyGrid ) );
					if ( null == propertyGrid )
						return;

					var comps = selectService.GetSelectedComponents();
					object[] array = new object[comps.Count];
					comps.CopyTo(array, 0);
					propertyGrid.SelectedObjects = array;
				};
			}
			DesignSurface2Collection.Add( surface );
			ActiveDesignSurface = surface;

			//- and return the DesignSurface (to let the its BeginLoad() method to be called)
			return surface;
		}

		public void DeleteDesignSurface(DesignSurface2 item) {
			DesignSurface2Collection.Remove( item );
			try {
				item.Dispose();
			}
			catch( Exception ex) {
				Log.ReportException(ex,"DeleteDesignSurface");
			}
			int currentIndex = DesignSurface2Collection.Count - 1;
			if ( currentIndex >= 0 )
				ActiveDesignSurface = DesignSurface2Collection[currentIndex];
			else
				ActiveDesignSurface = null;
		}

		public void DeleteDesignSurface( int index  ) {
			DesignSurface2 item = DesignSurface2Collection[index];
			DesignSurface2Collection.RemoveAt( index );
			try {
				item.Dispose();
			}
			catch( Exception ex ) {
				System.Diagnostics.Debug.WriteLine( ex.Message );
			}
			int currentIndex = DesignSurface2Collection.Count - 1;
			if ( currentIndex >= 0 )
				ActiveDesignSurface = DesignSurface2Collection[currentIndex];
			else
				ActiveDesignSurface = null;
		}


		//- loop through all the collection of DesignSurface 
		//- to find out a brand new Form name
		public string GetValidFormName() {
			//- we choose to use "Form_" with an underscore char as trailer 
			//- because the .NET design services provide a name of type: "FormN"
			//- with N=1,2,3,4,...
			//- therefore using a "Form", without an underscore char as trailer,
			//- cause some troubles when we have to decide if a name is used or not
			//- using a different building pattern (with the underscore) avoid this issue
			string newFormNameHeader  = "Site_";
			int newFormNametrailer = -1;
			string newFormName = string.Empty;
			bool isNew = true;
			do {
				isNew = true;
				newFormNametrailer++;
				newFormName = newFormNameHeader + newFormNametrailer;
				foreach( DesignSurface2 item in DesignSurface2Collection ) {
					string currentFormName = item.GetIDesignerHost().RootComponent.Site.Name;
					isNew &= ((newFormName == currentFormName) ? false : true);
				}//end_foreach
            
			} while( !isNew ); 
			return newFormName;
		}

		public new DesignSurface2 ActiveDesignSurface {
			get { return base.ActiveDesignSurface as DesignSurface2; }
			set { base.ActiveDesignSurface = value; }
		}
       
	}
}
