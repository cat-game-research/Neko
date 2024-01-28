using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Debris
{
	/// <summary>This component handles a single debris object.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtDebris")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Debris")]
	public class SgtDebris : MonoBehaviour
	{
		public enum StateType
		{
			Hide,
			Fade,
			Show,
		}

		/// <summary>Called when this debris is spawned (if pooling is enabled).</summary>
		public event System.Action OnSpawn;

		/// <summary>Called when this debris is despawned (if pooling is enabled).</summary>
		public event System.Action OnDespawn;

		/// <summary>Can this debris be pooled?</summary>
		public bool Pool { set { pool = value; } get { return pool; } } [SerializeField] private bool pool;

		/// <summary>The current state of the scaling.</summary>
		public StateType State { set { state = value; } get { return state; } } [SerializeField] private StateType state;

		/// <summary>The prefab this was instantiated from.</summary>
		public SgtDebris Prefab { set { prefab = value; } get { return prefab; } } [SerializeField] private SgtDebris prefab;

		/// <summary>This gets automatically copied when spawning debris.</summary>
		public Vector3 Scale { set { scale = value; } get { return scale; } } [SerializeField] private Vector3 scale;

		/// <summary>The cell this debris was spawned in.</summary>
		public SgtLong3 Cell { set { cell = value; } get { return cell; } } [SerializeField] private SgtLong3 cell;

		// The initial scale-in
		public float Show { set { show = value; } get { return show; } } [SerializeField] private float show;

		public void InvokeOnSpawn()
		{
			if (OnSpawn != null)
			{
				OnSpawn.Invoke();
			}
		}

		public void InvokeOnDespawn()
		{
			if (OnDespawn != null)
			{
				OnDespawn.Invoke();
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Debris
{
	using UnityEditor;
	using TARGET = SgtDebris;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtDebris_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("pool", "Can this debris be pooled?");

			Separator();

			BeginDisabled();
				Draw("state", "The current state of the scaling.");
				Draw("prefab", "The prefab this was instantiated from.");
				Draw("scale", "This gets automatically copied when spawning debris.");
				Draw("cell", "The cell this debris was spawned in.");
			EndDisabled();
		}
	}
}
#endif