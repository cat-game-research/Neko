namespace SpaceGraphicsToolkit
{
	/// <summary>This implements a Rect using the long data type.</summary>
	public struct SgtLongRect
	{
		public long minX;
		public long minY;
		public long maxX;
		public long maxY;

		public long SizeX
		{
			get
			{
				return maxX - minX;
			}
		}

		public long SizeY
		{
			get
			{
				return maxY - minY;
			}
		}

		public void ClampTo(SgtLongRect other)
		{
			if (minX < other.minX) minX = other.minX; else if (minX > other.maxX) minX = other.maxX;
			if (minY < other.minY) minY = other.minY; else if (minY > other.maxY) minY = other.maxY;
			if (maxX < other.minX) maxX = other.minX; else if (maxX > other.maxX) maxX = other.maxX;
			if (maxY < other.minY) maxY = other.minY; else if (maxY > other.maxY) maxY = other.maxY;
		}

		public SgtLongRect GetExpanded(long amount)
		{
			return new SgtLongRect(minX - amount, minY - amount, maxX + amount, maxY + amount);
		}

		public SgtLongRect(long centerX, long centerY, long size)
		{
			minX = centerX - size; minY = centerY - size; maxX = centerX + size; maxY = centerY + size;
		}

		public SgtLongRect(long newMinX, long newMinY, long newMaxX, long newMaxY)
		{
			minX = newMinX; minY = newMinY; maxX = newMaxX; maxY = newMaxY;
		}

		public bool Contains(long x, long y)
		{
			return x >= minX && x < maxX && y >= minY && y < maxY;
		}

		public void Clear()
		{
			minX = minY = maxX = maxY = 0;
		}

		public void SwapX()
		{
			var t = minX;

			minX = -maxX;
			maxX = -t;
		}

		public void SwapY()
		{
			var t = minY;

			minY = -maxY;
			maxY = -t;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator == (SgtLongRect a, SgtLongRect b)
		{
			return a.minX == b.minX && a.minY == b.minY && a.maxX == b.maxX && a.maxY == b.maxY;
		}

		public static bool operator != (SgtLongRect a, SgtLongRect b)
		{
			return a.minX != b.minX || a.minY != b.minY || a.maxX != b.maxX || a.maxY != b.maxY;
		}

		public static SgtLongRect operator * (SgtLongRect a, long b)
		{
			return new SgtLongRect(a.minX * b, a.minY * b, a.maxX * b, a.maxY * b);
		}

		public override string ToString()
		{
			return "(" + minX + ", " + minY + " : " + maxX + ", " + maxY + ")";
		}
	}
}