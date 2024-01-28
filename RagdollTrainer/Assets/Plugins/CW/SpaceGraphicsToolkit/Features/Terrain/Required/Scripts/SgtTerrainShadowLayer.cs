using UnityEngine;
using CW.Common;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the specified <b>SgtShadowLayer</b>.
	/// By default, only Unity's built-in shadows will be cast on terrains, and this allows you to cast shadow from rings and planets on this planet.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainShadowLayer")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Shadow Layer")]
	public class SgtTerrainShadowLayer : MonoBehaviour
	{
		/// <summary>The shared material that will be rendered.</summary>
		public SgtShadowLayer ShadowLayer { set { shadowLayer = value; } get { return shadowLayer; } } [SerializeField] private SgtShadowLayer shadowLayer;

		private SgtTerrain cachedTerrain;

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
			if (shadowLayer != null)
			{
				var finalMaterial = shadowLayer.Material;

				if (finalMaterial != null)
				{
					foreach (var mesh in quad.CurrentMeshes)
					{
						Graphics.DrawMesh(mesh, matrix, finalMaterial, gameObject.layer, camera);
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainShadowLayer;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainShadowLayer_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.ShadowLayer == null));
				Draw("shadowLayer", "The shared material that will be rendered.");
			EndError();
		}
	}
}
#endif