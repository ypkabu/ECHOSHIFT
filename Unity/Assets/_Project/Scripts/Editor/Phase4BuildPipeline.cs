using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoShift.Editor
{
    public static class Phase4BuildPipeline
    {
        public const string RelativeOutput = "Builds/Phase4_1/ECHOSHIFT_Phase4_1.exe";
        public const string NonDevelopmentRelativeOutput =
            "Builds/Phase4_1_NonDevelopment/ECHOSHIFT_Phase4_1_NonDevelopment.exe";
        public const string PresentationRelativeOutput =
            "Builds/Phase4_2/ECHOSHIFT_Phase4_2.exe";

        [MenuItem("ECHO SHIFT/Build Phase 4 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            BuildWindows(RelativeOutput, BuildOptions.Development, "PHASE4_BUILD_OK");
        }

        [MenuItem("ECHO SHIFT/Build Phase 4 Windows Non-Development")]
        public static void BuildWindowsNonDevelopment()
        {
            BuildWindows(NonDevelopmentRelativeOutput, BuildOptions.None,
                "PHASE4_NONDEVELOPMENT_BUILD_OK");
        }

        [MenuItem("ECHO SHIFT/Build Phase 4.2 Windows Development")]
        public static void BuildPhase4TwoWindowsDevelopment()
        {
            BuildWindows(PresentationRelativeOutput, BuildOptions.Development,
                "PHASE4_2_BUILD_OK");
        }

        private static void BuildWindows(string relativeOutput, BuildOptions buildOptions,
            string successMarker)
        {
            P3SceneBuilder.BuildScene();
            string repository = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
            string output = Path.Combine(repository, relativeOutput);
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
                options = buildOptions
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
            Debug.Log($"{successMarker} path={output};size={summary.totalSize};" +
                      $"warnings={summary.totalWarnings};errors={summary.totalErrors}");
        }
    }
}
