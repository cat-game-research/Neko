using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Cloudsphere
{
	/// <summary>This component is used to render the <b>SgtCloudsphere</b> component.
	/// NOTE: This component is automatically created and managed.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtCloudsphereModel")]
	[AddComponentMenu("")]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtCloudsphereModel : CwChild
	{
		[SerializeField]
		private SgtCloudsphere parent;

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

		public static SgtCloudsphereModel Create(SgtCloudsphere parent)
		{
			var gameObject = CwHelper.CreateGameObject("SgtCloudsphereModel", parent.gameObject.layer, parent.transform);
			var instance   = gameObject.AddComponent<SgtCloudsphereModel>();

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
namespace SpaceGraphicsToolkit.Cloudsphere
{
	using UnityEditor;
	using TARGET = SgtCloudsphereModel;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtCloudsphereModel_Editor : CwEditor
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