using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.LightAndShadow
{
	/// <summary>The lighting system built into Unity isn't suitable for all scenarios, so this component can be used to extend it with a custom lighting system for specific components that support this.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtLight")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Light")]
	public class SgtLight : MonoBehaviour
	{
		public const int MAX_LIGHTS = 16;

		/// <summary>If the <b>Light</b> component alongside this component is directional, but it's constantly rotated toward the camera to give the illusion of it being a point light, then you should enable this setting.</summary>
		public bool TreatAsPoint { set { treatAsPoint = value; } get { return treatAsPoint; } } [SerializeField] private bool treatAsPoint = false;

		/// <summary>If you enable this setting, SGT components will use the sibling <b>Light.intensity</b> value multiplied by the <b>Brightness</b> value.</summary>
		public bool UseLightIntensity { set { useLightIntensity = value; } get { return useLightIntensity; } } [SerializeField] private bool useLightIntensity = true;

		/// <summary>The brightness value of this light as seen by SGT components.
		/// NOTE: If you enable the <b>UseLightIntensity</b> setting, then this value will be multiplied by the sibling <b>Light.intensity</b> value.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		[System.NonSerialized]
		private Transform cachedTransform;

		[System.NonSerialized]
		private bool cachedTransformSet;

		[System.NonSerialized]
		private Light cachedLight;

		[System.NonSerialized]
		private bool cachedLightSet;

		private static LinkedList<SgtLight> instances = new LinkedList<SgtLight>();

		[System.NonSerialized]
		private LinkedListNode<SgtLight> node;

		private static CwShaderBundle.Pipeline pipe;

#if __HDRP__
		[System.NonSerialized]
		private UnityEngine.Rendering.HighDefinition.HDAdditionalLightData cachedLightData;
#endif

		private static List<SgtLight> tempLights = new List<SgtLight>();

		public static int InstanceCount
		{
			get
			{
				return instances.Count;
			}
		}

		public Light CachedLight
		{
			get
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				return cachedLight;
			}
		}

		public Transform CachedTransform
		{
			get
			{
				if (cachedTransformSet == false)
				{
					cachedTransform    = GetComponent<Transform>();
					cachedTransformSet = true;
				}

				return cachedTransform;
			}
		}

		public float CachedLightIntensity
		{
			get
			{
#if __HDRP__
					if (cachedLightData == null)
					{
						cachedLightData = GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
					}

					if (cachedLightData != null)
					{
						return cachedLightData.intensity;
					}
#endif
					return cachedLight.intensity;
			}
		}

		protected virtual void OnEnable()
		{
			node = instances.AddLast(this);

			pipe = CwShaderBundle.DetectProjectPipeline();
		}

		protected virtual void OnDisable()
		{
			instances.Remove(node);
			
			node = null;
		}

		private static Vector3 compareDistanceCenter;

		private static int CompareDistance(SgtLight a, SgtLight b)
		{
			var distA = Vector3.SqrMagnitude(a.CachedTransform.position - compareDistanceCenter);
			var distB = Vector3.SqrMagnitude(b.CachedTransform.position - compareDistanceCenter);

			return distA.CompareTo(distB);
		}

		private static System.Comparison<SgtLight> CompareDistanceDel = CompareDistance;

		public static List<SgtLight> Find(int mask, Vector3 center)
		{
			tempLights.Clear();

			foreach (var light in instances)
			{
				var cachedLight = light.CachedLight;

				if (cachedLight.isActiveAndEnabled == true && light.brightness > 0.0f && (cachedLight.cullingMask & mask) != 0)
				{
					tempLights.Add(light);
				}
			}

			compareDistanceCenter = center;

			tempLights.Sort(CompareDistanceDel);

			return tempLights;
		}

		public static void FilterOut(Vector3 center)
		{
			for (var i = tempLights.Count - 1; i >= 0; i--)
			{
				var tempLight = tempLights[i];

				if (tempLight.transform.position == center)
				{
					if (tempLight.treatAsPoint == true || tempLight.CachedLight.type != LightType.Directional)
					{
						tempLights.RemoveAt(i);
					}
				}
			}
		}

		public static void Calculate(SgtLight light, Vector3 center, float radius, Transform directionTransform, Transform positionTransform, ref Vector3 position, ref Vector3 direction, ref Color color, ref float intensity)
		{
			if (light != null)
			{
				var cachedLight = light.CachedLight;

				direction = -light.transform.forward;
				position  = light.transform.position;
				color     = cachedLight.color;
				intensity = light.brightness;

				if (light.useLightIntensity == true)
				{
					intensity *= light.CachedLightIntensity;
				}

				switch (cachedLight.type)
				{
					case LightType.Point:
					{
						direction = Vector3.Normalize(position - center);

						if (CwShaderBundle.IsStandard(pipe) == true)
						{
							var dist  = CwHelper.Divide(Vector3.Distance(center, position), light.CachedLight.range);
							var atten = Mathf.Clamp01(1.0f / (1.0f + 25.0f * dist * dist) * ((1.0f - dist) * 5.0f));

							intensity *= atten;
						}
						// Attenuation is more or less the same in URP and HDRP?
						else
						{
							var dist = Vector3.Distance(center, position) - radius;

							if (dist > 0.0f)
							{
								var atten = Mathf.Clamp01(1.0f / (dist * dist));

								intensity *= atten;
							}

							var range = 1.0f - CwHelper.Divide(dist, light.CachedLight.range);

							intensity *= Mathf.Clamp01(range * range);
						}
					}
					break;

					case LightType.Directional:
					{
						if (light.treatAsPoint == true)
						{
							direction = Vector3.Normalize(position - center);
						}
						else
						{
							position = center + direction * 10000000.0f;
						}
					}
					break;
				}

				// Transform into local space?
				if (directionTransform != null)
				{
					direction = directionTransform.InverseTransformDirection(direction);
				}

				if (positionTransform != null)
				{
					position = positionTransform.InverseTransformPoint(position);
				}
			}
		}

		private static Vector4[] tempColor     = new Vector4[MAX_LIGHTS];
		private static Vector4[] tempScatter   = new Vector4[MAX_LIGHTS];
		private static Vector4[] tempPosition  = new Vector4[MAX_LIGHTS];
		private static Vector4[] tempDirection = new Vector4[MAX_LIGHTS];

		private static int _SGT_LightCount     = Shader.PropertyToID("_SGT_LightCount");
		private static int _SGT_LightColor     = Shader.PropertyToID("_SGT_LightColor");
		private static int _SGT_LightPosition  = Shader.PropertyToID("_SGT_LightPosition");
		private static int _SGT_LightDirection = Shader.PropertyToID("_SGT_LightDirection");

		public static void Write(Vector3 center, float radius, Transform directionTransform, Transform positionTransform, int maxLights)
		{
			var lightCount = 0;

			for (var i = 0; i < tempLights.Count && lightCount < maxLights; i++)
			{
				var light     = tempLights[i];
				var position  = default(Vector3);
				var direction = default(Vector3);
				var color     = default(Color);
				var intensity = default(float);

				Calculate(light, center, radius, directionTransform, positionTransform, ref position, ref direction, ref color, ref intensity);

				tempColor[lightCount] = CwHelper.Brighten(color, intensity);
				tempPosition[lightCount] = new Vector4(position.x, position.y, position.z, 1.0f);
				tempDirection[lightCount] = direction;

				lightCount += 1;
			}

			foreach (var tempMaterial in CwHelper.tempMaterials)
			{
				tempMaterial.SetInt(_SGT_LightCount, lightCount);

				if (lightCount > 0)
				{
					tempMaterial.SetVectorArray(_SGT_LightColor, tempColor);
					tempMaterial.SetVectorArray(_SGT_LightPosition, tempPosition);
					tempMaterial.SetVectorArray(_SGT_LightDirection, tempDirection);
				}
			}

			foreach (var tempProperties in CwHelper.tempProperties)
			{
				tempProperties.SetInt(_SGT_LightCount, lightCount);

				if (lightCount > 0)
				{
					tempProperties.SetVectorArray(_SGT_LightColor, tempColor);
					tempProperties.SetVectorArray(_SGT_LightPosition, tempPosition);
					tempProperties.SetVectorArray(_SGT_LightDirection, tempDirection);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.LightAndShadow
{
	using UnityEditor;
	using TARGET = SgtLight;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtLight_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("treatAsPoint", "If the <b>Light</b> component alongside this component is directional, but it's constantly rotated toward the camera to give the illusion of it being a point light, then you should enable this setting.");
			Draw("useLightIntensity", "If you enable this setting, SGT components will use the sibling <b>Light.intensity</b> value multiplied by the <b>Brightness</b> value.");
			Draw("brightness", "The brightness value of this light as seen by SGT components.\n\nNOTE: If you enable the <b>UseLightIntensity</b> setting, then this value will be multiplied by the sibling <b>Light.intensity</b> value.");
		}
	}
}
#endif