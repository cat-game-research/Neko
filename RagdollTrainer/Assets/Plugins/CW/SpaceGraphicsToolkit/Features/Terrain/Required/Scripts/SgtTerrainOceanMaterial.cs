using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render the <b>SgtTerrain</b> component using the <b>SGT Planet</b> shader.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(200)]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainOceanMaterial")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Ocean Material")]
	public class SgtTerrainOceanMaterial : SgtTerrainPlanetMaterial
	{
		/// <summary>Should the ocean surface texture be animated?</summary>
		public bool Animate { set { animate = value; } get { return animate; } } [SerializeField] private bool animate;

		/// <summary>The generated water texture will be based on this texture.
		/// NOTE: This should be a normal map.</summary>
		public Texture BaseTexture { set { baseTexture = value; } get { return baseTexture; } } [SerializeField] private Texture baseTexture;

		/// <summary>The speed of the water animation.</summary>
		public float Strength { set { strength = value; } get { return strength; } } [SerializeField] private float strength = 1.0f;

		/// <summary>The speed of the water animation.</summary>
		public float Speed { set { speed = value; } get { return speed; } } [SerializeField] private float speed = 5.0f;

		/// <summary>Should the animated texture be applied to the first detail layer?</summary>
		public bool DetailA { set { detailA = value; } get { return detailA; } } [SerializeField] private bool detailA;

		/// <summary>Should the animated texture be applied to the second detail layer?</summary>
		public bool DetailB { set { detailB = value; } get { return detailB; } } [SerializeField] private bool detailB = true;

		/// <summary>Should the animated texture be applied to the third detail layer?</summary>
		public bool DetailC { set { detailC = value; } get { return detailC; } } [SerializeField] private bool detailC = true;

		[System.NonSerialized]
		private RenderTexture generatedTexture;

		[SerializeField]
		private float age;

		private static Material cachedMaterial;

		private static int _MainTex              = Shader.PropertyToID("_MainTex");
		private static int _Age                  = Shader.PropertyToID("_Age");
		private static int _NormalStrength       = Shader.PropertyToID("_NormalStrength");
		private static int _Radius               = Shader.PropertyToID("_Radius");
		private static int _WorldToLocal         = Shader.PropertyToID("_WorldToLocal");
		private static int _BakedDetailMap       = Shader.PropertyToID("_BakedDetailMap");
		private static int _BakedDetailMap_Ext_1 = Shader.PropertyToID("_BakedDetailMap_Ext_1");
		private static int _BakedDetailMap_Ext_2 = Shader.PropertyToID("_BakedDetailMap_Ext_2");

		protected virtual void OnDestroy()
		{
			if (generatedTexture != null)
			{
				generatedTexture = CwHelper.Destroy(generatedTexture);
			}
		}

		protected override void Update()
		{
			base.Update();

			if (Application.isPlaying == true)
			{
				age += Time.deltaTime * speed;
			}

			if (animate == true && baseTexture != null)
			{
				if (generatedTexture == null)
				{
					generatedTexture = new RenderTexture(baseTexture.width, baseTexture.height, 0, RenderTextureFormat.ARGB32, 8);

					generatedTexture.wrapMode         = TextureWrapMode.Repeat;
					generatedTexture.useMipMap        = true;
					generatedTexture.autoGenerateMips = false;
					generatedTexture.filterMode       = FilterMode.Trilinear;
					generatedTexture.anisoLevel       = 8;
				}

				if (cachedMaterial == null)
				{
					cachedMaterial = CwHelper.CreateTempMaterial("PlanetWater (Generated)", SgtCommon.ShaderNamePrefix + "PlanetWater");
				}

				cachedMaterial.SetTexture(_MainTex, baseTexture);
				cachedMaterial.SetFloat(_Age, age);
				cachedMaterial.SetFloat(_NormalStrength, strength);

				Graphics.Blit(null, generatedTexture, cachedMaterial);

				generatedTexture.GenerateMips();
			}

			var cachedTerrainOcean = cachedTerrain as SgtTerrainOcean;

			if (cachedTerrainOcean != null)
			{
				bakedDetailTilingA = cachedTerrainOcean.BakedDetailTilingA;
				bakedDetailTilingB = cachedTerrainOcean.BakedDetailTilingB;
				bakedDetailTilingC = cachedTerrainOcean.BakedDetailTilingC;
			}
		}

		protected override void PreRenderMeshes(SgtProperties properties)
		{
			base.PreRenderMeshes(properties);

			if (detailA == true && animate == true && generatedTexture != null)
			{
				properties.SetTexture(_BakedDetailMap, generatedTexture);
			}
			else
			{
				properties.Clear(_BakedDetailMap);
			}

			if (detailB == true && animate == true && generatedTexture != null)
			{
				properties.SetTexture(_BakedDetailMap_Ext_1, generatedTexture);
			}
			else
			{
				properties.Clear(_BakedDetailMap_Ext_1);
			}

			if (detailC == true && animate == true && generatedTexture != null)
			{
				properties.SetTexture(_BakedDetailMap_Ext_2, generatedTexture);
			}
			else
			{
				properties.Clear(_BakedDetailMap_Ext_2);
			}

			properties.SetFloat(_Radius, (float)cachedTerrain.Radius);
			properties.SetMatrix(_WorldToLocal, transform.worldToLocalMatrix);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainOceanMaterial;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainOceanMaterial_Editor : SgtTerrainPlanetMaterial_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			Draw("animate", "Should the ocean surface texture be animated?");
			if (Any(tgts, t => t.Animate == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.BaseTexture == null));
						Draw("baseTexture", "The generated water texture will be based on this texture.\n\nNOTE: This should be a normal map.");
					EndError();
					Draw("strength", "The strength of the normal map.");
					Draw("speed", "The speed of the water animation.");

					Separator();

					Draw("detailA", "Should the animated texture be applied to the first detail layer?");
					Draw("detailB", "Should the animated texture be applied to the second detail layer?");
					Draw("detailC", "Should the animated texture be applied to the third detail layer?");
				EndIndent();
			}

			CwDepthTextureMode_Editor.RequireDepth();
		}
	}
}
#endif