using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using EchoShift.Core;
using EchoShift.Editor;
using EchoShift.Input;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using EchoShift.Reset;
using EchoShift.Telemetry;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EchoShift.Tests
{
    public sealed class Phase3GreyboxEditModeTests
    {
        [Test]
        public void GameStateAcceptsOnlyDeclaredTransitionOrder()
        {
            GameplayStateController state = new GameplayStateController();
            Assert.That(state.TryTransition(GameplayState.Completed, "invalid"), Is.False);
            Assert.That(state.TryTransition(GameplayState.Playing, "boot"), Is.True);
            Assert.That(state.TryTransition(GameplayState.Paused, "pause"), Is.True);
            Assert.That(state.TryTransition(GameplayState.Playing, "resume"), Is.True);
            Assert.That(state.TryTransition(GameplayState.Completed, "done"), Is.True);
            Assert.That(state.TryTransition(GameplayState.Playing, "invalid"), Is.False);
        }

        [Test]
        public void PausedSimulationClockDoesNotConsumeTicks()
        {
            SimulationClock clock = new SimulationClock(60, 8);
            clock.Resume();
            clock.AddTime(1f / 60f);
            clock.Pause();
            Assert.That(clock.TryConsumeTick(), Is.False);
        }

        [Test]
        public void LoopTransitionCannotBeEnteredTwice()
        {
            GameplayStateController state = PlayingState();
            Assert.That(state.TryTransition(GameplayState.LoopTransition, "recorded"), Is.True);
            int revision = state.Revision;
            Assert.That(state.TryTransition(GameplayState.LoopTransition, "duplicate"), Is.False);
            Assert.That(state.Revision, Is.EqualTo(revision));
        }

        [Test]
        public void SectionTransitionModelRetainsNoReplayReference()
        {
            Type controller = typeof(GameplayStateController);
            Assert.That(controller.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Any(field => field.FieldType.FullName?.Contains("Replay") == true), Is.False);
        }

        [Test]
        public void IndependentResettableCapturesNextSectionBaseline()
        {
            GameObject obj = new GameObject("Section Player");
            TransformResettable reset = obj.AddComponent<TransformResettable>();
            obj.transform.position = new Vector3(30f, 1f, -7f);
            reset.CaptureInitialState();
            obj.transform.position = Vector3.zero;
            reset.RestoreInitialState();
            Assert.That(obj.transform.position, Is.EqualTo(new Vector3(30f, 1f, -7f)));
            UnityEngine.Object.DestroyImmediate(obj);
        }

        [Test]
        public void RestartSectionStateCanReturnToPlayingWithoutResettingCompletedStateModel()
        {
            GameplayStateController state = PlayingState();
            state.TryTransition(GameplayState.Paused, "pause");
            Assert.That(state.TryTransition(GameplayState.Playing, "restart-section"), Is.True);
            Assert.That(state.State, Is.EqualTo(GameplayState.Playing));
        }

        [Test]
        public void RestartGameResetsStateToBooting()
        {
            GameplayStateController state = PlayingState();
            state.Reset();
            Assert.That(state.State, Is.EqualTo(GameplayState.Booting));
            Assert.That(state.LastReason, Is.EqualTo("RestartGame"));
        }

        [Test]
        public void TutorialProgressIsTrackedPerSection()
        {
            TutorialProgress progress = new TutorialProgress();
            progress.BeginSection(0);
            progress.Advance(0);
            progress.BeginSection(1);
            progress.Advance(0);
            progress.Advance(1);
            Assert.That(progress.GetSectionStep(0), Is.EqualTo(1));
            Assert.That(progress.GetSectionStep(1), Is.EqualTo(2));
        }

        [Test]
        public void InputDevicePromptKindCanSwitchDeterministically()
        {
            GameObject obj = new GameObject("Input");
            InputSystemInputSource source = obj.AddComponent<InputSystemInputSource>();
            source.SetPromptDeviceForTests(InputPromptDevice.Gamepad);
            Assert.That(source.LastPromptDevice, Is.EqualTo(InputPromptDevice.Gamepad));
            source.SetPromptDeviceForTests(InputPromptDevice.Keyboard);
            Assert.That(source.LastPromptDevice, Is.EqualTo(InputPromptDevice.Keyboard));
            UnityEngine.Object.DestroyImmediate(obj);
        }

        [Test]
        public void TelemetryAggregatesLoopsInteractionsAndDrift()
        {
            PlaytestTelemetry telemetry = CreateTelemetry();
            telemetry.RecordLoop(new LoopHistorySummary(1, 100, 2, 1, 0.03f, 2, 0,
                ReplayHistoryState.Active, LoopEndReason.Manual));
            telemetry.RecordInteraction(InteractionExecution.Failure(
                new InteractionCommand(1, InteractionKind.PickupBattery, "id", Vector3.zero),
                InteractionFailureReason.TargetBusy));
            PlaytestTelemetrySnapshot snapshot = telemetry.Snapshot();
            Assert.That(snapshot.ManualLoopEnds, Is.EqualTo(1));
            Assert.That(snapshot.InteractionSuccessCount, Is.EqualTo(2));
            Assert.That(snapshot.InteractionFailureCount, Is.EqualTo(1));
            Assert.That(snapshot.GetFailureCount(InteractionFailureReason.TargetBusy), Is.EqualTo(1));
            UnityEngine.Object.DestroyImmediate(telemetry.gameObject);
        }

        [Test]
        public void TelemetrySnapshotDoesNotRetainReplayPayload()
        {
            Assert.That(typeof(PlaytestTelemetrySnapshot)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Any(field => field.FieldType.FullName?.Contains("ReplayFrame") == true ||
                              field.FieldType.FullName?.Contains("ReplayRecording") == true), Is.False);
        }

        [Test]
        public void TelemetryJsonUsesDeterministicVersionedSchema()
        {
            PlaytestTelemetry telemetry = CreateTelemetry();
            string json = telemetry.GenerateJson();
            Assert.That(json, Does.Contain("\"schemaVersion\": 1"));
            Assert.That(json.IndexOf("buildVersion", StringComparison.Ordinal),
                Is.LessThan(json.IndexOf("sectionLoops", StringComparison.Ordinal)));
            Assert.That(json, Does.Not.Contain("ReplayFrame").And.Not.Contain("position"));
            UnityEngine.Object.DestroyImmediate(telemetry.gameObject);
        }

        [Test]
        public void HistoryAndTelemetryCanRepresentSameLoopResult()
        {
            LoopHistorySummary summary = new LoopHistorySummary(2, 120, 2, 2,
                0.02f, 2, 0, ReplayHistoryState.Active, LoopEndReason.Manual);
            LoopHistory history = new LoopHistory();
            history.Add(summary);
            PlaytestTelemetry telemetry = CreateTelemetry();
            telemetry.RecordLoop(summary);
            Assert.That(telemetry.Snapshot().InteractionSuccessCount,
                Is.EqualTo(history[0].InteractionSuccessCount));
            UnityEngine.Object.DestroyImmediate(telemetry.gameObject);
        }

        [Test]
        public void Phase3LocalizationCatalogHasNoEmptyDisplayString()
        {
            Phase3TextCatalog catalog = ScriptableObject.CreateInstance<Phase3TextCatalog>();
            Assert.That(catalog.HasNoEmptyValues(), Is.True);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void Phase3BuilderIsIdempotentAndPreservesOlderScenesAndStableIds()
        {
            string[] older =
            {
                Path.Combine(Application.dataPath, "_Project/Scenes/P0_ReplayLab.unity"),
                Path.Combine(Application.dataPath, "_Project/Scenes/P1_InteractionLab.unity"),
                Path.Combine(Application.dataPath, "_Project/Scenes/P2_CoordinationLab.unity")
            };
            string[] before = older.Select(Hash).ToArray();
            P3SceneBuilder.BuildScene();
            string[] firstIds = UnityEngine.Object.FindObjectsByType<StableId>(
                    FindObjectsInactive.Include)
                .Select(id => id.Value).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            P3SceneBuilder.BuildScene();
            string[] secondIds = UnityEngine.Object.FindObjectsByType<StableId>(
                    FindObjectsInactive.Include)
                .Select(id => id.Value).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Assert.That(older.Select(Hash), Is.EqualTo(before));
            Assert.That(secondIds, Is.EqualTo(firstIds));
            Assert.That(secondIds.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(secondIds.Length));
            Assert.That(EditorBuildSettings.scenes.Select(scene => scene.path), Is.EqualTo(new[]
            {
                P3SceneBuilder.ScenePath,
                "Assets/_Project/Scenes/P2_CoordinationLab.unity",
                "Assets/_Project/Scenes/P1_InteractionLab.unity",
                "Assets/_Project/Scenes/P0_ReplayLab.unity"
            }));
        }

        private static GameplayStateController PlayingState()
        {
            GameplayStateController state = new GameplayStateController();
            state.TryTransition(GameplayState.Playing, "boot");
            return state;
        }

        private static PlaytestTelemetry CreateTelemetry()
        {
            PlaytestTelemetry telemetry = new GameObject("Telemetry").AddComponent<PlaytestTelemetry>();
            telemetry.BeginSession();
            return telemetry;
        }

        private static string Hash(string path)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)))
                .Replace("-", string.Empty);
        }
    }
}
