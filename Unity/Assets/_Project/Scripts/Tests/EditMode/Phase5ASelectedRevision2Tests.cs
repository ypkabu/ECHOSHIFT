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
    public sealed class Phase5ASelectedRevision2Tests
    {
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
        public void Revision2LogoKeepsReadableOAndBackgroundIndependentVariants()
        {
            string root = AbsoluteAssetPath(Phase5AIdentityPreviewBuilder.Revision2ArtRoot);
            string wordmark = File.ReadAllText(Path.Combine(root, "P5A_R2_Logo_Wordmark.svg"));
            string icon = File.ReadAllText(Path.Combine(root, "P5A_R2_Logo_Icon.svg"));
            string mono = File.ReadAllText(Path.Combine(root, "P5A_R2_Logo_Monochrome.svg"));
            Assert.That(wordmark, Does.Contain(">ECH</text>").And.Contain(">O</text>")
                .And.Contain(">SHIFT</text>"));
            Assert.That(icon, Does.Contain(">O</text>"));
            Assert.That(mono, Does.Contain(">ECH</text>").And.Contain(">O</text>")
                .And.Contain(">SHIFT</text>"));
            Assert.That(wordmark, Does.Not.Contain("fill=\"#07101A\""));
            Assert.That(icon, Does.Not.Contain("fill=\"#07101A\""));
        }

        [Test]
        public void Revision2SceneIsVisualOnlyAndChamberHasPhysicalSupport()
        {
            Scene scene = EditorSceneManager.OpenScene(
                Phase5AIdentityPreviewBuilder.Revision2ScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2 Preview");
                Assert.That(root, Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<StableId>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<LoopDirector>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<CharacterMotor>(true), Is.Empty);

                Transform chamber = FindNamed(root.transform, "Revision 2 Echo Chamber");
                Assert.That(FindNamed(chamber, "Floor Anchored Chamber Base"), Is.Not.Null);
                Assert.That(FindNamed(chamber, "Rear Structural Spine"), Is.Not.Null);
                Assert.That(FindNamed(chamber, "Connected Metal Outer Frame"), Is.Not.Null);
                Assert.That(FindNamed(chamber, "Core Mechanical Support"), Is.Not.Null);
                Assert.That(FindNamed(chamber, "Embedded Generation Indicators"), Is.Not.Null);
                Assert.That(CountNames(chamber, "Hologram Arc"), Is.EqualTo(3));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision2ActorMotifsAreSurfaceIntegratedAndGoalIsDistinct()
        {
            Scene scene = EditorSceneManager.OpenScene(
                Phase5AIdentityPreviewBuilder.Revision2ScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2 Preview");
                string[] actors =
                {
                    "Current Player Surface Motif", "Echo 1 Surface Motif",
                    "Echo 2 Surface Motif", "Echo 3 Surface Motif"
                };
                for (int i = 0; i < actors.Length; i++)
                {
                    Transform actor = FindNamed(root.transform, actors[i]);
                    Assert.That(actor, Is.Not.Null, actors[i]);
                    Assert.That(FindNamed(actor, "Chest Embedded Emission Strip"), Is.Not.Null);
                    Assert.That(FindNamed(actor, "Chest Surface Split Seal"), Is.Not.Null);
                    Assert.That(FindNamed(actor, "Grounded Segmented Identity Marker"), Is.Not.Null);
                    AssertInactiveWhenPresent(actor, "Generation Fin 1");
                    AssertInactiveWhenPresent(actor, "Generation Fin 2");
                    AssertInactiveWhenPresent(actor, "Generation Fin 3");
                    AssertInactiveWhenPresent(actor, "Replay Chevron Left");
                    AssertInactiveWhenPresent(actor, "Replay Chevron Right");
                }

                Transform goalGallery = FindNamed(root.transform, "Revision 2 Goal State Gallery");
                Assert.That(FindNamed(goalGallery, "Locked Goal State"), Is.Not.Null);
                Assert.That(FindNamed(goalGallery, "Unlocked Goal State"), Is.Not.Null);
                Assert.That(CountNames(goalGallery, "Goal Inner Segment"), Is.EqualTo(6));
                Assert.That(goalGallery.GetComponentsInChildren<TextMesh>(true).Length,
                    Is.EqualTo(2));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision2UsesSurfaceDecalsAndPurposefulRoomClusters()
        {
            string atlas = File.ReadAllText(AbsoluteAssetPath(
                $"{Phase5AIdentityPreviewBuilder.Revision2ArtRoot}/P5A_R2_SurfaceDecalAtlas.svg"));
            Assert.That(CountOccurrences(atlas, "id=\"r2-decal-"),
                Is.EqualTo(Phase5AIdentityPreviewBuilder.SelectedDecalCount));
            Assert.That(atlas, Does.Not.Contain("fill=\"#101923\"")
                .And.Not.Contain("<rect"));

            Scene scene = EditorSceneManager.OpenScene(
                Phase5AIdentityPreviewBuilder.Revision2ScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2 Preview");
                Transform maintenance = FindNamed(root.transform,
                    "Revision 2 Maintenance Bay Equipment Cluster");
                Transform observation = FindNamed(root.transform,
                    "Revision 2 Observation Bay Equipment Cluster");
                Assert.That(FindNamed(maintenance, "Wall Connected Battery Rack Assembly"),
                    Is.Not.Null);
                Assert.That(FindNamed(maintenance, "Grounded Maintenance Service Unit"),
                    Is.Not.Null);
                Assert.That(FindNamed(observation, "Wall Embedded Record Monitor"), Is.Not.Null);
                Assert.That(FindNamed(observation, "Grounded Observation Meter"), Is.Not.Null);
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    Assert.That(transforms[i].name, Does.Not.Contain("Floating Board"));
                    Assert.That(transforms[i].name, Does.Not.Contain("Maintenance Board"));
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision2CapturesAndEightIndividualCuesMeetDeliveryBounds()
        {
            string root = Path.Combine(RepositoryRoot(), "Captures", "Phase5A",
                "SelectedRevision2");
            for (int i = 0; i < Phase5AIdentityPreviewBuilder.Revision2Captures.Count; i++)
            {
                string path = Path.Combine(root,
                    Phase5AIdentityPreviewBuilder.Revision2Captures[i]);
                byte[] png = File.ReadAllBytes(path);
                Assert.That(ReadBigEndianInt32(png, 16),
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureWidth), path);
                Assert.That(ReadBigEndianInt32(png, 20),
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureHeight), path);
            }
            string contact = Path.Combine(root, "Phase5A_SelectedRevision2_ContactSheet.png");
            Assert.That(File.Exists(contact), Is.True);

            string audioRoot = Path.Combine(root, "Audio");
            for (int i = 0; i < Phase5AIdentityPreviewBuilder.Revision2Cues.Count; i++)
                AssertWave(Path.Combine(audioRoot, Phase5AIdentityPreviewBuilder.Revision2Cues[i]), 2d);
            AssertWave(Path.Combine(audioRoot, "revised_audio_preview_v2.wav"),
                Phase5AIdentityPreviewBuilder.MaximumAudioSeconds);
            string timeline = File.ReadAllText(Path.Combine(audioRoot, "audio_preview_timeline.md"));
            for (int i = 0; i < Phase5AIdentityPreviewBuilder.Revision2Cues.Count; i++)
                Assert.That(timeline,
                    Does.Contain(Phase5AIdentityPreviewBuilder.Revision2Cues[i]));
        }

        [Test]
        public void Revision2ManifestRetainsProductionSceneBaselines()
        {
            string manifest = File.ReadAllText(AbsoluteAssetPath(
                $"{Phase5AIdentityPreviewBuilder.Revision2ArtRoot}/P5A_R2_ProductionHashes.txt"));
            for (int i = 0; i < ProductionSceneHashes.Length; i++)
                Assert.That(manifest, Does.Contain(
                    $"{ProductionSceneHashes[i].Key}|{ProductionSceneHashes[i].Value}"));
        }

        private static void AssertInactiveWhenPresent(Transform root, string name)
        {
            Transform value = FindNamed(root, name);
            if (value != null) Assert.That(value.gameObject.activeSelf, Is.False, name);
        }

        private static int CountNames(Transform root, string prefix)
        {
            int count = 0;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name.StartsWith(prefix, StringComparison.Ordinal)) count++;
            return count;
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

        private static void AssertWave(string path, double maximumSeconds)
        {
            Assert.That(File.Exists(path), Is.True, path);
            byte[] data = File.ReadAllBytes(path);
            Assert.That(data.Length, Is.GreaterThan(44), path);
            double seconds = (data.Length - 44d) / (44100d * 2d);
            Assert.That(seconds, Is.InRange(0.1d, maximumSeconds), path);
            float peak = 0f;
            for (int offset = 44; offset + 1 < data.Length; offset += 2)
            {
                short value = (short)(data[offset] | data[offset + 1] << 8);
                peak = Mathf.Max(peak, Mathf.Abs(value / 32768f));
            }
            Assert.That(peak, Is.InRange(0.1f, 0.9f), path);
        }

        private static string AbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.StartsWith("Assets/", StringComparison.Ordinal) ?
                assetPath.Substring("Assets/".Length) : assetPath;
            return Path.Combine(Application.dataPath,
                relative.Replace('/', Path.DirectorySeparatorChar));
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
