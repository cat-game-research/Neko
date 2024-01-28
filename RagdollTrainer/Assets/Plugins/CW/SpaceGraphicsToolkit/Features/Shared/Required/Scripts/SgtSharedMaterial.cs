using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component stores a Material and a list of Renderers, and maintains that all renderers use the material as long as they are all part of this component.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtSharedMaterial")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shared Material")]
	public class SgtSharedMaterial : MonoBehaviour
	{
		public delegate void OverrideSharedMaterialSignature(ref SgtSharedMaterial sharedMaterial, Camera camera);

		/// <summary>The material that will be applied to all renderers.</summary>
		public Material Material
		{
			set
			{
				if (material != value)
				{
					if (material != null)
					{
						RemoveMaterial();
					}

					material = value;

					if (material != null)
					{
						ApplyMaterial();
					}
				}
			}

			get
			{
				return material;
			}
		}

		public int RendererCount
		{
			get
			{
				if (renderers != null)
				{
					return renderers.Count;
				}

				return 0;
			}
		}

		[SerializeField]
		private Material material;

		[SerializeField]
		private List<Renderer> renderers;

		public void ApplyMaterial()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					CwHelper.AddMaterial(renderers[i], material);
				}
			}
		}

		public void RemoveMaterial()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					CwHelper.RemoveMaterial(renderers[i], material);
				}
			}
		}

		public void AddRenderer(Renderer renderer)
		{
			if (renderer != null)
			{
				if (renderers == null)
				{
					renderers = new List<Renderer>();
				}

				if (renderers.Contains(renderer) == false)
				{
					renderers.Add(renderer);

					if (material != null)
					{
						CwHelper.AddMaterial(renderer, material);
					}
				}
			}
		}

		public void RemoveRenderer(Renderer renderer)
		{
			if (renderers != null && renderer != null)
			{
				if (renderers.Remove(renderer) == true)
				{
					CwHelper.RemoveMaterial(renderer, material);
				}
			}
		}

		[ContextMenu("Remove Null Renderers")]
		public void RemoveNullRenderers()
		{
			if (renderers != null)
			{
				for (var i = renderers.Count - 1; i >= 0; i--)
				{
					if (renderers[i] == null)
					{
						renderers.RemoveAt(i);
					}
				}
			}
		}

		protected virtual void OnEnable()
		{
			ApplyMaterial();
		}

		protected virtual void OnDisable()
		{
			RemoveMaterial();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtSharedMaterial;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSharedMaterial_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginDisabled();
				Draw("material", "The material that will be applied to all renderers.");
			EndDisabled();

			Each(tgts, t => t.RemoveMaterial());
			Draw("renderers", "The renderers the Material will be applied to.");
			Each(tgts, t => { if (CwHelper.Enabled(t) == true) { t.ApplyMaterial(); } });
		}
	}
}
#endif