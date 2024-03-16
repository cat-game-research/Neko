using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is used by all the demo scenes to perform common tasks. Including modifying the current scene to make it look consistent between different rendering pipelines.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class SgtDemo : CwDemo
	{
		/// <summary>If you enable this setting and your project is running with HDRP and your scene contains <b>SgtAtmosphere</b> components with scattering, then their <b>ScatteringHdr</b> settings will be enabled.</summary>
		public bool ForceScatteringHdrInHDRP { set { forceScatteringHdrInHDRP = value; } get { return forceScatteringHdrInHDRP; } } [SerializeField] private bool forceScatteringHdrInHDRP = true;

		protected override void TryApplyHDRP()
		{
			base.TryApplyHDRP();

			if (forceScatteringHdrInHDRP == true)
			{
				TryForceScattering();
			}
		}

		private void TryForceScattering()
		{
			//foreach (var atmosphere in CwHelper.FindObjectsByType<SgtAtmosphere>())
			{
			//	atmosphere.ScatteringHdr = true;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtDemo;

	[CustomEditor(typeof(TARGET))]
	public class SgtDemo_Editor : CwDemo_Editor
	{
		protected override void OnInspector()
		{
			base.OnInspector();

			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("forceScatteringHdrInHDRP", "If you enable this setting and your project is running with HDRP and your scene contains <b>SgtAtmosphere</b> components with scattering, then their <b>ScatteringHdr</b> settings will be enabled.");
		}
	}
}
#endif