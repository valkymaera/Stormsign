
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

    public class UISettingsAudio : MonoBehaviour
    {
        [SerializeField] UIAudioSignalToggle _togglePrefab;

        [SerializeField] Transform _contentParent;

        private List<UIAudioSignalToggle> _toggles = null;

        private void OnEnable()
        {
            if (_toggles == null)
            {
                GeneratePage();
            }
        }

        private void GeneratePage()
        {
            _toggles = new();
            if (Blackboard.Get<AudioSettings>(out var settings))
            {
                foreach (AudioSignal signal in System.Enum.GetValues(typeof(AudioSignal)))
                {
                    if (signal == AudioSignal.None) continue;
                    var toggle = Instantiate(_togglePrefab, _contentParent);
                    toggle.LoadFromSettings(settings, signal);
                    _toggles.Add(toggle);
                }
            }
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
