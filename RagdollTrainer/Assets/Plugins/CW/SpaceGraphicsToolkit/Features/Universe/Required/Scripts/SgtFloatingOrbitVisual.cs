using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component draws an orbit in 3D space.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingOrbitVisual")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Orbit Visual")]
	public class SgtFloatingOrbitVisual : MonoBehaviour
	{
		/// <summary>The orbit that will be rendered by this component.</summary>
		public SgtFloatingOrbit Orbit { set { orbit = value; } get { return orbit; } } [SerializeField] private SgtFloatingOrbit orbit;

		/// <summary>The material of the orbit.</summary>
		public Material Material { set { material = value; } get { return material; } } [SerializeField] private Material material;

		/// <summary>The thickness of the visual ring in local space.</summary>
		public SgtLength Thickness { set { thickness = value; } get { return thickness; } } [SerializeField] private SgtLength thickness = 100000.0f;

		/// <summary>The amount of points used to draw the orbit.</summary>
		public int Points { set { points = value; } get { return points; } } [SerializeField] private int points = 360;

		/// <summary>The color of the orbit ring as it goes around the orbit.</summary>
		public Gradient Colors { get { if (colors == null) colors = new Gradient(); return colors; } } [SerializeField] private Gradient colors;

		[System.NonSerialized]
		private Mesh visualMesh;

		[System.NonSerialized]
		private List<Vector3> meshPositions = new List<Vector3>(360 * 2);

		[System.NonSerialized]
		private List<Vector2> meshCoords = new List<Vector2>(360 * 2);

		[System.NonSerialized]
		private List<Color> meshColors = new List<Color>(360 * 2);

		[System.NonSerialized]
		private List<int> meshIndices = new List<int>(360 * 6);

		protected virtual void OnEnable()
		{
			SgtCamera.OnCameraDraw += HandleCameraDraw;
		}

		protected virtual void OnDisable()
		{
			SgtCamera.OnCameraDraw -= HandleCameraDraw;
		}

		private void HandleCameraDraw(Camera camera)
		{
			if (SgtCommon.CanDraw(gameObject, camera) == false) return;

			if (orbit != null)
			{
				var floatingCamera = default(SgtFloatingCamera);

				if (SgtFloatingCamera.TryGetInstance(ref floatingCamera) == true)
				{
					if (visualMesh == null)
					{
						visualMesh = SgtCommon.CreateTempMesh("Orbit Visual");
					}

					meshPositions.Clear();
					meshCoords.Clear();
					meshColors.Clear();
					meshIndices.Clear();

					var position = floatingCamera.CalculatePosition(orbit.ParentPoint.Position);
					var rotation = orbit.ParentPoint.transform.rotation * Quaternion.Euler(orbit.Tilt);
					var r1       = orbit.Radius;
					var r2       = orbit.Radius * (1.0f - orbit.Oblateness);
					var i1       = r1 - thickness * 0.5;
					var i2       = r2 - thickness * 0.5;
					var o1       = i1 + thickness;
					var o2       = i2 + thickness;
					var step     = 360.0 / points;
					var stepI    = 1.0f / (points - 1);

					for (var i = 0; i < points; i++)
					{
						var angle = (orbit.Angle - i * step) * Mathf.Deg2Rad;
						var sin   = System.Math.Sin(angle);
						var cos   = System.Math.Cos(angle);

						// Inner
						{
							var point   = position;
							var offsetX = orbit.Offset.x + sin * i1;
							var offsetY = orbit.Offset.y + 0.0;
							var offsetZ = orbit.Offset.z + cos * i2;

							SgtFloatingOrbit.Rotate(rotation, ref offsetX, ref offsetY, ref offsetZ);

							point.x += (float)offsetX;
							point.y += (float)offsetY;
							point.z += (float)offsetZ;

							point = transform.InverseTransformPoint(point);

							meshPositions.Add(point);
						}

						// Outer
						{
							var point   = position;
							var offsetX = orbit.Offset.x + sin * o1;
							var offsetY = orbit.Offset.y + 0.0;
							var offsetZ = orbit.Offset.z + cos * o2;

							SgtFloatingOrbit.Rotate(rotation, ref offsetX, ref offsetY, ref offsetZ);

							point.x += (float)offsetX;
							point.y += (float)offsetY;
							point.z += (float)offsetZ;

							point = transform.InverseTransformPoint(point);

							meshPositions.Add(point);
						}

						var u     = stepI * i;
						var color = colors.Evaluate(u);

						meshCoords.Add(new Vector2(u, 0.0f));
						meshCoords.Add(new Vector2(u, 1.0f));

						meshColors.Add(color);
						meshColors.Add(color);
					}

					for (var i = 0; i < points; i++)
					{
						var indexA = i * 2 + 0;
						var indexB = i * 2 + 1; 
						var indexC = i * 2 + 2; indexC %= points * 2;
						var indexD = i * 2 + 3; indexD %= points * 2;

						meshIndices.Add(indexA);
						meshIndices.Add(indexB);
						meshIndices.Add(indexC);
						meshIndices.Add(indexD);
						meshIndices.Add(indexC);
						meshIndices.Add(indexB);
					}

					visualMesh.SetVertices(meshPositions);
					visualMesh.SetTriangles(meshIndices, 0);
					visualMesh.SetUVs(0, meshCoords);
					visualMesh.SetColors(meshColors);
					visualMesh.RecalculateBounds();
				}

				if (visualMesh != null)
				{
					Graphics.DrawMesh(visualMesh, transform.localToWorldMatrix, material, gameObject.layer, camera);
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingOrbitVisual;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingOrbitVisual_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Orbit == null));
				Draw("orbit", "The orbit that will be rendered by this component.");
			EndError();
			BeginError(Any(tgts, t => t.Material == null));
				Draw("material", "The material of the orbit.");
			EndError();
			Draw("thickness", "The thickness of the visual ring in local space.");
			Draw("points", "The amount of points used to draw the orbit.");
			Draw("colors", "The color of the orbit ring as it goes around the orbit.");
		}
	}
}
#endif