using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class contains some useful methods used by this asset.</summary>
	public static partial class SgtCommon
	{
		public delegate void DistanceDelegate(Vector3 worldPosition, ref float distance);

		public delegate void OcclusionDelegate(int layers, Vector4 worldEye, Vector4 worldTgt, ref float occlusion);

		public const string ShaderNamePrefix = "Hidden/Sgt";

		public const string HelpUrlPrefix = "https://carloswilkes.com/Documentation/SpaceGraphicsToolkit#";

		public const string ComponentMenuPrefix = "Space Graphics Toolkit/SGT ";

		public const string GameObjectMenuPrefix = "GameObject/Space Graphics Toolkit/";

		public static event System.Action<Vector3> OnSnap;

		public static event DistanceDelegate OnCalculateDistance;

		/// <summary>This event allows you to register a custom occlusion calculation.</summary>
		public static event OcclusionDelegate OnCalculateOcclusion;

		public static Stack<Mesh> MeshPool = new Stack<Mesh>();

		public static void InvokeCalculateDistance(Vector3 worldPosition, ref float distance)
		{
			if (OnCalculateDistance != null)
			{
				OnCalculateDistance.Invoke(worldPosition, ref distance);
			}
		}

		public static void InvokeCalculateOcclusion(int layers, Vector4 worldEye, Vector4 worldTgt, ref float occlusion)
		{
			if (OnCalculateOcclusion != null)
			{
				OnCalculateOcclusion.Invoke(layers, worldEye, worldTgt, ref occlusion);
			}
		}

		public static void InvokeSnap(Vector3 delta)
		{
			if (OnSnap != null)
			{
				OnSnap.Invoke(delta);
			}
		}

		public static bool CanDraw(GameObject gameObject, Camera camera)
		{
#if UNITY_EDITOR
	#if UNITY_2019_2_OR_NEWER
			if (UnityEditor.SceneVisibilityManager.instance.IsHidden(gameObject) == true)
			{
				foreach (UnityEditor.SceneView sceneView in UnityEditor.SceneView.sceneViews)
				{
					if (sceneView.camera == camera)
					{
						return false;
					}
				}
			}
	#endif

			if (camera.scene.name != null && gameObject.scene != camera.scene)
			{
				return false;
			}
#endif
			return true;
		}

		public static Bounds NewBoundsCenter(Bounds b, Vector3 c)
		{
			var x = Mathf.Max(Mathf.Abs(c.x - b.min.x), Mathf.Abs(c.x - b.max.x));
			var y = Mathf.Max(Mathf.Abs(c.y - b.min.z), Mathf.Abs(c.y - b.max.y));
			var z = Mathf.Max(Mathf.Abs(c.z - b.min.z), Mathf.Abs(c.z - b.max.z));

			return new Bounds(c, new Vector3(x, y, z) * 2.0f);
		}

		public static Bounds NewBoundsFromMinMax(Vector3 min, Vector3 max)
		{
			var bounds = default(Bounds);

			bounds.SetMinMax(min, max);

			return bounds;
		}

		public static void ExpandBounds(ref bool minMaxSet, ref Vector3 min, ref Vector3 max, Vector3 position, float radius)
		{
			var radius3 = new Vector3(radius, radius, radius);

			if (minMaxSet == false)
			{
				minMaxSet = true;

				min = position - radius3;
				max = position + radius3;
			}

			min = Vector3.Min(min, position - radius3);
			max = Vector3.Max(max, position + radius3);
		}

		public static void EnableKeyword(string keyword, Material material)
		{
			if (material != null)
			{
				if (material.IsKeywordEnabled(keyword) == false)
				{
					material.EnableKeyword(keyword);
				}
			}
		}

		public static void DisableKeyword(string keyword, Material material)
		{
			if (material != null)
			{
				if (material.IsKeywordEnabled(keyword) == true)
				{
					material.DisableKeyword(keyword);
				}
			}
		}

		public static Mesh CreateTempMesh(string meshName)
		{
			var mesh = SgtCommon.MeshPool.Count > 0 ? SgtCommon.MeshPool.Pop() : new Mesh();

			mesh.name      = meshName;
			mesh.hideFlags = HideFlags.DontSave;

			return mesh;
		}

		#pragma warning disable 649
		private static GradientAlphaKey[] tempAlphaKeys = new GradientAlphaKey[2];
		private static GradientColorKey[] tempColorKeys = new GradientColorKey[2];

		public static Gradient CreateGradient(Color color)
		{
			var gradient = new Gradient();

			tempAlphaKeys[0].time = 0.0f; tempAlphaKeys[0].alpha = 1.0f;
			tempAlphaKeys[1].time = 1.0f; tempAlphaKeys[1].alpha = 1.0f;

			tempColorKeys[0].time = 0.0f; tempColorKeys[0].color = color;
			tempColorKeys[1].time = 1.0f; tempColorKeys[1].color = color;

			gradient.SetKeys(tempColorKeys, tempAlphaKeys);

			return gradient;
		}

		public static float CubicInterpolate(float a, float b, float c, float d, float t)
		{
			var tt = t * t;
		
			d = (d - c) - (a - b);
		
			return d * (tt * t) + ((a - b) - d) * tt + (c - a) * t + b;
		}
	
		public static float HermiteInterpolate(float a, float b, float c, float d, float t)
		{
			var tt  = t * t;
			var tt3 = tt * 3.0f;
			var ttt = t * tt;
			var ttt2 = ttt * 2.0f;
			float a0, a1, a2, a3;
		
			var m0 = (c - a) * 0.5f;
			var m1 = (d - b) * 0.5f;
		
			a0  =  ttt2 - tt3 + 1.0f;
			a1  =  ttt  - tt * 2.0f + t;
			a2  =  ttt  - tt;
			a3  = -ttt2 + tt3;
		
			return a0*b + a1*m0 + a2*m1 + a3*c;
		}

		public static Color HermiteInterpolate(Color a, Color b, Color c, Color d, float t)
		{
			var tt  = t * t;
			var tt3 = tt * 3.0f;
			var ttt = t * tt;
			var ttt2 = ttt * 2.0f;
			float a0, a1, a2, a3;
		
			var m0 = (c - a) * 0.5f;
			var m1 = (d - b) * 0.5f;
		
			a0  =  ttt2 - tt3 + 1.0f;
			a1  =  ttt  - tt * 2.0f + t;
			a2  =  ttt  - tt;
			a3  = -ttt2 + tt3;
		
			return a0*b + a1*m0 + a2*m1 + a3*c;
		}

		public static Vector3 HermiteInterpolate3(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
		{
			var tt  = t * t;
			var tt3 = tt * 3.0f;
			var ttt = t * tt;
			var ttt2 = ttt * 2.0f;
			float a0, a1, a2, a3;
		
			var m0 = (c - a) * 0.5f;
			var m1 = (d - b) * 0.5f;
		
			a0  =  ttt2 - tt3 + 1.0f;
			a1  =  ttt  - tt * 2.0f + t;
			a2  =  ttt  - tt;
			a3  = -ttt2 + tt3;
		
			return a0*b + a1*m0 + a2*m1 + a3*c;
		}

		public static void ClearCapacity<T>(List<T> list, int minCapacity)
		{
			if (list != null)
			{
				list.Clear();

				if (list.Capacity < minCapacity)
				{
					list.Capacity = minCapacity;
				}
			}
		}

		public static void CalculateHorizonThickness(float innerRadius, float middleRadius, float distance, out float innerThickness, out float outerThickness)
		{
			if (distance < innerRadius)
			{
				distance = innerRadius;
			}

			var horizonOuterDistance = Mathf.Sin(Mathf.Acos(innerRadius));
			var horizonDistance      = Mathf.Min(Mathf.Sqrt(distance * distance - innerRadius * innerRadius), horizonOuterDistance);

			outerThickness = horizonDistance + horizonOuterDistance;
			//innerThickness = horizonDistance;

			if (distance < middleRadius)
			{
				distance = middleRadius;
			}

			horizonDistance = Mathf.Min(Mathf.Sqrt(distance * distance - innerRadius * innerRadius), horizonOuterDistance);

			innerThickness = horizonDistance;
		}

		public static float GetBoundsRadius(Bounds b)
		{
			var min = b.min;
			var max = b.max;
			var avg = Mathf.Abs(min.x) + Mathf.Abs(min.y) + Mathf.Abs(min.z) + Mathf.Abs(max.x) + Mathf.Abs(max.y) + Mathf.Abs(max.z);

			return avg / 6.0f;
		}

		// This will begin a new need based on a seed and transform it based on a grid cell hash that tries to minimize visible symmetry
		public static int GetRandomSeed(int newSeed, long x, long y, long z)
		{
			var a = 1103515245;
			var b = 12345;

			newSeed = (int)(a * (newSeed + x) + b) % int.MaxValue;
			newSeed = (int)(a * (newSeed + y) + b) % int.MaxValue;
			newSeed = (int)(a * (newSeed + z) + b) % int.MaxValue;

			return newSeed;
		}

		public static void BeginRandomSeed(int newSeed, long x, long y, long z)
		{
			CwHelper.BeginSeed(GetRandomSeed(newSeed, x, y, z));
		}

		public static Vector4 CalculateSpriteUV(Sprite s)
		{
			var uv = default(Vector4);

			if (s != null)
			{
				var r = s.textureRect;
				var t = s.texture;

				uv.x = CwHelper.Divide(r.xMin, t.width);
				uv.y = CwHelper.Divide(r.yMin, t.height);
				uv.z = CwHelper.Divide(r.xMax, t.width);
				uv.w = CwHelper.Divide(r.yMax, t.height);
			}

			return uv;
		}

		public static Vector4 NewVector4(Vector3 xyz, float w)
		{
			return new Vector4(xyz.x, xyz.y, xyz.z, w);
		}

		public static Matrix4x4 ShearingZ(Vector2 xy) // Z changes with x/y
		{
			var matrix = Matrix4x4.identity;

			matrix.m20 = xy.x;
			matrix.m21 = xy.y;

			return matrix;
		}

		// return.x = -PI   .. +PI
		// return.y = -PI/2 .. +PI/2
		public static Vector2 CartesianToPolar(Vector3 xyz)
		{
			var longitude = Mathf.Atan2(xyz.x, xyz.z);
			var latitude  = Mathf.Asin(xyz.y / xyz.magnitude);

			return new Vector2(longitude, latitude);
		}

		// return.x = 0 .. 1
		// return.y = 0 .. 1
		public static Vector2 CartesianToPolarUV(Vector3 xyz)
		{
			var uv = CartesianToPolar(xyz);

			uv.x = Mathf.Repeat(-0.25f - uv.x / (Mathf.PI * 2.0f), 1.0f);
			uv.y = 0.5f + uv.y / Mathf.PI;

			return uv;
		}

		public static int GetStride(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.Alpha8: return 1;
				case TextureFormat.RGB24: return 3;
				case TextureFormat.RGBA32: return 4;
				case TextureFormat.ARGB32: return 4;
				case TextureFormat.BGRA32: return 4;
				case TextureFormat.R8: return 1;
			}

			return 0;
		}

		/// <summary>Gets the byte offset of a texture channel based on its format.
		/// Channel = 0 = Red.
		/// Channel = 1 = Green.
		/// Channel = 2 = Blue.
		/// Channel = 3 = Alpha.</summary>
		public static int GetOffset(TextureFormat format, int channel)
		{
			switch (format)
			{
				case TextureFormat.RGB24:
					switch (channel)
					{
						case 0: return 0;
						case 1: return 1;
						case 2: return 2;
					}
				break;
				case TextureFormat.RGBA32:
					switch (channel)
					{
						case 0: return 0;
						case 1: return 1;
						case 2: return 2;
						case 3: return 3;
					}
				break;
				case TextureFormat.ARGB32:
					switch (channel)
					{
						case 0: return 1;
						case 1: return 2;
						case 2: return 3;
						case 3: return 0;
					}
				break;
				case TextureFormat.BGRA32:
					switch (channel)
					{
						case 0: return 2;
						case 1: return 1;
						case 2: return 0;
						case 3: return 3;
					}
				break;
			}

			return 0;
		}

		public static void UpdateNativeArray<T>(ref NativeArray<T> array, int length)
			where T : struct
		{
			if (array.IsCreated == true && array.Length != length)
			{
				array.Dispose();
			}

			if (array.IsCreated == false)
			{
				array = new NativeArray<T>(length, Allocator.Persistent);
			}
		}

#if __BURST__
	#if UNITY_2019_3_OR_NEWER
		public static NativeArray<Unity.Mathematics.float2> ConvertNativeArray(NativeArray<Unity.Mathematics.float2> nativeArray)
		{
			return nativeArray;
		}
		public static NativeArray<Unity.Mathematics.float3> ConvertNativeArray(NativeArray<Unity.Mathematics.float3> nativeArray)
		{
			return nativeArray;
		}
		public static NativeArray<Unity.Mathematics.float4> ConvertNativeArray(NativeArray<Unity.Mathematics.float4> nativeArray)
		{
			return nativeArray;
		}
		public static NativeArray<int> ConvertNativeArray(NativeArray<int> nativeArray)
		{
			return nativeArray;
		}
		public static NativeArray<Color32> ConvertNativeArray(NativeArray<Color32> nativeArray)
		{
			return nativeArray;
		}
	#endif
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;

	public static partial class SgtCommon
	{
		private static List<Component> tempComponents = new List<Component>();

		public static void RequireCamera()
		{
			if (SgtCamera.Instances.Count == 0)
			{
				CwEditor.Separator();

				if (CwEditor.HelpButton("Your scene contains no SgtCameras", MessageType.Error, "Fix", 50.0f) == true)
				{
					CwHelper.ClearSelection();

					foreach (var camera in Camera.allCameras)
					{
						CwHelper.AddToSelection(camera.gameObject);

						CwHelper.GetOrAddComponent<SgtCamera>(camera.gameObject);
					}
				}
			}
		}

		public static void DestroyOldGameObjects(Transform parent, string name)
		{
			while (TryDestroyOldGameObject(parent, name))
			{
				Debug.Log("SGT Upgrade: Destroyed old " + name, parent);
			}
		}

		public static bool TryDestroyOldGameObject(Transform parent, string name)
		{
			foreach (Transform child in parent)
			{
				if (child.name == name)
				{
					child.GetComponents(tempComponents);

					foreach (var component in tempComponents)
					{
						if (component == null)
						{
							Undo.RecordObject(parent, "Removing " + name);

							Undo.DestroyObjectImmediate(child.gameObject);

							UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(parent.gameObject.scene);

							return true;
						}
					}
				}
			}

			return false;
		}

		public static void DrawSphere(Vector3 center, Vector3 right, Vector3 up, Vector3 forward, int resolution = 32)
		{
			DrawCircle(center, right, up, resolution);
			DrawCircle(center, right, forward, resolution);
			DrawCircle(center, forward, up, resolution);
		}

		public static void DrawCircle(Vector3 center, Vector3 right, Vector3 up, int resolution = 32)
		{
			var step = CwHelper.Reciprocal(resolution);

			for (var i = 0; i < resolution; i++)
			{
				var a = i * step;
				var b = a + step;

				a = a * Mathf.PI * 2.0f;
				b = b * Mathf.PI * 2.0f;

				Gizmos.DrawLine(center + right * Mathf.Sin(a) + up * Mathf.Cos(a), center + right * Mathf.Sin(b) + up * Mathf.Cos(b));
			}
		}

		public static void DrawCircle(Vector3 center, Vector3 axis, float radius, int resolution = 32)
		{
			var rotation = Quaternion.FromToRotation(Vector3.up, axis);
			var right    = rotation * Vector3.right   * radius;
			var forward  = rotation * Vector3.forward * radius;

			DrawCircle(center, right, forward, resolution);
		}
	}
}
#endif