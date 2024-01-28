using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will smoothly warp to the target, where the speed will slow down near the start of the travel, and near the end.</summary>
	[DefaultExecutionOrder(100)]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingWarpSmoothstep")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Warp Smoothstep")]
	public class SgtFloatingWarpSmoothstep : SgtFloatingWarp
	{
		/// <summary>Seconds it takes to complete a warp.</summary>
		public double WarpTime { set { warpTime = value; } get { return warpTime; } } [SerializeField] private double warpTime = 10.0;

		/// <summary>Warp smoothstep iterations.</summary>
		public int Smoothness { set { smoothness = value; } get { return smoothness; } } [SerializeField] private int smoothness = 3;

		/// <summary>Currently warping?</summary>
		public bool Warping { set { warping = value; } get { return warping; } } [SerializeField] private bool warping;

		/// <summary>Current warp progress in seconds.</summary>
		public double Progress { set { progress = value; } get { return progress; } } [SerializeField] private double progress;

		/// <summary>Start position of the warp.</summary>
		public SgtPosition StartPosition { set { startPosition = value; } get { return startPosition; } } [SerializeField] private SgtPosition startPosition;

		/// <summary>Target position of the warp.</summary>
		public SgtPosition TargetPosition { set { targetPosition = value; } get { return targetPosition; } } [SerializeField] private SgtPosition targetPosition;

		public override bool CanAbortWarp
		{
			get
			{
				return warping;
			}
		}

		public override void WarpTo(SgtPosition position)
		{
			warping        = true;
			progress       = 0.0;
			startPosition  = point.Position;
			targetPosition = position;
		}

		public override void AbortWarp()
		{
			warping = false;
		}

		protected virtual void Update()
		{
			if (warping == true)
			{
				progress += Time.deltaTime;

				if (progress > warpTime)
				{
					progress = warpTime;
				}

				var bend = SmoothStep(progress / warpTime, smoothness);

				if (point != null)
				{
					point.Position = SgtPosition.Lerp(ref startPosition, ref targetPosition, bend);
				}

				if (progress >= warpTime)
				{
					warping = false;
				}
			}
		}

		private static double SmoothStep(double m, int n)
		{
			for (int i = 0 ; i < n ; i++)
			{
				m = m * m * (3.0 - 2.0 * m);
			}

			return m;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingWarpSmoothstep;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingWarpSmoothstep_Editor : SgtFloatingWarp_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			BeginError(Any(tgts, t => t.WarpTime < 0.0));
				Draw("warpTime", "Seconds it takes to complete a warp.");
			EndError();
			BeginError(Any(tgts, t => t.Smoothness < 1));
				Draw("smoothness", "Warp smoothstep iterations.");
			EndError();

			Separator();

			BeginDisabled();
				Draw("warping", "Currently warping?");
				Draw("progress", "Current warp progress in seconds.");
				Draw("startPosition", "Start position of the warp.");
				Draw("targetPosition", "Target position of the warp.");
			EndDisabled();
		}
	}
}
#endif