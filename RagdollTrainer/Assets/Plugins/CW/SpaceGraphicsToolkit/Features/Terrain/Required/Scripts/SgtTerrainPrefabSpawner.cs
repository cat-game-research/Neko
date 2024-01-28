using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to spawn objects on the attached <b>SgtTerrain</b> using a splatmap.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainPrefabSpawner")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Prefab Spawner")]
	public class SgtTerrainPrefabSpawner : MonoBehaviour
	{
		public enum RotateType
		{
			Randomly,
			ToSurfaceNormal,
			ToPlanetCenter
		}

		struct Coord : System.IEquatable<Coord>
		{
			public int  Face;
			public long X;
			public long Y;

			public override int GetHashCode()
			{
				var hash = 43270662;
				hash = hash * -1521134295 + Face.GetHashCode();
				hash = hash * -1521134295 + X.GetHashCode();
				hash = hash * -1521134295 + Y.GetHashCode();
				return hash;
			}

			public override bool Equals(object obj)
			{
				return obj is Coord ? Equals((Coord)obj) : false;
			}

			public bool Equals(Coord other)
			{
				return Face == other.Face && X == other.X && Y == other.Y;
			}
		}

		class Chunk
		{
			public int              Face;
			public Coord            Coord;
			public bool             Marked;
			public float            Distance;
			public double3          PointC;
			public double3          PointH;
			public double3          PointV;
			public List<GameObject> Clones;

			public void Dispose()
			{
				if (Clones != null)
				{
					foreach (var clone in Clones)
					{
						DestroyImmediate(clone);
					}

					Clones = null;
				}
			}
		}

		/// <summary>This allows you to control where this biome appears based on the <b>SgtTerrain</b> component's <b>Areas</b> splatmap.</summary>
		public int Area { set { if (area != value) { area = value; MarkAsDirty(); } } get { return area; } } [SerializeField] private int area = -1;

		/// <summary>If the prefabs are set to spawn in a specific area, this allows you to specify how far into the area you must travel before prefabs begin spawning.</summary>
		public float Threshold { set { if (threshold != value) { threshold = value; MarkAsDirty(); } } get { return threshold; } } [SerializeField] [Range(0.0f, 0.9f)] private float threshold = 0.5f;

		/// <summary>The maximum amount of prefabs that can spawn per chunk.</summary>
		public int Limit { set { if (limit != value) { limit = value; MarkAsDirty(); } } get { return limit; } } [SerializeField] private int limit = 10;

		/// <summary>The terrain will be split into this many cells on each axis.</summary>
		public long Resolution { set { if (resolution != value) { resolution = value; MarkAsDirty(); } } get { return resolution; } } [SerializeField] private long resolution = 1000;

		/// <summary>The radius in chunks around the camera that will have prefabs.
		/// 1 = 3x3 chunks.
		/// 2 = 5x5 chunks.
		/// 3 = 7x7 chunks.</summary>
		public int Radius { set { radius = value; } get { return radius; } } [SerializeField] [Range(1, 20)] private int radius = 5;

		/// <summary>How should the spawned prefabs be rotated?</summary>
		public RotateType Rotate { set { if (rotate != value) { rotate = value; MarkAsDirty(); } } get { return rotate; } } [SerializeField] private RotateType rotate;

		/// <summary>If your terrain has an atmosphere/corona and your spawned objects are large enough to be covered by it, then specify its <b>SgtSharedMaterial</b> here.</summary>
		public SgtSharedMaterial SharedMaterial { set { if (sharedMaterial != value) { sharedMaterial = value; MarkAsDirty(); } } get { return sharedMaterial; } } [SerializeField] private SgtSharedMaterial sharedMaterial;

		/// <summary>The prefabs that will be spawned.</summary>
		public List<Transform> Prefabs { get { if (prefabs == null) prefabs = new List<Transform>(); return prefabs; } } [SerializeField] private List<Transform> prefabs;

		private SgtTerrain cachedTerrain;

		private NativeArray<float> tempWeights;

		private Dictionary<Coord, Chunk> chunks = new Dictionary<Coord, Chunk>();

		private static List<Chunk> tempChunks = new List<Chunk>();

		public void MarkAsDirty()
		{
			foreach (var chunk in chunks.Values)
			{
				chunk.Dispose();
			}

			chunks.Clear();
		}

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			tempWeights = new NativeArray<float>(0, Allocator.Persistent);
		}

		protected virtual void OnDisable()
		{
			foreach (var chunk in chunks.Values)
			{
				chunk.Dispose();
			}

			chunks.Clear();

			cachedTerrain.ScheduleDispose(tempWeights);
		}

		protected virtual void Update()
		{
			UpdateRoot();

			// Gen new prefabs?
			//if (currentTerrain == null)
			{
				var bestChunk = default(Chunk);

				foreach (var chunk in chunks.Values)
				{
					// Needs collider?
					if (chunk.Clones == null)// && chunk.Distance < radius * radius)
					{
						if (bestChunk == null || chunk.Distance < bestChunk.Distance)
						{
							bestChunk = chunk;
						}
					}
				}

				if (bestChunk != null)
				{
					Schedule(bestChunk);
				}
			}
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			MarkAsDirty();
		}

		private void Schedule(Chunk chunk)
		{
			var rng = (double)noise.snoise((float3)chunk.PointC);

			chunk.Clones = new List<GameObject>();

			var jobArea           = default(int);
			var jobAreaSize       = default(int2);
			var jobAreaSplatCount = default(int);
			var jobAreaWeights    = default(NativeArray<float>);

			if (cachedTerrain.Areas != null && cachedTerrain.Areas.SplatCount > 0 && area >= 0)
			{
				jobArea           = math.clamp(area, 0, cachedTerrain.Areas.SplatCount - 1);
				jobAreaSize       = cachedTerrain.Areas.Size;
				jobAreaSplatCount = cachedTerrain.Areas.SplatCount;
				jobAreaWeights    = cachedTerrain.Areas.Weights;
			}
			else
			{
				jobArea           = 0;
				jobAreaSize       = int2.zero;
				jobAreaSplatCount = 0;
				jobAreaWeights    = tempWeights;
			}

			for (var i = 0; i < limit; i++)
			{
				var u      = (float)(math.abs(rng * 6829.0) % 1.0);
				var v      = (float)(math.abs(rng * 7351.0) % 1.0);
				var point  = chunk.PointC + chunk.PointH * u + chunk.PointV * v;
				var weight = 1.0f;

				point = math.normalize(SgtTerrainTopology.Tilt(SgtTerrainTopology.Warp(point)));

				if (jobAreaWeights.Length > 0)
				{
					weight = SgtTerrainTopology.Sample_Cubic_Equirectangular(jobAreaWeights, jobAreaSplatCount, jobArea, jobAreaSize, point);

					if (weight > 0.0f)
					{
						weight = math.sqrt(weight) / 255.0f;
					}

					weight = math.saturate(1.0f - weight);

					weight = Mathf.InverseLerp(threshold, 1.0f, weight);
				}

				var sample = (float)(math.abs(rng * 4334.94437) % 1.0);

				if (sample > weight)
				{
					break;
				}

				var clone = Spawn(point, chunk.PointH * 0.01, chunk.PointV * 0.01, sample);

				chunk.Clones.Add(clone.gameObject);

				rng *= 3.12345;
			}
		}

		private void UpdateRoot()
		{
			var point  = SgtTerrainTopology.Unwarp(cachedTerrain.GetAboveGroundObserverCubePoint());
			var middle = (resolution + resolution % 2) / 2;
			var center = middle * point;
			var bounds = new SgtLongBounds((long)center.x, (long)center.y, (long)center.z, radius);
			var outer  = new SgtLongBounds(-middle, -middle, middle-1,middle,middle,middle);

			Mark();

			for (var i = 0; i < 6; i++)
			{
				var quadBounds = SgtTerrainTopology.GetQuadBounds(i, bounds);

				quadBounds.ClampTo(outer);

				UpdateChunk(i, quadBounds, middle);
			}

			Sweep();
		}

		private void UpdateChunk(int face, SgtLongBounds rect, long middle)
		{
			var cubeC   = SgtTerrainTopology.CubeC[face];
			var cubeH   = SgtTerrainTopology.CubeH[face];
			var cubeV   = SgtTerrainTopology.CubeV[face];
			var step    = 1.0 / (resolution + resolution % 2);
			var stepX   = cubeH * step;
			var stepY   = cubeV * step;
			var corner  = cubeC + stepX * middle + stepY * middle;
			var centerX = (rect.minX + rect.maxX) / 2;
			var centerY = (rect.minY + rect.maxY) / 2;

			if (rect.SizeZ > 0)
			{
				for (var y = rect.minY; y < rect.maxY; y++)
				{
					for (var x = rect.minX; x < rect.maxX; x++)
					{
						var chunk = default(Chunk);
						var coord = new Coord() { Face = face, X = x, Y = y };

						if (chunks.TryGetValue(coord, out chunk) == true)
						{
							chunk.Marked = false;
						}
						else
						{
							chunk = new Chunk() { Face = face, Coord = coord };

							var root = new GameObject("Chunk");

							root.transform.SetParent(transform, false);

							root.transform.localPosition = Vector3.zero;
							root.transform.localRotation = Quaternion.identity;
							root.transform.localScale    = Vector3.one;

							chunk.PointC = corner + x * stepX + y * stepY;
							chunk.PointH = stepX;
							chunk.PointV = stepY;

							chunks.Add(coord, chunk);
						}

						var distX = math.abs(centerX - x);
						var distY = math.abs(centerY - y);

						chunk.Distance = distX * distX + distY * distY;
					}
				}
			}
		}

		private Transform Spawn(double3 localPoint, double3 localH, double3 localV, double weight)
		{
			var index        = (int)math.floor(weight * 433494437.0) % prefabs.Count;
			var root         = Instantiate(prefabs[index]);
			var sampledPoint = cachedTerrain.GetLocalPoint(localPoint);

			root.SetParent(cachedTerrain.transform, false);
			root.localPosition = (float3)sampledPoint;

			if (sharedMaterial != null)
			{
				foreach (var renderer in root.GetComponentsInChildren<Renderer>())
				{
					CwHelper.AddMaterial(renderer, sharedMaterial.Material);
				}
			}

			switch (rotate)
			{
				case RotateType.Randomly:
				{
					var x = (float)((weight * 1224133.0) % 1787.0);
					var y = (float)((weight * 1324039.0) % 1597.0);
					var z = (float)((weight * 1424041.0) % 1321.0);

					root.rotation = quaternion.Euler(x, y, z);
				}
				break;

				case RotateType.ToSurfaceNormal:
				{
					var sampledPointL = cachedTerrain.GetLocalPoint(localPoint - localH);
					var sampledPointR = cachedTerrain.GetLocalPoint(localPoint + localH);
					var sampledPointB = cachedTerrain.GetLocalPoint(localPoint - localV);
					var sampledPointF = cachedTerrain.GetLocalPoint(localPoint + localV);

					var vectorA = sampledPointR - sampledPointL;
					var vectorB = sampledPointF - sampledPointB;
					var normal  = math.normalize(-math.cross(vectorA, vectorB));
					var angle   = (float)((weight * 1224133.0) % 1597.0);

					root.up = cachedTerrain.TransformVector(normal);
					root.Rotate(0.0f, angle, 0.0f, Space.Self);
				}
				break;

				case RotateType.ToPlanetCenter:
				{
					var normal  = math.normalize(sampledPoint);
					var angle   = (float)((weight * 1224133.0) % 1597.0);

					root.up = cachedTerrain.TransformVector(normal);
					root.Rotate(0.0f, angle, 0.0f, Space.Self);
				}
				break;
			}

			return root;
		}

		private void Mark()
		{
			foreach (var chunk in chunks.Values)
			{
				chunk.Marked = true;
			}
		}

		private void Sweep()
		{
			tempChunks.Clear();

			foreach (var chunk in chunks.Values)
			{
				if (chunk.Marked == true)
				{
					tempChunks.Add(chunk);
				}
			}

			foreach (var chunk in tempChunks)
			{
				chunk.Dispose();

				chunks.Remove(chunk.Coord);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainPrefabSpawner;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainPrefabSpawner_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			markAsDirty |= SgtTerrain_Editor.DrawArea(serializedObject.FindProperty("area"), tgt.GetComponent<SgtTerrain>());
			Draw("threshold", ref markAsDirty, "If the prefabs are set to spawn in a specific area, this allows you to specify how far into the area you must travel before prefabs begin spawning.");

			Separator();

			Draw("limit", ref markAsDirty, "The maximum amount of prefabs that can spawn per chunk.");
			BeginError(Any(tgts, t => t.Resolution <= 0));
				Draw("resolution", ref markAsDirty, "The terrain will be split into this many cells on each axis.");
			EndError();
			Draw("radius", ref markAsDirty, "The radius in chunks around the camera that will have colliders.\n\n1 = 3x3 chunks.\n\n2 = 5x5 chunks.\n\n3 = 7x7 chunks.");

			Separator();

			Draw("rotate", ref markAsDirty, "How should the spawned prefabs be rotated?");
			Draw("sharedMaterial", ref markAsDirty, "If your terrain has an atmosphere/corona and your spawned objects are large enough to be covered by it, then specify its SgtSharedMaterial here.");
			BeginError(Any(tgts, t => t.Prefabs.Count == 0 || t.Prefabs.Exists(p => p == null) == true));
				Draw("prefabs", ref markAsDirty, "The prefabs that will be spawned.");
			EndError();

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty(), true, true);
			}
		}
	}
}
#endif