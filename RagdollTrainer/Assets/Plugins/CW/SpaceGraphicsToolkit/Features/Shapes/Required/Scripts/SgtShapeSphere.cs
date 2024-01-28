using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Shapes
{
	/// <summary>This component allows you to define a sphere shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtShapeSphere")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shape Sphere")]
	public class SgtShapeSphere : SgtShape
	{
		/// <summary>The radius of this sphere in local space.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>If this is set, then any point within <b>Radius</b> will have maximum density, and it will fall off to zero within the <b>height</b> range.</summary>
		public float Height { set { height = value; } get { return height; } } [SerializeField] private float height;

		/// <summary>The transition style between minimum and maximum density.</summary>
		public SgtEase.Type Ease { set { ease = value; } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the sphere.</summary>
		public float Sharpness { set { sharpness = value; } get { return sharpness; } } [SerializeField] private float sharpness = 1.0f;

		public float RadiusInner
		{
			get
			{
				return height == 0.0f ? 0.0f : radius;
			}
		}

		public float RadiusOuter
		{
			get
			{
				return radius + height;
			}
		}

		public override float GetDensity(Vector3 worldPoint)
		{
			var distance   = transform.InverseTransformPoint(worldPoint).magnitude;
			var distance01 = Mathf.InverseLerp(RadiusOuter, RadiusInner, distance);

			return CwHelper.Sharpness(SgtEase.Evaluate(ease, distance01), sharpness);
		}

		public static SgtShapeSphere Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeSphere Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Shape Sphere", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtShapeSphere>();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			Gizmos.DrawWireSphere(Vector3.zero, RadiusInner);
			Gizmos.DrawWireSphere(Vector3.zero, RadiusOuter);

			for (var i = 0; i < 11; i++)
			{
				var distance   = Mathf.Lerp(RadiusInner, RadiusOuter, i * 0.1f);
				var worldPoint = transform.TransformPoint(0.0f, 0.0f, distance);

				Gizmos.color = new Color(1.0f, 1.0f, 1.0f, GetDensity(worldPoint));

				Gizmos.DrawWireSphere(Vector3.zero, distance);
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Shapes
{
	using UnityEditor;
	using TARGET = SgtShapeSphere;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtShapeSphere_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", "The radius of this sphere in local coordinates.");
			EndError();
			Draw("height", "If this is set, then any point within Radius will have maximum density, and it will fall off to zero within the height range.");
			Draw("ease", "The transition style between minimum and maximum density.");
			Draw("sharpness", "How quickly the density increases when inside the sphere.");
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Shape/Sphere", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtShapeSphere.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif