using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component scales the current GameObject based on optical thickness between the current camera and the current position.
	/// This is done using the <b>SgtOcclusion.Calculate</b> method, which works using <b>Physics.Raycast</b>, and custom occlusion checks.
	/// You can add your own custom occlusion checks by hooking into the <b>SgtHelper.OnCalculateOcclusion</b> event.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtOcclusionScaler")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Occlusion Scaler")]
	public class SgtOcclusionScaler : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Vector3 LocalScale;
		}

		/// <summary>The layers that will be sampled when calculating the occlusion.</summary>
		public LayerMask Layers { set { layers = value; } get { return layers; } } [SerializeField] private LayerMask layers = Physics.DefaultRaycastLayers;

		/// <summary>This allows you to set the maximum scale when there is no depth.</summary>
		public Vector3 MaxScale { set { maxScale = value; } get { return maxScale; } } [SerializeField] private Vector3 maxScale = Vector3.one;

		/// <summary>This allows you to set the radius of the object that will be scaled (e.g. light flare). This changes how it get occluded by objects that pass in front.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>Raycasts begin to break down beyond a certain distance depending on your scene scale, and this allows you to adjust that distance if required.</summary>
		public float MaxRaycastDistance { set { maxRaycastDistance = value; } get { return maxRaycastDistance; } } [SerializeField] private float maxRaycastDistance = 1000000.0f;

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraPreCull   += CameraPreCull;
			SgtCamera.OnCameraPreRender += CameraPreRender;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraPreCull   -= CameraPreCull;
			SgtCamera.OnCameraPreRender -= CameraPreRender;
		}

		private void Restore(Camera camera)
		{
			var cameraState = SgtCameraState.Restore(cameraStates, camera);

			if (cameraState != null)
			{
				transform.localScale = cameraState.LocalScale;
			}
		}

		private void CameraPreCull(Camera camera)
		{
			var cameraState = SgtCameraState.Find(ref cameraStates, camera);
			var eye         = camera.transform.position;
			var tgt         = transform.position;

			var occlusion   = SgtOcclusion.Calculate(layers, new Vector4(eye.x, eye.y, eye.z, 0.0f), new Vector4(tgt.x, tgt.y, tgt.z, radius), maxRaycastDistance);

			transform.localScale = maxScale * Mathf.Clamp01(1.0f - occlusion);

			// Store scale
			cameraState.LocalScale = transform.localScale;
		}

		private void CameraPreRender(Camera camera)
		{
			Restore(camera);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtOcclusionScaler;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtDepthScale_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("layers", "The layers that will be sampled when calculating the occlusion."); // Updated automatically
			Draw("radius", "This allows you to set the radius of the object that will be scaled (e.g. light flare). This changes how it get occluded by objects that pass in front."); // Updated automatically
			Draw("maxScale", "This allows you to set the maximum scale when there is no depth."); // Updated automatically
			Draw("maxRaycastDistance", "Raycasts begin to break down beyond a certain distance depending on your scene scale, and this allows you to adjust that distance if required.");
		}
	}
}
#endif