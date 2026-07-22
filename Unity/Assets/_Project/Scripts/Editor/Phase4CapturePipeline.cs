using System;
using System.IO;
using EchoShift.Debugging;
using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Presentation;
using EchoShift.Replay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class Phase4CapturePipeline
    {
        public static void CaptureAllFromCommandLine()
        {
            CaptureAll();
            Debug.Log("PHASE4_2_CAPTURES_OK count=8;width=1920;height=1080");
            EditorApplication.Exit(0);
        }

        public static void CaptureExistingFromCommandLine()
        {
            CaptureInternal(false);
            Debug.Log("PHASE4_2_CAPTURES_OK count=8;width=1920;height=1080;scene=existing");
            EditorApplication.Exit(0);
        }

        [MenuItem("ECHO SHIFT/Capture Phase 4.2 Presentation Frames")]
        public static void CaptureAll()
        {
            CaptureInternal(true);
        }

        private static void CaptureInternal(bool rebuildScene)
        {
            if (rebuildScene) P3SceneBuilder.BuildScene();
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath, OpenSceneMode.Single);
            PuzzleSectionController[] sections = Object.FindObjectsByType<PuzzleSectionController>(
                FindObjectsInactive.Include);
            Array.Sort(sections, (left, right) => left.SectionNumber.CompareTo(right.SectionNumber));
            if (sections.Length != 3)
                throw new InvalidOperationException("Capture scene needs three sections.");

            Phase4VisualSettings settings = AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(
                Phase4AssetBuilder.SettingsPath);
            Phase4CapturePreset preset = AssetDatabase.LoadAssetAtPath<Phase4CapturePreset>(
                Phase4AssetBuilder.CapturePresetPath);
            if (settings == null || preset == null || !preset.IsValid())
                throw new InvalidOperationException("Phase 4.2 capture settings are incomplete.");

            Camera camera = Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
            GameplayHud hud = Object.FindAnyObjectByType<GameplayHud>(FindObjectsInactive.Include);
            PauseMenuController pause = Object.FindAnyObjectByType<PauseMenuController>(
                FindObjectsInactive.Include);
            if (camera == null || hud == null || pause == null)
                throw new InvalidOperationException("Capture camera or UI is missing.");

            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string directory = Path.Combine(repository, settings.CaptureDirectory);
            Directory.CreateDirectory(directory);
            bool cursorVisible = Cursor.visible;
            try
            {
                Cursor.visible = false;
                DisableDebugOverlays();
                ConfigureCanvases(camera);
                Canvas gameplayCanvas = hud.GetComponent<Canvas>();
                Canvas pauseCanvas = pause.GetComponent<Canvas>();
                gameplayCanvas.enabled = false;
                pauseCanvas.enabled = false;
                GameObject pausePanel = FindByName(scene, "Pause Panel");
                if (pausePanel != null) pausePanel.SetActive(false);

                ConfigureRoutes(sections);
                CaptureSectionOne(sections, camera, gameplayCanvas, pauseCanvas,
                    preset, settings, directory);
                CaptureSectionTwo(sections, camera, gameplayCanvas, pauseCanvas,
                    preset, settings, directory);
                CaptureSectionThree(sections, camera, gameplayCanvas, pauseCanvas,
                    hud, preset, settings, directory);
                ValidateOutputs(preset, settings, directory);
            }
            finally
            {
                Cursor.visible = cursorVisible;
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static void CaptureSectionOne(
            PuzzleSectionController[] sections, Camera camera,
            Canvas gameplayCanvas, Canvas pauseCanvas,
            Phase4CapturePreset preset, Phase4VisualSettings settings, string directory)
        {
            PuzzleSectionController section = Activate(sections, 1);
            Capture(preset.GetShot(0), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);
            // Give the current Player and Echo enough time to diverge along their
            // legitimate routes so the identity shot does not read as one actor.
            AdvanceUntil(section, 2, 40, 500);
            Capture(preset.GetShot(1), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);
            AdvanceUntil(section, 2, 95, 300);
            PressurePlate plate = section.GetComponentInChildren<PressurePlate>();
            if (plate == null || !plate.IsPressed)
                throw new InvalidOperationException("Section 1 capture route did not press the Plate.");
            Capture(preset.GetShot(2), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);
        }

        private static void CaptureSectionTwo(
            PuzzleSectionController[] sections, Camera camera,
            Canvas gameplayCanvas, Canvas pauseCanvas,
            Phase4CapturePreset preset, Phase4VisualSettings settings, string directory)
        {
            PuzzleSectionController section = Activate(sections, 2);
            AdvanceUntil(section, 1, 80, 300);
            CarryableBattery battery = section.GetComponentInChildren<CarryableBattery>();
            if (battery == null || !battery.IsHeld)
                throw new InvalidOperationException("Section 2 capture route did not hold the Battery.");
            Capture(preset.GetShot(3), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);

            AdvanceUntil(section, 2, 112, 500);
            PowerSocket socket = section.GetComponentInChildren<PowerSocket>();
            DoorController door = section.GetComponentInChildren<DoorController>();
            if (socket == null || !socket.InsertedByReplay || !socket.RequestsDoorOpen ||
                door == null || !door.IsOpen)
                throw new InvalidOperationException(
                    "Section 2 capture route did not replay insertion and open the Door.");
            Capture(preset.GetShot(4), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);
        }

        private static void CaptureSectionThree(
            PuzzleSectionController[] sections, Camera camera,
            Canvas gameplayCanvas, Canvas pauseCanvas, GameplayHud hud,
            Phase4CapturePreset preset, Phase4VisualSettings settings, string directory)
        {
            PuzzleSectionController section = Activate(sections, 3);
            AdvanceUntil(section, 3, 185, 1200);
            CarryableBattery battery = section.GetComponentInChildren<CarryableBattery>();
            PressurePlate plate = section.GetComponentInChildren<PressurePlate>();
            if (section.Director.EchoCount < 2 || plate == null || !plate.IsPressed ||
                battery == null || !battery.IsHeld)
                throw new InvalidOperationException(
                    "Section 3 capture route did not establish two distinct Echo roles.");
            Capture(preset.GetShot(5), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);

            PopulateHudText(hud);
            Capture(preset.GetShot(7), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);

            int guard = 0;
            while (!section.Goal.IsReached && guard++ < 600)
            {
                section.Director.AdvanceOneTickForTests();
                section.RefreshCompletionSensor();
            }
            if (!section.Goal.IsReached)
            {
                PowerSocket socket = section.GetComponentInChildren<PowerSocket>();
                DoorController[] doors = section.GetComponentsInChildren<DoorController>();
                throw new InvalidOperationException(
                    $"Section 3 capture route did not reach Goal;" +
                    $"tick={section.Director.CurrentTick};player={section.Player.transform.position};" +
                    $"socketPowered={socket != null && socket.RequestsDoorOpen};" +
                    $"doors={doors.Length};doorA={(doors.Length > 0 && doors[0].IsOpen)};" +
                    $"doorB={(doors.Length > 1 && doors[1].IsOpen)}");
            }
            Capture(preset.GetShot(6), section, camera, gameplayCanvas, pauseCanvas,
                settings, directory);
        }

        private static PuzzleSectionController Activate(
            PuzzleSectionController[] sections, int sectionNumber)
        {
            PuzzleSectionController selected = sections[sectionNumber - 1];
            for (int i = 0; i < sections.Length; i++)
                sections[i].gameObject.SetActive(i == sectionNumber - 1);
            InitializeSerializedRuntimeCaches(selected);
            selected.ActivateSection(false);
            Physics.SyncTransforms();
            return selected;
        }

        private static void InitializeSerializedRuntimeCaches(PuzzleSectionController section)
        {
            StableId[] identities = section.GetComponentsInChildren<StableId>(true);
            for (int i = 0; i < identities.Length; i++)
            {
                if (!identities[i].EnsureResolved())
                    throw new InvalidOperationException(
                        $"Capture Stable ID is unresolved: {identities[i].name}");
            }
            DoorController[] doors = section.GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < doors.Length; i++)
            {
                if (!doors[i].EnsureSourceCache())
                    throw new InvalidOperationException(
                        $"Capture Door source is unresolved: {doors[i].name}");
            }
        }

        private static void ConfigureRoutes(PuzzleSectionController[] sections)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                Phase4CaptureRouteInputSource route =
                    sections[i].Player.gameObject.AddComponent<Phase4CaptureRouteInputSource>();
                route.Configure(i + 1, sections[i].Director);
                sections[i].Player.Configure(
                    route, sections[i].Player.Motor, sections[i].Player.Interactor);
            }
        }

        private static void AdvanceUntil(
            PuzzleSectionController section, int loop, int tick, int maximumAdvances)
        {
            int advances = 0;
            while ((section.Director.LoopNumber < loop ||
                    (section.Director.LoopNumber == loop && section.Director.CurrentTick < tick)) &&
                   advances++ < maximumAdvances)
            {
                section.Director.AdvanceOneTickForTests();
            }
            if (section.Director.LoopNumber != loop || section.Director.CurrentTick < tick)
                throw new InvalidOperationException(
                    $"Capture route timed out: section={section.SectionNumber};" +
                    $"loop={section.Director.LoopNumber};tick={section.Director.CurrentTick}");
        }

        private static void Capture(
            Phase4CaptureShotPreset shot, PuzzleSectionController section,
            Camera camera, Canvas gameplayCanvas, Canvas pauseCanvas,
            Phase4VisualSettings settings, string directory)
        {
            if (shot.SectionNumber != section.SectionNumber)
                throw new InvalidOperationException($"Capture preset section mismatch: {shot.FileName}");
            RefreshPresentation(section);
            PositionCamera(camera, section, shot, settings);
            gameplayCanvas.enabled = shot.ShowGameplayHud;
            pauseCanvas.enabled = false;
            ValidateActorFraming(camera, section, shot.Moment, 0.015f);
            string path = Path.Combine(directory, shot.FileName);
            WarmCamera(camera, settings);
            Render(camera, path, settings);
        }

        private static void PositionCamera(
            Camera camera, PuzzleSectionController section,
            Phase4CaptureShotPreset shot, Phase4VisualSettings settings)
        {
            Vector3 focus = section.transform.position + shot.LocalFocus;
            camera.transform.position = focus + shot.CameraOffset;
            camera.transform.rotation = Quaternion.LookRotation(
                focus - camera.transform.position, Vector3.up);
            camera.fieldOfView = shot.FieldOfView;
            camera.aspect = settings.CaptureWidth / (float)settings.CaptureHeight;
        }

        private static void RefreshPresentation(PuzzleSectionController section)
        {
            Phase4ActorVisual[] actors = section.GetComponentsInChildren<Phase4ActorVisual>(true);
            for (int i = 0; i < actors.Length; i++) actors[i].RefreshNowForTests();
            Phase4PressurePlateVisual[] plates =
                section.GetComponentsInChildren<Phase4PressurePlateVisual>(true);
            for (int i = 0; i < plates.Length; i++) plates[i].RefreshNowForTests();
            Phase4BatteryVisual[] batteries =
                section.GetComponentsInChildren<Phase4BatteryVisual>(true);
            for (int i = 0; i < batteries.Length; i++) batteries[i].RefreshNowForTests();
            Phase4SocketVisual[] sockets =
                section.GetComponentsInChildren<Phase4SocketVisual>(true);
            for (int i = 0; i < sockets.Length; i++) sockets[i].RefreshNowForTests();
            DoorVisualFeedback[] doors =
                section.GetComponentsInChildren<DoorVisualFeedback>(true);
            for (int i = 0; i < doors.Length; i++) doors[i].RefreshNowForTests();
        }

        private static void ValidateActorFraming(
            Camera camera, PuzzleSectionController section,
            Phase4CaptureMoment moment, float margin)
        {
            ValidateActor(camera, section.Player.GetComponent<LoopActor>(), margin);
            int requiredEchoes = moment == Phase4CaptureMoment.PlayerAndEchoOne ||
                                 moment == Phase4CaptureMoment.EchoOnPlatePlayerAtDoor ||
                                 moment == Phase4CaptureMoment.RecordedInsertionDoorOpen
                ? 1
                : moment == Phase4CaptureMoment.TwoEchoRoles ||
                  moment == Phase4CaptureMoment.GameplayHud
                    ? 2
                    : 0;
            if (section.Director.EchoCount < requiredEchoes)
                throw new InvalidOperationException(
                    $"Capture moment {moment} needs {requiredEchoes} Echoes.");
            for (int i = 0; i < requiredEchoes; i++)
                ValidateActor(camera, section.Director.GetEchoPlayback(i).Actor, margin);
        }

        private static void ValidateActor(Camera camera, LoopActor actor, float margin)
        {
            if (actor == null || !actor.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Capture requires an active Actor.");
            Transform visual = actor.transform.Find("P4 Robot Visual");
            if (visual == null)
                throw new InvalidOperationException($"Capture Actor has no visual: {actor.name}");
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                if (!renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
                Bounds bounds = renderer.bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = new Vector3(
                        (corner & 1) == 0 ? min.x : max.x,
                        (corner & 2) == 0 ? min.y : max.y,
                        (corner & 4) == 0 ? min.z : max.z);
                    Vector3 viewport = camera.WorldToViewportPoint(point);
                    if (viewport.z <= 0f || viewport.x < margin || viewport.x > 1f - margin ||
                        viewport.y < margin || viewport.y > 1f - margin)
                        throw new InvalidOperationException(
                            $"Capture clips {actor.name}/{renderer.name}: {viewport}");
                }
            }
        }

        private static void PopulateHudText(GameplayHud hud)
        {
            hud.ExpireTransientPresentationForTests();
            Text[] texts = hud.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == "Loop") texts[i].text = "ループ  3";
                else if (texts[i].name == "Timer") texts[i].text = "残り時間  31.4";
                else if (texts[i].name == "Echoes") texts[i].text = "エコー  2/3";
            }
            Transform intro = hud.transform.Find("P4 Section Intro Panel");
            Transform prompt = hud.transform.Find("P4 Context Prompt Panel");
            Transform carry = hud.transform.Find("P4 Battery Carry Chip");
            if (intro != null) intro.gameObject.SetActive(false);
            if (prompt != null) prompt.gameObject.SetActive(false);
            if (carry != null) carry.gameObject.SetActive(false);
            CanvasGroup group = hud.GetComponent<CanvasGroup>();
            group.alpha = 1f; group.interactable = true; group.blocksRaycasts = true;
        }

        private static void DisableDebugOverlays()
        {
            Phase0DebugOverlay[] overlays = Object.FindObjectsByType<Phase0DebugOverlay>(
                FindObjectsInactive.Include);
            for (int i = 0; i < overlays.Length; i++) overlays[i].SetVisible(false);
        }

        private static void ConfigureCanvases(Camera camera)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].renderMode = RenderMode.ScreenSpaceCamera;
                canvases[i].worldCamera = camera;
                canvases[i].planeDistance = 1f + i * 0.1f;
            }
        }

        private static GameObject FindByName(Scene scene, string name)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (int j = 0; j < transforms.Length; j++)
                    if (transforms[j].name == name) return transforms[j].gameObject;
            }
            return null;
        }

        private static void ValidateOutputs(
            Phase4CapturePreset preset, Phase4VisualSettings settings, string directory)
        {
            string[] files = Directory.GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly);
            if (files.Length != Phase4CapturePreset.RequiredShotCount)
                throw new InvalidOperationException($"Capture count mismatch: {files.Length}");
            for (int i = 0; i < preset.Count; i++)
            {
                string path = Path.Combine(directory, preset.GetShot(i).FileName);
                if (!File.Exists(path) || new FileInfo(path).Length < 20000)
                    throw new InvalidOperationException($"Capture is missing or too small: {path}");
                byte[] png = File.ReadAllBytes(path);
                int width = ReadBigEndianInt32(png, 16);
                int height = ReadBigEndianInt32(png, 20);
                if (width != settings.CaptureWidth || height != settings.CaptureHeight)
                    throw new InvalidOperationException(
                        $"Capture dimensions are invalid: {path};{width}x{height}");
            }
        }

        private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) |
            (bytes[offset + 2] << 8) | bytes[offset + 3];

        private static void Render(Camera camera, string path, Phase4VisualSettings settings)
        {
            RenderTexture target = new RenderTexture(settings.CaptureWidth,
                settings.CaptureHeight, 24, RenderTextureFormat.ARGB32) { antiAliasing = 1 };
            Texture2D image = new Texture2D(settings.CaptureWidth, settings.CaptureHeight,
                TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = target; RenderTexture.active = target;
            camera.Render();
            image.ReadPixels(new Rect(0, 0, settings.CaptureWidth, settings.CaptureHeight), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = previous;
            Object.DestroyImmediate(image); target.Release(); Object.DestroyImmediate(target);
        }

        private static void WarmCamera(Camera camera, Phase4VisualSettings settings)
        {
            RenderTexture target = new RenderTexture(
                settings.CaptureWidth, settings.CaptureHeight, 24);
            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = null;
            target.Release();
            Object.DestroyImmediate(target);
        }
    }

    public sealed class Phase4CaptureRouteInputSource : MonoBehaviour, IInputSource
    {
        private int _section;
        private EchoShift.Core.LoopDirector _director;

        public void Configure(int section, EchoShift.Core.LoopDirector director)
        {
            _section = section;
            _director = director;
        }

        public InputCommand Sample(int tick)
        {
            int loop = _director.LoopNumber;
            Vector2 move = Vector2.zero;
            InputButtonFlags buttons = InputButtonFlags.None;
            if (_section == 1)
            {
                if (loop == 1)
                {
                    if (tick < 30) move = Vector2.left;
                    else if (tick < 90) move = Vector2.up;
                    if (tick == 100) buttons = InputButtonFlags.EndLoop;
                }
                else move = Vector2.up;
            }
            else if (_section == 2)
            {
                if (loop == 1)
                {
                    if (tick < 45) move = Vector2.up;
                    else if (tick < 60) move = Vector2.right;
                    else if (tick == 61) buttons = InputButtonFlags.Interact;
                    else if (tick < 107) move = Vector2.up;
                    else if (tick == 107) buttons = InputButtonFlags.Interact;
                    if (tick == 120) buttons |= InputButtonFlags.EndLoop;
                }
                else move = Vector2.up;
            }
            else
            {
                if (loop == 1)
                {
                    if (tick < 45) move = Vector2.left;
                    else if (tick < 98) move = Vector2.up;
                    if (tick == 110) buttons = InputButtonFlags.EndLoop;
                }
                else if (loop == 2)
                {
                    if (tick < 155) move = Vector2.up;
                    else if (tick < 175) move = Vector2.right;
                    else if (tick == 175) buttons = InputButtonFlags.Interact;
                    else if (tick < 225) move = Vector2.up;
                    else if (tick == 225) buttons = InputButtonFlags.Interact;
                    if (tick == 240) buttons |= InputButtonFlags.EndLoop;
                }
                else move = Vector2.up;
            }
            return new InputCommand(tick, move, buttons);
        }
    }
}
