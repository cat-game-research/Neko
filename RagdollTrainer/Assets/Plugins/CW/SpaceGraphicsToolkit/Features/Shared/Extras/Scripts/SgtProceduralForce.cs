using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component adds force the current GameObject in a random direction, with a random speed.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtProceduralForce")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Procedural Force")]
	public class SgtProceduralForce : SgtProcedural
	{
		/// <summary>If you want to specify a force direction, set it here.</summary>
		public Vector3 Direction { set { direction = value; } get { return direction; } } [SerializeField] private Vector3 direction;

		/// <summary>Minimum degrees per second.</summary>
		public float SpeedMin { set { speedMin = value; } get { return speedMin; } } [SerializeField] private float speedMin;

		/// <summary>Maximum degrees per second.</summary>
		public float SpeedMax { set { speedMax = value; } get { return speedMax; } } [SerializeField] private float speedMax = 10.0f;

		protected override void DoGenerate()
		{
			var axis  = Random.onUnitSphere;
			var speed = Random.Range(speedMin, speedMax);

			if (direction != Vector3.zero)
			{
				axis = direction.normalized;
			}

			GetComponent<Rigidbody>().velocity = axis * speed;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtProceduralForce;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtProceduralForce_Editor : SgtProcedural_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Draw("direction", "If you want to specify a force direction, set it here.");
			Draw("speedMin", "Minimum degrees per second.");
			Draw("speedMax", "Maximum degrees per second.");
		}
	}
}
#endif