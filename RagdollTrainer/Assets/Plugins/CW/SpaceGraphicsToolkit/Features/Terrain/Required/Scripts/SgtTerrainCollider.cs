using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component generates colliders for the current <b>SgtTerrain</b>, allowing you to use normal physics.
	/// The colliders use the <b>MeshCollider</b> component, and are generated at a fixed resolution after the visual mesh for the specified <b>ChunkSize</b> is generated.
	/// NOTE: Collider generation is slow, so you should keep the <b>Resolution</b> as low as possible.</summary>
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainCollider")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Collider")]
	public class SgtTerrainCollider : MonoBehaviour
	{
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
			public int          Face;
			public Coord        Coord;
			public bool         Marked;
			public Vector3      Corner;
			public float        Distance;
			public double3      PointC;
			public double3      PointH;
			public double3      PointV;
			public MeshCollider Shape;

			public void Dispose()
			{
				if (currentChunk == this)
				{
					Complete();
				}

				DestroyImmediate(Shape.sharedMesh);
				DestroyImmediate(Shape.gameObject);
			}
		}

		[BurstCompile]
		struct StartJob : IJob
		{
			public double3 CubeC;
			public double3 CubeH;
			public double3 CubeV;
			public int     Verts;
			public int     Quads;
			public double  Radius;

			public NativeArray<double3> Points;
			public NativeArray<double>  Heights;

			public void Execute()
			{
				var step  = 1.0 / Quads;
				var index = 0;

				for (var y = 0; y < Verts; y++)
				{
					for (var x = 0; x < Verts; x++)
					{
						var point = CubeC + CubeH * x * step + CubeV * y * step;

						point = SgtTerrainTopology.Tilt(SgtTerrainTopology.Warp(point));

						Points[index] = math.normalize(point);
						Heights[index] = Radius;

						index++;
					}
				}
			}
		}

		[BurstCompile]
		struct EndJob : IJob
		{
			public int Verts;
			public int Quads;

			public NativeArray<double3> Points;
			public NativeArray<double>  Heights;
			public NativeArray<float3>  Positions;
			public NativeArray<int>     Indices;
			public NativeArray<float3>  Corner;

			public void Execute()
			{
				var index = 0;

				for (var y = 0; y < Quads; y++)
				{
					for (var x = 0; x < Quads; x++)
					{
						var a = x + y * Verts;
						var b = a + 1;
						var c = a + Verts;
						var d = c + 1;

						Indices[index++] = a; Indices[index++] = c; Indices[index++] = b;

						Indices[index++] = d; Indices[index++] = b; Indices[index++] = c;
					}
				}

				Corner[0] = (float3)math.floor(Points[0] * Heights[0]);

				for (var i = 0; i < Positions.Length; i++)
				{
					var sphere = Points[i] * Heights[i] - Corner[0];

					Positions[i] = (float3)sphere;
				}
			}
		}

		/// <summary>The terrain will be split into this many cells on each axis.</summary>
		public long Resolution { set { resolution = value; } get { return resolution; } } [SerializeField] private long resolution = 1000;

		/// <summary>The radius in chunks around the camera that will have colliders.
		/// 1 = 3x3 chunks.
		/// 2 = 5x5 chunks.
		/// 3 = 7x7 chunks.</summary>
		public int Radius { set { radius = value; } get { return radius; } } [SerializeField] [Range(1, 20)] private int radius = 5;

		/// <summary>The width & height of the generated collider in quads. The higher you set this, the higher the collider quality. Setting this too high may make the colliders slow to generate.</summary>
		public int Detail { set { detail = value; } get { return detail; } } [SerializeField] [Range(1, 64)] private int detail = 8;

		/// <summary>The amount of frames between each collider generation. If you want higher performance you should set this higher. If you want quicker collider generation you should set this lower.</summary>
		public int Delay { set { delay = value; } get { return delay; } } [SerializeField] [Range(0, 2)] private int delay = 0;

		private SgtTerrain cachedTerrain;

		private Dictionary<Coord, Chunk> chunks = new Dictionary<Coord, Chunk>();

		private static List<Chunk> tempChunks = new List<Chunk>();

		private NativeArray<double3> points;
		private NativeArray<double>  heights;
		private NativeArray<float3>  positions;
		private NativeArray<int>     indices;
		private NativeArray<float3>  corner;

		private static JobHandle  currentHandle;
		private static SgtTerrain currentTerrain;
		private static Chunk      currentChunk;
		private static int        currentAge;

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			cachedTerrain.OnDisabled += HandleDisabled;
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.OnDisabled -= HandleDisabled;

			Complete();

			foreach (var chunk in chunks.Values)
			{
				chunk.Dispose();
			}

			chunks.Clear();

			if (points.IsCreated == true) points.Dispose();
			if (heights.IsCreated == true) heights.Dispose();
			if (positions.IsCreated == true) positions.Dispose();
			if (indices.IsCreated == true) indices.Dispose();
			if (corner.IsCreated == true) corner.Dispose();
		}

		private void HandleDisabled()
		{
			Complete();
		}

		protected virtual void Update()
		{
			if (cachedTerrain.enabled == false)
			{
				return;
			}

			// Finish collider gen?
			if (currentTerrain == cachedTerrain)
			{
				if (++currentAge > delay)
				{
					var chunk = Complete();
					var mesh  = new Mesh();

					mesh.SetVertices(SgtCommon.ConvertNativeArray(positions));
#if UNITY_2019_3_OR_NEWER
					mesh.SetIndices(SgtCommon.ConvertNativeArray(indices), MeshTopology.Triangles, 0);
#else
					mesh.SetTriangles(SgtCommon.ConvertNativeArray(indices), 0);
#endif
					chunk.Shape.sharedMesh = mesh;

					chunk.Corner = corner[0];

					chunk.Shape.transform.localPosition = chunk.Corner;
				}
			}

			UpdateRoot();
			
			// Gen new collider?
			if (currentTerrain == null)
			{
				var bestChunk = default(Chunk);

				foreach (var chunk in chunks.Values)
				{
					// Needs collider?
					if (chunk.Shape.sharedMesh == null)// && chunk.Distance < radius * radius)
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
				var centerX    = (quadBounds.minX + quadBounds.maxX) / 2;
				var centerY    = (quadBounds.minY + quadBounds.maxY) / 2;
				var centerZ    = (quadBounds.minZ + quadBounds.maxZ) / 2;
				var quadCenter = new SgtLong3(centerX, centerY, centerZ);

				quadBounds.ClampTo(outer);

				UpdateColliders(i, quadBounds, quadCenter, middle);
			}

			Sweep();
		}

		private static Chunk Complete()
		{
			var chunk = currentChunk;

			if (chunk != null)
			{
				currentHandle.Complete();

				currentTerrain = null;
				currentChunk   = null;
				currentAge     = 0;
			}

			return chunk;
		}

		private void UpdateColliders(int face, SgtLongBounds rect, SgtLong3 center, long middle)
		{
			var cubeC   = SgtTerrainTopology.CubeC[face];
			var cubeH   = SgtTerrainTopology.CubeH[face];
			var cubeV   = SgtTerrainTopology.CubeV[face];
			var step    = 1.0 / (resolution + resolution % 2);
			var stepX   = cubeH * step;
			var stepY   = cubeV * step;
			var corner  = cubeC + stepX * middle + stepY * middle;

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

							var root = CwHelper.CreateGameObject("Collider", gameObject.layer, transform, chunk.Corner, quaternion.identity, Vector3.one);

							chunk.Shape = root.AddComponent<MeshCollider>();

							chunk.PointC = corner + x * stepX + y * stepY;
							chunk.PointH = stepX;
							chunk.PointV = stepY;

							chunks.Add(coord, chunk);
						}

						var distX = math.abs(center.x - x);
						var distY = math.abs(center.y - y);
						var distZ = math.abs(center.z - (rect.minZ + rect.maxZ) / 2);

						chunk.Distance = distX * distX + distY * distY + distZ * distZ;
					}
				}
			}
		}

		private void Schedule(Chunk chunk)
		{
			var quads = detail;
			var verts = quads + 1;

			SgtCommon.UpdateNativeArray(ref points, verts * verts);
			SgtCommon.UpdateNativeArray(ref heights, verts * verts);
			SgtCommon.UpdateNativeArray(ref positions, verts * verts);
			SgtCommon.UpdateNativeArray(ref indices, quads * quads * 6);
			SgtCommon.UpdateNativeArray(ref corner, 1);

			var startJob = new StartJob();

			startJob.CubeC   = chunk.PointC;
			startJob.CubeH   = chunk.PointH;
			startJob.CubeV   = chunk.PointV;
			startJob.Verts   = verts;
			startJob.Quads   = quads;
			startJob.Radius  = cachedTerrain.Radius;
			startJob.Points  = points;
			startJob.Heights = heights;

			currentHandle = startJob.Schedule();

			cachedTerrain.InvokeScheduleCombinedHeights(points, heights, ref currentHandle);

			var endJob = new EndJob();

			endJob.Verts     = verts;
			endJob.Quads     = quads;
			endJob.Points    = points;
			endJob.Heights   = heights;
			endJob.Positions = positions;
			endJob.Indices   = indices;
			endJob.Corner    = corner;

			currentHandle  = endJob.Schedule(currentHandle);
			currentTerrain = cachedTerrain;
			currentChunk   = chunk;
			currentAge     = 0;
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
	using TARGET = SgtTerrainCollider;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainCollider_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Resolution <= 0));
				Draw("resolution", "The terrain will be split into this many cells on each axis.");
			EndError();
			Draw("radius", "The radius in chunks around the camera that will have colliders.\n\n1 = 3x3 chunks.\n\n2 = 5x5 chunks.\n\n3 = 7x7 chunks.");
			Draw("detail", "The width & height of the generated collider in quads. The higher you set this, the higher the collider quality. Setting this too high may make the colliders slow to generate.");
			Draw("delay", "The amount of frames between each collider generation. If you want higher performance you should set this higher. If you want quicker collider generation you should set this lower.");
		}
	}
}
#endif