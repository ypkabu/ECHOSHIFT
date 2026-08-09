using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using EchoShift.Core;
using EchoShift.Editor;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoShift.Tests.EditMode
{
    public sealed class Phase4CharacterPresentationEditModeTests
    {
        private const string RobotFbxPath =
            "Assets/_Project/ThirdParty/Quaternius/AnimatedRobot/Robot.fbx";
        private const string RobotFbxSha =
            "38AFB56DB7FB17A74D30F0AFC8ADB5F00441A94E65D6B8AC1958732480F79EB8";

        [OneTimeSetUp]
        public void OpenGeneratedScene()
        {
            EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath);
        }

        [Test]
        public void AnimatorExistsOnlyBelowActorVisualChild()
        {
            LoopActor[] actors = FindAll<LoopActor>();
            Assert.That(actors, Is.Not.Empty);
            for (int i = 0; i < actors.Length; i++)
            {
                Assert.That(actors[i].GetComponent<Animator>(), Is.Null, actors[i].name);
                Transform visual = actors[i].transform.Find("P4 Robot Visual");
                Assert.That(visual, Is.Not.Null, actors[i].name);
                Animator[] animators = visual.GetComponentsInChildren<Animator>(true);
                Assert.That(animators.Length, Is.EqualTo(1), actors[i].name);
                Assert.That(animators[0].transform.IsChildOf(visual), Is.True);
            }
        }

        [Test]
        public void RobotAnimatorsDisableRootMotion()
        {
            Animator[] animators = FindAll<Animator>();
            Assert.That(animators, Is.Not.Empty);
            for (int i = 0; i < animators.Length; i++)
                Assert.That(animators[i].applyRootMotion, Is.False, animators[i].name);
        }

        [Test]
        public void RobotClipsContainNoGameplayRootCurves()
        {
            AnimationClip[] clips = LoadController().animationClips;
            Assert.That(clips.Length, Is.EqualTo(6));
            for (int i = 0; i < clips.Length; i++)
            {
                EditorCurveBinding[] curves = AnimationUtility.GetCurveBindings(clips[i]);
                EditorCurveBinding[] objectCurves =
                    AnimationUtility.GetObjectReferenceCurveBindings(clips[i]);
                Assert.That(curves, Is.Empty, clips[i].name);
                Assert.That(objectCurves, Is.Empty, clips[i].name);
            }
        }

        [Test]
        public void RobotClipsContainNoAnimationEvents()
        {
            AnimationClip[] clips = LoadController().animationClips;
            for (int i = 0; i < clips.Length; i++)
                Assert.That(AnimationUtility.GetAnimationEvents(clips[i]), Is.Empty,
                    clips[i].name);
        }

        [Test]
        public void ControllerContainsAllSixRequiredStates()
        {
            AnimatorStateMachine machine = LoadController().layers[0].stateMachine;
            HashSet<string> names = new HashSet<string>();
            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++) names.Add(states[i].state.name);
            string[] required =
            {
                "Idle", "Walk", "Carry Idle", "Carry Walk", "Interact", "Echo Stopped"
            };
            Assert.That(names, Is.EquivalentTo(required));
        }

        [Test]
        public void CarryPoseReferencesAreComplete()
        {
            Phase4RobotPoseController[] poses = FindAll<Phase4RobotPoseController>();
            Assert.That(poses, Is.Not.Empty);
            for (int i = 0; i < poses.Length; i++)
            {
                Assert.That(poses[i].HasRequiredReferences, Is.True, poses[i].name);
                Assert.That(poses[i].HasCarryPoseVisuals, Is.True, poses[i].name);
                Assert.That(poses[i].IsVisualOnly, Is.True, poses[i].name);
            }
        }

        [Test]
        public void BatteryVisualDimensionsMatchRobotRatioBudget()
        {
            const float robotHeight = 2.0f;
            const float torsoWidth = 0.9f;
            Assert.That(Phase4BatteryVisual.VisualLengthMeters / robotHeight,
                Is.InRange(0.30f, 0.40f));
            Assert.That(Phase4BatteryVisual.VisualDiameterMeters / torsoWidth,
                Is.InRange(0.35f, 0.55f));
        }

        [Test]
        public void PlayerAndEchoCarrySocketsUseSameForwardOffset()
        {
            Interactor player = FindAll<Interactor>()[0];
            GameObject echoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Actors/P3_Echo.prefab");
            Interactor echo = echoPrefab.GetComponent<Interactor>();
            Assert.That(player.CarrySocket.localPosition.z, Is.GreaterThan(0.5f));
            Assert.That(player.CarrySocket.localPosition.y, Is.InRange(0.15f, 0.35f));
            Assert.That(echo.CarrySocket.localPosition,
                Is.EqualTo(player.CarrySocket.localPosition));
        }

        [Test]
        public void DoorsHaveSeparatedLeftAndRightVisualPanels()
        {
            DoorVisualFeedback[] doors = FindAll<DoorVisualFeedback>();
            Assert.That(doors, Is.Not.Empty);
            for (int i = 0; i < doors.Length; i++)
            {
                Assert.That(doors[i].HasPhase4References, Is.True, doors[i].name);
                Assert.That(doors[i].UsesSplitPanels, Is.True, doors[i].name);
                Assert.That(doors[i].VisualsAreSeparatedFromGameplayRoot, Is.True,
                    doors[i].name);
            }
        }

        [Test]
        public void DoorPanelTravelRetractsBeyondHalfOpeningAndHidesAtCompletion()
        {
            Assert.That(DoorVisualFeedback.PanelTravel, Is.GreaterThanOrEqualTo(1.4f));
            Assert.That(DoorVisualFeedback.PreparationDuration, Is.InRange(0.05f, 0.15f));
            Assert.That(DoorVisualFeedback.SlideDuration, Is.InRange(0.25f, 0.5f));
            string source = File.ReadAllText(Path.Combine(Application.dataPath,
                "_Project/Scripts/Runtime/Presentation/DoorVisualFeedback.cs"));
            Assert.That(source, Does.Contain("_openProgress >= 0.98f"));
            Assert.That(source, Does.Contain("SetActive(panelsVisible)"));
        }

        [Test]
        public void FloorCircuitsStayWithinFloorHeightBudget()
        {
            Phase4FloorCircuitVisual[] circuits = FindAll<Phase4FloorCircuitVisual>();
            Assert.That(circuits, Is.Not.Empty);
            for (int i = 0; i < circuits.Length; i++)
            {
                Assert.That(circuits[i].FloorHeight, Is.InRange(0.005f, 0.02f));
                Assert.That(circuits[i].CircuitWidth,
                    Is.EqualTo(Phase4FloorCircuitVisual.DefaultWidth).Within(0.001f));
                Assert.That(circuits[i].GetComponentsInChildren<Collider>(true), Is.Empty);
            }
        }

        [Test]
        public void FloorCircuitsUseOnlyAxisAlignedMeshSegments()
        {
            Phase4FloorCircuitVisual[] circuits = FindAll<Phase4FloorCircuitVisual>();
            for (int i = 0; i < circuits.Length; i++)
            {
                Assert.That(circuits[i].SegmentCount, Is.EqualTo(4));
                Renderer[] segments = circuits[i].GetComponentsInChildren<Renderer>(true);
                for (int segment = 0; segment < segments.Length; segment++)
                {
                    Vector3 scale = segments[segment].transform.localScale;
                    bool horizontal = scale.x > scale.z &&
                                      Mathf.Approximately(scale.z,
                                          Phase4FloorCircuitVisual.DefaultWidth);
                    bool vertical = scale.z > scale.x &&
                                    Mathf.Approximately(scale.x,
                                        Phase4FloorCircuitVisual.DefaultWidth);
                    Assert.That(horizontal || vertical, Is.True, segments[segment].name);
                    Assert.That(segments[segment].transform.localRotation,
                        Is.EqualTo(Quaternion.identity));
                }
                Assert.That(circuits[i].GetComponent<LineRenderer>(), Is.Null);
            }
        }

        [Test]
        public void BuilderRerunKeepsAnimationDoorAndCircuitCountsStable()
        {
            P3SceneBuilder.BuildScene();
            int firstStates = LoadController().layers[0].stateMachine.states.Length;
            EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath);
            int firstDoors = FindAll<DoorVisualFeedback>().Length;
            int firstCircuits = FindAll<Phase4FloorCircuitVisual>().Length;
            P3SceneBuilder.BuildScene();
            EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath);
            Assert.That(LoadController().layers[0].stateMachine.states.Length,
                Is.EqualTo(firstStates));
            Assert.That(FindAll<DoorVisualFeedback>().Length, Is.EqualTo(firstDoors));
            Assert.That(FindAll<Phase4FloorCircuitVisual>().Length, Is.EqualTo(firstCircuits));
        }

        [TestCase("P0_ReplayLab.unity", "1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5")]
        [TestCase("P1_InteractionLab.unity", "4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB")]
        [TestCase("P2_CoordinationLab.unity", "73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C")]
        public void ValidatedPrePhaseFourSceneHashesRemainUnchanged(
            string fileName, string expected)
        {
            string path = Path.Combine(Application.dataPath, "_Project", "Scenes", fileName);
            Assert.That(Sha256(path), Is.EqualTo(expected));
        }

        [Test]
        public void ThirdPartyRobotOriginalRemainsByteIdentical()
        {
            string absolute = Path.Combine(
                Path.GetFullPath(Path.Combine(Application.dataPath, "..")), RobotFbxPath);
            Assert.That(Sha256(absolute), Is.EqualTo(RobotFbxSha));
        }

        private static AnimatorController LoadController()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                Phase4AssetBuilder.RobotAnimatorControllerPath);
            Assert.That(controller, Is.Not.Null);
            return controller;
        }

        private static T[] FindAll<T>() where T : Object =>
            Object.FindObjectsByType<T>(FindObjectsInactive.Include);

        private static string Sha256(string path)
        {
            using FileStream stream = File.OpenRead(path);
            using SHA256 sha = SHA256.Create();
            return System.BitConverter.ToString(sha.ComputeHash(stream))
                .Replace("-", string.Empty);
        }
    }
}
