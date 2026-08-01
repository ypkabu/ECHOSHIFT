using System;
using System.Collections.Generic;
using System.IO;
using EchoShift.Core;
using EchoShift.Editor;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoShift.Tests
{
    public sealed class Phase5ASelectedRevisionTests
    {
        private static readonly string[] CaptureNames =
        {
            "01_revised_logo.png",
            "02_simplified_echo_chamber.png",
            "03_player_echo_hierarchy.png",
            "04_door_battery_goal.png",
            "05_section3_overview.png",
            "06_ui_copy.png",
            "07_decal_closeup.png"
        };

        private static readonly KeyValuePair<string, string>[] ProductionSceneHashes =
        {
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P0_ReplayLab.unity",
                "1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5"),
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P1_InteractionLab.unity",
                "4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB"),
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P2_CoordinationLab.unity",
                "73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C"),
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P3_PlayableGreybox.unity",
                "4ACA5F734DB1C754E7C237CA6EE819B30F8F50B53BB5C37298C31C92F36961E8")
        };

        [Test]
        public void SelectedDirectionUsesExactDecalAndJapaneseCopySets()
        {
            Assert.That(Phase5AIdentityPreviewBuilder.SelectedDecals.Count,
                Is.EqualTo(Phase5AIdentityPreviewBuilder.SelectedDecalCount));
            Assert.That(Phase5AIdentityPreviewBuilder.SelectedDecals,
                Does.Contain("ECHO//SHIFT")
                    .And.Contain("RECORD SECTOR 03")
                    .And.Contain("TEMPORAL HAZARD")
                    .And.Contain("ES-PHASE-05A-031"));
            Assert.That(Phase5AIdentityPreviewBuilder.SelectedCopy,
                Does.Contain("記録区画 03")
                    .And.Contain("2体のエコーと協力して出口を開く")
                    .And.Contain("記録 02")
                    .And.Contain("エコー 2/3"));
        }

        [Test]
        public void SelectedSceneIsVisualOnlyAndUsesBoundedShapeLanguage()
        {
            Scene scene = EditorSceneManager.OpenScene(
                Phase5AIdentityPreviewBuilder.SelectedScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision Preview");
                Assert.That(root, Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<StableId>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<LoopDirector>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<CharacterMotor>(true), Is.Empty);

                Transform chamber = FindNamed(root.transform, "Selected Echo Chamber");
                Assert.That(chamber, Is.Not.Null);
                AssertChildCount(chamber, "Outer Split Hex Seal", 5);
                AssertChildCount(chamber, "Inner Phase Arc 1", 3);
                AssertChildCount(chamber, "Inner Phase Arc 2", 3);
                AssertChildCount(chamber, "Inner Phase Arc 3", 3);
                AssertChildCount(chamber, "Generation Phase Ticks", 3);

                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    string name = transforms[i].name;
                    Assert.That(name, Does.Not.Contain("Relay"));
                    Assert.That(name, Does.Not.Contain("Sun"));
                    Assert.That(name, Does.Not.Contain("Floating Board"));
                }

                string builderSource = File.ReadAllText(AbsoluteAssetPath(
                    "Assets/_Project/Scripts/Editor/Phase5ASelectedRevisionBuilder.cs"));
                Assert.That(builderSource, Does.Not.Contain("Afterimage Relay"));
                Assert.That(builderSource, Does.Not.Contain("Relay Chevron"));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void EchoGenerationsHaveDistinctNonTextIdentityValues()
        {
            Scene scene = EditorSceneManager.OpenScene(
                Phase5AIdentityPreviewBuilder.SelectedScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision Preview");
                for (int generation = 1; generation <= 3; generation++)
                {
                    Transform crest = FindNamed(root.transform,
                        $"Echo {generation} Identity Crest");
                    Assert.That(crest, Is.Not.Null, $"Echo {generation} crest is missing.");
                    AssertChildCount(crest, "Generation Tick Count", generation);
                    Assert.That(crest.GetComponentsInChildren<TextMesh>(true), Is.Empty,
                        $"Echo {generation} identity must not use a world-space generation label.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void SelectedAtlasCapturesAndAudioMeetDeliveryBounds()
        {
            string atlasPath = AbsoluteAssetPath(
                $"{Phase5AIdentityPreviewBuilder.SelectedArtRoot}/P5A_Selected_DecalAtlas.svg");
            string svg = File.ReadAllText(atlasPath);
            Assert.That(svg, Does.Contain("width=\"1024\"").And.Contain("height=\"1024\""));
            Assert.That(CountOccurrences(svg, "id=\"selected-decal-"),
                Is.EqualTo(Phase5AIdentityPreviewBuilder.SelectedDecalCount));

            string directory = Path.Combine(RepositoryRoot(), "Captures", "Phase5A",
                "SelectedRevision");
            Assert.That(Directory.GetFiles(directory, "0*.png", SearchOption.TopDirectoryOnly).Length,
                Is.EqualTo(Phase5AIdentityPreviewBuilder.SelectedCaptureCount));
            for (int i = 0; i < CaptureNames.Length; i++)
            {
                string path = Path.Combine(directory, CaptureNames[i]);
                byte[] png = File.ReadAllBytes(path);
                Assert.That(ReadBigEndianInt32(png, 16),
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureWidth), path);
                Assert.That(ReadBigEndianInt32(png, 20),
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureHeight), path);
            }

            string audio = Path.Combine(directory, "revised_audio_preview.wav");
            Assert.That(File.Exists(audio), Is.True);
            double seconds = (new FileInfo(audio).Length - 44d) / (44100d * 2d);
            Assert.That(seconds, Is.LessThanOrEqualTo(
                Phase5AIdentityPreviewBuilder.MaximumAudioSeconds));
        }

        [Test]
        public void ProductionP0ThroughP3ScenesRemainByteIdentical()
        {
            string manifestPath = AbsoluteAssetPath(
                $"{Phase5AIdentityPreviewBuilder.SelectedArtRoot}/" +
                "P5A_Selected_ProductionHashes.txt");
            string manifest = File.ReadAllText(manifestPath);
            for (int i = 0; i < ProductionSceneHashes.Length; i++)
                Assert.That(manifest, Does.Contain(
                    $"{ProductionSceneHashes[i].Key}|{ProductionSceneHashes[i].Value}"));
        }

        private static void AssertChildCount(Transform root, string name, int expected)
        {
            Transform child = FindNamed(root, name);
            Assert.That(child, Is.Not.Null, name);
            Assert.That(child.childCount, Is.EqualTo(expected), name);
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                if (roots[i].name == name) return roots[i];
            return null;
        }

        private static Transform FindNamed(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == name) return transforms[i];
            return null;
        }

        private static string AbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length)
                .Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }

        private static string RepositoryRoot() =>
            Directory.GetParent(Application.dataPath)?.Parent?.FullName;

        private static int CountOccurrences(string value, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = value.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }
            return count;
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
            (bytes[offset + 2] << 8) | bytes[offset + 3];
    }
}
