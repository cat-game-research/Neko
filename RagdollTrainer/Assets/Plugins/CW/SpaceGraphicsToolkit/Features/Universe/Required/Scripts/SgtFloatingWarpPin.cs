using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using CW.Common;

namespace SpaceGraphicsToolkit
{
	/// <summary>This component moves Rect above the currently picked SgtFloatingTarget. You can tap/click the screen to update the picked target.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtFloatingWarpPin")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Floating Warp Pin")]
	public class SgtFloatingWarpPin : MonoBehaviour
	{
		[System.Serializable] public class SgtFloatingTargetEvent : UnityEvent {}

		/// <summary>Fingers that began touching the screen on top of these UI layers will be ignored.</summary>
		public LayerMask GuiLayers { set { guiLayers = value; } get { return guiLayers; } } [SerializeField] private LayerMask guiLayers = 1 << 5;

		/// <summary>The maximum distance between the tap/click point at the SgtWarpTarget in scaled screen space.</summary>
		public float PickDistance { set { pickDistance = value; } get { return pickDistance; } } [SerializeField] private float pickDistance = 0.025f;

		/// <summary>The currently picked target.</summary>
		public SgtFloatingTarget CurrentTarget { set { currentTarget = value; } get { return currentTarget; } } [SerializeField] private SgtFloatingTarget currentTarget;

		/// <summary>The parent rect of the pin.</summary>
		public RectTransform Parent { set { parent = value; } get { return parent; } } [SerializeField] private RectTransform parent;

		/// <summary>The main rect of the pin that will be placed on the screen on top of the CurrentTarget.</summary>
		public RectTransform Rect { set { rect = value; } get { return rect; } } [SerializeField] private RectTransform rect;

		/// <summary>The name of the pin.</summary>
		public Text Title { set { title = value; } get { return title; } } [SerializeField] private Text title;

		/// <summary>The group that will control hide/show of the pin.</summary>
		public CanvasGroup Group { set { group = value; } get { return group; } } [SerializeField] private CanvasGroup group;

		/// <summary>The warp component that will be used.</summary>
		public SgtFloatingWarp Warp { set { warp = value; } get { return warp; } } [SerializeField] private SgtFloatingWarp warp;

		public SgtFloatingCamera FloatingCamera { set { floatingCamera = value; } get { return floatingCamera; } } [SerializeField] private SgtFloatingCamera floatingCamera;

		public Camera WorldCamera { set { worldCamera = value; } get { return worldCamera; } } [SerializeField] private Camera worldCamera;

		/// <summary>Hide the pin if we're within warping distance?</summary>
		public bool HideIfTooClose { set { hideIfTooClose = value; } get { return hideIfTooClose; } } [SerializeField] private bool hideIfTooClose = true;

		[HideInInspector]
		public float Alpha { set { alpha = value; } get { return alpha; } } [SerializeField] private float alpha;

		public SgtFloatingTargetEvent OnWarp { get { if (onWarp == null) onWarp = new SgtFloatingTargetEvent(); return onWarp; } } [SerializeField] private SgtFloatingTargetEvent onWarp;

		public void ClickWarp()
		{
			if (currentTarget != null)
			{
				if (warp != null)
				{
					warp.WarpTo(currentTarget);
				}

				if (onWarp != null)
				{
					onWarp.Invoke();
				}
			}
		}

		public void Pick(Vector2 pickScreenPoint)
		{
			if (floatingCamera != null && worldCamera != null)
			{
				var bestTarget   = default(SgtFloatingTarget);
				var bestDistance = float.PositiveInfinity;

				foreach (var warpTarget in SgtFloatingTarget.Instances)
				{
					var localPosition = floatingCamera.CalculatePosition(warpTarget.CachedPoint.Position);
					var screenPoint   = worldCamera.WorldToScreenPoint(localPosition);

					if (screenPoint.z >= 0.0f)
					{
						var distance = ((Vector2)screenPoint - pickScreenPoint).sqrMagnitude;

						if (distance <= bestDistance)
						{
							bestDistance = distance;
							bestTarget   = warpTarget;
						}
					}
				}

				if (bestTarget != null)
				{
					var pickThreshold = Mathf.Min(Screen.width, Screen.height) * pickDistance;

					if (bestDistance <= pickThreshold * pickThreshold)
					{
						currentTarget = bestTarget;
					}
				}
				else
				{
					currentTarget = null;
				}
			}
		}

		protected virtual void OnEnable()
		{
			CwInputManager.EnsureThisComponentExists();

			CwInputManager.OnFingerDown += HandleFingerDown;
		}

		protected virtual void OnDisable()
		{
			CwInputManager.OnFingerDown -= HandleFingerDown;
		}

		protected virtual void LateUpdate()
		{
			var targetAlpha = 0.0f;

			if (floatingCamera != null && worldCamera != null)
			{
				if (currentTarget != null)
				{
					var localPosition = floatingCamera.CalculatePosition(currentTarget.CachedPoint.Position);
					var screenPoint   = worldCamera.WorldToScreenPoint(localPosition);

					if (screenPoint.z >= 0.0f)
					{
						var anchoredPosition = default(Vector2);

						if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out anchoredPosition) == true)
						{
							rect.anchoredPosition = anchoredPosition;
						}

						targetAlpha = 1.0f;

						if (hideIfTooClose == true)
						{
							if (SgtPosition.SqrDistance(SgtFloatingCamera.Instances.First.Value.Position, currentTarget.CachedPoint.Position) <= currentTarget.WarpDistance * currentTarget.WarpDistance)
							{
								targetAlpha = 0.0f;
							}
						}
					}
					else
					{
						alpha = 0.0f;
					}

					title.text = currentTarget.WarpName;
				}
			}

			var factor = CwHelper.DampenFactor(10.0f, Time.deltaTime);

			alpha = Mathf.Lerp(alpha, targetAlpha, factor);

			group.alpha          = alpha;
			group.blocksRaycasts = targetAlpha > 0.0f;
		}

		private void HandleFingerDown(CwInputManager.Finger finger)
		{
			if (CwInputManager.PointOverGui(finger.ScreenPosition, guiLayers) == true)
			{
				return;
			}

			Pick(finger.ScreenPosition);
		}
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit
{
	using UnityEditor;
	using TARGET = SgtFloatingWarpPin;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtFloatingWarpPin_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("guiLayers", "Fingers that began touching the screen on top of these UI layers will be ignored.");
			Draw("pickDistance", "The maximum distance between the tap/click point at the SgtWarpTarget in scaled screen space.");
			Draw("currentTarget", "The currently picked target.");

			Separator();

			Draw("floatingCamera", "The currently picked target.");
			Draw("worldCamera", "The currently picked target.");

			Separator();

			BeginError(Any(tgts, t => t.Parent == null));
				Draw("parent", "The parent rect of the pin.");
			EndError();
			BeginError(Any(tgts, t => t.Rect == null));
				Draw("rect", "The main rect of the pin that will be placed on the screen on top of the CurrentTarget.");
			EndError();
			BeginError(Any(tgts, t => t.Title == null));
				Draw("title", "The name of the pin.");
			EndError();
			BeginError(Any(tgts, t => t.Group == null));
				Draw("group", "The group that will control hide/show of the pin.");
			EndError();
			BeginError(Any(tgts, t => t.Warp == null));
				Draw("warp", "The warp component that will be used.");
			EndError();
			Draw("hideIfTooClose", "Hide the pin if we're within warping distance?");

			Separator();

			Draw("onWarp");
		}
	}
}
#endif