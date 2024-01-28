using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Atmosphere
{
	/// <summary>This component allows you to generate the SgtAtmosphere.InnerDepthTex and SgtAtmosphere.OuterDepthTex fields.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAtmosphere))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtAtmosphereDepthTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Atmosphere DepthTex")]
	public class SgtAtmosphereDepthTex : MonoBehaviour
	{
		/// <summary>This allows you to set the color that appears on the horizon.</summary>
		public Color HorizonColor { set { if (horizonColor != value) { horizonColor = value; DirtyTextures(); } } get { return horizonColor; } } [SerializeField] private Color horizonColor = Color.white;

		/// <summary>The base color of the inner texture.</summary>
		public Color InnerColor { set { if (innerColor != value) { innerColor = value; DirtyTextures(); } } get { return innerColor; } } [SerializeField] private Color innerColor = new Color(0.15f, 0.54f, 1.0f);

		/// <summary>The transition style between the surface and horizon.</summary>
		public SgtEase.Type InnerEase { set { if (innerEase != value) { innerEase = value; DirtyTextures(); } } get { return innerEase; } } [SerializeField] private SgtEase.Type innerEase = SgtEase.Type.Exponential;

		/// <summary>The strength of the inner texture transition.</summary>
		public float InnerColorSharpness { set { if (innerColorSharpness != value) { innerColorSharpness = value; DirtyTextures(); } } get { return innerColorSharpness; } } [SerializeField] private float innerColorSharpness = 2.0f;

		/// <summary>The strength of the inner texture transition.</summary>
		public float InnerAlphaSharpness { set { if (innerAlphaSharpness != value) { innerAlphaSharpness = value; DirtyTextures(); } } get { return innerAlphaSharpness; } } [SerializeField] private float innerAlphaSharpness = 3.0f;

		/// <summary>The base color of the outer texture.</summary>
		public Color OuterColor { set { if (outerColor != value) { outerColor = value; DirtyTextures(); } } get { return outerColor; } } [SerializeField] private Color outerColor = new Color(0.29f, 0.73f, 1.0f);

		/// <summary>The transition style between the sky and horizon.</summary>
		public SgtEase.Type OuterEase { set { if (outerEase != value) { outerEase = value; DirtyTextures(); } } get { return outerEase; } } [SerializeField] private SgtEase.Type outerEase = SgtEase.Type.Quadratic;

		/// <summary>The strength of the outer texture transition.</summary>
		public float OuterColorSharpness { set { if (outerColorSharpness != value) { outerColorSharpness = value; DirtyTextures(); } } get { return outerColorSharpness; } } [SerializeField] private float outerColorSharpness = 2.0f;

		/// <summary>The strength of the outer texture transition.</summary>
		public float OuterAlphaSharpness { set { if (outerAlphaSharpness != value) { outerAlphaSharpness = value; DirtyTextures(); } } get { return outerAlphaSharpness; } } [SerializeField] private float outerAlphaSharpness = 3.0f;

		[System.NonSerialized]
		private Texture2D generatedInnerTexture;

		[System.NonSerialized]
		private Texture2D generatedOuterTexture;

		[System.NonSerialized]
		private SgtAtmosphere cachedAtmosphere;

		private static int _SGT_InnerDepthTex = Shader.PropertyToID("_SGT_InnerDepthTex");
		private static int _SGT_OuterDepthTex = Shader.PropertyToID("_SGT_OuterDepthTex");

		public void DirtyTextures()
		{
			UpdateTextures();
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.
		/// Once done, you can remove this component, and set the <b>SgtAtmosphere</b> component's <b>InnerDepth</b> setting using the exported asset.</summary>
		[ContextMenu("Export Inner Texture")]
		public void ExportInnerTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedOuterTexture, "Atmosphere Inner DepthTex");

			if (importer != null)
			{
				importer.textureType         = UnityEditor.TextureImporterType.SingleChannel;
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

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.
		/// Once done, you can remove this component, and set the <b>SgtAtmosphere</b> component's <b>OuterDepth</b> setting using the exported asset.</summary>
		[ContextMenu("Export Outer Texture")]
		public void ExportOuterTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedOuterTexture, "Atmosphere Outer DepthTex");

			if (importer != null)
			{
				importer.textureType         = UnityEditor.TextureImporterType.SingleChannel;
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

		private void UpdateTextures()
		{
			var width = 256;

			ValidateTexture(ref generatedInnerTexture, "Inner DepthTex (Generated)", width);
			ValidateTexture(ref generatedOuterTexture, "Outer DepthTex (Generated)", width);

			var stepU = 1.0f / (width - 1);

			for (var x = 0; x < width; x++)
			{
				var u = stepU * x;

				WritePixel(generatedInnerTexture, u, x, innerColor, innerEase, innerColorSharpness, innerAlphaSharpness);
				WritePixel(generatedOuterTexture, u, x, outerColor, outerEase, outerColorSharpness, outerAlphaSharpness);
			}

			generatedInnerTexture.Apply();
			generatedOuterTexture.Apply();
		}

		protected virtual void OnEnable()
		{
			cachedAtmosphere = GetComponent<SgtAtmosphere>();

			cachedAtmosphere.OnSetProperties += HandleSetProperties;

			UpdateTextures();
		}

		protected virtual void OnDisable()
		{
			cachedAtmosphere.OnSetProperties -= HandleSetProperties;
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(generatedInnerTexture);
			CwHelper.Destroy(generatedOuterTexture);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			UpdateTextures();
		}

		private void ValidateTexture(ref Texture2D texture2D, string createName, int width)
		{
			// Destroy if invalid
			if (texture2D != null)
			{
				if (texture2D.width != width || texture2D.height != 1)
				{
					texture2D = CwHelper.Destroy(texture2D);
				}
			}

			// Create?
			if (texture2D == null)
			{
				texture2D = CwHelper.CreateTempTexture2D(createName, width, 1, TextureFormat.ARGB32);

				texture2D.wrapMode = TextureWrapMode.Clamp;
			}
		}

		private void HandleSetProperties(Material innerMaterial, Material outerMaterial)
		{
			innerMaterial.SetTexture(_SGT_InnerDepthTex, generatedInnerTexture);
			outerMaterial.SetTexture(_SGT_OuterDepthTex, generatedOuterTexture);
		}

		private void WritePixel(Texture2D texture2D, float u, int x, Color baseColor, SgtEase.Type ease, float colorSharpness, float alphaSharpness)
		{
			var colorU = CwHelper.Sharpness(u, colorSharpness); colorU = SgtEase.Evaluate(ease, colorU);
			var alphaU = CwHelper.Sharpness(u, alphaSharpness); alphaU = SgtEase.Evaluate(ease, alphaU);
			var color  = Color.Lerp(baseColor, horizonColor, colorU);

			color.a = CwHelper.ToGamma(alphaU);

			texture2D.SetPixel(x, 0, CwHelper.ToLinear(CwHelper.Saturate(color)));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Atmosphere
{
	using UnityEditor;
	using TARGET = SgtAtmosphereDepthTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtAtmosphereDepthTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTextures = false;

			Draw("horizonColor", ref dirtyTextures, "This allows you to set the color that appears on the horizon.");

			Separator();

			Draw("innerColor", ref dirtyTextures, "The base color of the inner texture.");
			Draw("innerEase", ref dirtyTextures, "The transition style between the surface and horizon.");
			Draw("innerColorSharpness", ref dirtyTextures, "The strength of the inner texture transition.");
			Draw("innerAlphaSharpness", ref dirtyTextures, "The strength of the inner texture transition.");

			Separator();

			Draw("outerColor", ref dirtyTextures, "The base color of the outer texture.");
			Draw("outerEase", ref dirtyTextures, "The transition style between the sky and horizon.");
			Draw("outerColorSharpness", ref dirtyTextures, "The strength of the outer texture transition.");
			Draw("outerAlphaSharpness", ref dirtyTextures, "The strength of the outer texture transition.");

			if (dirtyTextures == true) Each(tgts, t => t.DirtyTextures(), true, true);
		}
	}
}
#endif