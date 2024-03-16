using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component moves the camera at the start of the scene.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtCameraPath")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Camera Path")]
	public class SgtCameraPath : MonoBehaviour
	{
		[System.Serializable]
		public struct CameraState
		{
			public Vector3 Position;
			public Vector3 Rotation;
		}

		public int Target { set { target = value; } get { return target; } } [SerializeField] private int target = -1;

		public float Damping { set { damping = value; } get { return damping; } } [SerializeField] private float damping = 5.0f;

		public float ThresholdPosition { set { thresholdPosition = value; } get { return thresholdPosition; } } [SerializeField] private float thresholdPosition = 0.1f;

		public float ThresholdRotation { set { thresholdRotation = value; } get { return thresholdRotation; } } [SerializeField] private float thresholdRotation = 0.1f;

		public bool SnapOnAwake { set { snapOnAwake = value; } get { return snapOnAwake; } } [SerializeField] private bool snapOnAwake;

		public Vector3 SnapPosition { set { snapPosition = value; } get { return snapPosition; } } [SerializeField] private Vector3 snapPosition;

		public Vector3 SnapRotation { set { snapRotation = value; } get { return snapRotation; } } [SerializeField] private Vector3 snapRotation;

		public bool AllowShortcuts { set { allowShortcuts = value; } get { return allowShortcuts; } } [SerializeField] private bool allowShortcuts;

		public List<CameraState> States { set { states = value; } get { return states; } } [SerializeField] private List<CameraState> states;

		[ContextMenu("Add As State")]
		public void AddAsState()
		{
			if (states == null)
			{
				states = new List<CameraState>();
			}

			var state = default(CameraState);

			state.Position = transform.position;
			state.Rotation = transform.eulerAngles;

			states.Add(state);
		}

		[ContextMenu("Snap To State")]
		public void SnapToState()
		{
			if (states != null && target >= 0 && target < states.Count)
			{
				var state = states[target];

				transform.position = state.Position;
				transform.rotation = Quaternion.Euler(state.Rotation);
			}
		}

		public void GoToState(int index)
		{
			target = index;
		}

		protected virtual void Awake()
		{
			if (snapOnAwake == true)
			{
				transform.position = snapPosition;
				transform.rotation = Quaternion.Euler(snapRotation);
			}
		}

		protected virtual void Update()
		{
			for (var i = 0; i < 9; i++)
			{
				if (CwInput.GetKeyWentDown(KeyCode.F1 + i) == true)
				{
					GoToState(i);
				}
			}

			if (states != null && target >= 0 && target < states.Count)
			{
				var state  = states[target];
				var tgtPos = state.Position;
				var tgtRot = Quaternion.Euler(state.Rotation);
				var factor = CwHelper.DampenFactor(damping, Time.deltaTime);

				transform.position = Vector3.Lerp(transform.position, tgtPos, factor);
				transform.rotation = Quaternion.Slerp(transform.rotation, tgtRot, factor);

				if (Vector3.Distance(transform.position, tgtPos) <= thresholdPosition && Quaternion.Angle(transform.rotation, tgtRot) < thresholdRotation)
				{
					target = -1;
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtCameraPath;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtCameraPath_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("target");
			Draw("damping");
			Draw("thresholdPosition");
			Draw("thresholdRotation");

			Separator();

			Draw("snapOnAwake");
			Draw("snapPosition");
			Draw("snapRotation");
			Draw("allowShortcuts");

			Separator();

			Draw("states");
		}
	}
}
#endif