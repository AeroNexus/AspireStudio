using System;

namespace Aspire.BrowsingUI.TestBench
{
	/// <summary>
	/// 
	/// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PublishTestResultAttribute: Attribute
    {
		/// <summary>
		/// 
		/// </summary>
        public PublishTestResultAttribute()
        { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="enabled"></param>
        public PublishTestResultAttribute(bool enabled)
            : this()
        {
            IsEnabled = enabled;
        }

		/// <summary>
		/// 
		/// </summary>
        public string TestIdentifier { get; set; }

		/// <summary>
		/// 
		/// </summary>
        public bool IsEnabled { get; set; }

		/// <summary>
		/// 
		/// </summary>
        public bool IsDeviceUidSpecific { get; set; }

		/// <summary>
		/// 
		/// </summary>
        public bool IsKindSpecific { get; set; }

    }
}
