using UnityEngine;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This class stores information about a single star inside a starfield. These can either be manually populated using a custom starfield, or they're procedurally generated.</summary>
	[System.Serializable]
	public class SgtStarfieldStar
	{
		// Temp instance used when generating the starfield
		public static SgtStarfieldStar Temp = new SgtStarfieldStar();

		/// <summary>The coordinate index in the asteroid texture.</summary>
		[Tooltip("The coordinate index in the asteroid texture.")]
		public int Variant;

		/// <summary>Color tint of this star.</summary>
		[Tooltip("Color tint of this star.")]
		public Color Color = Color.white;

		/// <summary>Radius of this star in local space.</summary>
		[Tooltip("Radius of this star in local space.")]
		public float Radius;

		/// <summary>Angle in degrees.</summary>
		[Tooltip("Angle in degrees.")]
		public float Angle;

		/// <summary>Local position of this star relative to the starfield.</summary>
		[Tooltip("Local position of this star relative to the starfield.")]
		public Vector3 Position;

		/// <summary>How fast this star pulses.
		/// NOTE: This requires the starfield material's PULSE setting to be enabled.</summary>
		[Tooltip("How fast this star pulses.\n\nNOTE: This requires the starfield material's PULSE setting to be enabled.")]
		[Range(0.0f, 1.0f)]
		public float PulseSpeed = 1.0f;

		/// <summary>The pulse position will be offset by this value so they don't all pulse the same.
		/// NOTE: This requires the starfield material's PULSE setting to be enabled.</summary>
		[Tooltip("The pulse position will be offset by this value so they don't all pulse the same.\n\nNOTE: This requires the starfield material's PULSE setting to be enabled.")]
		[Range(0.0f, 1.0f)]
		public float PulseOffset;

		public void CopyFrom(SgtStarfieldStar other)
		{
			Variant     = other.Variant;
			Color       = other.Color;
			Radius      = other.Radius;
			Angle       = other.Angle;
			Position    = other.Position;
			PulseSpeed  = other.PulseSpeed;
			PulseOffset = other.PulseOffset;
		}
	}
}