using EchoShift.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class Phase4HudVisual : MonoBehaviour
    {
        [SerializeField] private SectionTransitionCoordinator coordinator;
        [SerializeField] private Image timerFill;
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private PauseMenuController pauseMenu;
        [SerializeField] private CanvasGroup gameplayGroup;

        public bool HasRequiredReferences => coordinator != null && timerFill != null &&
            leftPanel != null && rightPanel != null && bottomPanel != null &&
            pauseMenu != null && gameplayGroup != null;
        public float TimerFillAmount => timerFill != null ? timerFill.fillAmount : 0f;
        public bool SupportsReferenceResolutions =>
            ValidateLayout(1280, 720) && ValidateLayout(1920, 1080) && ValidateLayout(2560, 1440);

        public void Configure(SectionTransitionCoordinator sectionCoordinator,
            Image loopTimerFill, RectTransform left, RectTransform right, RectTransform bottom,
            PauseMenuController menu, CanvasGroup group)
        {
            coordinator = sectionCoordinator;
            timerFill = loopTimerFill;
            leftPanel = left;
            rightPanel = right;
            bottomPanel = bottom;
            pauseMenu = menu;
            gameplayGroup = group;
        }

        private void LateUpdate() => RefreshNowForTests();

        public void RefreshNowForTests()
        {
            if (!HasRequiredReferences || coordinator.ActiveSection == null) return;
            Core.LoopDirector director = coordinator.ActiveSection.Director;
            timerFill.fillAmount = 1f - Mathf.Clamp01(
                director.CurrentTick / (float)Mathf.Max(1, director.MaxTicks));
            bool paused = pauseMenu.IsVisible;
            gameplayGroup.alpha = paused ? 0f : 1f;
            gameplayGroup.interactable = !paused;
            gameplayGroup.blocksRaycasts = !paused;
        }

        public bool ValidateLayout(int width, int height)
        {
            if (!HasRequiredReferences || width < 960 || height < 540) return false;
            float scale = Mathf.Min(width / 1920f, height / 1080f);
            float leftWidth = leftPanel.rect.width * scale;
            float rightWidth = rightPanel.rect.width * scale;
            float centerGap = width - leftWidth - rightWidth - 96f * scale;
            return centerGap >= 320f * scale && bottomPanel.rect.height * scale <= height * 0.22f;
        }
    }
}
