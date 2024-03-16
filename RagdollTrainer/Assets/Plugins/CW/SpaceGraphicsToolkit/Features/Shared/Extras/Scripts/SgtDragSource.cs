using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using SpaceGraphicsToolkit.Shapes;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to define a drag source, which can be used to apply drag to <b>Rigidbody</b> components that have the <b>SgtDragReceiver</b> attached.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtDragSource")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Drag Source")]
	public class SgtDragSource : MonoBehaviour
	{
		/// <summary>The drag applied to the drag receivers.</summary>
		public float Drag { set { drag = value; } get { return drag; } } [SerializeField] private float drag = 5.0f;

		/// <summary>This allows you to specify the shape of this drag volume.</summary>
		public SgtShape Shape { set { shape = value; } get { return shape; } } [SerializeField] private SgtShape shape;

		public static LinkedList<SgtDragSource> Instances { get { return instances; } } [System.NonSerialized] private static LinkedList<SgtDragSource> instances = new LinkedList<SgtDragSource>();

		[System.NonSerialized]
		private LinkedListNode<SgtDragSource> node;

		public static float GetDrag(Vector3 worldPosition)
		{
			var drag = 0.0f;

			foreach (var instance in instances)
			{
				if (instance.shape != null)
				{
					drag += instance.shape.GetDensity(worldPosition) * instance.drag;
				}
			}

			return drag;
		}

		protected virtual void OnEnable()
		{
			node = instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			instances.Remove(node);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtDragSource;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtDragSource_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Drag <= 0.0f));
				Draw("drag", "The drag applied to the drag receivers.");
			EndError();
			BeginError(Any(tgts, t => t.Shape == null));
				Draw("shape", "This allows you to specify the shape of this drag volume.");
			EndError();
		}
	}
}
#endif