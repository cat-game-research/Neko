using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Starfield
{
	/// <summary>This component allows you to specify the exact position/size/etc of each star in this starfield.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtStarfieldCustom")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Starfield Custom")]
	public class SgtStarfieldCustom : SgtStarfield
	{
		/// <summary>The stars that will be rendered by this starfield.
		/// NOTE: If you modify this then you must then call the <b>DirtyMesh</b> method.</summary>
		public List<SgtStarfieldStar> Stars { get { if (stars == null) stars = new List<SgtStarfieldStar>(); return stars; } } [SerializeField] private List<SgtStarfieldStar> stars;

		public static SgtStarfieldCustom Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtStarfieldCustom Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Starfield Custom", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtStarfieldCustom>();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (stars != null)
			{
				for (var i = stars.Count - 1; i >= 0; i--)
				{
					SgtPoolClass<SgtStarfieldStar>.Add(stars[i]);
				}
			}
		}

		protected override int BeginQuads()
		{
			if (stars != null)
			{
				return stars.Count;
			}

			return 0;
		}

		protected override void NextQuad(ref SgtStarfieldStar quad, int starIndex)
		{
			quad.CopyFrom(stars[starIndex]);
		}

		protected override void EndQuads()
		{
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Starfield
{
	using UnityEditor;
	using TARGET = SgtStarfieldCustom;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtStarfieldCustom_Editor : SgtStarfield_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			DrawBasic(ref dirtyMesh);

			Separator();

			Draw("stars", ref dirtyMesh, "The stars that will be rendered by this starfield.");

			SgtCommon.RequireCamera();

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Starfield/Custom", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtStarfieldCustom.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif