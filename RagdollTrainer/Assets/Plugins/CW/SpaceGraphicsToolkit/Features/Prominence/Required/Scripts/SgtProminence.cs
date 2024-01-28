using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit.Prominence
{
	/// <summary>This component allows you to render a series of randomly rotated disks around a star that make the corona look volumetric and detailed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtProminence")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Prominence")]
	public class SgtProminence : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Prominence</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the starfield.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the texture applied to the prominence.</summary>
		public Texture MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Texture mainTex;

		/// <summary>This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The amount of planes used to build the prominence.</summary>
		public int PlaneCount { set { if (planeCount != value) { planeCount = value; DirtyMesh(); } } get { return planeCount; } } [SerializeField] private int planeCount = 8;

		/// <summary>The amount of quads used to build each plane.</summary>
		public int PlaneDetail { set { if (planeDetail != value) { planeDetail = value; DirtyMesh(); } } get { return planeDetail; } } [SerializeField] private int planeDetail = 10;

		/// <summary>The inner radius of the prominence planes in local coordinates.</summary>
		public float RadiusMin { set { if (radiusMin != value) { radiusMin = value; DirtyMesh(); } } get { return radiusMin; } } [SerializeField] private float radiusMin = 1.0f;

		/// <summary>The outer radius of the prominence planes in local coordinates.</summary>
		public float RadiusMax { set { if (radiusMax != value) { radiusMax = value; DirtyMesh(); } } get { return radiusMax; } } [SerializeField] private float radiusMax = 2.0f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtProminenceModel model;

		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private Mesh mesh;

		public SgtProminenceModel Model
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

		public float Width
		{
			get
			{
				return radiusMax - radiusMin;
			}
		}

		public void DirtyMesh()
		{
			UpdateMesh();
		}

		private static List<Vector3> positions = new List<Vector3>();
		private static List<Vector3> normals   = new List<Vector3>();
		private static List<Vector2> coords1   = new List<Vector2>();
		private static List<Vector2> coords2   = new List<Vector2>();
		private static List<int>     indices   = new List<int>();

		private static int _SGT_MainTex       = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_Color         = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness    = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_WorldPosition = Shader.PropertyToID("_SGT_WorldPosition");

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			positions.Clear();
			normals.Clear();
			coords1.Clear();
			coords2.Clear();
			indices.Clear();

			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Plane");
			}
			else
			{
				mesh.Clear(false);
			}

			if (planeDetail >= 2)
			{
				CwHelper.BeginSeed(seed);
					for (var i = 0; i < planeCount; i++)
					{
						AddPlane(Random.rotation);
					}
				CwHelper.EndSeed();

				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				mesh.SetVertices(positions);
				mesh.SetNormals(normals);
				mesh.SetUVs(0, coords1);
				mesh.SetUVs(1, coords2);
				mesh.SetTriangles(indices, 0);
			}
		}

		private void AddPlane(Quaternion rotation)
		{
			var offset    = positions.Count;
			var angleStep = CwHelper.Divide(Mathf.PI * 2.0f, planeDetail);
			var coordStep = CwHelper.Reciprocal(planeDetail);

			for (var i = 0; i <= planeDetail; i++)
			{
				var coord = coordStep * i;
				var angle = angleStep * i;
				var sin   = Mathf.Sin(angle);
				var cos   = Mathf.Cos(angle);

				positions.Add(rotation * new Vector3(sin * radiusMin, 0.0f, cos * radiusMin));
				positions.Add(rotation * new Vector3(sin * radiusMax, 0.0f, cos * radiusMax));

				normals.Add(rotation * Vector3.up);
				normals.Add(rotation * Vector3.up);

				coords1.Add(new Vector2(0.0f, coord * radiusMin));
				coords1.Add(new Vector2(1.0f, coord * radiusMax));

				coords2.Add(new Vector2(radiusMin, 0.0f));
				coords2.Add(new Vector2(radiusMax, 0.0f));
			}

			for (var i = 0; i < planeDetail; i++)
			{
				var offV = offset + i * 2;

				indices.Add(offV + 0);
				indices.Add(offV + 1);
				indices.Add(offV + 2);
				indices.Add(offV + 3);
				indices.Add(offV + 2);
				indices.Add(offV + 1);
			}
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static SgtProminence Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtProminence Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Prominence", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtProminence>();
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
					material = CwHelper.CreateTempMaterial("Prominence (Generated)", sourceMaterial);

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

				material.SetVector(_SGT_WorldPosition, transform.position);
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

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtProminenceModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;

			UpdateMesh();
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
			if (mesh != null)
			{
				mesh.Clear(false);

				SgtCommon.MeshPool.Push(mesh);
			}

			CwHelper.Destroy(material);
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, radiusMin);

			Gizmos.DrawWireSphere(Vector3.zero, radiusMax);
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Prominence
{
	using UnityEditor;
	using TARGET = SgtProminence;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtProminence_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Prominence</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the prominence.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the texture applied to the starfield. If this texture can contains multiple stars then you can specify their location using the <b>Atlas</b> setting.");
			EndError();
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the jovian, giving you fine control over the render order.");

			Separator();

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.PlaneCount < 1));
				Draw("planeCount", ref dirtyMesh, "The amount of planes used to build the prominence.");
			EndError();
			BeginError(Any(tgts, t => t.PlaneDetail < 3));
				Draw("planeDetail", ref dirtyMesh, "The amount of quads used to build each plane.");
			EndError();
			BeginError(Any(tgts, t => t.RadiusMin <= 0.0f || t.RadiusMin >= t.RadiusMax));
				Draw("radiusMin", ref dirtyMesh, "The inner radius of the prominence planes in local coordinates.");
			EndError();
			BeginError(Any(tgts, t => t.RadiusMax < 0.0f || t.RadiusMin >= t.RadiusMax));
				Draw("radiusMax", ref dirtyMesh, "The outer radius of the prominence planes in local coordinates.");
			EndError();

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Prominence", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtProminence.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif