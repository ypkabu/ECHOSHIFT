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
        [SerializeField] private JapaneseFontApplier fontApplier;
        [SerializeField] private Text sectionText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text tutorialText;
        [SerializeField] private Text loopText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text echoText;
        [SerializeField] private Text promptText;
        [SerializeField] private Text endLoopText;
        [SerializeField] private Text pausePromptText;
        [SerializeField] private Text carryText;
        [SerializeField] private Text stateText;
        [SerializeField] private Text failureText;

        private SectionTransitionCoordinator _coordinator;
        private float _failureUntil;

        public Phase3TextCatalog TextCatalog => textCatalog;
        public bool IsJapaneseReady => textCatalog != null &&
            textCatalog.LanguageCode == Phase3TextCatalog.JapaneseLanguageCode &&
            fontApplier != null && fontApplier.IsReady;
        public string JapaneseFontName => fontApplier != null
            ? fontApplier.ResolvedFontName : string.Empty;
        public string CurrentPrompt { get; private set; } = string.Empty;
        public string CurrentObjective { get; private set; } = string.Empty;
        public string CurrentTutorial { get; private set; } = string.Empty;
        public string CurrentStateMessage { get; private set; } = string.Empty;
        public string LastFailureText { get; private set; } = string.Empty;
        public bool HasValidReferences => textCatalog != null && fontApplier != null && sectionText != null &&
            objectiveText != null && tutorialText != null &&
            loopText != null && timerText != null && echoText != null &&
            promptText != null && endLoopText != null && pausePromptText != null && carryText != null &&
            stateText != null && failureText != null;

        public void Configure(
            Phase3TextCatalog catalog, Text section, Text objective, Text tutorial,
            Text loop, Text timer, Text echoes, Text prompt, Text endLoopLabel,
            Text pauseLabel, Text carry,
            Text state, Text failure)
        {
            textCatalog = catalog;
            sectionText = section;
            objectiveText = objective;
            tutorialText = tutorial;
            loopText = loop;
            timerText = timer;
            echoText = echoes;
            promptText = prompt;
            endLoopText = endLoopLabel;
            pausePromptText = pauseLabel;
            carryText = carry;
            stateText = state;
            failureText = failure;
        }

        public void Bind(SectionTransitionCoordinator coordinator)
        {
            _coordinator = coordinator;
            RefreshNow();
        }

        public void SetFontApplier(JapaneseFontApplier applier)
        {
            fontApplier = applier;
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
            SetObjective(textCatalog.GetSectionObjective(_coordinator.ActiveSectionNumber - 1));
            loopText.text = textCatalog.FormatLoop(director.LoopNumber);
            float remaining = Mathf.Max(0f,
                (director.MaxTicks - director.CurrentTick) / (float)Mathf.Max(1, director.TickRate));
            timerText.text = textCatalog.FormatTime(remaining);
            echoText.text = textCatalog.FormatEchoCount(director.EchoCount, director.MaxEchoes);
            InputSystemInputSource source = section.Player.GetComponent<InputSystemInputSource>();
            bool hasCandidate = section.Player.Interactor?.Sensor?.CurrentTarget != null;
            CurrentPrompt = ResolvePrompt(
                source != null ? source.LastPromptDevice : InputPromptDevice.Keyboard,
                hasCandidate);
            promptText.text = CurrentPrompt;
            endLoopText.text = textCatalog.EndLoop;
            pausePromptText.text = textCatalog.PausePrompt;
            carryText.text = section.Player.Interactor?.CarriedBattery != null
                ? textCatalog.BatteryCarried : string.Empty;
        }

        public void SetStateMessage(string message)
        {
            CurrentStateMessage = message ?? string.Empty;
            if (stateText != null) stateText.text = CurrentStateMessage;
        }

        public void SetObjective(string message)
        {
            CurrentObjective = message ?? string.Empty;
            if (objectiveText != null) objectiveText.text = CurrentObjective;
        }

        public void SetTutorialMessage(string message)
        {
            CurrentTutorial = message ?? string.Empty;
            if (tutorialText != null) tutorialText.text = CurrentTutorial;
        }

        public void ShowInteractionFailure(InteractionFailureReason reason)
        {
            LastFailureText = textCatalog.GetFailureText(reason);
            failureText.text = LastFailureText;
            _failureUntil = Time.unscaledTime + 2f;
        }

        public string ResolvePrompt(InputPromptDevice device, bool hasCandidate)
        {
            if (device == InputPromptDevice.Gamepad)
            {
                return hasCandidate ? textCatalog.InteractGamepad : textCatalog.MoveGamepad;
            }
            return hasCandidate ? textCatalog.InteractKeyboard : textCatalog.MoveKeyboard;
        }
    }
}
