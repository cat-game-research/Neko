using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit.Spacetime
{
	/// <summary>This component allows you to render a grid that can be deformed by SgtSpacetimeWell components.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtSpacetime")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Spacetime")]
	public class SgtSpacetime : MonoBehaviour
	{
		/// <summary>The <b>Material</b> used to render all the spacetime meshes. This should use the <b>Space Graphics Toolkit/Spacetime</b> material.</summary>
		public Material Material { set { if (material != value) { material = value; DirtyMaterial(); } } get { return material; } } [SerializeField] private Material material;

		/// <summary>The spacetime data collected by this component will be rendered to these Renderers.
		/// NOTE: If you modify this list you should call the <b>DirtyRenderers</b> or <b>ApplyMaterialToRenderers</b> method.</summary>
		public List<Renderer> Renderers { get { if (renderers == null) renderers = new List<Renderer>(); return renderers; } } [SerializeField] private List<Renderer> renderers = null;

		/// <summary>The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Gaussian</b> distribution that can be rendered by this spacetime.</summary>
		public int MaxGaussianWells { set { maxGaussianWells = value; } get { return maxGaussianWells; } } [SerializeField] [Range(0, MAX_GAUSSIAN_WELLS)] private int maxGaussianWells = 12;

		/// <summary>The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Ripple</b> distribution that can be rendered by this spacetime.</summary>
		public int MaxRippleWells { set { maxRippleWells = value; } get { return maxRippleWells; } } [SerializeField] [Range(0, MAX_RIPPLE_WELLS)] private int maxRippleWells = 1;

		/// <summary>The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Twist</b> distribution that can be rendered by this spacetime.</summary>
		public int MaxTwistWells { set { maxTwistWells = value; } get { return maxTwistWells; } } [SerializeField] [Range(0, MAX_TWIST_WELLS)] private int maxTwistWells = 1;

		/// <summary>The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Pinch</b> distribution that can be rendered by this spacetime.</summary>
		public int MaxPinchWells { set { maxPinchWells = value; } get { return maxPinchWells; } } [SerializeField] [Range(0, MAX_GAUSSIAN_WELLS)] private int maxPinchWells = 12;

		/// <summary>Filter all the wells to require the same layer at this GameObject.</summary>
		public bool RequireSameLayer { set { if (requireSameLayer != value) { requireSameLayer = value; DirtyMaterial(); } } get { return requireSameLayer; } } [SerializeField] private bool requireSameLayer;

		/// <summary>Filter all the wells to require the same tag at this GameObject.</summary>
		public bool RequireSameTag { set { if (requireSameTag != value) { requireSameTag = value; DirtyMaterial(); } } get { return requireSameTag; } } [SerializeField] private bool requireSameTag;

		/// <summary>Filter all the wells to require a name that contains this.</summary>
		public string RequireNameContains { set { if (requireNameContains != value) { requireNameContains = value; DirtyMaterial(); } } get { return requireNameContains; } } [SerializeField] private string requireNameContains;

		private bool dirtyApply = true;

		private const int MAX_GAUSSIAN_WELLS = 16;

		private const int MAX_RIPPLE_WELLS = 16;

		private const int MAX_TWIST_WELLS = 16;

		private const int MAX_PINCH_WELLS = 16;

		// The well data arrays that get copied to the shader
		[System.NonSerialized] private Vector4  [] gauPos = new Vector4[MAX_GAUSSIAN_WELLS];
		[System.NonSerialized] private Vector4  [] gauStr = new Vector4[MAX_GAUSSIAN_WELLS];

		[System.NonSerialized] private Vector4  [] ripPos = new Vector4[MAX_RIPPLE_WELLS];
		[System.NonSerialized] private Vector4  [] ripStr = new Vector4[MAX_RIPPLE_WELLS];
		[System.NonSerialized] private Vector4  [] ripDat = new Vector4[MAX_RIPPLE_WELLS];

		[System.NonSerialized] private Vector4  [] twiPos = new Vector4[MAX_TWIST_WELLS];
		[System.NonSerialized] private Vector4  [] twiStr = new Vector4[MAX_TWIST_WELLS];
		[System.NonSerialized] private Vector4  [] twiDat = new Vector4[MAX_TWIST_WELLS];
		[System.NonSerialized] private Matrix4x4[] twiMat = new Matrix4x4[MAX_TWIST_WELLS];

		[System.NonSerialized] private Vector4  [] pinPos = new Vector4[MAX_PINCH_WELLS];
		[System.NonSerialized] private Vector4  [] pinStr = new Vector4[MAX_PINCH_WELLS];

		[System.NonSerialized]
		private List<SgtSpacetimeWell> wells = new List<SgtSpacetimeWell>();

		private static int _Gau    = Shader.PropertyToID("_Gau");
		private static int _GauPos = Shader.PropertyToID("_GauPos");
		private static int _GauStr = Shader.PropertyToID("_GauStr");
		private static int _Rip    = Shader.PropertyToID("_Rip");
		private static int _RipPos = Shader.PropertyToID("_RipPos");
		private static int _RipStr = Shader.PropertyToID("_RipStr");
		private static int _RipDat = Shader.PropertyToID("_RipDat");
		private static int _Twi    = Shader.PropertyToID("_Twi");
		private static int _TwiPos = Shader.PropertyToID("_TwiPos");
		private static int _TwiStr = Shader.PropertyToID("_TwiStr");
		private static int _TwiDat = Shader.PropertyToID("_TwiDat");
		private static int _TwiMat = Shader.PropertyToID("_TwiMat");
		private static int _Pin    = Shader.PropertyToID("_Pin");
		private static int _PinPos = Shader.PropertyToID("_PinPos");
		private static int _PinStr = Shader.PropertyToID("_PinStr");

		/// <summary>This tells you which wells this spacetime is currently rendering based on the current <b>Require</b> settings.</summary>
		public List<SgtSpacetimeWell> Wells
		{
			get
			{
				return wells;
			}
		}

		[ContextMenu("Update Wells")]
		public void UpdateWells()
		{
			if (material != null)
			{
				var gaussianCount = 0;
				var rippleCount   = 0;
				var twistCount    = 0;
				var pinchCount    = 0;

				WriteWells(ref gaussianCount, ref rippleCount, ref twistCount, ref pinchCount);
			}
		}

		/// <summary>This will immediately apply the <b>Material</b> to all <b>Renderers</b> if you've changed any.</summary>
		[ContextMenu("Apply Material To Renderers")]
		public void ApplyMaterialToRenderers()
		{
			dirtyApply = false;

			if (renderers != null)
			{
				foreach (var renderer in renderers)
				{
					if (renderer != null)
					{
						renderer.sharedMaterial = material;
					}
				}
			}
		}

		/// <summary>If you manually modified the <b>Material</b>, then you can call this method so they will be updated at the end of the frame.
		/// NOTE: This will automatically be called when modifying the <b>Material</b> property.</summary>
		[ContextMenu("Dirty Material")]
		public void DirtyMaterial()
		{
			dirtyApply = true;
		}

		/// <summary>If you manually modified the <b>Renderers</b> list, then you can call this method so they will be updated at the end of the frame.</summary>
		[ContextMenu("Dirty Renderers")]
		public void DirtyRenderers()
		{
			dirtyApply = true;
		}

		/// <summary>This allows you create a new GameObject with the <b>SgtSpacetime</b> component attached.</summary>
		public static SgtSpacetime Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		/// <summary>This allows you create a new GameObject with the <b>SgtSpacetime</b> component attached.</summary>
		public static SgtSpacetime Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Spacetime", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtSpacetime>();
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (material == null)
			{
				var guids = UnityEditor.AssetDatabase.FindAssets("t:Material Spacetime (Default)");

				foreach (var guid in guids)
				{
					var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

					if (path.Contains("Space Graphics Toolkit/Features/Spacetime/Media/Spacetime (Default)") == true)
					{
						material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);

						break;
					}
				}
			}
		}
#endif

		protected virtual void LateUpdate()
		{
			if (dirtyApply == true)
			{
				ApplyMaterialToRenderers();
			}

			UpdateWells();
		}

		// We don't know what was modified, so update everything
		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMaterial();
			DirtyRenderers();
		}

		private void WriteWells(ref int gaussianCount, ref int rippleCount, ref int twistCount, ref int pinchCount)
		{
			wells.Clear();

			foreach (var well in SgtSpacetimeWell.Instances)
			{
				if (CwHelper.Enabled(well) == true && well.Radius > 0.0f)
				{
					if (well.Distribution == SgtSpacetimeWell.DistributionType.Gaussian && gaussianCount >= maxGaussianWells)
					{
						continue;
					}

					if (well.Distribution == SgtSpacetimeWell.DistributionType.Ripple && rippleCount >= maxRippleWells)
					{
						continue;
					}

					if (well.Distribution == SgtSpacetimeWell.DistributionType.Twist && twistCount >= maxTwistWells)
					{
						continue;
					}

					if (well.Distribution == SgtSpacetimeWell.DistributionType.Pinch && pinchCount >= maxPinchWells)
					{
						continue;
					}

					// filter?
					if (requireSameLayer == true && gameObject.layer != well.gameObject.layer)
					{
						continue;
					}

					if (requireSameTag == true && tag != well.tag)
					{
						continue;
					}

					if (string.IsNullOrEmpty(requireNameContains) == false && well.name.Contains(requireNameContains) == false)
					{
						continue;
					}

					var wellPos = well.transform.position;

					switch (well.Distribution)
					{
						case SgtSpacetimeWell.DistributionType.Gaussian:
						{
							var index = gaussianCount++;

							gauPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							gauStr[index] = new Vector4(well.FinalStrength, well.Opacity, 0.0f, 0.0f);

							wells.Add(well);
						}
						break;

						case SgtSpacetimeWell.DistributionType.Ripple:
						{
							var index = rippleCount++;

							ripPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							ripStr[index] = new Vector4(well.FinalStrength, well.Opacity, 0.0f, 0.0f);
							ripDat[index] = new Vector4(well.Frequency, well.Offset, 0.0f, 0.0f);

							wells.Add(well);
						}
						break;

						case SgtSpacetimeWell.DistributionType.Twist:
						{
							var index = twistCount++;

							twiPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							twiStr[index] = new Vector4(well.FinalStrength, well.Opacity, 0.0f, 0.0f);
							twiDat[index] = new Vector4(well.Frequency, well.HoleSize, well.HolePower, 0.0f);
							twiMat[index] = Matrix4x4.Rotate(Quaternion.Euler(0.0f, well.Offset, 0.0f)) * well.transform.worldToLocalMatrix;

							wells.Add(well);
						}
						break;

						case SgtSpacetimeWell.DistributionType.Pinch:
						{
							var index = pinchCount++;

							pinPos[index] = new Vector4(wellPos.x, wellPos.y, wellPos.z, well.Radius);
							pinStr[index] = new Vector4(well.FinalStrength, well.Opacity, 0.0f, 0.0f);

							wells.Add(well);
						}
						break;
					}
				}
			}

			material.SetInt(_Gau, gaussianCount);
			material.SetVectorArray(_GauPos, gauPos);
			material.SetVectorArray(_GauStr, gauStr);

			material.SetInt(_Rip, rippleCount);
			material.SetVectorArray(_RipPos, ripPos);
			material.SetVectorArray(_RipStr, ripStr);
			material.SetVectorArray(_RipDat, ripDat);

			material.SetInt(_Twi, twistCount);
			material.SetVectorArray(_TwiPos, twiPos);
			material.SetVectorArray(_TwiStr, twiStr);
			material.SetVectorArray(_TwiDat, twiDat);
			material.SetMatrixArray(_TwiMat, twiMat);

			material.SetInt(_Pin, pinchCount);
			material.SetVectorArray(_PinPos, pinPos);
			material.SetVectorArray(_PinStr, pinStr);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Spacetime
{
	using UnityEditor;
	using TARGET = SgtSpacetime;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSpacetime_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMaterial  = false;
			var dirtyRenderers = false;
			var renderersExist = Any(tgts, t => t.Renderers.Exists(r => r != null));

			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", ref dirtyMaterial, "The <b>Material</b> used to render all the spacetime meshes. This should use the <b>Space Graphics Toolkit/Spacetime</b> material.");
			EndError();
			BeginError(renderersExist == false);
				Draw("renderers", ref dirtyRenderers, "The spacetime data collected by this component will be rendered to these Renderers.\n\nNOTE: If you modify this list you should call the <b>DirtyRenderers</b> or <b>ApplyMaterialToRenderers</b> method.");
			EndError();

			if (renderersExist == false)
			{
				Separator();

				if (HelpButton("This Spacetime has no Renderers set.", UnityEditor.MessageType.Warning, "Add", 40) == true)
				{
					Each(tgts, t => { var child = SgtSpacetimeMesh.Create(t.gameObject.layer, t.transform); CwHelper.SelectAndPing(child); });
				}
			}

			Separator();

			Draw("maxGaussianWells", "The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Gaussian</b> distribution that can be rendered by this spacetime.");
			Draw("maxRippleWells", "The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Ripple</b> distribution that can be rendered by this spacetime.");
			Draw("maxTwistWells", "The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Twist</b> distribution that can be rendered by this spacetime.");
			Draw("maxPinchWells", "The maximum amount of <b>SgtSpacetimeWell</b> components with the <b>Pinch</b> distribution that can be rendered by this spacetime.");

			Separator();

			Draw("requireSameLayer", "Filter all the wells to require the same layer at this GameObject.");
			Draw("requireSameTag", "Filter all the wells to require the same tag at this GameObject.");
			Draw("requireNameContains", "Filter all the wells to require a name that contains this.");

			Separator();

			EditorGUILayout.LabelField("Wells", EditorStyles.boldLabel);

			if (tgt.Wells.Count == 0)
			{
				Warning("Either your scene contains no active and enabled SgtSpacetimeWell components, or based on the above Require___ settings, none were found.");
			}

			BeginDisabled();
				foreach (var well in tgt.Wells)
				{
					var title = "";

					if (well != null)
					{
						title = well.Distribution.ToString();
					}

					EditorGUILayout.ObjectField(title, well, typeof(Object), true);
				}
			EndDisabled();

			if (dirtyMaterial == true)
			{
				Each(tgts, t => t.DirtyMaterial(), true, true);
			}

			if (dirtyRenderers == true)
			{
				Each(tgts, t => t.DirtyRenderers(), true, true);
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Spacetime", false, 10)]
		public static void CreateItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtSpacetime.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif