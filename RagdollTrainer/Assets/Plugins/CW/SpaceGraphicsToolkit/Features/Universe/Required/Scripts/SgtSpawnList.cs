using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to create a list of SgtFloatingObject prefabs that are associated with a specific Category name.
	/// This allows you to easily manage what objects get spawned from each type of spawner.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtSpawnList")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Spawn List")]
	public class SgtSpawnList : MonoBehaviour
	{
		/// <summary>The type of prefabs these are (e.g. Planet).</summary>
		public string Category { set { category = value; } get { return category; } } [SerializeField] private string category;

		/// <summary>The prefabs belonging to this spawn list.</summary>
		public List<SgtFloatingObject> Prefabs { get { if (prefabs == null) prefabs = new List<SgtFloatingObject>(); return prefabs; } } [SerializeField] private List<SgtFloatingObject> prefabs;

		/// <summary>This stores all active and enabled instances of this component.</summary>
		public static LinkedList<SgtSpawnList> Instances = new LinkedList<SgtSpawnList>(); private LinkedListNode<SgtSpawnList> node;

		public static SgtSpawnList Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtSpawnList Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Spawn List", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtSpawnList>();
		}

		protected virtual void OnEnable()
		{
			node = Instances.AddLast(this);
		}

		protected virtual void OnDisable()
		{
			Instances.Remove(node); node = null;
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtSpawnList;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtSpawnList_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("category", "The type of prefabs these are (e.g. Planet).");
			Draw("prefabs", "The prefabs belonging to this spawn list.");
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Spawn List", false, 10)]
		private static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtSpawnList.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif