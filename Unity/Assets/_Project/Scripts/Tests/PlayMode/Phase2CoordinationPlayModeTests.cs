using System.Collections;
using EchoShift.Core;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoShift.Tests
{
    public sealed class Phase2CoordinationPlayModeTests
    {
        [UnityTest]
        public IEnumerator Phase2SceneSupportsThreeLoopSolutionWithTwoSimultaneousEchoes()
        {
            yield return LoadScene("P2_CoordinationLab");
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PlayerSimulation player = Object.FindAnyObjectByType<PlayerSimulation>();
            PressurePlate plate = FindNamed<PressurePlate>("Pressure Plate A");
            DoorController gateA = FindNamed<DoorController>("Gate A - Plate");
            DoorController gateB = FindNamed<DoorController>("Gate B - Battery");
            PowerSocket socket = Object.FindAnyObjectByType<PowerSocket>();
            GoalVolume goal = Object.FindAnyObjectByType<GoalVolume>();
            Phase2RouteInputSource input = player.gameObject.AddComponent<Phase2RouteInputSource>();
            input.Director = director;
            player.Configure(input, player.Motor, player.Interactor);

            AdvanceUntilLoop(director, 2, 400);
            Assert.That(director.EchoCount, Is.EqualTo(1));
            AdvanceUntilTick(director, 2, 120);
            Assert.That(plate.IsPressed, Is.True, "Echo 1 must hold the pressure plate.");
            Assert.That(gateA.IsOpen, Is.True, "Gate A must open for the current player.");
            Assert.That(player.transform.position.z, Is.GreaterThan(-3f));

            AdvanceUntilLoop(director, 3, 500);
            Assert.That(director.EchoCount, Is.EqualTo(2));
            Assert.That(director.GetEchoPlayback(0).PlaybackTick, Is.Zero);
            Assert.That(director.GetEchoPlayback(1).PlaybackTick, Is.Zero);
            AdvanceUntilTick(director, 3, 228);
            Assert.That(director.GetEchoPlayback(0).PlaybackTick, Is.GreaterThan(0));
            Assert.That(director.GetEchoPlayback(1).PlaybackTick, Is.GreaterThan(0));
            Assert.That(socket.IsPowered, Is.True, "Echo 2 must insert the battery.");
            Assert.That(gateB.IsOpen, Is.True, "Gate B applies power on the next tick.");

            AdvanceUntilGoal(director, goal, 420);
            EchoPlayback echoOne = director.GetEchoPlayback(0);
            EchoPlayback echoTwo = director.GetEchoPlayback(1);
            Assert.That(goal.IsReached, Is.True);
            Assert.That(player.transform.position.z, Is.GreaterThan(5f));
            Assert.That(echoOne.InteractionFailureCount, Is.Zero);
            Assert.That(echoTwo.InteractionSuccessCount, Is.EqualTo(2));
            Assert.That(echoTwo.InteractionFailureCount, Is.Zero);
            Assert.That(director.MaximumReplayDrift, Is.LessThanOrEqualTo(0.05f));
            Assert.That(director.AnyEchoExceededTolerance, Is.False);
            Debug.Log(
                $"PHASE2_INTEGRATION_MAX_DRIFT={director.MaximumReplayDrift:R};" +
                $"INTERACTION_SUCCESS={echoOne.InteractionSuccessCount + echoTwo.InteractionSuccessCount};" +
                $"INTERACTION_FAILURE={echoOne.InteractionFailureCount + echoTwo.InteractionFailureCount};" +
                $"GOAL={goal.IsReached};LOOP={director.LoopNumber};TICK={director.CurrentTick}");
        }

        [UnityTest]
        public IEnumerator Phase2ActorsUseCollisionMatrixWithoutMutualBlocking()
        {
            yield return LoadScene("P2_CoordinationLab");
            int player = LayerMask.NameToLayer("Player");
            int echo = LayerMask.NameToLayer("Echo");
            int environment = LayerMask.NameToLayer("Environment");
            int trigger = LayerMask.NameToLayer("InteractionTrigger");
            Assert.That(Physics.GetIgnoreLayerCollision(player, player), Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(player, echo), Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(echo, echo), Is.True);
            Assert.That(Physics.GetIgnoreLayerCollision(player, environment), Is.False);
            Assert.That(Physics.GetIgnoreLayerCollision(echo, environment), Is.False);
            Assert.That(Physics.GetIgnoreLayerCollision(player, trigger), Is.False);
            Assert.That(Physics.GetIgnoreLayerCollision(echo, trigger), Is.False);
            Assert.That(Object.FindAnyObjectByType<PlayerSimulation>()
                .GetComponent<Collider>().isTrigger, Is.False);
        }

        [UnityTest]
        public IEnumerator SameBatteryConflictKeepsOldEchoOwnershipAndRejectsPlayer()
        {
            yield return LoadScene("P2_CoordinationLab");
            InteractionRegistry registry = Object.FindAnyObjectByType<InteractionRegistry>();
            CarryableBattery battery = Object.FindAnyObjectByType<CarryableBattery>();
            TestActor oldEcho = CreateActor("Conflict Echo", LoopActorKind.Echo, 1, registry);
            TestActor player = CreateActor("Conflict Player", LoopActorKind.Player, 2, registry);
            InteractionCommand command = new InteractionCommand(
                3, InteractionKind.PickupBattery, battery.StableId.Value, battery.transform.position);
            InteractionRequest[] requests =
            {
                player.Interactor.CreateRecordedRequest(command, player.Actor.SimulationOrder),
                oldEcho.Interactor.CreateRecordedRequest(command, oldEcho.Actor.SimulationOrder)
            };
            InteractionResolution[] results = new InteractionResolution[2];
            new InteractionConflictResolver().Resolve(requests, 2, results);
            Assert.That(results[0].Execution.Succeeded, Is.True);
            Assert.That(results[1].Execution.FailureReason,
                Is.EqualTo(InteractionFailureReason.TargetBusy));
            Assert.That(battery.Holder, Is.SameAs(oldEcho.Interactor));
            Assert.That(player.Interactor.CarriedBattery, Is.Null);
            Object.Destroy(oldEcho.GameObject);
            Object.Destroy(player.GameObject);
        }

        [UnityTest]
        public IEnumerator FourthReplayEvictsOldestAndCleansPlateAndBatteryReferences()
        {
            yield return LoadScene("P2_CoordinationLab");
            LoopDirector director = Object.FindAnyObjectByType<LoopDirector>();
            PlayerSimulation player = Object.FindAnyObjectByType<PlayerSimulation>();
            EndImmediatelyInputSource input = player.gameObject.AddComponent<EndImmediatelyInputSource>();
            player.Configure(input, player.Motor, player.Interactor);
            AdvanceUntilLoop(director, 4, 20);
            Assert.That(director.EchoCount, Is.EqualTo(3));
            EchoPlayback oldest = director.GetEchoPlayback(0);
            int evictedGeneration = oldest.ReplayGeneration;
            PressurePlate plate = Object.FindAnyObjectByType<PressurePlate>();
            CarryableBattery battery = Object.FindAnyObjectByType<CarryableBattery>();
            oldest.transform.position = battery.transform.position;
            Physics.SyncTransforms();
            InteractionExecution pickup = oldest.Interactor.ExecuteRecorded(
                new InteractionCommand(0, InteractionKind.PickupBattery,
                    battery.StableId.Value, oldest.transform.position));
            Assert.That(pickup.Succeeded, Is.True);
            plate.RegisterActor(oldest.Actor);
            Assert.That(plate.IsPressed, Is.True);

            AdvanceUntilLoop(director, 5, 20);
            plate.RefreshFromPhysics();
            Assert.That(director.EchoCount, Is.EqualTo(3));
            Assert.That(director.GetEchoPlayback(0).ReplayGeneration,
                Is.GreaterThan(evictedGeneration));
            Assert.That(plate.IsPressed, Is.False);
            Assert.That(battery.Holder, Is.Null);
            bool foundEvicted = false;
            for (int i = 0; i < director.History.Count; i++)
            {
                if (director.History[i].ReplayGeneration == evictedGeneration)
                {
                    foundEvicted = director.History[i].State == ReplayHistoryState.Evicted;
                }
            }
            Assert.That(foundEvicted, Is.True);
        }

        [UnityTest]
        public IEnumerator Phase2SceneHasRequiredReferencesAndNoMissingComponents()
        {
            yield return LoadScene("P2_CoordinationLab");
            Assert.That(Object.FindAnyObjectByType<LoopDirector>().HasValidReferences, Is.True);
            Assert.That(Object.FindAnyObjectByType<PlayerSimulation>().HasValidReferences, Is.True);
            Assert.That(Object.FindAnyObjectByType<PressurePlate>(), Is.Not.Null);
            Assert.That(FindNamed<DoorController>("Gate A - Plate").HasValidReferences, Is.True);
            Assert.That(FindNamed<DoorController>("Gate B - Battery").HasValidReferences, Is.True);
            Assert.That(Object.FindAnyObjectByType<CarryableBattery>().StableId, Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<PowerSocket>().StableId, Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<GoalVolume>(), Is.Not.Null);
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    Assert.That(child.GetComponents<Component>(), Has.None.Null,
                        $"Missing component on {child.name}");
                }
            }
        }

        [UnityTest]
        public IEnumerator Phase0AndPhase1ScenesRemainLoadableWithValidDirectors()
        {
            yield return LoadScene("P0_ReplayLab");
            Assert.That(Object.FindAnyObjectByType<LoopDirector>().HasValidReferences, Is.True);
            yield return LoadScene("P1_InteractionLab");
            Assert.That(Object.FindAnyObjectByType<LoopDirector>().HasValidReferences, Is.True);
            Assert.That(Object.FindAnyObjectByType<InteractionRegistry>(), Is.Not.Null);
        }

        private static IEnumerator LoadScene(string sceneName)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (!operation.isDone)
            {
                yield return null;
            }
            yield return null;
        }

        private static void AdvanceUntilLoop(LoopDirector director, int loop, int limit)
        {
            int advances = 0;
            while (director.LoopNumber < loop && advances++ < limit)
            {
                director.AdvanceOneTickForTests();
            }
            Assert.That(director.LoopNumber, Is.EqualTo(loop));
        }

        private static void AdvanceUntilTick(
            LoopDirector director, int loop, int tick)
        {
            while (director.LoopNumber == loop && director.CurrentTick < tick)
            {
                director.AdvanceOneTickForTests();
            }
            Assert.That(director.LoopNumber, Is.EqualTo(loop));
            Assert.That(director.CurrentTick, Is.GreaterThanOrEqualTo(tick));
        }

        private static void AdvanceUntilGoal(
            LoopDirector director, GoalVolume goal, int limit)
        {
            int advances = 0;
            while (!goal.IsReached && advances++ < limit)
            {
                director.AdvanceOneTickForTests();
            }
            Assert.That(goal.IsReached, Is.True);
        }

        private static T FindNamed<T>(string name) where T : Component
        {
            foreach (T component in Object.FindObjectsByType<T>())
            {
                if (component.name == name)
                {
                    return component;
                }
            }
            return null;
        }

        private static TestActor CreateActor(
            string name, LoopActorKind kind, int generation, InteractionRegistry registry)
        {
            GameObject actorObject = new GameObject(name);
            LoopActor actor = actorObject.AddComponent<LoopActor>();
            actor.Configure(kind, generation);
            GameObject carry = new GameObject("Carry Socket");
            carry.transform.SetParent(actorObject.transform, false);
            InteractionSensor sensor = actorObject.AddComponent<InteractionSensor>();
            sensor.Configure(2f, ~0);
            Interactor interactor = actorObject.AddComponent<Interactor>();
            interactor.Configure(actor, sensor, carry.transform, registry);
            return new TestActor(actorObject, actor, interactor);
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

    public sealed class Phase2RouteInputSource : MonoBehaviour, IInputSource
    {
        public LoopDirector Director { get; set; }

        public InputCommand Sample(int tick)
        {
            int loop = Director != null ? Director.LoopNumber : 1;
            Vector2 move = Vector2.zero;
            InputButtonFlags buttons = InputButtonFlags.None;
            if (loop == 1)
            {
                if (tick < 45) move = Vector2.left;
                else if (tick < 98) move = Vector2.up;
                if (tick == 110) buttons = InputButtonFlags.EndLoop;
            }
            else if (loop == 2)
            {
                if (tick < 155) move = Vector2.up;
                else if (tick < 175) move = Vector2.right;
                else if (tick == 175) buttons = InputButtonFlags.Interact;
                else if (tick < 225) move = Vector2.up;
                else if (tick == 225) buttons = InputButtonFlags.Interact;
                if (tick == 240) buttons |= InputButtonFlags.EndLoop;
            }
            else
            {
                move = Vector2.up;
            }
            return new InputCommand(tick, move, buttons);
        }
    }

    public sealed class EndImmediatelyInputSource : MonoBehaviour, IInputSource
    {
        public InputCommand Sample(int tick)
        {
            return new InputCommand(tick, Vector2.zero, InputButtonFlags.EndLoop);
        }
    }
}
