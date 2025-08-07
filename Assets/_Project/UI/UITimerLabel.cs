
namespace LorePath.Stormsign
{
    using TMPro;
    using UnityEngine;

    //using LorePath;
    //using DG.Tweening;

    public class UITimerLabel : ITimerListener
    {
        [SerializeField] private TextMeshProUGUI _label;

        protected override void OnTimerTick(TimerTickEvent evt)
        {
            base.OnTimerTick(evt);
            _label.text = $"{evt.Time.Minutes.ToString("00")}:{evt.Time.Seconds.ToString("00")}";
        }

    }

}


// q // cD // d
// Unity 6000.0.41f1
