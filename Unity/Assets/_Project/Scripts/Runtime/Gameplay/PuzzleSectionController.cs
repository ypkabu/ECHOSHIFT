using EchoShift.Core;
using EchoShift.Interaction;
using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Gameplay
{
    public sealed class PuzzleSectionController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int sectionNumber = 1;
        [SerializeField] private string sectionName = "ECHO BASICS";
        [SerializeField] private LoopDirector loopDirector;
        [SerializeField] private PlayerSimulation player;
        [SerializeField] private GoalVolume goal;
        [SerializeField] private Transform playerSpawn;

        public int SectionNumber => sectionNumber;
        public string SectionName => sectionName;
        public LoopDirector Director => loopDirector;
        public PlayerSimulation Player => player;
        public GoalVolume Goal => goal;
        public Transform PlayerSpawn => playerSpawn;
        public bool IsSectionActive => gameObject.activeSelf;
        public bool HasValidReferences => loopDirector != null && player != null &&
                                          goal != null && playerSpawn != null;

        public void Configure(
            int number, string displayName, LoopDirector director,
            PlayerSimulation playerSimulation, GoalVolume goalVolume,
            Transform spawn)
        {
            sectionNumber = number;
            sectionName = displayName;
            loopDirector = director;
            player = playerSimulation;
            goal = goalVolume;
            playerSpawn = spawn;
        }

        public void ActivateSection(bool paused)
        {
            gameObject.SetActive(true);
            if (!loopDirector.EnsureInitialized())
            {
                throw new System.InvalidOperationException(
                    $"Section {sectionNumber} LoopDirector failed initialization.");
            }
            loopDirector.SetSimulationPaused(paused);
        }

        public void RestartSection(bool paused)
        {
            loopDirector.SetSimulationPaused(true);
            loopDirector.RestartSectionLifecycle();
            goal.RestoreInitialState();
            player.Motor.ResetPose(playerSpawn.position, playerSpawn.rotation);
            Physics.SyncTransforms();
            loopDirector.SetSimulationPaused(paused);
        }

        public void DeactivateSection()
        {
            loopDirector.ShutdownSectionLifecycle();
            gameObject.SetActive(false);
        }

        public void RefreshCompletionSensor()
        {
            if (gameObject.activeInHierarchy)
            {
                goal.RefreshFromPhysics();
            }
        }
    }
}
