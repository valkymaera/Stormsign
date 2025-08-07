
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

    /// <summary>
    /// Toggles pausing and unpausing the timer.
    /// </summary>
    public class UIPauseToggle : MonoBehaviour
    {
        [SerializeField] private Image _playImage;
        [SerializeField] private Image _pauseImage;

        Color _activeColor = Color.white;
        Color _inactiveColor = new Color(1f, 1f, 1f, 0.25f);

        private TimerPauseRequest _togglePauseRequest = new() { Pause = true, Unpause = true };

        private void Awake()
        {
            _playImage.color = _activeColor;
            _pauseImage.color = _inactiveColor;
            EventBus.Subscribe<TimerPauseEvent>(OnPauseChange);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TimerPauseEvent>(OnPauseChange);
        }
        public void OnClick()
        {
            EventBus.RaiseEvent(_togglePauseRequest);
        }

        void OnPauseChange(TimerPauseEvent evt)
        {
            _playImage.color = evt.Time.IsPaused ? _inactiveColor : _activeColor;
            _pauseImage.color = evt.Time.IsPaused ? _activeColor : _inactiveColor;
        }

    }

}


// q // cD // d
// Unity 6000.0.41f1
