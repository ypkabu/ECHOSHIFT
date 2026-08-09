using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static partial class Phase5AIdentityPreviewBuilder
    {
        public const string ProductionArtRoot = "Assets/_Project/Art/Phase5A/Production";
        public const string ProductionAudioRoot = "Assets/_Project/Audio/Phase5A";
        public const string ProductionAudioSetPath =
            ProductionAudioRoot + "/Phase5AAudioCueSet.asset";
        public const string ProductionIdentityRootName = "Phase 5A Approved Identity";

        private static readonly KeyValuePair<string, string>[]
            ApprovedProductionSerializationHashes =
            {
                new KeyValuePair<string, string>(
                    Phase4AssetBuilder.RobotAnimatorControllerPath,
                    "EE885A3CE1AA515DC6C1EE40AEE629DA4E51CA9822C982A2D5335639E30D8C92"),
                new KeyValuePair<string, string>(
                    "Assets/_Project/Art/Phase4/Phase4VolumeProfile.asset",
                    "D85DF4E9C7C20D6FADC2DB06899D4B6437E79EEF4BB9B65922257485B51C19C3"),
                new KeyValuePair<string, string>(
                    "Assets/_Project/Prefabs/Actors/P3_Echo.prefab",
                    "0FBB217DCA6A1D6132F17F1F4B512A83124A5C33D47D2F492A4BC08820903B52"),
                new KeyValuePair<string, string>(
                    P3SceneBuilder.ScenePath,
                    "A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854")
            };

        private static readonly string[] ApprovedCueFiles =
        {
            "echo_spawn_01.wav", "echo_spawn_02.wav", "echo_spawn_03.wav",
            "echo_remove.wav", "loop_end.wav", "interaction_success.wav",
            "battery_insert.wav", "section_complete.wav"
        };

        public static IReadOnlyList<string> ProductionApprovedCueFiles => ApprovedCueFiles;

        private static readonly string[] ProductionCaptureNames =
        {
            "01_production_gameplay_overview.png", "02_actor_player.png",
            "03_actor_echo.png", "04_goal_locked.png", "05_goal_unlocked.png",
            "06_echo_chamber.png", "07_maintenance.png", "08_observation.png",
            "09_real_decal.png"
        };

        public static IReadOnlyList<string> ProductionCaptures => ProductionCaptureNames;

        public static IReadOnlyList<KeyValuePair<string, string>>
            CanonicalProductionSerializationHashes => ApprovedProductionSerializationHashes;

        public static bool IsApprovedProductionSerializationCanonical(out string status)
        {
            for (int i = 0; i < ApprovedProductionSerializationHashes.Length; i++)
            {
                KeyValuePair<string, string> expected =
                    ApprovedProductionSerializationHashes[i];
                string absolute = Path.GetFullPath(Path.Combine(
                    Application.dataPath, "..", expected.Key));
                if (!File.Exists(absolute))
                {
                    status = $"missing={expected.Key}";
                    return false;
                }

                string actual;
                using (SHA256 sha = SHA256.Create())
                {
                    actual = BitConverter.ToString(
                            sha.ComputeHash(File.ReadAllBytes(absolute)))
                        .Replace("-", string.Empty);
                }
                if (!string.Equals(actual, expected.Value,
                        StringComparison.OrdinalIgnoreCase))
                {
                    status = $"mismatch={expected.Key};expected={expected.Value};actual={actual}";
                    return false;
                }
            }

            status = $"assets={ApprovedProductionSerializationHashes.Length}";
            return true;
        }

        public static void ApplyApprovedRevision21ToProduction(
            PuzzleSectionController[] sections, Transform systems,
            Phase4VisualSettings settings)
        {
            if (sections == null || sections.Length != 3 || systems == null || settings == null)
                throw new InvalidOperationException(
                    "Phase 5A Production Application requires P3 sections, systems, and settings.");

            Material[] revision2 = LoadRevision2MaterialsForRevision21();
            Material[] revision21 = LoadRevision21MaterialsForProduction();
            Phase4ExternalAssetCatalog catalog =
                AssetDatabase.LoadAssetAtPath<Phase4ExternalAssetCatalog>(CatalogPath);
            if (catalog == null || !catalog.IsComplete)
                throw new InvalidOperationException("Phase 4 external asset catalog is incomplete.");

            Phase4AudioCueSet audio = PrepareApprovedProductionAssets(settings);

            ApplyProductionEnvironment(sections[2], settings, catalog, revision2, revision21);
            for (int i = 0; i < sections.Length; i++)
                ApplyProductionGoal(sections[i], revision2, revision21);
            ApplyProductionLogo(systems);
            ApplyProductionAudio(systems, audio);

            Phase4SceneBuilder.PolishEchoPrefab(EchoPrefabPath, settings);
            ValidateProductionApplication(sections, systems, audio);
            AssetDatabase.SaveAssets();
        }

        public static Phase4AudioCueSet PrepareApprovedProductionAssets(
            Phase4VisualSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            Material[] revision21 = LoadRevision21MaterialsForProduction();
            EnsureAssetFolder(ProductionArtRoot);
            Material player = ProductionMaterial("P5A_Production_ActorCyan",
                revision21[8]);
            Material echo = ProductionMaterial("P5A_Production_ActorViolet",
                revision21[7]);
            Phase4AudioCueSet audio = BuildProductionAudioCueSet(settings.AudioCues);
            settings.ApplyPhase5AIdentity(player, echo, audio);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return audio;
        }

        private static Material ProductionMaterial(string name, Material source)
        {
            string path = $"{ProductionArtRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(source);
                AssetDatabase.CreateAsset(material, path);
            }
            else EditorUtility.CopySerialized(source, material);
            material.name = name;
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void GenerateProductionCapturesFromCommandLine()
        {
            Scene scene = EditorSceneManager.OpenScene(P3SceneBuilder.ScenePath,
                OpenSceneMode.Single);
            PuzzleSectionController[] sections = FindSceneComponents<PuzzleSectionController>(scene);
            Array.Sort(sections, (left, right) => left.SectionNumber.CompareTo(right.SectionNumber));
            if (sections.Length != 3)
                throw new InvalidOperationException("Production capture requires all three P3 sections.");
            for (int i = 0; i < sections.Length; i++) sections[i].gameObject.SetActive(i == 2);
            PuzzleSectionController section = sections[2];

            string repository = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            if (string.IsNullOrEmpty(repository))
                throw new InvalidOperationException("Repository root could not be resolved.");
            string output = Path.Combine(repository, "Captures", "Phase5A",
                "ProductionApplication");
            Directory.CreateDirectory(output);

            GameObject cameraObject = new GameObject("Temporary Production Capture Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Hex("07101A");
            camera.allowHDR = true;
            camera.fieldOfView = 43f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 180f;
            Cursor.visible = false;

            Transform identity = section.transform.Find(ProductionIdentityRootName);
            Transform chamber = FindNamed(identity, "Approved Echo Chamber");
            Transform maintenance = FindNamed(identity, "Approved Maintenance Identity");
            Transform observation = FindNamed(identity, "Approved Observation Identity");
            DoorController[] doors = section.GetComponentsInChildren<DoorController>(true);
            Phase5AGoalIdentityVisual goalVisual =
                section.Goal.GetComponent<Phase5AGoalIdentityVisual>();
            section.Player.GetComponent<Phase4ActorVisual>()?.RefreshNowForTests();

            GameObject echo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(EchoPrefabPath), scene) as GameObject;
            if (echo == null) throw new InvalidOperationException("Capture Echo could not be created.");
            echo.name = "Temporary Capture Echo";
            echo.transform.position = section.Player.transform.position + new Vector3(1.5f, 0f, 1.2f);
            EchoShift.Player.LoopActor echoActor = echo.GetComponent<EchoShift.Player.LoopActor>();
            echoActor.Configure(EchoShift.Player.LoopActorKind.Echo, 1);
            echo.GetComponent<Phase4ActorVisual>()?.RefreshNowForTests();

            Vector3 sectionCenter = section.transform.position + new Vector3(0f, 0.8f, 0f);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[0]),
                sectionCenter, new Vector3(0f, 15.5f, -12.5f), 48f);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[1]),
                section.Player.transform.position + Vector3.up * 0.7f,
                new Vector3(-3.3f, 4.0f, -5.2f), 35f);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[2]),
                echo.transform.position + Vector3.up * 0.7f,
                new Vector3(3.2f, 3.8f, -4.8f), 35f);

            goalVisual.SetEvidenceStateForEditor(false);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[3]),
                section.Goal.transform.position + Vector3.up * 0.15f,
                new Vector3(-3.8f, 6.3f, -5.4f), 38f);
            goalVisual.SetEvidenceStateForEditor(true);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[4]),
                section.Goal.transform.position + Vector3.up * 0.15f,
                new Vector3(3.8f, 6.3f, -5.4f), 38f);
            goalVisual.SetEvidenceStateForEditor(false);

            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[5]),
                chamber.position + Vector3.up * 1.8f,
                new Vector3(-4.8f, 4.0f, -6.2f), 36f);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[6]),
                maintenance.TransformPoint(new Vector3(-4.65f, 1.1f, 2.9f)),
                new Vector3(4.6f, 4.0f, -5.8f), 38f);
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[7]),
                observation.TransformPoint(new Vector3(4.7f, 1.4f, 0.8f)),
                new Vector3(-4.8f, 4.2f, -5.6f), 38f);
            Transform floorDecal = FindNamed(identity, "Approved Floor Surface Decal");
            Vector3 decalTarget = floorDecal != null ? floorDecal.position : sectionCenter;
            RenderProductionShot(camera, Path.Combine(output, ProductionCaptureNames[8]),
                decalTarget, new Vector3(-2.8f, 4.8f, -3.8f), 36f);

            Object.DestroyImmediate(echo);
            Object.DestroyImmediate(cameraObject);
            for (int i = 0; i < ProductionCaptureNames.Length; i++)
            {
                string path = Path.Combine(output, ProductionCaptureNames[i]);
                if (!File.Exists(path))
                    throw new InvalidOperationException($"Production capture is missing: {path}");
            }
            Debug.Log($"PHASE5A_PRODUCTION_CAPTURES_OK count={ProductionCaptureNames.Length};" +
                      $"graphics={SystemInfo.graphicsDeviceType};resolution=1920x1080");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        private static void RenderProductionShot(Camera camera, string path,
            Vector3 target, Vector3 offset, float fieldOfView)
        {
            camera.transform.position = target + offset;
            camera.transform.rotation = Quaternion.LookRotation(
                target - camera.transform.position, Vector3.up);
            camera.fieldOfView = fieldOfView;
            RenderTexture texture = new RenderTexture(1920, 1080, 24,
                RenderTextureFormat.ARGB32);
            RenderTexture previous = RenderTexture.active;
            camera.targetTexture = texture;
            RenderTexture.active = texture;
            camera.Render();
            Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            image.Apply(false, false);
            File.WriteAllBytes(path, image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(texture);
        }

        private static T[] FindSceneComponents<T>(Scene scene) where T : Component
        {
            List<T> found = new List<T>();
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
                found.AddRange(roots[i].GetComponentsInChildren<T>(true));
            return found.ToArray();
        }

        private static Material[] LoadRevision21MaterialsForProduction()
        {
            string[] names =
            {
                "P5A_R21_GoalBed", "P5A_R21_GoalViolet", "P5A_R21_GoalNeutral",
                "P5A_R21_ArcVioletDim", "P5A_R21_ArcCyanDim",
                "P5A_R21_MonitorFace", "P5A_R21_MonitorSignal",
                "P5A_R21_ActorVioletSurface", "P5A_R21_ActorCyanSurface"
            };
            Material[] result = new Material[names.Length];
            for (int i = 0; i < result.Length; i++)
            {
                string path = $"{Revision21ArtRoot}/{names[i]}.mat";
                result[i] = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (result[i] == null)
                    throw new InvalidOperationException($"Approved Revision 2.1 material is missing: {path}");
            }
            return result;
        }

        private static void ApplyProductionEnvironment(PuzzleSectionController section,
            Phase4VisualSettings settings, Phase4ExternalAssetCatalog catalog,
            Material[] revision2, Material[] revision21)
        {
            Transform existing = section.transform.Find(ProductionIdentityRootName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            Transform root = Child(section.transform, ProductionIdentityRootName);

            Transform maintenance = BuildRevision2MaintenanceBay(root, catalog,
                revision2[0], revision2[1], revision2[2], revision2[3], revision2[5],
                revision2[8], settings.PackagedJapaneseFont);
            maintenance.name = "Approved Maintenance Identity";

            Transform observation = BuildRevision2ObservationBay(root, catalog,
                revision2[0], revision2[1], revision2[2], revision2[3], revision2[4],
                revision2[8], settings.PackagedJapaneseFont);
            observation.name = "Approved Observation Identity";
            ApplyRevision21Observation(observation, revision21[5], revision21[6]);

            Transform chamber = Child(root, "Approved Echo Chamber");
            chamber.localPosition = new Vector3(4.1f, 0f, -5.35f);
            BuildRevision2Chamber(chamber, revision2[0], revision2[1], revision2[2],
                revision2[3], revision2[4], revision2[8], settings.PackagedJapaneseFont);
            ApplyRevision21Chamber(chamber, revision21[3], revision21[4]);

            BuildApprovedRealDecals(section, root, revision2[3], revision2[8],
                settings.PackagedJapaneseFont);
        }

        private static void ApplyProductionGoal(PuzzleSectionController section,
            Material[] revision2, Material[] revision21)
        {
            GoalVolume goal = section.Goal;
            Transform oldPortal = goal.transform.Find("P4 Goal Portal");
            if (oldPortal != null) oldPortal.gameObject.SetActive(false);
            Transform existing = goal.transform.Find("Phase 5A Approved Goal Identity");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            Transform root = CompensatedVisualRoot(goal.transform,
                "Phase 5A Approved Goal Identity");
            Transform locked = Child(root, "Locked Goal State");
            BuildRevision2Goal(locked, false, revision2[0], revision2[1], revision2[2],
                revision2[3], revision2[6], revision2[8]);
            ApplyProductionGoalCorrection(locked, false, revision2[6], revision21);

            Transform unlocked = Child(root, "Unlocked Goal State");
            BuildRevision2Goal(unlocked, true, revision2[0], revision2[1], revision2[2],
                revision2[3], revision2[6], revision2[8]);
            ApplyProductionGoalCorrection(unlocked, true, revision2[6], revision21);
            unlocked.gameObject.SetActive(false);

            DoorController[] doors = section.GetComponentsInChildren<DoorController>(true);
            Phase5AGoalIdentityVisual state = goal.GetComponent<Phase5AGoalIdentityVisual>();
            if (state == null) state = goal.gameObject.AddComponent<Phase5AGoalIdentityVisual>();
            state.Configure(doors, locked.gameObject, unlocked.gameObject);
        }

        private static void ApplyProductionGoalCorrection(Transform goal, bool unlocked,
            Material lime, Material[] revision21)
        {
            FindNamed(goal, "Floor Embedded Goal Bed").GetComponent<Renderer>().sharedMaterial =
                revision21[0];
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
            for (int i = 0; i < signalRenderers.Length; i++)
            {
                signalRenderers[i].sharedMaterial = revision21[1];
                Transform segment = signalRenderers[i].transform;
                segment.localPosition = new Vector3(segment.localPosition.x, 0.118f,
                    segment.localPosition.z);
                segment.localScale = new Vector3(segment.localScale.x * 1.03f, 0.038f, 0.10f);
            }

            Transform inner = FindNamed(goal, "Goal Inner Three Segments");
            Renderer[] innerRenderers = inner.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < innerRenderers.Length; i++)
            {
                bool center = innerRenderers[i].name == "Goal Inner Segment 2";
                innerRenderers[i].sharedMaterial = center && unlocked ? lime :
                    center ? revision21[2] : revision21[1];
                innerRenderers[i].transform.localScale = new Vector3(0.56f, 0.035f, 0.17f);
                innerRenderers[i].transform.localPosition = new Vector3(
                    innerRenderers[i].transform.localPosition.x, 0.125f, 0.2f);
            }

            Transform registration = Child(goal, "Neutral Goal Registration Marks");
            BuildRevision2HorizontalSeal(registration, 1.40f, 0.126f, -1,
                revision21[2], 0.082f, 0.032f, new[] { 1, 3 });
            if (!unlocked) return;

            Transform completion = FindNamed(goal, "Unlocked Entrance Completion Segment");
            Renderer[] completionRenderers = completion.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < completionRenderers.Length; i++)
            {
                completionRenderers[i].sharedMaterial = lime;
                Transform segment = completionRenderers[i].transform;
                segment.localPosition = new Vector3(segment.localPosition.x, 0.137f,
                    segment.localPosition.z);
                segment.localScale = new Vector3(segment.localScale.x, 0.042f, 0.115f);
            }
            Transform extension = Child(goal, "Unlocked Success Perimeter Extension");
            BuildRevision2HorizontalSeal(extension, 1.42f, 0.137f, -1, lime,
                0.115f, 0.042f, new[] { 0, 4 });
        }

        private static void ApplyProductionLogo(Transform systems)
        {
            Transform card = FindNamed(systems, "P4 Pause Card");
            if (card == null)
                throw new InvalidOperationException("Pause card is missing for the approved logo.");
            Transform old = card.Find("Phase 5A Approved Wordmark");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            Phase4VisualSettings settings =
                AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(SettingsPath);
            Material violet = AssetDatabase.LoadAssetAtPath<Material>(
                Revision2ArtRoot + "/P5A_R2_Violet.mat");
            Material cyan = AssetDatabase.LoadAssetAtPath<Material>(
                Revision2ArtRoot + "/P5A_R2_Cyan.mat");
            Material white = AssetDatabase.LoadAssetAtPath<Material>(
                Revision2ArtRoot + "/P5A_R2_White.mat");
            if (settings == null || violet == null || cyan == null || white == null)
                throw new InvalidOperationException("Approved Phase 5A logo sources are incomplete.");
            BuildRevision21ScreenWordmark(card, "Phase 5A Approved Wordmark",
                new Vector2(0f, 205f), 1.25f, settings.PackagedJapaneseFont,
                white.color, violet.color, cyan.color, false);
        }

        private static void ApplyProductionAudio(Transform systems, Phase4AudioCueSet cueSet)
        {
            Phase4AudioController controller = systems.GetComponentInChildren<Phase4AudioController>(true);
            if (controller == null)
                throw new InvalidOperationException("Phase 4 audio controller is missing.");
            AudioSource[] sources = controller.GetComponentsInChildren<AudioSource>(true);
            controller.Configure(cueSet, sources);
        }

        private static Phase4AudioCueSet BuildProductionAudioCueSet(Phase4AudioCueSet fallback)
        {
            if (fallback == null || !fallback.HasAllCues)
                throw new InvalidOperationException("Fallback Phase 4 audio cue set is incomplete.");
            EnsureAssetFolder(ProductionAudioRoot);
            string repository = Directory.GetParent(Application.dataPath)?.Parent?.FullName;
            if (string.IsNullOrEmpty(repository))
                throw new InvalidOperationException("Repository root could not be resolved.");

            AudioClip[] approved = new AudioClip[ApprovedCueFiles.Length];
            for (int i = 0; i < ApprovedCueFiles.Length; i++)
            {
                string source = i == ApprovedCueFiles.Length - 1
                    ? Path.Combine(repository, "Captures", "Phase5A", "SelectedRevision21",
                        "Audio", ApprovedCueFiles[i])
                    : Path.Combine(repository, "Captures", "Phase5A", "SelectedRevision2",
                        "Audio", ApprovedCueFiles[i]);
                string assetPath = ProductionAudioRoot + "/" + ApprovedCueFiles[i];
                string destination = Path.GetFullPath(Path.Combine(Application.dataPath, "..",
                    assetPath));
                if (!File.Exists(source) && !File.Exists(destination))
                    throw new FileNotFoundException("Approved Human Review audio is missing.", source);
                if (File.Exists(source) && (!File.Exists(destination) ||
                    !FilesEqual(source, destination)))
                    File.Copy(source, destination, true);
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                approved[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
                if (approved[i] == null)
                    throw new InvalidOperationException($"Production audio import failed: {assetPath}");
            }

            AudioClip[] clips = new AudioClip[(int)Phase4AudioCue.Count];
            for (int i = 0; i < clips.Length; i++)
                clips[i] = fallback.Get((Phase4AudioCue)i);
            clips[(int)Phase4AudioCue.EchoSpawn] = approved[0];
            clips[(int)Phase4AudioCue.EchoSpawn2] = approved[1];
            clips[(int)Phase4AudioCue.EchoSpawn3] = approved[2];
            clips[(int)Phase4AudioCue.EchoRemove] = approved[3];
            clips[(int)Phase4AudioCue.LoopEnd] = approved[4];
            clips[(int)Phase4AudioCue.InteractionSuccess] = approved[5];
            clips[(int)Phase4AudioCue.BatteryInsert] = approved[6];
            clips[(int)Phase4AudioCue.SectionComplete] = approved[7];
            clips[(int)Phase4AudioCue.GameComplete] = approved[7];

            Phase4AudioCueSet set =
                AssetDatabase.LoadAssetAtPath<Phase4AudioCueSet>(ProductionAudioSetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<Phase4AudioCueSet>();
                AssetDatabase.CreateAsset(set, ProductionAudioSetPath);
            }
            set.Configure(clips);
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void BuildApprovedRealDecals(PuzzleSectionController section,
            Transform identityRoot, Material violet, Material white, Font font)
        {
            DoorController[] doors = section.GetComponentsInChildren<DoorController>(true);
            for (int i = 0; i < doors.Length; i++)
            {
                Transform root = CompensatedVisualRoot(doors[i].transform,
                    $"Approved Door Surface Decal {i + 1}");
                SelectedWorldText($"Door Sector Serial {i + 1}", "RECORD SECTOR 03", root,
                    new Vector3(0f, 2.95f, -0.31f), 0.17f,
                    new Color(white.color.r, white.color.g, white.color.b, 0.78f), font,
                    TextAnchor.MiddleCenter);
            }
            Transform floor = Child(identityRoot, "Approved Floor Surface Decal");
            floor.localPosition = new Vector3(0f, 0.018f, 7.3f);
            TextMesh floorText = SelectedWorldText("Floor Route Serial", "ECHO ROUTE 03", floor,
                Vector3.zero, 0.15f,
                new Color(violet.color.r, violet.color.g, violet.color.b, 0.72f), font,
                TextAnchor.MiddleCenter);
            floorText.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static Transform CompensatedVisualRoot(Transform parent, string name)
        {
            Transform root = Child(parent, name);
            Vector3 scale = parent.lossyScale;
            root.localScale = new Vector3(
                Mathf.Approximately(scale.x, 0f) ? 1f : 1f / scale.x,
                Mathf.Approximately(scale.y, 0f) ? 1f : 1f / scale.y,
                Mathf.Approximately(scale.z, 0f) ? 1f : 1f / scale.z);
            return root;
        }

        private static bool FilesEqual(string first, string second)
        {
            FileInfo a = new FileInfo(first);
            FileInfo b = new FileInfo(second);
            if (a.Length != b.Length) return false;
            byte[] left = File.ReadAllBytes(first);
            byte[] right = File.ReadAllBytes(second);
            for (int i = 0; i < left.Length; i++) if (left[i] != right[i]) return false;
            return true;
        }

        private static void ValidateProductionApplication(PuzzleSectionController[] sections,
            Transform systems, Phase4AudioCueSet audio)
        {
            if (sections[2].transform.Find(ProductionIdentityRootName) == null)
                throw new InvalidOperationException("Approved Section 3 identity root is missing.");
            if (systems.GetComponentInChildren<Phase4AudioController>(true)?.CueSet != audio)
                throw new InvalidOperationException("Approved Production audio is not wired.");
            for (int i = 0; i < sections.Length; i++)
            {
                Phase5AGoalIdentityVisual goal =
                    sections[i].Goal.GetComponent<Phase5AGoalIdentityVisual>();
                if (goal == null || !goal.HasRequiredReferences || goal.IsUnlockedVisual)
                    throw new InvalidOperationException($"Section {i + 1} goal identity is invalid.");
                if (sections[i].transform.GetComponentsInChildren<Collider>(true).Length == 0)
                    throw new InvalidOperationException($"Section {i + 1} gameplay colliders are missing.");
            }
            string[] forbidden =
            {
                "ACTUAL 64 PX", "4x Inspection", "Comparison", "Contact Sheet",
                "Preview Camera", "Evidence-only"
            };
            Transform[] objects = systems.root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < objects.Length; i++)
                for (int j = 0; j < forbidden.Length; j++)
                    if (objects[i].name.IndexOf(forbidden[j], StringComparison.OrdinalIgnoreCase) >= 0)
                        throw new InvalidOperationException(
                            $"Evidence-only object reached Production: {objects[i].name}");
        }
    }
}
