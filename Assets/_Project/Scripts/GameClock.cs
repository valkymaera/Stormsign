
namespace LorePath.Stormsign
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// The base class for visual displays that represent the day/night and dew cycles
    /// of the in-game clock, usually represented by rotating dials.
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        protected enum DewWindow { None = 0, Decent, Optimal, Undefined }

        protected enum DayWindow { None = 0, Night, Day, Undefined }

        [SerializeField] protected Transform[] _rotateTargets;
        [SerializeField] protected Image _dewIndicator;
        [SerializeField] protected Image _activeDewImage;        
        [SerializeField] protected TextMeshProUGUI _label;
        
        /// <summary>
        /// The cached angular rotation of the dial elements.
        /// </summary>
        protected float _rotation;
       
        /// <summary>
        /// Read-only data object stored in the blackboard by the GameClockPriary instance.
        /// Allows any game clock to be in sync with the primary clock.
        /// </summary>
        protected IClockData _clockData;

        protected DewWindow _dewWindow = DewWindow.None;
        protected DewWindow _lastDewWindow = DewWindow.Undefined;
        protected bool _dirtyDew => _dewWindow != _lastDewWindow;
        protected bool _dewIsPresent => _dewWindow == DewWindow.Decent || _dewWindow == DewWindow.Optimal;
        protected bool _dewIsOptimal => _dewWindow == DewWindow.Optimal;


        protected DayWindow _dayNightWindow = DayWindow.None;
        protected DayWindow _lastDayWindow = DayWindow.Undefined;
        protected bool _dirtyDay => _dayNightWindow != _lastDayWindow;
        protected bool _isDayOrNearlyDay => _dayNightWindow == DayWindow.Day;

        /// <summary>
        /// true if the rotation of the clock dial is ACTUALLY over the day portion.
        /// This is different from the private day window values, which detect if it's nearly day or newarly night,
        /// for the purpose of sending events or audio signals.
        /// </summary>
        public bool IsDay => _rotation >= 90f && _rotation <= 270f;

        protected virtual void Awake()
        {
            EventBus.Subscribe<TimerTickEvent>(OnTick);
        }
        protected virtual void OnDestroy()
        {
            EventBus.Unsubscribe<TimerTickEvent>(OnTick);
        }
      
        protected virtual void Update()
        {
            UpdateRotationValue();

            // don't do any rotating or indicators or anything until we actually have a clock value set.
            // setting it means the user has rotated it to match the game,
            // OR we detected player prefs from them setting it in a previous session.
            if (_clockData != null)
            {
                UpdateFaceRotation();
                UpdateTimeWindows();
                UpdateIndicators();
            }
        }

        protected virtual void UpdateRotationValue()
        {
            if (_clockData != null || Blackboard.Get(out _clockData))
            {
                // any game clock representation is actually controlled by a GameClockPrimary instance,
                // which sets the clock data every update.
                _rotation = _clockData.Rotation;
            }
        }

        /// <summary>
        /// Rotates any elements that are supposed to rotate. This is typically one or more dials.
        /// </summary>
        protected virtual void UpdateFaceRotation()
        {
            foreach (var target in _rotateTargets)
            {
                target.localEulerAngles = Vector3.forward * -_rotation;
            }
        }
 
        /// <summary>
        /// Updates the flags associated with dew and day/night cycles.
        /// </summary>
        protected virtual void UpdateTimeWindows()
        {
            // the clock is divided into 20 x 18 degree slices. 10 for night, and 10 for day.
            // each slice represents a minute and a half. 
            // there's 3 small slices for optimal dew, sandwiched between 2 small slices for "decent" dew where it is growing or shrinking.

            if (_rotation < 72f) // if we're not at sunrise yet (72 degrees)...
            {
                if (_rotation >= 18f) _dewWindow = DewWindow.Optimal;  // then if we're past that increasing slice, we must be optimal.
                else _dewWindow = DewWindow.Decent;
            }
            else if (_rotation < 76f || _rotation > 350f)  _dewWindow = DewWindow.Decent; // slight dew before sunrise, and moderate before our zero degree setting.
            else _dewWindow = DewWindow.None;

            // the day window doesn't perfectly match actual day time or night time, since we want to know if it's coming, not if it's here.
            _dayNightWindow = _rotation > 70f && _rotation < 252f ? DayWindow.Day : DayWindow.Night;

            // TODO: put moderate dew signal on a slider in settings, map it around the night slice.
        }


        /// <summary>
        /// Updates any visual indicators on the clock based on window changes creating dirty flags.
        /// optionally can keep the flags dirty, so the primary clock can trigger an audio alert when editing is finished.
        /// </summary>
        /// <param name="keepDirty"></param>
        protected virtual void UpdateIndicators()
        {
            if (_rotation < 72f || _rotation > 270f) 
            {
                float dewFocusedRotation = (_rotation + 90f) % 360f;
                _dewIndicator.fillAmount = Mathf.InverseLerp(0f, 108f, dewFocusedRotation);
                Debug.Log(" rotation / fill = " + dewFocusedRotation + " / " + _dewIndicator.fillAmount);
            }
            else if (_rotation < 78f)
            {
                _dewIndicator.fillAmount = Mathf.InverseLerp(78f, 72f, _rotation);
            }
            else _dewIndicator.fillAmount = 0f;

            _activeDewImage.enabled = _dewIsPresent;
            _lastDewWindow = _dewWindow;


            // not doing anything here with these flags,
            // primary clock uses them for sunrise/sunset alerts etc
            _lastDayWindow = _dayNightWindow;
        }


        public virtual void SetVisible(bool visible)
        {
            var group = this.GetComponentInParent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = visible ? 1f : 0f;
                group.interactable = visible;
                group.blocksRaycasts = visible;
            }
            else this.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Called every time the storm timer ticks (once a second).
        /// This lets us update the minute label without having to do it every frame.
        /// </summary>
        void OnTick(TimerTickEvent evt)
        {
            if (_label != null && _clockData != null)
            {                                 
                // since we're tracking minutes until dew, we keep it at zero minutes until the dew actually stops,
                // which is sunrise at the 90 degree rotation mark.
                float minutes = 0f;
                if (_rotation > 90f) minutes = Mathf.RoundToInt(((360f - (_rotation - 18f)) / (0.2f * _clockData.Multiplier)) / 60f);
                else if (_rotation < 18f) minutes = Mathf.RoundToInt(((18f - _rotation) / (0.2f * _clockData.Multiplier)) / 60f);

                _label.text = _clockData.HasBeenSet ? minutes.ToString() + "m" : "??m";
            }
        }

    }

    /// <summary>
    /// Shared blackboard data used across clock instances,
    /// managed by GameClockPrimary to keep clocks in sync
    /// </summary>
    public interface IClockData : IDataObject
    {
        float Multiplier { get; }
        bool HasBeenSet { get; }
        float Rotation { get; }
    }
     

}


// q // cD // d
// Unity 6000.0.41f1
