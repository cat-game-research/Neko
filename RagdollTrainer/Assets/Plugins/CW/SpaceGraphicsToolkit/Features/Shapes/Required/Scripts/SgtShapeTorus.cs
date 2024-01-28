using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Shapes
{
	/// <summary>This component allows you to define a torus shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtShapeTorus")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shape Torus")]
	public class SgtShapeTorus : SgtShape
	{
		/// <summary>The radius of this torus in local coordinates.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The radial thickness of the torus in local space.</summary>
		public float Thickness { set { thickness = value; } get { return thickness; } } [SerializeField] private float thickness = 0.1f;

		/// <summary>The transition style between minimum and maximum density.</summary>
		public SgtEase.Type Ease { set { ease = value; } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the torus.</summary>
		public float Sharpness { set { sharpness = value; } get { return sharpness; } } [SerializeField] private float sharpness = 1.0f;

		public override float GetDensity(Vector3 worldPoint)
		{
			var localPoint = transform.InverseTransformPoint(worldPoint);
			var distanceXZ = Mathf.Sqrt(localPoint.x * localPoint.x + localPoint.z * localPoint.z) - radius;
			var distanceY  = localPoint.y;
			var distance   = Mathf.Sqrt(distanceXZ * distanceXZ + distanceY * distanceY);
			var distance01 = Mathf.InverseLerp(thickness, 0.0f, distance);

			return CwHelper.Sharpness(SgtEase.Evaluate(ease, distance01), sharpness);
		}

		public static SgtShapeTorus Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeTorus Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Shape Torus", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtShapeTorus>();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			var rotationA = Quaternion.identity;

			for (var i = 1; i <= 36; i++)
			{
				var rotationB = Quaternion.Euler(0.0f, i * 10.0f, 0.0f);

				for (var j = 0; j < 10; j++)
				{
					var worldPoint = transform.TransformPoint(0.0f, 0.0f, radius + thickness * j * 0.1f);
					var density    = GetDensity(worldPoint);

					for (var k = 0; k < 36; k++)
					{
						var angA = (k * 10.0f        ) * Mathf.Deg2Rad;
						var angB = (k * 10.0f + 10.0f) * Mathf.Deg2Rad;
						var rad  = thickness * density;
						var a    = rotationA * new Vector3(0.0f, Mathf.Sin(angA) * rad, radius + Mathf.Cos(angA) * rad);
						var b    = rotationA * new Vector3(0.0f, Mathf.Sin(angB) * rad, radius + Mathf.Cos(angB) * rad);
						var c    = rotationB * new Vector3(0.0f, Mathf.Sin(angA) * rad, radius + Mathf.Cos(angA) * rad);
						var d    = rotationB * new Vector3(0.0f, Mathf.Sin(angB) * rad, radius + Mathf.Cos(angB) * rad);

						Gizmos.DrawLine(a, b);
						Gizmos.DrawLine(c, d);
						
						Gizmos.DrawLine(a, c);
						Gizmos.DrawLine(b, d);
					}
				}

				rotationA = rotationB;
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Shapes
{
	using UnityEditor;
	using TARGET = SgtShapeTorus;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtShapeTorus_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", "The radius of this sphere in local coordinates.");
			EndError();
			BeginError(Any(tgts, t => t.Thickness <= 0.0f));
				Draw("thickness", "The radial thickness of the torus in local space.");
			EndError();
			Draw("ease", "The transition style between minimum and maximum density.");
			Draw("sharpness", "How quickly the density increases when inside the sphere.");
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Shape/Torus", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtShapeTorus.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif