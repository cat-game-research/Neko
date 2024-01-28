using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will procedurally change the <b>Intensity</b> of the attached <b>Light</b> to simulate flickering.</summary>
	[RequireComponent(typeof(CwLightIntensity))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtLightFlicker")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Light Flicker")]
	public class SgtLightFlicker : MonoBehaviour
	{
		/// <summary>The minimum <b>Multiplier</b> value.</summary>
		public float MultiplierMin { set { multiplierMin = value; } get { return multiplierMin; } } [SerializeField] private float multiplierMin = 0.9f;

		/// <summary>The maximum <b>Multiplier</b> value.</summary>
		public float MultiplierMax { set { multiplierMax = value; } get { return multiplierMax; } } [SerializeField] private float multiplierMax = 1.1f;

		/// <summary>The current animation position.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] private float offset;

		/// <summary>The current animation speed.</summary>
		public float Speed { set { speed = value; } get { return speed; } } [SerializeField] private float speed = 5.0f;

		[System.NonSerialized]
		private CwLightIntensity cachedLight;

		[System.NonSerialized]
		private float[] points;

		protected virtual void OnEnable()
		{
			cachedLight = GetComponent<CwLightIntensity>();
		}

		protected virtual void Update()
		{
			if (points == null)
			{
				points = new float[128];

				for (var i = points.Length - 1; i >= 0; i--)
				{
					points[i] = Random.value;
				}
			}

			offset += speed * Time.deltaTime;

			cachedLight.Multiplier = Mathf.Lerp(multiplierMin, multiplierMax, Sample());
		}

		private float Sample()
		{
			var noise  = Mathf.Repeat(offset, points.Length);
			var index  = (int)noise;
			var frac   = noise % 1.0f;
			var pointA = points[index];
			var pointB = points[(index + 1) % points.Length];
			var pointC = points[(index + 2) % points.Length];
			var pointD = points[(index + 3) % points.Length];

			return SgtCommon.CubicInterpolate(pointA, pointB, pointC, pointD, frac);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtLightFlicker;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtLightFlicker_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.MultiplierMin == t.MultiplierMax));
				Draw("multiplierMin", "The minimum Multiplier value.");
				Draw("multiplierMax", "The maximum Multiplier value.");
			EndError();
			Draw("offset", "The current animation position.");
			BeginError(Any(tgts, t => t.Speed == 0.0f));
				Draw("speed", "The current animation speed.");
			EndError();
		}
	}
}
#endif