using System.Collections.Generic;
using UnityEngine;
using CW.Common;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit.Nebula
{
	/// <summary>This component allows you to render a nebula or galaxy based on a deformed sphere mesh with multiple textures that are blended together.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtNebula")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Nebula")]
	public class SgtNebula : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Nebula</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>The sphere mesh used to render the nebula.
		/// NOTE: This should be a sphere mesh.</summary>
		public Mesh SourceMesh { set { if (sourceMesh != value) { sourceMesh = value; DirtyMesh(); } } get { return sourceMesh; } } [SerializeField] private Mesh sourceMesh;

		/// <summary>The radius of the nebula in local space.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMesh(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The frequency of the displacement noise.</summary>
		public float Frequency { set { if (frequency != value) { frequency = value; DirtyMesh(); } } get { return frequency; } } [SerializeField] private float frequency = 1.0f;

		/// <summary>The maximum height displacement applied to the nebula mesh when deformed.</summary>
		public float Displacement { set { if (displacement != value) { displacement = value; DirtyMesh(); } } get { return displacement; } } [SerializeField] private float displacement = 0.1f;

		/// <summary>If you want the nebula to be round then set this to 0, if you want it to be more like a galaxy then increase this.</summary>
		public float Flattening { set { if (flattening != value) { flattening = value; DirtyMesh(); } } get { return flattening; } } [SerializeField] [Range(0.00f, 0.999f)] private float flattening;

		[SerializeField]
		private SgtNebulaModel model;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private List<Vector3> generatedPositions = new List<Vector3>();

		[System.NonSerialized]
		private bool dirtyMesh;

		public SgtNebulaModel Model
		{
			get
			{
				return model;
			}
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}


		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		/// <summary>This method causes the nebula mesh to update based on the current settings. You should call this after you finish modifying them.</summary>
		[ContextMenu("Rebuild")]
		public void Rebuild()
		{
			dirtyMesh     = false;
			generatedMesh = CwHelper.Destroy(generatedMesh);

			if (sourceMesh != null)
			{
				generatedMesh = Instantiate(sourceMesh);

				generatedMesh.GetVertices(generatedPositions);

				var count = generatedMesh.vertexCount;
				var scale = radius / generatedPositions[0].magnitude;

				for (var i = 0; i < count; i++)
				{
					var normal = math.normalize(generatedPositions[i]);

					generatedPositions[i] = normal * (radius + displacement * noise.snoise(normal * frequency));
				}

				generatedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * (radius + displacement) * 2.0f);

				generatedMesh.SetVertices(generatedPositions);
			}
		}

		public static SgtNebula Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtNebula Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Nebula", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtNebula>();
		}

		protected virtual void OnEnable()
		{
			if (model == null)
			{
				model = SgtNebulaModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;
		}

		protected virtual void OnDisable()
		{
			if (model != null)
			{
				model.CachedMeshRenderer.enabled = false;
			}
		}

		protected virtual void LateUpdate()
		{
			if (generatedMesh == null || dirtyMesh == true)
			{
				Rebuild();
			}

			if (model != null)
			{
				model.CachedMeshFilter.sharedMesh = generatedMesh;
				model.CachedMeshRenderer.sharedMaterial = sourceMaterial;

				model.transform.localScale = new Vector3(1.0f, 1.0f - flattening, 1.0f);
			}
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(generatedMesh);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Nebula
{
	using UnityEditor;
	using TARGET = SgtNebula;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtNebula_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Nebula</b> shader. You cannot use a normal shader.");
			EndError();
			BeginError(Any(tgts, t => t.SourceMesh == null));
				Draw("sourceMesh", ref dirtyMesh, "The sphere mesh used to render the nebula.\n\nNOTE: This should be a sphere mesh.");
			EndError();
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the nebula in local space.");
			EndError();
			Draw("frequency", ref dirtyMesh, "The frequency of the displacement noise.");
			Draw("displacement", ref dirtyMesh, "The maximum height displacement applied to the nebula mesh when deformed.");
			Draw("flattening", ref dirtyMesh, "If you want the nebula to be round then set this to 0, if you want it to be more like a galaxy then increase this.");

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Nebula", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtNebula.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif