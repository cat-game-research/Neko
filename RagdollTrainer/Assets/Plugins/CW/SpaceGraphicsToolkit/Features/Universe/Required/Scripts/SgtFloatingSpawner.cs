using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all Universe spawners, providing a useful methods for spawning and handling prefabs.</summary>
	[RequireComponent(typeof(SgtFloatingObject))]
	public abstract class SgtFloatingSpawner : MonoBehaviour
	{
		/// <summary>The camera must be within this range for this spawner to activate.</summary>
		public SgtLength Range { set { range = value; } get { return range; } } [SerializeField] private SgtLength range = new SgtLength(1.0, SgtLength.ScaleType.AU);

		/// <summary>If you want to define prefabs externally, then you can use the SgtSpawnList component with a matching Category name.</summary>
		public string Category { set { category = value; } get { return category; } } [SerializeField] private string category;

		/// <summary>If you aren't using a spawn list category, or just want to augment the spawn list, then define the prefabs you want to spawn here.</summary>
		public List<SgtFloatingObject> Prefabs { get { if (prefabs == null) prefabs = new List<SgtFloatingObject>(); return prefabs; } } [SerializeField] private List<SgtFloatingObject> prefabs;

		/// <summary>If you disable this then the spawned object will use the same prefab as the spawn index.</summary>
		public bool RandomizeIndex { set { randomizeIndex = value; } get { return randomizeIndex; } } [SerializeField] private bool randomizeIndex = true;

		private static List<SgtFloatingObject> tempPrefabs = new List<SgtFloatingObject>();

		[SerializeField]
		private List<SgtFloatingObject> instances;

		[SerializeField]
		private bool inside;

		[System.NonSerialized]
		private SgtFloatingObject cachedObject;

		[System.NonSerialized]
		private bool cachedObjectSet;

		/// <summary>The <b>SgtFloatingObject</b> component alongside this component.</summary>
		public SgtFloatingObject CachedObject
		{
			get
			{
				if (cachedObjectSet == false)
				{
					cachedObject    = GetComponent<SgtFloatingObject>();
					cachedObjectSet = true;
				}

				return cachedObject;
			}
		}

		protected virtual void OnEnable()
		{
			CachedObject.OnDistance += HandleDistance;
		}

		protected virtual void OnDisable()
		{
			cachedObject.OnDistance -= HandleDistance;

			DespawnAll();
		}

		protected bool BuildSpawnList()
		{
			if (instances == null)
			{
				instances = new List<SgtFloatingObject>();
			}

			tempPrefabs.Clear();

			if (string.IsNullOrEmpty(category) == false)
			{
				foreach (var spawnList in SgtSpawnList.Instances)
				{
					if (spawnList.Category == category)
					{
						BuildSpawnList(spawnList.Prefabs);
					}
				}
			}

			BuildSpawnList(prefabs);

			return tempPrefabs.Count > 0;
		}

		protected SgtFloatingObject SpawnAt(SgtPosition position, int index)
		{
			if (tempPrefabs.Count > 0)
			{
				if (randomizeIndex == true)
				{
					index = Random.Range(0, tempPrefabs.Count);
				}
				else
				{
					index %= tempPrefabs.Count;
				}

				var prefab = tempPrefabs[index];

				if (prefab != null)
				{
					var oldSeed        = prefab.Seed;
					var oldPosition    = prefab.Position;
					var oldPositionSet = prefab.PositionSet;

					prefab.Seed        = Random.Range(int.MinValue, int.MaxValue);
					prefab.Position    = position;
					prefab.PositionSet = true;

					var instance = Instantiate(prefab, SgtFloatingRoot.Root);

					prefab.Seed        = oldSeed;
					prefab.Position    = oldPosition;
					prefab.PositionSet = oldPositionSet;

					instances.Add(instance);

					instance.InvokeOnSpawn();

					return instance;
				}
			}

			return null;
		}

		protected abstract void SpawnAll();

		private void HandleDistance(double distance)
		{
			var floatingCamera = SgtFloatingCamera.Instances.First.Value;
			var sqrDistance    = SgtPosition.SqrDistance(CachedObject.Position, floatingCamera.Position);
			var newInside      = distance <= (double)range;

			if (inside != newInside)
			{
				inside = newInside;

				if (inside == true)
				{
					SpawnAll();
				}
				else
				{
					DespawnAll();
				}
			}
		}

		private void DespawnAll()
		{
			if (instances != null)
			{
				for (var i = instances.Count - 1; i >= 0; i--)
				{
					var instance = instances[i];

					if (instance != null)
					{
						CwHelper.Destroy(instance.gameObject);
					}
				}

				instances.Clear();
			}
		}

		private static void BuildSpawnList(List<SgtFloatingObject> floatingObjects)
		{
			if (floatingObjects != null)
			{
				for (var i = floatingObjects.Count - 1; i >= 0; i--)
				{
					var floatingObject = floatingObjects[i];

					if (floatingObject != null)
					{
						tempPrefabs.Add(floatingObject);
					}
				}
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using TARGET = SgtFloatingSpawner;

	public abstract class SgtFloatingSpawner_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			if (SgtFloatingRoot.Instances.Count == 0)
			{
				if (HelpButton("Your scene contains no SgtFloatingRoot component, so all spawned SgtFloatingSpawnable prefabs will be placed in the scene root.", UnityEditor.MessageType.Warning, "Add", 35.0f) == true)
				{
					new GameObject("Floating Root").AddComponent<SgtFloatingRoot>();
				}

				Separator();
			}

			var missing = true;

			if (Any(tgts, t => string.IsNullOrEmpty(t.Category) == false))
			{
				missing = false;
			}

			if (Any(tgts, t => t.Prefabs != null && t.Prefabs.Count > 0))
			{
				missing = false;
			}

			Draw("range", "The camera must be within this range for this spawner to activate.");
			BeginError(missing);
				Draw("category", "If you want to define prefabs externally, then you can use the SgtSpawnList component with a matching Category name.");
			EndError();
			Draw("randomizeIndex", "If you disable this then the spawned object will use the same prefab as the spawn index.");
			BeginError(missing);
				Draw("prefabs", "If you aren't using a spawn list category, or just want to augment the spawn list, then define the prefabs you want to spawn here.");
			EndError();
		}
	}
}
#endif