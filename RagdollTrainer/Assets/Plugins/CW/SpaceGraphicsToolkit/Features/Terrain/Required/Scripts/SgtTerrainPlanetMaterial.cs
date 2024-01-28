using UnityEngine;
using Unity.Mathematics;
using CW.Common;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the <b>SGT Planet</b> shader.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(200)]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainPlanetMaterial")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Planet Material")]
	public class SgtTerrainPlanetMaterial : MonoBehaviour
	{
		/// <summary>The planet material that will be rendered.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>Should the planet cast shadows?</summary>
		public bool CastShadows { set { castShadows = value; } get { return castShadows; } } [SerializeField] private bool castShadows = true;

		/// <summary>Should the planet receive shadows?</summary>
		public bool ReceiveShadows { set { receiveShadows = value; } get { return receiveShadows; } } [SerializeField] private bool receiveShadows = true;

		/// <summary>Normals bend incorrectly on high detail planets, so it's a good idea to fade them out. This allows you to set the camera distance at which the normals begin to fade out in local space.</summary>
		public double NormalFadeRange { set { normalFadeRange = value; } get { return normalFadeRange; } } [SerializeField] private double normalFadeRange;

		/// <summary>This allows you to specify the terrain used for the water surface. This is used to control where the beaches appear, if you enable that material feature.</summary>
		public SgtTerrain Water { set { if (water != value) { water = value; MarkAsDirty(); } } get { return water; } } [SerializeField] private SgtTerrain water;

		protected SgtTerrain cachedTerrain;

		private float bumpScale;

		protected int bakedDetailTilingA;
		protected int bakedDetailTilingB;
		protected int bakedDetailTilingC;

		private static int _BumpScale             = Shader.PropertyToID("_BumpScale");
		private static int _BakedDetailTilingA    = Shader.PropertyToID("_BakedDetailTilingA");
		private static int _BakedDetailTilingAMul = Shader.PropertyToID("_BakedDetailTilingAMul");
		private static int _BakedDetailTilingB    = Shader.PropertyToID("_BakedDetailTilingB");
		private static int _BakedDetailTilingC    = Shader.PropertyToID("_BakedDetailTilingC");
		private static int _ShoreHeight           = Shader.PropertyToID("_ShoreHeight");
		private static int _HasNight              = Shader.PropertyToID("_HasNight");
		private static int _NightDirection        = Shader.PropertyToID("_NightDirection");

		public void MarkAsDirty()
		{
			if (cachedTerrain != null)
			{
				cachedTerrain.MarkAsDirty();
			}
		}

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			cachedTerrain.OnDrawQuad += HandleDrawQuad;
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.OnDrawQuad -= HandleDrawQuad;

			MarkAsDirty();
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			MarkAsDirty();
		}

		protected virtual void Update()
		{
			if (material != null)
			{
				if (material.HasProperty(_BumpScale) == true)
				{
					bumpScale = material.GetFloat(_BumpScale);
				}
				else
				{
					bumpScale = 1.0f;
				}
			}

			if (normalFadeRange > 0.0 && CwHelper.Enabled(cachedTerrain) == true)
			{
				var localPosition = cachedTerrain.GetObserverLocalPosition();
				var localAltitude = math.length(localPosition);
				var localHeight   = cachedTerrain.GetLocalHeight(localPosition);

				bumpScale *= (float)math.saturate((localAltitude - localHeight) / normalFadeRange);
			}

			var cachedTerrainPlanet = cachedTerrain as SgtTerrainPlanet;

			if (cachedTerrainPlanet != null)
			{
				bakedDetailTilingA = cachedTerrainPlanet.BakedDetailTilingA;
				bakedDetailTilingB = cachedTerrainPlanet.BakedDetailTilingB;
				bakedDetailTilingC = cachedTerrainPlanet.BakedDetailTilingC;
			}
		}

		private void HandleDrawQuad(Camera camera, SgtTerrainQuad quad, Matrix4x4 matrix, int layer)
		{
			if (material != null)
			{
				var properties = quad.Properties;

				PreRenderMeshes(properties);

				foreach (var mesh in quad.CurrentMeshes)
				{
					Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, camera, 0, properties, castShadows, receiveShadows);
				}
			}
		}

		protected virtual void PreRenderMeshes(SgtProperties properties)
		{
			properties.SetFloat(_BumpScale, bumpScale);

			properties.SetInt(_BakedDetailTilingA, bakedDetailTilingA);
			properties.SetFloat(_BakedDetailTilingAMul, bakedDetailTilingA / 64.0f);
			properties.SetInt(_BakedDetailTilingB, bakedDetailTilingB);
			properties.SetInt(_BakedDetailTilingC, bakedDetailTilingC);

			if (water != null)
			{
				properties.SetFloat(_ShoreHeight, (float)(water.Radius - cachedTerrain.Radius));
			}

			// Write direction of nearest light?
			if (material != null && material.HasProperty(_HasNight) == true && material.GetFloat(_HasNight) == 1.0f)
			{
				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(mask, transform.position);

				SgtLight.FilterOut(transform.position);

				if (lights.Count > 0)
				{
					var position  = Vector3.zero;
					var direction = Vector3.forward;
					var color     = Color.white;
					var intensity = 0.0f;

					SgtLight.Calculate(lights[0], transform.position, 0.0f, default(Transform), default(Transform), ref position, ref direction, ref color, ref intensity);

					properties.SetVector(_NightDirection, -direction);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainPlanetMaterial;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainPlanetMaterial_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", "The planet material that will be rendered.");
			EndError();
			Draw("castShadows", "Should the planet cast shadows?");
			Draw("receiveShadows", "Should the planet receive shadows?");
			Draw("normalFadeRange", "Normals bend incorrectly on high detail planets, so it's a good idea to fade them out. This allows you to set the camera distance at which the normals begin to fade out in local space.");
			Draw("water", "This allows you to specify the terrain used for the water surface. This is used to control where the beaches appear, if you enable that material feature.");

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty());
			}
		}
	}
}
#endif