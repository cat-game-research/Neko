using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Ring
{
	/// <summary>This component allows you to generate the <b>NearTex</b> for the <b>SgtRing</b> component's material.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtRing))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtRingNearTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Ring NearTex")]
	public class SgtRingNearTex : MonoBehaviour
	{
		/// <summary>The ease type used for the transition.</summary>
		public SgtEase.Type Ease { set { if (ease != value) { ease = value; DirtyTexture(); } } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>The sharpness of the transition.</summary>
		public float Sharpness { set { if (sharpness != value) { sharpness = value; DirtyTexture(); } } get { return sharpness; } } [SerializeField] private float sharpness = 1.0f;

		/// <summary>The start point of the fading.</summary>
		public float Offset { set { if (offset != value) { offset = value; DirtyTexture(); } } get { return offset; } } [Range(0.0f, 1.0f)] [SerializeField] private float offset;

		/// <summary>Should this component also control the ring's <b>NearRangeRecip</b> setting?
		/// -1 = Don't override.</summary>
		public float OverrideRange { set { if (overrideRange != value) { overrideRange = value; } } get { return overrideRange; } } [SerializeField] private float overrideRange = -1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtRing cachedRing;

		private static int _SGT_NearTex        = Shader.PropertyToID("_SGT_NearTex");
		private static int _SGT_NearRangeRecip = Shader.PropertyToID("_SGT_NearRangeRecip");

		public void DirtyTexture()
		{
			UpdateTexture();
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated texture as an asset.
		/// Once done, you can remove this component, and set the <b>SgtRing</b> component's <b>NearTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Ring NearTex");

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
			cachedRing = GetComponent<SgtRing>();

			cachedRing.OnSetProperties += HandleSetProperties;

			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			cachedRing.OnSetProperties -= HandleSetProperties;
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(generatedTexture);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyTexture();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			DirtyTexture();
		}
#endif

		private void HandleSetProperties(Material properties)
		{
			properties.SetTexture(_SGT_NearTex, generatedTexture);

			if (overrideRange >= 0.0f)
			{
				properties.SetFloat(_SGT_NearRangeRecip, CwHelper.Reciprocal(overrideRange));
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
				generatedTexture = CwHelper.CreateTempTexture2D("NearTex (Generated)", width, 1, TextureFormat.ARGB32);

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
			var e     = CwHelper.Saturate(SgtEase.Evaluate(ease, CwHelper.Sharpness(Mathf.InverseLerp(offset, 1.0f, u), sharpness)));
			var color = new Color(1.0f, 1.0f, 1.0f, e);

			generatedTexture.SetPixel(x, 0, color);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Ring
{
	using UnityEditor;
	using TARGET = SgtRingNearTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtRingNearTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			Draw("ease", ref dirtyTexture, "The ease type used for the transition.");
			Draw("sharpness", ref dirtyTexture, "The sharpness of the transition.");
			BeginError(Any(tgts, t => t.Offset >= 1.0f));
				Draw("offset", ref dirtyTexture, "The start point of the fading.");
			EndError();
			Draw("overrideRange", "Should this component also control the ring's <b>RangeRecip</b> setting?\n\n-1 = Don't override.");

			if (dirtyTexture == true)
			{
				Each(tgts, t => t.DirtyTexture(), true, true);
			}
		}
	}
}
#endif