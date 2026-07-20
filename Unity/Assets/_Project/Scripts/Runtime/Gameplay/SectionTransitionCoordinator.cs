using System;
using System.Collections;
using EchoShift.Core;
using EchoShift.Input;
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
        [SerializeField] private bool waitForInteractiveStart = true;

        private readonly GameplayStateController _state = new GameplayStateController();
        private int _activeSectionIndex;
        private float _transitionRemaining;
        private bool _quitRequested;
        private bool _started;
        private bool _awaitingInteractiveStart;
        private GameplayState _pausedFromState = GameplayState.Playing;

        public GameplayState State => _state.State;
        public string LastTransitionReason => _state.LastReason;
        public PuzzleSectionController ActiveSection =>
            sections.Length > 0 ? sections[_activeSectionIndex] : null;
        public int ActiveSectionNumber => _activeSectionIndex + 1;
        public bool QuitRequested => _quitRequested;
        public PlaytestTelemetry Telemetry => telemetry;
        public GameplayHud Hud => hud;
        public PauseMenuController PauseMenu => pauseMenu;
        public bool IsAwaitingInteractiveStart => _awaitingInteractiveStart;
        public bool WaitForInteractiveStart => waitForInteractiveStart;
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
            if (_awaitingInteractiveStart)
            {
                if (HasInteractiveStartInput())
                {
                    ConfirmInteractiveStartForTests();
                }
                return;
            }

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
            if (waitForInteractiveStart && !Application.isBatchMode &&
                !HasCommandLineFlag("-phase3AutoQuit"))
            {
                ArmInteractiveStartForTests();
            }
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
                GameplayState origin = State;
                if ((origin != GameplayState.Playing &&
                     origin != GameplayState.Completed) ||
                    !_state.TryTransition(GameplayState.Paused, "PauseRequested"))
                {
                    return false;
                }
                _pausedFromState = origin;
                ActiveSection?.Director.SetSimulationPaused(true);
                pauseMenu.SetCompletionMode(origin == GameplayState.Completed);
                pauseMenu.SetVisible(true);
                hud.SetStateMessage(hud.TextCatalog.Paused);
                return true;
            }

            GameplayState resumeState = _pausedFromState == GameplayState.Completed
                ? GameplayState.Completed
                : GameplayState.Playing;
            if (State != GameplayState.Paused ||
                !_state.TryTransition(resumeState, "ResumeRequested"))
            {
                return false;
            }
            ActiveSection?.Director.SetSimulationPaused(
                resumeState != GameplayState.Playing);
            pauseMenu.SetVisible(false);
            hud.SetStateMessage(resumeState == GameplayState.Completed
                ? hud.TextCatalog.FormatGameCompleted()
                : string.Empty);
            return true;
        }

        public void RestartSection()
        {
            if (State == GameplayState.Completed ||
                State == GameplayState.SectionTransition ||
                (State == GameplayState.Paused &&
                 _pausedFromState == GameplayState.Completed))
            {
                return;
            }
            telemetry.RecordRestartSection();
            ActiveSection.RestartSection(false);
            ActiveSection.GetComponent<TutorialGuide>()?.ResetForSection();
            if (State == GameplayState.Paused)
            {
                _state.TryTransition(GameplayState.Playing, "RestartSection");
            }
            pauseMenu.SetVisible(false);
            hud.SetStateMessage(hud.TextCatalog.SectionRestarted);
            hud.SetTutorialMessage(hud.TextCatalog.GetInitialTutorial(_activeSectionIndex));
        }

        public void RestartGame()
        {
            telemetry.RecordRestartGame();
            telemetry.FinishAndSave(PlaytestOutcome.Quit);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        }

        public void RestartGameInPlaceForTests()
        {
            _awaitingInteractiveStart = false;
            _pausedFromState = GameplayState.Playing;
            ActiveSection.DeactivateSection();
            _state.Reset();
            _activeSectionIndex = 0;
            sections[0].gameObject.SetActive(true);
            sections[0].ActivateSection(false);
            sections[0].RestartSection(false);
            sections[0].GetComponent<TutorialGuide>()?.ResetForSection();
            Subscribe(sections[0]);
            sectionCamera.SetTarget(sections[0].Player.transform, true);
            telemetry.RecordRestartGame();
            telemetry.SectionStarted(1);
            _state.TryTransition(GameplayState.Playing, "RestartGameTest");
            hud.Bind(this);
        }

        public bool ArmInteractiveStartForTests()
        {
            if (_awaitingInteractiveStart || State != GameplayState.Playing ||
                ActiveSection == null)
            {
                return false;
            }

            _awaitingInteractiveStart = true;
            ActiveSection.Director.SetSimulationPaused(true);
            hud.SetStateMessage(hud.TextCatalog.StartPrompt);
            Debug.Log("PHASE3_INTERACTIVE_START waiting=true", this);
            return true;
        }

        public bool ConfirmInteractiveStartForTests()
        {
            if (!_awaitingInteractiveStart || State != GameplayState.Playing ||
                ActiveSection == null)
            {
                return false;
            }

            _awaitingInteractiveStart = false;
            ActiveSection.Player.GetComponent<InputSystemInputSource>()?
                .ClearPendingButtons();
            ActiveSection.Director.SetSimulationPaused(false);
            hud.SetStateMessage(string.Empty);
            Debug.Log("PHASE3_INTERACTIVE_START waiting=false", this);
            return true;
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
            hud.SetObjective(hud.TextCatalog.GetSectionObjective(_activeSectionIndex));
            TutorialGuide guide = section.GetComponent<TutorialGuide>();
            if (guide != null) guide.ResetForSection();
            else hud.SetTutorialMessage(hud.TextCatalog.GetInitialTutorial(_activeSectionIndex));
            hud.SetStateMessage(string.Empty);
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
            AdvanceTutorialAfterLoop(summary);
            if (State != GameplayState.Playing ||
                !_state.TryTransition(GameplayState.LoopTransition, "LoopRecorded"))
            {
                return;
            }
            ActiveSection.Director.SetSimulationPaused(true);
            _transitionRemaining = loopTransitionSeconds;
            hud.SetStateMessage(hud.TextCatalog.FormatLoopTransition(summary.ReplayGeneration));
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
                return;
            }

            if (ActiveSectionNumber != 2 ||
                actor == null || actor.Kind != EchoShift.Player.LoopActorKind.Player)
            {
                return;
            }

            TutorialGuide guide = ActiveSection.GetComponent<TutorialGuide>();
            if (execution.Command.Kind == InteractionKind.PickupBattery)
            {
                guide?.ShowStep(1, hud.TextCatalog.GetTutorialText(1, 1));
            }
            else if (execution.Command.Kind == InteractionKind.InsertBattery)
            {
                guide?.ShowStep(2, hud.TextCatalog.GetTutorialText(1, 2));
            }
        }

        private void AdvanceTutorialAfterLoop(LoopHistorySummary summary)
        {
            TutorialGuide guide = ActiveSection.GetComponent<TutorialGuide>();
            if (guide == null) return;

            if (ActiveSectionNumber == 1)
            {
                if (!ActiveSection.Director.LastCompletedPressurePlateWasPressed)
                {
                    guide.ShowMessage(hud.TextCatalog.GetTutorialText(0, 1));
                    return;
                }

                guide.ShowStep(2,
                    $"{hud.TextCatalog.GetTutorialText(0, 2)}\n" +
                    hud.TextCatalog.GetTutorialText(0, 3));
                return;
            }

            if (ActiveSectionNumber != 2 || summary.InteractionEventCount <= 0)
            {
                return;
            }

            if (summary.InteractionEventCount >= 2 && guide.Progress.CurrentStep >= 3)
            {
                guide.ShowStep(3, hud.TextCatalog.GetTutorialText(1, 3));
            }
            else
            {
                guide.ShowMessage(
                    $"{hud.TextCatalog.GetTutorialText(1, 1)}\n" +
                    hud.TextCatalog.GetTutorialText(1, 2));
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
                hud.SetStateMessage(hud.TextCatalog.FormatGameCompleted());
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

        private static bool HasInteractiveStartInput()
        {
            bool keyboard = Keyboard.current != null &&
                            Keyboard.current.anyKey.wasPressedThisFrame;
            bool mouse = Mouse.current != null &&
                         (Mouse.current.leftButton.wasPressedThisFrame ||
                          Mouse.current.rightButton.wasPressedThisFrame ||
                          Mouse.current.middleButton.wasPressedThisFrame);
            Gamepad currentGamepad = Gamepad.current;
            bool gamepad = currentGamepad != null &&
                           (currentGamepad.leftStick.ReadValue().sqrMagnitude >= 0.04f ||
                            currentGamepad.dpad.ReadValue().sqrMagnitude >= 0.25f ||
                            currentGamepad.buttonSouth.wasPressedThisFrame ||
                            currentGamepad.buttonNorth.wasPressedThisFrame ||
                            currentGamepad.buttonEast.wasPressedThisFrame ||
                            currentGamepad.buttonWest.wasPressedThisFrame ||
                            currentGamepad.startButton.wasPressedThisFrame ||
                            currentGamepad.selectButton.wasPressedThisFrame ||
                            currentGamepad.leftShoulder.wasPressedThisFrame ||
                            currentGamepad.rightShoulder.wasPressedThisFrame ||
                            currentGamepad.leftStickButton.wasPressedThisFrame ||
                            currentGamepad.rightStickButton.wasPressedThisFrame);
            return keyboard || mouse || gamepad;
        }

        private IEnumerator AutoQuitProbe()
        {
            yield return null;
            yield return null;
            Debug.Log($"PHASE3_PROBE_OK state={State};section={ActiveSectionNumber};" +
                      $"hud={hud.HasValidReferences};language={hud.TextCatalog.LanguageCode};" +
                      $"font={hud.JapaneseFontName};glyphs={hud.IsJapaneseReady};" +
                      $"telemetry={telemetry.SaveDirectory}", this);
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
