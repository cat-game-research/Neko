using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Lightning
{
	/// <summary>This component handles rendering of lightning spawned from the SgtLightningSpawner component.</summary>
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class SgtLightning : MonoBehaviour
	{
		/// <summary>The lightning spawner this belongs to. If this is null then this GameObject will automatically be destroyed.</summary>
		public SgtLightningSpawner LightningSpawner { set { lightningSpawner = value; } get { return lightningSpawner; } } [SerializeField] private SgtLightningSpawner lightningSpawner;

		/// <summary>The maximum amount of seconds this lightning has been active for.</summary>
		public float Age { set { age = value; } get { return age; } } [SerializeField] private float age;

		/// <summary>The maximum amount of seconds this lightning can be active for.</summary>
		public float Life { set { life = value; } get { return life; } } [SerializeField] private float life;

		[System.NonSerialized]
		private MeshFilter cachedMeshFilter;

		[System.NonSerialized]
		private bool cachedMeshFilterSet;

		[System.NonSerialized]
		private MeshRenderer cachedMeshRenderer;

		[System.NonSerialized]
		private bool cachedMeshRendererSet;

		[System.NonSerialized]
		private MaterialPropertyBlock properties;

		private static int _SGT_Age = Shader.PropertyToID("_SGT_Age");

		public MaterialPropertyBlock Properties
		{
			get
			{
				if (properties == null)
				{
					properties = new MaterialPropertyBlock();
				}

				return properties;
			}
		}

		public void SetMesh(Mesh mesh)
		{
			if (cachedMeshFilterSet == false)
			{
				cachedMeshFilter    = gameObject.GetComponent<MeshFilter>();
				cachedMeshFilterSet = true;
			}

			cachedMeshFilter.sharedMesh = mesh;
		}

		public void SetMaterial(Material newMaterial)
		{
			if (cachedMeshRendererSet == false)
			{
				cachedMeshRenderer    = gameObject.GetComponent<MeshRenderer>();
				cachedMeshRendererSet = true;
			}

			cachedMeshRenderer.sharedMaterial = newMaterial;

			cachedMeshRenderer.SetPropertyBlock(properties);
		}

		public static SgtLightning Create(SgtLightningSpawner lightningSpawner)
		{
			var model = SgtComponentPool<SgtLightning>.Pop(lightningSpawner.transform, "Lightning", lightningSpawner.gameObject.layer);

			model.LightningSpawner = lightningSpawner;

			return model;
		}

		public static void Pool(SgtLightning model)
		{
			if (model != null)
			{
				model.LightningSpawner = null;

				SgtComponentPool<SgtLightning>.Add(model);
			}
		}

		public static void MarkForDestruction(SgtLightning model)
		{
			if (model != null)
			{
				model.LightningSpawner = null;

				model.gameObject.SetActive(true);
			}
		}

		protected virtual void Update()
		{
			if (LightningSpawner == null)
			{
				Pool(this);
			}
			else
			{
				if (Application.isPlaying == true)
				{
					age += Time.deltaTime;
				}

				if (age >= life)
				{
					SgtComponentPool<SgtLightning>.Add(this);
				}
				else if (properties != null)
				{
					properties.SetFloat(_SGT_Age, CwHelper.Divide(age, life));

					cachedMeshRenderer.SetPropertyBlock(properties);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Lightning
{
	using UnityEditor;
	using TARGET = SgtLightning;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtLightning_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("age", "The maximum amount of seconds this lightning has been active for.");
			BeginError(Any(tgts, t => t.Life < 0.0f));
				Draw("life", "The maximum amount of seconds this lightning can be active for.");
			EndError();

			Separator();

			BeginDisabled();
				Draw("lightningSpawner", "The lightning spawner this belongs to. If this is null then this GameObject will automatically be destroyed.");
			EndDisabled();
		}
	}
}
#endif