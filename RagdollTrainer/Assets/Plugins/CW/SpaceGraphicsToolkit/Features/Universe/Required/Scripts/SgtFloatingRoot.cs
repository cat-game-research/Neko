using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>All prefabs spawned from SgtFloatingLod and SgtFloatingSpawner___ will be attached to this GameObject.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-100)]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingRoot")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Root")]
	public class SgtFloatingRoot : MonoBehaviour
	{
		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtFloatingRoot> Instances = new LinkedList<SgtFloatingRoot>(); private LinkedListNode<SgtFloatingRoot> node;

		public static Transform Root
		{
			get
			{
				if (Instances.Count > 0)
				{
					return Instances.First.Value.transform;
				}

				return null;
			}
		}

		public static Transform GetRoot()
		{
			if (Instances.Count == 0)
			{
				new GameObject("SgtFloatingRoot").AddComponent<SgtFloatingRoot>();
			}

			return Instances.First.Value.transform;
		}

		protected virtual void OnEnable()
		{
			if (Instances.Count > 0)
			{
				Debug.LogWarning("Your scene already contains an instance of SgtFloatingRoot!", Instances.First.Value);
			}

			node = Instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingRoot;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingRoot_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Info("All prefabs spawned from SgtFloatingLod and SgtFloatingSpawner___ will be attached to this GameObject.");
		}
	}
}
#endif