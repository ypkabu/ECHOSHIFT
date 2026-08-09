using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class Phase4AudioController : MonoBehaviour
    {
        [SerializeField] private Phase4AudioCueSet cueSet;
        [SerializeField] private AudioSource[] sources = System.Array.Empty<AudioSource>();
        [SerializeField, Min(0.01f)] private float minimumRepeatInterval = 0.075f;

        private readonly float[] _lastPlayed = new float[(int)Phase4AudioCue.Count];
        private int _nextSource;

        public int PlayCount { get; private set; }
        public int SuppressedCount { get; private set; }
        public Phase4AudioCueSet CueSet => cueSet;
        public bool HasValidReferences
        {
            get
            {
                if (cueSet == null || !cueSet.HasAllCues || sources == null || sources.Length == 0)
                    return false;
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i] == null) return false;
                }
                return true;
            }
        }

        public void Configure(Phase4AudioCueSet cues, AudioSource[] pooledSources)
        {
            cueSet = cues;
            sources = pooledSources ?? System.Array.Empty<AudioSource>();
            for (int i = 0; i < _lastPlayed.Length; i++) _lastPlayed[i] = -1000f;
        }

        public bool Play(Phase4AudioCue cue, float volume = 1f, float pitch = 1f)
        {
            if (!HasValidReferences) return false;
            int cueIndex = (int)cue;
            float now = Time.unscaledTime;
            if (now - _lastPlayed[cueIndex] < minimumRepeatInterval)
            {
                SuppressedCount++;
                return false;
            }

            AudioSource source = FindSource();
            source.clip = cueSet.Get(cue);
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Mathf.Clamp(pitch, 0.75f, 1.35f);
            source.Play();
            _lastPlayed[cueIndex] = now;
            PlayCount++;
            return true;
        }

        private AudioSource FindSource()
        {
            for (int i = 0; i < sources.Length; i++)
            {
                int index = (_nextSource + i) % sources.Length;
                if (!sources[index].isPlaying)
                {
                    _nextSource = (index + 1) % sources.Length;
                    return sources[index];
                }
            }
            AudioSource selected = sources[_nextSource];
            _nextSource = (_nextSource + 1) % sources.Length;
            return selected;
        }
    }
}
