using System;

namespace Aspire.Framework
{
   /// <summary>
	/// A basic implementation of <see cref="IBlackboardDisplayProperties"/> used when calling <see cref="Blackboard.Item.DisplayProperties"/>.
	/// </summary>
	public class BlackboardDisplayProperties : IBlackboardDisplayProperties, ICloneable
    {
        /// <summary>
        /// Gets a boolean indicating whether to use italic font; if null, display property will not change.
        /// </summary>
		public bool? FontItalic { get; set; }

		/// <summary>
		/// Gets a boolean indicating whether to use bold font; if null, display property will not change.
		/// </summary>
		public bool? FontBold { get; set; }

		/// <summary>
		/// Gets the color for text; if null, display property will not change.
		/// </summary>
		public System.Drawing.Color? ForeColor { get; set; }

		/// <summary>
		/// Gets the color for the items' background; if null, display property will not change.
		/// </summary>
		public System.Drawing.Color? BackColor { get; set; }

        /// <summary>
        /// Gets an image to represent the item
        /// </summary>
		public System.Drawing.Image Image { get; set;  }

		/// <summary>
		/// Clone a duplicate 
		/// </summary>
		/// <returns></returns>
		public object Clone()
		{
			var newProperties = new BlackboardDisplayProperties();
			if (Image != null)
				newProperties.Image = Image.Clone() as System.Drawing.Image;
			return newProperties;
		}
	}

}
