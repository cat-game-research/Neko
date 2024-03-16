using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Nebula
{
	/// <summary>This component is used to render the <b>SgtNebula</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtNebulaModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtNebulaModel : CwChild
	{
		[SerializeField]
		private SgtNebula parent;

		[SerializeField]
		private MeshFilter cachedMeshFilter;

		[SerializeField]
		private MeshRenderer cachedMeshRenderer;

		public MeshFilter CachedMeshFilter
		{
			get
			{
				return cachedMeshFilter;
			}
		}

		public MeshRenderer CachedMeshRenderer
		{
			get
			{
				return cachedMeshRenderer;
			}
		}

		public static SgtNebulaModel Create(SgtNebula parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtNebulaModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtNebulaModel>();

			instance.parent             = parent;
			instance.cachedMeshFilter   = instance.GetComponent<MeshFilter>();
			instance.cachedMeshRenderer = instance.GetComponent<MeshRenderer>();

			instance.cachedMeshFilter.sharedMesh = parent.SourceMesh;

			instance.cachedMeshRenderer.sharedMaterial = parent.SourceMaterial;

			return instance;
		}

		protected override IHasChildren GetParent()
		{
			return parent;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Nebula
{
	using UnityEditor;
	using TARGET = SgtNebulaModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtNebulaModel_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginDisabled();
				Draw("parent");
			EndDisabled();
		}
	}
}
#endif