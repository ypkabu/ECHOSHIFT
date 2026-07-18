using System;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Player;
using EchoShift.Replay;
using EchoShift.Reset;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class P0SceneBuilder
    {
        private const string ProjectRoot = "Assets/_Project";
        public const string ScenePath = ProjectRoot + "/Scenes/P0_ReplayLab.unity";
        private const string SettingsPath = ProjectRoot + "/Settings/LoopSettings.asset";
        private const string EchoPrefabPath = ProjectRoot + "/Prefabs/Actors/P0_Echo.prefab";
        private const string MaterialRoot = ProjectRoot + "/Art/Temp/Materials";

        private static readonly Vector3 ActorStartPosition = new Vector3(0f, 1f, -4.5f);
        private static readonly Quaternion ActorStartRotation = Quaternion.identity;

        [MenuItem("ECHO SHIFT/Build Phase 0 Scene")]
        public static void BuildFromMenu()
        {
            BuildScene();
            Debug.Log($"Phase 0 scene generated at {ScenePath}.");
        }

        public static void BuildFromCommandLine()
        {
            BuildScene();
            Debug.Log($"Phase 0 scene generated at {ScenePath}.");
        }

        public static void BuildScene()
        {
            EnsureFolders();
            int playerLayer = EnsureLayer("Player", 6, "LoopActor");
            int environmentLayer = EnsureLayer("Environment", 7);
            int echoLayer = EnsureLayer("Echo", 8);
            int interactionTriggerLayer = EnsureLayer("InteractionTrigger", 9);
            ConfigureLayerCollisionMatrix(
                playerLayer,
                echoLayer,
                environmentLayer,
                interactionTriggerLayer);

            LoopSettings settings = CreateOrUpdateLoopSettings();
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
            Material plateMaterial = CreateOrUpdateMaterial(
                MaterialRoot + "/PressurePlate.mat",
                new Color(0.1f, 0.45f, 1f, 1f));
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
                environmentLayer);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject environmentRoot = new GameObject("Environment");
            GameObject gameplayRoot = new GameObject("Gameplay");
            GameObject systemsRoot = new GameObject("Systems");
            GameObject echoContainer = new GameObject("Echoes");

            CreateRoom(environmentRoot.transform, floorMaterial, wallMaterial, environmentLayer);

            PlayerSimulation player = CreatePlayer(
                gameplayRoot.transform,
                playerMaterial,
                settings,
                playerLayer,
                environmentLayer,
                out TransformResettable playerReset);

            PressurePlate pressurePlate = CreatePressurePlate(
                gameplayRoot.transform,
                plateMaterial,
                interactionTriggerLayer);
            DoorController door = CreateDoor(
                gameplayRoot.transform,
                doorMaterial,
                pressurePlate,
                environmentLayer);
            GoalVolume goal = CreateGoal(
                gameplayRoot.transform,
                goalMaterial,
                interactionTriggerLayer);

            ResetRegistry resetRegistry = systemsRoot.AddComponent<ResetRegistry>();
            resetRegistry.Configure(new MonoBehaviour[]
            {
                pressurePlate,
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
                echoContainer.transform);

            Phase0DebugOverlay overlay = systemsRoot.AddComponent<Phase0DebugOverlay>();
            overlay.Configure(director, pressurePlate, goal);

            CreateCamera(systemsRoot.transform);
            CreateLight(systemsRoot.transform);

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
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        private static int EnsureLayer(
            string layerName,
            int preferredIndex,
            string legacyName = null)
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

            if (!string.IsNullOrEmpty(legacyName))
            {
                for (int i = 0; i < layers.arraySize; i++)
                {
                    SerializedProperty layer = layers.GetArrayElementAtIndex(i);
                    if (layer.stringValue == legacyName)
                    {
                        layer.stringValue = layerName;
                        tagManager.ApplyModifiedProperties();
                        return i;
                    }
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
            int interactionTriggerLayer)
        {
            Physics.IgnoreLayerCollision(playerLayer, playerLayer, true);
            Physics.IgnoreLayerCollision(playerLayer, echoLayer, true);
            Physics.IgnoreLayerCollision(echoLayer, echoLayer, true);

            Physics.IgnoreLayerCollision(playerLayer, environmentLayer, false);
            Physics.IgnoreLayerCollision(echoLayer, environmentLayer, false);
            Physics.IgnoreLayerCollision(playerLayer, interactionTriggerLayer, false);
            Physics.IgnoreLayerCollision(echoLayer, interactionTriggerLayer, false);
        }

        private static LoopSettings CreateOrUpdateLoopSettings()
        {
            LoopSettings settings = AssetDatabase.LoadAssetAtPath<LoopSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LoopSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.FindProperty("tickRate").intValue = 60;
            serializedSettings.FindProperty("loopDurationSeconds").intValue = 10;
            serializedSettings.FindProperty("maxEchoes").intValue = 3;
            serializedSettings.FindProperty("moveSpeed").floatValue = 4f;
            serializedSettings.FindProperty("driftTolerance").floatValue = 0.05f;
            serializedSettings.FindProperty("maxCatchUpTicksPerFrame").intValue = 8;
            serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
            return settings;
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

        private static EchoPlayback CreateOrUpdateEchoPrefab(
            LoopSettings settings,
            Material material,
            int actorLayer,
            int environmentLayer)
        {
            GameObject actorObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            actorObject.name = "Echo";
            actorObject.layer = actorLayer;
            actorObject.transform.SetPositionAndRotation(ActorStartPosition, ActorStartRotation);
            actorObject.GetComponent<Renderer>().sharedMaterial = material;

            CapsuleCollider collider = actorObject.GetComponent<CapsuleCollider>();
            collider.radius = 0.45f;
            collider.height = 2f;
            collider.isTrigger = false;

            Rigidbody rigidbody = actorObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            CharacterMotor motor = actorObject.AddComponent<CharacterMotor>();
            motor.Configure(settings.MoveSpeed, 0.45f, 2f, 1 << environmentLayer);
            LoopActor loopActor = actorObject.AddComponent<LoopActor>();
            loopActor.Configure(LoopActorKind.Echo);
            actorObject.AddComponent<EchoPlayback>();

            PrefabUtility.SaveAsPrefabAsset(actorObject, EchoPrefabPath);
            Object.DestroyImmediate(actorObject);

            EchoPlayback prefab = AssetDatabase.LoadAssetAtPath<EchoPlayback>(EchoPrefabPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("Echo prefab creation failed.");
            }

            return prefab;
        }

        private static void CreateRoom(
            Transform parent,
            Material floorMaterial,
            Material wallMaterial,
            int environmentLayer)
        {
            CreatePrimitive(
                "Floor",
                PrimitiveType.Cube,
                new Vector3(0f, -0.25f, 0f),
                new Vector3(12f, 0.5f, 12f),
                floorMaterial,
                parent,
                environmentLayer);

            CreatePrimitive("Wall West", PrimitiveType.Cube, new Vector3(-6.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 12.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Wall East", PrimitiveType.Cube, new Vector3(6.25f, 1.5f, 0f), new Vector3(0.5f, 3f, 12.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Wall South", PrimitiveType.Cube, new Vector3(0f, 1.5f, -6.25f), new Vector3(12.5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Wall North", PrimitiveType.Cube, new Vector3(0f, 1.5f, 6.25f), new Vector3(12.5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Divider West", PrimitiveType.Cube, new Vector3(-3.5f, 1.5f, 1f), new Vector3(5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
            CreatePrimitive("Divider East", PrimitiveType.Cube, new Vector3(3.5f, 1.5f, 1f), new Vector3(5f, 3f, 0.5f), wallMaterial, parent, environmentLayer);
        }

        private static PlayerSimulation CreatePlayer(
            Transform parent,
            Material material,
            LoopSettings settings,
            int actorLayer,
            int environmentLayer,
            out TransformResettable resettable)
        {
            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerObject.name = "Player";
            playerObject.layer = actorLayer;
            playerObject.transform.SetParent(parent);
            playerObject.transform.SetPositionAndRotation(ActorStartPosition, ActorStartRotation);
            playerObject.GetComponent<Renderer>().sharedMaterial = material;

            CapsuleCollider collider = playerObject.GetComponent<CapsuleCollider>();
            collider.radius = 0.45f;
            collider.height = 2f;
            collider.isTrigger = false;

            Rigidbody rigidbody = playerObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            CharacterMotor motor = playerObject.AddComponent<CharacterMotor>();
            motor.Configure(settings.MoveSpeed, 0.45f, 2f, 1 << environmentLayer);
            LoopActor actor = playerObject.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Player);
            KeyboardInputSource input = playerObject.AddComponent<KeyboardInputSource>();
            PlayerSimulation simulation = playerObject.AddComponent<PlayerSimulation>();
            simulation.Configure(input, motor);
            resettable = playerObject.AddComponent<TransformResettable>();
            return simulation;
        }

        private static PressurePlate CreatePressurePlate(
            Transform parent,
            Material material,
            int interactionTriggerLayer)
        {
            GameObject plateObject = CreatePrimitive(
                "Pressure Plate",
                PrimitiveType.Cube,
                new Vector3(-2f, 0.075f, -1.5f),
                new Vector3(2f, 0.15f, 2f),
                material,
                parent,
                interactionTriggerLayer);
            plateObject.GetComponent<BoxCollider>().isTrigger = true;
            return plateObject.AddComponent<PressurePlate>();
        }

        private static DoorController CreateDoor(
            Transform parent,
            Material material,
            PressurePlate pressurePlate,
            int environmentLayer)
        {
            GameObject doorObject = CreatePrimitive(
                "Door",
                PrimitiveType.Cube,
                new Vector3(0f, 1.5f, 1f),
                new Vector3(2f, 3f, 0.45f),
                material,
                parent,
                environmentLayer);
            DoorController door = doorObject.AddComponent<DoorController>();
            door.Configure(pressurePlate, new Vector3(0f, 3.5f, 0f), 5f);
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
            goalObject.GetComponent<BoxCollider>().isTrigger = true;
            return goalObject.AddComponent<GoalVolume>();
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
    }
}
