using System.Collections;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using EchoShift.Reset;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests
{
    public sealed class Phase1InteractionPlayModeTests
    {
        private const float TickDuration = 1f / 60f;
        private const int InteractionTestLayer = 31;

        [UnityTest]
        public IEnumerator PlayerCanPickUpBattery()
        {
            TestRig rig = CreateRig();
            Physics.SyncTransforms();

            InteractionExecution execution = rig.First.Interactor.TryLiveInteraction(0);

            Assert.That(execution.Succeeded, Is.True);
            Assert.That(execution.Command.Kind, Is.EqualTo(InteractionKind.PickupBattery));
            Assert.That(rig.Battery.Holder, Is.SameAs(rig.First.Interactor));
            Assert.That(rig.First.Interactor.CarriedBattery, Is.SameAs(rig.Battery));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator HeldBatteryFollowsCarrySocket()
        {
            TestRig rig = CreateRig();
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);

            rig.First.CarrySocket.position = new Vector3(3f, 2f, -1f);
            rig.First.CarrySocket.rotation = Quaternion.Euler(0f, 70f, 0f);
            yield return null;

            Assert.That(Vector3.Distance(rig.Battery.transform.position, rig.First.CarrySocket.position), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(rig.Battery.transform.rotation, rig.First.CarrySocket.rotation), Is.LessThan(0.001f));
            Assert.That(rig.Battery.transform.IsChildOf(rig.First.GameObject.transform), Is.False,
                "A held battery must not be destroyed with its actor hierarchy.");
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator PlayerCanDropHeldBatteryAwayFromSocket()
        {
            TestRig rig = CreateRig();
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);
            rig.First.GameObject.transform.position = new Vector3(-3f, 0f, 0f);
            Physics.SyncTransforms();
            yield return null;

            InteractionExecution execution = rig.First.Interactor.TryLiveInteraction(1);

            Assert.That(execution.Succeeded, Is.True);
            Assert.That(execution.Command.Kind, Is.EqualTo(InteractionKind.DropBattery));
            Assert.That(rig.Battery.Holder, Is.Null);
            Assert.That(rig.First.Interactor.CarriedBattery, Is.Null);
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator InvalidSocketCandidateDoesNotFallbackToDroppingHeldBattery()
        {
            TestRig rig = CreateRig();
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);
            rig.First.GameObject.transform.position =
                rig.Socket.transform.position + Vector3.back * 2.25f;
            Physics.SyncTransforms();

            InteractionExecution execution = rig.First.Interactor.TryLiveInteraction(1);

            Assert.That(rig.First.Interactor.Sensor.CurrentTarget, Is.SameAs(rig.Socket));
            Assert.That(rig.First.Interactor.Sensor.CurrentCanInteract, Is.False);
            Assert.That(execution.Succeeded, Is.False);
            Assert.That(execution.Command.Kind, Is.EqualTo(InteractionKind.InsertBattery));
            Assert.That(execution.FailureReason, Is.EqualTo(InteractionFailureReason.OutOfRange));
            Assert.That(rig.First.Interactor.CarriedBattery, Is.SameAs(rig.Battery));
            Assert.That(rig.Battery.Holder, Is.SameAs(rig.First.Interactor));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator BatteryCanBeInsertedIntoPowerSocket()
        {
            TestRig rig = CreateRig();
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);

            bool inserted = rig.Socket.TryInteract(
                rig.First.CreateContext(1),
                InteractionKind.InsertBattery,
                out InteractionFailureReason failure);

            Assert.That(inserted, Is.True);
            Assert.That(failure, Is.EqualTo(InteractionFailureReason.None));
            Assert.That(rig.Socket.IsPowered, Is.True);
            Assert.That(rig.Socket.InsertedBattery, Is.SameAs(rig.Battery));
            Assert.That(rig.Battery.InsertedSocket, Is.SameAs(rig.Socket));
            Assert.That(rig.First.Interactor.CarriedBattery, Is.Null);
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator PoweredSocketOpensDoor()
        {
            TestRig rig = CreateRig(true);
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);
            Assert.That(Insert(rig.Socket, rig.First, 1), Is.True);

            yield return WaitForDoorState(rig.Door, true);

            Assert.That(rig.Socket.RequestsDoorOpen, Is.True);
            Assert.That(rig.Door.IsOpenRequested, Is.True);
            Assert.That(rig.Door.IsOpen, Is.True);
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator PlayerAndEchoCannotOwnSameBattery()
        {
            TestRig rig = CreateRig();
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);

            bool secondPickup = rig.Battery.TryInteract(
                rig.Second.CreateContext(0),
                InteractionKind.PickupBattery,
                out InteractionFailureReason failure);

            Assert.That(secondPickup, Is.False);
            Assert.That(failure, Is.EqualTo(InteractionFailureReason.HeldByAnotherActor));
            Assert.That(rig.Battery.Holder, Is.SameAs(rig.First.Interactor));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator ActorDisableAndDestroyReleaseBatteryWithoutDestroyingIt()
        {
            TestRig rig = CreateRig();
            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);

            rig.First.GameObject.SetActive(false);
            yield return null;
            Assert.That(rig.Battery, Is.Not.Null);
            Assert.That(rig.Battery.Holder, Is.Null);

            rig.First.GameObject.SetActive(true);
            rig.First.GameObject.transform.position = Vector3.zero;
            Assert.That(PickUp(rig.Battery, rig.First, 1), Is.True);
            Object.Destroy(rig.First.GameObject);
            yield return null;

            Assert.That(rig.Battery, Is.Not.Null, "Destroying an actor also destroyed its held battery.");
            Assert.That(rig.Battery.Holder, Is.Null);
            Assert.That(rig.Registry.TryResolve("battery-test", out IInteractable target), Is.True);
            Assert.That(target, Is.SameAs(rig.Battery));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator DisabledTargetLeavesRegistryAndReregistersWhenEnabled()
        {
            TestRig rig = CreateRig();
            Assert.That(rig.Registry.TryResolve("battery-test", out _), Is.True);

            rig.Battery.gameObject.SetActive(false);
            yield return null;
            Assert.That(rig.Registry.TryResolve("battery-test", out _), Is.False);

            rig.Battery.gameObject.SetActive(true);
            yield return null;
            Assert.That(rig.Registry.TryResolve("battery-test", out IInteractable target), Is.True);
            Assert.That(target, Is.SameAs(rig.Battery));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator WorldResetRestoresBatterySocketAndDoorConsistently()
        {
            TestRig rig = CreateRig(true);
            GameObject resetObject = new GameObject("Reset Registry");
            resetObject.transform.SetParent(rig.Root.transform);
            resetObject.SetActive(false);
            ResetRegistry registry = resetObject.AddComponent<ResetRegistry>();
            registry.Configure(new MonoBehaviour[] { rig.Battery, rig.Socket, rig.Door });
            resetObject.SetActive(true);
            registry.CaptureInitialStates();

            Assert.That(PickUp(rig.Battery, rig.First, 0), Is.True);
            Assert.That(Insert(rig.Socket, rig.First, 1), Is.True);
            yield return WaitForDoorState(rig.Door, true);

            registry.RestoreInitialStates();
            yield return null;

            Assert.That(rig.Battery.Holder, Is.Null);
            Assert.That(rig.Battery.InsertedSocket, Is.Null);
            Assert.That(rig.First.Interactor.CarriedBattery, Is.Null);
            Assert.That(rig.Socket.InsertedBattery, Is.Null);
            Assert.That(rig.Socket.IsPowered, Is.False);
            Assert.That(rig.Door.IsOpenRequested, Is.False);
            Assert.That(rig.Door.IsOpen, Is.False);
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator EchoPicksUpRecordedStableIdBattery()
        {
            TestRig rig = CreateRig();
            ReplayRecording recording = CreateRecording(
                rig.First.GameObject.transform.position,
                new InteractionCommand(0, InteractionKind.PickupBattery, "battery-test", Vector3.zero));
            EchoPlayback echo = CreateEcho(rig, recording);

            echo.SimulateTick(TickDuration);

            Assert.That(echo.InteractionSuccessCount, Is.EqualTo(1));
            Assert.That(echo.InteractionFailureCount, Is.Zero);
            Assert.That(rig.Battery.Holder, Is.SameAs(echo.Interactor));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator EchoInsertsBatteryIntoRecordedStableIdSocket()
        {
            TestRig rig = CreateRig();
            ReplayRecorder recorder = new ReplayRecorder(2);
            Vector3 position = rig.First.GameObject.transform.position;
            Assert.That(recorder.TryRecord(CreateFrame(0, position)), Is.EqualTo(ReplayRecordResult.Recorded));
            Assert.That(recorder.TryRecordInteraction(
                new InteractionCommand(0, InteractionKind.PickupBattery, "battery-test", position)),
                Is.EqualTo(InteractionRecordResult.Recorded));
            Assert.That(recorder.TryRecord(CreateFrame(1, position)), Is.EqualTo(ReplayRecordResult.Recorded));
            Assert.That(recorder.TryRecordInteraction(
                new InteractionCommand(1, InteractionKind.InsertBattery, "socket-test", position)),
                Is.EqualTo(InteractionRecordResult.Recorded));
            EchoPlayback echo = CreateEcho(rig, recorder.FinalizeRecording());

            echo.SimulateTick(TickDuration);
            echo.SimulateTick(TickDuration);

            Assert.That(echo.InteractionSuccessCount, Is.EqualTo(2));
            Assert.That(echo.InteractionFailureCount, Is.Zero);
            Assert.That(rig.Socket.IsPowered, Is.True);
            Assert.That(rig.Socket.InsertedBattery, Is.SameAs(rig.Battery));
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator EchoDoesNotFallBackWhenRecordedTargetIsMissing()
        {
            TestRig rig = CreateRig();
            ReplayRecording recording = CreateRecording(
                rig.First.GameObject.transform.position,
                new InteractionCommand(0, InteractionKind.PickupBattery, "missing-battery", Vector3.zero));
            EchoPlayback echo = CreateEcho(rig, recording);

            echo.SimulateTick(TickDuration);

            Assert.That(echo.InteractionSuccessCount, Is.Zero);
            Assert.That(echo.InteractionFailureCount, Is.EqualTo(1));
            Assert.That(echo.LastInteractionFailure, Is.EqualTo(InteractionFailureReason.TargetNotFound));
            Assert.That(rig.Battery.Holder, Is.Null, "Playback silently substituted a nearby battery.");
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator ShortReplayStopsInteractionPlaybackAtRecordedEnd()
        {
            TestRig rig = CreateRig();
            ReplayRecording recording = CreateRecording(
                rig.First.GameObject.transform.position,
                new InteractionCommand(0, InteractionKind.PickupBattery, "battery-test", Vector3.zero));
            EchoPlayback echo = CreateEcho(rig, recording);

            for (int tick = 0; tick < 20; tick++)
            {
                echo.SimulateTick(TickDuration);
            }

            Assert.That(echo.PlaybackTick, Is.EqualTo(1));
            Assert.That(echo.NextInteractionTick, Is.EqualTo(-1));
            Assert.That(echo.InteractionSuccessCount, Is.EqualTo(1));
            Assert.That(echo.InteractionFailureCount, Is.Zero);
            LogAssert.NoUnexpectedReceived();
            yield return DestroyRig(rig);
        }

        [UnityTest]
        public IEnumerator GeneratedPhase1SceneCompletesRecordedInteractionFlow()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("P1_InteractionLab", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, "P1_InteractionLab must be generated and registered in Build Settings.");
            while (!load.isDone)
            {
                yield return null;
            }

            yield return null;
            Scene scene = SceneManager.GetActiveScene();
            Assert.That(scene.name, Is.EqualTo("P1_InteractionLab"));
            AssertSceneHasNoMissingComponents(scene);

            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PlayerSimulation player = Object.FindAnyObjectByType<PlayerSimulation>();
            CarryableBattery battery = Object.FindAnyObjectByType<CarryableBattery>();
            PowerSocket socket = Object.FindAnyObjectByType<PowerSocket>();
            DoorController door = Object.FindAnyObjectByType<DoorController>();
            GoalVolume goal = Object.FindAnyObjectByType<GoalVolume>();
            InteractionRegistry interactionRegistry = Object.FindAnyObjectByType<InteractionRegistry>();
            ResetRegistry resetRegistry = Object.FindAnyObjectByType<ResetRegistry>();
            Phase0DebugOverlay overlay = Object.FindAnyObjectByType<Phase0DebugOverlay>();

            Assert.That(director, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            Assert.That(battery, Is.Not.Null);
            Assert.That(socket, Is.Not.Null);
            Assert.That(door, Is.Not.Null);
            Assert.That(goal, Is.Not.Null);
            Assert.That(interactionRegistry, Is.Not.Null);
            Assert.That(resetRegistry, Is.Not.Null);
            Assert.That(overlay, Is.Not.Null);
            Assert.That(director.HasValidReferences, Is.True);
            Assert.That(player.HasValidReferences, Is.True);
            Assert.That(player.Interactor.HasValidReferences, Is.True);
            Assert.That(battery.StableId.HasValidConfiguration, Is.True);
            Assert.That(socket.StableId.HasValidConfiguration, Is.True);
            Assert.That(battery.StableId.Value, Is.Not.Empty);
            Assert.That(socket.StableId.Value, Is.Not.Empty);
            Assert.That(battery.StableId.Value, Is.Not.EqualTo(socket.StableId.Value));
            Assert.That(interactionRegistry.Count, Is.EqualTo(2));
            Assert.That(door.HasValidReferences, Is.True);
            Assert.That(resetRegistry.Count, Is.EqualTo(4));
            Assert.That(overlay.HasValidReferences, Is.True);
            Assert.That(overlay.IsPhase1Configured, Is.True);
            Assert.That(goal.GetComponent<Collider>().isTrigger, Is.True);

            Phase1RouteInputSource input = player.gameObject.AddComponent<Phase1RouteInputSource>();
            player.Configure(input, player.Motor, player.Interactor);
            input.SetRecordRoute();

            yield return WaitForLoop(director, 2);
            Assert.That(director.LastCompletedRecording, Is.Not.Null);
            Assert.That(director.LastCompletedRecording.Interactions.Count, Is.EqualTo(2));
            Assert.That(director.LastCompletedRecording.Interactions[0].Kind, Is.EqualTo(InteractionKind.PickupBattery));
            Assert.That(director.LastCompletedRecording.Interactions[1].Kind, Is.EqualTo(InteractionKind.InsertBattery));
            Assert.That(battery.Holder, Is.Null, "Loop reset retained the live player's ownership.");
            Assert.That(socket.IsPowered, Is.False, "Loop reset retained the inserted battery.");

            input.SetGoalRoute();
            EchoPlayback echo = director.GetEchoPlayback(0);
            bool observedPoweredSocket = false;
            bool observedOpenDoor = false;
            float deadline = Time.realtimeSinceStartup + 6f;
            while (!goal.IsReached && Time.realtimeSinceStartup < deadline)
            {
                observedPoweredSocket |= socket.IsPowered;
                observedOpenDoor |= door.IsOpen;
                yield return null;
            }

            Assert.That(echo.InteractionSuccessCount, Is.EqualTo(2));
            Assert.That(echo.InteractionFailureCount, Is.Zero);
            Assert.That(echo.LastInteractionFailure, Is.EqualTo(InteractionFailureReason.None));
            Assert.That(observedPoweredSocket, Is.True, "Echo did not insert the battery.");
            Assert.That(observedOpenDoor, Is.True, "Powered socket did not open the door.");
            Assert.That(socket.InsertedBattery, Is.SameAs(battery));
            Assert.That(battery.Holder, Is.Null);
            Assert.That(goal.IsReached, Is.True, "The current player did not reach the goal.");
            Assert.That(director.MaximumReplayDrift, Is.LessThanOrEqualTo(0.05f));

            string validation =
                $"PHASE1_INTEGRATION_MAX_DRIFT={director.MaximumReplayDrift:R};" +
                $"SUCCESS={echo.InteractionSuccessCount};FAILURE={echo.InteractionFailureCount}";
            LogAssert.Expect(LogType.Log, validation);
            Debug.Log(validation);
            LogAssert.NoUnexpectedReceived();
        }

        private static TestRig CreateRig(bool withDoor = false)
        {
            GameObject root = new GameObject("Phase 1 Test Rig");
            InteractionRegistry registry = new GameObject("Interaction Registry").AddComponent<InteractionRegistry>();
            registry.transform.SetParent(root.transform);
            TestActor first = CreateActor(root.transform, registry, "Player Actor", LoopActorKind.Player);
            TestActor second = CreateActor(root.transform, registry, "Echo Actor", LoopActorKind.Echo);
            CarryableBattery battery = CreateBattery(root.transform, registry, "battery-test", new Vector3(0f, 0f, 0.5f));
            PowerSocket socket = CreateSocket(root.transform, registry, "socket-test", new Vector3(0f, 0f, 1f));
            DoorController door = withDoor ? CreateDoor(root.transform, socket) : null;
            return new TestRig(root, registry, first, second, battery, socket, door);
        }

        private static TestActor CreateActor(
            Transform parent,
            InteractionRegistry registry,
            string name,
            LoopActorKind kind)
        {
            GameObject actorObject = new GameObject(name);
            actorObject.transform.SetParent(parent);
            LoopActor actor = actorObject.AddComponent<LoopActor>();
            actor.Configure(kind);
            CharacterMotor motor = actorObject.AddComponent<CharacterMotor>();
            motor.Configure(4f, 0.45f, 2f, 0);
            GameObject carryObject = new GameObject("Carry Socket");
            carryObject.transform.SetParent(actorObject.transform, false);
            carryObject.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
            InteractionSensor sensor = actorObject.AddComponent<InteractionSensor>();
            sensor.Configure(2f, 1 << InteractionTestLayer);
            Interactor interactor = actorObject.AddComponent<Interactor>();
            interactor.Configure(actor, sensor, carryObject.transform, registry);
            return new TestActor(actorObject, actor, motor, carryObject.transform, interactor);
        }

        private static CarryableBattery CreateBattery(
            Transform parent,
            InteractionRegistry registry,
            string id,
            Vector3 position)
        {
            GameObject batteryObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            batteryObject.name = id;
            batteryObject.layer = InteractionTestLayer;
            batteryObject.transform.SetParent(parent);
            batteryObject.transform.position = position;
            Collider interactionCollider = batteryObject.GetComponent<Collider>();
            interactionCollider.isTrigger = true;
            Rigidbody body = batteryObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            StableId identity = batteryObject.AddComponent<StableId>();
            CarryableBattery battery = batteryObject.AddComponent<CarryableBattery>();
            battery.Configure(identity, interactionCollider, body);
            identity.Configure(id, registry, battery);
            battery.CaptureInitialState();
            return battery;
        }

        private static PowerSocket CreateSocket(
            Transform parent,
            InteractionRegistry registry,
            string id,
            Vector3 position)
        {
            GameObject socketObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            socketObject.name = id;
            socketObject.layer = InteractionTestLayer;
            socketObject.transform.SetParent(parent);
            socketObject.transform.position = position;
            socketObject.GetComponent<Collider>().isTrigger = true;
            GameObject insertionObject = new GameObject("Insertion Point");
            insertionObject.transform.SetParent(socketObject.transform, false);
            insertionObject.transform.localPosition = Vector3.up;
            StableId identity = socketObject.AddComponent<StableId>();
            PowerSocket socket = socketObject.AddComponent<PowerSocket>();
            socket.Configure(identity, insertionObject.transform);
            identity.Configure(id, registry, socket);
            socket.CaptureInitialState();
            return socket;
        }

        private static DoorController CreateDoor(Transform parent, PowerSocket socket)
        {
            GameObject doorObject = new GameObject("Powered Door");
            doorObject.transform.SetParent(parent);
            doorObject.SetActive(false);
            doorObject.AddComponent<BoxCollider>();
            DoorController door = doorObject.AddComponent<DoorController>();
            door.Configure(new MonoBehaviour[] { socket }, Vector3.up * 3f, 100f);
            door.CaptureInitialState();
            doorObject.SetActive(true);
            return door;
        }

        private static EchoPlayback CreateEcho(TestRig rig, ReplayRecording recording)
        {
            TestActor actor = CreateActor(
                rig.Root.transform,
                rig.Registry,
                "Recorded Echo",
                LoopActorKind.Echo);
            EchoPlayback playback = actor.GameObject.AddComponent<EchoPlayback>();
            playback.Initialize(recording, actor.Motor, actor.Actor, 0.05f, actor.Interactor);
            return playback;
        }

        private static ReplayRecording CreateRecording(Vector3 position, InteractionCommand command)
        {
            ReplayRecorder recorder = new ReplayRecorder(1);
            Assert.That(recorder.TryRecord(CreateFrame(0, position)), Is.EqualTo(ReplayRecordResult.Recorded));
            Assert.That(recorder.TryRecordInteraction(command), Is.EqualTo(InteractionRecordResult.Recorded));
            return recorder.FinalizeRecording();
        }

        private static ReplayFrame CreateFrame(int tick, Vector3 position)
        {
            return new ReplayFrame(
                new InputCommand(tick, Vector2.zero, InputButtonFlags.None),
                position,
                Quaternion.identity);
        }

        private static bool PickUp(CarryableBattery battery, TestActor actor, int tick)
        {
            return battery.TryInteract(
                actor.CreateContext(tick),
                InteractionKind.PickupBattery,
                out _);
        }

        private static bool Insert(PowerSocket socket, TestActor actor, int tick)
        {
            return socket.TryInteract(
                actor.CreateContext(tick),
                InteractionKind.InsertBattery,
                out _);
        }

        private static IEnumerator WaitForDoorState(DoorController door, bool expectedOpen)
        {
            float deadline = Time.realtimeSinceStartup + 1f;
            while (door.IsOpen != expectedOpen && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(door.IsOpen, Is.EqualTo(expectedOpen));
        }

        private static IEnumerator WaitForLoop(LoopDirector director, int expectedLoop)
        {
            float deadline = Time.realtimeSinceStartup + 4f;
            while (director.LoopNumber < expectedLoop && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(director.LoopNumber, Is.EqualTo(expectedLoop));
        }

        private static IEnumerator DestroyRig(TestRig rig)
        {
            if (rig.Root != null)
            {
                Object.Destroy(rig.Root);
            }

            yield return null;
            LogAssert.NoUnexpectedReceived();
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

        private sealed class TestRig
        {
            public TestRig(
                GameObject root,
                InteractionRegistry registry,
                TestActor first,
                TestActor second,
                CarryableBattery battery,
                PowerSocket socket,
                DoorController door)
            {
                Root = root;
                Registry = registry;
                First = first;
                Second = second;
                Battery = battery;
                Socket = socket;
                Door = door;
            }

            public GameObject Root { get; }
            public InteractionRegistry Registry { get; }
            public TestActor First { get; }
            public TestActor Second { get; }
            public CarryableBattery Battery { get; }
            public PowerSocket Socket { get; }
            public DoorController Door { get; }
        }

        private sealed class TestActor
        {
            public TestActor(
                GameObject gameObject,
                LoopActor actor,
                CharacterMotor motor,
                Transform carrySocket,
                Interactor interactor)
            {
                GameObject = gameObject;
                Actor = actor;
                Motor = motor;
                CarrySocket = carrySocket;
                Interactor = interactor;
            }

            public GameObject GameObject { get; }
            public LoopActor Actor { get; }
            public CharacterMotor Motor { get; }
            public Transform CarrySocket { get; }
            public Interactor Interactor { get; }

            public InteractionContext CreateContext(int tick)
            {
                return new InteractionContext(Actor, Interactor, tick, false, 2f);
            }
        }
    }

    public sealed class Phase1RouteInputSource : MonoBehaviour, IInputSource
    {
        private bool _goalRoute;

        public void SetRecordRoute()
        {
            _goalRoute = false;
        }

        public void SetGoalRoute()
        {
            _goalRoute = true;
        }

        public InputCommand Sample(int tick)
        {
            if (_goalRoute)
            {
                return new InputCommand(tick, Vector2.up, InputButtonFlags.None);
            }

            Vector2 movement = Vector2.zero;
            if (tick <= 41)
            {
                movement = new Vector2(-1f, 1f).normalized;
            }
            else if (tick >= 43 && tick <= 72)
            {
                movement = Vector2.up;
            }

            InputButtonFlags buttons = InputButtonFlags.None;
            if (tick == 42 || tick == 73)
            {
                buttons |= InputButtonFlags.Interact;
            }

            if (tick == 80)
            {
                buttons |= InputButtonFlags.EndLoop;
            }

            return new InputCommand(tick, movement, buttons);
        }
    }
}
