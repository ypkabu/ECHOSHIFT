using System.Collections;
using System.Linq;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace EchoShift.Tests
{
    public sealed class Phase3JapaneseLocalizationPlayModeTests
    {
        [UnityTest]
        public IEnumerator Section1StartsWithJapaneseObjectiveAndValidGlyphFont()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            Assert.That(coordinator.Hud.CurrentObjective,
                Is.EqualTo("スイッチを使って扉を開ける"));
            Assert.That(coordinator.Hud.CurrentTutorial,
                Is.EqualTo("黄色の「自分」を青いスイッチまで移動する（WASD / 左スティック）"));
            Assert.That(coordinator.Hud.IsJapaneseReady, Is.True);
            Assert.That(coordinator.Hud.JapaneseFontName, Is.Not.Empty);
        }

        [UnityTest]
        public IEnumerator Section1ShowsMovementPromptBeforeAnInteractionTargetIsNear()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            coordinator.Hud.RefreshNow();
            Assert.That(coordinator.Hud.CurrentPrompt, Is.EqualTo("WASD：移動"));
            Assert.That(coordinator.Hud.ResolvePrompt(InputPromptDevice.Gamepad, false),
                Is.EqualTo("左スティック：移動"));
        }

        [UnityTest]
        public IEnumerator RestartSectionResetsStagedTutorialGuidance()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            TutorialGuide guide = coordinator.ActiveSection.GetComponent<TutorialGuide>();
            guide.ShowStep(2, "advanced");
            Assert.That(guide.Progress.CurrentStep, Is.EqualTo(3));
            Assert.That(coordinator.SetPaused(true), Is.True);
            coordinator.RestartSection();
            Assert.That(guide.Progress.CurrentStep, Is.Zero);
            Assert.That(coordinator.Hud.CurrentTutorial,
                Is.EqualTo("黄色の「自分」を青いスイッチまで移動する（WASD / 左スティック）"));
        }

        [UnityTest]
        public IEnumerator LoopEndShowsJapaneseTransitionAndEchoGeneration()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            AlwaysEndLoopInputSource input = coordinator.ActiveSection.Player.gameObject
                .AddComponent<AlwaysEndLoopInputSource>();
            coordinator.ActiveSection.Player.Configure(input,
                coordinator.ActiveSection.Player.Motor,
                coordinator.ActiveSection.Player.Interactor);
            coordinator.ActiveSection.Director.AdvanceOneTickForTests();
            coordinator.ActiveSection.Director.AdvanceOneTickForTests();
            Assert.That(coordinator.State, Is.EqualTo(GameplayState.LoopTransition));
            Assert.That(coordinator.Hud.CurrentStateMessage,
                Does.Contain("行動を記録しました").And.Contain("エコー1を生成")
                    .And.Contain("次のループを開始"));
            Assert.That(coordinator.Hud.CurrentTutorial,
                Does.Contain("E1・E2").And.Contain("黄色の「自分」")
                    .And.Contain("緑の扉"));
            yield return null;
            EchoVisualFeedback feedback = UnityEngine.Object
                .FindAnyObjectByType<EchoVisualFeedback>();
            Assert.That(feedback, Is.Not.Null);
            Assert.That(feedback.IdentityText, Is.EqualTo("E1"));
            Assert.That(feedback.HasIdentityRing, Is.True);
        }

        [UnityTest]
        public IEnumerator Section2StartsWithJapaneseObjective()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            AdvanceSection(coordinator);
            Assert.That(coordinator.ActiveSectionNumber, Is.EqualTo(2));
            Assert.That(coordinator.Hud.CurrentObjective,
                Is.EqualTo("電池で扉に電力を送る"));
            Assert.That(coordinator.Hud.CurrentTutorial,
                Is.EqualTo("オレンジの電池に近づき E / A・× で持つ"));
        }

        [UnityTest]
        public IEnumerator Section2UsesSpecificPromptsAndAdvancesRecordedInteractionGuidance()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            AdvanceSection(coordinator);
            PuzzleSectionController section = coordinator.ActiveSection;
            CarryableBattery battery = UnityEngine.Object.FindAnyObjectByType<CarryableBattery>();
            PowerSocket socket = UnityEngine.Object.FindAnyObjectByType<PowerSocket>();
            AlwaysInteractJapaneseInputSource input = section.Player.gameObject
                .AddComponent<AlwaysInteractJapaneseInputSource>();
            section.Player.Configure(input, section.Player.Motor, section.Player.Interactor);

            section.Player.Motor.ResetPose(battery.transform.position, Quaternion.identity);
            Physics.SyncTransforms();
            section.Player.RefreshInteractionCandidate(section.Director.CurrentTick);
            coordinator.Hud.RefreshNow();
            Assert.That(coordinator.Hud.CurrentPrompt,
                Is.EqualTo("E：オレンジの電池を持つ"));
            section.Director.AdvanceOneTickForTests();
            coordinator.Hud.RefreshNow();
            Assert.That(section.Player.Interactor.CarriedBattery, Is.SameAs(battery));
            Assert.That(coordinator.Hud.CurrentTutorial,
                Is.EqualTo("電池を持ったまま紫の電源へ運ぶ"));
            Assert.That(coordinator.Hud.TextCatalog.GetInteractionPrompt(
                    InteractionKind.PickupBattery, false),
                Is.EqualTo("E：オレンジの電池を持つ"));

            section.Player.Motor.ResetPose(
                battery.transform.position + Vector3.back * 5f, Quaternion.identity);
            Physics.SyncTransforms();
            section.Player.RefreshInteractionCandidate(section.Director.CurrentTick);
            coordinator.Hud.RefreshNow();
            Assert.That(coordinator.Hud.CurrentPrompt, Is.EqualTo("E：電池を置く"));

            section.Player.Motor.ResetPose(socket.transform.position, Quaternion.identity);
            Physics.SyncTransforms();
            section.Player.RefreshInteractionCandidate(section.Director.CurrentTick);
            coordinator.Hud.RefreshNow();
            Assert.That(coordinator.Hud.CurrentPrompt,
                Is.EqualTo("E：紫の電源に電池を入れる"));
            section.Director.AdvanceOneTickForTests();
            Assert.That(socket.IsPowered, Is.True);
            Assert.That(coordinator.Hud.CurrentTutorial,
                Is.EqualTo("電池を入れたら R / START で操作を記録する"));
            Assert.That(coordinator.Hud.TextCatalog.GetInteractionPrompt(
                    InteractionKind.InsertBattery, false),
                Is.EqualTo("E：紫の電源に電池を入れる"));

            AlwaysEndLoopInputSource end = section.Player.gameObject
                .AddComponent<AlwaysEndLoopInputSource>();
            section.Player.Configure(end, section.Player.Motor, section.Player.Interactor);
            section.Director.AdvanceOneTickForTests();
            section.Director.AdvanceOneTickForTests();
            Assert.That(coordinator.Hud.CurrentTutorial,
                Does.Contain("エコーが電池を運ぶ").And.Contain("黄色の「自分」"));
        }

        [UnityTest]
        public IEnumerator Section2PickupThenDropDoesNotShowInsertionCompleteGuidance()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            AdvanceSection(coordinator);
            PuzzleSectionController section = coordinator.ActiveSection;
            CarryableBattery battery = UnityEngine.Object.FindAnyObjectByType<CarryableBattery>();
            AlwaysInteractJapaneseInputSource input = section.Player.gameObject
                .AddComponent<AlwaysInteractJapaneseInputSource>();
            section.Player.Configure(input, section.Player.Motor, section.Player.Interactor);

            section.Player.Motor.ResetPose(battery.transform.position, Quaternion.identity);
            Physics.SyncTransforms();
            section.Director.AdvanceOneTickForTests();
            Assert.That(section.Player.Interactor.CarriedBattery, Is.SameAs(battery));

            section.Player.Motor.ResetPose(
                battery.transform.position + Vector3.back * 5f, Quaternion.identity);
            Physics.SyncTransforms();
            section.Director.AdvanceOneTickForTests();
            Assert.That(section.Player.Interactor.CarriedBattery, Is.Null);

            AlwaysEndLoopInputSource end = section.Player.gameObject
                .AddComponent<AlwaysEndLoopInputSource>();
            section.Player.Configure(end, section.Player.Motor, section.Player.Interactor);
            section.Director.AdvanceOneTickForTests();
            section.Director.AdvanceOneTickForTests();

            TutorialGuide guide = section.GetComponent<TutorialGuide>();
            Assert.That(guide.Progress.CurrentStep, Is.EqualTo(2));
            Assert.That(coordinator.Hud.CurrentTutorial,
                Does.Not.Contain("エコーが電池を運ぶ間に"));
            Assert.That(coordinator.Hud.CurrentTutorial,
                Does.Contain("紫の電源").And.Contain("R / START"));
        }

        [UnityTest]
        public IEnumerator InteractionWithoutCandidateShowsJapaneseFailureFromRealInputPath()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            PuzzleSectionController section = coordinator.ActiveSection;
            AlwaysInteractJapaneseInputSource input = section.Player.gameObject
                .AddComponent<AlwaysInteractJapaneseInputSource>();
            section.Player.Configure(input, section.Player.Motor, section.Player.Interactor);
            section.Director.AdvanceOneTickForTests();
            Assert.That(coordinator.Hud.LastFailureText,
                Is.EqualTo("操作する対象がありません"));
        }

        [UnityTest]
        public IEnumerator CameraRotationStaysFixedWhileFollowingPlayer()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            SectionCameraController camera = UnityEngine.Object
                .FindAnyObjectByType<SectionCameraController>();
            Quaternion before = camera.transform.rotation;
            Vector3 target = coordinator.ActiveSection.Player.Motor.Position + Vector3.right * 4f;
            coordinator.ActiveSection.Player.Motor.ResetPose(target, Quaternion.identity);
            Physics.SyncTransforms();
            yield return null;
            Assert.That(Quaternion.Angle(before, camera.transform.rotation), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator Section3StartsWithObjectiveButNoSolutionHint()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            AdvanceSection(coordinator);
            AdvanceSection(coordinator);
            Assert.That(coordinator.ActiveSectionNumber, Is.EqualTo(3));
            Assert.That(coordinator.Hud.CurrentObjective,
                Is.EqualTo("2体のエコーと協力して出口へ進む"));
            Assert.That(coordinator.Hud.CurrentTutorial, Is.Empty);
        }

        [UnityTest]
        public IEnumerator InteractionFailureShowsJapaneseReason()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            coordinator.Hud.ShowInteractionFailure(InteractionFailureReason.TargetBusy);
            Assert.That(coordinator.Hud.LastFailureText,
                Is.EqualTo("ほかのエコーが使用中です"));
        }

        [UnityTest]
        public IEnumerator PauseMenuUsesJapaneseLabelsAndConfirmation()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            Assert.That(coordinator.SetPaused(true), Is.True);
            PauseMenuController menu = coordinator.PauseMenu;
            Assert.That(menu.TitleLabel, Is.EqualTo("一時停止"));
            Assert.That(menu.ResumeLabel, Is.EqualTo("ゲームに戻る"));
            Assert.That(menu.RestartSectionLabel, Is.EqualTo("このセクションをやり直す"));
            Assert.That(menu.RestartGameLabel, Is.EqualTo("最初からやり直す"));
            Assert.That(menu.QuitLabel, Is.EqualTo("ゲームを終了する"));
            menu.RequestRestartSection();
            Assert.That(menu.ConfirmationLabel, Does.Contain("やり直しますか？"));
        }

        [UnityTest]
        public IEnumerator PauseMenuButtonsAcceptPointerAndKeyboardSubmitEvents()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            EventSystem eventSystem = UnityEngine.Object.FindAnyObjectByType<EventSystem>();
            Assert.That(eventSystem, Is.Not.Null);
            Assert.That(eventSystem.GetComponent<InputSystemUIInputModule>(), Is.Not.Null);

            Assert.That(coordinator.SetPaused(true), Is.True);
            Button[] buttons = coordinator.PauseMenu.GetComponentsInChildren<Button>(true);
            Button resume = buttons.Single(value =>
                value.GetComponentInChildren<Text>(true).text == "ゲームに戻る");
            Button quit = buttons.Single(value =>
                value.GetComponentInChildren<Text>(true).text == "ゲームを終了する");
            Assert.That(eventSystem.currentSelectedGameObject, Is.SameAs(resume.gameObject));

            ExecuteEvents.Execute(resume.gameObject, new BaseEventData(eventSystem),
                ExecuteEvents.submitHandler);
            Assert.That(coordinator.State, Is.EqualTo(GameplayState.Playing));
            Assert.That(coordinator.PauseMenu.IsVisible, Is.False);

            Assert.That(coordinator.SetPaused(true), Is.True);
            ExecuteEvents.Execute(quit.gameObject, new PointerEventData(eventSystem),
                ExecuteEvents.pointerClickHandler);
            Assert.That(coordinator.PauseMenu.ConfirmationLabel, Does.Contain("終了しますか？"));
        }

        [UnityTest]
        public IEnumerator CompletedStateShowsJapaneseCompletion()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            for (int section = 0; section < 3; section++) AdvanceSection(coordinator);
            Assert.That(coordinator.State, Is.EqualTo(GameplayState.Completed));
            Assert.That(coordinator.Hud.CurrentStateMessage,
                Does.Contain("すべての実験を完了しました")
                    .And.Contain("過去の自分たちとの同期に成功"));
        }

        [UnityTest]
        public IEnumerator DebugOverlayOffLeavesNoEnglishPlaceholderTutorial()
        {
            SectionTransitionCoordinator coordinator = null;
            yield return Load(value => coordinator = value);
            Phase0DebugOverlay overlay = UnityEngine.Object.FindObjectsByType<Phase0DebugOverlay>()
                .First(value => value.gameObject.activeInHierarchy);
            Assert.That(overlay.IsVisible, Is.False,
                "P3 black-box play must start without the technical debug overlay.");
            overlay.SetVisible(false);
            Assert.That(coordinator.Hud.CurrentTutorial,
                Does.Not.Contain("MOVE").And.Not.Contain("WORK WITH"));
            string[] visibleText = UnityEngine.Object.FindObjectsByType<Text>()
                .Where(value => value.gameObject.activeInHierarchy)
                .Select(value => value.text).ToArray();
            Assert.That(visibleText.Any(value =>
                value.Contains("SECTION 1") || value.Contains("END LOOP")), Is.False);
        }

        private static void AdvanceSection(SectionTransitionCoordinator coordinator)
        {
            coordinator.SetTransitionDurationsForTests(0f, 0f);
            Assert.That(coordinator.BeginSectionCompletion(), Is.True);
            coordinator.CompleteTransitionNowForTests();
        }

        private static IEnumerator Load(System.Action<SectionTransitionCoordinator> loaded)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync("P3_PlayableGreybox", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
            loaded(UnityEngine.Object.FindAnyObjectByType<SectionTransitionCoordinator>());
        }
    }

    public sealed class AlwaysInteractJapaneseInputSource : MonoBehaviour, EchoShift.Input.IInputSource
    {
        public EchoShift.Replay.InputCommand Sample(int tick) =>
            new EchoShift.Replay.InputCommand(
                tick, Vector2.zero, EchoShift.Replay.InputButtonFlags.Interact);
    }
}
