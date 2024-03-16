using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component works like <b>SgtTerrainHeightmap</b>, but the heightmap will apply to one small area of the planet. This is useful if you want to add a specific feature to your planet at a specific location (e.g. a mountain).</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtTerrain))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtTerrainFeature")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Terrain Feature")]
	public class SgtTerrainFeature : MonoBehaviour, SgtTerrain.IModifiesHeights, SgtTerrain.IModifiesCombinedHeights
	{
		public enum ChannelType
		{
			Red,
			Green,
			Blue,
			Alpha
		}

		/// <summary>The heightmap texture used to displace the mesh.</summary>
		public Texture2D Heightmap { set { if (heightmap != value) { heightmap = value; MarkAsDirty(); } } get { return heightmap; } } [SerializeField] private Texture2D heightmap;

		/// <summary>This allows you to choose which color channel from the heightmap texture will be used.
		/// NOTE: If your texture uses a 1 byte per channel format like Alpha8/R8, then this setting will be ignored.</summary>
		public ChannelType Channel { set { if (channel != value) { channel = value; MarkAsDirty(); } } get { return channel; } } [SerializeField] private ChannelType channel;

		/// <summary>This allows you to control the maximum height displacement applied to the terrain.</summary>
		public double Displacement { set { if (displacement != value) { displacement = value; MarkAsDirty(); } } get { return displacement; } } [SerializeField] private double displacement = 0.25;

		/// <summary>This allows you to specify the local angle on the planet where the feature should appear in degrees.
		/// 0,0,0 = The feature will appear on the local -Z axis side.</summary>
		public Vector3 Rotation { set { if (rotation != value) { rotation = value; MarkAsDirty(); } } get { return rotation; } } [SerializeField] private Vector3 rotation;

		/// <summary>This allows you to specify the scale of the feature in local space.</summary>
		public float Scale { set { if (scale != value) { scale = value; MarkAsDirty(); } } get { return scale; } } [SerializeField] private float scale = 1.0f;

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

			MarkAsDirty();
		}

		protected virtual void OnDisable()
		{
			MarkAsDirty();
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

		public void HandleScheduleCombinedHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			HandleScheduleHeights(points, heights, ref handle);
		}

		public void HandleScheduleHeights(NativeArray<double3> points, NativeArray<double> heights, ref JobHandle handle)
		{
			if (heightmap != null && heightmap.isReadable == true)
			{
				var job    = new HeightsJob();
				var scaleI = CwHelper.Reciprocal(scale);
				var aspect = CwHelper.Divide(heightmap.height, heightmap.width);

				job.Size         = new int2(heightmap.width, heightmap.height);
				job.Displacement = displacement;
				job.Stride       = SgtCommon.GetStride(heightmap.format);
				job.Offset       = SgtCommon.GetOffset(heightmap.format, (int)channel);
				job.Data         = heightmap.GetRawTextureData<byte>();
				job.Points       = points;
				job.Heights      = heights;
				job.Matrix       = math.mul(float3x3.Scale(scaleI * aspect, scaleI, 1.0f), float3x3.EulerXYZ(math.radians(rotation)));

				handle = job.Schedule(heights.Length, 32, handle);
			}
		}

		[BurstCompile]
		public struct HeightsJob : IJobParallelFor
		{
			public double    Displacement;
			public int2      Size;
			public int       Stride;
			public int       Offset;
			public double3x3 Matrix;

			[ReadOnly] public NativeArray<byte> Data;

			[ReadOnly] public NativeArray<double3> Points;

			public NativeArray<double> Heights;

			public void Execute(int i)
			{
				if (double.IsNegativeInfinity(Heights[i]) == false)
				{
					var point = math.mul(Matrix, Points[i]);

					// Only modify front.
					if (point.z < 0.0f)
					{
						var mid = math.abs(point.xy);

						if (math.max(mid.x, mid.y) < 0.5)
						{
							Heights[i] += SgtTerrainTopology.Sample_Cubic(Data, Stride, Offset, Size, point.xy + 0.5) * Displacement;
						}
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
	using TARGET = SgtTerrainFeature;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtTerrainFeature_Editor : CwEditor
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
			Draw("rotation", ref markAsDirty, "This allows you to specify the local angle on the planet where the feature should appear in degrees.\n\n0,0,0 = The feature will appear on the local -Z axis side.");
			Draw("scale", ref markAsDirty, "This allows you to specify the scale of the feature in local space.");

			if (markAsDirty == true)
			{
				Each(tgts, t => t.MarkAsDirty());
			}
		}
	}
}
#endif