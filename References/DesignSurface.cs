//------------------------------------------------------------------------------ 

// <copyright company="Microsoft" file="DesignSurface.cs">

//     Copyright (c) Microsoft Corporation.  All rights reserved.

// </copyright>

//----------------------------------------------------------------------------- 

 

namespace System.ComponentModel.Design { 

  

    using System;

    using System.Collections; 

    using System.ComponentModel;

    using System.ComponentModel.Design;

    using System.ComponentModel.Design.Serialization;

    using System.Design; 

    using System.Diagnostics;

    using System.Diagnostics.CodeAnalysis; 

    using System.Reflection; 

 

    /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface"]/*'> 

    /// <devdoc>

    ///     A design surface is an object that contains multiple designers

    ///     and presents a user-editable surface for them.

    /// </devdoc> 

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.InheritanceDemand, Name="FullTrust")]

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.LinkDemand, Name="FullTrust")] 

    public class DesignSurface : IDisposable, IServiceProvider { 

 

        private IServiceProvider _parentProvider; 

        private ServiceContainer _serviceContainer;

        private DesignerHost     _host;

        private ICollection      _loadErrors;

        private bool             _loaded; 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.DesignSurface"]/*'> 

        /// <devdoc> 

        ///     Creates a new DesignSurface.

        /// </devdoc> 

        public DesignSurface() : this((IServiceProvider)null) {

        }

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.DesignSurface1"]/*'> 

        /// <devdoc>

        ///     Creates a new DesignSurface given a parent service 

        ///     provider. 

        /// </devdoc>

        /// <param name="parentProvider"> 

        ///     The parent service provider.  If there is no parent

        ///     used to resolve services this can be null.

        /// 

        public DesignSurface(IServiceProvider parentProvider) { 

 

            _parentProvider = parentProvider; 

            _serviceContainer = new DesignSurfaceServiceContainer(_parentProvider); 

 

            // Configure our default services 

            //

            ServiceCreatorCallback callback = new ServiceCreatorCallback(OnCreateService);

            ServiceContainer.AddService(typeof(ISelectionService), callback);

            ServiceContainer.AddService(typeof(IExtenderProviderService), callback); 

            ServiceContainer.AddService(typeof(IExtenderListService), callback);

            ServiceContainer.AddService(typeof(ITypeDescriptorFilterService), callback); 

            ServiceContainer.AddService(typeof(IReferenceService), callback); 

 

            ServiceContainer.AddService(typeof(DesignSurface), this); 

 

            // And create the host.

            //

            _host = new DesignerHost(this); 

        }

  

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.DesignSurface2"]/*'>

        /// <devdoc> 

        ///     Creates a new DesignSurface.

        /// </devdoc>

        public DesignSurface(Type rootComponentType) : this(null, rootComponentType) {

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.DesignSurface3"]/*'> 

        /// <devdoc> 

        ///     Creates a new DesignSurface given a parent service

        ///     provider. 

        /// </devdoc>

        /// <param name="parentProvider">

        ///     The parent service provider.  If there is no parent

        ///     used to resolve services this can be null. 

        /// 

        public DesignSurface(IServiceProvider parentProvider, Type rootComponentType) : this(parentProvider) { 

            if (rootComponentType == null) { 

                throw new ArgumentNullException("rootComponentType");

            } 

            BeginLoad(rootComponentType);

        }

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.ComponentContainer"]/*'> 

        /// <devdoc>

        ///     Provides access to the design surface's container, which 

        ///     contains all components currently being designed. 

        /// </devdoc>

        public IContainer ComponentContainer { 

            get {

                if (_host == null) {

                    throw new ObjectDisposedException(this.GetType().FullName);

                } 

                return((IDesignerHost)_host).Container;

            } 

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.IsLoaded"]/*'> 

        /// <devdoc>

        ///     Returns true if the design surface is currently loaded.

        ///     This will be true when a successful load has completed,

        ///     or false for all other cases. 

        /// </devdoc>

        public bool IsLoaded { 

            get { 

                return _loaded;

            } 

        }

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.LoadErrors"]/*'>

        /// <devdoc> 

        ///     Returns a collection of LoadErrors or a void collection.

        /// </devdoc> 

        public ICollection LoadErrors { 

            get {

                if (_loadErrors != null) 

                {

                    return _loadErrors;

                }

                return new object[0]; 

 

            } 

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.ServiceContainer"]/*'> 

        /// <devdoc>

        ///     Provides access to the design surface's ServiceContainer.

        ///     This property allows inheritors to add their own services.

        /// </devdoc> 

        protected ServiceContainer ServiceContainer {

            get { 

                if (_serviceContainer == null) { 

                    throw new ObjectDisposedException(this.GetType().FullName);

                } 

 

                return _serviceContainer;

            }

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.View"]/*'> 

        /// <devdoc> 

        ///     This property will return the view for the root

        ///     designer.  BeginLoad must 

        ///     have been called beforehand to start the loading

        ///     process.  It is possible to return a view before

        ///     the designer loader finishes loading because the

        ///     root designer, which supplies the view, is the 

        ///     first object created by the designer loader.  If

        ///     a view is unavailable this method will throw an 

        ///     exception.  Possible exceptions: 

        ///

        ///     The design surface is not loading or the designer 

        ///     loader has not yet created a root designer:

        ///     InvalidOperationException

        ///

        ///     The design surface finished the load, but failed. 

        ///     (Various.  This will throw the first exception

        ///     the designer loader added to the error collection). 

        /// </devdoc> 

        public object View {

            get { 

                Exception ex;

 

                if (_host == null) {

                    throw new ObjectDisposedException(ToString()); 

                }

  

                IComponent rootComponent = ((IDesignerHost)_host).RootComponent; 

 

                if (rootComponent == null) { 

 

                    // Check to see if we have any load errors.  If so, use them.

                    //

                    if (_loadErrors != null) { 

                        foreach(object o in _loadErrors) {

                            ex = o as Exception; 

                            if (ex != null) { 

                                throw new InvalidOperationException(ex.Message, ex);

                            } 

                            else {

                                throw new InvalidOperationException(o.ToString());

                            }

                        } 

                    }

  

                    // loader didn't provide any help.  Just generally fail. 

                    //

                    ex = new InvalidOperationException(SR.GetString(SR.DesignSurfaceNoRootComponent)); 

                    ex.HelpLink = SR.DesignSurfaceNoRootComponent;

                    throw ex;

                }

  

                IRootDesigner rootDesigner = ((IDesignerHost)_host).GetDesigner(rootComponent) as IRootDesigner;

  

                if (rootDesigner == null) { 

                    ex = new InvalidOperationException(SR.GetString(SR.DesignSurfaceDesignerNotLoaded));

                    ex.HelpLink = SR.DesignSurfaceDesignerNotLoaded; 

                    throw ex;

                }

 

                ViewTechnology[] designerViews = rootDesigner.SupportedTechnologies; 

 

                // We just feed the available technologies back into the root 

                // designer.  ViewTechnology itself is outdated. 

                //

                foreach(ViewTechnology availableTech in designerViews) { 

                    return rootDesigner.GetView(availableTech);

                }

 

                // We are out of luck here.  Throw. 

                //

                ex = new NotSupportedException(SR.GetString(SR.DesignSurfaceNoSupportedTechnology)); 

                ex.HelpLink = SR.DesignSurfaceNoSupportedTechnology; 

                throw ex;

            } 

        }

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Disposed"]/*'>

        /// <devdoc> 

        ///     Adds a event handler to listen to the Disposed event on the component.

        /// </devdoc> 

        public event EventHandler Disposed; 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Flushed"]/*'> 

        /// <devdoc>

        ///     Adds a event handler to listen to the Flushed event on the component.

        ///     This is called after the design surface has asked the

        ///     designer loader to flush its state. 

        /// </devdoc>

        public event EventHandler Flushed; 

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Loaded"]/*'>

        /// <devdoc> 

        ///     Called when the designer load has completed.  This

        ///     is called for successful loads as well as

        ///     unsuccessful ones.  If code in this event handler

        ///     throws an exception the designer will be unloaded. 

        /// </devdoc>

        public event LoadedEventHandler Loaded; 

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Loading"]/*'>

        /// <devdoc> 

        ///     Called when the designer load is about to begin

        ///     the loading process.

        /// </devdoc>

        public event EventHandler Loading; 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Unloaded"]/*'> 

        /// <devdoc> 

        ///     Called when the designer has completed the unloading

        ///     process. 

        /// </devdoc>

        public event EventHandler Unloaded;

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Unloading"]/*'> 

        /// <devdoc>

        ///     Called when a designer is about to begin reloading. 

        ///     When a designer reloads, all of the state for that 

        ///     designer is recreated, including the designer's view.

        ///     The view should be unparented at this time. 

        /// </devdoc>

        public event EventHandler Unloading;

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.ViewActivated"]/*'> 

        /// <devdoc>

        ///     Called when someone has called the Activate method on 

        ///     IDesignerHost.  You should attach a handler to this 

        ///     event that activates the window for this design surface.

        /// </devdoc> 

        public event EventHandler ViewActivated;

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.BeginLoad"]/*'>

        /// <devdoc> 

        ///     This method begins the loading process with the

        ///     given designer loader.  Designer loading can be 

        ///     asynchronous, so the loading may continue to 

        ///     progress after this call has returned.  Listen

        ///     to the Loaded event to know when the design 

        ///     surface has completed loading.

        /// </devdoc>

        public void BeginLoad(DesignerLoader loader) {

  

            if (loader == null) {

                throw new ArgumentNullException("loader"); 

            } 

 

            if (_host == null) { 

                throw new ObjectDisposedException(this.GetType().FullName);

            }

 

            // Create the designer host.  We need the host so we can 

            // begin the loading process.

            // 

            _loadErrors = null; 

            _host.BeginLoad(loader);

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.BeginLoad1"]/*'>

        /// <devdoc>

        ///     This method begins the loading process for a 

        ///     component of the given type.  This will create

        ///     an instance of the component type and initialize 

        ///     a designer for that instance.  Loaded is 

        ///     raised before this method returns.

        /// </devdoc> 

        public void BeginLoad(Type rootComponentType) {

 

            if (rootComponentType == null) {

                throw new ArgumentNullException("rootComponentType"); 

            }

  

            if (_host == null) { 

                throw new ObjectDisposedException(this.GetType().FullName);

            } 

 

            BeginLoad(new DefaultDesignerLoader(rootComponentType));

        }

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.CreateComponent"]/*'>

        /// <devdoc> 

        ///     This method is called to create a component of the given type. 

        /// </devdoc>

        [Obsolete("CreateComponent has been replaced by CreateInstance and will be removed after Beta2")] 

        protected internal virtual IComponent CreateComponent(Type componentType) {

            return CreateInstance(componentType) as IComponent;

        }

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.CreateDesigner"]/*'>

        /// <devdoc> 

        ///     This method is called to create a designer for 

        ///     a component.

        /// </devdoc> 

        protected internal virtual IDesigner CreateDesigner(IComponent component, bool rootDesigner) {

 

            if (component == null) {

                throw new ArgumentNullException("component"); 

            }

  

            if (_host == null) { 

                throw new ObjectDisposedException(this.GetType().FullName);

            } 

 

            IDesigner designer = null;

 

            if (rootDesigner) { 

                designer = TypeDescriptor.CreateDesigner(component, typeof(IRootDesigner)) as IRootDesigner;

            } 

            else { 

                designer = TypeDescriptor.CreateDesigner(component, typeof(IDesigner));

            } 

 

            return designer;

        }

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.CreateInstance"]/*'>

        /// <devdoc> 

        ///     This method is called to create an instance of the given type.  If the type is a component 

        ///     this will search for a constructor of type IContainer first, and then an empty constructor.

        /// </devdoc> 

        protected internal virtual object CreateInstance(Type type) {

 

            if (type == null) {

                throw new ArgumentNullException("type"); 

            }

  

            // Locate an appropriate constructor for IComponents. 

            //

            ConstructorInfo ctor = null; 

            object instance = null;

 

 

            ctor = TypeDescriptor.GetReflectionType(type).GetConstructor(new Type[0]); 

 

            if (ctor != null) { 

                instance = TypeDescriptor.CreateInstance(this, type, new Type[0], new object[0]); 

            }

            else { 

                if (typeof(IComponent).IsAssignableFrom(type)) {

                    ctor = TypeDescriptor.GetReflectionType(type).GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.ExactBinding, null, new Type[] { typeof(IContainer) }, null);

                }

                if (ctor != null) { 

                    instance = TypeDescriptor.CreateInstance(this, type, new Type[] { typeof(IContainer) }, new object[] { ComponentContainer });

                } 

            } 

 

            if (instance == null) { 

                instance = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance, null, null, null);

            }

 

            return instance; 

        }

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.CreateNestedContainer"]/*'> 

        /// <devdoc>

        ///     Creates a container suitable for nesting controls or components.  Adding a component to a 

        ///     nested container creates its doesigner and makes it elligble for all all services available from

        ///     the design surface.  Components added to nested containers do not participate in serialization.

        ///     You may provide an additional name for this container by passing a value into containerName.

        /// </devdoc> 

        public INestedContainer CreateNestedContainer(IComponent owningComponent) {

            return CreateNestedContainer(owningComponent, null); 

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.CreateNestedContainer1"]/*'> 

        /// <devdoc>

        ///     Creates a container suitable for nesting controls or components.  Adding a component to a

        ///     nested container creates its doesigner and makes it elligble for all all services available from

        ///     the design surface.  Components added to nested containers do not participate in serialization. 

        ///     You may provide an additional name for this container by passing a value into containerName.

        /// </devdoc> 

        public INestedContainer CreateNestedContainer(IComponent owningComponent, string containerName) { 

 

            if (_host == null) { 

                throw new ObjectDisposedException(this.GetType().FullName);

            }

 

            if (owningComponent == null) { 

                throw new ArgumentNullException("owningComponent");

            } 

  

            return new SiteNestedContainer(owningComponent, containerName, _host);

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Dispose"]/*'>

        /// <devdoc>

        ///     Disposes the design surface. 

        /// </devdoc>

        public void Dispose() { 

            Dispose(true); 

        }

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Dispose1"]/*'>

        /// <devdoc>

        ///     Protected override of Dispose that allows for cleanup.

        /// </devdoc> 

        /// <param name="disposing">

        ///     True if Dispose is being called or false if this 

        ///     is being invoked by a finalizer. 

        /// 

        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")] 

        protected virtual void Dispose(bool disposing) {

            if (disposing) {

 

                // technically we should raise this after we've 

                // destroyed ourselves.  Unfortunately, too many

                // things query us for services so they can detatch. 

                // 

                if (Disposed != null) {

                    Disposed(this, EventArgs.Empty); 

                }

 

                // Destroying the host also destroys all components.

                // In most cases destroying the root component will 

                // destroy its designer which also kills the view.

                // So, we destroy the view below last (remember, this 

                // view is a "view container" so we are destroying 

                // the innermost view first and then destroying our

                // own view). 

                //

                try {

                    try {

                        if (_host != null) { 

                            _host.DisposeHost();

                        } 

                    } 

                    finally {

                        if (_serviceContainer != null) { 

                            _serviceContainer.RemoveService(typeof(DesignSurface));

                            _serviceContainer.Dispose();

                        }

                    } 

                }

                finally { 

                    _host = null; 

                    _serviceContainer = null;

                } 

            }

        }

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.Flush"]/*'> 

        /// <devdoc>

        ///     Flushes any design changes to the underlying loader. 

        /// </devdoc> 

        public void Flush() {

            if (_host != null) { 

                _host.Flush();

            }

 

            if (Flushed != null) { 

                Flushed(this, EventArgs.Empty);

            } 

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.GetService"]/*'> 

        /// <devdoc>

        ///     Retrieves a service in this design surface's service

        ///     container.

        /// </devdoc> 

        /// <param name="serviceType">

        ///     The type of service to retrieve. 

        ///  

        /// <returns>

        ///     An instance of the requested service or null if the service 

        ///     could not be found.

        /// </returns>

        public object GetService(Type serviceType) {

            if (_serviceContainer != null) { 

                return _serviceContainer.GetService(serviceType);

            } 

            return null; 

        }

  

        /// <devdoc>

        ///     Called by the designer host in response to an Activate call on its interface.

        /// </devdoc>

        internal void OnViewActivate() { 

            OnViewActivate(EventArgs.Empty);

        } 

  

        /// <devdoc>

        ///     Private method that demand-creates services we offer. 

        /// </devdoc>

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

  

            if (serviceType == typeof(ISelectionService)) {

                return new SelectionService(container); 

            } 

 

            if (serviceType == typeof(IExtenderProviderService)) { 

                return new ExtenderProviderService();

            }

 

            if (serviceType == typeof(IExtenderListService)) { 

                return GetService(typeof(IExtenderProviderService));

            } 

  

            if (serviceType == typeof(ITypeDescriptorFilterService)) {

                return new TypeDescriptorFilterService(); 

            }

 

            if (serviceType == typeof(IReferenceService)) {

                return new ReferenceService(container); 

            }

  

            Debug.Fail("Demand created service not supported: " + serviceType.Name); 

            return null;

        } 

 

        /// <devdoc>

        ///     This is invoked by the designer host when it has finished the load.

        /// </devdoc> 

        internal void OnLoaded(bool successful, ICollection errors) {

  

            _loaded = successful; 

            _loadErrors = errors;

  

            if (successful) {

                IComponent rootComponent = ((IDesignerHost)_host).RootComponent;

                if (rootComponent == null) {

                    ArrayList newErrors = new ArrayList(); 

                    Exception ex = new InvalidOperationException(SR.GetString(SR.DesignSurfaceNoRootComponent));

                    ex.HelpLink = SR.DesignSurfaceNoRootComponent; 

                    newErrors.Add(ex); 

                    if (errors != null) {

                        newErrors.AddRange(errors); 

                    }

                    errors = newErrors;

                    successful = false;

                } 

            }

            OnLoaded(new LoadedEventArgs(successful, errors)); 

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.OnLoaded"]/*'> 

        /// <devdoc>

        ///     Called when the loading process has completed.  This

        ///     is invoked for both successful and unsuccessful loads.

        ///     The EventArgs passed into this method can be used to 

        ///     tell a successful from an unsuccessful load.  It can also

        ///     be used to create a view for this design surface.  If 

        ///     code in this event handler or override throws an exception, 

        ///     the designer will be unloaded.

        /// </devdoc> 

        // System.Design does not have APTCA

        // SEC

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]

        protected virtual void OnLoaded(LoadedEventArgs e) { 

            if (Loaded != null) {

                Loaded(this, e); 

            } 

        }

  

        /// <devdoc>

        ///     Called when the loading process is about to begin.

        /// </devdoc>

        internal void OnLoading() { 

            OnLoading(EventArgs.Empty);

        } 

  

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.OnLoading"]/*'>

        /// <devdoc> 

        ///     Called when the loading process is about to begin.

        /// </devdoc>

        // System.Design does not have APTCA

        // SEC 

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]

        protected virtual void OnLoading(EventArgs e) { 

            if (Loading != null) { 

                Loading(this, e);

            } 

        }

 

        /// <devdoc>

        ///     This is invoked by the designer host after it has 

        ///     unloaded a document.

        /// </devdoc> 

        internal void OnUnloaded() { 

            OnUnloaded(EventArgs.Empty);

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.OnUnloaded"]/*'>

        /// <devdoc>

        ///     Called when a designer has finished unloading a 

        ///     document.

        /// </devdoc> 

        // System.Design does not have APTCA 

        // SEC

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] 

        protected virtual void OnUnloaded(EventArgs e) {

            if (Unloaded != null) {

                Unloaded(this, e);

            } 

        }

  

        /// <devdoc> 

        ///     This is invoked by the designer host when it is about to unload a document.

        /// </devdoc> 

        internal void OnUnloading() {

            OnUnloading(EventArgs.Empty);

            _loaded = false;

        } 

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.OnUnloading"]/*'> 

        /// <devdoc> 

        ///     Called when a designer is about to begin reloading.

        ///     When a designer reloads, all of the state for that 

        ///     designer is recreated, including the designer's view.

        ///     The view should be unparented at this time.

        /// </devdoc>

        // System.Design does not have APTCA 

        // SEC

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")] 

        protected virtual void OnUnloading(EventArgs e) { 

            if (Unloading != null) {

                Unloading(this, e); 

            }

        }

 

        /// <include file="doc\DesignSurface.uex" path='docs/doc[@for="DesignSurface.OnViewActivate"]/*'> 

        /// <devdoc>

        ///     Called when someone has called the Activate method on 

        ///     IDesignerHost.  You should attach a handler to this 

        ///     event that activates the window for this design surface.

        /// </devdoc> 

        // System.Design does not have APTCA

        // SEC

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]

        protected virtual void OnViewActivate(EventArgs e) { 

            if (ViewActivated != null) {

                ViewActivated(this, e); 

            } 

        }

  

        /// <devdoc>

        ///     This is a simple designer loader that creates an instance of the

        ///     given type and then calls EndLoad.  If a collection of objects

        ///     was passed, this will simply add those objects to the container. 

        /// </devdoc>

        private class DefaultDesignerLoader : DesignerLoader { 

  

            private Type        _type;

            private ICollection _components; 

 

            public DefaultDesignerLoader(Type type) {

                _type = type;

            } 

 

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] 

            public DefaultDesignerLoader(ICollection components) { 

                _components = components;

            } 

 

            public override void BeginLoad(IDesignerLoaderHost loaderHost) {

                string typeName = null;

                if (_type != null) { 

                    loaderHost.CreateComponent(_type);

                    typeName = _type.FullName; 

                } 

                else {

                    foreach(IComponent component in _components) { 

                        loaderHost.Container.Add(component);

                    }

                }

  

                loaderHost.EndLoad(typeName, true, null);

            } 

  

            public override void Dispose() {

  

            }

        }

    }

} 

 

 

// File provided for Reference Use Only by Microsoft Corporation (c) 2007.

// Copyright (c) Microsoft Corporation. All rights reserved.
