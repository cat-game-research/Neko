using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a gravity source, which can be used to attract Rigidbodies with the SgtGravityReceiver attached.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtGravitySource")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Gravity Source")]
	public class SgtGravitySource : MonoBehaviour
	{
		/// <summary>The mass of this gravity source.</summary>
		public float Mass { set { mass = value; } get { return mass; } } [SerializeField] private float mass = 100.0f;

		/// <summary>If you enable this then the Mass setting will be automatically copied from the attached Rigidbody.</summary>
		public bool AutoSetMass { set { autoSetMass = value; } get { return autoSetMass; } } [SerializeField] private bool autoSetMass;

		public static LinkedList<SgtGravitySource> Instances { get { return instances; } } [System.NonSerialized] private static LinkedList<SgtGravitySource> instances = new LinkedList<SgtGravitySource>();

		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		[System.NonSerialized]
		private LinkedListNode<SgtGravitySource> node;

		protected virtual void OnEnable()
		{
			node = instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			instances.Remove(node);
		}

		protected virtual void Update()
		{
			if (autoSetMass == true)
			{
				if (cachedRigidbody == null)
				{
					cachedRigidbody = GetComponent<Rigidbody>();
				}

				if (cachedRigidbody != null)
				{
					mass = cachedRigidbody.mass;
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtGravitySource;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtGravitySource_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("mass", "The mass of this gravity source.");
			Draw("autoSetMass", "If you enable this then the Mass setting will be automatically copied from the attached Rigidbody.");
		}
	}
}
#endif