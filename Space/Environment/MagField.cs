using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Primitives;
using Aspire.Studio;
using Aspire.Utilities;

namespace Aspire.Space
{
	public class MagField : EnvironmentModel
	{
		/// <summary>
		/// Ways the secular field is updated
		/// </summary>
		public enum Secular
		{
			/// <summary>
			/// Uses the Epoch date of the reference data
			/// </summary>
			Epoch,
			/// <summary>
			/// Uses the clock initial date once during initialization
			/// </summary>
			InitialTime,
			/// <summary>
			/// Uses secular derivatives continuously
			/// </summary>
			Dynamic
		};

		#region Declarations
		const double IgrfEquatorialRadius = 6371200.0;

		SphericalHarmonics sphHarm;

		[Description("Filename that coefficients loaded from")]
		string epochFileName = string.Empty;

		[Description("Epoch year of the coefficient data, [year]")]
		int epochYear;

		[Description("Epoch order of the coefficient data")]
		int epochOrder;

		AstroClock astroClock;
		Clock clock;

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			if (!(Parent is Environment))
				Environment.AddModel(this);

			sphHarm = new SphericalHarmonics(SphericalHarmonics.Normal.Schmidt, true, order, IgrfEquatorialRadius);
			clock = scenario.Clock;
			astroClock = clock.GetService(typeof(AstroClock)) as AstroClock;
			Enabled = false;

			base.Discover(scenario);
		}

		public override void Initialize()
		{
			string epochFile;

			if (epochFileName == string.Empty)
				epochFile = FindEpochFile(clock.DateTime.Year, ref epochYear);
			else
			{
				epochFile = epochFileName;
				// Clear the name so that if an error occurs, it won't lie
				epochFileName = string.Empty;
			}
			LoadCoefficients(epochFile, true);
			if (order == 0)
				order = epochOrder;
			if (order > 0)
				sphHarm.Order = order;

			if (secular == Secular.InitialTime)
				sphHarm.DateMjd = astroClock.ModifiedJulianDate;

			base.Initialize();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Access the requested epoch year
		/// </summary>
		[XmlAttribute("epoch")]
		[DefaultValue(0)]
		[Description("Epoch year used to load epoch coefficients, [year]")]
		//[Blackboard]
		public int Epoch
		{
			get { return epoch; }
			set
			{
				int oldEpoch = epoch;
				int year = (value / 5) * 5;
				if (epoch == 0)
					epoch = year;
				if (year != oldEpoch && sphHarm != null)
				{
					if (LoadCoefficients(FindEpochFile(year, ref epoch), false))
					{
						if (secular == Secular.InitialTime)
							sphHarm.DateMjd = astroClock.ModifiedJulianDate;
						Execute();
					}
					else
						epoch = 0;
				}
			}
		} int epoch;

		/// <summary>
		/// Load any epoch coefficient file
		/// </summary>
		[XmlAttribute("epochFile")]
		[DefaultValue("")]
		[Description("Coefficient data file name to force loading")]
		public string EpochFile
		{
			get { return fileName; }
			set
			{
				if (fileName == null)
					fileName = value;
				else if (sphHarm != null)
				{
					if (LoadCoefficients(value, false))
					{
						fileName = epochFileName;
						if (secular == Secular.InitialTime)
							sphHarm.DateMjd = astroClock.ModifiedJulianDate;
						Execute();
					}
					else
						fileName = null;
				}
			}
		} string fileName;

		/// <summary>
		/// Access the runtime order
		/// </summary>
		[XmlAttribute("order")]
		[DefaultValue(0)]
		[Description("Runtime order to evaluate the spherical harmonics. <= epochOrder")]
		public int Order
		{
			get { return order; }
			set
			{
				order = value;
				Limit.Clamp(order, 1, epochOrder);
				if (sphHarm != null)
				{
					sphHarm.Order = order;
					Execute();
				}
			}
		} int order;

		/// <summary>
		/// Access the secular type
		/// </summary>
		[XmlAttribute("secular")]
		[DefaultValue(Secular.InitialTime)]
		[Description("Method of evaluation of spherical harmonics with time. Usage: Epoch-use data as is,InitialTime-interpolate based on initial time,Dynamic-evaluate each frame")]
		public Secular SecularType
		{
			get { return secular; }
			set
			{
				secular = value;
				if (secular == Secular.InitialTime && sphHarm != null)
				{
					sphHarm.DateMjd = astroClock.ModifiedJulianDate;
					Execute();
				}
			}
		} Secular secular = Secular.InitialTime;


		/// <summary>
		/// Access IGRF BField using alt,lat,lon
		/// </summary>
		public double BField(double alt, double lat, double lon, ref Vector3 Bfield)
		{
			Bfield = sphHarm.FieldVector(alt, Constant.HalfPi - lat, lon);
			return Bfield.Magnitude;
		}

		/// <summary>
		/// Set the working date of the Magnetic Field model
		/// </summary>
		public double ModifiedJulianDate { set { sphHarm.DateMjd = value; } }

		#endregion 

		#region File Management

		class DirEntry
		{
			internal int year, order;
			internal string datFile, txtFile;
			internal DirEntry() { }
		}
		class Coefficients
		{
			internal string title;
			internal int order = 0;
			internal double radiusKm, year;
			internal double[,]
				g = new double[0, 0], h = new double[0, 0],
				dg = new double[0, 0], dh = new double[0, 0];
			internal Coefficients() { }
		}

		string FindEpochFile(int year, ref int epochYear)
		{
			int epochLine = -1, epochNdx = -1;
			var dirList = new List<DirEntry>();
			var lines = new List<string>();
			string defaultFileName = string.Empty;
			string indexPath = AppState.InstallDirectory + @"Data\Magnetic Field\Index.txt";

			if (epochYear == 0)
				epochYear = year;
			epochYear = (epochYear / 5) * 5;

			try
			{
				using (StreamReader sr = new StreamReader(indexPath))
				{
					TokenParser parser = new TokenParser();
					string line;
					DirEntry entry = null;
					int lineNo = -1, ndx = -1;
					while ((line = sr.ReadLine()) != null)
					{
						if (line.StartsWith("# Created by SDT")) continue;
						lines.Add(line);
						lineNo++;
						if (line.Length == 0 || line[0] == '#') continue;
						parser.Text = line;
						if (line.StartsWith("default:"))
						{
							parser.Token();
							defaultFileName = parser.Token();
							continue;
						}
						entry = new DirEntry();
						entry.year = Int32.Parse(parser.Token());
						entry.order = Int32.Parse(parser.Token());
						entry.datFile = parser.Token();
						entry.txtFile = parser.Token();
						dirList.Add(entry);
						ndx++;
						if (epochYear <= entry.year)
						{
							if (epochYear < entry.year)
							{
								if (epochNdx > -1) continue;
								Log.WriteLine("MagField.FindEpochFile",
									Log.Severity.Warning,
									"You have requested a magnetic field epoch, {0}, that predates our earliest epoch data, {1}. Defaulting to {1}", epochYear, entry.year, entry.year);
								epochYear = entry.year;
							}
							if (entry.txtFile.Length > 0)
								return entry.txtFile;
							epochNdx = ndx;
							epochLine = lineNo;
						}
					}
					if (entry != null && epochYear > entry.year)
					{
						epochYear = entry.year;
						if (entry.txtFile.Length > 0)
							return entry.txtFile;
						else // just use the 0 derivatives file
							return entry.datFile;
					}
				}
			}
			catch (System.Exception e)
			{
				Log.ReportException(e,
					"MagField.FindEpochFile: Can't find the magnetic field Index.txt file. Copy Index.bak to Index.txt for next run. " +
					"Defaulting to {0}", defaultFileName);
				return defaultFileName;
			}
			Coefficients
				coeffs = new Coefficients(),
				next = new Coefficients();
			// Load the current epoch's coefficients and the next
			DirEntry
				nEntry = dirList[epochNdx + 1] as DirEntry,
				eEntry = dirList[epochNdx] as DirEntry;
			if (!LoadCoeffs(eEntry.datFile, coeffs))
				return defaultFileName;
			if (!LoadCoeffs(nEntry.datFile, next))
				return defaultFileName;
			// Calculate new coefficients
			double dt = next.year - coeffs.year;
			int corder = Math.Min(coeffs.order, next.order);
			for (int n = 1; n <= corder; n++)
				for (int m = 0; m <= n; m++)
				{
					coeffs.dg[n, m] = (next.g[n, m] - coeffs.g[n, m]) / dt;
					coeffs.dh[n, m] = (next.h[n, m] - coeffs.h[n, m]) / dt;
				}
			eEntry.txtFile = eEntry.datFile.Replace(".dat", ".txt");
			if (!SaveCoeffs(eEntry.txtFile, coeffs))
				return defaultFileName;
			// Replace the line in the index lines
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0}\t{1}\t{2}", eEntry.year, eEntry.order, eEntry.datFile);
			if (eEntry.txtFile.Length > 0)
				sb.AppendFormat("\t{0}", eEntry.txtFile);
			lines.RemoveAt(epochLine);
			lines.Insert(epochLine, sb.ToString());
		SaveIt:
			// Stream out a new Index file

			try
			{
				using (StreamWriter sw = new StreamWriter(indexPath))
				{
					sw.WriteLine("#  Created by SDT on {0}", DateTime.Now.ToString("F"));
					lines.RemoveAll(line => line.StartsWith("#  Created by SDT on"));
					foreach (String line in lines)
						sw.WriteLine(line);
				}
				return eEntry.txtFile;
			}
			catch (System.UnauthorizedAccessException)
			{
				FileAttributes attr = File.GetAttributes(indexPath);
				File.SetAttributes(indexPath, attr &= ~FileAttributes.ReadOnly);
				goto SaveIt;
			}
			catch (System.Exception e)
			{
				Log.ReportException(e, "MagField.FindEpochFile: The file, {0}, could not be read", fileName);
				return defaultFileName;
			}
		}

		bool LoadCoefficients(string fileName, bool overRide)
		{
			Coefficients coeffs = new Coefficients();
			if (fileName.Length == 0 || !LoadCoeffs(fileName, coeffs))
			{
				if (!overRide) return false;
				coeffs.order = 1;
				coeffs.radiusKm = 6371.2;
				coeffs.year = 1975;
				int n = coeffs.order + 1;
				coeffs.g = new double[n, n];
				coeffs.h = new double[n, n];
				coeffs.dg = new double[n, n];
				coeffs.dh = new double[n, n];
				coeffs.g[1, 0] = -30186.0;
				coeffs.g[1, 1] = -2038.0;
				coeffs.h[1, 0] = 0.0;
				coeffs.h[1, 1] = 5735.0;
				fileName = "1975 dipole";
				Log.WriteLine("MagField.LoadCoefficients", Log.Severity.Warning,
					"Last resort, using " + fileName);
			}
			epochFileName = fileName;
			epochOrder = coeffs.order;
			epochYear = (int)coeffs.year;
			sphHarm.Radius = coeffs.radiusKm * Constant.Kilo;
			var epoch = new DateTime((int)coeffs.year, 0, 0);
			sphHarm.Init(epoch.ToModifiedJulianDate(), coeffs.g, coeffs.h, coeffs.dg, coeffs.dh);
			return true;
		}

		enum State { Id, Epoch, Data };
		bool LoadCoeffs(string fileName, Coefficients coeffs)
		{
			TokenParser parser = new TokenParser();
			try
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamReader sr = new StreamReader(
						   AppState.InstallDirectory + @"Data\Magnetic Field\" + fileName))
				{
					String line, token;
					State state = State.Id;
					int m = 0, n = 0;
					while ((line = sr.ReadLine()) != null)
					{
						parser.Text = line;
						switch (state)
						{
							case State.Id:
								coeffs.title = parser.Token();
								state = State.Epoch;
								break;
							case State.Epoch:
								coeffs.order = Int32.Parse(parser.Token());
								int dim = coeffs.order + 1;
								coeffs.g = new double[dim, dim];
								coeffs.h = new double[dim, dim];
								coeffs.dg = new double[dim, dim];
								coeffs.dh = new double[dim, dim];
								coeffs.radiusKm = Double.Parse(parser.Token());
								coeffs.year = Double.Parse(parser.Token());
								state = State.Data;
								break;
							case State.Data:
								if (n == m && n == coeffs.order)
									return true;
								n = Int32.Parse(parser.Token());
								//								n = (int)parser.Token(n);
								m = Int32.Parse(parser.Token());
								if (n > coeffs.order || m > coeffs.order) continue;
								coeffs.g[n, m] = Double.Parse(parser.Token());
								coeffs.h[n, m] = Double.Parse(parser.Token());
								token = parser.Token();
								if (token.Length > 0)
								{
									coeffs.dg[n, m] = Double.Parse(token);
									coeffs.dh[n, m] = Double.Parse(parser.Token());
								}
								break;
						}
					}
				}
				return true;
			}
			catch (Exception e)
			{
				Log.ReportException(e, "MagField.LoadCoeffs: The file, {0}, could not be read", fileName);
			}
			return false;
		}

		bool SaveCoeffs(string fileName, Coefficients coeffs)
		{
			try
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamWriter sw = new StreamWriter(
						   AppState.InstallDirectory + @"Data\Magnetic Field\" + fileName))
				{
					sw.WriteLine("    {0}", coeffs.title);
					sw.WriteLine("{0}  {1:F1} {2:F1}",
						coeffs.order, coeffs.radiusKm, coeffs.year);
					for (int n = 1; n <= coeffs.order; n++)
						for (int m = 0; m <= n; m++)
							sw.WriteLine(" {0,2} {1,2}\t{2,8:F1}\t{3,8:F1}\t{4,8:F2}\t{5,8:F2}", n, m,
								coeffs.g[n, m], coeffs.h[n, m],
								coeffs.dg[n, m], coeffs.dh[n, m]);
				}
				return true;
			}
			catch (Exception e)
			{
				Log.ReportException(e, "MagField.SaveCoeffs: The file, {0}, could not be written", fileName);
			}
			return false;
		}

		#endregion

		#region MagFieldContext
		/// <summary>
		/// 
		/// </summary>
		public class MagFieldContext : EnvironmentModelContext, IMagnetics 
		{
			AstroClock clock;
			IBody body;
			MagField magneticsModel;
			EcefState ecef;
			Frame bodyFrame;

			[Blackboard(Description = "Magnetic field intensity, ECI", Units = "nTesla")]
			Vector3 BfieldEci = new Vector3();

			[Blackboard(Description = "Magnetic field intensity, XYZ, Local frame", Units = "nTesla")]
			Vector3 BfieldLocal = new Vector3();

			[Blackboard(Description = "Magnetic field Radial, theta, phi, Local frame", Units = "nTesla")]
			Vector3 BfieldRtp = new Vector3();

			[Blackboard(Description = "Magnetic field intensity magnitude", Units = "nTesla")]
			double BfieldMag { get { return BfieldRtp.Magnitude; } }

			[Blackboard(Description = "Magnetic field intensity, body frame", Units = "nTesla")]
			Vector3 BfieldBody = new Vector3();

			public MagFieldContext() { }

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public MagFieldContext(EnvironmentModel model, Model owner)
				: base(model, owner)
			{
				EnableDependsOnReady = true;
				magneticsModel = model as MagField;

				if (owner is IBody)
					body = owner as IBody;
				else
					body = Body.GetBody(owner);
				if (body == null)
				{
					Enabled = false;
					return;
				}

				ecef = (body as Entity).Ecef;

				if (body is IHaveFrames)
				{
					bodyFrame = (body as IHaveFrames).GetFrame("body");

					if (bodyFrame == null)
						Enabled = false;
				}
				else
					Enabled = false;
			}

			#region Model implementation

			public override void Initialize()
			{
				clock = magneticsModel.astroClock;
				base.Initialize();
			}
			/// <summary>
			/// Do nothing for now.
			/// </summary>
			public override void Execute()
			{
				Vector3 B;
				if (magneticsModel.secular == Secular.Dynamic)
					B = magneticsModel.sphHarm.FieldVector(body.EciRmag, ecef.CosT, ecef.SinT,
						ecef.LongitudeRad, clock.JulianDate);
				else
					B = magneticsModel.sphHarm.FieldVector(body.EciRmag, ecef.CosT, ecef.SinT,
						ecef.LongitudeRad);

				BfieldRtp = B;

				BfieldEci[0] = (B[0] * ecef.SinT + B[1] * ecef.CosT) * ecef.CosRA - B[2] * ecef.SinRA;
				BfieldEci[1] = (B[0] * ecef.SinT + B[1] * ecef.CosT) * ecef.SinRA + B[2] * ecef.CosRA;
				BfieldEci[2] = B[0] * ecef.CosT - B[1] * ecef.SinT;
				BfieldBody = bodyFrame.FromParentDcm * BfieldEci;
			}

			#endregion

			/// <summary>
			/// Access the local B field, [X,Y,Z].
			/// </summary>
			public Vector3 CalculateBFieldLocal()
			{
				double a = Constant.EarthEquatorialRadius;
				double b = Constant.EarthPolarRadius;
				double a2 = a * a;
				double b2 = b * b;
				double c2 = a2 - b2;
				double a4 = a2 * a2;
				double b4 = b2 * b2;
				double c4 = a4 - b4;
				double rlat = ecef.LatitudeRad;
				double srlat = Math.Sin(rlat);
				double srlat2 = srlat * srlat;
				double crlat = Math.Cos(rlat);
				double crlat2 = crlat * crlat;
				double q = Math.Sqrt(a2 - c2 * srlat2);
				double alt = ecef.Altitude;
				double q1 = alt * q;
				double r2 = (alt * alt) + 2.0 * q1 + (a4 - c4 * srlat2) / (q * q);
				double r = Math.Sqrt(r2);
				double d = Math.Sqrt(a2 * crlat2 + b2 * srlat2);
				double ca = (alt + d) / r;
				double sa = c2 * crlat * srlat / (r * d);
				BfieldLocal[0] = -BfieldRtp[0] * sa - BfieldRtp[1] * ca;
				BfieldLocal[1] = BfieldRtp[2];
				BfieldLocal[2] = BfieldRtp[0] * ca - BfieldRtp[1] * sa;
				return BfieldLocal;
			}

			#region Properties
			/// <summary>
			/// Access the local B field, [radial, vertical, horizontal].
			/// </summary>
			public Vector3 BFieldRtp { get { return BfieldRtp; } }

			/// <summary>
			/// Access the local B field radial.
			/// </summary>
			public double Radial { get { return BfieldRtp[0]; } }

			/// <summary>
			/// Access the local B field vertical.
			/// </summary>
			public double Vertical { get { return BfieldRtp[1]; } }

			/// <summary>
			/// Access the local B field horizontal.
			/// </summary>
			public double Horizontal { get { return BfieldRtp[2]; } }

			#endregion

			#region IMagnetics Members

			/// <summary>
			/// Access BField in Body frame vector, [nTesla]
			/// </summary>
			public Vector3 BodyBField { get { return BfieldBody; } }

			/// <summary>
			/// Access BField in the inertial frame, [nTesla]
			/// </summary>
			public Vector3 BField { get { return this.BfieldEci; } }

			#endregion
		}

		/// <summary>
		/// returns a model context for this environmental model
		/// and the component passed in
		/// </summary>
		/// <param name="owner">The owning Component of the EnvironmentContext.</param>
		/// <param name="context">The containing EnvironmentContext. Used to defer creation until PostDiscover.</param>
		/// <returns>A new ModelContext or null if deferred until PostDiscover</returns>
		public override EnvironmentModelContext CreateModelContext(Model owner, EnvironmentContext context)
		{
			return new MagFieldContext(this, owner);
		}

		#endregion
	}
}
