using System;
using System.Collections.Generic;

using Aspire.Utilities;

namespace Aspire.Framework
{
	/// <summary>
	/// Manages all models in the running system
	/// </summary>
	public class ModelMgr
	{
		/// <summary>
		/// Default value for Model.Id
		/// </summary>
		public const int DefaultId = -1;
		static Dictionary<string,Model> modelByName = new Dictionary<string,Model>();
		static Model EmptyModel = new Model("empty");
		static ModelList modelById = new ModelList(); 
		static ModelList models = new ModelList();
		static Scenario scenario;

		/// <summary>
		/// A model has been added
		/// </summary>
		public static event EventHandler ModelAdded;
		/// <summary>
		/// A model has been removed
		/// </summary>
		public static event EventHandler ModelRemoved;

		/// <summary>
		/// Add a model
		/// </summary>
		/// <param name="model"></param>
		public static bool Add(Model model)
		{
			try
			{
				if (model.ParentModel != null)
				{
					model.Path = model.Parent.Name + '.' + model.Name;
					model.ParentModel.AddChild(model, scenario);
					model.Initialize();
				}
				else
				{
					model.Discover(scenario);
					model.Initialize();
					models.Add(model);
				}
			}
			catch (Exception e)
			{
				Log.ReportException(e, "Trying to Add({0} to ModelMgr", model.Name);
				return false;
			}

			if ( model.Path == null )
				model.Path = model.Name;

			modelByName.Add(model.Path, model);
			if (ModelAdded != null) ModelAdded(model, EventArgs.Empty);
			IsDirty = true;
			return true;
		}

		/// <summary>
		/// Just keep track of the model by its path for future lookups
		/// </summary>
		/// <param name="model"></param>
		public static void AddByName(Model model)
		{
			if (model.Name == null) model.Name = model.GetType().Name;
			if ( model.Path == null ) model.Path = model.Name;

			modelByName.Add(model.Path, model);
		}

		/// <summary>
		/// Verify or add an ordinal suffix to name so that it is unique under the parent
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static string GetUniqueName(string name, Model parent)
		{
			nameSuffix = 0;
			string newName;
			if (parent == null)
			{
				if (!modelByName.ContainsKey(name)) return name;

				do
				{
					newName = name + ++nameSuffix;
				} while (modelByName.ContainsKey(newName));
			}
			else
			{
				newName = name;
			Again:
				foreach (var model in parent.Models)
				{
					if (model.Name == name)
					{
						newName = name + ++nameSuffix;
						goto Again;
					}
				}
			}
			return newName;
		}

		/// <summary>
		/// Is the model configuration different from what was originally loaded ?
		/// </summary>
		public static bool IsDirty
		{
			get
			{
				if (mIsDirty) return true;
				foreach (var model in models)
					if (model.IsDirty) return true;
				return false;
			}
			private set { mIsDirty = true; }
		} static bool mIsDirty;

		/// <summary>
		/// Find a model with its name
		/// </summary>
		/// <param name="modelName"></param>
		/// <returns></returns>
		public static Model Model(string modelName)
		{
			Model model;
			modelByName.TryGetValue(modelName, out model);
			return model;
		}
		/// <summary>
		/// Move the model in its parent's model list to the index specified
		/// </summary>
		/// <param name="model"></param>
		/// <param name="index"></param>
		public static void Move(Model model, int index)
		{
			ModelList models;
			if (model.ParentModel != null)
				models = model.ParentModel.Models;
			else
				models = ModelMgr.models;
			models.Remove(model);
			models.Insert(index-1, model);
			model.Enabled = model.Enabled; // Trigger reschedule
			IsDirty = true;
		}

		/// <summary>
		/// Remove a model
		/// </summary>
		/// <param name="model"></param>
		public static void Remove(Model model)
		{
			var parent = model.ParentModel;
			if (parent == null)
				models.Remove(model);
			else
				parent.Models.Remove(model);
			if (model.Path != null)
			{
				modelByName.Remove(model.Path);
				Blackboard.Unpublish(model.Path);
			}
			if ( model.Id != DefaultId )
				modelById[model.Id] = null;
			if (ModelRemoved != null) ModelRemoved(model, EventArgs.Empty);
			IsDirty = true;
		}

		static int nameSuffix;

		/// <summary>
		/// Save the model configuration
		/// </summary>
		public static void Save()
		{
			scenario.Save();
			IsDirty = false;
			foreach (var model in models) model.IsDirty = false;
		}

		/// <summary>
		/// Add all of a scenario's models
		/// </summary>
		public static Scenario Scenario
		{
			set
			{
				scenario = value;
				models = scenario.Models;
				modelByName.Clear();
				modelById.Clear();
				TagModel(EmptyModel);
				foreach (var model in models)
				{
					if (model.Name == null) model.Name = model.GetType().Name;
					if (model.Parent != null)
						model.Path = model.Parent.Name + '.' + model.Name;
					if (model.Path == null)
						model.Path = model.Name;

					if ( modelByName.ContainsKey(model.Path) )
					{
						string origPath = model.Path, newPath;

						do
						{
							newPath = origPath + ++nameSuffix;
						} while (modelByName.ContainsKey(newPath));
						Log.WriteLine("{0} is already loaded. Renaming to {1}",model.Path,newPath);
						model.Name = model.Name + nameSuffix;
						model.Path = newPath;
					}
					modelByName.Add(model.Path, model);
				}
			}
		}

		/// <summary>
		/// Track a model by its Id
		/// </summary>
		/// <param name="model"></param>
		public static void TagModel(Model model)
		{
			if (model.Id == DefaultId)
			{
				model.Id = modelById.Count;
				modelById.Add(model);
			}
		}

	}

	/// <summary>
	/// Soecific list of models
	/// </summary>
	public class ModelList : List<Model>
	{
	}

	/// <summary>
	/// Specific list of models that notifies when changed
	/// </summary>
	public class NotifyingModelList : NotifyingList<Model>
	{
	}
}
