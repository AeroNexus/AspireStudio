using System;
using System.ComponentModel;

using System.Collections.Generic;

namespace Aspire.Primitives
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
    public class Frame
    {
		Dcm fromParentDcm, toParentDcm;
		Quaternion fromParentQ, toParentQ;
		Vector3 location;
		bool dirtyFromDcm, dirtyFromQ, dirtyToDcm, dirtyToQ;

		static Frame inertialFrame = new Frame("eci");
		public static Frame InertialFrame { get { return inertialFrame; } }
		static Frame current = inertialFrame; // used as the parent for all subsequent constructors

		public Frame(string name=null, FrameList frames=null)
		{
			dirtyFromDcm=dirtyFromQ=dirtyToDcm=dirtyToQ = false;
			toParentQ = new Quaternion().Identity();
			fromParentQ = new Quaternion().Identity();
			toParentDcm = new Dcm().Identity();
			fromParentDcm = new Dcm().Identity();
			Location = new Vector3();
			Name = name == null ? string.Empty : name;

			Parent = current;
			if (current != null)
			{
				current.list.Add(this);
				current.frames = current.list.ToArray();
			}

			if (frames != null) frames.AddAndCollect(this);
		}

		public string Alias { get; set; }

		public Dcm FromParentDcm
		{
			get
			{
				if (dirtyFromDcm)
				{
					if (!dirtyToDcm)
						toParentDcm.Transpose(ref fromParentDcm);
					else if (!dirtyFromQ)
						fromParentQ.ToDcm(ref fromParentDcm);
					else if (!dirtyToQ)
					{
						toParentQ.ToDcm(ref toParentDcm);
						dirtyToDcm = false;
						toParentDcm.Transpose(ref fromParentDcm);
					}
					dirtyFromDcm = false;
				}
				return fromParentDcm;
			}
			set
			{
				fromParentDcm = value;
				dirtyFromQ = dirtyToQ = dirtyToDcm = true;
			}
		}

		public Quaternion FromParentQ
		{
			get
			{
				if (dirtyFromQ)
				{
					if (!dirtyToQ)
						toParentQ.Conjugate(ref fromParentQ);
					else if (!dirtyFromDcm)
						fromParentDcm.ToQuaternion(ref fromParentQ);
					else if (!dirtyToDcm)
					{
						toParentDcm.ToQuaternion(ref toParentQ);
						dirtyToQ = false;
						toParentQ.Conjugate(ref fromParentQ);
					}
					dirtyFromQ = false;
				}
				return fromParentQ;
			}
			set
			{
				fromParentQ = value;
				dirtyFromDcm = dirtyToQ = dirtyToDcm = true;
			}
		}

		public Vector3 Location
		{
			get { return location; }
			set { location = value; }
		}

		public string Name { get; set; }

		public Frame Parent { get; set; }

		public Dcm ToParentDcm
		{
			get
			{
				if (dirtyToDcm)
				{
					if (!dirtyFromDcm)
						fromParentDcm.Transpose(ref toParentDcm);
					else if (!dirtyToQ)
						toParentQ.ToDcm(ref toParentDcm);
					else if (!dirtyFromQ)
					{
						fromParentQ.ToDcm(ref fromParentDcm);
						dirtyFromDcm = false;
						fromParentDcm.Transpose(ref toParentDcm);
					}
					dirtyToDcm = false;
				}
				return toParentDcm;
			}
			set
			{
				toParentDcm = value;
				dirtyFromDcm = dirtyToQ = dirtyFromQ = true;
			}
		}

		public Quaternion ToParentQ
		{
			get
			{
				if (dirtyToQ)
				{
					if (!dirtyFromQ)
						fromParentQ.Conjugate(ref toParentQ);
					else if (!dirtyToDcm)
						toParentDcm.ToQuaternion(ref toParentQ);
					else if (!dirtyToQ)
					{
						toParentQ.ToDcm(ref toParentDcm);
						dirtyToDcm = false;
						toParentDcm.Transpose(ref fromParentDcm);
					}
					dirtyToQ = false;
				}
				return toParentQ;
			}
			set
			{
				toParentQ = value;
				dirtyFromDcm = dirtyToDcm = dirtyFromQ = true;
			}
		}

		/// <summary>
		/// Sets this frame as the current frame, which is used as the parent for all subsequent constructors
		/// </summary>
		public Frame SetAsCurrent()
		{
			var prev = current;
			current = this;
			return prev;
		}

		public void Sync()
		{
			if (!dirtyFromDcm)
			{
				fromParentDcm.Transpose(ref toParentDcm);
				fromParentDcm.ToQuaternion(ref fromParentQ);
				fromParentQ.Conjugate(ref toParentQ);
				dirtyToDcm = dirtyFromQ = dirtyToQ = false;
			}
		}

		public override string ToString()
		{
			return Name;
		}

		#region IFrame

		/// <summary>
		/// String representation
		/// </summary>
		/// <returns></returns>
		public Frame[] Frames
		{
			get { return frames; }
		} Frame[] frames = new Frame[0];
		List<Frame> list = new List<Frame>();

		/// <summary>
		/// Get a child frame from a parent frame by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Frame GetFrame(string name)
		{
			foreach (var frame in list)
				if (frame.Name == name)
					return frame;
				//else if (frame.BaseName == name)
				//	return frame;
				else if (frame.Alias == name)
					return frame;
			return null;
		}


		/// <summary>
		/// Get a frame from a Frame[] by name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="frames"></param>
		/// <returns></returns>
		public static Frame GetFrame(string name, List<Frame> list)
		{
			foreach (var frame in list)
			{
				if (frame.Name == name)
					return frame;
				//else if (frame.BaseName == name)
				//	return frame;
				else if (frame.Alias == name)
					return frame;
			}
			return null;
		}

		#endregion

	}

	public class FrameList : List<Frame>
	{
		List<Frame> list = new List<Frame>();

		/// <summary>
		/// Add an existing frame to the list
		/// </summary>
		/// <param name="frame">The existing frame.</param>
		public void AddFrame(Frame frame)
		{
			list.Add(frame);
		}

		/// <summary>
		/// Add the frame to the list. Also, add the inertial and any Earth frames to the list.
		/// </summary>
		/// <param name="frame"></param>
		internal void AddAndCollect(Frame frame)
		{
			list.Add(frame);
			CollectAssociatedFrames(frame);
		}

		/// <summary>
		/// Add Earth and other interesting frames from under inertial to this list
		/// </summary>
		public void CollectAssociatedFrames(Frame frame)
		{
			Frame inertial = frame;
			while (inertial.Parent != null)
			{
				inertial = inertial.Parent;
				if (!list.Contains(inertial) )//&& inertial.BaseName != "body")
					list.Add(inertial);
				if (inertial.Parent == null)
				{
					foreach (Frame fr in inertial.Frames)
						if (fr.Name.StartsWith("Earth") && !list.Contains(fr))
							list.Add(fr);
					break;
				}
			}
		}

		/// <summary>
		/// Get a frame from the list
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Frame GetFrame(string name)
		{
			return Frame.GetFrame(name, list);
		}

		/// <summary>
		/// Just a moniker
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				foreach (Frame frame in list)
					if (frame.Name.StartsWith("."))
						frame.Name = name + frame.Name;
			}
		} string name = string.Empty;
	}
}
