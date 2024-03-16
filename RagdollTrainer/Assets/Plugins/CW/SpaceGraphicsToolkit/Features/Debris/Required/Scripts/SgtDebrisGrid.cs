using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using SpaceGraphicsToolkit.Shapes;

namespace SpaceGraphicsToolkit.Debris
{
	/// <summary>This component allows you to spawn debris prefabs around a target point (e.g. camera), where each debris object must lie inside a grid square, allowing you to evenly distribute debris over the scene.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtDebrisGrid")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Debris Grid")]
	public class SgtDebrisGrid : MonoBehaviour
	{
		/// <summary>The transform the debris will spawn around.
		/// None/null = Main Camera.</summary>
		public Transform Target { set { target = value; } get { return target; } } [SerializeField] private Transform target;

		/// <summary>The shapes the debris will spawn inside.</summary>
		public SgtShapeGroup SpawnInside { set { spawnInside = value; } get { return spawnInside; } } [SerializeField] private SgtShapeGroup spawnInside;

		/// <summary>The distance from the target that debris begins spawning.</summary>
		public float ShowDistance { set { showDistance = value; } get { return showDistance; } } [SerializeField] private float showDistance = 90.0f;

		/// <summary>The distance from the target that debris gets hidden.</summary>
		public float HideDistance { set { hideDistance = value; } get { return hideDistance; } } [SerializeField] private float hideDistance = 100.0f;

		/// <summary>This allows you to set how many cells are in the grid on each axis within the Hide Distance.</summary>
		public int CellCount { set { cellCount = value; } get { return cellCount; } } [SerializeField] private int cellCount = 30;

		/// <summary>How far from the center of each cell the debris can be spawned. This should be decreased to stop debris intersecting.</summary>
		public float CellNoise { set { cellNoise = value; } get { return cellNoise; } } [SerializeField] [Range(0.0f, 0.5f)] private float cellNoise = 0.5f;

		/// <summary>The maximum expected amount of debris based on the cell size settings.</summary>
		public float DebrisCountTarget { set { debrisCountTarget = value; } get { return debrisCountTarget; } } [SerializeField] private float debrisCountTarget = 100;

		/// <summary>The minimum scale multiplier of the debris.</summary>
		public float ScaleMin { set { scaleMin = value; } get { return scaleMin; } } [SerializeField] private float scaleMin = 1.0f;

		/// <summary>The maximum scale multiplier of the debris.</summary>
		public float ScaleMax { set { scaleMax = value; } get { return scaleMax; } } [SerializeField] private float scaleMax = 2.0f;

		/// <summary>If this is above 0 then small debris are more likely to spawn. If this value is below 0 then big debris are more likely to spawn.</summary>
		public float ScaleBias { set { scaleBias = value; } get { return scaleBias; } } [SerializeField] private float scaleBias = 0.0f;

		/// <summary>Should the debris be given a random rotation, or inherit from the prefab that spawned it?</summary>
		public bool RandomRotation { set { randomRotation = value; } get { return randomRotation; } } [SerializeField] private bool randomRotation = true;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { seed = value; } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>These prefabs are randomly picked from when spawning new debris.</summary>
		public List<SgtDebris> Prefabs { get { if (prefabs == null) prefabs = new List<SgtDebris>(); return prefabs; } } [SerializeField] private List<SgtDebris> prefabs;

		[SerializeField]
		private List<SgtDebris> spawnedDebris;

		[SerializeField]
		private SgtLongBounds bounds;

		[System.NonSerialized]
		private static float minScale = 0.001f;

		// Used during find
		[System.NonSerialized]
		private static SgtDebris targetPrefab;

		public List<SgtDebris> SpawnedDebris
		{
			get
			{
				if (spawnedDebris == null)
				{
					spawnedDebris = new List<SgtDebris>();
				}

				return spawnedDebris;
			}
		}

		[ContextMenu("Clear Debris")]
		public void ClearDebris()
		{
			if (spawnedDebris != null)
			{
				for (var i = spawnedDebris.Count - 1; i >= 0; i--)
				{
					var debris = spawnedDebris[i];

					if (debris != null)
					{
						Despawn(debris);
					}
				}

				spawnedDebris.Clear();
			}

			bounds.Clear();
		}

		public void UpdateDebris()
		{
			var finalTarget = target;

			if (finalTarget == null)
			{
				var camera = CwHelper.GetCamera(null);

				if (camera != null)
				{
					finalTarget = camera.transform;
				}
			}

			if (finalTarget != null && cellCount > 0.0f && prefabs != null && debrisCountTarget > 0)
			{
				var cellSize   = (long)System.Math.Ceiling(hideDistance / cellCount);
				var worldPoint = finalTarget.position - transform.position;
				var centerX    = (long)System.Math.Round(worldPoint.x / cellSize);
				var centerY    = (long)System.Math.Round(worldPoint.y / cellSize);
				var centerZ    = (long)System.Math.Round(worldPoint.z / cellSize);
				var newBounds  = new SgtLongBounds(centerX, centerY, centerZ, cellCount);

				if (newBounds != bounds)
				{
					var probability = debrisCountTarget / (cellSize * cellSize * cellSize);
					var cellMin     = cellSize * (0.5f - cellNoise);
					var cellMax     = cellSize * (0.5f + cellNoise);

					for (var z = newBounds.minZ; z <= newBounds.maxZ; z++)
					{
						for (var y = newBounds.minY; y <= newBounds.maxY; y++)
						{
							for (var x = newBounds.minX; x <= newBounds.maxX; x++)
							{
								if (bounds.Contains(x, y, z) == false)
								{
									SgtCommon.BeginRandomSeed(seed, x, y, z);
									{
										// Can debris potentially spawn in this cell?
										if (Random.value < probability)
										{
											var debrisPoint = default(Vector3);

											debrisPoint.x = x * cellSize + Random.Range(cellMin, cellMax);
											debrisPoint.y = y * cellSize + Random.Range(cellMin, cellMax);
											debrisPoint.z = z * cellSize + Random.Range(cellMin, cellMax);

											// Spawn everywhere, or only inside specified shapes?
											if (spawnInside == null || Random.value < spawnInside.GetDensity(debrisPoint))
											{
												Spawn(x, y, z, debrisPoint);
											}
										}
									}
									CwHelper.EndSeed();
								}
							}
						}
					}

					bounds = newBounds;

					if (spawnedDebris != null)
					{
						for (var i = spawnedDebris.Count - 1; i >= 0; i--)
						{
							var debris = spawnedDebris[i];

							if (debris == null)
							{
								spawnedDebris.RemoveAt(i);
							}
							else if (bounds.Contains(debris.Cell) == false)
							{
								Despawn(debris, i);
							}
						}
					}
				}

				UpdateDebrisScale(finalTarget.position);
			}
			else
			{
				ClearDebris();
			}
		}

		public void UpdateDebrisScale(Vector3 worldPoint)
		{
			if (spawnedDebris != null)
			{
				var hideSqrDistance = hideDistance * hideDistance;
				var showSqrDistance = showDistance * showDistance;

				for (var i = spawnedDebris.Count - 1; i >= 0; i--)
				{
					var debris = spawnedDebris[i];

					if (debris != null)
					{
						var debrisTransform = debris.transform;
						var sqrDistance     = Vector3.SqrMagnitude(debrisTransform.position - worldPoint);

						if (sqrDistance >= hideSqrDistance)
						{
							if (debris.State != SgtDebris.StateType.Hide)
							{
								debris.State = SgtDebris.StateType.Hide;

								debrisTransform.localScale = debris.Scale * minScale;
							}
						}
						else if (sqrDistance <= showSqrDistance)
						{
							if (debris.State != SgtDebris.StateType.Show)
							{
								debris.State = SgtDebris.StateType.Show;

								debrisTransform.localScale = debris.Scale;
							}
						}
						else
						{
							debris.State = SgtDebris.StateType.Fade;

							debrisTransform.localScale = debris.Scale * Mathf.Max(Mathf.InverseLerp(hideDistance, showDistance, Mathf.Sqrt(sqrDistance)), minScale);
						}
					}
				}
			}
		}

		public static SgtDebrisGrid Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtDebrisGrid Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Debris Grid", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtDebrisGrid>();
		}

		protected virtual void Update()
		{
			UpdateDebris();
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			if (target != null)
			{
				var point = target.position;

				Gizmos.DrawWireSphere(point, showDistance);
				Gizmos.DrawWireSphere(point, hideDistance);
			}
		}
#endif

		private void Spawn(long x, long y, long z, Vector3 point)
		{
			var index  = Random.Range(0, prefabs.Count);
			var prefab = prefabs[index];

			if (prefab != null)
			{
				var debris = Spawn(prefab);

				debris.Cell = new SgtLong3(x, y, z);

				debris.transform.localPosition = point;

				if (randomRotation == true)
				{
					debris.transform.localRotation = Random.rotation;
				}
				else
				{
					debris.transform.localRotation = prefab.transform.rotation;
				}

				debris.State = SgtDebris.StateType.Fade;
				debris.Scale = prefab.transform.localScale * Mathf.Lerp(scaleMin, scaleMax, CwHelper.Sharpness(Random.value, scaleBias));

				debris.InvokeOnSpawn();

				spawnedDebris.Add(debris);
			}
		}

		private SgtDebris Spawn(SgtDebris prefab)
		{
			if (prefab.Pool == true)
			{
				targetPrefab = prefab;

				var debris = SgtComponentPool<SgtDebris>.Pop(DebrisMatch);

				if (debris != null)
				{
					debris.transform.SetParent(transform, false);

					return debris;
				}
			}

			return Instantiate(prefab, transform);
		}

		private void Despawn(SgtDebris debris)
		{
			debris.InvokeOnDespawn();

			if (debris.Pool == true)
			{
				SgtComponentPool<SgtDebris>.Add(debris);
			}
			else
			{
				CwHelper.Destroy(debris.gameObject);
			}
		}

		private void Despawn(SgtDebris debris, int index)
		{
			Despawn(debris);

			spawnedDebris.RemoveAt(index);
		}

		private bool DebrisMatch(SgtDebris debris)
		{
			return debris != null && debris.Prefab == targetPrefab;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Debris
{
	using UnityEditor;
	using TARGET = SgtDebrisGrid;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtDebrisGrid_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var clearUpdate = false;

			Draw("target", "The transform the debris will spawn around.\n\nNone/null = Main Camera.");
			Draw("spawnInside", ref clearUpdate, "The shapes the debris will spawn inside.");

			Separator();

			BeginError(Any(tgts, t => t.ShowDistance <= 0.0f || t.ShowDistance > t.HideDistance));
				Draw("showDistance", "The distance from the target that debris begins spawning.");
			EndError();
			BeginError(Any(tgts, t => t.HideDistance < 0.0f || t.ShowDistance > t.HideDistance));
				Draw("hideDistance", "The distance from the target that debris gets hidden.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.CellCount <= 0));
				Draw("cellCount", ref clearUpdate, "This allows you to set how many cells are in the grid on each axis within the Hide Distance.");
			EndError();
			Draw("cellNoise", ref clearUpdate, "How far from the center of each cell the debris can be spawned. This should be decreased to stop debris intersecting.");
			BeginError(Any(tgts, t => t.DebrisCountTarget <= 0));
				Draw("debrisCountTarget", ref clearUpdate, "The maximum expected amount of debris based on the cell size settings.");
			EndError();
			Draw("seed", ref clearUpdate, "This allows you to set the random seed used during procedural generation.");

			Separator();

			BeginError(Any(tgts, t => t.ScaleMin < 0.0f || t.ScaleMin > t.ScaleMax));
				Draw("scaleMin", "The minimum scale multiplier of the debris.");
				Draw("scaleMax", "The maximum scale multiplier of the debris.");
			EndError();
			Draw("scaleBias", "If this is above 0 then small debris are more likely to spawn. If this value is below 0 then big debris are more likely to spawn.");
			Draw("randomRotation", "Should the debris be given a random rotation, or inherit from the prefab that spawned it?");

			Separator();

			BeginError(Any(tgts, t => t.Prefabs == null || t.Prefabs.Count == 0 || t.Prefabs.Contains(null) == true));
				Draw("prefabs", ref clearUpdate, "These prefabs are randomly picked from when spawning new debris.");
			EndError();

			if (clearUpdate == true) Each(tgts, t => { t.ClearDebris(); t.UpdateDebris(); }, true);
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Debris Grid", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtDebrisGrid.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif