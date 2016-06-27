using Aspire.Primitives;

namespace Aspire.Primitives
{
	/// <summary>
	/// The IGenerateMechanicalLoads allows a model to implement known mechanical loading behavior
	/// </summary>
	public interface IGenerateMechanicalLoads
	{
		/// <summary>
		/// Run the model and apply the loads
		/// </summary>
		void CalculateMechanicalLoads(ILoadReceiver loadReceiver);
	
		/// <summary>
		/// Tests for generate mechanical loads. 
		/// </summary>
		/// <returns></returns>
		bool GeneratesMechanicalLoads { get; }
	}

	/// <summary>
	/// Kinds of mechanical forces and torques
	/// </summary>
	public enum LoadType
	{
		/// <summary>
		/// Gravitational acceleration and gravity gradient torque
		/// </summary>
		Gravitation
	};

	public interface ILoadReceiver
	{
		void ApplyLoads(IGenerateMechanicalLoads loader, LoadType loadType , Vector3 force, Vector3 torque);
	}
}
