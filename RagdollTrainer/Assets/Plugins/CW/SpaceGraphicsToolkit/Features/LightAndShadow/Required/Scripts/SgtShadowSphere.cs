using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.LightAndShadow
{
	/// <summary>This component allows you to cast a sphere shadow from the current GameObject.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "CwShadowSphere")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shadow Sphere")]
	public class SgtShadowSphere : SgtShadow
	{
		/// <summary>The sharpness of the sunset red channel transition.</summary>
		public float SharpnessR { set { if (sharpnessR != value) { sharpnessR = value; } } get { return sharpnessR; } } [SerializeField] private float sharpnessR = 10.0f;

		/// <summary>The power of the sunset green channel transition.</summary>
		public float SharpnessG { set { if (sharpnessG != value) { sharpnessG = value; } } get { return sharpnessG; } } [SerializeField] private float sharpnessG = 10.0f;

		/// <summary>The power of the sunset blue channel transition.</summary>
		public float SharpnessB { set { if (sharpnessB != value) { sharpnessB = value; } } get { return sharpnessB; } } [SerializeField] private float sharpnessB = 10.0f;

		/// <summary>The outer radius of the sphere in local space.</summary>
		public float RadiusMax { set { if (radiusMax != value) { radiusMax = value; } } get { return radiusMax; } } [SerializeField] private float radiusMax = 1.1f;

		[SerializeField]
		[HideInInspector]
		private bool startCalled;

		public override bool CalculateShadow(SgtLight light, ref Clone clone)
		{
			var direction = default(Vector3);
			var position  = default(Vector3);
			var color     = default(Color);
			var intensity = 0.0f;

			SgtLight.Calculate(light, transform.position, 0.0f, null, null, ref position, ref direction, ref color, ref intensity);

			var dot      = Vector3.Dot(direction, transform.up);
			var radiusXZ = (transform.lossyScale.x + transform.lossyScale.z) * 0.5f * radiusMax;
			var radiusY  = transform.lossyScale.y * radiusMax;
			var radius   = GetRadius(radiusY, radiusXZ, dot * Mathf.PI * 0.5f);
			var rotation = Quaternion.FromToRotation(direction, Vector3.back);
			var vector   = rotation * transform.up;
			var spin     = Quaternion.LookRotation(Vector3.forward, new Vector2(-vector.x, vector.y)); // Orient the shadow ellipse
			var scale    = new Vector3(CwHelper.Reciprocal(radiusXZ), CwHelper.Reciprocal(radius), 1.0f);
			var shadowT  = Matrix4x4.Translate(-transform.position);
			var shadowR  = Matrix4x4.Rotate(spin * rotation);
			var shadowS  = Matrix4x4.Scale(scale);

			clone.Root     = this;
			clone.Matrix   = shadowS * shadowR * shadowT;
			//clone.Ratio   = CwHelper.Divide(radiusMax, radiusMax - radiusMin);
			clone.Power.x  = sharpnessR;
			clone.Power.y  = sharpnessG;
			clone.Power.z  = sharpnessB;
			clone.Power.w  = Mathf.Max(Mathf.Max(clone.Power.x, clone.Power.y), clone.Power.z);
			clone.Radius   = CwHelper.UniformScale(transform.lossyScale) * radiusMax;

			return true;
		}

		private float GetRadius(float a, float b, float theta)
		{
			var s = Mathf.Sin(theta);
			var c = Mathf.Cos(theta);
			var z = Mathf.Sqrt((a*a)*(s*s)+(b*b)*(c*c));

			if (z != 0.0f)
			{
				return (a * b) / z;
			}

			return a;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var mask   = 1 << gameObject.layer;
			var lights = SgtLight.Find(mask, transform.position);

			if (isActiveAndEnabled == true && lights.Count > 0)
			{
				Gizmos.matrix = transform.localToWorldMatrix;

				Gizmos.DrawWireSphere(Vector3.zero, radiusMax);

				var clone = default(Clone);

				if (CalculateShadow(lights[0], ref clone) == true)
				{
					Gizmos.matrix = clone.Matrix.inverse;

					var distA = 0.0f;
					var distB = 1.0f;
					var scale = 1.0f * Mathf.Deg2Rad;

					for (var i = 0; i < 10; i++)
					{
						var posA  = new Vector3(0.0f, 0.0f, distA);
						var posB  = new Vector3(0.0f, 0.0f, distB);

						Gizmos.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Pow(0.75f, i) * 0.125f);

						for (var a = 1; a <= 360; a++)
						{
							posA.x = posB.x = Mathf.Sin(a * scale);
							posA.y = posB.y = Mathf.Cos(a * scale);

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
	using TARGET = SgtShadowSphere;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtShadowSphere_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("sharpnessR", "The sharpness of the sunset red channel transition.");
			Draw("sharpnessG", "The sharpness of the sunset green channel transition.");
			Draw("sharpnessB", "The sharpness of the sunset blue channel transition.");
			BeginError(Any(tgts, t => t.RadiusMax < 0.0f));
				Draw("radiusMax", "The outer radius of the sphere in local space.");
			EndError();
		}
	}
}
#endif