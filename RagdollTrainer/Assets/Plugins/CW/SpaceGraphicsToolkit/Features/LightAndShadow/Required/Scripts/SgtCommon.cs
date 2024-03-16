using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceGraphicsToolkit.LightAndShadow
{
	/// <summary>This class contains some useful methods used by this asset.</summary>
	internal static class SgtCommon
	{
		public const string HelpUrlPrefix = "https://carloswilkes.com/Documentation/SpaceGraphicsToolkit_LightAndShadow#";

		public const string ComponentMenuPrefix = "Space Graphics Toolkit/SGT ";

		public const string GameObjectMenuPrefix = "GameObject/Space Graphics Toolkit/";

		public static List<Material> tempMaterials = new List<Material>();

		public static void SetTempMaterial(Material material)
		{
			tempMaterials.Clear();

			tempMaterials.Add(material);
		}

		public static void SetTempMaterial(Material material1, Material material2)
		{
			tempMaterials.Clear();

			tempMaterials.Add(material1);
			tempMaterials.Add(material2);
		}

		public static void EnableKeyword(string keyword)
		{
			for (var i = tempMaterials.Count - 1; i >= 0; i--)
			{
				var tempMaterial = tempMaterials[i];

				if (tempMaterial != null)
				{
					if (tempMaterial.IsKeywordEnabled(keyword) == false)
					{
						tempMaterial.EnableKeyword(keyword);
					}
				}
			}
		}

		public static void DisableKeyword(string keyword)
		{
			for (var i = tempMaterials.Count - 1; i >= 0; i--)
			{
				var tempMaterial = tempMaterials[i];

				if (tempMaterial != null)
				{
					if (tempMaterial.IsKeywordEnabled(keyword) == true)
					{
						tempMaterial.DisableKeyword(keyword);
					}
				}
			}
		}

		public static void SetMatrix(string key, Matrix4x4 value)
		{
			for (var i = tempMaterials.Count - 1; i >= 0; i--)
			{
				var tempMaterial = tempMaterials[i];

				if (tempMaterial != null)
				{
					tempMaterial.SetMatrix(key, value);
				}
			}
		}
	}
}