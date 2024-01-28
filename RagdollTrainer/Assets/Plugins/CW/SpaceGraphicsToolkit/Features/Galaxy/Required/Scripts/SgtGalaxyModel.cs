using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Galaxy
{
	/// <summary>This component is used to render the <b>SgtGalaxy</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtGalaxyModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtGalaxyModel : CwChild
	{
		[SerializeField]
		private SgtGalaxy parent;

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

		public static SgtGalaxyModel Create(SgtGalaxy parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtGalaxyModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtGalaxyModel>();

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
namespace SpaceGraphicsToolkit.Galaxy
{
	using UnityEditor;
	using TARGET = SgtGalaxyModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtGalaxyModel_Editor : CwEditor
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