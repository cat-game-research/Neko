using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to create a sphere mesh that will dynamically increase in detail as the camera (or other GameObject) approaches the surface.
	/// NOTE: You must use one of the <b>SgtTerrain___Material</b> components to actually render the mesh.</summary>
	public abstract class SgtTerrain : MonoBehaviour
	{
		/// <summary>This event gives you the currently generating mesh points and heights, allowing you to deform them in a custom way.</summary>
		public interface IModifiesHeights
		{
			void HandleScheduleHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle);
		}

		/// <summary>This works like <b>OnScheduleHeights</b>, except all terrain height modifiers register to this, even if they generate additional data.</summary>
		public interface IModifiesCombinedHeights
		{
			void HandleScheduleCombinedHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle);
		}

		/// <summary>The distance between the center and edge of the planet in local space before it's deformed.</summary>
		public double Radius { set { if (radius != value) { radius = value; MarkAsDirty(); } } get { return radius; } } [SerializeField] protected double radius = 1.0;

		/// <summary>When at the surface of the planet, how big should each triangle be?
		/// NOTE: This is an approximation. The final size of the triangle will depend on your planet radius, and will be a power of two.</summary>
		public double SmallestTriangleSize { set { smallestTriangleSize = value; } get { return smallestTriangleSize; } } [SerializeField] private double smallestTriangleSize = 1.0;

		/// <summary>The base resolution of the planet before LOD is used.</summary>
		public long Resolution { set { if (resolution != value) { resolution = value; MarkAsDirty(); } } get { return resolution; } } [SerializeField] protected long resolution = 128;

		/// <summary>The higher this value, the more triangles will be in view.</summary>
		public double Detail { set { detail = value; } get { return detail; } } [Range(0.2f, 5.0f)] [SerializeField] protected double detail = 1.0;

		/// <summary>If you enable this then the mesh will automatically update in edit mode.
		/// NOTE: Enabling this may cause your editor to run slow.</summary>
		public bool AutoPreview { set { autoPreview = value; } get { return autoPreview; } } [SerializeField] private bool autoPreview = true;

		/// <summary>The LOD will be based on the distance to this <b>Transform</b>.
		/// None = Main Camera.</summary>
		public Transform Observer { set { observer = value; } get { return observer; } } [SerializeField] private Transform observer;

		/// <summary>This allows you to control which areas certain terrain features appear on the mesh.</summary>
		public SgtTerrainAreas Areas { set { areas = value; } get { return areas; } } [SerializeField] private SgtTerrainAreas areas;

		/// <summary>This event tells you when a mesh surface is scheduled to update, allowing you prepare data for this new configuration.</summary>
		public event System.Action<SgtTerrainQuad> OnScheduleQuad;

		/// <summary>This event tells you when the terrain has finished updating.</summary>
		public event System.Action OnSwap;

		/// <summary>This event tells you when a mesh surface should render, allowing you specify a custom material.
		/// int = Render layer.</summary>
		public event System.Action<Camera, SgtTerrainQuad, Matrix4x4, int> OnDrawQuad;

		public event System.Action OnDisabled;

		private bool dirty;

		private LinkedListNode<SgtTerrain> node;

		private List<double> distances = new List<double>();

		private List<SgtTerrainCube> cubes = new List<SgtTerrainCube>();

		private Stack<SgtTerrainQuad> pendingQuads = new Stack<SgtTerrainQuad>();

		private NativeArray<double3> onePoint;
		private NativeArray<double>  oneHeight;

		private static LinkedList<SgtTerrain> instances = new LinkedList<SgtTerrain>();

		private static Stack<Mesh> meshPool = new Stack<Mesh>();

		private List<System.IDisposable> pendingDisposal = new List<System.IDisposable>();

		private List<IModifiesHeights> heightModifiers = new List<IModifiesHeights>();

		private List<IModifiesCombinedHeights> combinedHeightModifiers = new List<IModifiesCombinedHeights>();

		public static void DespawnMesh(Mesh mesh)
		{
			meshPool.Push(mesh);
		}

		public static Mesh SpawnMesh()
		{
			while (meshPool.Count > 0)
			{
				var mesh = meshPool.Pop();

				if (mesh != null)
				{
					return mesh;
				}
			}

			return new Mesh();
		}

		public void ScheduleDispose(System.IDisposable disposable)
		{
			if (disabled == true)
			{
				disposable.Dispose();
			}
			else
			{
				pendingDisposal.Add(disposable);
			}
		}

		/// <summary>This method will cause the whole terrain mesh to be rebuilt.</summary>
		[ContextMenu("Mark As Dirty")]
		public void MarkAsDirty()
		{
			dirty = true;

			heightModifiers.Clear();
			combinedHeightModifiers.Clear();
		}

		/// <summary>This method returns the nearest <b>SgtTerrain</b> to the specified world point.</summary>
		public static SgtTerrain FindNearest(Vector3 worldPosition)
		{
			var bestDistance = double.PositiveInfinity;
			var bestTerrain  = default(SgtTerrain);

			foreach (var terrain in instances)
			{
				var distance = (double)Vector3.Distance(worldPosition, terrain.transform.position);

				distance = math.max(0.0, distance - terrain.radius);

				if (distance < bestDistance)
				{
					bestDistance = distance;
					bestTerrain  = terrain;
				}
			}

			return bestTerrain;
		}

		public float3 TransformPoint(double3 local)
		{
			var world = math.mul(new double4x4(transform.localToWorldMatrix), new double4(local, 1.0)); return new float3((float)world.x, (float)world.y, (float)world.z);
		}

		public double3 InverseTransformPoint(float3 world)
		{
			var local = math.mul(new double4x4(transform.worldToLocalMatrix), new double4(world, 1.0)); return new double3(local.x, local.y, local.z);
		}

		public float3 TransformVector(double3 local)
		{
			var world = math.mul(new double4x4(transform.localToWorldMatrix), new double4(local, 0.0)); return new float3((float)world.x, (float)world.y, (float)world.z);
		}

		public double3 InverseTransformVector(float3 world)
		{
			var local = math.mul(new double4x4(transform.worldToLocalMatrix), new double4(world, 0.0)); return new double3(local.x, local.y, local.z);
		}

		public float3 GetWorldPoint(float3 worldPoint)
		{
			var localPoint = InverseTransformPoint(worldPoint);

			localPoint = GetLocalPoint(localPoint);

			return TransformPoint(localPoint);
		}

		private static Vector3 GetNormal(float3 vectorA, float3 vectorB, float length)
		{
			var smallsq = length * 0.1f; smallsq *= smallsq;

			if (math.lengthsq(vectorA) < smallsq)
			{
				return vectorB;
			}

			if (math.lengthsq(vectorB) < smallsq)
			{
				return vectorA;
			}

			return -math.cross(vectorA, vectorB);
		}

		public float3 GetWorldNormal(float3 worldPoint, float3 worldRight, float3 worldForward)
		{
			var sampledPointL = GetWorldPoint(worldPoint - worldRight  );
			var sampledPointR = GetWorldPoint(worldPoint + worldRight  );
			var sampledPointB = GetWorldPoint(worldPoint - worldForward);
			var sampledPointF = GetWorldPoint(worldPoint + worldForward);

			var vectorA = sampledPointR - sampledPointL;
			var vectorB = sampledPointF - sampledPointB;

			var sampledNormal = math.normalize(GetNormal(vectorA, vectorB, math.length(worldRight)));

			if (math.dot(sampledNormal, sampledPointL - (float3)transform.position) < 0.0f)
			{
				sampledNormal = -sampledNormal;
			}

			return sampledNormal;
		}

		public double GetLocalHeight(double3 localPoint)
		{
			var handle = default(JobHandle);

			onePoint[0] = math.normalize(localPoint);
			oneHeight[0] = radius;

			InvokeScheduleCombinedHeights(onePoint, oneHeight, ref handle);

			handle.Complete();

			return oneHeight[0];
		}

		/// <summary>This method will snap the specified position to the surface of the terrain in local space.</summary>
		public double3 GetLocalPoint(double3 localPoint)
		{
			return math.normalize(localPoint) * GetLocalHeight(localPoint);
		}

		public void InvokeScheduleCombinedHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			if (heightModifiers.Count == 0)
			{
				GetComponents(heightModifiers);
			}

			foreach (var modifier in heightModifiers)
			{
				modifier.HandleScheduleHeights(points, heights, ref handle);
			}
		}

		public void InvokeScheduleHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			if (combinedHeightModifiers.Count == 0)
			{
				GetComponents(combinedHeightModifiers);
			}

			foreach (var modifier in combinedHeightModifiers)
			{
				modifier.HandleScheduleCombinedHeights(points, heights, ref handle);
			}
		}

		/// <summary>This method tells you the LOD level where triangles are closest to the specified triangle size.</summary>
		public int GetDepth(double triangleSize)
		{
			var size = resolution * triangleSize;

			if (size > 0.0)
			{
				var depth = (int)math.log2(radius / size);

				return math.clamp(depth, 0, 32);
			}

			return 0;
		}

		/// <summary>This method allows you to force the mesh to finish updating immediately.
		/// NOTE: This may be slow.</summary>
		[ContextMenu("Complete")]
		public void Complete()
		{
			for (var i = 0; i < 10000; i++)
			{
				UpdateJob();

				UpdateState();
				
				// Finished?
				if (pendingQuads.Count == 0 && IsRunning == false)
				{
					break;
				}

				UpdatePending();
			}
		}

		protected virtual void LateUpdate()
		{
			if (Application.isPlaying == true)
			{
				UpdateJob();

				UpdateState();

				UpdatePending();
			}
			else if (autoPreview == true)
			{
				Complete();
			}
		}

		[System.NonSerialized]
		private bool disabled = true;

		protected virtual void OnEnable()
		{
			disabled = false;

			SgtCamera.OnCameraDraw += HandleCameraDraw;

			SgtCommon.OnCalculateDistance += HandleCalculateDistance;

			MarkAsDirty();

			node      = instances.AddLast(this);
			onePoint  = new NativeArray<double3>(1, Allocator.Persistent);
			oneHeight = new NativeArray<double>(1, Allocator.Persistent);

			foreach (var cube in cubes)
			{
				Allocate(cube);
			}
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;

			SgtCommon.OnCalculateDistance -= HandleCalculateDistance;

			instances.Remove(node); node = null;

			if (OnDisabled != null)
			{
				OnDisabled.Invoke();
			}

			onePoint.Dispose();
			oneHeight.Dispose();

			pendingQuads.Clear();

			foreach (var cube in cubes)
			{
				Dispose(cube);
			}

			foreach (var disposable in pendingDisposal)
			{
				disposable.Dispose();
			}

			pendingDisposal.Clear();

			disabled = true;
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			MarkAsDirty();
		}

		protected abstract bool IsRunning
		{
			get;
		}

		protected abstract void UpdateJob();

		protected abstract void ScheduleJob(SgtTerrainQuad quad);

		private void HandleCalculateDistance(Vector3 worldPosition, ref float distance)
		{
			var surfacePoint    = GetWorldPoint(worldPosition);
			var surfaceDistance = math.distance(surfacePoint, worldPosition);

			if (surfaceDistance < distance)
			{
				distance = surfaceDistance;
			}
		}

		private void UpdateState()
		{
			if (pendingQuads.Count == 0 && IsRunning == false)
			{
				foreach (var cube in cubes)
				{
					foreach (var quad in cube.Quads)
					{
						quad.Swap();
					}
				}

				if (OnSwap != null)
				{
					OnSwap.Invoke();
				}

				UpdateLists();
				UpdateLod();
			}
		}

		private void UpdatePending()
		{
			if (pendingQuads.Count > 0 && IsRunning == false)
			{
				var quad = pendingQuads.Pop();

				if (OnScheduleQuad != null)
				{
					OnScheduleQuad.Invoke(quad);
				}

				ScheduleJob(quad);
			}
		}

		private void UpdateLists()
		{
			var distanceCount = GetDepth(smallestTriangleSize);

			if (distanceCount > distances.Count)
			{
				for (var i = distances.Count; i < distanceCount; i++)
				{
					distances.Add(0.0);
				}
			}
			else if (distances.Count > distanceCount)
			{
				for (var i = distances.Count - 1; i >= distanceCount; i--)
				{
					distances.RemoveAt(i);
				}
			}

			var cubeCount = distanceCount + 1;

			if (cubeCount > cubes.Count)
			{
				for (var i = cubes.Count; i < cubeCount; i++)
				{
					var cube = new SgtTerrainCube(this);

					cubes.Add(cube);

					Allocate(cube);
				}
			}
			else if (cubes.Count > cubeCount)
			{
				for (var i = cubes.Count - 1; i >= cubeCount; i--)
				{
					Dispose(cubes[i]);

					cubes.RemoveAt(i);
				}
			}

			for (var i = 0; i < cubeCount; i++)
			{
				cubes[i].Setup(i, resolution << i);
			}
		}

		private void Allocate(SgtTerrainCube cube)
		{
			foreach (var quad in cube.Quads)
			{
				Allocate(quad);
			}
		}

		protected virtual void Allocate(SgtTerrainQuad quad)
		{
			quad.Allocate();
		}

		private void Dispose(SgtTerrainCube cube)
		{
			foreach (var quad in cube.Quads)
			{
				Dispose(quad);
			}
		}

		protected virtual void Dispose(SgtTerrainQuad quad)
		{
			quad.Dispose();
		}

		private void UpdateLod()
		{
			var spherePoint = GetSpherePoint();

			for (var i = 0; i < cubes.Count; i++)
			{
				var cube       = cubes[i];
				var cubeBounds = GetCubeBound(i, spherePoint, cube.Middle);
				var cubeLimits = new SgtLongBounds(0, 0, 0, cube.Middle);

				for (var j = 0; j < 6; j++)
				{
					var quad       = cube.Quads[j];
					var quadBounds = SgtTerrainTopology.GetQuadBounds(j, cubeBounds);

					if (i == 0)
					{
						quad.PendingOuter = new SgtLongBounds(0, 0, 0, cube.Middle);
						quad.VirtualOuter = new SgtLongBounds(0, 0, 0, cube.Middle * 2);
					}
					else
					{
						var prevCube       = cubes[i - 1];
						var prevQuad       = prevCube.Quads[j];
						var prevResolution = prevCube.Resolution;

						quad.PendingOuter = prevQuad.PendingInner * 2;
						quad.VirtualOuter = prevQuad.VirtualInner * 2;
					}

					if (quadBounds.minZ <= cube.Middle && quadBounds.maxZ >= cube.Middle)
					{
						quad.VirtualInner = quadBounds;
					}
					else
					{
						quad.VirtualInner = default(SgtLongBounds);
					}

					quad.PendingInner = quad.VirtualInner;
					quad.PendingInner.ClampTo(cubeLimits);

					if (quad.PendingOuter.SizeX < 0 || quad.PendingOuter.SizeY < 0 || quad.PendingOuter.SizeZ < 0)
					{
						quad.PendingOuter = default(SgtLongBounds);
						quad.PendingInner = default(SgtLongBounds);
					}

					var forceUpdate = false;

					if (dirty == true)
					{
						if (quad.CurrentOuter.Volume > 0 || quad.CurrentInner.Volume > 0)
						{
							forceUpdate = true;

							quad.CurrentOuter.Clear();
							quad.CurrentInner.Clear();
						}
					}

					if (quad.CurrentOuter.Volume == 0) quad.CurrentOuter = default(SgtLongBounds);
					if (quad.PendingOuter.Volume == 0) quad.PendingOuter = default(SgtLongBounds);
					if (quad.CurrentInner.Volume == 0) quad.CurrentInner = default(SgtLongBounds);
					if (quad.PendingInner.Volume == 0) quad.PendingInner = default(SgtLongBounds);

					if (quad.CurrentOuter != quad.PendingOuter || quad.CurrentInner != quad.PendingInner || forceUpdate == true)
					{
						pendingQuads.Push(quad);
					}
				}
			}

			dirty = false;
		}

		private SgtLongBounds GetCubeBound(int depth, double3 point, long middle)
		{
			if (depth < distances.Count)
			{
				var size   = middle * distances[depth];
				var center = middle * point;

				return new SgtLongBounds((long)center.x, (long)center.y, (long)center.z, (long)size);
			}

			return default(SgtLongBounds);
		}

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtCommon.CanDraw(gameObject, camera) == false) return;

			if (OnDrawQuad != null)
			{
				var matrix = transform.localToWorldMatrix;

				foreach (var cube in cubes)
				{
					foreach (var quad in cube.Quads)
					{
						if (quad.Points.Length > 0)
						{
							var matrix2 = matrix * Matrix4x4.Translate(quad.CurrentCorner);

							OnDrawQuad.Invoke(camera, quad, matrix2, gameObject.layer);
						}
					}
				}
			}
		}

		private Vector3 WorldObserver
		{
			get
			{
				if (observer != null)
				{
					return observer.position;
				}

				var mainCamera = Camera.main;

				if (mainCamera != null)
				{
					return mainCamera.transform.position;
				}

				return transform.position;
			}
		}

		public double3 GetSpherePoint()
		{
			var localPos    = (double3)(float3)transform.InverseTransformPoint(WorldObserver);
			var localAlt    = math.length(localPos);
			var localHeight = GetLocalHeight(localPos);
			var localDist   = math.abs(localAlt - localHeight) / radius;

			for (var i = 0; i < distances.Count; i++)
			{
				var distance = detail * math.pow(0.5, 1 + i);
				var frac     = math.saturate(localDist / distance);

				distance *= math.cos(math.asin(frac));

				distances[i] = distance;
			}

			return SgtTerrainTopology.Unwarp(SgtTerrainTopology.VectorToUnitCube(SgtTerrainTopology.Untilt(localPos)));
		}

		public double3 GetObserverLocalPosition()
		{
			return (float3)transform.InverseTransformPoint(WorldObserver);
		}

		public double3 GetAboveGroundObserverCubePoint()
		{
			var localPos    = (double3)(float3)transform.InverseTransformPoint(WorldObserver);
			var localAlt    = math.length(localPos);
			var localHeight = GetLocalHeight(localPos);
			var cubePoint   = SgtTerrainTopology.VectorToUnitCube(SgtTerrainTopology.Untilt(localPos));

			if (localAlt > localHeight && localHeight > 0.0)
			{
				cubePoint *= localAlt / localHeight;
			}

			return cubePoint;
		}

		[BurstCompile]
		protected struct IndicesJob : IJob
		{
			public SgtLongRect      Inner;
			public SgtLongRect      Outer;
			public NativeArray<int> Indices;
			public bool             Invert;

			public void Execute()
			{
				var points = (int)(Outer.maxX - Outer.minX + 1);
				var index  = 0;

				for (var y = Outer.minY; y < Outer.maxY; y++)
				{
					for (var x = Outer.minX; x < Outer.maxX; x++)
					{
						if (x < Inner.minX || x >= Inner.maxX || y < Inner.minY || y >= Inner.maxY)
						{
							var h = (int)(x - Outer.minX);
							var v = (int)(y - Outer.minY);
							var a = h + v * points;
							var b = a + 1;
							var c = a + points;
							var d = c + 1;

							if (Invert == true)
							{
								Indices[index++] = a; Indices[index++] = b; Indices[index++] = c;
								Indices[index++] = d; Indices[index++] = c; Indices[index++] = b;
							}
							else
							{
								Indices[index++] = a; Indices[index++] = c; Indices[index++] = b;
								Indices[index++] = d; Indices[index++] = b; Indices[index++] = c;
							}
						}
						else
						{
							if (Invert == true)
							{
								Indices[index++] = 0; Indices[index++] = 0; Indices[index++] = 0;
								Indices[index++] = 0; Indices[index++] = 0; Indices[index++] = 0;
							}
							else
							{
								Indices[index++] = 0; Indices[index++] = 0; Indices[index++] = 0;
								Indices[index++] = 0; Indices[index++] = 0; Indices[index++] = 0;
							}
						}
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtTerrain;

	public abstract class SgtTerrain_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			BeginError(Any(tgts, t => t.Radius <= 0.0));
				Draw("radius", ref markAsDirty, "The distance between the center and edge of the planet in local space before it's deformed.");
			EndError();
			Draw("smallestTriangleSize", "When at the surface of the planet, how big should each triangle be?\n\nNOTE: This is an approximation. The final size of the triangle will depend on your planet radius, and will be a power of two.");
			Draw("resolution", ref markAsDirty, "The base resolution of the planet before LOD is used.");
			Draw("detail", "The higher this value, the more triangles will be in view.");
			Draw("observer", "The LOD will be based on the distance to this Transform.\n\nNone = Main Camera.");
			Draw("areas", ref markAsDirty, "This allows you to control which areas certain terrain features appear on the mesh.");
			Draw("autoPreview", "If you enable this then the mesh will automatically update in edit mode.\n\nNOTE: Enabling this may cause your editor to run slow.");

			if (Any(tgts, t => t.AutoPreview == false))
			{
				if (GUILayout.Button("Complete (Preview Mesh)") == true)
				{
					Each(tgts, t => t.Complete(), true);
				}
			}

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty());
			}
		}

		public static bool DrawArea(UnityEditor.SerializedProperty property, SgtTerrain terrain)
		{
			var names    = GetAreaNames(terrain);
			var modified = false;
			var content  = new GUIContent("Area", "This allows you to control where this biome appears based on the SgtTerrainTerra component's Biomes splatmap.");

			UnityEditor.EditorGUI.BeginChangeCheck();

			var newValue = UnityEditor.EditorGUILayout.Popup(content, property.intValue + 1, names) - 1;

			if (UnityEditor.EditorGUI.EndChangeCheck() == true)
			{
				property.intValue = newValue;

				modified = true;
			}

			return modified;
		}

		private static string[] GetAreaNames(SgtTerrain terrain)
		{
			var names = new List<string>();

			names.Add("Everywhere");

			if (terrain.Areas != null)
			{
				foreach (var splat in terrain.Areas.Splats)
				{
					names.Add(splat.Name);
				}
			}

			return names.ToArray();
		}
	}
}
#endif