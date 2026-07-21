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
        private const int ConnectedPerimeterWallVariantCount = 2;
        private const int SolidPerimeterColumnVariantCount = 2;
        private const float PerimeterWallX = 6.42f;
        private const float PerimeterWallBackingX = 6.58f;
        private const float PerimeterColumnX = 6.24f;

        public static void PolishEchoPrefab(string prefabPath, Phase4VisualSettings settings)
        {
            Phase4ExternalAssetCatalog catalog = LoadExternalCatalog();
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            Transform old = root.transform.Find("P4 Robot Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Renderer rootRenderer = root.GetComponent<Renderer>();
            if (rootRenderer != null) rootRenderer.enabled = false;
            EchoPlayback playback = root.GetComponent<EchoPlayback>();
            LoopActor actor = root.GetComponent<LoopActor>();
            DisablePersistentWorldLabels(root.transform);
            BuildActorVisual(root, actor, playback, settings, catalog, false);
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

            Phase4ExternalAssetCatalog catalog = LoadExternalCatalog();

            for (int i = 0; i < sections.Length; i++)
            {
                DisablePersistentWorldLabels(sections[i].transform);
                BuildEnvironment(sections[i], i + 1, settings, catalog);
                BuildDevices(sections[i], settings, catalog);
                PlayerSimulation player = sections[i].Player;
                Renderer playerRenderer = player.GetComponent<Renderer>();
                if (playerRenderer != null) playerRenderer.enabled = false;
                BuildActorVisual(player.gameObject, player.GetComponent<LoopActor>(), null,
                    settings, catalog, true);
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
            PuzzleSectionController section, int number, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog)
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
                    GameObject floorModule = catalog.FloorModules[(x + z) % catalog.FloorModules.Length];
                    PrefabVisual($"External Floor {x + 1}-{z + 1}", floorModule, root,
                        new Vector3((x - 1) * 4.55f, 0.025f, localZ), Quaternion.identity,
                        new Vector3(0.98f, 1f, Mathf.Max(0.55f, (rowDepth - 0.08f) / 3f)));
                }
            }

            int wallRows = Mathf.CeilToInt(length / 3.8f);
            float wallDepth = length / wallRows;
            // A continuous project-owned backing makes every external panel/trim a wall
            // cladding piece instead of a standalone mesh against the black clear color.
            Visual("West Perimeter Wall Backing", PrimitiveType.Cube, root,
                new Vector3(-PerimeterWallBackingX, 1.5f, 0f),
                new Vector3(0.38f, 3f, length), settings.FacilityDarkMaterial);
            Visual("East Perimeter Wall Backing", PrimitiveType.Cube, root,
                new Vector3(PerimeterWallBackingX, 1.5f, 0f),
                new Vector3(0.38f, 3f, length), settings.FacilityDarkMaterial);
            for (int side = -1; side <= 1; side += 2)
            for (int z = 0; z < wallRows; z++)
            {
                float localZ = -length * 0.5f + wallDepth * (z + 0.5f);
                // Only the first two wrappers are full-height wall panels. The remaining
                // variants are trims or partial rails and read as floating geometry when
                // repeated along the open perimeter.
                GameObject wallModule = catalog.WallModules[(z + (side > 0 ? 1 : 0)) %
                    ConnectedPerimeterWallVariantCount];
                PrefabVisual($"External Wall {side}-{z}", wallModule, root,
                    new Vector3(side * PerimeterWallX, 1.5f, localZ),
                    Quaternion.Euler(0f, 90f, 0f),
                    new Vector3(Mathf.Max(0.6f, wallDepth / 4f), 1f, 1f));
                if ((z & 1) == 0)
                {
                    // Column 03 is a diagonal metal support intended to lean against a
                    // larger structure. It has no valid anchor in this top-down room shell.
                    GameObject column = catalog.ColumnModules[(z / 2) %
                        SolidPerimeterColumnVariantCount];
                    PrefabVisual($"External Column {side}-{z}", column, root,
                        new Vector3(side * PerimeterColumnX, 1.5f,
                            localZ - wallDepth * 0.42f),
                        Quaternion.Euler(0f, side > 0 ? -90f : 90f, 0f), Vector3.one);
                }
                if ((z & 1) == 0)
                    BuildMachineBank(root, side * 6.15f, localZ, side, settings, catalog,
                        (z + number) % catalog.PropModules.Length);
            }

            BuildBulkhead(root, -length * 0.5f, settings, catalog);
            BuildBulkhead(root, length * 0.5f, settings, catalog);
            Visual($"Section {number} Identity Strip", PrimitiveType.Cube, root,
                new Vector3(0f, 0.12f, -length * 0.5f + 0.75f),
                new Vector3(8.2f, 0.025f, 0.12f), settings.GetEchoMaterial(number));

            Light left = Light(root, $"Section {number} Fill A", new Vector3(-4.5f, 2.65f, -length * 0.22f));
            left.color = new Color(0.62f, 0.8f, 1f); left.intensity = 4.2f; left.range = 10f;
            Light right = Light(root, $"Section {number} Fill B", new Vector3(4.5f, 2.65f, length * 0.22f));
            right.color = new Color(1f, 0.7f, 0.46f); right.intensity = 3.6f; right.range = 10f;

            LineRenderer[] wires = section.GetComponentsInChildren<LineRenderer>(true);
            for (int i = 0; i < wires.Length; i++)
            {
                wires[i].widthMultiplier = 0.2f;
                wires[i].numCornerVertices = 6;
                wires[i].numCapVertices = 4;
            }
        }

        private static void BuildMachineBank(
            Transform root, float x, float z, int side, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog, int propIndex)
        {
            Transform bank = new GameObject("Facility Machine Bank").transform;
            bank.SetParent(root, false);
            bank.localPosition = new Vector3(x, 0f, z);
            PrefabVisual("External Facility Prop", catalog.PropModules[propIndex], bank,
                new Vector3(0f, 0.7f, 0f), Quaternion.Euler(0f, side > 0 ? -90f : 90f, 0f),
                new Vector3(0.75f, 1f, 0.75f));
            for (int i = -1; i <= 1; i++)
                Visual($"Status {i}", PrimitiveType.Cube, bank,
                    new Vector3(-side * 0.64f, 1.05f, i * 0.3f),
                    new Vector3(0.03f, 0.08f, 0.18f), i == 0
                        ? settings.PlayerMaterial : settings.PlateMaterial);
        }

        private static void BuildBulkhead(Transform root, float z, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog)
        {
            PrefabVisual($"Bulkhead West {z}", catalog.ColumnModules[0], root,
                new Vector3(-6.55f, 1.5f, z), Quaternion.identity, new Vector3(1.15f, 1f, 1.15f));
            PrefabVisual($"Bulkhead East {z}", catalog.ColumnModules[0], root,
                new Vector3(6.55f, 1.5f, z), Quaternion.identity, new Vector3(1.15f, 1f, 1.15f));
            Visual($"Bulkhead Floor Guide {z}", PrimitiveType.Cube, root,
                new Vector3(0f, 0.11f, z), new Vector3(9.2f, 0.025f, 0.16f),
                settings.FacilityTrimMaterial);
        }

        private static void BuildDevices(PuzzleSectionController section, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog)
        {
            PressurePlate[] plates = section.GetComponentsInChildren<PressurePlate>(true);
            for (int i = 0; i < plates.Length; i++) BuildPlate(plates[i], settings);
            CarryableBattery[] batteries = section.GetComponentsInChildren<CarryableBattery>(true);
            for (int i = 0; i < batteries.Length; i++) BuildBattery(batteries[i], settings);
            PowerSocket[] sockets = section.GetComponentsInChildren<PowerSocket>(true);
            for (int i = 0; i < sockets.Length; i++) BuildSocket(sockets[i], settings);
            DoorController[] doors = section.GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < doors.Length; i++) BuildDoor(doors[i], settings, catalog);
            GoalVolume[] goals = section.GetComponentsInChildren<GoalVolume>(true);
            for (int i = 0; i < goals.Length; i++) BuildGoal(goals[i], settings, catalog);
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

        private static void BuildDoor(DoorController door, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog)
        {
            DisableRootAndMarker(door.transform);
            Transform root = CompensatedRoot(door.transform, "P4 Door Panel Visual");
            GameObject panel = PrefabVisual("External Door Moving Panel", catalog.DoorPanel,
                root, Vector3.zero, Quaternion.identity, Vector3.one);
            Renderer panelRenderer = panel.GetComponentInChildren<Renderer>(true);
            GameObject centerLine = Visual("Door Closed Center Emission", PrimitiveType.Cube, root,
                new Vector3(0f, 0f, -0.24f), new Vector3(0.1f, 2.28f, 0.055f),
                settings.DangerMaterial);
            bool plateDoor = door.name.IndexOf("Plate", StringComparison.OrdinalIgnoreCase) >= 0;
            Material circuit = plateDoor ? settings.PlateMaterial : settings.BatteryMaterial;
            int symbolBars = plateDoor ? 1 : 2;
            for (int i = 0; i < symbolBars; i++)
                Visual($"Door Circuit Symbol {i + 1}", PrimitiveType.Cube, root,
                    new Vector3((i - (symbolBars - 1) * 0.5f) * 0.3f, 0.75f, -0.27f),
                    new Vector3(0.14f, 0.38f, 0.055f), circuit);
            Transform frame = new GameObject("P4 Doorway Frame").transform;
            frame.SetParent(door.transform.parent, false);
            frame.localPosition = door.transform.localPosition;
            PrefabVisual("External Door Frame", catalog.DoorFrame, frame,
                Vector3.zero, Quaternion.identity, Vector3.one);
            for (int side = -1; side <= 1; side += 2)
                Visual($"Door Open Edge {side}", PrimitiveType.Cube, frame,
                    new Vector3(side * 2.12f, 0f, -0.36f), new Vector3(0.075f, 2.72f, 0.07f),
                    circuit);
            DoorVisualFeedback visual = door.GetComponent<DoorVisualFeedback>();
            visual.Configure(door, panelRenderer, centerLine.GetComponent<Renderer>(), settings);
        }

        private static void BuildGoal(GoalVolume goal, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog)
        {
            WorldBillboardLabel sourceBillboard =
                goal.GetComponentInChildren<WorldBillboardLabel>(true);
            DisableRootAndMarker(goal.transform);
            Transform root = CompensatedRoot(goal.transform, "P4 Goal Portal");
            PrefabVisual("External Goal Frame", catalog.DoorFrame, root,
                new Vector3(0f, 1.55f, 0f), Quaternion.identity, new Vector3(0.78f, 0.9f, 0.8f));
            GameObject beam = Visual("Portal Rear Glow", PrimitiveType.Cube, root,
                new Vector3(0f, 1.42f, 0.32f), new Vector3(2.95f, 2.45f, 0.055f),
                settings.GoalMaterial);
            for (int side = -1; side <= 1; side += 2)
                Visual($"Goal Floor Guide {side}", PrimitiveType.Cube, root,
                    new Vector3(side * 0.82f, 0.015f, -2.2f), new Vector3(0.12f, 0.025f, 4.2f),
                    settings.GoalMaterial);

            GameObject particleObject = new GameObject("Goal Vertical Particles");
            particleObject.transform.SetParent(root, false);
            particleObject.transform.localPosition = new Vector3(0f, 0.15f, 0.2f);
            ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true; main.playOnAwake = true; main.startLifetime = 1.7f;
            main.startSpeed = 1.2f; main.startSize = 0.07f; main.maxParticles = 28;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 8f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.4f, 0.05f, 0.25f);
            ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial = settings.GoalMaterial;

            GameObject exitObject = new GameObject("出口ゲート Sign");
            exitObject.transform.SetParent(root, false);
            exitObject.transform.localPosition = new Vector3(0f, 3.25f, 0f);
            TextMesh exit = exitObject.AddComponent<TextMesh>();
            exit.text = "出口ゲート"; exit.font = settings.PackagedJapaneseFont;
            exit.fontSize = 48; exit.characterSize = 0.045f;
            exit.anchor = TextAnchor.MiddleCenter; exit.alignment = TextAlignment.Center;
            exit.color = settings.GoalColor;
            if (sourceBillboard != null) exitObject.transform.rotation = sourceBillboard.WorldRotation;
            Phase4GoalVisual visual = goal.gameObject.AddComponent<Phase4GoalVisual>();
            visual.Configure(goal, beam.transform, settings);
        }

        private static void BuildActorVisual(
            GameObject root, LoopActor actor, EchoPlayback playback,
            Phase4VisualSettings settings, Phase4ExternalAssetCatalog catalog, bool player)
        {
            Transform old = root.transform.Find("P4 Robot Visual");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Transform model = new GameObject("P4 Robot Visual").transform;
            model.SetParent(root.transform, false);
            Material accent = player ? settings.PlayerMaterial : settings.GetEchoMaterial(1);
            GameObject robot = PrefabVisual("Quaternius Robot Model", catalog.RobotVisual,
                model, Vector3.zero, Quaternion.Euler(0f, 180f, 0f), Vector3.one);
            Renderer[] bodies = robot.GetComponentsInChildren<Renderer>(true);
            Transform rightArm = new GameObject("Carry Pose Reference").transform;
            rightArm.SetParent(model, false);
            rightArm.localPosition = new Vector3(0.48f, 0.12f, 0.05f);
            Renderer[] accents = new Renderer[3];
            accents[0] = Visual("Compact Visor", PrimitiveType.Cube, model,
                new Vector3(0f, 0.55f, 0.57f), new Vector3(0.42f, 0.09f, 0.055f),
                accent).GetComponent<Renderer>();
            accents[1] = Visual("Chest Time Core", PrimitiveType.Cylinder, model,
                new Vector3(0f, 0.08f, 0.48f), new Vector3(0.17f, 0.035f, 0.17f),
                accent).GetComponent<Renderer>();
            accents[1].transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            accents[2] = Visual("Shoulder Time Band", PrimitiveType.Cube, model,
                new Vector3(0f, 0.22f, -0.36f), new Vector3(0.58f, 0.1f, 0.08f),
                accent).GetComponent<Renderer>();
            GameObject[] marks = new GameObject[3];
            for (int i = 0; i < marks.Length; i++)
            {
                marks[i] = Visual($"Generation Fin {i + 1}", PrimitiveType.Cube, model,
                    new Vector3((i - 1) * 0.18f, 0.97f, -0.05f),
                    new Vector3(0.08f, 0.2f + i * 0.045f, 0.1f), accent);
                marks[i].transform.localRotation = Quaternion.Euler(0f, 0f, (i - 1) * 13f);
            }
            Visual(player ? "Player Floor Ring" : "Echo Floor Ring", PrimitiveType.Cylinder,
                model, new Vector3(0f, -0.94f, 0f), new Vector3(0.72f, 0.02f, 0.72f), accent);
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
                directional.color = new Color(0.86f, 0.91f, 1f);
                directional.intensity = 1.32f;
                directional.shadows = LightShadows.Soft;
                directional.shadowStrength = 0.58f;
            }
            Camera camera = cameraController.GetComponent<Camera>();
            camera.allowHDR = true;
            camera.backgroundColor = new Color(0.055f, 0.07f, 0.105f);
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            GameObject volumeObject = new GameObject("Phase 4 Global Volume");
            volumeObject.transform.SetParent(systems, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true; volume.priority = 10f; volume.sharedProfile = settings.VolumeProfile;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.42f, 0.48f, 0.58f);
            RenderSettings.ambientEquatorColor = new Color(0.2f, 0.25f, 0.34f);
            RenderSettings.ambientGroundColor = new Color(0.09f, 0.11f, 0.15f);
            RenderSettings.ambientIntensity = 1.15f;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.07f, 0.09f, 0.13f);
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
                new Vector2(480f, 292f), new Vector2(0f, 1f));
            RectTransform right = Panel(canvas, "P4 Tutorial Panel",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -18f),
                new Vector2(760f, 250f), new Vector2(1f, 1f));
            RectTransform bottom = Panel(canvas, "P4 Prompt Panel",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 18f),
                new Vector2(700f, 88f), new Vector2(0.5f, 0f));
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
            CanvasGroup gameplayGroup = hud.GetComponent<CanvasGroup>();
            if (gameplayGroup == null) gameplayGroup = hud.gameObject.AddComponent<CanvasGroup>();
            Phase4HudVisual visual = hud.gameObject.AddComponent<Phase4HudVisual>();
            visual.Configure(coordinator, timerFill, left, right, bottom, pauseMenu, gameplayGroup);

            Transform pausePanel = pauseMenu.transform.Find("Pause Panel");
            if (pausePanel == null) throw new InvalidOperationException("Pause Panel is missing.");
            RectTransform pauseRect = (RectTransform)pausePanel;
            pauseRect.anchorMin = Vector2.zero; pauseRect.anchorMax = Vector2.one;
            pauseRect.offsetMin = pauseRect.offsetMax = Vector2.zero;
            Image pauseBackground = pausePanel.GetComponent<Image>();
            pauseBackground.color = new Color(0.008f, 0.014f, 0.026f, 0.84f);
            GameObject cardObject = new GameObject("P4 Pause Card");
            cardObject.transform.SetParent(pausePanel, false);
            RectTransform card = cardObject.AddComponent<RectTransform>();
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(760f, 720f);
            Image cardImage = cardObject.AddComponent<Image>();
            cardImage.color = new Color(0.035f, 0.065f, 0.105f, 0.985f);
            Outline cardOutline = cardObject.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0.08f, 0.7f, 0.9f, 0.78f);
            cardOutline.effectDistance = new Vector2(2f, -2f);
            Transform[] pauseChildren = new Transform[pausePanel.childCount - 1];
            int childIndex = 0;
            for (int i = 0; i < pausePanel.childCount; i++)
            {
                Transform child = pausePanel.GetChild(i);
                if (child != card) pauseChildren[childIndex++] = child;
            }
            for (int i = 0; i < pauseChildren.Length; i++) pauseChildren[i].SetParent(card, false);

            Text title = card.GetComponentInChildren<Text>(true);
            if (title != null && title.name == "Title")
                title.rectTransform.anchoredPosition = new Vector2(0f, 280f);
            Button[] buttons = pauseMenu.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform buttonRect = (RectTransform)buttons[i].transform;
                buttonRect.sizeDelta = new Vector2(560f, 72f);
                buttonRect.anchoredPosition = new Vector2(0f,
                    i == 0 ? 145f : i == 1 ? 45f : i == 2 ? -55f : -205f);
                ColorBlock colors = buttons[i].colors;
                colors.normalColor = new Color(0.08f, 0.14f, 0.22f, 1f);
                colors.highlightedColor = new Color(0.08f, 0.55f, 0.72f, 1f);
                colors.selectedColor = new Color(0.08f, 0.55f, 0.72f, 1f);
                colors.pressedColor = new Color(1f, 0.48f, 0.08f, 1f);
                buttons[i].colors = colors;
                buttons[i].gameObject.AddComponent<Phase4UiAudio>().Configure(audio);
                Outline border = buttons[i].gameObject.AddComponent<Outline>();
                border.effectColor = i == buttons.Length - 1
                    ? new Color(1f, 0.32f, 0.18f, 1f)
                    : new Color(0.1f, 0.86f, 1f, 1f);
                border.effectDistance = new Vector2(3f, -3f);
                GameObject arrow = new GameObject("Selection Arrow");
                arrow.transform.SetParent(buttons[i].transform, false);
                RectTransform arrowRect = arrow.AddComponent<RectTransform>();
                arrowRect.anchorMin = arrowRect.anchorMax = new Vector2(0f, 0.5f);
                arrowRect.pivot = new Vector2(0.5f, 0.5f);
                arrowRect.anchoredPosition = new Vector2(36f, 0f);
                arrowRect.sizeDelta = new Vector2(40f, 48f);
                Text arrowText = arrow.AddComponent<Text>();
                arrowText.font = settings.PackagedJapaneseFont;
                arrowText.fontSize = 28; arrowText.alignment = TextAnchor.MiddleCenter;
                arrowText.text = "▶"; arrowText.color = border.effectColor;
                buttons[i].gameObject.AddComponent<Phase4PauseButtonVisual>()
                    .Configure(border, arrow);
            }
            GameObject separator = new GameObject("Quit Separator");
            separator.transform.SetParent(card, false);
            RectTransform separatorRect = separator.AddComponent<RectTransform>();
            separatorRect.anchorMin = separatorRect.anchorMax = new Vector2(0.5f, 0.5f);
            separatorRect.anchoredPosition = new Vector2(0f, -130f);
            separatorRect.sizeDelta = new Vector2(560f, 2f);
            Image separatorImage = separator.AddComponent<Image>();
            separatorImage.color = new Color(1f, 0.32f, 0.18f, 0.62f);
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

        private static void DisablePersistentWorldLabels(Transform root)
        {
            WorldBillboardLabel[] labels = root.GetComponentsInChildren<WorldBillboardLabel>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                Renderer renderer = labels[i].GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].name.EndsWith(" Marker", StringComparison.Ordinal))
                    renderers[i].enabled = false;
            }
        }

        private static Phase4ExternalAssetCatalog LoadExternalCatalog()
        {
            Phase4ExternalAssetCatalog catalog =
                AssetDatabase.LoadAssetAtPath<Phase4ExternalAssetCatalog>(
                    Phase4ExternalAssetBuilder.CatalogPath);
            if (catalog == null || !catalog.IsComplete)
                throw new InvalidOperationException(
                    "Phase 4 external asset catalog has not been built or is incomplete.");
            return catalog;
        }

        private static GameObject PrefabVisual(string name, GameObject prefab, Transform parent,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (prefab == null) throw new InvalidOperationException($"Visual prefab is missing: {name}");
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            obj.name = name;
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
            obj.transform.localScale = scale;
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) Object.DestroyImmediate(colliders[i]);
            return obj;
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
