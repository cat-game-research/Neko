using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Aurora
{
	/// <summary>This component is used to render the <b>SgtAurora</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtAuroraModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtAuroraModel : CwChild
	{
		[SerializeField]
		private SgtAurora parent;

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

		public static SgtAuroraModel Create(SgtAurora parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtAuroraModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtAuroraModel>();

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
namespace SpaceGraphicsToolkit.Aurora
{
	using UnityEditor;
	using TARGET = SgtAuroraModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtAuroraModel_Editor : CwEditor
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