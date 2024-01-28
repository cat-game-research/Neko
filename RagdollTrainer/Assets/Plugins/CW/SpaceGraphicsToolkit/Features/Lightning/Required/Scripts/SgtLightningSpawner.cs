using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Lightning
{
	/// <summary>This component allows you to spawn animated lightning sprites around a planet.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtLightningSpawner")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Lightning Spawner")]
	public class SgtLightningSpawner : MonoBehaviour
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Lightning</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>The minimum delay between lightning spawns.</summary>
		public float DelayMin { set { delayMin = value; } get { return delayMin; } } [SerializeField] private float delayMin = 0.25f;

		/// <summary>The maximum delay between lightning spawns.</summary>
		public float DelayMax { set { delayMax = value; } get { return delayMax; } } [SerializeField] private float delayMax = 5.0f;

		/// <summary>The minimum life of each spawned lightning.</summary>
		public float LifeMin { set { lifeMin = value; } get { return lifeMin; } } [SerializeField] private float lifeMin = 0.5f;

		/// <summary>The maximum life of each spawned lightning.</summary>
		public float LifeMax { set { lifeMax = value; } get { return lifeMax; } } [SerializeField] private float lifeMax = 1.0f;

		/// <summary>The radius of the spawned lightning mesh in local coordinates.</summary>
		public float Radius { set { if (radius != value) { radius = value; DirtyMesh(); } } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The size of the lightning in degrees.</summary>
		public float Size { set { if (size != value) { size = value; DirtyMesh(); } } get { return size; } } [SerializeField] private float size = 10.0f;

		/// <summary>The amount of rows and columns in the lightning mesh.</summary>
		public int Detail { set { if (detail != value) { detail = value; DirtyMesh(); } } get { return detail; } } [SerializeField] [Range(1, 100)] private int detail = 10;

		/// <summary>When lightning is spawned, its base color will be randomly picked from this gradient.</summary>
		public Gradient Colors { get { if (colors == null) colors = SgtCommon.CreateGradient(Color.white); return colors; } } [SerializeField] private Gradient colors;

		/// <summary>The lightning color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>The random sprite used by the lightning.</summary>
		public List<Sprite> Sprites { get { if (sprites == null) sprites = new List<Sprite>(); return sprites; } } [SerializeField] private List<Sprite> sprites;

		[System.NonSerialized]
		private Mesh mesh;

		// When this reaches 0 a new lightning is spawned
		[System.NonSerialized]
		private float cooldown;

		private static int _SGT_MainTex    = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_Color      = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_Age        = Shader.PropertyToID("_SGT_Age");
		private static int _SGT_Offset     = Shader.PropertyToID("_SGT_Offset");
		private static int _SGT_Scale      = Shader.PropertyToID("_SGT_Scale");

		public Sprite RandomSprite
		{
			get
			{
				if (sprites != null)
				{
					var count = sprites.Count;

					if (count > 0)
					{
						var index = Random.Range(0, count);

						return sprites[index];
					}
				}

				return null;
			}
		}

		public Color RandomColor
		{
			get
			{
				return Colors.Evaluate(Random.value); // NOTE: Property
			}
		}

		public void DirtyMesh()
		{
			UpdateMesh();
		}

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Lightning");
			}
			else
			{
				mesh.Clear(false);
			}

			var detailAddOne = detail + 1;
			var positions    = new Vector3[detailAddOne * detailAddOne];
			var coords       = new Vector2[detailAddOne * detailAddOne];
			var indices      = new int[detail * detail * 6];
			var invDetail    = CwHelper.Reciprocal(detail);

			for (var y = 0; y < detailAddOne; y++)
			{
				for (var x = 0; x < detailAddOne; x++)
				{
					var vertex = x + y * detailAddOne;
					var fracX  = x * invDetail;
					var fracY  = y * invDetail;
					var angX   = (fracX - 0.5f) * size;
					var angY   = (fracY - 0.5f) * size;

					// TODO: Manually do this rotation
					positions[vertex] = Quaternion.Euler(angX, angY, 0.0f) * new Vector3(0.0f, 0.0f, radius);

					coords[vertex] = new Vector2(fracX, fracY);
				}
			}

			for (var y = 0; y < detail; y++)
			{
				for (var x = 0; x < detail; x++)
				{
					var index  = (x + y * detail) * 6;
					var vertex = x + y * detailAddOne;

					indices[index + 0] = vertex;
					indices[index + 1] = vertex + 1;
					indices[index + 2] = vertex + detailAddOne;
					indices[index + 3] = vertex + detailAddOne + 1;
					indices[index + 4] = vertex + detailAddOne;
					indices[index + 5] = vertex + 1;
				}
			}

			mesh.vertices  = positions;
			mesh.uv        = coords;
			mesh.triangles = indices;
		}

		public SgtLightning Spawn()
		{
			if (mesh != null && sourceMaterial != null && lifeMin > 0.0f && lifeMax > 0.0f)
			{
				var sprite = RandomSprite;

				if (sprite != null)
				{
					var lightning  = SgtLightning.Create(this);
					var properties = lightning.Properties;
					var uv         = SgtCommon.CalculateSpriteUV(sprite);

					lightning.Life = Random.Range(lifeMin, lifeMax);
					lightning.Age  = 0.0f;

					properties.SetTexture(_SGT_MainTex, sprite.texture);
					properties.SetColor(_SGT_Color, RandomColor);
					properties.SetFloat(_SGT_Brightness, brightness);
					properties.SetFloat(_SGT_Age, 0.0f);
					properties.SetVector(_SGT_Offset, new Vector2(uv.x, uv.y));
					properties.SetVector(_SGT_Scale, new Vector2(uv.z - uv.x, uv.w - uv.y));

					lightning.SetMesh(mesh);
					lightning.SetMaterial(sourceMaterial);

					lightning.transform.localRotation = Random.rotation;

					return lightning;
				}
			}

			return null;
		}

		public static SgtLightningSpawner Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtLightningSpawner Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Lightning Spawner", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtLightningSpawner>();
		}

		protected virtual void Awake()
		{
			ResetDelay();
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
		}

		protected virtual void Update()
		{
			cooldown -= Time.deltaTime;

			// Spawn new lightning?
			if (cooldown <= 0.0f)
			{
				ResetDelay();

				Spawn();
			}
		}

		protected virtual void OnDestroy()
		{
			if (mesh != null)
			{
				mesh.Clear(false);

				SgtCommon.MeshPool.Push(mesh);
			}
		}

		private void ResetDelay()
		{
			cooldown = Random.Range(delayMin, delayMax);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Lightning
{
	using UnityEditor;
	using TARGET = SgtLightningSpawner;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtLightningSpawner_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Lightning</b> shader. You cannot use a normal shader.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.DelayMin > t.DelayMax));
				Draw("delayMin", "The minimum delay between lightning spawns.");
				Draw("delayMax", "The maximum delay between lightning spawns.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.LifeMin > t.LifeMax));
				Draw("lifeMin", "The minimum life of each spawned lightning.");
				Draw("lifeMax", "The maximum life of each spawned lightning.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The radius of the spawned lightning mesh in local coordinates.");
			EndError();
			BeginError(Any(tgts, t => t.Size < 0.0f));
				Draw("size", ref dirtyMesh, "The size of the lightning in degrees.");
			EndError();
			BeginError(Any(tgts, t => t.Detail <= 0.0f));
				Draw("detail", ref dirtyMesh, "The amount of rows and columns in the lightning mesh.");
			EndError();
			Draw("colors", "When lightning is spawned, its base color will be randomly picked from this gradient.");
			Draw("brightness", "The Color.rgb values are multiplied by this, allowing you to quickly adjust the overall brightness.");
			BeginError(Any(tgts, t => t.Sprites == null || t.Sprites.Count == 0));
				Draw("sprites", "The random sprite used by the lightning.");
			EndError();

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Lightning Spawner", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtLightningSpawner.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif