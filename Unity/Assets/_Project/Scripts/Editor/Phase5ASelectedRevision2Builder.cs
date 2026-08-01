using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EchoShift.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift.Editor
{
    public static partial class Phase5AIdentityPreviewBuilder
    {
        public const int Revision2CaptureCount = 11;
        public const int Revision2CueCount = 8;
        public const string Revision2ArtRoot =
            "Assets/_Project/Art/Phase5A/SelectedRevision2";
        public const string Revision2ScenePath =
            "Assets/_Project/Scenes/Preview/Phase5A_SelectedRevision2.unity";

        private const string Revision2CaptureFolder = "SelectedRevision2";

        private static readonly string[] Revision2CaptureNames =
        {
            "01_logo_wordmark.png",
            "02_logo_64px_test.png",
            "03_echo_chamber_front.png",
            "04_echo_chamber_isometric.png",
            "05_player_echo_integrated_motif.png",
            "06_goal_locked_unlocked.png",
            "07_maintenance_bay.png",
            "08_observation_bay.png",
            "09_decal_surface_closeup.png",
            "10_section3_overview.png",
            "11_ui_copy.png"
        };

        private static readonly string[] Revision2CueNames =
        {
            "echo_spawn_01.wav",
            "echo_spawn_02.wav",
            "echo_spawn_03.wav",
            "echo_remove.wav",
            "loop_end.wav",
            "interaction_success.wav",
            "battery_insert.wav",
            "section_complete.wav"
        };

        public static IReadOnlyList<string> Revision2Captures => Revision2CaptureNames;
        public static IReadOnlyList<string> Revision2Cues => Revision2CueNames;

        [MenuItem("ECHO SHIFT/Phase 5A/Generate Selected Revision 2")]
        public static void GenerateSelectedRevision2()
        {
            Phase4VisualSettings settings =
                AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(SettingsPath);
            Phase4ExternalAssetCatalog catalog =
                AssetDatabase.LoadAssetAtPath<Phase4ExternalAssetCatalog>(CatalogPath);
            GameObject echoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath);
            if (settings == null || !settings.HasRequiredReferences)
                throw new InvalidOperationException("Phase 4 visual settings are missing or incomplete.");
            if (catalog == null || !catalog.IsComplete)
                throw new InvalidOperationException("Phase 4 external asset catalog is missing or incomplete.");
            if (echoPrefab == null)
                throw new InvalidOperationException("P3 Echo prefab is missing.");

            EnsureAssetFolder(Revision2ArtRoot);
            EnsureAssetFolder(SceneRoot);
            Material[] materials = BuildRevision2Materials();
            WriteRevision2LogoSources();
            WriteRevision2DecalAtlas();
            WriteRevision2ProductionHashManifest();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Phase5A Selected Revision 2 Preview");
            BuildRevision2World(root.transform, settings, catalog, echoPrefab, materials,
                out Revision2PreviewReferences preview);
            EditorSceneManager.SaveScene(scene, Revision2ScenePath);

            string repositoryRoot = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            if (string.IsNullOrEmpty(repositoryRoot))
                throw new InvalidOperationException("Repository root could not be resolved.");
            string captureDirectory = Path.Combine(repositoryRoot, "Captures", "Phase5A",
                Revision2CaptureFolder);
            string audioDirectory = Path.Combine(captureDirectory, "Audio");
            Directory.CreateDirectory(audioDirectory);
            Cursor.visible = false;
            RenderRevision2(preview, captureDirectory);
            WriteRevision2Audio(audioDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateRevision2(captureDirectory, audioDirectory);
            Debug.Log($"PHASE5A_SELECTED_REVISION2_OK captures={Revision2CaptureCount};" +
                      $"cues={Revision2CueCount};graphics={SystemInfo.graphicsDeviceType}");
        }

        public static void GenerateSelectedRevision2FromCommandLine()
        {
            GenerateSelectedRevision2();
        }

        private static void BuildRevision2World(Transform root, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog, GameObject echoPrefab, Material[] materials,
            out Revision2PreviewReferences preview)
        {
            Material dark = materials[0];
            Material panel = materials[1];
            Material metal = materials[2];
            Material violet = materials[3];
            Material cyan = materials[4];
            Material amber = materials[5];
            Material lime = materials[6];
            Material white = materials[8];

            Transform logo = Child(root, "Revision 2 Wordmark Gallery");
            BuildRevision2LogoGallery(logo, violet, cyan, white,
                settings.PackagedJapaneseFont, false);
            Transform logo64 = Child(root, "Revision 2 64px Gallery");
            BuildRevision2LogoGallery(logo64, violet, cyan, white,
                settings.PackagedJapaneseFont, true);

            Transform environment = Child(root, "Revision 2 Functional Section 3 Preview");
            Transform shell = BuildRevision2Foundation(environment, catalog, dark, panel, metal,
                violet);
            Transform maintenance = BuildRevision2MaintenanceBay(environment, catalog, dark,
                panel, metal, violet, amber, white, settings.PackagedJapaneseFont);
            Transform observation = BuildRevision2ObservationBay(environment, catalog, dark,
                panel, metal, violet, cyan, white, settings.PackagedJapaneseFont);
            Transform chamber = Child(environment, "Revision 2 Echo Chamber");
            chamber.localPosition = new Vector3(4.1f, 0f, -5.35f);
            BuildRevision2Chamber(chamber, dark, panel, metal, violet, cyan, white,
                settings.PackagedJapaneseFont);

            Transform gameplay = Child(environment, "Revision 2 Preview Gameplay");
            Transform actors = Child(gameplay, "Revision 2 Surface Integrated Actors");
            BuildRevision2Actors(actors, echoPrefab, violet, cyan);
            Transform devices = Child(gameplay, "Revision 2 Preview Devices");
            BuildRevision2Devices(devices, catalog, dark, panel, metal, violet, cyan, amber,
                lime, white, settings.PackagedJapaneseFont);

            Transform goalGallery = Child(root, "Revision 2 Goal State Gallery");
            BuildRevision2GoalGallery(goalGallery, dark, panel, metal, violet, lime, white,
                settings.PackagedJapaneseFont);

            Camera camera = BuildSelectedCamera(root, Hex("07101A"));
            BuildRevision2Lights(root, violet, cyan, amber, white);
            Canvas hud = BuildSelectedHud(root, camera, panel, violet, cyan, amber, white,
                settings.PackagedJapaneseFont);

            preview = new Revision2PreviewReferences(camera, logo.gameObject, logo64.gameObject,
                environment.gameObject, shell.gameObject, maintenance.gameObject,
                observation.gameObject, chamber.gameObject, gameplay.gameObject,
                actors.gameObject, devices.gameObject, goalGallery.gameObject, hud);
            preview.DisableAll();
        }

        private static void BuildRevision2LogoGallery(Transform root, Material violet,
            Material cyan, Material white, Font font, bool sizeTest)
        {
            if (!sizeTest)
            {
                Transform wordmark = Child(root, "Background Independent ECHO SHIFT Wordmark");
                BuildRevision2Wordmark(wordmark, new Vector3(0f, 2.35f, 0f), 1f, violet, cyan,
                    white, font);
                Transform icon = Child(root, "Independent Split Seal Icon");
                icon.localPosition = new Vector3(0f, 0.65f, 0f);
                BuildRevision2VerticalSeal(icon, 0.48f, 0f, 0f, 5, violet, 0.105f, 0.12f);
                SelectedWorldText("Icon O", "O", icon, new Vector3(0f, 0f, -0.08f), 0.42f,
                    white.color, font, TextAnchor.MiddleCenter);
                BuildRevision2SurfaceTicks(icon, "Icon Phase Index", new Vector3(0.72f, 0f, 0f),
                    3, violet, true);
                return;
            }

            Transform actual = Child(root, "Actual 64 Pixel Equivalent");
            actual.localPosition = new Vector3(0f, 3.8f, 0f);
            actual.localScale = Vector3.one * 0.19f;
            BuildRevision2Wordmark(actual, Vector3.zero, 1f, violet, cyan, white,
                font);
            SelectedWorldText("64 Pixel Label", "64 PX EQUIVALENT", root,
                new Vector3(0f, 3.52f, 0f), 0.16f, Color.Lerp(white.color, cyan.color, 0.25f),
                font, TextAnchor.MiddleCenter);
            Transform zoom = Child(root, "Four Times Inspection View");
            zoom.localPosition = new Vector3(0f, 1.65f, 0f);
            zoom.localScale = Vector3.one * 0.76f;
            BuildRevision2Wordmark(zoom, Vector3.zero, 1f, violet, cyan, white, font);
        }

        private static void BuildRevision2Wordmark(Transform root, Vector3 center, float scale,
            Material violet, Material cyan, Material white, Font font)
        {
            root.localPosition = center;
            root.localScale = Vector3.one * scale;
            SelectedWorldText("Wordmark ECH", "ECH", root, new Vector3(-2.55f, 0f, 0f),
                0.92f, white.color, font, TextAnchor.MiddleCenter);
            SelectedWorldText("Readable Wordmark O", "O", root, new Vector3(-0.98f, 0f, 0f),
                0.92f, white.color, font, TextAnchor.MiddleCenter);
            Transform seal = Child(root, "O Outer Split Seal");
            seal.localPosition = new Vector3(-0.98f, 0f, -0.02f);
            BuildRevision2VerticalSeal(seal, 0.52f, 0f, 0f, 5, violet, 0.095f, 0.11f);
            SelectedWorldText("Canonical Divider", "//", root, new Vector3(0.08f, 0f, 0f),
                0.86f, cyan.color, font, TextAnchor.MiddleCenter);
            SelectedWorldText("Wordmark SHIFT", "SHIFT", root, new Vector3(2.15f, 0f, 0f),
                0.92f, white.color, font, TextAnchor.MiddleCenter);
        }

        private static Transform BuildRevision2Foundation(Transform root,
            Phase4ExternalAssetCatalog catalog, Material dark, Material panel, Material metal,
            Material violet)
        {
            Transform shell = Child(root, "Revision 2 Connected Facility Shell");
            Visual("Continuous Facility Foundation", PrimitiveType.Cube, shell,
                new Vector3(0f, -0.38f, 1f), new Vector3(14.4f, 0.72f, 25.8f), dark);
            for (int z = 0; z < 8; z++)
            {
                float localZ = -9.5f + z * 3f;
                Visual($"Quiet Central Route {z + 1:00}", PrimitiveType.Cube, shell,
                    new Vector3(0f, 0.02f, localZ), new Vector3(4.2f, 0.08f, 2.82f), panel);
                int leftVariant = (z + 1) % catalog.FloorModules.Length;
                int rightVariant = (z * 2 + 3) % catalog.FloorModules.Length;
                PrefabVisual($"Maintenance Floor Module {z + 1:00}",
                    catalog.FloorModules[leftVariant], shell,
                    new Vector3(-4.55f, 0.04f, localZ), Quaternion.identity,
                    new Vector3(0.98f, 1f, 0.94f));
                PrefabVisual($"Observation Floor Module {z + 1:00}",
                    catalog.FloorModules[rightVariant], shell,
                    new Vector3(4.55f, 0.04f, localZ), Quaternion.identity,
                    new Vector3(0.98f, 1f, 0.94f));
            }

            Visual("West Continuous Wall", PrimitiveType.Cube, shell,
                new Vector3(-6.42f, 1.65f, 1f), new Vector3(0.34f, 3.3f, 24.6f), metal);
            Visual("East Continuous Wall", PrimitiveType.Cube, shell,
                new Vector3(6.42f, 1.65f, 1f), new Vector3(0.34f, 3.3f, 24.6f), metal);
            Visual("Rear Connected Wall", PrimitiveType.Cube, shell,
                new Vector3(0f, 1.65f, 11.35f), new Vector3(13.2f, 3.3f, 0.34f), metal);
            float[] westPanels = { -7.8f, -1.4f, 6.3f };
            float[] westWidths = { 3.8f, 5.1f, 3.2f };
            for (int i = 0; i < westPanels.Length; i++)
                Visual($"West Integrated Wall Bay {i + 1}", PrimitiveType.Cube, shell,
                    new Vector3(-6.22f, 1.5f, westPanels[i]),
                    new Vector3(0.06f, 2.55f, westWidths[i]), panel);
            float[] eastPanels = { -6.7f, 1.1f, 7.9f };
            float[] eastWidths = { 4.7f, 3.5f, 3.1f };
            for (int i = 0; i < eastPanels.Length; i++)
                Visual($"East Integrated Wall Bay {i + 1}", PrimitiveType.Cube, shell,
                    new Vector3(6.22f, 1.5f, eastPanels[i]),
                    new Vector3(0.06f, 2.55f, eastWidths[i]), panel);
            BuildRevision2SurfaceTicks(shell, "Rear Facility Phase Index",
                new Vector3(0f, 2.75f, 11.15f), 3, violet, false);
            return shell;
        }

        private static Transform BuildRevision2MaintenanceBay(Transform root,
            Phase4ExternalAssetCatalog catalog, Material dark, Material panel, Material metal,
            Material violet, Material amber, Material white, Font font)
        {
            Transform bay = Child(root, "Revision 2 Maintenance Bay Equipment Cluster");
            Visual("Maintenance Grounded Work Zone", PrimitiveType.Cube, bay,
                new Vector3(-4.65f, 0.1f, 2.7f), new Vector3(3.15f, 0.16f, 5.6f), dark);
            Visual("Maintenance Entry Wall", PrimitiveType.Cube, bay,
                new Vector3(-4.65f, 1.55f, 5.25f), new Vector3(3.25f, 3.1f, 0.26f), metal);
            SelectedWorldText("Maintenance Entrance Decal", "MAINTENANCE", bay,
                new Vector3(-4.65f, 2.48f, 5.1f), 0.25f,
                new Color(violet.color.r, violet.color.g, violet.color.b, 0.82f), font,
                TextAnchor.MiddleCenter);

            Transform rack = Child(bay, "Wall Connected Battery Rack Assembly");
            rack.localPosition = new Vector3(-4.85f, 0f, 4.85f);
            Visual("Rack Backplate", PrimitiveType.Cube, rack, new Vector3(0f, 1.15f, 0f),
                new Vector3(2.45f, 2.3f, 0.22f), panel);
            Visual("Rack Top Beam", PrimitiveType.Cube, rack, new Vector3(0f, 2.25f, -0.18f),
                new Vector3(2.55f, 0.16f, 0.38f), metal);
            Visual("Rack Left Beam", PrimitiveType.Cube, rack, new Vector3(-1.15f, 1.12f, -0.18f),
                new Vector3(0.16f, 2.25f, 0.38f), metal);
            Visual("Rack Right Beam", PrimitiveType.Cube, rack, new Vector3(1.15f, 1.12f, -0.18f),
                new Vector3(0.16f, 2.25f, 0.38f), metal);
            for (int i = 0; i < 3; i++)
            {
                float x = -0.72f + i * 0.72f;
                Visual($"Charging Slot {i + 1}", PrimitiveType.Cube, rack,
                    new Vector3(x, 1.15f, -0.24f), new Vector3(0.56f, 1.55f, 0.16f), dark);
                Visual($"Charging Contact {i + 1}", PrimitiveType.Cube, rack,
                    new Vector3(x, 0.48f, -0.34f), new Vector3(0.34f, 0.06f, 0.04f), amber);
            }
            SelectedWorldText("Battery Bay Rack Decal", "BATTERY BAY", rack,
                new Vector3(0f, 2.02f, -0.32f), 0.18f,
                new Color(amber.color.r, amber.color.g, amber.color.b, 0.86f), font,
                TextAnchor.MiddleCenter);
            SelectedWorldText("Rack Equipment Serial", "ES-RACK-03-17", rack,
                new Vector3(-0.72f, 0.18f, -0.32f), 0.09f,
                new Color(white.color.r, white.color.g, white.color.b, 0.58f), font,
                TextAnchor.MiddleCenter);

            Transform conduit = Child(bay, "Wall Attached Cable Conduit");
            Visual("Conduit Horizontal", PrimitiveType.Cube, conduit,
                new Vector3(-5.55f, 2.7f, 5.05f), new Vector3(1.35f, 0.13f, 0.13f), panel);
            Visual("Conduit Vertical", PrimitiveType.Cube, conduit,
                new Vector3(-5.92f, 1.65f, 5.05f), new Vector3(0.13f, 2.0f, 0.13f), panel);
            Visual("Conduit Amber Junction", PrimitiveType.Cube, conduit,
                new Vector3(-5.92f, 0.75f, 5.0f), new Vector3(0.25f, 0.25f, 0.18f), amber);

            GameObject service = PrefabVisual("Grounded Maintenance Service Unit",
                catalog.PropModules[1], bay, new Vector3(-5.25f, 0.62f, 1.15f),
                Quaternion.identity, new Vector3(0.78f, 0.88f, 0.78f));
            SelectedWorldText("Service Unit Inspected Stamp", "INSPECTED", service.transform,
                new Vector3(0.18f, 0.6f, -0.55f), 0.11f,
                new Color(white.color.r, white.color.g, white.color.b, 0.62f), font,
                TextAnchor.MiddleCenter);
            PrefabVisual("Maintenance Tool Box", catalog.PropModules[2], bay,
                new Vector3(-3.9f, 0.38f, 1.2f), Quaternion.Euler(0f, -12f, 0f),
                new Vector3(0.58f, 0.58f, 0.58f));
            PrefabVisual("Wall Integrated Maintenance Console", catalog.PropModules[3], bay,
                new Vector3(-3.7f, 0.62f, 4.5f), Quaternion.Euler(0f, 180f, 0f),
                new Vector3(0.72f, 0.78f, 0.72f));

            Visual("Maintenance Boundary Long", PrimitiveType.Cube, bay,
                new Vector3(-3.4f, 0.11f, 2.7f), new Vector3(0.055f, 0.025f, 5.2f), amber);
            Visual("Maintenance Boundary Short", PrimitiveType.Cube, bay,
                new Vector3(-4.72f, 0.11f, 0.15f), new Vector3(2.7f, 0.025f, 0.055f), amber);
            return bay;
        }

        private static Transform BuildRevision2ObservationBay(Transform root,
            Phase4ExternalAssetCatalog catalog, Material dark, Material panel, Material metal,
            Material violet, Material cyan, Material white, Font font)
        {
            Transform bay = Child(root, "Revision 2 Observation Bay Equipment Cluster");
            Visual("Observation Grounded Work Zone", PrimitiveType.Cube, bay,
                new Vector3(4.7f, 0.1f, 0.8f), new Vector3(3.0f, 0.16f, 5.2f), dark);
            Transform window = Child(bay, "Chamber Facing Observation Frame");
            Visual("Observation Frame Left", PrimitiveType.Cube, window,
                new Vector3(5.75f, 1.55f, -1.4f), new Vector3(0.24f, 2.8f, 0.34f), metal);
            Visual("Observation Frame Right", PrimitiveType.Cube, window,
                new Vector3(3.65f, 1.55f, -1.4f), new Vector3(0.24f, 2.8f, 0.34f), metal);
            Visual("Observation Frame Top", PrimitiveType.Cube, window,
                new Vector3(4.7f, 2.82f, -1.4f), new Vector3(2.34f, 0.24f, 0.34f), metal);
            Visual("Observation Lower Sill", PrimitiveType.Cube, window,
                new Vector3(4.7f, 0.52f, -1.4f), new Vector3(2.34f, 0.26f, 0.34f), panel);
            Visual("Observation Glass Recess", PrimitiveType.Cube, window,
                new Vector3(4.7f, 1.62f, -1.29f), new Vector3(1.8f, 1.78f, 0.04f), dark);

            Transform monitor = Child(bay, "Wall Embedded Record Monitor");
            Visual("Monitor Housing", PrimitiveType.Cube, monitor,
                new Vector3(5.2f, 1.55f, 2.92f), new Vector3(2.0f, 1.45f, 0.24f), metal);
            Visual("Monitor Recess", PrimitiveType.Cube, monitor,
                new Vector3(5.2f, 1.58f, 2.76f), new Vector3(1.7f, 1.15f, 0.04f), panel);
            SelectedWorldText("Record Status Decal", "RECORD STATUS", monitor,
                new Vector3(5.2f, 1.92f, 2.69f), 0.16f,
                new Color(violet.color.r, violet.color.g, violet.color.b, 0.84f), font,
                TextAnchor.MiddleCenter);
            for (int i = 0; i < 3; i++)
            {
                Visual($"Embedded Echo Generation Indicator {i + 1}", PrimitiveType.Cube,
                    monitor, new Vector3(4.72f + i * 0.48f, 1.45f, 2.68f),
                    new Vector3(0.3f, 0.07f, 0.03f), i == 2 ? cyan : violet);
                SelectedWorldText($"Generation Number {i + 1}", (i + 1).ToString(), monitor,
                    new Vector3(4.72f + i * 0.48f, 1.2f, 2.68f), 0.09f,
                    new Color(white.color.r, white.color.g, white.color.b, 0.64f), font,
                    TextAnchor.MiddleCenter);
            }
            SelectedWorldText("Observation Record Sector Decal", "RECORD SECTOR 03", bay,
                new Vector3(4.7f, 2.97f, -1.58f), 0.18f,
                new Color(white.color.r, white.color.g, white.color.b, 0.76f), font,
                TextAnchor.MiddleCenter);

            PrefabVisual("Wall Integrated Observation Console", catalog.PropModules[3], bay,
                new Vector3(4.55f, 0.63f, 2.1f), Quaternion.Euler(0f, 180f, 0f),
                new Vector3(0.75f, 0.82f, 0.75f));
            PrefabVisual("Grounded Observation Meter", catalog.PropModules[0], bay,
                new Vector3(5.45f, 0.58f, 0.2f), Quaternion.Euler(0f, 90f, 0f),
                new Vector3(0.62f, 0.72f, 0.62f));
            Visual("Observation Cable Termination", PrimitiveType.Cube, bay,
                new Vector3(5.95f, 0.22f, 2.05f), new Vector3(0.42f, 0.18f, 0.42f), panel);
            BuildRevision2SurfaceTicks(bay, "Observation Cable Status",
                new Vector3(5.95f, 0.34f, 2.05f), 3, violet, false);
            return bay;
        }

        private static void BuildRevision2Chamber(Transform root, Material dark, Material panel,
            Material metal, Material violet, Material cyan, Material white, Font font)
        {
            Visual("Floor Anchored Chamber Base", PrimitiveType.Cube, root,
                new Vector3(0f, 0.22f, 0f), new Vector3(3.7f, 0.42f, 2.2f), dark);
            Visual("Chamber Base Metal Cap", PrimitiveType.Cube, root,
                new Vector3(0f, 0.47f, 0f), new Vector3(3.35f, 0.16f, 1.82f), metal);
            Visual("Rear Structural Spine", PrimitiveType.Cube, root,
                new Vector3(0f, 2.05f, 0.42f), new Vector3(0.55f, 3.3f, 0.52f), metal);
            Visual("Rear Structural Top Brace", PrimitiveType.Cube, root,
                new Vector3(0f, 3.62f, 0.25f), new Vector3(1.75f, 0.28f, 0.45f), metal);

            Vector3 frameCenter = new Vector3(0f, 2.15f, 0f);
            BuildRevision2MetalHexFrame(root, frameCenter, 1.62f, metal, violet);
            BuildRevision2ConnectedBeam(root, "Left Base Frame Support",
                new Vector3(-1.35f, 0.5f, 0.18f), new Vector3(-1.4f, 1.33f, 0.12f),
                0.28f, metal);
            BuildRevision2ConnectedBeam(root, "Right Base Frame Support",
                new Vector3(1.35f, 0.5f, 0.18f), new Vector3(1.4f, 1.33f, 0.12f),
                0.28f, metal);
            BuildRevision2ConnectedBeam(root, "Rear Top Frame Connection",
                new Vector3(0f, 3.5f, 0.25f), new Vector3(0f, 3.78f, 0.08f),
                0.24f, metal);

            Transform holograms = Child(root, "Supported Internal Hologram Arcs");
            BuildRevision2HologramArc(holograms, "Hologram Arc 1", frameCenter, 1.08f,
                -145f, 70f, violet, 0.075f);
            BuildRevision2HologramArc(holograms, "Hologram Arc 2", frameCenter, 0.79f,
                -55f, 160f, violet, 0.065f);
            BuildRevision2HologramArc(holograms, "Hologram Arc 3", frameCenter, 0.52f,
                35f, 250f, cyan, 0.055f);

            Visual("Core Mechanical Support", PrimitiveType.Cube, root,
                new Vector3(0f, 1.22f, 0.15f), new Vector3(0.18f, 1.25f, 0.18f), metal);
            Visual("Core Dark Housing", PrimitiveType.Sphere, root,
                new Vector3(0f, 2.15f, -0.03f), new Vector3(0.58f, 0.58f, 0.42f), panel);
            Visual("Core Cyan Active Lens", PrimitiveType.Sphere, root,
                new Vector3(0f, 2.15f, -0.27f), new Vector3(0.22f, 0.22f, 0.08f), cyan);

            Transform indicators = Child(root, "Embedded Generation Indicators");
            Visual("Indicator Housing", PrimitiveType.Cube, indicators,
                new Vector3(1.73f, 2.15f, 0f), new Vector3(0.25f, 1.35f, 0.34f), metal);
            for (int i = 0; i < 3; i++)
                Visual($"Generation Indicator {i + 1}", PrimitiveType.Cube, indicators,
                    new Vector3(1.58f, 1.78f + i * 0.37f, -0.19f),
                    new Vector3(0.04f, 0.18f, 0.16f), i == 2 ? cyan : violet);
            SelectedWorldText("Chamber Equipment Serial", "ES-CM-03-042", root,
                new Vector3(-0.92f, 0.55f, -1.13f), 0.1f,
                new Color(white.color.r, white.color.g, white.color.b, 0.58f), font,
                TextAnchor.MiddleCenter);
        }

        private static void BuildRevision2Actors(Transform root, GameObject echoPrefab,
            Material violet, Material cyan)
        {
            GameObject player = BuildActor(root, echoPrefab, "Current Player Surface Motif",
                new Vector3(-0.6f, 0f, -2.25f), 5f, cyan, cyan,
                Phase4RobotPoseState.Idle, false, "Revision2");
            IntegrateRevision2ActorMotif(player.transform, cyan, 0, 3, false);
            GameObject echoOne = BuildActor(root, echoPrefab, "Echo 1 Surface Motif",
                new Vector3(1.25f, 0f, -1.3f), -14f, violet, violet,
                Phase4RobotPoseState.EchoStopped, true, "Revision2");
            IntegrateRevision2ActorMotif(echoOne.transform, violet, 1, 0, true);
            GameObject echoTwo = BuildActor(root, echoPrefab, "Echo 2 Surface Motif",
                new Vector3(-2.05f, 0f, 0.25f), 12f, violet, violet,
                Phase4RobotPoseState.Walk, true, "Revision2");
            IntegrateRevision2ActorMotif(echoTwo.transform, violet, 2, 2, true);
            GameObject echoThree = BuildActor(root, echoPrefab, "Echo 3 Surface Motif",
                new Vector3(1.95f, 0f, 1.0f), -20f, violet, violet,
                Phase4RobotPoseState.Walk, true, "Revision2");
            IntegrateRevision2ActorMotif(echoThree.transform, violet, 3, 4, true);
        }

        private static void IntegrateRevision2ActorMotif(Transform actor, Material material,
            int generation, int missingSegment, bool echo)
        {
            string[] protrudingNames =
            {
                "Generation Fin 1", "Generation Fin 2", "Generation Fin 3",
                "Replay Chevron Left", "Replay Chevron Right",
                "Echo Replaying Mark", "Echo Stopped Mark"
            };
            for (int i = 0; i < protrudingNames.Length; i++)
                SetNamedActive(actor, protrudingNames[i], false);

            Transform motif = Child(actor, echo ?
                $"Echo {generation} Body Integrated Identity" :
                "Current Player Body Integrated Identity");
            Visual("Chest Embedded Emission Strip", PrimitiveType.Cube, motif,
                new Vector3(0f, 1.28f, -0.61f), new Vector3(0.48f, 0.07f, 0.035f), material);
            Transform chestSeal = Child(motif, "Chest Surface Split Seal");
            chestSeal.localPosition = new Vector3(0f, 1.05f, -0.62f);
            BuildRevision2VerticalSeal(chestSeal, 0.16f, 0f, 0f, missingSegment, material,
                0.035f, 0.025f);
            Transform ticks = Child(motif, "Back Panel Surface Generation Ticks");
            int tickCount = echo ? generation : 1;
            for (int i = 0; i < tickCount; i++)
                Visual($"Embedded Surface Tick {i + 1}", PrimitiveType.Cube, ticks,
                    new Vector3((i - (tickCount - 1) * 0.5f) * 0.15f, 1.48f, 0.57f),
                    new Vector3(0.1f, 0.04f, 0.025f), material);
            Transform floorMarker = Child(motif, "Grounded Segmented Identity Marker");
            BuildRevision2HorizontalSeal(floorMarker, 0.68f, 0.035f,
                echo ? generation % 6 : 5, material, 0.045f, 0.025f);
        }

        private static void BuildRevision2Devices(Transform root,
            Phase4ExternalAssetCatalog catalog, Material dark, Material panel, Material metal,
            Material violet, Material cyan, Material amber, Material lime, Material white,
            Font font)
        {
            Transform plate = Child(root, "Revision 2 Pressure Plate");
            plate.localPosition = new Vector3(-2.05f, 0.04f, 0.25f);
            Visual("Plate Neutral Housing", PrimitiveType.Cube, plate, Vector3.zero,
                new Vector3(2.45f, 0.16f, 2.45f), dark);
            Visual("Plate Cyan Active Surface", PrimitiveType.Cube, plate,
                new Vector3(0f, 0.12f, 0f), new Vector3(1.95f, 0.08f, 1.95f), cyan);

            Transform battery = Child(root, "Revision 2 Amber Battery");
            battery.localPosition = new Vector3(-3.15f, 0.72f, 2.25f);
            GameObject core = Visual("Battery Amber Core", PrimitiveType.Cylinder, battery,
                Vector3.zero, new Vector3(0.44f, 0.43f, 0.44f), amber);
            core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject cap = Visual($"Battery Neutral End {side}", PrimitiveType.Cylinder,
                    battery, new Vector3(0f, 0f, side * 0.42f),
                    new Vector3(0.52f, 0.08f, 0.52f), metal);
                cap.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            Transform batterySeal = Child(battery, "Battery Small Surface Seal");
            batterySeal.localPosition = new Vector3(0f, 0.02f, -0.45f);
            BuildRevision2VerticalSeal(batterySeal, 0.16f, 0f, 0f, 5, amber, 0.035f, 0.025f);

            Transform socket = Child(root, "Revision 2 Connected Socket");
            socket.localPosition = new Vector3(2.25f, 0f, 3.35f);
            Visual("Socket Neutral Base", PrimitiveType.Cube, socket,
                new Vector3(0f, 0.18f, 0f), new Vector3(1.6f, 0.34f, 1.6f), dark);
            Visual("Socket Amber Interaction Surface", PrimitiveType.Cube, socket,
                new Vector3(0f, 0.38f, 0f), new Vector3(0.9f, 0.08f, 0.9f), amber);
            BuildRevision2SurfaceTicks(socket, "Connected Socket Success Index",
                new Vector3(0f, 0.46f, 0f), 3, lime, false);

            Transform door = Child(root, "Revision 2 Unlocked Split Door");
            door.localPosition = new Vector3(0f, 0f, 5.75f);
            PrefabVisual("Door Neutral Frame", catalog.DoorFrame, door,
                new Vector3(0f, 1.5f, 0f), Quaternion.identity, new Vector3(1.1f, 1f, 1f));
            GameObject left = PrefabVisual("Door Left Retracted Panel", catalog.DoorPanel, door,
                new Vector3(-1.55f, 1.5f, 0f), Quaternion.identity, new Vector3(0.48f, 1f, 1f));
            GameObject right = PrefabVisual("Door Right Retracted Panel", catalog.DoorPanel, door,
                new Vector3(1.55f, 1.5f, 0f), Quaternion.identity, new Vector3(0.48f, 1f, 1f));
            TintNamedRenderers(left, metal);
            TintNamedRenderers(right, metal);
            Transform doorSeal = Child(door, "Door Small Unlocked Seal");
            doorSeal.localPosition = new Vector3(0f, 0f, -0.2f);
            BuildRevision2VerticalSeal(doorSeal, 0.31f, 2.82f, 0f, -1, lime, 0.06f, 0.05f);
            SelectedWorldText("Door Record Sector Decal", "RECORD SECTOR 03", door,
                new Vector3(0f, 3.15f, -0.32f), 0.2f,
                new Color(white.color.r, white.color.g, white.color.b, 0.78f), font,
                TextAnchor.MiddleCenter);

            Transform goal = Child(root, "Revision 2 Section Goal Unlocked");
            goal.localPosition = new Vector3(0f, 0f, 9.8f);
            BuildRevision2Goal(goal, true, dark, panel, metal, violet, lime, white);
            Visual("Single Goal Guidance Line", PrimitiveType.Cube, goal,
                new Vector3(0f, 0.045f, -2.05f), new Vector3(0.055f, 0.025f, 1.3f), violet);
            SelectedWorldText("Goal Floor Label", "回収ポイント", goal,
                new Vector3(0f, 0.08f, 1.9f), 0.22f,
                new Color(white.color.r, white.color.g, white.color.b, 0.8f), font,
                TextAnchor.MiddleCenter).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void BuildRevision2GoalGallery(Transform root, Material dark,
            Material panel, Material metal, Material violet, Material lime, Material white,
            Font font)
        {
            Visual("Goal State Gallery Floor", PrimitiveType.Cube, root,
                new Vector3(0f, -0.12f, 0f), new Vector3(8.8f, 0.22f, 5.5f), dark);
            Transform locked = Child(root, "Locked Goal State");
            locked.localPosition = new Vector3(-2.15f, 0f, 0f);
            BuildRevision2Goal(locked, false, dark, panel, metal, violet, lime, white);
            Transform unlocked = Child(root, "Unlocked Goal State");
            unlocked.localPosition = new Vector3(2.15f, 0f, 0f);
            BuildRevision2Goal(unlocked, true, dark, panel, metal, violet, lime, white);
            SelectedWorldText("Locked State Label", "未解錠", root,
                new Vector3(-2.15f, 0.08f, 2.05f), 0.2f, white.color, font,
                TextAnchor.MiddleCenter).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            SelectedWorldText("Unlocked State Label", "解錠済み", root,
                new Vector3(2.15f, 0.08f, 2.05f), 0.2f, lime.color, font,
                TextAnchor.MiddleCenter).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void BuildRevision2Goal(Transform root, bool unlocked, Material dark,
            Material panel, Material metal, Material violet, Material lime, Material white)
        {
            Visual("Floor Embedded Goal Bed", PrimitiveType.Cube, root,
                new Vector3(0f, 0.025f, 0f), new Vector3(3.7f, 0.06f, 3.25f), dark);
            BuildRevision2HorizontalSeal(root, 1.48f, 0.07f, 5, metal, 0.16f, 0.08f);
            Transform outerSignal = Child(root, "Goal Outer Identity Signal");
            BuildRevision2HorizontalSeal(outerSignal, 1.42f, 0.09f, 5, violet, 0.055f, 0.035f);
            Transform inner = Child(root, "Goal Inner Three Segments");
            for (int i = 0; i < 3; i++)
                Visual($"Goal Inner Segment {i + 1}", PrimitiveType.Cube, inner,
                    new Vector3((i - 1) * 0.58f, 0.1f, 0.2f),
                    new Vector3(0.42f, 0.025f, 0.11f), i == 1 ? white : violet);
            if (unlocked)
            {
                Transform completion = Child(root, "Unlocked Entrance Completion Segment");
                BuildRevision2HorizontalSeal(completion, 1.42f, 0.105f, -1, lime, 0.06f, 0.035f,
                    new[] { 5 });
            }
            else
            {
                Visual("Locked Entrance Gap Index", PrimitiveType.Cube, root,
                    new Vector3(0f, 0.1f, -1.22f), new Vector3(0.32f, 0.025f, 0.06f), panel);
            }
        }

        private static void BuildRevision2Lights(Transform root, Material violet, Material cyan,
            Material amber, Material white)
        {
            GameObject sunObject = new GameObject("Revision 2 Neutral Key Light");
            sunObject.transform.SetParent(root, false);
            sunObject.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = white.color;
            sun.intensity = 0.95f;
            sun.shadows = LightShadows.Soft;
            PointLight(root, "Revision 2 Chamber Violet Light", new Vector3(4.1f, 4.1f, -5.3f),
                violet.color, 6.5f, 1.1f);
            PointLight(root, "Revision 2 Player Cyan Light", new Vector3(-0.4f, 3.2f, -1.8f),
                cyan.color, 5f, 0.75f);
            PointLight(root, "Revision 2 Maintenance Amber Light", new Vector3(-4.7f, 3f, 3f),
                amber.color, 5f, 0.65f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(Hex("07101A"), white.color, 0.18f);
            RenderSettings.reflectionIntensity = 0.35f;
        }

        private static void RenderRevision2(Revision2PreviewReferences preview,
            string outputDirectory)
        {
            Vector3[] positions =
            {
                new Vector3(0f, 3.0f, -13.8f),
                new Vector3(0f, 3.0f, -13.8f),
                new Vector3(4.1f, 3.4f, -12.0f),
                new Vector3(9.0f, 5.8f, -10.0f),
                new Vector3(-6.0f, 4.3f, -6.2f),
                new Vector3(6.5f, 5.1f, -5.2f),
                new Vector3(-1.25f, 4.0f, -0.7f),
                new Vector3(1.25f, 4.0f, -2.25f),
                new Vector3(-2.45f, 2.75f, -0.2f),
                new Vector3(14.2f, 22.5f, -13.4f),
                new Vector3(10.2f, 10.4f, -5.4f)
            };
            Vector3[] targets =
            {
                new Vector3(0f, 2.1f, 0f),
                new Vector3(0f, 2.1f, 0f),
                new Vector3(4.1f, 2.0f, -5.35f),
                new Vector3(4.1f, 1.9f, -5.35f),
                new Vector3(0f, 1.0f, -0.65f),
                new Vector3(0f, 0.1f, 0f),
                new Vector3(-4.65f, 1.25f, 3.35f),
                new Vector3(4.75f, 1.45f, 0.75f),
                new Vector3(-4.75f, 1.45f, 4.35f),
                new Vector3(0.4f, 0.35f, 0.4f),
                new Vector3(0f, 0.9f, 1.2f)
            };
            float[] fieldsOfView = { 35f, 35f, 37f, 39f, 38f, 40f, 40f, 40f, 36f, 50f, 44f };
            for (int i = 0; i < Revision2CaptureNames.Length; i++)
            {
                preview.ConfigureForCapture(i);
                preview.Camera.transform.position = positions[i];
                preview.Camera.transform.rotation = Quaternion.LookRotation(
                    targets[i] - positions[i], Vector3.up);
                preview.Camera.fieldOfView = fieldsOfView[i];
                Render(preview.Camera, Path.Combine(outputDirectory, Revision2CaptureNames[i]));
            }
            preview.DisableAll();
        }

        private static void WriteRevision2LogoSources()
        {
            const string violet = "8B72D6";
            const string cyan = "5FD7E8";
            const string white = "D8E3EA";

            StringBuilder horizontal = new StringBuilder(3072);
            horizontal.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"320\" viewBox=\"0 0 1024 320\">");
            horizontal.Append($"<text x=\"42\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">ECH</text>");
            horizontal.Append($"<text x=\"315\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\">O</text>");
            AppendSelectedSealSvg(horizontal, 362, 161, 62, violet, 5, 11);
            horizontal.Append($"<text x=\"447\" y=\"205\" fill=\"#{cyan}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"102\" font-weight=\"800\">//</text>");
            horizontal.Append($"<text x=\"555\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">SHIFT</text>");
            AppendSelectedSvgTicks(horizontal, 420, 114, violet);
            horizontal.Append("</svg>");
            WriteRevision2Svg("P5A_R2_Logo_Wordmark.svg", horizontal);

            StringBuilder icon = new StringBuilder(1536);
            icon.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"512\" height=\"512\" viewBox=\"0 0 512 512\">");
            icon.Append($"<text x=\"128\" y=\"350\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"280\" font-weight=\"800\">O</text>");
            AppendSelectedSealSvg(icon, 256, 256, 146, violet, 5, 24);
            AppendSelectedSvgTicks(icon, 407, 211, violet);
            icon.Append("</svg>");
            WriteRevision2Svg("P5A_R2_Logo_Icon.svg", icon);

            StringBuilder monochrome = new StringBuilder(3072);
            monochrome.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"320\" viewBox=\"0 0 1024 320\">");
            monochrome.Append($"<text x=\"42\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">ECH</text>");
            monochrome.Append($"<text x=\"315\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\">O</text>");
            AppendSelectedSealSvg(monochrome, 362, 161, 62, white, 5, 11);
            monochrome.Append($"<text x=\"447\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"102\" font-weight=\"800\">//</text>");
            monochrome.Append($"<text x=\"555\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">SHIFT</text>");
            AppendSelectedSvgTicks(monochrome, 420, 114, white);
            monochrome.Append("</svg>");
            WriteRevision2Svg("P5A_R2_Logo_Monochrome.svg", monochrome);
        }

        private static void WriteRevision2DecalAtlas()
        {
            StringBuilder svg = new StringBuilder(12288);
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"1024\" viewBox=\"0 0 1024 1024\">");
            for (int i = 0; i < SelectedDecalValues.Length; i++)
            {
                int column = i % 3;
                int row = i / 3;
                int x = column * 341;
                int y = row * 204;
                string color = i == 7 ? "E8AE46" : i == 8 ? "D8E3EA" :
                    i == 4 || i == 11 ? "B75A64" : i % 3 == 0 ? "8B72D6" : "9EB4C3";
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<g id=\"r2-decal-{0:00}\" opacity=\"0.78\">", i + 1);
                AppendSelectedSealSvg(svg, x + 42, y + 52, 23, color, i % 6, 5);
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<text x=\"{0}\" y=\"{1}\" fill=\"#{2}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"{3}\" font-weight=\"700\" letter-spacing=\"1\">{4}</text>",
                    x + 22, y + 130, color, SelectedDecalValues[i].Length > 18 ? 18 : 23,
                    EscapeXml(SelectedDecalValues[i]));
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<path d=\"M{0} {1}h{2}\" stroke=\"#{3}\" stroke-width=\"3\"/></g>",
                    x + 22, y + 151, 190 + (i % 3) * 25, color);
            }
            svg.Append("</svg>");
            WriteRevision2Svg("P5A_R2_SurfaceDecalAtlas.svg", svg);
        }

        private static void WriteRevision2ProductionHashManifest()
        {
            string[] scenePaths =
            {
                "Assets/_Project/Scenes/P0_ReplayLab.unity",
                "Assets/_Project/Scenes/P1_InteractionLab.unity",
                "Assets/_Project/Scenes/P2_CoordinationLab.unity",
                "Assets/_Project/Scenes/P3_PlayableGreybox.unity"
            };
            StringBuilder manifest = new StringBuilder(512);
            for (int i = 0; i < scenePaths.Length; i++)
            {
                manifest.Append(scenePaths[i]);
                manifest.Append('|');
                manifest.Append(Sha256Revision2Asset(scenePaths[i]));
                manifest.Append('\n');
            }
            string assetPath = $"{Revision2ArtRoot}/P5A_R2_ProductionHashes.txt";
            File.WriteAllText(AbsoluteAssetPath(assetPath), manifest.ToString(),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static string Sha256Revision2Asset(string assetPath)
        {
            using (SHA256 algorithm = SHA256.Create())
            using (FileStream stream = File.OpenRead(AbsoluteAssetPath(assetPath)))
            {
                byte[] hash = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        private static void WriteRevision2Svg(string fileName, StringBuilder svg)
        {
            string assetPath = $"{Revision2ArtRoot}/{fileName}";
            File.WriteAllText(AbsoluteAssetPath(assetPath), svg.ToString(),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void WriteRevision2Audio(string audioDirectory)
        {
            const int sampleRate = 44100;
            AudioClip tick = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioRoot + "laserSmall_000.ogg");
            AudioClip field = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioRoot + "forceField_000.ogg");
            AudioClip impact = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioRoot + "impactMetal_001.ogg");
            if (tick == null || field == null || impact == null)
                throw new InvalidOperationException("Required Kenney source audio is missing.");

            float[] tickData = ReadMono(tick);
            float[] fieldData = ReadMono(field);
            float[] impactData = ReadMono(impact);
            float[][] cues =
            {
                CreateRevision2Cue(0, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(1, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(2, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(3, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(4, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(5, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(6, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency),
                CreateRevision2Cue(7, sampleRate, tickData, tick.frequency, fieldData,
                    field.frequency, impactData, impact.frequency)
            };
            for (int i = 0; i < cues.Length; i++)
                WritePcm16Wave(Path.Combine(audioDirectory, Revision2CueNames[i]), cues[i],
                    sampleRate);

            float[] starts = { 0.3f, 1.2f, 2.35f, 3.7f, 4.75f, 6.15f, 7.0f, 8.1f };
            float[] preview = new float[sampleRate * 10];
            StringBuilder timeline = new StringBuilder(1024);
            timeline.AppendLine("# Selected Revision 2 Audio Preview Timeline");
            timeline.AppendLine();
            timeline.AppendLine("| Cue | Start | End | Intent |");
            timeline.AppendLine("|---|---:|---:|---|");
            string[] intents =
            {
                "single split-seal pulse", "two-step generation index", "three-step generation index",
                "descending archive release", "loop closure and rewind", "short interaction acknowledgement",
                "mechanical battery lock", "three-part section completion cadence"
            };
            for (int i = 0; i < cues.Length; i++)
            {
                MixRevision2Cue(cues[i], preview, Mathf.RoundToInt(starts[i] * sampleRate));
                float end = starts[i] + cues[i].Length / (float)sampleRate;
                timeline.AppendFormat(CultureInfo.InvariantCulture,
                    "| `{0}` | {1:0.00}s | {2:0.00}s | {3} |\n",
                    Revision2CueNames[i], starts[i], end, intents[i]);
            }
            Normalize(preview, 0.86f);
            WritePcm16Wave(Path.Combine(audioDirectory, "revised_audio_preview_v2.wav"),
                preview, sampleRate);
            File.WriteAllText(Path.Combine(audioDirectory, "audio_preview_timeline.md"),
                timeline.ToString(), new UTF8Encoding(false));
        }

        private static float[] CreateRevision2Cue(int index, int sampleRate, float[] tick,
            int tickRate, float[] field, int fieldRate, float[] impact, int impactRate)
        {
            float[] cue = new float[Mathf.RoundToInt(sampleRate *
                new[] { 0.65f, 0.85f, 1.05f, 0.85f, 1.2f, 0.42f, 0.8f, 1.75f }[index])];
            switch (index)
            {
                case 0:
                    MixResampled(tick, tickRate, cue, sampleRate, 0.03f, 1f, 0.42f);
                    MixResampled(field, fieldRate, cue, sampleRate, 0.05f, 1.16f, 0.08f);
                    break;
                case 1:
                    MixResampled(tick, tickRate, cue, sampleRate, 0.03f, 1f, 0.34f);
                    MixResampled(field, fieldRate, cue, sampleRate, 0.05f, 1.12f, 0.07f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.28f, 1.28f, 0.31f);
                    AddDelay(cue, sampleRate, 0.06f, 0.08f);
                    break;
                case 2:
                    MixResampled(tick, tickRate, cue, sampleRate, 0.03f, 1f, 0.3f);
                    MixResampled(field, fieldRate, cue, sampleRate, 0.05f, 1.08f, 0.06f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.25f, 1.27f, 0.27f);
                    MixResampled(impact, impactRate, cue, sampleRate, 0.27f, 1.4f, 0.05f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.48f, 1.52f, 0.24f);
                    break;
                case 3:
                    MixResampled(impact, impactRate, cue, sampleRate, 0.04f, 0.9f, 0.14f);
                    AddReverseTail(field, fieldRate, cue, sampleRate, 0.08f, 0.08f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.22f, 0.78f, 0.24f);
                    ApplyOnePoleLowPass(cue, 0.34f);
                    break;
                case 4:
                    MixResampled(tick, tickRate, cue, sampleRate, 0.04f, 1f, 0.28f);
                    MixResampled(field, fieldRate, cue, sampleRate, 0.07f, 0.86f, 0.12f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.44f, 0.76f, 0.22f);
                    ApplyOnePoleLowPass(cue, 0.42f);
                    AddDelay(cue, sampleRate, 0.12f, 0.12f);
                    break;
                case 5:
                    MixResampled(tick, tickRate, cue, sampleRate, 0.02f, 1.32f, 0.34f);
                    MixResampled(impact, impactRate, cue, sampleRate, 0.02f, 1.6f, 0.05f);
                    break;
                case 6:
                    MixResampled(impact, impactRate, cue, sampleRate, 0.03f, 1.02f, 0.22f);
                    MixResampled(field, fieldRate, cue, sampleRate, 0.06f, 1.18f, 0.1f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.28f, 1.36f, 0.24f);
                    break;
                default:
                    MixResampled(impact, impactRate, cue, sampleRate, 0.03f, 0.72f, 0.12f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.06f, 1f, 0.25f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.33f, 1.26f, 0.23f);
                    MixResampled(tick, tickRate, cue, sampleRate, 0.62f, 1.5f, 0.22f);
                    MixResampled(field, fieldRate, cue, sampleRate, 0.72f, 0.7f, 0.1f);
                    ApplyOnePoleLowPass(cue, 0.38f);
                    AddDelay(cue, sampleRate, 0.11f, 0.12f);
                    break;
            }
            Normalize(cue, index == 5 ? 0.8f : 0.84f);
            return cue;
        }

        private static void MixRevision2Cue(float[] source, float[] target, int start)
        {
            int count = Mathf.Min(source.Length, target.Length - start);
            for (int i = 0; i < count; i++) target[start + i] += source[i];
        }

        private static void ValidateRevision2(string captureDirectory, string audioDirectory)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Revision2ScenePath) == null)
                throw new InvalidOperationException("Revision 2 preview Scene was not saved.");
            string[] sourceAssets =
            {
                "P5A_R2_Logo_Wordmark.svg", "P5A_R2_Logo_Icon.svg",
                "P5A_R2_Logo_Monochrome.svg", "P5A_R2_SurfaceDecalAtlas.svg",
                "P5A_R2_ProductionHashes.txt"
            };
            for (int i = 0; i < sourceAssets.Length; i++)
                if (!File.Exists(AbsoluteAssetPath($"{Revision2ArtRoot}/{sourceAssets[i]}")))
                    throw new InvalidOperationException($"Revision 2 source asset is missing: {sourceAssets[i]}");
            for (int i = 0; i < Revision2CaptureNames.Length; i++)
            {
                string path = Path.Combine(captureDirectory, Revision2CaptureNames[i]);
                if (!File.Exists(path) || new FileInfo(path).Length < 30000)
                    throw new InvalidOperationException($"Revision 2 capture is missing: {path}");
                byte[] png = File.ReadAllBytes(path);
                if (ReadBigEndianInt32(png, 16) != CaptureWidth ||
                    ReadBigEndianInt32(png, 20) != CaptureHeight)
                    throw new InvalidOperationException($"Revision 2 capture size is invalid: {path}");
            }
            for (int i = 0; i < Revision2CueNames.Length; i++)
                ValidateRevision2Wave(Path.Combine(audioDirectory, Revision2CueNames[i]), 2f);
            ValidateRevision2Wave(Path.Combine(audioDirectory, "revised_audio_preview_v2.wav"),
                MaximumAudioSeconds);
            if (!File.Exists(Path.Combine(audioDirectory, "audio_preview_timeline.md")))
                throw new InvalidOperationException("Revision 2 audio preview timeline is missing.");
        }

        private static void ValidateRevision2Wave(string path, float maximumSeconds)
        {
            if (!File.Exists(path) || new FileInfo(path).Length <= 44)
                throw new InvalidOperationException($"Revision 2 audio is missing: {path}");
            byte[] data = File.ReadAllBytes(path);
            double seconds = (data.Length - 44d) / (44100d * 2d);
            if (seconds <= 0d || seconds > maximumSeconds)
                throw new InvalidOperationException($"Revision 2 audio duration is invalid: {path}");
            float peak = 0f;
            for (int offset = 44; offset + 1 < data.Length; offset += 2)
            {
                short value = (short)(data[offset] | data[offset + 1] << 8);
                peak = Mathf.Max(peak, Mathf.Abs(value / 32768f));
            }
            if (peak < 0.1f || peak > 0.9f)
                throw new InvalidOperationException($"Revision 2 audio peak is invalid: {path} ({peak:0.000}).");
        }

        private static Material[] BuildRevision2Materials()
        {
            return new[]
            {
                MaterialAsset(Revision2ArtRoot, "P5A_R2_Dark", Hex("07101A"), 0.55f, 0.48f, 0f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_Panel", Hex("142534"), 0.5f, 0.45f, 0f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_Metal", Hex("3C4B58"), 0.72f, 0.52f, 0f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_Violet", Hex("8B72D6"), 0.2f, 0.44f, 0.46f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_Cyan", Hex("5FD7E8"), 0.18f, 0.42f, 0.44f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_Amber", Hex("E8AE46"), 0.24f, 0.42f, 0.32f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_LimeSuccess", Hex("8AD55E"), 0.18f, 0.42f, 0.44f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_RedClosed", Hex("C5525A"), 0.2f, 0.42f, 0.35f),
                MaterialAsset(Revision2ArtRoot, "P5A_R2_White", Hex("D8E3EA"), 0.1f, 0.38f, 0.12f)
            };
        }

        private static void BuildRevision2VerticalSeal(Transform parent, float radius,
            float centerY, float z, int missingSegment, Material material, float thickness,
            float depth)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == missingSegment) continue;
                Vector2 start = HexVertex(radius, i);
                Vector2 end = HexVertex(radius, i + 1);
                Vector2 midpoint = (start + end) * 0.5f;
                Vector2 edge = end - start;
                GameObject segment = Visual($"Seal Segment {i + 1}", PrimitiveType.Cube, parent,
                    new Vector3(midpoint.x, centerY + midpoint.y, z),
                    new Vector3(edge.magnitude * 0.88f, thickness, depth), material);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(edge.y, edge.x) * Mathf.Rad2Deg);
            }
        }

        private static void BuildRevision2HorizontalSeal(Transform parent, float radius, float y,
            int missingSegment, Material material, float thickness, float height,
            int[] includedSegments = null)
        {
            for (int i = 0; i < 6; i++)
            {
                if (includedSegments == null && i == missingSegment) continue;
                if (includedSegments != null && Array.IndexOf(includedSegments, i) < 0) continue;
                Vector2 start = HexVertex(radius, i);
                Vector2 end = HexVertex(radius, i + 1);
                Vector2 midpoint = (start + end) * 0.5f;
                Vector2 edge = end - start;
                GameObject segment = Visual($"Floor Seal Segment {i + 1}", PrimitiveType.Cube,
                    parent, new Vector3(midpoint.x, y, midpoint.y),
                    new Vector3(edge.magnitude * 0.88f, height, thickness), material);
                segment.transform.localRotation = Quaternion.Euler(0f,
                    -Mathf.Atan2(edge.y, edge.x) * Mathf.Rad2Deg, 0f);
            }
        }

        private static Vector2 HexVertex(float radius, int index)
        {
            float angle = (30f + index * 60f) * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private static void BuildRevision2MetalHexFrame(Transform parent, Vector3 center,
            float radius, Material metal, Material violet)
        {
            Transform frame = Child(parent, "Connected Metal Outer Frame");
            for (int i = 0; i < 6; i++)
            {
                Vector2 start2 = HexVertex(radius, i);
                Vector2 end2 = HexVertex(radius, i + 1);
                Vector3 start = center + new Vector3(start2.x, start2.y, 0f);
                Vector3 end = center + new Vector3(end2.x, end2.y, 0f);
                BuildRevision2ConnectedBeam(frame, $"Metal Outer Frame Segment {i + 1}",
                    start, end, 0.24f, metal);
                if (i != 5)
                    BuildRevision2ConnectedBeam(frame, $"Violet Frame Inset {i + 1}",
                        start + Vector3.back * 0.17f, end + Vector3.back * 0.17f,
                        0.055f, violet);
            }
        }

        private static void BuildRevision2ConnectedBeam(Transform parent, string name,
            Vector3 start, Vector3 end, float thickness, Material material)
        {
            Vector3 direction = end - start;
            GameObject beam = Visual(name, PrimitiveType.Cube, parent, (start + end) * 0.5f,
                new Vector3(direction.magnitude, thickness, thickness), material);
            beam.transform.localRotation = Quaternion.FromToRotation(Vector3.right, direction);
        }

        private static void BuildRevision2HologramArc(Transform parent, string name,
            Vector3 center, float radius, float startDegrees, float endDegrees, Material material,
            float thickness)
        {
            Transform arc = Child(parent, name);
            const int segmentCount = 4;
            float step = (endDegrees - startDegrees) / segmentCount;
            for (int i = 0; i < segmentCount; i++)
            {
                float a0 = (startDegrees + step * i) * Mathf.Deg2Rad;
                float a1 = (startDegrees + step * (i + 1)) * Mathf.Deg2Rad;
                Vector3 start = center + new Vector3(Mathf.Cos(a0) * radius,
                    Mathf.Sin(a0) * radius, -0.2f);
                Vector3 end = center + new Vector3(Mathf.Cos(a1) * radius,
                    Mathf.Sin(a1) * radius, -0.2f);
                BuildRevision2ConnectedBeam(arc, $"Hologram Segment {i + 1}", start, end,
                    thickness, material);
            }
        }

        private static void BuildRevision2SurfaceTicks(Transform parent, string name,
            Vector3 position, int count, Material material, bool vertical)
        {
            Transform ticks = Child(parent, name);
            ticks.localPosition = position;
            for (int i = 0; i < count; i++)
                Visual($"Embedded Tick {i + 1}", PrimitiveType.Cube, ticks,
                    vertical ? new Vector3(0f, (i - (count - 1) * 0.5f) * 0.23f, 0f) :
                        new Vector3((i - (count - 1) * 0.5f) * 0.23f, 0f, 0f),
                    vertical ? new Vector3(0.17f, 0.045f, 0.035f) :
                        new Vector3(0.17f, 0.035f, 0.045f), material);
        }

        private sealed class Revision2PreviewReferences
        {
            public Revision2PreviewReferences(Camera camera, GameObject logo, GameObject logo64,
                GameObject environment, GameObject shell, GameObject maintenance,
                GameObject observation, GameObject chamber, GameObject gameplay,
                GameObject actors, GameObject devices, GameObject goalGallery, Canvas hud)
            {
                Camera = camera;
                Logo = logo;
                Logo64 = logo64;
                Environment = environment;
                Shell = shell;
                Maintenance = maintenance;
                Observation = observation;
                Chamber = chamber;
                Gameplay = gameplay;
                Actors = actors;
                Devices = devices;
                GoalGallery = goalGallery;
                Hud = hud;
            }

            public Camera Camera { get; }
            public GameObject Logo { get; }
            public GameObject Logo64 { get; }
            public GameObject Environment { get; }
            public GameObject Shell { get; }
            public GameObject Maintenance { get; }
            public GameObject Observation { get; }
            public GameObject Chamber { get; }
            public GameObject Gameplay { get; }
            public GameObject Actors { get; }
            public GameObject Devices { get; }
            public GameObject GoalGallery { get; }
            public Canvas Hud { get; }

            public void DisableAll()
            {
                Logo.SetActive(false);
                Logo64.SetActive(false);
                Environment.SetActive(false);
                GoalGallery.SetActive(false);
                Hud.gameObject.SetActive(false);
            }

            public void ConfigureForCapture(int index)
            {
                DisableAll();
                if (index == 0) { Logo.SetActive(true); return; }
                if (index == 1) { Logo64.SetActive(true); return; }
                if (index == 5) { GoalGallery.SetActive(true); return; }

                Environment.SetActive(true);
                Shell.SetActive(true);
                Maintenance.SetActive(index == 6 || index == 8 || index == 9 || index == 10);
                Observation.SetActive(index == 7 || index == 9 || index == 10);
                Chamber.SetActive(index == 2 || index == 3 || index == 7 || index == 9 || index == 10);
                Gameplay.SetActive(index == 4 || index == 9 || index == 10);
                Actors.SetActive(index == 4 || index == 9 || index == 10);
                Devices.SetActive(index == 9 || index == 10);
                Hud.gameObject.SetActive(index == 10);
            }
        }
    }
}
