using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to warp to the target when clicking a button.
	/// NOTE: The button's OnClick event must be linked to the Click method.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingWarpButton")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Warp Button")]
	public class SgtFloatingWarpButton : MonoBehaviour
	{
		/// <summary>The point that will be warped to.</summary>
		public SgtFloatingTarget Target { set { target = value; } get { return target; } } [SerializeField] private SgtFloatingTarget target;

		/// <summary>The warp effect that will be used.</summary>
		public SgtFloatingWarp Warp { set { warp = value; } get { return warp; } } [SerializeField] private SgtFloatingWarp warp;

		public void Click()
		{
			warp.WarpTo(target.CachedPoint.Position, target.WarpDistance);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingWarpButton;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingWarpButton_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Target == null));
				Draw("target", "The point that will be warped to.");
			EndError();
			BeginError(Any(tgts, t => t.Warp == null));
				Draw("warp", "The warp effect that will be used.");
			EndError();
		}
	}
}
#endif