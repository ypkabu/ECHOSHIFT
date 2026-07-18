using EchoShift.Core;
using EchoShift.Interaction;
using UnityEngine;

namespace EchoShift.Debugging
{
    public sealed class Phase0DebugOverlay : MonoBehaviour
    {
        [SerializeField] private LoopDirector loopDirector;
        [SerializeField] private PressurePlate pressurePlate;
        [SerializeField] private GoalVolume goalVolume;

        private GUIStyle _labelStyle;
        private GUIStyle _titleStyle;

        public bool HasValidReferences =>
            loopDirector != null &&
            pressurePlate != null &&
            goalVolume != null;

        public void Configure(
            LoopDirector director,
            PressurePlate plate,
            GoalVolume goal)
        {
            loopDirector = director;
            pressurePlate = plate;
            goalVolume = goal;
        }

        private void OnGUI()
        {
            if (loopDirector == null || pressurePlate == null || goalVolume == null)
            {
                return;
            }

            EnsureStyles();

            GUILayout.BeginArea(new Rect(20f, 20f, 440f, 260f), GUI.skin.box);
            GUILayout.Label("ECHO//SHIFT - PHASE 0", _titleStyle);
            GUILayout.Label($"Loop: {loopDirector.LoopNumber}", _labelStyle);
            GUILayout.Label(
                $"Tick: {loopDirector.CurrentTick}/{loopDirector.MaxTicks}",
                _labelStyle);
            GUILayout.Label($"Echoes: {loopDirector.EchoCount}/3", _labelStyle);
            GUILayout.Label(
                $"Maximum replay drift: {loopDirector.MaximumReplayDrift:F4} m",
                _labelStyle);
            GUILayout.Label(
                $"Pressure plate: {(pressurePlate.IsPressed ? "PRESSED" : "RELEASED")}",
                _labelStyle);
            GUILayout.Label(
                $"Goal: {(goalVolume.IsReached ? "REACHED" : "NOT REACHED")}",
                _labelStyle);
            GUILayout.Label("WASD: Move    R: End loop", _labelStyle);

            if (loopDirector.AnyEchoExceededTolerance)
            {
                GUILayout.Label("DRIFT TOLERANCE EXCEEDED", _titleStyle);
            }

            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_labelStyle != null)
            {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                normal = { textColor = Color.white }
            };
            _titleStyle = new GUIStyle(_labelStyle)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
