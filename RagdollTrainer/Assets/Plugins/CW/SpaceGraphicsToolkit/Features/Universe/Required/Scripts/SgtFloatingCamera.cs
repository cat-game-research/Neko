using UnityEngine;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component marks the current GameObject as a camera. This means as soon as the transform.position strays too far from the origin (0,0,0), it will snap back to the origin.
	/// After it snaps back, the SnappedPoint field will be updated with the current position of the SgtFloatingOrigin component.</summary>
	[ExecuteInEditMode]
	[DefaultExecutionOrder(-50)]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingCamera")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Camera")]
	public class SgtFloatingCamera : SgtFloatingPoint
	{
		public static LinkedList<SgtFloatingCamera> Instances = new LinkedList<SgtFloatingCamera>(); private LinkedListNode<SgtFloatingCamera> node;

		/// <summary>When the transform.position.magnitude exceeds this value, the position will be snapped back to the origin.</summary>
		public float SnapDistance { set { snapDistance = value; } get { return snapDistance; } } [SerializeField] private float snapDistance = 100.0f;

		/// <summary>Every time this camera's position gets snapped, its position at that time is stored here. This allows other objects to correctly position themselves relative to this.
		/// NOTE: This requires you to use the SgtFloatingOrigin component.</summary>
		public SgtPosition SnappedPoint { set { snappedPoint = value; } get { return snappedPoint; } } [SerializeField] private SgtPosition snappedPoint;

		public bool SnappedPointSet { set { snappedPointSet = value; } get { return snappedPointSet; } } [SerializeField] private bool snappedPointSet;

		/// <summary>Called when this camera's position snaps back to the origin (Vector3 = delta).</summary>
		public static event System.Action<SgtFloatingCamera, Vector3> OnSnap;

		/// <summary>This method will fill the instance with the first active and enabled <b>SgtFloatingCamera</b> instance in the scene and return true, or return false.</summary>
		public static bool TryGetInstance(ref SgtFloatingCamera instance)
		{
			if (Instances.Count > 0) { instance = Instances.First.Value; return true; } return false;
		}

		/// <summary>This method converts the specified world space position into a universal SgtPosition.</summary>
		public SgtPosition GetPosition(Vector3 worldPosition)
		{
			var o = snappedPoint;

			o.LocalX += worldPosition.x;
			o.LocalY += worldPosition.y;
			o.LocalZ += worldPosition.z;

			o.SnapLocal();

			return o;
		}

		/// <summary>This method converts the specified universal SgtPosition into a world space position.</summary>
		public Vector3 CalculatePosition(SgtPosition input)
		{
			return CalculatePosition(ref input);
		}

		/// <summary>This method converts the specified universal SgtPosition into a world space position.</summary>
		public Vector3 CalculatePosition(ref SgtPosition input)
		{
			var x = (input.GlobalX - snappedPoint.GlobalX) * SgtPosition.CELL_SIZE + (input.LocalX - snappedPoint.LocalX);
			var y = (input.GlobalY - snappedPoint.GlobalY) * SgtPosition.CELL_SIZE + (input.LocalY - snappedPoint.LocalY);
			var z = (input.GlobalZ - snappedPoint.GlobalZ) * SgtPosition.CELL_SIZE + (input.LocalZ - snappedPoint.LocalZ);

			return new Vector3((float)x, (float)y, (float)z);
		}

		/// <summary>If the current <b>Transform.position</b> has strayed too far from the origin, this method will then call <b>Snap</b>.</summary>
		[ContextMenu("Try Snap")]
		public void TrySnap()
		{
			// Did we move far enough?
			if (transform.position.magnitude > snapDistance)
			{
				Snap();
			}
		}

		/// <summary>This method will reset the current <b>Transform</b> to 0,0,0 then update all <b>SgtFloatingObjects</b> in the scene.</summary>
		[ContextMenu("Snap")]
		public void Snap()
		{
			CheckForPositionChanges();

			snappedPoint    = position;
			snappedPointSet = true;

			snappedPoint.LocalX = System.Math.Floor(snappedPoint.LocalX);
			snappedPoint.LocalY = System.Math.Floor(snappedPoint.LocalY);
			snappedPoint.LocalZ = System.Math.Floor(snappedPoint.LocalZ);

			var oldPosition = transform.position;

			UpdatePositionNow();

			var newPosition = transform.position;
			var delta       = newPosition - oldPosition;

			if (OnSnap != null)
			{
				OnSnap(this, delta);
			}

			SgtCommon.InvokeSnap(delta);
		}

		protected virtual void OnEnable()
		{
			node = Instances.AddFirst(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;
		}

		protected virtual void LateUpdate()
		{
			UpdatePosition();

			TrySnap();
		}

		private void UpdatePosition()
		{
			CheckForPositionChanges();
			UpdatePositionNow();
		}

		private void UpdatePositionNow()
		{
			transform.position = expectedPosition = CalculatePosition(ref position);

			expectedPositionSet = true;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingCamera;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingCamera_Editor : SgtFloatingPoint_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			BeginError(Any(tgts, t => t.SnapDistance <= 0.0));
				Draw("snapDistance", "When the transform.position.magnitude exceeds this value, the position will be snapped back to the origin.");
			EndError();
			Draw("snappedPoint", "Every time this camera's position gets snapped, its position at that time is stored here. This allows other objects to correctly position themselves relative to this.");
		}
	}
}
#endif