using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.LightAndShadow
{
	/// <summary>This component allows you to cast a ring shadow from the current GameObject.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtShadowRing")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shadow Ring")]
	public class SgtShadowRing : SgtShadow
	{
		/// <summary>The texture of the shadow (left = inside, right = outside).</summary>
		public Texture Texture { set { texture = value; } get { return texture; } } [SerializeField] private Texture texture;

		/// <summary>The inner radius of the ring casting this shadow (auto set if Ring is set).</summary>
		public float RadiusMin { set { radiusMin = value; } get { return radiusMin; } } [SerializeField] private float radiusMin = 1.0f;

		/// <summary>The outer radius of the ring casting this shadow (auto set if Ring is set).</summary>
		public float RadiusMax { set { radiusMax = value; } get { return radiusMax; } } [SerializeField] private float radiusMax = 2.0f;

		public override bool CalculateShadow(SgtLight light, ref Clone clone)
		{
			if (texture != null)
			{
				var direction = default(Vector3);
				var position  = default(Vector3);
				var color     = default(Color);
				var intensity = 0.0f;

				SgtLight.Calculate(light, transform.position, 0.0f, null, null, ref position, ref direction, ref color, ref intensity);

				var rotation = Quaternion.FromToRotation(direction, Vector3.back);
				var squash   = Vector3.Dot(direction, transform.up); // Find how squashed the ellipse is based on light direction
				var width    = transform.lossyScale.x * radiusMax;
				var length   = transform.lossyScale.z * radiusMax;
				var axis     = rotation * transform.up; // Find the transformed up axis
				var spin     = Quaternion.LookRotation(Vector3.forward, new Vector2(-axis.x, axis.y)); // Orient the shadow ellipse
				var scale    = new Vector3(CwHelper.Reciprocal(width), CwHelper.Reciprocal(length * Mathf.Abs(squash)), 1.0f);
				var skew     = Mathf.Tan(CwHelper.Acos(-squash));

				var shadowT = Matrix4x4.Translate(-transform.position);
				var shadowR = Matrix4x4.Rotate(spin * rotation); // Spin the shadow so lines up with its tilt
				var shadowS = Matrix4x4.Scale(scale); // Scale the ring into an oval
				var shadowK = ShearingZ(new Vector2(0.0f, skew)); // Skew the shadow so it aligns with the ring plane

				clone.Root   = this;
				clone.Matrix = shadowS * shadowK * shadowR * shadowT;
				clone.Ratio  = CwHelper.Divide(radiusMax, radiusMax - radiusMin);
				clone.Radius = CwHelper.UniformScale(transform.lossyScale) * radiusMax;

				return true;
			}
			
			return false;
		}

		private static Matrix4x4 ShearingZ(Vector2 xy) // Z changes with x/y
		{
			var matrix = Matrix4x4.identity;

			matrix.m20 = xy.x;
			matrix.m21 = xy.y;

			return matrix;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(mask, transform.position);

			if (isActiveAndEnabled == true && lights.Count > 0)
			{
				var clone = default(Clone);

				if (CalculateShadow(lights[0], ref clone) == true)
				{
					Gizmos.matrix = clone.Matrix.inverse;

					var distA = 0.0f;
					var distB = 1.0f;
					var scale = 1.0f * Mathf.Deg2Rad;
					var inner = CwHelper.Divide(radiusMin, radiusMax);

					for (var i = 1; i < 10; i++)
					{
						var posA  = new Vector3(0.0f, 0.0f, distA);
						var posB  = new Vector3(0.0f, 0.0f, distB);

						Gizmos.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(0.75f, i) * 0.125f);

						for (var a = 1; a <= 360; a++)
						{
							posA.x = posB.x = Mathf.Sin(a * scale);
							posA.y = posB.y = Mathf.Cos(a * scale);

							Gizmos.DrawLine(posA, posB);

							posA.x = posB.x = posA.x * inner;
							posA.y = posB.y = posA.y * inner;

							Gizmos.DrawLine(posA, posB);
						}

						distA = distB;
						distB = distB * 2.0f;
					}
				}
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.LightAndShadow
{
	using UnityEditor;
	using TARGET = SgtShadowRing;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtShadowRing_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Texture == null));
				Draw("texture", "The texture of the shadow (left = inside, right = outside).");
			EndError();
			BeginError(Any(tgts, t => t.RadiusMin < 0.0f || t.RadiusMin >= t.RadiusMax));
				Draw("radiusMin", "The inner radius of the ring casting this shadow (auto set if Ring is set).");
			EndError();
			BeginError(Any(tgts, t => t.RadiusMax < 0.0f || t.RadiusMin >= t.RadiusMax));
				Draw("radiusMax", "The outer radius of the ring casting this shadow (auto set if Ring is set).");
			EndError();
		}
	}
}
#endif