using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Singularity
{
	/// <summary>This component allows you to add a gravitational lensing effect to your scene, as seen around black holes, and other dense bodies.
	/// The lens effect works similar to reflection probes, and uses a cubemap as a source for the texture data.
	/// This means it renders very fast, works consistently across the screen, and can simulate lensing in any direction.
	/// NOTE: Updating the cubemap texture can be expensive, and the effect won't work so well if you have rendered objects to the lens center.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(100)]
	[DisallowMultipleComponent]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtLens")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Lens")]
	[RequireComponent(typeof(Camera))]
	public class SgtLens : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Lens</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>The mesh used to render the lens effect. This should be a sphere.</summary>
		public Mesh Mesh { set { mesh = value; } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>This allows you to set the width & height of each face in the cubemap this component renders.
		/// NOTE: The higher you set this, the higher the visual quality, but the more expensive the texture updates will be.</summary>
		public int Resolution { set { resolution = value; } get { return resolution; } } [SerializeField] private int resolution = 1024;

		/*
		/// <summary>This allows you to set the world space distance where the specified resolution will be used. Each time the distance doubles, the resolution will be halved.
		/// 0 = Ignore distance, and use a fixed resolution.</summary>
		public float Range { set { range = value; } get { return range; } } [SerializeField] private float range;
		*/

		/// <summary>This allows you to control how often the cubemap texture updates in seconds.
		/// -1 = Manual only.
		/// 0 = Every frame.
		/// 2 = Every two seconds.</summary>
		public float Interval { set { interval = value; } get { return interval; } } [SerializeField] private float interval = -1.0f;

		/// <summary>This allows you to control how the edge of the effect fades away into the normal scene. There will usually be some difference between the actual scene and the spatially distorted scene rendered from the cubemap, and this can hide that transition.</summary>
		public float FadeOuter { set { fadeOuter = value; } get { return fadeOuter; } } [Range(0.01f, 1.0f)] [SerializeField] private float fadeOuter = 0.1f;

		/// <summary>This allows you control how far from the outer edge the spatial distortion begins.</summary>
		public float WarpOuter { set { warpOuter = value; } get { return warpOuter; } } [Range(0.001f, 1.0f)] [SerializeField] private float warpOuter = 0.1f;

		/// <summary>This allows you control the strength of the spatial distortion at the event horizon. Higher values will cause the light to bend around, allowing you to see backwards.</summary>
		public float WarpStrength { set { warpStrength = value; } get { return warpStrength; } } [SerializeField] private float warpStrength = 1.5f;

		/// <summary>This allows you to set the size of the black hole disc.</summary>
		public float HoleSize { set { holeSize = value; } get { return holeSize; } } [Range(0.99f, 1.1f)] [SerializeField] private float holeSize = 1.01f;

		/// <summary>This allows you to control the thickness of the black hole disc (event horizon).</summary>
		public float HoleEdge { set { holeEdge = value; } get { return holeEdge; } } [Range(0.001f, 1.0f)] [SerializeField] private float holeEdge = 0.01f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtLensModel model;

		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private Camera cachedCamera;

		[System.NonSerialized]
		private bool cachedCameraSet;

		[System.NonSerialized]
		private Cubemap cubemap;

		[System.NonSerialized]
		private float updateTime;

		[System.NonSerialized]
		private int mask = -1;

		private static int _SGT_MainTex       = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_WorldPosition = Shader.PropertyToID("_SGT_WorldPosition");
		private static int _SGT_WarpOuter     = Shader.PropertyToID("_SGT_WarpOuter");
		private static int _SGT_WarpStrength  = Shader.PropertyToID("_SGT_WarpStrength");
		private static int _SGT_HoleSize      = Shader.PropertyToID("_SGT_HoleSize");
		private static int _SGT_HoleEdge      = Shader.PropertyToID("_SGT_HoleEdge");
		private static int _SGT_FadeOuter     = Shader.PropertyToID("_SGT_FadeOuter");

		public SgtLensModel Model
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

		/// <summary>This property gives you the <b>Camera</b> alongside this component.</summary>
		public Camera CachedCamera
		{
			get
			{
				if (cachedCameraSet == false)
				{
					cachedCamera    = GetComponent<Camera>();
					cachedCameraSet = true;
				}

				return cachedCamera;
			}
		}

		[ContextMenu("Update Cubemap")]
		public void UpdateCubemap()
		{
			if (cubemap != null && cubemap.width != resolution)
			{
				cubemap = CwHelper.Destroy(cubemap);
			}

			if (cubemap == null)
			{
				cubemap = new Cubemap(resolution, TextureFormat.RGB24, false);

				if (material != null)
				{
					material.SetTexture(_SGT_MainTex, cubemap);
				}
			}

			cachedCamera.RenderToCubemap(cubemap, mask);

			updateTime = 0.0f;
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static SgtLens Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtLens Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Lens", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtLens>();
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
					material = CwHelper.CreateTempMaterial("Lens (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				material.SetVector(_SGT_WorldPosition, transform.position);
				material.SetFloat(_SGT_WarpOuter, CwHelper.Reciprocal(warpOuter));
				material.SetFloat(_SGT_WarpStrength, warpStrength);
				material.SetFloat(_SGT_HoleSize, CwHelper.Reciprocal(HoleSize));
				material.SetFloat(_SGT_HoleEdge, CwHelper.Reciprocal(holeEdge));
				material.SetFloat(_SGT_FadeOuter, CwHelper.Reciprocal(fadeOuter));

				material.SetTexture(_SGT_MainTex, cubemap);
			}
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtLensModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;

			if (CachedCamera.enabled == true) // NOTE: Property
			{
				cachedCamera.enabled = false;

				var camera = Camera.main;

				if (camera != null)
				{
					cachedCamera.enabled         = false;
					cachedCamera.clearFlags      = CameraClearFlags.Color;
					cachedCamera.backgroundColor = camera.backgroundColor;
					cachedCamera.nearClipPlane   = camera.nearClipPlane;
					cachedCamera.farClipPlane    = camera.farClipPlane;
				}
			}
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

			if (cubemap == null)
			{
				UpdateCubemap();
			}
			else if (interval >= 0.0f)
			{
				if (updateTime >= interval)
				{
					UpdateCubemap();
				}
			}

			updateTime += Time.deltaTime;
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
	using TARGET = SgtLens;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtLens_Editor : CwEditor
	{
		private static int[] resolutionValues = new int[] { 32, 64, 128, 256, 512, 1024, 2048 };

		private static GUIContent[] resolutionNames = new GUIContent[] { new GUIContent("32"), new GUIContent("64"), new GUIContent("128"), new GUIContent("256"), new GUIContent("512"), new GUIContent("1024"), new GUIContent("2048") };

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var updateCubemap = false;

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Lens</b> shader. You cannot use a normal shader.");
			EndError();
			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", "The mesh used to render the lens effect. This should be a sphere.");
			EndError();
			DrawIntPopup(resolutionValues, resolutionNames, "resolution", ref updateCubemap, "This allows you to set the width & height of each face in the cubemap this component renders.\n\nNOTE: The higher you set this, the higher the visual quality, but the more expensive the texture updates will be.");
			//Draw("range", "This allows you to set the world space distance where the specified resolution will be used. Each time the distance doubles, the resolution will be halved.\n\n0 = Ignore distance, and use a fixed resolution.");
			Draw("interval", "This allows you to control how often the cubemap texture updates in seconds.\n\n-1 = Manual only.\n\n0 = Every frame.\n\n2 = Every two seconds.");
			Draw("fadeOuter", "This allows you to control how the edge of the effect fades away into the normal scene. There will usually be some difference between the actual scene and the spatially distorted scene rendered from the cubemap, and this can hide that transition.");

			Separator();

			Draw("warpOuter", "This allows you control how far from the outer edge the spatial distortion begins.");
			Draw("warpStrength", "This allows you control the strength of the spatial distortion at the event horizon. Higher values will cause the light to bend around, allowing you to see backwards.");
			
			Separator();
			
			Draw("holeSize", "This allows you to set the size of the black hole disc.");
			Draw("holeEdge", "This allows you to control the thickness of the black hole disc (event horizon).");

			if (Any(tgts, t => SetMesh(t, false)))
			{
				Separator();

				if (Button("Set Mesh") == true)
				{
					Each(tgts, t => SetMesh(t, true));
				}
			}

			if (Any(tgts, t => t.gameObject.layer == 0))
			{
				Warning("You should change this GameObject's layer, so it can render normal objects, but not itself.");
			}

			if (Any(tgts, t => (t.CachedCamera.cullingMask & 1 << t.gameObject.layer) != 0))
			{
				if (HelpButton("You should remove this GameObject's layer from the camera culling mask.", UnityEditor.MessageType.Warning, "Fix", 30) == true)
				{
					Each(tgts, t => t.CachedCamera.cullingMask &= ~(1 << t.gameObject.layer), true);
				}
			}

			Separator();

			if (Button("Update Cubemap") == true)
			{
				Each(tgts, t => t.UpdateCubemap());
			}

			if (updateCubemap == true)
			{
				Each(tgts, t => t.UpdateCubemap());
			}
		}

		private bool SetMesh(SgtLens lens, bool apply)
		{
			if (lens.Mesh == null)
			{
				var mesh = CwHelper.LoadFirstAsset<Mesh>("Geosphere20 t:mesh");

				if (mesh != null)
				{
					if (apply == true)
					{
						lens.Mesh = mesh;
					}

					return true;
				}
			}

			return false;
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Lens", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtLens.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif