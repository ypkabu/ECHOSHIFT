using System;
using System.IO;
using EchoShift.Presentation;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.LowLevel;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class Phase4AssetBuilder
    {
        public const string SettingsPath = "Assets/_Project/Settings/Phase4VisualSettings.asset";
        public const string CapturePresetPath = "Assets/_Project/Settings/Phase4CapturePreset.asset";
        public const string CharacterCapturePresetPath =
            "Assets/_Project/Settings/Phase4CharacterCapturePreset.asset";
        public const string FontPath = "Assets/_Project/Fonts/ThirdParty/NotoSansJP/NotoSansJP-Regular.ttf";
        public const string TmpFontPath = "Assets/_Project/Fonts/NotoSansJP_Phase4.asset";
        public const string RobotAnimatorControllerPath =
            "Assets/_Project/Art/Phase4/Animation/P4_RobotVisual.controller";
        private const string ArtRoot = "Assets/_Project/Art/Phase4";
        private const string MaterialRoot = ArtRoot + "/Materials";
        private const string UiRoot = ArtRoot + "/UI";
        private const string AudioRoot = "Assets/_Project/Audio/Phase4";
        private const string PrefabRoot = "Assets/_Project/Prefabs/VFX";
        private const string VolumePath = ArtRoot + "/Phase4VolumeProfile.asset";
        private const string VfxPath = PrefabRoot + "/P4_Pulse.prefab";
        private static readonly Color[] EchoColors =
        {
            new Color(0.06f, 0.78f, 1f, 1f),
            new Color(0.66f, 0.32f, 1f, 1f),
            new Color(0.16f, 1f, 0.68f, 1f)
        };

        public static Phase4VisualSettings Build(Phase3TextCatalog catalog)
        {
            EnsureFolders();
            EnsureTmpSettings();
            Phase4ExternalAssetBuilder.Build();
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (font == null) throw new InvalidOperationException(
                $"Packaged Noto Sans JP font is missing at {FontPath}.");
            TMP_FontAsset tmpFont = BuildTmpFont(font, catalog);

            Material panels = Material("FacilityPanels", new Color(0.68f, 0.72f, 0.76f), 0.18f, 0.72f);
            Material dark = Material("FacilityDark", new Color(0.025f, 0.038f, 0.06f), 0.78f, 0.68f);
            Material trim = Material("FacilityTrim", new Color(0.08f, 0.13f, 0.19f), 0.58f, 0.82f);
            Material player = Material("PlayerAccent", new Color(1f, 0.68f, 0.12f), 0.3f, 0.58f, 1.4f);
            Material plate = Material("PlateAccent", new Color(0.05f, 0.82f, 1f), 0.2f, 0.62f, 1.15f);
            Material battery = Material("BatteryAccent", new Color(1f, 0.36f, 0.055f), 0.25f, 0.7f, 1.4f);
            Material goal = Material("GoalAccent", new Color(0.72f, 1f, 0.82f), 0.1f, 0.55f, 1.7f);
            Material danger = Material("DangerAccent", new Color(1f, 0.12f, 0.16f), 0.15f, 0.55f, 1.4f);
            Material glass = TransparentMaterial("FacilityGlass", new Color(0.12f, 0.38f, 0.52f, 0.27f));
            Material[] echoes = new Material[EchoColors.Length];
            for (int i = 0; i < echoes.Length; i++)
                echoes[i] = Material($"Echo{i + 1}", EchoColors[i], 0.1f, 0.62f, 1.55f);

            Sprite[] icons = BuildHudIcons();
            Phase4AudioCueSet cues = BuildAudioCues();
            GameObject vfx = BuildVfxPrefab(plate);
            VolumeProfile volume = BuildVolumeProfile();
            BuildRobotAnimatorController();

            Phase4VisualSettings settings =
                AssetDatabase.LoadAssetAtPath<Phase4VisualSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<Phase4VisualSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            settings.Configure(panels, dark, trim, player, plate, battery, goal, danger,
                glass, echoes, font, tmpFont, vfx, cues, icons, volume);
            BuildCapturePreset();
            BuildCharacterCapturePreset();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static void BuildCapturePreset()
        {
            Phase4CapturePreset preset =
                AssetDatabase.LoadAssetAtPath<Phase4CapturePreset>(CapturePresetPath);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<Phase4CapturePreset>();
                AssetDatabase.CreateAsset(preset, CapturePresetPath);
            }
            preset.Configure(new[]
            {
                new Phase4CaptureShotPreset("01_section1_plate_door_overview.png",
                    Phase4CaptureMoment.Section1Overview, 1,
                    new Vector3(0f, 0f, -0.6f), new Vector3(0f, 16.5f, -12.5f), 52f, false),
                new Phase4CaptureShotPreset("02_player_echo1_identity.png",
                    Phase4CaptureMoment.PlayerAndEchoOne, 1,
                    new Vector3(-0.4f, 0f, -5.2f), new Vector3(-2.4f, 10.5f, -7.2f), 43f, false),
                new Phase4CaptureShotPreset("03_echo_plate_player_door.png",
                    Phase4CaptureMoment.EchoOnPlatePlayerAtDoor, 1,
                    new Vector3(-0.4f, 0f, -1.5f), new Vector3(2.2f, 12.5f, -8.8f), 46f, false),
                new Phase4CaptureShotPreset("04_battery_held_close.png",
                    Phase4CaptureMoment.BatteryHeldClose, 2,
                    new Vector3(0.6f, 0.4f, -2.5f), new Vector3(-3.2f, 8.5f, -5.8f), 40f, false),
                new Phase4CaptureShotPreset("05_recorded_insert_door_open.png",
                    Phase4CaptureMoment.RecordedInsertionDoorOpen, 2,
                    new Vector3(0.4f, 0f, 0.2f), new Vector3(2.8f, 11.5f, -7.5f), 44f, false),
                new Phase4CaptureShotPreset("06_echo1_echo2_roles.png",
                    Phase4CaptureMoment.TwoEchoRoles, 3,
                    new Vector3(-0.3f, 0f, -2f), new Vector3(-1f, 14.5f, -10.5f), 49f, false),
                new Phase4CaptureShotPreset("07_player_goal_arrival.png",
                    Phase4CaptureMoment.GoalArrival, 3,
                    new Vector3(0f, 0.5f, 9.2f), new Vector3(2.4f, 9.2f, -6.2f), 42f, false),
                new Phase4CaptureShotPreset("08_compact_gameplay_hud.png",
                    Phase4CaptureMoment.GameplayHud, 3,
                    new Vector3(0f, 0f, -0.5f), new Vector3(0f, 15.2f, -11.2f), 50f, true)
            });
            EditorUtility.SetDirty(preset);
        }

        private static void BuildRobotAnimatorController()
        {
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>(RobotAnimatorControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(
                    RobotAnimatorControllerPath);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            string[] names =
            {
                "Idle", "Walk", "Carry Idle", "Carry Walk", "Interact", "Echo Stopped"
            };
            AnimatorState idle = null;
            for (int i = 0; i < names.Length; i++)
            {
                string clipPath = $"{ArtRoot}/Animation/P4_{names[i].Replace(" ", string.Empty)}.anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    clip = new AnimationClip { name = names[i], wrapMode = WrapMode.Loop };
                    AssetDatabase.CreateAsset(clip, clipPath);
                }
                AnimationUtility.SetAnimationEvents(clip, System.Array.Empty<AnimationEvent>());
                EditorUtility.SetDirty(clip);

                AnimatorState state = FindState(machine, names[i]);
                if (state == null) state = machine.AddState(names[i]);
                state.motion = clip;
                state.writeDefaultValues = false;
                if (i == 0) idle = state;
            }
            machine.defaultState = idle;
            EditorUtility.SetDirty(controller);
        }

        private static void BuildCharacterCapturePreset()
        {
            Phase4CapturePreset preset =
                AssetDatabase.LoadAssetAtPath<Phase4CapturePreset>(CharacterCapturePresetPath);
            if (preset == null)
            {
                preset = ScriptableObject.CreateInstance<Phase4CapturePreset>();
                AssetDatabase.CreateAsset(preset, CharacterCapturePresetPath);
            }
            preset.Configure(new[]
            {
                new Phase4CaptureShotPreset("01_player_idle.png",
                    Phase4CaptureMoment.PlayerIdle, 1,
                    new Vector3(0f, 0.1f, -5.4f), new Vector3(-2.8f, 8.8f, -6.4f), 40f, false),
                new Phase4CaptureShotPreset("02_player_walk_turn.png",
                    Phase4CaptureMoment.PlayerWalk, 1,
                    new Vector3(-1.1f, 0.1f, -4.5f), new Vector3(3.4f, 9.4f, -5.9f), 41f, false),
                new Phase4CaptureShotPreset("03_player_echo_pose.png",
                    Phase4CaptureMoment.PlayerEchoPose, 1,
                    new Vector3(-0.5f, 0.1f, -4.7f), new Vector3(-3.5f, 10.2f, -6.8f), 43f, false),
                new Phase4CaptureShotPreset("04_battery_carry_idle.png",
                    Phase4CaptureMoment.BatteryCarryIdle, 2,
                    new Vector3(0.6f, 0.4f, -2.5f), new Vector3(-3.0f, 8.2f, 5.4f), 39f, false),
                new Phase4CaptureShotPreset("05_battery_carry_walk.png",
                    Phase4CaptureMoment.BatteryCarryWalk, 2,
                    new Vector3(0.5f, 0.35f, -1.7f), new Vector3(3.5f, 9.1f, 6.2f), 41f, false),
                new Phase4CaptureShotPreset("06_battery_insertion.png",
                    Phase4CaptureMoment.BatteryInsertion, 2,
                    new Vector3(0.7f, 0.25f, -0.5f), new Vector3(-3.2f, 8.6f, -5.5f), 38f, false),
                new Phase4CaptureShotPreset("07_split_door_open.png",
                    Phase4CaptureMoment.DoorOpen, 2,
                    new Vector3(0f, 0.1f, 0.9f), new Vector3(3.4f, 9.8f, -6.0f), 40f, false),
                new Phase4CaptureShotPreset("08_two_echo_roles.png",
                    Phase4CaptureMoment.CharacterTwoEchoRoles, 3,
                    new Vector3(-0.3f, 0.1f, -2f), new Vector3(-1.2f, 14.5f, -10.4f), 49f, false)
            });
            EditorUtility.SetDirty(preset);
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            ChildAnimatorState[] states = machine.states;
            for (int i = 0; i < states.Length; i++)
                if (states[i].state != null && states[i].state.name == name) return states[i].state;
            return null;
        }

        private static TMP_FontAsset BuildTmpFont(Font source, Phase3TextCatalog catalog)
        {
            TMP_FontAsset asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TmpFontPath);
            if (asset != null && (asset.characterTable == null || asset.characterTable.Count < 100))
            {
                AssetDatabase.DeleteAsset(TmpFontPath);
                asset = null;
            }
            bool created = asset == null;
            if (created)
            {
                asset = TMP_FontAsset.CreateFontAsset(source, 48, 5,
                    GlyphRenderMode.SDFAA, 2048, 2048,
                    AtlasPopulationMode.Dynamic, true);
                asset.name = "NotoSansJP_Phase4";
                AssetDatabase.CreateAsset(asset, TmpFontPath);
                if (asset.material != null && !AssetDatabase.Contains(asset.material))
                    AssetDatabase.AddObjectToAsset(asset.material, asset);
                Texture2D[] atlases = asset.atlasTextures;
                for (int i = 0; i < atlases.Length; i++)
                {
                    if (atlases[i] != null && !AssetDatabase.Contains(atlases[i]))
                        AssetDatabase.AddObjectToAsset(atlases[i], asset);
                }
            }

            string required = catalog.GetRequiredGlyphCharacters() +
                "ECHO SHIFT PHASE SECTION LOOP 0123456789:/.-+[]()";
            if (created)
            {
                if (!asset.TryAddCharacters(required, out string missing) ||
                    !string.IsNullOrEmpty(missing))
                    throw new InvalidOperationException(
                        $"Noto Sans JP TMP atlas is missing required glyphs: {missing}");
                asset.atlasPopulationMode = AtlasPopulationMode.Static;
                SerializedObject serialized = new SerializedObject(asset);
                SerializedProperty clearOnBuild = serialized.FindProperty("m_ClearDynamicDataOnBuild");
                if (clearOnBuild != null) clearOnBuild.boolValue = false;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(asset);
            if (created) AssetDatabase.ImportAsset(TmpFontPath, ImportAssetOptions.ForceUpdate);
            return asset;
        }

        private static Material Material(
            string name, Color color, float metallic, float smoothness, float emission = 0f)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission > 0f)
            {
                material.SetColor("_EmissionColor", color * emission);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags =
                    MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags =
                    MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material TransparentMaterial(string name, Color color)
        {
            Material material = Material(name, color, 0.05f, 0.88f);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Sprite[] BuildHudIcons()
        {
            Sprite[] icons = new Sprite[Phase4VisualSettings.RequiredHudIcons];
            string[] names = { "Loop", "Timer", "Echo", "Carry", "Interact", "Pause" };
            for (int i = 0; i < icons.Length; i++)
            {
                string path = $"{UiRoot}/{names[i]}Icon.asset";
                icons[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (icons[i] != null) continue;
                Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false)
                {
                    name = names[i] + "IconTexture",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                Color clear = new Color(0f, 0f, 0f, 0f);
                Color ink = i == 3 ? new Color(1f, 0.36f, 0.055f) : new Color(0.1f, 0.82f, 1f);
                for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    float dx = x - 15.5f;
                    float dy = y - 15.5f;
                    float radius = Mathf.Sqrt(dx * dx + dy * dy);
                    bool ring = radius > 8f && radius < 12.5f;
                    bool spoke = Mathf.Abs(dx) < 2f || Mathf.Abs(dy) < 2f;
                    texture.SetPixel(x, y, ring || (spoke && radius < 8f) ? ink : clear);
                }
                texture.Apply();
                AssetDatabase.CreateAsset(texture, path);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32),
                    new Vector2(0.5f, 0.5f), 32f);
                sprite.name = names[i] + "Icon";
                AssetDatabase.AddObjectToAsset(sprite, texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                icons[i] = sprite;
            }
            return icons;
        }

        private static Phase4AudioCueSet BuildAudioCues()
        {
            string assetPath = AudioRoot + "/Phase4AudioCueSet.asset";
            Phase4AudioCueSet set = AssetDatabase.LoadAssetAtPath<Phase4AudioCueSet>(assetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<Phase4AudioCueSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }
            AudioClip[] clips = new AudioClip[(int)Phase4AudioCue.Count];
            for (int i = 0; i < clips.Length; i++)
            {
                string externalPath = ExternalAudioPath((Phase4AudioCue)i);
                if (!string.IsNullOrEmpty(externalPath))
                {
                    clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(externalPath);
                    if (clips[i] == null)
                        throw new InvalidOperationException($"External audio import failed: {externalPath}");
                    continue;
                }
                string wavPath = $"{AudioRoot}/{(Phase4AudioCue)i}.wav";
                string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", wavPath));
                if (!File.Exists(absolute))
                    File.WriteAllBytes(absolute, GenerateWav(0.14f + (i % 4) * 0.035f,
                        280f + i * 45f, i == (int)Phase4AudioCue.InteractionFailure));
                AssetDatabase.ImportAsset(wavPath, ImportAssetOptions.ForceUpdate);
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(wavPath);
                if (clips[i] == null) throw new InvalidOperationException($"Audio import failed: {wavPath}");
            }
            set.Configure(clips);
            EditorUtility.SetDirty(set);
            return set;
        }

        private static string ExternalAudioPath(Phase4AudioCue cue)
        {
            const string root = "Assets/_Project/ThirdParty/Kenney/SciFiSounds/Audio/";
            switch (cue)
            {
                case Phase4AudioCue.InteractionSuccess: return root + "laserSmall_000.ogg";
                case Phase4AudioCue.InteractionFailure: return root + "lowFrequency_explosion_001.ogg";
                case Phase4AudioCue.BatteryPickup: return root + "impactMetal_001.ogg";
                case Phase4AudioCue.Door: return root + "doorOpen_001.ogg";
                case Phase4AudioCue.EchoSpawn: return root + "forceField_000.ogg";
                case Phase4AudioCue.GameComplete: return root + "forceField_004.ogg";
                default: return string.Empty;
            }
        }

        private static byte[] GenerateWav(float seconds, float frequency, bool descending)
        {
            const int sampleRate = 22050;
            int count = Mathf.CeilToInt(seconds * sampleRate);
            byte[] bytes = new byte[44 + count * 2];
            WriteAscii(bytes, 0, "RIFF"); WriteInt(bytes, 4, bytes.Length - 8);
            WriteAscii(bytes, 8, "WAVEfmt "); WriteInt(bytes, 16, 16);
            WriteShort(bytes, 20, 1); WriteShort(bytes, 22, 1); WriteInt(bytes, 24, sampleRate);
            WriteInt(bytes, 28, sampleRate * 2); WriteShort(bytes, 32, 2); WriteShort(bytes, 34, 16);
            WriteAscii(bytes, 36, "data"); WriteInt(bytes, 40, count * 2);
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)sampleRate;
                float envelope = Mathf.Sin(Mathf.Clamp01(i / (float)count) * Mathf.PI);
                float pitch = descending ? frequency * (1f - 0.35f * i / count) : frequency;
                short sample = (short)(Mathf.Sin(t * pitch * Mathf.PI * 2f) * envelope * 7600f);
                WriteShort(bytes, 44 + i * 2, sample);
            }
            return bytes;
        }

        private static GameObject BuildVfxPrefab(Material material)
        {
            GameObject root = new GameObject("P4 Pulse VFX");
            ParticleSystem particles = root.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false; main.loop = false; main.duration = 0.8f;
            main.startLifetime = 0.55f; main.startSpeed = 2.2f; main.startSize = 0.14f;
            main.maxParticles = 32; main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere; shape.radius = 0.24f;
            ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, VfxPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static VolumeProfile BuildVolumeProfile()
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumePath);
            }
            for (int i = profile.components.Count - 1; i >= 0; i--)
            {
                VolumeComponent existing = profile.components[i];
                profile.components.RemoveAt(i);
                if (existing != null) Object.DestroyImmediate(existing, true);
            }
            Tonemapping tone = AddVolumeComponent<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.ACES);
            Bloom bloom = AddVolumeComponent<Bloom>(profile);
            bloom.intensity.Override(0.3f); bloom.threshold.Override(1.2f); bloom.scatter.Override(0.48f);
            ColorAdjustments color = AddVolumeComponent<ColorAdjustments>(profile);
            color.postExposure.Override(0.35f); color.contrast.Override(2f); color.saturation.Override(-2f);
            Vignette vignette = AddVolumeComponent<Vignette>(profile);
            vignette.intensity.Override(0.07f); vignette.smoothness.Override(0.28f);
            EditorUtility.SetDirty(profile);
            return profile;
        }

        private static T AddVolumeComponent<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T component = profile.Add<T>();
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
        }

        private static void EnsureFolders()
        {
            Folder("Assets/_Project/Art", "Phase4");
            Folder(ArtRoot, "Materials"); Folder(ArtRoot, "UI");
            Folder(ArtRoot, "Animation");
            Folder("Assets/_Project", "Audio"); Folder("Assets/_Project/Audio", "Phase4");
            Folder("Assets/_Project/Prefabs", "VFX");
            Folder("Assets/_Project", "Resources");
        }

        private static void EnsureTmpSettings()
        {
            if (TMP_Settings.instance != null &&
                Shader.Find("TextMeshPro/Mobile/Distance Field") != null) return;
            const string temporaryPath = "Assets/_Project/Resources/TMP Settings.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(temporaryPath) != null)
                AssetDatabase.DeleteAsset(temporaryPath);
            string packagePath = Path.GetFullPath(
                "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage");
            if (!File.Exists(packagePath))
                throw new FileNotFoundException("TMP Essential Resources package was not found.", packagePath);
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (Resources.Load<TMP_Settings>("TMP Settings") == null ||
                Shader.Find("TextMeshPro/Mobile/Distance Field") == null)
                throw new InvalidOperationException("TMP Essential Resources could not be imported.");
        }

        private static void Folder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void WriteAscii(byte[] bytes, int offset, string value)
        {
            for (int i = 0; i < value.Length; i++) bytes[offset + i] = (byte)value[i];
        }
        private static void WriteInt(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)value; bytes[offset + 1] = (byte)(value >> 8);
            bytes[offset + 2] = (byte)(value >> 16); bytes[offset + 3] = (byte)(value >> 24);
        }
        private static void WriteShort(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)value; bytes[offset + 1] = (byte)(value >> 8);
        }
    }
}
