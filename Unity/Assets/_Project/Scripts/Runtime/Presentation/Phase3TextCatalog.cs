using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Presentation
{
    [CreateAssetMenu(fileName = "Phase3TextCatalog", menuName = "ECHO SHIFT/Phase 3 Text Catalog")]
    public sealed class Phase3TextCatalog : ScriptableObject
    {
        [SerializeField] private string section1Name = "SECTION 1  •  ECHO BASICS";
        [SerializeField] private string section2Name = "SECTION 2  •  RECORDED INTERACTION";
        [SerializeField] private string section3Name = "SECTION 3  •  MULTI-ECHO COORDINATION";
        [SerializeField] private string section1Objective = "STAND ON THE SWITCH  •  END THE LOOP";
        [SerializeField] private string section2Objective = "CARRY THE CELL  •  POWER THE DOOR";
        [SerializeField] private string section3Objective = "WORK WITH YOUR PAST SELVES";
        [SerializeField] private string interactKeyboard = "E  INTERACT";
        [SerializeField] private string interactGamepad = "SOUTH  INTERACT";
        [SerializeField] private string endLoop = "R / START  END LOOP";
        [SerializeField] private string loopRecorded = "LOOP RECORDED";
        [SerializeField] private string sectionCompleted = "SECTION COMPLETE";
        [SerializeField] private string gameCompleted = "ECHO//SHIFT COMPLETE";
        [SerializeField] private string paused = "PAUSED";
        [SerializeField] private string sectionRestarted = "SECTION RESTARTED";

        public string InteractKeyboard => interactKeyboard;
        public string InteractGamepad => interactGamepad;
        public string EndLoop => endLoop;
        public string LoopRecorded => loopRecorded;
        public string SectionCompleted => sectionCompleted;
        public string GameCompleted => gameCompleted;
        public string Paused => paused;
        public string SectionRestarted => sectionRestarted;

        public string GetSectionName(int index) => index switch
        {
            0 => section1Name,
            1 => section2Name,
            _ => section3Name
        };

        public string GetSectionObjective(int index) => index switch
        {
            0 => section1Objective,
            1 => section2Objective,
            _ => section3Objective
        };

        public string GetFailureText(InteractionFailureReason reason) => reason switch
        {
            InteractionFailureReason.TargetBusy => "TARGET BUSY",
            InteractionFailureReason.TargetNotFound => "TARGET MISSING",
            InteractionFailureReason.TargetInactive => "TARGET MISSING",
            InteractionFailureReason.OutOfRange => "OUT OF RANGE",
            InteractionFailureReason.HeldByAnotherActor => "ALREADY HELD",
            InteractionFailureReason.SocketOccupied => "SOCKET OCCUPIED",
            InteractionFailureReason.TargetUnavailable => "INVALID STATE",
            InteractionFailureReason.ActorNotCarrying => "INVALID STATE",
            _ => "INTERACTION FAILED"
        };

        public bool HasNoEmptyValues()
        {
            for (int i = 0; i < 3; i++)
            {
                if (string.IsNullOrWhiteSpace(GetSectionName(i)) ||
                    string.IsNullOrWhiteSpace(GetSectionObjective(i))) return false;
            }
            return !string.IsNullOrWhiteSpace(interactKeyboard) &&
                   !string.IsNullOrWhiteSpace(interactGamepad) &&
                   !string.IsNullOrWhiteSpace(endLoop) &&
                   !string.IsNullOrWhiteSpace(loopRecorded) &&
                   !string.IsNullOrWhiteSpace(sectionCompleted) &&
                   !string.IsNullOrWhiteSpace(gameCompleted) &&
                   !string.IsNullOrWhiteSpace(paused);
        }
    }

    public sealed class TutorialProgress
    {
        private readonly byte[] _steps = new byte[3];
        public int ActiveSection { get; private set; }
        public byte CurrentStep => _steps[ActiveSection];

        public void BeginSection(int zeroBasedSection)
        {
            ActiveSection = Mathf.Clamp(zeroBasedSection, 0, 2);
        }

        public bool Advance(byte completedStep)
        {
            if (completedStep < _steps[ActiveSection]) return false;
            _steps[ActiveSection] = (byte)(completedStep + 1);
            return true;
        }

        public byte GetSectionStep(int zeroBasedSection) => _steps[zeroBasedSection];
    }
}
