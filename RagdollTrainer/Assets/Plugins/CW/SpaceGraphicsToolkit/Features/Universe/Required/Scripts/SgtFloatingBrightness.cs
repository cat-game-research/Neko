using UnityEngine;
using UnityEngine.Events;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to adjust the brightness of .</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingBrightness")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Brightness")]
	[RequireComponent(typeof(SgtFloatingObject))]
	public class SgtFloatingBrightness : MonoBehaviour
	{
		[System.Serializable] public class FloatEvent : UnityEvent<float> {}

		/// <summary>The brightness value when at or below the <b>DistanceMin</b>.</summary>
		public float BrightnessNear { set { brightnessNear = value; } get { return brightnessNear; } } [SerializeField] private float brightnessNear = 1.0f;

		/// <summary>The brightness value when at or above the <b>DistanceMax</b>.</summary>
		public float BrightnessFar { set { brightnessFar = value; } get { return brightnessFar; } } [SerializeField] private float brightnessFar = 10.0f;

		/// <summary>The distance where the <b>BrightnessNear</b> will be used.</summary>
		public SgtLength DistanceMin { set { distanceMin = value; } get { return distanceMin; } } [SerializeField] private SgtLength distanceMin;

		/// <summary>The distance where the <b>BrightnessFar</b> will be used.</summary>
		public SgtLength DistanceMax { set { distanceMax = value; } get { return distanceMax; } } [SerializeField] private SgtLength distanceMax = new SgtLength(1.0, SgtLength.ScaleType.Kilometer);

		/// <summary>The calculated brightness value will be output via this event.</summary>
		public FloatEvent OnBrightness { get { if (onBrightness == null) onBrightness = new FloatEvent(); return onBrightness; } } [SerializeField] private FloatEvent onBrightness;

		[System.NonSerialized]
		private SgtFloatingObject cachedFloatingObject;

		protected virtual void OnEnable()
		{
			cachedFloatingObject = GetComponent<SgtFloatingObject>();

			cachedFloatingObject.OnDistance += HandleDistance;
		}

		protected virtual void OnDisable()
		{
			cachedFloatingObject.OnDistance -= HandleDistance;
		}

		private void HandleDistance(double distance)
		{
			var distance01 = 0.0;

			if (distance <= distanceMin)
			{
				distance01 = 0.0;
			}
			else if (distance >= distanceMax)
			{
				distance01 = 1.0;
			}
			else if (distanceMax > distanceMin)
			{
				distance01 = (distance - distanceMin) / (distanceMax - distanceMin);
			}

			var brightness = Mathf.Lerp(brightnessNear, brightnessFar, (float)distance01);

			if (onBrightness != null)
			{
				onBrightness.Invoke(brightness);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingBrightness;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingBrightness_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("brightnessNear", "The brightness value when at or below the <b>DistanceMin</b>.");
			Draw("brightnessFar", "The brightness value when at or above the <b>DistanceMax</b>.");
			Draw("distanceMin", "The distance where the <b>BrightnessNear</b> will be used.");
			Draw("distanceMax", "The distance where the <b>BrightnessFar</b> will be used.");

			Separator();

			Draw("onBrightness", "The calculated brightness value will be output via this event.");
		}
	}
}
#endif