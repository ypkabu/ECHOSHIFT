using System;
using UnityEngine;

namespace EchoShift.Presentation
{
    public enum Phase4FeedbackEvent
    {
        EchoCreated,
        LoopTransition,
        EchoRemoved,
        InteractionSuccess,
        InteractionFailure,
        BatteryPickup,
        BatteryInsert,
        DoorOpened,
        DoorClosed,
        SectionCompleted,
        GameCompleted
    }

    public sealed class Phase4FeedbackPool : MonoBehaviour
    {
        public const float MaximumParticleSize = 0.32f;
        public const float MaximumParticleAlpha = 0.42f;
        public const int DefaultConcurrentLimit = 4;

        [SerializeField] private GameObject effectPrefab;
        [SerializeField, Min(4)] private int capacity = 12;
        [SerializeField, Range(1, DefaultConcurrentLimit)]
        private int maxConcurrentEffects = DefaultConcurrentLimit;

        private ParticleSystem[] _systems = Array.Empty<ParticleSystem>();
        private uint[] _sequences = Array.Empty<uint>();
        private uint _sequence;
        private bool _auditEnabled;

        public int Capacity => capacity;
        public int MaximumConcurrentEffects => maxConcurrentEffects;
        public int EmitCount { get; private set; }
        public int MaximumConcurrentObserved { get; private set; }
        public Phase4FeedbackEvent LastEvent { get; private set; }
        public float LastRequestedSize { get; private set; }
        public float LastRequestedAlpha { get; private set; }
        public int ActiveEffectCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < _systems.Length; i++)
                    if (_systems[i] != null && _systems[i].IsAlive(true)) count++;
                return count;
            }
        }
        public bool HasValidReferences => effectPrefab != null && capacity >= 4 &&
            maxConcurrentEffects >= 1 && maxConcurrentEffects <= DefaultConcurrentLimit;

        public void Configure(GameObject prefab, int poolCapacity,
            int concurrentLimit = DefaultConcurrentLimit)
        {
            effectPrefab = prefab;
            capacity = Mathf.Max(4, poolCapacity);
            maxConcurrentEffects = Mathf.Clamp(
                concurrentLimit, 1, DefaultConcurrentLimit);
        }

        private void Awake()
        {
            if (!HasValidReferences)
            {
                Debug.LogError("Phase 4 feedback pool requires a prefab and capacity.", this);
                enabled = false;
                return;
            }

            _systems = new ParticleSystem[capacity];
            _sequences = new uint[capacity];
            _auditEnabled = HasFlag("-phase43HumanReviewProbe") ||
                            HasFlag("-phase43VfxAudit");
            for (int i = 0; i < capacity; i++)
            {
                GameObject instance = Instantiate(effectPrefab, transform);
                instance.name = $"Feedback {i + 1:00}";
                ParticleSystem system = instance.GetComponent<ParticleSystem>();
                if (system == null)
                {
                    Debug.LogError("Phase 4 feedback prefab requires ParticleSystem.", instance);
                    enabled = false;
                    return;
                }
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                _systems[i] = system;
            }
        }

        public void Emit(
            Vector3 position,
            Color color,
            float size = 0.16f,
            float duration = 0.35f,
            Phase4FeedbackEvent feedbackEvent = Phase4FeedbackEvent.InteractionSuccess)
        {
            if (!enabled || _systems.Length == 0) return;
            int slot = SelectSlot();
            ParticleSystem system = _systems[slot];
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.transform.position = position;
            ParticleSystem.MainModule main = system.main;
            color.a = Mathf.Clamp(color.a, 0.04f, MaximumParticleAlpha);
            float clampedSize = Mathf.Clamp(size, 0.06f, MaximumParticleSize);
            main.startColor = color;
            main.startSize = clampedSize;
            main.startLifetime = Mathf.Clamp(duration, 0.12f, 0.55f);
            system.Play(true);
            _sequences[slot] = ++_sequence;
            EmitCount++;
            LastEvent = feedbackEvent;
            LastRequestedSize = clampedSize;
            LastRequestedAlpha = color.a;
            MaximumConcurrentObserved = Mathf.Max(
                MaximumConcurrentObserved, ActiveEffectCount);
            if (_auditEnabled)
            {
                Debug.Log($"PHASE4_VFX_EVENT event={feedbackEvent};" +
                          $"time={Time.realtimeSinceStartup:F3};size={clampedSize:F3};" +
                          $"alpha={color.a:F3};duration={main.startLifetime.constant:F3};" +
                          $"active={ActiveEffectCount};position={position}", this);
            }
        }

        private int SelectSlot()
        {
            int firstInactive = -1;
            int oldest = 0;
            uint oldestSequence = uint.MaxValue;
            int active = 0;
            for (int i = 0; i < _systems.Length; i++)
            {
                if (_systems[i] != null && _systems[i].IsAlive(true))
                {
                    active++;
                    if (_sequences[i] < oldestSequence)
                    {
                        oldest = i;
                        oldestSequence = _sequences[i];
                    }
                }
                else if (firstInactive < 0)
                {
                    firstInactive = i;
                }
            }
            if (active >= maxConcurrentEffects) return oldest;
            return firstInactive >= 0 ? firstInactive : oldest;
        }

        private static bool HasFlag(string value)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
                if (string.Equals(arguments[i], value,
                    StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
