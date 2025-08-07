
namespace LorePath.Stormsign
{
       
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.AI;
    using TMPro;

    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;

    //using LorePath;
    //using DG.Tweening;

    public class AudioSettings : IDataObject
    {
        private const string prefkey = "audio_";

        private Dictionary<AudioSignal, float> _baseVolume = new Dictionary<AudioSignal, float>();
        private HashSet<AudioSignal> _disabledByDefault = new()
        {
            AudioSignal.Storm_Risk_Low, 
            AudioSignal.Sunrise, 
            AudioSignal.Nightfall
        };

        public void LoadFromPrefs()
        {
            foreach (AudioSignal signalType in System.Enum.GetValues(typeof(AudioSignal)))
            {
                string key = prefkey + signalType.ToString();
                if (PlayerPrefs.HasKey(key))
                {
                    float volume = PlayerPrefs.GetFloat(key);
                    _baseVolume[signalType] = volume;
                }
                else
                {
                    float volume = _disabledByDefault.Contains(signalType) ? 0f : 1f;
                    SetVolume(signalType, volume);
                }
            }
            PlayerPrefs.Save();
        }

        public void SetVolume(AudioSignal signal, float volume)
        {
            _baseVolume[signal] = volume;
            PlayerPrefs.SetFloat("audio_" + signal.ToString(), volume);
        }

        public float GetVolume(AudioSignal signal, float defaultVolume = 1f)
        {
            if (!_baseVolume.TryGetValue(signal, out var volume))
            {
                volume = defaultVolume;
            }
            return volume;
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
