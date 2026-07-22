using System.Collections;
using EchoShift.Core;
using EchoShift.Gameplay;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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
