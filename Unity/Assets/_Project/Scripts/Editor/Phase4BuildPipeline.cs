using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoShift.Editor
{
    public static class Phase4BuildPipeline
    {
        public const string RelativeOutput = "Builds/Phase4/ECHOSHIFT_Phase4.exe";

        [MenuItem("ECHO SHIFT/Build Phase 4 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            P3SceneBuilder.BuildScene();
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string output = Path.Combine(repository, RelativeOutput);
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? repository);
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
            if (summary.result != BuildResult.Succeeded || summary.totalErrors != 0)
                throw new InvalidOperationException(
                    $"Phase 4 build failed: {summary.result};errors={summary.totalErrors};" +
                    $"warnings={summary.totalWarnings}");
            if (summary.totalWarnings != 0)
                throw new InvalidOperationException(
                    $"Phase 4 build produced {summary.totalWarnings} BuildReport warnings.");
            Debug.Log($"PHASE4_BUILD_OK path={output};size={summary.totalSize};" +
                      $"warnings={summary.totalWarnings};errors={summary.totalErrors}");
        }
    }
}
