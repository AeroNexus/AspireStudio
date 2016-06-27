using System;
using System.Collections.Generic;
using System.ComponentModel;

using Aspire.Core.Messaging;
using Aspire.Core.Utilities;
using Aspire.Core.xTEDS;
using Aspire.Utilities;

namespace Aspire.CoreModels
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public static class PnPBrowsers
	{
		static Dictionary<string, IPnPBrowser> browsersByDomain = new Dictionary<string, IPnPBrowser>();
		static Dictionary<string, IPnPBrowser> browsersByName = new Dictionary<string, IPnPBrowser>();
		static List<IPnPBrowser> browsers = new List<IPnPBrowser>();

		// These are static so that clients may UseComponent or Subscribe prior to the browser's creation
		public static event EventHandler ComponentAvailable;
		static Dictionary<string, string> usageRequests = new Dictionary<string, string>();

		static public void Add(IPnPBrowser browser, bool dontRegister=false)
		{
		  if (browsers.Contains(browser)) return;

			browsers.Add(browser);
			browsersByName.Add(browser.Name, browser);
			if (!dontRegister && !browsersByDomain.ContainsKey(browser.DomainName))
				browsersByDomain.Add(browser.DomainName, browser);
		}

		public static void AddUsage(string key, string componentName)
		{
			lock (usageRequests)
				if (!usageRequests.ContainsKey(key))
					usageRequests[key] = componentName;
		}

		public static AspireComponent AllComponent(string name)
		{
			foreach (var browser in browsers)
			{
				var comp = browser[name];
				if (comp != null) return comp;
			}
			return null;
		}

		public static List<AspireComponent> AllComponents
		{
			get
			{
				var allComponents = new List<AspireComponent>();
				browsers.ForEach(browser =>
					browser.Components.ForEach(comp =>
						allComponents.Add(comp)));
				return allComponents;
			}
		}

		public static List<IPnPBrowser> Browsers { get { return browsers; } }

		public static IPnPBrowser ByDomain(string domainName, string label=null)
		{
			IPnPBrowser browser = null;
			if (!browsersByDomain.TryGetValue(domainName, out browser))
				if (label != null )
					Log.WriteLine("{0}: Can't find browser supporting domain: {1}", label, domainName);
			return browser;
		}

		public static IPnPBrowser ByName(string browserName, string label)
		{
			IPnPBrowser browser = null;
			if (!browsersByName.TryGetValue(browserName, out browser))
				Log.WriteLine("{0}: Can't find browser named {1}", label, browserName);
			return browser;
		}

		public static void BrowserSubscribe(string browserName, string componentName, string interfaceName, string messageName)
		{
			var browser = ByName(browserName, "BrowserSubscribe");
			if (browser != null)
				browser.Subscribe(componentName, interfaceName, messageName);
		}

		public static void DomainSubscribe(string domainName, string componentName, string interfaceName, string messageName)
		{
			var browser = ByDomain(domainName, "DomainSubscribe");
			if (browser != null)
				browser.Subscribe(componentName, interfaceName, messageName);
		}

		public static void RaiseComponentAvailable(AspireComponent comp)
		{
			if (ComponentAvailable != null)
				ComponentAvailable(comp, EventArgs.Empty);
		}

		public static void RemoveUsage(string key)
		{
			lock (usageRequests)
				usageRequests.Remove(key);
		}

		public static void Unload()
		{
			//foreach (var browser in browsers)
			//	browser.Unload();
			browsersByDomain.Clear();
			browsersByName.Clear();
			browsers.Clear();
		}

		public static bool UsageRequestsContain(string key)
		{
			lock (usageRequests)
				if (usageRequests.ContainsKey(key))
					return true;
			return false;
		}

		/// <summary>
		/// A client wants to insure that a component is loaded and available for messaging
		/// </summary>
		/// <param name="domainName">Which domain the component belongs to</param>
		/// <param name="componentName">The component name</param>
		public static void UseComponent(string browserName, string componentName)
		{
			IPnPBrowser browser = null;
			if (!browsersByName.TryGetValue(browserName, out browser))
			{
				Log.WriteLine(
					"UseComponent({0},{1}): Can't find browser {0}",
					browserName, componentName);
				return;
			}
			browser.UseComponent(componentName);
		}

	}
}
