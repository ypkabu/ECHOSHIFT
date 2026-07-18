using System.Linq;
using EchoShift.Editor;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoShift.Tests
{
    public sealed class Phase1InteractionEditModeTests
    {
        [Test]
        public void EmptyStableIdIsDetected()
        {
            StableId identity = CreateIdentityOnly(string.Empty);

            StableIdValidationResult result = StableIdSceneValidator.Validate(
                new[] { identity },
                out string error);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.EmptyCount, Is.EqualTo(1));
            Assert.That(error, Does.Contain("empty=1"));
            Object.DestroyImmediate(identity.gameObject);
        }

        [Test]
        public void DuplicateStableIdIsDetected()
        {
            StableId first = CreateIdentityOnly("duplicate-id");
            StableId second = CreateIdentityOnly("duplicate-id");

            StableIdValidationResult result = StableIdSceneValidator.Validate(
                new[] { first, second },
                out string error);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.DuplicateCount, Is.EqualTo(1));
            Assert.That(error, Does.Contain("duplicates=1"));
            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }

        [Test]
        public void RegistryResolvesCorrectTargetByStableId()
        {
            InteractionRegistry registry = new GameObject("Registry").AddComponent<InteractionRegistry>();
            CarryableBattery battery = CreateBattery(registry, "battery-a", Vector3.zero);

            bool resolved = registry.TryResolve("battery-a", out IInteractable target);

            Assert.That(resolved, Is.True);
            Assert.That(target, Is.SameAs(battery));
            Object.DestroyImmediate(battery.gameObject);
            Object.DestroyImmediate(registry.gameObject);
        }

        [Test]
        public void RegistryRemovesInactiveTarget()
        {
            InteractionRegistry registry = new GameObject("Registry").AddComponent<InteractionRegistry>();
            CarryableBattery battery = CreateBattery(registry, "battery-a", Vector3.zero);
            Assert.That(registry.TryResolve("battery-a", out _), Is.True);

            battery.gameObject.SetActive(false);
            Assert.That(registry.TryResolve("battery-a", out _), Is.False);
            Assert.That(registry.Count, Is.Zero);

            Object.DestroyImmediate(battery.gameObject);
            Assert.That(registry.TryResolve("battery-a", out _), Is.False);
            Object.DestroyImmediate(registry.gameObject);
        }

        [Test]
        public void InteractionEventsFinalizeInTickOrder()
        {
            ReplayRecorder recorder = new ReplayRecorder(8);
            recorder.TryRecord(CreateFrame(0));
            recorder.TryRecord(CreateFrame(1));
            Assert.That(
                recorder.TryRecordInteraction(CreateInteraction(0, "battery")),
                Is.EqualTo(InteractionRecordResult.Recorded));
            Assert.That(
                recorder.TryRecordInteraction(CreateInteraction(1, "socket")),
                Is.EqualTo(InteractionRecordResult.Recorded));

            ReplayRecording recording = recorder.FinalizeRecording();

            Assert.That(recording.Interactions.Count, Is.EqualTo(2));
            Assert.That(recording.Interactions[0].Tick, Is.EqualTo(0));
            Assert.That(recording.Interactions[1].Tick, Is.EqualTo(1));
        }

        [Test]
        public void FinalizedInteractionEventsCannotBeChanged()
        {
            ReplayRecorder recorder = new ReplayRecorder(4);
            recorder.TryRecord(CreateFrame(0));
            recorder.TryRecordInteraction(CreateInteraction(0, "battery"));
            ReplayRecording recording = recorder.FinalizeRecording();

            InteractionRecordResult result = recorder.TryRecordInteraction(
                CreateInteraction(0, "other"));

            Assert.That(result, Is.EqualTo(InteractionRecordResult.Finalized));
            Assert.That(recording.Interactions.Count, Is.EqualTo(1));
            Assert.That(recording.Interactions[0].TargetStableId, Is.EqualTo("battery"));
        }

        [Test]
        public void ShortReplayRejectsInteractionOutsideRecordedFrames()
        {
            ReplayRecorder recorder = new ReplayRecorder(600);
            recorder.TryRecord(CreateFrame(0));

            InteractionRecordResult result = recorder.TryRecordInteraction(
                CreateInteraction(1, "outside"));
            ReplayRecording recording = recorder.FinalizeRecording();

            Assert.That(result, Is.EqualTo(InteractionRecordResult.OutsideRecordedFrames));
            Assert.That(recording.Count, Is.EqualTo(1));
            Assert.That(recording.Interactions.Count, Is.Zero);
        }

        [Test]
        public void CandidateSelectionUsesStableIdAsDeterministicTieBreak()
        {
            InteractionCandidateScore current = new InteractionCandidateScore(
                true, 1f, 0.5f, "b-target");
            InteractionCandidateScore candidate = new InteractionCandidateScore(
                true, 1f, 0.5f, "a-target");

            Assert.That(
                InteractionCandidateSelector.IsBetter(candidate, current),
                Is.True);
            Assert.That(
                InteractionCandidateSelector.IsBetter(current, candidate),
                Is.False);
        }

        [Test]
        public void BatteryRejectsSecondHolder()
        {
            InteractionRegistry registry = new GameObject("Registry").AddComponent<InteractionRegistry>();
            TestActor first = CreateActor(registry, "First Actor", Vector3.zero);
            TestActor second = CreateActor(registry, "Second Actor", Vector3.zero);
            CarryableBattery battery = CreateBattery(registry, "battery-a", Vector3.forward * 0.5f);
            battery.CaptureInitialState();

            InteractionContext firstContext = first.CreateContext(0);
            InteractionContext secondContext = second.CreateContext(0);
            Assert.That(
                battery.TryInteract(
                    firstContext,
                    InteractionKind.PickupBattery,
                    out InteractionFailureReason firstFailure),
                Is.True);
            Assert.That(firstFailure, Is.EqualTo(InteractionFailureReason.None));
            Assert.That(
                battery.TryInteract(
                    secondContext,
                    InteractionKind.PickupBattery,
                    out InteractionFailureReason secondFailure),
                Is.False);
            Assert.That(secondFailure, Is.EqualTo(InteractionFailureReason.HeldByAnotherActor));
            Assert.That(battery.Holder, Is.SameAs(first.Interactor));

            Object.DestroyImmediate(first.GameObject);
            Object.DestroyImmediate(second.GameObject);
            Object.DestroyImmediate(battery.gameObject);
            Object.DestroyImmediate(registry.gameObject);
        }

        [Test]
        public void BatteryAndSocketAreConsistentAfterReset()
        {
            InteractionRegistry registry = new GameObject("Registry").AddComponent<InteractionRegistry>();
            TestActor actor = CreateActor(registry, "Actor", Vector3.zero);
            CarryableBattery battery = CreateBattery(registry, "battery-a", Vector3.forward * 0.5f);
            PowerSocket socket = CreateSocket(registry, "socket-a", Vector3.forward);
            battery.CaptureInitialState();
            socket.CaptureInitialState();
            InteractionContext context = actor.CreateContext(0);

            Assert.That(
                battery.TryInteract(context, InteractionKind.PickupBattery, out _),
                Is.True);
            Assert.That(
                socket.TryInteract(context, InteractionKind.InsertBattery, out _),
                Is.True);
            Assert.That(socket.IsPowered, Is.True);

            battery.RestoreInitialState();
            socket.RestoreInitialState();

            Assert.That(battery.Holder, Is.Null);
            Assert.That(battery.InsertedSocket, Is.Null);
            Assert.That(socket.InsertedBattery, Is.Null);
            Assert.That(socket.IsPowered, Is.False);
            Assert.That(actor.Interactor.CarriedBattery, Is.Null);

            Object.DestroyImmediate(actor.GameObject);
            Object.DestroyImmediate(battery.gameObject);
            Object.DestroyImmediate(socket.gameObject);
            Object.DestroyImmediate(registry.gameObject);
        }

        [Test]
        public void InputActionsContainRequiredKeyboardAndGamepadBindings()
        {
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                P0SceneBuilder.InputActionsPath);
            InputActionMap gameplay = asset.FindActionMap("Gameplay", true);

            Assert.That(
                gameplay.FindAction("Move", true).bindings.Select(binding => binding.path),
                Does.Contain("<Gamepad>/leftStick"));
            Assert.That(
                gameplay.FindAction("Interact", true).bindings.Select(binding => binding.path),
                Does.Contain("<Keyboard>/e").And.Contain("<Gamepad>/buttonSouth"));
            Assert.That(
                gameplay.FindAction("EndLoop", true).bindings.Select(binding => binding.path),
                Does.Contain("<Keyboard>/r").And.Contain("<Gamepad>/start"));
        }

        private static StableId CreateIdentityOnly(string value)
        {
            GameObject gameObject = new GameObject("Identity");
            StableId identity = gameObject.AddComponent<StableId>();
            identity.Configure(value, null, null);
            return identity;
        }

        private static TestActor CreateActor(
            InteractionRegistry registry,
            string name,
            Vector3 position)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.position = position;
            LoopActor loopActor = gameObject.AddComponent<LoopActor>();
            loopActor.Configure(LoopActorKind.Player);
            GameObject carryObject = new GameObject("Carry Socket");
            carryObject.transform.SetParent(gameObject.transform, false);
            InteractionSensor sensor = gameObject.AddComponent<InteractionSensor>();
            sensor.Configure(2f, 0);
            Interactor interactor = gameObject.AddComponent<Interactor>();
            interactor.Configure(loopActor, sensor, carryObject.transform, registry);
            return new TestActor(gameObject, loopActor, interactor);
        }

        private static CarryableBattery CreateBattery(
            InteractionRegistry registry,
            string id,
            Vector3 position)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = id;
            gameObject.transform.position = position;
            Collider collider = gameObject.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            StableId identity = gameObject.AddComponent<StableId>();
            CarryableBattery battery = gameObject.AddComponent<CarryableBattery>();
            battery.Configure(identity, collider, body);
            identity.Configure(id, registry, battery);
            return battery;
        }

        private static PowerSocket CreateSocket(
            InteractionRegistry registry,
            string id,
            Vector3 position)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = id;
            gameObject.transform.position = position;
            gameObject.GetComponent<Collider>().isTrigger = true;
            StableId identity = gameObject.AddComponent<StableId>();
            PowerSocket socket = gameObject.AddComponent<PowerSocket>();
            socket.Configure(identity, gameObject.transform);
            identity.Configure(id, registry, socket);
            return socket;
        }

        private static ReplayFrame CreateFrame(int tick)
        {
            return new ReplayFrame(
                new InputCommand(tick, Vector2.zero, InputButtonFlags.None),
                Vector3.zero,
                Quaternion.identity);
        }

        private static InteractionCommand CreateInteraction(int tick, string id)
        {
            return new InteractionCommand(
                tick,
                InteractionKind.PickupBattery,
                id,
                Vector3.zero);
        }

        private readonly struct TestActor
        {
            public TestActor(GameObject gameObject, LoopActor actor, Interactor interactor)
            {
                GameObject = gameObject;
                Actor = actor;
                Interactor = interactor;
            }

            public GameObject GameObject { get; }
            public LoopActor Actor { get; }
            public Interactor Interactor { get; }

            public InteractionContext CreateContext(int tick)
            {
                return new InteractionContext(Actor, Interactor, tick, false, 2f);
            }
        }
    }
}
