
namespace Aspire.Core.xTEDS
{
	public struct OpStr
	{
		public OpStr(StructuredQuery.sq _op) { op = _op; str = null; i = 0; }
		public OpStr(string _str) { op = StructuredQuery.sq.End; str = _str; i = 0; }
		public OpStr(int _i) { op = StructuredQuery.sq.End; str = null; i = _i; }
		public StructuredQuery.sq op;
		public string str;
		public int i;
	}

	public partial class StructuredQuery
	{

		public enum sq
		{
			ccFirst=' ',
			// Elements
			Xteds=128,
			Elements=Xteds,
			Component, Application, Device, Interface, Command, Notification, Request,
			Message, CommandMsg, DataMsg, DataReplyMsg, FaultMsg, Variable, VariableRef, Qualifier,
			Location, Orientation, Drange, Option, Curve, Coef,
			// Attributes
			// common to more than one class.
			Name,
			Attributes=Name,
			Description, Id, Kind, Units, Value, Version,
			// Component
			ManufacturerId,
			// Application
			Architecture, OperatingSystem, PathForAssembly, PathOnSpacecraft, MemoryMinimum,
			// Device
			CalDueDate, CalibrationDate, DirectionXYZ,
			ElectricalOutput, MeasurementRange, ModelId,
			PowerRequirements, QualityFactor,
			ReferenceFrequency, ReferenceTemperature, SensitivityAtReference, SerialNumber,
			SpaHub, SpaPort, TemperatureCoefficient, VersionLetter,
			// coef
			Exponent,
			// Component
			Address, CompUid, XtedsUid,
			// curve - all global ops
			// drange - all global ops
			// interface
			Extends, Scope,
			// location
			X, Y, Z,
			// message
			MsgId,
			// data message
			MsgArrival, MsgRate,
			// option
			Alarm,
			// orientation
			Angle, Axis,
			// qualifier - all global ops
			//variable
			Accuracy, DefaultValue, Format, InvalidValue, Length, LengthStr,
			Precision, RHigh, RLow, RangeMax, RangeMin, ScaleFactor, ScaleUnits, YHgh, YLow,
			//variable ref - all global ops
			// xTEDS - all global ops
			// Ops
			Up,
			Ops=Up,
			Push, Pop, Peek,
			MatchLocation, Deliver, ElementName, String, MessageSpec,
			StartsWith, EndsWith, Equals, Contains, NoCase, Within,
			NotEqual, LessThan, LessThanEqual, GreaterThan, GreaterThanEqual,
			LastOp, End=0
		}
	}
}
