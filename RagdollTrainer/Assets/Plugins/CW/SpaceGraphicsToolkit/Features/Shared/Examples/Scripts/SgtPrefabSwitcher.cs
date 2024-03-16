using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component allows you to switch between a list of prefabs from UI button events.</summary>
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtPrefabSwitcher")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Prefab Switcher")]
	public class SgtPrefabSwitcher : MonoBehaviour
	{
		[System.Serializable] public class StringEvent : UnityEvent<string> {}

		/// <summary>The speed the prefabs will switch.</summary>
		public float Speed { set { speed = value; } get { return speed; } } [SerializeField] private float speed = 2.0f;

		/// <summary>The prefab index that will be displayed.</summary>
		public int Index { set { index = value; } get { return index; } } [SerializeField] private int index;

		/// <summary>The prefabs that can be switched between.</summary>
		public List<Transform> Prefabs { get { if (prefabs == null) prefabs = new List<Transform>(); return prefabs; } } [SerializeField] private List<Transform> prefabs;

		public StringEvent OnTitle { get { if (onTitle == null) onTitle = new StringEvent(); return onTitle; } } [SerializeField] private StringEvent onTitle;

		[SerializeField]
		private Transform expectedPrefab;

		[SerializeField]
		private Transform clone;

		[SerializeField]
		private Vector3 scale;

		[SerializeField]
		private float transition;

		/// <summary>This method decrements <b>Index</b> by one, and wraps around the prefab count.</summary>
		[ContextMenu("Switch To Prev")]
		public void SwitchToPrev()
		{
			index--;
		}

		/// <summary>This method increments <b>Index</b> by one, and wraps around the prefab count.</summary>
		[ContextMenu("Switch To Next")]
		public void SwitchToNext()
		{
			index++;
		}

		protected virtual void Update()
		{
			if (prefabs == null)
			{
				prefabs = new List<Transform>();
			}

			if (index < 0)
			{
				index = prefabs.Count - 1;
			}
			else if (index >= prefabs.Count)
			{
				index = 0;
			}

			var prefab = default(Transform);

			if (prefabs.Count > 0)
			{
				prefab = prefabs[index];
			}

			if (prefab != expectedPrefab)
			{
				if (clone == null)
				{
					transition = 0.0f;
				}
				else
				{
					transition = Mathf.Clamp01(transition - speed * Time.deltaTime);
				}

				if (transition == 0.0f)
				{
					if (clone != null)
					{
						CwHelper.Destroy(clone.gameObject);

						clone = null;
					}

					expectedPrefab = prefab;

					if (prefab != null)
					{
						clone = Instantiate(prefab, transform);
						scale = clone.localScale;

						clone.localScale = new Vector3(0.0f, 0.0f, 0.01f);

						if (onTitle != null)
						{
							onTitle.Invoke(prefab.name);
						}
					}
				}
			}
			else
			{
				transition = Mathf.Clamp01(transition + speed * Time.deltaTime);
			}

			if (clone != null)
			{
				clone.localScale = scale * Mathf.SmoothStep(0.0f, 1.0f, transition);
			}
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtPrefabSwitcher;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtPrefabSwitcher_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.Speed <= 0.0f));
				Draw("speed", "The speed the prefabs will switch.");
			EndError();
			Draw("index", "The prefab index that will be displayed.");

			Separator();

			Draw("prefabs", "The prefabs that can be switched between.");

			Separator();
			Draw("onTitle");
		}
	}
}
#endif