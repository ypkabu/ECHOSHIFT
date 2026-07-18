using System;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Replay;
using EchoShift.Reset;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EchoShift.Editor
{
    public static class P2SceneBuilder
    {
        private const string Root = "Assets/_Project";
        public const string ScenePath = Root + "/Scenes/P2_CoordinationLab.unity";
        public const string SettingsPath = Root + "/Settings/LoopSettings_P2.asset";
        private const string EchoPrefabPath = Root + "/Prefabs/Actors/P2_Echo.prefab";
        private const string Phase1EchoPrefabPath = Root + "/Prefabs/Actors/P1_Echo.prefab";
        private const string Phase1ScenePath = Root + "/Scenes/P1_InteractionLab.unity";
        private const string Phase0ScenePath = Root + "/Scenes/P0_ReplayLab.unity";
        private const string MaterialRoot = Root + "/Art/Temp/Materials";
        private const string BatteryId = "p2-battery-23e6140f1a8d4c838a34";
        private const string SocketId = "p2-socket-b7df8e5b30f74620a8b2";

        private static readonly Vector3 StartPosition = new Vector3(0f, 1f, -10.5f);

        [MenuItem("ECHO SHIFT/Build Phase 2 Scene")]
        public static void BuildFromMenu()
        {
            BuildScene();
            Debug.Log($"Phase 2 scene generated at {ScenePath}.");
        }

        public static void BuildFromCommandLine()
        {
            BuildScene();
            Debug.Log($"Phase 2 scene generated at {ScenePath}.");
        }

        public static void BuildScene()
        {
            LoopSettings settings = CreateOrUpdateSettings();
            int playerLayer = LayerMask.NameToLayer("Player");
            int environmentLayer = LayerMask.NameToLayer("Environment");
            int echoLayer = LayerMask.NameToLayer("Echo");
            int triggerLayer = LayerMask.NameToLayer("InteractionTrigger");
            int targetLayer = LayerMask.NameToLayer("InteractionTarget");
            if (playerLayer < 0 || environmentLayer < 0 || echoLayer < 0 ||
                triggerLayer < 0 || targetLayer < 0)
            {
                throw new InvalidOperationException("Phase 2 required layers are missing.");
            }

            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                P0SceneBuilder.InputActionsPath);
            if (actions == null)
            {
                throw new InvalidOperationException("EchoShiftControls input actions are missing.");
            }

            EchoPlayback echoPrefab = CreateOrUpdateEchoPrefab();
            Material playerMaterial = LoadMaterial("Player.mat");
            Material floorMaterial = LoadMaterial("Floor.mat");
            Material wallMaterial = LoadMaterial("Wall.mat");
            Material batteryMaterial = LoadMaterial("Battery.mat");
            Material socketMaterial = LoadMaterial("PowerSocket.mat");
            Material doorMaterial = LoadMaterial("Door.mat");
            Material goalMaterial = LoadMaterial("Goal.mat");
            Material plateMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/P2_Plate.mat", new Color(0.12f, 0.7f, 1f, 1f));

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject environment = new GameObject("Environment");
            GameObject gameplay = new GameObject("Gameplay");
            GameObject systems = new GameObject("Systems");
            GameObject echoes = new GameObject("Echoes");

            CreateRoom(environment.transform, floorMaterial, wallMaterial, environmentLayer);
            InteractionRegistry interactionRegistry = systems.AddComponent<InteractionRegistry>();
            PlayerSimulation player = CreatePlayer(
                gameplay.transform, playerMaterial, settings, actions, interactionRegistry,
                playerLayer, environmentLayer, targetLayer, out TransformResettable playerReset);
            PressurePlate plate = CreatePlate(
                gameplay.transform, plateMaterial, triggerLayer);
            DoorController gateA = CreateDoor(
                "Gate A - Plate", new Vector3(0f, 1.5f, -3f), gameplay.transform,
                doorMaterial, plate, environmentLayer);
            CarryableBattery battery = CreateBattery(
                gameplay.transform, batteryMaterial, interactionRegistry, targetLayer);
            PowerSocket socket = CreateSocket(
                gameplay.transform, socketMaterial, interactionRegistry, targetLayer);
            DoorController gateB = CreateDoor(
                "Gate B - Battery", new Vector3(0f, 1.5f, 5f), gameplay.transform,
                doorMaterial, socket, environmentLayer);
            GoalVolume goal = CreateGoal(gameplay.transform, goalMaterial, triggerLayer);

            ResetRegistry resets = systems.AddComponent<ResetRegistry>();
            resets.Configure(new MonoBehaviour[] { plate, battery, socket, gateA, gateB, goal });
            LoopDirector director = systems.AddComponent<LoopDirector>();
            director.Configure(settings, player, playerReset, resets, echoPrefab,
                echoes.transform, interactionRegistry, new[] { plate },
                new[] { gateA, gateB }, goal);
            Phase0DebugOverlay overlay = systems.AddComponent<Phase0DebugOverlay>();
            overlay.ConfigurePhase2(director, player, plate, battery, socket, goal);
            Phase2StartupProbe startupProbe = systems.AddComponent<Phase2StartupProbe>();
            startupProbe.Configure(director, player);

            CreateCameraAndLight(systems.transform);
            StableIdValidationResult validation =
                StableIdSceneValidator.ValidateScene(scene, out string validationError);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validationError);
            }

            PlayerSettings.companyName = "Echo Shift Prototype";
            PlayerSettings.productName = "ECHO SHIFT";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(Phase1ScenePath, true),
                new EditorBuildSettingsScene(Phase0ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static LoopSettings CreateOrUpdateSettings()
        {
            LoopSettings asset = AssetDatabase.LoadAssetAtPath<LoopSettings>(SettingsPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LoopSettings>();
                AssetDatabase.CreateAsset(asset, SettingsPath);
            }

            SerializedObject serialized = new SerializedObject(asset);
            serialized.FindProperty("tickRate").intValue = 60;
            serialized.FindProperty("loopDurationSeconds").intValue = 15;
            serialized.FindProperty("maxEchoes").intValue = 3;
            serialized.FindProperty("moveSpeed").floatValue = 4f;
            serialized.FindProperty("driftTolerance").floatValue = 0.05f;
            serialized.FindProperty("maxCatchUpTicksPerFrame").intValue = 32;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            if (!asset.TryValidate(out string error) || asset.MaxTicks != 900)
            {
                throw new InvalidOperationException(error);
            }

            return asset;
        }

        private static EchoPlayback CreateOrUpdateEchoPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<EchoPlayback>(EchoPrefabPath) == null)
            {
                if (!AssetDatabase.CopyAsset(Phase1EchoPrefabPath, EchoPrefabPath))
                {
                    throw new InvalidOperationException("Could not create Phase 2 Echo prefab.");
                }
            }

            return AssetDatabase.LoadAssetAtPath<EchoPlayback>(EchoPrefabPath);
        }

        private static PlayerSimulation CreatePlayer(
            Transform parent, Material material, LoopSettings settings,
            InputActionAsset actions, InteractionRegistry registry,
            int playerLayer, int environmentLayer, int targetLayer,
            out TransformResettable resettable)
        {
            GameObject actorObject = Primitive("Player", PrimitiveType.Capsule,
                StartPosition, Vector3.one, material, parent, playerLayer);
            CapsuleCollider collider = actorObject.GetComponent<CapsuleCollider>();
            collider.radius = 0.45f;
            collider.height = 2f;
            Rigidbody body = actorObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            CharacterMotor motor = actorObject.AddComponent<CharacterMotor>();
            motor.Configure(settings.MoveSpeed, 0.45f, 2f, 1 << environmentLayer);
            LoopActor actor = actorObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Player, 1);
            GameObject socketObject = new GameObject("Carry Socket");
            socketObject.transform.SetParent(actorObject.transform, false);
            socketObject.transform.localPosition = new Vector3(0f, 0.35f, 0.8f);
            InteractionSensor sensor = actorObject.AddComponent<InteractionSensor>();
            sensor.Configure(1.5f, 1 << targetLayer);
            Interactor interactor = actorObject.AddComponent<Interactor>();
            interactor.Configure(actor, sensor, socketObject.transform, registry);
            InputSystemInputSource input = actorObject.AddComponent<InputSystemInputSource>();
            input.Configure(actions);
            PlayerSimulation player = actorObject.AddComponent<PlayerSimulation>();
            player.Configure(input, motor, interactor);
            resettable = actorObject.AddComponent<TransformResettable>();
            return player;
        }

        private static PressurePlate CreatePlate(Transform parent, Material material, int layer)
        {
            GameObject plateObject = Primitive("Pressure Plate A", PrimitiveType.Cube,
                new Vector3(-3f, 0.1f, -7f), new Vector3(2.4f, 0.2f, 2.4f),
                material, parent, layer);
            plateObject.GetComponent<Collider>().isTrigger = true;
            return plateObject.AddComponent<PressurePlate>();
        }

        private static CarryableBattery CreateBattery(
            Transform parent, Material material, InteractionRegistry registry, int layer)
        {
            GameObject batteryObject = Primitive("Battery", PrimitiveType.Cube,
                new Vector3(1.5f, 0.45f, 0f), new Vector3(0.7f, 0.5f, 1f),
                material, parent, layer);
            Collider collider = batteryObject.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody body = batteryObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            StableId identity = batteryObject.AddComponent<StableId>();
            CarryableBattery battery = batteryObject.AddComponent<CarryableBattery>();
            battery.Configure(identity, collider, body);
            identity.Configure(BatteryId, registry, battery);
            return battery;
        }

        private static PowerSocket CreateSocket(
            Transform parent, Material material, InteractionRegistry registry, int layer)
        {
            GameObject socketObject = Primitive("Power Socket B", PrimitiveType.Cube,
                new Vector3(1.5f, 0.4f, 3.2f), new Vector3(1.1f, 0.8f, 1.1f),
                material, parent, layer);
            socketObject.GetComponent<Collider>().isTrigger = true;
            GameObject insertion = new GameObject("Insertion Point");
            insertion.transform.SetParent(socketObject.transform, false);
            insertion.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            StableId identity = socketObject.AddComponent<StableId>();
            PowerSocket socket = socketObject.AddComponent<PowerSocket>();
            socket.Configure(identity, insertion.transform);
            identity.Configure(SocketId, registry, socket);
            return socket;
        }

        private static DoorController CreateDoor(
            string name, Vector3 position, Transform parent, Material material,
            MonoBehaviour source, int layer)
        {
            GameObject doorObject = Primitive(name, PrimitiveType.Cube, position,
                new Vector3(4f, 3f, 0.45f), material, parent, layer);
            DoorController door = doorObject.AddComponent<DoorController>();
            door.Configure(new[] { source }, new Vector3(0f, 3.5f, 0f), 5f);
            return door;
        }

        private static GoalVolume CreateGoal(Transform parent, Material material, int layer)
        {
            GameObject goalObject = Primitive("Goal Volume", PrimitiveType.Cube,
                new Vector3(0f, 0.1f, 10.2f), new Vector3(3f, 0.2f, 2.4f),
                material, parent, layer);
            goalObject.GetComponent<Collider>().isTrigger = true;
            return goalObject.AddComponent<GoalVolume>();
        }

        private static void CreateRoom(
            Transform parent, Material floor, Material wall, int layer)
        {
            Primitive("Floor", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f),
                new Vector3(16f, 0.5f, 26f), floor, parent, layer);
            Primitive("Wall West", PrimitiveType.Cube, new Vector3(-8.25f, 1.5f, 0f),
                new Vector3(0.5f, 3f, 26.5f), wall, parent, layer);
            Primitive("Wall East", PrimitiveType.Cube, new Vector3(8.25f, 1.5f, 0f),
                new Vector3(0.5f, 3f, 26.5f), wall, parent, layer);
            Primitive("Wall South", PrimitiveType.Cube, new Vector3(0f, 1.5f, -13.25f),
                new Vector3(16.5f, 3f, 0.5f), wall, parent, layer);
            Primitive("Wall North", PrimitiveType.Cube, new Vector3(0f, 1.5f, 13.25f),
                new Vector3(16.5f, 3f, 0.5f), wall, parent, layer);
            CreateDivider(parent, wall, layer, -3f);
            CreateDivider(parent, wall, layer, 5f);
        }

        private static void CreateDivider(Transform parent, Material wall, int layer, float z)
        {
            Primitive($"Divider West {z}", PrimitiveType.Cube, new Vector3(-6f, 1.5f, z),
                new Vector3(8f, 3f, 0.5f), wall, parent, layer);
            Primitive($"Divider East {z}", PrimitiveType.Cube, new Vector3(6f, 1.5f, z),
                new Vector3(8f, 3f, 0.5f), wall, parent, layer);
        }

        private static void CreateCameraAndLight(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(0f, 24f, -20f);
            cameraObject.transform.LookAt(new Vector3(0f, 0f, 1f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.065f, 1f);
            camera.fieldOfView = 52f;
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
        }

        private static Material LoadMaterial(string fileName)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialRoot + "/" + fileName);
            if (material == null)
            {
                throw new InvalidOperationException($"Material {fileName} is missing.");
            }

            return material;
        }

        private static Material CreateOrUpdateMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject Primitive(
            string name, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Transform parent, int layer)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.layer = layer;
            result.transform.SetParent(parent);
            result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }
    }
}
