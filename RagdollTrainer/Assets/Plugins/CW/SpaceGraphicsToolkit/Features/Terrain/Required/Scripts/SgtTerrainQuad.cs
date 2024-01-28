using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit
{
	public class SgtTerrainQuad
	{
		public SgtTerrainCube Cube { get { return cube; } } private SgtTerrainCube cube;

		public double3 CubeC { get { return cubeC; } } private double3 cubeC;

		public double3 CubeH { get { return cubeH; } } private double3 cubeH;

		public double3 CubeV { get { return cubeV; } } private double3 cubeV;

		public double3 CubeO { get { return cubeO; } } private double3 cubeO;

		public double Twist { get { return twist; } } private double twist;

		public int Face { get { return face; } } private int face;

		public SgtProperties Properties { get { if (properties == null) properties = new SgtProperties(); return properties; } } private SgtProperties properties;

		public List<Mesh> CurrentMeshes = new List<Mesh>();
		public List<Mesh> PendingMeshes = new List<Mesh>();

		public Texture2D CurrentSplat;
		public Texture2D PendingSplat;

		public NativeArray<double3> Points;
		public NativeArray<int>     Biomes;

		public float3        CurrentCorner;
		public float3        PendingCorner;
		public SgtLongBounds CurrentInner;
		public SgtLongBounds CurrentOuter;
		public SgtLongBounds PendingOuter;
		public SgtLongBounds PendingInner;
		public SgtLongBounds VirtualInner;
		public SgtLongBounds VirtualOuter;

		public SgtTerrainQuad(SgtTerrainCube newCube, int newFace, double newTwist, double3 newCubeC, double3 newCubeH, double3 newCubeV)
		{
			cube  = newCube;
			face  = newFace;
			twist = newTwist;
			cubeC = SgtTerrainTopology.Tilt(newCubeC);
			cubeH = SgtTerrainTopology.Tilt(newCubeH);
			cubeV = SgtTerrainTopology.Tilt(newCubeV);
			cubeO = math.cross(math.normalize(cubeH), cubeV);

			Allocate();
		}

		public void Allocate()
		{
			if (Points.IsCreated == false)
			{
				Points = new NativeArray<double3>(0, Allocator.Persistent);
			}

			if (Biomes.IsCreated == false)
			{
				Biomes = new NativeArray<int>(0, Allocator.Persistent);
			}

			CurrentInner.Clear();
			CurrentOuter.Clear();
			PendingOuter.Clear();
			PendingInner.Clear();
			VirtualInner.Clear();
			VirtualOuter.Clear();
		}

		public void Dispose()
		{
			if (Points.IsCreated == true) Points.Dispose();

			if (Biomes.IsCreated == true) Biomes.Dispose();

			foreach (var mesh in CurrentMeshes)
			{
				Object.DestroyImmediate(mesh);
			}

			foreach (var mesh in PendingMeshes)
			{
				Object.DestroyImmediate(mesh);
			}

			Object.DestroyImmediate(CurrentSplat);
			Object.DestroyImmediate(PendingSplat);

			CurrentMeshes.Clear();
			PendingMeshes.Clear();
		}

		public void Swap()
		{
			if (PendingMeshes.Count > 0)
			{
				foreach (var mesh in CurrentMeshes)
				{
					Object.DestroyImmediate(mesh);
				}

				CurrentMeshes.Clear();

				foreach (var mesh in PendingMeshes)
				{
					CurrentMeshes.Add(mesh);
				}

				PendingMeshes.Clear();
			}

			if (PendingSplat != null)
			{
				if (CurrentSplat != null)
				{
					Object.DestroyImmediate(CurrentSplat);
				}

				CurrentSplat = PendingSplat;
				PendingSplat = null;
			}

			CurrentOuter  = PendingOuter;
			CurrentInner  = PendingInner;
			CurrentCorner = PendingCorner;
		}
	}
}