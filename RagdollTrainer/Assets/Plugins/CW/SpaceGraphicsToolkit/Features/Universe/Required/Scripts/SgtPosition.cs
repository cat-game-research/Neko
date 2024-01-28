using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class stores a coordinate in the Universe, and provides methods to manipulate them.</summary>
	[System.Serializable]
	public struct SgtPosition
	{
		/// <summary>When the LocalX/Y/Z values exceed this value, the Global/X/Y/Z values will be modified to account for it, creating a grid system
		/// The universe is 4.4e+26~ meters in radius
		/// A long stores up to +- 9.2e+18~
		/// Divide them, and you get 47696652.0723
		/// Thus the below value allows you to simulate a universe that is larger than the current observable universe</summary>
		public static readonly double CELL_SIZE = 50000000.0;

		/// <summary>The position in meters along the X axis, relative to the current global cell position.</summary>
		public double LocalX;

		/// <summary>The position in meters along the Y axis, relative to the current global cell position.</summary>
		public double LocalY;

		/// <summary>The position in meters along the Z axis, relative to the current global cell position.</summary>
		public double LocalZ;

		/// <summary>The current grid cell along the X axis. Each grid cell is equal to 50000000 meters.</summary>
		public long GlobalX;

		/// <summary>The current grid cell along the Y axis. Each grid cell is equal to 50000000 meters.</summary>
		public long GlobalY;

		/// <summary>The current grid cell along the Z axis. Each grid cell is equal to 50000000 meters.</summary>
		public long GlobalZ;

		public SgtPosition(Vector3 localXYZ, double scale = 1)
		{
			LocalX = localXYZ.x * scale;
			LocalY = localXYZ.y * scale;
			LocalZ = localXYZ.z * scale;

			GlobalX = GlobalY = GlobalZ = 0;

			SnapLocal();
		}

		public static double Distance(SgtPosition a, SgtPosition b)
		{
			var x = (b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX;
			var y = (b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY;
			var z = (b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ;

			return System.Math.Sqrt(x * x + y * y + z * z);
		}

		public static double Distance(ref SgtPosition a, ref SgtPosition b)
		{
			var x = (b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX;
			var y = (b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY;
			var z = (b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ;

			return System.Math.Sqrt(x * x + y * y + z * z);
		}

		public static double SqrDistance(SgtPosition a, SgtPosition b)
		{
			var x = (b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX;
			var y = (b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY;
			var z = (b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ;

			return x * x + y * y + z * z;
		}

		public static double SqrDistance(ref SgtPosition a, ref SgtPosition b)
		{
			var x = (b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX;
			var y = (b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY;
			var z = (b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ;

			return x * x + y * y + z * z;
		}

		public static SgtPosition Delta(ref SgtPosition a, ref SgtPosition b)
		{
			var o = default(SgtPosition);

			o.LocalX = a.LocalX - b.LocalX;
			o.LocalY = a.LocalY - b.LocalY;
			o.LocalZ = a.LocalZ - b.LocalZ;
			o.GlobalX = a.GlobalX - b.GlobalX;
			o.GlobalY = a.GlobalY - b.GlobalY;
			o.GlobalZ = a.GlobalZ - b.GlobalZ;

			return o;
		}

		public static bool Equal(ref SgtPosition a, ref SgtPosition b)
		{
			if (a.GlobalX == b.GlobalX && a.GlobalY == b.GlobalY && a.GlobalZ == b.GlobalZ)
			{
				if (a.LocalX == b.LocalX && a.LocalY == b.LocalY && a.LocalZ == b.LocalZ)
				{
					return true;
				}
			}

			return false;
		}

		public static Vector3 Direction(SgtPosition a, SgtPosition b)
		{
			return Direction(ref a, ref b);
		}

		public static Vector3 Direction(ref SgtPosition a, ref SgtPosition b)
		{
			var x = (b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX;
			var y = (b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY;
			var z = (b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ;
			var m = System.Math.Sqrt(x * x + y * y + z * z);

			if (m > 0.0)
			{
				x /= m;
				y /= m;
				z /= m;
			}

			return new Vector3((float)x, (float)y, (float)z);
		}

		public static SgtPosition Lerp(ref SgtPosition a, ref SgtPosition b, double t)
		{
			var o = default(SgtPosition);

			if (t > 0.5)
			{
				t = 1.0 - t;

				o.GlobalX = b.GlobalX;
				o.GlobalY = b.GlobalY;
				o.GlobalZ = b.GlobalZ;
				o.LocalX  = b.LocalX - ((b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX) * t;
				o.LocalY  = b.LocalY - ((b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY) * t;
				o.LocalZ  = b.LocalZ - ((b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ) * t;
			}
			else
			{
				o.GlobalX = a.GlobalX;
				o.GlobalY = a.GlobalY;
				o.GlobalZ = a.GlobalZ;
				o.LocalX  = a.LocalX + ((b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX) * t;
				o.LocalY  = a.LocalY + ((b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY) * t;
				o.LocalZ  = a.LocalZ + ((b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ) * t;
			}

			o.SnapLocal();

			return o;
		}

		// Get the world space vector between two positions
		public static Vector3 Vector(ref SgtPosition a, ref SgtPosition b, double scale = 1.0)
		{
			var x = (b.GlobalX - a.GlobalX) * CELL_SIZE + b.LocalX - a.LocalX;
			var y = (b.GlobalY - a.GlobalY) * CELL_SIZE + b.LocalY - a.LocalY;
			var z = (b.GlobalZ - a.GlobalZ) * CELL_SIZE + b.LocalZ - a.LocalZ;

			x *= scale;
			y *= scale;
			z *= scale;

			return new Vector3((float)x, (float)y, (float)z);
		}

		// Did the local position stray too far from the origin?
		public bool SnapLocal()
		{
			var updatePosition = false;
			var shiftX         = CalculateShift(LocalX, CELL_SIZE);
			var shiftY         = CalculateShift(LocalY, CELL_SIZE);
			var shiftZ         = CalculateShift(LocalZ, CELL_SIZE);

			if (shiftX != 0)
			{
				GlobalX += shiftX;
				LocalX  -= shiftX * CELL_SIZE;

				updatePosition = true;
			}

			if (shiftY != 0)
			{
				GlobalY += shiftY;
				LocalY  -= shiftY * CELL_SIZE;

				updatePosition = true;
			}

			if (shiftZ != 0)
			{
				GlobalZ += shiftZ;
				LocalZ  -= shiftZ * CELL_SIZE;

				updatePosition = true;
			}

			return updatePosition;
		}

		public static SgtPosition operator + (SgtPosition a, SgtPosition b)
		{
			a.GlobalX += b.GlobalX;
			a.GlobalY += b.GlobalY;
			a.GlobalZ += b.GlobalZ;

			a.LocalX += b.LocalX;
			a.LocalY += b.LocalY;
			a.LocalZ += b.LocalZ;

			a.SnapLocal();

			return a;
		}

		public static SgtPosition operator + (SgtPosition a, Vector3 b)
		{
			a.LocalX += b.x;
			a.LocalY += b.y;
			a.LocalZ += b.z;

			a.SnapLocal();

			return a;
		}

		public static SgtPosition operator + (Vector3 a, SgtPosition b)
		{
			b.LocalX += a.x;
			b.LocalY += a.y;
			b.LocalZ += a.z;

			b.SnapLocal();

			return b;
		}

		public override string ToString()
		{
			return "(" + LocalX + ", " + LocalY + ", " + LocalZ + " - " + GlobalX + ", " + GlobalY + ", " + GlobalZ + ")";
		}

		private long CalculateShift(double coordinate, double cellSize)
		{
			var shift = coordinate / cellSize;

			return (long)shift;
		}
	}
}