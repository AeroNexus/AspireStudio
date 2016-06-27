//------------------------------------------------------------------------------ 

// <copyright company="Microsoft" file="DesignSurfaceManager.cs">

//     Copyright (c) Microsoft Corporation.  All rights reserved.

// </copyright>

//----------------------------------------------------------------------------- 

 

namespace System.ComponentModel.Design { 

  

    using System;

    using System.Collections; 

    using System.ComponentModel;

    using System.ComponentModel.Design;

    using System.Design;

    using System.Diagnostics; 

 

    /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager"]/*'> 

    /// <devdoc> 

    ///     A service container that supports "fixed" services.  Fixed

    ///     services cannot be removed. 

    /// </devdoc>

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")]

    public class DesignSurfaceManager : IServiceProvider, IDisposable { 

 

        private IServiceProvider                        _parentProvider; 

        private ServiceContainer                        _serviceContainer; 

        private ActiveDesignSurfaceChangedEventHandler  _activeDesignSurfaceChanged;

        private DesignSurfaceEventHandler               _designSurfaceCreated; 

        private DesignSurfaceEventHandler               _designSurfaceDisposed;

        private EventHandler                            _selectionChanged;

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.DesignSurfaceManager"]/*'> 

        /// <devdoc>

        ///     Creates a new designer application. 

        /// </devdoc> 

        public DesignSurfaceManager() : this(null) {

        } 

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.DesignSurfaceManager1"]/*'>

        /// <devdoc>

        ///     Creates a new designer application and provides a 

        ///     parent service provider.

        /// </devdoc> 

        public DesignSurfaceManager(IServiceProvider parentProvider) { 

            _parentProvider = parentProvider;

  

            ServiceCreatorCallback callback = new ServiceCreatorCallback(OnCreateService);

            ServiceContainer.AddService(typeof(IDesignerEventService), callback);

        }

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.ActiveDesignSurface"]/*'>

        /// <devdoc> 

        ///     This property should be set by the designer's user interface 

        ///     whenever a designer becomes the active window.  The default

        ///     implementation of this property works with the default 

        ///     implementation of IDesignerEventService to notify interested

        ///     parties that a new designer is now active.  If you provide

        ///     your own implementation of IDesignerEventService you should

        ///     override this property to notify your service appropriately. 

        /// </devdoc>

        public virtual DesignSurface ActiveDesignSurface { 

            get { 

                IDesignerEventService eventService = EventService;

                if (eventService != null) { 

                    IDesignerHost activeHost = eventService.ActiveDesigner;

                    if (activeHost != null) {

                        return activeHost.GetService(typeof(DesignSurface)) as DesignSurface;

                    } 

                }

  

                return null; 

            }

            set { 

                // If we are providing IDesignerEventService, then we are responsible for

                // notifying it of new designers coming into place.  If we aren't

                // the ones providing the event service, then whoever is providing

                // it will be responsible for updating it when new designers 

                // are created.

                // 

                DesignerEventService eventService = EventService as DesignerEventService; 

                if (eventService != null) {

                    eventService.OnActivateDesigner(value); 

                }

            }

        }

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.DesignSurfaces"]/*'>

        /// <devdoc> 

        ///     A collection of design surfaces.  This is offered 

        ///     for convience, and simply maps to IDesignerEventService.

        /// </devdoc> 

        public DesignSurfaceCollection DesignSurfaces {

            get {

                IDesignerEventService eventService = EventService;

                if (eventService != null) { 

                    return new DesignSurfaceCollection(eventService.Designers);

                } 

  

                return new DesignSurfaceCollection(null);

            } 

        }

 

        /// <devdoc>

        ///     We access this a lot. 

        /// </devdoc>

        private IDesignerEventService EventService { 

            get { 

                return GetService(typeof(IDesignerEventService)) as IDesignerEventService;

            } 

        }

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.ServiceContainer"]/*'>

        /// <devdoc> 

        ///     Provides access to the designer application's

        ///     ServiceContainer. This property allows 

        ///     inheritors to add their own services. 

        /// </devdoc>

        protected ServiceContainer ServiceContainer { 

            get {

                if (_serviceContainer == null) {

                    _serviceContainer = new ServiceContainer(_parentProvider);

                } 

                return _serviceContainer;

            } 

        } 

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.ActiveDesignSurfaceChanged"]/*'> 

        /// <devdoc>

        ///     This event is raised when a new design surface gains

        ///     activation.  This is mapped through IDesignerEventService.

        /// </devdoc> 

        public event ActiveDesignSurfaceChangedEventHandler ActiveDesignSurfaceChanged {

            add { 

                if (_activeDesignSurfaceChanged == null) { 

                    IDesignerEventService eventService = EventService;

                    if (eventService != null) { 

                        eventService.ActiveDesignerChanged += new ActiveDesignerEventHandler(OnActiveDesignerChanged);

                    }

                }

                _activeDesignSurfaceChanged += value; 

            }

            remove { 

                _activeDesignSurfaceChanged -= value; 

                if (_activeDesignSurfaceChanged == null) {

                    IDesignerEventService eventService = EventService; 

                    if (eventService != null) {

                        eventService.ActiveDesignerChanged -= new ActiveDesignerEventHandler(OnActiveDesignerChanged);

                    }

                } 

            }

        } 

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.DesignSurfaceCreated"]/*'>

        /// <devdoc> 

        ///     This event is raised when a new design surface is

        ///     created.  This is mapped through IDesignerEventService.

        /// </devdoc>

        public event DesignSurfaceEventHandler DesignSurfaceCreated { 

            add {

                if (_designSurfaceCreated == null) { 

                    IDesignerEventService eventService = EventService; 

                    if (eventService != null) {

                        eventService.DesignerCreated += new DesignerEventHandler(OnDesignerCreated); 

                    }

                }

                _designSurfaceCreated += value;

            } 

            remove {

                _designSurfaceCreated -= value; 

                if (_designSurfaceCreated == null) { 

                    IDesignerEventService eventService = EventService;

                    if (eventService != null) { 

                        eventService.DesignerCreated -= new DesignerEventHandler(OnDesignerCreated);

                    }

                }

            } 

        }

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.DesignSurfaceDisposed"]/*'> 

        /// <devdoc>

        ///     This event is raised when a design surface is disposed. 

        ///     This is mapped through IDesignerEventService.

        /// </devdoc>

        public event DesignSurfaceEventHandler DesignSurfaceDisposed {

            add { 

                if (_designSurfaceDisposed == null) {

                    IDesignerEventService eventService = EventService; 

                    if (eventService != null) { 

                        eventService.DesignerDisposed += new DesignerEventHandler(OnDesignerDisposed);

                    } 

                }

                _designSurfaceDisposed += value;

            }

            remove { 

                _designSurfaceDisposed -= value;

                if (_designSurfaceDisposed == null) { 

                    IDesignerEventService eventService = EventService; 

                    if (eventService != null) {

                        eventService.DesignerDisposed -= new DesignerEventHandler(OnDesignerDisposed); 

                    }

                }

            }

        } 

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.SelectionChanged"]/*'> 

        /// <devdoc> 

        ///     This event is raised when the active designer's

        ///     selection of component set changes.  This is mapped 

        ///     through IDesignerEventService.

        /// </devdoc>

        public event EventHandler SelectionChanged {

            add { 

                if (_selectionChanged == null) {

                    IDesignerEventService eventService = EventService; 

                    if (eventService != null) { 

                        eventService.SelectionChanged += new EventHandler(OnSelectionChanged);

                    } 

                }

                _selectionChanged += value;

            }

            remove { 

                _selectionChanged -= value;

                if (_selectionChanged == null) { 

                    IDesignerEventService eventService = EventService; 

                    if (eventService != null) {

                        eventService.SelectionChanged -= new EventHandler(OnSelectionChanged); 

                    }

                }

            }

        } 

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.CreateDesignSurface"]/*'> 

        /// <devdoc> 

        ///     Public method to create a design surface.

        /// </devdoc> 

        public DesignSurface CreateDesignSurface() {

 

            DesignSurface surface = CreateDesignSurfaceCore(this);

  

            // If we are providing IDesignerEventService, then we are responsible for

            // notifying it of new designers coming into place.  If we aren't 

            // the ones providing the event service, then whoever is providing 

            // it will be responsible for updating it when new designers

            // are created. 

            //

            DesignerEventService eventService = GetService(typeof(IDesignerEventService)) as DesignerEventService;

            if (eventService != null) {

                eventService.OnCreateDesigner(surface); 

            }

  

            return surface; 

        }

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.CreateDesignSurface1"]/*'>

        /// <devdoc>

        ///     Public method to create a design surface.  This method

        ///     takes an additional service provider.  This service 

        ///     provider will be combined with the service provider

        ///     already contained within DesignSurfaceManager.  Service 

        ///     requests will go to this provider first, and then bubble 

        ///     up to the service provider owned by DesignSurfaceManager.

        ///     This allows for services to be tailored for each design 

        ///     surface.

        /// </devdoc>

        public DesignSurface CreateDesignSurface(IServiceProvider parentProvider) {

  

            if (parentProvider == null) {

                throw new ArgumentNullException("parentProvider"); 

            } 

 

            IServiceProvider mergedProvider = new MergedServiceProvider(parentProvider, this); 

 

            DesignSurface surface = CreateDesignSurfaceCore(mergedProvider);

 

            // If we are providing IDesignerEventService, then we are responsible for 

            // notifying it of new designers coming into place.  If we aren't

            // the ones providing the event service, then whoever is providing 

            // it will be responsible for updating it when new designers 

            // are created.

            // 

            DesignerEventService eventService = GetService(typeof(IDesignerEventService)) as DesignerEventService;

            if (eventService != null) {

                eventService.OnCreateDesigner(surface);

            } 

 

            return surface; 

        } 

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.CreateDesignSurfaceCore"]/*'> 

        /// <devdoc>

        ///     Creates an instance of a design surface.  This can be

        ///     overridden to provide a derived version of DesignSurface.

        /// </devdoc> 

        protected virtual DesignSurface CreateDesignSurfaceCore(IServiceProvider parentProvider) {

            return new DesignSurface(parentProvider); 

        } 

 

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.Dispose"]/*'> 

        /// <devdoc>

        ///     Disposes the designer application.

        /// </devdoc>

        public void Dispose() { 

            Dispose(true);

        } 

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.Dispose1"]/*'>

        /// <devdoc> 

        ///     Protected override of Dispose that allows for cleanup.

        /// </devdoc>

        protected virtual void Dispose(bool disposing) {

            if (disposing && _serviceContainer != null) { 

                _serviceContainer.Dispose();

                _serviceContainer = null; 

            } 

        }

  

        /// <include file="doc\DesignSurfaceManager.uex" path='docs/doc[@for="DesignSurfaceManager.GetService"]/*'>

        /// <devdoc>

        ///     Retrieves a service in this design surface's service

        ///     container. 

        /// </devdoc>

        public object GetService(Type serviceType) { 

            if (_serviceContainer != null) { 

                return _serviceContainer.GetService(serviceType);

            } 

            return null;

        }

 

        /// <summary> 

        ///     Private method that demand-creates services we offer.

        /// </summary> 

        /// <param name="container"> 

        ///     The service container requesting the service.

        ///  

        /// <param name="serviceType">

        ///     The type of service being requested.

        /// 

        /// <returns> 

        ///     A new instance of the service.  It is an error to call this with

        ///     a service type it doesn't know how to create 

        /// </returns> 

        private object OnCreateService(IServiceContainer container, Type serviceType) {

  

            if (serviceType == typeof(IDesignerEventService)) {

                return new DesignerEventService();

            }

  

            Debug.Fail("Demand created service not supported: " + serviceType.Name);

            return null; 

        } 

 

        /// <devdoc> 

        ///     Handles the IDesignerEventService event and relays it to

        ///     DesignSurfaceManager's similar event.

        /// </devdoc>

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs e) { 

            Debug.Assert(_activeDesignSurfaceChanged != null, "Should have detached this event handler.");

            if (_activeDesignSurfaceChanged != null) { 

  

                DesignSurface newSurface = null;

                DesignSurface oldSurface = null; 

 

                if (e.OldDesigner != null) {

                    oldSurface = e.OldDesigner.GetService(typeof(DesignSurface)) as DesignSurface;

                } 

 

                if (e.NewDesigner != null) { 

                    newSurface = e.NewDesigner.GetService(typeof(DesignSurface)) as DesignSurface; 

                }

  

                _activeDesignSurfaceChanged(this, new ActiveDesignSurfaceChangedEventArgs(oldSurface, newSurface));

            }

        }

  

        /// <devdoc>

        ///     Handles the IDesignerEventService event and relays it to 

        ///     DesignSurfaceManager's similar event. 

        /// </devdoc>

        private void OnDesignerCreated(object sender, DesignerEventArgs e) { 

            Debug.Assert(_designSurfaceCreated != null, "Should have detached this event handler.");

            if (_designSurfaceCreated != null) {

                DesignSurface surface = e.Designer.GetService(typeof(DesignSurface)) as DesignSurface;

                if (surface != null) { 

                    _designSurfaceCreated(this, new DesignSurfaceEventArgs(surface));

                } 

            } 

        }

  

        /// <devdoc>

        ///     Handles the IDesignerEventService event and relays it to

        ///     DesignSurfaceManager's similar event.

        /// </devdoc> 

        private void OnDesignerDisposed(object sender, DesignerEventArgs e) {

            Debug.Assert(_designSurfaceDisposed != null, "Should have detached this event handler."); 

            if (_designSurfaceDisposed != null) { 

                DesignSurface surface = e.Designer.GetService(typeof(DesignSurface)) as DesignSurface;

                if (surface != null) { 

                    _designSurfaceDisposed(this, new DesignSurfaceEventArgs(surface));

                }

            }

        } 

 

        /// <devdoc> 

        ///     Handles the IDesignerEventService event and relays it to 

        ///     DesignSurfaceManager's similar event.

        /// </devdoc> 

        private void OnSelectionChanged(object sender, EventArgs e) {

            Debug.Assert(_selectionChanged != null, "Should have detached this event handler.");

            if (_selectionChanged != null) {

                _selectionChanged(this, e); 

            }

        } 

  

        /// <devdoc>

        ///     Simple service provider that merges two providers together. 

        /// </devdoc>

        private sealed class MergedServiceProvider : IServiceProvider {

 

            private IServiceProvider _primaryProvider; 

            private IServiceProvider _secondaryProvider;

  

            internal MergedServiceProvider(IServiceProvider primaryProvider, IServiceProvider secondaryProvider) { 

                _primaryProvider = primaryProvider;

                _secondaryProvider = secondaryProvider; 

            }

 

            object IServiceProvider.GetService(Type serviceType) {

  

                if (serviceType == null) {

                    throw new ArgumentNullException("serviceType"); 

                } 

 

                object service = _primaryProvider.GetService(serviceType); 

 

                if (service == null) {

                    service = _secondaryProvider.GetService(serviceType);

                } 

 

                return service; 

            } 

        }

    } 

}

 

 

// File provided for Reference Use Only by Microsoft Corporation (c) 2007.

// Copyright (c) Microsoft Corporation. All rights reserved.