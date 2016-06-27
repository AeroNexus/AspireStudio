using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;

namespace Aspire.Primitives
{
	public class SignalGenerator : Model, IBlackboardMenuItems
	{
		const string category = "Signal";

		List<Signal> signals = new List<Signal>();
		[XmlElement("Signal", typeof(Signal))]
		public List<Signal> Signals { get { return signals; } set { signals = value; } }

		#region Model implementation

		Clock clock;

		public override void Discover(Scenario scenario)
		{
			clock = scenario.Clock;

			base.Discover(scenario);

			foreach (var signal in signals)
			{
				signal.Parent = this;
				Blackboard.Publish(signal);
			}
		}

		public override void Execute()
		{
			double t = clock.ElapsedSeconds;
			foreach (var signal in signals)
				signal.Calculate(t);
		}

		public override void Initialize()
		{
			Execute();
		}

		enum Menu { AddSine, AddCounter, AddArray };
		
		string[] menuItems = new string[] { "Add sine", "Add counter", "Add array" };

		#region IModelHasMenu
		[Browsable(false),XmlIgnore]
		public string[] MenuItems { get { return menuItems; } }

		public void OnMenu(int menuItemIndex)
		{
			Signal signal = null;
			switch ( (Menu)menuItemIndex )
			{
				case Menu.AddSine:
					signal = new SineWave() { Name = "sin" + Signals.Count, Parent = this };
					break;
				case Menu.AddCounter:
					signal = new Counter() { Name = "counter" + Signals.Count, Parent = this };
					break;
				case Menu.AddArray:
					signal = new TestArray() { Name = "test" + Signals.Count, Parent = this };
					break;
			}
			if (signal == null) return;
			Blackboard.Publish(signal);
			signals.Add(signal);
			IsDirty = true;
		}
		#endregion

		#endregion

		[XmlInclude(typeof(SineWave)),XmlInclude(typeof(Counter)),XmlInclude(typeof(TestArray))]
		public class Signal : IPublishable
		{
			const string category = "Signal";
			[Category(category), Blackboard, XmlIgnore, Description("Current value of the signal generator. (double)")]
			public virtual double Value { get; set; }

			public virtual void Calculate(double t) {}

			#region IPublishable Members

			[XmlIgnore,Browsable(false)]
			public string Path { get; set; }

			[Category(category), XmlAttribute("name"), Description("Name of the signal.")]
			public string Name { get; set; }

			[Browsable(false), XmlIgnore]
			public IPublishable Parent { get; set; }

			#endregion
		}

		public class SineWave : Signal
		{
			const string category = "Sine";
			[Category(category), Blackboard, XmlAttribute("amplitude"), DefaultValue(0.0),
			 Description("Amplitude of the sine wave. Half the peak to peak value.")]
			public double Amplitude { get; set; }

			[Category(category), Blackboard, XmlAttribute("bias"), DefaultValue(0.0),
			 Description("Bias or offset of the signal. The constant portion of the signal.")]
			public double Bias { get; set; }

			[Category(category), Blackboard(Units = "Hz"), XmlAttribute("frequency"), DefaultValue(0.0),
			 Description("Frequency of the sine wave, [Hz]")]
			public double Frequency
			{
				get { return mOmega / Constant.TwoPi; }
				set { mOmega = Constant.TwoPi * value; }
			} protected double mOmega;

			[Category(category), Blackboard(Units = "deg"), XmlAttribute("phase"), DefaultValue(0.0),
			 Description("Phase angle added to omega*T, [deg]")]
			public double Phase
			{
				get { return mPhase * Constant.DegPerRad; }
				set { mPhase = value * Constant.RadPerDeg; }
			} protected double mPhase;

			public override void Calculate(double t)
			{
				Value = Amplitude * Math.Sin(mOmega * t + mPhase) + Bias;
			}
		}

		public class Counter : Signal
		{
			const string category = "Counter";

			double Bump(double value)
			{
				value += mIncrement;
				if (mIncrement > 0 && value > mMax) value = mInitial;
				else if (mIncrement < 0 && value < -mMax) value = mInitial;
				Value = value;
				return value;
			}

			public override void Calculate(double t)
			{
				if (Period > 0 && t - lastT < Period) return;
				lastT = t;

				switch (TypeCode)
				{
					case TypeCode.Byte: Byte = (byte)Bump(Byte); break;
					case TypeCode.SByte: Sbyte = (sbyte)Bump(Sbyte); break;
					case TypeCode.UInt16: Ushort = (ushort)Bump(Ushort); break;
					case TypeCode.Int16: Short = (short)Bump(Short); break;
					case TypeCode.UInt32: Uint = (uint)Bump(Uint); break;
					case TypeCode.Int32: Int = (int)Bump(Int); break;
					case TypeCode.UInt64: Ulong = (ulong)Bump(Ulong); break;
					case TypeCode.Int64: Long = (long)Bump(Long); break;
					case TypeCode.Single: Single = (float)Bump(Single); break;
					case TypeCode.Double: Bump(Value); break;
				}
			}

			double lastT;

			[Category(category), Blackboard, XmlAttribute("increment"), DefaultValue(1.0), Description("Amount to add every period.")]
			public double Increment { get { return mIncrement; } set { mIncrement = value; } }
			double mIncrement = 1;

			[Category(category), Blackboard, XmlAttribute("initial"), DefaultValue(0.0), Description("Initial value. Startup or reset value.")]
			public double Initial { get { return mInitial; } set { mInitial = value; } }
			double mInitial;

			[Category(category), Blackboard, XmlAttribute("max"), DefaultValue(1e37), Description("Maximum value. When reached, resets to Initial")]
			public double Max { get { return mMax; } set { mMax = value; } }
			double mMax = 1e37;

			[Category(category), Blackboard, XmlAttribute("period"), DefaultValue(0.0), Description("How often the value is updated, [s]. 0=Clock.StepSize")]
			public double Period { get; set; }

			[Category(category), Blackboard, XmlAttribute("type"), DefaultValue(TypeCode.Empty),
			 Description("What type of operand is counted. Note: Only the blackboard variable with this type and Value are updated.")]
			public TypeCode TypeCode { get; set; }

			[Category(category), Blackboard, XmlIgnore, Description("A 1 byte, unsigned counted value")]
			protected byte Byte;

			[Category(category), Blackboard, XmlIgnore, Description("A 1 byte, signed counted value; also Value")]
			protected sbyte Sbyte;

			[Category(category), Blackboard, XmlIgnore, Description("A 2 byte, unsigned counted value; also Value")]
			protected ushort Ushort;

			[Category(category), Blackboard, XmlIgnore, Description("A 2 byte, signed counted value; also Value")]
			protected short Short;

			[Category(category), Blackboard, XmlIgnore, Description("A 4 byte, unsigned counted value; also Value")]
			protected uint Uint;

			[Category(category), Blackboard, XmlIgnore, Description("A 4 byte, signed counted value; also Value")]
			protected int Int;

			[Category(category), Blackboard, XmlIgnore, Description("An 8 byte, unsigned counted value; also Value")]
			protected ulong Ulong;

			[Category(category), Blackboard, XmlIgnore, Description("An 8 byte, signed counted value; also Value")]
			protected long Long;

			[Category(category), Blackboard, XmlIgnore, Description("A float 32 single precision floating point counted value")]
			protected float Single;
		}

		public class TestArray : SineWave
		{
			const string category = "Array";
			public TestArray()
			{
				FloatValues = new float[3];
				DoubleValues = new double[3];
			}

			public override void Calculate(double t)
			{
				Value = Amplitude * Math.Sin(mOmega * t + mPhase) + Bias;
				value = Value;
				FloatValue = (float)Value - 0.2f;

				DoubleValues[0] = Value + 0.5;
				DoubleValues[1] = 0.8 * Value + 0.5;
				DoubleValues[2] = 1.2 * Value + 0.5;

				FloatValues[0] = FloatValue;
				FloatValues[1] = 0.8f * FloatValue;
				FloatValues[2] = 1.2f * FloatValue;
			}

			[Category(category), Blackboard, XmlIgnore]
			public double value;

			[Category(category), Blackboard, XmlIgnore]
			public float FloatValue;

			[Category(category), Blackboard, XmlIgnore]
			public float[] FloatValues;

			[Category(category), Blackboard, XmlIgnore]
			public double[] DoubleValues;
		}
	}
}
