using System;

using Aspire.Framework;

namespace Aspire.Primitives
{
	/// <summary>
	/// Name:       SphericalHarmonics
	///
	/// Purpose:    Computes the coefficients of the recurrence relations for the
	///             associated Legendre polynomials and the Schmidt normalization
	///             factors, since these quantities are independant of time and
	///             position.  The normalization factors are then used to modify
	///             the Gaussian coefficients and the secular terms to
	///             Diagnostics computational time and space.
	///
	/// Relevant Requirements:
	///
	/// Assumptions and External Effects:
	///    1. The output vectors shall be arranged so that the i-th element
	///       is related to the quantity depending on n and m as follows:
	///             y(i(n,m)) = F(x[m,n])
	///       where i = i(m,n) = n*(n+1)/2 + m - 1, y is the quantity in the output
	///       array(e.g., sgnm), F is the function applied to x and x[m,n] is the
	///       quantity in the input table (e.g.,FieldCoeffTBL.tg) or some quantity
	///       depending on the indices n,m (such as the case for kn[m]). Note that
	///       i(n,n)+1 yields the degeneracy for a fixed n.
	///
	/// Inputs:
	///    FieldCoeffTBL  magnetic field coefficients table
	///
	/// Outputs:
	///    kn[Order]               coefficients used in recursion relations
	///    sgnm[Order]             Normal factor * Gaussian coefficients
	///    shnm[Order]
	///    dsgnm[Order]            Normal factor * Secular terms
	///    dshnm[Order]
	/// </summary>
	public class SphericalHarmonics
	{
		[Blackboard(Description="Order of Harmonic Series to be evaluated")]
		int order;
		[Blackboard(Description = "Epoch of the dataset", Units = "days")]
		double epochMjd;
		[Blackboard(Description="Normal assumed in input coefficients")]
		Normal normal = Normal.Gauss;
		[Blackboard(Description="Average Radius of (Oblate)Sphere",Units="m")]
		double radius = Constant.EarthEquatorialRadius;
		[Blackboard(Description="Gaussian Coefficients and Secular Terms")]
		double[] cosCoef;
		[Blackboard(Description="Gaussian Coefficients and Secular Terms")]
		double[] sinCoef;
		[Blackboard(Description="Gaussian Coefficients and Secular Terms")]
		double[] dcosCoef;
		[Blackboard(Description="Gaussian Coefficients and Secular Terms")]
		double[] dsinCoef;

		/// <summary>
		/// Type of input coefficient normalization
		/// </summary>
		public enum Normal 
		{
			/// <summary>
			/// 
			/// </summary>
			Gauss,
			/// <summary>
			/// 
			/// </summary>
			Neumann,
			/// <summary>
			/// 
			/// </summary>
			Schmidt,
			/// <summary>
			/// 
			/// </summary>
			Wgs84 };

		bool derivs;
		int
			maxOrder;	// Maximum order; the size of the input coefficients
		double[]
			kn,			// Coefficients for the Legendre polynomial recurrence relations
			sgnm,		// modified Gaussian coefficients and secular
			dsgnm,		// terms. The product of the Schmidt factor
			shnm,		// and the Gaussian coefficient or secular term
			dshnm,
			sgnmEpoch, shnmEpoch, // Values of sgnm, shgm at the epoch
			pn,			// Values of the associated Legendre polynomials 
			pn1,		// corresponding to the eigenfunctions for the n-1 and n-2
						// degenerate eigenvalues respectively.
			dpn,		// Values of the derivatives of the associated
			dpn1,		// Legendre polynomials corresponding to the eigenfunctions for
						// the n-1 and n-2 degenerate eigenvalues respectively.
			cosP,		// cosP[k] = cos(k*phi)
			sinP;		// sinP[k] = sin(k*phi)
		Vector3 Field;  // (Radial,Theta,Phi) Gradient
  
		/// <summary>
		/// Default constructor
		/// </summary>
		public SphericalHarmonics( Normal norm, bool coefDerivs, int maxOrder,
			double avgRadius )
		{
			this.maxOrder = maxOrder;
			order = 0;
			radius = avgRadius;
			normal = norm;
			derivs = coefDerivs;
			kn = new double[0];
			sgnm = new double[0];
			dsgnm = new double[0];
			sgnmEpoch = new double[0];
			shnmEpoch = new double[0];
			shnm = new double[0];
			dshnm = new double[0];
			pn = new double[0];
			pn1 = new double[0];
			dpn = new double[0];
			dpn1 = new double[0];
			cosP = new double[0];
			sinP = new double[0];
			Field = new Vector3();
		}

		/// <summary>
		/// Set the working date of the Field model
		/// </summary>
		public double DateMjd
		{
			set
			{
				if ( sgnm.Length == 0 ) return;
				int degeneracy = Degeneracy( maxOrder );
				double dYear = (value-epochMjd)/Constant.DayPerYear;
				for( int i=0; i<degeneracy; i++ )
				{
					sgnm[i] = sgnmEpoch[i] + dYear*dsgnm[i];
					shnm[i] = shnmEpoch[i] + dYear*dshnm[i];
				}
			}
		}
		/// <summary>
		/// Init
		///
		/// Inputs:
		///    FieldCoeffTBL  magnetic field coefficients table
		///
		/// Outputs:
		///    kn[Order]               coefficients used in recursion relations
		///    sgnm[Order]             Normal factor * Gaussian coefficients
		///    shnm[Order]
		///    dsgnm[Order]            Normal factor * Secular terms
		///    dshnm[Order]
		/// </summary>
		/// <param name="epochMjd"></param>
		/// <param name="cosCoeff"></param>
		/// <param name="sinCoeff"></param>
		/// <param name="dcosCoeff"></param>
		/// <param name="dsinCoeff"></param>
		public void Init( double epochMjd, double[,] cosCoeff, double[,] sinCoeff,
			double[,] dcosCoeff, double[,] dsinCoeff )
		{
			maxOrder = cosCoeff.GetUpperBound(0);
			int i, degeneracy = Degeneracy( maxOrder );
			double sn0,snm;

			this.epochMjd = epochMjd;

			order = maxOrder;
			int dim = maxOrder + 1;
			cosCoef = new double[dim*dim];
			dcosCoef = new double[dim*dim];
			sinCoef = new double[dim*dim];
			dsinCoef = new double[dim*dim];
			kn = new double[degeneracy];
			sgnm = new double[degeneracy];
			shnm = new double[degeneracy];
			sgnmEpoch = new double[degeneracy];
			shnmEpoch = new double[degeneracy];
			pn = new double[degeneracy];
			pn1 = new double[degeneracy];
			dpn = new double[degeneracy];
			dpn1 = new double[degeneracy];
			cosP = new double[degeneracy];
			sinP = new double[degeneracy];

			if ( derivs )
			{
				dsgnm = new double[degeneracy];
				dshnm = new double[degeneracy];
			}
			else
			{
				dsgnm = new double[0];
				dshnm = new double[0];
			}

			for ( i=0; i<degeneracy; i++ )
			{
				kn[i]   = 0.0;
				sgnm[i] = 0.0;
				shnm[i] = 0.0;
				pn[i]   = 0.0;
				pn1[i]  = 0.0;
				dpn[i]  = 0.0;
				dpn1[i] = 0.0;
				cosP[i] = 0.0;
				sinP[i] = 0.0;
				if ( derivs )
				{
					dsgnm[i] = 0.0;
					dshnm[i] = 0.0;
				}
			}

			if ( maxOrder > 0 )
			{
				sgnm[0] = cosCoeff[0,0];
				shnm[0] = sinCoeff[0,0];
				if ( derivs )
				{
					dsgnm[0] = dcosCoeff[0,0];
					dshnm[0] = dsinCoeff[0,0];
				}
			}
			i = 0;
			for ( int k=0; k<=maxOrder; k++ )
				for ( int j=0; j<=maxOrder; j++ )
				{
					cosCoef[i] = cosCoeff[k,j]; 
					sinCoef[i] = sinCoeff[k,j]; 
					if(derivs)
					{
						dcosCoef[i] = dcosCoeff[k,j];
						dsinCoef[i] = dsinCoeff[k,j];
					}
					i++;
				}
   
			// Legendre Polynomials are converted from either Schmidt or Neumann
			// normalized to Gauss normalized to save about 7% in computation time

			double wnm;
			i = 0;
			sn0 = 1.0;
			snm = 1.0;
			for ( int k=1; k<=order; k++ )
			{
				for (int j = 0; j <= k; j++)
				{
					// Compute (Schmidt,Normal.Wgs84,Neumann) normalization factors
					if (j == 0)
						switch(normal)
						{
							case Normal.Neumann:
								sn0 = snm = sn0*(2.0*k + 1.0)/(k + 1.0);
								break;
							case Normal.Schmidt:
								sn0 = snm = sn0*(2.0 - 1.0/(double)k);
								break;
							case Normal.Wgs84:
								sn0 = snm = sn0*Math.Sqrt(((double)(2*k-1)*(2*k+1)))/k;
								break;
							case Normal.Gauss:
							default:
								sn0 = 1.0;
								break;
						}
					else if(j == 1)
						switch(normal)
						{
							case Normal.Schmidt:
								snm *= Math.Sqrt( (double)(2*k)/(double)(k + 1) );
								break;
							case Normal.Wgs84:
							{
								snm *= Math.Sqrt(((double)(2*k))/((double)(k+1)));
								break;
							}
							case Normal.Neumann:
								snm = snm/(double)k;
								break;
							case Normal.Gauss:
							default:
								snm = 1.0;
								break;
						}
					else
						switch(normal)
						{
							case Normal.Schmidt:
								snm *= Math.Sqrt( (double)(k - j + 1)/(double)(k + j) );
								break;
							case Normal.Wgs84:
							{
								snm *= Math.Sqrt(((double)(k-j+1))/((double)(k+j)));
								break;
							}
							case Normal.Neumann:
								snm = snm/(double)(k - j + 1);
								break;
							case Normal.Gauss:
							default:
								snm = 1.0;
								break;
						}

					// Compute coefficients used in recurrence relations
					if (k == 1)
						kn[i] = 0;
					else
						kn[i] = ((double)((k - 1)*(k - 1) - j*j))/((double)((2*k - 1)*(2*k - 3)));

					// Modify Gaussian coefficients and the secular terms during
					//	initialization in order to Diagnostics time during the control cycle
					//	calculation
					if ( normal==Normal.Wgs84)
					{
						//Normal.Wgs84 To Neumann
						if(j==0)
							wnm = Math.Sqrt(((double)(Maths.Factorial(k-j)*(2*k+1))) / ((double)Maths.Factorial(k+j)));
						else
							wnm = Math.Sqrt(((double)(2*Maths.Factorial(k-j)*(2*k+1)))/((double)Maths.Factorial(k+j)) );
						// Neumann To Gauss
						wnm *= (double)Maths.AltFactorial(2*k-1)/(double)Maths.Factorial(k-j);
						//wnm *= (double)Maths.Factorial(k-j)/(double)Maths.AltFactorial(2*k-1);
					}

					sgnmEpoch[i] = snm*cosCoeff[k,j];
					shnmEpoch[i] = snm*sinCoeff[k,j];
					sgnm[i] = sgnmEpoch[i];
					shnm[i] = shnmEpoch[i];
					if ( derivs )
					{
						dsgnm[i] = snm*dcosCoeff[k,j];
						dshnm[i] = snm*dsinCoeff[k,j];
					}
					i++;
				} /* end for (j) */
			} /* end for (k) */

			return;
		}

		/// <summary>
		/// Calculate the potential field
		/// </summary>
		/// <param name="distance">Altitude</param>
		/// <param name="theta">co-Latitude</param>
		/// <param name="phi">Longitude</param>
		/// <returns></returns>
		public double Potential( double distance, double theta, double phi )
		{
			double RAratio = radius / distance; // The radius-altitude ratio
			double cosPhi = Math.Cos(phi);
			double sinPhi = Math.Sin(phi);
			double cosTheta = Math.Cos(theta);
			double sinTheta = Math.Sin(theta);

			// Initialize recursive and iterative quantities for n=1,m=0
			double rn = RAratio; // Radius-Altitude ratio to the n-th power
			double dpnn = 0; // Value of the derivative of the associated Legendre polynomial corresponding to the n-1 sectoral harmonic.
			double U = 0.0; // Potential

			int i = Degeneracy(order-1);

			double
				gnm, hnm, // Gaussian coefficients of the m-th eigenfunction corresponding to
				temp,
				sumU; // Partial Sum of Potential

			lock (cosP)
			{
				// Value of the associated Legendre polynomial corresponding to the n-1 sectoral harmonic.
				double pnn = pn[0] = pn1[0] = 1.0;
				cosP[0] = 1.0;
				sinP[0] = 0.0;
				for (int m = 1; m <= order; m++)
				{
					// cos(m*phi), sin(m*phi)
					cosP[m] = (cosP[m-1]*cosPhi) - (sinPhi*sinP[m-1]);
					sinP[m] = (cosP[m-1]*sinPhi) + (cosPhi*sinP[m-1]);
				}

				for (int n = 1; n <= order; n++)
				{
					sumU = 0.0;
					if (n<=order)
					{  // Compute Zonal Harmonics

						// First order approximation to the time dependent
						//     Gaussian coefficients
						gnm = sgnm[i];
						hnm = shnm[i];

						// Calculate the value of the associated Legendre polynomial
						//    then update the recursive quantities
						temp = pn[0];
						pn[0] = temp*cosTheta - kn[i]*pn1[0];
						pn1[0] = temp;
						sumU += gnm*pn[0];
					}
					i++;
					// Compute Tesseral Harmonics
					if (n<=order)
					{
						int m;
						for (m = 1; m < n; m++)
						{
							// First order approximation to the time dependent
							//     Gaussian coefficients
							gnm = sgnm[i];
							hnm = shnm[i];

							// Calculate the value of the associated Legendre polynomial
							//    then update the recursive quantities
							temp = pn[m];
							pn[m] = temp*cosTheta - kn[i]*pn1[m];
							i++;
							pn1[m] = temp;

							sumU += (gnm*cosP[m] + hnm*sinP[m])*pn[m];
						} // end for m

						// Compute values associated with the n-th sectoral harmonic
						gnm = sgnm[i];
						hnm = shnm[i];
						i++;
						dpnn = dpnn*sinTheta + pnn*cosTheta;
						dpn1[m] = dpn[m];
						dpn[m] = dpnn;
						pnn = pnn*sinTheta;
						pn1[m] = pn[m];
						pn[m] = pnn;

						sumU += (gnm*cosP[m] + hnm*sinP[m])*pn[m];
					}
					else
						i+=n;

					// n-th partial sum Field Vector
					rn = rn*RAratio;
					sumU *= rn;
					U += sumU;
				}  // end for n
			}

			return U;
		}

		/// <summary>
		/// FieldVector
		///
		/// Purpose:    Approximates the Cartesian coordinates of the normalized
		///             magnetic force vector via a truncated spherical harmonic
		///             decomposition followed by a coordinate transformation.
		///
		/// Relevant Requirements:
		///
		///
		/// Assumptions and External Effects:
		///    1. The input vectors (excluding the s/c inertial position) shall be arranged so that
		///       the i-th element is related to the quantity depending on n and m as follows:
		///             y(i(n,m)) = F(x[m,n])
		///       where i = i(m,n) = n*(n+1)/2 + m - 1, y is the quantity in the output
		///       array(e.g., sgnm), F is the function applied to x and x[m,n] is the
		///       quantity in the input table(e.g., FieldCoeffTBL.tg) or some quantity
		///       depending on the indices n,m (such as the case for kn[m]). Note that
		///       i(n,n)+1 yields the degeneracy for a fixed n.
		/// Inputs:
		///    UtcTime                     ACS time in UTC format
		///    FieldCoeffTBL           Field model coefficients table
		///		Position                    inertial position of spacecraft
		///
		///    kn[77]                      coefficients used in recurrence relations
		///
		///    sgnm[77]                    Normal factor * Gaussian coefficient
		///    shnm[77]
		///
		///    dsgnm[77]                   Normal factor * Secular term
		///    dshnm[77]
		///
		/// Outputs:
		///    FieldInertial[3]        Earth's magnetic field vector (unit vector)
		///    Field                   Field vector, micro-Tesla
		/// </summary>
		/// <param name="distance">Altitude</param>
		/// <param name="cosTheta">cos co-Latitude</param>
		/// <param name="sinTheta">sin co-Latitude</param>
		/// <param name="phi">Longitude</param>
		/// <param name="mjd">Modified julian date</param>
		/// <returns></returns>
		public Vector3 FieldVector(double distance,double cosTheta, double sinTheta,double phi,double mjd)
		{
			double dYear = (mjd-epochMjd)/Constant.DayPerYear;
			double RAratio = radius / distance; // The radius-altitude ratio
			double cosPhi = Math.Cos(phi);
			double sinPhi = Math.Sin(phi);
			// Initialize recursive and iterative quantities for n=1,m=0
			double rn = RAratio * RAratio; // Radius-Altitude ratio to the n-th power
			double Bradial = 0; // These values contain partial sums of the eigenfunction values  at the current
			double Btheta = 0;  // time and position.
			double Bphi = 0.0;
			int i = 0;
			int n1 = 0;
			double gnm, hnm, temp; // Gaussian coefficients of the m-th eigenfunction corresponding to
			double br, bt, bp;   // These values contain the partial sums of the eigenfunctions corresponding the
								 // n-th eigenvalue evaluated an the current time and position.
			lock (cosP)   
			{
				// Value of the associated Legendre polynomial corresponding to the n-1 sectoral harmonic.
				double pnn = pn[0] = pn1[0] = 1.0;
				// Value of the derivative of the associated Legendre polynomial corresponding to the n-1 sectoral harmonic.
				double dpnn = dpn[0] = dpn1[0] = 0.0;
				cosP[0] = 1.0;
				sinP[0] = 0.0;

				for (int n = 1; n <= order; n++)
				{
					cosP[n] = (cosP[n1]*cosPhi) - (sinPhi*sinP[n1]);
					sinP[n] = (cosP[n1]*sinPhi) + (cosPhi*sinP[n1]);
					n1 = n; //n1 = n-1
					br = bt = bp = 0.0;
					int m;
					for (m = 0; m < n; m++)
					{
						// First order approximation to the time dependent
						//     Gaussian coefficients
						gnm = sgnm[i] + dsgnm[i]*dYear;
						hnm = shnm[i] + dshnm[i]*dYear;

						// Calculate the value of the associated Legendre polynomial
						//    then update the recursive quantities
						temp = dpn[m];
						dpn[m] = temp*cosTheta - pn[m]*sinTheta - kn[i]*dpn1[m];
						dpn1[m] = temp;
						temp = pn[m];
						pn[m] = temp*cosTheta - kn[i]*pn1[m];
						i++;
						pn1[m] = temp;

						temp = gnm*cosP[m] + hnm*sinP[m];
						br += temp*pn[m];
						bt += temp*dpn[m];
						bp += m*(-gnm*sinP[m]+ hnm*cosP[m])*pn[m];
					} // end for m

					// First order approximation to the time dependent
					//     Gaussian coefficients
					gnm = sgnm[i] + dsgnm[i]*dYear;
					hnm = shnm[i] + dshnm[i]*dYear;

					// Compute values associated with the n-th sectoral harmonic
					i++;
					dpnn = dpnn*sinTheta + pnn*cosTheta;
					dpn1[m] = dpn[m];
					dpn[m] = dpnn;
					pnn = pnn*sinTheta;
					pn1[m] = pn[m];
					pn[m] = pnn;
					temp = gnm*cosP[m] + hnm*sinP[m];

					br += temp*pnn;
					bt += temp*dpnn;
					bp += m*(-gnm*sinP[m] + hnm*cosP[m])*pnn;

					// n-th partial sum Field Vector
					rn *= RAratio;
					Bradial +=  ((double)(n+1))*rn*br;
					Btheta += rn*bt;
					Bphi += rn*bp;
				}  // end for n

				Btheta = -Btheta;
				if (sinTheta != 0.0)
					Bphi = (-1.0/sinTheta)*Bphi;
				else
					Bphi = 0.0;

				// NOTE::There still exists a dependency on the GMT hour angle
				Field[0] = Bradial;
				Field[1] = Btheta;
				Field[2] = Bphi;
			}
   
			return Field;
		}

		/// <summary>
		/// Time-independent Gaussian coefficients
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="theta">co-Latitude</param>
		/// <param name="phi">Longitude</param>
		/// <returns></returns>
		public Vector3 FieldVector(double distance,double theta,double phi)
		{
			return FieldVector(distance,
				Math.Cos(theta), Math.Sin(theta), Math.Cos(phi), Math.Sin(phi), false );
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="cosTheta">cos Latitude</param>
		/// <param name="sinTheta">sin Latitude</param>
		/// <param name="phi">Longitude</param>
		/// <returns></returns>
		public Vector3 FieldVector( double distance, double cosTheta, double sinTheta, double phi )
		{
			return FieldVector(distance, cosTheta, sinTheta, Math.Cos(phi), Math.Sin(phi), false );
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="distance">Altitude</param>
		/// <param name="cosTheta">cos co-Latitude</param>
		/// <param name="sinTheta">sin co-Latitude</param>
		/// <param name="cosPhi">cos Longitude</param>
		/// <param name="sinPhi">sin Longitude</param>
		/// <param name="unused"></param>
		/// <returns></returns>
		public Vector3 FieldVector( double distance,
			double cosTheta, double sinTheta, double cosPhi, double sinPhi, bool unused )
		{
			double RAratio = radius / distance; // The radius-altitude ratio
			// Initialize recursive and iterative quantities for n=1,m=0
			double rn = RAratio * RAratio; // Radius-Altitude ratio to the n-th power
			double Bradial = 0;
			double Btheta = 0;
			double Bphi = 0.0;
			int i = 0;
			int n1 = 0;
			double br, bt, bp, gnm, hnm, temp; // Gaussian coefficients of the m-th eigenfunction corresponding to

			lock (cosP)
			{
				// Value of the associated Legendre polynomial corresponding to the n-1 sectoral harmonic.
				double pnn = pn[0] = pn1[0] = 1.0;
				// Value of the derivative of the associated Legendre polynomial corresponding to the n-1 sectoral harmonic.
				double dpnn = dpn[0] = dpn1[0] = 0.0;
				cosP[0] = 1.0;
				sinP[0] = 0.0;

				for (int n = 1; n <= order; n++)
				{
					cosP[n] = (cosP[n1]*cosPhi) - (sinPhi*sinP[n1]);
					sinP[n] = (cosP[n1]*sinPhi) + (cosPhi*sinP[n1]);
					n1 = n; //n1 = n-1;
					br = bt = bp = 0.0;
					int m;
					for (m = 0; m < n; m++)
					{
						gnm = sgnm[i];
						hnm = shnm[i];

						// Calculate the value of the associated Legendre polynomial
						//    then update the recursive quantities
						temp = dpn[m];
						dpn[m] = temp*cosTheta - pn[m]*sinTheta - kn[i]*dpn1[m];
						dpn1[m] = temp;
						temp = pn[m];
						pn[m] = temp*cosTheta - kn[i]*pn1[m];
						i++;
						pn1[m] = temp;

						temp = gnm*cosP[m] + hnm*sinP[m];
						br += temp*pn[m];
						bt += temp*dpn[m];
						bp += m*(-gnm*sinP[m]+ hnm*cosP[m])*pn[m];
					} // end for m

					// Compute values associated with the n-th sectoral harmonic
					gnm = sgnm[i];
					hnm = shnm[i];
					i++;
					dpnn = dpnn*sinTheta + pnn*cosTheta;
					dpn1[m] = dpn[m];
					dpn[m] = dpnn;
					pnn = pnn*sinTheta;
					pn1[m] = pn[m];
					pn[m] = pnn;
					temp = gnm*cosP[m] + hnm*sinP[m];

					br += temp*pnn;
					bt += temp*dpnn;
					bp += m*(-gnm*sinP[m] + hnm*cosP[m])*pnn;

					// n-th partial sum Field Vector
					rn *= RAratio;
					Bradial +=  ((double)(n+1))*rn*br;
					Btheta += rn*bt;
					Bphi += rn*bp;
				}  // end for n

				Btheta = -Btheta;
				if (sinTheta != 0.0)
					Bphi = (-1.0/sinTheta)*Bphi;
				else
					Bphi = 0.0;

				// NOTE::There still exists a dependency on the GMT hour angle
				Field[0] = Bradial;
				Field[1] = Btheta;
				Field[2] = Bphi;
			}
   
			return Field;
		}

		/// <summary>
		/// Set the oblate radius
		/// </summary>
		public double Radius
		{
			set { radius = value; }
		}

		/// <summary>
		/// Access the working order
		/// </summary>
		public int Order { get { return order; } set { order = Math.Min(value,maxOrder); } }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public int Degeneracy( int order )
		{
			return order*(order+1)/2 + order + 1;
		}
	}
}

