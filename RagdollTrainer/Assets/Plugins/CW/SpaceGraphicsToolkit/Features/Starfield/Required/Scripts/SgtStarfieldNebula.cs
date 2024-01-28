using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This component allows you to render a nebula as a starfield from a single picture.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtStarfieldNebula")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Starfield Nebula")]
	public class SgtStarfieldNebula : SgtStarfield
	{
		public enum SourceType
		{
			None,
			Red,
			Green,
			Blue,
			Alpha,
			AverageRgb,
			MinRgb,
			MaxRgb
		}

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>This texture used to color the nebula particles.</summary>
		public Texture SourceTex { set { if (sourceTex != value) { sourceTex = value; DirtyMesh(); } } get { return sourceTex; } } [SerializeField] private Texture sourceTex;

		/// <summary>This brightness of the sampled SourceTex pixel for a particle to be spawned.</summary>
		public float Threshold { set { if (threshold != value) { threshold = value; DirtyMesh(); } } get { return threshold; } } [SerializeField] [Range(0.0f, 1.0f)] private float threshold = 0.1f;

		/// <summary>The amount of times a nebula point is randomly sampled, before the brightest sample is used.</summary>
		public int Samples { set { if (samples != value) { samples = value; DirtyMesh(); } } get { return samples; } } [SerializeField] [Range(1, 5)] private int samples = 2;

		/// <summary>This allows you to randomly offset each nebula particle position.</summary>
		public float Jitter { set { if (jitter != value) { jitter = value; DirtyMesh(); } } get { return jitter; } } [SerializeField] [Range(0.0f, 1.0f)] private float jitter;

		/// <summary>The calculation used to find the height offset of a particle in the nebula.</summary>
		public SourceType HeightSource { set { if (heightSource != value) { heightSource = value; DirtyMesh(); } } get { return heightSource; } } [SerializeField] private SourceType heightSource = SourceType.None;

		/// <summary>The calculation used to find the scale modified of each particle in the nebula.</summary>
		public SourceType ScaleSource { set { if (scaleSource != value) { scaleSource = value; DirtyMesh(); } } get { return scaleSource; } } [SerializeField] private SourceType scaleSource = SourceType.None;

		/// <summary>The size of the generated nebula.</summary>
		public Vector3 Size { set { if (size != value) { size = value; DirtyMesh(); } } get { return size; } } [SerializeField] private Vector3 size = new Vector3(1.0f, 1.0f, 1.0f);

		/// <summary>The brightness of the nebula when viewed from the side (good for galaxies).</summary>
		public float HorizontalBrightness { set { horizontalBrightness = value; } get { return horizontalBrightness; } } [SerializeField] private float horizontalBrightness = 0.25f;

		/// <summary>The relationship between the Brightness and HorizontalBrightness relative to the viewing angle.</summary>
		public float HorizontalPower { set { horizontalPower = value; } get { return horizontalPower; } } [SerializeField] private float horizontalPower = 1.0f;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount { set { if (starCount != value) { starCount = value; DirtyMesh(); } } get { return starCount; } } [SerializeField] private int starCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors { get { if (starColors == null) starColors = new Gradient(); return starColors; } } [SerializeField] private Gradient starColors;

		/// <summary>This allows you to control how much the underlying nebula pixel color influences the generated star color.
		/// 0 = StarColors gradient will be used directly.
		/// 1 = Colors will be multiplied together.</summary>
		public float StarTint { set { if (starTint != value) { starTint = value; DirtyMesh(); } } get { return starTint; } } [SerializeField] [Range(0.0f, 1.0f)] private float starTint = 1.0f;

		/// <summary>Should the star color luminosity be boosted?</summary>
		public float StarBoost { set { if (starBoost != value) { starBoost = value; DirtyMesh(); } } get { return starBoost; } } [SerializeField] [Range(0.0f, 5.0f)] private float starBoost;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin { set { if (starRadiusMin != value) { starRadiusMin = value; DirtyMesh(); } } get { return starRadiusMin; } } [SerializeField] private float starRadiusMin = 0.0f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax { set { if (starRadiusMax != value) { starRadiusMax = value; DirtyMesh(); } } get { return starRadiusMax; } } [SerializeField] private float starRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias { set { if (starRadiusBias != value) { starRadiusBias = value; DirtyMesh(); } } get { return starRadiusBias; } } [SerializeField] private float starRadiusBias = 1.0f;

		/// <summary>The minimum animation speed of the pulsing.</summary>
		public float StarPulseSpeedMin { set { if (starPulseSpeedMin != value) { starPulseSpeedMin = value; DirtyMesh(); } } get { return starPulseSpeedMin; } } [Range(0.0f, 1.0f)] [SerializeField] private float starPulseSpeedMin = 0.0f;

		/// <summary>The maximum animation speed of the pulsing.</summary>
		public float StarPulseSpeedMax { set { if (starPulseSpeedMax != value) { starPulseSpeedMax = value; DirtyMesh(); } } get { return starPulseSpeedMax; } } [Range(0.0f, 1.0f)] [SerializeField] private float starPulseSpeedMax = 1.0f;

		// Temp vars used during generation
		private static Texture2D sourceTex2D;
		private static Vector3   halfSize;

		private static int _SGT_Brightness = Shader.PropertyToID("_SGT_Brightness");

		public static SgtStarfieldNebula Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldNebula Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Starfield Nebula", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtStarfieldNebula>();
		}

		protected override void SetBrightness(Camera camera)
		{
			// Change brightness based on viewing angle?
			var dir    = (transform.position - camera.transform.position).normalized;
			var theta  = Mathf.Abs(Vector3.Dot(transform.up, dir));
			var bright = Mathf.Lerp(horizontalBrightness, Brightness, Mathf.Pow(theta, horizontalPower));

			material.SetFloat(_SGT_Brightness, bright);
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(Vector3.zero, size);
		}
#endif

		protected override int BeginQuads()
		{
			CwHelper.BeginSeed(seed);

			if (starColors == null)
			{
				starColors = SgtCommon.CreateGradient(Color.white);
			}

			sourceTex2D = sourceTex as Texture2D;

			if (sourceTex2D != null && samples > 0)
			{
				halfSize = size * 0.5f;

				return starCount;
			}

			return 0;
		}

		protected override void NextQuad(ref SgtStarfieldStar star, int starIndex)
		{
			for (var i = samples - 1; i >= 0; i--)
			{
				var sampleX = Random.Range(0.0f, 1.0f);
				var sampleY = Random.Range(0.0f, 1.0f);
				var pixel   = sourceTex2D.GetPixelBilinear(sampleX, sampleY);
				var gray    = pixel.grayscale;

				if (gray > threshold || i == 0)
				{
					var position = -halfSize + Random.insideUnitSphere * jitter * starRadiusMax;

					position.x += size.x * sampleX;
					position.y += size.y * GetWeight(heightSource, pixel, 0.5f);
					position.z += size.z * sampleY;

					star.Variant     = Random.Range(int.MinValue, int.MaxValue);
					star.Color       = starColors.Evaluate(Random.value) * Color.LerpUnclamped(Color.white, GetBoosted(pixel), starTint);
					star.Radius      = Mathf.Lerp(starRadiusMin, starRadiusMax, Mathf.Pow(Random.value, starRadiusBias)) * GetWeight(scaleSource, pixel, 1.0f);
					star.Angle       = Random.Range(-180.0f, 180.0f);
					star.Position    = position;
					star.PulseSpeed  = Random.Range(starPulseSpeedMin, starPulseSpeedMax);
					star.PulseOffset = Random.value;

					return;
				}
			}
		}

		private Color GetBoosted(Color c)
		{
			if (starBoost > 0.0f)
			{
				float h; float s; float v;
						
				Color.RGBToHSV(c, out h, out s, out v);

				v = 1.0f - Mathf.Pow(1.0f - v, 1.0f + starBoost);

				var n = Color.HSVToRGB(h,s,v);

				return new Color(n.r, n.g, n.b, c.a);
			}

			return c;
		}

		protected override void EndQuads()
		{
			CwHelper.EndSeed();
		}

		private float GetWeight(SourceType source, Color pixel, float defaultWeight)
		{
			switch (source)
			{
				case SourceType.Red: return pixel.r;
				case SourceType.Green: return pixel.g;
				case SourceType.Blue: return pixel.b;
				case SourceType.Alpha: return pixel.a;
				case SourceType.AverageRgb: return (pixel.r + pixel.g + pixel.b) / 3.0f;
				case SourceType.MinRgb: return Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
				case SourceType.MaxRgb: return Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
			}

			return defaultWeight;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Starfield
{
	using UnityEditor;
	using TARGET = SgtStarfieldNebula;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtStarfieldNebula_Editor : SgtStarfield_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			DrawBasic(ref dirtyMesh);

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.SourceTex == null));
				Draw("sourceTex", ref dirtyMesh, "This texture used to color the nebula particles.");
			EndError();
			if (Any(tgts, t => t.SourceTex != null && t.SourceTex.isReadable == false))
			{
				Warning("This texture is non-readable.");
			}
			Draw("threshold", ref dirtyMesh, "This brightness of the sampled SourceTex pixel for a particle to be spawned.");
			Draw("samples", ref dirtyMesh, "The amount of times a nebula point is randomly sampled, before the brightest sample is used.");
			Draw("jitter", ref dirtyMesh, "This allows you to randomly offset each nebula particle position.");
			Draw("heightSource", ref dirtyMesh, "The calculation used to find the height offset of a particle in the nebula.");
			Draw("scaleSource", ref dirtyMesh, "The calculation used to find the scale modified of each particle in the nebula.");
			BeginError(Any(tgts, t => t.Size.x <= 0.0f || t.Size.y <= 0.0f || t.Size.z <= 0.0f));
				Draw("size", ref dirtyMesh, "The size of the generated nebula.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.HorizontalBrightness < 0.0f));
				Draw("horizontalBrightness", "The brightness of the nebula when viewed from the side (good for galaxies).");
			EndError();
			BeginError(Any(tgts, t => t.HorizontalPower < 0.0f));
				Draw("horizontalPower", "The relationship between the Brightness and HorizontalBrightness relative to the viewing angle.");
			EndError();

			Separator();

			Draw("starCount", ref dirtyMesh, "The amount of stars that will be generated in the starfield.");
			Draw("starColors", ref dirtyMesh, "Each star is given a random color from this gradient.");
			Draw("starTint", ref dirtyMesh, "This allows you to control how much the underlying nebula pixel color influences the generated star color.\n\n0 = StarColors gradient will be used directly.\n\n1 = Colors will be multiplied together.");
			Draw("starBoost", ref dirtyMesh, "Should the star color luminosity be boosted?");
			BeginError(Any(tgts, t => t.StarRadiusMin < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				Draw("starRadiusMin", ref dirtyMesh, "The minimum radius of stars in the starfield.");
			EndError();
			BeginError(Any(tgts, t => t.StarRadiusMax < 0.0f || t.StarRadiusMin > t.StarRadiusMax));
				Draw("starRadiusMax", ref dirtyMesh, "The maximum radius of stars in the starfield.");
			EndError();
			Draw("starRadiusBias", ref dirtyMesh, "How likely the size picking will pick smaller stars over larger ones (1 = default/linear).");
			Draw("starPulseSpeedMin", ref dirtyMesh, "The minimum animation speed of the pulsing.\n\nNOTE: Your <b>SourceMaterial</b> must have <b>PULSE</b> enabled for this setting to be used.");
			Draw("starPulseSpeedMax", ref dirtyMesh, "The minimum animation speed of the pulsing.\n\nNOTE: Your <b>SourceMaterial</b> must have <b>PULSE</b> enabled for this setting to be used.");

			SgtCommon.RequireCamera();

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Starfield/Nebula", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtStarfieldNebula.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif