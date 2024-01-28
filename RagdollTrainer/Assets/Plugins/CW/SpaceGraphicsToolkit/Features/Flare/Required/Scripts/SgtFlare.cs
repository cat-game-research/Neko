using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Flare
{
	/// <summary>This component allows you to generate a high resolution mesh flare.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFlare")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Flare")]
	public class SgtFlare : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Flare</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the mesh used to render the flare.</summary>
		public Mesh Mesh { set { mesh = value; } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>Should the flare automatically snap to cameras.</summary>
		public bool FollowCameras { set { followCameras = value; } get { return followCameras; } } [SerializeField] private bool followCameras;

		/// <summary>The distance from the camera this flare will be placed in world space.</summary>
		public float FollowDistance { set { followDistance = value; } get { return followDistance; } } [SerializeField] private float followDistance = 100.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the flare, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtFlareModel model;

		[System.NonSerialized]
		private Material material;

		private static int _SGT_MainTex = Shader.PropertyToID("_SGT_MainTex");

		public SgtFlareModel Model
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

		public static SgtFlare Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtFlare Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Flare", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtFlare>();
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
					material = CwHelper.CreateTempMaterial("Flare (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				if (cameraOffset != 0.0f)
				{
					var eye       = camera.transform.position;
					var direction = Vector3.Normalize(eye - transform.position);

					model.transform.position = transform.position + direction * cameraOffset;
				}
				else
				{
					model.transform.localPosition = Vector3.zero;
				}
			}
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtFlareModel.Create(this);
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
		
		public bool NeedsMainTex
		{
			get
			{
				if (material != null && material.GetTexture(_SGT_MainTex) == null)
				{
					return true;
				}

				return false;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Flare
{
	using UnityEditor;
	using TARGET = SgtFlare;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFlare_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Flare</b> shader. You cannot use a normal shader.");
			EndError();
			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "This allows you to set the mesh used to render the flare.");
			EndError();
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the flare, giving you fine control over the render order."); // Updated automatically
			Draw("followCameras", "Should the flare automatically snap to cameras."); // Automatically updated

			if (Any(tgts, t => t.FollowCameras == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.FollowDistance <= 0.0f));
						Draw("followDistance", "The distance from the camera this flare will be placed in world space."); // Automatically updated
					EndError();
				EndIndent();
			}

			if (Any(tgts, t => t.Mesh == null && t.GetComponent<SgtFlareMesh>() == null))
			{
				Separator();

				if (Button("Add Mesh") == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtFlareMesh>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsMainTex == true && t.GetComponent<SgtFlareMainTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a MainTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtFlareMainTex>(t.gameObject));
				}
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Flare", false, 10)]
		public static void CreateItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtFlare.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif