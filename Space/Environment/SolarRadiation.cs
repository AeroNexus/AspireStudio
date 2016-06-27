using Aspire.Framework;

namespace Aspire.Space
{
	public class SolarRadiation : EnvironmentModel
	{
		#region Declarations

		#endregion

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
		}

		public override void Initialize()
		{
			Execute();
			base.Initialize();
		}

		public override void Execute()
		{
		}

		#endregion

		#region Properties

		#endregion

		#region Context
		/// <summary>
		/// 
		/// </summary>
		public class SolarRadiationContext : EnvironmentModelContext
		{
			/// <summary>
			/// default constructor
			/// </summary>
			public SolarRadiationContext() {}

			/// <summary>
			/// Constructor to set model and owner
			/// </summary>
			/// <param name="model"></param>
			/// <param name="owner"></param>
			public SolarRadiationContext(EnvironmentModel model, Model owner)
				: base(model, owner)
			{
			}

			/// <summary>
			/// Do nothing for now.
			/// </summary>
			public override void Execute()
			{
			}

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
			return new SolarRadiationContext(this, owner);
		}

		#endregion
	}
}
