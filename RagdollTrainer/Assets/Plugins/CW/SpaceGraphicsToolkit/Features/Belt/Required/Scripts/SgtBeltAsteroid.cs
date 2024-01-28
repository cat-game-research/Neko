using UnityEngine;

namespace SpaceGraphicsToolkit.Belt
{
	/// <summary>This stores all information about an asteroid in an SgtBelt___ component.</summary>
	[System.Serializable]
	public class SgtBeltAsteroid
	{
		/// <summary>Temp instance used when generating the belt.</summary>
		public static SgtBeltAsteroid Temp = new SgtBeltAsteroid();

		/// <summary>The coordinate index in the asteroid texture.</summary>
		[Tooltip("The coordinate index in the asteroid texture.")]
		public int Variant;

		/// <summary>Color tint of this asteroid.</summary>
		[Tooltip("Color tint of this asteroid.")]
		public Color Color = Color.white;
		
		/// <summary>Radius of this asteroid in local space.</summary>
		[Tooltip("Radius of this asteroid in local space.")]
		public float Radius;
		
		/// <summary>Height of this asteroid's orbit in local space.</summary>
		[Tooltip("Height of this asteroid's orbit in local space.")]
		public float Height;
		
		/// <summary>The base roll angle of this asteroid in radians.</summary>
		[Tooltip("The base roll angle of this asteroid in radians.")]
		public float Angle;
		
		/// <summary>How fast this asteroid rolls in radians per second.</summary>
		[Tooltip("How fast this asteroid rolls in radians per second.")]
		public float Spin;
		
		/// <summary>The base angle of this asteroid's orbit in radians.</summary>
		[Tooltip("The base angle of this asteroid's orbit in radians.")]
		public float OrbitAngle;
		
		/// <summary>The speed of this asteroid's orbit in radians.</summary>
		[Tooltip("The speed of this asteroid's orbit in radians.")]
		public float OrbitSpeed;
		
		/// <summary>The distance of this asteroid's orbit in radians.</summary>
		[Tooltip("The distance of this asteroid's orbit in radians.")]
		public float OrbitDistance;

		public void CopyFrom(SgtBeltAsteroid other)
		{
			Variant       = other.Variant;
			Color         = other.Color;
			Radius        = other.Radius;
			Height        = other.Height;
			Angle         = other.Angle;
			Spin          = other.Spin;
			OrbitAngle    = other.OrbitAngle;
			OrbitSpeed    = other.OrbitSpeed;
			OrbitDistance = other.OrbitDistance;
		}
	}
}