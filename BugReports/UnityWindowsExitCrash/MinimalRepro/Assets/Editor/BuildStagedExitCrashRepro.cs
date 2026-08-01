using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityWindowsExitCrashRepro;

namespace UnityWindowsExitCrashRepro.Editor
{
    public static class BuildStagedExitCrashRepro
    {
        private const string ScenePath = "Assets/ExitCrashRepro.unity";

        public static void BuildFromCommandLine()
        {
            string stageName = GetArgument("-reproStage", "A").ToUpperInvariant();
            if (stageName.Length != 1 || stageName[0] < 'A' || stageName[0] > 'I')
            {
                throw new ArgumentException($"Unsupported repro stage: {stageName}");
            }

            int stage = stageName[0] - 'A';
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string output = GetArgument(
                "-reproOutput",
                Path.Combine(projectRoot, "Builds", stageName, "MinimalExitCrash.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? projectRoot);

            ConfigureGraphics();
            if (stage >= 6)
            {
                ApplyProductPlayerSettings();
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            if (stage >= 1)
            {
                AddUrp(stageName);
            }
            if (stage >= 2)
            {
                AddComponentByName(new GameObject("Input System"), "UnityEngine.InputSystem.PlayerInput, Unity.InputSystem");
            }
            if (stage >= 3)
            {
                AddUiAndTmp();
            }
            if (stage >= 4)
            {
                AddComponentByName(new GameObject("Global Volume"), "UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
            }
            if (stage >= 5)
            {
                AddComponentByName(new GameObject("Audio"), "UnityEngine.AudioSource, UnityEngine.AudioModule");
            }
            if (stage >= 7)
            {
                ExitCrashRepro repro = new GameObject("Project Quit Path").AddComponent<ExitCrashRepro>();
                SerializedObject serialized = new SerializedObject(repro);
                serialized.FindProperty("useProjectQuitPath").boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                new GameObject("Window Close Marker").AddComponent<ExitCrashRepro>();
            }
            if (stage >= 8)
            {
                AddMinimalVisualAsset();
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.CleanBuildCache
            });

            BuildSummary summary = report.summary;
            Debug.Log(
                $"EXIT_REPRO_BUILD stage={stageName};unity={Application.unityVersion};" +
                $"result={summary.result};warnings={summary.totalWarnings};errors={summary.totalErrors};" +
                $"output={output}");
            if (summary.result != BuildResult.Succeeded || summary.totalErrors != 0)
            {
                throw new InvalidOperationException(
                    $"Stage {stageName} build failed: {summary.result}; errors={summary.totalErrors}");
            }
        }

        private static void ConfigureGraphics()
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(
                BuildTarget.StandaloneWindows64,
                new[] { GraphicsDeviceType.Direct3D11 });
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
        }

        private static void ApplyProductPlayerSettings()
        {
            PlayerSettings.companyName = "Echo Shift Prototype";
            PlayerSettings.productName = "ECHO SHIFT Exit Repro";
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = false;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.useFlipModelSwapchain = true;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.02f, 0.03f, 0.05f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void AddUrp(string stageName)
        {
            Type assetType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime");
            Type rendererType = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalRendererData, Unity.RenderPipelines.Universal.Runtime");
            if (assetType == null || rendererType == null)
            {
                throw new InvalidOperationException($"URP types unavailable in stage {stageName}");
            }

            const string assetPath = "Assets/ReproUniversalRenderPipelineAsset.asset";
            const string rendererPath = "Assets/ReproUniversalRenderer.asset";
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.DeleteAsset(rendererPath);
            ScriptableObject renderer = ScriptableObject.CreateInstance(rendererType);
            AssetDatabase.CreateAsset(renderer, rendererPath);
            System.Reflection.MethodInfo create = assetType.GetMethod(
                "Create",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (create == null)
            {
                throw new InvalidOperationException("URP Create method unavailable.");
            }
            RenderPipelineAsset pipeline = (RenderPipelineAsset)create.Invoke(null, new object[] { renderer });
            AssetDatabase.CreateAsset(pipeline, assetPath);
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            AssetDatabase.SaveAssets();
        }

        private static void AddUiAndTmp()
        {
            Type settingsType = Type.GetType("TMPro.TMP_Settings, Unity.TextMeshPro");
            if (settingsType == null)
            {
                throw new InvalidOperationException("TMP settings type unavailable.");
            }
            const string resourcesFolder = "Assets/Resources";
            const string settingsPath = resourcesFolder + "/TMP Settings.asset";
            if (!AssetDatabase.IsValidFolder(resourcesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            ScriptableObject settingsAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(settingsPath);
            if (settingsAsset == null)
            {
                settingsAsset = ScriptableObject.CreateInstance(settingsType);
                AssetDatabase.CreateAsset(settingsAsset, settingsPath);
            }
            SerializedObject settingsSerialized = new SerializedObject(settingsAsset);
            SerializedProperty assetVersion = settingsSerialized.FindProperty("assetVersion");
            if (assetVersion != null)
            {
                assetVersion.stringValue = "2";
                settingsSerialized.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();

            GameObject canvas = new GameObject("Canvas");
            AddComponentByName(canvas, "UnityEngine.Canvas, UnityEngine.UIModule");
            AddComponentByName(canvas, "UnityEngine.UI.CanvasScaler, UnityEngine.UI");
            GameObject text = new GameObject("TMP Label");
            text.transform.SetParent(canvas.transform, false);
            AddComponentByName(text, "TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        }

        private static void AddMinimalVisualAsset()
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Minimal Visual Component";
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.1f, 0.8f, 0.9f, 1f);
            visual.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Component AddComponentByName(GameObject target, string qualifiedTypeName)
        {
            Type type = Type.GetType(qualifiedTypeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Required type unavailable: {qualifiedTypeName}");
            }
            return target.AddComponent(type);
        }

        private static string GetArgument(string name, string fallback)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }
            return fallback;
        }
    }
}
