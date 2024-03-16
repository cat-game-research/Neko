using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class handles ease types used for various SGT features like atmosphere color falloff.</summary>
	public static class SgtEase
	{
		public enum Type
		{
			Linear,
			Smoothstep,
			Sinusoidial,
			Quadratic,
			Circular,
			Cubic,
			Quartic,
			Quintic,
			Exponential
		}

		public static float Evaluate(Type ease, float t)
		{
			switch (ease)
			{
				case Type.Linear:
				return t;

				case Type.Smoothstep:
				return t * t * (3.0f - 2.0f * t);

				case Type.Sinusoidial:
				return Mathf.Sin(t * (Mathf.PI/2.0f));

				case Type.Quadratic:
				return -1.0f * t*(t-2.0f);

				case Type.Circular:
					t -= 1.0f;
				return Mathf.Sqrt(1.0f - t*t);

				case Type.Cubic:
					t -= 1.0f;
				return t*t*t + 1.0f;

				case Type.Quartic:
					t -= 1.0f;
				return -1.0f * (t*t*t*t - 1.0f);

				case Type.Quintic:
					t -= 1.0f;
				return t*t*t*t*t + 1.0f;

				case Type.Exponential:
				return -Mathf.Pow(2.0f, -10.0f * t) + 1.0f;
			}
		
			return t;
		}
	}
}