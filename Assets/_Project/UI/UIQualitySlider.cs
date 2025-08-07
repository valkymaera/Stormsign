
namespace LorePath.Stormsign
{
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    public class UIQualitySlider : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _sliderImage;

        private void Awake()
        {
            EventBus.Subscribe<RecentStormChangeEvent>(OnRecentStormChange);
        }
        private void OnDestroy()
        {
            EventBus.Unsubscribe<RecentStormChangeEvent>(OnRecentStormChange);
        }
        public void OnSliderChange(float value)
        {
            _sliderImage.color = Color.Lerp(Color.red, Color.green, value / 5f);
            RecentStorm recent;
            if (Blackboard.Get(out recent) && recent.Data != null)
            {
                recent.Data.DataQuality = Mathf.RoundToInt(value);
                EventBus.RaiseEvent(new RecentStormUpdateEvent());
            }
        }

        void OnRecentStormChange(RecentStormChangeEvent evt)
        {
            RecentStorm recent;
            if (Blackboard.Get(out recent) && recent.Data != null)
            {
                _slider.SetValueWithoutNotify(recent.Data.DataQuality);
                _sliderImage.color = Color.Lerp(Color.red, Color.green, recent.Data.DataQuality / 5f);
            }
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
