using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace Aspire.Studio.Dashboard
{
	public class MenuCommandService2 : IMenuCommandService
	{
        //- this ServiceProvider is the DesignsurfaceExt2 instance 
        //- passed as paramter inside the ctor
        IServiceProvider _serviceProvider = null;

        MenuCommandService _menuCommandService = null;

        public MenuCommandService2( IServiceProvider serviceProvider ) {
            _serviceProvider = serviceProvider;
            _menuCommandService = new MenuCommandService( serviceProvider );
        }
        
        public void ShowContextMenu( CommandID menuID, int x, int y ) {
            var contextMenu = new ContextMenu();

            // Add the standard commands CUT/COPY/PASTE/DELETE
            var command = FindCommand( StandardCommands.Cut );
            if ( command != null ) {
				var menuItem = new MenuItem("Cut", new EventHandler(OnMenuClicked)) { Tag = command };
                contextMenu.MenuItems.Add( menuItem );
            }
            command = FindCommand( StandardCommands.Copy );
            if ( command != null ) {
				var menuItem = new MenuItem("Copy", new EventHandler(OnMenuClicked)) { Tag = command };
                contextMenu.MenuItems.Add( menuItem );
            }
            command = FindCommand( StandardCommands.Paste );
            if ( command != null ) {
				var menuItem = new MenuItem("Paste", new EventHandler(OnMenuClicked)) { Tag = command };
                contextMenu.MenuItems.Add( menuItem );
            }
            command = FindCommand( StandardCommands.Delete );
            if ( command != null ) {
				var menuItem = new MenuItem("Delete", new EventHandler(OnMenuClicked)) { Tag = command };
                contextMenu.MenuItems.Add( menuItem );
            }

            //- Show the contexteMenu
            var surface = _serviceProvider as DesignSurface;
            var viewService = surface.View as Control;
            
            if ( viewService != null ) {
                contextMenu.Show( viewService, viewService.PointToClient( new Point( x, y ) ) );
            }
        }

        //- Management of the selections of the contexteMenu
        private void OnMenuClicked( object sender, EventArgs e ) {
            var menuItem = sender as MenuItem;
            if ( menuItem != null && menuItem.Tag is MenuCommand ) {
                var command = menuItem.Tag as MenuCommand;
                command.Invoke();
            }
        }


        public void AddCommand( MenuCommand command ) {
            _menuCommandService.AddCommand( command );
        }


        public void AddVerb( DesignerVerb verb ) {
            _menuCommandService.AddVerb( verb );
        }


        public MenuCommand FindCommand( CommandID commandID ) {
            return _menuCommandService.FindCommand( commandID );
        }


        public bool GlobalInvoke( CommandID commandID ) {
            return _menuCommandService.GlobalInvoke( commandID );
        }


        public void RemoveCommand( MenuCommand command ) {
            _menuCommandService.RemoveCommand( command );
        }

    
        public void RemoveVerb( DesignerVerb verb ) {
            _menuCommandService.RemoveVerb( verb );
        }


        public DesignerVerbCollection Verbs {
            get {
                return _menuCommandService.Verbs;
            }
        }
	}
}
