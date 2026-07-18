using System;
using System.Linq;
using System.Reflection;
using EchoShift.Core;
using EchoShift.Editor;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EchoShift.Tests
{
    public sealed class Phase2CoordinationEditModeTests
    {
        [Test]
        public void Phase2SettingsCalculateFifteenSecondsAndNineHundredTicks()
        {
            LoopSettings settings = AssetDatabase.LoadAssetAtPath<LoopSettings>(P2SceneBuilder.SettingsPath);
            Assert.That(settings.TickRate, Is.EqualTo(60));
            Assert.That(settings.LoopDurationSeconds, Is.EqualTo(15));
            Assert.That(settings.MaxTicks, Is.EqualTo(900));
            Assert.That(settings.MaxEchoes, Is.EqualTo(3));
            Assert.That(settings.DriftTolerance, Is.EqualTo(0.05f));
        }

        [Test]
        public void ReplayRecorderCapacityUsesSceneSettings()
        {
            LoopSettings settings = AssetDatabase.LoadAssetAtPath<LoopSettings>(P2SceneBuilder.SettingsPath);
            ReplayRecorder recorder = new ReplayRecorder(settings.MaxTicks);
            Assert.That(recorder.Capacity, Is.EqualTo(900));
        }

        [Test]
        public void InvalidLoopSettingsAreRejected()
        {
            LoopSettings settings = ScriptableObject.CreateInstance<LoopSettings>();
            SerializedObject serialized = new SerializedObject(settings);
            serialized.FindProperty("tickRate").intValue = 0;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(settings.TryValidate(out string zeroRate), Is.False);
            Assert.That(zeroRate, Does.Contain("Tick rate"));
            serialized.FindProperty("tickRate").intValue = 60;
            serialized.FindProperty("driftTolerance").floatValue = -1f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(settings.TryValidate(out string negativeDrift), Is.False);
            Assert.That(negativeDrift, Does.Contain("Drift tolerance"));
            serialized.FindProperty("driftTolerance").floatValue = 0.05f;
            serialized.FindProperty("loopDurationSeconds").intValue = 601;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Assert.That(settings.TryValidate(out string unsafeTicks), Is.False);
            Assert.That(unsafeTicks, Does.Contain(LoopSettings.MaximumSafeTicks.ToString()));
            UnityEngine.Object.DestroyImmediate(settings);
        }

        [Test]
        public void ActorReplayGenerationOrderIsDeterministic()
        {
            ActorSimulationOrder echoTwo = new ActorSimulationOrder(LoopActorKind.Echo, 2);
            ActorSimulationOrder player = new ActorSimulationOrder(LoopActorKind.Player, 3);
            ActorSimulationOrder echoOne = new ActorSimulationOrder(LoopActorKind.Echo, 1);
            ActorSimulationOrder[] values = { player, echoTwo, echoOne };
            Array.Sort(values);
            Assert.That(values, Is.EqualTo(new[] { echoOne, echoTwo, player }));
        }

        [Test]
        public void SameTickConflictPrefersOldestEcho()
        {
            using ConflictFixture fixture = new ConflictFixture();
            InteractionResolution[] results = fixture.Resolve(
                fixture.PlayerRequest, fixture.OldEchoRequest);
            Assert.That(results[0].Request.ActorOrder,
                Is.EqualTo(fixture.OldEchoRequest.ActorOrder));
            Assert.That(results[0].Execution.Succeeded, Is.True);
            Assert.That(results[1].Execution.FailureReason,
                Is.EqualTo(InteractionFailureReason.TargetBusy));
            Assert.That(fixture.Battery.Holder, Is.SameAs(fixture.OldEcho.Interactor));
        }

        [Test]
        public void CurrentPlayerNeverPreemptsPastEcho()
        {
            using ConflictFixture fixture = new ConflictFixture();
            InteractionResolution[] results = fixture.Resolve(
                fixture.PlayerRequest, fixture.NewEchoRequest, fixture.OldEchoRequest);
            Assert.That(results.Select(result => result.Request.ActorOrder),
                Is.EqualTo(new[]
                {
                    fixture.OldEchoRequest.ActorOrder,
                    fixture.NewEchoRequest.ActorOrder,
                    fixture.PlayerRequest.ActorOrder
                }));
            Assert.That(results[2].Execution.FailureReason,
                Is.EqualTo(InteractionFailureReason.TargetBusy));
        }

        [Test]
        public void StableIdIsDeterministicRequestTieBreak()
        {
            ActorSimulationOrder order = new ActorSimulationOrder(LoopActorKind.Echo, 1);
            InteractionRequest b = new InteractionRequest(order,
                new InteractionCommand(4, InteractionKind.PickupBattery, "b", Vector3.zero), null);
            InteractionRequest a = new InteractionRequest(order,
                new InteractionCommand(4, InteractionKind.PickupBattery, "a", Vector3.zero), null);
            Assert.That(InteractionConflictResolver.Compare(a, b), Is.LessThan(0));
            Assert.That(InteractionConflictResolver.Compare(b, a), Is.GreaterThan(0));
        }

        [Test]
        public void BusyConflictDoesNotSwitchTargetOrRetry()
        {
            using ConflictFixture fixture = new ConflictFixture();
            InteractionResolution[] results = fixture.Resolve(
                fixture.OldEchoRequest, fixture.PlayerRequest);
            Assert.That(results[1].Execution.Command.TargetStableId,
                Is.EqualTo(fixture.PlayerRequest.Command.TargetStableId));
            Assert.That(results[1].Execution.FailureReason,
                Is.EqualTo(InteractionFailureReason.TargetBusy));
            Assert.That(fixture.Player.Interactor.CarriedBattery, Is.Null);
        }

        [Test]
        public void LoopHistoryStoresImmutableSummaryValues()
        {
            LoopHistory history = new LoopHistory(2);
            history.Add(new LoopHistorySummary(1, 90, 1, 1, 0.01f, 1, 0,
                ReplayHistoryState.Active, LoopEndReason.Manual));
            LoopHistorySummary summary = history[0];
            Assert.That(summary.LoopNumber, Is.EqualTo(1));
            Assert.That(summary.RecordedTicks, Is.EqualTo(90));
            Assert.That(summary.InteractionEventCount, Is.EqualTo(1));
            Assert.That(summary.EndReason, Is.EqualTo(LoopEndReason.Manual));
            Assert.That(typeof(LoopHistorySummary).GetFields(BindingFlags.Instance |
                BindingFlags.Public).Length, Is.Zero);
        }

        [Test]
        public void LoopHistoryDropsOldestSummaryAtCapacity()
        {
            LoopHistory history = new LoopHistory(2);
            history.Add(Summary(1, 1));
            history.Add(Summary(2, 2));
            history.Add(Summary(3, 3));
            Assert.That(history.Count, Is.EqualTo(2));
            Assert.That(history[0].ReplayGeneration, Is.EqualTo(2));
            Assert.That(history[1].ReplayGeneration, Is.EqualTo(3));
        }

        [Test]
        public void EvictedHistoryContainsNoRuntimeReference()
        {
            LoopHistory history = new LoopHistory(2);
            history.Add(Summary(1, 4));
            Assert.That(history.MarkEvicted(4), Is.True);
            Assert.That(history[0].State, Is.EqualTo(ReplayHistoryState.Evicted));
            Type[] fieldTypes = typeof(LoopHistorySummary)
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Select(field => field.FieldType).ToArray();
            Assert.That(fieldTypes.Any(type => typeof(UnityEngine.Object).IsAssignableFrom(type)), Is.False);
            Assert.That(fieldTypes.Contains(typeof(ReplayRecording)), Is.False);
        }

        [Test]
        public void DoorAppliesCommittedOpenStateOnNextTick()
        {
            GameObject plateObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plateObject.GetComponent<Collider>().isTrigger = true;
            PressurePlate plate = plateObject.AddComponent<PressurePlate>();
            GameObject actorObject = new GameObject("Actor");
            LoopActor actor = actorObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Echo, 1);
            GameObject doorObject = new GameObject("Door");
            DoorController door = doorObject.AddComponent<DoorController>();
            door.Configure(plate, Vector3.up * 3f, 5f);
            door.CaptureInitialState();
            door.UseCoordinatedTicks();
            door.BeginSimulationTick();
            plate.RegisterActor(actor);
            door.CommitDeviceState();
            Assert.That(door.AppliedOpenRequest, Is.False);
            Assert.That(door.IsOpen, Is.False);
            door.BeginSimulationTick();
            Assert.That(door.AppliedOpenRequest, Is.True);
            Assert.That(door.IsOpen, Is.True);
            UnityEngine.Object.DestroyImmediate(doorObject);
            UnityEngine.Object.DestroyImmediate(actorObject);
            UnityEngine.Object.DestroyImmediate(plateObject);
        }

        private static LoopHistorySummary Summary(int loop, int generation)
        {
            return new LoopHistorySummary(loop, 10, 0, generation, 0f, 0, 0,
                ReplayHistoryState.Active, LoopEndReason.Test);
        }

        private sealed class ConflictFixture : IDisposable
        {
            private readonly InteractionRegistry _registry;
            public ConflictFixture()
            {
                _registry = new GameObject("Registry").AddComponent<InteractionRegistry>();
                OldEcho = CreateActor("Old Echo", LoopActorKind.Echo, 1);
                NewEcho = CreateActor("New Echo", LoopActorKind.Echo, 2);
                Player = CreateActor("Player", LoopActorKind.Player, 3);
                Battery = CreateBattery();
                InteractionCommand command = new InteractionCommand(
                    5, InteractionKind.PickupBattery, "shared-battery", Vector3.zero);
                OldEchoRequest = OldEcho.Interactor.CreateRecordedRequest(
                    command, OldEcho.Actor.SimulationOrder);
                NewEchoRequest = NewEcho.Interactor.CreateRecordedRequest(
                    command, NewEcho.Actor.SimulationOrder);
                PlayerRequest = Player.Interactor.CreateRecordedRequest(
                    command, Player.Actor.SimulationOrder);
            }

            public TestActor OldEcho { get; }
            public TestActor NewEcho { get; }
            public TestActor Player { get; }
            public CarryableBattery Battery { get; }
            public InteractionRequest OldEchoRequest { get; }
            public InteractionRequest NewEchoRequest { get; }
            public InteractionRequest PlayerRequest { get; }

            public InteractionResolution[] Resolve(params InteractionRequest[] requests)
            {
                Battery.RestoreInitialState();
                InteractionResolution[] results = new InteractionResolution[requests.Length];
                new InteractionConflictResolver().Resolve(requests, requests.Length, results);
                return results;
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(OldEcho.GameObject);
                UnityEngine.Object.DestroyImmediate(NewEcho.GameObject);
                UnityEngine.Object.DestroyImmediate(Player.GameObject);
                UnityEngine.Object.DestroyImmediate(Battery.gameObject);
                UnityEngine.Object.DestroyImmediate(_registry.gameObject);
            }

            private TestActor CreateActor(string name, LoopActorKind kind, int generation)
            {
                GameObject gameObject = new GameObject(name);
                LoopActor actor = gameObject.AddComponent<LoopActor>();
                actor.Configure(kind, generation);
                GameObject carry = new GameObject("Carry Socket");
                carry.transform.SetParent(gameObject.transform, false);
                InteractionSensor sensor = gameObject.AddComponent<InteractionSensor>();
                sensor.Configure(2f, ~0);
                Interactor interactor = gameObject.AddComponent<Interactor>();
                interactor.Configure(actor, sensor, carry.transform, _registry);
                return new TestActor(gameObject, actor, interactor);
            }

            private CarryableBattery CreateBattery()
            {
                GameObject batteryObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Collider collider = batteryObject.GetComponent<Collider>();
                collider.isTrigger = true;
                Rigidbody body = batteryObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                StableId id = batteryObject.AddComponent<StableId>();
                CarryableBattery battery = batteryObject.AddComponent<CarryableBattery>();
                battery.Configure(id, collider, body);
                id.Configure("shared-battery", _registry, battery);
                battery.CaptureInitialState();
                return battery;
            }
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
        }
    }
}
