using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Presentation
{
    [RequireComponent(typeof(Collider))]
    public sealed class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private TutorialGuide guide;
        [SerializeField] private byte step;
        [SerializeField] private string message = "MOVE";
        private bool _consumed;

        public void Configure(TutorialGuide tutorialGuide, byte tutorialStep, string text)
        {
            guide = tutorialGuide;
            step = tutorialStep;
            message = text;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_consumed || other.GetComponentInParent<LoopActor>()?.Kind != LoopActorKind.Player)
            {
                return;
            }
            _consumed = true;
            guide.ShowStep(step, message);
        }
    }
}
