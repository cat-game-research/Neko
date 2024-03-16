using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Backdrop
{
	/// <summary>This component allows you to generate procedurally placed quads on the edge of a sphere.
	/// The quads can then be textured using clouds or stars, and will follow the rendering camera, creating a backdrop.
	/// This backdrop is very quick to render, and provides a good alternative to skyboxes because of the vastly reduced memory requirements.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBackdrop")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Backdrop")]
	public class SgtBackdrop : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Backdrop</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the backdrop.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the texture applied to the starfield. If this texture can contains multiple stars then you can specify their location using the <b>Atlas</b> setting.</summary>
		public Texture MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Texture mainTex;

		/// <summary>If the main texture of this material contains multiple textures in an atlas, then you can specify them here.</summary>
		public SgtAtlas Atlas { get { if (atlas == null) atlas = new SgtAtlas(); return atlas; } } [SerializeField] private SgtAtlas atlas;

		/// <summary>Should the stars fade out instead of shrink when they reach a certain minimum size on screen?</summary>
		public float ClampSizeMin { set { if (clampSizeMin != value) { clampSizeMin = value; DirtyMesh(); } } get { return clampSizeMin; } } [SerializeField] [Range(0.0f, 1000.0f)] private float clampSizeMin;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The radius of the starfield.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMesh(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>Should more stars be placed near the horizon?</summary>
		public float Squash { set { if (squash != value) { squash = value; DirtyMesh(); } } get { return squash; } } [SerializeField] [Range(0.0f, 1.0f)] private float squash;

		/// <summary>The amount of stars that will be generated in the starfield.</summary>
		public int StarCount { set { if (starCount != value) { starCount = value; DirtyMesh(); } } get { return starCount; } } [SerializeField] private int starCount = 1000; public void SetStarCount(float value) { StarCount = (int)value; }

		/// <summary>Each star is given a random color from this gradient.</summary>
		public Gradient StarColors { get { if (starColors == null) starColors = new Gradient(); return starColors; } } [SerializeField] private Gradient starColors;

		/// <summary>The minimum radius of stars in the starfield.</summary>
		public float StarRadiusMin { set { if (starRadiusMin != value) { starRadiusMin = value; DirtyMesh(); } } get { return starRadiusMin; } } [SerializeField] private float starRadiusMin = 0.01f;

		/// <summary>The maximum radius of stars in the starfield.</summary>
		public float StarRadiusMax { set { if (starRadiusMax != value) { starRadiusMax = value; DirtyMesh(); } } get { return starRadiusMax; } } [SerializeField] private float starRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller stars over larger ones (1 = default/linear).</summary>
		public float StarRadiusBias { set { if (starRadiusBias != value) { starRadiusBias = value; DirtyMesh(); } } get { return starRadiusBias; } } [SerializeField] private float starRadiusBias;

		/// <summary>The minimum animation speed of the pulsing.
		/// NOTE: Your <b>SourceMaterial</b> must have <b>PULSE</b> enabled for this setting to be used.</summary>
		public float StarPulseSpeedMin { set { if (starPulseSpeedMin != value) { starPulseSpeedMin = value; DirtyMesh(); } } get { return starPulseSpeedMin; } } [Range(0.0f, 1.0f)] [SerializeField] private float starPulseSpeedMin = 0.0f;

		/// <summary>The maximum animation speed of the pulsing.
		/// NOTE: Your <b>SourceMaterial</b> must have <b>PULSE</b> enabled for this setting to be used.</summary>
		public float StarPulseSpeedMax { set { if (starPulseSpeedMax != value) { starPulseSpeedMax = value; DirtyMesh(); } } get { return starPulseSpeedMax; } } [Range(0.0f, 1.0f)] [SerializeField] private float starPulseSpeedMax = 1.0f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtBackdropModel model;

		[System.NonSerialized]
		private Mesh mesh;

		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private bool dirtyMesh = true;

		private static List<Vector3> positions = new List<Vector3>();
		private static List<Color32> colors32  = new List<Color32>();
		private static List<Vector3> coords1   = new List<Vector3>();
		private static List<Vector3> coords2   = new List<Vector3>();
		private static List<int>     indices   = new List<int>();

		private static int _SGT_MainTex        = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_Color          = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness     = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_Radius         = Shader.PropertyToID("_SGT_Radius");
		private static int _SGT_ClampSizeMin   = Shader.PropertyToID("_SGT_ClampSizeMin");
		private static int _SGT_ClampSizeScale = Shader.PropertyToID("_SGT_ClampSizeScale");

		public SgtBackdropModel Model
		{
			get
			{
				return model;
			}
		}

		public Material Material
		{
			get
			{
				return material;
			}
		}

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static SgtBackdrop Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBackdrop Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var instance = CwHelper.CreateGameObject("Backdrop", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtBackdrop>();

			return instance;
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtBackdropModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;
		}

		protected virtual void OnDisable()
		{
			CwHelper.OnCameraPreRender -= HandleCameraPreRender;

			if (model != null)
			{
				model.CachedMeshRenderer.enabled = false;
			}
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(mesh);
			CwHelper.Destroy(material);
		}

		protected virtual void LateUpdate()
		{
#if UNITY_EDITOR
			SyncMaterial();
#endif
			if (dirtyMesh == true)
			{
				UpdateMesh();
			}

			if (model != null)
			{
				model.CachedMeshFilter.sharedMesh = mesh;
			}
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			dirtyMesh = true;
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			dirtyMesh = true;
		}
#endif

		private void UpdateMesh()
		{
			dirtyMesh = false;

			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Backdrop Mesh (Generated)");
			}
			else
			{
				mesh.Clear();
			}

			var count = BeginQuads();

			BuildMesh(mesh, count);

			EndQuads();
		}

		[ContextMenu("Sync Material")]
		public void SyncMaterial()
		{
			if (sourceMaterial != null)
			{
				if (material != null)
				{
					if (material.shader != sourceMaterial.shader)
					{
						material.shader = sourceMaterial.shader;
					}

					material.CopyPropertiesFromMaterial(sourceMaterial);

					if (OnSetProperties != null)
					{
						OnSetProperties.Invoke(material);
					}
				}
			}
		}

		private void HandleCameraPreRender(Camera camera)
		{
			if (sourceMaterial != null)
			{
				if (material == null)
				{
					material = CwHelper.CreateTempMaterial("Starfield (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				material.SetTexture(_SGT_MainTex, mainTex != null ? mainTex : Texture2D.whiteTexture);
				material.SetColor(_SGT_Color, color);
				material.SetFloat(_SGT_Brightness, brightness);
				material.SetFloat(_SGT_Radius, radius);

				if (clampSizeMin > 0.0f)
				{
					SgtCommon.EnableKeyword("_SGT_CLAMP_SIZE_MIN", material);

					material.SetFloat(_SGT_ClampSizeMin, clampSizeMin);

					if (camera.orthographic == true)
					{
						material.SetFloat(_SGT_ClampSizeScale, camera.orthographicSize * 0.0025f);
					}
					else
					{
						material.SetFloat(_SGT_ClampSizeScale, Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 2.0f);
					}
				}
				else
				{
					SgtCommon.DisableKeyword("_SGT_CLAMP_SIZE_MIN", material);
				}
			}
		}

		protected virtual int BeginQuads()
		{
			CwHelper.BeginSeed(seed);

			if (starColors == null)
			{
				starColors = SgtCommon.CreateGradient(Color.white);
			}
		
			return starCount;
		}

		protected virtual void NextQuad(ref SgtBackdropQuad star, int starIndex)
		{
			var position = Random.insideUnitSphere;

			position.y *= 1.0f - squash;

			star.Variant     = Random.Range(int.MinValue, int.MaxValue);
			star.Color       = StarColors.Evaluate(Random.value);
			star.Radius      = Mathf.Lerp(starRadiusMin, starRadiusMax, CwHelper.Sharpness(Random.value, starRadiusBias));
			star.Angle       = Random.Range(-180.0f, 180.0f);
			star.Position    = position.normalized * radius;
			star.PulseSpeed  = Random.Range(starPulseSpeedMin, starPulseSpeedMax);
			star.PulseOffset = Random.value;
		}

		protected virtual void EndQuads()
		{
			CwHelper.EndSeed();
		}

		protected virtual void BuildMesh(Mesh mesh, int count)
		{
			var tempCoords = Atlas.GetCoords(); // NOTE: Property
			var minMaxSet  = false;
			var min        = default(Vector3);
			var max        = default(Vector3);

			CwHelper.Resize(positions, count * 4);
			CwHelper.Resize(colors32, count * 4);
			CwHelper.Resize(coords1, count * 4);
			CwHelper.Resize(coords2, count * 4);
			CwHelper.Resize(indices, count * 6);

			for (var i = 0; i < count; i++)
			{
				NextQuad(ref SgtBackdropQuad.Temp, i);

				var offV     = i * 4;
				var offI     = i * 6;
				var radius   = SgtBackdropQuad.Temp.Radius;
				var uv       = tempCoords[CwHelper.Mod(SgtBackdropQuad.Temp.Variant, tempCoords.Count)];
				var rotation = Quaternion.FromToRotation(Vector3.back, SgtBackdropQuad.Temp.Position) * Quaternion.Euler(0.0f, 0.0f, SgtBackdropQuad.Temp.Angle);
				var up       = rotation * Vector3.up    * radius;
				var right    = rotation * Vector3.right * radius;
				var pulse    = Mathf.Clamp01(SgtBackdropQuad.Temp.PulseSpeed) * 0.99f + Mathf.Clamp01(SgtBackdropQuad.Temp.PulseOffset) * 4096.0f;

				SgtCommon.ExpandBounds(ref minMaxSet, ref min, ref max, SgtBackdropQuad.Temp.Position, radius);

				positions[offV + 0] = SgtBackdropQuad.Temp.Position - up - right;
				positions[offV + 1] = SgtBackdropQuad.Temp.Position - up + right;
				positions[offV + 2] = SgtBackdropQuad.Temp.Position + up - right;
				positions[offV + 3] = SgtBackdropQuad.Temp.Position + up + right;

				colors32[offV + 0] =
				colors32[offV + 1] =
				colors32[offV + 2] =
				colors32[offV + 3] = SgtBackdropQuad.Temp.Color;

				coords1[offV + 0] = new Vector3(uv.x, uv.y, pulse);
				coords1[offV + 1] = new Vector3(uv.z, uv.y, pulse);
				coords1[offV + 2] = new Vector3(uv.x, uv.w, pulse);
				coords1[offV + 3] = new Vector3(uv.z, uv.w, pulse);

				coords2[offV + 0] =
				coords2[offV + 1] =
				coords2[offV + 2] =
				coords2[offV + 3] = SgtBackdropQuad.Temp.Position;

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.bounds      = SgtCommon.NewBoundsFromMinMax(min, max);

			mesh.Clear();
			mesh.SetVertices(positions);
			mesh.SetColors(colors32);
			mesh.SetUVs(0, coords1);
			mesh.SetUVs(1, coords2);
			mesh.SetTriangles(indices, 0, false);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Backdrop
{
	using UnityEditor;
	using TARGET = SgtBackdrop;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBackdrop_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Backdrop</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the backdrop.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the texture applied to the backdrop. If this texture can contains multiple stars then you can specify their location using the <b>Atlas</b> setting.");
			EndError();
			Draw("atlas", ref dirtyMesh, "If the main texture of this material contains multiple textures in an atlas, then you can specify them here.");

			Separator();

			Draw("clampSizeMin", "Should the stars fade out instead of shrink when they reach a certain minimum size on screen?");

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the starfield.");
			EndError();
			Draw("squash", ref dirtyMesh, "Should more stars be placed near the horizon?");

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

			if (dirtyMesh     == true) Each(tgts, t => t.DirtyMesh    (), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Backdrop", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtBackdrop.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif