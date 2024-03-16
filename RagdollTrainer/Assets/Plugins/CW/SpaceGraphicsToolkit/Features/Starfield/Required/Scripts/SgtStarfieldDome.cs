using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This component allows you to render a starfield with a dome like distribution.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtStarfieldDome")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Starfield Dome")]
	public class SgtStarfieldDome : SgtStarfield
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The radius of the starfield.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMesh(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>This allows you to control if the stars should cluster near the horizon, or near the azimuth.</summary>
		public float Bias { set { if (bias != value) { bias = value; DirtyMesh(); } } get { return bias; } } [SerializeField] private float bias = 1.0f;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount { set { if (starCount != value) { starCount = value; DirtyMesh(); } } get { return starCount; } } [SerializeField] private int starCount = 1000;

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors { get { if (starColors == null) starColors = new Gradient(); return starColors; } } [SerializeField] private Gradient starColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin { set { if (starRadiusMin != value) { starRadiusMin = value; DirtyMesh(); } } get { return starRadiusMin; } } [SerializeField] private float starRadiusMin = 0.01f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax { set { if (starRadiusMax != value) { starRadiusMax = value; DirtyMesh(); } } get { return starRadiusMax; } } [SerializeField] private float starRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias { set { if (starRadiusBias != value) { starRadiusBias = value; DirtyMesh(); } } get { return starRadiusBias; } } [SerializeField] private float starRadiusBias = 1.0f;

		/// <summary>The minimum animation speed of the pulsing.</summary>
		public float StarPulseSpeedMin { set { if (starPulseSpeedMin != value) { starPulseSpeedMin = value; DirtyMesh(); } } get { return starPulseSpeedMin; } } [Range(0.0f, 1.0f)] [SerializeField] private float starPulseSpeedMin = 0.0f;

		/// <summary>The maximum animation speed of the pulsing.</summary>
		public float StarPulseSpeedMax { set { if (starPulseSpeedMax != value) { starPulseSpeedMax = value; DirtyMesh(); } } get { return starPulseSpeedMax; } } [Range(0.0f, 1.0f)] [SerializeField] private float starPulseSpeedMax = 1.0f;

		public static SgtStarfieldDome Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldDome Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Starfield Dome", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtStarfieldDome>();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, radius);
		}
#endif

		protected override int BeginQuads()
		{
			CwHelper.BeginSeed(seed);

			if (starColors == null)
			{
				starColors = SgtCommon.CreateGradient(Color.white);
			}

			return starCount;
		}

		protected override void NextQuad(ref SgtStarfieldStar star, int starIndex)
		{
			var sphere = Random.insideUnitSphere;

			sphere.y = Mathf.Abs(sphere.y) * bias;

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = starColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(starRadiusMin, starRadiusMax, CwHelper.Sharpness(Random.value, starRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = sphere.normalized * radius;
			star.PulseSpeed  = Random.Range(starPulseSpeedMin, starPulseSpeedMax);
			star.PulseOffset = Random.value;
		}

		protected override void EndQuads()
		{
			CwHelper.EndSeed();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Starfield
{
	using UnityEditor;
	using TARGET = SgtStarfieldDome;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtStarfieldDome_Editor : SgtStarfield_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			DrawBasic(ref dirtyMesh);

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the starfield.");
			EndError();
			Draw("bias", ref dirtyMesh, "This allows you to control if the stars should cluster near the horizon, or near the azimuth.");

			Separator();

			BeginError(Any(tgts, t => t.StarCount < 0));
				Draw("starCount", ref dirtyMesh, "The amount of stars that will be generated in the starfield.");
			EndError();
			Draw("starColors", ref dirtyMesh, "Each star is given a random color from this gradient.");
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

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Starfield/Dome", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtStarfieldDome.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif