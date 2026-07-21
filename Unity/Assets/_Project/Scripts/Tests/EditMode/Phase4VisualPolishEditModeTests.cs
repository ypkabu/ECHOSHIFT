using System.IO;
using System.Security.Cryptography;
using EchoShift.Editor;
using EchoShift.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoShift.Tests.EditMode
{
    public sealed class Phase4VisualPolishEditModeTests
    {
        private Phase4VisualSettings Settings =>
            AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(Phase4AssetBuilder.SettingsPath);

        [Test] public void VisualSettingsAssetExists() => Assert.That(Settings, Is.Not.Null);

        [Test] public void VisualSettingsHasAllRequiredReferences() =>
            Assert.That(Settings.HasRequiredReferences, Is.True);

        [Test] public void EchoGenerationColorsAreDistinct() =>
            Assert.That(Settings.EchoColorsAreDistinct(), Is.True);

        [Test] public void EchoVisualSlotsCycleWithoutClampingDuplicates()
        {
            Assert.That(Settings.GetEchoColor(4), Is.EqualTo(Settings.GetEchoColor(1)));
            Assert.That(Settings.GetEchoMaterial(4), Is.SameAs(Settings.GetEchoMaterial(1)));
            Assert.That(Settings.GetEchoColor(4), Is.Not.EqualTo(Settings.GetEchoColor(3)));
        }

        [Test] public void PostProcessingValuesStayWithinSafeLimits() =>
            Assert.That(Settings.HasSafePostProcessingValues, Is.True);

        [Test] public void CaptureResolutionIsFullHd()
        {
            Assert.That(Settings.CaptureWidth, Is.EqualTo(1920));
            Assert.That(Settings.CaptureHeight, Is.EqualTo(1080));
        }

        [Test] public void FeedbackPoolIsBounded() =>
            Assert.That(Settings.FeedbackPoolSize, Is.InRange(4, 16));

        [Test] public void EveryAudioCueHasAClip() =>
            Assert.That(Settings.AudioCues.HasAllCues, Is.True);

        [Test] public void AudioCueCountMatchesEnum()
        {
            for (int i = 0; i < (int)Phase4AudioCue.Count; i++)
                Assert.That(Settings.AudioCues.Get((Phase4AudioCue)i), Is.Not.Null);
        }

        [Test] public void PackagedJapaneseFontIsAProjectAsset()
        {
            string path = AssetDatabase.GetAssetPath(Settings.PackagedJapaneseFont);
            Assert.That(path, Does.StartWith("Assets/_Project/Fonts/"));
            Assert.That(Settings.PackagedJapaneseFont.dynamic, Is.True);
        }

        [Test] public void TmpAtlasIsStaticAndContainsCatalogGlyphs()
        {
            TMP_FontAsset font = Settings.PackagedJapaneseTmpFont;
            Phase3TextCatalog catalog = AssetDatabase.LoadAssetAtPath<Phase3TextCatalog>(
                P3SceneBuilder.TextCatalogPath);
            Assert.That(font.atlasPopulationMode, Is.EqualTo(AtlasPopulationMode.Static));
            Assert.That(font.characterTable.Count, Is.GreaterThan(100));
            Assert.That(JapaneseFontApplier.SupportsAllRequiredGlyphs(
                font, catalog.GetRequiredGlyphCharacters()), Is.True);
        }

        [Test] public void RuntimeFontCodeDoesNotCreateOsFont()
        {
            string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string source = File.ReadAllText(Path.Combine(project,
                "Assets/_Project/Scripts/Runtime/Presentation/JapaneseFontApplier.cs"));
            Assert.That(source, Does.Not.Contain("CreateDynamicFontFromOSFont"));
        }

        [Test] public void SharedMaterialsUseUrpLitShader()
        {
            Material[] materials =
            {
                Settings.FacilityPanelMaterial, Settings.FacilityDarkMaterial,
                Settings.FacilityTrimMaterial, Settings.PlayerMaterial,
                Settings.PlateMaterial, Settings.BatteryMaterial, Settings.GoalMaterial
            };
            for (int i = 0; i < materials.Length; i++)
                Assert.That(materials[i].shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
        }

        [Test] public void VolumeUsesOnlySafeOverrides()
        {
            VolumeProfile profile = Settings.VolumeProfile;
            Assert.That(profile.TryGet(out Bloom bloom), Is.True);
            Assert.That(profile.TryGet(out Vignette vignette), Is.True);
            Assert.That(profile.TryGet(out ColorAdjustments color), Is.True);
            Assert.That(profile.TryGet(out Tonemapping tone), Is.True);
            Assert.That(profile.TryGet(out MotionBlur _), Is.False);
            Assert.That(profile.TryGet(out ChromaticAberration _), Is.False);
            Assert.That(bloom.intensity.value, Is.LessThanOrEqualTo(0.75f));
            Assert.That(vignette.intensity.value, Is.LessThanOrEqualTo(0.22f));
        }

        [Test] public void HudContainsSixReusableIcons() =>
            Assert.That(Settings.HudIcons.Length, Is.GreaterThanOrEqualTo(6));

        [Test] public void EchoPrefabContainsConfiguredCompoundVisual()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Actors/P3_Echo.prefab");
            Phase4ActorVisual visual = prefab.GetComponent<Phase4ActorVisual>();
            Assert.That(visual, Is.Not.Null);
            Assert.That(visual.HasRequiredReferences, Is.True);
            Assert.That(prefab.GetComponent<Renderer>().enabled, Is.False);
        }

        [Test] public void DeviceStateVisualsUsePropertyBlocksInsteadOfMaterialInstances()
        {
            string[] sources =
            {
                "Phase4PressurePlateVisual.cs", "DoorVisualFeedback.cs",
                "Phase4BatteryVisual.cs", "Phase4SocketVisual.cs"
            };
            for (int i = 0; i < sources.Length; i++)
            {
                string source = ReadPresentationSource(sources[i]);
                Assert.That(source, Does.Contain("MaterialPropertyBlock"), sources[i]);
                Assert.That(source, Does.Not.Contain(".material"), sources[i]);
                Assert.That(source, Does.Not.Contain("new Material("), sources[i]);
            }
        }

        [Test] public void RequiredVfxPrefabIsAUniqueProjectAsset()
        {
            Assert.That(Settings.PulseVfxPrefab, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(Settings.PulseVfxPrefab),
                Is.EqualTo("Assets/_Project/Prefabs/VFX/P4_Pulse.prefab"));
            Assert.That(AssetDatabase.FindAssets("P4_Pulse t:Prefab",
                new[] { "Assets/_Project/Prefabs/VFX" }).Length, Is.EqualTo(1));
        }

        [Test] public void FontLicenseNoticeExists()
        {
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string notice = Path.Combine(repository, "ThirdPartyNotices", "NotoSansJP-OFL.txt");
            Assert.That(File.Exists(notice), Is.True);
            Assert.That(File.ReadAllText(notice), Does.Contain("SIL OPEN FONT LICENSE"));
        }

        [Test] public void BuilderOutputsHaveUniqueCanonicalAssets()
        {
            Assert.That(AssetDatabase.FindAssets("t:Phase4VisualSettings",
                new[] { "Assets/_Project/Settings" }).Length, Is.EqualTo(1));
            Assert.That(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                Phase4AssetBuilder.TmpFontPath), Is.SameAs(Settings.PackagedJapaneseTmpFont));
            Assert.That(AssetDatabase.LoadAssetAtPath<Font>(
                Phase4AssetBuilder.FontPath), Is.SameAs(Settings.PackagedJapaneseFont));
        }

        [TestCase("P0_ReplayLab.unity", "1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5")]
        [TestCase("P1_InteractionLab.unity", "4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB")]
        [TestCase("P2_CoordinationLab.unity", "73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C")]
        public void ValidatedPrePhase4SceneHashIsUnchanged(string fileName, string expected)
        {
            string path = Path.Combine(Application.dataPath, "_Project", "Scenes", fileName);
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            string actual = System.BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty);
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static string ReadPresentationSource(string fileName)
        {
            string project = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return File.ReadAllText(Path.Combine(project,
                "Assets/_Project/Scripts/Runtime/Presentation", fileName));
        }
    }
}
