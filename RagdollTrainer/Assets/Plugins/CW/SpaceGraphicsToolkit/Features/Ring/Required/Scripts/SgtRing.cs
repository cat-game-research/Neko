using UnityEngine;
using CW.Common;
using System.Collections.Generic;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit.Ring
{
	/// <summary>This component allows you to render a ring (e.g. planetary ring, accretion disk).
	/// This ring is split in half, to improve depth sorting behavior when placed around a planet.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtRing")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Ring")]
	public class SgtRing : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Ring</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the ring.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the texture applied to the starfield. If this texture can contains multiple stars then you can specify their location using the <b>Atlas</b> setting.</summary>
		public Texture MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Texture mainTex;

		/// <summary>The radius of the inner edge of the ring in local space.</summary>
		public float RadiusInner { set { radiusInner = value; } get { return radiusInner; } } [SerializeField] private float radiusInner = 0.5f;

		/// <summary>The radius of the outer edge of the ring in local space.</summary>
		public float RadiusOuter { set { radiusOuter = value; } get { return radiusOuter; } } [SerializeField] private float radiusOuter = 1.0f;

		/// <summary>Should the ring be split into two parts to improve depth sorting with planets?</summary>
		public bool Split { set { split = value; } get { return split; } } [SerializeField] private bool split = true;

		/// <summary>This allows you to set the draw order sorting point distance of the ring in local space.
		/// This can be used to improve depth sorting when the ring is around a transparent jovian or atmosphere. This value should be similar to the <b>RadiusOuter</b> value, perhaps larger depending on the size of your planet.</summary>
		public float SortDistance { set { sortDistance = value; } get { return sortDistance; } } [SerializeField] private float sortDistance = 2.0f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtRingModel foregroundModel;

		[SerializeField]
		private SgtRingModel backgroundModel;

		[System.NonSerialized]
		private Mesh foregroundMesh;

		[System.NonSerialized]
		private Mesh backgroundMesh;

		[System.NonSerialized]
		private Material material;

		private static List<Vector3> positions = new List<Vector3>();
		private static List<Vector2> coords    = new List<Vector2>();
		private static List<int>     indices   = new List<int>();
		private static List<float>   angles    = new List<float>();

		private static readonly int SEGMENT_COUNT = 33;

		private static int _SGT_MainTex      = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_NearTex      = Shader.PropertyToID("_SGT_NearTex");
		private static int _SGT_LightingTex  = Shader.PropertyToID("_SGT_LightingTex");
		private static int _SGT_Color        = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness   = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_World2Object = Shader.PropertyToID("_SGT_World2Object");
		private static int _SGT_Radius       = Shader.PropertyToID("_SGT_Radius");

		public SgtRingModel ForegroundModel
		{
			get
			{
				return foregroundModel;
			}
		}

		public SgtRingModel BackgroundModel
		{
			get
			{
				return backgroundModel;
			}
		}

		public Material Material
		{
			get
			{
				return material;
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

		public bool HasChild(CwChild child)
		{
			return child == foregroundModel || child == backgroundModel;
		}

		public static SgtRing Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtRing Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Ring", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtRing>();
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
					material = CwHelper.CreateTempMaterial("Ring (Generated)", sourceMaterial);

					if (foregroundModel != null)
					{
						foregroundModel.CachedMeshRenderer.sharedMaterial = material;
					}

					if (backgroundModel != null)
					{
						backgroundModel.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				material.SetTexture(_SGT_MainTex, mainTex != null ? mainTex : Texture2D.whiteTexture);
				material.SetColor(_SGT_Color, color);
				material.SetFloat(_SGT_Brightness, brightness);

				material.SetMatrix(_SGT_World2Object, transform.worldToLocalMatrix);

				material.SetVector(_SGT_Radius, new Vector3(radiusInner, radiusOuter, CwHelper.Reciprocal(radiusOuter - radiusInner)));

				if (material != null && material.IsKeywordEnabled("_SGT_LIGHTING") == true)
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
					SgtLight.Write(transform.position, CwHelper.UniformScale(transform.lossyScale) * radiusOuter, null, null, SgtLight.MAX_LIGHTS);
				}

				UpdateMeshes(camera.transform.position);
			}
		}

		private void UpdateMeshes(Vector3 eye)
		{
			var view      = transform.InverseTransformPoint(eye); view.y = 0.0f;
			var bearing   = (float)Mathf.Atan2(view.x, view.z) * Mathf.Rad2Deg;
			var distance  = Vector3.Magnitude(view);
			var radiusMin = (double)radiusInner;
			var radiusMax = (double)radiusOuter;

			radiusMax /= Mathf.Cos(Mathf.PI / SEGMENT_COUNT);

			if (split == true)
			{
				var visibleRadians = distance > 0.0 ? Mathf.Acos(radiusInner / distance) : Mathf.PI;
				var visibleDegrees = Mathf.Rad2Deg * Mathf.Max(visibleRadians, 0.1f);
				var inverseDegrees = 360.0f - visibleDegrees;

				UpdateMesh(ref foregroundMesh, (float)radiusMin, (float)radiusMax, (bearing - visibleDegrees) * Mathf.Deg2Rad, (bearing + visibleDegrees) * Mathf.Deg2Rad, Mathf.CeilToInt(visibleDegrees / 180.0f * SEGMENT_COUNT));
				UpdateMesh(ref backgroundMesh, (float)radiusMin, (float)radiusMax, (bearing + visibleDegrees) * Mathf.Deg2Rad, (bearing + inverseDegrees) * Mathf.Deg2Rad, Mathf.CeilToInt(inverseDegrees / 360.0f * SEGMENT_COUNT));

				foregroundModel.CachedMeshFilter.sharedMesh = foregroundMesh;
				backgroundModel.CachedMeshFilter.sharedMesh = backgroundMesh;
			}
			else
			{
				UpdateMesh(ref foregroundMesh, (float)radiusMin, (float)radiusMax, (bearing - 180.0f) * Mathf.Deg2Rad, (bearing + 180.0f) * Mathf.Deg2Rad, SEGMENT_COUNT);

				foregroundModel.CachedMeshFilter.sharedMesh = foregroundMesh;
				backgroundModel.CachedMeshFilter.sharedMesh = null;
			}
		}

		private void UpdateMesh(ref Mesh mesh, float radiusMin, float radiusMax, float angleMin, float angleMax, int segments)
		{
			if (mesh == null)
			{
				mesh = new Mesh();

				mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			}
			else
			{
				mesh.Clear();
			}

			positions.Clear();
			coords.Clear();
			indices.Clear();
			angles.Clear();

			for (var i = 0; i <= segments; i++)
			{
				var frac  = i / (float)segments;
				var angle = Mathf.Lerp(angleMin, angleMax, frac);

				angles.Add(angle);
			}

			for (var i = 0; i <= segments; i++)
			{
				var angle = angles[i];
				var u     = angle / (Mathf.PI * 2.0f);
				var sin   = Mathf.Sin(angle);
				var cos   = Mathf.Cos(angle);

				positions.Add(new Vector3(sin * radiusMin, 0.0f, cos * radiusMin));
				positions.Add(new Vector3(sin * radiusMax, 0.0f, cos * radiusMax));

				coords.Add(new Vector2(radiusMin, u * radiusMin));
				coords.Add(new Vector2(radiusMax, u * radiusMax));
			}

			for (var i = 0; i < segments; i++)
			{
				indices.Add(i * 2 + 0); indices.Add(i * 2 + 1); indices.Add(i * 2 + 2);
				indices.Add(i * 2 + 3); indices.Add(i * 2 + 2); indices.Add(i * 2 + 1);
			}

			mesh.SetVertices(positions);
			mesh.SetUVs(0, coords);
			mesh.SetTriangles(indices, 0);
			mesh.RecalculateBounds();

			if (split == true)
			{
				var b = mesh.bounds;
				b.center = b.center.normalized * sortDistance;
				b.Expand(Vector3.one * radiusOuter);
				mesh.bounds = b;
			}
		}

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (foregroundModel == null)
			{
				foregroundModel = SgtRingModel.Create(this);
			}

			foregroundModel.CachedMeshRenderer.enabled = true;

			if (backgroundModel == null)
			{
				backgroundModel = SgtRingModel.Create(this);
			}

			backgroundModel.CachedMeshRenderer.enabled = true;
		}

		protected virtual void OnDisable()
		{
			CwHelper.OnCameraPreRender -= HandleCameraPreRender;

			if (foregroundModel != null)
			{
				foregroundModel.CachedMeshRenderer.enabled = false;
			}

			if (backgroundModel != null)
			{
				backgroundModel.CachedMeshRenderer.enabled = false;
			}
		}

		protected virtual void OnDestroy()
		{
			if (foregroundMesh != null)
			{
				foregroundMesh.Clear(false);

				SgtCommon.MeshPool.Push(foregroundMesh);
			}

			if (backgroundMesh != null)
			{
				backgroundMesh.Clear(false);

				SgtCommon.MeshPool.Push(backgroundMesh);
			}

			CwHelper.Destroy(material);
		}

		protected virtual void LateUpdate()
		{
#if UNITY_EDITOR
			SyncMaterial();
#endif
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			var step = (Mathf.PI * 2.0f) / 36.0f;

			for (var i = 0; i <= 36; i++)
			{
				var angA = step * i;
				var angB = step + angA;
				var posA = new Vector3(Mathf.Sin(angA), 0.0f, Mathf.Cos(angA));
				var posB = new Vector3(Mathf.Sin(angB), 0.0f, Mathf.Cos(angB));

				Gizmos.DrawLine(posA * radiusInner, posB * radiusInner);
				Gizmos.DrawLine(posA * radiusInner, posB * radiusInner);
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Ring
{
	using UnityEditor;
	using TARGET = SgtRing;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtRing_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Ring</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the ring.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the texture applied to the ring.");
			EndError();

			Separator();

			Draw("radiusInner", "The radius of the inner edge of the ring in local space.");
			Draw("radiusOuter", "The radius of the outer edge of the ring in local space.");
			Draw("split", "Should the ring be split into two parts to improve depth sorting with planets?");
			Draw("sortDistance", "This allows you to set the draw order sorting point distance of the ring in local space.\n\nThis can be used to improve depth sorting when the ring is around a transparent jovian or atmosphere. This value should be similar to the <b>RadiusOuter</b> value, perhaps larger depending on the size of your planet.");

			if (Any(tgts, t => t.NeedsNearTex == true && t.GetComponent<SgtRingNearTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a NearTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtRingNearTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsLightingTex == true && t.GetComponent<SgtRingLightingTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a LightingTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtRingLightingTex>(t.gameObject));
				}
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Ring", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtRing.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif