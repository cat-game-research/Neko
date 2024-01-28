using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject along a random axis, with a random speed.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtProceduralSpin")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Procedural Spin")]
	public class SgtProceduralSpin : SgtProcedural
	{
		/// <summary>Minimum degrees per second.</summary>
		public float SpeedMin { set { speedMin = value; } get { return speedMin; } } [SerializeField] private float speedMin;

		/// <summary>Maximum degrees per second.</summary>
		public float SpeedMax { set { speedMax = value; } get { return speedMax; } } [SerializeField] private float speedMax = 10.0f;

		[SerializeField]
		private Vector3 axis = Vector3.up;

		[SerializeField]
		private float speed;

		protected override void DoGenerate()
		{
			axis  = Random.onUnitSphere;
			speed = Random.Range(speedMin, speedMax);

			transform.localRotation = Random.rotation;
		}

		protected virtual void Update()
		{
			transform.Rotate(axis, speed * Time.deltaTime);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtProceduralSpin;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtProceduralSpin_Editor : SgtProcedural_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Draw("speedMin", "Minimum degrees per second.");
			Draw("speedMax", "Maximum degrees per second.");
		}
	}
}
#endif