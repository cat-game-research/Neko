using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component is the basis for all procedural components in SGT.</summary>
	public abstract class SgtProcedural : MonoBehaviour
	{
		public enum GenerateType
		{
			Automatically,
			WithRandomSeed,
			WithFixedSeed,
			Manually
		}

		/// <summary>This allows you to control when this component will be generated.</summary>
		public GenerateType Generate { set { generate = value; } get { return generate; } } [SerializeField] private GenerateType generate;

		/// <summary>The seed used for automatic generation.</summary>
		public int Seed { set { seed = value; } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>This method allows you to manually generate this component with the specified seed.</summary>
		public void GenerateWith(int seed)
		{
			CwHelper.BeginSeed(seed);
			{
				DoGenerate();
			}
			CwHelper.EndSeed();
		}

		[ContextMenu("Generate Now")]
		public void GenerateNow()
		{
			switch (generate)
			{
				case GenerateType.Automatically:
				{
					DoGenerate();
				}
				break;

				case GenerateType.WithRandomSeed:
				{
					var randomSeed = Random.Range(int.MinValue, int.MaxValue);

					GenerateWith(randomSeed);
				}
				break;

				case GenerateType.WithFixedSeed:
				{
					GenerateWith(seed);
				}
				break;

				case GenerateType.Manually:
				{
					DoGenerate();
				}
				break;
			}
		}

		protected abstract void DoGenerate();

		protected virtual void Awake()
		{
			switch (generate)
			{
				case GenerateType.Automatically:
				{
					DoGenerate();
				}
				break;

				case GenerateType.WithRandomSeed:
				{
					var randomSeed = Random.Range(int.MinValue, int.MaxValue);

					GenerateWith(randomSeed);
				}
				break;

				case GenerateType.WithFixedSeed:
				{
					GenerateWith(seed);
				}
				break;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtProcedural;

	public class SgtProcedural_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("generate", "This allows you to control when this component will be generated.");

			if (Any(tgts, t => t.Generate == SgtProcedural.GenerateType.WithFixedSeed))
			{
				Draw("seed", "The seed used for automatic generation.");
			}

			Separator();
		}
	}
}
#endif