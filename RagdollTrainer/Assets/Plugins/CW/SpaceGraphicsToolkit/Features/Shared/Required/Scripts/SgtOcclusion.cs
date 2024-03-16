using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class allows you to calculate how much light is occluded between two 3D points.</summary>
	public static class SgtOcclusion
	{
		private static List<float> distances = new List<float>();

		private static RaycastHit[] tempHits = new RaycastHit[1024];

		private static float NearestDist(Vector3 linePoint, Vector3 lineDirection, Vector3 point)
		{
			return Vector3.Dot(point - linePoint, lineDirection);
		}

		public static void TryScaleBackDistantPositions(ref Vector4 worldEye, ref Vector4 worldTgt, Vector3 worldCenter, float worldRadius)
		{
			// Scale back worldTgt if it's too far away
			var worldVec = worldTgt - worldEye;
			var worldDir = worldVec.normalized;
			var worldDst = worldVec.magnitude;
			var closeDst = NearestDist(worldEye, worldDir, worldCenter) + worldRadius;

			if (closeDst > 0.0f && closeDst < worldDst)
			{
				worldTgt = worldEye + worldDir * closeDst;
			}
		}

		public static bool IsValid(float occlusion, int layers, GameObject gameObject)
		{
			if (occlusion < 1.0f)
			{
				var mask = 1 << gameObject.layer;

				if ((mask & layers) != 0)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>This method performs the calculations. The <b>eye</b> and <b>tgt</b> arguments store the position in .xyz, and the radius in .w, both in world space.</summary>
		public static float Calculate(int layers, Vector4 eye, Vector4 tgt, float maxRaycastDistance)
		{
			var occlusion = 0.0f;

			CalculateRaycast(layers, eye, tgt, ref occlusion, maxRaycastDistance);

			if (occlusion < 1.0f)
			{
				SgtCommon.InvokeCalculateOcclusion(layers, eye, tgt, ref occlusion);
			}

			return Mathf.Clamp01(occlusion);
		}

		private static int CompareDistance(float a, float b)
		{
			return a.CompareTo(b);
		}

		private static System.Comparison<float> CompareDistanceDel = CompareDistance;

		private static void CalculateRaycast(int layers, Vector4 eye, Vector4 tgt, ref float occlusion, float maxRaycastDistance)
		{
			var distance = Vector3.Distance(eye, tgt);

			if (distance > 0.0f)
			{
				distances.Clear();

				TryScaleBackDistantPositions(ref eye, ref tgt, eye, maxRaycastDistance);

				DoRaycast(layers, eye, tgt, false);
				DoRaycast(layers, tgt, eye, true);

				if (distances.Count % 2 == 1)
				{
					distances.Add(distance);
				}

				distances.Sort(CompareDistanceDel);

				for (var i = 0; i < distances.Count; i += 2)
				{
					var a = distances[i    ];
					var b = distances[i + 1];
					var m = (a + b) * 0.5f;
					var f = m / distance;
					var p = Vector4.Lerp(eye, tgt, f);
					var t = b - a;

					if (p.w > 0.0f)
					{
						occlusion += t / p.w;
					}
				}
			}
		}

		private static void DoRaycast(int layers, Vector3 eye, Vector3 tgt, bool invert)
		{
			var direction = Vector3.Normalize(tgt - eye);
			var distance  = Vector3.Magnitude(tgt - eye);
			var hitCount  = Physics.RaycastNonAlloc(eye, direction, tempHits, distance, layers, QueryTriggerInteraction.Ignore);

			for (var i = 0; i < hitCount; i++)
			{
				if (invert == true)
				{
					distances.Add(distance - tempHits[i].distance);
				}
				else
				{
					distances.Add(tempHits[i].distance);
				}
			}
		}
	}
}