using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component works like <b>SgtFloatingObject</b>, but it will scale the object toward the camera if it's too far away to render.
	/// NOTE: This component requires you to carefully set the <b>StartDistance</b> and <b>StepDistance</b> values, which depend on your camera and scene requirements.
	/// 
	/// NOTE: This component overrides the <b>Transform</b> component settings, and will not react to any manual changes made to it.</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingMassive")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Massive")]
	public class SgtFloatingMassive : SgtFloatingObject
	{
		/// <summary>This setting allows you to adjust the per-axis size of this object.</summary>
		public Vector3 Scale { set { scale = value; } get { return scale; } } [SerializeField] protected Vector3 scale = Vector3.one;

		/// <summary>The base scale of this object.</summary>
		public SgtLength Size { set { size = value; } get { return size; } } [SerializeField] protected SgtLength size = new SgtLength(1.0, SgtLength.ScaleType.Meter);

		/// <summary>If the distance between the camera and this object exceeds this value, this component will begin scaling down.
		/// NOTE: This value must be lower than your scene camera's <b>Far</b> clipping distance.</summary>
		public double StartDistance { set { startDistance = value; } get { return startDistance; } } [SerializeField] protected double startDistance = 100000.0;

		/// <summary>The distance between the camera and this object will be incremented by this value every time the excess distance crosses a multiple of 10 of this value.
		/// For example, if this value is 300, and the excess distance is 3000, then the excess distance will be reduced to 600. If the excess distance is 30000, then the excess distance will be reduced to 900, etc. In other words, a logarithmic distance increase will be converted into a linear distance increase.
		/// NOTE: The <b>StartDistance</b> value must be lower than your camera's <b>Far</b> clipping distance. Additionally, the total of the <b>StartDistance</b> + <b>StepDistance</b> * StepCount must also be below the clipping distance. The maximum StepCount that is reached depends on your scene and how big your objects are - a typical distant galaxy will reach an exponent of 25 or so.</summary>
		public double StepDistance { set { stepDistance = value; } get { return stepDistance; } } [SerializeField] protected double stepDistance = 10000.0;

		protected override void ApplyPosition(SgtFloatingCamera floatingCamera)
		{
			// Ignore standard snaps
		}

		protected override void CheckForPositionChanges()
		{
			// Ignore position changes
		}

		protected virtual void LateUpdate()
		{
			if (SgtFloatingCamera.Instances.Count > 0)
			{
				var floatingCamera = SgtFloatingCamera.Instances.First.Value;
				var camPos         = floatingCamera.Position;
				var distance       = SgtPosition.Distance(ref camPos, ref position);
				var sca            = CalculateScale(distance);

				transform.position   = floatingCamera.transform.position + SgtPosition.Vector(ref camPos, ref position, sca);
				transform.localScale = scale * (float)(size * sca);
			}
		}

		private double CalculateScale(double distance)
		{
			if (distance > 0.0f && distance > startDistance && stepDistance > 0.0f)
			{
				var extraDistance   = System.Math.Log10((distance - startDistance) / stepDistance + 1.0f) * stepDistance;
				var desiredDistance = startDistance + extraDistance;

				if (distance > desiredDistance)
				{
					return desiredDistance / distance;
				}
			}

			return 1.0;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingMassive;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET), true)]
	public class SgtFloatingMassive_Editor : SgtFloatingObject_Editor
	{
		protected override void OnInspector()
		{
			base.OnInspector();

			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("scale", "This setting allows you to adjust the per-axis size of this object.");
			Draw("size", "The base scale of this object.");

			Separator();

			Draw("startDistance", "This component's scaling feature will enable once this object goes beyond this world space distance.\n\nNOTE: This value must be lower than your scene camera's <b>Far</b> clipping distance.");
			Draw("stepDistance", "Once this component's world space distance goes beyond <b>StartDistance</b>, every additional base 10 exponent of the distance will increase the actual distance by this value.\n\nNOTE: The <b>StartDistance</b> combined with many step distances must be lower than your scene camera's <b>Far</b> clipping distance. The maximum total distance that will be reached depends on your scene - a typical distant galaxy will reach an exponent of 25 or so.");
		}
	}
}
#endif