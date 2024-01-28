using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Singularity
{
	/// <summary>This component allows you to render a singularity/black hole.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBlackHole")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Black Hole")]
	public class SgtBlackHole : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/BlackHole</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>The mesh applied used to render the black hole. This should be a sphere.</summary>
		public Mesh Mesh { set  { mesh = value; } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>The higher you set this, the smaller the spatial distortion will be.</summary>
		public float Pinch { set  { pinch = value; } get { return pinch; } } [SerializeField] private float pinch = 10.0f;

		/// <summary>The higher you set this, the more space will bend around the black hole.</summary>
		public float Warp { set  { warp = value; } get { return warp; } } [SerializeField] [Range(0.0f, 15.0f)] private float warp = 4.0f;

		/// <summary>This allows you to control the overall size of the hole relative to the pinch.</summary>
		public float HoleSize { set { holeSize = value; } get { return holeSize; } } [SerializeField] float holeSize = 0.5f;

		/// <summary>This allows you to control how sharp/abrupt the transition between space and the event horizon is.</summary>
		public float HoleSharpness { set { holeSharpness = value; } get { return holeSharpness; } } [SerializeField] float holeSharpness = 10.0f;

		/// <summary>This allows you to control the color of the black hole past the event horizon.</summary>
		public Color HoleColor { set { holeColor = value; } get { return holeColor; } } [SerializeField] Color holeColor = Color.black;

		/// <summary>The color of the tint.</summary>
		public Color TintColor { set { tintColor = value; } get { return tintColor; } } [SerializeField] Color tintColor = Color.red;

		/// <summary>This allows you to control how sharp/abrupt the transition between the event horizon and the tinted space is.</summary>
		public float TintSharpness { set { tintSharpness = value; } get { return tintSharpness; } } [SerializeField] float tintSharpness = 4.0f;

		/// <summary>This allows you to fade the edges of the black hole. This is useful if you have multiple black holes near each other.</summary>
		public float FadePower { set { fadePower = value; } get { return fadePower; } } [SerializeField] float fadePower = 10.0f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtBlackHoleModel model;

		[System.NonSerialized]
		private Material material;

		private static int _SGT_WorldPosition = Shader.PropertyToID("_SGT_WorldPosition");
		private static int _SGT_PinchPower    = Shader.PropertyToID("_SGT_PinchPower");
		private static int _SGT_PinchScale    = Shader.PropertyToID("_SGT_PinchScale");
		private static int _SGT_HolePower     = Shader.PropertyToID("_SGT_HolePower");
		private static int _SGT_HoleColor     = Shader.PropertyToID("_SGT_HoleColor");
		private static int _SGT_HoleSize      = Shader.PropertyToID("_SGT_HoleSize");
		private static int _SGT_TintPower     = Shader.PropertyToID("_SGT_TintPower");
		private static int _SGT_TintColor     = Shader.PropertyToID("_SGT_TintColor");
		private static int _SGT_FadePower     = Shader.PropertyToID("_SGT_FadePower");

		public SgtBlackHoleModel Model
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

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static SgtBlackHole Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBlackHole Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Black Hole", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtBlackHole>();
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
					material = CwHelper.CreateTempMaterial("BlackHole (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				material.SetVector(_SGT_WorldPosition, SgtCommon.NewVector4(transform.position, 1.0f));

				material.SetFloat(_SGT_PinchPower, pinch);
				material.SetFloat(_SGT_PinchScale, warp);

				material.SetFloat(_SGT_HolePower, holeSharpness);
				material.SetColor(_SGT_HoleColor, holeColor);
				material.SetFloat(_SGT_HoleSize, holeSize);

				material.SetFloat(_SGT_TintPower, tintSharpness);
				material.SetColor(_SGT_TintColor, tintColor);

				material.SetFloat(_SGT_FadePower, fadePower);
			}
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtBlackHoleModel.Create(this);
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

		protected virtual void LateUpdate()
		{
#if UNITY_EDITOR
			SyncMaterial();
#endif
			if (model != null)
			{
				model.CachedMeshFilter.sharedMesh = mesh;
			}
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(material);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Singularity
{
	using UnityEditor;
	using TARGET = SgtBlackHole;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBlackHole_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/BlackHole</b> shader. You cannot use a normal shader.");
			EndError();
			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "The mesh applied used to render the black hole. This should be a sphere.");
			EndError();

			Separator();

			Draw("pinch", "The higher you set this, the smaller the spatial distortion will be.");
			Draw("warp", "The higher you set this, the more space will bend around the black hole.");

			Separator();

			BeginError(Any(tgts, t => t.HoleSize <= 0.0f));
				Draw("holeSize", "This allows you to control the overall size of the hole relative to the pinch.");
			EndError();
			BeginError(Any(tgts, t => t.HoleSharpness <= 0.0f));
				Draw("holeSharpness", "This allows you to control how sharp/abrupt the transition between space and the event horizon is.");
			EndError();
			Draw("holeColor", "This allows you to control the color of the black hole past the event horizon.");

			Separator();

			BeginError(Any(tgts, t => t.TintSharpness < 0.0f));
				Draw("tintSharpness", "How sharp the tint color gradient is.");
			EndError();
			Draw("tintColor", "The color of the tint.");

			Separator();

			Draw("fadePower", "This allows you to fade the edges of the black hole. This is useful if you have multiple black holes near each other.");
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Black Hole", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtBlackHole.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif