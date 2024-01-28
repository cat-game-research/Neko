using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Billboard
{
	/// <summary>This component turns the current GameObject into a billboard that always faces the camera.
	/// This feature can be used with the <b>SpriteRenderer</b> or <b>SgtFlare</b> (Flare feature) components.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBillboard")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Billboard")]
	public class SgtBillboard : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Quaternion LocalRotation;
		}

		/// <summary>If the camera rolls, should this billboard roll with it?</summary>
		public bool RollWithCamera { set { rollWithCamera = value; } get { return rollWithCamera; } } [SerializeField] private bool rollWithCamera = true;

		/// <summary>If your billboard is clipping out of view at extreme angles, then enable this.</summary>
		public bool AvoidClipping { set { avoidClipping = value; } get { return avoidClipping; } } [SerializeField] private bool avoidClipping;

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		[System.NonSerialized]
		public Transform cachedTransform;

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraPreCull   += CameraPreCull;
			SgtCamera.OnCameraPreRender += CameraPreRender;

			cachedTransform = GetComponent<Transform>();
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraPreCull   -= CameraPreCull;
			SgtCamera.OnCameraPreRender -= CameraPreRender;
		}

		private void CameraPreCull(Camera camera)
		{
			Revert();
			{
				var cameraRotation = camera.transform.rotation;
				var rollRotation   = cameraRotation;
				var observer       = default(SgtCamera);
				var rotation       = default(Quaternion);

				if (SgtCamera.TryFind(camera, ref observer) == true)
				{
					rollRotation *= observer.RollQuaternion;
				}

				if (rollWithCamera == true)
				{
					rotation = rollRotation;
				}
				else
				{
					rotation = cameraRotation;
				}

				if (avoidClipping == true)
				{
					var directionA = Vector3.Normalize(transform.position - camera.transform.position);
					var directionB = rotation * Vector3.forward;
					var theta      = Vector3.Angle(directionA, directionB);
					var axis       = Vector3.Cross(directionA, directionB);

					rotation = Quaternion.AngleAxis(theta, -axis) * rotation;
				}

				cachedTransform.rotation = rotation;
			}
			Save(camera);
		}

		private void CameraPreRender(Camera camera)
		{
			Restore(camera);
		}

		public void Save(Camera camera)
		{
			var cameraState = SgtCameraState.Find(ref cameraStates, camera);
		
			cameraState.LocalRotation = transform.localRotation;
		}

		private void Restore(Camera camera)
		{
			var cameraState = SgtCameraState.Restore(cameraStates, camera);

			if (cameraState != null)
			{
				transform.localRotation = cameraState.LocalRotation;
			}
		}

		public void Revert()
		{
			transform.localRotation = Quaternion.identity;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Billboard
{
	using UnityEditor;
	using TARGET = SgtBillboard;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBillboard_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("rollWithCamera", "If the camera rolls, should this billboard roll with it?");
			Draw("avoidClipping", "If your billboard is clipping out of view at extreme angles, then enable this.");
		}
	}
}
#endif