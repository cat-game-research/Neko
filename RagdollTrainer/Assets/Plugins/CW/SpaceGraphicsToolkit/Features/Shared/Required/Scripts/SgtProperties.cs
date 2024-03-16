using System.Collections.Generic;
using UnityEngine;

namespace SpaceGraphicsToolkit
{
	/// <summary>This class wraps <b>MaterialPropertyBlock</b> to allow for the removal of specific properties.</summary>
	public class SgtProperties
	{
		private MaterialPropertyBlock properties;

		private Dictionary<int, Texture  > textures = new Dictionary<int, Texture  >();
		private Dictionary<int, Vector4  > vectors  = new Dictionary<int, Vector4  >();
		private Dictionary<int, Color    > colors   = new Dictionary<int, Color    >();
		private Dictionary<int, int      > ints     = new Dictionary<int, int      >();
		private Dictionary<int, float    > floats   = new Dictionary<int, float    >();
		private Dictionary<int, Matrix4x4> matrices = new Dictionary<int, Matrix4x4>();

		public static implicit operator MaterialPropertyBlock(SgtProperties p)
		{
			p.UpdateInstance();

			return p.properties;
		}

		public void SetTexture(int k, Texture v)
		{
			UpdateInstance(); textures.Remove(k); textures.Add(k, v); properties.SetTexture(k, v);
		}

		public void SetVector(int k, Vector4 v)
		{
			UpdateInstance(); vectors.Remove(k); vectors.Add(k, v); properties.SetVector(k, v);
		}

		public void SetInt(int k, int v)
		{
			UpdateInstance(); ints.Remove(k); ints.Add(k, v); properties.SetInt(k, v);
		}

		public void SetFloat(int k, float v)
		{
			UpdateInstance(); floats.Remove(k); floats.Add(k, v); properties.SetFloat(k, v);
		}

		public void SetMatrix(int k, Matrix4x4 v)
		{
			UpdateInstance(); matrices.Remove(k); matrices.Add(k, v); properties.SetMatrix(k, v);
		}

		public void Clear(int k)
		{
			if (textures.Remove(k) == true ||
				 vectors.Remove(k) == true ||
				  colors.Remove(k) == true ||
				    ints.Remove(k) == true ||
				  floats.Remove(k) == true ||
				matrices.Remove(k) == true)
			{
				Rebuild();
			}
		}

		private void UpdateInstance()
		{
			if (properties == null)
			{
				properties = new MaterialPropertyBlock();
			}
		}

		private void Rebuild()
		{
			properties.Clear();

			foreach (var pair in textures) properties.SetTexture(pair.Key, pair.Value);
			foreach (var pair in vectors ) properties.SetVector (pair.Key, pair.Value);
			foreach (var pair in colors  ) properties.SetColor  (pair.Key, pair.Value);
			foreach (var pair in ints    ) properties.SetInt    (pair.Key, pair.Value);
			foreach (var pair in floats  ) properties.SetFloat  (pair.Key, pair.Value);
			foreach (var pair in matrices) properties.SetMatrix (pair.Key, pair.Value);
		}
	}
}