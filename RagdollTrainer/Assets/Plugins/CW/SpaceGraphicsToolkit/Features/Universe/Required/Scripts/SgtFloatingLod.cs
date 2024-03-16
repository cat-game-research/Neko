using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to spawn a prefab as a child of the current GameObject when the floating camera gets within the specified range.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingLod")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating LOD")]
	[RequireComponent(typeof(SgtFloatingObject))]
	public class SgtFloatingLod : MonoBehaviour
	{
		/// <summary>The prefab that will be spawned.</summary>
		public GameObject Prefab { set { prefab = value; } get { return prefab; } } [SerializeField] private GameObject prefab;

		/// <summary>If the camera is closer than this distance, then the LOD will disappear.</summary>
		public SgtLength DistanceMin { set { distanceMin = value; } get { return distanceMin; } } [SerializeField] private SgtLength distanceMin;

		/// <summary>If the camera is farther than this distance, then the LOD will disappear.</summary>
		public SgtLength DistanceMax { set { distanceMax = value; } get { return distanceMax; } } [SerializeField] private SgtLength distanceMax = new SgtLength(1.0, SgtLength.ScaleType.Kilometer);

		[SerializeField]
		private bool spawned;

		[SerializeField]
		private GameObject clone;

		[System.NonSerialized]
		private SgtFloatingObject cachedFloatingObject;

		/// <summary>This will be set while the LOD is within range.</summary>
		public bool Spawned
		{
			get
			{
				return spawned;
			}
		}

		/// <summary>This allows you to get the spawned prefab clone.</summary>
		public GameObject Clone
		{
			get
			{
				return clone;
			}
		}

		protected virtual void OnEnable()
		{
			cachedFloatingObject = GetComponent<SgtFloatingObject>();

			cachedFloatingObject.OnDistance += HandleDistance;
		}

		protected virtual void OnDisable()
		{
			cachedFloatingObject.OnDistance -= HandleDistance;
		}

		private void HandleDistance(double distance)
		{
			if (distance >= distanceMin && distance <= distanceMax)
			{
				if (spawned == false)
				{
					spawned = true;

					if (prefab != null)
					{
						clone = Instantiate(prefab, transform, false);
					}
				}
			}
			else if (spawned == true)
			{
				spawned = false;
				clone   = CwHelper.Destroy(clone);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingLod;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingLod_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Prefab == null));
				Draw("prefab", "The prefab that will be spawned.");
			EndError();
			Draw("distanceMin", "If the camera is closer than this distance, then the LOD will disappear.");
			Draw("distanceMax", "If the camera is farther than this distance, then the LOD will disappear.");
		}
	}
}
#endif