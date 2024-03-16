using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This component allows you to render a starfield that repeats forever.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtStarfieldInfinite")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Starfield Infinite")]
	public class SgtStarfieldInfinite : SgtStarfield
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The size of the starfield in local space.</summary>
		public Vector3 Size { set { if (size != value) { size = value; DirtyMesh(); } } get { return size; } } [SerializeField] private Vector3 size = Vector3.one;

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

		private static int _SGT_WrapSize   = Shader.PropertyToID("_SGT_WrapSize");
		private static int _SGT_WrapScale  = Shader.PropertyToID("_SGT_WrapScale");
		private static int _SGT_ScaleRecip = Shader.PropertyToID("_SGT_ScaleRecip");
		private static int _SGT_Scale      = Shader.PropertyToID("_SGT_Scale");

		public static SgtStarfieldInfinite Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldInfinite Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Starfield Infinite", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtStarfieldInfinite>();
		}

		protected override void OnEnable()
		{
			SgtFloatingCamera.OnSnap += FloatingCameraSnap;

			base.OnEnable();
		}

		protected override void OnDisable()
		{
			SgtFloatingCamera.OnSnap -= FloatingCameraSnap;

			base.OnDisable();
		}

		private void FloatingCameraSnap(SgtFloatingCamera floatingCamera, Vector3 delta)
		{
			var position = transform.position + delta;
			var extent   = size * CwHelper.UniformScale(transform.lossyScale);

			position.x = position.x % extent.x;
			position.y = position.y % extent.y;
			position.z = position.z % extent.z;

			transform.position = position;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireCube(Vector3.zero, size);
		}
#endif

		protected override void HandleCameraPreRender(Camera camera)
		{
			if (SourceMaterial != null)
			{
				base.HandleCameraPreRender(camera);

				material.SetVector(_SGT_WrapSize, size);
				material.SetVector(_SGT_WrapScale, new Vector3(CwHelper.Reciprocal(size.x), CwHelper.Reciprocal(size.y), CwHelper.Reciprocal(size.z)));
				material.SetFloat(_SGT_ScaleRecip, CwHelper.Reciprocal(material.GetFloat(_SGT_Scale)));
			}
		}

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
			var x = Random.Range(size.x * -0.5f, size.x * 0.5f);
			var y = Random.Range(size.y * -0.5f, size.y * 0.5f);
			var z = Random.Range(size.z * -0.5f, size.z * 0.5f);

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = starColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(starRadiusMin, starRadiusMax, Mathf.Pow(Random.value, starRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = new Vector3(x, y, z);
			star.PulseSpeed  = Random.Range(starPulseSpeedMin, starPulseSpeedMax);
			star.PulseOffset = Random.value;
		}

		protected override void BuildMesh(int count)
		{
			base.BuildMesh(count);

			mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 1.0e10f);
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
	using TARGET = SgtStarfieldInfinite;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtStarfieldInfinite_Editor : SgtStarfield_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			DrawBasic(ref dirtyMesh);

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.Size.x <= 0.0f || t.Size.y <= 0.0f || t.Size.z <= 0.0f));
				Draw("size", ref dirtyMesh, "The radius of the starfield.");
			EndError();

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

			if (Any(tgts, t => t.GetComponentInParent<SgtFloatingObject>()))
			{
				Warning("SgtStarfieldInfinite automatically snaps with the Universe feature, using SgtFloatingObject may cause issues with this GameObject.");
			}

			if (SgtFloatingCamera.Instances.Count > 0 && Any(tgts, t => t.transform.rotation != Quaternion.identity))
			{
				Warning("This transform is rotated, this may cause issues with the Universe feature.");
			}

			if (SgtFloatingCamera.Instances.Count > 0 && Any(tgts, t => IsUniform(t.transform) == false))
			{
				Warning("This transform is non-uniformly scaled, this may cause issues with the Universe feature.");
			}
		}

		private bool IsUniform(Transform t)
		{
			var scale = t.localScale;

			if (scale.x != scale.y || scale.x != scale.z)
			{
				return false;
			}

			if (t.parent != null)
			{
				return IsUniform(t.parent);
			}

			return true;
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Starfield/Infinite", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtStarfieldInfinite.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif