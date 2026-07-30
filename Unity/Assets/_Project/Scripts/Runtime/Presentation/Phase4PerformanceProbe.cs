using System;
using System.Collections;
using System.IO;
using EchoShift.Core;
using EchoShift.Gameplay;
using Unity.Profiling;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4PerformanceProbe : MonoBehaviour
    {
        private const int WarmupFrames = 120;
        private const int RecorderWarmupFrames = 60;
        private const int ComponentCostFrames = 120;
        private const int SampleFrames = 600;
        private const float EchoPreparationTimeoutSeconds = 5f;
        [SerializeField] private SectionTransitionCoordinator coordinator;
        private readonly float[] _frameMilliseconds = new float[SampleFrames];
        private ProfilerRecorder _mainThread;
        private ProfilerRecorder _gpuFrameTime;
        private ProfilerRecorder _gcAllocated;
        private ProfilerRecorder _drawCalls;
        private ProfilerRecorder _setPassCalls;
        private ProfilerRecorder _triangles;
        private ProfilerRecorder _vertices;
        private ProfilerRecorder _usedMemory;
        private ProfilerRecorder _textureMemory;
        private ProfilerRecorder _cameraUpdate;
        private ProfilerRecorder _uiUpdate;
        private ProfilerRecorder _animatorUpdate;
        private ProfilerRecorder _doorVisualUpdate;
        private bool _recording;
        private float _transitionMaximumFrameMs;
        private float _transitionMaximumMainThreadMs;

        [Serializable]
        private sealed class Result
        {
            public string unityVersion;
            public string device;
            public int width;
            public int height;
            public int samples;
            public float averageFrameMs;
            public float p95FrameMs;
            public float maximumFrameMs;
            public float averageMainThreadMs;
            public float maximumMainThreadMs;
            public float averageCameraUpdateMs;
            public float maximumCameraUpdateMs;
            public float averageUiUpdateMs;
            public float maximumUiUpdateMs;
            public float averageAnimatorUpdateMs;
            public float maximumAnimatorUpdateMs;
            public float averageDoorVisualUpdateMs;
            public float maximumDoorVisualUpdateMs;
            public float averageGpuFrameMs;
            public float maximumGpuFrameMs;
            public float transitionMaximumFrameMs;
            public float transitionMaximumMainThreadMs;
            public long maximumGcBytesPerFrame;
            public long totalGcBytes;
            public int nonZeroGcFrames;
            public long maximumDrawCalls;
            public long maximumSetPassCalls;
            public long maximumTriangles;
            public long maximumVertices;
            public long maximumUsedMemoryBytes;
            public long maximumTextureMemoryBytes;
            public int finalEchoCount;
            public int requiredEchoCount;
            public bool maximumEchoConditionReached;
            public bool steadyStateGcIsZero;
            public bool mainThreadAvailable;
            public bool gpuFrameTimeAvailable;
            public bool gcAvailable;
            public bool drawCallsAvailable;
            public bool setPassAvailable;
            public bool trianglesAvailable;
            public bool verticesAvailable;
            public bool memoryAvailable;
            public bool textureMemoryAvailable;
            public bool cameraUpdateAvailable;
            public bool uiUpdateAvailable;
            public bool animatorUpdateAvailable;
            public bool doorVisualUpdateAvailable;
            public bool graphicalDevice;
        }

        public void Configure(SectionTransitionCoordinator sectionCoordinator) =>
            coordinator = sectionCoordinator;

        private void Start()
        {
            if (!HasFlag("-phase4PerfProbe"))
            {
                enabled = false;
                return;
            }
            StartCoroutine(RunProbe());
        }

        private IEnumerator RunProbe()
        {
            Application.targetFrameRate = 120;
            QualitySettings.vSyncCount = 0;
            Screen.SetResolution(1920, 1080, false);
            _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            _gpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time", 1);
            _gcAllocated = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 1);
            _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
            _setPassCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count", 1);
            _triangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count", 1);
            _vertices = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count", 1);
            _usedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1);
            _textureMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Texture Memory", 1);
            _cameraUpdate = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts, SectionCameraController.ProfilerMarkerName, 1);
            _uiUpdate = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts, Phase4HudVisual.ProfilerMarkerName, 1);
            _animatorUpdate = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts, Phase4RobotPoseController.ProfilerMarkerName, 1);
            _doorVisualUpdate = ProfilerRecorder.StartNew(
                ProfilerCategory.Scripts, DoorVisualFeedback.ProfilerMarkerName, 1);
            _recording = true;
            for (int i = 0; i < RecorderWarmupFrames; i++) yield return null;
            yield return PrepareMaximumEchoes();
            float totalAnimatorUpdate = 0f;
            float maximumAnimatorUpdate = 0f;
            float totalDoorVisualUpdate = 0f;
            float maximumDoorVisualUpdate = 0f;
            bool animatorUpdateAvailable = _animatorUpdate.Valid;
            bool doorVisualUpdateAvailable = _doorVisualUpdate.Valid;
            for (int frame = 0; frame < ComponentCostFrames; frame++)
            {
                yield return null;
                if (_animatorUpdate.Valid)
                {
                    float animatorMs = _animatorUpdate.LastValue / 1000000f;
                    totalAnimatorUpdate += animatorMs;
                    maximumAnimatorUpdate = Mathf.Max(maximumAnimatorUpdate, animatorMs);
                }
                if (_doorVisualUpdate.Valid)
                {
                    float doorMs = _doorVisualUpdate.LastValue / 1000000f;
                    totalDoorVisualUpdate += doorMs;
                    maximumDoorVisualUpdate = Mathf.Max(maximumDoorVisualUpdate, doorMs);
                }
            }
            // These two recorders add measurement-side managed traffic on this
            // Unity version. Dispose them before the independent steady-GC window.
            if (_animatorUpdate.Valid) _animatorUpdate.Dispose();
            if (_doorVisualUpdate.Valid) _doorVisualUpdate.Dispose();
            for (int i = 0; i < WarmupFrames; i++) yield return null;

            float total = 0f;
            float totalMain = 0f;
            float totalGpu = 0f;
            float maximum = 0f;
            float maximumMain = 0f;
            float maximumGpu = 0f;
            float totalCameraUpdate = 0f;
            float maximumCameraUpdate = 0f;
            float totalUiUpdate = 0f;
            float maximumUiUpdate = 0f;
            long maximumGc = 0;
            long totalGc = 0;
            int nonZeroGcFrames = 0;
            long maximumDraw = 0;
            long maximumSetPass = 0;
            long maximumTriangles = 0;
            long maximumVertices = 0;
            long maximumMemory = 0;
            long maximumTextureMemory = 0;
            for (int frame = 0; frame < SampleFrames; frame++)
            {
                yield return null;
                float frameMs = Time.unscaledDeltaTime * 1000f;
                _frameMilliseconds[frame] = frameMs;
                total += frameMs;
                if (frameMs > maximum) maximum = frameMs;
                if (_mainThread.Valid)
                {
                    float mainMs = _mainThread.LastValue / 1000000f;
                    totalMain += mainMs;
                    maximumMain = Mathf.Max(maximumMain, mainMs);
                }
                if (_gpuFrameTime.Valid)
                {
                    float gpuMs = _gpuFrameTime.LastValue / 1000000f;
                    totalGpu += gpuMs;
                    maximumGpu = Mathf.Max(maximumGpu, gpuMs);
                }
                if (_cameraUpdate.Valid)
                {
                    float cameraMs = _cameraUpdate.LastValue / 1000000f;
                    totalCameraUpdate += cameraMs;
                    maximumCameraUpdate = Mathf.Max(maximumCameraUpdate, cameraMs);
                }
                if (_uiUpdate.Valid)
                {
                    float uiMs = _uiUpdate.LastValue / 1000000f;
                    totalUiUpdate += uiMs;
                    maximumUiUpdate = Mathf.Max(maximumUiUpdate, uiMs);
                }
                if (_gcAllocated.Valid)
                {
                    long gcBytes = _gcAllocated.LastValue;
                    maximumGc = Math.Max(maximumGc, gcBytes);
                    totalGc += gcBytes;
                    if (gcBytes > 0) nonZeroGcFrames++;
                }
                if (_drawCalls.Valid) maximumDraw = Math.Max(maximumDraw, _drawCalls.LastValue);
                if (_setPassCalls.Valid) maximumSetPass = Math.Max(maximumSetPass, _setPassCalls.LastValue);
                if (_triangles.Valid) maximumTriangles = Math.Max(maximumTriangles, _triangles.LastValue);
                if (_vertices.Valid) maximumVertices = Math.Max(maximumVertices, _vertices.LastValue);
                if (_usedMemory.Valid) maximumMemory = Math.Max(maximumMemory, _usedMemory.LastValue);
                if (_textureMemory.Valid)
                    maximumTextureMemory = Math.Max(maximumTextureMemory, _textureMemory.LastValue);
            }

            Array.Sort(_frameMilliseconds);
            int requiredEchoCount = coordinator != null && coordinator.ActiveSection != null
                ? coordinator.ActiveSection.Director.MaxEchoes
                : -1;
            int finalEchoCount = coordinator != null && coordinator.ActiveSection != null
                ? coordinator.ActiveSection.Director.EchoCount
                : -1;
            Result result = new Result
            {
                unityVersion = Application.unityVersion,
                device = SystemInfo.graphicsDeviceName,
                width = Screen.width,
                height = Screen.height,
                samples = SampleFrames,
                averageFrameMs = total / SampleFrames,
                p95FrameMs = _frameMilliseconds[Mathf.FloorToInt(SampleFrames * 0.95f) - 1],
                maximumFrameMs = maximum,
                averageMainThreadMs = _mainThread.Valid ? totalMain / SampleFrames : 0f,
                maximumMainThreadMs = maximumMain,
                averageCameraUpdateMs = _cameraUpdate.Valid
                    ? totalCameraUpdate / SampleFrames : 0f,
                maximumCameraUpdateMs = maximumCameraUpdate,
                averageUiUpdateMs = _uiUpdate.Valid ? totalUiUpdate / SampleFrames : 0f,
                maximumUiUpdateMs = maximumUiUpdate,
                averageAnimatorUpdateMs = animatorUpdateAvailable
                    ? totalAnimatorUpdate / ComponentCostFrames : 0f,
                maximumAnimatorUpdateMs = maximumAnimatorUpdate,
                averageDoorVisualUpdateMs = doorVisualUpdateAvailable
                    ? totalDoorVisualUpdate / ComponentCostFrames : 0f,
                maximumDoorVisualUpdateMs = maximumDoorVisualUpdate,
                averageGpuFrameMs = _gpuFrameTime.Valid ? totalGpu / SampleFrames : 0f,
                maximumGpuFrameMs = maximumGpu,
                transitionMaximumFrameMs = _transitionMaximumFrameMs,
                transitionMaximumMainThreadMs = _transitionMaximumMainThreadMs,
                maximumGcBytesPerFrame = maximumGc,
                totalGcBytes = totalGc,
                nonZeroGcFrames = nonZeroGcFrames,
                maximumDrawCalls = maximumDraw,
                maximumSetPassCalls = maximumSetPass,
                maximumTriangles = maximumTriangles,
                maximumVertices = maximumVertices,
                maximumUsedMemoryBytes = maximumMemory,
                maximumTextureMemoryBytes = maximumTextureMemory,
                finalEchoCount = finalEchoCount,
                requiredEchoCount = requiredEchoCount,
                maximumEchoConditionReached = finalEchoCount == requiredEchoCount,
                steadyStateGcIsZero = _gcAllocated.Valid && nonZeroGcFrames == 0,
                mainThreadAvailable = _mainThread.Valid,
                gpuFrameTimeAvailable = _gpuFrameTime.Valid,
                gcAvailable = _gcAllocated.Valid,
                drawCallsAvailable = _drawCalls.Valid,
                setPassAvailable = _setPassCalls.Valid,
                trianglesAvailable = _triangles.Valid,
                verticesAvailable = _vertices.Valid,
                memoryAvailable = _usedMemory.Valid,
                textureMemoryAvailable = _textureMemory.Valid,
                cameraUpdateAvailable = _cameraUpdate.Valid,
                uiUpdateAvailable = _uiUpdate.Valid,
                animatorUpdateAvailable = animatorUpdateAvailable,
                doorVisualUpdateAvailable = doorVisualUpdateAvailable,
                graphicalDevice = SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null
            };
            string directory = Path.Combine(Application.persistentDataPath, "Phase4Performance");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "performance.json");
            File.WriteAllText(path, JsonUtility.ToJson(result, true));
            Debug.Log($"PHASE4_PERF_RESULT path={path};avgMs={result.averageFrameMs:F3};" +
                      $"p95Ms={result.p95FrameMs:F3};maxMs={result.maximumFrameMs:F3};" +
                      $"mainAvgMs={result.averageMainThreadMs:F3};" +
                      $"mainMaxMs={result.maximumMainThreadMs:F3};" +
                      $"cameraAvgMs={result.averageCameraUpdateMs:F4};" +
                      $"cameraMaxMs={result.maximumCameraUpdateMs:F4};" +
                      $"uiAvgMs={result.averageUiUpdateMs:F4};" +
                      $"uiMaxMs={result.maximumUiUpdateMs:F4};" +
                      $"animatorAvgMs={result.averageAnimatorUpdateMs:F4};" +
                      $"animatorMaxMs={result.maximumAnimatorUpdateMs:F4};" +
                      $"doorAvgMs={result.averageDoorVisualUpdateMs:F4};" +
                      $"doorMaxMs={result.maximumDoorVisualUpdateMs:F4};" +
                      $"gpuAvgMs={result.averageGpuFrameMs:F3};gpuMaxMs={result.maximumGpuFrameMs:F3};" +
                      $"transitionFrameMaxMs={result.transitionMaximumFrameMs:F3};" +
                      $"transitionMainMaxMs={result.transitionMaximumMainThreadMs:F3};" +
                      $"gcMax={result.maximumGcBytesPerFrame};" +
                      $"gcTotal={result.totalGcBytes};gcFrames={result.nonZeroGcFrames};" +
                      $"drawMax={result.maximumDrawCalls};setPassMax={result.maximumSetPassCalls};" +
                      $"trianglesMax={result.maximumTriangles};verticesMax={result.maximumVertices};" +
                      $"memoryMax={result.maximumUsedMemoryBytes};textureMemoryMax={result.maximumTextureMemoryBytes};" +
                      $"echoes={result.finalEchoCount};" +
                      $"gpu={result.device};graphical={result.graphicalDevice}", this);
            DisposeRecorders();
            coordinator?.RequestQuit();
        }

        private IEnumerator PrepareMaximumEchoes()
        {
            if (coordinator == null || coordinator.ActiveSection == null)
            {
                Debug.LogError("PHASE4_PERF_PREP_FAILED reason=MissingCoordinator", this);
                yield break;
            }

            LoopDirector director = coordinator.ActiveSection.Director;
            int target = director.MaxEchoes;
            while (director.EchoCount < target)
            {
                int expected = director.EchoCount + 1;
                director.RequestLoopEnd();
                float deadline = Time.realtimeSinceStartup + EchoPreparationTimeoutSeconds;
                while (director.EchoCount < expected && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                    CaptureTransitionFrame();
                }
                while (coordinator.State != GameplayState.Playing &&
                       Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                    CaptureTransitionFrame();
                }
                if (director.EchoCount < expected || coordinator.State != GameplayState.Playing)
                {
                    Debug.LogError($"PHASE4_PERF_PREP_FAILED expected={expected};" +
                                   $"actual={director.EchoCount};state={coordinator.State}", this);
                    yield break;
                }
            }

            Debug.Log($"PHASE4_PERF_PREP_OK echoes={director.EchoCount};target={target}", this);
        }

        private void CaptureTransitionFrame()
        {
            _transitionMaximumFrameMs = Mathf.Max(
                _transitionMaximumFrameMs, Time.unscaledDeltaTime * 1000f);
            if (_mainThread.Valid)
            {
                _transitionMaximumMainThreadMs = Mathf.Max(
                    _transitionMaximumMainThreadMs,
                    _mainThread.LastValue / 1000000f);
            }
        }

        private void OnDestroy() => DisposeRecorders();

        private void DisposeRecorders()
        {
            if (!_recording) return;
            _mainThread.Dispose(); _gpuFrameTime.Dispose(); _gcAllocated.Dispose();
            _drawCalls.Dispose(); _setPassCalls.Dispose(); _triangles.Dispose();
            _vertices.Dispose(); _usedMemory.Dispose(); _textureMemory.Dispose();
            _cameraUpdate.Dispose(); _uiUpdate.Dispose();
            if (_animatorUpdate.Valid) _animatorUpdate.Dispose();
            if (_doorVisualUpdate.Valid) _doorVisualUpdate.Dispose();
            _recording = false;
        }

        private static bool HasFlag(string value)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
                if (string.Equals(arguments[i], value, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
