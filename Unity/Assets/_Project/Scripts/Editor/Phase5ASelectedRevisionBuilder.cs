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
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static partial class Phase5AIdentityPreviewBuilder
    {
        public const int SelectedCaptureCount = 7;
        public const int SelectedDecalCount = 15;
        public const string SelectedArtRoot = "Assets/_Project/Art/Phase5A/SelectedRevision";
        public const string SelectedScenePath =
            "Assets/_Project/Scenes/Preview/Phase5A_SelectedRevision.unity";

        private static readonly string[] SelectedCaptureNames =
        {
            "01_revised_logo.png",
            "02_simplified_echo_chamber.png",
            "03_player_echo_hierarchy.png",
            "04_door_battery_goal.png",
            "05_section3_overview.png",
            "06_ui_copy.png",
            "07_decal_closeup.png"
        };

        private static readonly string[] SelectedDecalValues =
        {
            "ECHO//SHIFT",
            "RECORD SECTOR 01",
            "RECORD SECTOR 02",
            "RECORD SECTOR 03",
            "TEMPORAL HAZARD",
            "ECHO TEST",
            "PHASE SYNC",
            "BATTERY BAY",
            "EXIT GATE",
            "MAINTENANCE",
            "INSPECTED",
            "OUT OF SERVICE",
            "ES-PHASE-05A-031",
            "ROUTE  ->",
            "INDEX 01 / 02 / --"
        };

        private static readonly string[] SelectedCopyValues =
        {
            "記録区画 03",
            "2体のエコーと協力して出口を開く",
            "記録 02",
            "残り 18.4",
            "エコー 2/3",
            "バッテリーを取得",
            "[E] 操作",
            "[E] 同期",
            "バッテリーを接続",
            "出口ゲート",
            "回収ポイント",
            "記録区画を突破",
            "実験完了"
        };

        public static IReadOnlyList<string> SelectedDecals => SelectedDecalValues;
        public static IReadOnlyList<string> SelectedCopy => SelectedCopyValues;

        [MenuItem("ECHO SHIFT/Phase 5A/Generate Selected Revision")]
        public static void GenerateSelectedRevision()
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
            if (echoPrefab == null) throw new InvalidOperationException("P3 Echo prefab is missing.");

            EnsureAssetFolder(SelectedArtRoot);
            EnsureAssetFolder(SceneRoot);
            Material[] materials = BuildSelectedMaterials();
            WriteSelectedLogoSources();
            WriteSelectedDecalAtlas();
            WriteSelectedProductionHashManifest();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Phase5A Selected Revision Preview");
            BuildSelectedRevisionWorld(root.transform, settings, catalog, echoPrefab, materials,
                out SelectedPreviewReferences preview);
            EditorSceneManager.SaveScene(scene, SelectedScenePath);

            string repositoryRoot = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            if (string.IsNullOrEmpty(repositoryRoot))
                throw new InvalidOperationException("Repository root could not be resolved.");
            string captureDirectory = Path.Combine(repositoryRoot, "Captures", "Phase5A",
                "SelectedRevision");
            Directory.CreateDirectory(captureDirectory);
            Cursor.visible = false;
            RenderSelectedRevision(preview, captureDirectory);
            WriteSelectedAudioPreview(Path.Combine(captureDirectory, "revised_audio_preview.wav"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateSelectedRevision(captureDirectory);
            Debug.Log($"PHASE5A_SELECTED_REVISION_OK captures={SelectedCaptureCount};" +
                      $"decals={SelectedDecalCount};graphics={SystemInfo.graphicsDeviceType}");
        }

        public static void GenerateSelectedRevisionFromCommandLine()
        {
            GenerateSelectedRevision();
        }

        private static void BuildSelectedRevisionWorld(
            Transform root, Phase4VisualSettings settings, Phase4ExternalAssetCatalog catalog,
            GameObject echoPrefab, Material[] materials, out SelectedPreviewReferences preview)
        {
            Material dark = materials[0];
            Material panel = materials[1];
            Material violet = materials[2];
            Material cyan = materials[3];
            Material amber = materials[4];
            Material lime = materials[5];
            Material white = materials[7];
            Material neutral = materials[8];
            Material logoDark = materials[9];
            Material logoPanel = materials[10];

            Transform logoGallery = Child(root, "Selected Logo Gallery");
            BuildSelectedLogoGallery(logoGallery, logoDark, logoPanel, violet, cyan, white,
                settings.PackagedJapaneseFont);

            Transform environment = Child(root, "Selected Functional Section 3 Preview");
            BuildSelectedFloor(environment, catalog, dark, panel, violet);
            Transform maintenance = BuildSelectedMaintenanceBay(environment, catalog, dark, panel,
                violet, neutral, amber, white, settings.PackagedJapaneseFont);
            BuildSelectedObservationBay(environment, catalog, dark, panel, violet, cyan, white,
                settings.PackagedJapaneseFont);
            Transform chamber = Child(environment, "Selected Echo Chamber");
            chamber.localPosition = new Vector3(4.2f, 0f, -5.5f);
            BuildSimplifiedSelectedChamber(chamber, dark, violet, cyan, white);
            BuildSelectedOrthogonalShell(environment, neutral, violet);

            Transform gameplay = Child(environment, "Selected Preview Actors and Devices");
            GameObject player = BuildActor(gameplay, echoPrefab, "Current Player Preview",
                new Vector3(-0.6f, 0f, -2.25f), 5f, cyan, cyan,
                Phase4RobotPoseState.Idle, false, "Selected");
            BuildSelectedActorCrest(player.transform, cyan, 0, 3, false);
            GameObject echoOne = BuildActor(gameplay, echoPrefab, "Echo 1 Preview",
                new Vector3(1.25f, 0f, -1.3f), -14f, violet, violet,
                Phase4RobotPoseState.EchoStopped, true, "Selected");
            BuildSelectedActorCrest(echoOne.transform, violet, 1, 0, true);
            GameObject echoTwo = BuildActor(gameplay, echoPrefab, "Echo 2 Preview",
                new Vector3(-2.05f, 0f, 0.25f), 12f, violet, violet,
                Phase4RobotPoseState.Walk, true, "Selected");
            BuildSelectedActorCrest(echoTwo.transform, violet, 2, 2, true);
            GameObject echoThree = BuildActor(gameplay, echoPrefab, "Echo 3 Preview",
                new Vector3(1.95f, 0f, 1.0f), -20f, violet, violet,
                Phase4RobotPoseState.Walk, true, "Selected");
            BuildSelectedActorCrest(echoThree.transform, violet, 3, 4, true);

            BuildSelectedPlate(gameplay, new Vector3(-2.05f, 0.04f, 0.25f), dark, cyan, violet);
            BuildSelectedBatteryAndSocket(gameplay, new Vector3(-3.15f, 0f, 2.25f),
                new Vector3(2.25f, 0f, 3.35f), dark, neutral, amber, lime, violet);
            BuildSelectedDoor(gameplay, new Vector3(0f, 0f, 5.75f), catalog, neutral, lime, violet);
            BuildSelectedGoal(gameplay, new Vector3(0f, 0f, 9.8f), catalog, dark, neutral,
                violet, lime, white, settings.PackagedJapaneseFont);

            Camera camera = BuildSelectedCamera(root, Hex("08111B"));
            BuildSelectedLights(environment, violet, cyan, amber, white);
            Canvas hud = BuildSelectedHud(root, camera, panel, violet, cyan, amber, white,
                settings.PackagedJapaneseFont);
            hud.gameObject.SetActive(false);
            logoGallery.gameObject.SetActive(false);

            preview = new SelectedPreviewReferences(camera, logoGallery.gameObject,
                environment.gameObject, gameplay.gameObject, hud, chamber, maintenance);
        }

        private static void BuildSelectedLogoGallery(Transform root, Material dark, Material panel,
            Material violet, Material cyan, Material white, Font font)
        {
            Visual("Grounded Logo Wall", PrimitiveType.Cube, root, new Vector3(0f, 2.6f, 0.2f),
                new Vector3(12f, 5.2f, 0.35f), dark);
            Visual("Horizontal Logo Recess", PrimitiveType.Cube, root, new Vector3(-1.1f, 2.9f, -0.02f),
                new Vector3(8.8f, 1.85f, 0.08f), panel);
            SelectedWorldText("Horizontal ECH", "ECH", root, new Vector3(-4.45f, 3.05f, -0.1f),
                0.92f, Color.white, font, TextAnchor.MiddleLeft);
            Transform logoSeal = Child(root, "Logo Integrated O Seal");
            logoSeal.localPosition = new Vector3(-1.45f, 1.04f, -0.16f);
            BuildVerticalHexSegments(logoSeal, "Logo O", 0.7f, 2.02f, 0f,
                new[] { 0, 1, 2, 3, 4 }, violet, 0.14f);
            BuildThreeShortTicks(logoSeal, "Logo Phase Ticks", new Vector3(0.98f, 2.02f, 0f),
                cyan, 3, true);
            SelectedWorldText("Horizontal Divider", "//", root, new Vector3(-0.12f, 3.02f, -0.11f),
                0.82f, cyan.color, font, TextAnchor.MiddleCenter);
            SelectedWorldText("Horizontal SHIFT", "SHIFT", root, new Vector3(2.42f, 3.05f, -0.1f),
                0.92f, Color.white, font, TextAnchor.MiddleCenter);
            SelectedWorldText("Canonical Caption", "RECORDED TIME / ACTIVE PRESENT", root,
                new Vector3(-1.0f, 1.65f, -0.11f), 0.24f, cyan.color, font,
                TextAnchor.MiddleCenter);

            Visual("Square Icon Grounded Panel", PrimitiveType.Cube, root,
                new Vector3(4.45f, 1.35f, -0.02f), new Vector3(2.1f, 2.3f, 0.12f), panel);
            Transform icon = Child(root, "Square Seal Icon");
            icon.localPosition = new Vector3(4.45f, 0.06f, -0.16f);
            BuildVerticalHexSegments(icon, "Icon Seal", 0.72f, 1.34f, 0f,
                new[] { 0, 1, 2, 3, 4 }, violet, 0.15f);
            BuildThreeShortTicks(icon, "Icon Ticks", new Vector3(1.0f, 1.34f, 0f), cyan, 3, true);

            Visual("Monochrome Grounded Strip", PrimitiveType.Cube, root,
                new Vector3(-2.55f, 0.55f, -0.01f), new Vector3(5.8f, 0.72f, 0.12f), panel);
            SelectedWorldText("Monochrome Logo", "ECHO//SHIFT", root,
                new Vector3(-2.55f, 0.53f, -0.1f), 0.43f, white.color, font,
                TextAnchor.MiddleCenter);

        }

        private static void BuildSelectedFloor(Transform root, Phase4ExternalAssetCatalog catalog,
            Material dark, Material panel, Material violet)
        {
            Transform floor = Child(root, "Selected Grounded Floor");
            Visual("Facility Foundation", PrimitiveType.Cube, floor, new Vector3(0f, -0.4f, 1f),
                new Vector3(14.2f, 0.75f, 25.5f), dark);
            for (int z = 0; z < 8; z++)
            {
                float localZ = -9.5f + z * 3f;
                for (int x = -1; x <= 1; x++)
                {
                    if (x == 0)
                    {
                        Visual($"Quiet Gameplay Route {z + 1:00}", PrimitiveType.Cube, floor,
                            new Vector3(0f, 0.02f, localZ), new Vector3(4.15f, 0.08f, 2.82f), panel);
                        continue;
                    }
                    int variant = x < 0 ? (z + 1) % catalog.FloorModules.Length :
                        (z * 2 + 3) % catalog.FloorModules.Length;
                    PrefabVisual($"{(x < 0 ? "Maintenance" : "Observation")} Floor {z + 1:00}",
                        catalog.FloorModules[variant], floor, new Vector3(x * 4.55f, 0.04f, localZ),
                        Quaternion.identity, new Vector3(0.98f, 1f, 0.94f));
                }
            }
            BuildThreeShortTicks(floor, "Maintenance Zone Phase Index",
                new Vector3(-3.55f, 0.1f, -1.2f), violet, 3, false);
            BuildThreeShortTicks(floor, "Observation Zone Phase Index",
                new Vector3(3.55f, 0.1f, -1.2f), violet, 3, false);
        }

        private static void BuildSelectedOrthogonalShell(Transform root, Material neutral,
            Material violet)
        {
            Transform shell = Child(root, "Selected Orthogonal Grounded Shell");
            float[] westZ = { -8.8f, -4.8f, -0.6f, 4.2f, 8.3f };
            for (int i = 0; i < westZ.Length; i++)
                Visual($"Grounded Orthogonal West Wall {i + 1:00}", PrimitiveType.Cube, shell,
                    new Vector3(-6.35f, 1.5f, westZ[i]), new Vector3(0.28f, 3f, 3.65f), neutral);
            float[] eastZ = { -8.6f, -3.9f, 2.2f, 8.1f };
            for (int i = 0; i < eastZ.Length; i++)
                Visual($"Grounded Orthogonal East Wall {i + 1:00}", PrimitiveType.Cube, shell,
                    new Vector3(6.35f, 1.5f, eastZ[i]), new Vector3(0.28f, 3f, 3.65f), neutral);
            Visual("Rear Facility Wall", PrimitiveType.Cube, shell, new Vector3(0f, 1.5f, 11.2f),
                new Vector3(12.7f, 3f, 0.28f), neutral);
            BuildThreeShortTicks(shell, "Rear Facility Phase Index",
                new Vector3(0f, 2.65f, 11.02f), violet, 3, false);
        }

        private static Transform BuildSelectedMaintenanceBay(Transform root,
            Phase4ExternalAssetCatalog catalog, Material dark, Material panel, Material violet,
            Material neutral, Material amber, Material white, Font font)
        {
            Transform bay = Child(root, "Left Maintenance Bay");
            Visual("Maintenance Bay Grounded Base", PrimitiveType.Cube, bay,
                new Vector3(-4.65f, 0.1f, 2.5f), new Vector3(3.1f, 0.18f, 5.3f), dark);
            Transform rack = Child(bay, "Grounded Battery Storage Rack");
            rack.localPosition = new Vector3(-5.1f, 0f, 0.7f);
            Visual("Rack Left", PrimitiveType.Cube, rack, new Vector3(-0.65f, 1.1f, 0f),
                new Vector3(0.12f, 2.2f, 1.2f), panel);
            Visual("Rack Right", PrimitiveType.Cube, rack, new Vector3(0.65f, 1.1f, 0f),
                new Vector3(0.12f, 2.2f, 1.2f), panel);
            for (int i = 0; i < 3; i++)
                Visual($"Rack Shelf {i + 1}", PrimitiveType.Cube, rack,
                    new Vector3(0f, 0.3f + i * 0.7f, 0f), new Vector3(1.35f, 0.09f, 1.2f), panel);
            for (int i = 0; i < 2; i++)
            {
                GameObject stored = Visual($"Stored Amber Battery {i + 1}", PrimitiveType.Cylinder,
                    rack, new Vector3(0f, 0.58f + i * 0.7f, -0.05f),
                    new Vector3(0.24f, 0.48f, 0.24f), amber);
                stored.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            PrefabVisual("Grounded Maintenance Equipment", catalog.PropModules[1], bay,
                new Vector3(-4.4f, 0.65f, 4.45f), Quaternion.identity,
                new Vector3(0.75f, 0.88f, 0.75f));

            Transform wall = Child(bay, "Maintenance Decal Wall");
            wall.localPosition = new Vector3(-4.65f, 1.5f, 4.95f);
            Visual("Grounded Maintenance Wall Module", PrimitiveType.Cube, wall, Vector3.zero,
                new Vector3(3.25f, 3f, 0.28f), neutral);
            Visual("Integrated Maintenance Decal Recess", PrimitiveType.Cube, wall,
                new Vector3(0f, 0.22f, -0.17f), new Vector3(2.6f, 1.55f, 0.04f), panel);
            SelectedWorldText("Maintenance Decal", "MAINTENANCE", wall,
                new Vector3(0f, 0.62f, -0.2f), 0.24f, violet.color, font,
                TextAnchor.MiddleCenter);
            SelectedWorldText("Battery Bay Decal", "BATTERY BAY", wall,
                new Vector3(-0.46f, 0.2f, -0.2f), 0.17f, amber.color, font,
                TextAnchor.MiddleCenter);
            SelectedWorldText("Inspected Decal", "INSPECTED", wall,
                new Vector3(0.58f, -0.12f, -0.2f), 0.145f,
                Color.Lerp(white.color, violet.color, 0.25f), font, TextAnchor.MiddleCenter);
            SelectedWorldText("Serial Decal", "ES-PHASE-05A-031", wall,
                new Vector3(0f, -0.42f, -0.2f), 0.13f,
                Color.Lerp(white.color, Color.gray, 0.3f), font, TextAnchor.MiddleCenter);
            BuildThreeShortTicks(wall, "Maintenance Phase Ticks",
                new Vector3(-0.98f, 0.92f, -0.21f), violet, 3, false);
            return bay;
        }

        private static void BuildSelectedObservationBay(Transform root,
            Phase4ExternalAssetCatalog catalog, Material dark, Material panel, Material violet,
            Material cyan, Material white, Font font)
        {
            Transform bay = Child(root, "Right Observation Bay");
            Visual("Observation Bay Grounded Base", PrimitiveType.Cube, bay,
                new Vector3(4.75f, 0.1f, 0.6f), new Vector3(2.7f, 0.18f, 4.6f), dark);
            Visual("Observation Window Body", PrimitiveType.Cube, bay,
                new Vector3(5.55f, 1.55f, 1.5f), new Vector3(0.28f, 2.8f, 3.1f), panel);
            Visual("Observation Window Recess", PrimitiveType.Cube, bay,
                new Vector3(5.36f, 1.62f, 1.5f), new Vector3(0.06f, 1.65f, 2.2f), dark);
            Visual("Observation Monitor", PrimitiveType.Cube, bay,
                new Vector3(4.45f, 1.15f, 2.3f), new Vector3(1.45f, 1.15f, 0.16f), panel);
            Visual("Observation Monitor Signal", PrimitiveType.Cube, bay,
                new Vector3(4.45f, 1.2f, 2.2f), new Vector3(1.08f, 0.08f, 0.03f), cyan);
            for (int i = 0; i < 3; i++)
                Visual($"Observation Phase Sample {i + 1}", PrimitiveType.Cube, bay,
                    new Vector3(4.05f + i * 0.4f, 1.48f + i * 0.12f, 2.19f),
                    new Vector3(0.17f, 0.04f, 0.03f), violet);
            SelectedWorldText("Record Sector Decal", "RECORD SECTOR 03", bay,
                new Vector3(4.45f, 0.72f, 2.18f), 0.18f,
                Color.Lerp(white.color, violet.color, 0.2f), font, TextAnchor.MiddleCenter);
            PrefabVisual("Wall Connected Observation Console", catalog.PropModules[3], bay,
                new Vector3(4.95f, 0.65f, -0.85f), Quaternion.Euler(0f, 180f, 0f),
                new Vector3(0.72f, 0.82f, 0.72f));
        }

        private static void BuildSimplifiedSelectedChamber(Transform root, Material dark,
            Material violet, Material cyan, Material white)
        {
            Visual("Chamber Grounded Hex Plinth", PrimitiveType.Cylinder, root,
                new Vector3(0f, 0.12f, 0f), new Vector3(1.9f, 0.12f, 1.9f), dark);
            BuildVerticalHexSegments(root, "Outer Split Hex Seal", 1.45f, 1.75f, 0f,
                new[] { 0, 1, 2, 3, 4 }, violet, 0.1f);
            BuildVerticalHexSegments(root, "Inner Phase Arc 1", 1.05f, 1.75f, -0.03f,
                new[] { 0, 1, 2 }, violet, 0.07f);
            BuildVerticalHexSegments(root, "Inner Phase Arc 2", 0.78f, 1.75f, -0.06f,
                new[] { 1, 2, 3 }, cyan, 0.065f);
            BuildVerticalHexSegments(root, "Inner Phase Arc 3", 0.52f, 1.75f, -0.09f,
                new[] { 2, 3, 4 }, white, 0.055f);
            Visual("Single Missing Segment Index", PrimitiveType.Cube, root,
                new Vector3(-1.28f, 0.56f, -0.04f), new Vector3(0.28f, 0.055f, 0.07f), cyan);
            BuildThreeShortTicks(root, "Generation Phase Ticks", new Vector3(1.78f, 1.75f, 0f),
                violet, 3, true);
            Visual("Compact Time Core", PrimitiveType.Sphere, root, new Vector3(0f, 1.75f, -0.08f),
                new Vector3(0.34f, 0.34f, 0.16f), cyan);
        }

        private static void BuildSelectedActorCrest(Transform actor, Material material,
            int tickCount, int missingIndex, bool echo)
        {
            Transform crest = Child(actor, echo ? $"Echo {tickCount} Identity Crest" :
                "Current Player Identity Crest");
            crest.localPosition = new Vector3(0f, 0.04f, -0.58f);
            int[] segments = new int[5];
            int write = 0;
            for (int i = 0; i < 6; i++)
                if (i != missingIndex && write < segments.Length) segments[write++] = i;
            BuildVerticalHexSegments(crest, "Small Split Seal", 0.18f, 1.58f, 0f,
                segments, material, 0.045f);
            if (tickCount > 0)
                BuildThreeShortTicks(crest, "Generation Tick Count",
                    new Vector3(0.34f, 1.58f, 0f), material, tickCount, true);
            if (!echo)
                BuildThreeShortTicks(crest, "Current Recording Ticks",
                    new Vector3(0.34f, 1.58f, 0f), material, 3, true);
        }

        private static void BuildSelectedPlate(Transform parent, Vector3 position, Material dark,
            Material cyan, Material violet)
        {
            Transform plate = Child(parent, "Selected Pressure Plate Preview");
            plate.localPosition = position;
            Visual("Plate Neutral Housing", PrimitiveType.Cube, plate, Vector3.zero,
                new Vector3(2.45f, 0.16f, 2.45f), dark);
            Visual("Plate Cyan Time Surface", PrimitiveType.Cube, plate,
                new Vector3(0f, 0.12f, 0f), new Vector3(2.0f, 0.08f, 2.0f), cyan);
            BuildHorizontalHexSegments(plate, "Plate Small Facility Seal", 0.56f, 0.18f,
                new[] { 0, 1, 2, 3, 4 }, violet, 0.06f);
        }

        private static void BuildSelectedBatteryAndSocket(Transform parent, Vector3 batteryPosition,
            Vector3 socketPosition, Material dark, Material neutral, Material amber, Material lime,
            Material violet)
        {
            Transform battery = Child(parent, "Selected Amber Battery Preview");
            battery.localPosition = batteryPosition + Vector3.up * 0.72f;
            GameObject core = Visual("Amber Battery Core", PrimitiveType.Cylinder, battery,
                Vector3.zero, new Vector3(0.45f, 0.44f, 0.45f), amber);
            core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject cap = Visual($"Neutral Battery End {side}", PrimitiveType.Cylinder,
                    battery, new Vector3(0f, 0f, side * 0.43f),
                    new Vector3(0.54f, 0.08f, 0.54f), neutral);
                cap.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            BuildVerticalHexSegments(battery, "Small Battery Seal Engraving", 0.18f, 0.05f, -0.46f,
                new[] { 0, 1, 2, 3, 4 }, violet, 0.045f);

            Transform socket = Child(parent, "Selected Connected Power Socket Preview");
            socket.localPosition = socketPosition;
            Visual("Socket Neutral Base", PrimitiveType.Cube, socket, new Vector3(0f, 0.18f, 0f),
                new Vector3(1.65f, 0.34f, 1.65f), dark);
            Visual("Socket Amber Interaction Ring", PrimitiveType.Cylinder, socket,
                new Vector3(0f, 0.42f, 0f), new Vector3(0.72f, 0.08f, 0.72f), amber);
            BuildHorizontalHexSegments(socket, "Connected Lime Seal", 0.42f, 0.5f,
                new[] { 0, 1, 2, 3, 4, 5 }, lime, 0.06f);
        }

        private static void BuildSelectedDoor(Transform parent, Vector3 position,
            Phase4ExternalAssetCatalog catalog, Material neutral, Material lime, Material violet)
        {
            Transform door = Child(parent, "Selected Unlocked Split Door Preview");
            door.localPosition = position;
            PrefabVisual("Neutral Door Frame", catalog.DoorFrame, door,
                new Vector3(0f, 1.5f, 0f), Quaternion.identity, new Vector3(1.1f, 1f, 1f));
            GameObject left = PrefabVisual("Unlocked Left Retracted Panel", catalog.DoorPanel, door,
                new Vector3(-1.55f, 1.5f, 0f), Quaternion.identity, new Vector3(0.48f, 1f, 1f));
            GameObject right = PrefabVisual("Unlocked Right Retracted Panel", catalog.DoorPanel, door,
                new Vector3(1.55f, 1.5f, 0f), Quaternion.identity, new Vector3(0.48f, 1f, 1f));
            TintNamedRenderers(left, neutral);
            TintNamedRenderers(right, neutral);
            Transform seal = Child(door, "Unlocked Door Center Seal");
            BuildVerticalHexSegments(seal, "Unlocked Lime Segments", 0.34f, 2.85f, -0.22f,
                new[] { 0, 1, 2, 3, 4, 5 }, lime, 0.07f);
            BuildThreeShortTicks(door, "Door Facility Identity Ticks",
                new Vector3(0.72f, 2.85f, -0.22f), violet, 3, true);
        }

        private static void BuildSelectedGoal(Transform parent, Vector3 position,
            Phase4ExternalAssetCatalog catalog, Material dark, Material neutral, Material violet,
            Material lime, Material white, Font font)
        {
            Transform goal = Child(parent, "Selected Unlocked Goal Preview");
            goal.localPosition = position;
            PrefabVisual("Goal Neutral Entry Frame", catalog.DoorFrame, goal,
                new Vector3(0f, 1.5f, 0.55f), Quaternion.identity, new Vector3(1.15f, 1f, 1f));
            Visual("Goal Dark Aperture", PrimitiveType.Cube, goal,
                new Vector3(0f, 1.48f, 0.72f), new Vector3(3.5f, 2.7f, 0.14f), dark);
            BuildHorizontalHexSegments(goal, "Goal Floor Split Seal", 1.45f, 0.13f,
                new[] { 0, 1, 2, 3, 4 }, violet, 0.12f);
            BuildHorizontalHexSegments(goal, "Unlocked Goal Completion Segment", 1.45f, 0.14f,
                new[] { 5 }, lime, 0.12f);
            SelectedWorldText("Goal Japanese Label", "回収ポイント", goal,
                new Vector3(0f, 2.86f, 0.38f), 0.34f, white.color, font,
                TextAnchor.MiddleCenter);
        }

        private static Camera BuildSelectedCamera(Transform root, Color background)
        {
            GameObject cameraObject = new GameObject("Phase 5A Selected Preview Camera");
            cameraObject.transform.SetParent(root, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.fieldOfView = 43f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.allowHDR = true;
            camera.aspect = CaptureWidth / (float)CaptureHeight;
            return camera;
        }

        private static void BuildSelectedLights(Transform root, Material violet, Material cyan,
            Material amber, Material white)
        {
            GameObject sunObject = new GameObject("Selected Neutral Key Light");
            sunObject.transform.SetParent(root, false);
            sunObject.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = white.color;
            sun.intensity = 0.92f;
            sun.shadows = LightShadows.Soft;
            PointLight(root, "Selected Chamber Violet Light", new Vector3(4.2f, 4.0f, -5.5f),
                violet.color, 7f, 1.75f);
            PointLight(root, "Selected Player Cyan Light", new Vector3(-0.4f, 3.2f, -1.8f),
                cyan.color, 6f, 1.25f);
            PointLight(root, "Selected Battery Amber Light", new Vector3(-3f, 2.5f, 2.2f),
                amber.color, 5f, 1.05f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(Hex("08111B"), white.color, 0.16f);
        }

        private static Canvas BuildSelectedHud(Transform root, Camera camera, Material panel,
            Material violet, Material cyan, Material amber, Material white, Font font)
        {
            GameObject canvasObject = new GameObject("Selected Japanese HUD Preview");
            canvasObject.transform.SetParent(root, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.5f;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CaptureWidth, CaptureHeight);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            Color panelColor = new Color(panel.color.r, panel.color.g, panel.color.b, 0.92f);
            Image status = UiPanel("Selected Status Capsule", canvas.transform,
                new Vector2(36f, -34f), new Vector2(455f, 112f), new Vector2(0f, 1f), panelColor);
            UiText("Selected Loop Copy", status.transform, SelectedCopyValues[2],
                new Vector2(18f, -14f), new Vector2(120f, 40f), new Vector2(0f, 1f), 25f,
                cyan.color, font, TextAnchor.MiddleLeft);
            UiText("Selected Remaining Copy", status.transform, SelectedCopyValues[3],
                new Vector2(148f, -14f), new Vector2(132f, 40f), new Vector2(0f, 1f), 25f,
                white.color, font, TextAnchor.MiddleLeft);
            UiText("Selected Echo Copy", status.transform, SelectedCopyValues[4],
                new Vector2(292f, -14f), new Vector2(145f, 40f), new Vector2(0f, 1f), 25f,
                violet.color, font, TextAnchor.MiddleLeft);
            UiText("Selected Battery Copy", status.transform, SelectedCopyValues[5],
                new Vector2(18f, -62f), new Vector2(250f, 34f), new Vector2(0f, 1f), 20f,
                amber.color, font, TextAnchor.MiddleLeft);

            Image objective = UiPanel("Selected Section Intro", canvas.transform,
                new Vector2(0f, -30f), new Vector2(690f, 96f), new Vector2(0.5f, 1f), panelColor);
            UiText("Selected Section Copy", objective.transform, SelectedCopyValues[0],
                new Vector2(0f, -9f), new Vector2(650f, 34f), new Vector2(0.5f, 1f), 23f,
                violet.color, font, TextAnchor.MiddleCenter);
            UiText("Selected Objective Copy", objective.transform, SelectedCopyValues[1],
                new Vector2(0f, -45f), new Vector2(650f, 38f), new Vector2(0.5f, 1f), 22f,
                white.color, font, TextAnchor.MiddleCenter);

            Image prompt = UiPanel("Selected Context Prompt", canvas.transform,
                new Vector2(-34f, -34f), new Vector2(190f, 56f), new Vector2(1f, 1f), panelColor);
            UiText("Selected Prompt Copy", prompt.transform, SelectedCopyValues[6], Vector2.zero,
                new Vector2(172f, 48f), new Vector2(0.5f, 0.5f), 24f, white.color, font,
                TextAnchor.MiddleCenter);
            return canvas;
        }

        private static void RenderSelectedRevision(SelectedPreviewReferences preview,
            string outputDirectory)
        {
            Vector3[] positions =
            {
                new Vector3(0f, 3.2f, -14.6f),
                new Vector3(4.2f, 3.5f, -12.6f),
                new Vector3(-6.4f, 4.3f, -6.8f),
                new Vector3(7.6f, 5.2f, 1.35f),
                new Vector3(14f, 22.5f, -13.5f),
                new Vector3(10.2f, 10.4f, -5.4f),
                new Vector3(-2.3f, 3.1f, 0.6f)
            };
            Vector3[] targets =
            {
                new Vector3(0f, 2.45f, 0f),
                new Vector3(4.2f, 1.75f, -5.5f),
                new Vector3(0f, 1.05f, -0.8f),
                new Vector3(0f, 1.05f, 5.7f),
                new Vector3(0.2f, 0.35f, 0.7f),
                new Vector3(0f, 0.9f, 1.2f),
                new Vector3(-4.65f, 1.55f, 4.95f)
            };
            float[] fieldsOfView = { 36f, 34f, 38f, 39f, 51f, 44f, 36f };
            for (int i = 0; i < SelectedCaptureNames.Length; i++)
            {
                bool logo = i == 0;
                preview.LogoGallery.SetActive(logo);
                preview.Environment.SetActive(!logo);
                preview.Gameplay.SetActive(!logo && i != 1 && i != 6);
                preview.Chamber.gameObject.SetActive(!logo && i != 2 && i != 3 && i != 5 && i != 6);
                preview.Hud.gameObject.SetActive(i == 5);
                preview.Camera.transform.position = positions[i];
                preview.Camera.transform.rotation =
                    Quaternion.LookRotation(targets[i] - positions[i], Vector3.up);
                preview.Camera.fieldOfView = fieldsOfView[i];
                Render(preview.Camera, Path.Combine(outputDirectory, SelectedCaptureNames[i]));
            }
            preview.LogoGallery.SetActive(false);
            preview.Environment.SetActive(true);
            preview.Gameplay.SetActive(true);
            preview.Chamber.gameObject.SetActive(true);
            preview.Hud.gameObject.SetActive(false);
        }

        private static void WriteSelectedLogoSources()
        {
            string violet = "9D84E8";
            string cyan = "67E8F9";
            string white = "E7EFF5";
            StringBuilder horizontal = new StringBuilder(2048);
            horizontal.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"320\" viewBox=\"0 0 1024 320\">");
            horizontal.Append($"<text x=\"42\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">ECH</text>");
            AppendSelectedSealSvg(horizontal, 362, 161, 62, violet, 5, 13);
            horizontal.Append($"<text x=\"447\" y=\"205\" fill=\"#{cyan}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"102\" font-weight=\"800\">//</text>");
            horizontal.Append($"<text x=\"555\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">SHIFT</text>");
            AppendSelectedSvgTicks(horizontal, 420, 114, cyan);
            horizontal.Append("</svg>");
            WriteSelectedSvg("P5A_Selected_Logo_Horizontal.svg", horizontal);

            StringBuilder icon = new StringBuilder(1024);
            icon.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"512\" height=\"512\" viewBox=\"0 0 512 512\">");
            AppendSelectedSealSvg(icon, 232, 256, 142, violet, 5, 26);
            AppendSelectedSvgTicks(icon, 385, 210, cyan);
            icon.Append("</svg>");
            WriteSelectedSvg("P5A_Selected_Logo_Icon.svg", icon);

            StringBuilder monochrome = new StringBuilder(2048);
            monochrome.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"320\" viewBox=\"0 0 1024 320\">");
            monochrome.Append($"<text x=\"42\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">ECH</text>");
            AppendSelectedSealSvg(monochrome, 362, 161, 62, white, 5, 13);
            monochrome.Append($"<text x=\"447\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"102\" font-weight=\"800\">//</text>");
            monochrome.Append($"<text x=\"555\" y=\"205\" fill=\"#{white}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"118\" font-weight=\"800\" letter-spacing=\"7\">SHIFT</text>");
            AppendSelectedSvgTicks(monochrome, 420, 114, white);
            monochrome.Append("</svg>");
            WriteSelectedSvg("P5A_Selected_Logo_Monochrome.svg", monochrome);
        }

        private static void WriteSelectedDecalAtlas()
        {
            StringBuilder svg = new StringBuilder(12288);
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"1024\" viewBox=\"0 0 1024 1024\">");
            for (int i = 0; i < SelectedDecalValues.Length; i++)
            {
                int column = i % 3;
                int row = i / 3;
                int x = column * 341;
                int y = row * 204;
                string color = i == 7 ? "F2B84B" : i == 8 ? "E7EFF5" :
                    i == 4 || i == 11 ? "C96B74" : i % 3 == 0 ? "9D84E8" : "AFC3D4";
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<g id=\"selected-decal-{0:00}\"><rect x=\"{1}\" y=\"{2}\" width=\"319\" height=\"182\" rx=\"6\" fill=\"#101923\" stroke=\"#{3}\" stroke-width=\"3\" opacity=\"0.92\"/>",
                    i + 1, x + 10, y + 10, color);
                AppendSelectedSealSvg(svg, x + 50, y + 58, 27, color, i % 6, 6);
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<text x=\"{0}\" y=\"{1}\" fill=\"#{2}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"{3}\" font-weight=\"700\" letter-spacing=\"1\">{4}</text></g>",
                    x + 28, y + 143, color, SelectedDecalValues[i].Length > 18 ? 18 : 23,
                    EscapeXml(SelectedDecalValues[i]));
            }
            svg.Append("</svg>");
            WriteSelectedSvg("P5A_Selected_DecalAtlas.svg", svg);
        }

        private static void WriteSelectedProductionHashManifest()
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
                manifest.Append(Sha256SelectedAsset(scenePaths[i]));
                manifest.Append('\n');
            }
            string assetPath = $"{SelectedArtRoot}/P5A_Selected_ProductionHashes.txt";
            File.WriteAllText(AbsoluteAssetPath(assetPath), manifest.ToString(),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static string Sha256SelectedAsset(string assetPath)
        {
            using (SHA256 algorithm = SHA256.Create())
            using (FileStream stream = File.OpenRead(AbsoluteAssetPath(assetPath)))
            {
                byte[] hash = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        private static void WriteSelectedAudioPreview(string path)
        {
            const int sampleRate = 44100;
            float[] mix = new float[sampleRate * 10];
            AudioClip phaseTick = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioRoot + "laserSmall_000.ogg");
            AudioClip field = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioRoot + "forceField_000.ogg");
            AudioClip impact = AssetDatabase.LoadAssetAtPath<AudioClip>(
                AudioRoot + "impactMetal_001.ogg");
            if (phaseTick == null || field == null || impact == null)
                throw new InvalidOperationException("Required Kenney source audio is missing.");
            float[] tickData = ReadMono(phaseTick);
            float[] fieldData = ReadMono(field);
            float[] impactData = ReadMono(impact);

            MixResampled(tickData, phaseTick.frequency, mix, sampleRate, 0.55f, 1.00f, 0.42f);
            MixResampled(tickData, phaseTick.frequency, mix, sampleRate, 2.20f, 1.00f, 0.34f);
            MixResampled(fieldData, field.frequency, mix, sampleRate, 2.23f, 1.13f, 0.14f);
            MixResampled(tickData, phaseTick.frequency, mix, sampleRate, 2.66f, 1.26f, 0.31f);
            MixResampled(tickData, phaseTick.frequency, mix, sampleRate, 4.65f, 1.00f, 0.30f);
            MixResampled(fieldData, field.frequency, mix, sampleRate, 4.68f, 1.09f, 0.12f);
            MixResampled(tickData, phaseTick.frequency, mix, sampleRate, 5.08f, 1.26f, 0.29f);
            MixResampled(impactData, impact.frequency, mix, sampleRate, 5.10f, 1.34f, 0.08f);
            MixResampled(tickData, phaseTick.frequency, mix, sampleRate, 5.50f, 1.50f, 0.27f);
            MixResampled(impactData, impact.frequency, mix, sampleRate, 8.25f, 0.76f, 0.13f);
            AddReverseTail(fieldData, field.frequency, mix, sampleRate, 8.05f, 0.08f);
            ApplyOnePoleLowPass(mix, 0.44f);
            AddDelay(mix, sampleRate, 0.085f, 0.17f);
            Normalize(mix, 0.86f);
            WritePcm16Wave(path, mix, sampleRate);
        }

        private static Material[] BuildSelectedMaterials()
        {
            return new[]
            {
                MaterialAsset(SelectedArtRoot, "P5A_Selected_Dark", Hex("08111B"), 0.68f, 0.68f, 0f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_Panel", Hex("152534"), 0.58f, 0.62f, 0f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_Violet", Hex("9D84E8"), 0.25f, 0.5f, 0.82f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_Cyan", Hex("67E8F9"), 0.22f, 0.5f, 0.86f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_Amber", Hex("F2B84B"), 0.24f, 0.48f, 0.72f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_LimeSuccess", Hex("9FE870"), 0.2f, 0.5f, 0.78f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_RedClosed", Hex("D85E67"), 0.22f, 0.48f, 0.62f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_White", Hex("E7EFF5"), 0.12f, 0.42f, 0.24f),
                MaterialAsset(SelectedArtRoot, "P5A_Selected_Neutral", Hex("4B5968"), 0.66f, 0.6f, 0f),
                UnlitSelectedMaterialAsset("P5A_Selected_LogoDark", Hex("08111B")),
                UnlitSelectedMaterialAsset("P5A_Selected_LogoPanel", Hex("152534"))
            };
        }

        private static Material UnlitSelectedMaterialAsset(string name, Color color)
        {
            string path = $"{SelectedArtRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                throw new InvalidOperationException("URP Unlit shader is unavailable.");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildVerticalHexSegments(Transform parent, string name, float radius,
            float centerY, float z, int[] segments, Material material, float thickness)
        {
            Transform seal = Child(parent, name);
            for (int index = 0; index < segments.Length; index++)
            {
                int segmentIndex = segments[index];
                float angle0 = (30f + segmentIndex * 60f) * Mathf.Deg2Rad;
                float angle1 = (30f + (segmentIndex + 1) * 60f) * Mathf.Deg2Rad;
                Vector2 start = new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * radius;
                Vector2 end = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                Vector2 midpoint = (start + end) * 0.5f;
                Vector2 edge = end - start;
                GameObject segment = Visual($"Seal Segment {segmentIndex + 1}", PrimitiveType.Cube,
                    seal, new Vector3(midpoint.x, centerY + midpoint.y, z),
                    new Vector3(edge.magnitude * 0.84f, thickness, thickness), material);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f,
                    Mathf.Atan2(edge.y, edge.x) * Mathf.Rad2Deg);
            }
        }

        private static void BuildHorizontalHexSegments(Transform parent, string name, float radius,
            float y, int[] segments, Material material, float thickness)
        {
            Transform seal = Child(parent, name);
            for (int index = 0; index < segments.Length; index++)
            {
                int segmentIndex = segments[index];
                float angle0 = (30f + segmentIndex * 60f) * Mathf.Deg2Rad;
                float angle1 = (30f + (segmentIndex + 1) * 60f) * Mathf.Deg2Rad;
                Vector2 start = new Vector2(Mathf.Cos(angle0), Mathf.Sin(angle0)) * radius;
                Vector2 end = new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                Vector2 midpoint = (start + end) * 0.5f;
                Vector2 edge = end - start;
                GameObject segment = Visual($"Floor Seal Segment {segmentIndex + 1}", PrimitiveType.Cube,
                    seal, new Vector3(midpoint.x, y, midpoint.y),
                    new Vector3(edge.magnitude * 0.84f, 0.025f, thickness), material);
                segment.transform.localRotation = Quaternion.Euler(0f,
                    -Mathf.Atan2(edge.y, edge.x) * Mathf.Rad2Deg, 0f);
            }
        }

        private static void BuildThreeShortTicks(Transform parent, string name, Vector3 position,
            Material material, int count, bool vertical)
        {
            Transform ticks = Child(parent, name);
            ticks.localPosition = position;
            for (int i = 0; i < count; i++)
                Visual($"Phase Tick {i + 1}", PrimitiveType.Cube, ticks,
                    vertical ? new Vector3(0f, (i - (count - 1) * 0.5f) * 0.24f, 0f) :
                        new Vector3((i - (count - 1) * 0.5f) * 0.24f, 0f, 0f),
                    vertical ? new Vector3(0.22f + i * 0.04f, 0.055f, 0.055f) :
                        new Vector3(0.16f, 0.055f, 0.055f), material);
        }

        private static TextMesh SelectedWorldText(string objectName, string value, Transform parent,
            Vector3 position, float fontSize, Color color, Font font, TextAnchor alignment)
        {
            GameObject obj = new GameObject(objectName, typeof(TextMesh));
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            TextMesh text = obj.GetComponent<TextMesh>();
            text.text = value;
            text.font = font;
            text.fontSize = 64;
            text.characterSize = fontSize * 0.095f;
            text.color = color;
            text.anchor = alignment;
            text.alignment = TextAlignment.Center;
            text.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return text;
        }

        private static void AppendSelectedSealSvg(StringBuilder svg, int centerX, int centerY,
            int radius, string color, int missingSegment, int stroke)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == missingSegment) continue;
                float a0 = (30f + i * 60f) * Mathf.Deg2Rad;
                float a1 = (30f + (i + 1) * 60f) * Mathf.Deg2Rad;
                int x0 = centerX + Mathf.RoundToInt(Mathf.Cos(a0) * radius);
                int y0 = centerY + Mathf.RoundToInt(Mathf.Sin(a0) * radius);
                int x1 = centerX + Mathf.RoundToInt(Mathf.Cos(a1) * radius);
                int y1 = centerY + Mathf.RoundToInt(Mathf.Sin(a1) * radius);
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<path d=\"M{0} {1} L{2} {3}\" fill=\"none\" stroke=\"#{4}\" stroke-width=\"{5}\" stroke-linecap=\"square\"/>",
                    x0, y0, x1, y1, color, stroke);
            }
        }

        private static void AppendSelectedSvgTicks(StringBuilder svg, int x, int y, string color)
        {
            for (int i = 0; i < 3; i++)
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"8\" fill=\"#{3}\"/>",
                    x, y + i * 20, 30 + i * 7, color);
        }

        private static void WriteSelectedSvg(string fileName, StringBuilder svg)
        {
            string assetPath = $"{SelectedArtRoot}/{fileName}";
            File.WriteAllText(AbsoluteAssetPath(assetPath), svg.ToString(), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private static void ValidateSelectedRevision(string outputDirectory)
        {
            if (SelectedDecalValues.Length != SelectedDecalCount)
                throw new InvalidOperationException("Selected decal count is invalid.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(SelectedScenePath) == null)
                throw new InvalidOperationException("Selected preview Scene was not saved.");
            string[] sourceAssets =
            {
                "P5A_Selected_Logo_Horizontal.svg",
                "P5A_Selected_Logo_Icon.svg",
                "P5A_Selected_Logo_Monochrome.svg",
                "P5A_Selected_DecalAtlas.svg",
                "P5A_Selected_ProductionHashes.txt"
            };
            for (int i = 0; i < sourceAssets.Length; i++)
                if (!File.Exists(AbsoluteAssetPath($"{SelectedArtRoot}/{sourceAssets[i]}")))
                    throw new InvalidOperationException($"Selected source asset is missing: {sourceAssets[i]}");
            for (int i = 0; i < SelectedCaptureNames.Length; i++)
            {
                string path = Path.Combine(outputDirectory, SelectedCaptureNames[i]);
                if (!File.Exists(path) || new FileInfo(path).Length < 30000)
                    throw new InvalidOperationException($"Selected capture is missing: {path}");
                byte[] png = File.ReadAllBytes(path);
                if (ReadBigEndianInt32(png, 16) != CaptureWidth ||
                    ReadBigEndianInt32(png, 20) != CaptureHeight)
                    throw new InvalidOperationException($"Selected capture size is invalid: {path}");
            }
            string wav = Path.Combine(outputDirectory, "revised_audio_preview.wav");
            if (!File.Exists(wav) || new FileInfo(wav).Length <= 44)
                throw new InvalidOperationException("Selected audio preview is missing.");
            double seconds = (new FileInfo(wav).Length - 44d) / (44100d * 2d);
            if (seconds > MaximumAudioSeconds)
                throw new InvalidOperationException($"Selected audio preview is too long: {seconds:0.000}s");
        }

        private sealed class SelectedPreviewReferences
        {
            public SelectedPreviewReferences(Camera camera, GameObject logoGallery,
                GameObject environment, GameObject gameplay, Canvas hud, Transform chamber,
                Transform maintenance)
            {
                Camera = camera;
                LogoGallery = logoGallery;
                Environment = environment;
                Gameplay = gameplay;
                Hud = hud;
                Chamber = chamber;
                Maintenance = maintenance;
            }

            public Camera Camera { get; }
            public GameObject LogoGallery { get; }
            public GameObject Environment { get; }
            public GameObject Gameplay { get; }
            public Canvas Hud { get; }
            public Transform Chamber { get; }
            public Transform Maintenance { get; }
        }
    }
}
