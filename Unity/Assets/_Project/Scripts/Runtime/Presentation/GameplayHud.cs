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
        [SerializeField, Min(1f)] private float sectionIntroDuration = 4.5f;
        [SerializeField, Min(1f)] private float tutorialNoticeDuration = 6f;

        private SectionTransitionCoordinator _coordinator;
        private float _failureUntil;
        private InputSystemInputSource _inputSource;
        private Interactor _interactor;
        private InteractionSensor _sensor;
        private string[] _timeTextCache = System.Array.Empty<string>();
        private int _lastSectionNumber = -1;
        private int _lastLoopNumber = -1;
        private int _lastEchoCount = -1;
        private int _lastMaximumEchoes = -1;
        private int _lastRemainingTenths = -1;
        private int _cachedMaximumTenths = -1;
        private bool _lastCarrying;
        private float _sectionIntroUntil;
        private float _tutorialUntil;

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
        public float SectionIntroDuration => sectionIntroDuration;
        public bool IsSectionIntroVisible { get; private set; }
        public bool IsTutorialVisible { get; private set; }
        public bool IsCarrying { get; private set; }
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
            _lastSectionNumber = -1;
            _lastLoopNumber = -1;
            _lastEchoCount = -1;
            _lastMaximumEchoes = -1;
            _lastRemainingTenths = -1;
            _cachedMaximumTenths = -1;
            RefreshNow();
        }

        public void SetFontApplier(JapaneseFontApplier applier)
        {
            fontApplier = applier;
        }

        private void Update()
        {
            RefreshNow();
            RefreshTransientPresentation(Time.unscaledTime);
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
            int sectionNumber = _coordinator.ActiveSectionNumber;
            if (sectionNumber != _lastSectionNumber)
            {
                _lastSectionNumber = sectionNumber;
                sectionText.text = textCatalog.GetSectionName(sectionNumber - 1);
                SetObjective(textCatalog.GetSectionObjective(sectionNumber - 1));
                _inputSource = section.Player.GetComponent<InputSystemInputSource>();
                _interactor = section.Player.Interactor;
                _sensor = _interactor?.Sensor;
                _sectionIntroUntil = Time.unscaledTime + sectionIntroDuration;
                _lastCarrying = !(_interactor?.CarriedBattery != null);
                _lastLoopNumber = -1;
                _lastEchoCount = -1;
                _lastMaximumEchoes = -1;
                _lastRemainingTenths = -1;
                endLoopText.text = textCatalog.EndLoop;
                pausePromptText.text = textCatalog.PausePrompt;
            }

            if (director.LoopNumber != _lastLoopNumber)
            {
                _lastLoopNumber = director.LoopNumber;
                loopText.text = textCatalog.FormatLoop(_lastLoopNumber);
            }

            int tickRate = Mathf.Max(1, director.TickRate);
            int maximumTenths = Mathf.CeilToInt(director.MaxTicks * 10f / tickRate);
            if (_cachedMaximumTenths != maximumTenths)
                BuildTimeTextCache(maximumTenths);
            int remainingTenths = Mathf.Clamp(
                Mathf.RoundToInt((director.MaxTicks - director.CurrentTick) * 10f / tickRate),
                0, maximumTenths);
            if (remainingTenths != _lastRemainingTenths)
            {
                _lastRemainingTenths = remainingTenths;
                timerText.text = _timeTextCache[remainingTenths];
            }

            if (director.EchoCount != _lastEchoCount || director.MaxEchoes != _lastMaximumEchoes)
            {
                _lastEchoCount = director.EchoCount;
                _lastMaximumEchoes = director.MaxEchoes;
                echoText.text = textCatalog.FormatEchoCount(_lastEchoCount, _lastMaximumEchoes);
            }

            bool hasCandidate = _sensor?.CurrentTarget != null;
            bool carrying = _interactor?.CarriedBattery != null;
            string resolvedPrompt;
            if (hasCandidate && !_sensor.CurrentCanInteract)
            {
                resolvedPrompt = textCatalog.GetFailureText(_sensor.CurrentFailureReason);
            }
            else
            {
                InteractionKind promptKind = hasCandidate
                    ? _sensor.CurrentKind
                    : carrying
                        ? InteractionKind.DropBattery
                        : InteractionKind.None;
                bool showMovementOnboarding = promptKind == InteractionKind.None &&
                    Time.unscaledTime < _sectionIntroUntil;
                resolvedPrompt = promptKind != InteractionKind.None || showMovementOnboarding
                    ? ResolvePrompt(
                        _inputSource != null
                            ? _inputSource.LastPromptDevice
                            : InputPromptDevice.Keyboard,
                        promptKind,
                        promptKind != InteractionKind.None)
                    : string.Empty;
            }
            if (CurrentPrompt != resolvedPrompt)
            {
                CurrentPrompt = resolvedPrompt;
                promptText.text = CurrentPrompt;
            }

            IsCarrying = carrying;
            if (carrying != _lastCarrying)
            {
                _lastCarrying = carrying;
                carryText.text = carrying ? textCatalog.BatteryCarried : string.Empty;
            }
        }

        private void BuildTimeTextCache(int maximumTenths)
        {
            _cachedMaximumTenths = Mathf.Max(0, maximumTenths);
            _timeTextCache = new string[_cachedMaximumTenths + 1];
            for (int i = 0; i < _timeTextCache.Length; i++)
                _timeTextCache[i] = textCatalog.FormatTime(i * 0.1f);
            _lastRemainingTenths = -1;
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
            _tutorialUntil = string.IsNullOrEmpty(CurrentTutorial)
                ? 0f
                : Time.unscaledTime + tutorialNoticeDuration;
        }

        public void RefreshTransientPresentationForTests(float unscaledTime)
        {
            RefreshTransientPresentation(unscaledTime);
        }

        public void ExpireTransientPresentationForTests()
        {
            _sectionIntroUntil = float.NegativeInfinity;
            _tutorialUntil = float.NegativeInfinity;
            RefreshTransientPresentation(Time.unscaledTime);
        }

        public void ShowInteractionFailure(InteractionFailureReason reason)
        {
            LastFailureText = textCatalog.GetFailureText(reason);
            failureText.text = LastFailureText;
            _failureUntil = Time.unscaledTime + 2f;
        }

        public string ResolvePrompt(InputPromptDevice device, bool hasCandidate)
        {
            return ResolvePrompt(device, InteractionKind.None, hasCandidate);
        }

        public string ResolvePrompt(
            InputPromptDevice device,
            InteractionKind interactionKind,
            bool hasCandidate)
        {
            if (device == InputPromptDevice.Gamepad)
            {
                return hasCandidate
                    ? textCatalog.GetInteractionPrompt(interactionKind, true)
                    : textCatalog.MoveGamepad;
            }
            return hasCandidate
                ? textCatalog.GetInteractionPrompt(interactionKind, false)
                : textCatalog.MoveKeyboard;
        }

        private void RefreshTransientPresentation(float unscaledTime)
        {
            IsSectionIntroVisible = unscaledTime < _sectionIntroUntil;
            IsTutorialVisible = !string.IsNullOrEmpty(CurrentTutorial) &&
                unscaledTime < _tutorialUntil;
            if (sectionText != null && sectionText.gameObject.activeSelf != IsSectionIntroVisible)
                sectionText.gameObject.SetActive(IsSectionIntroVisible);
            if (objectiveText != null && objectiveText.gameObject.activeSelf != IsSectionIntroVisible)
                objectiveText.gameObject.SetActive(IsSectionIntroVisible);
            if (tutorialText != null && tutorialText.gameObject.activeSelf != IsTutorialVisible)
                tutorialText.gameObject.SetActive(IsTutorialVisible);
        }
    }
}
