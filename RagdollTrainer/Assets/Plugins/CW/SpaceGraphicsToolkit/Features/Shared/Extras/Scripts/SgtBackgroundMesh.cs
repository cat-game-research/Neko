using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will procedurally generate a mesh that can be used to render stars or clouds in the background..</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBackgroundMesh")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Background Mesh")]
	public class SgtBackgroundMesh : MonoBehaviour
	{
		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; DirtyMesh(); } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The radius of the background mesh.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMesh(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>Should more quads be placed near the horizon?</summary>
		public float Squash { set { if (squash != value) { squash = value; DirtyMesh(); } } get { return squash; } } [SerializeField] [Range(0.0f, 1.0f)] private float squash;

		/// <summary>The amount of quads that will be generated in the background.</summary>
		public int QuadCount { set { if (quadCount != value) { quadCount = value; DirtyMesh(); } } get { return quadCount; } } [SerializeField] private int quadCount = 1000; public void SetQuadCount(float value) { QuadCount = (int)value; }

		/// <summary>The minimum radius of quads in the background.</summary>
		public float QuadRadiusMin { set { if (quadRadiusMin != value) { quadRadiusMin = value; DirtyMesh(); } } get { return quadRadiusMin; } } [SerializeField] private float quadRadiusMin = 0.01f;

		/// <summary>The maximum radius of quads in the background.</summary>
		public float QuadRadiusMax { set { if (quadRadiusMax != value) { quadRadiusMax = value; DirtyMesh(); } } get { return quadRadiusMax; } } [SerializeField] private float quadRadiusMax = 0.05f;

		/// <summary>How likely the size picking will pick smaller quads over larger ones (1 = default/linear).</summary>
		public float QuadRadiusBias { set { if (quadRadiusBias != value) { quadRadiusBias = value; DirtyMesh(); } } get { return quadRadiusBias; } } [SerializeField] private float quadRadiusBias;

		[System.NonSerialized]
		private Mesh mesh;

		[System.NonSerialized]
		private bool dirtyMesh = true;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		private static List<Vector3> positions = new List<Vector3>();
		private static List<Vector2> coords    = new List<Vector2>();
		private static List<int>     indices   = new List<int>();

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		protected virtual void OnEnable()
		{
			cachedMeshFilter = GetComponent<MeshFilter>();
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(mesh);
		}

		protected virtual void LateUpdate()
		{
			if (dirtyMesh == true)
			{
				UpdateMesh();
			}

			cachedMeshFilter.sharedMesh = mesh;
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			dirtyMesh = true;
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			dirtyMesh = true;
		}
#endif

		private void UpdateMesh()
		{
			dirtyMesh = false;

			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Background Mesh (Generated)");
			}
			else
			{
				mesh.Clear();
			}

			var count = Mathf.Max(0, quadCount);

			CwHelper.Resize(positions, count * 4);
			CwHelper.Resize(coords, count * 4);
			CwHelper.Resize(indices, count * 6);

			CwHelper.BeginSeed(seed);
				for (var i = 0; i < count; i++)
				{
					AddQuad(i);
				}
			CwHelper.EndSeed();

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.RecalculateBounds();

			mesh.Clear();
			mesh.SetVertices(positions);
			mesh.SetUVs(0, coords);
			mesh.SetTriangles(indices, 0, false);
		}

		private void AddQuad(int index)
		{
			var quadRadius   = Mathf.Lerp(quadRadiusMin, quadRadiusMax, CwHelper.Sharpness(Random.value, quadRadiusBias));
			var quadAngle    = Random.Range(-180.0f, 180.0f);
			var quadPosition = Random.insideUnitSphere;

			quadPosition.y *= 1.0f - squash;
			quadPosition = quadPosition.normalized * radius;

			var offV     = index * 4;
			var offI     = index * 6;
			var rotation = Quaternion.FromToRotation(Vector3.back, quadPosition) * Quaternion.Euler(0.0f, 0.0f, quadAngle);
			var up       = rotation * Vector3.up    * quadRadius;
			var right    = rotation * Vector3.right * quadRadius;

			positions[offV + 0] = quadPosition - up - right;
			positions[offV + 1] = quadPosition - up + right;
			positions[offV + 2] = quadPosition + up - right;
			positions[offV + 3] = quadPosition + up + right;

			coords[offV + 0] = new Vector2(0.0f, 0.0f);
			coords[offV + 1] = new Vector2(1.0f, 0.0f);
			coords[offV + 2] = new Vector2(0.0f, 1.0f);
			coords[offV + 3] = new Vector2(1.0f, 1.0f);

			indices[offI + 0] = offV + 0;
			indices[offI + 1] = offV + 1;
			indices[offI + 2] = offV + 2;
			indices[offI + 3] = offV + 3;
			indices[offI + 4] = offV + 2;
			indices[offI + 5] = offV + 1;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Backdrop
{
	using UnityEditor;
	using TARGET = SgtBackgroundMesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBackgroundMesh_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			Draw("seed", ref dirtyMesh, "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the background.");
			EndError();
			Draw("squash", ref dirtyMesh, "Should more quads be placed near the horizon?");

			Separator();

			BeginError(Any(tgts, t => t.QuadCount < 0));
				Draw("quadCount", ref dirtyMesh, "The amount of quads that will be generated in the background.");
			EndError();
			BeginError(Any(tgts, t => t.QuadRadiusMin < 0.0f || t.QuadRadiusMin > t.QuadRadiusMax));
				Draw("quadRadiusMin", ref dirtyMesh, "The minimum radius of quads in the background.");
			EndError();
			BeginError(Any(tgts, t => t.QuadRadiusMax < 0.0f || t.QuadRadiusMin > t.QuadRadiusMax));
				Draw("quadRadiusMax", ref dirtyMesh, "The maximum radius of quads in the background.");
			EndError();
			Draw("quadRadiusBias", ref dirtyMesh, "How likely the size picking will pick smaller quads over larger ones (1 = default/linear).");

			if (dirtyMesh     == true) Each(tgts, t => t.DirtyMesh    (), true, true);
		}
	}
}
#endif