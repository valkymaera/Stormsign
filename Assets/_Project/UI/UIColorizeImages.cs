
namespace LorePath.Stormsign
{

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Applies a color from a palette to the list of images, based on the current 
    /// sandstorm risk bracket.
    /// </summary>
    public class UIColorizeImages : ITimerListener
    {
        [SerializeField] private ColorPalette _palette;
        [SerializeField] private List<Image> _colorizedImages;

        protected override void Awake()
        {
            base.Awake();
            SetColor(StormRisk.High);
        }

        protected override void OnRiskLevelChange(RiskLevelEvent evt)
        {            
            
            SetColor(evt.Time.RiskLevel);
        }

        private void SetColor(StormRisk risk)
        {
            Color color;
            color = _palette.Colors.Count > 0 ? _palette.Colors[Math.Min(_palette.Colors.Count - 1, (int)risk)] : Color.white;
            foreach (var img in _colorizedImages)
            {
                img.color = color;
            }
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
