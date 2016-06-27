

namespace Aspire.Studio.Dashboard
{
	public interface IPropertiesView
	{
		void ReloadSelectableObjects();
		object SelectedObject { get; set; }
		object[] SelectableObjects { get; set; }
		bool SelectableObjectsVisible { get; set; }
	}
}
