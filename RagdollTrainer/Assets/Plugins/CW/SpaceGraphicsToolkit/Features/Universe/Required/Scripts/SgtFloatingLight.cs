using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will rotate the current GameObject toward the SgtFloatingOrigin point. This makes directional lights compatible with the Universe feature.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingLight")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Light")]
	public class SgtFloatingLight : MonoBehaviour
	{
		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtFloatingLight> Instances = new LinkedList<SgtFloatingLight>(); private LinkedListNode<SgtFloatingLight> node;

		protected virtual void OnEnable()
		{
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
			if (SgtFloatingCamera.Instances.Count > 0)
			{
				var floatingCamera = SgtFloatingCamera.Instances.First.Value;

				transform.forward = floatingCamera.transform.position - transform.position;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingLight;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingLight_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);
		}
	}
}
#endif