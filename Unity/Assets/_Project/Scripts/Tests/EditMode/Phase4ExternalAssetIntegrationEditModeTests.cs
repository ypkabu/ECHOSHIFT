using System;
using System.IO;
using System.Security.Cryptography;
using EchoShift.Editor;
using EchoShift.Interaction.Recorded;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoShift.Tests.EditMode
{
    public sealed class Phase4ExternalAssetIntegrationEditModeTests
    {
        private const string ThirdPartyRoot = "Assets/_Project/ThirdParty";

        [Test]
        public void ExternalAssetCatalogIsCompleteAndProjectOwned()
        {
            Phase4ExternalAssetCatalog catalog = AssetDatabase.LoadAssetAtPath<Phase4ExternalAssetCatalog>(
                Phase4ExternalAssetBuilder.CatalogPath);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.IsComplete, Is.True);
            Assert.That(AssetDatabase.GetAssetPath(catalog.FloorMaterial),
                Does.StartWith("Assets/_Project/Art/"));
            Assert.That(AssetDatabase.GetAssetPath(catalog.RobotVisual),
                Does.StartWith("Assets/_Project/Art/"));
        }

        [Test]
        public void ExternalAssetBuilderDoesNotRewriteExistingWrapperPrefabs()
        {
            const string path = "Assets/_Project/Art/Characters/P4X_RobotVisual.prefab";
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
            string before = Sha256(absolute);
            Phase4ExternalAssetBuilder.Build();
            string after = Sha256(absolute);
            Assert.That(after, Is.EqualTo(before));
        }

        [Test]
        public void ThirdPartyPackagesHaveNoticesAndManifestHashes()
        {
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string manifest = File.ReadAllText(Path.Combine(repository, "Docs", "ThirdPartyAssets.md"));
            string[] notices =
            {
                "Quaternius-ModularSciFiMegaKit-CC0.txt",
                "Quaternius-AnimatedRobot-CC0.txt",
                "Kenney-SciFiSounds-CC0.txt"
            };
            for (int i = 0; i < notices.Length; i++)
            {
                string path = Path.Combine(repository, "ThirdPartyNotices", notices[i]);
                Assert.That(File.Exists(path), Is.True, notices[i]);
                Assert.That(File.ReadAllText(path), Does.Contain("CC0"), notices[i]);
            }
            Assert.That(manifest, Does.Contain("6FAE60CF5189E44DFF0BD91097F094A765ACC6D57D64A85A0CC0DD56E03035E3"));
            Assert.That(manifest, Does.Contain("119340F351A5098AD814F78719438C0DA355A9CE8A4C8A3AF6A8D48AA3D49E04"));
            Assert.That(manifest, Does.Contain("38AFB56DB7FB17A74D30F0AFC8ADB5F00441A94E65D6B8AC1958732480F79EB8"));
        }

        [Test]
        public void ImportedModelsUseBoundedSettingsAndNoGeneratedColliders()
        {
            string[] guids = AssetDatabase.FindAssets("t:Model", new[] { ThirdPartyRoot });
            Assert.That(guids.Length, Is.EqualTo(25));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.materialImportMode, Is.EqualTo(ModelImporterMaterialImportMode.None), path);
                Assert.That(importer.isReadable, Is.False, path);
                Assert.That(importer.meshCompression, Is.EqualTo(ModelImporterMeshCompression.Medium), path);
                Assert.That(importer.addCollider, Is.False, path);
                Assert.That(importer.importAnimation, Is.False, path);
                Assert.That(importer.animationType, Is.EqualTo(ModelImporterAnimationType.None), path);
            }
        }

        [Test]
        public void ImportedTexturesAreReadableOffAndAtMost2048()
        {
            string textureRoot = ThirdPartyRoot + "/Quaternius/ModularSciFiMegaKit/Textures";
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { textureRoot });
            Assert.That(guids.Length, Is.EqualTo(8));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.isReadable, Is.False, path);
                Assert.That(importer.maxTextureSize, Is.LessThanOrEqualTo(2048), path);
                Assert.That(importer.mipmapEnabled, Is.True, path);
                if (path.EndsWith("_Normal.png", StringComparison.Ordinal))
                    Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.NormalMap), path);
            }
        }

        [Test]
        public void ProjectMaterialsHaveSupportedUrpShadersAndNoMissingTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Material",
                new[] { "Assets/_Project/Art/Materials" });
            Assert.That(guids.Length, Is.EqualTo(4));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                Assert.That(material.shader, Is.Not.Null, path);
                Assert.That(material.shader.isSupported, Is.True, path);
                Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"), path);
                Assert.That(material.shader.name, Does.Not.Contain("InternalError"), path);
                Assert.That(material.mainTexture, Is.Not.Null, path);
            }
        }

        [Test]
        public void ProjectOwnedWrappersContainNoColliderOrRuntimeAnimator()
        {
            string[] roots =
            {
                "Assets/_Project/Art/Environment",
                "Assets/_Project/Art/Props",
                "Assets/_Project/Art/Characters"
            };
            string[] guids = AssetDatabase.FindAssets("t:Prefab", roots);
            Assert.That(guids.Length, Is.GreaterThanOrEqualTo(20));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(prefab.GetComponentsInChildren<Collider>(true), Is.Empty, path);
                AssertNoAnimationComponents(prefab, path);
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                Assert.That(renderers, Is.Not.Empty, path);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Material[] materials = renderers[rendererIndex].sharedMaterials;
                    for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                        Assert.That(materials[materialIndex], Is.Not.Null, path);
                }
            }
        }

        private static void AssertNoAnimationComponents(GameObject root, string context)
        {
            Component[] components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null) continue;
                string name = components[i].GetType().Name;
                Assert.That(name, Is.Not.EqualTo("Animator"), context);
                Assert.That(name, Is.Not.EqualTo("Animation"), context);
            }
        }

        private static string Sha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
        }

        [Test]
        public void Phase3SceneKeepsStableIdsAndGameplayCollidersWhileVisualsStayNonPhysical()
        {
            EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath, OpenSceneMode.Single);
            StableId[] ids = UnityEngine.Object.FindObjectsByType<StableId>(
                FindObjectsInactive.Include);
            string[] values = new string[ids.Length];
            for (int i = 0; i < ids.Length; i++) values[i] = ids[i].Value;
            Array.Sort(values, StringComparer.Ordinal);
            Assert.That(values, Is.EqualTo(new[]
            {
                "p3-s2-battery-063ce56d",
                "p3-s2-socket-a906013e",
                "p3-s3-battery-3aaf6a48",
                "p3-s3-socket-852fdd89"
            }));

            Phase4ActorVisual[] actors = UnityEngine.Object.FindObjectsByType<Phase4ActorVisual>(
                FindObjectsInactive.Include);
            Assert.That(actors.Length, Is.GreaterThanOrEqualTo(3));
            for (int i = 0; i < actors.Length; i++)
            {
                Assert.That(actors[i].GetComponent<Collider>(), Is.Not.Null, actors[i].name);
                Transform model = actors[i].transform.Find("P4 Robot Visual");
                Assert.That(model, Is.Not.Null, actors[i].name);
                Assert.That(model.GetComponentsInChildren<Collider>(true), Is.Empty, actors[i].name);
            }
        }

        [Test]
        public void Phase3SceneHasNoPersistentWorldLabelsOrCameraCrossingBeams()
        {
            EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath, OpenSceneMode.Single);
            WorldBillboardLabel[] labels = UnityEngine.Object.FindObjectsByType<WorldBillboardLabel>(
                FindObjectsInactive.Include);
            Assert.That(labels, Is.Not.Empty);
            for (int i = 0; i < labels.Length; i++)
            {
                Renderer renderer = labels[i].GetComponent<Renderer>();
                bool allowedGoalSign = labels[i].name == "出口 Sign";
                Assert.That(renderer == null || !renderer.enabled || allowedGoalSign,
                    Is.True, labels[i].name);
            }

            Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include);
            for (int i = 0; i < transforms.Length; i++)
            {
                string lower = transforms[i].name.ToLowerInvariant();
                Assert.That(lower, Does.Not.Contain("overhead rail"), transforms[i].name);
                Assert.That(lower, Does.Not.Contain("cross beam"), transforms[i].name);
            }
        }
    }
}
