
namespace LorePath.Stormsign
{

    using System;
    using UnityEngine;

    //using LorePath;
    //using DG.Tweening;

    public class UITimeBar : ITimerListener
    {
        [SerializeField] private RectTransform _scrubber;
        [SerializeField] private RectTransform _container;

        [Range(0f, 1f)]
        public float Scrubtest;
        
        protected override void OnTimerTick(TimerTickEvent evt)
        {
            _scrubber.anchoredPosition = Vector2.right * _container.rect.width * evt.Time.NormalizedTime;
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
