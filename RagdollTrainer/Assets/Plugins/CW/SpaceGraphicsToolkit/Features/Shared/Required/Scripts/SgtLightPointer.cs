using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component points the current light toward the rendering camera, giving you the illusion that it's a point light. This is useful for distant lights that you want to cast shadows.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Light))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtLightPointer")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Light Pointer")]
	public class SgtLightPointer : MonoBehaviour
	{
		public class CameraState : SgtCameraState
		{
			public Quaternion LocalRotation;
		}

		[System.NonSerialized]
		private Light cachedLight;

		[System.NonSerialized]
		private bool cachedLightSet;

		[System.NonSerialized]
		private List<CameraState> cameraStates;

		public Light CachedLight
		{
			get
			{
				if (cachedLightSet == false)
				{
					cachedLight    = GetComponent<Light>();
					cachedLightSet = true;
				}

				return cachedLight;
			}
		}

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

		private void CameraPreCull(Camera camera)
		{
			Revert();
			{
				transform.forward = camera.transform.position - transform.position;
			}
			Save(camera);
		}

		private void CameraPreRender(Camera camera)
		{
			Restore(camera);
		}

		private void Save(Camera camera)
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

		private void Revert()
		{
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtLightPointer;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtLightPointer_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component points the current light toward the rendering camera, giving you the illusion that it's a point light. This is useful for distant lights that you want to cast shadows.");
			
			if (Any(tgts, t => t.CachedLight.type != LightType.Directional))
			{
				if (HelpButton("The attached light isn't set to be directional.", UnityEditor.MessageType.Warning, "Fix", 30.0f) == true)
				{
					Each(tgts, t => t.CachedLight.type = LightType.Directional);
				}
			}
		}
	}
}
#endif