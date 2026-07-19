using EchoShift.Gameplay;
using EchoShift.Input;
using EchoShift.Interaction.Recorded;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class GameplayHud : MonoBehaviour
    {
        [SerializeField] private Phase3TextCatalog textCatalog;
        [SerializeField] private Text sectionText;
        [SerializeField] private Text loopText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text echoText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text endLoopText;
        [SerializeField] private Text carryText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text failureText;

        private SectionTransitionCoordinator _coordinator;
        private float _failureUntil;

        public Phase3TextCatalog TextCatalog => textCatalog;
        public string CurrentPrompt { get; private set; } = string.Empty;
        public string LastFailureText { get; private set; } = string.Empty;
        public bool HasValidReferences => textCatalog != null && sectionText != null &&
            loopText != null && timerText != null && echoText != null &&
            promptText != null && endLoopText != null && carryText != null &&
            stateText != null && failureText != null;

        public void Configure(
            Phase3TextCatalog catalog, Text section, Text loop, Text timer,
            Text echoes, Text prompt, Text endLoopLabel, Text carry,
            Text state, Text failure)
        {
            textCatalog = catalog;
            sectionText = section;
            loopText = loop;
            timerText = timer;
            echoText = echoes;
            promptText = prompt;
            endLoopText = endLoopLabel;
            carryText = carry;
            stateText = state;
            failureText = failure;
        }

        public void Bind(SectionTransitionCoordinator coordinator)
        {
            _coordinator = coordinator;
            RefreshNow();
        }

        private void Update()
        {
            RefreshNow();
            if (_failureUntil > 0f && Time.unscaledTime >= _failureUntil)
            {
                failureText.text = string.Empty;
                _failureUntil = 0f;
            }
        }

        public void RefreshNow()
        {
            if (_coordinator == null || _coordinator.ActiveSection == null ||
                !HasValidReferences) return;
            PuzzleSectionController section = _coordinator.ActiveSection;
            Core.LoopDirector director = section.Director;
            sectionText.text = textCatalog.GetSectionName(_coordinator.ActiveSectionNumber - 1);
            loopText.text = $"LOOP  {director.LoopNumber}";
            float remaining = Mathf.Max(0f,
                (director.MaxTicks - director.CurrentTick) / (float)Mathf.Max(1, director.TickRate));
            timerText.text = $"TIME  {remaining:00.0}";
            echoText.text = $"ECHOES  {director.EchoCount}/{director.MaxEchoes}";
            InputSystemInputSource source = section.Player.GetComponent<InputSystemInputSource>();
            bool hasCandidate = section.Player.Interactor?.Sensor?.CurrentTarget != null;
            CurrentPrompt = ResolvePrompt(
                source != null ? source.LastPromptDevice : InputPromptDevice.Keyboard,
                hasCandidate);
            promptText.text = CurrentPrompt;
            endLoopText.text = textCatalog.EndLoop;
            carryText.text = section.Player.Interactor?.CarriedBattery != null
                ? "CELL  CARRIED" : string.Empty;
        }

        public void SetStateMessage(string message)
        {
            if (stateText != null) stateText.text = message ?? string.Empty;
        }

        public void ShowInteractionFailure(InteractionFailureReason reason)
        {
            LastFailureText = textCatalog.GetFailureText(reason);
            failureText.text = LastFailureText;
            _failureUntil = Time.unscaledTime + 2f;
        }

        public string ResolvePrompt(InputPromptDevice device, bool hasCandidate)
        {
            if (!hasCandidate) return string.Empty;
            return device == InputPromptDevice.Gamepad
                ? textCatalog.InteractGamepad
                : textCatalog.InteractKeyboard;
        }
    }
}
