
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

    public class UIFadePattern : MonoBehaviour
    {
        [SerializeField] private FadePatternSet _patternSet;
        [SerializeField] private AnimationCurve _activePattern;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private CanvasGroup _canvasGroup;

        float _time;

        private void Awake()
        {
            EventBus.Subscribe<RiskLevelEvent>(OnRiskLevelChange);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RiskLevelEvent>(OnRiskLevelChange);
        }


        private void Update()
        {
            if (_activePattern != null)
            {
                _time += Time.deltaTime;
                float curveTime = _duration > 0f ? _time / _duration : 0f;
                float alpha = _activePattern.Evaluate(curveTime);
                _canvasGroup.alpha = alpha;
                if (curveTime >= 1f) _time -= _duration;
            }

        }

        void OnRiskLevelChange(RiskLevelEvent evt)
        {

            int index = Math.Min((int)evt.Time.RiskLevel, _patternSet.Curves.Count-1);
            if (index >= 0) _activePattern = _patternSet.Curves[index];

        }

    }

}


// q // cD // d
// Unity 6000.0.41f1
