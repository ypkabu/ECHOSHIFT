using System;
using System.IO;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using EchoShift.Presentation;
using EchoShift.Replay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class Phase4CapturePipeline
    {
        private static readonly string[] FileNames =
        {
            "01_section1_facility.png",
            "02_player_echo1.png",
            "03_battery_carry.png",
            "04_socket_door_open.png",
            "05_echo1_echo2_roles.png",
            "06_goal_arrival.png",
            "07_gameplay_hud.png",
            "08_pause_menu.png"
        };

        public static void CaptureAllFromCommandLine()
        {
            CaptureAll();
            Debug.Log("PHASE4_CAPTURES_OK count=8;width=1920;height=1080");
            EditorApplication.Exit(0);
        }

        [MenuItem("ECHO SHIFT/Capture Phase 4 Frames")]
        public static void CaptureAll()
        {
            P3SceneBuilder.BuildScene();
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath, OpenSceneMode.Single);
            PuzzleSectionController[] sections = Object.FindObjectsByType<PuzzleSectionController>(
                FindObjectsInactive.Include);
            Array.Sort(sections, (a, b) => a.SectionNumber.CompareTo(b.SectionNumber));
            if (sections.Length != 3) throw new InvalidOperationException("Capture scene needs three sections.");
            Camera camera = Object.FindAnyObjectByType<Camera>(FindObjectsInactive.Include);
            Phase3CameraSettings cameraSettings = AssetDatabase.LoadAssetAtPath<Phase3CameraSettings>(
                P3SceneBuilder.CameraSettingsPath);
            Phase4VisualSettings settings = AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(
                Phase4AssetBuilder.SettingsPath);
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string directory = Path.Combine(repository, settings.CaptureDirectory);
            Directory.CreateDirectory(directory);

            ConfigureCanvases(camera);
            Canvas gameplayCanvas = Object.FindAnyObjectByType<GameplayHud>(FindObjectsInactive.Include)
                .GetComponent<Canvas>();
            Canvas pauseCanvas = Object.FindAnyObjectByType<PauseMenuController>(FindObjectsInactive.Include)
                .GetComponent<Canvas>();
            gameplayCanvas.enabled = false;
            pauseCanvas.enabled = false;
            Activate(sections, 0); PositionCamera(camera, sections[0], cameraSettings, 0f);
            WarmCamera(camera, settings);
            Render(camera, Path.Combine(directory, FileNames[0]), settings);

            GameObject echo1 = StageEcho(sections[0], 1, new Vector3(-2f, 1f, -3f));
            sections[0].Player.transform.localPosition = new Vector3(1.7f, 1f, -1.4f);
            Render(camera, Path.Combine(directory, FileNames[1]), settings);
            Object.DestroyImmediate(echo1);

            Activate(sections, 1); PositionCamera(camera, sections[1], cameraSettings, 0f);
            CarryableBattery battery = sections[1].GetComponentInChildren<CarryableBattery>(true);
            Transform carry = sections[1].Player.transform.Find("Carry Socket");
            battery.transform.SetParent(carry, false); battery.transform.localPosition = Vector3.zero;
            sections[1].Player.transform.localPosition = new Vector3(0.8f, 1f, -2.2f);
            Render(camera, Path.Combine(directory, FileNames[2]), settings);

            PowerSocket socket = sections[1].GetComponentInChildren<PowerSocket>(true);
            Transform insertion = socket.transform.Find("Insertion Point");
            battery.transform.SetParent(insertion, false); battery.transform.localPosition = Vector3.zero;
            DoorController door = sections[1].GetComponentInChildren<DoorController>(true);
            door.transform.localPosition += new Vector3(4.5f, 0f, 0f);
            Render(camera, Path.Combine(directory, FileNames[3]), settings);

            Activate(sections, 2); PositionCamera(camera, sections[2], cameraSettings, 0f);
            GameObject role1 = StageEcho(sections[2], 1, new Vector3(-3f, 1f, -7f));
            GameObject role2 = StageEcho(sections[2], 2, new Vector3(1.5f, 1f, 1.1f));
            sections[2].Player.transform.localPosition = new Vector3(-1f, 1f, 3.6f);
            Render(camera, Path.Combine(directory, FileNames[4]), settings);
            Object.DestroyImmediate(role1); Object.DestroyImmediate(role2);

            sections[2].Player.transform.localPosition = new Vector3(0f, 1f, 9f);
            PositionCamera(camera, sections[2], cameraSettings, 6f);
            Render(camera, Path.Combine(directory, FileNames[5]), settings);

            PopulateHudText();
            gameplayCanvas.enabled = true;
            GameObject pausePanel = FindByName(scene, "Pause Panel");
            pauseCanvas.enabled = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            Render(camera, Path.Combine(directory, FileNames[6]), settings);

            gameplayCanvas.enabled = false;
            pauseCanvas.enabled = true;
            if (pausePanel != null) pausePanel.SetActive(true);
            Phase4PauseButtonVisual selectedButton = pauseCanvas.GetComponentInChildren<Phase4PauseButtonVisual>(true);
            if (selectedButton != null) selectedButton.OnSelect(new BaseEventData(EventSystem.current));
            Render(camera, Path.Combine(directory, FileNames[7]), settings);

            for (int i = 0; i < FileNames.Length; i++)
            {
                string path = Path.Combine(directory, FileNames[i]);
                if (!File.Exists(path) || new FileInfo(path).Length < 20000)
                    throw new InvalidOperationException($"Capture is missing or too small: {path}");
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        private static void Activate(PuzzleSectionController[] sections, int index)
        {
            for (int i = 0; i < sections.Length; i++) sections[i].gameObject.SetActive(i == index);
        }

        private static void PositionCamera(Camera camera, PuzzleSectionController section,
            Phase3CameraSettings settings, float forward)
        {
            Vector3 target = section.transform.position + settings.LookOffset + Vector3.forward * forward;
            camera.transform.position = section.transform.position + settings.Offset + Vector3.forward * forward;
            camera.transform.rotation = Quaternion.LookRotation(target - camera.transform.position, Vector3.up);
        }

        private static GameObject StageEcho(PuzzleSectionController section, int generation, Vector3 local)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Project/Prefabs/Actors/P3_Echo.prefab");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, section.transform);
            instance.name = $"Capture Echo {generation}";
            instance.transform.localPosition = local;
            LoopActor actor = instance.GetComponent<LoopActor>(); actor.Configure(LoopActorKind.Echo, generation);
            Phase4ActorVisual visual = instance.GetComponent<Phase4ActorVisual>();
            visual.RefreshNowForTests();
            return instance;
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

        private static void PopulateHudText()
        {
            Text[] texts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == "Section") texts[i].text = "セクション3：過去との協力";
                else if (texts[i].name == "Objective") texts[i].text = "2体のエコーと協力して出口へ進む";
                else if (texts[i].name == "Loop") texts[i].text = "ループ 3";
                else if (texts[i].name == "Timer") texts[i].text = "残り時間 31.4";
                else if (texts[i].name == "Echoes") texts[i].text = "エコー 2 / 3";
                else if (texts[i].name == "Prompt") texts[i].text = "E：装置を調べる";
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

        private static void Render(Camera camera, string path, Phase4VisualSettings settings)
        {
            RenderTexture target = new RenderTexture(settings.CaptureWidth, settings.CaptureHeight, 24,
                RenderTextureFormat.ARGB32) { antiAliasing = 1 };
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
            RenderTexture target = new RenderTexture(settings.CaptureWidth, settings.CaptureHeight, 24);
            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = null;
            target.Release();
            Object.DestroyImmediate(target);
        }
    }
}
