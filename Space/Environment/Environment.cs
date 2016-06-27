using System.Collections.Generic;
//using System.Xml.Serialization;

using Aspire.Framework;
using Aspire.Utilities;

namespace Aspire.Space
{
    public class Environment : Model
	{
		List<EnvironmentModel> envModels = new List<EnvironmentModel>();

		#region Model implementation

		public override void Discover(Scenario scenario)
		{
			foreach (var model in Models)
				Log.WriteLine("env model: {0}", model);
			base.Discover(scenario);
		}

		#endregion

		public static void AddModel(EnvironmentModel model)
		{
			var env = ModelMgr.Model("Environment") as Environment;
			if (env == null)
			{
				env = new Environment();
				ModelMgr.AddByName(env);
			}
			env.envModels.Add(model);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public EnvironmentContext Register(Model model)
		{
			return new EnvironmentContext(model, this, envModels);
		}

		#region Properties

		public ICelestialBody CelestialBody(string name)
		{
			foreach (var model in envModels)
				if (model is ICelestialBody && model.Name == name)
					return model as ICelestialBody;
			return null;
		}

		#endregion
	}
}
