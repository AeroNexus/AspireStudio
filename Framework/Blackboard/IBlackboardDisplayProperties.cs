//using System;

namespace Aspire.Framework
{
   /// <summary>
    /// Interface for specifying optional display properties for a <see cref="Blackboard.Item"/>.
    /// </summary>
    public interface IBlackboardDisplayProperties
    {
        /// <summary>
        /// Gets a boolean indicating whether to use italic font; if null, display property will not change.
        /// </summary>
        bool? FontItalic { get; }

		/// <summary>
		/// Gets a boolean indicating whether to use bold font; if null, display property will not change.
		/// </summary>
		bool? FontBold { get; }

		/// <summary>
		/// Gets the color for text; if null, display property will not change.
		/// </summary>
		System.Drawing.Color? ForeColor { get; }

		/// <summary>
		/// Gets the color for the items' background; if null, display property will not change.
		/// </summary>
		System.Drawing.Color? BackColor { get; }

        /// <summary>
        /// Gets an image to represent the item
        /// </summary>
        System.Drawing.Image Image { get; }
	}
}
