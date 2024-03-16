using UnityEngine;
using CW.Common;
using SpaceGraphicsToolkit.LightAndShadow;

namespace SpaceGraphicsToolkit.Belt
{
	/// <summary>This base class contains the functionality to render an asteroid belt.</summary>
	public abstract class SgtBelt : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Belt</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the belt.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the texture applied to the belt. If this texture can contains multiple asteroids then you can specify their location using the <b>Atlas</b> setting.</summary>
		public Texture MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Texture mainTex;

		/// <summary>If the main texture of this material contains multiple textures in an atlas, then you can specify them here.</summary>
		public SgtAtlas Atlas { get { if (atlas == null) atlas = new SgtAtlas(); return atlas; } } [SerializeField] private SgtAtlas atlas;

		/// <summary>The amount of seconds this belt has been animating for.</summary>
		public float OrbitOffset { set { orbitOffset = value; } get { return orbitOffset; } } [SerializeField] private float orbitOffset;

		/// <summary>The animation speed of this belt.</summary>
		public float OrbitSpeed { set { orbitSpeed = value; } get { return orbitSpeed; } } [SerializeField] private float orbitSpeed = 1.0f;

		public event System.Action<Material> OnSetProperties;

		private static int _SGT_MainTex         = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_LightingTex     = Shader.PropertyToID("_SGT_LightingTex");
		private static int _SGT_Color           = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness      = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_Scale           = Shader.PropertyToID("_SGT_Scale");
		private static int _SGT_Age             = Shader.PropertyToID("_SGT_Age");
		private static int _SGT_CameraRollAngle = Shader.PropertyToID("_SGT_CameraRollAngle");

		[SerializeField]
		private SgtBeltModel model;

		[System.NonSerialized]
		private Mesh mesh;

		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private bool dirtyMesh = true;

		public SgtBeltModel Model
		{
			get
			{
				return model;
			}
		}

		public Material Material
		{
			get
			{
				return material;
			}
		}

		protected abstract float GetOuterRadius();

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		public static Vector3 CalculateLocalPosition(ref SgtBeltAsteroid asteroid, float age)
		{
			var a = asteroid.OrbitAngle + asteroid.OrbitSpeed * age;
			var x = (float)System.Math.Sin(a) * asteroid.OrbitDistance;
			var y = asteroid.Height;
			var z = (float)System.Math.Cos(a) * asteroid.OrbitDistance;

			return new Vector3(x, y, z);
		}

		public SgtBeltCustom MakeEditableCopy(int layer = 0, Transform parent = null)
		{
			return MakeEditableCopy(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public SgtBeltCustom MakeEditableCopy(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject = CwHelper.CreateGameObject(name + " (Editable Copy)", layer, parent, localPosition, localRotation, localScale, "Create Editable Belt Copy");
			var customBelt = CwHelper.AddComponent<SgtBeltCustom>(gameObject, false);
			var asteroids  = customBelt.Asteroids;
			var quadCount  = BeginQuads();

			for (var i = 0; i < quadCount; i++)
			{
				var asteroid = SgtPoolClass<SgtBeltAsteroid>.Pop() ?? new SgtBeltAsteroid();

				NextQuad(ref asteroid, i);

				asteroids.Add(asteroid);
			}

			EndQuads();

			// Copy common settings
			customBelt.sourceMaterial = sourceMaterial;
			customBelt.color          = color;
			customBelt.brightness     = brightness;
			customBelt.mainTex        = mainTex;
			customBelt.atlas          = atlas;
			customBelt.orbitOffset    = orbitOffset;
			customBelt.orbitSpeed     = orbitSpeed;

			return customBelt;
		}

#if UNITY_EDITOR
		[ContextMenu("Make Editable Copy")]
		public void MakeEditableCopyContext()
		{
			var customBelt = MakeEditableCopy(gameObject.layer, transform.parent, transform.localPosition, transform.localRotation, transform.localScale);

			customBelt.DirtyMesh();

			CwHelper.SelectAndPing(customBelt);
		}
#endif

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtBeltModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;
		}

		protected virtual void OnDisable()
		{
			CwHelper.OnCameraPreRender -= HandleCameraPreRender;

			if (model != null)
			{
				model.CachedMeshRenderer.enabled = false;
			}
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(mesh);
			CwHelper.Destroy(material);
		}

		protected virtual void LateUpdate()
		{
			if (dirtyMesh == true)
			{
				UpdateMesh();
			}

#if UNITY_EDITOR
			SyncMaterial();
#endif

			if (Application.isPlaying == true)
			{
				orbitOffset += Time.deltaTime * orbitSpeed;
			}

			if (material != null)
			{
				material.SetFloat(_SGT_Age, orbitOffset);
			}

			if (model != null)
			{
				model.CachedMeshFilter.sharedMesh = mesh;
			}
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

		public bool NeedsLightingTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_LIGHTING") == true && material.GetTexture(_SGT_LightingTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		private void UpdateMesh()
		{
			dirtyMesh = false;

			var count = BeginQuads();

			BuildMesh(count);

			EndQuads();
		}

		[ContextMenu("Sync Material")]
		public void SyncMaterial()
		{
			if (sourceMaterial != null)
			{
				if (material != null)
				{
					if (material.shader != sourceMaterial.shader)
					{
						material.shader = sourceMaterial.shader;
					}

					material.CopyPropertiesFromMaterial(sourceMaterial);

					if (OnSetProperties != null)
					{
						OnSetProperties.Invoke(material);
					}
				}
			}
		}

		private void HandleCameraPreRender(Camera camera)
		{
			if (sourceMaterial != null)
			{
				if (material == null)
				{
					material = CwHelper.CreateTempMaterial("Belt (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}

				material.SetTexture(_SGT_MainTex, mainTex != null ? mainTex : Texture2D.whiteTexture);
				material.SetColor(_SGT_Color, color);
				material.SetFloat(_SGT_Brightness, brightness);
				material.SetFloat(_SGT_Scale, CwHelper.UniformScale(transform.lossyScale));
				material.SetFloat(_SGT_Age, orbitOffset);

				if (material.IsKeywordEnabled("_SGT_LIGHTING") == true)
				{
					// Write lights and shadows
					CwHelper.SetTempMaterial(material);

					var mask   = 1 << gameObject.layer;
					var lights = SgtLight.Find(mask, transform.position);

					SgtShadow.Find(true, mask, lights);
					SgtShadow.FilterOutRing(transform.position);
					SgtShadow.WriteSphere(SgtShadow.MAX_SPHERE_SHADOWS);
					SgtShadow.WriteRing(SgtShadow.MAX_RING_SHADOWS);

					SgtLight.Write(transform.position, CwHelper.UniformScale(transform.lossyScale) * GetOuterRadius(), null, null, SgtLight.MAX_LIGHTS);
				}

				var sgtCamera = default(SgtCamera);

				if (SgtCamera.TryFind(camera, ref sgtCamera) == true)
				{
					material.SetFloat(_SGT_CameraRollAngle, sgtCamera.RollAngle * Mathf.Deg2Rad);
				}
				else
				{
					material.SetFloat(_SGT_CameraRollAngle, 0.0f);
				}
			}
		}

		protected abstract int BeginQuads();

		protected abstract void NextQuad(ref SgtBeltAsteroid asteroid, int asteroidIndex);

		protected abstract void EndQuads();

		protected virtual void BuildMesh(int count)
		{
			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Belt Mesh (Generated)");
			}
			else
			{
				mesh.Clear();
			}

			var tempCoords = Atlas.GetCoords(); // NOTE: Property
			var positions  = new Vector3[count * 4];
			var colors     = new Color[count * 4];
			var normals    = new Vector3[count * 4];
			var tangents   = new Vector4[count * 4];
			var coords1    = new Vector2[count * 4];
			var coords2    = new Vector2[count * 4];
			var indices    = new int[count * 6];
			var maxWidth   = 0.0f;
			var maxHeight  = 0.0f;

			for (var i = 0; i < count; i++)
			{
				NextQuad(ref SgtBeltAsteroid.Temp, i);

				var offV     = i * 4;
				var offI     = i * 6;
				var radius   = SgtBeltAsteroid.Temp.Radius;
				var distance = SgtBeltAsteroid.Temp.OrbitDistance;
				var height   = SgtBeltAsteroid.Temp.Height;
				var uv       = tempCoords[CwHelper.Mod(SgtBeltAsteroid.Temp.Variant, tempCoords.Count)];

				maxWidth  = Mathf.Max(maxWidth , distance + radius);
				maxHeight = Mathf.Max(maxHeight, height   + radius);

				positions[offV + 0] =
				positions[offV + 1] =
				positions[offV + 2] =
				positions[offV + 3] = new Vector3(SgtBeltAsteroid.Temp.OrbitAngle, distance, SgtBeltAsteroid.Temp.OrbitSpeed);

				colors[offV + 0] =
				colors[offV + 1] =
				colors[offV + 2] =
				colors[offV + 3] = SgtBeltAsteroid.Temp.Color;

				normals[offV + 0] = new Vector3(-1.0f,  1.0f, 0.0f);
				normals[offV + 1] = new Vector3( 1.0f,  1.0f, 0.0f);
				normals[offV + 2] = new Vector3(-1.0f, -1.0f, 0.0f);
				normals[offV + 3] = new Vector3( 1.0f, -1.0f, 0.0f);

				tangents[offV + 0] =
				tangents[offV + 1] =
				tangents[offV + 2] =
				tangents[offV + 3] = new Vector4(SgtBeltAsteroid.Temp.Angle / Mathf.PI, SgtBeltAsteroid.Temp.Spin / Mathf.PI, 0.0f, 0.0f);

				coords1[offV + 0] = new Vector2(uv.x, uv.y);
				coords1[offV + 1] = new Vector2(uv.z, uv.y);
				coords1[offV + 2] = new Vector2(uv.x, uv.w);
				coords1[offV + 3] = new Vector2(uv.z, uv.w);

				coords2[offV + 0] =
				coords2[offV + 1] =
				coords2[offV + 2] =
				coords2[offV + 3] = new Vector2(radius, height);

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.vertices    = positions;
			mesh.colors      = colors;
			mesh.normals     = normals;
			mesh.tangents    = tangents;
			mesh.uv          = coords1;
			mesh.uv2         = coords2;
			mesh.triangles   = indices;
			mesh.bounds      = new Bounds(Vector3.zero, new Vector3(maxWidth * 2.0f, maxHeight * 2.0f, maxWidth * 2.0f));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Belt
{
	using UnityEditor;
	using TARGET = SgtBelt;

	public abstract class SgtBelt_Editor : CwEditor
	{
		protected void DrawBasic(ref bool dirtyMesh)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Belt</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the belt.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the texture applied to the belt. If this texture can contains multiple asteroids then you can specify their location using the <b>Atlas</b> setting.");
			EndError();
			Draw("atlas", ref dirtyMesh, "If the main texture of this material contains multiple textures in an atlas, then you can specify them here.");

			Draw("orbitOffset", "The amount of seconds this belt has been animating for."); // Updated automatically
			Draw("orbitSpeed", "The animation speed of this belt."); // Updated automatically

			if (Any(tgts, t => t.NeedsLightingTex == true && t.GetComponent<SgtBeltLightingTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a LightingTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtBeltLightingTex>(t.gameObject));
				}
			}
		}
	}
}
#endif