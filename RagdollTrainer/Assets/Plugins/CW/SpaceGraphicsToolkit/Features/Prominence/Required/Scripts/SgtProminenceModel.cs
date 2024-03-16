using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Prominence
{
	/// <summary>This component is used to render the <b>SgtProminence</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtProminenceModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtProminenceModel : CwChild
	{
		[SerializeField]
		private SgtProminence parent;

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

		public static SgtProminenceModel Create(SgtProminence parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtProminenceModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtProminenceModel>();

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
namespace SpaceGraphicsToolkit.Prominence
{
	using UnityEditor;
	using TARGET = SgtProminenceModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtProminenceModel_Editor : CwEditor
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