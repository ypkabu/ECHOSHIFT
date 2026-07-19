using System;
using EchoShift.Core;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Presentation;
using EchoShift.Replay;
using EchoShift.Reset;
using EchoShift.Telemetry;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class P3SceneBuilder
    {
        private const string Root = "Assets/_Project";
        public const string ScenePath = Root + "/Scenes/P3_PlayableGreybox.unity";
        public const string SettingsPath = Root + "/Settings/LoopSettings_P3.asset";
        public const string TextCatalogPath = Root + "/Settings/Phase3TextCatalog.asset";
        public const string CameraSettingsPath = Root + "/Settings/Phase3CameraSettings.asset";
        private const string EchoPrefabPath = Root + "/Prefabs/Actors/P3_Echo.prefab";
        private const string P2EchoPrefabPath = Root + "/Prefabs/Actors/P2_Echo.prefab";
        private const string MaterialRoot = Root + "/Art/Temp/Materials/P3";
        private const string P2Scene = Root + "/Scenes/P2_CoordinationLab.unity";
        private const string P1Scene = Root + "/Scenes/P1_InteractionLab.unity";
        private const string P0Scene = Root + "/Scenes/P0_ReplayLab.unity";

        [MenuItem("ECHO SHIFT/Build Phase 3 Scene")]
        public static void BuildFromMenu()
        {
            BuildScene();
            Debug.Log($"Phase 3 scene generated at {ScenePath}.");
        }

        public static void BuildFromCommandLine()
        {
            BuildScene();
            Debug.Log($"Phase 3 scene generated at {ScenePath}.");
        }

        public static void BuildScene()
        {
            EnsureFolders();
            int playerLayer = RequireLayer("Player");
            int environmentLayer = RequireLayer("Environment");
            int echoLayer = RequireLayer("Echo");
            int triggerLayer = RequireLayer("InteractionTrigger");
            int targetLayer = RequireLayer("InteractionTarget");
            LoopSettings settings = CreateSettings();
            Phase3TextCatalog catalog = CreateCatalog();
            Phase3CameraSettings cameraSettings = CreateCameraSettings();
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                P0SceneBuilder.InputActionsPath);
            ValidateInput(actions);

            Material floor = Material("Floor", new Color(0.075f, 0.09f, 0.13f, 1f));
            Material wall = Material("Wall", new Color(0.24f, 0.29f, 0.38f, 1f));
            Material player = Material("Player", new Color(1f, 0.75f, 0.16f, 1f));
            Material plate = Material("Plate", new Color(0.1f, 0.7f, 1f, 1f));
            Material battery = Material("Battery", new Color(1f, 0.38f, 0.05f, 1f));
            Material socket = Material("Socket", new Color(0.65f, 0.15f, 1f, 1f));
            Material door = Material("Door", new Color(1f, 0.18f, 0.1f, 1f));
            Material goal = Material("Goal", new Color(0.12f, 1f, 0.42f, 0.65f), true);
            Material plateWire = Material("PlateWire", new Color(0.1f, 0.75f, 1f, 1f), false, true);
            Material powerWire = Material("PowerWire", new Color(0.8f, 0.2f, 1f, 1f), false, true);
            EchoPlayback echoPrefab = CreateEchoPrefab(plateWire, echoLayer);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject sectionContainer = new GameObject("Puzzle Sections");
            PuzzleSectionController[] sections = new PuzzleSectionController[3];
            sections[0] = CreateSection(
                1, catalog.GetSectionName(0), catalog, new Vector3(0f, 0f, 0f), sectionContainer.transform,
                settings, actions, echoPrefab, floor, wall, player, plate, battery,
                socket, door, goal, plateWire, powerWire, playerLayer,
                environmentLayer, triggerLayer, targetLayer);
            sections[1] = CreateSection(
                2, catalog.GetSectionName(1), catalog, new Vector3(30f, 0f, 0f), sectionContainer.transform,
                settings, actions, echoPrefab, floor, wall, player, plate, battery,
                socket, door, goal, plateWire, powerWire, playerLayer,
                environmentLayer, triggerLayer, targetLayer);
            sections[2] = CreateSection(
                3, catalog.GetSectionName(2), catalog, new Vector3(65f, 0f, 0f), sectionContainer.transform,
                settings, actions, echoPrefab, floor, wall, player, plate, battery,
                socket, door, goal, plateWire, powerWire, playerLayer,
                environmentLayer, triggerLayer, targetLayer);
            sections[1].gameObject.SetActive(false);
            sections[2].gameObject.SetActive(false);

            GameObject global = new GameObject("Phase 3 Systems");
            SectionCameraController camera = CreateCamera(global.transform, cameraSettings);
            CreateLight(global.transform);
            PlaytestTelemetry telemetry = global.AddComponent<PlaytestTelemetry>();
            GameplayHud hud = CreateHud(global.transform, catalog);
            SectionTransitionCoordinator coordinator =
                global.AddComponent<SectionTransitionCoordinator>();
            PauseMenuController pauseMenu = CreatePauseMenu(global.transform, coordinator, catalog);
            JapaneseFontApplier fontApplier = global.AddComponent<JapaneseFontApplier>();
            fontApplier.Configure(catalog, new[] { sectionContainer.transform, global.transform });
            hud.SetFontApplier(fontApplier);
            coordinator.Configure(sections, camera, hud, pauseMenu, telemetry);

            for (int i = 0; i < sections.Length; i++)
            {
                TutorialGuide guide = sections[i].gameObject.AddComponent<TutorialGuide>();
                guide.Configure(sections[i], catalog, hud);
                CreateTutorialTrigger(sections[i].transform, guide, catalog, i, triggerLayer);
            }

            StableIdValidationResult validation =
                StableIdSceneValidator.ValidateScene(scene, out string validationError);
            if (!validation.IsValid) throw new InvalidOperationException(validationError);

            PlayerSettings.companyName = "Echo Shift Prototype";
            PlayerSettings.productName = "ECHO SHIFT";
            PlayerSettings.bundleVersion = "0.3.0-ja-pretest";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException($"Could not save {ScenePath}.");
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true),
                new EditorBuildSettingsScene(P2Scene, true),
                new EditorBuildSettingsScene(P1Scene, true),
                new EditorBuildSettingsScene(P0Scene, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static PuzzleSectionController CreateSection(
            int number, string displayName, Phase3TextCatalog catalog,
            Vector3 worldOffset, Transform parent,
            LoopSettings settings, InputActionAsset actions, EchoPlayback echoPrefab,
            Material floor, Material wall, Material playerMaterial, Material plateMaterial,
            Material batteryMaterial, Material socketMaterial, Material doorMaterial,
            Material goalMaterial, Material plateWire, Material powerWire,
            int playerLayer, int environmentLayer, int triggerLayer, int targetLayer)
        {
            GameObject root = new GameObject($"Section {number} - {displayName}");
            root.transform.SetParent(parent);
            root.transform.position = worldOffset;
            GameObject environment = new GameObject("Environment");
            environment.transform.SetParent(root.transform, false);
            GameObject gameplay = new GameObject("Gameplay");
            gameplay.transform.SetParent(root.transform, false);
            GameObject systems = new GameObject("Systems");
            systems.transform.SetParent(root.transform, false);
            GameObject echoes = new GameObject("Echoes");
            echoes.transform.SetParent(root.transform, false);
            CreateRoom(environment.transform, floor, wall, environmentLayer,
                number == 3 ? 26f : 16f);

            InteractionRegistry registry = systems.AddComponent<InteractionRegistry>();
            Vector3 startLocal = new Vector3(0f, 1f, number == 3 ? -10.5f : -7f);
            Transform spawn = new GameObject("Player Spawn").transform;
            spawn.SetParent(root.transform, false);
            spawn.localPosition = startLocal;
            PlayerSimulation player = CreatePlayer(
                gameplay.transform, startLocal, playerMaterial, settings, actions, registry,
                playerLayer, environmentLayer, targetLayer, catalog,
                out TransformResettable playerReset);

            PressurePlate pressurePlate = null;
            CarryableBattery battery = null;
            PowerSocket powerSocket = null;
            DoorController gateA = null;
            DoorController gateB = null;
            GoalVolume goal;
            if (number == 1)
            {
                pressurePlate = CreatePlate(gameplay.transform,
                    new Vector3(-2f, 0.1f, -3f), plateMaterial, catalog, triggerLayer);
                gateA = CreateDoor("Plate Door", gameplay.transform,
                    new Vector3(0f, 1.5f, 0f), doorMaterial,
                    pressurePlate, catalog, environmentLayer);
                CreateDivider(environment.transform, wall, environmentLayer, 0f);
                goal = CreateGoal(gameplay.transform,
                    new Vector3(0f, 0.1f, 5f), goalMaterial, catalog, triggerLayer);
                CreateWire(root.transform, new Vector3(-2f, 0.03f, -3f),
                    new Vector3(0f, 0.03f, 0f), plateWire);
            }
            else if (number == 2)
            {
                battery = CreateBattery(gameplay.transform,
                    new Vector3(1f, 0.45f, -4f), batteryMaterial, registry,
                    catalog, targetLayer, "p3-s2-battery-063ce56d");
                powerSocket = CreateSocket(gameplay.transform,
                    new Vector3(1f, 0.4f, -1f), socketMaterial, registry,
                    catalog, targetLayer, "p3-s2-socket-a906013e");
                gateB = CreateDoor("Powered Door", gameplay.transform,
                    new Vector3(0f, 1.5f, 1f), doorMaterial,
                    powerSocket, catalog, environmentLayer);
                CreateDivider(environment.transform, wall, environmentLayer, 1f);
                goal = CreateGoal(gameplay.transform,
                    new Vector3(0f, 0.1f, 5f), goalMaterial, catalog, triggerLayer);
                CreateWire(root.transform, new Vector3(1f, 0.03f, -1f),
                    new Vector3(0f, 0.03f, 1f), powerWire);
            }
            else
            {
                pressurePlate = CreatePlate(gameplay.transform,
                    new Vector3(-3f, 0.1f, -7f), plateMaterial, catalog, triggerLayer);
                gateA = CreateDoor("Gate A - Plate", gameplay.transform,
                    new Vector3(0f, 1.5f, -3f), doorMaterial,
                    pressurePlate, catalog, environmentLayer);
                CreateDivider(environment.transform, wall, environmentLayer, -3f);
                battery = CreateBattery(gameplay.transform,
                    new Vector3(1.5f, 0.45f, 0f), batteryMaterial, registry,
                    catalog, targetLayer, "p3-s3-battery-3aaf6a48");
                powerSocket = CreateSocket(gameplay.transform,
                    new Vector3(1.5f, 0.4f, 3.2f), socketMaterial, registry,
                    catalog, targetLayer, "p3-s3-socket-852fdd89");
                gateB = CreateDoor("Gate B - Battery", gameplay.transform,
                    new Vector3(0f, 1.5f, 5f), doorMaterial,
                    powerSocket, catalog, environmentLayer);
                CreateDivider(environment.transform, wall, environmentLayer, 5f);
                goal = CreateGoal(gameplay.transform,
                    new Vector3(0f, 0.1f, 10.2f), goalMaterial, catalog, triggerLayer);
                CreateWire(root.transform, new Vector3(-3f, 0.03f, -7f),
                    new Vector3(0f, 0.03f, -3f), plateWire);
                CreateWire(root.transform, new Vector3(1.5f, 0.03f, 3.2f),
                    new Vector3(0f, 0.03f, 5f), powerWire);
            }

            MonoBehaviour[] resetValues = BuildResetArray(
                pressurePlate, battery, powerSocket, gateA, gateB, goal);
            ResetRegistry resetRegistry = systems.AddComponent<ResetRegistry>();
            resetRegistry.Configure(resetValues);
            DoorController[] doors = gateB != null && gateA != null
                ? new[] { gateA, gateB }
                : gateA != null ? new[] { gateA } : new[] { gateB };
            PressurePlate[] plates = pressurePlate != null
                ? new[] { pressurePlate } : Array.Empty<PressurePlate>();
            LoopDirector director = systems.AddComponent<LoopDirector>();
            director.Configure(settings, player, playerReset, resetRegistry,
                echoPrefab, echoes.transform, registry, plates, doors, null);
            InteractionHighlight highlight = systems.AddComponent<InteractionHighlight>();
            highlight.Configure(player);
            PuzzleSectionController section = root.AddComponent<PuzzleSectionController>();
            section.Configure(number, displayName, director, player, goal, spawn);
            Phase0DebugOverlay overlay = systems.AddComponent<Phase0DebugOverlay>();
            if (number == 1) overlay.Configure(director, pressurePlate, goal);
            else if (number == 2) overlay.ConfigurePhase1(director, player, battery, powerSocket, goal);
            else overlay.ConfigurePhase2(director, player, pressurePlate, battery, powerSocket, goal);
            overlay.SetVisible(false);
            return section;
        }

        private static PlayerSimulation CreatePlayer(
            Transform parent, Vector3 localPosition, Material material,
            LoopSettings settings, InputActionAsset actions, InteractionRegistry registry,
            int playerLayer, int environmentLayer, int targetLayer,
            Phase3TextCatalog catalog,
            out TransformResettable resettable)
        {
            GameObject obj = Primitive("Player", PrimitiveType.Capsule, localPosition,
                Vector3.one, material, parent, playerLayer, true);
            CapsuleCollider collider = obj.GetComponent<CapsuleCollider>();
            collider.radius = 0.45f;
            collider.height = 2f;
            Rigidbody body = obj.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            CharacterMotor motor = obj.AddComponent<CharacterMotor>();
            motor.Configure(settings.MoveSpeed, 0.45f, 2f, 1 << environmentLayer);
            LoopActor actor = obj.AddComponent<LoopActor>();
            actor.Configure(LoopActorKind.Player, 1);
            Transform carry = new GameObject("Carry Socket").transform;
            carry.SetParent(obj.transform, false);
            carry.localPosition = new Vector3(0f, 0.35f, 0.8f);
            InteractionSensor sensor = obj.AddComponent<InteractionSensor>();
            sensor.Configure(1.5f, 1 << targetLayer);
            Interactor interactor = obj.AddComponent<Interactor>();
            interactor.Configure(actor, sensor, carry, registry);
            InputSystemInputSource input = obj.AddComponent<InputSystemInputSource>();
            input.Configure(actions);
            PlayerSimulation simulation = obj.AddComponent<PlayerSimulation>();
            simulation.Configure(input, motor, interactor);
            resettable = obj.AddComponent<TransformResettable>();
            CreateMarker(obj.transform, catalog.PlayerMarker, new Vector3(0f, 1.4f, 0f), material);
            return simulation;
        }

        private static PressurePlate CreatePlate(
            Transform parent, Vector3 local, Material material,
            Phase3TextCatalog catalog, int layer)
        {
            GameObject obj = Primitive("Pressure Plate", PrimitiveType.Cube, local,
                new Vector3(2.4f, 0.2f, 2.4f), material, parent, layer, true);
            obj.GetComponent<Collider>().isTrigger = true;
            CreateMarker(obj.transform, catalog.SwitchMarker, new Vector3(0f, 0.7f, 0f), material);
            return obj.AddComponent<PressurePlate>();
        }

        private static CarryableBattery CreateBattery(
            Transform parent, Vector3 local, Material material,
            InteractionRegistry registry, Phase3TextCatalog catalog,
            int layer, string stableValue)
        {
            GameObject obj = Primitive("Battery Cell", PrimitiveType.Cylinder, local,
                new Vector3(0.45f, 0.65f, 0.45f), material, parent, layer, true);
            Collider collider = obj.GetComponent<Collider>();
            collider.isTrigger = true;
            Rigidbody body = obj.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            StableId id = obj.AddComponent<StableId>();
            CarryableBattery battery = obj.AddComponent<CarryableBattery>();
            battery.Configure(id, collider, body);
            id.Configure(stableValue, registry, battery);
            CreateMarker(obj.transform, catalog.BatteryMarker, new Vector3(0f, 1.1f, 0f), material);
            return battery;
        }

        private static PowerSocket CreateSocket(
            Transform parent, Vector3 local, Material material,
            InteractionRegistry registry, Phase3TextCatalog catalog,
            int layer, string stableValue)
        {
            GameObject obj = Primitive("Power Socket", PrimitiveType.Cube, local,
                new Vector3(1.1f, 0.8f, 1.1f), material, parent, layer, true);
            obj.GetComponent<Collider>().isTrigger = true;
            Transform insertion = new GameObject("Insertion Point").transform;
            insertion.SetParent(obj.transform, false);
            insertion.localPosition = new Vector3(0f, 0.75f, 0f);
            StableId id = obj.AddComponent<StableId>();
            PowerSocket socket = obj.AddComponent<PowerSocket>();
            socket.Configure(id, insertion);
            id.Configure(stableValue, registry, socket);
            CreateMarker(obj.transform, catalog.PowerMarker, new Vector3(0f, 1.2f, 0f), material);
            return socket;
        }

        private static DoorController CreateDoor(
            string name, Transform parent, Vector3 local, Material material,
            MonoBehaviour source, Phase3TextCatalog catalog, int layer)
        {
            GameObject obj = Primitive(name, PrimitiveType.Cube, local,
                new Vector3(4f, 3f, 0.45f), material, parent, layer, true);
            DoorController controller = obj.AddComponent<DoorController>();
            controller.Configure(new[] { source }, new Vector3(0f, 3.5f, 0f), 5f);
            DoorVisualFeedback visual = obj.AddComponent<DoorVisualFeedback>();
            visual.Configure(controller, obj.GetComponent<Renderer>());
            CreateMarker(obj.transform, catalog.GateMarker, new Vector3(0f, 0.8f, 0.55f), material);
            return controller;
        }

        private static GoalVolume CreateGoal(
            Transform parent, Vector3 local, Material material,
            Phase3TextCatalog catalog, int layer)
        {
            GameObject obj = Primitive("Goal Exit", PrimitiveType.Cylinder, local,
                new Vector3(1.8f, 0.12f, 1.8f), material, parent, layer, true);
            obj.GetComponent<Collider>().isTrigger = true;
            CreateMarker(obj.transform, catalog.ExitMarker, new Vector3(0f, 5f, 0f), material);
            return obj.AddComponent<GoalVolume>();
        }

        private static void CreateRoom(
            Transform parent, Material floor, Material wall, int layer, float length)
        {
            Primitive("Floor", PrimitiveType.Cube, new Vector3(0f, -0.25f, 0f),
                new Vector3(14f, 0.5f, length), floor, parent, layer, true);
            float half = length * 0.5f;
            Primitive("Wall West", PrimitiveType.Cube, new Vector3(-7.25f, 1.5f, 0f),
                new Vector3(0.5f, 3f, length + 0.5f), wall, parent, layer, true);
            Primitive("Wall East", PrimitiveType.Cube, new Vector3(7.25f, 1.5f, 0f),
                new Vector3(0.5f, 3f, length + 0.5f), wall, parent, layer, true);
            Primitive("Wall South", PrimitiveType.Cube, new Vector3(0f, 1.5f, -half - 0.25f),
                new Vector3(14.5f, 3f, 0.5f), wall, parent, layer, true);
            Primitive("Wall North", PrimitiveType.Cube, new Vector3(0f, 1.5f, half + 0.25f),
                new Vector3(14.5f, 3f, 0.5f), wall, parent, layer, true);
        }

        private static void CreateDivider(
            Transform parent, Material wall, int layer, float z)
        {
            Primitive($"Divider West {z}", PrimitiveType.Cube,
                new Vector3(-4.5f, 1.5f, z), new Vector3(5f, 3f, 0.5f),
                wall, parent, layer, true);
            Primitive($"Divider East {z}", PrimitiveType.Cube,
                new Vector3(4.5f, 1.5f, z), new Vector3(5f, 3f, 0.5f),
                wall, parent, layer, true);
        }

        private static void CreateWire(
            Transform parent, Vector3 start, Vector3 end, Material material)
        {
            GameObject obj = new GameObject("Floor Wiring");
            obj.transform.SetParent(parent, false);
            LineRenderer line = obj.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 3;
            line.SetPosition(0, start);
            line.SetPosition(1, new Vector3(start.x, start.y, end.z));
            line.SetPosition(2, end);
            line.widthMultiplier = 0.12f;
            line.sharedMaterial = material;
            line.numCornerVertices = 3;
        }

        private static void CreateMarker(
            Transform parent, string markerName, Vector3 local, Material material)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = $"{markerName} Marker";
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = local;
            marker.transform.localScale = Vector3.one * 0.22f;
            marker.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static SectionCameraController CreateCamera(
            Transform parent, Phase3CameraSettings settings)
        {
            GameObject obj = new GameObject("Main Camera");
            obj.transform.SetParent(parent);
            Camera camera = obj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.018f, 0.026f, 0.05f, 1f);
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 120f;
            SectionCameraController controller = obj.AddComponent<SectionCameraController>();
            controller.Configure(settings);
            return controller;
        }

        private static void CreateLight(Transform parent)
        {
            GameObject obj = new GameObject("Directional Light");
            obj.transform.SetParent(parent);
            obj.transform.rotation = Quaternion.Euler(52f, -32f, 0f);
            Light light = obj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.shadows = LightShadows.Soft;
        }

        private static GameplayHud CreateHud(Transform parent, Phase3TextCatalog catalog)
        {
            GameObject canvasObject = new GameObject("Gameplay HUD");
            canvasObject.transform.SetParent(parent);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            Text section = Text(canvasObject.transform, "Section", new Vector2(24f, -22f),
                new Vector2(840f, 54f), TextAnchor.UpperLeft, 28);
            Text objective = Text(canvasObject.transform, "Objective", new Vector2(24f, -66f),
                new Vector2(840f, 50f), TextAnchor.UpperLeft, 22);
            Text loop = Text(canvasObject.transform, "Loop", new Vector2(24f, -120f),
                new Vector2(340f, 44f), TextAnchor.UpperLeft, 22);
            Text timer = Text(canvasObject.transform, "Timer", new Vector2(24f, -164f),
                new Vector2(340f, 44f), TextAnchor.UpperLeft, 22);
            Text echoes = Text(canvasObject.transform, "Echoes", new Vector2(24f, -208f),
                new Vector2(340f, 44f), TextAnchor.UpperLeft, 22);
            Text carry = Text(canvasObject.transform, "Carry", new Vector2(24f, -252f),
                new Vector2(420f, 44f), TextAnchor.UpperLeft, 22);
            Text tutorial = BottomText(canvasObject.transform, "Tutorial", 170f, 24);
            Text prompt = BottomText(canvasObject.transform, "Prompt", 105f, 30);
            Text endLoop = BottomText(canvasObject.transform, "EndLoop", 55f, 20);
            Text pausePrompt = BottomText(canvasObject.transform, "PausePrompt", 18f, 17);
            Text state = CenterText(canvasObject.transform, "State", 90f, 34);
            Text failure = CenterText(canvasObject.transform, "Failure", 20f, 28);
            failure.color = new Color(1f, 0.35f, 0.2f, 1f);
            GameplayHud hud = canvasObject.AddComponent<GameplayHud>();
            hud.Configure(catalog, section, objective, tutorial, loop, timer, echoes,
                prompt, endLoop, pausePrompt, carry, state, failure);
            return hud;
        }

        private static PauseMenuController CreatePauseMenu(
            Transform parent, SectionTransitionCoordinator coordinator,
            Phase3TextCatalog catalog)
        {
            GameObject eventSystemObject = new GameObject("Event System");
            eventSystemObject.transform.SetParent(parent, false);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

            GameObject canvasObject = new GameObject("Pause Menu Canvas");
            canvasObject.transform.SetParent(parent);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.AddComponent<GraphicRaycaster>();
            GameObject panel = new GameObject("Pause Panel");
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.30f, 0.12f);
            rect.anchorMax = new Vector2(0.70f, 0.88f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.03f, 0.045f, 0.08f, 0.94f);
            Text title = CenterAnchoredText(panel.transform, "Title", 34);
            title.text = catalog.Paused;
            title.rectTransform.anchoredPosition = new Vector2(0f, 240f);
            Text confirmation = CenterAnchoredText(panel.transform, "Confirmation", 20);
            confirmation.rectTransform.sizeDelta = new Vector2(620f, 90f);
            confirmation.rectTransform.anchoredPosition = new Vector2(0f, -230f);
            Button resume = Button(panel.transform, catalog.Resume, 140f);
            Button restartSection = Button(panel.transform, catalog.RestartSection, 50f);
            Button restartGame = Button(panel.transform, catalog.RestartGame, -40f);
            Button quit = Button(panel.transform, catalog.Quit, -130f);
            PauseMenuController menu = canvasObject.AddComponent<PauseMenuController>();
            menu.Configure(coordinator, catalog, panel, title, confirmation,
                resume, restartSection, restartGame, quit);
            panel.SetActive(false);
            return menu;
        }

        private static Text Text(
            Transform parent, string name, Vector2 position, Vector2 size,
            TextAnchor alignment, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Text BottomText(Transform parent, string name, float y, int fontSize)
        {
            Text text = CenterAnchoredText(parent, name, fontSize);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, y);
            return text;
        }

        private static Text CenterText(Transform parent, string name, float y, int fontSize)
        {
            Text text = CenterAnchoredText(parent, name, fontSize);
            text.rectTransform.anchoredPosition = new Vector2(0f, y);
            return text;
        }

        private static Text CenterAnchoredText(Transform parent, string name, int fontSize)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(1000f, 70f);
            Text text = obj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button Button(Transform parent, string label, float y)
        {
            GameObject obj = new GameObject(label);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(540f, 70f);
            rect.anchoredPosition = new Vector2(0f, y);
            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.12f, 0.2f, 0.32f, 1f);
            Button button = obj.AddComponent<Button>();
            Text text = CenterAnchoredText(obj.transform, "Label", 24);
            text.text = label;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 16;
            text.resizeTextMaxSize = 24;
            text.rectTransform.anchoredPosition = Vector2.zero;
            return button;
        }

        private static void CreateTutorialTrigger(
            Transform parent, TutorialGuide guide, Phase3TextCatalog catalog,
            int sectionIndex, int layer)
        {
            if (sectionIndex >= 2) return;
            GameObject obj = new GameObject("Tutorial Trigger - Move");
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = new Vector3(0f, 1f, -5.5f);
            obj.layer = layer;
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(5f, 2f, 2f);
            TutorialTrigger trigger = obj.AddComponent<TutorialTrigger>();
            trigger.Configure(guide, 0, catalog.GetTutorialText(sectionIndex, 0));
        }

        private static EchoPlayback CreateEchoPrefab(Material trailMaterial, int echoLayer)
        {
            if (AssetDatabase.LoadAssetAtPath<EchoPlayback>(EchoPrefabPath) == null &&
                !AssetDatabase.CopyAsset(P2EchoPrefabPath, EchoPrefabPath))
                throw new InvalidOperationException("Could not create P3 Echo prefab.");
            GameObject root = PrefabUtility.LoadPrefabContents(EchoPrefabPath);
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root) > 0)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(root);
            root.layer = echoLayer;
            TrailRenderer trail = root.GetComponent<TrailRenderer>();
            if (trail == null) trail = root.AddComponent<TrailRenderer>();
            trail.time = 1.2f;
            trail.startWidth = 0.22f;
            trail.endWidth = 0.02f;
            trail.sharedMaterial = trailMaterial;
            trail.minVertexDistance = 0.08f;
            if (root.GetComponent<EchoVisualFeedback>() == null)
                root.AddComponent<EchoVisualFeedback>();
            PrefabUtility.SaveAsPrefabAsset(root, EchoPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
            return AssetDatabase.LoadAssetAtPath<EchoPlayback>(EchoPrefabPath);
        }

        private static LoopSettings CreateSettings()
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
            return asset;
        }

        private static Phase3TextCatalog CreateCatalog()
        {
            Phase3TextCatalog asset = AssetDatabase.LoadAssetAtPath<Phase3TextCatalog>(TextCatalogPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<Phase3TextCatalog>();
                AssetDatabase.CreateAsset(asset, TextCatalogPath);
            }
            asset.ApplyJapaneseDefaults();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Phase3CameraSettings CreateCameraSettings()
        {
            Phase3CameraSettings asset =
                AssetDatabase.LoadAssetAtPath<Phase3CameraSettings>(CameraSettingsPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<Phase3CameraSettings>();
                AssetDatabase.CreateAsset(asset, CameraSettingsPath);
            }
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Material Material(
            string name, Color color, bool transparent = false, bool unlit = false)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(unlit
                ? "Universal Render Pipeline/Unlit"
                : "Universal Render Pipeline/Lit");
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
            if (unlit) material.SetColor("_EmissionColor", color * 1.5f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject Primitive(
            string name, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Transform parent, int layer, bool local)
        {
            GameObject result = GameObject.CreatePrimitive(type);
            result.name = name;
            result.layer = layer;
            result.transform.SetParent(parent, false);
            if (local) result.transform.localPosition = position;
            else result.transform.position = position;
            result.transform.localScale = scale;
            result.GetComponent<Renderer>().sharedMaterial = material;
            return result;
        }

        private static MonoBehaviour[] BuildResetArray(params MonoBehaviour[] values)
        {
            int count = 0;
            for (int i = 0; i < values.Length; i++) if (values[i] != null) count++;
            MonoBehaviour[] result = new MonoBehaviour[count];
            int target = 0;
            for (int i = 0; i < values.Length; i++)
                if (values[i] != null) result[target++] = values[i];
            return result;
        }

        private static void ValidateInput(InputActionAsset actions)
        {
            InputActionMap gameplay = actions?.FindActionMap("Gameplay", false);
            if (gameplay == null || gameplay.FindAction("Move", false) == null ||
                gameplay.FindAction("Interact", false) == null ||
                gameplay.FindAction("EndLoop", false) == null ||
                gameplay.FindAction("Pause", false) == null)
                throw new InvalidOperationException("P3 input actions are incomplete.");
        }

        private static int RequireLayer(string name)
        {
            int value = LayerMask.NameToLayer(name);
            if (value < 0) throw new InvalidOperationException($"Required layer {name} is missing.");
            return value;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(MaterialRoot))
                AssetDatabase.CreateFolder(Root + "/Art/Temp/Materials", "P3");
        }
    }
}
