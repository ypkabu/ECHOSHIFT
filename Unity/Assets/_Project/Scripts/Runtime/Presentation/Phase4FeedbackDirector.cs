using System;
using EchoShift.Core;
using EchoShift.Gameplay;
using EchoShift.Interaction;
using EchoShift.Interaction.Recorded;
using EchoShift.Player;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4FeedbackDirector : MonoBehaviour
    {
        public const float LargestCueSize = 0.28f;
        public const float LargestCueAlpha = 0.38f;

        [SerializeField] private PuzzleSectionController[] sections =
            Array.Empty<PuzzleSectionController>();
        [SerializeField] private SectionTransitionCoordinator coordinator;
        [SerializeField] private Phase4VisualSettings settings;
        [SerializeField] private Phase4FeedbackPool feedbackPool;
        [SerializeField] private Phase4AudioController audioController;
        [SerializeField] private DoorController[] trackedDoors = Array.Empty<DoorController>();

        private bool _subscribed;
        private bool[] _doorStates = Array.Empty<bool>();
        private bool[] _goalFeedbackPlayed = Array.Empty<bool>();

        public int DoorFeedbackCount { get; private set; }
        public int EchoRemovalFeedbackCount { get; private set; }
        public int GoalCompletionFeedbackCount { get; private set; }
        public bool IsSubscribed => _subscribed;
        public int TrackedDoorCount => trackedDoors != null ? trackedDoors.Length : 0;

        public bool HasValidReferences =>
            sections != null && sections.Length == 3 && coordinator != null &&
            settings != null && feedbackPool != null && audioController != null &&
            trackedDoors != null && trackedDoors.Length >= 3;

        public void Configure(
            PuzzleSectionController[] puzzleSections,
            SectionTransitionCoordinator sectionCoordinator,
            Phase4VisualSettings visualSettings,
            Phase4FeedbackPool pool,
            Phase4AudioController audio)
        {
            sections = puzzleSections ?? Array.Empty<PuzzleSectionController>();
            coordinator = sectionCoordinator;
            settings = visualSettings;
            feedbackPool = pool;
            audioController = audio;
            BuildDoorCache();
            _goalFeedbackPlayed = new bool[sections.Length];
        }

        private void Awake()
        {
            _doorStates = new bool[trackedDoors != null ? trackedDoors.Length : 0];
            _goalFeedbackPlayed = new bool[sections != null ? sections.Length : 0];
        }

        private void OnEnable() => Subscribe();
        private void Start() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            for (int i = 0; i < trackedDoors.Length; i++)
            {
                DoorController door = trackedDoors[i];
                if (door == null || !door.gameObject.activeInHierarchy) continue;
                bool requested = door.IsOpenRequested;
                if (requested == _doorStates[i]) continue;
                _doorStates[i] = requested;
                Color color = requested ? settings.GoalColor : settings.DangerColor;
                feedbackPool.Emit(
                    door.transform.position + Vector3.up,
                    WithAlpha(color, requested ? 0.32f : 0.24f),
                    requested ? 0.22f : 0.16f,
                    0.32f,
                    requested
                        ? Phase4FeedbackEvent.DoorOpened
                        : Phase4FeedbackEvent.DoorClosed);
                audioController.Play(Phase4AudioCue.Door, 0.62f, requested ? 1.05f : 0.82f);
                DoorFeedbackCount++;
            }

            PuzzleSectionController active = coordinator != null ? coordinator.ActiveSection : null;
            int activeIndex = coordinator != null ? coordinator.ActiveSectionNumber - 1 : -1;
            if (active != null && activeIndex >= 0 && activeIndex < _goalFeedbackPlayed.Length &&
                !active.Goal.IsReached)
                _goalFeedbackPlayed[activeIndex] = false;
        }

        private void Subscribe()
        {
            if (_subscribed || !HasValidReferences) return;
            for (int i = 0; i < sections.Length; i++)
            {
                PuzzleSectionController section = sections[i];
                section.Director.LoopCompleted += OnLoopCompleted;
                section.Director.InteractionResolved += OnInteractionResolved;
                section.Director.EchoRemoved += OnEchoRemoved;
                section.Goal.ActorEntered += OnGoalEntered;
            }
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            for (int i = 0; i < sections.Length; i++)
            {
                PuzzleSectionController section = sections[i];
                if (section == null) continue;
                section.Director.LoopCompleted -= OnLoopCompleted;
                section.Director.InteractionResolved -= OnInteractionResolved;
                section.Director.EchoRemoved -= OnEchoRemoved;
                section.Goal.ActorEntered -= OnGoalEntered;
            }
            _subscribed = false;
        }

        private void OnLoopCompleted(LoopHistorySummary summary)
        {
            Vector3 position = coordinator.ActiveSection != null
                ? coordinator.ActiveSection.Player.transform.position
                : transform.position;
            feedbackPool.Emit(
                position + Vector3.up,
                WithAlpha(settings.GetEchoColor(summary.ReplayGeneration), 0.34f),
                0.22f,
                0.34f,
                Phase4FeedbackEvent.EchoCreated);
            feedbackPool.Emit(
                position + Vector3.up * 0.35f,
                WithAlpha(Color.white, 0.18f),
                0.10f,
                0.20f,
                Phase4FeedbackEvent.LoopTransition);
            audioController.Play(Phase4AudioCue.LoopEnd, 0.72f);
            audioController.Play(SpawnCue(summary.ReplayGeneration), 0.58f);
        }

        private void OnEchoRemoved(Vector3 position, int generation)
        {
            feedbackPool.Emit(
                position + Vector3.up,
                WithAlpha(settings.GetEchoColor(generation), 0.24f),
                0.16f,
                0.25f,
                Phase4FeedbackEvent.EchoRemoved);
            audioController.Play(Phase4AudioCue.EchoRemove, 0.58f);
            EchoRemovalFeedbackCount++;
        }

        private void OnInteractionResolved(InteractionExecution execution, LoopActor actor)
        {
            Vector3 position = actor != null ? actor.transform.position + Vector3.up : transform.position;
            if (!execution.Succeeded)
            {
                feedbackPool.Emit(
                    position,
                    WithAlpha(settings.DangerColor, 0.32f),
                    0.15f,
                    0.28f,
                    Phase4FeedbackEvent.InteractionFailure);
                audioController.Play(Phase4AudioCue.InteractionFailure, 0.55f);
                actor?.GetComponent<Phase4ActorVisual>()?.PulseInteraction(false);
                return;
            }

            Phase4FeedbackEvent feedbackEvent = Phase4FeedbackEvent.InteractionSuccess;
            if (execution.Command.Kind == InteractionKind.PickupBattery)
                feedbackEvent = Phase4FeedbackEvent.BatteryPickup;
            else if (execution.Command.Kind == InteractionKind.InsertBattery)
                feedbackEvent = Phase4FeedbackEvent.BatteryInsert;
            feedbackPool.Emit(
                position,
                WithAlpha(Color.white, 0.20f),
                0.12f,
                0.24f,
                feedbackEvent);
            audioController.Play(Phase4AudioCue.InteractionSuccess, 0.5f);
            if (execution.Command.Kind == InteractionKind.PickupBattery)
                audioController.Play(Phase4AudioCue.BatteryPickup, 0.6f);
            else if (execution.Command.Kind == InteractionKind.InsertBattery)
                audioController.Play(Phase4AudioCue.BatteryInsert, 0.65f);
            actor?.GetComponent<Phase4ActorVisual>()?.PulseInteraction(true);
        }

        private void OnGoalEntered(LoopActor actor)
        {
            if (actor == null || actor.Kind != LoopActorKind.Player) return;
            int sectionIndex = coordinator.ActiveSectionNumber - 1;
            if (sectionIndex < 0 || sectionIndex >= _goalFeedbackPlayed.Length ||
                _goalFeedbackPlayed[sectionIndex]) return;
            _goalFeedbackPlayed[sectionIndex] = true;
            bool final = coordinator.ActiveSectionNumber >= sections.Length;
            feedbackPool.Emit(
                actor.transform.position + Vector3.up,
                WithAlpha(settings.GoalColor, LargestCueAlpha),
                LargestCueSize,
                0.48f,
                final
                    ? Phase4FeedbackEvent.GameCompleted
                    : Phase4FeedbackEvent.SectionCompleted);
            audioController.Play(
                final ? Phase4AudioCue.GameComplete : Phase4AudioCue.SectionComplete,
                final ? 0.9f : 0.75f);
            GoalCompletionFeedbackCount++;
        }

        private void BuildDoorCache()
        {
            int count = 0;
            for (int i = 0; i < sections.Length; i++)
                if (sections[i] != null)
                    count += sections[i].GetComponentsInChildren<DoorController>(true).Length;
            trackedDoors = new DoorController[count];
            int offset = 0;
            for (int i = 0; i < sections.Length; i++)
            {
                if (sections[i] == null) continue;
                DoorController[] found = sections[i].GetComponentsInChildren<DoorController>(true);
                Array.Copy(found, 0, trackedDoors, offset, found.Length);
                offset += found.Length;
            }
            _doorStates = new bool[trackedDoors.Length];
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Phase4AudioCue SpawnCue(int generation)
        {
            return generation switch
            {
                2 => Phase4AudioCue.EchoSpawn2,
                3 => Phase4AudioCue.EchoSpawn3,
                _ => Phase4AudioCue.EchoSpawn
            };
        }
    }
}
