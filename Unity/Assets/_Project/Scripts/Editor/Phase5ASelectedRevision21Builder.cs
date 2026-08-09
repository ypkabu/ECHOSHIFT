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

namespace EchoShift.Editor
{
    public static partial class Phase5AIdentityPreviewBuilder
    {
        public const int Revision21CaptureCount = 14;
        public const string Revision21ArtRoot =
            "Assets/_Project/Art/Phase5A/SelectedRevision21";
        public const string Revision21ScenePath =
            "Assets/_Project/Scenes/Preview/Phase5A_SelectedRevision21.unity";

        private const string Revision21CaptureFolder = "SelectedRevision21";

        private static readonly string[] Revision21CaptureNames =
        {
            "01_logo_horizontal.png",
            "02_logo_icon_monochrome.png",
            "03_logo_actual_64px_comparison.png",
            "04_chamber_main.png",
            "05_chamber_isometric.png",
            "06_actor_surface_motif_close.png",
            "07_goal_locked.png",
            "08_goal_unlocked.png",
            "09_maintenance_bay.png",
            "10_observation_bay.png",
            "11_real_decal_closeup.png",
            "12_gameplay_overview.png",
            "13_gameplay_goal_readability.png",
            "14_gameplay_actor_readability.png"
        };

        private static readonly string[] Revision21AudioNames =
        {
            "echo_spawn_03.wav",
            "section_complete.wav",
            "spawn03_section_complete_comparison.wav"
        };

        public static IReadOnlyList<string> Revision21Captures => Revision21CaptureNames;
        public static IReadOnlyList<string> Revision21AudioFiles => Revision21AudioNames;

        [MenuItem("ECHO SHIFT/Phase 5A/Generate Selected Revision 2.1")]
        public static void GenerateSelectedRevision21()
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

            EnsureAssetFolder(Revision21ArtRoot);
            EnsureAssetFolder(SceneRoot);
            Material[] revision2Materials = LoadRevision2MaterialsForRevision21();
            Material[] revision21Materials = BuildRevision21Materials();
            WriteRevision21ProductionHashManifest();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("Phase5A Selected Revision 2.1 Preview");
            BuildRevision2World(root.transform, settings, catalog, echoPrefab, revision2Materials,
                out Revision2PreviewReferences revision2);
            ApplyRevision21VisualCorrections(root.transform, revision2, revision2Materials,
                revision21Materials);
            Revision21LogoEvidence logoEvidence = BuildRevision21LogoEvidence(
                root.transform, revision2.Camera, settings.PackagedJapaneseFont,
                revision2Materials[0].color, revision2Materials[8].color,
                revision2Materials[3].color, revision2Materials[4].color);
            Revision21PreviewReferences preview = new Revision21PreviewReferences(
                revision2, logoEvidence);
            preview.DisableAll();
            EditorSceneManager.SaveScene(scene, Revision21ScenePath);

            string repositoryRoot = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            if (string.IsNullOrEmpty(repositoryRoot))
                throw new InvalidOperationException("Repository root could not be resolved.");
            string captureDirectory = Path.Combine(repositoryRoot, "Captures", "Phase5A",
                Revision21CaptureFolder);
            string audioDirectory = Path.Combine(captureDirectory, "Audio");
            Directory.CreateDirectory(audioDirectory);
            Cursor.visible = false;
            RenderRevision21(preview, captureDirectory);
            WriteRevision21Audio(audioDirectory);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateRevision21(captureDirectory, audioDirectory);
            Debug.Log($"PHASE5A_SELECTED_REVISION21_OK captures={Revision21CaptureCount};" +
                      $"audio={Revision21AudioNames.Length};graphics={SystemInfo.graphicsDeviceType}");
        }

        public static void GenerateSelectedRevision21FromCommandLine()
        {
            GenerateSelectedRevision21();
        }

        private static Material[] LoadRevision2MaterialsForRevision21()
        {
            string[] names =
            {
                "P5A_R2_Dark", "P5A_R2_Panel", "P5A_R2_Metal", "P5A_R2_Violet",
                "P5A_R2_Cyan", "P5A_R2_Amber", "P5A_R2_LimeSuccess",
                "P5A_R2_RedClosed", "P5A_R2_White"
            };
            Material[] materials = new Material[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                string path = $"{Revision2ArtRoot}/{names[i]}.mat";
                materials[i] = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (materials[i] == null)
                    throw new InvalidOperationException($"Revision 2 material is missing: {path}");
            }
            return materials;
        }

        private static Material[] BuildRevision21Materials()
        {
            return new[]
            {
                MaterialAsset(Revision21ArtRoot, "P5A_R21_GoalBed", Hex("102333"),
                    0.5f, 0.42f, 0.02f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_GoalViolet", Hex("A38BE8"),
                    0.18f, 0.42f, 0.54f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_GoalNeutral", Hex("E7EEF3"),
                    0.1f, 0.36f, 0.18f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_ArcVioletDim", Hex("7562B0"),
                    0.28f, 0.4f, 0.18f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_ArcCyanDim", Hex("4C9EAB"),
                    0.26f, 0.4f, 0.16f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_MonitorFace", Hex("1D3A4D"),
                    0.34f, 0.38f, 0.1f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_MonitorSignal", Hex("A894EB"),
                    0.18f, 0.4f, 0.46f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_ActorVioletSurface", Hex("806BC2"),
                    0.3f, 0.4f, 0.14f),
                MaterialAsset(Revision21ArtRoot, "P5A_R21_ActorCyanSurface", Hex("51B5C4"),
                    0.28f, 0.4f, 0.14f)
            };
        }

        private static void ApplyRevision21VisualCorrections(Transform root,
            Revision2PreviewReferences preview, Material[] revision2, Material[] revision21)
        {
            ApplyRevision21ActorMotifs(preview.Actors.transform, revision21[7], revision21[8]);
            ApplyRevision21Goals(root, revision2[6], revision21[0], revision21[1],
                revision21[2]);
            ApplyRevision21Chamber(preview.Chamber.transform, revision21[3], revision21[4]);
            ApplyRevision21Observation(preview.Observation.transform, revision21[5],
                revision21[6]);
        }

        private static void ApplyRevision21ActorMotifs(Transform actors, Material violetSurface,
            Material cyanSurface)
        {
            string[] actorNames =
            {
                "Current Player Surface Motif", "Echo 1 Surface Motif",
                "Echo 2 Surface Motif", "Echo 3 Surface Motif"
            };
            for (int i = 0; i < actorNames.Length; i++)
            {
                Transform actor = FindNamed(actors, actorNames[i]);
                if (actor == null)
                    throw new InvalidOperationException($"Revision 2.1 actor is missing: {actorNames[i]}");

                Material surface = i == 0 ? cyanSurface : violetSurface;
                Transform strip = FindNamed(actor, "Chest Embedded Emission Strip");
                strip.localPosition = new Vector3(0f, 0.58f, -0.49f);
                strip.localScale = new Vector3(0.28f, 0.035f, 0.01f);
                strip.GetComponent<Renderer>().sharedMaterial = surface;

                Transform seal = FindNamed(actor, "Chest Surface Split Seal");
                seal.localPosition = new Vector3(0f, 0.42f, -0.492f);
                seal.localScale = Vector3.one * 0.48f;
                Renderer[] sealParts = seal.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < sealParts.Length; j++)
                    sealParts[j].sharedMaterial = surface;

                Transform ticks = FindNamed(actor, "Back Panel Surface Generation Ticks");
                Transform[] tickParts = ticks.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < tickParts.Length; j++)
                {
                    if (!tickParts[j].name.StartsWith("Embedded Surface Tick",
                            StringComparison.Ordinal)) continue;
                    Vector3 position = tickParts[j].localPosition;
                    tickParts[j].localPosition = new Vector3(position.x * 0.58f, 0.65f, 0.48f);
                    tickParts[j].localScale = new Vector3(0.055f, 0.018f, 0.008f);
                    tickParts[j].GetComponent<Renderer>().sharedMaterial = surface;
                }

                Transform floor = FindNamed(actor, "Grounded Segmented Identity Marker");
                Transform[] floorParts = floor.GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < floorParts.Length; j++)
                {
                    if (!floorParts[j].name.StartsWith("Floor Seal Segment",
                            StringComparison.Ordinal)) continue;
                    Vector3 position = floorParts[j].localPosition;
                    Vector3 scale = floorParts[j].localScale;
                    floorParts[j].localPosition = new Vector3(position.x * 0.52f, 0.006f,
                        position.z * 0.52f);
                    floorParts[j].localScale = new Vector3(scale.x * 0.52f, 0.006f,
                        scale.z * 0.52f);
                    floorParts[j].GetComponent<Renderer>().sharedMaterial = surface;
                }
            }
        }

        private static void ApplyRevision21Goals(Transform root, Material lime,
            Material goalBed, Material goalViolet, Material goalNeutral)
        {
            string[] goalNames =
            {
                "Locked Goal State", "Unlocked Goal State", "Revision 2 Section Goal Unlocked"
            };
            for (int i = 0; i < goalNames.Length; i++)
            {
                Transform goal = FindNamed(root, goalNames[i]);
                if (goal == null)
                    throw new InvalidOperationException($"Revision 2.1 goal is missing: {goalNames[i]}");
                bool unlocked = goalNames[i] != "Locked Goal State";

                Renderer bed = FindNamed(goal, "Floor Embedded Goal Bed")?.GetComponent<Renderer>();
                if (bed != null) bed.sharedMaterial = goalBed;

                for (int child = 0; child < goal.childCount; child++)
                {
                    Transform segment = goal.GetChild(child);
                    if (!segment.name.StartsWith("Floor Seal Segment", StringComparison.Ordinal))
                        continue;
                    segment.localPosition = new Vector3(segment.localPosition.x, 0.085f,
                        segment.localPosition.z);
                    segment.localScale = new Vector3(segment.localScale.x, 0.055f, 0.19f);
                }

                Transform signal = FindNamed(goal, "Goal Outer Identity Signal");
                Renderer[] signalRenderers = signal.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < signalRenderers.Length; j++)
                {
                    signalRenderers[j].sharedMaterial = goalViolet;
                    Transform segment = signalRenderers[j].transform;
                    segment.localPosition = new Vector3(segment.localPosition.x, 0.118f,
                        segment.localPosition.z);
                    segment.localScale = new Vector3(segment.localScale.x * 1.03f, 0.038f, 0.10f);
                }

                Transform inner = FindNamed(goal, "Goal Inner Three Segments");
                Renderer[] innerRenderers = inner.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < innerRenderers.Length; j++)
                {
                    bool center = innerRenderers[j].name == "Goal Inner Segment 2";
                    innerRenderers[j].sharedMaterial = center && unlocked ? lime :
                        center ? goalNeutral : goalViolet;
                    innerRenderers[j].transform.localScale = new Vector3(0.56f, 0.035f, 0.17f);
                    innerRenderers[j].transform.localPosition = new Vector3(
                        innerRenderers[j].transform.localPosition.x, 0.125f, 0.2f);
                }

                Transform registration = Child(goal, "Neutral Goal Registration Marks");
                BuildRevision2HorizontalSeal(registration, 1.40f, 0.126f, -1,
                    goalNeutral, 0.082f, 0.032f, new[] { 1, 3 });

                if (!unlocked) continue;
                Transform completion = FindNamed(goal, "Unlocked Entrance Completion Segment");
                Renderer[] completionRenderers = completion.GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < completionRenderers.Length; j++)
                {
                    completionRenderers[j].sharedMaterial = lime;
                    completionRenderers[j].transform.localPosition = new Vector3(
                        completionRenderers[j].transform.localPosition.x, 0.137f,
                        completionRenderers[j].transform.localPosition.z);
                    completionRenderers[j].transform.localScale = new Vector3(
                        completionRenderers[j].transform.localScale.x, 0.042f, 0.115f);
                }
                Transform extension = Child(goal, "Unlocked Success Perimeter Extension");
                BuildRevision2HorizontalSeal(extension, 1.42f, 0.137f, -1, lime,
                    0.115f, 0.042f, new[] { 0, 4 });
            }
        }

        private static void ApplyRevision21Chamber(Transform chamber, Material dimViolet,
            Material dimCyan)
        {
            Transform holograms = FindNamed(chamber, "Supported Internal Hologram Arcs");
            Renderer[] renderers = holograms.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                bool cyan = renderers[i].sharedMaterial != null &&
                    renderers[i].sharedMaterial.name.IndexOf("Cyan",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                renderers[i].sharedMaterial = cyan ? dimCyan : dimViolet;
                Vector3 scale = renderers[i].transform.localScale;
                renderers[i].transform.localScale = new Vector3(scale.x, scale.y * 0.68f,
                    scale.z * 0.68f);
            }
        }

        private static void ApplyRevision21Observation(Transform observation,
            Material monitorFace, Material monitorSignal)
        {
            Transform recess = FindNamed(observation, "Monitor Recess");
            if (recess != null) recess.GetComponent<Renderer>().sharedMaterial = monitorFace;
            for (int i = 1; i <= 3; i++)
            {
                Transform indicator = FindNamed(observation,
                    $"Embedded Echo Generation Indicator {i}");
                if (indicator == null) continue;
                indicator.GetComponent<Renderer>().sharedMaterial = monitorSignal;
                Vector3 scale = indicator.localScale;
                indicator.localScale = new Vector3(scale.x * 1.12f, scale.y * 1.18f,
                    scale.z);
            }
            Transform record = FindNamed(observation, "Record Status Decal");
            TextMesh text = record != null ? record.GetComponent<TextMesh>() : null;
            if (text != null) text.color = new Color(monitorSignal.color.r,
                monitorSignal.color.g, monitorSignal.color.b, 0.96f);
        }

        private static Revision21LogoEvidence BuildRevision21LogoEvidence(Transform root,
            Camera camera, Font font, Color dark, Color white, Color violet, Color cyan)
        {
            GameObject canvasObject = new GameObject("Revision 2.1 Exact Pixel Logo Evidence");
            canvasObject.transform.SetParent(root, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.35f;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(CaptureWidth, CaptureHeight);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform horizontal = LogoEvidencePanel("Logo Horizontal Evidence",
                canvas.transform, dark);
            BuildRevision21ScreenWordmark(horizontal, "Horizontal Wordmark",
                new Vector2(0f, 45f), 4.8f, font, white, violet, cyan, false);
            UiText("Horizontal Evidence Label", horizontal, "HORIZONTAL WORDMARK",
                new Vector2(0f, -250f), new Vector2(800f, 48f), new Vector2(0.5f, 0.5f),
                25f, Color.Lerp(white, cyan, 0.28f), font, TextAnchor.MiddleCenter);

            RectTransform variants = LogoEvidencePanel("Logo Icon Monochrome Evidence",
                canvas.transform, dark);
            RectTransform iconEvidence = BuildRevision21ScreenIcon(variants, "Square Icon",
                new Vector2(-445f, 40f), 5.4f, font, white, violet);
            RectTransform monochromeEvidence = BuildRevision21ScreenWordmark(variants,
                "Monochrome Wordmark",
                new Vector2(330f, 40f), 3.65f, font, white, white, white, true);
            UiText("Icon Evidence Label", iconEvidence, "ICON", new Vector2(0f, -198f),
                new Vector2(300f, 42f), new Vector2(0.5f, 0.5f), 24f, violet, font,
                TextAnchor.MiddleCenter);
            UiText("Monochrome Evidence Label", monochromeEvidence, "MONOCHROME",
                new Vector2(0f, -150f), new Vector2(500f, 42f), new Vector2(0.5f, 0.5f),
                24f, white, font, TextAnchor.MiddleCenter);

            RectTransform actual = LogoEvidencePanel("Logo Actual 64 Pixel Evidence",
                canvas.transform, dark);
            BuildRevision21ScreenIcon(actual, "Actual 64 Square Icon Sample",
                new Vector2(-300f, 302f), 1f, font, white, violet);
            RectTransform actualImage = BuildRevision21ScreenWordmark(actual,
                "Actual 64 Pixel Sample", new Vector2(120f, 302f), 1f, font, white,
                violet, cyan, false);
            RectTransform enlarged = BuildRevision21ScreenWordmark(actual,
                "Enlarged 64 Pixel Inspection", new Vector2(0f, -40f), 4f, font,
                white, violet, cyan, false);
            UiText("Actual 64 Square Icon Label", actual, "ACTUAL 64 X 64 ICON",
                new Vector2(-300f, 235f), new Vector2(360f, 42f), new Vector2(0.5f, 0.5f),
                22f, violet, font, TextAnchor.MiddleCenter);
            UiText("Actual 64 Pixel Label", actual, "ACTUAL 64 PX HEIGHT",
                new Vector2(120f, 235f), new Vector2(480f, 42f), new Vector2(0.5f, 0.5f),
                24f, cyan, font, TextAnchor.MiddleCenter);
            UiText("Enlarged Inspection Label", actual, "4X INSPECTION",
                new Vector2(0f, -220f), new Vector2(480f, 42f), new Vector2(0.5f, 0.5f),
                24f, violet, font, TextAnchor.MiddleCenter);

            return new Revision21LogoEvidence(canvas, horizontal.gameObject,
                variants.gameObject, actual.gameObject, actualImage, enlarged);
        }

        private static RectTransform LogoEvidencePanel(string name, Transform parent, Color dark)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(dark.r, dark.g, dark.b, 1f);
            return rect;
        }

        private static RectTransform BuildRevision21ScreenWordmark(Transform parent, string name,
            Vector2 position, float scale, Font font, Color white, Color violet, Color cyan,
            bool monochrome)
        {
            GameObject wordmark = new GameObject(name, typeof(RectTransform));
            RectTransform rect = wordmark.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(205f * scale, 64f * scale);
            Color sealColor = monochrome ? white : violet;
            Color dividerColor = monochrome ? white : cyan;
            UiText("Wordmark ECH", rect, "ECH", new Vector2(-75f * scale, 0f),
                new Vector2(65f * scale, 54f * scale), new Vector2(0.5f, 0.5f),
                27f * scale, white, font, TextAnchor.MiddleCenter);
            UiText("Readable Wordmark O", rect, "O", new Vector2(-18f * scale, 0f),
                new Vector2(29f * scale, 54f * scale), new Vector2(0.5f, 0.5f),
                28f * scale, white, font, TextAnchor.MiddleCenter);
            BuildRevision21UiSeal(rect, new Vector2(-18f * scale, 0f), 20f * scale,
                3f * scale, sealColor, 5);
            for (int i = 0; i < 3; i++)
                Revision21UiBar($"Wordmark Phase Tick {i + 1}", rect,
                    new Vector2(5f * scale, (-7f + i * 7f) * scale),
                    new Vector2(7f * scale, 1.7f * scale), sealColor, 0f);
            UiText("Canonical Divider", rect, "//", new Vector2(21f * scale, 0f),
                new Vector2(34f * scale, 54f * scale), new Vector2(0.5f, 0.5f),
                25f * scale, dividerColor, font, TextAnchor.MiddleCenter);
            UiText("Wordmark SHIFT", rect, "SHIFT", new Vector2(70f * scale, 0f),
                new Vector2(82f * scale, 54f * scale), new Vector2(0.5f, 0.5f),
                27f * scale, white, font, TextAnchor.MiddleCenter);
            return rect;
        }

        private static RectTransform BuildRevision21ScreenIcon(Transform parent, string name,
            Vector2 position, float scale, Font font, Color white, Color violet)
        {
            GameObject icon = new GameObject(name, typeof(RectTransform));
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(64f * scale, 64f * scale);
            UiText("Icon O", rect, "O", Vector2.zero,
                new Vector2(54f * scale, 54f * scale), new Vector2(0.5f, 0.5f),
                30f * scale, white, font, TextAnchor.MiddleCenter);
            BuildRevision21UiSeal(rect, Vector2.zero, 23f * scale, 4f * scale,
                violet, 5);
            for (int i = 0; i < 3; i++)
                Revision21UiBar($"Icon Phase Tick {i + 1}", rect,
                    new Vector2(27f * scale, (-8f + i * 8f) * scale),
                    new Vector2(6f * scale, 2f * scale), violet, 0f);
            return rect;
        }

        private static void BuildRevision21UiSeal(Transform parent, Vector2 center,
            float radius, float thickness, Color color, int missingSegment)
        {
            for (int i = 0; i < 6; i++)
            {
                if (i == missingSegment) continue;
                float a0 = (30f + i * 60f) * Mathf.Deg2Rad;
                float a1 = (30f + (i + 1) * 60f) * Mathf.Deg2Rad;
                Vector2 start = center + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * radius;
                Vector2 end = center + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * radius;
                Vector2 edge = end - start;
                Revision21UiBar($"Screen Seal Segment {i + 1}", parent,
                    (start + end) * 0.5f, new Vector2(edge.magnitude * 0.86f, thickness),
                    color, Mathf.Atan2(edge.y, edge.x) * Mathf.Rad2Deg);
            }
        }

        private static RectTransform Revision21UiBar(string name, Transform parent,
            Vector2 position, Vector2 size, Color color, float rotation)
        {
            GameObject bar = new GameObject(name, typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = bar.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            Image image = bar.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        private static void RenderRevision21(Revision21PreviewReferences preview,
            string outputDirectory)
        {
            Vector3[] positions =
            {
                new Vector3(0f, 3f, -13.8f),
                new Vector3(0f, 3f, -13.8f),
                new Vector3(0f, 3f, -13.8f),
                new Vector3(4.1f, 3.4f, -12f),
                new Vector3(8.4f, 5.1f, -9.4f),
                new Vector3(-4.5f, 2.4f, -5.0f),
                new Vector3(-2.15f, 5.1f, -5.8f),
                new Vector3(2.15f, 5.1f, -5.8f),
                new Vector3(-1.25f, 4f, -0.7f),
                new Vector3(1.4f, 4f, -2.1f),
                new Vector3(-2.45f, 2.75f, -0.2f),
                new Vector3(14.2f, 22.5f, -13.4f),
                new Vector3(7.2f, 6.4f, 3.7f),
                new Vector3(-6.5f, 5.4f, -5.8f)
            };
            Vector3[] targets =
            {
                new Vector3(0f, 2.1f, 0f),
                new Vector3(0f, 2.1f, 0f),
                new Vector3(0f, 2.1f, 0f),
                new Vector3(4.1f, 2f, -5.35f),
                new Vector3(4.1f, 1.85f, -5.35f),
                new Vector3(0.3f, 0.72f, -1.45f),
                new Vector3(-2.15f, 0.1f, 0f),
                new Vector3(2.15f, 0.1f, 0f),
                new Vector3(-4.65f, 1.25f, 3.35f),
                new Vector3(4.75f, 1.45f, 0.75f),
                new Vector3(-4.75f, 1.45f, 4.35f),
                new Vector3(0.4f, 0.35f, 0.4f),
                new Vector3(0f, 0.25f, 8.8f),
                new Vector3(0f, 0.95f, -0.25f)
            };
            float[] fieldsOfView =
            {
                35f, 35f, 35f, 37f, 37f, 34f, 37f, 37f, 40f, 38f, 36f, 50f, 42f, 40f
            };
            for (int i = 0; i < Revision21CaptureNames.Length; i++)
            {
                preview.ConfigureForCapture(i);
                preview.Camera.transform.position = positions[i];
                preview.Camera.transform.rotation = Quaternion.LookRotation(
                    targets[i] - positions[i], Vector3.up);
                preview.Camera.fieldOfView = fieldsOfView[i];
                Canvas.ForceUpdateCanvases();
                Render(preview.Camera, Path.Combine(outputDirectory, Revision21CaptureNames[i]));
            }
            preview.DisableAll();
        }

        private static void WriteRevision21Audio(string audioDirectory)
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
            float[] spawnThree = CreateRevision2Cue(2, sampleRate, tickData, tick.frequency,
                fieldData, field.frequency, impactData, impact.frequency);
            float[] sectionComplete = CreateRevision21SectionComplete(sampleRate, tickData,
                tick.frequency, fieldData, field.frequency, impactData, impact.frequency);

            string spawnPath = Path.Combine(audioDirectory, Revision21AudioNames[0]);
            string sectionPath = Path.Combine(audioDirectory, Revision21AudioNames[1]);
            WritePcm16Wave(spawnPath, spawnThree, sampleRate);
            WritePcm16Wave(sectionPath, sectionComplete, sampleRate);

            float[] comparison = new float[sampleRate * 5];
            MixRevision2Cue(spawnThree, comparison, Mathf.RoundToInt(0.35f * sampleRate));
            MixRevision2Cue(sectionComplete, comparison, Mathf.RoundToInt(2.25f * sampleRate));
            string comparisonPath = Path.Combine(audioDirectory, Revision21AudioNames[2]);
            WritePcm16Wave(comparisonPath, comparison, sampleRate);

            AnalyzeRevision21Wave(spawnPath, out double spawnSeconds, out float spawnPeak,
                out int spawnClipping);
            AnalyzeRevision21Wave(sectionPath, out double sectionSeconds, out float sectionPeak,
                out int sectionClipping);
            AnalyzeRevision21Wave(comparisonPath, out double comparisonSeconds,
                out float comparisonPeak, out int comparisonClipping);
            StringBuilder evidence = new StringBuilder(1024);
            evidence.AppendLine("# Revision 2.1 Audio Comparison");
            evidence.AppendLine();
            evidence.AppendLine("Only `section_complete` was revised. `echo_spawn_03` uses the unchanged Revision 2 synthesis path.");
            evidence.AppendLine();
            evidence.AppendLine("| Cue | Start in comparison | Duration | Peak | Clipping samples | Change |");
            evidence.AppendLine("|---|---:|---:|---:|---:|---|");
            evidence.AppendFormat(CultureInfo.InvariantCulture,
                "| `echo_spawn_03` | 0.35s | {0:0.000}s | {1:0.0000} | {2} | unchanged reference |\n",
                spawnSeconds, spawnPeak, spawnClipping);
            evidence.AppendFormat(CultureInfo.InvariantCulture,
                "| `section_complete` | 2.25s | {0:0.000}s | {1:0.0000} | {2} | lower mechanical terminal at 1.18s and final confirmation at 1.38s |\n",
                sectionSeconds, sectionPeak, sectionClipping);
            evidence.AppendFormat(CultureInfo.InvariantCulture,
                "| comparison | 0.00s | {0:0.000}s | {1:0.0000} | {2} | direct near-field review |\n",
                comparisonSeconds, comparisonPeak, comparisonClipping);
            File.WriteAllText(Path.Combine(audioDirectory, "revision21_audio_evidence.md"),
                evidence.ToString(), new UTF8Encoding(false));
        }

        private static float[] CreateRevision21SectionComplete(int sampleRate, float[] tick,
            int tickRate, float[] field, int fieldRate, float[] impact, int impactRate)
        {
            float[] cue = new float[Mathf.RoundToInt(sampleRate * 1.95f)];
            MixResampled(impact, impactRate, cue, sampleRate, 0.03f, 0.72f, 0.12f);
            MixResampled(tick, tickRate, cue, sampleRate, 0.06f, 1f, 0.24f);
            MixResampled(tick, tickRate, cue, sampleRate, 0.37f, 1.27f, 0.22f);
            MixResampled(tick, tickRate, cue, sampleRate, 0.70f, 1.52f, 0.20f);
            MixResampled(field, fieldRate, cue, sampleRate, 0.77f, 0.68f, 0.09f);
            MixResampled(impact, impactRate, cue, sampleRate, 1.18f, 0.48f, 0.18f);
            MixResampled(tick, tickRate, cue, sampleRate, 1.38f, 0.80f, 0.15f);
            ApplyOnePoleLowPass(cue, 0.42f);
            AddDelay(cue, sampleRate, 0.13f, 0.1f);
            Normalize(cue, 0.84f);
            return cue;
        }

        private static void ValidateRevision21(string captureDirectory, string audioDirectory)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Revision21ScenePath) == null)
                throw new InvalidOperationException("Revision 2.1 preview Scene was not saved.");
            string manifest = AbsoluteAssetPath(
                $"{Revision21ArtRoot}/P5A_R21_ProductionHashes.txt");
            if (!File.Exists(manifest))
                throw new InvalidOperationException("Revision 2.1 production hash manifest is missing.");
            for (int i = 0; i < Revision21CaptureNames.Length; i++)
            {
                string path = Path.Combine(captureDirectory, Revision21CaptureNames[i]);
                if (!File.Exists(path) || new FileInfo(path).Length < 30000)
                    throw new InvalidOperationException($"Revision 2.1 capture is missing: {path}");
                byte[] png = File.ReadAllBytes(path);
                if (ReadBigEndianInt32(png, 16) != CaptureWidth ||
                    ReadBigEndianInt32(png, 20) != CaptureHeight)
                    throw new InvalidOperationException($"Revision 2.1 capture size is invalid: {path}");
            }
            ValidateRevision21Wave(Path.Combine(audioDirectory, Revision21AudioNames[0]), 1.2d);
            ValidateRevision21Wave(Path.Combine(audioDirectory, Revision21AudioNames[1]), 2.1d);
            ValidateRevision21Wave(Path.Combine(audioDirectory, Revision21AudioNames[2]), 5.1d);
            if (!File.Exists(Path.Combine(audioDirectory, "revision21_audio_evidence.md")))
                throw new InvalidOperationException("Revision 2.1 audio evidence is missing.");
        }

        private static void ValidateRevision21Wave(string path, double maximumSeconds)
        {
            AnalyzeRevision21Wave(path, out double seconds, out float peak, out int clipping);
            if (seconds <= 0.1d || seconds > maximumSeconds)
                throw new InvalidOperationException($"Revision 2.1 audio duration is invalid: {path}");
            if (peak < 0.1f || peak > 0.9f || clipping != 0)
                throw new InvalidOperationException(
                    $"Revision 2.1 audio metric is invalid: {path} ({peak:0.000}, clips={clipping}).");
        }

        private static void AnalyzeRevision21Wave(string path, out double seconds,
            out float peak, out int clipping)
        {
            if (!File.Exists(path) || new FileInfo(path).Length <= 44)
                throw new InvalidOperationException($"Revision 2.1 audio is missing: {path}");
            byte[] data = File.ReadAllBytes(path);
            seconds = (data.Length - 44d) / (44100d * 2d);
            peak = 0f;
            clipping = 0;
            for (int offset = 44; offset + 1 < data.Length; offset += 2)
            {
                short value = (short)(data[offset] | data[offset + 1] << 8);
                float amplitude = Mathf.Abs(value / 32768f);
                peak = Mathf.Max(peak, amplitude);
                if (amplitude >= 0.999f) clipping++;
            }
        }

        private static void WriteRevision21ProductionHashManifest()
        {
            string[] paths =
            {
                "Assets/_Project/Scenes/P0_ReplayLab.unity",
                "Assets/_Project/Scenes/P1_InteractionLab.unity",
                "Assets/_Project/Scenes/P2_CoordinationLab.unity",
                "Assets/_Project/Scenes/P3_PlayableGreybox.unity"
            };
            StringBuilder manifest = new StringBuilder(512);
            for (int i = 0; i < paths.Length; i++)
            {
                manifest.Append(paths[i]);
                manifest.Append('|');
                manifest.Append(Sha256Revision2Asset(paths[i]));
                manifest.Append('\n');
            }
            string assetPath = $"{Revision21ArtRoot}/P5A_R21_ProductionHashes.txt";
            File.WriteAllText(AbsoluteAssetPath(assetPath), manifest.ToString(),
                new UTF8Encoding(false));
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        private sealed class Revision21LogoEvidence
        {
            public Revision21LogoEvidence(Canvas canvas, GameObject horizontal,
                GameObject variants, GameObject actual, RectTransform actualImage,
                RectTransform enlargedImage)
            {
                Canvas = canvas;
                Horizontal = horizontal;
                Variants = variants;
                Actual = actual;
                ActualImage = actualImage;
                EnlargedImage = enlargedImage;
            }

            public Canvas Canvas { get; }
            public GameObject Horizontal { get; }
            public GameObject Variants { get; }
            public GameObject Actual { get; }
            public RectTransform ActualImage { get; }
            public RectTransform EnlargedImage { get; }
        }

        private sealed class Revision21PreviewReferences
        {
            private readonly Revision2PreviewReferences revision2;
            private readonly Revision21LogoEvidence logo;

            public Revision21PreviewReferences(Revision2PreviewReferences revision2,
                Revision21LogoEvidence logo)
            {
                this.revision2 = revision2;
                this.logo = logo;
            }

            public Camera Camera => revision2.Camera;

            public void DisableAll()
            {
                revision2.DisableAll();
                logo.Canvas.gameObject.SetActive(false);
                logo.Horizontal.SetActive(false);
                logo.Variants.SetActive(false);
                logo.Actual.SetActive(false);
            }

            public void ConfigureForCapture(int index)
            {
                DisableAll();
                if (index <= 2)
                {
                    logo.Canvas.gameObject.SetActive(true);
                    logo.Horizontal.SetActive(index == 0);
                    logo.Variants.SetActive(index == 1);
                    logo.Actual.SetActive(index == 2);
                    return;
                }

                if (index == 6 || index == 7)
                {
                    revision2.GoalGallery.SetActive(true);
                    Transform locked = FindNamed(revision2.GoalGallery.transform,
                        "Locked Goal State");
                    Transform unlocked = FindNamed(revision2.GoalGallery.transform,
                        "Unlocked Goal State");
                    Transform lockedLabel = FindNamed(revision2.GoalGallery.transform,
                        "Locked State Label");
                    Transform unlockedLabel = FindNamed(revision2.GoalGallery.transform,
                        "Unlocked State Label");
                    locked.gameObject.SetActive(index == 6);
                    unlocked.gameObject.SetActive(index == 7);
                    lockedLabel.gameObject.SetActive(index == 6);
                    unlockedLabel.gameObject.SetActive(index == 7);
                    return;
                }

                revision2.Environment.SetActive(true);
                revision2.Shell.SetActive(index >= 8);
                revision2.Maintenance.SetActive(index == 8 || index == 10 || index == 11);
                revision2.Observation.SetActive(index == 9 || index == 11);
                revision2.Chamber.SetActive(index == 3 || index == 4 || index == 9 || index == 11);
                revision2.Gameplay.SetActive(index == 5 || index >= 11);
                revision2.Actors.SetActive(index == 5 || index >= 11);
                revision2.Devices.SetActive(index == 11 || index == 12);
                revision2.Hud.gameObject.SetActive(false);
                if (index == 5)
                {
                    FindNamed(revision2.Actors.transform, "Echo 2 Surface Motif")
                        .gameObject.SetActive(false);
                    FindNamed(revision2.Actors.transform, "Echo 3 Surface Motif")
                        .gameObject.SetActive(false);
                }
                else if (index >= 11)
                {
                    FindNamed(revision2.Actors.transform, "Echo 2 Surface Motif")
                        .gameObject.SetActive(true);
                    FindNamed(revision2.Actors.transform, "Echo 3 Surface Motif")
                        .gameObject.SetActive(true);
                }
            }
        }
    }
}
