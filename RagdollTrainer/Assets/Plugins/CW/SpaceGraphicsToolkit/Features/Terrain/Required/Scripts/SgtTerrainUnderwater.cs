using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component swaps the planet's atmosphere material when the camera goes underwater.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainUnderwater")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Underwater")]
	public class SgtTerrainUnderwater : MonoBehaviour
	{
		/// <summary>The main ocean applied to the planet. This is used for the radius check to see if the camera is underwater.</summary>
		public SgtTerrainOcean Ocean { set { ocean = value; } get { return ocean; } } [SerializeField] private SgtTerrainOcean ocean;

		/// <summary>The terrain material renderer. This component swaps its shared material based on the camera position.</summary>
		public SgtTerrainSharedMaterial TerrainMaterial { set { terrainMaterial = value; } get { return terrainMaterial; } } [SerializeField] private SgtTerrainSharedMaterial terrainMaterial;

		/// <summary>The material applied to the terrain when the camera is under the water.</summary>
		public SgtSharedMaterial UnderMaterial { set { underwaterAtmosphere = value; } get { return underwaterAtmosphere; } } [SerializeField] private SgtSharedMaterial underwaterAtmosphere;

		[System.NonSerialized]
		private SgtTerrainSharedMaterial registeredTSM;

		protected virtual void OnEnable()
		{
			if (terrainMaterial != null)
			{
				registeredTSM = terrainMaterial;

				terrainMaterial.OnOverrideSharedMaterial += HandleOverrideSharedMaterial;
			}
		}

		protected virtual void OnDisable()
		{
			if (registeredTSM != null)
			{
				registeredTSM.OnOverrideSharedMaterial -= HandleOverrideSharedMaterial;

				registeredTSM = null;
			}
		}

		private void HandleOverrideSharedMaterial(ref SgtSharedMaterial sharedMaterial, Camera camera)
		{
			var eye = ocean.transform.InverseTransformPoint(camera.transform.position);

			if (eye.magnitude < ocean.Radius)
			{
				sharedMaterial = underwaterAtmosphere;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainUnderwater;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainUnderwater_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("ocean", "The main ocean applied to the planet. This is used for the radius check to see if the camera is underwater.");
			Draw("terrainMaterial", "The terrain material renderer. This component swaps its shared material based on the camera position.");
			Draw("underwaterAtmosphere", "The material applied to the terrain when the camera is under the water.");
		}
	}
}
#endif