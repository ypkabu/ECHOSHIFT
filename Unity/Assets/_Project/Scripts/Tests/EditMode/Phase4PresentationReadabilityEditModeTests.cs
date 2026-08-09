using System.Collections.Generic;
using System.IO;
using EchoShift.Editor;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EchoShift.Tests.EditMode
{
    public sealed class Phase4PresentationReadabilityEditModeTests
    {
        private Phase4VisualSettings Settings =>
            AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(Phase4AssetBuilder.SettingsPath);

        private Phase4CapturePreset Preset =>
            AssetDatabase.LoadAssetAtPath<Phase4CapturePreset>(Phase4AssetBuilder.CapturePresetPath);

        [Test]
        public void CapturePresetDefinesEightUniqueValidCompositions()
        {
            Assert.That(Preset, Is.Not.Null);
            Assert.That(Preset.IsValid(), Is.True);
            Assert.That(Preset.Count, Is.EqualTo(Phase4CapturePreset.RequiredShotCount));
            HashSet<Phase4CaptureMoment> moments = new HashSet<Phase4CaptureMoment>();
            for (int i = 0; i < Preset.Count; i++)
            {
                Phase4CaptureShotPreset shot = Preset.GetShot(i);
                Assert.That(moments.Add(shot.Moment), Is.True, shot.FileName);
                Assert.That(shot.FileName, Does.EndWith(".png"));
            }
        }

        [Test]
        public void CaptureOutputUsesFullHdPhaseFourTwoDirectory()
        {
            Assert.That(Settings.CaptureWidth, Is.EqualTo(1920));
            Assert.That(Settings.CaptureHeight, Is.EqualTo(1080));
            Assert.That(Settings.CaptureDirectory, Is.EqualTo("Captures/Phase4_2"));
        }

        [Test]
        public void OnlyTheHudShowcaseEnablesGameplayHud()
        {
            for (int i = 0; i < Preset.Count; i++)
            {
                Phase4CaptureShotPreset shot = Preset.GetShot(i);
                Assert.That(shot.ShowGameplayHud,
                    Is.EqualTo(shot.Moment == Phase4CaptureMoment.GameplayHud),
                    shot.FileName);
            }
        }

        [Test]
        public void ActorAndInteractionAccentsKeepEmissionEnabled()
        {
            Material[] accents =
            {
                Settings.PlayerMaterial,
                Settings.GetEchoMaterial(1),
                Settings.GetEchoMaterial(2),
                Settings.GetEchoMaterial(3),
                Settings.BatteryMaterial,
                Settings.PlateMaterial,
                Settings.GoalMaterial
            };
            for (int i = 0; i < accents.Length; i++)
            {
                Assert.That(accents[i].IsKeywordEnabled("_EMISSION"), Is.True,
                    accents[i].name);
                Assert.That(accents[i].GetColor("_EmissionColor").maxColorComponent,
                    Is.GreaterThan(0f), accents[i].name);
            }
        }

        [Test]
        public void CapturePipelineUsesRecordedRoutesAndValidatesActorFraming()
        {
            string source = File.ReadAllText(Path.Combine(Application.dataPath,
                "_Project/Scripts/Editor/Phase4CapturePipeline.cs"));
            Assert.That(source, Does.Contain("AdvanceOneTickForTests"));
            Assert.That(source, Does.Contain("InsertedByReplay"));
            Assert.That(source, Does.Contain("ValidateActorFraming"));
            Assert.That(source, Does.Contain("WorldToViewportPoint"));
            Assert.That(source, Does.Not.Contain("Object.Instantiate"));
            Assert.That(source, Does.Not.Contain("SetPositionAndRotation"));
        }
    }
}
