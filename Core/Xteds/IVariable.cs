using System;
using System.Reflection;

using Aspire.Core.Utilities;

namespace Aspire.Core.xTEDS
{
	public interface IVariable
	{
		void Get(byte[] buf, int offset, MemberInfo info);
		TypeCode TypeCode { get; }
		int Length { get; }
		string Kind { get; }
		string Name { get; }
		void NotifyOnChange();
		int Size { get; }
		PrimitiveType Type { get; }
		object Value { get; }
	}
}
