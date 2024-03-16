using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component scales the current SgtFloatingObject based on its distance to the SgtFloatingOrigin.
	/// This scaling allows you to see the object from a greater distance than usual, which is very useful for star/planet/etc billboards you need to see from a distance.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFloatingObject))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingScaler")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Scaler")]
	public class SgtFloatingScaler : MonoBehaviour
	{
		/// <summary>If you want a different <b>Transform</b> to be scaled, you can specify it here.</summary>
		public Transform Target { set { target = value; } get { return target; } } [SerializeField] private Transform target;

		/// <summary>The base scale of the object.</summary>
		public Vector3 Scale { set { scale = value; } get { return scale; } } [SerializeField] private Vector3 scale = Vector3.one;

		/// <summary>The final scale will be multiplied by this.</summary>
		public double Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] private double multiplier = 0.0f;

		/// <summary>If the distance between the camera and this object is beyond this value, this object will be scaled to 0 and be invisible.</summary>
		public SgtLength Range { set { range = value; } get { return range; } } [SerializeField] private SgtLength range = 1000000.0;

		[System.NonSerialized]
		private SgtFloatingObject cachedObject;

		[System.NonSerialized]
		private bool cachedObjectSet;

		/// <summary>The <b>SgtFloatingObject</b> component alongside this component.</summary>
		public SgtFloatingObject CachedObject
		{
			get
			{
				if (cachedObjectSet == false)
				{
					cachedObject    = GetComponent<SgtFloatingObject>();
					cachedObjectSet = true;
				}

				return cachedObject;
			}
		}

		protected virtual void OnEnable()
		{
			CachedObject.OnDistance += HandleDistance;
		}

		protected virtual void OnDisable()
		{
			cachedObject.OnDistance -= HandleDistance;
		}

		private void HandleDistance(double distance)
		{
			var finalTarget = target;

			if (finalTarget == null)
			{
				finalTarget = transform;
			}

			var map = System.Math.Log10(System.Math.Max(1.0, distance));
			var sca = 1.0 - map / System.Math.Log10(range);

			sca = System.Math.Min(sca, 1.0);
			sca = System.Math.Max(sca, 0.0);

			finalTarget.localScale = scale * (float)(sca * distance * multiplier);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingScaler;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingScaler_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("target", "If you want a different Transform to be scaled, you can specify it here.");
			BeginError(Any(tgts, t => t.Scale == Vector3.zero));
				Draw("scale", "The scale of the object when at DistanceMin.");
			EndError();
			BeginError(Any(tgts, t => t.Multiplier <= 0.0));
				Draw("multiplier", "The final scale will be multiplied by this.");
			EndError();
			BeginError(Any(tgts, t => t.Range <= 0.0));
				Draw("range", "If the distance between the camera and this object is beyond this value, this object will be scaled to 0 and be invisible.");
			EndError();
		}
	}
}
#endif