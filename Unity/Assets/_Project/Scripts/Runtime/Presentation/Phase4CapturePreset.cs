using System;
using UnityEngine;

namespace EchoShift.Presentation
{
    public enum Phase4CaptureMoment : byte
    {
        Section1Overview,
        PlayerAndEchoOne,
        EchoOnPlatePlayerAtDoor,
        BatteryHeldClose,
        RecordedInsertionDoorOpen,
        TwoEchoRoles,
        GoalArrival,
        GameplayHud
    }

    [Serializable]
    public struct Phase4CaptureShotPreset
    {
        [SerializeField] private string fileName;
        [SerializeField] private Phase4CaptureMoment moment;
        [SerializeField, Range(1, 3)] private int sectionNumber;
        [SerializeField] private Vector3 localFocus;
        [SerializeField] private Vector3 cameraOffset;
        [SerializeField, Range(30f, 70f)] private float fieldOfView;
        [SerializeField] private bool showGameplayHud;

        public Phase4CaptureShotPreset(
            string outputFileName, Phase4CaptureMoment captureMoment,
            int section, Vector3 focus, Vector3 offset, float fov, bool showHud)
        {
            fileName = outputFileName;
            moment = captureMoment;
            sectionNumber = section;
            localFocus = focus;
            cameraOffset = offset;
            fieldOfView = fov;
            showGameplayHud = showHud;
        }

        public string FileName => fileName;
        public Phase4CaptureMoment Moment => moment;
        public int SectionNumber => sectionNumber;
        public Vector3 LocalFocus => localFocus;
        public Vector3 CameraOffset => cameraOffset;
        public float FieldOfView => fieldOfView;
        public bool ShowGameplayHud => showGameplayHud;
    }

    [CreateAssetMenu(fileName = "Phase4CapturePreset",
        menuName = "ECHO SHIFT/Phase 4 Capture Preset")]
    public sealed class Phase4CapturePreset : ScriptableObject
    {
        public const int RequiredShotCount = 8;

        [SerializeField] private Phase4CaptureShotPreset[] shots =
            Array.Empty<Phase4CaptureShotPreset>();

        public int Count => shots?.Length ?? 0;
        public Phase4CaptureShotPreset GetShot(int index) => shots[index];

        public void Configure(Phase4CaptureShotPreset[] captureShots)
        {
            shots = captureShots ?? Array.Empty<Phase4CaptureShotPreset>();
        }

        public bool IsValid()
        {
            if (shots == null || shots.Length != RequiredShotCount) return false;
            for (int i = 0; i < shots.Length; i++)
            {
                Phase4CaptureShotPreset shot = shots[i];
                if (string.IsNullOrWhiteSpace(shot.FileName) ||
                    !shot.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    shot.SectionNumber < 1 || shot.SectionNumber > 3 ||
                    shot.CameraOffset.sqrMagnitude < 1f ||
                    shot.FieldOfView < 30f || shot.FieldOfView > 70f)
                    return false;
                for (int j = i + 1; j < shots.Length; j++)
                {
                    if (string.Equals(shot.FileName, shots[j].FileName,
                            StringComparison.OrdinalIgnoreCase)) return false;
                    if (Vector3.SqrMagnitude(shot.CameraOffset - shots[j].CameraOffset) < 0.01f &&
                        Vector3.SqrMagnitude(shot.LocalFocus - shots[j].LocalFocus) < 0.01f &&
                        Mathf.Abs(shot.FieldOfView - shots[j].FieldOfView) < 0.01f)
                        return false;
                }
            }
            return true;
        }
    }
}
