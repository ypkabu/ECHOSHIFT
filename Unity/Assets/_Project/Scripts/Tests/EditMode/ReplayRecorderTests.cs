using EchoShift.Core;
using EchoShift.Replay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EchoShift.Tests
{
    public sealed class ReplayRecorderTests
    {
        [Test]
        public void FramesAreAcceptedOnlyInTickOrder()
        {
            ReplayRecorder recorder = new ReplayRecorder(3);

            Assert.That(recorder.TryRecord(CreateFrame(0)), Is.EqualTo(ReplayRecordResult.Recorded));
            Assert.That(recorder.TryRecord(CreateFrame(2)), Is.EqualTo(ReplayRecordResult.OutOfOrder));
            Assert.That(recorder.TryRecord(CreateFrame(1)), Is.EqualTo(ReplayRecordResult.Recorded));

            ReplayRecording recording = recorder.FinalizeRecording();
            Assert.That(recording.Count, Is.EqualTo(2));
            Assert.That(recording[0].Command.Tick, Is.EqualTo(0));
            Assert.That(recording[1].Command.Tick, Is.EqualTo(1));
        }

        [Test]
        public void FinalizedRecordingCannotBeChangedThroughRecorder()
        {
            ReplayRecorder recorder = new ReplayRecorder(2);
            recorder.TryRecord(CreateFrame(0));
            ReplayRecording recording = recorder.FinalizeRecording();

            ReplayRecordResult result = recorder.TryRecord(CreateFrame(1));

            Assert.That(result, Is.EqualTo(ReplayRecordResult.Finalized));
            Assert.That(recording.Count, Is.EqualTo(1));
            Assert.That(recording[0].Command.Tick, Is.EqualTo(0));
            Assert.That(recorder.FinalizeRecording(), Is.SameAs(recording));
        }

        [Test]
        public void DefaultLoopPreallocatesExactlySixHundredFrames()
        {
            LoopSettings settings = ScriptableObject.CreateInstance<LoopSettings>();
            ReplayRecorder recorder = new ReplayRecorder(settings.MaxTicks);

            Assert.That(settings.TickRate, Is.EqualTo(60));
            Assert.That(settings.MaxTicks, Is.EqualTo(600));
            Assert.That(recorder.Capacity, Is.EqualTo(600));

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void RecorderRejectsOverflowWithoutGrowing()
        {
            ReplayRecorder recorder = new ReplayRecorder(2);
            recorder.TryRecord(CreateFrame(0));
            recorder.TryRecord(CreateFrame(1));

            Assert.That(recorder.TryRecord(CreateFrame(2)), Is.EqualTo(ReplayRecordResult.Full));
            Assert.That(recorder.Capacity, Is.EqualTo(2));
            Assert.That(recorder.Count, Is.EqualTo(2));
        }

        [Test]
        public void ShortRecordingFinalizesOnlyItsRecordedPrefix()
        {
            ReplayRecorder recorder = new ReplayRecorder(600);
            for (int tick = 0; tick < 4; tick++)
            {
                Assert.That(
                    recorder.TryRecord(CreateFrame(tick)),
                    Is.EqualTo(ReplayRecordResult.Recorded));
            }

            ReplayRecording recording = recorder.FinalizeRecording();

            Assert.That(recording.Count, Is.EqualTo(4));
            Assert.That(recording[3].Command.Tick, Is.EqualTo(3));
            Assert.That(recording[3].ExpectedPosition, Is.EqualTo(new Vector3(0f, 0f, 4f)));
            Assert.That(
                recorder.TryRecord(CreateFrame(4)),
                Is.EqualTo(ReplayRecordResult.Finalized));
            Assert.That(recording.Count, Is.EqualTo(4));
        }

        [TestCase("tickRate", 0)]
        [TestCase("loopDurationSeconds", 0)]
        [TestCase("maxEchoes", 0)]
        [TestCase("maxEchoes", 4)]
        [TestCase("maxCatchUpTicksPerFrame", 0)]
        public void InvalidIntegerLoopSettingIsDetected(string propertyName, int value)
        {
            LoopSettings settings = ScriptableObject.CreateInstance<LoopSettings>();
            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty(propertyName).intValue = value;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(settings.TryValidate(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            Object.DestroyImmediate(settings);
        }

        [TestCase("moveSpeed", 0f)]
        [TestCase("driftTolerance", 0f)]
        public void InvalidFloatLoopSettingIsDetected(string propertyName, float value)
        {
            LoopSettings settings = ScriptableObject.CreateInstance<LoopSettings>();
            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty(propertyName).floatValue = value;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();

            Assert.That(settings.TryValidate(out string error), Is.False);
            Assert.That(error, Is.Not.Empty);

            Object.DestroyImmediate(settings);
        }

        private static ReplayFrame CreateFrame(int tick)
        {
            return new ReplayFrame(
                new InputCommand(tick, Vector2.up, InputButtonFlags.None),
                new Vector3(0f, 0f, tick + 1f),
                Quaternion.identity);
        }
    }
}
