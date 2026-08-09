using System.Collections;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Presentation;
using EchoShift.Replay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests.PlayMode
{
    public sealed class Phase4VisualPolishPlayModeTests
    {
        [UnityTest] public IEnumerator Phase4SceneLoads()
        {
            yield return Load();
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("P3_PlayableGreybox"));
        }

        [UnityTest] public IEnumerator SceneRetainsExactlyThreePuzzleSections()
        {
            yield return Load();
            Assert.That(FindAll<PuzzleSectionController>().Length, Is.EqualTo(3));
        }

        [UnityTest] public IEnumerator CoordinatorRetainsRequiredReferences()
        {
            yield return Load();
            Assert.That(Find<SectionTransitionCoordinator>().HasValidReferences, Is.True);
        }

        [UnityTest] public IEnumerator EveryPlayerHasConfiguredCompoundVisual()
        {
            yield return Load();
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            for (int i = 0; i < sections.Length; i++)
                Assert.That(sections[i].Player.GetComponent<Phase4ActorVisual>().HasRequiredReferences, Is.True);
        }

        [UnityTest] public IEnumerator PlayerPrimitiveRendererIsHidden()
        {
            yield return Load();
            Assert.That(Find<SectionTransitionCoordinator>().ActiveSection.Player
                .GetComponent<Renderer>().enabled, Is.False);
        }

        [UnityTest] public IEnumerator EverySectionHasModularFacilityRoot()
        {
            yield return Load();
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            for (int i = 0; i < sections.Length; i++)
            {
                Transform root = sections[i].transform.Find("Environment/P4 Facility");
                Assert.That(root, Is.Not.Null);
                Assert.That(root.childCount, Is.GreaterThan(25));
            }
        }

        [UnityTest] public IEnumerator DeviceVisualAdaptersHaveReferences()
        {
            yield return Load();
            Phase4PressurePlateVisual[] plates = FindAll<Phase4PressurePlateVisual>();
            Phase4BatteryVisual[] batteries = FindAll<Phase4BatteryVisual>();
            Phase4SocketVisual[] sockets = FindAll<Phase4SocketVisual>();
            Phase4GoalVisual[] goals = FindAll<Phase4GoalVisual>();
            Assert.That(plates.Length, Is.EqualTo(2));
            Assert.That(batteries.Length, Is.EqualTo(2));
            Assert.That(sockets.Length, Is.EqualTo(2));
            Assert.That(goals.Length, Is.EqualTo(3));
            for (int i = 0; i < plates.Length; i++) Assert.That(plates[i].HasRequiredReferences, Is.True);
            for (int i = 0; i < batteries.Length; i++) Assert.That(batteries[i].HasRequiredReferences, Is.True);
            for (int i = 0; i < sockets.Length; i++) Assert.That(sockets[i].HasRequiredReferences, Is.True);
            for (int i = 0; i < goals.Length; i++) Assert.That(goals[i].HasRequiredReferences, Is.True);
        }

        [UnityTest] public IEnumerator EveryDoorHasPhase4StatusFeedback()
        {
            yield return Load();
            DoorVisualFeedback[] doors = FindAll<DoorVisualFeedback>();
            Assert.That(doors.Length, Is.EqualTo(4));
            for (int i = 0; i < doors.Length; i++) Assert.That(doors[i].HasPhase4References, Is.True);
        }

        [UnityTest] public IEnumerator FeedbackPoolIsConfiguredAndBounded()
        {
            yield return Load();
            Phase4FeedbackPool pool = Find<Phase4FeedbackPool>();
            Assert.That(pool.HasValidReferences, Is.True);
            Assert.That(pool.Capacity, Is.InRange(4, 16));
        }

        [UnityTest] public IEnumerator FeedbackPoolPreallocatesExactCapacity()
        {
            yield return Load();
            Phase4FeedbackPool pool = Find<Phase4FeedbackPool>();
            Assert.That(pool.GetComponentsInChildren<ParticleSystem>(true).Length,
                Is.EqualTo(pool.Capacity));
        }

        [UnityTest] public IEnumerator AudioControllerHasBoundedSourcesAndCues()
        {
            yield return Load();
            Assert.That(Find<Phase4AudioController>().HasValidReferences, Is.True);
            Assert.That(Find<Phase4AudioController>().GetComponentsInChildren<AudioSource>(true).Length,
                Is.EqualTo(8));
        }

        [UnityTest] public IEnumerator PackagedJapaneseFontInitializesWithoutOsLookup()
        {
            yield return Load();
            JapaneseFontApplier font = Find<JapaneseFontApplier>();
            Assert.That(font.UsesPackagedFont, Is.True);
            Assert.That(font.IsReady, Is.True);
            Assert.That(font.ResolvedFontName, Is.EqualTo("Noto Sans JP (Packaged)"));
        }

        [UnityTest] public IEnumerator HudHasPhase4References()
        {
            yield return Load();
            Assert.That(Find<Phase4HudVisual>().HasRequiredReferences, Is.True);
        }

        [UnityTest] public IEnumerator HudSupportsThreeReferenceResolutions()
        {
            yield return Load();
            Assert.That(Find<Phase4HudVisual>().SupportsReferenceResolutions, Is.True);
        }

        [UnityTest] public IEnumerator CameraUsesHdrAndPostProcessing()
        {
            yield return Load();
            Camera camera = Find<Camera>();
            Assert.That(camera.allowHDR, Is.True);
            Assert.That(camera.GetUniversalAdditionalCameraData().renderPostProcessing, Is.True);
        }

        [UnityTest] public IEnumerator SceneHasSafeGlobalVolume()
        {
            yield return Load();
            Volume volume = Find<Volume>();
            Assert.That(volume.isGlobal, Is.True);
            Assert.That(volume.sharedProfile, Is.Not.Null);
            Assert.That(volume.sharedProfile.TryGet(out MotionBlur _), Is.False);
            Assert.That(volume.sharedProfile.TryGet(out ChromaticAberration _), Is.False);
        }

        [UnityTest] public IEnumerator EachSectionUsesOnlyTwoUnshadowedLocalLights()
        {
            yield return Load();
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            for (int i = 0; i < sections.Length; i++)
            {
                Light[] lights = sections[i].transform.Find("Environment/P4 Facility")
                    .GetComponentsInChildren<Light>(true);
                Assert.That(lights.Length, Is.EqualTo(2));
                for (int j = 0; j < lights.Length; j++) Assert.That(lights[j].shadows, Is.EqualTo(LightShadows.None));
            }
        }

        [UnityTest] public IEnumerator PresentationRootsDoNotAddGameplayColliders()
        {
            yield return Load();
            Transform[] transforms = FindAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!transforms[i].name.StartsWith("P4 ")) continue;
                Assert.That(transforms[i].GetComponentsInChildren<Collider>(true), Is.Empty,
                    transforms[i].name);
            }
        }

        [UnityTest] public IEnumerator PlateVisualRespondsToActorRegistration()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            PressurePlate plate = coordinator.ActiveSection.GetComponentInChildren<PressurePlate>();
            Phase4PressurePlateVisual visual = plate.GetComponent<Phase4PressurePlateVisual>();
            LoopActor actor = coordinator.ActiveSection.Player.GetComponent<LoopActor>();
            plate.RegisterActor(actor); visual.RefreshNowForTests();
            Assert.That(visual.IsPressedVisual, Is.True);
            plate.UnregisterActor(actor); visual.RefreshNowForTests();
            Assert.That(visual.IsPressedVisual, Is.False);
        }

        [UnityTest] public IEnumerator AudioSuppressesImmediateDuplicateCue()
        {
            yield return Load();
            Phase4AudioController audio = Find<Phase4AudioController>();
            Assert.That(audio.Play(Phase4AudioCue.UiSelect), Is.True);
            Assert.That(audio.Play(Phase4AudioCue.UiSelect), Is.False);
            Assert.That(audio.SuppressedCount, Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator PauseKeepsHudLayoutSafeAndResumes()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Assert.That(coordinator.SetPaused(true), Is.True);
            Assert.That(Find<Phase4HudVisual>().ValidateLayout(1280, 720), Is.True);
            Assert.That(coordinator.SetPaused(false), Is.True);
        }

        [UnityTest] public IEnumerator ShortLoopCreatesPolishedEcho()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            for (int i = 0; i < 8; i++) yield return null;
            coordinator.ActiveSection.Director.RequestLoopEnd();
            for (int i = 0; i < 30 && coordinator.ActiveSection.Director.EchoCount == 0; i++)
                yield return null;
            Assert.That(coordinator.ActiveSection.Director.EchoCount, Is.EqualTo(1));
            EchoPlayback echo = coordinator.ActiveSection.GetComponentInChildren<EchoPlayback>();
            Assert.That(echo, Is.Not.Null);
            Phase4ActorVisual visual = echo.GetComponent<Phase4ActorVisual>();
            visual.RefreshNowForTests();
            Assert.That(visual.HasRequiredReferences, Is.True);
            Assert.That(visual.VisibleGenerationMarkCount, Is.EqualTo(1));
            Assert.That(coordinator.ActiveSection.Director.MaximumReplayDrift, Is.LessThanOrEqualTo(0.05f));
        }

        [UnityTest] public IEnumerator MaximumEchoesRemainVisuallyDistinctAndEvictionEmitsOnce()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Phase4FeedbackDirector feedback = Find<Phase4FeedbackDirector>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            for (int generation = 1; generation <= 3; generation++)
            {
                coordinator.ActiveSection.Director.RequestLoopEnd();
                for (int frame = 0; frame < 30 &&
                     coordinator.ActiveSection.Director.EchoCount < generation; frame++)
                    yield return null;
            }

            Assert.That(coordinator.ActiveSection.Director.EchoCount, Is.EqualTo(3));
            for (int index = 0; index < 3; index++)
            {
                Phase4ActorVisual visual = coordinator.ActiveSection.Director
                    .GetEchoPlayback(index).GetComponent<Phase4ActorVisual>();
                visual.RefreshNowForTests();
                Assert.That(visual.VisibleGenerationMarkCount, Is.EqualTo(index + 1));
            }

            int removals = feedback.EchoRemovalFeedbackCount;
            coordinator.ActiveSection.Director.RequestLoopEnd();
            for (int frame = 0; frame < 30 && coordinator.ActiveSection.Director
                     .GetEchoPlayback(2).ReplayGeneration < 4; frame++)
                yield return null;
            Assert.That(feedback.EchoRemovalFeedbackCount, Is.EqualTo(removals + 1));
            Phase4ActorVisual cycled = coordinator.ActiveSection.Director
                .GetEchoPlayback(2).GetComponent<Phase4ActorVisual>();
            cycled.RefreshNowForTests();
            Assert.That(cycled.VisibleGenerationMarkCount, Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator PlateDrivenDoorUpdatesVisualAndFeedback()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            PressurePlate plate = coordinator.ActiveSection.GetComponentInChildren<PressurePlate>();
            DoorController door = coordinator.ActiveSection.GetComponentInChildren<DoorController>();
            DoorVisualFeedback visual = door.GetComponent<DoorVisualFeedback>();
            Phase4FeedbackDirector feedback = Find<Phase4FeedbackDirector>();
            LoopActor actor = coordinator.ActiveSection.Player.GetComponent<LoopActor>();
            Assert.That(feedback.HasValidReferences, Is.True);
            Assert.That(feedback.IsSubscribed, Is.True);
            Assert.That(feedback.TrackedDoorCount, Is.GreaterThanOrEqualTo(3));
            int before = feedback.DoorFeedbackCount;

            coordinator.ActiveSection.Director.SetSimulationPaused(true);
            plate.RegisterActor(actor);
            door.BeginSimulationTick();
            door.CommitDeviceState();
            door.BeginSimulationTick();
            yield return null;
            visual.RefreshNowForTests();

            Assert.That(door.IsOpen, Is.True);
            Assert.That(visual.IsOpenVisual, Is.True);
            Assert.That(feedback.DoorFeedbackCount, Is.EqualTo(before + 1));
        }

        [UnityTest] public IEnumerator BatteryAndSocketVisualsFollowRecordedInteractionState()
        {
            yield return Load();
            PuzzleSectionController[] sections = FindAll<PuzzleSectionController>();
            PuzzleSectionController section = System.Array.Find(sections, item => item.SectionNumber == 2);
            section.ActivateSection(true);
            CarryableBattery battery = section.GetComponentInChildren<CarryableBattery>();
            PowerSocket socket = section.GetComponentInChildren<PowerSocket>();
            Interactor interactor = section.Player.GetComponent<Interactor>();

            section.Player.transform.position = battery.transform.position;
            Physics.SyncTransforms();
            InteractionExecution pickup = interactor.ExecuteRecorded(new InteractionCommand(
                0, InteractionKind.PickupBattery, battery.StableId.Value,
                section.Player.transform.position));
            Phase4BatteryVisual batteryVisual = battery.GetComponent<Phase4BatteryVisual>();
            batteryVisual.RefreshNowForTests();
            Assert.That(pickup.Succeeded, Is.True);
            Assert.That(batteryVisual.IsHeldVisual, Is.True);

            section.Player.transform.position = socket.transform.position;
            Physics.SyncTransforms();
            InteractionExecution insert = interactor.ExecuteRecorded(new InteractionCommand(
                1, InteractionKind.InsertBattery, socket.StableId.Value,
                section.Player.transform.position));
            Phase4SocketVisual socketVisual = socket.GetComponent<Phase4SocketVisual>();
            batteryVisual.RefreshNowForTests();
            socketVisual.RefreshNowForTests();
            Assert.That(insert.Succeeded, Is.True);
            Assert.That(batteryVisual.IsInsertedVisual, Is.True);
            Assert.That(socketVisual.IsPoweredVisual, Is.True);
        }

        [UnityTest] public IEnumerator GoalCompletionFeedbackIsEmittedOnlyOnce()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Phase4FeedbackDirector feedback = Find<Phase4FeedbackDirector>();
            Assert.That(feedback.HasValidReferences, Is.True);
            Assert.That(feedback.IsSubscribed, Is.True);
            GoalVolume goal = coordinator.ActiveSection.Goal;
            Collider playerCollider = coordinator.ActiveSection.Player.GetComponent<Collider>();
            goal.SendMessage("OnTriggerEnter", playerCollider);
            goal.SendMessage("OnTriggerEnter", playerCollider);
            Assert.That(feedback.GoalCompletionFeedbackCount, Is.EqualTo(1));
        }

        private static IEnumerator Load()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync("P3_PlayableGreybox", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
        }

        private static T Find<T>() where T : Object =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);

        private static T[] FindAll<T>() where T : Object =>
            Object.FindObjectsByType<T>(FindObjectsInactive.Include);
    }
}
