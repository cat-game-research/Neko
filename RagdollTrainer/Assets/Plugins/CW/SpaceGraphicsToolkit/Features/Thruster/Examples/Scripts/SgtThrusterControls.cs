using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Thruster
{
	/// <summary>This component allows you to control the specified thrusters with the specified control axes.</summary>
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Thruster Controls")]
	public class SgtThrusterControls : MonoBehaviour
	{
		[System.Serializable]
		public class ThrusterGroup
		{
			[Tooltip("The fingers or keys used to control these thrusters.")]
			public CwInputManager.Axis Controls;

			public bool Inverse;

			public bool Bidirectional;

			public List<SgtThruster> Positive;

			public List<SgtThruster> Negative;
		}

		/// <summary>Is this component currently listening for inputs?</summary>
		public bool Listen { set { listen = value; } get { return listen; } } [SerializeField] private bool listen = true;

		/// <summary>This allows you to specify each thruster group, each of which is controlled separately.</summary>
		public List<ThrusterGroup> Groups { get { if (groups == null) groups = new List<ThrusterGroup>(); return groups; } } [SerializeField] private List<ThrusterGroup> groups;

		protected virtual void OnEnable()
		{
			CwInputManager.EnsureThisComponentExists();
		}
		
		protected virtual void Update()
		{
			if (groups != null)
			{
				for (var i = groups.Count - 1; i >= 0; i--)
				{
					var control = groups[i];

					if (control != null)
					{
						var throttle = 0.0f;

						if (listen == true)
						{
							throttle = control.Controls.GetValue(1.0f);
						}

						if (control.Inverse == true)
						{
							throttle = -throttle;
						}

						if (control.Bidirectional == false)
						{
							if (throttle < 0.0f)
							{
								throttle = 0.0f;
							}
						}

						for (var j = control.Positive.Count - 1; j >= 0; j--)
						{
							var thruster = control.Positive[j];

							if (thruster != null)
							{
								thruster.Throttle = throttle;
							}
						}

						for (var j = control.Negative.Count - 1; j >= 0; j--)
						{
							var thruster = control.Negative[j];

							if (thruster != null)
							{
								thruster.Throttle = throttle;
							}
						}
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Thruster
{
	using UnityEditor;
	using TARGET = SgtThrusterControls;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtThrusterControls_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("groups", "This allows you to specify each thruster group, each of which is controlled separately.");
		}
	}
}
#endif