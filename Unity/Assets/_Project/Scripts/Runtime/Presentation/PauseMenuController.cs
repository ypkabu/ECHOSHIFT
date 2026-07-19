using EchoShift.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        private enum PendingAction : byte
        {
            None,
            RestartSection,
            RestartGame,
            Quit
        }

        [SerializeField] private SectionTransitionCoordinator coordinator;
        [SerializeField] private Phase3TextCatalog catalog;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text confirmationText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartSectionButton;
        [SerializeField] private Button restartGameButton;
        [SerializeField] private Button quitButton;

        private PendingAction _pendingAction;

        public bool IsVisible => panel != null && panel.activeSelf;
        public string TitleLabel => titleText != null ? titleText.text : string.Empty;
        public string ResumeLabel => Label(resumeButton);
        public string RestartSectionLabel => Label(restartSectionButton);
        public string RestartGameLabel => Label(restartGameButton);
        public string QuitLabel => Label(quitButton);
        public string ConfirmationLabel => confirmationText != null
            ? confirmationText.text : string.Empty;

        public void Configure(
            SectionTransitionCoordinator stateCoordinator,
            Phase3TextCatalog textCatalog,
            GameObject menuPanel,
            Text title,
            Text confirmation,
            Button resume,
            Button restartSection,
            Button restartGame,
            Button quit)
        {
            coordinator = stateCoordinator;
            catalog = textCatalog;
            panel = menuPanel;
            titleText = title;
            confirmationText = confirmation;
            resumeButton = resume;
            restartSectionButton = restartSection;
            restartGameButton = restartGame;
            quitButton = quit;
        }

        private void Start()
        {
            titleText.text = catalog.Paused;
            resumeButton.onClick.AddListener(Resume);
            restartSectionButton.onClick.AddListener(RequestRestartSection);
            restartGameButton.onClick.AddListener(RequestRestartGame);
            quitButton.onClick.AddListener(RequestQuit);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (panel == null)
            {
                return;
            }

            if (visible)
            {
                panel.SetActive(true);
                if (EventSystem.current != null && resumeButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
                }
                return;
            }

            ClearConfirmation();
            if (EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(panel.transform))
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            panel.SetActive(false);
        }

        public void Resume()
        {
            ClearConfirmation();
            coordinator.SetPaused(false);
        }

        public void RequestRestartSection()
        {
            if (!Confirm(PendingAction.RestartSection, catalog.ConfirmRestartSection)) return;
            coordinator.RestartSection();
        }

        public void RequestRestartGame()
        {
            if (!Confirm(PendingAction.RestartGame, catalog.ConfirmRestartGame)) return;
            coordinator.RestartGame();
        }

        public void RequestQuit()
        {
            if (!Confirm(PendingAction.Quit, catalog.ConfirmQuit)) return;
            coordinator.RequestQuit();
        }

        private bool Confirm(PendingAction action, string message)
        {
            if (_pendingAction == action)
            {
                ClearConfirmation();
                return true;
            }
            _pendingAction = action;
            confirmationText.text = message;
            return false;
        }

        private void ClearConfirmation()
        {
            _pendingAction = PendingAction.None;
            if (confirmationText != null) confirmationText.text = string.Empty;
        }

        private static string Label(Button button)
        {
            Text label = button != null ? button.GetComponentInChildren<Text>(true) : null;
            return label != null ? label.text : string.Empty;
        }
    }
}
