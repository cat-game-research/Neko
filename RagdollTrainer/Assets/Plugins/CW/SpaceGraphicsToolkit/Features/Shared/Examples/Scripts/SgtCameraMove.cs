using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to move the current GameObject based on WASD/mouse/finger drags. NOTE: This requires the CwInputManager in your scene to function.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtCameraMove")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Camera Move")]
	public class SgtCameraMove : MonoBehaviour
	{
		public enum RotationType
		{
			None,
			Acceleration,
			MainCamera
		}

		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>How quickly the position goes to the target value (-1 = instant).</summary>
		public float Damping { set { damping = value; } get { return damping; } } [SerializeField] private float damping = 10.0f;

		/// <summary>If you want movements to apply to Rigidbody.velocity, set it here.</summary>
		public Rigidbody Target { set { target = value; } get { return target; } } [SerializeField] private Rigidbody target;

		/// <summary>If the target is something like a spaceship, rotate it based on movement?</summary>
		public RotationType TargetRotation { set { targetRotation = value; } get { return targetRotation; } } [SerializeField] private RotationType targetRotation;

		/// <summary>The speed of the velocity rotation.</summary>
		public float TargetDamping { set { targetDamping = value; } get { return targetDamping; } } [SerializeField] private float targetDamping = 1.0f;

		/// <summary>The movement speed will be multiplied by this when near to planets.</summary>
		public float SpeedMin { set { speedMin = value; } get { return speedMin; } } [SerializeField] private float speedMin = 1.0f;

		/// <summary>The movement speed will be multiplied by this when far from planets.</summary>
		public float SpeedMax { set { speedMax = value; } get { return speedMax; } } [SerializeField] private float speedMax = 10.0f;

		/// <summary>The higher you set this, the faster the <b>SpeedMin</b> value will be reached when approaching planets.</summary>
		public float SpeedRange { set { speedRange = value; } get { return speedRange; } } [SerializeField] private float speedRange = 100.0f;

		/// <summary></summary>
		public float SpeedWheel { set { speedWheel = value; } get { return speedWheel; } } [SerializeField] [Range(0.0f, 0.5f)] private float speedWheel = 0.1f;

		/// <summary>The keys/fingers required to move left/right.</summary>
		public CwInputManager.Axis HorizontalControls { set { horizontalControls = value; } get { return horizontalControls; } } [SerializeField] private CwInputManager.Axis horizontalControls = new CwInputManager.Axis(2, false, CwInputManager.AxisGesture.HorizontalDrag, 1.0f, KeyCode.A, KeyCode.D, KeyCode.LeftArrow, KeyCode.RightArrow, 100.0f);

		/// <summary>The keys/fingers required to move backward/forward.</summary>
		public CwInputManager.Axis DepthControls { set { depthControls = value; } get { return depthControls; } } [SerializeField] private CwInputManager.Axis depthControls = new CwInputManager.Axis(2, false, CwInputManager.AxisGesture.HorizontalDrag, 1.0f, KeyCode.S, KeyCode.W, KeyCode.DownArrow, KeyCode.UpArrow, 100.0f);

		/// <summary>The keys/fingers required to move down/up.</summary>
		public CwInputManager.Axis VerticalControls { set { verticalControls = value; } get { return verticalControls; } } [SerializeField] private CwInputManager.Axis verticalControls = new CwInputManager.Axis(3, false, CwInputManager.AxisGesture.HorizontalDrag, 1.0f, KeyCode.F, KeyCode.R, KeyCode.None, KeyCode.None, 100.0f);

		[System.NonSerialized]
		private Vector3 remainingDelta;

		[System.NonSerialized]
		private Vector3 lastFixedDelta;

		private Vector3 GetDelta(float deltaTime)
		{
			var delta = default(Vector3);

			delta.x = horizontalControls.GetValue(deltaTime);
			delta.y =   verticalControls.GetValue(deltaTime);
			delta.z =      depthControls.GetValue(deltaTime);

			return delta;
		}

		protected virtual void OnEnable()
		{
			CwInputManager.EnsureThisComponentExists();
		}

		protected virtual void Update()
		{
			lastFixedDelta = GetDelta(Time.fixedDeltaTime);

			if (target == null && listen == true)
			{
				AddToDelta(GetDelta(Time.deltaTime));
				DampenDelta();
			}

			if (CwInput.GetMouseExists() == true)
			{
				speedRange *= 1.0f - Mathf.Clamp(CwInput.GetMouseWheelDelta(), -1.0f, 1.0f) * speedWheel;
			}
		}

		protected virtual void FixedUpdate()
		{
			if (target != null && listen == true)
			{
				AddToDelta(lastFixedDelta);
				DampenDelta();
			}
		}

		private float GetSpeedMultiplier()
		{
			if (speedMax > 0.0f)
			{
				var distance = float.PositiveInfinity;

				SgtCommon.InvokeCalculateDistance(transform.position, ref distance);

				var distance01 = Mathf.InverseLerp(speedMin * speedRange, speedMax * speedRange, distance);

				return Mathf.Lerp(speedMin, speedMax, distance01);
			}

			return 0.0f;
		}

		private void AddToDelta(Vector3 delta)
		{
			// Store old position
			var oldPosition = transform.position;

			// Translate
			transform.Translate(delta * GetSpeedMultiplier(), Space.Self);

			// Add to remaining
			var acceleration = transform.position - oldPosition;

			remainingDelta += acceleration;

			// Revert position
			transform.position = oldPosition;

			// Rotate to acceleration?
			if (target != null && targetRotation != RotationType.None && delta != Vector3.zero)
			{
				var factor   = CwHelper.DampenFactor(targetDamping, Time.deltaTime);
				var rotation = target.transform.rotation;

				switch (targetRotation)
				{
					case RotationType.Acceleration:
					{
						rotation = Quaternion.LookRotation(acceleration, target.transform.up);
					}
					break;

					case RotationType.MainCamera:
					{
						var camera = Camera.main;

						if (camera != null)
						{
							rotation = camera.transform.rotation;
						}
					}
					break;
				}

				target.transform.rotation = Quaternion.Slerp(target.transform.rotation, rotation, factor);
				target.angularVelocity    = Vector3.Lerp(target.angularVelocity, Vector3.zero, factor);
			}
		}

		private void DampenDelta()
		{
			// Dampen remaining delta
			var factor   = CwHelper.DampenFactor(damping, Time.deltaTime);
			var newDelta = Vector3.Lerp(remainingDelta, Vector3.zero, factor);

			// Translate by difference
			if (target != null)
			{
				target.velocity += remainingDelta - newDelta;
			}
			else
			{
				transform.position += remainingDelta - newDelta;
			}

			// Update remaining
			remainingDelta = newDelta;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtCameraMove;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtCameraMove_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("listen", "Is this component currently listening for inputs?");
			Draw("damping", "How quickly the position goes to the target value (-1 = instant).");

			Separator();

			Draw("target", "If you want movements to apply to Rigidbody.velocity, set it here.");
			Draw("targetRotation", "If the target is something like a spaceship, rotate it based on movement?");
			Draw("targetDamping", "The speed of the velocity rotation.");

			Separator();

			Draw("speedMin", "The movement speed will be multiplied by this when near to planets.");
			Draw("speedMax", "The movement speed will be multiplied by this when far from planets.");
			Draw("speedRange", "The higher you set this, the faster the <b>SpeedMin</b> value will be reached when approaching planets.");
			Draw("speedWheel");

			Separator();

			Draw("horizontalControls", "The keys/fingers required to move right/left.");
			Draw("depthControls", "The keys/fingers required to move backward/forward.");
			Draw("verticalControls", "The keys/fingers required to move down/up.");
		}
	}
}
#endif