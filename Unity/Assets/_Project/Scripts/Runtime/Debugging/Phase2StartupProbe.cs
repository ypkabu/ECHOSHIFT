using EchoShift.Core;
using EchoShift.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoShift.Debugging
{
    public sealed class Phase2StartupProbe : MonoBehaviour
    {
        [SerializeField] private LoopDirector director;
        [SerializeField] private PlayerSimulation player;

        public void Configure(LoopDirector loopDirector, PlayerSimulation playerSimulation)
        {
            director = loopDirector;
            player = playerSimulation;
        }

        private void Start()
        {
            Debug.Log(
                $"PHASE2_STARTUP_OK scene={SceneManager.GetActiveScene().name};" +
                $"tickRate={director.TickRate};duration={director.LoopDurationSeconds};" +
                $"maxTicks={director.MaxTicks};maxEchoes={director.MaxEchoes};" +
                $"driftTolerance={director.DriftTolerance:R};" +
                $"player={(player != null ? player.name : "missing")};" +
                $"echoCount={director.EchoCount}",
                this);
        }
    }
}
