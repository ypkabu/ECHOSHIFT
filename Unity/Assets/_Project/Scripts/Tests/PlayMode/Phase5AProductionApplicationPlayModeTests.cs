using System.Collections;
using EchoShift.Gameplay;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests.PlayMode
{
    public sealed class Phase5AProductionApplicationPlayModeTests
    {
        [UnityTest]
        public IEnumerator ProductionIdentityLoadsAndEchoLifecycleKeepsReplayStable()
        {
            LogAssert.Expect(LogType.Log,
                "PHASE3_STATE Playing reason=BootComplete section=1");
            AsyncOperation load = SceneManager.LoadSceneAsync("P3_PlayableGreybox",
                LoadSceneMode.Single);
            while (!load.isDone) yield return null;
            yield return null;

            SectionTransitionCoordinator coordinator = Find<SectionTransitionCoordinator>();
            Assert.That(coordinator, Is.Not.Null);
            PuzzleSectionController section = coordinator.ActiveSection;
            Assert.That(section, Is.Not.Null);
            Assert.That(section.Goal.GetComponent<Phase5AGoalIdentityVisual>(), Is.Not.Null);
            Phase4AudioController audio = Find<Phase4AudioController>();
            Assert.That(audio, Is.Not.Null);
            Assert.That(audio.HasValidReferences, Is.True);
            Assert.That(audio.CueSet.name, Is.EqualTo("Phase5AAudioCueSet"));

            coordinator.SetTransitionDurationsForTests(0f, 0f);
            Phase4FeedbackDirector feedback = Find<Phase4FeedbackDirector>();
            for (int generation = 1; generation <= 4; generation++)
            {
                section.Director.RequestLoopEnd();
                for (int frame = 0; frame < 40; frame++) yield return null;
            }
            Assert.That(section.Director.EchoCount, Is.EqualTo(3));
            Assert.That(feedback.EchoRemovalFeedbackCount, Is.EqualTo(1));
            Assert.That(section.Director.MaximumReplayDrift, Is.LessThanOrEqualTo(0.05f));
            LogAssert.NoUnexpectedReceived();
        }

        private static T Find<T>() where T : Object
        {
            T[] values = Resources.FindObjectsOfTypeAll<T>();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is Component component && component.gameObject.scene.IsValid())
                    return values[i];
            }
            return null;
        }
    }
}
