using UnityEngine;
using CW.Common;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit.Jovian
{
	/// <summary>This component allows you to render volumetric jovian (gas giant) planets.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtJovian")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Jovian")]
	public class SgtJovian : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Jovian</b> shader. You cannot use a normal shader.
		/// NOTE: If you modify the settings of this material in a build, you must manually call the <b>SyncMaterial</b> method.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the jovian.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the texture applied to the jovian.</summary>
		public Texture MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Texture mainTex;

		/// <summary>This allows you to set the flow texture applied to the jovian.
		/// NOTE: This is only used if the specified <b>SourceMaterial</b> enables <b>FLOW</b>.</summary>
		public Texture FlowTex { set { flowTex = value; } get { return flowTex; } } [SerializeField] private Texture flowTex;

		/// <summary>This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		/// <summary>This setting allows you to increase or decrease how much this atmosphere occludes flares using the <b>SgtOcclusionScaler</b> component.</summary>
		public float OcclusionPower { set { occlusionPower = value; } get { return occlusionPower; } } [SerializeField] private float occlusionPower = 1.0f;

		/// <summary>This allows you to control how thick the atmosphere is when the camera is inside its radius.</summary>
		public float Sky { set { if (sky != value) { sky = value; } } get { return sky; } } [SerializeField] private float sky = 1.0f;

		/// <summary>This allows you to set the mesh used to render the jovian. This should be a sphere.</summary>
		public Mesh Mesh { set { if (mesh != value) { mesh = value; } } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>This allows you to set the radius of the Mesh. If this is incorrectly set then the jovian will render incorrectly.</summary>
		public float MeshRadius { set { if (meshRadius != value) { meshRadius = value; } } get { return meshRadius; } } [SerializeField] private float meshRadius = 1.0f;

		/// <summary>This allows you to set the radius of the jovian in local space.</summary>
		public float Radius { set { if (radius != value) { radius = value; } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>This allows you to set the radius of the jovian in local space when using distance calculations. Distance calculations normally tell you how far from the surface of a planet you are, but since a jovian isn't solid, you may want to customize this based on your project.</summary>
		public float DistanceRadius { set { if (distanceRadius != value) { distanceRadius = value; } } get { return distanceRadius; } } [SerializeField] private float distanceRadius = 0.9f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtJovianModel model;

		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		private static int _SGT_MainTex       = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_Color         = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness    = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_FlowTex       = Shader.PropertyToID("_SGT_FlowTex");
		private static int _SGT_WorldToLocal  = Shader.PropertyToID("_SGT_WorldToLocal");
		private static int _SGT_LocalToWorld  = Shader.PropertyToID("_SGT_LocalToWorld");
		private static int _SGT_Sky           = Shader.PropertyToID("_SGT_Sky");
		private static int _SGT_DepthTex      = Shader.PropertyToID("_SGT_DepthTex");
		private static int _SGT_LightingTex   = Shader.PropertyToID("_SGT_LightingTex");
		private static int _SGT_ScatteringTex = Shader.PropertyToID("_SGT_ScatteringTex");

		public SgtJovianModel Model
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

		public Transform CachedTransform
		{
			get
			{
				CacheTransform(); return cachedTransform;
			}
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static SgtJovian Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtJovian Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Jovian", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtJovian>();
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
					material = CwHelper.CreateTempMaterial("Jovian (Generated)", sourceMaterial);

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

				if (material.IsKeywordEnabled("_SGT_FLOW") == true)
				{
					material.SetTexture(_SGT_FlowTex, flowTex != null ? flowTex : Texture2D.whiteTexture);
				}

				var eye = camera.transform.position;

				// Write matrices
				var scale        = radius;
				var localToWorld = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(scale, scale, scale)); // Double mesh radius so the max thickness caps at 1.0

				material.SetMatrix(_SGT_WorldToLocal, localToWorld.inverse);
				material.SetMatrix(_SGT_LocalToWorld, localToWorld);

				if (material.IsKeywordEnabled("_SGT_LIGHTING") == true)
				{
					// Write lights and shadows
					CwHelper.SetTempMaterial(material);

					var mask   = 1 << gameObject.layer;
					var lights = SgtLight.Find(mask, transform.position);

					SgtShadow.Find(true, mask, lights);
					SgtShadow.FilterOutSphere(transform.position);
					SgtShadow.WriteSphere(SgtShadow.MAX_SPHERE_SHADOWS);
					SgtShadow.WriteRing(SgtShadow.MAX_RING_SHADOWS);

					SgtLight.FilterOut(transform.position);
					SgtLight.Write(transform.position, CwHelper.UniformScale(transform.lossyScale) * radius, transform, null, SgtLight.MAX_LIGHTS);
				}

				var depthTex = GetDepthTex();

				if (depthTex != null)
				{
					material.SetFloat(_SGT_Sky, GetSky(eye, depthTex));
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
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender     += HandleCameraPreRender;
			SgtCommon.OnCalculateDistance  += HandleCalculateDistance;
			SgtCommon.OnCalculateOcclusion += HandleCalculateOcclusion;

			if (model == null)
			{
				model = SgtJovianModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;

			CacheTransform();
		}

		protected virtual void OnDisable()
		{
			CwHelper.OnCameraPreRender     -= HandleCameraPreRender;
			SgtCommon.OnCalculateDistance  -= HandleCalculateDistance;
			SgtCommon.OnCalculateOcclusion -= HandleCalculateOcclusion;

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

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (isActiveAndEnabled == true)
			{
				var r0 = transform.lossyScale;

				SgtCommon.DrawSphere(transform.position, transform.right * r0.x, transform.up * r0.y, transform.forward * r0.z);
			}
		}
#endif

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

		public bool NeedsFlowTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_FLOW") == true && material.GetTexture(_SGT_FlowTex) == null)
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

		public bool NeedsScatteringTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_SCATTERING") == true && material.GetTexture(_SGT_ScatteringTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public Texture2D GetDepthTex()
		{
			if (material != null)
			{
				return material.GetTexture(_SGT_DepthTex) as Texture2D;
			}

			return null;
		}

		private float GetSky(Vector3 eye, Texture2D depthTex)
		{
			if (depthTex.isReadable == false)
			{
				return 0.0f;
			}

			var localCameraPosition = transform.InverseTransformPoint(eye);
			var localDistance       = localCameraPosition.magnitude;
			var scaleDistance       = CwHelper.Divide(localDistance, radius);

			return sky * depthTex.GetPixelBilinear(1.0f - scaleDistance, 0.0f).a;
		}

		private bool GetPoint(Vector3 ray, Vector3 dir, ref Vector3 point)
		{
			var a = Vector3.Dot(ray, dir);
			var b = Vector3.Dot(ray, ray) - 1.0f;

			if (b <= 0.0f) { point = ray; return true; } // Inside?

			var c = a * a - b;

			if (c < 0.0f) { return false; } // Miss?

			var d = -a - Mathf.Sqrt(c);

			if (d < 0.0f) { return false; } // Behind?

			point = ray + dir * d; return true;
		}

		private bool GetLength(Vector3 ray, Vector3 dir, float len, ref float length)
		{
			var a = default(Vector3);
			var b = default(Vector3);

			if (GetPoint(ray, dir, ref a) == true && GetPoint(ray + dir * len, -dir, ref b) == true)
			{
				length = Vector3.Distance(a, b); return true;
			}

			return false;
		}

		private void HandleCalculateOcclusion(int layers, Vector4 worldEye, Vector4 worldTgt, ref float occlusion)
		{
			if (SgtOcclusion.IsValid(occlusion, layers, gameObject) == true && radius > 0.0f)
			{
				var depthTex = GetDepthTex();

				if (depthTex != null && depthTex.isReadable == true)
				{
					SgtOcclusion.TryScaleBackDistantPositions(ref worldEye, ref worldTgt, cachedTransform.position, radius);

					var eye    = transform.InverseTransformPoint(worldEye) / radius;
					var tgt    = transform.InverseTransformPoint(worldTgt) / radius;
					var dir    = Vector3.Normalize(tgt - eye);
					var len    = Vector3.Magnitude(tgt - eye);
					var length = default(float);

					if (GetLength(eye, dir, len, ref length) == true)
					{
						var depth = depthTex.GetPixelBilinear(length, length).a;

						depth = 1.0f - Mathf.Pow(1.0f - depth, occlusionPower);

						depth = Mathf.Clamp01(depth + (1.0f - depth) * GetSky(eye, depthTex));

						occlusion += (1.0f - occlusion) * depth;
					}
				}
			}
		}

		private void HandleCalculateDistance(Vector3 worldPosition, ref float distance)
		{
			var localPosition = transform.InverseTransformPoint(worldPosition);

			localPosition = localPosition.normalized * distanceRadius;

			var surfacePosition = transform.TransformPoint(localPosition);
			var thisDistance    = Vector3.Distance(worldPosition, surfacePosition);

			if (thisDistance < distance)
			{
				distance = thisDistance;
			}
		}

		private void CacheTransform()
		{
			if (cachedTransformSet == false)
			{
				cachedTransform    = GetComponent<Transform>();
				cachedTransformSet = true;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Jovian
{
	using UnityEditor;
	using TARGET = SgtJovian;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtJovian_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Jovian</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the jovian.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the texture applied to the jovian.");
			EndError();
			BeginError(Any(tgts, t => t.NeedsFlowTex == true));
				Draw("flowTex", "This allows you to set the flow texture applied to the jovian.\n\nNOTE: This is only used if the specified <b>SourceMaterial</b> enables <b>FLOW</b>.");
			EndError();
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order."); // Updated automatically
			Draw("occlusionPower", "This setting allows you to increase or decrease how much this atmosphere occludes flares using the <b>SgtOcclusionScaler</b> component."); // Updated automatically

			Separator();

			BeginError(Any(tgts, t => t.Sky < 0.0f));
				Draw("sky", "This allows you to control how thick the atmosphere is when the camera is inside its radius"); // Updated when rendering
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "This allows you to set the mesh used to render the jovian. This should be a sphere.");
			EndError();
			BeginError(Any(tgts, t => t.MeshRadius <= 0.0f));
				Draw("meshRadius", "This allows you to set the radius of the Mesh. If this is incorrectly set then the jovian will render incorrectly.");
			EndError();
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", "This allows you to set the radius of the jovian in local space.");
			EndError();
			Draw("distanceRadius", "This allows you to set the radius of the jovian in local space when using distance calculations. Distance calculations normally tell you how far from the surface of a planet you are, but since a jovian isn't solid, you may want to customize this based on your project.");

			if (Any(tgts, t => t.NeedsDepthTex == true && t.GetComponent<SgtJovianDepthTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a DepthTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtJovianDepthTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsLightingTex == true && t.GetComponent<SgtJovianLightingTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a LightingTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtJovianLightingTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsScatteringTex == true && t.GetComponent<SgtJovianScatteringTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a ScatteringTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtJovianScatteringTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.GetDepthTex() != null && t.GetDepthTex().isReadable == false))
			{
				Error("SourceMaterial has a DepthTex, but this texture is non-readable.");
			}

			if (Any(tgts, t => SetMeshAndMeshRadius(t, false)))
			{
				Separator();

				if (Button("Set Mesh & MeshRadius") == true)
				{
					Each(tgts, t => SetMeshAndMeshRadius(t, true));
				}
			}
		}

		private bool SetMeshAndMeshRadius(SgtJovian jovian, bool apply)
		{
			if (jovian.Mesh == null)
			{
				var mesh = CwHelper.LoadFirstAsset<Mesh>("Geosphere40 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						jovian.Mesh       = mesh;
						jovian.MeshRadius = SgtCommon.GetBoundsRadius(mesh.bounds);
					}

					return true;
				}
			}

			return false;
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Jovian", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtJovian.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif