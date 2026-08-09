using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using EchoShift.Editor;
using EchoShift.Gameplay;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoShift.Tests
{
    public sealed class Phase5AProductionApplicationEditModeTests
    {
        private static readonly KeyValuePair<string, string>[] ProtectedSceneHashes =
        {
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P0_ReplayLab.unity",
                "1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5"),
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P1_InteractionLab.unity",
                "4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB"),
            new KeyValuePair<string, string>("Assets/_Project/Scenes/P2_CoordinationLab.unity",
                "73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C")
        };

        private static readonly KeyValuePair<string, string>[] ApprovedAudioHashes =
        {
            Pair("echo_spawn_01.wav", "80B9D7EA143D2A04770A3D1674BEC6BEA5E7B8DF922F269E6314EC5CB6AB41C3"),
            Pair("echo_spawn_02.wav", "191F2017CA1DB75FB29AFDE4BA04BBAF8D0411B5BB08512266631C7A01ACDFF9"),
            Pair("echo_spawn_03.wav", "E1D8C5624FA94FE07137ABE932132CDB87509F700460D07EC95ABAFB9EA13E58"),
            Pair("echo_remove.wav", "F29BC367844D9BA17A841C14E1FAD5C7C54C5E66ADDEB9FB80203227B8765694"),
            Pair("loop_end.wav", "3AFAC17282AB377747F8BBEB36D82B510FE4D42AE54D8832BA478E34C1905C73"),
            Pair("interaction_success.wav", "311D2C78C190593645628B60BDC8E05B83B74E7B8F6B97A9195D4D10C28C2556"),
            Pair("battery_insert.wav", "496EE8DAAEB920D51BE26ACE71410DF62700CAAE17A8E614635CD7594FF54528"),
            Pair("section_complete.wav", "3A0317258FCE13ED0F4BF59F08D32156E3D73EFAAD9B172417082C78B8504B7D")
        };

        [Test]
        public void ApprovedIdentityIsAppliedOnlyAsVisualPresentation()
        {
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                PuzzleSectionController section = FindSection(scene, 3);
                Transform root = section.transform.Find(
                    Phase5AIdentityPreviewBuilder.ProductionIdentityRootName);
                Assert.That(root, Is.Not.Null);
                Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
                Assert.That(FindNamed(root, "Approved Echo Chamber"), Is.Not.Null);
                Assert.That(FindNamed(root, "Approved Maintenance Identity"), Is.Not.Null);
                Assert.That(FindNamed(root, "Approved Observation Identity"), Is.Not.Null);
                Assert.That(FindNamed(root, "Approved Floor Surface Decal"), Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ActorMotifUsesApprovedSurfaceAndFloorDimensions()
        {
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                PuzzleSectionController[] sections = FindSections(scene);
                for (int i = 0; i < sections.Length; i++)
                {
                    Transform model = sections[i].Player.transform.Find("P4 Robot Visual");
                    AssertApprovedMotif(model);
                    Assert.That(FindNamed(model, "Generation Fin 1"), Is.Null);
                }
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/Actors/P3_Echo.prefab");
                AssertApprovedMotif(prefab.transform.Find("P4 Robot Visual"));
                Assert.That(FindNamed(prefab.transform, "Generation Fin 1"), Is.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GoalKeepsLimeOutOfLockedStateAndUsesItWhenUnlocked()
        {
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                PuzzleSectionController[] sections = FindSections(scene);
                for (int i = 0; i < sections.Length; i++)
                {
                    Phase5AGoalIdentityVisual state =
                        sections[i].Goal.GetComponent<Phase5AGoalIdentityVisual>();
                    Assert.That(state, Is.Not.Null);
                    Assert.That(state.HasRequiredReferences, Is.True);
                    Transform root = sections[i].Goal.transform.Find(
                        "Phase 5A Approved Goal Identity");
                    Transform locked = FindNamed(root, "Locked Goal State");
                    Transform unlocked = FindNamed(root, "Unlocked Goal State");
                    Assert.That(CountMaterial(locked, "Lime"), Is.Zero);
                    Assert.That(CountMaterial(unlocked, "Lime"), Is.GreaterThanOrEqualTo(3));
                    Assert.That(locked.gameObject.activeSelf, Is.True);
                    Assert.That(unlocked.gameObject.activeSelf, Is.False);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ApprovedAudioFilesAndEventSlotsAreExact()
        {
            for (int i = 0; i < ApprovedAudioHashes.Length; i++)
            {
                string assetPath = Phase5AIdentityPreviewBuilder.ProductionAudioRoot + "/" +
                    ApprovedAudioHashes[i].Key;
                Assert.That(AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath), Is.Not.Null);
                Assert.That(Hash(assetPath), Is.EqualTo(ApprovedAudioHashes[i].Value));
            }
            Phase4AudioCueSet set = AssetDatabase.LoadAssetAtPath<Phase4AudioCueSet>(
                Phase5AIdentityPreviewBuilder.ProductionAudioSetPath);
            Assert.That(set, Is.Not.Null);
            AssertCue(set, Phase4AudioCue.EchoSpawn, "echo_spawn_01.wav");
            AssertCue(set, Phase4AudioCue.EchoSpawn2, "echo_spawn_02.wav");
            AssertCue(set, Phase4AudioCue.EchoSpawn3, "echo_spawn_03.wav");
            AssertCue(set, Phase4AudioCue.EchoRemove, "echo_remove.wav");
            AssertCue(set, Phase4AudioCue.LoopEnd, "loop_end.wav");
            AssertCue(set, Phase4AudioCue.InteractionSuccess, "interaction_success.wav");
            AssertCue(set, Phase4AudioCue.BatteryInsert, "battery_insert.wav");
            AssertCue(set, Phase4AudioCue.SectionComplete, "section_complete.wav");
            AssertCue(set, Phase4AudioCue.GameComplete, "section_complete.wav");
            Assert.That(AssetDatabase.GetAssetPath(set.Get(Phase4AudioCue.SectionComplete)),
                Does.Not.Contain("comparison"));
        }

        [Test]
        public void EvidenceOnlyObjectsAreAbsentFromProductionScene()
        {
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                string[] forbidden =
                {
                    "ACTUAL 64 PX", "4x Inspection", "Comparison", "Contact Sheet",
                    "Preview Camera", "Temporary Production Capture", "Evidence-only"
                };
                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    Transform[] all = roots[i].GetComponentsInChildren<Transform>(true);
                    for (int j = 0; j < all.Length; j++)
                        for (int k = 0; k < forbidden.Length; k++)
                            Assert.That(all[j].name, Does.Not.Contain(forbidden[k]).IgnoreCase);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void P0ThroughP2ScenesRemainByteIdentical()
        {
            for (int i = 0; i < ProtectedSceneHashes.Length; i++)
                Assert.That(Hash(ProtectedSceneHashes[i].Key),
                    Is.EqualTo(ProtectedSceneHashes[i].Value), ProtectedSceneHashes[i].Key);
        }

        private static void AssertApprovedMotif(Transform model)
        {
            Assert.That(model, Is.Not.Null);
            Transform strip = FindNamed(model, "Chest Embedded Emission Strip");
            Assert.That(strip.localPosition, Is.EqualTo(new Vector3(0f, 0.58f, -0.49f)));
            Assert.That(strip.localScale, Is.EqualTo(new Vector3(0.28f, 0.035f, 0.01f)));
            Transform ticks = FindNamed(model, "Back Panel Surface Generation Ticks");
            Assert.That(ticks.GetComponentsInChildren<Renderer>(true).Length, Is.EqualTo(3));
            Transform marker = FindNamed(model, "Floor Seal Segment 1") ??
                FindNamed(model, "Floor Seal Segment 2");
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.localScale.y, Is.EqualTo(0.006f).Within(0.0001f));
        }

        private static void AssertCue(Phase4AudioCueSet set, Phase4AudioCue cue,
            string fileName) => Assert.That(AssetDatabase.GetAssetPath(set.Get(cue)),
                Does.EndWith(fileName));

        private static int CountMaterial(Transform root, string token)
        {
            int count = 0;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i].sharedMaterial != null &&
                    renderers[i].sharedMaterial.name.IndexOf(token,
                        StringComparison.OrdinalIgnoreCase) >= 0) count++;
            return count;
        }

        private static PuzzleSectionController FindSection(Scene scene, int number)
        {
            PuzzleSectionController[] sections = FindSections(scene);
            for (int i = 0; i < sections.Length; i++)
                if (sections[i].SectionNumber == number) return sections[i];
            return null;
        }

        private static PuzzleSectionController[] FindSections(Scene scene)
        {
            List<PuzzleSectionController> result = new List<PuzzleSectionController>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                result.AddRange(roots[i].GetComponentsInChildren<PuzzleSectionController>(true));
            return result.ToArray();
        }

        private static Transform FindNamed(Transform root, string name)
        {
            if (root == null) return null;
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++) if (all[i].name == name) return all[i];
            return null;
        }

        private static string Hash(string assetPath)
        {
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(absolute)))
                .Replace("-", string.Empty);
        }

        private static KeyValuePair<string, string> Pair(string key, string value) =>
            new KeyValuePair<string, string>(key, value);
    }
}
