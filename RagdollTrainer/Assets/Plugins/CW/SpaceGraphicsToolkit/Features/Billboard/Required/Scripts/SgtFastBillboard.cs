using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit.Billboard
{
	/// <summary>This component rotates the current GameObject to the rendering camera.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFastBillboard")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Fast Billboard")]
	public class SgtFastBillboard : MonoBehaviour
	{
		/// <summary>If the camera rolls, should this billboard roll with it?</summary>
		public bool RollWithCamera { set { rollWithCamera = value; } get { return rollWithCamera; } } [SerializeField] private bool rollWithCamera = true;

		/// <summary>If your billboard is clipping out of view at extreme angles, then enable this.</summary>
		public bool AvoidClipping { set { avoidClipping = value; } get { return avoidClipping; } } [SerializeField] private bool avoidClipping;

		[HideInInspector]
		public Quaternion Rotation = Quaternion.identity;

		[System.NonSerialized]
		public int Mask;

		[System.NonSerialized]
		public Transform cachedTransform;

		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtFastBillboard> Instances = new LinkedList<SgtFastBillboard>(); private LinkedListNode<SgtFastBillboard> node;

		public void RandomlyRotate(int seed)
		{
			Rotation = Quaternion.Euler(0.0f, 0.0f, Random.value * 360.0f);
		}

		protected virtual void OnEnable()
		{
			node = Instances.AddLast(this);

			Mask = 1 << gameObject.layer;

			cachedTransform = GetComponent<Transform>();
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Billboard
{
	using UnityEditor;
	using TARGET = SgtFastBillboard;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFastBillboard_Editor : CwEditor
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