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
        private const string JapaneseBuildRelativePath =
            "Builds/Phase3-JA/ECHOSHIFT_Phase3_JA.exe";

        [MenuItem("ECHO SHIFT/Build Phase 3 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            BuildWindowsDevelopment(BuildRelativePath, "PHASE3_BUILD_OK");
        }

        [MenuItem("ECHO SHIFT/Build Phase 3 Japanese Windows Development")]
        public static void BuildJapaneseWindowsDevelopment()
        {
            BuildWindowsDevelopment(JapaneseBuildRelativePath, "PHASE3_JA_BUILD_OK");
        }

        private static void BuildWindowsDevelopment(string relativePath, string successMarker)
        {
            P3SceneBuilder.BuildScene();
            string repositoryRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string output = Path.Combine(repositoryRoot, relativePath);
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
            Debug.Log($"{successMarker} path={output};size={summary.totalSize};" +
                      $"warnings={summary.totalWarnings}");
        }
    }
}
