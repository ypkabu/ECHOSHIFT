using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoShift.Editor
{
    public static class Phase2BuildPipeline
    {
        private const string BuildRelativePath = "Builds/Phase2/ECHOSHIFT_Phase2.exe";

        [MenuItem("ECHO SHIFT/Build Phase 2 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            P2SceneBuilder.BuildScene();
            string repositoryRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", ".."));
            string outputPath = Path.Combine(repositoryRoot, BuildRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? repositoryRoot);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    P2SceneBuilder.ScenePath,
                    "Assets/_Project/Scenes/P1_InteractionLab.unity",
                    "Assets/_Project/Scenes/P0_ReplayLab.unity"
                },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Phase 2 build failed: {summary.result}, errors={summary.totalErrors}, " +
                    $"warnings={summary.totalWarnings}.");
            }

            Debug.Log($"PHASE2_BUILD_OK path={outputPath};size={summary.totalSize};" +
                      $"warnings={summary.totalWarnings}");
        }
    }
}
