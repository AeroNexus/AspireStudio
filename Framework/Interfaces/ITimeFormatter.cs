
namespace Aspire.Framework
{
	/// <summary>
	/// Present time in a specific format
	/// </summary>
	public interface ITimeFormatter
	{
		/// <summary>
		/// Selects one of many formats that the formatter knows
		/// </summary>
		string Format { set; }
		/// <summary>
		/// Format the time is a specific presentation
		/// </summary>
		string FormatTime(Clock clock);
		/// <summary>
		/// Displayed name
		/// </summary>
		string[] Names { get; }
	}
}
