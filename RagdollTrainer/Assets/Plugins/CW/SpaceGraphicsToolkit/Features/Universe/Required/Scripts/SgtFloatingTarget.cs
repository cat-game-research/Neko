using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component marks the attached LeanFloatingPoint component as a warpable target point. This allows you to pick the target using the SgtWarpPin component.</summary>
	[RequireComponent(typeof(SgtFloatingPoint))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingTarget")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Target")]
	public class SgtFloatingTarget : MonoBehaviour
	{
		/// <summary>The shorthand name for this warp target.</summary>
		public string WarpName { set { warpName = value; } get { return warpName; } } [SerializeField] private string warpName;

		/// <summary>The distance from this SgtFloatingPoint we should warp to, to prevent you warping too close.</summary>
		public SgtLength WarpDistance { set { warpDistance = value; } get { return warpDistance; } } [SerializeField] private SgtLength warpDistance = 1000.0;

		[System.NonSerialized]
		private SgtFloatingPoint cachedPoint;

		[System.NonSerialized]
		private bool cachedPointSet;

		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtFloatingTarget> Instances = new LinkedList<SgtFloatingTarget>(); private LinkedListNode<SgtFloatingTarget> node;

		public SgtFloatingPoint CachedPoint
		{
			get
			{
				if (cachedPointSet == false)
				{
					cachedPoint    = GetComponent<SgtFloatingPoint>();
					cachedPointSet = true;
				}

				return cachedPoint;
			}
		}

		protected virtual void OnEnable()
		{
			node = Instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingTarget;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtWarpTarget_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => string.IsNullOrEmpty(t.WarpName)));
				Draw("warpName", "The shorthand name for this warp target.");
			EndError();
			BeginError(Any(tgts, t => t.WarpDistance < 0.0));
				Draw("warpDistance", "The distance from this SgtFloatingPoint we should warp to, to prevent you warping too close.");
			EndError();
		}
	}
}
#endif