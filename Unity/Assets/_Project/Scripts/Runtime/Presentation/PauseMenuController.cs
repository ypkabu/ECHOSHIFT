using EchoShift.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private SectionTransitionCoordinator coordinator;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartSectionButton;
        [SerializeField] private Button restartGameButton;
        [SerializeField] private Button quitButton;

        public bool IsVisible => panel != null && panel.activeSelf;

        public void Configure(
            SectionTransitionCoordinator stateCoordinator, GameObject menuPanel,
            Button resume, Button restartSection, Button restartGame, Button quit)
        {
            coordinator = stateCoordinator;
            panel = menuPanel;
            resumeButton = resume;
            restartSectionButton = restartSection;
            restartGameButton = restartGame;
            quitButton = quit;
        }

        private void Start()
        {
            resumeButton.onClick.AddListener(() => coordinator.SetPaused(false));
            restartSectionButton.onClick.AddListener(coordinator.RestartSection);
            restartGameButton.onClick.AddListener(coordinator.RestartGame);
            quitButton.onClick.AddListener(coordinator.RequestQuit);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            panel?.SetActive(visible);
        }
    }
}
