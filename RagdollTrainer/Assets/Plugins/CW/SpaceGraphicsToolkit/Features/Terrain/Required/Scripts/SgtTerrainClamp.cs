using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to clamp the heights of the attached <b>SgtTerrain</b> to be within a specific range.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainClamp")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Clamp")]
	public class SgtTerrainClamp : MonoBehaviour, SgtTerrain.IModifiesHeights, SgtTerrain.IModifiesCombinedHeights
	{
		/// <summary>This allows you to control where this biome appears based on the <b>SgtTerrain</b> component's <b>Areas</b> splatmap.</summary>
		public int Area { set { area = value; MarkAsDirty(); } get { return area; } } [SerializeField] private int area = -1;

		/// <summary>Terrain heights below this value will be clamped.</summary>
		public double Minimum { set { minimum = value; MarkAsDirty(); } get { return minimum; } } [SerializeField] private double minimum = 1000.0;

		/// <summary>Terrain heights above this value will be clamped.</summary>
		public double Maximum { set { maximum = value; MarkAsDirty(); } get { return maximum; } } [SerializeField] private double maximum = 2000.0;

		/// <summary>If you don't want the clamping to be abrupt, you can increase the thickness where the clamp transitions.</summary>
		public double Thickness { set { thickness = value; MarkAsDirty(); } get { return thickness; } } [SerializeField] private double thickness = 100.0;

		/// <summary>This controls the shape of the <b>Thickness</b> transition.</summary>
		public double Power { set { power = value; MarkAsDirty(); } get { return power; } } [SerializeField] private double power = 1.0;

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

			job.Minimum   = minimum;
			job.Maximum   = maximum;
			job.Thickness = thickness;
			job.Power     = power;
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
			public double Minimum;
			public double Maximum;
			public double Thickness;
			public double Power;

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
						var oldHeight = Heights[i];
						var newHeight = oldHeight;
						var strength  = 0.0;

						if (Thickness > 0.0)
						{
							var bot = Minimum + Thickness;
							var top = Maximum - Thickness;

							if (oldHeight < bot)
							{
								strength  = math.saturate((bot - oldHeight) / Thickness);
								newHeight = Minimum;
							}
							else if (oldHeight > top)
							{
								strength  = math.saturate((oldHeight - top) / Thickness);
								newHeight = Maximum;
							}

							strength = 1.0 - math.pow(1.0 - strength, Power);
						}
						else
						{
							strength  = 1.0;
							newHeight = math.clamp(oldHeight, Minimum, Maximum);
						}

						Heights[i] = math.lerp(oldHeight, newHeight, strength * weight);
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainClamp;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainClamp_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			markAsDirty |= SgtTerrain_Editor.DrawArea(serializedObject.FindProperty("area"), tgt.GetComponent<SgtTerrain>());

			Separator();

			BeginError(Any(tgts, t => t.Minimum > t.Maximum));
				Draw("minimum", ref markAsDirty, "Terrain heights below this value will be clamped.");
				Draw("maximum", ref markAsDirty, "Terrain heights above this value will be clamped.");
			EndError();
			Draw("thickness", ref markAsDirty, "If you don't want the clamping to be abrupt, you can increase the thickness where the clamp transitions.");
			Draw("power", ref markAsDirty, "This controls the shape of the <b>Thickness</b> transition.");

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty(), true, true);
			}
		}
	}
}
#endif