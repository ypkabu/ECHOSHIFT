using UnityEngine;
using UnityEngine.EventSystems;

namespace EchoShift.Presentation
{
    public sealed class Phase4UiAudio : MonoBehaviour, ISelectHandler, ISubmitHandler, ICancelHandler
    {
        [SerializeField] private Phase4AudioController audioController;

        public void Configure(Phase4AudioController audio) => audioController = audio;
        public void OnSelect(BaseEventData eventData) =>
            audioController?.Play(Phase4AudioCue.UiSelect, 0.2f);
        public void OnSubmit(BaseEventData eventData) =>
            audioController?.Play(Phase4AudioCue.UiConfirm, 0.3f);
        public void OnCancel(BaseEventData eventData) =>
            audioController?.Play(Phase4AudioCue.UiBack, 0.25f);
    }
}
