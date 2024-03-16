using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Belt
{
	/// <summary>This component is used to render the <b>SgtBelt</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBeltModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtBeltModel : CwChild
	{
		[SerializeField]
		private SgtBelt parent;

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

		public static SgtBeltModel Create(SgtBelt parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtBeltModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtBeltModel>();

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
namespace SpaceGraphicsToolkit.Belt
{
	using UnityEditor;
	using TARGET = SgtBeltModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBeltModel_Editor : CwEditor
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