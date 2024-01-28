using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Flare
{
	/// <summary>This component allows you to generate the material and texture for an SgtFlare.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFlare))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFlareMainTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Flare MainTex")]
	public class SgtFlareMainTex : MonoBehaviour
	{
		/// <summary>The base color will be multiplied by this.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The color transition style.</summary>
		public SgtEase.Type Ease { set { ease = value; } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Exponential;

		/// <summary>The sharpness of the red transition.</summary>
		public float SharpnessR { set { sharpnessR = value; } get { return sharpnessR; } } [SerializeField] private float sharpnessR = 3.0f;

		/// <summary>The sharpness of the green transition.</summary>
		public float SharpnessG { set { sharpnessG = value; } get { return sharpnessG; } } [SerializeField] private float sharpnessG = 2.0f;

		/// <summary>The sharpness of the blue transition.</summary>
		public float SharpnessB { set { sharpnessB = value; } get { return sharpnessB; } } [SerializeField] private float sharpnessB = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtFlare cachedFlare;

		private static int _SGT_MainTex = Shader.PropertyToID("_SGT_MainTex");

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
		/// Once done, you can remove this component, and set the <b>SgtFlare</b> component's <b>Material</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Flare Texture (Generated)");

			if (importer != null)
			{
				importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
				importer.alphaSource        = UnityEditor.TextureImporterAlphaSource.None;
				importer.wrapMode           = TextureWrapMode.Clamp;
				importer.filterMode         = FilterMode.Trilinear;
				importer.anisoLevel         = 16;

				importer.SaveAndReimport();
			}
		}
#endif

		protected virtual void OnEnable()
		{
			cachedFlare = GetComponent<SgtFlare>();

			cachedFlare.OnSetProperties += HandleSetProperties;

			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			cachedFlare.OnSetProperties -= HandleSetProperties;
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
			properties.SetTexture(_SGT_MainTex, generatedTexture);
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
				generatedTexture = CwHelper.CreateTempTexture2D("MainTex (Generated)", width, 1, TextureFormat.ARGB32);

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
			var finalColor = color;

			finalColor.r *= 1.0f - SgtEase.Evaluate(ease, CwHelper.Sharpness(u, sharpnessR));
			finalColor.g *= 1.0f - SgtEase.Evaluate(ease, CwHelper.Sharpness(u, sharpnessG));
			finalColor.b *= 1.0f - SgtEase.Evaluate(ease, CwHelper.Sharpness(u, sharpnessB));
			finalColor.a  = finalColor.grayscale;

			generatedTexture.SetPixel(x, 0, CwHelper.ToGamma(CwHelper.Saturate(finalColor)));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Flare
{
	using UnityEditor;
	using TARGET = SgtFlareMainTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFlareMainTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			Draw("color", ref dirtyTexture, "The base color will be multiplied by this.");
			Draw("ease", ref dirtyTexture, "The color transition style.");
			Draw("sharpnessR", ref dirtyTexture, "The sharpness of the red transition.");
			Draw("sharpnessG", ref dirtyTexture, "The sharpness of the green transition.");
			Draw("sharpnessB", ref dirtyTexture, "The sharpness of the blue transition.");

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif