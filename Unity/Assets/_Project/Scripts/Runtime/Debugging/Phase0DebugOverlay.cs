using EchoShift.Core;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Debugging
{
    public sealed class Phase0DebugOverlay : MonoBehaviour
    {
        [SerializeField] private LoopDirector loopDirector;
        [SerializeField] private PressurePlate pressurePlate;
        [SerializeField] private GoalVolume goalVolume;
        [SerializeField] private PlayerSimulation playerSimulation;
        [SerializeField] private CarryableBattery battery;
        [SerializeField] private PowerSocket powerSocket;
        [SerializeField] private bool isPhase2;

        private GUIStyle _labelStyle;
        private GUIStyle _titleStyle;

        public bool HasValidReferences =>
            loopDirector != null &&
            goalVolume != null &&
            (pressurePlate != null || IsPhase1Configured);

        public bool IsPhase1Configured =>
            playerSimulation != null &&
            battery != null &&
            powerSocket != null;

        public void Configure(
            LoopDirector director,
            PressurePlate plate,
            GoalVolume goal)
        {
            loopDirector = director;
            pressurePlate = plate;
            goalVolume = goal;
            playerSimulation = null;
            battery = null;
            powerSocket = null;
            isPhase2 = false;
        }

        public void ConfigurePhase1(
            LoopDirector director,
            PlayerSimulation player,
            CarryableBattery carryableBattery,
            PowerSocket socket,
            GoalVolume goal)
        {
            loopDirector = director;
            pressurePlate = null;
            goalVolume = goal;
            playerSimulation = player;
            battery = carryableBattery;
            powerSocket = socket;
            isPhase2 = false;
        }

        public void ConfigurePhase2(
            LoopDirector director,
            PlayerSimulation player,
            PressurePlate plate,
            CarryableBattery carryableBattery,
            PowerSocket socket,
            GoalVolume goal)
        {
            loopDirector = director;
            playerSimulation = player;
            pressurePlate = plate;
            battery = carryableBattery;
            powerSocket = socket;
            goalVolume = goal;
            isPhase2 = true;
        }

        private void OnGUI()
        {
            if (!HasValidReferences)
            {
                return;
            }

            EnsureStyles();

            float height = isPhase2 ? 650f : IsPhase1Configured ? 520f : 260f;
            GUILayout.BeginArea(new Rect(20f, 20f, 520f, height), GUI.skin.box);
            GUILayout.Label(
                isPhase2 ? "ECHO//SHIFT - PHASE 2" :
                IsPhase1Configured ? "ECHO//SHIFT - PHASE 1" : "ECHO//SHIFT - PHASE 0",
                _titleStyle);
            GUILayout.Label($"Loop: {loopDirector.LoopNumber}", _labelStyle);
            GUILayout.Label(
                $"Tick: {loopDirector.CurrentTick}/{loopDirector.MaxTicks}",
                _labelStyle);
            GUILayout.Label($"Echoes: {loopDirector.EchoCount}/3", _labelStyle);
            GUILayout.Label(
                $"Maximum replay drift: {loopDirector.MaximumReplayDrift:F4} m",
                _labelStyle);
            if (pressurePlate != null)
            {
                GUILayout.Label(
                    $"Pressure plate: {(pressurePlate.IsPressed ? "PRESSED" : "RELEASED")}",
                    _labelStyle);
            }
            GUILayout.Label(
                $"Goal: {(goalVolume.IsReached ? "REACHED" : "NOT REACHED")}",
                _labelStyle);
            if (IsPhase1Configured)
            {
                DrawPhase1State();
                GUILayout.Label("WASD / Left Stick: Move", _labelStyle);
                GUILayout.Label("E / South Button: Interact    R / Start: End loop", _labelStyle);
            }
            else
            {
                GUILayout.Label("WASD / Left Stick: Move    R / Start: End loop", _labelStyle);
            }

            if (loopDirector.AnyEchoExceededTolerance)
            {
                GUILayout.Label("DRIFT TOLERANCE EXCEEDED", _titleStyle);
            }

            GUILayout.EndArea();
        }

        private void DrawPhase1State()
        {
            Interactor playerInteractor = playerSimulation.Interactor;
            InteractionSensor sensor = playerInteractor != null
                ? playerInteractor.Sensor
                : null;
            GUILayout.Label(
                $"Candidate: {(sensor != null ? sensor.CurrentTargetName : string.Empty)}",
                _labelStyle);
            GUILayout.Label(
                $"Candidate ID: {(sensor != null ? sensor.CurrentStableId : string.Empty)}",
                _labelStyle);
            GUILayout.Label(
                $"Player carrying: {GetBatteryName(playerInteractor?.CarriedBattery)}",
                _labelStyle);
            GUILayout.Label(
                $"Battery holder: {(battery.Holder != null ? battery.Holder.Actor.name : "none")}",
                _labelStyle);
            GUILayout.Label(
                $"Power socket: {(powerSocket.IsPowered ? "POWERED" : "UNPOWERED")}",
                _labelStyle);
            GUILayout.Label(
                $"Recorded interactions: {loopDirector.CurrentInteractionCount}",
                _labelStyle);
            LoopActor playerActor = playerSimulation.GetComponent<LoopActor>();
            GUILayout.Label(
                $"Player gen={playerActor?.ReplayGeneration ?? 0}, " +
                $"success={playerSimulation.InteractionSuccessCount}, " +
                $"failed={playerSimulation.InteractionFailureCount}, " +
                $"held={GetBatteryName(playerInteractor?.CarriedBattery)}, " +
                $"pos={playerSimulation.transform.position:F2}",
                _labelStyle);

            for (int i = 0; i < loopDirector.EchoCount; i++)
            {
                EchoShift.Replay.EchoPlayback echo = loopDirector.GetEchoPlayback(i);
                GUILayout.Label(
                    $"Echo gen={echo.ReplayGeneration}: tick={echo.PlaybackTick}/{echo.RecordingLength}, " +
                    $"success={echo.InteractionSuccessCount}, failed={echo.InteractionFailureCount}, " +
                    $"held={GetBatteryName(echo.Interactor?.CarriedBattery)}, " +
                    $"pos={echo.transform.position:F2}, drift={echo.MaximumDrift:F4}, " +
                    $"last={echo.LastInteractionFailure}",
                    _labelStyle);
            }

            if (isPhase2)
            {
                GUILayout.Label($"Loop history: {loopDirector.History.Count}/{loopDirector.History.Capacity}",
                    _labelStyle);
            }
        }

        private static string GetBatteryName(CarryableBattery carriedBattery)
        {
            return carriedBattery != null ? carriedBattery.name : "none";
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
