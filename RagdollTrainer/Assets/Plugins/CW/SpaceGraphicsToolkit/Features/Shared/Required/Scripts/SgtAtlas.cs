using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This is the base class for all starfields, providing a simple interface for generating meshes from a list of stars, as well as the material to render it.</summary>
	[System.Serializable]
	public class SgtAtlas
	{
		public enum LayoutType
		{
			Grid,
			Custom
		}

		/// <summary>The layout of cells in the texture.</summary>
		public LayoutType Layout { set { layout = value; } get { return layout; } } [SerializeField] private LayoutType layout;

		/// <summary>The amount of columns in the texture.</summary>
		public int LayoutColumns { set { layoutColumns = value; } get { return layoutColumns; } } [SerializeField] private int layoutColumns = 1;

		/// <summary>The amount of rows in the texture.</summary>
		public int LayoutRows { set { layoutRows = value; } get { return layoutRows; } } [SerializeField] private int layoutRows = 1;

		/// <summary>The rects of each cell in the texture.</summary>
		public List<Rect> LayoutRects { get { if (layoutRects == null) layoutRects = new List<Rect>(); return layoutRects; } } [SerializeField] private List<Rect> layoutRects;

		protected static List<Vector4> tempCoords = new List<Vector4>();

		public List<Vector4> GetCoords()
		{
			if (layoutRects == null) layoutRects = new List<Rect>();

			if (layout == LayoutType.Grid)
			{
				layoutRects.Clear();

				if (layoutColumns > 0 && layoutRows > 0)
				{
					var invX = CwHelper.Reciprocal(layoutColumns);
					var invY = CwHelper.Reciprocal(layoutRows   );

					for (var y = 0; y < layoutRows; y++)
					{
						var offY = y * invY;

						for (var x = 0; x < layoutColumns; x++)
						{
							var offX = x * invX;
							var rect = new Rect(offX, offY, invX, invY);

							layoutRects.Add(rect);
						}
					}
				}
			}

			tempCoords.Clear();

			for (var i = 0; i < layoutRects.Count; i++)
			{
				var rect = layoutRects[i];

				tempCoords.Add(new Vector4(rect.xMin, rect.yMin, rect.xMax, rect.yMax));
			}

			if (tempCoords.Count == 0) tempCoords.Add(default(Vector4));

			return tempCoords;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtAtlas;

	[CustomPropertyDrawer(typeof(TARGET))]
	public class SgtAtlas_Drawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var layout = (SgtAtlas.LayoutType)property.FindPropertyRelative("layout").enumValueIndex;
			var height = base.GetPropertyHeight(property, label);

			height += 2;

			switch (layout)
			{
				case SgtAtlas.LayoutType.Grid:
				{
					height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("layoutColumns"));
					height += 2;
					height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("layoutRows"));
				}
				break;

				case SgtAtlas.LayoutType.Custom:
				{
					height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("layoutRects"));
				}
				break;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var layout = (SgtAtlas.LayoutType)property.FindPropertyRelative("layout").enumValueIndex;

			EditorGUI.BeginProperty(position, label, property);

			DrawProperty(ref position, property, label, "layout", "The layout of cells in the texture.", property.displayName);

			EditorGUI.indentLevel++;
			switch (layout)
			{
				case SgtAtlas.LayoutType.Grid:
				{
					DrawProperty(ref position, property, label, "layoutColumns", "The amount of columns in the texture.", "Columns");
					DrawProperty(ref position, property, label, "layoutRows", "The amount of rows in the texture.", "Rows");
				}
				break;

				case SgtAtlas.LayoutType.Custom:
				{
					DrawProperty(ref position, property, label, "layoutRects", "The rects of each cell in the texture.", "Rects");
				}
				break;
			}
			EditorGUI.indentLevel--;

			EditorGUI.EndProperty();
		}

		private void DrawProperty(ref Rect rect, SerializedProperty property, GUIContent label, string childName, string overrideTooltip = null, string overrideName = null)
		{
			var childProperty = property.FindPropertyRelative(childName);

			label.text = string.IsNullOrEmpty(overrideName) == false ? overrideName : childProperty.displayName;

			label.tooltip = string.IsNullOrEmpty(overrideTooltip) == false ? overrideTooltip : childProperty.tooltip;

			rect.height = EditorGUI.GetPropertyHeight(childProperty);

			EditorGUI.PropertyField(rect, childProperty, label);

			rect.y += rect.height + 2;
		}
	}
}
#endif