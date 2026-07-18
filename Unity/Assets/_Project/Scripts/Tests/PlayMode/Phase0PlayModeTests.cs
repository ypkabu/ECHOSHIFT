using System.Collections;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Player;
using EchoShift.Replay;
using EchoShift.Reset;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests
{
    public sealed class Phase0PlayModeTests
    {
        [UnityTest]
        public IEnumerator TransformResettableRestoresCapturedPose()
        {
            GameObject gameObject = new GameObject("Reset Test");
            gameObject.transform.SetPositionAndRotation(
                new Vector3(1f, 2f, 3f),
                Quaternion.Euler(0f, 45f, 0f));
            TransformResettable resettable = gameObject.AddComponent<TransformResettable>();
            resettable.CaptureInitialState();

            gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            resettable.RestoreInitialState();

            Assert.That(gameObject.transform.position, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            Assert.That(Quaternion.Angle(gameObject.transform.rotation, Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.001f));

            Object.Destroy(gameObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EchoPlaybackFinishesWithinDriftTolerance()
        {
            const int frameCount = 60;
            const float tickDuration = 1f / 60f;
            const float speed = 4f;

            ReplayRecorder recorder = new ReplayRecorder(frameCount);
            Vector3 expectedPosition = Vector3.zero;
            for (int tick = 0; tick < frameCount; tick++)
            {
                expectedPosition += Vector3.forward * speed * tickDuration;
                ReplayFrame frame = new ReplayFrame(
                    new InputCommand(tick, Vector2.up, InputButtonFlags.None),
                    expectedPosition,
                    Quaternion.identity);
                Assert.That(recorder.TryRecord(frame), Is.EqualTo(ReplayRecordResult.Recorded));
            }

            GameObject echoObject = new GameObject("Echo Playback Test");
            CharacterMotor motor = echoObject.AddComponent<CharacterMotor>();
            motor.Configure(speed, 0.45f, 2f, 0);
            LoopActor actor = echoObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Echo);
            EchoPlayback playback = echoObject.AddComponent<EchoPlayback>();
            playback.Initialize(recorder.FinalizeRecording(), motor, actor, 0.05f);

            for (int tick = 0; tick < frameCount; tick++)
            {
                playback.SimulateTick(tickDuration);
            }

            Assert.That(playback.PlaybackTick, Is.EqualTo(frameCount));
            Assert.That(playback.MaximumDrift, Is.LessThanOrEqualTo(0.05f));
            Assert.That(Vector3.Distance(motor.Position, expectedPosition), Is.LessThanOrEqualTo(0.05f));

            Object.Destroy(echoObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShortReplayStopsAtItsFinalRecordedPose()
        {
            const int frameCount = 5;
            const float tickDuration = 1f / 60f;
            const float speed = 4f;
            ReplayRecorder recorder = new ReplayRecorder(600);
            Vector3 expectedPosition = Vector3.zero;
            Quaternion expectedRotation = Quaternion.identity;

            for (int tick = 0; tick < frameCount; tick++)
            {
                Vector2 movement = tick < frameCount - 1 ? Vector2.right : Vector2.up;
                expectedPosition += new Vector3(movement.x, 0f, movement.y) * speed * tickDuration;
                expectedRotation = Quaternion.LookRotation(
                    new Vector3(movement.x, 0f, movement.y),
                    Vector3.up);
                Assert.That(
                    recorder.TryRecord(new ReplayFrame(
                        new InputCommand(tick, movement, InputButtonFlags.None),
                        expectedPosition,
                        expectedRotation)),
                    Is.EqualTo(ReplayRecordResult.Recorded));
            }

            GameObject echoObject = new GameObject("Short Echo Playback Test");
            CharacterMotor motor = echoObject.AddComponent<CharacterMotor>();
            motor.Configure(speed, 0.45f, 2f, 0);
            LoopActor actor = echoObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Echo);
            EchoPlayback playback = echoObject.AddComponent<EchoPlayback>();
            playback.Initialize(recorder.FinalizeRecording(), motor, actor, 0.05f);

            for (int tick = 0; tick < frameCount + 20; tick++)
            {
                playback.SimulateTick(tickDuration);
            }

            Vector3 stoppedPosition = motor.Position;
            Quaternion stoppedRotation = motor.Rotation;
            float stoppedDrift = playback.MaximumDrift;
            for (int tick = 0; tick < 20; tick++)
            {
                playback.SimulateTick(tickDuration);
            }

            Assert.That(playback.RecordingLength, Is.EqualTo(frameCount));
            Assert.That(playback.PlaybackTick, Is.EqualTo(frameCount));
            Assert.That(Vector3.Distance(stoppedPosition, expectedPosition), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(motor.Position, stoppedPosition), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(stoppedRotation, expectedRotation), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(motor.Rotation, stoppedRotation), Is.LessThan(0.001f));
            Assert.That(playback.MaximumDrift, Is.EqualTo(stoppedDrift));
            LogAssert.NoUnexpectedReceived();

            Object.Destroy(echoObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PressurePlateTracksMultipleActors()
        {
            GameObject plateObject = new GameObject("Pressure Plate Test");
            BoxCollider trigger = plateObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            PressurePlate plate = plateObject.AddComponent<PressurePlate>();

            GameObject firstObject = new GameObject("First Actor");
            LoopActor first = firstObject.AddComponent<LoopActor>();
            GameObject secondObject = new GameObject("Second Actor");
            LoopActor second = secondObject.AddComponent<LoopActor>();

            plate.RegisterActor(first);
            plate.RegisterActor(second);
            Assert.That(plate.IsPressed, Is.True);
            Assert.That(plate.OccupantCount, Is.EqualTo(2));

            plate.UnregisterActor(first);
            Assert.That(plate.IsPressed, Is.True);
            Assert.That(plate.OccupantCount, Is.EqualTo(1));

            plate.UnregisterActor(second);
            Assert.That(plate.IsPressed, Is.False);
            Assert.That(plate.OccupantCount, Is.Zero);

            Object.Destroy(plateObject);
            Object.Destroy(firstObject);
            Object.Destroy(secondObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PressurePlatePrunesDisabledAndDestroyedActorsWithoutTriggerExit()
        {
            GameObject plateObject = new GameObject("Pressure Plate Stale Actor Test");
            BoxCollider trigger = plateObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            PressurePlate plate = plateObject.AddComponent<PressurePlate>();

            GameObject actorObject = new GameObject("Stale Actor");
            LoopActor actor = actorObject.AddComponent<LoopActor>();
            plate.RegisterActor(actor);
            plate.RegisterActor(actor);
            Assert.That(plate.OccupantCount, Is.EqualTo(1), "Duplicate trigger entries must be ignored.");

            actor.enabled = false;
            yield return null;
            Assert.That(plate.IsPressed, Is.False);
            Assert.That(plate.OccupantCount, Is.Zero);

            actor.enabled = true;
            plate.RegisterActor(actor);
            actorObject.SetActive(false);
            yield return null;
            Assert.That(plate.IsPressed, Is.False);
            Assert.That(plate.OccupantCount, Is.Zero);

            actorObject.SetActive(true);
            plate.RegisterActor(actor);
            Object.Destroy(actorObject);
            yield return null;
            Assert.That(plate.IsPressed, Is.False);
            Assert.That(plate.OccupantCount, Is.Zero);

            GameObject sceneEndActorObject = new GameObject("Scene End Actor");
            LoopActor sceneEndActor = sceneEndActorObject.AddComponent<LoopActor>();
            plate.RegisterActor(sceneEndActor);
            plate.enabled = false;
            Assert.That(plate.IsPressed, Is.False, "Plate disable must clear occupants without OnTriggerExit.");
            Assert.That(plate.OccupantCount, Is.Zero);

            Object.Destroy(plateObject);
            Object.Destroy(sceneEndActorObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LoopTransitionHandsRecordingToNewEcho()
        {
            yield return LoadReplayLab();
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            Assert.That(director, Is.Not.Null);

            director.RequestLoopEnd();
            yield return WaitForLoop(director, 2);

            Assert.That(director.LastCompletedRecording, Is.Not.Null);
            Assert.That(director.EchoCount, Is.EqualTo(1));
            Assert.That(director.GetEchoPlayback(0).RecordingLength, Is.EqualTo(director.LastCompletedRecording.Count));
        }

        [UnityTest]
        public IEnumerator ManualShortLoopCreatesBoundedEchoReplay()
        {
            yield return LoadReplayLab();
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PlayerSimulation player = Object.FindAnyObjectByType<PlayerSimulation>();
            ScriptedTestInputSource input = player.gameObject.AddComponent<ScriptedTestInputSource>();
            player.Configure(input, player.Motor);

            int endLoopTick = director.CurrentTick + 4;
            input.SetMovementAndEndLoop(Vector2.right, 100, endLoopTick);
            yield return WaitForLoop(director, 2);

            Assert.That(director.LastCompletedRecording.Count, Is.EqualTo(endLoopTick + 1));
            Assert.That(director.LastCompletedRecording.Count, Is.LessThan(600));
            EchoPlayback echo = director.GetEchoPlayback(0);
            director.enabled = false;

            for (int tick = 0; tick < echo.RecordingLength + 20; tick++)
            {
                echo.SimulateTick(1f / 60f);
            }

            Vector3 stoppedPosition = echo.Motor.Position;
            Quaternion stoppedRotation = echo.Motor.Rotation;
            float stoppedDrift = echo.MaximumDrift;
            for (int tick = 0; tick < 20; tick++)
            {
                echo.SimulateTick(1f / 60f);
            }

            Assert.That(echo.PlaybackTick, Is.EqualTo(echo.RecordingLength));
            Assert.That(Vector3.Distance(echo.Motor.Position, stoppedPosition), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(echo.Motor.Rotation, stoppedRotation), Is.LessThan(0.001f));
            Assert.That(echo.MaximumDrift, Is.EqualTo(stoppedDrift));
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator ExistingEchoRewindsWhenNextLoopStarts()
        {
            yield return LoadReplayLab();
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();

            director.RequestLoopEnd();
            yield return WaitForLoop(director, 2);
            yield return new WaitForSecondsRealtime(0.1f);

            EchoPlayback firstEcho = director.GetEchoPlayback(0);
            director.RequestLoopEnd();
            yield return WaitForLoop(director, 3);

            Assert.That(firstEcho.PlaybackTick, Is.EqualTo(0));
            Assert.That(director.EchoCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator EchoOpensDoorSoCurrentPlayerCanReachGoal()
        {
            yield return LoadReplayLab();

            Scene scene = SceneManager.GetActiveScene();
            Assert.That(scene.name, Is.EqualTo("P0_ReplayLab"));
            AssertSceneHasNoMissingComponents(scene);

            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PlayerSimulation player = Object.FindAnyObjectByType<PlayerSimulation>();
            PressurePlate plate = Object.FindAnyObjectByType<PressurePlate>();
            DoorController door = Object.FindAnyObjectByType<DoorController>();
            GoalVolume goal = Object.FindAnyObjectByType<GoalVolume>();
            ResetRegistry resetRegistry = Object.FindAnyObjectByType<ResetRegistry>();
            Phase0DebugOverlay overlay = Object.FindAnyObjectByType<Phase0DebugOverlay>();
            Assert.That(director, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            Assert.That(plate, Is.Not.Null);
            Assert.That(door, Is.Not.Null);
            Assert.That(goal, Is.Not.Null);
            Assert.That(resetRegistry, Is.Not.Null);
            Assert.That(overlay, Is.Not.Null);
            Assert.That(director.HasValidReferences, Is.True);
            Assert.That(director.enabled, Is.True);
            Assert.That(player.HasValidReferences, Is.True);
            Assert.That(door.HasValidReferences, Is.True);
            Assert.That(resetRegistry.Count, Is.EqualTo(3));
            Assert.That(overlay.HasValidReferences, Is.True);
            Assert.That(plate.GetComponent<Collider>().isTrigger, Is.True);
            Assert.That(goal.GetComponent<Collider>().isTrigger, Is.True);

            ScriptedTestInputSource input = player.gameObject.AddComponent<ScriptedTestInputSource>();
            player.Configure(input, player.Motor);
            input.SetMovement(new Vector2(-2f, 3f).normalized, 100);

            float deadline = Time.realtimeSinceStartup + 3f;
            while (!plate.IsPressed && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(plate.IsPressed, Is.True, "The loop-1 player did not reach the plate.");
            director.RequestLoopEnd();
            yield return WaitForLoop(director, 2);
            Assert.That(plate.IsPressed, Is.False, "Loop reset left the plate latched.");

            input.SetMovement(Vector2.up, 600);
            bool observedEchoOnPlate = false;
            bool observedOpenDoor = false;
            deadline = Time.realtimeSinceStartup + 4f;
            while (!goal.IsReached && Time.realtimeSinceStartup < deadline)
            {
                observedEchoOnPlate |= plate.IsPressed;
                observedOpenDoor |= door.IsOpen;
                yield return null;
            }

            Assert.That(observedEchoOnPlate, Is.True, "The Echo did not reactivate the pressure plate.");
            Assert.That(observedOpenDoor, Is.True, "The pressure plate did not open the door.");
            Assert.That(goal.IsReached, Is.True, "The current player did not reach the goal.");
            Assert.That(director.MaximumReplayDrift, Is.LessThanOrEqualTo(0.05f));
            string driftMessage = $"PHASE0_INTEGRATION_MAX_DRIFT={director.MaximumReplayDrift:R}";
            LogAssert.Expect(LogType.Log, driftMessage);
            Debug.Log(driftMessage);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator GeneratedSceneSeparatesActorsFromEnvironmentAndKeepsTriggers()
        {
            yield return LoadReplayLab();
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PlayerSimulation player = Object.FindAnyObjectByType<PlayerSimulation>();
            GoalVolume goal = Object.FindAnyObjectByType<GoalVolume>();

            director.RequestLoopEnd();
            yield return WaitForLoop(director, 2);
            EchoPlayback echo = director.GetEchoPlayback(0);
            director.enabled = false;

            int playerLayer = LayerMask.NameToLayer("Player");
            int echoLayer = LayerMask.NameToLayer("Echo");
            int environmentLayer = LayerMask.NameToLayer("Environment");
            int interactionLayer = LayerMask.NameToLayer("InteractionTrigger");
            Assert.That(playerLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(echoLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(environmentLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(interactionLayer, Is.GreaterThanOrEqualTo(0));
            Assert.That(player.gameObject.layer, Is.EqualTo(playerLayer));
            Assert.That(echo.gameObject.layer, Is.EqualTo(echoLayer));
            Assert.That(Physics.GetIgnoreLayerCollision(playerLayer, echoLayer), Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(echoLayer, echoLayer), Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(playerLayer, environmentLayer), Is.False);
            Assert.That(Physics.GetIgnoreLayerCollision(echoLayer, environmentLayer), Is.False);
            Assert.That(Physics.GetIgnoreLayerCollision(playerLayer, interactionLayer), Is.False);
            Assert.That(Physics.GetIgnoreLayerCollision(echoLayer, interactionLayer), Is.False);
            Assert.That(player.GetComponent<Collider>().isTrigger, Is.False);
            Assert.That(echo.GetComponent<Collider>().isTrigger, Is.False);

            player.Motor.ResetPose(new Vector3(0f, 1f, 0f), Quaternion.identity);
            echo.Motor.ResetPose(new Vector3(0f, 1f, 0f), Quaternion.identity);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(Vector3.Distance(player.Motor.Position, echo.Motor.Position), Is.LessThan(0.0001f));

            player.Motor.ResetPose(new Vector3(0f, 1f, 0f), Quaternion.identity);
            SimulateMovement(player.Motor, Vector2.up, 90);
            Assert.That(player.Motor.Position.z, Is.LessThan(0.4f), "Player passed through the closed door.");
            echo.Motor.ResetPose(new Vector3(0f, 1f, 0f), Quaternion.identity);
            SimulateMovement(echo.Motor, Vector2.up, 90);
            Assert.That(echo.Motor.Position.z, Is.LessThan(0.4f), "Echo passed through the closed door.");

            player.Motor.ResetPose(new Vector3(5f, 1f, -3f), Quaternion.identity);
            SimulateMovement(player.Motor, Vector2.right, 90);
            Assert.That(player.Motor.Position.x, Is.LessThan(5.6f), "Player passed through the stage boundary.");
            echo.Motor.ResetPose(new Vector3(5f, 1f, -3f), Quaternion.identity);
            SimulateMovement(echo.Motor, Vector2.right, 90);
            Assert.That(echo.Motor.Position.x, Is.LessThan(5.6f), "Echo passed through the stage boundary.");

            LoopActor enteredGoalActor = null;
            goal.ActorEntered += actor => enteredGoalActor = actor;
            echo.Motor.ResetPose(new Vector3(0f, 1f, 4.25f), Quaternion.identity);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(enteredGoalActor, Is.SameAs(echo.Actor), "Echo was not detected by GoalVolume.");
            Assert.That(goal.IsReached, Is.False, "Echo must be detectable without completing the player goal.");
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator LoopDirectorKeepsThreeNonBlockingEchoes()
        {
            yield return LoadReplayLab();
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();

            for (int expectedLoop = 2; expectedLoop <= 5; expectedLoop++)
            {
                director.RequestLoopEnd();
                yield return WaitForLoop(director, expectedLoop);
            }

            Assert.That(director.EchoCount, Is.EqualTo(3));
            LoopActor[] actors = Object.FindObjectsByType<LoopActor>();
            Assert.That(actors.Length, Is.EqualTo(4));
            for (int i = 0; i < actors.Length; i++)
            {
                Collider actorCollider = actors[i].GetComponent<Collider>();
                Assert.That(actorCollider, Is.Not.Null);
                Assert.That(actorCollider.isTrigger, Is.False);
            }

            int playerLayer = LayerMask.NameToLayer("Player");
            int echoLayer = LayerMask.NameToLayer("Echo");
            Assert.That(Physics.GetIgnoreLayerCollision(playerLayer, echoLayer), Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(echoLayer, echoLayer), Is.True);
        }

        [UnityTest]
        public IEnumerator RemovingOldestEchoWhileOnPlateDoesNotLatchPlate()
        {
            yield return LoadReplayLab();
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PressurePlate plate = Object.FindAnyObjectByType<PressurePlate>();

            for (int expectedLoop = 2; expectedLoop <= 4; expectedLoop++)
            {
                director.RequestLoopEnd();
                yield return WaitForLoop(director, expectedLoop);
            }

            EchoPlayback oldest = director.GetEchoPlayback(0);
            oldest.Motor.ResetPose(plate.transform.position + Vector3.up, Quaternion.identity);
            Physics.SyncTransforms();
            plate.RegisterActor(oldest.Actor);
            Assert.That(plate.IsPressed, Is.True);

            director.RequestLoopEnd();
            yield return WaitForLoop(director, 5);
            yield return null;

            Assert.That(oldest == null, Is.True, "The oldest Echo was not destroyed at the three-Echo cap.");
            Assert.That(director.EchoCount, Is.EqualTo(3));
            Assert.That(plate.IsPressed, Is.False);
            Assert.That(plate.OccupantCount, Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }

        private static IEnumerator LoadReplayLab()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("P0_ReplayLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, "P0_ReplayLab must be generated and registered in Build Settings.");
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        private static IEnumerator WaitForLoop(LoopDirector director, int expectedLoop)
        {
            float deadline = Time.realtimeSinceStartup + 3f;
            while (director.LoopNumber < expectedLoop && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(director.LoopNumber, Is.EqualTo(expectedLoop));
        }

        private static void SimulateMovement(CharacterMotor motor, Vector2 movement, int ticks)
        {
            for (int tick = 0; tick < ticks; tick++)
            {
                motor.Simulate(movement, 1f / 60f);
            }
        }

        private static void AssertSceneHasNoMissingComponents(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
            {
                Component[] components = roots[rootIndex].GetComponentsInChildren<Component>(true);
                for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
                {
                    Assert.That(
                        components[componentIndex],
                        Is.Not.Null,
                        $"Missing script under scene root {roots[rootIndex].name}.");
                }
            }
        }
    }

    public sealed class ScriptedTestInputSource : MonoBehaviour, IInputSource
    {
        private Vector2 _movement;
        private int _stopAfterTick;
        private int _endLoopTick = -1;

        public void SetMovement(Vector2 movement, int stopAfterTick)
        {
            _movement = movement;
            _stopAfterTick = stopAfterTick;
            _endLoopTick = -1;
        }

        public void SetMovementAndEndLoop(Vector2 movement, int stopAfterTick, int endLoopTick)
        {
            _movement = movement;
            _stopAfterTick = stopAfterTick;
            _endLoopTick = endLoopTick;
        }

        public InputCommand Sample(int tick)
        {
            Vector2 movement = tick < _stopAfterTick ? _movement : Vector2.zero;
            InputButtonFlags buttons = tick == _endLoopTick
                ? InputButtonFlags.EndLoop
                : InputButtonFlags.None;
            return new InputCommand(tick, movement, buttons);
        }
    }
}
