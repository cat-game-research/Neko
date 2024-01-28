using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the specified <b>SgtSharedMaterial</b>.
	/// Components like <b>SgtAtmosphere</b> give you a shared material.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainSharedMaterial")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Shared Material")]
	public class SgtTerrainSharedMaterial : MonoBehaviour, IOverridableSharedMaterial
	{
		/// <summary>The shared material that will be rendered.</summary>
		public SgtSharedMaterial SharedMaterial { set { sharedMaterial = value; } get { return sharedMaterial; } } [SerializeField] private SgtSharedMaterial sharedMaterial;

		//public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		public event SgtSharedMaterial.OverrideSharedMaterialSignature OnOverrideSharedMaterial;

		private SgtTerrain cachedTerrain;

		public void RegisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e)
		{
			OnOverrideSharedMaterial += e;
		}

		public void UnregisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e)
		{
			OnOverrideSharedMaterial -= e;
		}

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			cachedTerrain.OnDrawQuad += HandleDrawQuad;
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.OnDrawQuad -= HandleDrawQuad;
		}

		private void HandleDrawQuad(Camera camera, SgtTerrainQuad quad, Matrix4x4 matrix, int layer)
		{
			var finalSharedMaterial = sharedMaterial;

			if (OnOverrideSharedMaterial != null)
			{
				OnOverrideSharedMaterial.Invoke(ref finalSharedMaterial, camera);
			}

			if (CwHelper.Enabled(finalSharedMaterial) == true && finalSharedMaterial.Material != null)
			{
				//if (cameraOffset != 0.0f)
				//{
				//	var direction = Vector3.Normalize(camera.transform.position - transform.position);
				//
				//	matrix = Matrix4x4.Translate(direction * cameraOffset) * matrix;
				//}
				foreach (var mesh in quad.CurrentMeshes)
				{
					Graphics.DrawMesh(mesh, matrix, finalSharedMaterial.Material, gameObject.layer, camera);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainSharedMaterial;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainSharedMaterial_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SharedMaterial == null));
				Draw("sharedMaterial", "The shared material that will be rendered.");
			EndError();
			//Draw("cameraOffset");
		}
	}
}
#endif