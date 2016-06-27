using System;

using System.Reflection;

namespace Aspire.Utilities
{
	/// <summary>
	/// Summary description for DataInfo.
	/// </summary>
	public class DataInfo
	{
		/// <summary>
		/// Get MemberInfo for just Fields and Properties
		/// </summary>
		/// <param name="type">The Type to reflect upon.</param>
		/// <param name="bindingFlags">BindingFlags to winnow the search.</param>
		/// <returns></returns>
		public static MemberInfo[] GetMembers(Type type, BindingFlags bindingFlags)
		{
			BindingFlags bf = bindingFlags;
			bf &= ~BindingFlags.GetField;
			bf &= ~BindingFlags.GetProperty;
			FieldInfo[] fi = type.GetFields(bindingFlags);
			PropertyInfo[] pi = type.GetProperties(bindingFlags);
			MemberInfo[] info = new MemberInfo[fi.Length + pi.Length];
			fi.CopyTo(info, 0);
			pi.CopyTo(info, fi.Length);
			return info;
		}
	}
}
