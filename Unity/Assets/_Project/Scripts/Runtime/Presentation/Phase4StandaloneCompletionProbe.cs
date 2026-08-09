using System;
using System.Collections;
using System.IO;
using EchoShift.Core;
using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction.Recorded;
using EchoShift.Replay;
using EchoShift.Telemetry;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4StandaloneCompletionProbe : MonoBehaviour
    {
        private const string CommandLineFlag = "-phase4AutoCompleteProbe";
        [SerializeField] private SectionTransitionCoordinator coordinator;

        public void Configure(SectionTransitionCoordinator sectionCoordinator) =>
            coordinator = sectionCoordinator;

        private void Start()
        {
            if (!HasFlag(CommandLineFlag))
            {
                enabled = false;
                return;
            }
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return null;
            yield return null;
            if (coordinator == null)
            {
                Fail("MissingCoordinator");
                yield break;
            }

            coordinator.ConfirmInteractiveStartForTests();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController[] sections = FindObjectsByType<PuzzleSectionController>(
                FindObjectsInactive.Include);
            Array.Sort(sections, (left, right) =>
                left.SectionNumber.CompareTo(right.SectionNumber));
            if (sections.Length != 3)
            {
                Fail($"SectionCount{sections.Length}");
                yield break;
            }
            PowerSocket[] sockets = new PowerSocket[sections.Length];
            for (int i = 0; i < sections.Length; i++)
            {
                sockets[i] = sections[i].GetComponentInChildren<PowerSocket>(true);
                Phase4StandaloneRouteInputSource route =
                    sections[i].Player.gameObject.AddComponent<Phase4StandaloneRouteInputSource>();
                route.Configure(i + 1, sections[i].Director);
                sections[i].Player.Configure(
                    route, sections[i].Player.Motor, sections[i].Player.Interactor);
            }
            // Install deterministic input before resetting to tick zero. A D3D11
            // startup frame can otherwise advance past the route's opening input
            // window before this probe coroutine begins.
            coordinator.RestartGameInPlaceForTests();

            int advances = 0;
            int maximumEchoes = 0;
            bool section2Recorded = false;
            bool section3Recorded = false;
            while (coordinator.State != GameplayState.Completed && advances++ < 4000)
            {
                if (coordinator.State == GameplayState.Playing)
                {
                    PuzzleSectionController active = coordinator.ActiveSection;
                    int activeSectionNumber = active.SectionNumber;
                    maximumEchoes = Mathf.Max(maximumEchoes, active.Director.EchoCount);
                    active.Director.AdvanceOneTickForTests();
                    coordinator.EvaluateActiveSectionForTests();
                    PowerSocket socket = sockets[activeSectionNumber - 1];
                    if (socket != null && socket.RequestsDoorOpen && socket.InsertedByReplay)
                    {
                        if (activeSectionNumber == 2) section2Recorded = true;
                        else if (activeSectionNumber == 3) section3Recorded = true;
                    }
                }
                else
                {
                    coordinator.CompleteTransitionNowForTests();
                }

                if (advances % 10 == 0) yield return new WaitForFixedUpdate();
            }

            PlaytestTelemetrySnapshot snapshot = coordinator.Telemetry.Snapshot();
            bool valid = coordinator.State == GameplayState.Completed &&
                maximumEchoes >= 2 && section2Recorded && section3Recorded &&
                snapshot.InteractionSuccessCount == 4 && snapshot.InteractionFailureCount == 0 &&
                snapshot.MaximumDrift <= 0.05f &&
                !string.IsNullOrEmpty(coordinator.Telemetry.LastSavedPath) &&
                File.Exists(coordinator.Telemetry.LastSavedPath);
            if (!valid)
            {
                Fail($"state={coordinator.State};advances={advances};echoes={maximumEchoes};" +
                     $"recorded2={section2Recorded};recorded3={section3Recorded};" +
                     $"success={snapshot.InteractionSuccessCount};" +
                     $"failure={snapshot.InteractionFailureCount};drift={snapshot.MaximumDrift:R}");
                yield break;
            }

            Debug.Log($"PHASE4_3_STANDALONE_COMPLETE_OK advances={advances};" +
                      $"interactionSuccess={snapshot.InteractionSuccessCount};" +
                      $"interactionFailure={snapshot.InteractionFailureCount};" +
                      $"drift={snapshot.MaximumDrift:R};" +
                      $"telemetry={coordinator.Telemetry.LastSavedPath}", this);
            Application.Quit(0);
        }

        private void Fail(string reason)
        {
            Debug.LogError($"PHASE4_3_STANDALONE_COMPLETE_FAILED reason={reason}", this);
            Application.Quit(2);
        }

        private static bool HasFlag(string value)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
                if (string.Equals(arguments[i], value,
                    StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }

    public sealed class Phase4StandaloneRouteInputSource : MonoBehaviour, IInputSource
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
}
