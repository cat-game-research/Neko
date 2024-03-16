using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Belt
{
	/// <summary>This component allows you to generate an asteroid belt with a simple exponential distribution.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBeltSimple")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Belt Simple")]
	public class SgtBeltSimple : SgtBelt
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The thickness of the belt in local coordinates.</summary>
		public float Thickness { set { if (thickness != value) { thickness = value; DirtyMesh(); } } get { return thickness; } } [SerializeField] private float thickness;

		/// <summary>The higher this value, the less large asteroids will be generated.</summary>
		public float ThicknessBias { set { if (thicknessBias != value) { thicknessBias = value; DirtyMesh(); } } get { return thicknessBias; } } [SerializeField] private float thicknessBias = 1.0f;

		/// <summary>The radius of the inner edge of the belt in local coordinates.</summary>
		public float InnerRadius { set { if (innerRadius != value) { innerRadius = value; DirtyMesh(); } } get { return innerRadius; } } [SerializeField] private float innerRadius = 1.0f;

		/// <summary>The speed of asteroids orbiting on the inner edge of the belt in radians.</summary>
		public float InnerSpeed { set { if (innerSpeed != value) { innerSpeed = value; DirtyMesh(); } } get { return innerSpeed; } } [SerializeField] private float innerSpeed = 0.1f;

		/// <summary>The radius of the outer edge of the belt in local coordinates.</summary>
		public float OuterRadius { set { if (outerRadius != value) { outerRadius = value; DirtyMesh(); } } get { return outerRadius; } } [SerializeField] private float outerRadius = 2.0f;

		/// <summary>The speed of asteroids orbiting on the outer edge of the belt in radians.</summary>
		public float OuterSpeed { set { if (outerSpeed != value) { outerSpeed = value; DirtyMesh(); } } get { return outerSpeed; } } [SerializeField] private float outerSpeed = 0.05f;

		/// <summary>The higher this value, the more likely asteroids will spawn on the inner edge of the ring.</summary>
		public float RadiusBias { set { if (radiusBias != value) { radiusBias = value; DirtyMesh(); } } get { return radiusBias; } } [SerializeField] private float radiusBias = 0.25f;

		/// <summary>How much random speed can be added to each asteroid.</summary>
		public float SpeedSpread { set { if (speedSpread != value) { speedSpread = value; DirtyMesh(); } } get { return speedSpread; } } [SerializeField] private float speedSpread;

		/// <summary>The amount of asteroids generated in the belt.</summary>
		public int AsteroidCount { set { if (asteroidCount != value) { asteroidCount = value; DirtyMesh(); } } get { return asteroidCount; } } [SerializeField] private int asteroidCount = 1000;

		/// <summary>Each asteroid is given a random color from this gradient.</summary>
		public Gradient AsteroidColors { get { if (asteroidColors == null) asteroidColors = new Gradient(); return asteroidColors; } } [SerializeField] private Gradient asteroidColors;

		/// <summary>The maximum amount of angular velocity each asteroid has.</summary>
		public float AsteroidSpin { set { if (asteroidSpin != value) { asteroidSpin = value; DirtyMesh(); } } get { return asteroidSpin; } } [SerializeField] private float asteroidSpin = 1.0f;

		/// <summary>The minimum asteroid radius in local coordinates.</summary>
		public float AsteroidRadiusMin { set { if (asteroidRadiusMin != value) { asteroidRadiusMin = value; DirtyMesh(); } } get { return asteroidRadiusMin; } } [SerializeField] private float asteroidRadiusMin = 0.025f;

		/// <summary>The maximum asteroid radius in local coordinates.</summary>
		public float AsteroidRadiusMax { set { if (asteroidRadiusMax != value) { asteroidRadiusMax = value; DirtyMesh(); } } get { return asteroidRadiusMax; } } [SerializeField] private float asteroidRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller asteroids over larger ones (1 = default/linear).</summary>
		public float AsteroidRadiusBias { set { if (asteroidRadiusBias != value) { asteroidRadiusBias = value; DirtyMesh(); } } get { return asteroidRadiusBias; } } [SerializeField] private float asteroidRadiusBias = 0.0f;

		public static SgtBeltSimple Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBeltSimple Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Belt Simple", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtBeltSimple>();
		}

		protected override float GetOuterRadius()
		{
			return outerRadius;
		}

		protected override int BeginQuads()
		{
			CwHelper.BeginSeed(seed);

			if (asteroidColors == null)
			{
				asteroidColors = SgtCommon.CreateGradient(Color.white);
			}

			return asteroidCount;
		}

		protected override void NextQuad(ref SgtBeltAsteroid asteroid, int asteroidIndex)
		{
			var distance01 = CwHelper.Sharpness(Random.value * Random.value, radiusBias);

			asteroid.Variant       = Random.Range(int.MinValue, int.MaxValue);
			asteroid.Color         = asteroidColors.Evaluate(Random.value);
			asteroid.Radius        = Mathf.Lerp(asteroidRadiusMin, asteroidRadiusMax, CwHelper.Sharpness(Random.value, asteroidRadiusBias));
			asteroid.Height        = Mathf.Pow(Random.value, thicknessBias) * thickness * (Random.value < 0.5f ? -0.5f : 0.5f);
			asteroid.Angle         = Random.Range(0.0f, Mathf.PI * 2.0f);
			asteroid.Spin          = Random.Range(-asteroidSpin, asteroidSpin);
			asteroid.OrbitAngle    = Random.Range(0.0f, Mathf.PI * 2.0f);
			asteroid.OrbitSpeed    = Mathf.Lerp(innerSpeed, outerSpeed, distance01);
			asteroid.OrbitDistance = Mathf.Lerp(innerRadius, outerRadius, distance01);

			asteroid.OrbitSpeed += Random.Range(-speedSpread, speedSpread) * asteroid.OrbitSpeed;
		}

		protected override void EndQuads()
		{
			CwHelper.EndSeed();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Belt
{
	using UnityEditor;
	using TARGET = SgtBeltSimple;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBeltSimple_Editor : SgtBelt_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			DrawBasic(ref dirtyMesh);

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			Draw("thickness", ref dirtyMesh, "The thickness of the belt in local coordinates.");
			BeginError(Any(tgts, t => t.ThicknessBias < 1.0f));
				Draw("thicknessBias", ref dirtyMesh, "The higher this value, the less large asteroids will be generated.");
			EndError();
			BeginError(Any(tgts, t => t.InnerRadius < 0.0f || t.InnerRadius > t.OuterRadius));
				Draw("innerRadius", ref dirtyMesh, "The radius of the inner edge of the belt in local coordinates.");
			EndError();
			Draw("innerSpeed", ref dirtyMesh, "The speed of asteroids orbiting on the inner edge of the belt in radians.");
			BeginError(Any(tgts, t => t.OuterRadius < 0.0f || t.InnerRadius > t.OuterRadius));
				Draw("outerRadius", ref dirtyMesh, "The radius of the outer edge of the belt in local coordinates.");
			EndError();
			Draw("outerSpeed", ref dirtyMesh, "The speed of asteroids orbiting on the outer edge of the belt in radians.");

			Separator();

			Draw("radiusBias", ref dirtyMesh, "The higher this value, the more likely asteroids will spawn on the inner edge of the ring.");
			Draw("speedSpread", ref dirtyMesh, "How much random speed can be added to each asteroid.");

			Separator();

			BeginError(Any(tgts, t => t.AsteroidCount < 0));
				Draw("asteroidCount", ref dirtyMesh, "The amount of asteroids generated in the belt.");
			EndError();
			Draw("asteroidColors", ref dirtyMesh, "Each asteroid is given a random color from this gradient.");
			Draw("asteroidSpin", ref dirtyMesh, "The maximum amount of angular velocity each asteroid has.");
			BeginError(Any(tgts, t => t.AsteroidRadiusMin < 0.0f || t.AsteroidRadiusMin > t.AsteroidRadiusMax));
				Draw("asteroidRadiusMin", ref dirtyMesh, "The minimum asteroid radius in local coordinates.");
			EndError();
			BeginError(Any(tgts, t => t.AsteroidRadiusMax < 0.0f || t.AsteroidRadiusMin > t.AsteroidRadiusMax));
				Draw("asteroidRadiusMax", ref dirtyMesh, "The maximum asteroid radius in local coordinates.");
			EndError();
			Draw("asteroidRadiusBias", ref dirtyMesh, "How likely the size picking will pick smaller asteroids over larger ones (0 = default/linear).");

			SgtCommon.RequireCamera();

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Belt Simple", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtBeltSimple.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif