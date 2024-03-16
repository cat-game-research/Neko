using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Singularity
{
	/// <summary>This component is used to render the <b>SgtBlackHole</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBlackHoleModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtBlackHoleModel : CwChild
	{
		[SerializeField]
		private SgtBlackHole parent;

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

		public static SgtBlackHoleModel Create(SgtBlackHole parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtBlackHoleModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtBlackHoleModel>();

			instance.parent             = parent;
			instance.cachedMeshFilter   = instance.GetComponent<MeshFilter>();
			instance.cachedMeshRenderer = instance.GetComponent<MeshRenderer>();

			instance.cachedMeshRenderer.sharedMaterial = parent.Material;

			return instance;
		}

		protected override IHasChildren GetParent()
		{
			return parent;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Singularity
{
	using UnityEditor;
	using TARGET = SgtBlackHoleModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBlackHoleModel_Editor : CwEditor
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