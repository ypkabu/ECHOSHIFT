using System;
using System.Collections;
using System.IO;
using System.Linq;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using EchoShift.Replay;
using EchoShift.Telemetry;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests
{
    public sealed class Phase3GreyboxPlayModeTests
    {
        [UnityTest]
        public IEnumerator Phase3RealSceneAutomaticallyCompletesAllThreeSections()
        {
            yield return Load("P3_PlayableGreybox");
            SectionTransitionCoordinator coordinator =
                UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController[] sections = GetSections();
            ConfigureRoutes(coordinator, sections);
            int advances = 0;
            int maximumEchoes = 0;
            bool section2OpenedByRecordedInsertion = false;
            bool section3OpenedByRecordedInsertion = false;
            while (coordinator.State != GameplayState.Completed && advances++ < 4000)
            {
                if (coordinator.State == GameplayState.Playing)
                {
                    maximumEchoes = Mathf.Max(maximumEchoes,
                        coordinator.ActiveSection.Director.EchoCount);
                    coordinator.ActiveSection.Director.AdvanceOneTickForTests();
                    coordinator.EvaluateActiveSectionForTests();
                    PowerSocket activeSocket = coordinator.ActiveSection
                        .GetComponentInChildren<PowerSocket>();
                    if (activeSocket != null && activeSocket.RequestsDoorOpen)
                    {
                        Assert.That(activeSocket.InsertedByReplay, Is.True,
                            "A P3 powered Door must be opened by a recorded Echo insertion.");
                        if (coordinator.ActiveSectionNumber == 2)
                            section2OpenedByRecordedInsertion = true;
                        else if (coordinator.ActiveSectionNumber == 3)
                            section3OpenedByRecordedInsertion = true;
                    }
                }
                else
                {
                    coordinator.CompleteTransitionNowForTests();
                }

                // Let trigger callbacks and door animation observe the deterministic
                // tick batches; the standalone flow is not a single-frame simulation.
                if (advances % 10 == 0)
                {
                    yield return new WaitForFixedUpdate();
                }
            }

            Debug.Log($"PHASE3_ROUTE_RESULT state={coordinator.State};advances={advances};" +
                      $"section={coordinator.ActiveSectionNumber};loop={coordinator.ActiveSection.Director.LoopNumber};" +
                      $"tick={coordinator.ActiveSection.Director.CurrentTick};" +
                      $"player={coordinator.ActiveSection.Player.transform.position};" +
                      $"goal={coordinator.ActiveSection.Goal.IsReached}");

            Assert.That(coordinator.State, Is.EqualTo(GameplayState.Completed));
            Assert.That(maximumEchoes, Is.GreaterThanOrEqualTo(2));
            Assert.That(sections[0].Director.EchoCount, Is.Zero);
            Assert.That(sections[1].Director.EchoCount, Is.Zero);
            Assert.That(sections[2].Director.EchoCount, Is.Zero);
            Assert.That(sections[0].gameObject.activeSelf, Is.False);
            Assert.That(sections[1].gameObject.activeSelf, Is.False);
            Assert.That(sections[2].gameObject.activeSelf, Is.False);
            PlaytestTelemetrySnapshot snapshot = coordinator.Telemetry.Snapshot();
            Assert.That(snapshot.FinalSection, Is.EqualTo(3));
            Assert.That(snapshot.Outcome, Is.EqualTo(PlaytestOutcome.Completed));
            Assert.That(snapshot.InteractionFailureCount, Is.Zero);
            Assert.That(snapshot.InteractionSuccessCount, Is.EqualTo(4));
            Assert.That(section2OpenedByRecordedInsertion, Is.True);
            Assert.That(section3OpenedByRecordedInsertion, Is.True);
            Assert.That(snapshot.MaximumDrift, Is.LessThanOrEqualTo(0.05f));
            Assert.That(File.Exists(coordinator.Telemetry.LastSavedPath), Is.True);
            Assert.That(coordinator.Telemetry.GenerateJson(), Does.Not.Contain("ReplayFrame"));
            AssertNoMissingComponents();
            Debug.Log($"PHASE3_INTEGRATION_MAX_DRIFT={snapshot.MaximumDrift:R};" +
                      $"INTERACTION_SUCCESS={snapshot.InteractionSuccessCount};" +
                      $"INTERACTION_FAILURE={snapshot.InteractionFailureCount};" +
                      $"ADVANCES={advances};TELEMETRY={coordinator.Telemetry.LastSavedPath}");
        }

        [UnityTest]
        public IEnumerator PauseFreezesPlayerEchoDoorAndTimerThenResumesSameTick()
        {
            yield return Load("P3_PlayableGreybox");
            SectionTransitionCoordinator coordinator =
                UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>();
            PuzzleSectionController section = coordinator.ActiveSection;
            ConstantMoveInputSource input = section.Player.gameObject.AddComponent<ConstantMoveInputSource>();
            section.Player.Configure(input, section.Player.Motor, section.Player.Interactor);
            int tick = section.Director.CurrentTick;
            Vector3 position = section.Player.transform.position;
            DoorController door = UnityEngine.Object.FindObjectsByType<DoorController>()
                .First(value => value.gameObject.activeInHierarchy);
            Vector3 doorPosition = door.transform.position;
            Assert.That(coordinator.SetPaused(true), Is.True);
            for (int i = 0; i < 10; i++) section.Director.AdvanceOneTickForTests();
            Assert.That(section.Director.CurrentTick, Is.EqualTo(tick));
            Assert.That(section.Player.transform.position, Is.EqualTo(position));
            Assert.That(door.transform.position, Is.EqualTo(doorPosition));
            Assert.That(coordinator.SetPaused(false), Is.True);
            section.Director.AdvanceOneTickForTests();
            Assert.That(section.Director.CurrentTick, Is.EqualTo(tick + 1));
            Assert.That(section.Player.transform.position.z, Is.GreaterThan(position.z));
        }

        [UnityTest]
        public IEnumerator RestartSectionAndRestartGameClearReplayAtCorrectScope()
        {
            yield return Load("P3_PlayableGreybox");
            SectionTransitionCoordinator coordinator =
                UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController first = coordinator.ActiveSection;
            AlwaysEndLoopInputSource input = first.Player.gameObject.AddComponent<AlwaysEndLoopInputSource>();
            first.Player.Configure(input, first.Player.Motor, first.Player.Interactor);
            AdvanceUntilLoop(first.Director, 2, 10);
            Assert.That(first.Director.EchoCount, Is.EqualTo(1));
            coordinator.RestartSection();
            Assert.That(coordinator.ActiveSectionNumber, Is.EqualTo(1));
            Assert.That(first.Director.EchoCount, Is.Zero);
            Assert.That(first.Director.LoopNumber, Is.EqualTo(1));
            Assert.That(first.Director.CurrentTick, Is.Zero);
            coordinator.BeginSectionCompletion();
            coordinator.CompleteTransitionNowForTests();
            Assert.That(coordinator.ActiveSectionNumber, Is.EqualTo(2));
            coordinator.RestartGameInPlaceForTests();
            Assert.That(coordinator.ActiveSectionNumber, Is.EqualTo(1));
            Assert.That(coordinator.State, Is.EqualTo(GameplayState.Playing));
        }

        [UnityTest]
        public IEnumerator HudSwitchesPromptShowsFailureAndDebugOverlayCanHide()
        {
            yield return Load("P3_PlayableGreybox");
            SectionTransitionCoordinator coordinator =
                UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>();
            GameplayHud hud = coordinator.Hud;
            Assert.That(hud.ResolvePrompt(InputPromptDevice.Keyboard, true),
                Is.EqualTo(hud.TextCatalog.InteractKeyboard));
            Assert.That(hud.ResolvePrompt(InputPromptDevice.Gamepad, true),
                Is.EqualTo(hud.TextCatalog.InteractGamepad));
            hud.ShowInteractionFailure(InteractionFailureReason.TargetBusy);
            Assert.That(hud.LastFailureText, Is.EqualTo("ほかのエコーが使用中です"));
            Phase0DebugOverlay overlay = UnityEngine.Object.FindObjectsByType<Phase0DebugOverlay>()
                .First(value => value.gameObject.activeInHierarchy);
            overlay.SetVisible(false);
            Assert.That(overlay.IsVisible, Is.False);
            int tick = coordinator.ActiveSection.Director.CurrentTick;
            coordinator.ActiveSection.Director.AdvanceOneTickForTests();
            Assert.That(coordinator.ActiveSection.Director.CurrentTick, Is.EqualTo(tick + 1));
        }

        [UnityTest]
        public IEnumerator QuitRequestWritesTelemetryAndRaisesStandaloneExitIntent()
        {
            yield return Load("P3_PlayableGreybox");
            SectionTransitionCoordinator coordinator =
                UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>();
            coordinator.RequestQuitForTests();
            Assert.That(coordinator.QuitRequested, Is.True);
            Assert.That(coordinator.Telemetry.Outcome, Is.EqualTo(PlaytestOutcome.Quit));
            Assert.That(File.Exists(coordinator.Telemetry.LastSavedPath), Is.True);
        }

        [UnityTest]
        public IEnumerator CompletedStateDoesNotCreateAnotherReplay()
        {
            yield return Load("P3_PlayableGreybox");
            SectionTransitionCoordinator coordinator =
                UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            for (int section = 0; section < 3; section++)
            {
                coordinator.BeginSectionCompletion();
                coordinator.CompleteTransitionNowForTests();
            }
            Assert.That(coordinator.State, Is.EqualTo(GameplayState.Completed));
            LoopDirector director = coordinator.ActiveSection.Director;
            int echoCount = director.EchoCount;
            director.AdvanceOneTickForTests();
            Assert.That(director.EchoCount, Is.EqualTo(echoCount));
            Assert.That(director.IsSimulationPaused, Is.True);
        }

        [UnityTest]
        public IEnumerator P0P1AndP2RealScenesRemainLoadable()
        {
            yield return Load("P0_ReplayLab");
            Assert.That(UnityEngine.Object.FindAnyObjectByType<LoopDirector>().HasValidReferences, Is.True);
            yield return Load("P1_InteractionLab");
            Assert.That(UnityEngine.Object.FindAnyObjectByType<LoopDirector>().HasValidReferences, Is.True);
            yield return Load("P2_CoordinationLab");
            Assert.That(UnityEngine.Object.FindAnyObjectByType<LoopDirector>().HasValidReferences, Is.True);
        }

        private static void ConfigureRoutes(
            SectionTransitionCoordinator coordinator,
            PuzzleSectionController[] sections)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                Phase3RouteInputSource route =
                    sections[i].Player.gameObject.AddComponent<Phase3RouteInputSource>();
                route.Configure(i + 1, sections[i].Director);
                sections[i].Player.Configure(
                    route, sections[i].Player.Motor, sections[i].Player.Interactor);
            }
        }

        private static PuzzleSectionController[] GetSections()
        {
            PuzzleSectionController[] sections =
                UnityEngine.Object.FindObjectsByType<PuzzleSectionController>(FindObjectsInactive.Include);
            Array.Sort(sections, (left, right) => left.SectionNumber.CompareTo(right.SectionNumber));
            return sections;
        }

        private static void AdvanceUntilLoop(LoopDirector director, int target, int limit)
        {
            int count = 0;
            while (director.LoopNumber < target && count++ < limit)
            {
                director.AdvanceOneTickForTests();
            }
            Assert.That(director.LoopNumber, Is.EqualTo(target));
        }

        private static IEnumerator Load(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
        }

        private static void AssertNoMissingComponents()
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                Assert.That(child.GetComponents<Component>(), Has.None.Null,
                    $"Missing component on {child.name}");
        }
    }

    public sealed class Phase3RouteInputSource : MonoBehaviour, IInputSource
    {
        private int _section;
        private LoopDirector _director;

        public void Configure(int section, LoopDirector director)
        {
            _section = section;
            _director = director;
        }

        public InputCommand Sample(int tick)
        {
            int loop = _director.LoopNumber;
            Vector2 move = Vector2.zero;
            InputButtonFlags buttons = InputButtonFlags.None;
            if (_section == 1)
            {
                if (loop == 1)
                {
                    if (tick < 30) move = Vector2.left;
                    else if (tick < 90) move = Vector2.up;
                    if (tick == 100) buttons = InputButtonFlags.EndLoop;
                }
                else move = Vector2.up;
            }
            else if (_section == 2)
            {
                if (loop == 1)
                {
                    if (tick < 45) move = Vector2.up;
                    else if (tick < 60) move = Vector2.right;
                    else if (tick == 61) buttons = InputButtonFlags.Interact;
                    else if (tick < 107) move = Vector2.up;
                    else if (tick == 107) buttons = InputButtonFlags.Interact;
                    if (tick == 120) buttons |= InputButtonFlags.EndLoop;
                }
                else move = Vector2.up;
            }
            else
            {
                if (loop == 1)
                {
                    if (tick < 45) move = Vector2.left;
                    else if (tick < 98) move = Vector2.up;
                    if (tick == 110) buttons = InputButtonFlags.EndLoop;
                }
                else if (loop == 2)
                {
                    if (tick < 155) move = Vector2.up;
                    else if (tick < 175) move = Vector2.right;
                    else if (tick == 175) buttons = InputButtonFlags.Interact;
                    else if (tick < 225) move = Vector2.up;
                    else if (tick == 225) buttons = InputButtonFlags.Interact;
                    if (tick == 240) buttons |= InputButtonFlags.EndLoop;
                }
                else move = Vector2.up;
            }
            return new InputCommand(tick, move, buttons);
        }
    }

    public sealed class ConstantMoveInputSource : MonoBehaviour, IInputSource
    {
        public InputCommand Sample(int tick) =>
            new InputCommand(tick, Vector2.up, InputButtonFlags.None);
    }

    public sealed class AlwaysEndLoopInputSource : MonoBehaviour, IInputSource
    {
        public InputCommand Sample(int tick) =>
            new InputCommand(tick, Vector2.zero, InputButtonFlags.EndLoop);
    }
}
