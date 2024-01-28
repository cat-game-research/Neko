using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Aurora
{
	/// <summary>This component allows you to generate the SgtAurora.MainTex field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtAurora))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtAuroraMainTex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Aurora MainTex")]
	public class SgtAuroraMainTex : MonoBehaviour
	{
		/// <summary>The strength of the noise points.</summary>
		public float NoiseStrength { set { if (noiseStrength != value) { noiseStrength = value; DirtyTexture(); } } get { return noiseStrength; } } [SerializeField] [Range(0.0f, 1.0f)] private float noiseStrength = 0.75f;

		/// <summary>The amount of noise points.</summary>
		public int NoisePoints { set { if (noisePoints != value) { noisePoints = value; DirtyTexture(); } } get { return noisePoints; } } [SerializeField] private int noisePoints = 30;

		/// <summary>The random seed used when generating this texture.</summary>
		public int NoiseSeed { set { if (noiseSeed != value) { noiseSeed = value; DirtyTexture(); } } get { return noiseSeed; } } [SerializeField] [CwSeed] private int noiseSeed;

		/// <summary>The transition style between the top and middle.</summary>
		public SgtEase.Type TopEase { set { if (topEase != value) { topEase = value; DirtyTexture(); } } get { return topEase; } } [SerializeField] private SgtEase.Type topEase = SgtEase.Type.Quintic;

		/// <summary>The transition strength between the top and middle.</summary>
		public float TopSharpness { set { if (topSharpness != value) { topSharpness = value; DirtyTexture(); } } get { return topSharpness; } } [SerializeField] private float topSharpness = 1.0f;

		/// <summary>The point separating the top from bottom.</summary>
		public float MiddlePoint { set { if (middlePoint != value) { middlePoint = value; DirtyTexture(); } } get { return middlePoint; } } [SerializeField] [Range(0.0f, 1.0f)] private float middlePoint = 0.25f;

		/// <summary>The base color of the aurora starting from the bottom.</summary>
		public Color MiddleColor { set { if (middleColor != value) { middleColor = value; DirtyTexture(); } } get { return middleColor; } } [SerializeField] private Color middleColor = Color.green;

		/// <summary>The transition style between the bottom and top of the aurora.</summary>
		public SgtEase.Type MiddleEase { set { if (middleEase != value) { middleEase = value; DirtyTexture(); } } get { return middleEase; } } [SerializeField] private SgtEase.Type middleEase = SgtEase.Type.Exponential;

		/// <summary>The strength of the color transition between the bottom and top.</summary>
		public float MiddleSharpness { set { if (middleSharpness != value) { middleSharpness = value; DirtyTexture(); } } get { return middleSharpness; } } [SerializeField] private float middleSharpness = 3.0f;

		/// <summary>The transition style between the bottom and middle.</summary>
		public SgtEase.Type BottomEase { set { if (bottomEase != value) { bottomEase = value; DirtyTexture(); } } get { return bottomEase; } } [SerializeField] private SgtEase.Type bottomEase = SgtEase.Type.Exponential;

		/// <summary>The transition strength between the bottom and middle.</summary>
		public float BottomSharpness { set { if (bottomSharpness != value) { bottomSharpness = value; DirtyTexture(); } } get { return bottomSharpness; } } [SerializeField] private float bottomSharpness = 1.0f;

		[System.NonSerialized]
		private Texture2D generatedTexture;

		[System.NonSerialized]
		private SgtAurora cachedAurora;

		[System.NonSerialized]
		private static List<float> tempPoints = new List<float>();

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
		/// Once done, you can remove this component, and set the <b>SgtAurora</b> component's <b>MainTex</b> setting using the exported asset.</summary>
		[ContextMenu("Export Texture")]
		public void ExportTexture()
		{
			var importer = CwHelper.ExportTextureDialog(generatedTexture, "Aurora MainTex");

			if (importer != null)
			{
				importer.textureCompression  = UnityEditor.TextureImporterCompression.Uncompressed;
				importer.alphaSource         = UnityEditor.TextureImporterAlphaSource.FromInput;
				importer.wrapMode            = TextureWrapMode.Repeat;
				importer.filterMode          = FilterMode.Trilinear;
				importer.anisoLevel          = 16;
				importer.alphaIsTransparency = true;

				importer.SaveAndReimport();
			}
		}
#endif

		protected virtual void OnEnable()
		{
			cachedAurora = GetComponent<SgtAurora>();

			cachedAurora.OnSetProperties += HandleSetProperties;

			UpdateTexture();
		}

		protected virtual void OnDisable()
		{
			cachedAurora.OnSetProperties -= HandleSetProperties;
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
			var width  = 256;
			var height = 64;

			if (noisePoints > 0)
			{
				// Destroy if invalid
				if (generatedTexture != null)
				{
					if (generatedTexture.width != width || generatedTexture.height != height)
					{
						generatedTexture = CwHelper.Destroy(generatedTexture);
					}
				}

				// Create?
				if (generatedTexture == null)
				{
					generatedTexture = CwHelper.CreateTempTexture2D("Aurora MainTex (Generated)", width, height, TextureFormat.ARGB32);

					generatedTexture.wrapMode = TextureWrapMode.Repeat;
				}

				CwHelper.BeginSeed(noiseSeed);
				{
					tempPoints.Clear();

					for (var i = 0; i < noisePoints; i++)
					{
						tempPoints.Add(1.0f - Random.Range(0.0f, noiseStrength));
					}
				}
				CwHelper.EndSeed();

				var stepU = 1.0f / (width  - 1);
				var stepV = 1.0f / (height - 1);

				for (var y = 0; y < height; y++)
				{
					var v = stepV * y;

					for (var x = 0; x < width; x++)
					{
						WritePixel(stepU * x, v, x, y);
					}
				}

				generatedTexture.Apply();
			}
		}

		private void WritePixel(float u, float v, int x, int y)
		{
			var noise      = u * noisePoints;
			var noiseIndex = (int)noise;
			var noiseFrac  = noise % 1.0f;
			var noiseA     = tempPoints[(noiseIndex + 0) % noisePoints];
			var noiseB     = tempPoints[(noiseIndex + 1) % noisePoints];
			var noiseC     = tempPoints[(noiseIndex + 2) % noisePoints];
			var noiseD     = tempPoints[(noiseIndex + 3) % noisePoints];
			var color      = middleColor;

			if (v < middlePoint)
			{
				color.a = SgtEase.Evaluate(bottomEase, CwHelper.Sharpness(Mathf.InverseLerp(0.0f, middlePoint, v), bottomSharpness));
			}
			else
			{
				color.a = SgtEase.Evaluate(topEase, CwHelper.Sharpness(Mathf.InverseLerp(1.0f, middlePoint, v), topSharpness));
			}

			var middle = SgtEase.Evaluate(middleEase, CwHelper.Sharpness(1.0f - v, middleSharpness));

			color.a *= SgtCommon.HermiteInterpolate(noiseA, noiseB, noiseC, noiseD, noiseFrac);

			color.r *= middle * color.a;
			color.g *= middle * color.a;
			color.b *= middle * color.a;
			color.a *= 1.0f - middle;
		
			generatedTexture.SetPixel(x, y, CwHelper.ToGamma(CwHelper.Saturate(color)));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Aurora
{
	using UnityEditor;
	using TARGET = SgtAuroraMainTex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtAuroraMainTex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyTexture = false;

			Draw("noiseStrength", ref dirtyTexture, "The strength of the noise points.");
			BeginError(Any(tgts, t => t.NoisePoints <= 0));
				Draw("noisePoints", ref dirtyTexture, "The amount of noise points.");
			EndError();
			Draw("noiseSeed", ref dirtyTexture, "The random seed used when generating this texture.");

			Separator();

			Draw("topEase", ref dirtyTexture, "The transition style between the top and middle.");
			Draw("topSharpness", ref dirtyTexture, "The transition strength between the top and middle.");

			Separator();

			Draw("middlePoint", ref dirtyTexture, "The point separating the top from bottom.");
			Draw("middleColor", ref dirtyTexture, "The base color of the aurora starting from the bottom.");
			Draw("middleEase", ref dirtyTexture, "The transition style between the bottom and top of the aurora.");
			Draw("middleSharpness", ref dirtyTexture, "The strength of the color transition between the bottom and top.");

			Separator();

			Draw("bottomEase", ref dirtyTexture, "The transition style between the bottom and middle.");
			Draw("bottomSharpness", ref dirtyTexture, "The transition strength between the bottom and middle.");

			if (dirtyTexture == true) Each(tgts, t => t.DirtyTexture(), true, true);
		}
	}
}
#endif