using UnityEngine;

namespace SpaceGraphicsToolkit.Shapes
{
	/// <summary>This is the base class for all volumetric shapes. These can be used to define regions where effects are visible.</summary>
	public abstract class SgtShape : MonoBehaviour
	{
		/// <summary>Returns a 0..1 value, where 1 is fully inside</summary>
		public abstract float GetDensity(Vector3 worldPoint);
	}
}