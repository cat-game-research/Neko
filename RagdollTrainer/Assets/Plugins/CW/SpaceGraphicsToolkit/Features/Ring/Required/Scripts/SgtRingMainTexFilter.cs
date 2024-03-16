using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Ring
{
	/// <summary>This component allows you to generate the SgtRing.MainTex field based on a simple RGB texture of a ring.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtRing))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtRingMainTexFilter")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Ring MainTex Filter")]
	public class SgtRingMainTexFilter : MonoBehaviour
	{
		/// <summary>The source ring texture that will be filtered.</summary>
		public Texture2D Source { set { if (source != value) { source = value; DirtyTexture(); } } get { return source; } } [SerializeField] private Texture2D source;

		/// <summary>The format of the generated texture.</summary>
		public TextureFormat Format { set { if (format != value) { format = value; DirtyTexture(); } } get { return format; } } [SerializeField] private TextureFormat format = TextureFormat.ARGB32;

		/// <summary>The sharpness of the light/dark transition.</summary>
		public float Power { set { if (power != value) { power = value; DirtyTexture(); } } get { return power; } } [SerializeField] private float power = 0.5f;

		/// <summary>This allows you to control the brightness.</summary>
		public float Exposure { set { if (exposure != value) { exposure = value; DirtyTexture(); } } get { return exposure; } } [SerializeField] [Range(-1.0f, 1.0f)] private float exposure = 0.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtRing cachedRing;

		[System.NonSerialized]
		private bool cachedRingSet;

		public SgtRing CachedRing
		{
			get
			{
				if (cachedRingSet == false)
				{
					cachedRing    = GetComponent<SgtRing>();
					cachedRingSet = true;
				}

				return cachedRing;
			}
		}

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
		/// Once done, you can remove this component, and set the <b>SgtRing</b> component's <b>MainTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Ring MainTex");

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

		[ContextMenu("Apply Texture")]
		public void ApplyTexture()
		{
			CachedRing.MainTex = generatedTexture;
		}

		[ContextMenu("Remove Texture")]
		public void RemoveTexture()
		{
			if (CachedRing.MainTex == generatedTexture)
			{
				cachedRing.MainTex = null;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			RemoveTexture();
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

		private void UpdateTexture()
		{
			if (source != null)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != source.width || generatedTexture.height != 1 || generatedTexture.format != format)
					{
						generatedTexture = CwHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = CwHelper.CreateTempTexture2D("MainTex (Generated)", source.width, 1, format);

					generatedTexture.wrapMode = TextureWrapMode.Clamp;

					ApplyTexture();
				}

				for (var x = source.width - 1; x >= 0; x--)
				{
					WritePixel(x);
				}

				generatedTexture.Apply();
			}

			ApplyTexture();
		}

		private void WritePixel(int x)
		{
			var pixel   = source.GetPixel(x, 0);
			var highest = 0.0f;

			if (pixel.r > highest) highest = pixel.r;
			if (pixel.g > highest) highest = pixel.g;
			if (pixel.b > highest) highest = pixel.b;

			if (highest > 0.0f)
			{
				highest = 1.0f - Mathf.Pow(1.0f - highest, power);
				//var inv = 1.0f / highest;

				//pixel.r *= inv;
				//pixel.g *= inv;
				//pixel.b *= inv;
				pixel.a  = highest;
			}
			else
			{
				pixel.a = 0.0f;
			}

			pixel.r = Mathf.Pow(pixel.r, 1.0f - exposure);
			pixel.g = Mathf.Pow(pixel.g, 1.0f - exposure);
			pixel.b = Mathf.Pow(pixel.b, 1.0f - exposure);

			generatedTexture.SetPixel(x, 0, CwHelper.ToGamma(CwHelper.Saturate(pixel)));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Ring
{
	using UnityEditor;
	using TARGET = SgtRingMainTexFilter;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtRingMainTexFilter_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			BeginError(Any(tgts, t => t.Source == null));
				Draw("source", ref dirtyTexture, "The source ring texture that will be filtered.");
			EndError();
			Draw("format", ref dirtyTexture, "The format of the generated texture.");

			Separator();

			BeginError(Any(tgts, t => t.Power < 0.0f));
				Draw("power", ref dirtyTexture, "The sharpness of the light/dark transition.");
			EndError();
			Draw("exposure", ref dirtyTexture, "This allows you to control the brightness.");

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif