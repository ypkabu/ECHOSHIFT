using System.Collections;
using EchoShift.Core;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoShift.Tests.PlayMode
{
    public sealed class Phase4PresentationReadabilityPlayModeTests
    {
        private static readonly Vector2Int[] ReferenceResolutions =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1920, 1080),
            new Vector2Int(2560, 1440),
            new Vector2Int(1920, 1200),
            new Vector2Int(2560, 1080)
        };

        [UnityTest]
        public IEnumerator CameraKeepsCurrentPlayerBodyAndMarkerInsideReferenceFrames()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            SectionCameraController controller = Find<SectionCameraController>();
            Camera camera = controller.GetComponent<Camera>();
            controller.SetSection(coordinator.ActiveSection, true);
            Renderer[] renderers = coordinator.ActiveSection.Player.transform
                .Find("P4 Robot Visual").GetComponentsInChildren<Renderer>(true);

            for (int i = 0; i < ReferenceResolutions.Length; i++)
            {
                camera.aspect = ReferenceResolutions[i].x / (float)ReferenceResolutions[i].y;
                controller.RefreshNowForTests(true);
                AssertRenderersInsideViewport(camera, renderers, 0.035f);
                float playerViewportY = camera.WorldToViewportPoint(
                    coordinator.ActiveSection.Player.transform.position).y;
                Assert.That(playerViewportY, Is.InRange(0.35f, 0.40f),
                    $"Player framing at {ReferenceResolutions[i].x}x{ReferenceResolutions[i].y}");
            }
        }

        [UnityTest]
        public IEnumerator CameraUsesLimitedZoomToKeepPlayerAndTwoEchoesVisible()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            PuzzleSectionController section = coordinator.ActiveSection;
            LoopDirector director = section.Director;
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            for (int generation = 1; generation <= 2; generation++)
            {
                director.RequestLoopEnd();
                for (int frame = 0; frame < 30 && director.EchoCount < generation; frame++)
                    yield return null;
            }

            section.Player.transform.localPosition = new Vector3(0f, 1f, 3.6f);
            director.GetEchoPlayback(0).transform.localPosition = new Vector3(-2f, 1f, -3f);
            director.GetEchoPlayback(1).transform.localPosition = new Vector3(2f, 1f, 0.8f);
            Physics.SyncTransforms();

            SectionCameraController controller = Find<SectionCameraController>();
            Camera camera = controller.GetComponent<Camera>();
            controller.SetSection(section, true);
            Assert.That(controller.CurrentFieldOfView,
                Is.InRange(52f, 54f));
            AssertActorInsideViewport(camera, section.Player.transform, 0.025f);
            AssertActorInsideViewport(camera, director.GetEchoPlayback(0).transform, 0.025f);
            AssertActorInsideViewport(camera, director.GetEchoPlayback(1).transform, 0.025f);
        }

        [UnityTest]
        public IEnumerator CameraFocusCannotEscapeActiveSectionBounds()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            PuzzleSectionController section = coordinator.ActiveSection;
            SectionCameraController controller = Find<SectionCameraController>();
            Phase3CameraSettings settings = Resources.FindObjectsOfTypeAll<Phase3CameraSettings>()[0];
            section.Player.transform.position = section.transform.position +
                new Vector3(100f, 1f, 100f);
            controller.SetSection(section, true);

            SectionCameraBounds bounds = settings.GetSectionBounds(section.SectionNumber);
            Vector3 localFocus = controller.CurrentFocus - section.transform.position;
            Assert.That(localFocus.x, Is.InRange(bounds.Minimum.x, bounds.Maximum.x));
            Assert.That(localFocus.z, Is.InRange(bounds.Minimum.y, bounds.Maximum.y));
        }

        [UnityTest]
        public IEnumerator HudHidesTransientIntroAndEmptyContextAfterRevealDuration()
        {
            yield return Load();
            GameplayHud hud = Find<GameplayHud>();
            Phase4HudVisual visual = Find<Phase4HudVisual>();
            Assert.That(hud.IsSectionIntroVisible, Is.True);
            Assert.That(visual.IsIntroPanelVisible, Is.True);

            hud.ExpireTransientPresentationForTests();
            hud.RefreshNow();
            visual.RefreshNowForTests();
            Assert.That(hud.IsSectionIntroVisible, Is.False);
            Assert.That(hud.IsTutorialVisible, Is.False);
            Assert.That(hud.CurrentPrompt, Is.Empty);
            Assert.That(visual.IsIntroPanelVisible, Is.False);
            Assert.That(visual.IsPromptPanelVisible, Is.False);
        }

        [UnityTest]
        public IEnumerator HudHasNoBottomBarAndHidesEmptyBatteryChip()
        {
            yield return Load();
            GameplayHud hud = Find<GameplayHud>();
            Phase4HudVisual visual = Find<Phase4HudVisual>();
            hud.RefreshNow();
            visual.RefreshNowForTests();
            Assert.That(hud.IsCarrying, Is.False);
            Assert.That(visual.IsCarryPanelVisible, Is.False);
            Assert.That(hud.transform.Find("P4 Prompt Panel"), Is.Null);
            Assert.That(hud.transform.Find("P4 Bottom Panel"), Is.Null);
            Assert.That(hud.transform.Find("P4 Battery Carry Chip"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator HudSupportsWideAndTallReferenceLayouts()
        {
            yield return Load();
            Phase4HudVisual visual = Find<Phase4HudVisual>();
            for (int i = 0; i < ReferenceResolutions.Length; i++)
            {
                Assert.That(visual.ValidateLayout(
                    ReferenceResolutions[i].x, ReferenceResolutions[i].y), Is.True,
                    $"{ReferenceResolutions[i].x}x{ReferenceResolutions[i].y}");
            }
        }

        [UnityTest]
        public IEnumerator PauseHidesHudAndUsesDistinctSelectionFill()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Assert.That(coordinator.SetPaused(true), Is.True);
            yield return null;
            Phase4HudVisual hud = Find<Phase4HudVisual>();
            hud.RefreshNowForTests();
            Assert.That(hud.IsGameplayHudVisible, Is.False);
            Phase4PauseButtonVisual[] buttons = coordinator.PauseMenu
                .GetComponentsInChildren<Phase4PauseButtonVisual>(true);
            Assert.That(buttons.Length, Is.EqualTo(4));
            Assert.That(buttons[0].IsHighlighted, Is.True);
            Assert.That(buttons[0].IsSelectionFillVisible, Is.True);
            for (int i = 1; i < buttons.Length; i++)
                Assert.That(buttons[i].IsSelectionFillVisible, Is.False);
            Button quit = buttons[buttons.Length - 1].GetComponent<Button>();
            Assert.That(quit.colors.pressedColor.r, Is.GreaterThan(quit.colors.pressedColor.g));
        }

        [UnityTest]
        public IEnumerator PlayerAndEchoUseDifferentScaleAndFloorMarkerLanguage()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Phase4ActorVisual player = coordinator.ActiveSection.Player
                .GetComponent<Phase4ActorVisual>();
            Assert.That(player.VisualScaleMultiplier, Is.EqualTo(1.12f).Within(0.001f));
            Assert.That(player.UsesNonCircularFloorMarker, Is.False);
            Assert.That(player.transform.localScale, Is.EqualTo(Vector3.one));

            coordinator.ActiveSection.Director.RequestLoopEnd();
            for (int frame = 0; frame < 30 &&
                 coordinator.ActiveSection.Director.EchoCount == 0; frame++) yield return null;
            Phase4ActorVisual echo = coordinator.ActiveSection.Director
                .GetEchoPlayback(0).GetComponent<Phase4ActorVisual>();
            echo.RefreshNowForTests();
            Assert.That(echo.VisualScaleMultiplier, Is.EqualTo(1.08f).Within(0.001f));
            Assert.That(echo.UsesNonCircularFloorMarker, Is.True);
            Assert.That(echo.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(echo.VisualScaleMultiplier, Is.Not.EqualTo(player.VisualScaleMultiplier));
        }

        [UnityTest]
        public IEnumerator EchoGenerationsHaveThreeNonColorSilhouetteVariants()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            LoopDirector director = coordinator.ActiveSection.Director;
            for (int generation = 1; generation <= 3; generation++)
            {
                director.RequestLoopEnd();
                for (int frame = 0; frame < 30 && director.EchoCount < generation; frame++)
                    yield return null;
                Phase4ActorVisual visual = director.GetEchoPlayback(generation - 1)
                    .GetComponent<Phase4ActorVisual>();
                visual.RefreshNowForTests();
                Assert.That(visual.SilhouetteVariant, Is.EqualTo(generation));
                Assert.That(visual.VisibleGenerationMarkCount, Is.EqualTo(generation));
                Assert.That(visual.UsesNonCircularFloorMarker, Is.True);
                Assert.That(visual.transform.Find(
                    "P4 Robot Visual/Echo Segmented Hex Marker"), Is.Not.Null);
            }
        }

        [UnityTest]
        public IEnumerator EchoReplayAndStoppedMarksAreMutuallyExclusive()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            LoopDirector director = coordinator.ActiveSection.Director;
            for (int tick = 0; tick < 8; tick++) director.AdvanceOneTickForTests();
            director.RequestLoopEnd();
            director.AdvanceOneTickForTests();
            Phase4ActorVisual visual = director.GetEchoPlayback(0)
                .GetComponent<Phase4ActorVisual>();
            visual.RefreshNowForTests();
            Assert.That(visual.IsPlayingMarkVisible, Is.True);
            Assert.That(visual.IsStoppedMarkVisible, Is.False);
            EchoShift.Replay.EchoPlayback playback = director.GetEchoPlayback(0);
            for (int tick = 0; tick < 120 &&
                 playback.PlaybackTick < playback.RecordingLength; tick++)
                director.AdvanceOneTickForTests();
            Assert.That(playback.PlaybackTick, Is.EqualTo(playback.RecordingLength));
            visual.RefreshNowForTests();
            Assert.That(visual.IsPlayingMarkVisible, Is.False);
            Assert.That(visual.IsStoppedMarkVisible, Is.True);
        }

        [UnityTest]
        public IEnumerator ActorIdentityDoesNotGenerateLargeWorldSpaceGenerationText()
        {
            yield return Load();
            TextMesh[] labels = Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include);
            for (int i = 0; i < labels.Length; i++)
            {
                bool actorGenerationText = labels[i].name.Contains("Identity Label") ||
                    labels[i].transform.IsChildOf(Find<SectionTransitionCoordinator>()
                        .ActiveSection.Player.transform);
                if (!actorGenerationText) continue;
                Renderer renderer = labels[i].GetComponent<Renderer>();
                Assert.That(renderer == null || !renderer.enabled, Is.True, labels[i].name);
            }
            Phase4ActorVisual player = Find<SectionTransitionCoordinator>()
                .ActiveSection.Player.GetComponent<Phase4ActorVisual>();
            Assert.That(player.transform.Find("P4 Robot Visual")
                .GetComponentsInChildren<TextMesh>(true), Is.Empty);
        }

        [UnityTest]
        public IEnumerator GoalPhysicsRefreshSupportsGeneratedCapsuleTrigger()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            PuzzleSectionController section = coordinator.ActiveSection;
            GoalVolume goal = section.Goal;
            Assert.That(goal.GetComponent<Collider>(), Is.TypeOf<CapsuleCollider>());
            goal.RestoreInitialState();
            section.Player.transform.position = goal.GetComponent<Collider>().bounds.center;
            Physics.SyncTransforms();
            goal.RefreshFromPhysics();
            Assert.That(goal.IsReached, Is.True);
        }

        private static void AssertActorInsideViewport(
            Camera camera, Transform actor, float margin)
        {
            Transform visual = actor.Find("P4 Robot Visual");
            Assert.That(visual, Is.Not.Null, actor.name);
            AssertRenderersInsideViewport(
                camera, visual.GetComponentsInChildren<Renderer>(true), margin);
        }

        private static void AssertRenderersInsideViewport(
            Camera camera, Renderer[] renderers, float margin)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (!renderers[i].enabled || !renderers[i].gameObject.activeInHierarchy) continue;
                Bounds bounds = renderers[i].bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = new Vector3(
                        (corner & 1) == 0 ? min.x : max.x,
                        (corner & 2) == 0 ? min.y : max.y,
                        (corner & 4) == 0 ? min.z : max.z);
                    Vector3 viewport = camera.WorldToViewportPoint(point);
                    Assert.That(viewport.z, Is.GreaterThan(0f), renderers[i].name);
                    Assert.That(viewport.x, Is.InRange(margin, 1f - margin), renderers[i].name);
                    Assert.That(viewport.y, Is.InRange(margin, 1f - margin), renderers[i].name);
                }
            }
        }

        private static IEnumerator Load()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                "P3_PlayableGreybox", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
        }

        private static T Find<T>() where T : Object =>
            Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
    }
}
