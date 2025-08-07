
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

    public class DewTimer : ClickableTimer
    {
        [SerializeField] private AudioClip _optimalClip;

        [SerializeField] private Image _backdrop;

        private bool _optimalAnnounced = false;
        private RequestAudioEvent _optimalEvent;

        protected override void Awake()
        {
            base.Awake();
            _optimalEvent = new RequestAudioEvent() { Signal = AudioSignal.Dew_Optimal, Clip = _optimalClip, Volume = 1f };
        }
        protected override void Update()
        {
            base.Update();

            if (_active && _normalTime >= 0.1 && _normalTime < 0.25f && !_optimalAnnounced)
            {
                _optimalAnnounced = true;
                EventBus.RaiseEvent(_optimalEvent);
                _backdrop.fillAmount = 1f;
            }
            else if (_active && _normalTime < 0.1f)
            {
                _backdrop.fillAmount = 0.5f;
            }
            else if (!_active || _normalTime > 0.25f) _backdrop.fillAmount = 0f;
        }
        protected override void OnTimerComplete()
        {
            base.OnTimerComplete();
            _optimalAnnounced = false;
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
