using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component makes the current <b>SgtPoint</b> follow the <b>Target SgtPoint</b>.
	/// This is useful because you can't parent/child <b>SgtPoint</b> components like <b>SgtFloatingCamera</b> and <b>SgtFloatingObject</b>.</summary>
	[DefaultExecutionOrder(200)]
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFloatingPoint))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingFollow")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Follow")]
	public class SgtFloatingFollow : MonoBehaviour
	{
		/// <summary>This allows you to specify the <b>SgtFloatingPoint</b> this component will follow.</summary>
		public SgtFloatingPoint Target { set { target = value; } get { return target; } } [SerializeField] private SgtFloatingPoint target;

		/// <summary>How quickly this point follows the target.
		/// -1 = instant.</summary>
		public float Damping { set { damping = value; } get { return damping; } } [SerializeField] private float damping = -1.0f;

		/// <summary>Should this transform's rotation also match that of the target?</summary>
		public bool Rotate { set { rotate = value; } get { return rotate; } } [SerializeField] private bool rotate;

		/// <summary>This allows you to specify a positional offset relative to the <b>Target</b>.</summary>
		public Vector3 LocalPosition { set { localPosition = value; } get { return localPosition; } } [SerializeField] private Vector3 localPosition;

		/// <summary>This allows you to specify a rotational offset relative to the <b>Target</b>.</summary>
		public Vector3 LocalRotation { set { localRotation = value; } get { return localRotation; } } [SerializeField] private Vector3 localRotation;

		[System.NonSerialized]
		private SgtFloatingPoint cachedPoint;

		protected virtual void OnEnable()
		{
			cachedPoint = GetComponent<SgtFloatingPoint>();
		}

		protected virtual void Update()
		{
			if (target != null)
			{
				var currentPosition = cachedPoint.Position;
				var targetRotation  = target.transform.rotation;
				var targetPosition  = target.Position + targetRotation * localPosition;
				var factor          = CwHelper.DampenFactor(damping, Time.deltaTime);

				cachedPoint.SetPosition(SgtPosition.Lerp(ref currentPosition, ref targetPosition, factor));

				if (rotate == true)
				{
					targetRotation *= Quaternion.Euler(localRotation);

					transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, factor);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingFollow;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingFollow_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Target == null));
				Draw("target", "This allows you to specify the <b>SgtFloatingPoint</b> this component will follow.");
			EndError();
			Draw("damping", "How quickly this point follows the target.\n\n-1 = instant.");
			Draw("rotate", "Should this transform's rotation also match that of the target?");
			Draw("localPosition", "This allows you to specify a positional offset relative to the Target.");
			Draw("localRotation", "This allows you to specify a rotational offset relative to the Target.");
		}
	}
}
#endif