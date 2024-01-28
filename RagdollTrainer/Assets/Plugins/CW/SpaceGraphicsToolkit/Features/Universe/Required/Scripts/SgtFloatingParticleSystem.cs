using UnityEngine;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component updates the position of the attached <b>ParticleSystem</b> component when the origin snaps with the Universe feature.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(ParticleSystem))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingParticleSystem")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating ParticleSystem")]
	public class SgtFloatingParticleSystem : MonoBehaviour
	{
		[System.NonSerialized]
		private ParticleSystem cachedParticleSystem;

		private static ParticleSystem.Particle[] tempParticles;

		protected virtual void OnEnable()
		{
			cachedParticleSystem = GetComponent<ParticleSystem>();

			SgtCommon.OnSnap += HandleSnap;
		}

		protected virtual void OnDisable()
		{
			SgtCommon.OnSnap -= HandleSnap;
		}

		private void HandleSnap(Vector3 delta)
		{
			var count = cachedParticleSystem.main.maxParticles;

			if (tempParticles == null || tempParticles.Length < count)
			{
				tempParticles = new ParticleSystem.Particle[Mathf.Max(1024, count)];
			}

			count = cachedParticleSystem.GetParticles(tempParticles);

			for (var i = 0; i < count; i++)
			{
				tempParticles[i].position += delta;
			}

			cachedParticleSystem.SetParticles(tempParticles, count);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingParticleSystem;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingParticleSystem_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

		}
	}
}
#endif