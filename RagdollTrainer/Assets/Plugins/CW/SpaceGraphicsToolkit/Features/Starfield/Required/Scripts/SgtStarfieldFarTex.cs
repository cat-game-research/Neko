using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This component allows you to generate the SgtStarfield.FarTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtStarfield))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtStarfieldFarTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Starfield FarTex")]
	public class SgtStarfieldFarTex : MonoBehaviour
	{
		/// <summary>The transition style.</summary>
		public SgtEase.Type Ease { set { if (ease != value) { ease = value; DirtyTexture(); } } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>The sharpness of the transition.</summary>
		public float Sharpness { set { if (sharpness != value) { sharpness = value; DirtyTexture(); } } get { return sharpness; } } [SerializeField] private float sharpness = 1.0f;

		/// <summary>Should this component also control the starfield's <b>FarRangeRadius</b> setting?
		/// -1 = Don't override.</summary>
		public float OverrideRadius { set { if (overrideRadius != value) { overrideRadius = value; DirtyTexture(); } } get { return overrideRadius; } } [SerializeField] private float overrideRadius = -1.0f;

		/// <summary>Should this component also control the starfield's <b>FarRangeRecip</b> setting?
		/// -1 = Don't override.</summary>
		public float OverrideRange { set { if (overrideRange != value) { overrideRange = value; DirtyTexture(); } } get { return overrideRange; } } [SerializeField] private float overrideRange = -1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtStarfield cachedStarfield;

		private static int _SGT_FarTex        = Shader.PropertyToID("_SGT_FarTex");
		private static int _SGT_FarRadius     = Shader.PropertyToID("_SGT_FarRadius");
		private static int _SGT_FarRangeRecip = Shader.PropertyToID("_SGT_FarRangeRecip");

		public Texture2D GeneratedTexture
		{
			get
			{
				return generatedTexture;
			}
		}

		public void DirtyTexture()
		{
			UpdateTexture();
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.
		/// Once done, you can remove this component, and set the <b>SgtStarfield</b> component's <b>FarTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Starfield FarTex");

			if (importer != null)
			{
				importer.textureCompression  = UnityEditor.TextureImporterCompression.Uncompressed;
				importer.alphaSource         = UnityEditor.TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Clamp;
				importer.filterMode          = FilterMode.Trilinear;
				importer.anisoLevel          = 16;
				importer.alphaIsTransparency = true;

				importer.SaveAndReimport();
			}
		}
#endif

		protected virtual void OnEnable()
		{
			cachedStarfield = GetComponent<SgtStarfield>();

			cachedStarfield.OnSetProperties += HandleSetProperties;
		}

		protected virtual void OnDisable()
		{
			cachedStarfield.OnSetProperties -= HandleSetProperties;
		}

		protected virtual void Start()
		{
			if (generatedTexture == null)
			{
				UpdateTexture();
			}
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(generatedTexture);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			UpdateTexture();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			UpdateTexture();
		}
#endif

		private void HandleSetProperties(Material properties)
		{
			properties.SetTexture(_SGT_FarTex, generatedTexture);

			if (overrideRadius >= 0.0f)
			{
				properties.SetFloat(_SGT_FarRadius, overrideRadius);
			}

			if (overrideRange >= 0.0f)
			{
				properties.SetFloat(_SGT_FarRangeRecip, CwHelper.Reciprocal(overrideRange));
			}
		}

		private void UpdateTexture()
		{
			var width = 256;

			// Destroy if invalid
			if (generatedTexture != null)
			{
				if (generatedTexture.width != width || generatedTexture.height != 1)
				{
					generatedTexture = CwHelper.Destroy(generatedTexture);
				}
			}

			// Create?
			if (generatedTexture == null)
			{
				generatedTexture = CwHelper.CreateTempTexture2D("Far (Generated)", width, 1, TextureFormat.ARGB32);

				generatedTexture.wrapMode = TextureWrapMode.Clamp;
			}

			var stepU = 1.0f / (width - 1);

			for (var x = 0; x < width; x++)
			{
				WritePixel(stepU * x, x);
			}

			generatedTexture.Apply();
		}

		private void WritePixel(float u, int x)
		{
			var fade  = CwHelper.Saturate(SgtEase.Evaluate(ease, CwHelper.Sharpness(u, sharpness)));
			var color = new Color(fade, fade, fade, fade);

			generatedTexture.SetPixel(x, 0, CwHelper.ToGamma(color));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Starfield
{
	using UnityEditor;
	using TARGET = SgtStarfieldFarTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtStarfieldFarTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			Draw("ease", ref dirtyTexture, "The transition style.");
			BeginError(Any(tgts, t => t.Sharpness == 0.0f));
				Draw("sharpness", ref dirtyTexture, "The sharpness of the transition.");
			EndError();
			Draw("overrideRadius", "Should this component also control the starfield's <b>RangeRadius</b> setting?\n\n-1 = Don't override.");
			Draw("overrideRange", "Should this component also control the starfield's <b>RangeRecip</b> setting?\n\n-1 = Don't override.");

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif