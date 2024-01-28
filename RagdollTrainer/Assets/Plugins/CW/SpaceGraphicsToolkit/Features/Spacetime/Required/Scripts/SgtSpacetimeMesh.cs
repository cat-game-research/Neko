using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Spacetime
{
	/// <summary>This component can generate a plane grid mesh suitable for use with the <b>SgtSpacetime</b> component.
	/// NOTE: For maximum performance it's recommended that you manually use the <b>ExportMesh</b> context menu option of this component to turn this mesh into an asset. You can then remove this component and use the exported mesh.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtSpacetimeMesh")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Spacetime Mesh")]
	public class SgtSpacetimeMesh : MonoBehaviour
	{
		/// <summary>The size of the grid along the X axis.</summary>
		public float SizeX { set { if (sizeX != value) { sizeX = value; DirtyMesh(); } } get { return sizeX; } } [SerializeField] private float sizeX = 1.0f;

		/// <summary>The size of the grid along the Z axis.</summary>
		public float SizeZ { set { if (sizeZ != value) { sizeZ = value; DirtyMesh(); } } get { return sizeZ; } } [SerializeField] private float sizeZ = 1.0f;

		/// <summary>The amount of quads along the X axis.</summary>
		public int QuadsX { set { if (quadsX != value) { quadsX = value; DirtyMesh(); } } get { return quadsX; } } [SerializeField] private int quadsX = 16;

		/// <summary>The amount of quads along the Z axis.</summary>
		public int QuadsZ { set { if (quadsZ != value) { quadsZ = value; DirtyMesh(); } } get { return quadsZ; } } [SerializeField] private int quadsZ = 16;

		/// <summary>Should the mesh be centered, or begin at local 0,0?</summary>
		public bool Center { set { if (center != value) { center = value; DirtyMesh(); } } get { return center; } } [SerializeField] private bool center = true;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private bool cachedMeshRendererSet;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private bool cachedMeshFilterSet;

		public MeshRenderer CachedMeshRenderer
		{
			get
			{
				if (cachedMeshRendererSet == false)
				{
					cachedMeshRenderer    = GetComponent<MeshRenderer>();
					cachedMeshRendererSet = true;
				}

				return cachedMeshRenderer;
			}
		}

		public MeshFilter CachedMeshFilter
		{
			get
			{
				if (cachedMeshFilterSet == false)
				{
					cachedMeshFilter    = GetComponent<MeshFilter>();
					cachedMeshFilterSet = true;
				}

				return cachedMeshFilter;
			}
		}

		public void DirtyMesh()
		{
			UpdateMesh();
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated mesh as an asset.
		/// Once done, you can remove this component, and set the <b>MeshFilter</b> component's <b>Mesh</b> setting using the exported asset.</summary>
		[ContextMenu("Export Mesh")]
		public void ExportMesh()
		{
			UpdateMesh();

			if (generatedMesh != null)
			{
				CwHelper.ExportAssetDialog(generatedMesh, "Spacetime Mesh");
			}
		}
#endif

		[ContextMenu("Apply Mesh")]
		public void ApplyMesh()
		{
			CachedMeshFilter.sharedMesh = generatedMesh;
		}

		[ContextMenu("Remove Mesh")]
		public void RemoveMesh()
		{
			if (CachedMeshFilter.sharedMesh == generatedMesh)
			{
				CachedMeshFilter.sharedMesh = null;
			}
		}

		/// <summary>This allows you create a new GameObject with the <b>SgtSpacetimeMesh</b> component attached.</summary>
		public static SgtSpacetimeMesh Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		/// <summary>This allows you create a new GameObject with the <b>SgtSpacetimeMesh</b> component attached.</summary>
		public static SgtSpacetimeMesh Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Spacetime Mesh", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtSpacetimeMesh>();
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			var parent = GetComponentInParent<SgtSpacetime>();

			if (parent != null)
			{
				if (parent.Renderers.Contains(CachedMeshRenderer) == false)
				{
					parent.Renderers.Add(CachedMeshRenderer);

					parent.DirtyRenderers();
				}
			}
		}
#endif

		protected virtual void OnEnable()
		{
			UpdateMesh();
		}

		protected virtual void OnDestroy()
		{
			if (generatedMesh != null)
			{
				generatedMesh.Clear(false);

				SgtCommon.MeshPool.Push(generatedMesh);
			}
		}

		// We don't know what was modified, so update everything
		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
		}

		private void UpdateMesh()
		{
			if (quadsX > 0 && quadsZ > 0)
			{
				if (generatedMesh == null)
				{
					generatedMesh = SgtCommon.CreateTempMesh("Spacetime Mesh (Generated)");
				}

				var vertsX    = quadsX + 1;
				var vertsZ    = quadsZ + 1;
				var total     = vertsX * vertsZ;
				var positions = new Vector3[total];
				var coords    = new Vector2[total];
				var indices   = new int[quadsX * quadsZ * 6];
				var stepX     = 1.0f / quadsX;
				var stepZ     = 1.0f / quadsZ;

				for (var z = 0; z < vertsZ; z++)
				{
					for (var x = 0; x < vertsX; x++)
					{
						var u = x * stepX;
						var v = z * stepZ;
						var i = x + z * vertsX;

						coords[i] = new Vector2(u, v);

						if (center == true)
						{
							u -= 0.5f;
							v -= 0.5f;
						}

						positions[i] = new Vector3(u * sizeX, 0.0f, v * sizeZ);
					}
				}

				for (var z = 0; z < quadsZ; z++)
				{
					for (var x = 0; x < quadsX; x++)
					{
						var i = (x + z * quadsX) * 6;
						var a = x + z * vertsX;
						var b = a + 1;
						var c = a + vertsX;
						var d = b + vertsX;

						indices[i + 0] = a;
						indices[i + 1] = c;
						indices[i + 2] = b;

						indices[i + 3] = d;
						indices[i + 4] = b;
						indices[i + 5] = c;
					}
				}

				generatedMesh.Clear(false);
				generatedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
				generatedMesh.vertices    = positions;
				generatedMesh.uv          = coords;
				generatedMesh.triangles   = indices;
				generatedMesh.RecalculateNormals();
				generatedMesh.RecalculateTangents();
				generatedMesh.RecalculateBounds();

				generatedMesh.bounds = new Bounds(generatedMesh.bounds.center, generatedMesh.bounds.size + Vector3.up * 100.0f);
			}

			ApplyMesh();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Spacetime
{
	using UnityEditor;
	using TARGET = SgtSpacetimeMesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSpacetimeMesh_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.SizeX == 0.0f));
				Draw("sizeX", ref dirtyMesh, "The size of the grid along the X axis.");
			EndError();
			BeginError(Any(tgts, t => t.SizeZ == 0.0f));
				Draw("sizeZ", ref dirtyMesh, "The size of the grid along the X axis.");
			EndError();
			BeginError(Any(tgts, t => t.QuadsX < 1));
				Draw("quadsX", ref dirtyMesh, "The amount of quads along the X axis.");
			EndError();
			BeginError(Any(tgts, t => t.QuadsZ < 1));
				Draw("quadsZ", ref dirtyMesh, "The amount of quads along the Z axis.");
			EndError();
			Draw("center", ref dirtyMesh, "Should the mesh be centered, or begin at local 0,0?");

			if (dirtyMesh == true)
			{
				Each(tgts, t => t.DirtyMesh(), true, true);
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Spacetime Mesh", false, 10)]
		public static void CreateItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtSpacetimeMesh.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif