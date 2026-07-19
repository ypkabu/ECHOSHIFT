using System;
using System.Linq;
using EchoShift.Editor;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.Tests
{
    public sealed class Phase3JapaneseLocalizationEditModeTests
    {
        [Test]
        public void RequiredJapanesePlayerTextHasNoEmptyValue()
        {
            Phase3TextCatalog catalog = JapaneseCatalog();
            Assert.That(catalog.LanguageCode, Is.EqualTo("ja-JP"));
            Assert.That(catalog.HasNoEmptyValues(), Is.True);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void PlayerFacingCatalogContainsNoLegacyEnglishDisplayText()
        {
            Phase3TextCatalog catalog = JapaneseCatalog();
            Assert.That(catalog.ContainsLegacyEnglishPlayerText(), Is.False);
            Assert.That(catalog.EnumeratePlayerFacingTexts().Any(value =>
                value.Contains("SECTION") || value.Contains("INTERACT")), Is.False);
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void EveryInteractionFailureReasonMapsToJapanesePlayerText()
        {
            Phase3TextCatalog catalog = JapaneseCatalog();
            foreach (InteractionFailureReason reason in Enum.GetValues(typeof(InteractionFailureReason)))
            {
                string message = catalog.GetFailureText(reason);
                Assert.That(message, Is.Not.Empty, reason.ToString());
                Assert.That(message, Does.Match("[ぁ-んァ-ヶ一-龯]"), reason.ToString());
                Assert.That(message, Is.Not.EqualTo(reason.ToString()), reason.ToString());
            }
            Assert.That(catalog.GetFailureText(InteractionFailureReason.TargetBusy),
                Is.EqualTo("ほかのエコーが使用中です"));
            Assert.That(catalog.GetFailureText(InteractionFailureReason.OutOfRange),
                Is.EqualTo("もう少し近づいてください"));
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void KeyboardAndGamepadPromptsSwitchInsideJapaneseText()
        {
            Phase3TextCatalog catalog = JapaneseCatalog();
            Assert.That(catalog.MoveKeyboard, Does.StartWith("WASD：").And.Contain("移動"));
            Assert.That(catalog.MoveGamepad, Does.Contain("左スティック").And.Contain("移動"));
            Assert.That(catalog.InteractKeyboard, Does.StartWith("E：").And.Contain("装置を調べる"));
            Assert.That(catalog.InteractGamepad, Does.Contain("A / ×").And.Contain("装置を調べる"));
            Assert.That(catalog.EndLoop, Does.Contain("ループを終了"));
            Assert.That(catalog.PausePrompt, Does.Contain("一時停止"));
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void TextCatalogKeysAreUnique()
        {
            Assert.That(Phase3TextCatalog.HasUniqueKeys(), Is.True);
            Assert.That(Phase3TextCatalog.PlayerFacingTextKeys.Distinct(StringComparer.Ordinal).Count(),
                Is.EqualTo(Phase3TextCatalog.PlayerFacingTextKeys.Count));
        }

        [Test]
        public void BuilderRerunPreservesJapaneseCatalogAndFontReferences()
        {
            P3SceneBuilder.BuildScene();
            P3SceneBuilder.BuildScene();
            GameplayHud hud = UnityEngine.Object.FindObjectsByType<GameplayHud>(FindObjectsInactive.Include)
                .Single();
            JapaneseFontApplier font = UnityEngine.Object
                .FindObjectsByType<JapaneseFontApplier>(FindObjectsInactive.Include).Single();
            Assert.That(hud.HasValidReferences, Is.True);
            Assert.That(hud.TextCatalog.LanguageCode, Is.EqualTo("ja-JP"));
            Assert.That(hud.TextCatalog.ContainsLegacyEnglishPlayerText(), Is.False);
            Assert.That(font.ApplyNow(), Is.True);
            Assert.That(font.HasRequiredGlyphs, Is.True);

            string[] legacy = { "RESUME", "RESTART SECTION", "QUIT TO DESKTOP", "SWITCH", "CELL", "GATE", "EXIT" };
            string[] sceneText = UnityEngine.Object.FindObjectsByType<Text>(FindObjectsInactive.Include)
                .Select(value => value.text)
                .Concat(UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsInactive.Include)
                    .Select(value => value.text))
                .Where(value => !string.IsNullOrEmpty(value)).ToArray();
            foreach (string value in sceneText)
            foreach (string token in legacy)
                Assert.That(value, Does.Not.Contain(token));
        }

        [Test]
        public void JapaneseHudAndPauseLayoutUseResponsiveWrappingAndButtonFit()
        {
            P3SceneBuilder.BuildScene();
            CanvasScaler[] scalers = UnityEngine.Object
                .FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include);
            Assert.That(scalers, Has.Length.EqualTo(2));
            foreach (CanvasScaler scaler in scalers)
            {
                Assert.That(scaler.uiScaleMode,
                    Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
                Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
                Assert.That(scaler.matchWidthOrHeight, Is.InRange(0f, 0.5f));
            }

            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            Assert.That(buttons, Has.Length.EqualTo(4));
            foreach (Button button in buttons)
            {
                Assert.That(button.GetComponent<RectTransform>().sizeDelta.x,
                    Is.GreaterThanOrEqualTo(540f));
                Text label = button.GetComponentInChildren<Text>(true);
                Assert.That(label.resizeTextForBestFit, Is.True);
                Assert.That(label.horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Wrap));
            }
        }

        private static Phase3TextCatalog JapaneseCatalog()
        {
            Phase3TextCatalog catalog = ScriptableObject.CreateInstance<Phase3TextCatalog>();
            catalog.ApplyJapaneseDefaults();
            return catalog;
        }
    }
}
