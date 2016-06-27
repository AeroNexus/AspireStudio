using System;

namespace Aspire.BrowsingUI.TestBench
{
    /// <summary>
    /// This attribute describes the device that will be tested by the test method or class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,AllowMultiple=true)]
    public class DeviceUnderTestAttribute: Attribute
    {
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name"></param>
        public DeviceUnderTestAttribute(string name)
		    {
		      var names = name.Split('.');
          if ( names.Length == 1)
            Name = name;
          else if (names.Length == 2)
          {
            Domain = names[0];
            Name = names[1];
          }
        }

        /// <summary>
        /// The domain of the device-under-test
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// If set to true, indicates that the <see cref="Name"/> property specifies a KIND and that all devices of that KIND should be tested.
        /// </summary>
        public bool IsSpecifiedAsDeviceKind { get; set; }

        /// <summary>
        /// The name of the device-under-test
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of semi-colon separated data messages to which the test bench needs to subscribe
        /// </summary>
        public string NotificationsRequired { get; set; }
    }
}
