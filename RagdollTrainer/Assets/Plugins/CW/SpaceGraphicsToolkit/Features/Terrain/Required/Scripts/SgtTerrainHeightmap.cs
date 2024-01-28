using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to deform the attached <b>SgtTerrain</b> with a heightmap using equirectangular cylindrical projection.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainHeightmap")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Heightmap")]
	public class SgtTerrainHeightmap : MonoBehaviour, SgtTerrain.IModifiesHeights, SgtTerrain.IModifiesCombinedHeights
	{
		public enum ChannelType
		{
			Red,
			Green,
			Blue,
			Alpha
		}

		/// <summary>The heightmap texture used to displace the mesh.
		/// NOTE: This should use the equirectangular cylindrical projection.</summary>
		public Texture2D Heightmap { set { if (heightmap != value) { heightmap = value; PrepareTexture(); } } get { return heightmap; } } [SerializeField] private Texture2D heightmap;

		/// <summary>This allows you to choose which color channel from the heightmap texture will be used.
		/// NOTE: If your texture uses a 1 byte per channel format like Alpha8/R8, then this setting will be ignored.</summary>
		public ChannelType Channel { set { if (channel != value) { channel = value; MarkAsDirty(); } } get { return channel; } } [SerializeField] private ChannelType channel;

		/// <summary>This allows you to control the maximum height displacement applied to the terrain.</summary>
		public double Displacement { set { if (displacement != value) { displacement = value; MarkAsDirty(); } } get { return displacement; } } [SerializeField] private double displacement = 0.25;

		private SgtTerrain cachedTerrain;

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

			PrepareTexture();
		}

		protected virtual void OnDisable()
		{
			cachedTerrain.MarkAsDirty();
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			MarkAsDirty();
		}
#endif

		protected virtual void OnDidApplyAnimationProperties()
		{
			MarkAsDirty();
		}

		private void PrepareTexture()
		{
			MarkAsDirty();
		}

		public void HandleScheduleCombinedHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			HandleScheduleHeights(points, heights, ref handle);
		}

		public void HandleScheduleHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			if (heightmap != null && heightmap.isReadable == true)
			{
				var job = new HeightsJob();

				job.Size         = new int2(heightmap.width, heightmap.height);
				job.Displacement = displacement;
				job.Stride       = SgtCommon.GetStride(heightmap.format);
				job.Offset       = SgtCommon.GetOffset(heightmap.format, (int)channel);
				job.Data         = heightmap.GetRawTextureData<byte>();
				job.Points       = points;
				job.Heights      = heights;

				handle = job.Schedule(heights.Length, 32, handle);
			}
		}

		[BurstCompile]
		public struct HeightsJob : IJobParallelFor
		{
			public double Displacement;
			public int2   Size;
			public int    Stride;
			public int    Offset;

			[ReadOnly] public NativeArray<byte> Data;

			[ReadOnly] public NativeArray<double3> Points;

			public NativeArray<double> Heights;

			public void Execute(int i)
			{
				if (double.IsNegativeInfinity(Heights[i]) == false)
				{
					Heights[i] += SgtTerrainTopology.Sample_Cubic_Equirectangular(Data, Stride, Offset, Size, Points[i]) * Displacement;
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtTerrainHeightmap;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainHeightmap_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var markAsDirty = false;

			BeginError(Any(tgts, t => t.Heightmap == null));
				Draw("heightmap", ref markAsDirty, "The heightmap texture used to displace the mesh.\n\nNOTE: The height data should be stored in the alpha channel.\n\nNOTE: This should use the equirectangular cylindrical projection.");
			EndError();
			if (Any(tgts, t => t.Heightmap != null && t.Heightmap.isReadable == false))
			{
				Warning("This texture is non-readable.");
			}
			Draw("channel", ref markAsDirty, "This allows you to choose which color channel from the heightmap texture will be used.");
			BeginError(Any(tgts, t => t.Displacement == 0.0));
				Draw("displacement", ref markAsDirty, "This allows you to control the maximum height displacement applied to the terrain.");
			EndError();

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty());
			}
		}
	}
}
#endif