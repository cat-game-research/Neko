using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component scales the current Transform based on its distance to the camera.
	/// This scaling allows you to see the object from a greater distance than usual, which is very useful for star/planet/etc billboards you need to see from a distance.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtSimpleScaler")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Simple Scaler")]
	public class SgtSimpleScaler : MonoBehaviour
	{
		/// <summary>The base scale of the object.</summary>
		public Vector3 Scale { set { scale = value; } get { return scale; } } [SerializeField] private Vector3 scale = Vector3.one;

		/// <summary>Scale is multiplied by this when at DistanceMin.</summary>
		public float Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] private float multiplier = 1.0f;

		/// <summary>The distance where the scaling begins.</summary>
		public float DistanceMin { set { distanceMin = value; } get { return distanceMin; } } [SerializeField] private float distanceMin = 1000.0f;

		/// <summary>The distance where the scaling stops.</summary>
		public float DistanceMax { set { distanceMax = value; } get { return distanceMax; } } [SerializeField] float distanceMax = 1000000.0f;

		[System.NonSerialized]
		private Transform cachedTransform;

		protected virtual void OnEnable()
		{
			cachedTransform = GetComponent<Transform>();

			SgtCamera.OnCameraPreCull += HandleCameraPreCull;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraPreCull -= HandleCameraPreCull;
		}

		private void HandleCameraPreCull(Camera camera)
		{
			UpdateDistance(Vector3.Distance(camera.transform.position, cachedTransform.position));
		}

		private void UpdateDistance(float distance)
		{
			if (distance <= DistanceMin)
			{
				cachedTransform.localScale = Vector3.zero;
			}
			else
			{
				var distanceRange = DistanceMax - DistanceMin;

				distance -= DistanceMin;

				if (distance >= distanceRange)
				{
					distance = distanceRange * 0.5f;
				}
				else
				{
					var distance01 = distance / distanceRange;

					distance -= distance * 0.5f * distance01;
				}

				var linear = distance * Multiplier;

				cachedTransform.localScale = Scale * (float)linear;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtSimpleScaler;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSimpleScaler_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Scale == Vector3.zero));
				Draw("scale", "The scale of the object when at DistanceMin.");
			EndError();
			BeginError(Any(tgts, t => t.Multiplier <= 0.0));
				Draw("multiplier", "Scale is multiplied by this, allowing you to more easily tweak large scales.");
			EndError();
			BeginError(Any(tgts, t => t.DistanceMin < 0.0 || t.DistanceMin >= t.DistanceMax));
				Draw("distanceMin", "The distance where the scaling begins.");
				Draw("distanceMax", "The distance where the scaling stops.");
			EndError();
		}
	}
}
#endif