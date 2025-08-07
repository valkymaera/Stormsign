
namespace LorePath.Stormsign
{

    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    public class ClickableTimer : MonoBehaviour, IPointerClickHandler
    {
        private long _now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        [SerializeField] private float durationSeconds = 2700; // 45m default
        [SerializeField] private TextMeshProUGUI _timeLabel;        
        [SerializeField] private bool _loop;

        [SerializeField] private Image _meterImage;

        [SerializeField] private AudioSignal _audioSignal;

        [SerializeField] private AudioClip _audioClipOverride;

        private long _startTime;
        protected bool _active = false;
        protected float _normalTime = 0f;

        private RequestAudioEvent _audioEvent;


        protected virtual void Awake()
        {
            _audioEvent = new RequestAudioEvent() { Signal = _audioSignal, Clip = _audioClipOverride, Volume = 1f };
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            ResetTimer();
        }

        void ResetTimer()
        {
            _active = true;
            _startTime = _now;
        }


        protected virtual void Update()
        {
            if (_active)
            {
                long timePassed = _now - _startTime;
                _normalTime = timePassed / durationSeconds;
                if (_meterImage) _meterImage.fillAmount = _normalTime;

                if (timePassed > durationSeconds)
                {
                    OnTimerComplete();
                }
                if (_timeLabel)
                {
                    _timeLabel.text = Mathf.RoundToInt((durationSeconds - timePassed)/60f).ToString() + "m";
                }
            }
        }

        protected virtual void OnTimerComplete()
        {
            _active = _loop;
            if (_active) _startTime = _now;
            EventBus.RaiseEvent(_audioEvent);
        }

    }

}


// q // cD // d
// Unity 6000.0.41f1
