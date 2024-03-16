using UnityEngine;
using CW.Common;
using System.Collections.Generic;

namespace SpaceGraphicsToolkit.Spacetime
{
	/// <summary>This component allows you to deform SgtSpacetime grids.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtSpacetimeWell")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Spacetime Well")]
	public class SgtSpacetimeWell : MonoBehaviour
	{
		public enum DistributionType
		{
			Gaussian,
			Ripple,
			Twist,
			Pinch
		}

		/// <summary>The method used to deform the spacetime.</summary>
		public DistributionType Distribution { set { distribution = value; } get { return distribution; } } [SerializeField] private DistributionType distribution = DistributionType.Gaussian;

		/// <summary>The radius of this spacetime well.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 1.0f;

		/// <summary>The well will modify the spacetime positions by this amount.</summary>
		public float Strength { set { strength = value; } get { return strength; } } [SerializeField] private float strength = 1.0f;

		/// <summary>The overall effect of the well will be multiplied by this.</summary>
		public float Opacity { set { opacity = value; } get { return opacity; } } [SerializeField] [Range(0.0f, 1.0f)] private float opacity = 1.0f;

		/// <summary>Should the <b>Strength</b> get multiplied by the <b>Opacity</b>?</summary>
		public bool Combine { set { combine = value; } get { return combine; } } [SerializeField] private bool combine;

		/// <summary>The frequency of the ripple.</summary>
		public float Frequency { set { frequency = value; } get { return frequency; } } [SerializeField] private float frequency = 1.0f;

		/// <summary>The frequency offset.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] private float offset;

		/// <summary>The frequency offset speed per second.</summary>
		public float OffsetSpeed { set { offsetSpeed = value; } get { return offsetSpeed; } } [SerializeField] private float offsetSpeed;

		/// <summary>The size of the twist hole.</summary>
		public float HoleSize { set { holeSize = value; } get { return holeSize; } } [SerializeField] [Range(0.0f, 0.9f)] private float holeSize;

		/// <summary>The power of the twist hole.</summary>
		public float HolePower { set { holePower = value; } get { return holePower; } } [SerializeField] private float holePower = 10.0f;

		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtSpacetimeWell> Instances = new LinkedList<SgtSpacetimeWell>(); private LinkedListNode<SgtSpacetimeWell> node;

		/// <summary>This returns the <b>Strength</b> value depending on the <b>Combine</b> and <b>Opacity</b> settings.</summary>
		public float FinalStrength
		{
			get
			{
				if (combine == true)
				{
					return strength * opacity;
				}

				return strength;
			}
		}

		/// <summary>This allows you create a new GameObject with the <b>SgtSpacetimeWell</b> component attached.</summary>
		public static SgtSpacetimeWell Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		/// <summary>This allows you create a new GameObject with the <b>SgtSpacetimeWell</b> component attached.</summary>
		public static SgtSpacetimeWell Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Spacetime Well", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtSpacetimeWell>();
		}

		protected virtual void OnEnable()
		{
			node = Instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;
		}

		protected virtual void Update()
		{
#if UNITY_EDITOR
		if (Application.isPlaying == false)
		{
			return;
		}
#endif
			offset += offsetSpeed * Time.deltaTime;
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(transform.position, radius);
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Spacetime
{
	using UnityEditor;
	using TARGET = SgtSpacetimeWell;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSpacetimeWell_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("distribution", "The method used to deform the spacetime.");

			Separator();

			BeginError(Any(tgts, t => t.Radius < 0.0f));
				Draw("radius", "The radius of this spacetime well.");
			EndError();
			Draw("strength", "The well will modify the spacetime positions by this amount.");
			Draw("opacity", "The overall effect of the well will be multiplied by this.");
			Draw("combine", "Should the <b>Strength</b> get multiplied by the <b>Opacity</b>?");

			Separator();

			if (Any(tgts, t => t.Distribution == SgtSpacetimeWell.DistributionType.Ripple || t.Distribution == SgtSpacetimeWell.DistributionType.Twist))
			{
				Draw("frequency", "The frequency of the ripple.");
			}

			if (Any(tgts, t => t.Distribution == SgtSpacetimeWell.DistributionType.Twist))
			{
				BeginError(Any(tgts, t => t.HoleSize < 0.0f));
					Draw("holeSize", "The size of the twist hole.");
				EndError();
				Draw("holePower", "The power of the twist hole.");
			}

			Separator();

			if (Any(tgts, t => t.Distribution == SgtSpacetimeWell.DistributionType.Ripple || t.Distribution == SgtSpacetimeWell.DistributionType.Twist))
			{
				Draw("offset", "The frequency offset.");
				Draw("offsetSpeed", "The frequency offset speed per second.");
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Spacetime Well", false, 10)]
		public static void CreateItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtSpacetimeWell.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif