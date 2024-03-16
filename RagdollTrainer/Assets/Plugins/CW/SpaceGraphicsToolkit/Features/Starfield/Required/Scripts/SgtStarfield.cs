using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This is the base class for all starfields that store star corner vertices the same point/location and are stretched out in the vertex shader, allowing billboarding in view space, and dynamic resizing.</summary>
	public abstract class SgtStarfield : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Starfield</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the starfield.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to set the texture applied to the starfield. If this texture can contains multiple stars then you can specify their location using the <b>Atlas</b> setting.</summary>
		public Texture MainTex { set { mainTex = value; } get { return mainTex; } } [SerializeField] private Texture mainTex;

		/// <summary>If the main texture of this material contains multiple textures in an atlas, then you can specify them here.</summary>
		public SgtAtlas Atlas { get { if (atlas == null) atlas = new SgtAtlas(); return atlas; } } [SerializeField] private SgtAtlas atlas;

		/// <summary>Should the stars fade out instead of shrink when they reach a certain minimum size on screen?</summary>
		public float ClampSizeMin { set { if (clampSizeMin != value) { clampSizeMin = value; DirtyMesh(); } } get { return clampSizeMin; } } [SerializeField] [Range(0.0f, 1000.0f)] private float clampSizeMin;

		/// <summary>Should the stars fade out if they're intersecting solid geometry?</summary>
		public float Softness { set { if (softness != value) { softness = value; DirtyMesh(); } } get { return softness; } } [SerializeField] [Range(0.0f, 1000.0f)] private float softness;

		/// <summary>Should the stars stretch if an observer moves?</summary>
		public bool Stretch { set { stretch = value; } get { return stretch; } } [SerializeField] private bool stretch;

		/// <summary>The vector of the stretching.</summary>
		public Vector3 StretchVector { set { stretchVector = value; } get { return stretchVector; } } [SerializeField] private Vector3 stretchVector;

		/// <summary>The scale of the stretching relative to the velocity.</summary>
		public float StretchScale { set { stretchScale = value; } get { return stretchScale; } } [SerializeField] private float stretchScale = 1.0f;

		/// <summary>When warping with the Universe feature, the camera velocity can get too large, this allows you to limit it.</summary>
		public float StretchLimit { set { stretchLimit = value; } get { return stretchLimit; } } [SerializeField] private float stretchLimit = 10000.0f;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtStarfieldModel model;

		[System.NonSerialized]
		protected Mesh mesh;

		[System.NonSerialized]
		protected Material material;

		[System.NonSerialized]
		private bool dirtyMesh = true;

		private static int _SGT_MainTex             = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_NearTex             = Shader.PropertyToID("_SGT_NearTex");
		private static int _SGT_FarTex              = Shader.PropertyToID("_SGT_FarTex");
		private static int _SGT_Color               = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness          = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_Scale               = Shader.PropertyToID("_SGT_Scale");
		private static int _SGT_ClampSizeMin        = Shader.PropertyToID("_SGT_ClampSizeMin");
		private static int _SGT_ClampSizeScale      = Shader.PropertyToID("_SGT_ClampSizeScale");
		private static int _SGT_StretchVector       = Shader.PropertyToID("_SGT_StretchVector");
		private static int _SGT_StretchDirection    = Shader.PropertyToID("_SGT_StretchDirection");
		private static int _SGT_StretchLength       = Shader.PropertyToID("_SGT_StretchLength");
		private static int _SGT_SoftParticlesFactor = Shader.PropertyToID("_SGT_SoftParticlesFactor");
		private static int _SGT_CameraRollAngle     = Shader.PropertyToID("_SGT_CameraRollAngle");

		public SgtStarfieldModel Model
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

		public void DirtyMesh()
		{
			dirtyMesh = true;
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
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

		public SgtStarfieldCustom MakeEditableCopy(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			var gameObject      = CwHelper.CreateGameObject(name + " (Editable Copy)", layer, parent, localPosition, localRotation, localScale, "Create Editable Starfield Copy");
			var customStarfield = CwHelper.AddComponent<SgtStarfieldCustom>(gameObject, false);
			var stars           = customStarfield.Stars;
			var starCount       = BeginQuads();

			for (var i = 0; i < starCount; i++)
			{
				var star = SgtPoolClass<SgtStarfieldStar>.Pop() ?? new SgtStarfieldStar();

				NextQuad(ref star, i);

				stars.Add(star);
			}

			EndQuads();

			// Copy common settings
			customStarfield.sourceMaterial = sourceMaterial;
			customStarfield.atlas          = atlas;
			customStarfield.stretch        = stretch;
			customStarfield.stretchVector  = stretchVector;
			customStarfield.stretchScale   = stretchScale;
			customStarfield.stretchLimit   = stretchLimit;

			return customStarfield;
		}

		public SgtStarfieldCustom MakeEditableCopy(int layer = 0, Transform parent = null)
		{
			return MakeEditableCopy(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

#if UNITY_EDITOR
		[ContextMenu("Make Editable Copy")]
		public void MakeEditableCopyContext()
		{
			var customStarfield = MakeEditableCopy(gameObject.layer, transform.parent, transform.localPosition, transform.localRotation, transform.localScale);

			customStarfield.DirtyMesh();

			CwHelper.SelectAndPing(customStarfield);
		}
#endif

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtStarfieldModel.Create(this);
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
#if UNITY_EDITOR
			SyncMaterial();
#endif
			if (dirtyMesh == true)
			{
				UpdateMesh();
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

		public bool NeedsNearTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_NEAR") == true && material.GetTexture(_SGT_NearTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public bool NeedsFarTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_FAR") == true && material.GetTexture(_SGT_FarTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		private void UpdateMesh()
		{
			dirtyMesh = false;

			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Starfield Mesh (Generated)");
			}
			else
			{
				mesh.Clear();
			}

			var count = BeginQuads();

			BuildMesh(count);

			EndQuads();
		}

		protected virtual void SetBrightness(Camera camera)
		{
			material.SetFloat(_SGT_Brightness, brightness);
		}

		protected virtual void HandleCameraPreRender(Camera camera)
		{
			if (sourceMaterial != null)
			{
				if (material == null)
				{
					material = CwHelper.CreateTempMaterial("Starfield (Generated)", sourceMaterial);

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
				material.SetFloat(_SGT_Scale, CwHelper.UniformScale(transform.lossyScale));

				SetBrightness(camera);

				if (clampSizeMin > 0.0f)
				{
					SgtCommon.EnableKeyword("_SGT_CLAMP_SIZE_MIN", material);

					material.SetFloat(_SGT_ClampSizeMin, clampSizeMin);

					if (camera.orthographic == true)
					{
						material.SetFloat(_SGT_ClampSizeScale, camera.orthographicSize * 0.0025f);
					}
					else
					{
						material.SetFloat(_SGT_ClampSizeScale, Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 2.0f);
					}
				}
				else
				{
					SgtCommon.DisableKeyword("_SGT_CLAMP_SIZE_MIN", material);
				}

				if (softness > 0.0f)
				{
					SgtCommon.EnableKeyword("_SGT_SOFTNESS", material);

					material.SetFloat(_SGT_SoftParticlesFactor, CwHelper.Reciprocal(softness));
				}
				else
				{
					SgtCommon.DisableKeyword("_SGT_SOFTNESS", material);
				}

				if (stretch == true)
				{
					SgtCommon.EnableKeyword("_SGT_STRETCH", material);
				}
				else
				{
					SgtCommon.DisableKeyword("_SGT_STRETCH", material);
				}

				var sgtCamera = default(SgtCamera);
				var velocity  = Vector3.zero;

				if (SgtCamera.TryFind(camera, ref sgtCamera) == true)
				{
					material.SetFloat(_SGT_CameraRollAngle, sgtCamera.RollAngle * Mathf.Deg2Rad);

					var cameraVelocity = sgtCamera.Velocity;
					var cameraSpeed    = cameraVelocity.magnitude;

					if (cameraSpeed > stretchLimit)
					{
						cameraVelocity = cameraVelocity.normalized * stretchLimit;
					}

					velocity += cameraVelocity * stretchScale;
				}
				else
				{
					material.SetFloat(_SGT_CameraRollAngle, 0.0f);
				}

				if (stretch == true)
				{
					material.SetVector(_SGT_StretchVector, velocity);
					material.SetVector(_SGT_StretchDirection, velocity.normalized);
					material.SetFloat(_SGT_StretchLength, velocity.magnitude);
				}
				else
				{
					material.SetVector(_SGT_StretchVector, Vector3.zero);
					material.SetVector(_SGT_StretchDirection, Vector3.zero);
					material.SetFloat(_SGT_StretchLength, 0.0f);
				}
			}
		}

		protected abstract int BeginQuads();

		protected abstract void NextQuad(ref SgtStarfieldStar quad, int starIndex);

		protected abstract void EndQuads();

		private static List<Vector3> positions = new List<Vector3>();
		private static List<Vector3> normals   = new List<Vector3>();
		private static List<Color32> colors32  = new List<Color32>();
		private static List<Vector3> coords1   = new List<Vector3>();
		private static List<Vector2> coords2   = new List<Vector2>();
		private static List<int>     indices   = new List<int>();

		protected virtual void BuildMesh(int count)
		{
			var tempCoords = Atlas.GetCoords(); // NOTE: Property
			var minMaxSet  = false;
			var min        = default(Vector3);
			var max        = default(Vector3);

			CwHelper.Resize(positions, count * 4);
			CwHelper.Resize(normals, count * 4);
			CwHelper.Resize(colors32, count * 4);
			CwHelper.Resize(coords1, count * 4);
			CwHelper.Resize(coords2, count * 4);
			CwHelper.Resize(indices, count * 6);

			for (var i = 0; i < count; i++)
			{
				NextQuad(ref SgtStarfieldStar.Temp, i);

				var offV     = i * 4;
				var offI     = i * 6;
				var position = SgtStarfieldStar.Temp.Position;
				var radius   = SgtStarfieldStar.Temp.Radius;
				var angle    = Mathf.Repeat(SgtStarfieldStar.Temp.Angle / 180.0f, 2.0f) - 1.0f;
				var uv       = tempCoords[CwHelper.Mod(SgtStarfieldStar.Temp.Variant, tempCoords.Count)];
				var pulse    = Mathf.Clamp01(SgtStarfieldStar.Temp.PulseSpeed) * 0.99f + Mathf.Clamp01(SgtStarfieldStar.Temp.PulseOffset) * 4096.0f;

				SgtCommon.ExpandBounds(ref minMaxSet, ref min, ref max, position, radius);

				positions[offV + 0] =
				positions[offV + 1] =
				positions[offV + 2] =
				positions[offV + 3] = position;

				colors32[offV + 0] =
				colors32[offV + 1] =
				colors32[offV + 2] =
				colors32[offV + 3] = SgtStarfieldStar.Temp.Color;

				normals[offV + 0] = new Vector3(-1.0f,  1.0f, angle);
				normals[offV + 1] = new Vector3( 1.0f,  1.0f, angle);
				normals[offV + 2] = new Vector3(-1.0f, -1.0f, angle);
				normals[offV + 3] = new Vector3( 1.0f, -1.0f, angle);

				coords1[offV + 0] = new Vector3(uv.x, uv.y, pulse);
				coords1[offV + 1] = new Vector3(uv.z, uv.y, pulse);
				coords1[offV + 2] = new Vector3(uv.x, uv.w, pulse);
				coords1[offV + 3] = new Vector3(uv.z, uv.w, pulse);

				coords2[offV + 0] = new Vector3(radius,  0.5f);
				coords2[offV + 1] = new Vector3(radius, -0.5f);
				coords2[offV + 2] = new Vector3(radius,  0.5f);
				coords2[offV + 3] = new Vector3(radius, -0.5f);

				indices[offI + 0] = offV + 0;
				indices[offI + 1] = offV + 1;
				indices[offI + 2] = offV + 2;
				indices[offI + 3] = offV + 3;
				indices[offI + 4] = offV + 2;
				indices[offI + 5] = offV + 1;
			}

			mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			mesh.bounds      = SgtCommon.NewBoundsFromMinMax(min, max);

			mesh.Clear();
			mesh.SetVertices(positions);
			mesh.SetColors(colors32);
			mesh.SetNormals(normals);
			mesh.SetUVs(0, coords1);
			mesh.SetUVs(1, coords2);
			mesh.SetTriangles(indices, 0, false);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Starfield
{
	using UnityEditor;
	using TARGET = SgtStarfield;

	public class SgtStarfield_Editor : CwEditor
	{
		protected void DrawBasic(ref bool dirtyMesh)
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Starfield</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the starfield.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			BeginError(Any(tgts, t => t.MainTex == null));
				Draw("mainTex", "This allows you to set the texture applied to the starfield. If this texture can contains multiple stars then you can specify their location using the <b>Atlas</b> setting.");
			EndError();
			Draw("atlas", ref dirtyMesh, "If the main texture of this material contains multiple textures in an atlas, then you can specify them here.");

			Separator();

			Draw("clampSizeMin", "Should the stars fade out instead of shrink when they reach a certain minimum size on screen?");
			Draw("softness", "Should the stars fade out if they're intersecting solid geometry?");

			if (Any(tgts, t => t.Softness > 0.0f))
			{
				CwDepthTextureMode_Editor.RequireDepth();
			}

			Separator();

			Draw("stretch", "Should the stars stretch if an observer moves?");

			if (Any(tgts, t => t.Stretch == true))
			{
				BeginIndent();
					Draw("stretchVector", "The vector of the stretching.");
					BeginError(Any(tgts, t => t.StretchScale < 0.0f));
						Draw("stretchScale", "The scale of the stretching relative to the velocity.");
					EndError();
					BeginError(Any(tgts, t => t.StretchLimit <= 0.0f));
						Draw("stretchLimit", "When warping with the Universe feature the camera velocity can get too large, this allows you to limit it.");
					EndError();
				EndIndent();
			}

			if (Any(tgts, t => t.NeedsNearTex == true && t.GetComponent<SgtStarfieldNearTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a NearTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtStarfieldNearTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsFarTex == true && t.GetComponent<SgtStarfieldFarTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a FarTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtStarfieldFarTex>(t.gameObject));
				}
			}
		}
	}
}
#endif