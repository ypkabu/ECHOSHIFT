using System.Collections;
using EchoShift.Gameplay;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoShift.Tests.PlayMode
{
    public sealed class Phase4ExternalAssetIntegrationPlayModeTests
    {
        [UnityTest]
        public IEnumerator SceneUsesExternalEnvironmentDoorGoalAndRobotVisuals()
        {
            yield return Load();
            Assert.That(FindByName("External Floor 1-1"), Is.Not.Null);
            Assert.That(FindByName("External Door Left Panel"), Is.Not.Null);
            Assert.That(FindByName("External Door Right Panel"), Is.Not.Null);
            Assert.That(FindByName("External Goal Frame"), Is.Not.Null);
            Assert.That(FindByName("Quaternius Robot Model"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator ImportedRobotAnimationStaysBelowStableVisualAdapterTransform()
        {
            yield return Load();
            Transform robot = FindByName("P4 Robot Visual");
            Assert.That(robot, Is.Not.Null);
            Assert.That(robot.GetComponent<Animator>(), Is.Null);
            Animator[] animators = robot.GetComponentsInChildren<Animator>(true);
            Assert.That(animators.Length, Is.EqualTo(1));
            Assert.That(animators[0].applyRootMotion, Is.False);
            Assert.That(animators[0].transform, Is.Not.SameAs(robot));
            Vector3 position = robot.localPosition;
            Quaternion rotation = robot.localRotation;
            for (int i = 0; i < 12; i++) yield return null;
            Assert.That(Vector3.Distance(robot.localPosition, position), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(robot.localRotation, rotation), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator PauseHidesGameplayHudAndShowsKeyboardFocus()
        {
            yield return Load();
            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Assert.That(coordinator.SetPaused(true), Is.True);
            yield return null;
            Phase4HudVisual hud = Find<Phase4HudVisual>();
            hud.RefreshNowForTests();
            CanvasGroup group = hud.GetComponent<CanvasGroup>();
            Assert.That(group.alpha, Is.EqualTo(0f));
            Assert.That(group.interactable, Is.False);
            Phase4PauseButtonVisual[] focus = coordinator.PauseMenu
                .GetComponentsInChildren<Phase4PauseButtonVisual>(true);
            Assert.That(focus.Length, Is.EqualTo(4));
            Assert.That(focus[0].IsHighlighted, Is.True);
            Assert.That(coordinator.SetPaused(false), Is.True);
            yield return null;
            hud.RefreshNowForTests();
            Assert.That(group.alpha, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator WorldLabelsRemainHiddenDuringGameplay()
        {
            yield return Load();
            WorldBillboardLabel[] labels = FindAll<WorldBillboardLabel>();
            Assert.That(labels, Is.Not.Empty);
            for (int i = 0; i < labels.Length; i++)
            {
                Renderer renderer = labels[i].GetComponent<Renderer>();
                bool allowedGoalSign = labels[i].name == "出口 Sign";
                Assert.That(renderer == null || !renderer.enabled || allowedGoalSign,
                    Is.True, labels[i].name);
            }
        }

        [UnityTest]
        public IEnumerator ExternalPresentationHierarchyAddsNoCollider()
        {
            yield return Load();
            Transform[] transforms = FindAll<Transform>();
            int externalRoots = 0;
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!transforms[i].name.StartsWith("External ", System.StringComparison.Ordinal) &&
                    transforms[i].name != "Quaternius Robot Model") continue;
                externalRoots++;
                Assert.That(transforms[i].GetComponentsInChildren<Collider>(true), Is.Empty,
                    transforms[i].name);
            }
            Assert.That(externalRoots, Is.GreaterThan(50));
        }

        [UnityTest]
        public IEnumerator HudAndPauseStaySafeAtSupportedResolutions()
        {
            yield return Load();
            Phase4HudVisual hud = Find<Phase4HudVisual>();
            Assert.That(hud.ValidateLayout(1280, 720), Is.True);
            Assert.That(hud.ValidateLayout(1920, 1080), Is.True);
            Assert.That(hud.ValidateLayout(2560, 1440), Is.True);
            RectTransform card = FindByName("P4 Pause Card") as RectTransform;
            Assert.That(card, Is.Not.Null);
            Assert.That(card.rect.width, Is.LessThanOrEqualTo(760f));
            Assert.That(card.rect.height, Is.LessThanOrEqualTo(720f));
            Button[] buttons = card.GetComponentsInChildren<Button>(true);
            Assert.That(buttons.Length, Is.EqualTo(4));
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

        private static Transform FindByName(string name)
        {
            Transform[] transforms = FindAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == name) return transforms[i];
            return null;
        }
    }
}
