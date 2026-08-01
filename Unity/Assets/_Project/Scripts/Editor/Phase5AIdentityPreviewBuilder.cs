using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    public sealed class Phase5AIdentityOptionDefinition
    {
        public Phase5AIdentityOptionDefinition(
            string id, string concept, string motif, Color background, Color panel,
            Color primary, Color secondary, Color accent, string[] copy,
            string[] decals, float[] motifPitches, string audioTreatment)
        {
            Id = id;
            Concept = concept;
            Motif = motif;
            Background = background;
            Panel = panel;
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
            Copy = copy;
            Decals = decals;
            MotifPitches = motifPitches;
            AudioTreatment = audioTreatment;
        }

        public string Id { get; }
        public string Concept { get; }
        public string Motif { get; }
        public Color Background { get; }
        public Color Panel { get; }
        public Color Primary { get; }
        public Color Secondary { get; }
        public Color Accent { get; }
        public string[] Copy { get; }
        public string[] Decals { get; }
        public float[] MotifPitches { get; }
        public string AudioTreatment { get; }
    }

    public static partial class Phase5AIdentityPreviewBuilder
    {
        public const int CaptureWidth = 1920;
        public const int CaptureHeight = 1080;
        public const int RequiredCaptureCountPerOption = 5;
        public const int RequiredDecalCount = 12;
        public const float MaximumAudioSeconds = 15f;
        public const string ArtRoot = "Assets/_Project/Art/Phase5A/Options";
        public const string SceneRoot = "Assets/_Project/Scenes/Preview";

        private const string SettingsPath = "Assets/_Project/Settings/Phase4VisualSettings.asset";
        private const string CatalogPath = "Assets/_Project/Settings/Phase4ExternalAssetCatalog.asset";
        private const string EchoPrefabPath = "Assets/_Project/Prefabs/Actors/P3_Echo.prefab";
        private const string AudioRoot = "Assets/_Project/ThirdParty/Kenney/SciFiSounds/Audio/";

        private static readonly string[] CaptureNames =
        {
            "01_logo_echo_chamber.png",
            "02_section3_overview.png",
            "03_player_echo_motif.png",
            "04_door_battery_decal.png",
            "05_hud_goal.png"
        };

        private static readonly Phase5AIdentityOptionDefinition[] OptionValues =
        {
            new Phase5AIdentityOptionDefinition(
                "A", "Phase Lattice", "二重の欠けた時間輪と一定間隔の位相tick",
                Hex("08121F"), Hex("10283A"), Hex("67E8F9"), Hex("FFD166"), Hex("E8F3F5"),
                new[]
                {
                    "記録層 03", "軌跡を重ね、出口を解錠", "記録 02", "残り 18.4",
                    "残響 02", "セル携行", "[E] 接続", "同期出口", "記録完了",
                    "記録を一時停止", "この層を再記録", "実験を終了"
                },
                Decals("LATTICE", "SYNC"), new[] { 1.00f, 1.26f, 1.50f },
                "bright three-step lattice; high-pass source, short reverse tail, 90 ms stereo-like delay"),
            new Phase5AIdentityOptionDefinition(
                "B", "Afterimage Relay", "ずれた二本の軌跡と前方へ送るrelay chevron",
                Hex("10131C"), Hex("24273A"), Hex("FF6B6B"), Hex("72F1B8"), Hex("F6E7CB"),
                new[]
                {
                    "反復 03", "二つの残像へ役目を渡す", "走査 02", "残り 18.4",
                    "残像 02", "セル保持", "[E] 引き継ぐ", "帰還点", "軌跡を接続",
                    "反復を止める", "この区間をやり直す", "施設を離れる"
                },
                Decals("RELAY", "TRACE"), new[] { 1.34f, 1.10f, 0.92f },
                "descending relay; band-limited transient, two delayed taps, warm impact layer"),
            new Phase5AIdentityOptionDefinition(
                "C", "Archive Seal", "分割六角sealと欠番を残すarchive index",
                Hex("11130F"), Hex("242A24"), Hex("B89CFF"), Hex("B7F36B"), Hex("ECE7D9"),
                new[]
                {
                    "保管区画 03", "二つの記録体で封鎖を解除", "索引 02", "残り 18.4",
                    "記録体 02", "電源体 搬送中", "[E] 照合", "搬出口", "記録を封緘",
                    "照合を中断", "区画記録を復元", "保管庫を閉じる"
                },
                Decals("ARCHIVE", "INDEX"), new[] { 0.78f, 1.00f, 1.19f },
                "low archive seal; low-pass body, restrained reverse tail, single 140 ms echo")
        };

        public static IReadOnlyList<Phase5AIdentityOptionDefinition> Options => OptionValues;

        [MenuItem("ECHO SHIFT/Phase 5A/Generate Identity Options")]
        public static void GenerateAll()
        {
            Phase4VisualSettings settings = AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(SettingsPath);
            Phase4ExternalAssetCatalog catalog =
                AssetDatabase.LoadAssetAtPath<Phase4ExternalAssetCatalog>(CatalogPath);
            GameObject echoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath);
            if (settings == null || !settings.HasRequiredReferences)
                throw new InvalidOperationException("Phase 4 visual settings are missing or incomplete.");
            if (catalog == null || !catalog.IsComplete)
                throw new InvalidOperationException("Phase 4 external asset catalog is missing or incomplete.");
            if (echoPrefab == null) throw new InvalidOperationException("P3 Echo prefab is missing.");

            EnsureAssetFolder(SceneRoot);
            EnsureAssetFolder("Assets/_Project/Art/Phase5A");
            EnsureAssetFolder(ArtRoot);
            string repositoryRoot = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            if (string.IsNullOrEmpty(repositoryRoot))
                throw new InvalidOperationException("Repository root could not be resolved.");

            Cursor.visible = false;
            for (int i = 0; i < OptionValues.Length; i++)
                GenerateOption(OptionValues[i], settings, catalog, echoPrefab, repositoryRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"PHASE5A_PREVIEWS_OK options={OptionValues.Length};" +
                      $"captures={OptionValues.Length * RequiredCaptureCountPerOption};" +
                      $"graphics={SystemInfo.graphicsDeviceType}");
        }

        public static void GenerateAllFromCommandLine()
        {
            GenerateAll();
        }

        private static void GenerateOption(
            Phase5AIdentityOptionDefinition option, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog, GameObject echoPrefab, string repositoryRoot)
        {
            string optionAssetFolder = $"{ArtRoot}/{option.Id}";
            EnsureAssetFolder(optionAssetFolder);
            Material[] materials = BuildMaterials(option, optionAssetFolder);
            WriteSvgAtlas(option, optionAssetFolder);
            WriteLogoSvg(option, optionAssetFolder);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject($"Phase5A Option {option.Id} - {option.Concept}");
            BuildPreviewWorld(root.transform, option, settings, catalog, echoPrefab, materials,
                out Camera camera, out Canvas hud);
            string scenePath = $"{SceneRoot}/Phase5A_Option_{option.Id}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            string captureDirectory = Path.Combine(repositoryRoot, "Captures", "Phase5A", "Options", option.Id);
            Directory.CreateDirectory(captureDirectory);
            RenderShots(option, root.transform, camera, hud, captureDirectory);
            WriteAudioPreview(option, Path.Combine(captureDirectory, "audio_preview.wav"));
            ValidateOutputs(option, captureDirectory, scenePath);
        }

        private static void BuildPreviewWorld(
            Transform root, Phase5AIdentityOptionDefinition option, Phase4VisualSettings settings,
            Phase4ExternalAssetCatalog catalog, GameObject echoPrefab, Material[] materials,
            out Camera camera, out Canvas hud)
        {
            Material dark = materials[0];
            Material panel = materials[1];
            Material primary = materials[2];
            Material secondary = materials[3];
            Material accent = materials[4];
            Material pale = materials[5];

            Transform environment = Child(root, "Authored Section 3 Preview");
            Visual("Foundation", PrimitiveType.Cube, environment, new Vector3(0f, -0.42f, 1f),
                new Vector3(14f, 0.75f, 26f), dark);
            BuildModularFloor(environment, option, catalog, panel, dark);
            BuildAsymmetricShell(environment, option, catalog, dark, primary, secondary);
            BuildRoute(environment, option, primary, secondary, accent);

            Transform chamber = Child(environment, $"Option {option.Id} Echo Chamber");
            chamber.localPosition = new Vector3(0f, 0f, -7.2f);
            BuildEchoChamber(chamber, option, dark, primary, secondary, pale,
                settings.PackagedJapaneseFont);
            WorldText("ECHO SHIFT", chamber, new Vector3(0f, 4.25f, -0.7f),
                new Vector2(7.4f, 1.2f), 1.15f, option.Accent, settings.PackagedJapaneseFont,
                TextAnchor.MiddleCenter, Quaternion.identity);
            WorldText(option.Concept.ToUpperInvariant(), chamber, new Vector3(0f, 3.45f, -0.69f),
                new Vector2(6f, 0.7f), 0.38f, option.Primary, settings.PackagedJapaneseFont,
                TextAnchor.MiddleCenter, Quaternion.identity);

            Transform gameplay = Child(root, "Preview Actors and Devices");
            GameObject player = BuildActor(gameplay, echoPrefab, "Current Player Preview",
                new Vector3(-1.25f, 0f, -1.2f), 0f, primary, pale,
                Phase4RobotPoseState.Idle, false, option.Id);
            GameObject echoOne = BuildActor(gameplay, echoPrefab, "Echo 1 Preview",
                new Vector3(1.35f, 0f, -0.55f), -18f, secondary, secondary,
                Phase4RobotPoseState.EchoStopped, true, option.Id);
            BuildActor(gameplay, echoPrefab, "Echo 2 Preview", new Vector3(-2.8f, 0f, 4.9f),
                8f, accent, accent, Phase4RobotPoseState.Walk, true, option.Id);
            BuildActorMotif(player.transform, option, primary, false);
            BuildActorMotif(echoOne.transform, option, secondary, true);

            BuildPlate(gameplay, new Vector3(-2.8f, 0.04f, 4.9f), dark, primary, option);
            BuildBatteryAndSocket(gameplay, new Vector3(2.15f, 0f, 1.2f),
                new Vector3(2.3f, 0f, 3.75f), dark, secondary, option);
            BuildDoor(gameplay, new Vector3(0f, 0f, 5.9f), catalog, dark, secondary, option);
            BuildGoal(gameplay, new Vector3(0f, 0f, 10.1f), catalog, dark, pale, accent, option,
                settings.PackagedJapaneseFont);
            BuildDecalBoard(gameplay, option, dark, primary, settings.PackagedJapaneseFont);

            camera = BuildCamera(root, option);
            BuildLights(root, option, primary, secondary);
            hud = BuildHud(root, camera, option, settings.PackagedJapaneseFont, materials);
            hud.gameObject.SetActive(false);
        }

        private static void BuildModularFloor(Transform root, Phase5AIdentityOptionDefinition option,
            Phase4ExternalAssetCatalog catalog, Material panel, Material dark)
        {
            Transform floor = Child(root, "Deliberate Floor Hierarchy");
            for (int z = 0; z < 8; z++)
            {
                float localZ = -10.5f + z * 3f;
                for (int x = -1; x <= 1; x++)
                {
                    bool quietRoute = x == 0 && (z == 2 || z == 5 || z == 6);
                    if (quietRoute)
                    {
                        Visual($"Quiet Route {z}", PrimitiveType.Cube, floor,
                            new Vector3(0f, 0f, localZ), new Vector3(4.25f, 0.08f, 2.82f), panel);
                        continue;
                    }
                    int variant = (z * 2 + x + 4 + option.Id[0]) % catalog.FloorModules.Length;
                    GameObject tile = PrefabVisual($"Authored Floor {x + 2}-{z + 1}",
                        catalog.FloorModules[variant], floor, new Vector3(x * 4.55f, 0.04f, localZ),
                        Quaternion.identity, new Vector3(0.98f, 1f, 0.94f));
                    if ((z + x + option.Id[0]) % 5 == 0)
                        Visual("Dark Service Inlay", PrimitiveType.Cube, tile.transform,
                            new Vector3(0f, 0.08f, 0f), new Vector3(0.75f, 0.03f, 1.8f), dark);
                }
            }
        }

        private static void BuildAsymmetricShell(
            Transform root, Phase5AIdentityOptionDefinition option,
            Phase4ExternalAssetCatalog catalog, Material dark, Material primary, Material secondary)
        {
            Transform shell = Child(root, "Asymmetric Facility Shell");
            for (int z = 0; z < 6; z++)
            {
                float localZ = -9.5f + z * 4f;
                PrefabVisual($"West Wall {z + 1}", catalog.WallModules[z % 2], shell,
                    new Vector3(-6.35f, 1.5f, localZ), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
                if (z != (option.Id == "B" ? 1 : 4))
                    PrefabVisual($"East Wall {z + 1}", catalog.WallModules[(z + 1) % 2], shell,
                        new Vector3(6.35f, 1.5f, localZ), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            }

            int[] propIndices = option.Id == "A" ? new[] { 1, 4, 6 } :
                option.Id == "B" ? new[] { 0, 2, 5 } : new[] { 3, 6, 1 };
            float side = option.Id == "B" ? 1f : -1f;
            for (int i = 0; i < propIndices.Length; i++)
            {
                float x = side * (4.7f + i * 0.38f);
                float z = -2.5f + i * 4.1f;
                PrefabVisual($"Purposeful {option.Concept} Prop {i + 1}",
                    catalog.PropModules[propIndices[i]], shell, new Vector3(x, 0.7f, z),
                    Quaternion.Euler(0f, side > 0f ? -90f : 90f, 0f),
                    new Vector3(0.78f, 0.95f, 0.78f));
                Visual($"Indexed Status {i + 1}", PrimitiveType.Cube, shell,
                    new Vector3(x - side * 0.45f, 1.22f, z), new Vector3(0.05f, 0.12f, 0.5f),
                    (i & 1) == 0 ? primary : secondary);
            }

            Transform observation = Child(shell, option.Id == "C" ? "Archive Index Bay" : "Observation Bay");
            observation.localPosition = new Vector3(-side * 5.25f, 0f, 6.8f);
            Visual("Observation Base", PrimitiveType.Cube, observation, Vector3.zero,
                new Vector3(1.7f, 0.18f, 3.6f), dark);
            for (int i = 0; i < 3; i++)
                Visual($"Observation Slit {i + 1}", PrimitiveType.Cube, observation,
                    new Vector3(0f, 0.8f + i * 0.42f, 0f), new Vector3(0.07f, 0.12f, 2.7f),
                    i == 1 ? secondary : primary);
        }

        private static void BuildRoute(Transform root, Phase5AIdentityOptionDefinition option,
            Material primary, Material secondary, Material accent)
        {
            Transform route = Child(root, "Identity Route Grammar");
            float offset = option.Id == "B" ? 0.55f : option.Id == "C" ? -0.35f : 0f;
            for (int i = 0; i < 10; i++)
            {
                float z = -8f + i * 1.8f;
                float x = offset + (option.Id == "B" ? Mathf.Sin(i * 0.85f) * 0.42f : 0f);
                Visual($"Route Tick {i + 1:00}", PrimitiveType.Cube, route,
                    new Vector3(x, 0.11f, z), new Vector3(i % 3 == 0 ? 0.7f : 0.26f, 0.025f, 0.08f),
                    i < 4 ? primary : i < 7 ? secondary : accent);
            }
        }

        private static void BuildEchoChamber(Transform root, Phase5AIdentityOptionDefinition option,
            Material dark, Material primary, Material secondary, Material pale, Font font)
        {
            Visual("Chamber Plinth", PrimitiveType.Cylinder, root, new Vector3(0f, 0.15f, 0f),
                new Vector3(3.2f, 0.18f, 3.2f), dark);
            Visual("Chamber Active Surface", PrimitiveType.Cylinder, root, new Vector3(0f, 0.3f, 0f),
                new Vector3(2.55f, 0.03f, 2.55f), primary);
            Visual("Chamber Left Support", PrimitiveType.Cube, root, new Vector3(-2.35f, 2.25f, 0.18f),
                new Vector3(0.26f, 3.85f, 0.38f), dark);
            Visual("Chamber Right Support", PrimitiveType.Cube, root, new Vector3(2.35f, 2.25f, 0.18f),
                new Vector3(0.26f, 3.85f, 0.38f), dark);
            Visual("Chamber Header", PrimitiveType.Cube, root, new Vector3(0f, 4.25f, 0.18f),
                new Vector3(4.95f, 0.62f, 0.38f), dark);
            if (option.Id == "A")
            {
                BuildVerticalSegmentedRing(root, "Lattice Outer Ring", 1.92f, 20, 3, primary, 0f);
                BuildVerticalSegmentedRing(root, "Lattice Inner Ring", 1.46f, 16, 2, secondary, 11f);
                BuildVerticalSegmentedRing(root, "Recorded Offset Ring", 0.98f, 12, 1, pale, -8f);
            }
            else if (option.Id == "B")
            {
                for (int i = 0; i < 4; i++)
                {
                    Transform frame = Child(root, $"Relay Frame {i + 1}");
                    frame.localPosition = new Vector3((i - 1.5f) * 0.23f, 2.18f, i * 0.08f);
                    frame.localRotation = Quaternion.Euler(0f, 0f, (i - 1.5f) * 4f);
                    Material material = (i & 1) == 0 ? primary : secondary;
                    Visual("Left Trace", PrimitiveType.Cube, frame, new Vector3(-1.38f, 0f, 0f),
                        new Vector3(0.11f, 3.25f, 0.11f), material);
                    Visual("Right Trace", PrimitiveType.Cube, frame, new Vector3(1.38f, 0f, 0f),
                        new Vector3(0.11f, 3.25f, 0.11f), material);
                    Visual("Relay Crown", PrimitiveType.Cube, frame, new Vector3(0f, 1.58f, 0f),
                        new Vector3(2.85f, 0.11f, 0.11f), material);
                }
            }
            else
            {
                BuildVerticalSegmentedRing(root, "Archive Seal Outer", 1.95f, 6, 0, primary, 30f);
                BuildVerticalSegmentedRing(root, "Archive Seal Index", 1.48f, 6, 1, secondary, 0f);
                BuildVerticalSegmentedRing(root, "Archive Seal Core", 0.98f, 6, 2, pale, 30f);
                for (int i = 0; i < 3; i++)
                    Visual($"Archive Index {i + 1}", PrimitiveType.Cube, root,
                        new Vector3(-3.35f, 1.2f + i * 0.55f, 0f),
                        new Vector3(0.48f + i * 0.22f, 0.09f, 0.12f), i == 1 ? secondary : primary);
            }
            WorldText("READ ONLY // LOOP SIGNAL", root, new Vector3(0f, 0.62f, -2.9f),
                new Vector2(5f, 0.5f), 0.24f, option.Accent, font, TextAnchor.MiddleCenter,
                Quaternion.identity);
        }

        private static GameObject BuildActor(
            Transform parent, GameObject prefab, string name, Vector3 position, float yaw,
            Material accent, Material marker, Phase4RobotPoseState pose, bool echo,
            string optionId)
        {
            GameObject source = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Phase4RobotPoseController poseController =
                source.GetComponentInChildren<Phase4RobotPoseController>(true);
            if (poseController != null) poseController.ForcePoseForCapture(pose);
            PrefabUtility.UnpackPrefabInstance(source, PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);

            Transform visual = FindNamed(source.transform, "P4 Robot Visual");
            if (visual == null)
            {
                Object.DestroyImmediate(source);
                throw new InvalidOperationException(
                    $"{prefab.name} does not contain the expected P4 Robot Visual child.");
            }

            Vector3 visualPosition = visual.localPosition;
            Quaternion visualRotation = visual.localRotation;
            Vector3 visualScale = visual.localScale;
            visual.SetParent(null, true);
            Object.DestroyImmediate(source);

            GameObject actor = new GameObject(name);
            actor.transform.SetParent(parent, false);
            actor.transform.localPosition = position;
            actor.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            visual.SetParent(actor.transform, false);
            visual.localPosition = visualPosition;
            visual.localRotation = visualRotation;
            visual.localScale = visualScale;

            Renderer[] renderers = actor.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                string rendererName = renderers[i].name;
                if (rendererName.IndexOf("Marker", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    rendererName.IndexOf("Core", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    rendererName.IndexOf("Outline", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    rendererName.IndexOf("Fin", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    rendererName.IndexOf("Chevron", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    rendererName.IndexOf("Visor", StringComparison.OrdinalIgnoreCase) >= 0)
                    renderers[i].sharedMaterial = rendererName.IndexOf("Marker", StringComparison.OrdinalIgnoreCase) >= 0
                        ? marker : accent;
            }
            SetNamedActive(actor.transform, "Echo Segmented Hex Marker", echo);
            SetNamedActive(actor.transform, "Echo Stopped Mark", echo && pose == Phase4RobotPoseState.EchoStopped);
            SetNamedActive(actor.transform, "Echo Replaying Mark", echo && pose != Phase4RobotPoseState.EchoStopped);
            SetNamedActive(actor.transform, "Echo Identity Label", false);
            actor.transform.localScale = echo ? Vector3.one * 1.03f : Vector3.one * 1.12f;
            StripGameplayComponents(actor);
            return actor;
        }

        private static void BuildActorMotif(Transform actor, Phase5AIdentityOptionDefinition option,
            Material material, bool echo)
        {
            Transform motif = Child(actor, $"Option {option.Id} {(echo ? "Echo" : "Player")} Crest");
            motif.localPosition = new Vector3(0f, 1.92f, 0f);
            if (option.Id == "A")
            {
                Visual("Offset Tick Left", PrimitiveType.Cube, motif, new Vector3(-0.18f, 0f, 0f),
                    new Vector3(0.07f, 0.3f, 0.07f), material);
                Visual("Offset Tick Right", PrimitiveType.Cube, motif, new Vector3(0.18f, 0.1f, 0f),
                    new Vector3(0.07f, 0.3f, 0.07f), material);
            }
            else if (option.Id == "B")
            {
                for (int i = 0; i < (echo ? 3 : 2); i++)
                {
                    GameObject trace = Visual($"Relay Trace {i + 1}", PrimitiveType.Cube, motif,
                        new Vector3((i - 1f) * 0.14f, i * 0.08f, 0f),
                        new Vector3(0.08f, 0.32f, 0.06f), material);
                    trace.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
                }
            }
            else
            {
                int bars = echo ? 3 : 1;
                for (int i = 0; i < bars; i++)
                    Visual($"Archive Index Bar {i + 1}", PrimitiveType.Cube, motif,
                        new Vector3((i - (bars - 1) * 0.5f) * 0.14f, 0f, 0f),
                        new Vector3(0.08f, 0.34f - i * 0.05f, 0.06f), material);
            }
        }

        private static void BuildPlate(Transform parent, Vector3 position, Material dark,
            Material primary, Phase5AIdentityOptionDefinition option)
        {
            Transform root = Child(parent, "Pressure Plate Preview");
            root.localPosition = position;
            Visual("Plate Housing", PrimitiveType.Cube, root, Vector3.zero,
                new Vector3(2.55f, 0.16f, 2.55f), dark);
            Visual("Plate Active Field", PrimitiveType.Cube, root, new Vector3(0f, 0.12f, 0f),
                new Vector3(2.08f, 0.08f, 2.08f), primary);
            BuildFloorMotif(root, option, primary, 0.18f);
        }

        private static void BuildBatteryAndSocket(Transform parent, Vector3 batteryPosition,
            Vector3 socketPosition, Material dark, Material secondary,
            Phase5AIdentityOptionDefinition option)
        {
            Transform battery = Child(parent, "Battery Preview");
            battery.localPosition = batteryPosition + Vector3.up * 0.72f;
            GameObject core = Visual("Battery Core", PrimitiveType.Cylinder, battery, Vector3.zero,
                new Vector3(0.48f, 0.48f, 0.48f), secondary);
            core.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject cap = Visual($"Battery End {side}", PrimitiveType.Cylinder, battery,
                    new Vector3(0f, 0f, side * 0.46f), new Vector3(0.58f, 0.08f, 0.58f), dark);
                cap.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            }
            BuildDeviceMotif(battery, option, secondary, new Vector3(0f, 0.6f, -0.42f));

            Transform socket = Child(parent, "Power Socket Preview");
            socket.localPosition = socketPosition;
            Visual("Socket Base", PrimitiveType.Cube, socket, new Vector3(0f, 0.18f, 0f),
                new Vector3(1.65f, 0.34f, 1.65f), dark);
            Visual("Socket Energy Well", PrimitiveType.Cylinder, socket, new Vector3(0f, 0.42f, 0f),
                new Vector3(0.72f, 0.08f, 0.72f), secondary);
            BuildFloorMotif(socket, option, secondary, 0.52f);
        }

        private static void BuildDoor(Transform parent, Vector3 position,
            Phase4ExternalAssetCatalog catalog, Material dark, Material secondary,
            Phase5AIdentityOptionDefinition option)
        {
            Transform root = Child(parent, "Split Door Preview");
            root.localPosition = position;
            PrefabVisual("Authored Door Frame", catalog.DoorFrame, root,
                new Vector3(0f, 1.5f, 0f), Quaternion.identity, new Vector3(1.1f, 1f, 1f));
            GameObject left = PrefabVisual("Left Retracted Panel", catalog.DoorPanel, root,
                new Vector3(-1.52f, 1.5f, 0f), Quaternion.identity, new Vector3(0.5f, 1f, 1f));
            GameObject right = PrefabVisual("Right Retracted Panel", catalog.DoorPanel, root,
                new Vector3(1.52f, 1.5f, 0f), Quaternion.identity, new Vector3(0.5f, 1f, 1f));
            TintNamedRenderers(left, secondary);
            TintNamedRenderers(right, secondary);
            Visual("Door Identity Seam Left", PrimitiveType.Cube, root,
                new Vector3(-0.92f, 1.5f, -0.15f), new Vector3(0.07f, 2.55f, 0.08f), secondary);
            Visual("Door Identity Seam Right", PrimitiveType.Cube, root,
                new Vector3(0.92f, 1.5f, -0.15f), new Vector3(0.07f, 2.55f, 0.08f), secondary);
            BuildDeviceMotif(root, option, secondary, new Vector3(0f, 2.9f, -0.22f));
        }

        private static void BuildGoal(Transform parent, Vector3 position,
            Phase4ExternalAssetCatalog catalog, Material dark, Material pale, Material accent,
            Phase5AIdentityOptionDefinition option, Font font)
        {
            Transform root = Child(parent, "Goal Preview");
            root.localPosition = position;
            PrefabVisual("Goal Frame", catalog.DoorFrame, root, new Vector3(0f, 1.5f, 0f),
                Quaternion.identity, new Vector3(1.18f, 1.05f, 1f));
            Visual("Goal Dark Aperture", PrimitiveType.Cube, root, new Vector3(0f, 1.48f, 0.18f),
                new Vector3(3.55f, 2.72f, 0.16f), dark);
            BuildVerticalSegmentedRing(root, "Goal Identity Seal", 1.18f,
                option.Id == "C" ? 6 : 14, option.Id == "A" ? 2 : 1, accent,
                option.Id == "C" ? 30f : 0f);
            WorldText(option.Copy[7], root, new Vector3(0f, 2.88f, -0.12f),
                new Vector2(4f, 0.75f), 0.42f, option.Accent, font,
                TextAnchor.MiddleCenter, Quaternion.identity);
        }

        private static void BuildDecalBoard(Transform parent,
            Phase5AIdentityOptionDefinition option, Material dark, Material primary, Font font)
        {
            Transform board = Child(parent, "Phase 5A Decal Preview Board");
            board.localPosition = new Vector3(4.25f, 1.55f, 4.75f);
            board.localRotation = Quaternion.Euler(0f, -16f, 0f);
            Visual("Decal Backing", PrimitiveType.Cube, board, Vector3.zero,
                new Vector3(2.9f, 1.35f, 0.12f), dark);
            WorldText(option.Decals[7], board, new Vector3(0f, 0.18f, -0.08f),
                new Vector2(2.6f, 0.4f), 0.3f, option.Accent, font,
                TextAnchor.MiddleCenter, Quaternion.identity);
            WorldText(option.Decals[8], board, new Vector3(0f, -0.26f, -0.08f),
                new Vector2(2.6f, 0.35f), 0.22f, option.Primary, font,
                TextAnchor.MiddleCenter, Quaternion.identity);
            Visual("Decal Index Bar", PrimitiveType.Cube, board, new Vector3(-1.12f, 0.48f, -0.1f),
                new Vector3(0.44f, 0.08f, 0.03f), primary);
        }

        private static Camera BuildCamera(Transform root, Phase5AIdentityOptionDefinition option)
        {
            GameObject cameraObject = new GameObject("Phase 5A Preview Camera");
            cameraObject.transform.SetParent(root, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = option.Background;
            camera.fieldOfView = 44f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.allowHDR = true;
            camera.aspect = CaptureWidth / (float)CaptureHeight;
            return camera;
        }

        private static void BuildLights(Transform root, Phase5AIdentityOptionDefinition option,
            Material primary, Material secondary)
        {
            GameObject sunObject = new GameObject("Phase 5A Key Light");
            sunObject.transform.SetParent(root, false);
            sunObject.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = option.Accent;
            sun.intensity = 1.28f;
            sun.shadows = LightShadows.Soft;

            PointLight(root, "Chamber Light", new Vector3(-2.8f, 4.2f, -6.8f), option.Primary, 9f, 5.5f);
            PointLight(root, "Relay Light", new Vector3(3.8f, 3.5f, 2.5f), option.Secondary, 7f, 4.2f);
            PointLight(root, "Goal Light", new Vector3(0f, 4f, 9.2f), option.Accent, 8f, 4.6f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(option.Background, option.Accent, 0.14f);
        }

        private static Canvas BuildHud(Transform root, Camera camera,
            Phase5AIdentityOptionDefinition option, Font font, Material[] materials)
        {
            GameObject canvasObject = new GameObject($"Option {option.Id} HUD Preview");
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

            Image status = UiPanel("Status Capsule", canvas.transform,
                new Vector2(36f, -34f), new Vector2(430f, 118f), new Vector2(0f, 1f),
                new Color(option.Panel.r, option.Panel.g, option.Panel.b, 0.91f));
            UiText("Loop Copy", status.transform, option.Copy[2], new Vector2(18f, -16f),
                new Vector2(118f, 42f), new Vector2(0f, 1f), 25f, option.Accent, font,
                TextAnchor.MiddleLeft);
            UiText("Time Copy", status.transform, option.Copy[3], new Vector2(144f, -16f),
                new Vector2(132f, 42f), new Vector2(0f, 1f), 25f, option.Accent, font,
                TextAnchor.MiddleLeft);
            UiText("Echo Copy", status.transform, option.Copy[4], new Vector2(284f, -16f),
                new Vector2(126f, 42f), new Vector2(0f, 1f), 25f, option.Secondary, font,
                TextAnchor.MiddleLeft);
            UiText("Carry Copy", status.transform, option.Copy[5], new Vector2(18f, -65f),
                new Vector2(230f, 34f), new Vector2(0f, 1f), 20f, option.Secondary, font,
                TextAnchor.MiddleLeft);

            Image objective = UiPanel("Section Intro Preview", canvas.transform,
                new Vector2(0f, -30f), new Vector2(640f, 98f), new Vector2(0.5f, 1f),
                new Color(option.Panel.r, option.Panel.g, option.Panel.b, 0.88f));
            UiText("Section Copy", objective.transform, option.Copy[0], new Vector2(0f, -10f),
                new Vector2(590f, 34f), new Vector2(0.5f, 1f), 23f, option.Primary, font,
                TextAnchor.MiddleCenter);
            UiText("Objective Copy", objective.transform, option.Copy[1], new Vector2(0f, -46f),
                new Vector2(590f, 38f), new Vector2(0.5f, 1f), 22f, option.Accent, font,
                TextAnchor.MiddleCenter);

            Image prompt = UiPanel("Context Prompt", canvas.transform,
                new Vector2(-34f, -34f), new Vector2(210f, 58f), new Vector2(1f, 1f),
                new Color(option.Panel.r, option.Panel.g, option.Panel.b, 0.9f));
            UiText("Prompt Copy", prompt.transform, option.Copy[6], Vector2.zero,
                new Vector2(190f, 50f), new Vector2(0.5f, 0.5f), 24f, option.Accent, font,
                TextAnchor.MiddleCenter);
            return canvas;
        }

        private static void RenderShots(
            Phase5AIdentityOptionDefinition option, Transform root, Camera camera,
            Canvas hud, string outputDirectory)
        {
            Vector3[] positions =
            {
                new Vector3(0f, 4.9f, -17.8f),
                new Vector3(11.8f, 19.5f, -13.8f),
                new Vector3(6.4f, 5.2f, -7.4f),
                new Vector3(7.2f, 5.1f, -0.6f),
                new Vector3(8.8f, 9.4f, 3.1f)
            };
            Vector3[] targets =
            {
                new Vector3(0f, 2.25f, -7.1f),
                new Vector3(0f, 0.1f, 0.5f),
                new Vector3(0f, 1.0f, -0.7f),
                new Vector3(0.7f, 1.1f, 3.5f),
                new Vector3(0f, 1.2f, 8.2f)
            };
            for (int i = 0; i < CaptureNames.Length; i++)
            {
                hud.gameObject.SetActive(i == 4);
                camera.transform.position = positions[i];
                camera.transform.rotation = Quaternion.LookRotation(targets[i] - positions[i], Vector3.up);
                camera.fieldOfView = i == 1 ? 48f : i == 2 || i == 3 ? 39f : 43f;
                Render(camera, Path.Combine(outputDirectory, CaptureNames[i]));
            }
            hud.gameObject.SetActive(false);
        }

        private static void WriteSvgAtlas(Phase5AIdentityOptionDefinition option, string folder)
        {
            StringBuilder svg = new StringBuilder(8192);
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"1024\" viewBox=\"0 0 1024 1024\">");
            svg.AppendFormat(CultureInfo.InvariantCulture,
                "<rect width=\"1024\" height=\"1024\" fill=\"#{0}\"/>", ColorHex(option.Background));
            for (int i = 0; i < option.Decals.Length; i++)
            {
                int column = i % 3;
                int row = i / 3;
                int x = column * 341;
                int y = row * 256;
                string color = i % 3 == 0 ? ColorHex(option.Primary) :
                    i % 3 == 1 ? ColorHex(option.Secondary) : ColorHex(option.Accent);
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<g id=\"decal-{0:00}\"><rect x=\"{1}\" y=\"{2}\" width=\"325\" height=\"238\" rx=\"8\" fill=\"none\" stroke=\"#{3}\" stroke-width=\"3\"/>",
                    i + 1, x + 8, y + 8, color);
                AppendSvgMotif(svg, option.Id, x + 34, y + 54, color);
                svg.AppendFormat(CultureInfo.InvariantCulture,
                    "<text x=\"{0}\" y=\"{1}\" fill=\"#{2}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"{3}\" font-weight=\"700\" letter-spacing=\"2\">{4}</text></g>",
                    x + 34, y + 170, color, option.Decals[i].Length > 18 ? 21 : 27,
                    EscapeXml(option.Decals[i]));
            }
            svg.Append("</svg>");
            string path = $"{folder}/P5A_{option.Id}_DecalAtlas.svg";
            File.WriteAllText(AbsoluteAssetPath(path), svg.ToString(), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static void WriteLogoSvg(Phase5AIdentityOptionDefinition option, string folder)
        {
            StringBuilder svg = new StringBuilder(2048);
            svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1024\" height=\"320\" viewBox=\"0 0 1024 320\">");
            svg.AppendFormat("<rect width=\"1024\" height=\"320\" fill=\"#{0}\"/>", ColorHex(option.Background));
            AppendSvgMotif(svg, option.Id, 70, 70, ColorHex(option.Primary));
            svg.AppendFormat("<text x=\"250\" y=\"154\" fill=\"#{0}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"92\" font-weight=\"800\" letter-spacing=\"10\">ECHO SHIFT</text>", ColorHex(option.Accent));
            svg.AppendFormat("<text x=\"256\" y=\"214\" fill=\"#{0}\" font-family=\"Noto Sans JP, sans-serif\" font-size=\"28\" letter-spacing=\"5\">{1}</text>", ColorHex(option.Secondary), option.Concept.ToUpperInvariant());
            svg.Append("</svg>");
            string path = $"{folder}/P5A_{option.Id}_Logo.svg";
            File.WriteAllText(AbsoluteAssetPath(path), svg.ToString(), new UTF8Encoding(false));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

        private static void AppendSvgMotif(StringBuilder svg, string id, int x, int y, string color)
        {
            if (id == "A")
            {
                svg.AppendFormat("<circle cx=\"{0}\" cy=\"{1}\" r=\"55\" fill=\"none\" stroke=\"#{2}\" stroke-width=\"12\" stroke-dasharray=\"210 70\"/><circle cx=\"{0}\" cy=\"{1}\" r=\"34\" fill=\"none\" stroke=\"#{2}\" stroke-width=\"6\" stroke-dasharray=\"80 28\" transform=\"rotate(28 {0} {1})\"/>", x + 58, y + 34, color);
            }
            else if (id == "B")
            {
                svg.AppendFormat("<path d=\"M{0} {1} L{2} {3} L{0} {4}\" fill=\"none\" stroke=\"#{5}\" stroke-width=\"12\"/><path d=\"M{6} {1} L{7} {3} L{6} {4}\" fill=\"none\" stroke=\"#{5}\" stroke-width=\"6\"/>", x, y, x + 52, y + 34, y + 68, color, x + 20, x + 72);
            }
            else
            {
                svg.AppendFormat("<path d=\"M{0} {1} l34 -20 l34 20 v40 l-34 20 l-34 -20 z\" fill=\"none\" stroke=\"#{2}\" stroke-width=\"10\" stroke-dasharray=\"62 14\"/><path d=\"M{3} {4} v50\" stroke=\"#{2}\" stroke-width=\"7\"/>", x, y, color, x + 86, y - 2);
            }
        }

        private static void WriteAudioPreview(Phase5AIdentityOptionDefinition option, string path)
        {
            const int sampleRate = 44100;
            const int outputSamples = sampleRate * 10;
            float[] mix = new float[outputSamples];
            AudioClip echo = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioRoot + "forceField_000.ogg");
            AudioClip click = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioRoot + "laserSmall_000.ogg");
            AudioClip impact = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioRoot + "impactMetal_001.ogg");
            if (echo == null || click == null || impact == null)
                throw new InvalidOperationException("Required Kenney source audio is missing.");
            float[] echoData = ReadMono(echo);
            float[] clickData = ReadMono(click);
            float[] impactData = ReadMono(impact);
            for (int i = 0; i < option.MotifPitches.Length; i++)
                MixResampled(clickData, click.frequency, mix, sampleRate,
                    0.55f + i * 0.48f, option.MotifPitches[i], 0.42f);
            MixResampled(echoData, echo.frequency, mix, sampleRate, 2.6f,
                option.Id == "B" ? 1.12f : option.Id == "C" ? 0.82f : 1f, 0.34f);
            MixResampled(impactData, impact.frequency, mix, sampleRate, 4.2f,
                option.Id == "A" ? 1.18f : option.Id == "B" ? 0.94f : 0.74f, 0.27f);
            for (int i = 0; i < option.MotifPitches.Length; i++)
                MixResampled(clickData, click.frequency, mix, sampleRate,
                    6.15f + i * 0.38f, option.MotifPitches[i], 0.28f);

            ApplyOnePoleLowPass(mix, option.Id == "A" ? 0.42f : option.Id == "B" ? 0.27f : 0.16f);
            AddDelay(mix, sampleRate, option.Id == "A" ? 0.09f : option.Id == "B" ? 0.075f : 0.14f,
                option.Id == "B" ? 0.26f : 0.2f);
            AddReverseTail(echoData, echo.frequency, mix, sampleRate, 5.35f,
                option.Id == "C" ? 0.18f : 0.11f);
            Normalize(mix, 0.88f);
            WritePcm16Wave(path, mix, sampleRate);
        }

        private static float[] ReadMono(AudioClip clip)
        {
            clip.LoadAudioData();
            float[] interleaved = new float[clip.samples * clip.channels];
            if (!clip.GetData(interleaved, 0))
                throw new InvalidOperationException($"Could not read source AudioClip {clip.name}.");
            float[] mono = new float[clip.samples];
            for (int sample = 0; sample < clip.samples; sample++)
            {
                float value = 0f;
                for (int channel = 0; channel < clip.channels; channel++)
                    value += interleaved[sample * clip.channels + channel];
                mono[sample] = value / clip.channels;
            }
            return mono;
        }

        private static void MixResampled(float[] source, int sourceRate, float[] target, int targetRate,
            float startSeconds, float pitch, float gain)
        {
            int start = Mathf.RoundToInt(startSeconds * targetRate);
            int available = target.Length - start;
            int count = Mathf.Min(available, Mathf.FloorToInt(source.Length / pitch * targetRate / sourceRate));
            for (int i = 0; i < count; i++)
            {
                float sourcePosition = i * pitch * sourceRate / targetRate;
                int index = Mathf.FloorToInt(sourcePosition);
                if (index + 1 >= source.Length) break;
                float fraction = sourcePosition - index;
                target[start + i] += Mathf.Lerp(source[index], source[index + 1], fraction) * gain;
            }
        }

        private static void ApplyOnePoleLowPass(float[] samples, float coefficient)
        {
            float previous = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                previous += coefficient * (samples[i] - previous);
                samples[i] = previous;
            }
        }

        private static void AddDelay(float[] samples, int sampleRate, float seconds, float feedback)
        {
            int offset = Mathf.Max(1, Mathf.RoundToInt(seconds * sampleRate));
            for (int i = offset; i < samples.Length; i++) samples[i] += samples[i - offset] * feedback;
        }

        private static void AddReverseTail(float[] source, int sourceRate, float[] target,
            int targetRate, float startSeconds, float gain)
        {
            int start = Mathf.RoundToInt(startSeconds * targetRate);
            int count = Mathf.Min(Mathf.RoundToInt(0.75f * targetRate), target.Length - start);
            for (int i = 0; i < count; i++)
            {
                int sourceIndex = source.Length - 1 - Mathf.FloorToInt(i * sourceRate / (float)targetRate);
                if (sourceIndex < 0) break;
                target[start + i] += source[sourceIndex] * gain * (i / (float)Mathf.Max(1, count - 1));
            }
        }

        private static void Normalize(float[] samples, float targetPeak)
        {
            float peak = 0f;
            for (int i = 0; i < samples.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(samples[i]));
            if (peak < 0.0001f) throw new InvalidOperationException("Generated audio preview is silent.");
            float gain = targetPeak / peak;
            for (int i = 0; i < samples.Length; i++) samples[i] *= gain;
        }

        private static void WritePcm16Wave(string path, float[] samples, int sampleRate)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                int dataLength = samples.Length * 2;
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataLength);
                writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataLength);
                for (int i = 0; i < samples.Length; i++)
                    writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(samples[i], -1f, 1f) * 32767f));
            }
        }

        private static void ValidateOutputs(
            Phase5AIdentityOptionDefinition option, string outputDirectory, string scenePath)
        {
            if (option.Decals.Length != RequiredDecalCount)
                throw new InvalidOperationException($"Option {option.Id} decal count is invalid.");
            if (option.MotifPitches.Length == 0 || option.MotifPitches.Length > 3)
                throw new InvalidOperationException($"Option {option.Id} audio motif exceeds three notes.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                throw new InvalidOperationException($"Option {option.Id} preview scene was not saved.");
            for (int i = 0; i < CaptureNames.Length; i++)
            {
                string path = Path.Combine(outputDirectory, CaptureNames[i]);
                if (!File.Exists(path) || new FileInfo(path).Length < 30000)
                    throw new InvalidOperationException($"Option {option.Id} capture is missing: {path}");
                byte[] png = File.ReadAllBytes(path);
                if (ReadBigEndianInt32(png, 16) != CaptureWidth ||
                    ReadBigEndianInt32(png, 20) != CaptureHeight)
                    throw new InvalidOperationException($"Option {option.Id} capture size is invalid: {path}");
            }
            string wav = Path.Combine(outputDirectory, "audio_preview.wav");
            if (!File.Exists(wav) || new FileInfo(wav).Length <= 44)
                throw new InvalidOperationException($"Option {option.Id} audio preview is missing.");
            double seconds = (new FileInfo(wav).Length - 44d) / (44100d * 2d);
            if (seconds > MaximumAudioSeconds)
                throw new InvalidOperationException($"Option {option.Id} audio preview is too long: {seconds:0.000}s");
        }

        private static Material[] BuildMaterials(Phase5AIdentityOptionDefinition option, string folder)
        {
            return new[]
            {
                MaterialAsset(folder, $"P5A_{option.Id}_Dark", option.Background, 0.7f, 0.72f, 0f),
                MaterialAsset(folder, $"P5A_{option.Id}_Panel", option.Panel, 0.62f, 0.68f, 0f),
                MaterialAsset(folder, $"P5A_{option.Id}_Primary", option.Primary, 0.28f, 0.56f, 1.05f),
                MaterialAsset(folder, $"P5A_{option.Id}_Secondary", option.Secondary, 0.24f, 0.54f, 1.0f),
                MaterialAsset(folder, $"P5A_{option.Id}_Accent", option.Accent, 0.16f, 0.42f, 0.55f),
                MaterialAsset(folder, $"P5A_{option.Id}_Pale", Color.Lerp(option.Accent, Color.white, 0.24f),
                    0.08f, 0.48f, 0.32f)
            };
        }

        private static Material MaterialAsset(string folder, string name, Color color,
            float metallic, float smoothness, float emission)
        {
            string path = $"{folder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetColor("_EmissionColor", color * emission);
            if (emission > 0f) material.EnableKeyword("_EMISSION");
            else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildVerticalSegmentedRing(Transform parent, string name, float radius,
            int segments, int gapEvery, Material material, float angleOffset)
        {
            Transform ring = Child(parent, name);
            ring.localPosition = new Vector3(0f, radius + 0.45f, 0f);
            float arc = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                if (gapEvery > 0 && i % (gapEvery + 4) == gapEvery) continue;
                float angle = (angleOffset + i * arc) * Mathf.Deg2Rad;
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                GameObject segment = Visual($"Ring Segment {i + 1:00}", PrimitiveType.Cube, ring,
                    position, new Vector3(radius * arc * Mathf.Deg2Rad * 0.72f, 0.12f, 0.12f), material);
                segment.transform.localRotation = Quaternion.Euler(0f, 0f, angleOffset + i * arc + 90f);
            }
        }

        private static void BuildFloorMotif(Transform parent, Phase5AIdentityOptionDefinition option,
            Material material, float y)
        {
            Transform root = Child(parent, $"{option.Concept} Floor Mark");
            root.localPosition = new Vector3(0f, y, 0f);
            if (option.Id == "A")
            {
                for (int i = 0; i < 8; i++)
                {
                    float a = i * 45f * Mathf.Deg2Rad;
                    GameObject segment = Visual($"Lattice Tick {i + 1}", PrimitiveType.Cube, root,
                        new Vector3(Mathf.Cos(a) * 0.72f, 0f, Mathf.Sin(a) * 0.72f),
                        new Vector3(0.32f, 0.025f, 0.07f), material);
                    segment.transform.localRotation = Quaternion.Euler(0f, -i * 45f, 0f);
                }
            }
            else if (option.Id == "B")
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject chevron = Visual($"Relay Chevron {i + 1}", PrimitiveType.Cube, root,
                        new Vector3((i - 1) * 0.36f, 0f, i * 0.22f),
                        new Vector3(0.08f, 0.025f, 0.72f), material);
                    chevron.transform.localRotation = Quaternion.Euler(0f, -28f, 0f);
                }
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    GameObject side = Visual($"Seal Side {i + 1}", PrimitiveType.Cube, root,
                        new Vector3(Mathf.Cos(a) * 0.72f, 0f, Mathf.Sin(a) * 0.72f),
                        new Vector3(0.62f, 0.025f, 0.06f), material);
                    side.transform.localRotation = Quaternion.Euler(0f, -i * 60f, 0f);
                }
            }
        }

        private static void BuildDeviceMotif(Transform parent, Phase5AIdentityOptionDefinition option,
            Material material, Vector3 localPosition)
        {
            Transform root = Child(parent, $"{option.Concept} Device Crest");
            root.localPosition = localPosition;
            if (option.Id == "A")
            {
                Visual("Lattice Left", PrimitiveType.Cube, root, new Vector3(-0.16f, 0f, 0f),
                    new Vector3(0.08f, 0.5f, 0.07f), material);
                Visual("Lattice Right", PrimitiveType.Cube, root, new Vector3(0.16f, 0.1f, 0f),
                    new Vector3(0.08f, 0.5f, 0.07f), material);
            }
            else if (option.Id == "B")
            {
                for (int i = 0; i < 2; i++)
                {
                    GameObject slash = Visual($"Relay Slash {i + 1}", PrimitiveType.Cube, root,
                        new Vector3((i - 0.5f) * 0.24f, 0f, 0f), new Vector3(0.08f, 0.55f, 0.07f), material);
                    slash.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                    Visual($"Index {i + 1}", PrimitiveType.Cube, root,
                        new Vector3((i - 1) * 0.18f, 0f, 0f),
                        new Vector3(0.08f, 0.55f - i * 0.12f, 0.07f), material);
            }
        }

        private static Image UiPanel(string name, Transform parent, Vector2 anchoredPosition,
            Vector2 size, Vector2 anchor, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = obj.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void UiText(string name, Transform parent, string value,
            Vector2 anchoredPosition, Vector2 size, Vector2 anchor, float fontSize,
            Color color, Font font, TextAnchor alignment)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Text text = obj.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = Mathf.RoundToInt(fontSize);
            text.color = color;
            text.alignment = alignment;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private static TextMesh WorldText(string name, Transform parent, Vector3 position,
            Vector2 size, float fontSize, Color color, Font font,
            TextAnchor alignment, Quaternion rotation)
        {
            GameObject obj = new GameObject(name, typeof(TextMesh));
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localRotation = rotation;
            TextMesh text = obj.GetComponent<TextMesh>();
            text.text = name;
            text.font = font;
            text.fontSize = 64;
            text.characterSize = fontSize * 0.095f;
            text.color = color;
            text.anchor = alignment;
            text.alignment = TextAlignment.Center;
            text.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            return text;
        }

        private static void PointLight(Transform parent, string name, Vector3 position,
            Color color, float range, float intensity)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            Light light = obj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        private static GameObject PrefabVisual(string name, GameObject prefab, Transform parent,
            Vector3 position, Quaternion rotation, Vector3 scale)
        {
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
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localScale = scale;
            Object.DestroyImmediate(obj.GetComponent<Collider>());
            obj.GetComponent<Renderer>().sharedMaterial = material;
            return obj;
        }

        private static Transform Child(Transform parent, string name)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj.transform;
        }

        private static void StripGameplayComponents(GameObject root)
        {
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) Object.DestroyImmediate(colliders[i]);
            Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < rigidbodies.Length; i++) Object.DestroyImmediate(rigidbodies[i]);
            MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++) Object.DestroyImmediate(behaviours[i]);
            Animator[] animators = root.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++) Object.DestroyImmediate(animators[i]);
        }

        private static Transform FindNamed(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == name) return transforms[i];
            return null;
        }

        private static void SetNamedActive(Transform root, string name, bool active)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i].name == name) transforms[i].gameObject.SetActive(active);
        }

        private static void TintNamedRenderers(GameObject root, Material material)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++) renderers[i].sharedMaterial = material;
        }

        private static void Render(Camera camera, string path)
        {
            RenderTexture target = new RenderTexture(CaptureWidth, CaptureHeight, 24,
                RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            Texture2D image = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            image.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
            target.Release();
            Object.DestroyImmediate(target);
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
            (bytes[offset + 2] << 8) | bytes[offset + 3];

        private static string[] Decals(string system, string action) => new[]
        {
            "ECHO SHIFT", "SECTION 01", "SECTION 02", "SECTION 03", "HAZARD",
            "ECHO TEST // " + system, "ROUTE ->", "MAINTENANCE", "ES-05A-031",
            "CAUTION // " + action, "INSPECTED", "OUT OF SERVICE"
        };

        private static Color Hex(string value)
        {
            if (!ColorUtility.TryParseHtmlString("#" + value, out Color color))
                throw new InvalidOperationException($"Invalid color {value}.");
            return color;
        }

        private static string ColorHex(Color color) => ColorUtility.ToHtmlStringRGB(color);
        private static string EscapeXml(string value) => value.Replace("&", "&amp;")
            .Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

        private static string AbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            Directory.CreateDirectory(AbsoluteAssetPath(assetPath));
            AssetDatabase.Refresh();
        }
    }
}
