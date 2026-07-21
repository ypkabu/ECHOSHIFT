using System;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Presentation;
using EchoShift.Replay;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class Phase4SceneBuilder
    {
        public static void PolishEchoPrefab(string prefabPath, Phase4VisualSettings settings)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            Transform old = root.transform.Find("P4 Robot Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Renderer rootRenderer = root.GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;
            EchoPlayback playback = root.GetComponent<EchoPlayback>();
            LoopActor actor = root.GetComponent<LoopActor>();
            BuildActorVisual(root, actor, playback, settings, false);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        public static void Apply(
            PuzzleSectionController[] sections,
            SectionCameraController cameraController,
            GameplayHud hud,
            PauseMenuController pauseMenu,
            SectionTransitionCoordinator coordinator,
            JapaneseFontApplier fontApplier,
            Phase4VisualSettings settings)
        {
            if (sections == null || sections.Length != 3 || settings == null)
                throw new InvalidOperationException(
                    $"Phase 4 requires exactly three sections and settings;" +
                    $"sectionsNull={sections == null};count={(sections == null ? -1 : sections.Length)};" +
                    $"settingsNull={settings == null}.");

            for (int i = 0; i < sections.Length; i++)
            {
                BuildEnvironment(sections[i], i + 1, settings);
                BuildDevices(sections[i], settings);
                PlayerSimulation player = sections[i].Player;
                Renderer playerRenderer = player.GetComponent<Renderer>();
                if (playerRenderer != null) playerRenderer.enabled = false;
                BuildActorVisual(player.gameObject, player.GetComponent<LoopActor>(), null, settings, true);
            }

            Transform systems = coordinator.transform;
            BuildLighting(sections, cameraController, systems, settings);
            Phase4AudioController audio = BuildAudio(systems, settings);
            BuildFeedback(sections, coordinator, systems, settings, audio);
            for (int i = 0; i < sections.Length; i++)
            {
                Phase4MovementAudio movement = sections[i].Player.gameObject.AddComponent<Phase4MovementAudio>();
                movement.Configure(audio);
            }
            BuildHud(hud, pauseMenu, coordinator, settings, audio);
            fontApplier.Configure(hud.TextCatalog,
                new[] { sections[0].transform.parent, systems },
                settings.PackagedJapaneseFont, settings.PackagedJapaneseTmpFont);
            if (!fontApplier.ApplyNow())
            {
                string missing = JapaneseFontApplier.GetMissingRequiredGlyphs(
                    settings.PackagedJapaneseTmpFont,
                    hud.TextCatalog.GetRequiredGlyphCharacters());
                throw new InvalidOperationException(
                    $"Packaged Japanese font validation failed;settingsFontNull={settings.PackagedJapaneseFont == null};" +
                    $"settingsTmpNull={settings.PackagedJapaneseTmpFont == null};applierPackaged={fontApplier.UsesPackagedFont};" +
                    $"applierCatalog={fontApplier.HasCatalog};roots={fontApplier.RootCount};" +
                    $"missingLength={missing.Length};missing={missing}");
            }
            PlayerSettings.bundleVersion = "0.4.0-automation";
        }

        private static void BuildEnvironment(
            PuzzleSectionController section, int number, Phase4VisualSettings settings)
        {
            Transform environment = section.transform.Find("Environment");
            if (environment == null) throw new InvalidOperationException($"{section.name} has no Environment.");
            Renderer[] old = environment.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < old.Length; i++) old[i].enabled = false;
            Transform oldRoot = environment.Find("P4 Facility");
            if (oldRoot != null) Object.DestroyImmediate(oldRoot.gameObject);
            Transform root = new GameObject("P4 Facility").transform;
            root.SetParent(environment, false);

            Transform floor = environment.Find("Floor");
            float length = floor != null ? floor.localScale.z : number == 3 ? 26f : 16f;
            int rows = Mathf.CeilToInt(length / 2.7f);
            float rowDepth = length / rows;
            for (int z = 0; z < rows; z++)
            {
                float localZ = -length * 0.5f + rowDepth * (z + 0.5f);
                for (int x = 0; x < 3; x++)
                {
                    Material panel = ((x + z) & 1) == 0
                        ? settings.FacilityPanelMaterial : settings.FacilityTrimMaterial;
                    Visual($"Floor Panel {x + 1}-{z + 1}", PrimitiveType.Cube, root,
                        new Vector3((x - 1) * 4.55f, -0.015f, localZ),
                        new Vector3(4.34f, 0.12f, rowDepth - 0.12f), panel);
                }
            }

            int wallRows = Mathf.CeilToInt(length / 3.1f);
            float wallDepth = length / wallRows;
            for (int side = -1; side <= 1; side += 2)
            for (int z = 0; z < wallRows; z++)
            {
                float localZ = -length * 0.5f + wallDepth * (z + 0.5f);
                for (int y = 0; y < 2; y++)
                {
                    Visual($"Wall Panel {side}-{y}-{z}", PrimitiveType.Cube, root,
                        new Vector3(side * 7.08f, 0.76f + y * 1.48f, localZ),
                        new Vector3(0.18f, 1.33f, wallDepth - 0.12f),
                        y == 0 ? settings.FacilityDarkMaterial : settings.FacilityPanelMaterial);
                }
                if ((z & 1) == 0)
                    BuildMachineBank(root, side * 6.45f, localZ, side, settings);
            }

            for (float z = -length * 0.5f + 1.2f; z < length * 0.5f; z += 4.5f)
            {
                Visual($"Overhead Rail {z:0.0}", PrimitiveType.Cube, root,
                    new Vector3(0f, 3.05f, z), new Vector3(13.8f, 0.16f, 0.18f),
                    settings.FacilityTrimMaterial);
                Visual($"West Rail Lamp {z:0.0}", PrimitiveType.Cube, root,
                    new Vector3(-4.8f, 2.91f, z), new Vector3(2.2f, 0.045f, 0.28f),
                    settings.PlateMaterial);
                Visual($"East Rail Lamp {z:0.0}", PrimitiveType.Cube, root,
                    new Vector3(4.8f, 2.91f, z), new Vector3(2.2f, 0.045f, 0.28f),
                    settings.PlayerMaterial);
            }

            BuildBulkhead(root, -length * 0.5f, settings);
            BuildBulkhead(root, length * 0.5f, settings);
            Visual($"Section {number} Identity Strip", PrimitiveType.Cube, root,
                new Vector3(0f, 0.02f, -length * 0.5f + 0.65f),
                new Vector3(9f, 0.04f, 0.16f), settings.GetEchoMaterial(number));

            Light left = Light(root, $"Section {number} Fill A", new Vector3(-4.5f, 2.65f, -length * 0.22f));
            left.color = new Color(0.46f, 0.72f, 1f); left.intensity = 3.1f; left.range = 8f;
            Light right = Light(root, $"Section {number} Fill B", new Vector3(4.5f, 2.65f, length * 0.22f));
            right.color = new Color(1f, 0.56f, 0.32f); right.intensity = 2.6f; right.range = 8f;

            LineRenderer[] wires = section.GetComponentsInChildren<LineRenderer>(true);
            for (int i = 0; i < wires.Length; i++)
            {
                wires[i].widthMultiplier = 0.2f;
                wires[i].numCornerVertices = 6;
                wires[i].numCapVertices = 4;
            }
        }

        private static void BuildMachineBank(
            Transform root, float x, float z, int side, Phase4VisualSettings settings)
        {
            Transform bank = new GameObject("Facility Machine Bank").transform;
            bank.SetParent(root, false);
            bank.localPosition = new Vector3(x, 0f, z);
            Visual("Cabinet", PrimitiveType.Cube, bank, new Vector3(0f, 0.72f, 0f),
                new Vector3(0.65f, 1.35f, 1.7f), settings.FacilityDarkMaterial);
            Visual("Cabinet Face", PrimitiveType.Cube, bank,
                new Vector3(-side * 0.34f, 0.86f, 0f), new Vector3(0.045f, 0.78f, 1.24f),
                settings.FacilityTrimMaterial);
            for (int i = -1; i <= 1; i++)
                Visual($"Status {i}", PrimitiveType.Cube, bank,
                    new Vector3(-side * 0.37f, 1.05f, i * 0.34f),
                    new Vector3(0.03f, 0.08f, 0.18f), i == 0
                        ? settings.PlayerMaterial : settings.PlateMaterial);
        }

        private static void BuildBulkhead(Transform root, float z, Phase4VisualSettings settings)
        {
            Visual($"Bulkhead West {z}", PrimitiveType.Cube, root,
                new Vector3(-5.7f, 1.55f, z), new Vector3(2.5f, 3.05f, 0.24f),
                settings.FacilityDarkMaterial);
            Visual($"Bulkhead East {z}", PrimitiveType.Cube, root,
                new Vector3(5.7f, 1.55f, z), new Vector3(2.5f, 3.05f, 0.24f),
                settings.FacilityDarkMaterial);
            Visual($"Bulkhead Header {z}", PrimitiveType.Cube, root,
                new Vector3(0f, 2.85f, z), new Vector3(9f, 0.42f, 0.24f),
                settings.FacilityTrimMaterial);
        }

        private static void BuildDevices(PuzzleSectionController section, Phase4VisualSettings settings)
        {
            PressurePlate[] plates = section.GetComponentsInChildren<PressurePlate>(true);
            for (int i = 0; i < plates.Length; i++) BuildPlate(plates[i], settings);
            CarryableBattery[] batteries = section.GetComponentsInChildren<CarryableBattery>(true);
            for (int i = 0; i < batteries.Length; i++) BuildBattery(batteries[i], settings);
            PowerSocket[] sockets = section.GetComponentsInChildren<PowerSocket>(true);
            for (int i = 0; i < sockets.Length; i++) BuildSocket(sockets[i], settings);
            DoorController[] doors = section.GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < doors.Length; i++) BuildDoor(doors[i], settings);
            GoalVolume[] goals = section.GetComponentsInChildren<GoalVolume>(true);
            for (int i = 0; i < goals.Length; i++) BuildGoal(goals[i], settings);
        }

        private static void BuildPlate(PressurePlate plate, Phase4VisualSettings settings)
        {
            DisableRootAndMarker(plate.transform);
            Transform root = CompensatedRoot(plate.transform, "P4 Plate Visual");
            Visual("Plate Housing", PrimitiveType.Cube, root, Vector3.zero,
                new Vector3(2.65f, 0.16f, 2.65f), settings.FacilityDarkMaterial);
            GameObject pad = Visual("Pressure Pad", PrimitiveType.Cube, root,
                new Vector3(0f, 0.09f, 0f), new Vector3(2.18f, 0.12f, 2.18f), settings.PlateMaterial);
            Renderer[] accents = new Renderer[4];
            for (int i = 0; i < 4; i++)
            {
                float x = (i < 2 ? -1f : 1f) * 0.82f;
                float z = (i % 2 == 0 ? -1f : 1f) * 0.82f;
                accents[i] = Visual($"Plate Corner {i + 1}", PrimitiveType.Cube, root,
                    new Vector3(x, 0.18f, z), new Vector3(0.24f, 0.08f, 0.24f),
                    settings.PlateMaterial).GetComponent<Renderer>();
            }
            Phase4PressurePlateVisual visual = plate.gameObject.AddComponent<Phase4PressurePlateVisual>();
            visual.Configure(plate, pad.transform, accents, settings);
        }

        private static void BuildBattery(CarryableBattery battery, Phase4VisualSettings settings)
        {
            DisableRootAndMarker(battery.transform);
            Transform root = CompensatedRoot(battery.transform, "P4 Battery Visual");
            GameObject core = Visual("Energy Core", PrimitiveType.Cylinder, root, Vector3.zero,
                new Vector3(0.42f, 0.78f, 0.42f), settings.BatteryMaterial);
            core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                Visual($"Battery Endcap {side}", PrimitiveType.Cylinder, root,
                    new Vector3(0f, 0f, side * 0.43f), new Vector3(0.55f, 0.09f, 0.55f),
                    settings.FacilityDarkMaterial).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Visual($"Battery Key {side}", PrimitiveType.Cube, root,
                    new Vector3(0f, 0.34f, side * 0.2f), new Vector3(0.16f, 0.16f, 0.52f),
                    settings.PlayerMaterial);
            }
            Phase4BatteryVisual visual = battery.gameObject.AddComponent<Phase4BatteryVisual>();
            visual.Configure(battery, root, core.GetComponent<Renderer>(), settings);
        }

        private static void BuildSocket(PowerSocket socket, Phase4VisualSettings settings)
        {
            DisableRootAndMarker(socket.transform);
            Transform root = CompensatedRoot(socket.transform, "P4 Socket Visual");
            Visual("Socket Plinth", PrimitiveType.Cube, root, new Vector3(0f, -0.08f, 0f),
                new Vector3(1.4f, 0.46f, 1.4f), settings.FacilityDarkMaterial);
            GameObject ring = Visual("Socket Power Ring", PrimitiveType.Cylinder, root,
                new Vector3(0f, 0.25f, 0f), new Vector3(0.78f, 0.08f, 0.78f),
                settings.BatteryMaterial);
            Visual("Socket Recess", PrimitiveType.Cylinder, root,
                new Vector3(0f, 0.32f, 0f), new Vector3(0.5f, 0.09f, 0.5f),
                settings.FacilityDarkMaterial);
            Visual("Socket Key", PrimitiveType.Cube, root, new Vector3(0f, 0.38f, 0.42f),
                new Vector3(0.18f, 0.12f, 0.32f), settings.PlayerMaterial);
            Phase4SocketVisual visual = socket.gameObject.AddComponent<Phase4SocketVisual>();
            visual.Configure(socket, ring.GetComponent<Renderer>(), settings);
        }

        private static void BuildDoor(DoorController door, Phase4VisualSettings settings)
        {
            DisableRootAndMarker(door.transform);
            Transform root = CompensatedRoot(door.transform, "P4 Door Panel Visual");
            GameObject panel = Visual("Door Moving Panel", PrimitiveType.Cube, root, Vector3.zero,
                new Vector3(3.75f, 2.7f, 0.34f), settings.FacilityDarkMaterial);
            for (int i = -1; i <= 1; i += 2)
                Visual($"Door Edge {i}", PrimitiveType.Cube, root,
                    new Vector3(i * 1.78f, 0f, -0.2f), new Vector3(0.15f, 2.82f, 0.1f),
                    settings.FacilityTrimMaterial);
            GameObject badge = Visual("Door Status Badge", PrimitiveType.Cube, root,
                new Vector3(0f, 0.72f, -0.22f), new Vector3(0.68f, 0.18f, 0.08f),
                settings.DangerMaterial);
            Transform frame = new GameObject("P4 Doorway Frame").transform;
            frame.SetParent(door.transform.parent, false);
            frame.localPosition = door.transform.localPosition;
            for (int i = -1; i <= 1; i += 2)
                Visual($"Frame Side {i}", PrimitiveType.Cube, frame,
                    new Vector3(i * 2.2f, 0f, 0.05f), new Vector3(0.38f, 3.35f, 0.72f),
                    settings.FacilityTrimMaterial);
            Visual("Frame Header", PrimitiveType.Cube, frame,
                new Vector3(0f, 1.68f, 0.05f), new Vector3(4.75f, 0.36f, 0.72f),
                settings.FacilityTrimMaterial);
            DoorVisualFeedback visual = door.GetComponent<DoorVisualFeedback>();
            visual.Configure(door, panel.GetComponent<Renderer>(), badge.GetComponent<Renderer>(), settings);
        }

        private static void BuildGoal(GoalVolume goal, Phase4VisualSettings settings)
        {
            DisableRootAndMarker(goal.transform);
            Transform root = CompensatedRoot(goal.transform, "P4 Goal Portal");
            for (int side = -1; side <= 1; side += 2)
                Visual($"Portal Pillar {side}", PrimitiveType.Cube, root,
                    new Vector3(side * 1.35f, 1.25f, 0f), new Vector3(0.28f, 2.6f, 0.34f),
                    settings.GoalMaterial);
            Visual("Portal Header", PrimitiveType.Cube, root, new Vector3(0f, 2.48f, 0f),
                new Vector3(2.95f, 0.28f, 0.34f), settings.GoalMaterial);
            GameObject beam = Visual("Portal Beam", PrimitiveType.Cylinder, root,
                new Vector3(0f, 0.08f, 0f), new Vector3(1.55f, 0.025f, 1.55f),
                settings.GoalMaterial);
            Phase4GoalVisual visual = goal.gameObject.AddComponent<Phase4GoalVisual>();
            visual.Configure(goal, beam.transform, settings);
        }

        private static void BuildActorVisual(
            GameObject root, LoopActor actor, EchoPlayback playback,
            Phase4VisualSettings settings, bool player)
        {
            Transform old = root.transform.Find("P4 Robot Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform model = new GameObject("P4 Robot Visual").transform;
            model.SetParent(root.transform, false);
            Material body = settings.FacilityPanelMaterial;
            Material accent = player ? settings.PlayerMaterial : settings.GetEchoMaterial(1);
            Renderer[] bodies = new Renderer[7];
            bodies[0] = Visual("Torso", PrimitiveType.Cube, model, new Vector3(0f, 0.18f, 0f),
                new Vector3(0.76f, 0.72f, 0.52f), body).GetComponent<Renderer>();
            bodies[1] = Visual("Pelvis", PrimitiveType.Cube, model, new Vector3(0f, -0.28f, 0f),
                new Vector3(0.62f, 0.24f, 0.44f), body).GetComponent<Renderer>();
            bodies[2] = Visual("Head", PrimitiveType.Cube, model, new Vector3(0f, 0.75f, 0.03f),
                new Vector3(0.62f, 0.42f, 0.48f), body).GetComponent<Renderer>();
            Transform rightArm = null;
            int bodyIndex = 3;
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject arm = Visual(side < 0 ? "Left Arm" : "Right Arm", PrimitiveType.Cube,
                    model, new Vector3(side * 0.52f, 0.12f, 0f), new Vector3(0.18f, 0.62f, 0.2f), body);
                bodies[bodyIndex++] = arm.GetComponent<Renderer>();
                if (side > 0) rightArm = arm.transform;
                bodies[bodyIndex++] = Visual(side < 0 ? "Left Leg" : "Right Leg", PrimitiveType.Cube,
                    model, new Vector3(side * 0.23f, -0.7f, 0f), new Vector3(0.23f, 0.62f, 0.28f), body)
                    .GetComponent<Renderer>();
            }
            Renderer[] accents = new Renderer[3];
            accents[0] = Visual("Visor", PrimitiveType.Cube, model, new Vector3(0f, 0.78f, 0.285f),
                new Vector3(0.48f, 0.12f, 0.045f), accent).GetComponent<Renderer>();
            accents[1] = Visual("Chest Core", PrimitiveType.Cube, model, new Vector3(0f, 0.23f, 0.285f),
                new Vector3(0.25f, 0.3f, 0.05f), accent).GetComponent<Renderer>();
            accents[2] = Visual("Back Spine", PrimitiveType.Cube, model, new Vector3(0f, 0.16f, -0.285f),
                new Vector3(0.12f, 0.52f, 0.045f), accent).GetComponent<Renderer>();
            GameObject[] marks = new GameObject[3];
            for (int i = 0; i < marks.Length; i++)
                marks[i] = Visual($"Generation Mark {i + 1}", PrimitiveType.Cube, model,
                    new Vector3((i - 1) * 0.2f, 1.05f, 0f), new Vector3(0.11f, 0.2f, 0.11f), accent);
            Phase4ActorVisual visual = root.GetComponent<Phase4ActorVisual>();
            if (visual == null) visual = root.AddComponent<Phase4ActorVisual>();
            visual.Configure(settings, actor, playback, model, rightArm, bodies, accents, marks);
        }

        private static void BuildLighting(PuzzleSectionController[] sections,
            SectionCameraController cameraController, Transform systems,
            Phase4VisualSettings settings)
        {
            Light directional = systems.GetComponentInChildren<Light>(true);
            if (directional != null)
            {
                directional.color = new Color(0.76f, 0.84f, 1f);
                directional.intensity = 1.05f;
                directional.shadows = LightShadows.Soft;
            }
            Camera camera = cameraController.GetComponent<Camera>();
            camera.allowHDR = true;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            GameObject volumeObject = new GameObject("Phase 4 Global Volume");
            volumeObject.transform.SetParent(systems, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true; volume.priority = 10f; volume.sharedProfile = settings.VolumeProfile;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.24f, 0.3f, 0.42f);
            RenderSettings.ambientEquatorColor = new Color(0.08f, 0.12f, 0.18f);
            RenderSettings.ambientGroundColor = new Color(0.025f, 0.032f, 0.045f);
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.025f, 0.04f, 0.065f);
            RenderSettings.fogStartDistance = 45f; RenderSettings.fogEndDistance = 100f;
        }

        private static Phase4AudioController BuildAudio(Transform systems, Phase4VisualSettings settings)
        {
            GameObject root = new GameObject("Phase 4 Audio");
            root.transform.SetParent(systems, false);
            AudioSource[] sources = new AudioSource[8];
            for (int i = 0; i < sources.Length; i++)
            {
                GameObject sourceObject = new GameObject($"Audio Source {i + 1:00}");
                sourceObject.transform.SetParent(root.transform, false);
                sources[i] = sourceObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false; sources[i].spatialBlend = 0.15f;
            }
            Phase4AudioController audio = root.AddComponent<Phase4AudioController>();
            audio.Configure(settings.AudioCues, sources);
            return audio;
        }

        private static void BuildFeedback(PuzzleSectionController[] sections,
            SectionTransitionCoordinator coordinator, Transform systems,
            Phase4VisualSettings settings, Phase4AudioController audio)
        {
            GameObject root = new GameObject("Phase 4 Feedback");
            root.transform.SetParent(systems, false);
            Phase4FeedbackPool pool = root.AddComponent<Phase4FeedbackPool>();
            pool.Configure(settings.PulseVfxPrefab, settings.FeedbackPoolSize);
            Phase4FeedbackDirector director = root.AddComponent<Phase4FeedbackDirector>();
            director.Configure(sections, coordinator, settings, pool, audio);
            Phase4PerformanceProbe probe = root.AddComponent<Phase4PerformanceProbe>();
            probe.Configure(coordinator);
        }

        private static void BuildHud(GameplayHud hud, PauseMenuController pauseMenu,
            SectionTransitionCoordinator coordinator, Phase4VisualSettings settings,
            Phase4AudioController audio)
        {
            RectTransform canvas = (RectTransform)hud.transform;
            RectTransform left = Panel(canvas, "P4 Left Status Panel",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -18f),
                new Vector2(520f, 292f), new Vector2(0f, 1f));
            RectTransform right = Panel(canvas, "P4 Tutorial Panel",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -18f),
                new Vector2(850f, 270f), new Vector2(1f, 1f));
            RectTransform bottom = Panel(canvas, "P4 Prompt Panel",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f),
                new Vector2(760f, 96f), new Vector2(0.5f, 0f));
            left.SetAsFirstSibling(); right.SetSiblingIndex(1); bottom.SetSiblingIndex(2);

            string[] textNames = { "Loop", "Timer", "Echoes", "Carry" };
            for (int i = 0; i < textNames.Length; i++)
            {
                Transform label = canvas.Find(textNames[i]);
                if (label == null) continue;
                RectTransform labelRect = (RectTransform)label;
                labelRect.anchoredPosition += new Vector2(42f, 0f);
                GameObject iconObject = new GameObject(textNames[i] + " Icon");
                iconObject.transform.SetParent(canvas, false);
                RectTransform iconRect = iconObject.AddComponent<RectTransform>();
                iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 1f);
                iconRect.pivot = new Vector2(0f, 1f);
                iconRect.anchoredPosition = labelRect.anchoredPosition + new Vector2(-40f, -3f);
                iconRect.sizeDelta = new Vector2(28f, 28f);
                Image image = iconObject.AddComponent<Image>(); image.sprite = settings.HudIcons[i];
                image.color = Color.white;
            }
            GameObject timerObject = new GameObject("Loop Timer Bar");
            timerObject.transform.SetParent(canvas, false);
            RectTransform timerRect = timerObject.AddComponent<RectTransform>();
            timerRect.anchorMin = timerRect.anchorMax = new Vector2(0f, 1f);
            timerRect.pivot = new Vector2(0f, 1f); timerRect.anchoredPosition = new Vector2(64f, -204f);
            timerRect.sizeDelta = new Vector2(360f, 8f);
            Image timerFill = timerObject.AddComponent<Image>();
            timerFill.color = settings.PlateColor; timerFill.type = Image.Type.Filled;
            timerFill.fillMethod = Image.FillMethod.Horizontal;
            Phase4HudVisual visual = hud.gameObject.AddComponent<Phase4HudVisual>();
            visual.Configure(coordinator, timerFill, left, right, bottom);

            Image pauseBackground = pauseMenu.GetComponentInChildren<Image>(true);
            if (pauseBackground != null) pauseBackground.color = new Color(0.018f, 0.032f, 0.058f, 0.97f);
            Button[] buttons = pauseMenu.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                ColorBlock colors = buttons[i].colors;
                colors.normalColor = new Color(0.08f, 0.14f, 0.22f, 1f);
                colors.highlightedColor = new Color(0.08f, 0.55f, 0.72f, 1f);
                colors.selectedColor = new Color(0.08f, 0.55f, 0.72f, 1f);
                colors.pressedColor = new Color(1f, 0.48f, 0.08f, 1f);
                buttons[i].colors = colors;
                buttons[i].gameObject.AddComponent<Phase4UiAudio>().Configure(audio);
            }
        }

        private static RectTransform Panel(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = pivot;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            Image image = obj.AddComponent<Image>();
            image.color = new Color(0.018f, 0.032f, 0.058f, 0.86f);
            image.raycastTarget = false;
            return rect;
        }

        private static Transform CompensatedRoot(Transform parent, string name)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            Vector3 scale = parent.localScale;
            root.localScale = new Vector3(
                Mathf.Abs(scale.x) > 0.0001f ? 1f / scale.x : 1f,
                Mathf.Abs(scale.y) > 0.0001f ? 1f / scale.y : 1f,
                Mathf.Abs(scale.z) > 0.0001f ? 1f / scale.z : 1f);
            return root;
        }

        private static void DisableRootAndMarker(Transform root)
        {
            Renderer renderer = root.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].name.EndsWith(" Marker", StringComparison.Ordinal))
                    renderers[i].enabled = false;
            }
        }

        private static GameObject Visual(string name, PrimitiveType type, Transform parent,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            Object.DestroyImmediate(obj.GetComponent<Collider>());
            obj.GetComponent<Renderer>().sharedMaterial = material;
            return obj;
        }

        private static Light Light(Transform parent, string name, Vector3 localPosition)
        {
            GameObject obj = new GameObject(name); obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            Light light = obj.AddComponent<Light>(); light.type = LightType.Point;
            light.shadows = LightShadows.None; light.renderMode = LightRenderMode.Auto;
            return light;
        }
    }
}
