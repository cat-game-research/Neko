using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component makes the current gameObject orbit around its parent in a basic circle or ellipse shape.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Simple Orbit")]
	public class SgtSimpleOrbit : MonoBehaviour
	{
		/// <summary>The radius of the orbit in local coordinates.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>How squashed the orbit is.</summary>
		public Vector2 Scale { set { scale = value; } get { return scale; } } [SerializeField] private Vector2 scale = Vector2.one;

		/// <summary>The local position offset of the orbit.</summary>
		public Vector3 Offset { set { offset = value; } get { return offset; } } [SerializeField] private Vector3 offset;

		/// <summary>The local rotation offset of the orbit in degrees.</summary>
		public Vector3 Tilt { set { tilt = value; } get { return tilt; } } [SerializeField] private Vector3 tilt;

		/// <summary>The current position along the orbit in degrees.</summary>
		public float Angle { set { angle = value; } get { return angle; } } [SerializeField] private float angle;

		/// <summary>The orbit speed.</summary>
		public float DegreesPerSecond { set { degreesPerSecond = value; } get { return degreesPerSecond; } } [SerializeField] private float degreesPerSecond = 10.0f;

		protected virtual void Update()
		{
			if (Application.isPlaying == true)
			{
				angle += degreesPerSecond * Time.deltaTime;
			}

			var localPosition = offset;

			localPosition.x += Mathf.Sin(angle * Mathf.Deg2Rad) * Radius * scale.x;
			localPosition.z += Mathf.Cos(angle * Mathf.Deg2Rad) * Radius * scale.y;

			transform.localPosition = Quaternion.Euler(tilt) * localPosition;
		}
	
#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (isActiveAndEnabled == true)
			{
				if (transform.parent != null)
				{
					Gizmos.matrix = transform.parent.localToWorldMatrix;
				}

				var rotation = Quaternion.Euler(tilt);

				SgtCommon.DrawCircle(offset, rotation * Vector3.right * Radius * scale.x, rotation * Vector3.forward * Radius * scale.y);

				Gizmos.DrawLine(Vector3.zero, transform.localPosition);
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtSimpleOrbit;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSimpleOrbit_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Radius == 0.0f));
				Draw("radius", "The radius of the orbit in local coordinates.");
			EndError();
			Draw("scale", "How squashed the orbit is.");

			Separator();

			Draw("offset", "The local position offset of the orbit.");
			Draw("tilt", "The local rotation offset of the orbit in degrees.");

			Separator();

			Draw("angle", "The current position along the orbit in degrees.");
			Draw("degreesPerSecond", "The orbit speed.");
		}
	}
}
#endif