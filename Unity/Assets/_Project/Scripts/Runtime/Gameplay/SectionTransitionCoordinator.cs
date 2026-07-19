using System;
using System.Collections;
using EchoShift.Core;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using EchoShift.Telemetry;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoShift.Gameplay
{
    public sealed class SectionTransitionCoordinator : MonoBehaviour
    {
        [SerializeField] private PuzzleSectionController[] sections =
            Array.Empty<PuzzleSectionController>();
        [SerializeField] private SectionCameraController sectionCamera;
        [SerializeField] private GameplayHud hud;
        [SerializeField] private PauseMenuController pauseMenu;
        [SerializeField] private PlaytestTelemetry telemetry;
        [SerializeField, Min(0f)] private float loopTransitionSeconds = 0.75f;
        [SerializeField, Min(0f)] private float sectionTransitionSeconds = 1f;

        private readonly GameplayStateController _state = new GameplayStateController();
        private int _activeSectionIndex;
        private float _transitionRemaining;
        private bool _quitRequested;
        private bool _started;

        public GameplayState State => _state.State;
        public string LastTransitionReason => _state.LastReason;
        public PuzzleSectionController ActiveSection =>
            sections.Length > 0 ? sections[_activeSectionIndex] : null;
        public int ActiveSectionNumber => _activeSectionIndex + 1;
        public bool QuitRequested => _quitRequested;
        public PlaytestTelemetry Telemetry => telemetry;
        public GameplayHud Hud => hud;
        public bool HasValidReferences => sections.Length == 3 &&
            sectionCamera != null && hud != null && pauseMenu != null && telemetry != null;

        public void Configure(
            PuzzleSectionController[] puzzleSections,
            SectionCameraController cameraController,
            GameplayHud gameplayHud,
            PauseMenuController menu,
            PlaytestTelemetry playtestTelemetry,
            float loopSeconds = 0.75f,
            float sectionSeconds = 1f)
        {
            sections = puzzleSections ?? Array.Empty<PuzzleSectionController>();
            sectionCamera = cameraController;
            hud = gameplayHud;
            pauseMenu = menu;
            telemetry = playtestTelemetry;
            loopTransitionSeconds = Mathf.Max(0f, loopSeconds);
            sectionTransitionSeconds = Mathf.Max(0f, sectionSeconds);
        }

        private void Start()
        {
            InitializeGame();
            if (HasCommandLineFlag("-phase3AutoQuit"))
            {
                StartCoroutine(AutoQuitProbe());
            }
        }

        private void Update()
        {
            HandlePauseInput();
            if (State == GameplayState.LoopTransition ||
                State == GameplayState.SectionTransition)
            {
                _transitionRemaining -= Time.unscaledDeltaTime;
                if (_transitionRemaining <= 0f)
                {
                    FinishPendingTransition();
                }
                return;
            }

            if (State != GameplayState.Playing)
            {
                return;
            }

            ActiveSection.RefreshCompletionSensor();
            if (ActiveSection.Goal.IsReached)
            {
                BeginSectionCompletion();
            }
        }

        public void InitializeGame()
        {
            if (_started)
            {
                return;
            }
            if (!HasValidReferences)
            {
                Debug.LogError("Phase 3 coordinator has missing references.", this);
                enabled = false;
                return;
            }

            for (int i = 0; i < sections.Length; i++)
            {
                if (i != 0)
                {
                    sections[i].gameObject.SetActive(false);
                }
            }
            _activeSectionIndex = 0;
            ActivateCurrentSection();
            _state.TryTransition(GameplayState.Playing, "BootComplete");
            hud.SetStateMessage(string.Empty);
            _started = true;
            Debug.Log("PHASE3_STATE Playing reason=BootComplete section=1", this);
        }

        public bool BeginSectionCompletion()
        {
            if (State != GameplayState.Playing ||
                !_state.TryTransition(GameplayState.SectionTransition, "SectionCompleted"))
            {
                return false;
            }

            ActiveSection.Director.SetSimulationPaused(true);
            telemetry.SectionCompleted(ActiveSectionNumber, ActiveSection.Director.LoopNumber);
            telemetry.SetMaximumDrift(ActiveSection.Director.MaximumReplayDrift);
            _transitionRemaining = sectionTransitionSeconds;
            hud.SetStateMessage(_activeSectionIndex == sections.Length - 1
                ? hud.TextCatalog.GameCompleted
                : hud.TextCatalog.SectionCompleted);
            Debug.Log($"PHASE3_STATE SectionTransition reason=SectionCompleted " +
                      $"section={ActiveSectionNumber}", this);
            if (_transitionRemaining <= 0f)
            {
                FinishPendingTransition();
            }
            return true;
        }

        public bool SetPaused(bool paused)
        {
            if (paused)
            {
                if (State != GameplayState.Playing ||
                    !_state.TryTransition(GameplayState.Paused, "PauseRequested"))
                {
                    return false;
                }
                ActiveSection.Director.SetSimulationPaused(true);
                pauseMenu.SetVisible(true);
                hud.SetStateMessage(hud.TextCatalog.Paused);
                return true;
            }

            if (State != GameplayState.Paused ||
                !_state.TryTransition(GameplayState.Playing, "ResumeRequested"))
            {
                return false;
            }
            ActiveSection.Director.SetSimulationPaused(false);
            pauseMenu.SetVisible(false);
            hud.SetStateMessage(string.Empty);
            return true;
        }

        public void RestartSection()
        {
            if (State == GameplayState.Completed || State == GameplayState.SectionTransition)
            {
                return;
            }
            telemetry.RecordRestartSection();
            ActiveSection.RestartSection(false);
            if (State == GameplayState.Paused)
            {
                _state.TryTransition(GameplayState.Playing, "RestartSection");
            }
            pauseMenu.SetVisible(false);
            hud.SetStateMessage(hud.TextCatalog.SectionRestarted);
        }

        public void RestartGame()
        {
            telemetry.RecordRestartGame();
            telemetry.FinishAndSave(PlaytestOutcome.Quit);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        public void RestartGameInPlaceForTests()
        {
            ActiveSection.DeactivateSection();
            _state.Reset();
            _activeSectionIndex = 0;
            sections[0].gameObject.SetActive(true);
            sections[0].ActivateSection(false);
            sections[0].RestartSection(false);
            Subscribe(sections[0]);
            sectionCamera.SetTarget(sections[0].Player.transform, true);
            telemetry.RecordRestartGame();
            telemetry.SectionStarted(1);
            _state.TryTransition(GameplayState.Playing, "RestartGameTest");
            hud.Bind(this);
        }

        public void RequestQuit()
        {
            RequestQuitCore(true);
        }

        public void RequestQuitForTests()
        {
            RequestQuitCore(false);
        }

        private void RequestQuitCore(bool quitApplication)
        {
            if (_quitRequested)
            {
                return;
            }
            _quitRequested = true;
            ActiveSection?.Director.SetSimulationPaused(true);
            telemetry.FinishAndSave(PlaytestOutcome.Quit);
            Debug.Log($"PHASE3_QUIT_REQUESTED telemetry={telemetry.LastSavedPath}", this);
            if (quitApplication)
            {
                Application.Quit(0);
            }
        }

        public void CompleteTransitionNowForTests()
        {
            _transitionRemaining = 0f;
            FinishPendingTransition();
        }

        public void SetTransitionDurationsForTests(float loopSeconds, float sectionSeconds)
        {
            loopTransitionSeconds = Mathf.Max(0f, loopSeconds);
            sectionTransitionSeconds = Mathf.Max(0f, sectionSeconds);
        }

        public void EvaluateActiveSectionForTests()
        {
            if (State != GameplayState.Playing)
            {
                return;
            }
            ActiveSection.RefreshCompletionSensor();
            if (ActiveSection.Goal.IsReached)
            {
                BeginSectionCompletion();
            }
        }

        private void ActivateCurrentSection()
        {
            PuzzleSectionController section = sections[_activeSectionIndex];
            section.ActivateSection(false);
            Subscribe(section);
            sectionCamera.SetTarget(section.Player.transform, _activeSectionIndex == 0);
            telemetry.SectionStarted(ActiveSectionNumber);
            hud.Bind(this);
            hud.SetStateMessage(hud.TextCatalog.GetSectionObjective(_activeSectionIndex));
        }

        private void Subscribe(PuzzleSectionController section)
        {
            section.Director.LoopCompleted -= OnLoopCompleted;
            section.Director.LoopCompleted += OnLoopCompleted;
            section.Director.InteractionResolved -= OnInteractionResolved;
            section.Director.InteractionResolved += OnInteractionResolved;
        }

        private void Unsubscribe(PuzzleSectionController section)
        {
            section.Director.LoopCompleted -= OnLoopCompleted;
            section.Director.InteractionResolved -= OnInteractionResolved;
        }

        private void OnLoopCompleted(LoopHistorySummary summary)
        {
            telemetry.RecordLoop(summary);
            if (State != GameplayState.Playing ||
                !_state.TryTransition(GameplayState.LoopTransition, "LoopRecorded"))
            {
                return;
            }
            ActiveSection.Director.SetSimulationPaused(true);
            _transitionRemaining = loopTransitionSeconds;
            hud.SetStateMessage($"{hud.TextCatalog.LoopRecorded}  •  " +
                                $"ECHO {summary.ReplayGeneration}");
            if (_transitionRemaining <= 0f)
            {
                FinishPendingTransition();
            }
        }

        private void OnInteractionResolved(
            InteractionExecution execution,
            EchoShift.Player.LoopActor actor)
        {
            telemetry.RecordInteraction(execution);
            if (!execution.Succeeded)
            {
                hud.ShowInteractionFailure(execution.FailureReason);
            }
        }

        private void FinishPendingTransition()
        {
            if (State == GameplayState.LoopTransition)
            {
                if (_state.TryTransition(GameplayState.Playing, "LoopTransitionComplete"))
                {
                    ActiveSection.Director.SetSimulationPaused(false);
                    hud.SetStateMessage(string.Empty);
                }
                return;
            }

            if (State != GameplayState.SectionTransition)
            {
                return;
            }

            PuzzleSectionController previous = ActiveSection;
            Unsubscribe(previous);
            previous.DeactivateSection();
            if (_activeSectionIndex >= sections.Length - 1)
            {
                _state.TryTransition(GameplayState.Completed, "AllSectionsCompleted");
                telemetry.FinishAndSave(PlaytestOutcome.Completed);
                hud.SetStateMessage(hud.TextCatalog.GameCompleted);
                Debug.Log($"PHASE3_COMPLETED telemetry={telemetry.LastSavedPath}", this);
                return;
            }

            _activeSectionIndex++;
            ActivateCurrentSection();
            _state.TryTransition(GameplayState.Playing, "SectionTransitionComplete");
            Debug.Log($"PHASE3_STATE Playing reason=SectionTransitionComplete " +
                      $"section={ActiveSectionNumber}", this);
        }

        private void HandlePauseInput()
        {
            bool requested = Keyboard.current != null &&
                             Keyboard.current.escapeKey.wasPressedThisFrame;
            requested |= Gamepad.current != null &&
                         Gamepad.current.selectButton.wasPressedThisFrame;
            if (!requested)
            {
                return;
            }
            SetPaused(State != GameplayState.Paused);
        }

        private IEnumerator AutoQuitProbe()
        {
            yield return null;
            yield return null;
            Debug.Log($"PHASE3_PROBE_OK state={State};section={ActiveSectionNumber};" +
                      $"hud={hud.HasValidReferences};telemetry={telemetry.SaveDirectory}", this);
            RequestQuit();
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
