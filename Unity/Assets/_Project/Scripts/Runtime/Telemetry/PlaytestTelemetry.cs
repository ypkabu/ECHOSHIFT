using System;
using System.IO;
using EchoShift.Core;
using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Telemetry
{
    public enum PlaytestOutcome : byte
    {
        InProgress,
        Completed,
        Quit,
        ForcedTermination
    }

    public readonly struct PlaytestTelemetrySnapshot
    {
        private readonly int[] _sectionLoops;
        private readonly float[] _sectionSeconds;
        private readonly int[] _failureCounts;

        internal PlaytestTelemetrySnapshot(
            string buildVersion, string unityVersion, string startedUtc,
            float totalSeconds, int finalSection, PlaytestOutcome outcome,
            int manualEnds, int timerEnds, int restartSection, int restartGame,
            int successCount, int failureCount, float maximumDrift,
            int[] sectionLoops, float[] sectionSeconds, int[] failureCounts)
        {
            BuildVersion = buildVersion;
            UnityVersion = unityVersion;
            StartedUtc = startedUtc;
            TotalSeconds = totalSeconds;
            FinalSection = finalSection;
            Outcome = outcome;
            ManualLoopEnds = manualEnds;
            TimerLoopEnds = timerEnds;
            RestartSectionCount = restartSection;
            RestartGameCount = restartGame;
            InteractionSuccessCount = successCount;
            InteractionFailureCount = failureCount;
            MaximumDrift = maximumDrift;
            _sectionLoops = (int[])sectionLoops.Clone();
            _sectionSeconds = (float[])sectionSeconds.Clone();
            _failureCounts = (int[])failureCounts.Clone();
        }

        public string BuildVersion { get; }
        public string UnityVersion { get; }
        public string StartedUtc { get; }
        public float TotalSeconds { get; }
        public int FinalSection { get; }
        public PlaytestOutcome Outcome { get; }
        public int ManualLoopEnds { get; }
        public int TimerLoopEnds { get; }
        public int RestartSectionCount { get; }
        public int RestartGameCount { get; }
        public int InteractionSuccessCount { get; }
        public int InteractionFailureCount { get; }
        public float MaximumDrift { get; }
        public int GetSectionLoops(int zeroBasedIndex) => _sectionLoops[zeroBasedIndex];
        public float GetSectionSeconds(int zeroBasedIndex) => _sectionSeconds[zeroBasedIndex];
        public int GetFailureCount(InteractionFailureReason reason) =>
            _failureCounts[(int)reason];
    }

    public sealed class PlaytestTelemetry : MonoBehaviour
    {
        private const int SectionCount = 3;
        private readonly int[] _sectionLoops = new int[SectionCount];
        private readonly float[] _sectionSeconds = new float[SectionCount];
        private readonly float[] _sectionStartTimes = new float[SectionCount];
        private readonly int[] _failureCounts =
            new int[Enum.GetValues(typeof(InteractionFailureReason)).Length];

        private string _startedUtc;
        private float _sessionStart;
        private int _finalSection;
        private PlaytestOutcome _outcome;
        private int _manualEnds;
        private int _timerEnds;
        private int _restartSection;
        private int _restartGame;
        private int _successCount;
        private int _failureCount;
        private float _maximumDrift;
        private bool _writeFailureLogged;

        public string SaveDirectory => Path.Combine(
            Application.persistentDataPath, "EchoShiftPlaytests");
        public string LastSavedPath { get; private set; } = string.Empty;
        public PlaytestOutcome Outcome => _outcome;

        private void Awake()
        {
            BeginSession();
        }

        public void BeginSession()
        {
            Array.Clear(_sectionLoops, 0, _sectionLoops.Length);
            Array.Clear(_sectionSeconds, 0, _sectionSeconds.Length);
            Array.Clear(_sectionStartTimes, 0, _sectionStartTimes.Length);
            Array.Clear(_failureCounts, 0, _failureCounts.Length);
            _startedUtc = DateTime.UtcNow.ToString("O");
            _sessionStart = Time.realtimeSinceStartup;
            _finalSection = 0;
            _outcome = PlaytestOutcome.InProgress;
            _manualEnds = 0;
            _timerEnds = 0;
            _restartSection = 0;
            _restartGame = 0;
            _successCount = 0;
            _failureCount = 0;
            _maximumDrift = 0f;
        }

        public void SectionStarted(int oneBasedSection)
        {
            int index = Mathf.Clamp(oneBasedSection - 1, 0, SectionCount - 1);
            _finalSection = Mathf.Max(_finalSection, oneBasedSection);
            _sectionStartTimes[index] = Time.realtimeSinceStartup;
        }

        public void SectionCompleted(int oneBasedSection, int loopsUsed)
        {
            int index = Mathf.Clamp(oneBasedSection - 1, 0, SectionCount - 1);
            _sectionLoops[index] = Mathf.Max(1, loopsUsed);
            _sectionSeconds[index] = Mathf.Max(
                0f, Time.realtimeSinceStartup - _sectionStartTimes[index]);
        }

        public void RecordLoop(in LoopHistorySummary summary)
        {
            if (summary.EndReason == LoopEndReason.Manual) _manualEnds++;
            if (summary.EndReason == LoopEndReason.Timer) _timerEnds++;
            _successCount += summary.InteractionSuccessCount;
            // Failures arrive through InteractionResolved so their reason can be retained.
            // Adding the loop summary here would count the same Player failure twice.
            _maximumDrift = Mathf.Max(_maximumDrift, summary.MaximumDrift);
        }

        public void RecordInteraction(in InteractionExecution execution)
        {
            if (execution.Succeeded)
            {
                return;
            }

            _failureCount++;
            _failureCounts[(int)execution.FailureReason]++;
        }

        public void RecordRestartSection() => _restartSection++;
        public void RecordRestartGame() => _restartGame++;
        public void SetMaximumDrift(float drift) =>
            _maximumDrift = Mathf.Max(_maximumDrift, drift);

        public PlaytestTelemetrySnapshot Snapshot()
        {
            return new PlaytestTelemetrySnapshot(
                Application.version, Application.unityVersion, _startedUtc,
                Mathf.Max(0f, Time.realtimeSinceStartup - _sessionStart),
                _finalSection, _outcome, _manualEnds, _timerEnds,
                _restartSection, _restartGame, _successCount, _failureCount,
                _maximumDrift, _sectionLoops, _sectionSeconds, _failureCounts);
        }

        public string GenerateJson()
        {
            TelemetryJson dto = new TelemetryJson
            {
                schemaVersion = 1,
                buildVersion = Application.version,
                unityVersion = Application.unityVersion,
                startedUtc = _startedUtc,
                totalSeconds = Mathf.Max(0f, Time.realtimeSinceStartup - _sessionStart),
                sectionLoops = (int[])_sectionLoops.Clone(),
                sectionSeconds = (float[])_sectionSeconds.Clone(),
                manualLoopEnds = _manualEnds,
                timerLoopEnds = _timerEnds,
                restartSectionCount = _restartSection,
                restartGameCount = _restartGame,
                interactionSuccessCount = _successCount,
                interactionFailureCount = _failureCount,
                interactionFailureByReason = (int[])_failureCounts.Clone(),
                maximumReplayDrift = _maximumDrift,
                finalSection = _finalSection,
                outcome = _outcome.ToString()
            };
            return JsonUtility.ToJson(dto, true);
        }

        public bool FinishAndSave(PlaytestOutcome outcome)
        {
            _outcome = outcome;
            try
            {
                Directory.CreateDirectory(SaveDirectory);
                string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
                LastSavedPath = Path.Combine(SaveDirectory, $"session-{stamp}.json");
                File.WriteAllText(LastSavedPath, GenerateJson());
                return true;
            }
            catch (Exception exception)
            {
                if (!_writeFailureLogged)
                {
                    _writeFailureLogged = true;
                    Debug.LogError($"Playtest telemetry write failed once: {exception.Message}", this);
                }
                return false;
            }
        }

        [Serializable]
        private sealed class TelemetryJson
        {
            public int schemaVersion;
            public string buildVersion;
            public string unityVersion;
            public string startedUtc;
            public float totalSeconds;
            public int[] sectionLoops;
            public float[] sectionSeconds;
            public int manualLoopEnds;
            public int timerLoopEnds;
            public int restartSectionCount;
            public int restartGameCount;
            public int interactionSuccessCount;
            public int interactionFailureCount;
            public int[] interactionFailureByReason;
            public float maximumReplayDrift;
            public int finalSection;
            public string outcome;
        }
    }
}
