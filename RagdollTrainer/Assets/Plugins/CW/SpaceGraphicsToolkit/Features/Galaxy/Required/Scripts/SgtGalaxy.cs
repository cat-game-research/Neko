using System.Collections.Generic;
using UnityEngine;
using CW.Common;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit.Galaxy
{
	/// <summary>This component allows you to render a galaxy based on a deformed sphere mesh with multiple textures that are blended together.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtGalaxy")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Galaxy")]
	public class SgtGalaxy : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Galaxy</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>The sphere mesh used to render the galaxy.
		/// NOTE: This should be a sphere mesh.</summary>
		public Mesh SourceMesh { set { if (sourceMesh != value) { sourceMesh = value; } } get { return sourceMesh; } } [SerializeField] private Mesh sourceMesh;

		/// <summary>The radius of the nebula in local space.</summary>
		public float Radius { set { if (radius != value) { radius = value; } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>If you want the nebula to be round then set this to 0, if you want it to be more like a galaxy then increase this.</summary>
		public float Flattening { set { if (flattening != value) { flattening = value; } } get { return flattening; } } [SerializeField] [Range(0.00f, 0.999f)] private float flattening;

		[SerializeField]
		private SgtGalaxyModel model;

		public SgtGalaxyModel Model
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

		public static SgtGalaxy Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtGalaxy Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Galaxy", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtGalaxy>();
		}

		protected virtual void OnEnable()
		{
			if (model == null)
			{
				model = SgtGalaxyModel.Create(this);
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
			if (model != null)
			{
				model.CachedMeshFilter.sharedMesh = sourceMesh;
				model.CachedMeshRenderer.sharedMaterial = sourceMaterial;

				var scale = CwHelper.Divide(radius, CwHelper.UniformScale(sourceMesh.bounds.size));

				model.transform.localScale = new Vector3(scale, scale * (1.0f - flattening), scale);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Galaxy
{
	using UnityEditor;
	using TARGET = SgtGalaxy;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtGalaxy_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Galaxy</b> shader. You cannot use a normal shader.");
			EndError();
			BeginError(Any(tgts, t => t.SourceMesh == null));
				Draw("sourceMesh", "The sphere mesh used to render the galaxy.\n\nNOTE: This should be a sphere mesh.");
			EndError();
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", "The radius of the galaxy in local space.");
			EndError();
			Draw("flattening", "If you want the galaxy to be round then set this to 0, if you want it to be more like a galaxy then increase this.");
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Galaxy", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtGalaxy.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif