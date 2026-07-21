using System;
using UnityEngine;

namespace EchoShift.Presentation
{
    public enum Phase4AudioCue : byte
    {
        Footstep,
        InteractionSuccess,
        InteractionFailure,
        BatteryPickup,
        BatteryInsert,
        Door,
        LoopEnd,
        EchoSpawn,
        SectionComplete,
        GameComplete,
        UiSelect,
        UiConfirm,
        UiBack,
        Count
    }

    [CreateAssetMenu(menuName = "ECHO SHIFT/Phase 4 Audio Cues")]
    public sealed class Phase4AudioCueSet : ScriptableObject
    {
        [SerializeField] private AudioClip[] clips = Array.Empty<AudioClip>();

        public int Count => clips?.Length ?? 0;
        public bool HasAllCues
        {
            get
            {
                if (clips == null || clips.Length != (int)Phase4AudioCue.Count) return false;
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] == null) return false;
                }
                return true;
            }
        }

        public AudioClip Get(Phase4AudioCue cue)
        {
            int index = (int)cue;
            return clips != null && index >= 0 && index < clips.Length ? clips[index] : null;
        }

        public void Configure(AudioClip[] values)
        {
            clips = values ?? Array.Empty<AudioClip>();
        }
    }
}
