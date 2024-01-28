using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component wraps SgtPosition into a component, and defines a single point in the Universe.
	/// Normal transform position coordinates are stored using floats (Vector3), but SgtPosition coordinates are stored using a long and a double pair.
	/// The long is used to specify the current grid cell, and the double is used to specify the high precision relative offset to the grid cell.
	/// Combined, these values allow simulation of the whole observable universe.</summary>
	[DisallowMultipleComponent]
	public abstract class SgtFloatingPoint : MonoBehaviour
	{
		/// <summary>The position wrapped by this component.</summary>
		public SgtPosition Position { set { SetPosition(value); } get { return position; } } [SerializeField] protected SgtPosition position;

		/// <summary>Whenever the <b>Position</b> values are modified, this gets called. This is useful for components that depend on this position being known at all times (e.g. SgtFloatingOrbit).</summary>
		public event System.Action OnPositionChanged;

		[SerializeField]
		protected Vector3 expectedPosition;

		[SerializeField]
		protected bool expectedPositionSet;

		[System.NonSerialized]
		private bool notifying;

		/// <summary>This method will invoke the <b>OnPositionChanged</b> event.</summary>
		public void NotifyPositionChanged()
		{
			if (notifying == false)
			{
				notifying = true;

				if (OnPositionChanged != null)
				{
					OnPositionChanged();
				}

				notifying = false;
			}
		}

		/// <summary>This method will apply the current <b>Position</b> to the <b>Transform.Position</b>, in case they go out of sync.</summary>
		public void ApplyPosition()
		{
			if (SgtFloatingCamera.Instances.Count > 0)
			{
				ApplyPosition(SgtFloatingCamera.Instances.First.Value);
			}
		}

		protected virtual void ApplyPosition(SgtFloatingCamera floatingCamera)
		{
			expectedPosition    = floatingCamera.CalculatePosition(position);
			expectedPositionSet = true;

			transform.position = expectedPosition;
		}

		/// <summary>This method allows you to change the whole <b>Position</b> state, and it will automatically call the <b>PositionChanged</b> method if the position is different.</summary>
		public void SetPosition(SgtPosition newPosition)
		{
			if (SgtPosition.Equal(ref newPosition, ref position) == false)
			{
				position = newPosition;

				ApplyPosition();

				NotifyPositionChanged();
			}
		}

		protected virtual void CheckForPositionChanges()
		{
			var currentPosition = transform.position;

			if (expectedPositionSet == true)
			{
				if (expectedPosition.x != currentPosition.x || expectedPosition.y != currentPosition.y || expectedPosition.z != currentPosition.z)
				{
					position.LocalX += currentPosition.x - expectedPosition.x;
					position.LocalY += currentPosition.y - expectedPosition.y;
					position.LocalZ += currentPosition.z - expectedPosition.z;

					position.SnapLocal();

					expectedPosition = currentPosition;

					NotifyPositionChanged();
				}
			}
			else
			{
				expectedPositionSet = true;
				expectedPosition    = currentPosition;
			}
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			if (expectedPositionSet == true)
			{
				ApplyPosition();
			}
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingPoint;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(SgtFloatingPoint))]
	public class SgtFloatingPoint_Editor : CwEditor
	{
		delegate void DoubleDel(ref SgtPosition position, double value);

		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (Any(tgts, t => t.GetComponentsInParent<SgtFloatingPoint>().Length > 1))
			{
				Error("This component is parented to a SgtFloatingObject/Camera, which will not work. Detach it, and use the SgtFollow component instead.");
			}

			var modified = false;

			modified |= Draw("position.LocalX", "The position in meters along the X axis, relative to the current global cell position.");
			modified |= Draw("position.LocalY", "The position in meters along the Y axis, relative to the current global cell position.");
			modified |= Draw("position.LocalZ", "The position in meters along the Z axis, relative to the current global cell position.");
			modified |= Draw("position.GlobalX", "The current grid cell along the X axis. Each grid cell is equal to 50000000 meters.");
			modified |= Draw("position.GlobalY", "The current grid cell along the Y axis. Each grid cell is equal to 50000000 meters.");
			modified |= Draw("position.GlobalZ", "The current grid cell along the Z axis. Each grid cell is equal to 50000000 meters.");

			if (modified == true)
			{
				Each(tgts, t => { t.ApplyPosition(); t.NotifyPositionChanged(); }, true, true);
			}
		}
	}
}
#endif