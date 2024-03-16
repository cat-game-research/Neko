using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Thruster
{
	/// <summary>This component allows you to create simple thrusters that can apply forces to Rigidbodies based on their position. You can also use sprites to change the graphics</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtThrusterScale")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Thruster Scale")]
	public class SgtThrusterScale : MonoBehaviour
	{
		/// <summary>The thruster the scale will be based on.</summary>
		public SgtThruster Thruster { set { thruster = value; } get { return thruster; } } [SerializeField] private SgtThruster thruster;

		/// <summary>The speed at which the scale reaches its target value.</summary>
		public float Damping { set { damping = value; } get { return damping; } } [SerializeField] private float damping = 10.0f;

		/// <summary>The scale value that's applied by default.</summary>
		public Vector3 BaseScale { set { baseScale = value; } get { return baseScale; } } [SerializeField] private Vector3 baseScale;

		/// <summary>The scale value that's added when the throttle is 1.</summary>
		public Vector3 ThrottleScale { set { throttleScale = value; } get { return throttleScale; } } [SerializeField] private Vector3 throttleScale = Vector3.one;

		/// <summary>The amount the ThrottleScale flickers over time.</summary>
		public float Flicker { set { flicker = value; } get { return flicker; } } [SerializeField] [Range(0.0f, 1.0f)] private float flicker = 0.1f;

		/// <summary>The offset of the flicker animation.</summary>
		public float FlickerOffset { set { flickerOffset = value; } get { return flickerOffset; } } [SerializeField] private float flickerOffset;

		/// <summary>The speed of the flicker animation.</summary>
		public float FlickerSpeed { set { flickerSpeed = value; } get { return flickerSpeed; } } [SerializeField] private float flickerSpeed = 5.0f;

		[SerializeField]
		private float throttle;

		[System.NonSerialized]
		private float[] points;

		protected virtual void Start()
		{
			if (thruster == null)
			{
				thruster = GetComponentInParent<SgtThruster>();
			}
		}

		protected virtual void Update()
		{
			if (thruster != null)
			{
				if (Application.isPlaying == true)
				{
					flickerOffset += flickerSpeed * Time.deltaTime;
				}

				if (points == null)
				{
					points = new float[128];

					for (var i = points.Length - 1; i >= 0; i--)
					{
						points[i] = Random.value;
					}
				}

				var noise  = Mathf.Repeat(flickerOffset, points.Length);
				var index  = (int)noise;
				var frac   = noise % 1.0f;
				var pointA = points[index];
				var pointB = points[(index + 1) % points.Length];
				var pointC = points[(index + 2) % points.Length];
				var pointD = points[(index + 3) % points.Length];
				var f      = 1.0f - SgtCommon.CubicInterpolate(pointA, pointB, pointC, pointD, frac) * flicker;
				var factor = CwHelper.DampenFactor(damping, Time.deltaTime);

				throttle = Mathf.Lerp(throttle, thruster.Throttle, factor);

				transform.localScale = baseScale + throttleScale * throttle * f;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Thruster
{
	using UnityEditor;
	using TARGET = SgtThrusterScale;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtThrusterScale_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Thruster == null));
				Draw("thruster", "The thruster the scale will be based on.");
			EndError();
			Draw("damping", "The speed at which the scale reaches its target value.");
			Draw("baseScale", "The scale value that's applied by default.");
			Draw("throttleScale", "The scale value that's added when the throttle is 1.");

			Separator();

			Draw("flicker", "The amount the ThrottleScale flickers over time.");
			Draw("flickerOffset", "The offset of the flicker animation.");
			Draw("flickerSpeed", "The speed of the flicker animation.");
		}
	}
}
#endif