
namespace LorePath.Stormsign
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Displays text corresponding to the window of time remaining before a storm spawns,
    /// and the general risk bracket that window falls within.
    /// </summary>
    public class UIBracketBlock : ITimerListener
    {
        [SerializeField] private TextMeshProUGUI _windowLabel;
        [SerializeField] private TextMeshProUGUI _bracketLabel;
        protected override void OnTimerTick(TimerTickEvent evt)
        {
            base.OnTimerTick(evt);
            _windowLabel.text = $"{evt.Time.StormWindow[0]}m - {evt.Time.StormWindow[1]}m";
        }
        protected override void OnRiskLevelChange(RiskLevelEvent evt)
        {
            base.OnRiskLevelChange(evt);
            _bracketLabel.text = $"{evt.Time.RiskBracket[0]}m - {evt.Time.RiskBracket[1]}m";
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
