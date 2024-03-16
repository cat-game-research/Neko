using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Jovian
{
	/// <summary>This component allows you to generate the <b>DepthTex</b> setting for jovian materials.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtJovian))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtJovianDepthTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Jovian DepthTex")]
	public class SgtJovianDepthTex : MonoBehaviour
	{
		/// <summary>The rim transition style.</summary>
		public SgtEase.Type RimEase { set { if (rimEase != value) { rimEase = value; DirtyTexture(); } } get { return rimEase; } } [SerializeField] private SgtEase.Type rimEase = SgtEase.Type.Exponential;

		/// <summary>The rim transition sharpness.</summary>
		public float RimPower { set { if (rimPower != value) { rimPower = value; DirtyTexture(); } } get { return rimPower; } } [SerializeField] private float rimPower = 5.0f;

		/// <summary>The rim color.</summary>
		public Color RimColor { set { if (rimColor != value) { rimColor = value; DirtyTexture(); } } get { return rimColor; } } [SerializeField] private Color rimColor = new Color(1.0f, 0.0f, 0.0f, 0.25f);

		/// <summary>The density of the atmosphere.</summary>
		public float AlphaDensity { set { if (alphaDensity != value) { alphaDensity = value; DirtyTexture(); } } get { return alphaDensity; } } [SerializeField] private float alphaDensity = 50.0f;

		/// <summary>The strength of the density fading in the upper atmosphere.</summary>
		public float AlphaFade { set { if (alphaFade != value) { alphaFade = value; DirtyTexture(); } } get { return alphaFade; } } [SerializeField] private float alphaFade = 2.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtJovian cachedJovian;

		private static int _SGT_DepthTex = Shader.PropertyToID("_SGT_DepthTex");

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
		/// Once done, you can remove this component, and set the <b>SgtJovian</b> component's <b>DepthTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Jovian DepthTex");

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
			cachedJovian = GetComponent<SgtJovian>();

			cachedJovian.OnSetProperties += HandleSetProperties;

			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			cachedJovian.OnSetProperties -= HandleSetProperties;
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
			properties.SetTexture(_SGT_DepthTex, generatedTexture);
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
				generatedTexture = CwHelper.CreateTempTexture2D("DepthTex (Generated)", width, 1, TextureFormat.ARGB32);

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
			var rim   = 1.0f - SgtEase.Evaluate(rimEase, 1.0f - Mathf.Pow(1.0f - u, rimPower));
			var color = Color.Lerp(Color.white, rimColor, rim * rimColor.a);

			color.a = 1.0f - Mathf.Pow(1.0f - Mathf.Pow(u, alphaFade), alphaDensity);

			generatedTexture.SetPixel(x, 0, CwHelper.ToGamma(CwHelper.Saturate(color)));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Jovian
{
	using UnityEditor;
	using TARGET = SgtJovianDepthTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtJovianDepthTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			Draw("rimEase", ref dirtyTexture, "The rim transition style.");
			BeginError(Any(tgts, t => t.RimPower < 1.0f));
				Draw("rimPower", ref dirtyTexture, "The rim transition sharpness.");
			EndError();
			Draw("rimColor", ref dirtyTexture, "The rim color.");

			Separator();

			BeginError(Any(tgts, t => t.AlphaDensity < 1.0f));
				Draw("alphaDensity", ref dirtyTexture, "The density of the atmosphere.");
			EndError();
			BeginError(Any(tgts, t => t.AlphaFade < 1.0f));
				Draw("alphaFade", ref dirtyTexture, "The strength of the density fading in the upper atmosphere.");
			EndError();

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif