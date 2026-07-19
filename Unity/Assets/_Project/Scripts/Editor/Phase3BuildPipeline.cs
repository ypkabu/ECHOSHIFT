using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoShift.Editor
{
    public static class Phase3BuildPipeline
    {
        private const string BuildRelativePath = "Builds/Phase3/ECHOSHIFT_Phase3.exe";

        [MenuItem("ECHO SHIFT/Build Phase 3 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            P3SceneBuilder.BuildScene();
            string repositoryRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string output = Path.Combine(repositoryRoot, BuildRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? repositoryRoot);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    P3SceneBuilder.ScenePath,
                    "Assets/_Project/Scenes/P2_CoordinationLab.unity",
                    "Assets/_Project/Scenes/P1_InteractionLab.unity",
                    "Assets/_Project/Scenes/P0_ReplayLab.unity"
                },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(
                    $"Phase 3 build failed: {summary.result};errors={summary.totalErrors};" +
                    $"warnings={summary.totalWarnings}");
            Debug.Log($"PHASE3_BUILD_OK path={output};size={summary.totalSize};" +
                      $"warnings={summary.totalWarnings}");
        }
    }
}
