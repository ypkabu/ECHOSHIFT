using System;
using System.Collections;
using System.IO;
using EchoShift.Core;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using EchoShift.Player;
using EchoShift.Replay;
using EchoShift.Telemetry;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests.PlayMode
{
    public sealed class Phase4CharacterPresentationPlayModeTests
    {
        private static bool _fullRouteReady;
        private static GameplayState _fullRouteState;
        private static int _fullRouteMaximumEchoes;
        private static int _fullRouteSuccesses;
        private static int _fullRouteFailures;
        private static float _fullRouteDrift;

        [UnityTest]
        public IEnumerator IdlePoseLowersBothHandsBelowShoulders()
        {
            yield return Load();
            Phase4RobotPoseController pose = PlayerPose();
            pose.ForcePoseForCapture(Phase4RobotPoseState.Idle);
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.Idle));
            Transform robot = pose.Animator.transform;
            Assert.That(pose.LeftPresentationHand.position.y,
                Is.LessThan(FindBone(robot, "UpperArm.L").position.y - 0.08f));
            Assert.That(pose.RightPresentationHand.position.y,
                Is.LessThan(FindBone(robot, "UpperArm.R").position.y - 0.08f));
        }

        [UnityTest]
        public IEnumerator MovementTransitionsIdlePoseToWalk()
        {
            yield return Load();
            Phase4RobotPoseController pose = PlayerPose();
            pose.RefreshNowForTests();
            pose.GetComponentInParent<LoopActor>().transform.position += Vector3.forward * 0.1f;
            pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.Walk));
        }

        [UnityTest]
        public IEnumerator StoppingReturnsWalkPoseToIdle()
        {
            yield return Load();
            Phase4RobotPoseController pose = PlayerPose();
            pose.RefreshNowForTests();
            pose.GetComponentInParent<LoopActor>().transform.position += Vector3.right * 0.1f;
            pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.Walk));
            pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.Idle));
        }

        [UnityTest]
        public IEnumerator BatteryPickupTransitionsToCarryIdle()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 1, 70, 300);
            Assert.That(section.Player.Interactor.CarriedBattery, Is.Not.Null);
            Phase4RobotPoseController pose = section.Player.GetComponentInChildren<
                Phase4RobotPoseController>(true);
            for (int i = 0; i < 24; i++) pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.CarryIdle));
            Assert.That(pose.HasCarryPoseVisuals, Is.True);
        }

        [UnityTest]
        public IEnumerator HeldBatteryMovementUsesCarryWalk()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 1, 70, 300);
            Phase4RobotPoseController pose = section.Player.GetComponentInChildren<
                Phase4RobotPoseController>(true);
            for (int i = 0; i < 24; i++) pose.RefreshNowForTests();
            section.Player.transform.position += Vector3.forward * 0.1f;
            pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.CarryWalk));
        }

        [UnityTest]
        public IEnumerator SuccessfulInteractionTriggersOneBoundedInteractReaction()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            Phase4RobotPoseController pose = section.Player.GetComponentInChildren<
                Phase4RobotPoseController>(true);
            AdvanceUntil(section, 1, 62, 300);
            Assert.That(section.Player.InteractionSuccessCount, Is.EqualTo(1));
            pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.Interact));
            for (int i = 0; i < 24; i++) pose.RefreshNowForTests();
            Assert.That(pose.CurrentState, Is.Not.EqualTo(Phase4RobotPoseState.Interact));
            Assert.That(section.Player.InteractionSuccessCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator EchoUsesTheSameSixStatePoseController()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController section = coordinator.ActiveSection;
            for (int i = 0; i < 8; i++) section.Director.AdvanceOneTickForTests();
            section.Director.RequestLoopEnd();
            section.Director.AdvanceOneTickForTests();
            Phase4RobotPoseController echo = section.Director.GetEchoPlayback(0)
                .GetComponentInChildren<Phase4RobotPoseController>(true);
            echo.ForcePoseForCapture(Phase4RobotPoseState.Walk);
            Assert.That(echo.CurrentState, Is.EqualTo(Phase4RobotPoseState.Walk));
            Assert.That(echo.Animator.runtimeAnimatorController,
                Is.SameAs(PlayerPose().Animator.runtimeAnimatorController));
        }

        [UnityTest]
        public IEnumerator ReplayCompletionUsesEchoStoppedPose()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController section = coordinator.ActiveSection;
            for (int i = 0; i < 8; i++) section.Director.AdvanceOneTickForTests();
            section.Director.RequestLoopEnd();
            section.Director.AdvanceOneTickForTests();
            EchoPlayback echo = section.Director.GetEchoPlayback(0);
            for (int i = 0; i < 120 && echo.PlaybackTick < echo.RecordingLength; i++)
                section.Director.AdvanceOneTickForTests();
            Phase4RobotPoseController pose = echo.GetComponentInChildren<
                Phase4RobotPoseController>(true);
            pose.RefreshNowForTests();
            Assert.That(echo.PlaybackTick, Is.EqualTo(echo.RecordingLength));
            Assert.That(pose.CurrentState, Is.EqualTo(Phase4RobotPoseState.EchoStopped));
        }

        [UnityTest]
        public IEnumerator VisualStatesNeverMoveGameplayRoot()
        {
            yield return Load();
            Phase4RobotPoseController pose = PlayerPose();
            Transform root = pose.GetComponentInParent<LoopActor>().transform;
            Vector3 position = root.position;
            Quaternion rotation = root.rotation;
            for (int state = 0; state <= (int)Phase4RobotPoseState.EchoStopped; state++)
                pose.ForcePoseForCapture((Phase4RobotPoseState)state);
            Assert.That(root.position, Is.EqualTo(position));
            Assert.That(root.rotation, Is.EqualTo(rotation));
        }

        [UnityTest]
        public IEnumerator AnimatedEchoReplayRemainsWithinDriftTolerance()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController section = coordinator.ActiveSection;
            for (int i = 0; i < 20; i++) section.Director.AdvanceOneTickForTests();
            section.Director.RequestLoopEnd();
            section.Director.AdvanceOneTickForTests();
            for (int i = 0; i < 20; i++) section.Director.AdvanceOneTickForTests();
            Assert.That(section.Director.EchoCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(section.Director.MaximumReplayDrift, Is.LessThanOrEqualTo(0.05f));
        }

        [UnityTest]
        public IEnumerator HeldBatteryStaysInFrontOfRobotBody()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 1, 70, 300);
            yield return null;
            CarryableBattery battery = section.Player.Interactor.CarriedBattery;
            Vector3 local = section.Player.transform.InverseTransformPoint(battery.transform.position);
            Assert.That(local.z, Is.GreaterThan(0.5f));
            Assert.That(Mathf.Abs(local.x), Is.LessThan(0.05f));
        }

        [UnityTest]
        public IEnumerator HeldBatteryVisualDoesNotTouchFloor()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 1, 70, 300);
            Phase4BatteryVisual visual = section.GetComponentInChildren<Phase4BatteryVisual>(true);
            Bounds bounds = CombinedBounds(visual.VisualRoot.GetComponentsInChildren<Renderer>(true));
            Assert.That(bounds.min.y, Is.GreaterThan(0.05f));
        }

        [UnityTest]
        public IEnumerator InsertionVisualPreservesPoweredSocketState()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 1, 108, 400);
            PowerSocket socket = section.GetComponentInChildren<PowerSocket>(true);
            Phase4BatteryVisual visual = section.GetComponentInChildren<Phase4BatteryVisual>(true);
            visual.AdvanceForTests(Phase4BatteryVisual.InsertionVisualDuration * 0.5f);
            Assert.That(socket.IsPowered, Is.True);
            Assert.That(socket.InsertedBattery, Is.Not.Null);
            Assert.That(visual.InsertionProgress, Is.InRange(0.45f, 0.55f));
        }

        [UnityTest]
        public IEnumerator PoweredDoorOpensWithSplitPanels()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 2, 112, 600);
            DoorVisualFeedback visual = section.GetComponentInChildren<DoorVisualFeedback>(true);
            visual.RefreshNowForTests();
            Assert.That(visual.IsOpenVisual, Is.True);
            Assert.That(visual.UsesSplitPanels, Is.True);
            Assert.That(visual.LeftPanel.gameObject.activeSelf, Is.False);
            Assert.That(visual.RightPanel.gameObject.activeSelf, Is.False);
        }

        [UnityTest]
        public IEnumerator OpenDoorPanelsDoNotVisuallyObstructPassage()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 2, 112, 600);
            DoorVisualFeedback visual = section.GetComponentInChildren<DoorVisualFeedback>(true);
            visual.RefreshNowForTests();
            Assert.That(visual.LeftPanel.gameObject.activeInHierarchy, Is.False);
            Assert.That(visual.RightPanel.gameObject.activeInHierarchy, Is.False);
            Assert.That(visual.VisualsAreSeparatedFromGameplayRoot, Is.True);
        }

        [UnityTest]
        public IEnumerator DoorColliderStillUsesGameplayDoorTickState()
        {
            yield return Load();
            PuzzleSectionController section = PrepareRoute(2);
            AdvanceUntil(section, 2, 112, 600);
            DoorController door = section.GetComponentInChildren<DoorController>(true);
            Collider collider = door.GetComponent<Collider>();
            Assert.That(door.IsOpen, Is.True);
            Assert.That(door.AppliedOpenRequest, Is.True);
            Assert.That(collider, Is.Not.Null);
            Assert.That(collider.transform, Is.SameAs(door.transform));
        }

        [UnityTest]
        public IEnumerator FloorCircuitMeshesRemainBelowActorBody()
        {
            yield return Load();
            Phase4FloorCircuitVisual[] circuits = FindAll<Phase4FloorCircuitVisual>();
            Assert.That(circuits, Is.Not.Empty);
            for (int i = 0; i < circuits.Length; i++)
            {
                Renderer[] renderers = circuits[i].GetComponentsInChildren<Renderer>(true);
                for (int segment = 0; segment < renderers.Length; segment++)
                    Assert.That(renderers[segment].bounds.max.y, Is.LessThan(0.04f),
                        renderers[segment].name);
            }
        }

        [UnityTest]
        public IEnumerator P3IntegrationSceneKeepsAllValidatedSectionRoots()
        {
            yield return Load();
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            Assert.That(sections.Length, Is.EqualTo(3));
            for (int i = 1; i <= 3; i++)
                Assert.That(Array.Exists(sections, section => section.SectionNumber == i), Is.True);
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("P3_PlayableGreybox"));
        }

        [UnityTest, Order(200)]
        public IEnumerator FullP3RouteCompletesAllSectionsWithPresentationEnabled()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.ConfirmInteractiveStartForTests();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            Array.Sort(sections, (left, right) =>
                left.SectionNumber.CompareTo(right.SectionNumber));
            for (int i = 0; i < sections.Length; i++)
            {
                Phase4StandaloneRouteInputSource route = sections[i].Player.gameObject
                    .AddComponent<Phase4StandaloneRouteInputSource>();
                route.Configure(i + 1, sections[i].Director);
                sections[i].Player.Configure(route, sections[i].Player.Motor,
                    sections[i].Player.Interactor);
            }
            int advances = 0;
            int maxEchoes = 0;
            while (coordinator.State != GameplayState.Completed && advances++ < 4000)
            {
                if (coordinator.State == GameplayState.Playing)
                {
                    maxEchoes = Mathf.Max(maxEchoes,
                        coordinator.ActiveSection.Director.EchoCount);
                    coordinator.ActiveSection.Director.AdvanceOneTickForTests();
                    coordinator.EvaluateActiveSectionForTests();
                }
                else coordinator.CompleteTransitionNowForTests();
                if (advances % 20 == 0) yield return null;
            }
            PlaytestTelemetrySnapshot snapshot = coordinator.Telemetry.Snapshot();
            _fullRouteReady = true;
            _fullRouteState = coordinator.State;
            _fullRouteMaximumEchoes = maxEchoes;
            _fullRouteSuccesses = snapshot.InteractionSuccessCount;
            _fullRouteFailures = snapshot.InteractionFailureCount;
            _fullRouteDrift = snapshot.MaximumDrift;
            Assert.That(_fullRouteState, Is.EqualTo(GameplayState.Completed));
            Assert.That(_fullRouteMaximumEchoes, Is.GreaterThanOrEqualTo(2));
        }

        [Test, Order(201)]
        public void FullRouteReplayDriftStaysBelowFiveCentimeters()
        {
            Assert.That(_fullRouteReady, Is.True);
            Assert.That(_fullRouteDrift, Is.LessThanOrEqualTo(0.05f));
        }

        [Test, Order(202)]
        public void FullRouteHasFourSuccessfulAndZeroFailedInteractions()
        {
            Assert.That(_fullRouteReady, Is.True);
            Assert.That(_fullRouteSuccesses, Is.EqualTo(4));
            Assert.That(_fullRouteFailures, Is.Zero);
        }

        [UnityTest]
        public IEnumerator GeneratedPresentationHasNoMissingRequiredReferencesOrExceptions()
        {
            yield return Load();
            Phase4RobotPoseController[] poses = FindAll<Phase4RobotPoseController>();
            DoorVisualFeedback[] doors = FindAll<DoorVisualFeedback>();
            Phase4FloorCircuitVisual[] circuits = FindAll<Phase4FloorCircuitVisual>();
            Assert.That(poses, Is.Not.Empty);
            Assert.That(doors, Is.Not.Empty);
            Assert.That(circuits, Is.Not.Empty);
            for (int i = 0; i < poses.Length; i++)
                Assert.That(poses[i].HasRequiredReferences, Is.True, poses[i].name);
            for (int i = 0; i < doors.Length; i++)
                Assert.That(doors[i].HasPhase4References, Is.True, doors[i].name);
            for (int i = 0; i < circuits.Length; i++)
                Assert.That(circuits[i].HasRequiredReferences, Is.True, circuits[i].name);
            int faults = 0;
            Application.LogCallback callback = (_, _, type) =>
            {
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                    faults++;
            };
            Application.logMessageReceived += callback;
            yield return null;
            Application.logMessageReceived -= callback;
            Assert.That(faults, Is.Zero);
        }

        private static Phase4RobotPoseController PlayerPose() =>
            Find<SectionTransitionCoordinator>().ActiveSection.Player
                .GetComponentInChildren<Phase4RobotPoseController>(true);

        private static PuzzleSectionController PrepareRoute(int sectionNumber)
        {
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            PuzzleSectionController selected = null;
            for (int i = 0; i < sections.Length; i++)
            {
                bool active = sections[i].SectionNumber == sectionNumber;
                sections[i].gameObject.SetActive(active);
                if (active) selected = sections[i];
            }
            Assert.That(selected, Is.Not.Null);
            StableId[] ids = selected.GetComponentsInChildren<StableId>(true);
            for (int i = 0; i < ids.Length; i++) Assert.That(ids[i].EnsureResolved(), Is.True);
            DoorController[] doors = selected.GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < doors.Length; i++) Assert.That(doors[i].EnsureSourceCache(), Is.True);
            Phase4StandaloneRouteInputSource route = selected.Player.gameObject
                .AddComponent<Phase4StandaloneRouteInputSource>();
            route.Configure(sectionNumber, selected.Director);
            selected.Player.Configure(route, selected.Player.Motor, selected.Player.Interactor);
            selected.ActivateSection(false);
            Physics.SyncTransforms();
            return selected;
        }

        private static void AdvanceUntil(
            PuzzleSectionController section, int loop, int tick, int maximumAdvances)
        {
            int advances = 0;
            while ((section.Director.LoopNumber < loop ||
                    (section.Director.LoopNumber == loop &&
                     section.Director.CurrentTick < tick)) && advances++ < maximumAdvances)
                section.Director.AdvanceOneTickForTests();
            Assert.That(section.Director.LoopNumber, Is.EqualTo(loop));
            Assert.That(section.Director.CurrentTick, Is.GreaterThanOrEqualTo(tick));
        }

        private static Transform FindBone(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == name) return transforms[i];
            Assert.Fail($"Missing bone {name}");
            return null;
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            Assert.That(renderers, Is.Not.Empty);
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static IEnumerator Load()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                "P3_PlayableGreybox", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
        }

        private static T Find<T>() where T : UnityEngine.Object =>
            UnityEngine.Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        private static T[] FindAll<T>() where T : UnityEngine.Object =>
            UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include);
    }
}
