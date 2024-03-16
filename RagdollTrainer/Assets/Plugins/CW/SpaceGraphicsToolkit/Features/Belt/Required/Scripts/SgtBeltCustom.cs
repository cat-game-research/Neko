using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Belt
{
	/// <summary>This component allows you to specify the exact position/size/etc of each asteroid in this asteroid belt.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtBeltCustom")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Belt Custom")]
	public class SgtBeltCustom : SgtBelt
	{
		/// <summary>This allows you to specify how much light the asteroids can block when using light flares.</summary>
		public float Occlusion { set { occlusion = value; } get { return occlusion; } } [SerializeField] private float occlusion;

		/// <summary>This allows you to specify the outer radius of the belt when calculating lighting.</summary>
		public float OuterRadius { set { outerRadius = value; } get { return outerRadius; } } [SerializeField] private float outerRadius;

		/// <summary>The custom asteroids in this belt.
		/// NOTE: If you modify this then you must then call the <b>DirtyMesh</b> method.</summary>
		public List<SgtBeltAsteroid> Asteroids { get { if (asteroids == null) asteroids = new List<SgtBeltAsteroid>(); return asteroids; } } [SerializeField] private List<SgtBeltAsteroid> asteroids;

		public static SgtBeltCustom Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtBeltCustom Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Belt Custom", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtBeltCustom>();
		}

		protected override float GetOuterRadius()
		{
			return outerRadius;
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			SgtCommon.OnCalculateOcclusion += HandleCalculateOcclusion;
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			SgtCommon.OnCalculateOcclusion -= HandleCalculateOcclusion;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (asteroids != null)
			{
				for (var i = asteroids.Count - 1; i >= 0; i--)
				{
					SgtPoolClass<SgtBeltAsteroid>.Add(asteroids[i]);
				}
			}
		}

		protected override int BeginQuads()
		{
			if (asteroids != null)
			{
				return asteroids.Count;
			}

			return 0;
		}

		protected override void NextQuad(ref SgtBeltAsteroid asteroid, int asteroidIndex)
		{
			asteroid.CopyFrom(asteroids[asteroidIndex]);
		}

		protected override void EndQuads()
		{
		}

		private void HandleCalculateOcclusion(int layers, Vector4 worldEye, Vector4 worldTgt, ref float flareOcclusion)
		{
			if (occlusion > 0.0f && SgtOcclusion.IsValid(occlusion, layers, gameObject) == true)
			{
				var localEye = transform.InverseTransformPoint(worldEye);
				var localTgt = transform.InverseTransformPoint(worldTgt);

				if (asteroids != null)
				{
					for (var i = 0; i < asteroids.Count; i++)
					{
						var asteroid = asteroids[i];

						if (asteroid.Radius > 0.0f)
						{
							var position = CalculateLocalPosition(ref asteroid, OrbitOffset);
							var distance = GetDistance(localEye, localTgt, position);

							if (distance < asteroid.Radius)
							{
								//var blocking = 1.0f - CwHelper.Sharpness(1.0f - distance / asteroid.Radius, occlusion);

								//flareOcclusion += blocking * (1.0f - flareOcclusion);
								flareOcclusion += (asteroid.Radius - distance) * occlusion;
							}

							if (flareOcclusion > 0.99f)
							{
								flareOcclusion = 1.0f;

								break;
							}
						}
					}
				}
			}
		}

		private static float GetDistance(Vector3 a, Vector3 b, Vector3 p)
		{
			var vecA = b - a;
			var vecB = p - b;
			var vecD = Vector3.Dot(vecA, vecB);
			var best = b;

			if (vecD < 0.0f)
			{
				vecB = p - a;
				vecD  = Vector3.Dot(vecA, vecB);

				if (vecD <= 0.0f)
				{
					best = a;
				}
				else
				{
					var sqrLength = Vector3.Dot(vecA, vecA);

					if (sqrLength > 0.0f)
					{
						vecD /= sqrLength;
						best  = a + vecD * vecA;
					}
					else
					{
						best = a;
					}
				}
			}

			vecB = p - best;

			return (float)System.Math.Sqrt(Vector3.Dot(vecB, vecB));
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Belt
{
	using UnityEditor;
	using TARGET = SgtBeltCustom;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtBeltCustom_Editor : SgtBelt_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			DrawBasic(ref dirtyMesh);

			Separator();

			Draw("occlusion", "This allows you to specify how much light the asteroids can block when using light flares.");
			Draw("outerRadius", "This allows you to specify the outer radius of the belt when calculating lighting.");

			Separator();

			Draw("asteroids", ref dirtyMesh, "The custom asteroids in this belt.");

			SgtCommon.RequireCamera();

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Belt Custom", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtBeltCustom.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif