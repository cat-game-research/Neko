using UnityEngine;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to glue the current Transform to the specified <b>SgtTerrain</b>. If you don't specify one, then the nearest one will be used.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainObject")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Object")]
	public class SgtTerrainObject : MonoBehaviour
	{
		public enum SnapType
		{
			Update,
			LateUpdate,
			FixedUpdate,
			Start
		}

		/// <summary>The terrain this object is attached to.
		/// None/null = Nearest.</summary>
		public SgtTerrain Terrain { set { terrain = value; } get { return terrain; } } [SerializeField] private SgtTerrain terrain;

		/// <summary>This allows you to move the object up based on the surface normal in world space.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] private float offset;

		/// <summary>The surface normal will be calculated using this sample radius in world space. Larger values = Smoother.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 0.1f;

		/// <summary>This allows you to control where in the game loop the object position will be snapped.</summary>
		public SnapType SnapIn { set { snapIn = value; } get { return snapIn; } } [SerializeField] private SnapType snapIn;

		[System.NonSerialized]
		private float3 delta;

		[System.NonSerialized]
		private bool deltaSet;

		protected virtual void Start()
		{
			if (snapIn == SnapType.Start)
			{
				SnapNow();
			}
		}

		protected virtual void Update()
		{
			if (snapIn == SnapType.Update)
			{
				SnapNow();
			}
		}

		protected virtual void LateUpdate()
		{
			if (snapIn == SnapType.LateUpdate)
			{
				SnapNow();
			}
		}

		protected virtual void FixedUpdate()
		{
			if (snapIn == SnapType.FixedUpdate)
			{
				SnapNow();
			}
		}

		private SgtTerrain GetTerrain()
		{
			if (terrain == null)
			{
				return SgtTerrain.FindNearest(transform.position);
			}

			return terrain;
		}
#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var finalTerrain = GetTerrain();

			if (finalTerrain != null)
			{
				var worldPoint   = transform.position;
				var worldRight   = transform.right   * radius;
				var worldForward = transform.forward * radius;

				if (deltaSet == true)
				{
					worldPoint -= (Vector3)delta;
				}

				var sampledPoint  = finalTerrain.GetWorldPoint(worldPoint);
				var sampledNormal = finalTerrain.GetWorldNormal(worldPoint, worldRight, worldForward);

				Gizmos.matrix = Matrix4x4.Rotate(Quaternion.LookRotation(worldForward, sampledNormal));
				Gizmos.DrawWireSphere(sampledPoint, radius);
			}
		}
#endif
		/// <summary>This method updates the position and rotation of the current <b>Transform</b>.</summary>
		[ContextMenu("Snap Now")]
		private void SnapNow()
		{
			var finalTerrain = GetTerrain();

			if (finalTerrain != null)
			{
				var worldPoint   = transform.position;
				var worldRight   = transform.right   * radius;
				var worldForward = transform.forward * radius;

				if (deltaSet == true)
				{
					worldPoint -= (Vector3)delta;
				}

				var sampledPoint  = finalTerrain.GetWorldPoint(worldPoint);
				var sampledNormal = finalTerrain.GetWorldNormal(worldPoint, worldRight, worldForward);

				delta    = sampledNormal * offset;
				deltaSet = true;

				transform.position = sampledPoint + delta;
				transform.rotation = Quaternion.FromToRotation(transform.up, sampledNormal) * transform.rotation;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainObject;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainObject_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("terrain", "The heightmap texture used to displace the mesh.\n\nNOTE: The height data should be stored in the alpha channel.\n\nNOTE: This should use the equirectangular cylindrical projection.");
			Draw("offset", "This allows you to move the object up based on the surface normal in world space.");
			Draw("radius", "The surface normal will be calculated using this sample radius in world space. Larger values = Smoother.");
			Draw("snapIn", "This allows you to control where in the game loop the object position will be snapped.");
		}
	}
}
#endif