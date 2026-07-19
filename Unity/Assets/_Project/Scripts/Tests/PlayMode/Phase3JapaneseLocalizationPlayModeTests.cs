using System.Collections;
using System.Linq;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
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
            Assert.That(coordinator.Hud.CurrentTutorial, Is.EqualTo("スイッチの上に乗る"));
            Assert.That(coordinator.Hud.IsJapaneseReady, Is.True);
            Assert.That(coordinator.Hud.JapaneseFontName, Is.Not.Empty);
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
            Assert.That(coordinator.Hud.CurrentTutorial, Is.EqualTo("電池を持つ"));
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
}
