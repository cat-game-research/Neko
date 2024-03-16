namespace SpaceGraphicsToolkit
{
	/// <summary>This implements Vector3 using the long data type.</summary>
	[System.Serializable]
	public struct SgtLong3
	{
		public long x;
		public long y;
		public long z;

		public SgtLong3(long newX, long newY, long newZ)
		{
			x = newX; y = newY; z = newZ;
		}

		public override string ToString()
		{
			return "(" + x + ", " + y + ", " + z + ")";
		}
	}
}