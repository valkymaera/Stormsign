
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

    public class UIAudioSignalToggle : MonoBehaviour
    {
        [SerializeField] private AudioSignal _signal;
        [SerializeField] private Image _muteImage, _activeImage;
        [SerializeField] private bool _isActive;
        [SerializeField] private TextMeshProUGUI _label;

        public void LoadFromSettings(AudioSettings settings, AudioSignal signal)
        {
            _signal = signal;
            _isActive = settings.GetVolume(_signal) > 0.5f;
            _label.text = signal.ToString().Replace("_", " ");            
            RefreshUI();
        }

        public void OnClick()
        {
            if (Blackboard.Get<AudioSettings>(out var settings))
            {                
                _isActive = !_isActive;
                settings.SetVolume(_signal, _isActive ? 1f : 0f);
                PlayerPrefs.Save();
                RefreshUI();
            }
            else Debug.LogError("no settings.");
        }

        private void RefreshUI()
        {
            _muteImage.enabled = !_isActive;
            _activeImage.enabled = _isActive;
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
