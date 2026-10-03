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
    public sealed class Phase5ASelectedRevision21Tests
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
                "A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854")
        };

        [Test]
        public void Revision21SceneRemainsVisualOnlyAndProductionIndependent()
        {
            Scene scene = OpenRevision21Scene();
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2.1 Preview");
                Assert.That(root, Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<StableId>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<LoopDirector>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<CharacterMotor>(true), Is.Empty);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision21ActorMotifsStayOnBodyAndFloorSurfaces()
        {
            Scene scene = OpenRevision21Scene();
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2.1 Preview");
                string[] actors =
                {
                    "Current Player Surface Motif", "Echo 1 Surface Motif",
                    "Echo 2 Surface Motif", "Echo 3 Surface Motif"
                };
                for (int i = 0; i < actors.Length; i++)
                {
                    Transform actor = FindNamed(root.transform, actors[i]);
                    Transform strip = FindNamed(actor, "Chest Embedded Emission Strip");
                    Assert.That(strip.localPosition.y, Is.InRange(0.55f, 0.61f), actors[i]);
                    Assert.That(strip.localPosition.z, Is.InRange(-0.5f, -0.48f), actors[i]);
                    Assert.That(strip.localScale.z, Is.LessThanOrEqualTo(0.011f), actors[i]);

                    Transform seal = FindNamed(actor, "Chest Surface Split Seal");
                    Assert.That(seal.localPosition.y, Is.InRange(0.4f, 0.45f), actors[i]);
                    Assert.That(seal.localPosition.z, Is.InRange(-0.5f, -0.48f), actors[i]);
                    Assert.That(seal.localScale.x, Is.LessThanOrEqualTo(0.49f), actors[i]);

                    Transform ticks = FindNamed(actor, "Back Panel Surface Generation Ticks");
                    Renderer[] tickParts = ticks.GetComponentsInChildren<Renderer>(true);
                    for (int j = 0; j < tickParts.Length; j++)
                    {
                        Assert.That(tickParts[j].transform.localPosition.y,
                            Is.InRange(0.6f, 0.7f), tickParts[j].name);
                        Assert.That(tickParts[j].transform.localPosition.z,
                            Is.InRange(0.47f, 0.49f), tickParts[j].name);
                        Assert.That(tickParts[j].transform.localScale.z,
                            Is.LessThanOrEqualTo(0.0081f), tickParts[j].name);
                    }

                    Transform floor = FindNamed(actor, "Grounded Segmented Identity Marker");
                    Renderer[] floorParts = floor.GetComponentsInChildren<Renderer>(true);
                    Assert.That(floorParts.Length, Is.GreaterThanOrEqualTo(5));
                    for (int j = 0; j < floorParts.Length; j++)
                    {
                        Assert.That(floorParts[j].transform.localPosition.y,
                            Is.LessThanOrEqualTo(0.0061f), floorParts[j].name);
                        Assert.That(floorParts[j].transform.localScale.y,
                            Is.LessThanOrEqualTo(0.0061f), floorParts[j].name);
                    }
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision21LockedGoalHasNoLimeAndUnlockedGoalHasExpandedSuccessArea()
        {
            Scene scene = OpenRevision21Scene();
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2.1 Preview");
                Transform locked = FindNamed(root.transform, "Locked Goal State");
                Transform unlocked = FindNamed(root.transform, "Unlocked Goal State");
                Assert.That(FindNamed(locked, "Neutral Goal Registration Marks"), Is.Not.Null);
                Assert.That(FindNamed(unlocked, "Unlocked Success Perimeter Extension"), Is.Not.Null);
                Assert.That(CountMaterialRenderers(locked, "Lime"), Is.EqualTo(0));
                Assert.That(CountMaterialRenderers(unlocked, "Lime"), Is.GreaterThanOrEqualTo(4));

                Transform lockedSignal = FindNamed(locked, "Goal Outer Identity Signal");
                Renderer[] signalParts = lockedSignal.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < signalParts.Length; i++)
                    Assert.That(signalParts[i].transform.localScale.z,
                        Is.GreaterThanOrEqualTo(0.099f));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision21LogoEvidenceContainsSeparateExact64AndFourTimesSamples()
        {
            Scene scene = OpenRevision21Scene();
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2.1 Preview");
                RectTransform actual = FindNamed(root.transform,
                    "Actual 64 Pixel Sample") as RectTransform;
                RectTransform actualIcon = FindNamed(root.transform,
                    "Actual 64 Square Icon Sample") as RectTransform;
                RectTransform enlarged = FindNamed(root.transform,
                    "Enlarged 64 Pixel Inspection") as RectTransform;
                Assert.That(actual, Is.Not.Null);
                Assert.That(actualIcon, Is.Not.Null);
                Assert.That(enlarged, Is.Not.Null);
                Assert.That(actual.sizeDelta, Is.EqualTo(new Vector2(205f, 64f)));
                Assert.That(actualIcon.sizeDelta, Is.EqualTo(new Vector2(64f, 64f)));
                Assert.That(enlarged.sizeDelta, Is.EqualTo(new Vector2(820f, 256f)));
                Assert.That(Mathf.Abs(actual.anchoredPosition.y - enlarged.anchoredPosition.y),
                    Is.GreaterThan(300f));
                Assert.That(FindNamed(root.transform, "Actual 64 Pixel Label"), Is.Not.Null);
                Assert.That(FindNamed(root.transform, "Actual 64 Square Icon Label"), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision21LowRiskP2ChangesOnlyAdjustArcAndMonitorHierarchy()
        {
            Scene scene = OpenRevision21Scene();
            try
            {
                GameObject root = FindRoot(scene, "Phase5A Selected Revision 2.1 Preview");
                Transform arcs = FindNamed(root.transform, "Supported Internal Hologram Arcs");
                Renderer[] arcParts = arcs.GetComponentsInChildren<Renderer>(true);
                Assert.That(arcParts.Length, Is.EqualTo(12));
                for (int i = 0; i < arcParts.Length; i++)
                    Assert.That(arcParts[i].sharedMaterial.name, Does.Contain("Arc").And.Contain("Dim"));

                Transform monitor = FindNamed(root.transform, "Monitor Recess");
                Assert.That(monitor.GetComponent<Renderer>().sharedMaterial.name,
                    Does.Contain("R21_MonitorFace"));
                Assert.That(FindNamed(root.transform, "Observation Cable Status"), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Revision21CapturesMeetCountAndExactResolution()
        {
            string root = Path.Combine(RepositoryRoot(), "Captures", "Phase5A",
                "SelectedRevision21");
            Assert.That(Phase5AIdentityPreviewBuilder.Revision21Captures.Count,
                Is.EqualTo(Phase5AIdentityPreviewBuilder.Revision21CaptureCount));
            for (int i = 0; i < Phase5AIdentityPreviewBuilder.Revision21Captures.Count; i++)
            {
                string path = Path.Combine(root,
                    Phase5AIdentityPreviewBuilder.Revision21Captures[i]);
                byte[] png = File.ReadAllBytes(path);
                Assert.That(ReadBigEndianInt32(png, 16),
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureWidth), path);
                Assert.That(ReadBigEndianInt32(png, 20),
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureHeight), path);
            }
        }

        [Test]
        public void Revision21AudioChangesOnlySectionCompleteAndDoesNotClip()
        {
            string root = Path.Combine(RepositoryRoot(), "Captures", "Phase5A",
                "SelectedRevision21", "Audio");
            string spawn = Path.Combine(root, "echo_spawn_03.wav");
            string section = Path.Combine(root, "section_complete.wav");
            string comparison = Path.Combine(root, "spawn03_section_complete_comparison.wav");
            WaveMetrics spawnMetrics = ReadWave(spawn);
            WaveMetrics sectionMetrics = ReadWave(section);
            WaveMetrics comparisonMetrics = ReadWave(comparison);
            Assert.That(spawnMetrics.Seconds, Is.InRange(1.04d, 1.06d));
            Assert.That(sectionMetrics.Seconds, Is.InRange(1.94d, 1.96d));
            Assert.That(sectionMetrics.Seconds, Is.GreaterThan(spawnMetrics.Seconds + 0.8d));
            Assert.That(spawnMetrics.Peak, Is.InRange(0.1f, 0.9f));
            Assert.That(sectionMetrics.Peak, Is.InRange(0.1f, 0.9f));
            Assert.That(comparisonMetrics.Peak, Is.InRange(0.1f, 0.9f));
            Assert.That(spawnMetrics.Clipping + sectionMetrics.Clipping +
                comparisonMetrics.Clipping, Is.EqualTo(0));
            Assert.That(File.Exists(Path.Combine(root, "revision21_audio_evidence.md")), Is.True);
        }

        [Test]
        public void Revision21ManifestRetainsProductionSceneBaselines()
        {
            string manifest = File.ReadAllText(AbsoluteAssetPath(
                $"{Phase5AIdentityPreviewBuilder.Revision21ArtRoot}/P5A_R21_ProductionHashes.txt"));
            for (int i = 0; i < ProductionSceneHashes.Length; i++)
            {
                Assert.That(manifest, Does.Contain(
                    $"{ProductionSceneHashes[i].Key}|{ProductionSceneHashes[i].Value}"));
                using (System.Security.Cryptography.SHA256 sha =
                       System.Security.Cryptography.SHA256.Create())
                {
                    string actual = BitConverter.ToString(sha.ComputeHash(
                        File.ReadAllBytes(AbsoluteAssetPath(ProductionSceneHashes[i].Key))))
                        .Replace("-", string.Empty);
                    Assert.That(actual, Is.EqualTo(ProductionSceneHashes[i].Value),
                        ProductionSceneHashes[i].Key);
                }
            }
        }

        private static Scene OpenRevision21Scene()
        {
            return EditorSceneManager.OpenScene(
                Phase5AIdentityPreviewBuilder.Revision21ScenePath, OpenSceneMode.Additive);
        }

        private static int CountMaterialRenderers(Transform root, string token)
        {
            int count = 0;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].sharedMaterial != null &&
                    renderers[i].sharedMaterial.name.IndexOf(token,
                        StringComparison.OrdinalIgnoreCase) >= 0) count++;
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

        private static WaveMetrics ReadWave(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            Assert.That(data.Length, Is.GreaterThan(44), path);
            float peak = 0f;
            int clipping = 0;
            for (int offset = 44; offset + 1 < data.Length; offset += 2)
            {
                short value = (short)(data[offset] | data[offset + 1] << 8);
                float amplitude = Mathf.Abs(value / 32768f);
                peak = Mathf.Max(peak, amplitude);
                if (amplitude >= 0.999f) clipping++;
            }
            return new WaveMetrics((data.Length - 44d) / (44100d * 2d), peak, clipping);
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

        private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
            (bytes[offset + 2] << 8) | bytes[offset + 3];

        private readonly struct WaveMetrics
        {
            public WaveMetrics(double seconds, float peak, int clipping)
            {
                Seconds = seconds;
                Peak = peak;
                Clipping = clipping;
            }

            public double Seconds { get; }
            public float Peak { get; }
            public int Clipping { get; }
        }
    }
}
