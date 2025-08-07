

namespace LorePath.Stormsign
{
    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(fileName = "AudioClipLibrary_", menuName = "Scriptable Objects/AudioClipLibrary")]
    public class AudioClipLibrary : ScriptableObject
    {
        [SerializeField] private List<ClipEntry> _clipEntries = new();


        [System.NonSerialized] // Dictionaries generally not serialized but the editor can do weird things, and we want this null.
        private Dictionary<AudioSignal, AudioClip> _clipLookup = null;

        /// <summary>
        /// Gets the audio clip for a signal if possible.
        /// </summary>        
        public bool TryGetClip(AudioSignal signal, out AudioClip clip)
        {
            if (_clipLookup == null)
            {
                _clipLookup = new();
                _clipEntries.ForEach(x => _clipLookup[x.Signal] = x.Clip);
            }
            
            return _clipLookup.TryGetValue(signal, out clip);
        }


        /// <summary>
        /// Helper method to easily build an audio request event from a signal.
        /// </summary>        
        /// <returns></returns>
        public bool TryGetAudioRequest (AudioSignal signal, out RequestAudioEvent evt)
        {
            evt = null;
            AudioClip clip;
            if (TryGetClip(signal, out clip))
            {
                evt = new();
                evt.Clip = clip;
                evt.Signal = signal;
                return true;
            }
            return false;
        }


        [System.Serializable]
        private class ClipEntry
        {
            public AudioSignal Signal;
            public AudioClip Clip;
        }
    }
}
