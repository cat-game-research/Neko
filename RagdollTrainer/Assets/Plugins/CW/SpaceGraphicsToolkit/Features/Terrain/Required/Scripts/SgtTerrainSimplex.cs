using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to deform the attached <b>SgtTerrain</b> with procedural simplex noise.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainSimplex")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Simplex")]
	public class SgtTerrainSimplex : MonoBehaviour, SgtTerrain.IModifiesHeights, SgtTerrain.IModifiesCombinedHeights
	{
		/// <summary>This allows you to control where this biome appears based on the <b>SgtTerrain</b> component's <b>Areas</b> splatmap.</summary>
		public int Area { set { area = value; MarkAsDirty(); } get { return area; } } [SerializeField] private int area = -1;

		/// <summary>The amount of peaks and valleys across the mesh.</summary>
		public double Frequency { set { frequency = value; MarkAsDirty(); } get { return frequency; } } [SerializeField] private double frequency = 1000.0;

		/// <summary>The maximum +- displacement of the first octave.
		/// NOTE: The final displacement may be greater than this range when using multiple octaves.</summary>
		public double Amplitude { set { amplitude = value; MarkAsDirty(); } get { return amplitude; } } [SerializeField] private double amplitude = 100.0;

		/// <summary>Use ridged simplex noise?</summary>
		public bool Ridged { set { ridged = value; MarkAsDirty(); } get { return ridged; } } [SerializeField] private bool ridged;

		/// <summary>The random noise seed.</summary>
		public int Seed { set { seed = value; MarkAsDirty(); } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The amount of noise layers.</summary>
		public int Octaves { set { octaves = value; MarkAsDirty(); } get { return octaves; } } [SerializeField] [Range(1, 20)] private int octaves = 8;

		private SgtTerrain cachedTerrain;

		private NativeArray<float> tempWeights;

		public void MarkAsDirty()
		{
			if (cachedTerrain != null)
			{
				cachedTerrain.MarkAsDirty();
			}
		}

		protected virtual void OnEnable()
		{
			cachedTerrain = GetComponent<SgtTerrain>();

			tempWeights = new NativeArray<float>(0, Allocator.Persistent);

			cachedTerrain.MarkAsDirty();
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.ScheduleDispose(tempWeights);

			cachedTerrain.MarkAsDirty();
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			MarkAsDirty();
		}

		public void HandleScheduleCombinedHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			HandleScheduleHeights(points, heights, ref handle);
		}

		public void HandleScheduleHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			var job = new HeightsJob();

			job.Frequency = frequency;
			job.Amplitude = amplitude;
			job.Ridged    = ridged;
			job.Offset.x  = seed % 6131;
			job.Offset.y  = seed % 6569;
			job.Offset.z  = seed % 6991;
			job.Octaves   = octaves;
			job.Points    = points;
			job.Heights   = heights;

			if (cachedTerrain.Areas != null && cachedTerrain.Areas.SplatCount > 0 && area >= 0)
			{
				job.Area           = math.clamp(area, 0, cachedTerrain.Areas.SplatCount - 1);
				job.AreaSize       = cachedTerrain.Areas.Size;
				job.AreaSplatCount = cachedTerrain.Areas.SplatCount;
				job.AreaWeights    = cachedTerrain.Areas.Weights;
			}
			else
			{
				job.Area           = 0;
				job.AreaSize       = int2.zero;
				job.AreaSplatCount = 0;
				job.AreaWeights    = tempWeights;
			}

			handle = job.Schedule(heights.Length, 32, handle);
		}

		[BurstCompile]
		public struct HeightsJob : IJobParallelFor
		{
			public double  Frequency;
			public double  Amplitude;
			public bool    Ridged;
			public int     Octaves;
			public double3 Offset;

			public NativeArray<double3> Points;
			public NativeArray<double>  Heights;

			[ReadOnly] public int                Area;
			[ReadOnly] public int2               AreaSize;
			[ReadOnly] public int                AreaSplatCount;
			[ReadOnly] public NativeArray<float> AreaWeights;

			public void Execute(int i)
			{
				if (double.IsNegativeInfinity(Heights[i]) == false)
				{
					var weight = 1.0f;

					if (AreaWeights.Length > 0)
					{
						weight = SgtTerrainTopology.Sample_Cubic_Equirectangular(AreaWeights, AreaSplatCount, Area, AreaSize, Points[i]);
						weight = math.clamp(20000.0f - weight, 0.0f, 20000.0f) / 20000.0f;
					}

					if (weight > 0.0f)
					{
						Heights[i] -= FBM(Points[i] * Frequency, Amplitude) * weight;
					}
				}
			}

			private double FBM(double3 p, double str)
			{
				var height = 0.0;

				if (Ridged == true)
				{
					for (var i = 0; i < Octaves; i++)
					{
						height += math.abs(noise.snoise((float3)(p + Offset))) * str;

						str *= 0.5f;
						p   *= 2.0f;
					}
				}
				else
				{
					for (var i = 0; i < Octaves; i++)
					{
						str *= 0.5f;

						height += (1.0f + noise.snoise((float3)(p + Offset))) * str;

						p *= 2.0f;
					}
				}

				return height;
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainSimplex;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtWorldSimplex_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			markAsDirty |= SgtTerrain_Editor.DrawArea(serializedObject.FindProperty("area"), tgt.GetComponent<SgtTerrain>());

			Separator();

			BeginError(Any(tgts, t => t.Frequency == 0.0));
				Draw("frequency", ref markAsDirty, "The amount of peaks and valleys across the mesh.");
			EndError();
			BeginError(Any(tgts, t => t.Amplitude == 0.0));
				Draw("amplitude", ref markAsDirty, "The maximum +- displacement of the first octave.\n\nNOTE: The final displacement may be greater than this range when using multiple octaves.");
			EndError();
			Draw("ridged", ref markAsDirty, "Use ridged simplex noise?");
			Draw("seed", ref markAsDirty, "The random noise seed.");
			Draw("octaves", ref markAsDirty, "The amount of noise layers.");

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty(), true, true);
			}
		}
	}
}
#endif