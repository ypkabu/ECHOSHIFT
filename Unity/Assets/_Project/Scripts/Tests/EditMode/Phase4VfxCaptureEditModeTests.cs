using EchoShift.Editor;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoShift.Tests.EditMode
{
    public sealed class Phase4VfxCaptureEditModeTests
    {
        [Test]
        public void PulseVfxUsesSoftTransparentParticleMaterial()
        {
            Phase4VisualSettings settings =
                AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(
                    Phase4AssetBuilder.SettingsPath);
            Assert.That(settings, Is.Not.Null);
            GameObject prefab = settings.PulseVfxPrefab;
            Assert.That(prefab, Is.Not.Null);
            ParticleSystemRenderer renderer = prefab.GetComponent<ParticleSystemRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Material material = renderer.sharedMaterial;
            Assert.That(material, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(material),
                Is.EqualTo(Phase4AssetBuilder.VfxMaterialPath));
            Assert.That(material.shader.name,
                Is.EqualTo("Universal Render Pipeline/Particles/Unlit"));
            Assert.That(material.GetFloat("_Surface"), Is.EqualTo(1f));
            Assert.That(material.GetFloat("_DstBlend"),
                Is.EqualTo((float)BlendMode.One));
            Assert.That(material.renderQueue, Is.GreaterThanOrEqualTo(3000));

            Texture2D texture = material.GetTexture("_BaseMap") as Texture2D;
            Assert.That(texture, Is.Not.Null);
            Color32[] pixels = texture.GetPixels32();
            Assert.That(pixels[0].a, Is.Zero);
            Assert.That(pixels[pixels.Length / 2 + texture.width / 2].a,
                Is.GreaterThan(220));
        }

        [Test]
        public void PulseVfxParticleBudgetIsBounded()
        {
            Phase4VisualSettings settings =
                AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(
                    Phase4AssetBuilder.SettingsPath);
            ParticleSystem particles = settings.PulseVfxPrefab.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            ParticleSystem.ShapeModule shape = particles.shape;
            ParticleSystem.Burst[] bursts =
                new ParticleSystem.Burst[particles.emission.burstCount];
            particles.emission.GetBursts(bursts);
            Assert.That(main.maxParticles, Is.LessThanOrEqualTo(12));
            Assert.That(main.startSpeed.constantMax, Is.LessThanOrEqualTo(0.8f));
            Assert.That(shape.radius, Is.LessThanOrEqualTo(0.12f));
            Assert.That(bursts, Has.Length.EqualTo(1));
            Assert.That(bursts[0].count.constantMax, Is.LessThanOrEqualTo(8f));
            Assert.That(settings.PulseVfxPrefab.GetComponent<ParticleSystemRenderer>()
                .maxParticleSize, Is.LessThanOrEqualTo(0.08f));
            Assert.That(Phase4FeedbackPool.MaximumParticleSize,
                Is.LessThanOrEqualTo(0.32f));
            Assert.That(Phase4FeedbackPool.MaximumParticleAlpha,
                Is.LessThanOrEqualTo(0.42f));
            Assert.That(Phase4FeedbackDirector.LargestCueSize,
                Is.LessThanOrEqualTo(Phase4FeedbackPool.MaximumParticleSize));
            Assert.That(Phase4FeedbackDirector.LargestCueAlpha,
                Is.LessThanOrEqualTo(Phase4FeedbackPool.MaximumParticleAlpha));
        }

        [Test]
        public void BloomRemainsEnabledButRestrained()
        {
            Phase4VisualSettings settings =
                AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(
                    Phase4AssetBuilder.SettingsPath);
            VolumeProfile profile = settings.VolumeProfile;
            Assert.That(profile.TryGet(out Bloom bloom), Is.True);
            Assert.That(bloom.active, Is.True);
            Assert.That(bloom.intensity.value, Is.InRange(0.15f, 0.25f));
            Assert.That(bloom.threshold.value, Is.GreaterThanOrEqualTo(1.3f));
            Assert.That(settings.BloomIntensity, Is.EqualTo(0.22f).Within(0.001f));
        }

        [Test]
        public void HumanReviewCaptureContractDefinesAllTwelveScenes()
        {
            Assert.That(Phase4HumanReviewProbe.SceneCount,
                Is.EqualTo(Phase4HumanReviewProbe.RequiredSceneCount));
            float total = 0f;
            for (int i = 0; i < Phase4HumanReviewProbe.SceneCount; i++)
            {
                Assert.That(Phase4HumanReviewProbe.GetSceneName(i), Is.Not.Empty);
                Assert.That(Phase4HumanReviewProbe.GetSceneMinimumDuration(i),
                    Is.GreaterThanOrEqualTo(2f));
                total += Phase4HumanReviewProbe.GetSceneMinimumDuration(i);
            }
            Assert.That(total, Is.GreaterThanOrEqualTo(30f));
            Assert.That(Phase4HumanReviewProbe.MinimumVideoDurationSeconds,
                Is.EqualTo(30f));
            Assert.That(Phase4HumanReviewProbe.MaximumVideoDurationSeconds,
                Is.EqualTo(40f));
            Assert.That(Phase4HumanReviewProbe.MinimumGameplayDurationSeconds,
                Is.EqualTo(25f));
            Assert.That(Phase4HumanReviewProbe.MaximumCompletedDurationSeconds,
                Is.EqualTo(2f));
        }
    }
}
