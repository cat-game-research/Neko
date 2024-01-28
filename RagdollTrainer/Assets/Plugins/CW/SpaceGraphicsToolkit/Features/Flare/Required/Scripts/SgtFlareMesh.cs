using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Flare
{
	/// <summary>This component allows you to generate the SgtFlare.Mesh field.</summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(SgtFlare))]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFlareMesh")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Flare Mesh")]
	public class SgtFlareMesh : MonoBehaviour
	{
		/// <summary>The amount of points used to make the flare mesh.</summary>
		public int Detail { set { detail = value; DirtyMesh(); } get { return detail; } } [SerializeField] private int detail = 512;

		/// <summary>The base radius of the flare in local space.</summary>
		public float Radius { set { radius = value; DirtyMesh(); } get { return radius; } } [SerializeField] private float radius = 2.0f;

		/// <summary>Deform the flare based on cosine wave?</summary>
		public bool Wave { set { wave = value; DirtyMesh(); } get { return wave; } } [SerializeField] private bool wave;

		/// <summary>The strength of the wave in local space.</summary>
		public float WaveStrength { set { waveStrength = value; DirtyMesh(); } get { return waveStrength; } } [SerializeField] private float waveStrength = 5.0f;

		/// <summary>The amount of wave peaks.</summary>
		public int WavePoints { set { wavePoints = value; DirtyMesh(); } get { return wavePoints; } } [SerializeField] private int wavePoints = 4;

		/// <summary>The sharpness of the waves.</summary>
		public float WavePower { set { wavePower = value; DirtyMesh(); } get { return wavePower; } } [SerializeField] private float wavePower = 5.0f;

		/// <summary>The angle offset of the waves.</summary>
		public float WavePhase { set { wavePhase = value; DirtyMesh(); } get { return wavePhase; } } [SerializeField] private float wavePhase;

		/// <summary>Deform the flare based on noise?</summary>
		public bool Noise { set { noise = value; DirtyMesh(); } get { return noise; } } [SerializeField] private bool noise;

		/// <summary>The strength of the noise in local space.</summary>
		public float NoiseStrength { set { noiseStrength = value; DirtyMesh(); } get { return noiseStrength; } } [SerializeField] private float noiseStrength = 5.0f;

		/// <summary>The amount of noise points.</summary>
		public int NoisePoints { set { noisePoints = value; DirtyMesh(); } get { return noisePoints; } } [SerializeField] private int noisePoints = 50;

		/// <summary>The angle offset of the noise.</summary>
		public float NoisePhase { set { noisePhase = value; DirtyMesh(); } get { return noisePhase; } } [SerializeField] private float noisePhase;

		/// <summary>The random seed used for the random noise.</summary>
		public int NoiseSeed { set { noiseSeed = value; DirtyMesh(); } get { return noiseSeed; } } [SerializeField] [CwSeed] private int noiseSeed;

		[System.NonSerialized]
		private Mesh generatedMesh;

		[System.NonSerialized]
		private SgtFlare cachedFlare;

		[System.NonSerialized]
		private bool cachedFlareSet;

		private static List<float> points = new List<float>();

		public SgtFlare CachedFlare
		{
			get
			{
				if (cachedFlareSet == false)
				{
					cachedFlare    = GetComponent<SgtFlare>();
					cachedFlareSet = true;
				}

				return cachedFlare;
			}
		}

		public Mesh GeneratedMesh
		{
			get
			{
				return generatedMesh;
			}
		}

		public void DirtyMesh()
		{
			UpdateMesh();
		}

#if UNITY_EDITOR
		/// <summary>This method allows you to export the generated mesh as an asset.
		/// Once done, you can remove this component, and set the <b>SgtFlare</b> component's <b>Mesh</b> setting using the exported asset.</summary>
		[ContextMenu("Export Mesh")]
		public void ExportMesh()
		{
			CwHelper.ExportAssetDialog(generatedMesh, "Flare Mesh");
		}
#endif

		[ContextMenu("Update Mesh")]
		public void UpdateMesh()
		{
			if (detail > 2)
			{
				if (generatedMesh == null)
				{
					generatedMesh = SgtCommon.CreateTempMesh("Mesh (Generated)");

					ApplyMesh();
				}

				var total     = detail + 1;
				var positions = new Vector3[total];
				var coords1   = new Vector2[total];
				var indices   = new int[detail * 3];
				var angleStep = (Mathf.PI * 2.0f) / detail;
				var noiseStep = 0.0f;

				if (noise == true && noisePoints > 0)
				{
					CwHelper.BeginSeed(noiseSeed);
					{
						points.Clear();

						for (var i = 0; i < noisePoints; i++)
						{
							points.Add(Random.value);
						}

						noiseStep = noisePoints / (float)detail;
					}
					CwHelper.EndSeed();
				}

				// Write center vertices
				positions[0] = Vector3.zero;
				coords1[0] = Vector2.zero;

				// Write outer vertices
				for (var point = 0; point < detail; point++)
				{
					var angle = angleStep * point;
					var x     = Mathf.Sin(angle);
					var y     = Mathf.Cos(angle);
					var r     = radius;

					if (wave == true)
					{
						var waveAngle = (angle + wavePhase * Mathf.Deg2Rad) * wavePoints;

						r += Mathf.Pow(Mathf.Cos(waveAngle) * 0.5f + 0.5f, wavePower * wavePower) * waveStrength;
					}

					if (noise == true && noisePoints > 0)
					{
						var noise  = Mathf.Repeat(noiseStep * point + noisePhase, noisePoints);
						//var noise = point * noiseStep + NoisePhase;
						var index  = (int)noise;
						var frac   = noise % 1.0f;
						var pointA = points[(index + 0) % noisePoints];
						var pointB = points[(index + 1) % noisePoints];
						var pointC = points[(index + 2) % noisePoints];
						var pointD = points[(index + 3) % noisePoints];

						r += SgtCommon.CubicInterpolate(pointA, pointB, pointC, pointD, frac) * noiseStrength;
					}

					// Write outer vertices
					var v = point + 1;

					positions[v] = new Vector3(x * r, y * r, 0.0f);
					coords1[v] = new Vector2(1.0f, 0.0f);
				}

				for (var tri = 0; tri < detail; tri++)
				{
					var i  = tri * 3;
					var v0 = tri + 1;
					var v1 = tri + 2;

					if (v1 >= total)
					{
						v1 = 1;
					}

					indices[i + 0] = 0;
					indices[i + 1] = v0;
					indices[i + 2] = v1;
				}

				generatedMesh.Clear(false);
				generatedMesh.vertices  = positions;
				generatedMesh.uv        = coords1;
				generatedMesh.triangles = indices;
				generatedMesh.RecalculateNormals();
				generatedMesh.RecalculateBounds();
			}
		}

		[ContextMenu("Apply Mesh")]
		public void ApplyMesh()
		{
			CachedFlare.Mesh = generatedMesh;
		}

		[ContextMenu("Remove Mesh")]
		public void RemoveMesh()
		{
			if (CachedFlare.Mesh == generatedMesh)
			{
				cachedFlare.Mesh = null;
			}
		}

		protected virtual void OnEnable()
		{
			UpdateMesh();
			ApplyMesh();
		}

		protected virtual void OnDisable()
		{
			RemoveMesh();
		}

		protected virtual void OnDestroy()
		{
			if (generatedMesh != null)
			{
				generatedMesh.Clear(false);

				SgtCommon.MeshPool.Push(generatedMesh);
			}
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			DirtyMesh();
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Flare
{
	using UnityEditor;
	using TARGET = SgtFlareMesh;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFlareMesh_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			var dirtyMesh = false;

			BeginError(Any(tgts, t => t.Detail <= 2));
				Draw("detail", ref dirtyMesh, "The amount of points used to make the flare mesh.");
			EndError();
			BeginError(Any(tgts, t => t.Radius <= 0.0f));
				Draw("radius", ref dirtyMesh, "The base radius of the flare in local space.");
			EndError();

			Separator();

			Draw("wave", ref dirtyMesh, "Deform the flare based on cosine wave?");

			if (Any(tgts, t => t.Wave == true))
			{
				BeginIndent();
					Draw("waveStrength", ref dirtyMesh, "The strength of the wave in local space.");
					BeginError(Any(tgts, t => t.WavePoints < 0));
						Draw("wavePoints", ref dirtyMesh, "The amount of wave peaks.");
					EndError();
					BeginError(Any(tgts, t => t.WavePower < 1.0f));
						Draw("wavePower", ref dirtyMesh, "The sharpness of the waves.");
					EndError();
					Draw("wavePhase", ref dirtyMesh, "The angle offset of the waves.");
				EndIndent();
			}

			Separator();
		
			Draw("noise", ref dirtyMesh, "Deform the flare based on noise?");

			if (Any(tgts, t => t.Noise == true))
			{
				BeginIndent();
					BeginError(Any(tgts, t => t.NoiseStrength < 0.0f));
						Draw("noiseStrength", ref dirtyMesh, "The strength of the noise in local space.");
					EndError();
					BeginError(Any(tgts, t => t.NoisePoints <= 0));
						Draw("noisePoints", ref dirtyMesh, "The amount of noise points.");
					EndError();
					Draw("noisePhase", ref dirtyMesh, "The angle offset of the noise.");
					Draw("noiseSeed", ref dirtyMesh, "The random seed used for the random noise.");
				EndIndent();
			}

			if (dirtyMesh == true) Each(tgts, t => t.DirtyMesh(), true, true);
		}
	}
}
#endif