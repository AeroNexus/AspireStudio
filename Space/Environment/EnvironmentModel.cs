using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Aspire.Framework;

namespace Aspire.Space
{
    public abstract class EnvironmentModel : Model
	{
		/// <summary>
		/// This event is raised when the shared model wishes to let its 
		/// individual ModelContexts know that it is ready for their use.
		/// 
		/// </summary>
		public event EventHandler IsReady;

		#region Model implementation

		public override void Initialize()
		{
			if (IsReady != null) IsReady(this, EventArgs.Empty);
			base.Initialize();
		}

		#endregion

		/// <summary>
		/// Creates a model context for this environmental model and the component passed in
		/// note, if the context needs to live 'under' the owner i.e. to generate loads for the owner, the call to 
		/// create model context needs to place the created context in the owner's component list!
		/// </summary>
		/// <param name="owner">The owning Component of the EnvironmentContext.</param>
		/// <param name="context">The containing the EnvironmentContext. Used to defer creation until PostDiscover.</param>
		/// <returns>A new ModelContext or null if deferred until PostDiscover</returns>
		public abstract EnvironmentModelContext CreateModelContext(Model owner, EnvironmentContext context);
	}

	public class EnvironmentModelContext : Model
	{
		public EnvironmentModelContext() { }

		/// <summary>
		/// Constructor to set model and owner
		/// </summary>
		/// <param name="model"></param>
		/// <param name="owner"></param>
		public EnvironmentModelContext(EnvironmentModel model, Model owner)
		{
			Model = model;
			model.IsReady +=new EventHandler(model_IsReady);
			Owner = owner;
//			this.Name = owner.Name + "." + model.Name;
			Name = model.Name;
		}

		void model_IsReady(object sender, EventArgs e)
		{
			if (!Enabled && enableDependsOnReady)
				Enabled = true;
		}

		#region Properties

		/// <summary>
		/// Derived class defaults !ready if this is true and is enabled when 
		/// the shared model raises its IsReady event;
		/// </summary>
		protected bool EnableDependsOnReady
		{
			set
			{
				enableDependsOnReady = value;
				if (enableDependsOnReady) Enabled = false;
			}
		} bool enableDependsOnReady;
		[XmlIgnore]
		public Model Model { get; private set; }
		[XmlIgnore]
		public Model Owner { get; private set; }

		#endregion
	}

	public class EnvironmentContext
	{
		List<EnvironmentModelContext> modelContexts = new List<EnvironmentModelContext>();

		internal EnvironmentContext(Model model, Environment environment, List<EnvironmentModel> envModels)
		{
			Environment = environment;

			foreach (var em in envModels)
			{
				var emc = em.CreateModelContext(model, this);
				if (emc is IEarth) Earth = em as IEarth;
				modelContexts.Add(emc);
			}

			var modelModels = model.Models.ToArray();
			model.Models.Clear();
			model.Models.AddRange(modelContexts);
			model.Models.AddRange(modelModels);
		}

		public ICelestialBody CelestialBody(string name)
		{
			return Environment.CelestialBody(name);
		}

		public IEarth Earth { get; private set; }

		public Environment Environment { get; private set; }

		/// <summary>
		/// Lookup the celestial body by name.
		/// </summary>
		/// <param name="centralBody"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public I_NBodyGravity NBodyGravity(ICelestialBody centralBody)
		{
			foreach (var ctx in modelContexts)
				if (ctx.Model == centralBody)
					return ctx as I_NBodyGravity;
			return null;
		}
	}

}
