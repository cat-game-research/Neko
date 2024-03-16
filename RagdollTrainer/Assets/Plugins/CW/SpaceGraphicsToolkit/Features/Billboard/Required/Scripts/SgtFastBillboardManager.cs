using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit.Billboard
{
	/// <summary>All SgtFastBillboards will be updated from here.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-100)]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFastBillboardManager")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Fast Billboard Manager")]
	public class SgtFastBillboardManager : MonoBehaviour
	{
		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtFastBillboardManager> Instances = new LinkedList<SgtFastBillboardManager>(); private LinkedListNode<SgtFastBillboardManager> node;

		protected virtual void OnEnable()
		{
			if (Instances.Count > 0)
			{
				Debug.LogWarning("Your scene already contains an instance of SgtFastBillboardManager!", Instances.First.Value);
			}

			node = Instances.AddLast(this);

			SgtCamera.OnCameraPreCull += PreCull;
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;

			SgtCamera.OnCameraPreCull -= PreCull;
		}

		private void PreCull(Camera camera)
		{
			if (node == Instances.First)
			{
				var cameraRotation = camera.transform.rotation;
				var rollRotation   = cameraRotation;
				var observer       = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref observer) == true)
				{
					rollRotation *= observer.RollQuaternion;
				}

				var mask      = camera.cullingMask;
				var position  = camera.transform.position;

				foreach (var billboard in SgtFastBillboard.Instances)
				{
					if ((billboard.Mask & mask) != 0)
					{
						var rotation = default(Quaternion);

						if (billboard.RollWithCamera == true)
						{
							rotation = rollRotation * billboard.Rotation;
						}
						else
						{
							rotation = cameraRotation * billboard.Rotation;
						}

						if (billboard.AvoidClipping == true)
						{
							var directionA = Vector3.Normalize(billboard.transform.position - position);
							var directionB = rotation * Vector3.forward;
							var theta      = Vector3.Angle(directionA, directionB);
							var axis       = Vector3.Cross(directionA, directionB);

							rotation = Quaternion.AngleAxis(theta, -axis) * rotation;
						}

						billboard.cachedTransform.rotation = rotation;
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Billboard
{
	using UnityEditor;
	using TARGET = SgtFastBillboardManager;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFastBillboardManager_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("This component marks where all spawned SgtFloatingObjects will be attached to.");
		}
	}
}
#endif