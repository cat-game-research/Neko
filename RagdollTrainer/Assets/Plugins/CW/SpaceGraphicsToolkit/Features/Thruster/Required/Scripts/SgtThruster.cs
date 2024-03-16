using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Thruster
{
	/// <summary>This component allows you to create simple thrusters that can apply forces to Rigidbodies based on their position. You can also use sprites to change the graphics.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtThruster")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Thruster")]
	public class SgtThruster : MonoBehaviour
	{
		/// <summary>How active is this thruster? 0 for off, 1 for max power, -1 for max reverse, etc.</summary>
		public float Throttle { set { throttle = value; } get { return throttle; } } [SerializeField] private float throttle;

		/// <summary>The rigidbody you want to apply the thruster forces to</summary>
		public Rigidbody Body { set { body = value; } get { return body; } } [SerializeField] private Rigidbody body;

		/// <summary>The type of force we want to apply to the Rigidbody.</summary>
		public bool ForceAtPosition { set { forceAtPosition = value; } get { return forceAtPosition; } } [SerializeField] private bool forceAtPosition;

		/// <summary>The force mode used when adding force to the Rigidbody.</summary>
		public ForceMode ForceMode { set { forceMode = value; } get { return forceMode; } } [SerializeField] private ForceMode forceMode = ForceMode.Acceleration;

		/// <summary>The maximum amount of force applied to the rigidbody (when the throttle is -1 or 1).</summary>
		public float ForceMagnitude { set { forceMagnitude = value; } get { return forceMagnitude; } } [SerializeField] private float forceMagnitude = 1.0f;

		/// <summary>Create a child GameObject with a thruster attached</summary>
		public static SgtThruster Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtThruster Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Thruster", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtThruster>();
		}

		protected virtual void FixedUpdate()
		{
#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				return;
			}
#endif
			// Apply thruster force to rigidbody
			if (body != null)
			{
				var force = transform.forward * forceMagnitude * throttle * Time.fixedDeltaTime;

				if (forceAtPosition == true)
				{
					body.AddForceAtPosition(force, transform.position, forceMode);
				}
				else
				{
					body.AddForce(force, forceMode);
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			var a = transform.position;
			var b = transform.position + transform.forward * forceMagnitude;

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
			Gizmos.DrawLine(a, b);

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
			Gizmos.DrawLine(a, a + (b - a) * throttle);
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Thruster
{
	using UnityEditor;
	using TARGET = SgtThruster;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtThruster_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("throttle", "How active is this thruster? 0 for off, 1 for max power, -1 for max reverse, etc.");
			Draw("body", "The rigidbody you want to apply the thruster forces to");

			if (Any(tgts, t => t.Body != null))
			{
				BeginIndent();
					Draw("forceAtPosition", "The type of force we want to apply to the Rigidbody.");
					Draw("forceMode", "The force mode used when adding force to the Rigidbody.");
					Draw("forceMagnitude", "The maximum amount of force applied to the rigidbody (when the throttle is -1 or 1).");
				EndIndent();
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Thruster", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtThruster.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif