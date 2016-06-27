using Aspire.Framework;


namespace Aspire.Primitives
{
    public static class Body
    {
		/// <summary>
		/// Ascend the parent chain looking for an IBody
		/// </summary>
		/// <param name="comp"></param>
		/// <returns></returns>
		public static IBody GetBody(Model model)
		{
			if (model is IBody) return model as IBody;
			var parent = model.ParentModel;
			if (parent == null) return null;
			parent = parent.HasA(typeof(Model));
			while (parent != null)
			{
				if (parent is IBody)
					return parent as IBody;
				parent = parent.ParentModel;
				if (parent == null) return null;
				parent = parent.HasA(typeof(Model));
			}
			return null;
		}

	}
}
