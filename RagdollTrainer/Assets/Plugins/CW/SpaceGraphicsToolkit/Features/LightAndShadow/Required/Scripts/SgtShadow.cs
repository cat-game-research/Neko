using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.LightAndShadow
{
	/// <summary>This base class handles calculation of a shadow matrix and shadow texture.</summary>
	public abstract class SgtShadow : MonoBehaviour
	{
		public struct Clone
		{
			public SgtShadow Root;

			public Matrix4x4 Matrix;

			public Vector4 Power;

			public float Ratio;

			public float Radius;
		}

		public const int MAX_SPHERE_SHADOWS = 16;

		public const int MAX_RING_SHADOWS = 1;

		private static List<Clone> tempShadows = new List<Clone>();

		public abstract bool CalculateShadow(SgtLight light, ref Clone clone);

		private static LinkedList<SgtShadow> instances = new LinkedList<SgtShadow>();

		[System.NonSerialized]
		private LinkedListNode<SgtShadow> node;

		private static int _SGT_SphereShadowCount  = Shader.PropertyToID("_SGT_SphereShadowCount");
		private static int _SGT_SphereShadowMatrix = Shader.PropertyToID("_SGT_SphereShadowMatrix");
		private static int _SGT_SphereShadowPower  = Shader.PropertyToID("_SGT_SphereShadowPower");
		private static int _SGT_RingShadowCount    = Shader.PropertyToID("_SGT_RingShadowCount");
		private static int _SGT_RingShadowTexture  = Shader.PropertyToID("_SGT_RingShadowTexture");
		private static int _SGT_RingShadowMatrix   = Shader.PropertyToID("_SGT_RingShadowMatrix");
		private static int _SGT_RingShadowRatio    = Shader.PropertyToID("_SGT_RingShadowRatio");

		public static List<Clone> Find(bool lit, int mask, List<SgtLight> lights)
		{
			tempShadows.Clear();

			if (lit == true && lights != null && lights.Count > 0)
			{
				foreach (var light in lights)
				{
					foreach (var shadow in instances)
					{
						var mask2 = 1 << shadow.gameObject.layer;

						if ((mask & mask2) != 0)
						{
							//if (shadow.calculatedThisFrame == false)
							//{
							//	shadow.calculatedThisFrame = true;

								var clone = default(Clone);

								if (shadow.CalculateShadow(light, ref clone) == true)
								{
									tempShadows.Add(clone);
								}
							//}
						}
					}
				}
			}

			return tempShadows;
		}

		public static void FilterOutSphere(Vector3 center)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				if (tempShadow.Root is SgtShadowSphere && tempShadow.Root.transform.position == center)
				{
					tempShadows.RemoveAt(i);
				}
			}
		}

		public static void FilterOutRing(Vector3 center)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				if (tempShadow.Root is SgtShadowRing && tempShadow.Root.transform.position == center)
				{
					tempShadows.RemoveAt(i);
				}
			}
		}

		public static void FilterOutMiss(Vector3 center, float radius)
		{
			for (var i = tempShadows.Count - 1; i >= 0; i--)
			{
				var tempShadow = tempShadows[i];

				// Skip if overlapping
				if (Vector3.Distance(center, tempShadow.Root.transform.position) > radius + tempShadow.Radius)
				{
					var point = tempShadow.Matrix.MultiplyPoint(center);

					if (point.z > 0.0f)
					{
						var distance = Mathf.Sqrt(point.x * point.x + point.y * point.y);

						if (distance * tempShadow.Radius <= radius + tempShadow.Radius)
						{
							continue;
						}
					}

					tempShadows.RemoveAt(i);
				}
			}
		}

		private static Matrix4x4[] tempSphereMatrix = new Matrix4x4[MAX_SPHERE_SHADOWS];
		private static Vector4[]   tempSpherePower  = new Vector4[MAX_SPHERE_SHADOWS];

		public static void WriteSphere(int maxShadows)
		{
			var shadowCount = 0;

			for (var i = 0; i < tempShadows.Count && shadowCount < maxShadows; i++)
			{
				var shadow = tempShadows[i];

				if (shadow.Root is SgtShadowSphere)
				{
					tempSphereMatrix[shadowCount] = shadow.Matrix;
					tempSpherePower[shadowCount] = shadow.Power;

					shadowCount += 1;
				}
			}

			foreach (var tempMaterial in CwHelper.tempMaterials)
			{
				tempMaterial.SetInt(_SGT_SphereShadowCount, shadowCount);

				if (shadowCount > 0)
				{
					tempMaterial.SetMatrixArray(_SGT_SphereShadowMatrix, tempSphereMatrix);
					tempMaterial.SetVectorArray(_SGT_SphereShadowPower, tempSpherePower);
				}
			}

			foreach (var tempProperties in CwHelper.tempProperties)
			{
				tempProperties.SetInt(_SGT_SphereShadowCount, shadowCount);

				if (shadowCount > 0)
				{
					tempProperties.SetMatrixArray(_SGT_SphereShadowMatrix, tempSphereMatrix);
					tempProperties.SetVectorArray(_SGT_SphereShadowPower, tempSpherePower);
				}
			}
		}

		private static Texture     tempRingTexture;
		private static Matrix4x4[] tempRingMatrix = new Matrix4x4[MAX_RING_SHADOWS];
		private static float[]     tempRingRatio  = new float[MAX_RING_SHADOWS];

		public static void WriteRing(int maxShadows)
		{
			var shadowCount = 0;

			for (var i = 0; i < tempShadows.Count && shadowCount < maxShadows; i++)
			{
				var shadow     = tempShadows[i];
				var shadowRing = shadow.Root as SgtShadowRing;

				if (shadowRing != null)
				{
					tempRingTexture = shadowRing.Texture;
					tempRingMatrix[shadowCount] = shadow.Matrix;
					tempRingRatio[shadowCount] = shadow.Ratio;

					shadowCount += 1;
				}
			}

			foreach (var tempMaterial in CwHelper.tempMaterials)
			{
				tempMaterial.SetInt(_SGT_RingShadowCount, shadowCount);

				if (shadowCount > 0)
				{
					tempMaterial.SetTexture(_SGT_RingShadowTexture, tempRingTexture);
					tempMaterial.SetMatrixArray(_SGT_RingShadowMatrix, tempRingMatrix);
					tempMaterial.SetFloatArray(_SGT_RingShadowRatio, tempRingRatio);
				}
			}

			foreach (var tempProperties in CwHelper.tempProperties)
			{
				tempProperties.SetInt(_SGT_RingShadowCount, shadowCount);

				if (shadowCount > 0)
				{
					tempProperties.SetTexture(_SGT_RingShadowTexture, tempRingTexture);
					tempProperties.SetMatrixArray(_SGT_RingShadowMatrix, tempRingMatrix);
					tempProperties.SetFloatArray(_SGT_RingShadowRatio, tempRingRatio);
				}
			}
		}

		protected virtual void OnEnable()
		{
			node = instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			instances.Remove(node);
			
			node = null;
		}
	}
}