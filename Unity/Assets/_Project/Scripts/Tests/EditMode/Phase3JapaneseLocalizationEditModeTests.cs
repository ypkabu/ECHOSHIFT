using System;
using System.Linq;
using EchoShift.Core;
using EchoShift.Editor;
using EchoShift.Gameplay;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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

            UnityEditor.SerializedObject serializedCatalog = new UnityEditor.SerializedObject(catalog);
            serializedCatalog.FindProperty("failureTargetBusy").stringValue = "Busy override";
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(catalog.GetFailureText(InteractionFailureReason.TargetBusy),
                Is.EqualTo("Busy override"),
                "Failure text must remain replaceable through the serialized catalog.");
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
            Assert.That(catalog.GetInteractionPrompt(InteractionKind.PickupBattery, false),
                Is.EqualTo("E：オレンジの電池を持つ"));
            Assert.That(catalog.GetInteractionPrompt(InteractionKind.InsertBattery, false),
                Is.EqualTo("E：紫の電源に電池を入れる"));
            Assert.That(catalog.GetInteractionPrompt(InteractionKind.DropBattery, true),
                Is.EqualTo("A / ×：電池を置く"));
            Assert.That(catalog.EndLoop, Does.Contain("ループを終了"));
            Assert.That(catalog.PausePrompt, Does.Contain("一時停止"));
            Assert.That(catalog.StartPrompt, Does.Contain("開始"));
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
            SectionTransitionCoordinator coordinator = UnityEngine.Object
                .FindObjectsByType<SectionTransitionCoordinator>(FindObjectsInactive.Include)
                .Single();
            Assert.That(coordinator.WaitForInteractiveStart, Is.True);
            EventSystem eventSystem = UnityEngine.Object
                .FindObjectsByType<EventSystem>(FindObjectsInactive.Include).Single();
            Assert.That(eventSystem.GetComponent<InputSystemUIInputModule>(), Is.Not.Null,
                "The generated Standalone needs a UI input module for mouse and keyboard menus.");

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

        [Test]
        public void BuilderAuthorsReadableFramingLabelsAndLongerPhase3Loop()
        {
            P3SceneBuilder.BuildScene();
            LoopSettings settings = UnityEditor.AssetDatabase.LoadAssetAtPath<LoopSettings>(
                "Assets/_Project/Settings/LoopSettings_P3.asset");
            Assert.That(settings.LoopDurationSeconds, Is.EqualTo(45));
            Phase3CameraSettings cameraSettings = UnityEditor.AssetDatabase
                .LoadAssetAtPath<Phase3CameraSettings>(
                    "Assets/_Project/Settings/Phase3CameraSettings.asset");
            Quaternion expectedLabelRotation = Quaternion.LookRotation(
                cameraSettings.LookOffset - cameraSettings.Offset, Vector3.up);

            Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include);
            Transform southVisual = transforms.First(value =>
                value.gameObject.activeInHierarchy && value.name == "Wall South");
            Transform southBoundary = transforms.First(value =>
                value.gameObject.activeInHierarchy && value.name == "South Boundary Collider");
            Assert.That(southVisual.localScale.y, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(southVisual.GetComponent<Collider>(), Is.Null);
            Assert.That(southBoundary.GetComponent<BoxCollider>().size.y,
                Is.EqualTo(3f).Within(0.001f));
            TutorialTrigger[] triggers = UnityEngine.Object.FindObjectsByType<TutorialTrigger>(
                FindObjectsInactive.Include);
            Assert.That(triggers, Has.Length.EqualTo(1));
            Assert.That(triggers[0].transform.localPosition,
                Is.EqualTo(new Vector3(-2f, 1f, -3f)));
            UnityEditor.SerializedObject serializedTrigger =
                new UnityEditor.SerializedObject(triggers[0]);
            Assert.That(serializedTrigger.FindProperty("step").intValue, Is.EqualTo(1));

            string[] labels = UnityEngine.Object.FindObjectsByType<TextMesh>(
                    FindObjectsInactive.Include)
                .Select(value => value.text).ToArray();
            Assert.That(labels, Does.Contain("自分"));
            Assert.That(labels, Does.Contain("スイッチ"));
            Assert.That(labels, Does.Contain("電池"));
            Assert.That(labels, Does.Contain("電源"));
            Assert.That(labels, Does.Contain("扉"));
            Assert.That(labels, Does.Contain("出口"));
            WorldBillboardLabel[] billboards = UnityEngine.Object
                .FindObjectsByType<WorldBillboardLabel>(FindObjectsInactive.Include);
            Assert.That(billboards, Is.Not.Empty);
            Assert.That(billboards.All(value =>
                Quaternion.Angle(value.WorldRotation, expectedLabelRotation) < 0.01f), Is.True);

            GameObject echoPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Actors/P3_Echo.prefab");
            EchoVisualFeedback feedback = echoPrefab.GetComponent<EchoVisualFeedback>();
            Assert.That(feedback, Is.Not.Null);
            Assert.That(echoPrefab.transform.Find("Echo Identity Label"), Is.Not.Null);
            Assert.That(echoPrefab.transform.Find("Echo Identity Ring"), Is.Not.Null);
            WorldBillboardLabel echoBillboard = echoPrefab
                .transform.Find("Echo Identity Label").GetComponent<WorldBillboardLabel>();
            Assert.That(Quaternion.Angle(
                echoBillboard.WorldRotation, expectedLabelRotation), Is.LessThan(0.01f));
        }

        [Test]
        public void BuilderAuthorsUnitScaleWorldLabelsAndNonOverlappingHudSafeArea()
        {
            P3SceneBuilder.BuildScene();
            WorldBillboardLabel[] billboards = UnityEngine.Object
                .FindObjectsByType<WorldBillboardLabel>(FindObjectsInactive.Include);
            Assert.That(billboards, Is.Not.Empty);
            foreach (WorldBillboardLabel billboard in billboards)
            {
                Assert.That(billboard.transform.lossyScale.x, Is.EqualTo(1f).Within(0.001f),
                    billboard.name);
                Assert.That(billboard.transform.lossyScale.y, Is.EqualTo(1f).Within(0.001f),
                    billboard.name);
                Assert.That(billboard.transform.lossyScale.z, Is.EqualTo(1f).Within(0.001f),
                    billboard.name);
            }

            TextMesh[] labels = UnityEngine.Object.FindObjectsByType<TextMesh>(
                FindObjectsInactive.Include);
            TextMesh[] exitLabels = labels.Where(value => value.text == "出口").ToArray();
            TextMesh[] doorLabels = labels.Where(value => value.text == "扉").ToArray();
            Assert.That(exitLabels, Has.Length.EqualTo(3));
            Assert.That(doorLabels, Has.Length.EqualTo(4));
            Assert.That(exitLabels.All(value => value.transform.position.y >= 2.25f), Is.True);
            Assert.That(doorLabels.All(value => value.transform.position.y >= 3.5f), Is.True);

            GameplayHud hud = UnityEngine.Object.FindObjectsByType<GameplayHud>(
                FindObjectsInactive.Include).Single();
            string[] rowNames = { "Tutorial", "Prompt", "EndLoop", "PausePrompt" };
            RectTransform[] rows = rowNames.Select(name =>
                hud.transform.Find(name).GetComponent<RectTransform>()).ToArray();
            foreach (RectTransform row in rows)
            {
                Assert.That(row.anchorMin, Is.EqualTo(Vector2.one), row.name);
                Assert.That(row.anchorMax, Is.EqualTo(Vector2.one), row.name);
                Assert.That(row.pivot, Is.EqualTo(Vector2.one), row.name);
            }
            for (int i = 0; i < rows.Length; i++)
            for (int j = i + 1; j < rows.Length; j++)
            {
                float firstTop = rows[i].anchoredPosition.y;
                float firstBottom = firstTop - rows[i].sizeDelta.y;
                float secondTop = rows[j].anchoredPosition.y;
                float secondBottom = secondTop - rows[j].sizeDelta.y;
                float overlap = Mathf.Min(firstTop, secondTop) -
                                Mathf.Max(firstBottom, secondBottom);
                Assert.That(overlap, Is.LessThanOrEqualTo(0f),
                    $"HUD safe-area rows overlap: {rows[i].name} / {rows[j].name}");
            }
        }

        [Test]
        public void TutorialProgressCanResetASectionForRestart()
        {
            TutorialProgress progress = new TutorialProgress();
            progress.BeginSection(0);
            Assert.That(progress.Advance(2), Is.True);
            Assert.That(progress.CurrentStep, Is.EqualTo(3));
            progress.ResetSection(0);
            Assert.That(progress.CurrentStep, Is.Zero);
            Assert.That(progress.Advance(1), Is.True);
        }

        private static Phase3TextCatalog JapaneseCatalog()
        {
            Phase3TextCatalog catalog = ScriptableObject.CreateInstance<Phase3TextCatalog>();
            catalog.ApplyJapaneseDefaults();
            return catalog;
        }
    }
}
