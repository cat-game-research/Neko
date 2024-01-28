using UnityEngine;
using System.Collections.Generic;
using CW.Common;
using SpaceGraphicsToolkit.Shapes;

namespace SpaceGraphicsToolkit.Debris
{
	/// <summary>This component allows you to randomly spawn debris around the camera over time.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtDebrisSpawner")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Debris Spawner")]
	public class SgtDebrisSpawner : MonoBehaviour
	{
		/// <summary>If this transform is inside the radius then debris will begin spawning.</summary>
		public Transform Target { set { target = value; } get { return target; } } [SerializeField] private Transform target;

		/// <summary>The shapes the debris will spawn inside.</summary>
		public SgtShapeGroup SpawnInside { set { spawnInside = value; } get { return spawnInside; } } [SerializeField] private SgtShapeGroup spawnInside;

		/// <summary>How quickly the debris shows after it spawns.</summary>
		public float ShowSpeed { set { showSpeed = value; } get { return showSpeed; } } [SerializeField] private float showSpeed = 10.0f;

		/// <summary>The distance from the follower that debris begins spawning.</summary>
		public float ShowDistance { set { showDistance = value; } get { return showDistance; } } [SerializeField] private float showDistance = 0.9f;

		/// <summary>The distance from the follower that debris gets hidden.</summary>
		public float HideDistance { set { hideDistance = value; } get { return hideDistance; } } [SerializeField] private float hideDistance = 1.0f;

		/// <summary>Should all the debris be automatically spawned at the start?</summary>
		public bool SpawnOnAwake { set { spawnOnAwake = value; } get { return spawnOnAwake; } } [SerializeField] private bool spawnOnAwake;

		/// <summary>The maximum amount of debris that can be spawned.</summary>
		public int SpawnLimit { set { spawnLimit = value; } get { return spawnLimit; } } [SerializeField] private int spawnLimit = 50;

		/// <summary>The minimum amount of seconds between debris spawns.</summary>
		public float SpawnRateMin { set { spawnRateMin = value; } get { return spawnRateMin; } } [SerializeField] private float spawnRateMin = 0.5f;

		/// <summary>The maximum amount of seconds between debris spawns.</summary>
		public float SpawnRateMax { set { spawnRateMax = value; } get { return spawnRateMax; } } [SerializeField] private float spawnRateMax = 1.0f;

		/// <summary>The minimum scale multiplier applied to spawned debris.</summary>
		public float SpawnScaleMin { set { spawnScaleMin = value; } get { return spawnScaleMin; } } [SerializeField] private float spawnScaleMin = 1.0f;

		/// <summary>The maximum scale multiplier applied to spawned debris.</summary>
		public float SpawnScaleMax { set { spawnScaleMax = value; } get { return spawnScaleMax; } } [SerializeField] private float spawnScaleMax = 1.0f;

		/// <summary>These prefabs are randomly picked from when spawning new debris.</summary>
		public List<SgtDebris> Prefabs { get { if (prefabs == null) prefabs = new List<SgtDebris>(); return prefabs; } } [SerializeField] private List<SgtDebris> prefabs;

		// The currently spawned debris
		public List<SgtDebris> Debris { get { if (debris == null) debris = new List<SgtDebris>(); return debris; } } [SerializeField] private List<SgtDebris> debris;

		// Seconds until a new debris can be spawned
		private float spawnCooldown;

		private Vector3 followerPosition;

		private Vector3 followerVelocity;

		private float minScale = 0.001f;

		// Used during find
		private SgtDebris targetPrefab;

		[ContextMenu("Clear Debris")]
		public void ClearDebris()
		{
			if (debris != null)
			{
				for (var i = debris.Count - 1; i >= 0; i--)
				{
					var debris = this.debris[i];

					if (debris != null)
					{
						Despawn(debris, i);
					}
					else
					{
						this.debris.RemoveAt(i);
					}
				}
			}
		}

		[ContextMenu("Spawn Debris Inside")]
		public void SpawnDebrisInside()
		{
			SpawnDebris(true);
		}

		// Spawns 1 debris regardless of the spawn limit, if inside is false then the debris will be spawned along the HideDistance
		public void SpawnDebris(bool inside)
		{
			if (prefabs != null && prefabs.Count > 0 && target != null)
			{
				var index  = Random.Range(0, prefabs.Count - 1);
				var prefab = prefabs[index];

				if (prefab != null)
				{
					var debris   = Spawn(prefab);
					var vector   = Random.insideUnitSphere * hideDistance + followerVelocity;
					var distance = hideDistance;

					if (inside == true)
					{
						distance = Random.Range(0.0f, hideDistance);
					}
					else
					{
						distance = Random.Range(showDistance, hideDistance);
					}

					if (vector.sqrMagnitude <= 0.0f)
					{
						vector = Random.onUnitSphere;
					}

					debris.Show   = 0.0f;
					debris.Prefab = prefab;
					debris.Scale  = prefab.transform.localScale * Random.Range(spawnScaleMin, spawnScaleMax);

					debris.transform.SetParent(transform, false);

					debris.transform.position   = target.transform.position + vector.normalized * distance;
					debris.transform.rotation   = Random.rotationUniform;
					debris.transform.localScale = prefab.transform.localScale * minScale;

					debris.InvokeOnSpawn();

					if (this.debris == null)
					{
						this.debris = new List<SgtDebris>();
					}

					this.debris.Add(debris);
				}
			}
		}

		[ContextMenu("Spawn All Debris Inside")]
		public void SpawnAllDebrisInside()
		{
			if (spawnLimit > 0)
			{
				var count = debris != null ? debris.Count : 0;

				for (var i = count; i < spawnLimit; i++)
				{
					SpawnDebrisInside();
				}
			}
		}

		public float GetFollowerDensity()
		{
			var density  = 1.0f;

			if (spawnInside != null && target != null)
			{
				density = spawnInside.GetDensity(target.position);
			}

			return density;
		}

		public static SgtDebrisSpawner Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtDebrisSpawner Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Debris Spawner", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtDebrisSpawner>();
		}

		protected virtual void Awake()
		{
			ResetFollower();

			if (spawnOnAwake == true)
			{
				SpawnAllDebrisInside();
			}
		}

		protected virtual void OnEnable()
		{
			ResetFollower();
		}
	
		protected virtual void FixedUpdate()
		{
			var newFollowerPosition = target != null ? target.position : Vector3.zero;

			followerVelocity = (newFollowerPosition - followerPosition) * CwHelper.Reciprocal(Time.fixedDeltaTime);
			followerPosition = newFollowerPosition;
		}

		protected virtual void Update()
		{
			if (target == null)
			{
				ClearDebris(); return;
			}

			var followerDensity  = GetFollowerDensity();

			if (followerDensity > 0.0f)
			{
				var debrisCount = debris != null ? debris.Count : 0;

				if (debrisCount < spawnLimit)
				{
					spawnCooldown -= Time.deltaTime;

					while (spawnCooldown <= 0.0f)
					{
						spawnCooldown += Random.Range(spawnRateMin, spawnRateMax);

						SpawnDebris(false);

						debrisCount += 1;

						if (debrisCount >= spawnLimit)
						{
							break;
						}
					}
				}
			}

			followerPosition = target.position;

			if (debris != null)
			{
				var distanceRange = hideDistance - showDistance;

				for (var i = debris.Count - 1; i >= 0; i--)
				{
					var debris = this.debris[i];

					if (debris != null)
					{
						var targetScale = default(float);
						var distance    = Vector3.Distance(followerPosition, debris.transform.position);

						// Fade its size in
						var factor = CwHelper.DampenFactor(showSpeed, Time.deltaTime, 0.1f);

						debris.Show = Mathf.Lerp(debris.Show, 1.0f, factor);

						if (distance < showDistance)
						{
							targetScale = 1.0f;
						}
						else if (distance > hideDistance)
						{
							targetScale = 0.0f;
						}
						else
						{
							targetScale = 1.0f - CwHelper.Divide(distance - showDistance, distanceRange);
						}

						debris.transform.localScale = debris.Scale * debris.Show * Mathf.Max(minScale, targetScale);

						if (targetScale <= 0.0f)
						{
							Despawn(debris, i);
						}
					}
					else
					{
						this.debris.RemoveAt(i);
					}
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = Matrix4x4.Translate(target != null ? target.position : transform.position);

			Gizmos.DrawWireSphere(Vector3.zero, showDistance);

			Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

			Gizmos.DrawWireSphere(Vector3.zero, hideDistance);
		}
#endif

		private SgtDebris Spawn(SgtDebris prefab)
		{
			if (prefab.Pool == true)
			{
				targetPrefab = prefab;

				var debris = SgtComponentPool<SgtDebris>.Pop(DebrisMatch);

				if (debris != null)
				{
					debris.transform.SetParent(null, false);

					return debris;
				}
			}

			var clone = Instantiate(prefab);

			clone.gameObject.SetActive(true);

			return clone;
		}

		private void Despawn(SgtDebris debris, int index)
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

			this.debris.RemoveAt(index);
		}

		private void ResetFollower()
		{
			followerVelocity = Vector3.zero;
			followerPosition = target != null ? target.position : Vector3.zero;
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
	using TARGET = SgtDebrisSpawner;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtDebrisSpawner_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Target == null));
				Draw("target", "If this transform is inside the radius then debris will begin spawning.");
			EndError();
			BeginDisabled();
				BeginIndent();
					EditorGUILayout.Slider("Density", tgt.GetFollowerDensity(), 0.0f, 1.0f);
				EndIndent();
			EndDisabled();
			Draw("spawnInside", "The shapes the debris will spawn inside.");

			Separator();

			BeginError(Any(tgts, t => t.ShowSpeed <= 0.0f));
				Draw("showSpeed", "How quickly the debris shows after it spawns.");
			EndError();
			BeginError(Any(tgts, t => t.ShowDistance <= 0.0f || t.ShowDistance > t.HideDistance));
				Draw("showDistance", "The distance from the follower that debris begins spawning.");
			EndError();
			BeginError(Any(tgts, t => t.HideDistance < 0.0f || t.ShowDistance > t.HideDistance));
				Draw("hideDistance", "The distance from the follower that debris gets hidden.");
			EndError();

			Separator();

			Draw("spawnOnAwake", "Should all the debris be automatically spawned at the start?");
			BeginError(Any(tgts, t => t.SpawnLimit < 0));
				Draw("spawnLimit", "The maximum amount of debris that can be spawned.");
			EndError();
			BeginError(Any(tgts, t => t.SpawnRateMin < 0.0f || t.SpawnRateMin > t.SpawnRateMax));
				Draw("spawnRateMin", "The minimum amount of seconds between debris spawns.");
			EndError();
			BeginError(Any(tgts, t => t.SpawnRateMax < 0.0f || t.SpawnRateMin > t.SpawnRateMax));
				Draw("spawnRateMax", "The maximum amount of seconds between debris spawns.");
			EndError();
			BeginError(Any(tgts, t => t.SpawnScaleMin < 0.0f || t.SpawnScaleMin > t.SpawnScaleMax));
				Draw("spawnScaleMin", "The minimum scale multiplier applied to spawned debris.");
			EndError();
			BeginError(Any(tgts, t => t.SpawnScaleMax < 0.0f || t.SpawnScaleMin > t.SpawnScaleMax));
				Draw("spawnScaleMax", "The maximum scale multiplier applied to spawned debris.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.Prefabs == null || t.Prefabs.Count == 0 || t.Prefabs.Contains(null) == true));
				Draw("prefabs", "These prefabs are randomly picked from when spawning new debris.");
			EndError();
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Debris Spawner", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtDebrisSpawner.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif