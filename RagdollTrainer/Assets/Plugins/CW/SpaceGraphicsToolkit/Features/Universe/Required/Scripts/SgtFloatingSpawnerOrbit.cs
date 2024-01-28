using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will automatically spawn prefabs in orbit around the attached SgtFloatingPoint.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingSpawnerOrbit")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Spawner Orbit")]
	public class SgtFloatingSpawnerOrbit : SgtFloatingSpawner
	{
		/// <summary>The amount of prefabs that will be spawned.</summary>
		public int Count { set { count = value; } get { return count; } } [SerializeField] private int count = 10;

		/// <summary>The maximum degrees an orbit can tilt.</summary>
		public float TiltMax { set { tiltMax = value; } get { return tiltMax; } } [SerializeField] [Range(0.0f, 180.0f)] private float tiltMax = 10.0f;

		/// <summary>The maximum amount an orbit can be squashed.</summary>
		public float OblatenessMax { set { oblatenessMax = value; } get { return oblatenessMax; } } [SerializeField] [Range(0.0f, 1.0f)] private float oblatenessMax;

		/// <summary>The minimum distance away the prefabs can spawn.</summary>
		public SgtLength RadiusMin { set { radiusMin = value; } get { return radiusMin; } } [SerializeField] private SgtLength radiusMin = 200000.0;

		/// <summary>The maximum distance away the prefabs can spawn in meters.</summary>
		public SgtLength RadiusMax { set { radiusMax = value; } get { return radiusMax; } } [SerializeField] private SgtLength radiusMax = 2000000.0;

		protected override void SpawnAll()
		{
			var parentPoint = CachedObject;

			BuildSpawnList();

			CwHelper.BeginSeed(CachedObject.Seed);
			{
				var radMin = (double)radiusMin;
				var radMax = (double)radiusMax;
				var radRng = radMax - radMin;

				for (var i = 0; i < count; i++)
				{
					var radius     = radMin + radRng * Random.value;
					var angle      = Random.Range(0.0f, 360.0f);
					var tilt       = new Vector3(Random.Range(-tiltMax, tiltMax), 0.0f, Random.Range(-tiltMax, tiltMax));
					var oblateness = Random.Range(0.0f, oblatenessMax);
					var position   = SgtFloatingOrbit.CalculatePosition(parentPoint, radius, angle, tilt, Vector3.zero, oblateness);
					var instance   = SpawnAt(position, i);
					var orbit      = instance.GetComponent<SgtFloatingOrbit>();

					if (orbit == null)
					{
						orbit = instance.gameObject.AddComponent<SgtFloatingOrbit>();
					}

					orbit.ParentPoint      = parentPoint; 
					orbit.Radius           = radius;
					orbit.Angle            = angle;
					orbit.Oblateness       = oblateness;
					orbit.DegreesPerSecond = 1000.0 / radius;
					orbit.Tilt             = tilt;
				}
			}
			CwHelper.EndSeed();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingSpawnerOrbit;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingSpawnerOrbit_Editor : SgtFloatingSpawner_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			Draw("count", "The amount of prefabs that will be spawned.");
			Draw("tiltMax", "The maximum degrees an orbit can tilt.");
			Draw("oblatenessMax", "The maximum amount an orbit can be squashed.");
			BeginError(Any(tgts, t => t.RadiusMin <= 0.0 || t.RadiusMin > t.RadiusMax));
				Draw("radiusMin", "The minimum distance away the prefabs can spawn in meters.");
				Draw("radiusMax");
			EndError();

			if (Any(tgts, t => t.RadiusMin > t.Range || t.RadiusMax > t.Range))
			{
				Warning("The spawn range should be greater than the spawn radius.");
			}
		}
	}
}
#endif