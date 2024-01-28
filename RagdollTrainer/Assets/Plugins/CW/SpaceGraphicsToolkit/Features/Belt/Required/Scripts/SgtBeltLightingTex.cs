using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Belt
{
	/// <summary>This component allows you to generate the SgtBelt.LightingTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtBelt))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBeltLightingTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Belt LightingTex")]
	public class SgtBeltLightingTex : MonoBehaviour
	{
		/// <summary>How sharp the incoming light scatters forward.</summary>
		public float FrontPower { set { if (frontPower != value) { frontPower = value; DirtyTexture(); } } get { return frontPower; } } [SerializeField] private float frontPower = 2.0f;

		/// <summary>How sharp the incoming light scatters backward.</summary>
		public float BackPower { set { if (backPower != value) { backPower = value; DirtyTexture(); } } get { return backPower; } } [SerializeField] private float backPower = 3.0f;

		/// <summary>The strength of the back scattered light.</summary>
		public float BackStrength { set { if (backStrength != value) { backStrength = value; DirtyTexture(); } } get { return backStrength; } } [SerializeField] [Range(0.0f, 1.0f)] private float backStrength = 0.0f;

		/// <summary>The of the perpendicular scattered light.</summary>
		public float BaseStrength { set { if (baseStrength != value) { baseStrength = value; DirtyTexture(); } } get { return baseStrength; } } [SerializeField] [Range(0.0f, 1.0f)] private float baseStrength = 0.0f;

		[System.NonSerialized]
		private SgtBelt cachedBelt;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		private static int _SGT_LightingTex = Shader.PropertyToID("_SGT_LightingTex");
	
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
		/// Once done, you can remove this component, and set the <b>SgtBelt</b> component's <b>LightingTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Belt LightingTex");

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
			cachedBelt = GetComponent<SgtBelt>();

			cachedBelt.OnSetProperties += HandleSetProperties;

			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			cachedBelt.OnSetProperties -= HandleSetProperties;
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
			UpdateTexture();
		}
#endif

		private void HandleSetProperties(Material properties)
		{
			properties.SetTexture(_SGT_LightingTex, generatedTexture);
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
				generatedTexture = CwHelper.CreateTempTexture2D("LightingTex (Generated)", width, 1, TextureFormat.ARGB32);

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
			var back     = Mathf.Pow(1.0f - u,  backPower) * backStrength;
			var front    = Mathf.Pow(       u, frontPower);
			var lighting = baseStrength;

			lighting = Mathf.Lerp(lighting, 1.0f, back );
			lighting = Mathf.Lerp(lighting, 1.0f, front);
			lighting = CwHelper.Saturate(lighting);

			var color = new Color(lighting, lighting, lighting, 0.0f);
		
			generatedTexture.SetPixel(x, 0, CwHelper.ToGamma(color));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Belt
{
	using UnityEditor;
	using TARGET = SgtBeltLightingTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBeltLightingTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			BeginError(Any(tgts, t => t.FrontPower < 0.0f));
				Draw("frontPower", ref dirtyTexture, "How sharp the incoming light scatters forward.");
			EndError();
			BeginError(Any(tgts, t => t.BackPower < 0.0f));
				Draw("backPower", ref dirtyTexture, "How sharp the incoming light scatters backward.");
			EndError();

			BeginError(Any(tgts, t => t.BackStrength < 0.0f));
				Draw("backStrength", ref dirtyTexture, "The strength of the back scattered light.");
			EndError();
			BeginError(Any(tgts, t => t.BackStrength < 0.0f));
				Draw("baseStrength", ref dirtyTexture, "The of the perpendicular scattered light.");
			EndError();

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif