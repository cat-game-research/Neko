using System.Collections.Generic;
using UnityEngine;

namespace SpaceGraphicsToolkit
{
	public class SgtShaderProperties
	{
		public class Entry
		{
			public Material              Material;
			public Camera                Camera;
			public MaterialPropertyBlock Properties;
			public bool                  Marked;
		}

		private List<Entry> entries = new List<Entry>();

		public MaterialPropertyBlock GetProperties(Material material, Camera camera = null)
		{
			foreach (var entry in entries)
			{
				if (entry.Material == material && entry.Camera == camera)
				{
					entry.Marked = false;

					return entry.Properties;
				}
			}

			var newEntry = new Entry();
			
			newEntry.Material   = material;
			newEntry.Camera     = camera;
			newEntry.Properties = new MaterialPropertyBlock();

			entries.Add(newEntry);

			return newEntry.Properties;
		}

		public void Clear()
		{
			for (var i = entries.Count - 1; i >= 0; i--)
			{
				var entry = entries[i];

				if (entry.Marked == true)
				{
					entries.RemoveAt(i);
				}
				else
				{
					entry.Marked = true;
				}
			}
		}
	}
}