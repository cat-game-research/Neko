using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component should modify <b>SgtFloatingObject</b> to work with Rigidbodies that have interpolation to eliminate stuttering from origin shifts. But for some reason it doesn't do anything?</summary>
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingRigidbody")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Rigidbody")]
	public class SgtFloatingRigidbody : SgtFloatingObject
	{
		[System.NonSerialized]
		private Rigidbody cachedRigidbody;

		protected override void ApplyPosition(SgtFloatingCamera floatingCamera)
		{
			base.ApplyPosition(floatingCamera);

			if (cachedRigidbody == null)
			{
				cachedRigidbody = GetComponent<Rigidbody>();
			}

			cachedRigidbody.position = expectedPosition;
		}
	}
}