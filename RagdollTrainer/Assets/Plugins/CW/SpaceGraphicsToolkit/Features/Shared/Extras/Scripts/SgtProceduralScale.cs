using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component rotates the current GameObject along a random axis, with a random speed.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtProceduralScale")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Procedural Scale")]
	public class SgtProceduralScale : SgtProcedural
	{
		/// <summary>The default scale of your object.</summary>
		public Vector3 BaseScale { set { baseScale = value; } get { return baseScale; } } [SerializeField] private Vector3 baseScale = Vector3.one;

		/// <summary>The minimum multiplication of the BaseScale.</summary>
		public float ScaleMultiplierMin { set { scaleMultiplierMin = value; } get { return scaleMultiplierMin; } } [SerializeField] private float scaleMultiplierMin = 1.0f;

		/// <summary>The maximum multiplication of the BaseScale.</summary>
		public float ScaleMultiplierMax { set { scaleMultiplierMax = value; } get { return scaleMultiplierMax; } } [SerializeField] private float scaleMultiplierMax = 2.0f;

		protected override void DoGenerate()
		{
			transform.localScale = baseScale * Mathf.Lerp(scaleMultiplierMin, scaleMultiplierMax, Random.value);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtProceduralScale;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtProceduralScale_Editor : SgtProcedural_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			BeginError(Any(tgts, t => t.BaseScale == Vector3.zero));
				Draw("baseScale", "The default scale of your object.");
			EndError();
			Draw("scaleMultiplierMin", "The minimum multiplication of the BaseScale.");
			Draw("scaleMultiplierMax", "The maximum multiplication of the BaseScale.");
		}
	}
}
#endif