using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Shapes
{
	/// <summary>This component allows you to define a box shape that can be used by other components to perform actions confined to the volume.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtShapeBox")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shape Box")]
	public class SgtShapeBox : SgtShape
	{
		/// <summary>The min/max size of the box.</summary>
		public Vector3 Extents { set { extents = value; } get { return extents; } } [SerializeField] private Vector3 extents = Vector3.one;

		/// <summary>The transition style between minimum and maximum density.</summary>
		public SgtEase.Type Ease { set { ease = value; } get { return ease; } } [SerializeField] private SgtEase.Type ease = SgtEase.Type.Smoothstep;

		/// <summary>How quickly the density increases when inside the sphere.</summary>
		public float Sharpness { set { sharpness = value; } get { return sharpness; } } [SerializeField] private float sharpness = 1.0f;

		public override float GetDensity(Vector3 worldPoint)
		{
			var localPoint = transform.InverseTransformPoint(worldPoint);
			var distanceX  = Mathf.InverseLerp(extents.x, 0.0f, Mathf.Abs(localPoint.x));
			var distanceY  = Mathf.InverseLerp(extents.y, 0.0f, Mathf.Abs(localPoint.y));
			var distanceZ  = Mathf.InverseLerp(extents.z, 0.0f, Mathf.Abs(localPoint.z));
			var distance01 = Mathf.Min(distanceX, Mathf.Min(distanceY, distanceZ));

			return CwHelper.Sharpness(SgtEase.Evaluate(ease, distance01), sharpness);
		}

		public static SgtShapeBox Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtShapeBox Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Shape Box", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtShapeBox>();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color  = new Color(1.0f, 1.0f, 1.0f, 0.25f);

			for (var i = 0; i <= 10; i++)
			{
				var distance = i * 0.1f;
				var size     = GetDensity(transform.TransformPoint(distance * extents)) * extents;

				Gizmos.DrawWireCube(Vector3.zero, size * 2.0f);
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Shapes
{
	using UnityEditor;
	using TARGET = SgtShapeBox;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtShapeBox_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Extents == Vector3.zero));
				Draw("extents", "The min/max size of the box.");
			EndError();
			Draw("ease", "The transition style between minimum and maximum density.");
			Draw("sharpness", "How quickly the density increases when inside the sphere.");
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Shape/Box", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtShapeBox.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif