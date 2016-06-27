using System;

namespace Aspire.Core.Utilities
{
	public interface ILogger
	{
		void Flush();
		void Log(string text);		
	}
}
