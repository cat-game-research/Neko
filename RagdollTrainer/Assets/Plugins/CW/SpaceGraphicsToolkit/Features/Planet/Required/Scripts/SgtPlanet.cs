using System.Collections.Generic;
using UnityEngine;
using CW.Common;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to render a planet that has been displaced with a heightmap, and has a dynamic water level.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtPlanet")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Planet")]
	public class SgtPlanet : MonoBehaviour, IOverridableSharedMaterial
	{
		class Seam
		{
			public List<int> Indices = new List<int>();

			public static Stack<Seam> Pool = new Stack<Seam>();
		}

		class Geom
		{
			public List<Seam> Seams = new List<Seam>();
		}

		/// <summary>The sphere mesh used to render the planet.</summary>
		public Mesh Mesh { set { if (mesh != value) { mesh = value; DirtyMesh(); } } get { return mesh; } } [SerializeField] private Mesh mesh;

		/// <summary>If you want the generated mesh to have a matching collider, you can specify it here.</summary>
		public MeshCollider MeshCollider { set { if (meshCollider != value) { meshCollider = value; DirtyMesh(); } } get { return meshCollider; } } [SerializeField] private MeshCollider meshCollider;

		/// <summary>The radius of the planet in local space.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMesh(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The material used to render the planet. For best results, this should use the SGT Planet shader.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>If you want to apply a shared material (e.g. atmosphere) to this terrain, then specify it here.</summary>
		public SgtSharedMaterial SharedMaterial { set { sharedMaterial = value; } get { return sharedMaterial; } } [SerializeField] private SgtSharedMaterial sharedMaterial;

		/// <summary>Should the planet cast shadows?</summary>
		public bool CastShadows { set { castShadows = value; } get { return castShadows; } } [SerializeField] private bool castShadows = true;

		/// <summary>Should the planet receive shadows?</summary>
		public bool ReceiveShadows { set { receiveShadows = value; } get { return receiveShadows; } } [SerializeField] private bool receiveShadows = true;

		/// <summary>The current water level.
		/// 0 = Radius.
		/// 1 = Radius + Displacement.</summary>
		public float WaterLevel { set { if (waterLevel != value) { waterLevel = value; DirtyMesh(); } } get { return waterLevel; } } [Range(-2.0f, 2.0f)] [SerializeField] private float waterLevel;

		/// <summary>Should the planet mesh be displaced using the heightmap in the planet material?</summary>
		public bool Displace { set { if (displace != value) { displace = value; DirtyMesh(); } } get { return displace; } } [SerializeField] private bool displace;

		/// <summary>The maximum height displacement applied to the planet mesh when the heightmap alpha value is 1.</summary>
		public float Displacement { set { if (displacement != value) { displacement = value; DirtyMesh(); } } get { return displacement; } } [SerializeField] private float displacement = 0.1f;

		/// <summary>If you enable this then the water will not rise, instead the terrain will shrink down.</summary>
		public bool ClampWater { set { if (clampWater != value) { clampWater = value; DirtyMesh(); } } get { return clampWater; } } [SerializeField] private bool clampWater;

		public event SgtSharedMaterial.OverrideSharedMaterialSignature OnOverrideSharedMaterial;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private List<Vector3> generatedPositions = new List<Vector3>();

		[System.NonSerialized]
		private List<Vector3> generatedNormals = new List<Vector3>();

		[System.NonSerialized]
		private List<Vector4> generatedTangents = new List<Vector4>();

		[System.NonSerialized]
		private SgtProperties properties = new SgtProperties();

		[System.NonSerialized]
		private bool dirtyMesh;

		[System.NonSerialized]
		private Texture2D lastHeightmap;

		private static Dictionary<Mesh, Geom> meshToGeom = new Dictionary<Mesh, Geom>();

		private static Dictionary<Vector3, Seam> tempPoints = new Dictionary<Vector3, Seam>();

		private static int _HeightMap      = Shader.PropertyToID("_HeightMap");
		private static int _HasWater       = Shader.PropertyToID("_HasWater");
		private static int _WaterLevel     = Shader.PropertyToID("_WaterLevel");
		private static int _HasNight       = Shader.PropertyToID("_HasNight");
		private static int _NightDirection = Shader.PropertyToID("_NightDirection");

		public SgtProperties Properties
		{
			get
			{
				return properties;
			}
		}

		public Texture2D MaterialHeightmap
		{
			get
			{
				return material != null ? material.GetTexture(_HeightMap) as Texture2D : null;
			}
		}

		public bool MaterialHasWater
		{
			get
			{
				return material != null ? material.GetFloat(_HasWater) == 1.0f : false;
			}
		}

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		public void RegisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e)
		{
			OnOverrideSharedMaterial += e;
		}

		public void UnregisterSharedMaterialOverride(SgtSharedMaterial.OverrideSharedMaterialSignature e)
		{
			OnOverrideSharedMaterial -= e;
		}

		/// <summary>This method causes the planet mesh to update based on the current settings. You should call this after you finish modifying them.</summary>
		[ContextMenu("Rebuild")]
		public void Rebuild()
		{
			dirtyMesh     = false;
			generatedMesh = CwHelper.Destroy(generatedMesh);

			if (mesh != null)
			{
				generatedMesh = Instantiate(mesh);

				if (displace == true)
				{
					var count = generatedMesh.vertexCount;

					lastHeightmap = MaterialHeightmap;

					generatedMesh.GetVertices(generatedPositions);

					for (var i = 0; i < count; i++)
					{
						var vector = generatedPositions[i].normalized;

						generatedPositions[i] = vector * Sample(vector);
					}

					generatedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * (radius + displacement) * 2.0f);

					generatedMesh.SetVertices(generatedPositions);

					generatedMesh.RecalculateNormals();
					generatedMesh.RecalculateTangents();

					generatedMesh.GetNormals(generatedNormals);
					generatedMesh.GetTangents(generatedTangents);

					// Fix seams
					var geom = GetGeom(mesh);

					foreach (var seam in geom.Seams)
					{
						if (seam.Indices.Count > 1)
						{
							var averageNormal  = default(Vector3);
							var averageTangent = default(Vector4);

							for (var i = 0; i < seam.Indices.Count; i++)
							{
								var index = seam.Indices[i];

								averageNormal  += generatedNormals[index];
								averageTangent += generatedTangents[index];
							}

							averageNormal  /= seam.Indices.Count;
							averageTangent /= seam.Indices.Count;

							for (var i = 0; i < seam.Indices.Count; i++)
							{
								var index = seam.Indices[i];

								generatedNormals[index] = averageNormal;
								generatedTangents[index] = averageTangent;
							}
						}
					}

					generatedMesh.SetNormals(generatedNormals);
					generatedMesh.SetTangents(generatedTangents);
				}
				else
				{
					generatedMesh.GetVertices(generatedPositions);

					var count = generatedMesh.vertexCount;
					var scale = radius / generatedPositions[0].magnitude;

					for (var i = 0; i < count; i++)
					{
						generatedPositions[i] *= scale;
					}

					generatedMesh.bounds = new Bounds(Vector3.zero, Vector3.one * (radius + displacement) * 2.0f);

					generatedMesh.SetVertices(generatedPositions);
				}

				if (meshCollider != null)
				{
					meshCollider.sharedMesh = null;
					meshCollider.sharedMesh = generatedMesh;
				}
			}
		}

		private static Geom GetGeom(Mesh mesh)
		{
			var geom = default(Geom);

			if (meshToGeom.TryGetValue(mesh, out geom) == false)
			{
				geom = new Geom();

				tempPoints.Clear();

				var positions = mesh.vertices;

				for (var i = 0; i < positions.Length; i++)
				{
					var point = positions[i];
					var seam  = default(Seam);

					if (tempPoints.TryGetValue(point, out seam) == false)
					{
						seam = Seam.Pool.Count > 0 ? Seam.Pool.Pop() : new Seam();

						tempPoints.Add(point, seam);
					}

					seam.Indices.Add(i);
				}

				foreach (var pair in tempPoints)
				{
					var seam = pair.Value;

					if (seam.Indices.Count > 1)
					{
						geom.Seams.Add(pair.Value);
					}
					else
					{
						seam.Indices.Clear();

						Seam.Pool.Push(seam);
					}
				}

				tempPoints.Clear();

				meshToGeom.Add(mesh, geom);
			}

			return geom;
		}

		public static SgtPlanet Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtPlanet Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Planet", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtPlanet>();
		}

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw        += HandleCameraDraw;
			SgtCommon.OnCalculateDistance += HandleCalculateDistance;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw        -= HandleCameraDraw;
			SgtCommon.OnCalculateDistance -= HandleCalculateDistance;
		}

		protected virtual void LateUpdate()
		{
			if (generatedMesh == null || dirtyMesh == true)
			{
				Rebuild();
			}

			if (generatedMesh != null && material != null)
			{
				Properties.SetFloat(_WaterLevel, waterLevel);

				// Write direction of nearest light?
				if (material.GetFloat(_HasNight) == 1.0f)
				{
					var mask   = 1 << gameObject.layer;
					var lights = SgtLight.Find(mask, transform.position);

					SgtLight.FilterOut(transform.position);

					if (lights.Count > 0)
					{
						var position  = Vector3.zero;
						var direction = Vector3.forward;
						var color     = Color.white;
						var intensity = 0.0f;

						SgtLight.Calculate(lights[0], transform.position, 0.0f, default(Transform), default(Transform), ref position, ref direction, ref color, ref intensity);

						properties.SetVector(_NightDirection, -direction);
					}
				}
			}
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
		}

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtCommon.CanDraw(gameObject, camera) == false) return;

			//var layer = SgtHelper.GetRenderingLayers(gameObject, renderingLayer);
			var layer = gameObject.layer;

			Graphics.DrawMesh(generatedMesh, transform.localToWorldMatrix, material, layer, camera, 0, properties, castShadows, receiveShadows);

			var finalSharedMaterial = sharedMaterial;

			if (OnOverrideSharedMaterial != null)
			{
				OnOverrideSharedMaterial.Invoke(ref finalSharedMaterial, camera);
			}

			if (CwHelper.Enabled(finalSharedMaterial) == true && finalSharedMaterial.Material != null)
			{
				Graphics.DrawMesh(generatedMesh, transform.localToWorldMatrix, finalSharedMaterial.Material, layer, camera, 0, properties);
			}
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(generatedMesh);
		}

		private void HandleCalculateDistance(Vector3 worldPosition, ref float distance)
		{
			var localPosition = transform.InverseTransformPoint(worldPosition);

			localPosition = localPosition.normalized * Sample(localPosition);

			var surfacePosition = transform.TransformPoint(localPosition);
			var thisDistance    = Vector3.Distance(worldPosition, surfacePosition);

			if (thisDistance < distance)
			{
				distance = thisDistance;
			}
		}

		private float Sample(Vector3 vector)
		{
			var final = radius;

			if (lastHeightmap != null)
			{
				var uv   = SgtCommon.CartesianToPolarUV(vector);
				var land = lastHeightmap.GetPixelBilinear(uv.x, uv.y).a;

				if (clampWater == true)
				{
					final += displacement * Mathf.InverseLerp(Mathf.Clamp01(waterLevel), 1.0f, land);
				}
				else
				{
					final += displacement * Mathf.Max(land, waterLevel);
				}
			}

			return final;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtPlanet;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtPlanet_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.Mesh == null));
				Draw("mesh", ref dirtyMesh, "The sphere mesh used to render the planet.");
			EndError();
			//Draw("renderingLayer", "The rendering layer used to render the planet.");
			Draw("meshCollider", ref dirtyMesh, "If you want the generated mesh to have a matching collider, you can specify it here.");
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the planet in local space.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", "The material used to render the planet. For best results, this should use the SGT Planet shader.");
			EndError();
			Draw("sharedMaterial", "If you want to apply a shared material (e.g. atmosphere) to this terrain, then specify it here.");
			Draw("castShadows", "Should the planet cast shadows?");
			Draw("receiveShadows", "Should the planet receive shadows?");

			Separator();

			if (Any(tgts, t => t.MaterialHeightmap != null))
			{
				if (Any(tgts, t => t.MaterialHasWater == true))
				{
					Draw("waterLevel", ref dirtyMesh, "The current water level.\n\n0 = Radius.\n\n1 = Radius + Displacement.");
				}
				Draw("displace", ref dirtyMesh, "Should the planet mesh be displaced using the heightmap in the planet material?");
				if (Any(tgts, t => t.Displace == true))
				{
					BeginIndent();
						BeginError(Any(tgts, t => t.Displacement == 0.0f));
							Draw("displacement", ref dirtyMesh, "The maximum height displacement applied to the planet mesh when the heightmap alpha value is 1.");
						EndError();
						Draw("clampWater", ref dirtyMesh, "If you enable this then the water will not rise, instead the terrain will shrink down.");
					EndIndent();
				}
			}

			if (Any(tgts, t => t.MaterialHasWater == true && t.GetComponent<SgtPlanetWaterGradient>() == null))
			{
				Separator();

				if (HelpButton("This material has water, but you have no WaterGradient component.", UnityEditor.MessageType.Info, "Fix", 50) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtPlanetWaterGradient>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.MaterialHasWater == true && t.GetComponent<SgtPlanetWaterTexture>() == null))
			{
				Separator();

				if (HelpButton("This material has water, but you have no WaterTexture component.", UnityEditor.MessageType.Info, "Fix", 50) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtPlanetWaterTexture>(t.gameObject));
				}
			}

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Planet", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtPlanet.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif