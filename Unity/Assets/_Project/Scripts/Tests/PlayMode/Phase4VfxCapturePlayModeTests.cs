using System.Collections;
using EchoShift.Gameplay;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests.PlayMode
{
    public sealed class Phase4VfxCapturePlayModeTests
    {
        [UnityTest]
        public IEnumerator FeedbackPoolLimitsConcurrentEffectsAndCueBounds()
        {
            yield return Load();
            Phase4FeedbackPool pool = Object.FindAnyObjectByType<Phase4FeedbackPool>(
                FindObjectsInactive.Include);
            SectionTransitionCoordinator coordinator =
                Object.FindAnyObjectByType<SectionTransitionCoordinator>(
                    FindObjectsInactive.Include);
            Assert.That(pool, Is.Not.Null);
            Vector3 position = coordinator.ActiveSection.Player.transform.position +
                               Vector3.up;
            for (int i = 0; i < 12; i++)
                pool.Emit(position, Color.white, 3f, 1.2f,
                    Phase4FeedbackEvent.GameCompleted);
            yield return null;
            Assert.That(pool.LastRequestedSize,
                Is.EqualTo(Phase4FeedbackPool.MaximumParticleSize));
            Assert.That(pool.LastRequestedAlpha,
                Is.EqualTo(Phase4FeedbackPool.MaximumParticleAlpha));
            Assert.That(pool.ActiveEffectCount,
                Is.LessThanOrEqualTo(Phase4FeedbackPool.DefaultConcurrentLimit));
            Assert.That(pool.MaximumConcurrentObserved,
                Is.LessThanOrEqualTo(Phase4FeedbackPool.DefaultConcurrentLimit));
        }

        [UnityTest]
        public IEnumerator LargestCueEstimatedViewportOccupancyStaysBelowTwelvePercent()
        {
            yield return Load();
            Phase4FeedbackPool pool = Object.FindAnyObjectByType<Phase4FeedbackPool>(
                FindObjectsInactive.Include);
            SectionTransitionCoordinator coordinator =
                Object.FindAnyObjectByType<SectionTransitionCoordinator>(
                    FindObjectsInactive.Include);
            Camera camera = Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
            Vector3 position = coordinator.ActiveSection.Player.transform.position +
                               Vector3.up;
            Color cue = new Color(1f, 1f, 1f,
                Phase4FeedbackDirector.LargestCueAlpha);
            pool.Emit(position, cue, Phase4FeedbackDirector.LargestCueSize, 0.48f,
                Phase4FeedbackEvent.GameCompleted);
            yield return null;

            ParticleSystem[] systems = pool.GetComponentsInChildren<ParticleSystem>(true);
            bool hasBounds = false;
            Rect union = default;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[16];
            for (int i = 0; i < systems.Length; i++)
            {
                int count = systems[i].GetParticles(particles);
                for (int particle = 0; particle < count; particle++)
                {
                    Vector3 center = camera.WorldToViewportPoint(particles[particle].position);
                    Vector3 edge = camera.WorldToViewportPoint(
                        particles[particle].position +
                        camera.transform.right * particles[particle].GetCurrentSize(systems[i]) *
                        0.5f);
                    float radius = Mathf.Abs(edge.x - center.x);
                    Rect projected = Rect.MinMaxRect(
                        center.x - radius, center.y - radius,
                        center.x + radius, center.y + radius);
                    if (!hasBounds)
                    {
                        union = projected;
                        hasBounds = true;
                    }
                    else
                    {
                        union = Rect.MinMaxRect(
                            Mathf.Min(union.xMin, projected.xMin),
                            Mathf.Min(union.yMin, projected.yMin),
                            Mathf.Max(union.xMax, projected.xMax),
                            Mathf.Max(union.yMax, projected.yMax));
                    }
                }
            }
            Assert.That(hasBounds, Is.True);
            float occupancy = Mathf.Clamp01(union.width) * Mathf.Clamp01(union.height);
            Assert.That(occupancy, Is.LessThanOrEqualTo(0.12f));
        }

        [UnityTest]
        public IEnumerator GeneratedSceneContainsHumanReviewProbe()
        {
            yield return Load();
            Phase4HumanReviewProbe probe =
                Object.FindAnyObjectByType<Phase4HumanReviewProbe>(
                    FindObjectsInactive.Include);
            Assert.That(probe, Is.Not.Null);
            Assert.That(Phase4HumanReviewProbe.SceneCount, Is.EqualTo(12));
        }

        private static IEnumerator Load()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                "P3_PlayableGreybox", LoadSceneMode.Single);
            while (!operation.isDone) yield return null;
            yield return null;
        }
    }
}
