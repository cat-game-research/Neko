using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.LightAndShadow
{
	/// <summary>This component allows you to add shadows cast from an SgtShadow___ component to any opaque renderer in your scene.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtShadowLayer")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Shadow Layer")]
	public class SgtShadowLayer : MonoBehaviour
	{
		/// <summary>The radius of this shadow receiver.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The renderers you want the shadows to be applied to.</summary>
		public List<MeshRenderer> Renderers { get { if (renderers == null) renderers = new List<MeshRenderer>(); return renderers; } } [SerializeField] private List<MeshRenderer> renderers;

		public Material Material
		{
			get
			{
				return material;
			}
		}

		[System.NonSerialized]
		private Material material;

		[ContextMenu("Apply Material")]
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

		[ContextMenu("Remove Material")]
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

		public void AddRenderer(MeshRenderer renderer)
		{
			if (renderer != null)
			{
				if (renderers == null)
				{
					renderers = new List<MeshRenderer>();
				}

				if (renderers.Contains(renderer) == false)
				{
					renderers.Add(renderer);

					CwHelper.AddMaterial(renderer, material);
				}
			}
		}

		public void RemoveRenderer(MeshRenderer renderer)
		{
			if (renderer != null && renderers != null)
			{
				if (renderers.Remove(renderer) == true)
				{
					CwHelper.RemoveMaterial(renderer, material);
				}
			}
		}

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraPreRender += CameraPreRender;

			if (material == null)
			{
				material = CwHelper.CreateTempMaterial("Shadow Layer (Generated)", "Hidden/SgtShadowLayer");
			}

			if (renderers == null)
			{
				AddRenderer(GetComponent<MeshRenderer>());
			}

			ApplyMaterial();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, CwHelper.UniformScale(transform.lossyScale) * radius);
		}
#endif

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraPreRender -= CameraPreRender;

			RemoveMaterial();
		}

		protected virtual void CameraPreRender(Camera camera)
		{
			if (material != null)
			{
				CwHelper.SetTempMaterial(material);

				var mask   = 1 << gameObject.layer;
				var lights = SgtLight.Find(mask, transform.position);

				SgtShadow.Find(true, mask, lights);
				SgtShadow.FilterOutSphere(transform.position);
				SgtShadow.FilterOutMiss(transform.position, CwHelper.UniformScale(transform.lossyScale) * radius);
				SgtShadow.WriteSphere(2);
				SgtShadow.WriteRing(1);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.LightAndShadow
{
	using UnityEditor;
	using TARGET = SgtShadowLayer;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtShadowLayer_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("radius", "The radius of this shadow receiver.");

			Separator();

			Each(tgts, t => { if (t.isActiveAndEnabled == true) t.RemoveMaterial(); });
				BeginError(Any(tgts, t => t.Renderers != null && t.Renderers.Exists(s => s == null)));
					Draw("renderers", "The renderers you want the shadows to be applied to.");
				EndError();
			Each(tgts, t => { if (t.isActiveAndEnabled == true) t.ApplyMaterial(); });
		}
	}
}
#endif