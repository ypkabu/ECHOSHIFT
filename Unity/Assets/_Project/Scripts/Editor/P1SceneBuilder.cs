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
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class P1SceneBuilder
    {
        private const string ProjectRoot = "Assets/_Project";
        public const string ScenePath = ProjectRoot + "/Scenes/P1_InteractionLab.unity";
        private const string Phase0ScenePath = ProjectRoot + "/Scenes/P0_ReplayLab.unity";
        private const string SettingsPath = ProjectRoot + "/Settings/LoopSettings.asset";
        private const string EchoPrefabPath = ProjectRoot + "/Prefabs/Actors/P1_Echo.prefab";
        private const string MaterialRoot = ProjectRoot + "/Art/Temp/Materials";

        private const string BatteryStableId = "0d78d08f9425481bb102ce75d02439aa";
        private const string PowerSocketStableId = "a6b80e4fbf9b4899b60a4d5e1f71c099";

        private static readonly Vector3 ActorStartPosition = new Vector3(0f, 1f, -4.5f);
        private static readonly Quaternion ActorStartRotation = Quaternion.identity;

        [MenuItem("ECHO SHIFT/Build Phase 1 Scene")]
        public static void BuildFromMenu()
        {
            BuildScene();
            Debug.Log($"Phase 1 scene generated at {ScenePath}.");
        }

        public static void BuildFromCommandLine()
        {
            BuildScene();
            Debug.Log($"Phase 1 scene generated at {ScenePath}.");
        }

        public static void BuildScene()
        {
            EnsureFolders();
            int playerLayer = EnsureLayer("Player", 6);
            int environmentLayer = EnsureLayer("Environment", 7);
            int echoLayer = EnsureLayer("Echo", 8);
            int interactionTriggerLayer = EnsureLayer("InteractionTrigger", 9);
            int interactionTargetLayer = EnsureLayer("InteractionTarget", 10);
            ConfigureLayerCollisionMatrix(
                playerLayer,
                echoLayer,
                environmentLayer,
                interactionTriggerLayer,
                interactionTargetLayer);
            ConfigureInputSystemBackend();

            LoopSettings settings = AssetDatabase.LoadAssetAtPath<LoopSettings>(SettingsPath);
            if (settings == null)
            {
                throw new InvalidOperationException("LoopSettings asset is missing.");
            }

            if (!settings.TryValidate(out string settingsError))
            {
                throw new InvalidOperationException(settingsError);
            }

            InputActionAsset inputActions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(P0SceneBuilder.InputActionsPath);
            ValidateInputActions(inputActions);

            Material playerMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Player.mat",
                new Color(1f, 0.72f, 0.15f, 1f));
            Material echoMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Echo.mat",
                new Color(0.2f, 0.85f, 1f, 0.55f),
                true);
            Material floorMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Floor.mat",
                new Color(0.12f, 0.15f, 0.2f, 1f));
            Material wallMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Wall.mat",
                new Color(0.32f, 0.36f, 0.44f, 1f));
            Material batteryMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Battery.mat",
                new Color(1f, 0.45f, 0.05f, 1f));
            Material socketMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/PowerSocket.mat",
                new Color(0.55f, 0.15f, 0.85f, 1f));
            Material doorMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Door.mat",
                new Color(0.9f, 0.3f, 0.12f, 1f));
            Material goalMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/Goal.mat",
                new Color(0.15f, 0.9f, 0.35f, 0.55f),
                true);

            EchoPlayback echoPrefab = CreateOrUpdateEchoPrefab(
                settings,
                echoMaterial,
                echoLayer,
                environmentLayer,
                interactionTargetLayer);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject environmentRoot = new GameObject("Environment");
            GameObject gameplayRoot = new GameObject("Gameplay");
            GameObject systemsRoot = new GameObject("Systems");
            GameObject echoContainer = new GameObject("Echoes");

            CreateRoom(environmentRoot.transform, floorMaterial, wallMaterial, environmentLayer);

            InteractionRegistry interactionRegistry =
                systemsRoot.AddComponent<InteractionRegistry>();
            PlayerSimulation player = CreatePlayer(
                gameplayRoot.transform,
                playerMaterial,
                settings,
                inputActions,
                interactionRegistry,
                playerLayer,
                environmentLayer,
                interactionTargetLayer,
                out TransformResettable playerReset);
            CarryableBattery battery = CreateBattery(
                gameplayRoot.transform,
                batteryMaterial,
                interactionRegistry,
                interactionTargetLayer);
            PowerSocket powerSocket = CreatePowerSocket(
                gameplayRoot.transform,
                socketMaterial,
                interactionRegistry,
                interactionTargetLayer);
            DoorController door = CreateDoor(
                gameplayRoot.transform,
                doorMaterial,
                powerSocket,
                environmentLayer);
            GoalVolume goal = CreateGoal(
                gameplayRoot.transform,
                goalMaterial,
                interactionTriggerLayer);

            ResetRegistry resetRegistry = systemsRoot.AddComponent<ResetRegistry>();
            resetRegistry.Configure(new MonoBehaviour[]
            {
                battery,
                powerSocket,
                door,
                goal
            });

            LoopDirector director = systemsRoot.AddComponent<LoopDirector>();
            director.Configure(
                settings,
                player,
                playerReset,
                resetRegistry,
                echoPrefab,
                echoContainer.transform,
                interactionRegistry);

            Phase0DebugOverlay overlay = systemsRoot.AddComponent<Phase0DebugOverlay>();
            overlay.ConfigurePhase1(director, player, battery, powerSocket, goal);

            CreateCamera(systemsRoot.transform);
            CreateLight(systemsRoot.transform);

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
                throw new InvalidOperationException($"Could not save scene at {ScenePath}.");
            }

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(Phase0ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static EchoPlayback CreateOrUpdateEchoPrefab(
            LoopSettings settings,
            Material material,
            int echoLayer,
            int environmentLayer,
            int interactionTargetLayer)
        {
            GameObject echoObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            echoObject.name = "Echo";
            echoObject.layer = echoLayer;
            echoObject.transform.SetPositionAndRotation(ActorStartPosition, ActorStartRotation);
            echoObject.GetComponent<Renderer>().sharedMaterial = material;

            CapsuleCollider collider = echoObject.GetComponent<CapsuleCollider>();
            collider.radius = 0.45f;
            collider.height = 2f;
            collider.isTrigger = false;
            Rigidbody body = echoObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            CharacterMotor motor = echoObject.AddComponent<CharacterMotor>();
            motor.Configure(settings.MoveSpeed, 0.45f, 2f, 1 << environmentLayer);
            LoopActor actor = echoObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Echo);
            Transform carrySocket = CreateCarrySocket(echoObject.transform);
            InteractionSensor sensor = echoObject.AddComponent<InteractionSensor>();
            sensor.Configure(1.5f, 1 << interactionTargetLayer);
            Interactor interactor = echoObject.AddComponent<Interactor>();
            interactor.Configure(actor, sensor, carrySocket, null);
            echoObject.AddComponent<EchoPlayback>();

            PrefabUtility.SaveAsPrefabAsset(echoObject, EchoPrefabPath);
            Object.DestroyImmediate(echoObject);
            EchoPlayback prefab = AssetDatabase.LoadAssetAtPath<EchoPlayback>(EchoPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("Phase 1 Echo prefab creation failed.");
            }

            return prefab;
        }

        private static PlayerSimulation CreatePlayer(
            Transform parent,
            Material material,
            LoopSettings settings,
            InputActionAsset inputActions,
            InteractionRegistry registry,
            int playerLayer,
            int environmentLayer,
            int interactionTargetLayer,
            out TransformResettable resettable)
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Player";
            playerObject.layer = playerLayer;
            playerObject.transform.SetParent(parent);
            playerObject.transform.SetPositionAndRotation(ActorStartPosition, ActorStartRotation);
            playerObject.GetComponent<Renderer>().sharedMaterial = material;

            CapsuleCollider collider = playerObject.GetComponent<CapsuleCollider>();
            collider.radius = 0.45f;
            collider.height = 2f;
            collider.isTrigger = false;
            Rigidbody body = playerObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            CharacterMotor motor = playerObject.AddComponent<CharacterMotor>();
            motor.Configure(settings.MoveSpeed, 0.45f, 2f, 1 << environmentLayer);
            LoopActor actor = playerObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Player);
            Transform carrySocket = CreateCarrySocket(playerObject.transform);
            InteractionSensor sensor = playerObject.AddComponent<InteractionSensor>();
            sensor.Configure(1.5f, 1 << interactionTargetLayer);
            Interactor interactor = playerObject.AddComponent<Interactor>();
            interactor.Configure(actor, sensor, carrySocket, registry);
            InputSystemInputSource input = playerObject.AddComponent<InputSystemInputSource>();
            input.Configure(inputActions);
            PlayerSimulation simulation = playerObject.AddComponent<PlayerSimulation>();
            simulation.Configure(input, motor, interactor);
            resettable = playerObject.AddComponent<TransformResettable>();
            return simulation;
        }

        private static CarryableBattery CreateBattery(
            Transform parent,
            Material material,
            InteractionRegistry registry,
            int interactionTargetLayer)
        {
            GameObject batteryObject = CreatePrimitive(
                "Battery",
                PrimitiveType.Cube,
                new Vector3(-2f, 0.45f, -2.5f),
                new Vector3(0.7f, 0.5f, 1f),
                material,
                parent,
                interactionTargetLayer);
            Collider collider = batteryObject.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody body = batteryObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            StableId identity = batteryObject.AddComponent<StableId>();
            CarryableBattery battery = batteryObject.AddComponent<CarryableBattery>();
            battery.Configure(identity, collider, body);
            identity.Configure(BatteryStableId, registry, battery);
            return battery;
        }

        private static PowerSocket CreatePowerSocket(
            Transform parent,
            Material material,
            InteractionRegistry registry,
            int interactionTargetLayer)
        {
            GameObject socketObject = CreatePrimitive(
                "Power Socket",
                PrimitiveType.Cube,
                new Vector3(-2f, 0.4f, -0.5f),
                new Vector3(1.1f, 0.8f, 1.1f),
                material,
                parent,
                interactionTargetLayer);
            socketObject.GetComponent<Collider>().isTrigger = true;
            GameObject insertionObject = new GameObject("Insertion Point");
            insertionObject.transform.SetParent(socketObject.transform, false);
            insertionObject.transform.localPosition = new Vector3(0f, 0.75f, 0f);

            StableId identity = socketObject.AddComponent<StableId>();
            PowerSocket socket = socketObject.AddComponent<PowerSocket>();
            socket.Configure(identity, insertionObject.transform);
            identity.Configure(PowerSocketStableId, registry, socket);
            return socket;
        }

        private static DoorController CreateDoor(
            Transform parent,
            Material material,
            PowerSocket powerSocket,
            int environmentLayer)
        {
            GameObject doorObject = CreatePrimitive(
                "Powered Door",
                PrimitiveType.Cube,
                new Vector3(0f, 1.5f, 1f),
                new Vector3(2f, 3f, 0.45f),
                material,
                parent,
                environmentLayer);
            DoorController door = doorObject.AddComponent<DoorController>();
            door.Configure(
                new MonoBehaviour[] { powerSocket },
                new Vector3(0f, 3.5f, 0f),
                5f);
            return door;
        }

        private static GoalVolume CreateGoal(
            Transform parent,
            Material material,
            int interactionTriggerLayer)
        {
            GameObject goalObject = CreatePrimitive(
                "Goal Volume",
                PrimitiveType.Cube,
                new Vector3(0f, 0.1f, 4.25f),
                new Vector3(2.5f, 0.2f, 2f),
                material,
                parent,
                interactionTriggerLayer);
            goalObject.GetComponent<Collider>().isTrigger = true;
            return goalObject.AddComponent<GoalVolume>();
        }

        private static void CreateRoom(
            Transform parent,
            Material floorMaterial,
            Material wallMaterial,
            int environmentLayer)
        {
            CreatePrimitive("Floor", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f), new Vector3(12f, 0.5f, 12f), floorMaterial, parent, environmentLayer);
            CreatePrimitive("Wall West", PrimitiveType.Cube, new Vector3(-6.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 12.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Wall East", PrimitiveType.Cube, new Vector3(6.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 12.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Wall South", PrimitiveType.Cube, new Vector3(0f, 1.5f, -6.25f), new Vector3(12.5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Wall North", PrimitiveType.Cube, new Vector3(0f, 1.5f, 6.25f), new Vector3(12.5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Divider West", PrimitiveType.Cube, new Vector3(-3.5f, 1.5f, 1f), new Vector3(5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Divider East", PrimitiveType.Cube, new Vector3(3.5f, 1.5f, 1f), new Vector3(5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
        }

        private static Transform CreateCarrySocket(Transform actor)
        {
            GameObject carrySocket = new GameObject("Carry Socket");
            carrySocket.transform.SetParent(actor, false);
            carrySocket.transform.localPosition = new Vector3(0f, 0.35f, 0.8f);
            return carrySocket.transform;
        }

        private static void CreateCamera(Transform parent)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.transform.SetParent(parent);
            cameraObject.transform.position = new Vector3(0f, 14f, -10f);
            cameraObject.transform.LookAt(new Vector3(0f, 0f, 0.5f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.07f, 1f);
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
        }

        private static void CreateLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(parent);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
        }

        private static Material CreateOrUpdateMaterial(
            string path,
            Color color,
            bool transparent = false)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("The URP Lit shader is unavailable.");
            }

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.SetColor("_BaseColor", color);
            if (transparent)
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                material.SetFloat("_Surface", 0f);
                material.SetFloat("_ZWrite", 1f);
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.renderQueue = -1;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject CreatePrimitive(
            string name,
            PrimitiveType primitiveType,
            Vector3 position,
            Vector3 scale,
            Material material,
            Transform parent,
            int layer)
        {
            GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
            gameObject.name = name;
            gameObject.layer = layer;
            gameObject.transform.SetParent(parent);
            gameObject.transform.position = position;
            gameObject.transform.localScale = scale;
            gameObject.GetComponent<Renderer>().sharedMaterial = material;
            return gameObject;
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "_Project");
            EnsureFolder(ProjectRoot, "Art");
            EnsureFolder(ProjectRoot + "/Art", "Temp");
            EnsureFolder(ProjectRoot + "/Art/Temp", "Materials");
            EnsureFolder(ProjectRoot, "Prefabs");
            EnsureFolder(ProjectRoot + "/Prefabs", "Actors");
            EnsureFolder(ProjectRoot, "Scenes");
            EnsureFolder(ProjectRoot, "Settings");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static int EnsureLayer(string layerName, int preferredIndex)
        {
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 0; i < layers.arraySize; i++)
            {
                if (layers.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    return i;
                }
            }

            int targetIndex = preferredIndex;
            if (!string.IsNullOrEmpty(layers.GetArrayElementAtIndex(targetIndex).stringValue))
            {
                targetIndex = -1;
                for (int i = 8; i < layers.arraySize; i++)
                {
                    if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
                    {
                        targetIndex = i;
                        break;
                    }
                }
            }

            if (targetIndex < 0)
            {
                throw new InvalidOperationException($"No free layer is available for {layerName}.");
            }

            layers.GetArrayElementAtIndex(targetIndex).stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            return targetIndex;
        }

        private static void ConfigureLayerCollisionMatrix(
            int playerLayer,
            int echoLayer,
            int environmentLayer,
            int interactionTriggerLayer,
            int interactionTargetLayer)
        {
            Physics.IgnoreLayerCollision(playerLayer, playerLayer, true);
            Physics.IgnoreLayerCollision(playerLayer, echoLayer, true);
            Physics.IgnoreLayerCollision(echoLayer, echoLayer, true);
            Physics.IgnoreLayerCollision(playerLayer, environmentLayer, false);
            Physics.IgnoreLayerCollision(echoLayer, environmentLayer, false);
            Physics.IgnoreLayerCollision(playerLayer, interactionTriggerLayer, false);
            Physics.IgnoreLayerCollision(echoLayer, interactionTriggerLayer, false);
            Physics.IgnoreLayerCollision(playerLayer, interactionTargetLayer, false);
            Physics.IgnoreLayerCollision(echoLayer, interactionTargetLayer, false);
        }

        private static void ConfigureInputSystemBackend()
        {
            SerializedObject projectSettings = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            SerializedProperty activeInputHandler =
                projectSettings.FindProperty("activeInputHandler");
            if (activeInputHandler == null)
            {
                throw new InvalidOperationException(
                    "ProjectSettings.activeInputHandler could not be resolved.");
            }

            activeInputHandler.intValue = 1;
            projectSettings.ApplyModifiedProperties();
        }

        private static void ValidateInputActions(InputActionAsset inputActions)
        {
            InputActionMap gameplay = inputActions?.FindActionMap("Gameplay", false);
            if (gameplay == null ||
                gameplay.FindAction("Move", false) == null ||
                gameplay.FindAction("Interact", false) == null ||
                gameplay.FindAction("EndLoop", false) == null)
            {
                throw new InvalidOperationException(
                    "EchoShiftControls must contain Gameplay/Move, Interact, and EndLoop actions.");
            }
        }
    }
}
