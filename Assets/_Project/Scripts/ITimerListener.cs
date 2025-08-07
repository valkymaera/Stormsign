
namespace LorePath.Stormsign
{
    using UnityEngine;

    //using LorePath;
    //using DG.Tweening;

    public abstract class ITimerListener : MonoBehaviour
    {
        protected virtual void Awake()
        {
            EventBus.Subscribe<TimerUpdateEvent>(OnUpdateTimer);
            EventBus.Subscribe<TimerTickEvent>(OnTimerTick);
            EventBus.Subscribe<RiskLevelEvent>(OnRiskLevelChange);
        }
        protected virtual void OnDestroy()
        {
            EventBus.Unsubscribe<TimerUpdateEvent>(OnUpdateTimer);
            EventBus.Unsubscribe<TimerTickEvent>(OnTimerTick);
            EventBus.Unsubscribe<RiskLevelEvent>(OnRiskLevelChange);
        }

        protected virtual void OnUpdateTimer(TimerUpdateEvent evt) { }
       
        protected virtual void OnTimerTick(TimerTickEvent evt) { }

        protected virtual void OnRiskLevelChange(RiskLevelEvent evt) { }

    }

}


// q // cD // d
// Unity 6000.0.41f1
