using System;
using System.Xml.Serialization;

namespace Aspire.Framework
{
	public partial class Blackboard
	{
		/// <summary>
		/// A pair of <see cref="Blackboard.Item"/>s that are used to update the destination from the source 
		/// </summary>
		public class Binding
		{
			enum State { DestinationUnbound, SourceUnbound, Bound };
			State state;

			/// <summary>
			/// <see cref=" Blackboard"/> path to the destination item
			/// </summary>
			[XmlAttribute("dest")]
			public string DestinationName { get; set; }

			/// <summary>
			/// <see cref=" Blackboard"/> path to the source item
			/// </summary>
			[XmlAttribute("src")]
			public string SourceName { get; set; }

			/// <summary>
			/// Destination, <see cref="Item"/>
			/// </summary>
			internal Item Destination
			{
				get { return mDestination; }
				set
				{
					mDestination = value;
					if (mDestination != null)
					{
						state = mSource != null ? State.Bound : State.SourceUnbound;
						DestinationName = mDestination.Path;
					}
				}
			} Item mDestination;
			/// <summary>
			/// Source <see cref="Item"/>
			/// </summary>
			internal Item Source
			{
				get { return mSource; }
				set
				{
					mSource = value;
					if (mSource != null)
					{
						state = mDestination != null ? State.Bound : State.DestinationUnbound;
						SourceName = mSource.Path;
					}
				}
			} Item mSource;

			/// <summary>
			/// String representation
			/// </summary>
			/// <returns></returns>
			public override string ToString()
			{
				if (DestinationName == null)
					return GetType().Name;
				int dot = DestinationName.LastIndexOf('.');
				if (dot == -1)
					return DestinationName;
				else
					return DestinationName.Substring(dot + 1);
			}
			/// <summary>
			/// Update the Destination from the Source
			/// </summary>
			public void Update()
			{
				switch (state)
				{
					case State.DestinationUnbound:
						if ( DestinationName != null )
							Destination = SubscribeExisting(DestinationName);
						break;
					case State.SourceUnbound:
						if ( SourceName != null )
							Source = SubscribeExisting(SourceName);
						break;
					case State.Bound:
						mDestination.Value = Convert.ChangeType(mSource.Value, mDestination.Type);
						break;
				}
			}
		}
	}
}
