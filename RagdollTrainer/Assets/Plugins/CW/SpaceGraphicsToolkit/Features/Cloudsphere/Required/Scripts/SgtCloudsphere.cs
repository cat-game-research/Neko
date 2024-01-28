using UnityEngine;
using CW.Common;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit.Cloudsphere
{
	/// <summary>This component allows you to render a sphere around a planet with a cloud cubemap.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtCloudsphere")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Cloudsphere")]
	public class SgtCloudsphere : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Cloudsphere</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the cloudsphere.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the cubemap texture applied to the cloudsphere.</summary>
		public Cubemap MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Cubemap mainTex;

		/// <summary>This allows you to set the radius of the cloudsphere in local space.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.5f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the cloudsphere, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		/// <summary>Should the stars fade out if they're intersecting solid geometry?</summary>
		public float Softness { set { softness = value; } get { return softness; } } [SerializeField] [Range(0.0f, 1000.0f)] private float softness;

		/// <summary>This allows you to set the mesh used to render the cloudsphere. This should be a sphere.</summary>
		public Mesh Mesh { set { mesh = value; } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the cloudsphere will render incorrectly.</summary>
		public float MeshRadius { set { meshRadius = value; } get { return meshRadius; } } [SerializeField] private float meshRadius = 1.0f;

		public event System.Action<Material> OnSetProperties;

		private static int _SGT_Cull                = Shader.PropertyToID("_SGT_Cull");
		private static int _SGT_MainTex             = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_DepthTex            = Shader.PropertyToID("_SGT_DepthTex");
		private static int _SGT_LightingTex         = Shader.PropertyToID("_SGT_LightingTex");
		private static int _SGT_NearTex             = Shader.PropertyToID("_SGT_NearTex");
		private static int _SGT_Color               = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness          = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_SoftParticlesFactor = Shader.PropertyToID("_SGT_SoftParticlesFactor");
		private static int _SGT_ScatteringStrength  = Shader.PropertyToID("_SGT_ScatteringStrength");

		[SerializeField]
		private SgtCloudsphereModel model;

		[System.NonSerialized]
		protected Material material;

		[System.NonSerialized]
		private Transform cachedTransform;

		public SgtCloudsphereModel Model
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

		public bool NeedsDepthTex
		{
			get
			{
				if (material != null && material.GetTexture(_SGT_DepthTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public bool NeedsLightingTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_LIGHTING") == true && material.GetTexture(_SGT_LightingTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public bool NeedsNearTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_NEAR") == true && material.GetTexture(_SGT_NearTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static SgtCloudsphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtCloudsphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Cloudsphere", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtCloudsphere>();
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
			var eye = camera.transform.position;

			if (sourceMaterial != null)
			{
				if (material == null)
				{
					material = CwHelper.CreateTempMaterial("Cloudsphere (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				material.SetTexture(_SGT_MainTex, mainTex != null ? mainTex : default(Cubemap)); // TODO: Make this fall back to white?
				material.SetColor(_SGT_Color, color);
				material.SetFloat(_SGT_Brightness, brightness);

				if (softness > 0.0f)
				{
					SgtCommon.EnableKeyword("_SGT_SOFTNESS", material);

					material.SetFloat(_SGT_SoftParticlesFactor, CwHelper.Reciprocal(softness));
				}
				else
				{
					SgtCommon.DisableKeyword("_SGT_SOFTNESS", material);
				}

				/*
				if (Vector3.Magnitude(transform.InverseTransformPoint(camera.transform.position)) < radius)
				{
					material.SetInt(_SGT_Cull, 1); // Front
				}
				else
				{
					material.SetInt(_SGT_Cull, 2); // Back
				}
				*/

				if (material.IsKeywordEnabled("_SGT_LIGHTING") == true)
				{
					// Write lights and shadows
					CwHelper.SetTempMaterial(material);

					var mask   = 1 << gameObject.layer;
					var lights = SgtLight.Find(mask, transform.position);

					SgtShadow.Find(true, mask, lights);
					SgtShadow.FilterOutRing(transform.position);
					SgtShadow.WriteSphere(SgtShadow.MAX_SPHERE_SHADOWS);
					SgtShadow.WriteRing(SgtShadow.MAX_RING_SHADOWS);

					SgtLight.FilterOut(transform.position);
					SgtLight.Write(transform.position, CwHelper.UniformScale(transform.lossyScale) * radius, null, null, SgtLight.MAX_LIGHTS);
				}
			}

			model.transform.localScale = Vector3.one * CwHelper.Divide(radius, meshRadius);

			if (cameraOffset != 0.0f)
			{
				var direction = Vector3.Normalize(eye - cachedTransform.position);

				model.transform.position = cachedTransform.position + direction * cameraOffset;
			}
			else
			{
				model.transform.localPosition = Vector3.zero;
			}
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtCloudsphereModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;

			cachedTransform = GetComponent<Transform>();
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
			CwHelper.Destroy(material);
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
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Cloudsphere
{
	using UnityEditor;
	using TARGET = SgtCloudsphere;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtCloudsphere_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Cloudsphere</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the starfield.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the cubemap texture applied to the cloudsphere.");
			EndError();
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order."); // Updated automatically

			Separator();

			BeginError(Any(tgts, t => t.Radius < 0.0f));
				Draw("radius", "This allows you to set the radius of the cloudsphere in local space.");
			EndError();
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the cloudsphere, giving you fine control over the render order."); // Updated automatically

			Separator();

			Draw("softness", "Should the stars fade out if they're intersecting solid geometry?");

			if (Any(tgts, t => t.Softness > 0.0f))
			{
				CwDepthTextureMode_Editor.RequireDepth();
			}

			Separator();

			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "This allows you to set the mesh used to render the cloudsphere. This should be a sphere.");
			EndError();
			BeginError(Any(tgts, t => t.MeshRadius <= 0.0f));
				Draw("meshRadius", "This allows you to set the radius of the Mesh. If this is incorrectly set then the cloudsphere will render incorrectly.");
			EndError();

			if (Any(tgts, t => t.NeedsDepthTex == true && t.GetComponent<SgtCloudsphereDepthTex>() == null))
			{
				Separator();

				if (Button("Add InnerDepthTex & OuterDepthTex") == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtCloudsphereDepthTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsLightingTex == true && t.GetComponent<SgtCloudsphereLightingTex>() == null))
			{
				Separator();

				if (Button("Add LightingTex") == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtCloudsphereLightingTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsNearTex == true && t.GetComponent<SgtCloudsphereNearTex>() == null))
			{
				Separator();

				if (Button("Add NearTex") == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtCloudsphereNearTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => SetMeshAndMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & Mesh Radius") == true)
				{
					Each(tgts, t => SetMeshAndMeshRadius(t, true));
				}
			}
		}

		private bool SetMeshAndMeshRadius(SgtCloudsphere cloudsphere, bool apply)
		{
			if (cloudsphere.Mesh == null)
			{
				var mesh = CwHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						cloudsphere.Mesh       = mesh;
						cloudsphere.MeshRadius = SgtCommon.GetBoundsRadius(mesh.bounds);
					}

					return true;
				}
			}

			return false;
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Cloudsphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtCloudsphere.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif