using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoShift.Editor
{
    public static class Phase0BuildPipeline
    {
        private const string BuildRelativePath = "Builds/Phase0/ECHOSHIFT_Phase0.exe";

        [MenuItem("ECHO SHIFT/Build Phase 0 Windows Development")]
        public static void BuildWindowsDevelopment()
        {
            P0SceneBuilder.BuildScene();

            string repositoryRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", ".."));
            string outputPath = Path.Combine(repositoryRoot, BuildRelativePath);
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new InvalidOperationException("Could not resolve the Phase 0 build directory.");
            }

            Directory.CreateDirectory(outputDirectory);
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { P0SceneBuilder.ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Phase 0 Windows build failed: {summary.result}, " +
                    $"errors={summary.totalErrors}, warnings={summary.totalWarnings}.");
            }

            Debug.Log(
                $"Phase 0 Windows Development Build succeeded at {outputPath}. " +
                $"Size={summary.totalSize} bytes, warnings={summary.totalWarnings}.");
        }
    }
}
