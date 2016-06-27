using System;
using System.ComponentModel;
using System.Xml.Serialization;

using Aspire.Framework;

namespace Aspire.Primitives
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class FieldOfView
    {
		/// <summary>
		/// Discriminate between several fields of view.
		/// </summary>
		public enum Type {
			/// <summary>
			/// Invalid value indicating user neglected to set a valid value.
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// The FOV is described by the angle from bore sight to the edge of the
			/// Field of View. Sometimes known as the half-angle
			/// </summary>
			Circular,
			/// <summary>
			/// The FOV is described by the angle from bore sight to the center of a
			/// polygon's side, and the number of sides (minimum of 3 sides).
			/// </summary>
			RegularPolygon,
			/// <summary>
			/// The FOV is described by the angular width, W, and the aspect ratio, height/width.
			/// The angular width is measured from bore sight to the center of the
			/// line forming the edge of the field of view.  Care should be taken that
			/// the corner of the rectangle is not further from bore sight than 90 degrees.
			/// </summary>
			Prismatic,
			/// <summary>
			/// The FOV is described by an array of points connected by straight lines.
			/// Each point gives the angle from bore sight to the edge of the Field of
			/// View, and the angle from the alignment direction at which the angle is
			/// measured.
			/// </summary>
			Polygonal
		}

		/// <summary>
		/// Available methods to interpolate between the current point and the next
		/// in the sensor field of view definition.
		/// </summary>
		public enum Interpolation {
			/// <summary>
			/// Invalid method; it has not been set to a valid value.
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// use linear interpolation
			/// </summary>
			Linear,
			/// <summary>
			/// use circular (about boresight) interpolation
			/// </summary>
			Circular,
		}

		/// <summary>
		/// a single point in a field of view definition
		/// </summary>
		public class Point {
			/// <summary>
			/// Default constructor
			/// </summary>
			public Point(){}
			/// <summary>
			/// Construct an Circular point
			/// </summary>
			/// <param name="azimuthDeg">Angle around the boresight, starting at the up vector, [deg]</param>
			/// <param name="fovDeg">Angle from the boresight, [deg]</param>
			public Point(double azimuthDeg, double fovDeg)
			{
				angleAboutBoresight = azimuthDeg*Constant.RadPerDeg;
				angleOffBoresight = fovDeg*Constant.RadPerDeg;
				method = Interpolation.Circular;
			}
			/// <summary>
			/// angle off boresight (radians)
			/// </summary>
			public double angleOffBoresight ;
			/// <summary>
			/// angle about boresight (radians)
			/// </summary>
			public double angleAboutBoresight;
			// rectangular representation of above: mag = angleOffBoresight , atan2(y/x)=angleAboutBoresight 
			/// <summary>
			/// rectangular representation of above: mag = angleOffBoresight , atan2(y/x)=angleAboutBoresight 
			/// </summary>
			public double x;
			/// <summary>
			/// rectangular representation of above: mag = angleOffBoresight , atan2(y/x)=angleAboutBoresight 
			/// </summary>
			public double y;
			/// <summary>
			/// interpolation method to use between this point and the next
			/// </summary>
			public Interpolation method;
		}

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public FieldOfView(){}
		/// <summary>
		/// Construct a Circular FOV
		/// </summary>
		/// <param name="radius">Angular measure from centerline to edge, [deg].</param>
		public FieldOfView( double radius ) {
			this.type = Type.Circular;
			maxRadial = radius;
		}

		/// <summary>
		/// Construct a RegularPolygon FOV
		/// </summary>
		/// <param name="nsides">Number of sides. Must be greater than 2.</param>
		/// <param name="radius">Angular measure from centerline to vertex, [deg].</param>
		public FieldOfView( int nsides, double radius ) {
			this.type = Type.RegularPolygon;
			sides = Math.Max(3,nsides);
			maxRadial = radius;
		}

		/// <summary>
		/// Construct a Prismatic FOV
		/// </summary>
		/// <param name="width">Angular measure from centerline to horizontal edge, [deg].</param>
		/// <param name="aspect">Horizontal/vertical ratio.</param>
		public FieldOfView( double width, double aspect ) {
			this.type = Type.Prismatic;
			this.width = width;
			this.aspect = aspect;
		}

		/// <summary>
		/// Construct a Polygonal FOV
		/// </summary>
		/// <param name="values">Pairs of {angle[deg],radial[deg]} points.</param>
		public FieldOfView( params double[] values ) {
			this.type = Type.Polygonal;
			int n = values.Length/2;
			points = new Point[n];
			int j=0;
			for( int i=0; i<n; i++ ) {
				points[i].angleAboutBoresight  = values[j++];
				points[i].angleOffBoresight  = values[j++];
				if ( points[i].angleOffBoresight  > maxRadial )
					maxRadial = points[i].angleOffBoresight ;
			}
		}

		#endregion 

		/// <summary>
		/// This event is raised whenever a property changes.
		/// </summary>
		public event EventHandler Changed;

		/// <summary>
		/// Aspect ratio, horizontal to vertical used for Prismatic.
		/// </summary>
		[Blackboard(Description="Aspect ratio, horizontal to vertical used for Prismatic")]
		[DefaultValue(1), XmlAttribute("aspect")]
		public double Aspect
		{
			get { return aspect; }
			set { aspect = value; OnChanged(); }
		} double aspect=1;

		/// <summary>
		/// Type of geometry
		/// </summary>
		[Blackboard(Description="Type of geometry")]
		[DefaultValue(Type.Circular),XmlAttribute("type")]
		public Type FovType
		{
			get { return type; }
			set {
				type = value;
				switch ( type ) {
					case Type.RegularPolygon:
						if ( sides < 3 )
							sides = 3;
						break;
					case Type.Prismatic:
						sides = 4;
						if ( width == 0 )
							width = maxRadial;
						if ( width == 0 )
							width = 1;
						break;
				}
				OnChanged();
			}
		} Type type=Type.Circular;

		/// <summary>
		/// Also used as the maximum angle off boresight for the
		/// RegularPolygon and Prismatic FOVs.
		/// </summary>
		[Blackboard(Description="Maximum angle off boresight for the RegularPolygon and Prismatic"
			 ,Units="deg")]
		[XmlAttribute("maxRadial")]
		public double MaxRadial
		{
			get { return maxRadial; }
			set { maxRadial = value; OnChanged(); }
		} double maxRadial;

		/// <summary>
		/// Angle from bore sight to edge of FOV, used in Circular FOV, [deg].
		/// </summary>
		[Blackboard(Description="Angle from bore sight to edge of FOV, used in Circular FOV", Units="deg")]
		[XmlAttribute("radius")]
		public double Radius {
			get { return maxRadial; }
			set { maxRadial = value; OnChanged(); }
		}

		/// <summary>
		/// Number of sides in a RegularPolygon FOV.
		/// </summary>
		[Blackboard(Description="Number of sides in a RegularPolygon FOV")]
		[DefaultValue(0), XmlAttribute("sides")]
		public int Sides
		{
			get { return sides; }
			set { sides = value; OnChanged(); }
		} int sides;

		/// <summary>
		/// Width, angle from boresight to center of side of field of view,
		/// used by Prismatic FOV.
		/// </summary>
		[Blackboard(Description="Width, angle from boresight to center of side of field of view",Units="deg")]
		[DefaultValue(0),XmlAttribute("width")]
		public double Width
		{
			get { return width; }
			set { width = value; OnChanged(); }
		} double width;

		/// <summary>
		/// Points used in Polygonal
		/// </summary>
		[Blackboard(Description="Points used in Polygonal")]
		public Point[] Points {
			get { return points; }
			set { points = value; OnChanged(); }
		} Point[] points;

		void OnChanged() {
			if ( Changed != null )
				Changed(this,EventArgs.Empty);
		}
	}
}

