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
    public sealed class Phase5AIdentityPreviewTests
    {
        [Test]
        public void OptionsExposeThreeDistinctCompleteIdentitySystems()
        {
            IReadOnlyList<Phase5AIdentityOptionDefinition> options =
                Phase5AIdentityPreviewBuilder.Options;
            Assert.That(options.Count, Is.EqualTo(3));
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> motifs = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < options.Count; i++)
            {
                Phase5AIdentityOptionDefinition option = options[i];
                Assert.That(ids.Add(option.Id), Is.True, $"Duplicate option ID {option.Id}.");
                Assert.That(motifs.Add(option.Motif), Is.True, $"Duplicate motif {option.Motif}.");
                Assert.That(option.Copy.Length, Is.EqualTo(12));
                Assert.That(option.Decals.Length,
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.RequiredDecalCount));
                Assert.That(option.MotifPitches.Length, Is.InRange(1, 3));
                Assert.That(option.AudioTreatment, Is.Not.Empty);
            }
        }

        [Test]
        public void GeneratedAtlasesContainTwelveDecalsWithin1024Canvas()
        {
            IReadOnlyList<Phase5AIdentityOptionDefinition> options =
                Phase5AIdentityPreviewBuilder.Options;
            for (int i = 0; i < options.Count; i++)
            {
                string path = AbsoluteAssetPath(
                    $"{Phase5AIdentityPreviewBuilder.ArtRoot}/{options[i].Id}/" +
                    $"P5A_{options[i].Id}_DecalAtlas.svg");
                Assert.That(File.Exists(path), Is.True, path);
                string svg = File.ReadAllText(path);
                Assert.That(svg, Does.Contain("width=\"1024\"").And.Contain("height=\"1024\""));
                int count = 0;
                int offset = 0;
                while ((offset = svg.IndexOf("id=\"decal-", offset, StringComparison.Ordinal)) >= 0)
                {
                    count++;
                    offset += 10;
                }
                Assert.That(count, Is.EqualTo(Phase5AIdentityPreviewBuilder.RequiredDecalCount),
                    options[i].Id);
            }
        }

        [Test]
        public void PreviewScenesContainNoGameplayIdentityOrPhysicsComponents()
        {
            for (int i = 0; i < 3; i++)
            {
                string path = $"{Phase5AIdentityPreviewBuilder.SceneRoot}/Phase5A_Option_{(char)('A' + i)}.unity";
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                try
                {
                    GameObject[] roots = scene.GetRootGameObjects();
                    for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                    {
                        Assert.That(roots[rootIndex].GetComponentsInChildren<Collider>(true), Is.Empty, path);
                        Assert.That(roots[rootIndex].GetComponentsInChildren<Rigidbody>(true), Is.Empty, path);
                        Assert.That(roots[rootIndex].GetComponentsInChildren<StableId>(true), Is.Empty, path);
                        Assert.That(roots[rootIndex].GetComponentsInChildren<LoopDirector>(true), Is.Empty, path);
                        Assert.That(roots[rootIndex].GetComponentsInChildren<CharacterMotor>(true), Is.Empty, path);
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        [Test]
        public void GeneratedCapturesAndAudioMeetDeliveryBounds()
        {
            string repository = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            Assert.That(repository, Is.Not.Null.And.Not.Empty);
            for (int optionIndex = 0; optionIndex < 3; optionIndex++)
            {
                string id = ((char)('A' + optionIndex)).ToString();
                string directory = Path.Combine(repository, "Captures", "Phase5A", "Options", id);
                string[] pngs = Directory.GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly);
                Assert.That(pngs.Length,
                    Is.EqualTo(Phase5AIdentityPreviewBuilder.RequiredCaptureCountPerOption));
                for (int i = 0; i < pngs.Length; i++)
                {
                    byte[] png = File.ReadAllBytes(pngs[i]);
                    Assert.That(ReadBigEndianInt32(png, 16),
                        Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureWidth), pngs[i]);
                    Assert.That(ReadBigEndianInt32(png, 20),
                        Is.EqualTo(Phase5AIdentityPreviewBuilder.CaptureHeight), pngs[i]);
                }
                string wav = Path.Combine(directory, "audio_preview.wav");
                Assert.That(File.Exists(wav), Is.True, wav);
                double seconds = (new FileInfo(wav).Length - 44d) / (44100d * 2d);
                Assert.That(seconds, Is.LessThanOrEqualTo(
                    Phase5AIdentityPreviewBuilder.MaximumAudioSeconds));
            }
        }

        private static string AbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length)
                .Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
            (bytes[offset + 2] << 8) | bytes[offset + 3];
    }
}
