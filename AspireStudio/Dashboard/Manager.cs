using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

using Aspire.Studio.DockedViews;
using Aspire.Utilities;

namespace Aspire.Studio.Dashboard
{
	public class DashboardManager : IServiceProvider
	{
		//List<DesignSurface2> surfaces = new List<DesignSurface2>();
		PropertiesView mPropertiesView;
		Toolbox mToolbox;

		public DashboardManager(PropertiesView propertiesView, Toolbox toolbox)
		{
			mPropertiesView = propertiesView;
			mToolbox = toolbox;
			SurfaceManager = new DesignSurfaceManager2(this);
		}

		public static DesignSurfaceManager2 SurfaceManager { get; private set; }

		#region IServiceProvider Members

		public object GetService(Type serviceType)
		{
			if (serviceType == typeof(IToolboxService))
				return mToolbox;
			else if (serviceType == typeof(IPropertiesView))
				return mPropertiesView;
			return null;
 		}
       

		#endregion

		class SomeService
		{
		}
	}
}
