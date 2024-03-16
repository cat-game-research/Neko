using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class contains the core classes and methods used to construct the SgtTerrain mesh.</summary>
	public static class SgtTerrainTopology
	{
		public static double3[] CubeC = new double3[] { new double3( 1.0, -1.0, -1.0), new double3( 1.0,  1.0, -1.0), new double3( 1.0, -1.0,  1.0), new double3(-1.0, -1.0,  1.0), new double3(-1.0, -1.0, -1.0), new double3(-1.0, -1.0, -1.0) };

		public static double3[] CubeH = new double3[] { new double3( 0.0,  0.0,  2.0), new double3( 0.0,  0.0,  2.0), new double3(-2.0,  0.0,  0.0), new double3( 0.0,  0.0, -2.0), new double3( 0.0,  0.0,  2.0), new double3( 2.0,  0.0,  0.0) };

		public static double3[] CubeV = new double3[] { new double3( 0.0,  2.0,  0.0), new double3(-2.0,  0.0,  0.0), new double3( 0.0,  2.0,  0.0), new double3( 0.0,  2.0,  0.0), new double3( 2.0,  0.0,  0.0), new double3( 0.0,  2.0,  0.0) };

		public static SgtLongBounds GetQuadBounds(int index, SgtLongBounds bounds)
		{
			switch (index)
			{
				case 0: return new SgtLongBounds( bounds.minZ,  bounds.minY,  bounds.minX,  bounds.maxZ,  bounds.maxY,  bounds.maxX);
				case 1: return new SgtLongBounds( bounds.minZ, -bounds.maxX,  bounds.minY,  bounds.maxZ, -bounds.minX,  bounds.maxY);
				case 2: return new SgtLongBounds(-bounds.maxX,  bounds.minY,  bounds.minZ, -bounds.minX,  bounds.maxY,  bounds.maxZ);
				case 3: return new SgtLongBounds(-bounds.maxZ,  bounds.minY, -bounds.maxX, -bounds.minZ,  bounds.maxY, -bounds.minX);
				case 4: return new SgtLongBounds( bounds.minZ,  bounds.minX, -bounds.maxY,  bounds.maxZ,  bounds.maxX, -bounds.minY);
				case 5: return new SgtLongBounds( bounds.minX,  bounds.minY, -bounds.maxZ,  bounds.maxX,  bounds.maxY, -bounds.minZ);
			}

			return bounds;
		}

		public static double3 VectorToUnitCube(double3 vector)
		{
			var abs = math.abs(vector);

			if (abs.x >= abs.y) // X > Y
			{
				if (abs.x >= abs.z) // X > Y & Z
				{
					return vector / abs.x;
				}
			}
			else // Y > X
			{
				if (abs.y >= abs.z) // Y > X & Z
				{
					return vector / abs.y;
				}
			}

			return vector / abs.z; // Z > X & Y
		}

		public static double3 Tilt(double3 cube)
		{
			var rotation = quaternion.Euler(math.atan(1.0f / math.sqrt(2.0f)), 0.0f, 0.785398f);

			return math.mul(rotation, (float3)cube);
		}

		public static double3 Untilt(double3 cube)
		{
			var rotation = math.inverse(quaternion.Euler(math.atan(1.0f / math.sqrt(2.0f)), 0.0f, 0.785398f));

			return math.mul(rotation, (float3)cube);
		}

		public static double3 Warp(double3 cube)
		{
			return cube / (1.25 - 0.25 * cube * cube);
		}

		public static double3 Unwarp(double3 v)
		{
			var isZero = 1.0 - math.sign(math.abs(v));

			return (math.sqrt(5.0 * v * v + 4.0) - 2.0) / (v + isZero);
		}

		public static double3 UnitCubeToSphere(double3 cube)
		{
			return math.normalize(Tilt(Warp(Untilt(cube))));
		}

		public static byte Tex2D_Point(NativeArray<byte> pixels, int stride, int offset, int2 size, double2 uv)
		{
			var x = (long)math.floor(uv.x * size.x);
			var y = (long)math.floor(uv.y * size.y);

			return Sample(pixels, stride, offset, size, x, y);
		}

		public static float Tex2D_Linear_WrapXY(NativeArray<byte> data, int stride, int offset, int2 size, double2 uv)
		{
			uv *= size;

			var fracX = (uv.x % 1.0 + 1.0) % 1.0;
			var fracY = (uv.y % 1.0 + 1.0) % 1.0;
			var x     = (long)math.floor(uv.x % size.x);
			var y     = (long)math.floor(uv.y % size.y);

			var aa = Sample_WrapXY(data, stride, offset, size, x, y    ); var ba = Sample_WrapXY(data, stride, offset, size, x + 1, y    );
			var ab = Sample_WrapXY(data, stride, offset, size, x, y + 1); var bb = Sample_WrapXY(data, stride, offset, size, x + 1, y + 1);

			var a = math.lerp(aa, ba, (float)fracX);
			var b = math.lerp(ab, bb, (float)fracX);

			return math.lerp(a, b, (float)fracY);
		}

		public static float Sample_WrapXY(NativeArray<byte> data, int stride, int offset, int2 size, long x, long y)
		{
			x = (x % size.x + size.x) % size.x;
			y = (y % size.y + size.y) % size.y;

			return data[(int)((x + y * size.x) * stride + offset)] / 255.0f;
		}

		public static float Sample_Cubic(NativeArray<byte> data, int stride, int offset, int2 size, double2 uv)
		{
			uv = math.saturate(uv);

			var fracX = (float)(uv.x * size.x % 1.0);
			var fracY = (float)(uv.y * size.y % 1.0);
			var x     = (long)math.floor(uv.x * size.x);
			var y     = (long)math.floor(uv.y * size.y);

			var aa = Sample(data, stride, offset, size, x - 1, y - 1); var ba = Sample(data, stride, offset, size, x, y - 1); var ca = Sample(data, stride, offset, size, x + 1, y - 1); var da = Sample(data, stride, offset, size, x + 2, y - 1);
			var ab = Sample(data, stride, offset, size, x - 1, y    ); var bb = Sample(data, stride, offset, size, x, y    ); var cb = Sample(data, stride, offset, size, x + 1, y    ); var db = Sample(data, stride, offset, size, x + 2, y    );
			var ac = Sample(data, stride, offset, size, x - 1, y + 1); var bc = Sample(data, stride, offset, size, x, y + 1); var cc = Sample(data, stride, offset, size, x + 1, y + 1); var dc = Sample(data, stride, offset, size, x + 2, y + 1);
			var ad = Sample(data, stride, offset, size, x - 1, y + 2); var bd = Sample(data, stride, offset, size, x, y + 2); var cd = Sample(data, stride, offset, size, x + 1, y + 2); var dd = Sample(data, stride, offset, size, x + 2, y + 2);

			var a = Hermite(aa, ba, ca, da, fracX);
			var b = Hermite(ab, bb, cb, db, fracX);
			var c = Hermite(ac, bc, cc, dc, fracX);
			var d = Hermite(ad, bd, cd, dd, fracX);

			return Hermite(a, b, c, d, fracY) / 255.0f;
		}

		public static float Sample_Cubic_Equirectangular(NativeArray<float> data, int stride, int offset, int2 size, double3 direction)
		{
			var s  = size / new double2(math.PI * 2.0, math.PI);
			var u  = (math.PI * 0.5 - math.atan2(direction.x, direction.z)) * s.x;
			var v  = (math.asin(direction.y) + math.PI * 0.5) * s.y;
			var uv = new double2(u, v);

			var fracX = (float)((uv.x % 1.0 + 1.0) % 1.0);
			var fracY = (float)((uv.y % 1.0 + 1.0) % 1.0);
			var x     = (long)math.floor(uv.x % size.x);
			var y     = (long)math.floor(uv.y % size.y);

			var aa = Sample_WrapX(data, stride, offset, size, x - 1, y - 1); var ba = Sample_WrapX(data, stride, offset, size, x, y - 1); var ca = Sample_WrapX(data, stride, offset, size, x + 1, y - 1); var da = Sample_WrapX(data, stride, offset, size, x + 2, y - 1);
			var ab = Sample_WrapX(data, stride, offset, size, x - 1, y    ); var bb = Sample_WrapX(data, stride, offset, size, x, y    ); var cb = Sample_WrapX(data, stride, offset, size, x + 1, y    ); var db = Sample_WrapX(data, stride, offset, size, x + 2, y    );
			var ac = Sample_WrapX(data, stride, offset, size, x - 1, y + 1); var bc = Sample_WrapX(data, stride, offset, size, x, y + 1); var cc = Sample_WrapX(data, stride, offset, size, x + 1, y + 1); var dc = Sample_WrapX(data, stride, offset, size, x + 2, y + 1);
			var ad = Sample_WrapX(data, stride, offset, size, x - 1, y + 2); var bd = Sample_WrapX(data, stride, offset, size, x, y + 2); var cd = Sample_WrapX(data, stride, offset, size, x + 1, y + 2); var dd = Sample_WrapX(data, stride, offset, size, x + 2, y + 2);

			var a = Hermite(aa, ba, ca, da, fracX);
			var b = Hermite(ab, bb, cb, db, fracX);
			var c = Hermite(ac, bc, cc, dc, fracX);
			var d = Hermite(ad, bd, cd, dd, fracX);

			return Hermite(a, b, c, d, fracY);
		}

		public static float Sample_Cubic_Equirectangular(NativeArray<byte> data, int stride, int offset, int2 size, double3 direction)
		{
			var s  = size / new double2(math.PI * 2.0, math.PI);
			var u  = (math.PI * 0.5 - math.atan2(direction.x, direction.z)) * s.x;
			var v  = (math.asin(direction.y) + math.PI * 0.5) * s.y;
			var uv = new double2(u, v);

			var fracX = (float)((uv.x % 1.0 + 1.0) % 1.0);
			var fracY = (float)((uv.y % 1.0 + 1.0) % 1.0);
			var x     = (long)math.floor(uv.x % size.x);
			var y     = (long)math.floor(uv.y % size.y);

			var aa = Sample_WrapX(data, stride, offset, size, x - 1, y - 1); var ba = Sample_WrapX(data, stride, offset, size, x, y - 1); var ca = Sample_WrapX(data, stride, offset, size, x + 1, y - 1); var da = Sample_WrapX(data, stride, offset, size, x + 2, y - 1);
			var ab = Sample_WrapX(data, stride, offset, size, x - 1, y    ); var bb = Sample_WrapX(data, stride, offset, size, x, y    ); var cb = Sample_WrapX(data, stride, offset, size, x + 1, y    ); var db = Sample_WrapX(data, stride, offset, size, x + 2, y    );
			var ac = Sample_WrapX(data, stride, offset, size, x - 1, y + 1); var bc = Sample_WrapX(data, stride, offset, size, x, y + 1); var cc = Sample_WrapX(data, stride, offset, size, x + 1, y + 1); var dc = Sample_WrapX(data, stride, offset, size, x + 2, y + 1);
			var ad = Sample_WrapX(data, stride, offset, size, x - 1, y + 2); var bd = Sample_WrapX(data, stride, offset, size, x, y + 2); var cd = Sample_WrapX(data, stride, offset, size, x + 1, y + 2); var dd = Sample_WrapX(data, stride, offset, size, x + 2, y + 2);

			var a = Hermite(aa, ba, ca, da, fracX);
			var b = Hermite(ab, bb, cb, db, fracX);
			var c = Hermite(ac, bc, cc, dc, fracX);
			var d = Hermite(ad, bd, cd, dd, fracX);

			return Hermite(a, b, c, d, fracY);
		}

		public static byte Sample(NativeArray<byte> pixels, int stride, int offset, int2 size, long x, long y)
		{
			x = math.clamp(x, 0, size.x - 1);
			y = math.clamp(y, 0, size.y - 1);

			return pixels[(int)((x + y * size.x) * stride + offset)];
		}

		public static byte Sample_Wrap(NativeArray<byte> pixels, int stride, int offset, int2 size, long x, long y)
		{
			x = (x % size.x + size.x) % size.x;
			y = (y % size.y + size.y) % size.y;

			return pixels[(int)((x + y * size.x) * stride + offset)];
		}

		private static float Sample_WrapX(NativeArray<float> data, int stride, int offset, int2 size, long x, long y)
		{
			x = (x % size.x + size.x) % size.x;
			y = math.clamp(y, 0, size.y - 1);

			var i = (int)x + (int)y * size.x;

			return data[i * stride + offset];
		}

		private static float Sample_WrapX(NativeArray<byte> data, int stride, int offset, int2 size, long x, long y)
		{
			x = (x % size.x + size.x) % size.x;
			y = math.clamp(y, 0, size.y - 1);

			var i = (int)x + (int)y * size.x;

			return data[i * stride + offset] / 255.0f;
		}

		public static Color Hermite(Color a, Color b, Color c, Color d, float t)
		{
			var tt   = t * t;
			var tt3  = tt * 3.0f;
			var ttt  = t * tt;
			var ttt2 = ttt * 2.0f;
			var m0   = (c - a) * 0.5f;
			var m1   = (d - b) * 0.5f;
			var a0   =  ttt2 - tt3 + 1.0f;
			var a1   =  ttt  - tt * 2.0f + t;
			var a2   =  ttt  - tt;
			var a3   = -ttt2 + tt3;

			return a0 * b + a1 * m0 + a2 * m1 + a3 * c;
		}

		public static float Hermite(float a, float b, float c, float d, float t)
		{
			var tt   = t * t;
			var tt3  = tt * 3.0f;
			var ttt  = t * tt;
			var ttt2 = ttt * 2.0f;
			var m0   = (c - a) * 0.5f;
			var m1   = (d - b) * 0.5f;
			var a0   =  ttt2 - tt3 + 1.0f;
			var a1   =  ttt  - tt * 2.0f + t;
			var a2   =  ttt  - tt;
			var a3   = -ttt2 + tt3;

			return a0 * b + a1 * m0 + a2 * m1 + a3 * c;
		}
	}
}