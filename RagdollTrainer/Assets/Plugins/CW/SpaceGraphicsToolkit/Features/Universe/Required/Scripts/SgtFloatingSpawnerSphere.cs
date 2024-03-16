using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component will automatically spawn prefabs in a circle around the attached SgtFloatingPoint.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingSpawnerSphere")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Spawner Sphere")]
	public class SgtFloatingSpawnerSphere : SgtFloatingSpawner
	{
		/// <summary>The amount of prefabs that will be spawned.</summary>
		public int Count { set { count = value; } get { return count; } } [SerializeField] private int count = 10;

		/// <summary>This component will spawn prefabs within this radius in world space.</summary>
		public SgtLength Radius { set { radius = value; } get { return radius; } } [SerializeField] private SgtLength radius = new SgtLength(2000000.0, SgtLength.ScaleType.Meter);

		/// <summary>The higher this value, the more likely the spawned objects will be pushed to the edge of the radius.</summary>
		public float Offset { set { offset = value; } get { return offset; } } [SerializeField] [Range(0.0f, 1.0f)] private float offset;

		/// <summary>This allows you to set how much orbital velocity the spawned objects get if they have a Rigidbody attached.</summary>
		public float VelocityScale { set { velocityScale = value; } get { return velocityScale; } } [SerializeField] private float velocityScale;

		protected override void SpawnAll()
		{
			var parentPoint = GetComponentInParent<SgtFloatingPoint>();

			BuildSpawnList();

			CwHelper.BeginSeed(CachedObject.Seed);
			{
				var rad = (double)radius;

				for (var i = 0; i < count; i++)
				{
					var position  = parentPoint.Position;
					var direction = Random.onUnitSphere;
					var distance  = Mathf.Lerp(this.offset, 1.0f, Random.value);
					var offset    = distance * direction;

					position.LocalX += offset.x * rad;
					position.LocalY += offset.y * rad;
					position.LocalZ += offset.z * rad;
					position.SnapLocal();

					var clone = SpawnAt(position, i);

					if (velocityScale > 0.0f)
					{
						var rigidbody = clone.GetComponent<Rigidbody>();

						if (rigidbody != null)
						{
							var cross = Vector3.Cross(direction, Random.onUnitSphere).normalized;

							rigidbody.velocity = (cross * velocityScale) / (distance * distance);
						}
					}
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
	using TARGET = SgtFloatingSpawnerSphere;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingSpawnerSphere_Editor : SgtFloatingSpawner_Editor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			base.OnInspector();

			Separator();

			Draw("count", "The amount of prefabs that will be spawned.");
			BeginError(Any(tgts, t => t.Radius <= 0.0));
				Draw("radius", "This component will spawn prefabs within this world space radius in world space.");
			EndError();

			if (Any(tgts, t => t.Radius > t.Range))
			{
				Warning("The spawn range should be greater than the spawn radius.");
			}

			Draw("offset", "The higher this value, the more likely the spawned objects will be pushed to the edge of the radius.");
			BeginError(Any(tgts, t => t.VelocityScale < 0.0f));
				Draw("velocityScale", "This allows you to set how much orbital velocity the spawned objects get if they have a Rigidbody attached.");
			EndError();
		}
	}
}
#endif