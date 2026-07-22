using EchoShift.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class Phase4HudVisual : MonoBehaviour
    {
        [SerializeField] private SectionTransitionCoordinator coordinator;
        [SerializeField] private GameplayHud hud;
        [SerializeField] private Image timerFill;
        [SerializeField] private RectTransform statusPanel;
        [SerializeField] private RectTransform introPanel;
        [SerializeField] private RectTransform promptPanel;
        [SerializeField] private RectTransform carryPanel;
        [SerializeField] private PauseMenuController pauseMenu;
        [SerializeField] private CanvasGroup gameplayGroup;

        public bool HasRequiredReferences => coordinator != null && hud != null && timerFill != null &&
            statusPanel != null && introPanel != null && promptPanel != null && carryPanel != null &&
            pauseMenu != null && gameplayGroup != null;
        public float TimerFillAmount => timerFill != null ? timerFill.fillAmount : 0f;
        public bool IsPromptPanelVisible => promptPanel != null && promptPanel.gameObject.activeSelf;
        public bool IsCarryPanelVisible => carryPanel != null && carryPanel.gameObject.activeSelf;
        public bool IsIntroPanelVisible => introPanel != null && introPanel.gameObject.activeSelf;
        public bool IsGameplayHudVisible => gameplayGroup != null && gameplayGroup.alpha > 0.5f;
        public bool SupportsReferenceResolutions =>
            ValidateLayout(1280, 720) && ValidateLayout(1920, 1080) &&
            ValidateLayout(2560, 1440) && ValidateLayout(1920, 1200) &&
            ValidateLayout(2560, 1080);

        public void Configure(SectionTransitionCoordinator sectionCoordinator,
            GameplayHud gameplayHud, Image loopTimerFill,
            RectTransform status, RectTransform intro, RectTransform prompt, RectTransform carry,
            PauseMenuController menu, CanvasGroup group)
        {
            coordinator = sectionCoordinator;
            hud = gameplayHud;
            timerFill = loopTimerFill;
            statusPanel = status;
            introPanel = intro;
            promptPanel = prompt;
            carryPanel = carry;
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
            SetActive(promptPanel, !paused && !string.IsNullOrEmpty(hud.CurrentPrompt));
            SetActive(carryPanel, !paused && hud.IsCarrying);
            SetActive(introPanel, !paused &&
                (hud.IsSectionIntroVisible || hud.IsTutorialVisible));
        }

        public bool ValidateLayout(int width, int height)
        {
            if (!HasRequiredReferences || width < 960 || height < 540) return false;
            float scale = Mathf.Min(width / 1920f, height / 1080f);
            float horizontalMargin = 20f * scale;
            float statusWidth = statusPanel.rect.width * scale;
            float promptWidth = promptPanel.rect.width * scale;
            float introWidth = introPanel.rect.width * scale;
            return statusWidth <= width * 0.34f &&
                promptWidth <= width * 0.28f &&
                introWidth <= width - horizontalMargin * 2f &&
                statusPanel.rect.height * scale <= height * 0.22f &&
                carryPanel.rect.height * scale <= height * 0.09f;
        }

        private static void SetActive(RectTransform target, bool active)
        {
            if (target != null && target.gameObject.activeSelf != active)
                target.gameObject.SetActive(active);
        }
    }
}
