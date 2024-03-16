using Unity.Mathematics;

namespace SpaceGraphicsToolkit
{
	public class SgtTerrainCube
	{
		public SgtTerrainQuad[] Quads { get { return quads; } } private SgtTerrainQuad[] quads = new SgtTerrainQuad[6];

		public SgtTerrain Terrain { get { return terrain; } } private SgtTerrain terrain;

		public int Depth { get { return depth; } } private int depth;

		public long Resolution { get { return resolution; } } private long resolution;

		public long Middle { get { return middle; } } private long middle;

		public double Step { get { return step; } } private double step;

		public SgtTerrainCube(SgtTerrain newTerrain)
		{
			terrain = newTerrain;

			quads[0] = new SgtTerrainQuad(this, 0, 0.5, SgtTerrainTopology.CubeC[0], SgtTerrainTopology.CubeH[0], SgtTerrainTopology.CubeV[0]);
			quads[1] = new SgtTerrainQuad(this, 1, 0.0, SgtTerrainTopology.CubeC[1], SgtTerrainTopology.CubeH[1], SgtTerrainTopology.CubeV[1]);
			quads[2] = new SgtTerrainQuad(this, 2, 0.0, SgtTerrainTopology.CubeC[2], SgtTerrainTopology.CubeH[2], SgtTerrainTopology.CubeV[2]);
			quads[3] = new SgtTerrainQuad(this, 3, 0.0, SgtTerrainTopology.CubeC[3], SgtTerrainTopology.CubeH[3], SgtTerrainTopology.CubeV[3]);
			quads[4] = new SgtTerrainQuad(this, 4, 0.5, SgtTerrainTopology.CubeC[4], SgtTerrainTopology.CubeH[4], SgtTerrainTopology.CubeV[4]);
			quads[5] = new SgtTerrainQuad(this, 5, 0.0, SgtTerrainTopology.CubeC[5], SgtTerrainTopology.CubeH[5], SgtTerrainTopology.CubeV[5]);
		}

		public void Setup(int newDepth, long newResolution)
		{
			depth      = newDepth;
			resolution = newResolution;
			step       = 1.0 / newResolution;
			middle     = newResolution / 2;
		}
	}
}