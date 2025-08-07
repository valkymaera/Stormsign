
namespace LorePath.Stormsign
{

    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// An editable version of the game clock, that manages ground truth about the time
    /// for other clocks to read from.
    /// </summary>
    public class GameClockPrimary : GameClock, IPointerDownHandler, IPointerUpHandler
    {       

        private const string _clockSaveKey = "clock_save_time";
        private const string _clockRotateKey = "clock_rotation";
        private const float _degreesPerSecond = 0.2f;

        [SerializeField] private bool _closeOnEscape = true;
        [SerializeField] private float _clockSpeed = 1f;
        [SerializeField] private bool _canEdit = true;

        [Header("Optional audio overrides")]
        [SerializeField] private AudioClip _dewOptimalOverrideClip;
        [SerializeField] private AudioClip _dewStartedOverrideClip;
        [SerializeField] private AudioClip _sunriseOverrideClip;
        [SerializeField] private AudioClip _nightfallOverrideClip;

        private bool _isEditMode;

        private float _rotationAtEditStart;
       
        private Vector2 _dragStartVector;

        /// <summary>
        /// our settable clock data values, stored as read-only in the blackboard.
        /// </summary>
        private ManagedClockData _managedClockData; 


        protected override void Awake()
        {
            // we will be creating and managing the clock data used across game clock instances.
            // but we store it as its read only public interface for other clocks.
            _managedClockData = new();
            _clockData = _managedClockData;
            Blackboard.Set(_clockData);

            base.Awake();
        }


        void Start()
        {
            // we'll have saved the clock state previously. This allows us to only set it once, even betwen sessions,
            // though it may go out of sync over time.
            if (PlayerPrefs.HasKey(_clockSaveKey) && long.TryParse(PlayerPrefs.GetString(_clockSaveKey), out long saveTime))
            {
                float savedRotation = PlayerPrefs.GetFloat(_clockRotateKey);
                long deltaMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - saveTime;
                float degreesPerMillisecond = _degreesPerSecond / 1000f;

                _rotation = (savedRotation + (deltaMilliseconds * degreesPerMillisecond)) % 360f;
                _managedClockData.HasBeenSet = true;
            }
        }

        protected override void Update()
        {
            /// largely the same as the base Update(), 
            /// we just make sure the clock data is up to date, and check for escape to hide.
            
            _managedClockData.Multiplier = _clockSpeed;
            
            base.Update();
            // the primary clock can generally be closed on escape
            UpdateVisibility();
        }


        protected override void UpdateRotationValue()
        {
            if (!_isEditMode)
            {
                if (_rotation < 0f) _rotation = 360f - (Mathf.Abs(_rotation) % 360f);
                _rotation = (_rotation + (_clockSpeed * _degreesPerSecond * Time.deltaTime)) % 360f;
            }
            else UpdateMouseDrag();
            _managedClockData.Rotation = _rotation;

        }

        /// <summary>
        /// Updates the visual indicators for dew / day night, but also raises audio events.
        /// Only does this when not editing
        /// </summary>
        protected override void UpdateIndicators()
        {
            // we don't want to send audio events or update the dew indicator until we stop dragging.
            if (_isEditMode) return;

            if (_dirtyDew && _dewIsPresent)
            {
                EventBus.RaiseEvent(new RequestAudioEvent()
                {
                    Signal = _dewIsOptimal ? AudioSignal.Dew_Optimal : AudioSignal.Dew_Start,
                    Clip = _dewIsPresent ? _dewOptimalOverrideClip : _dewStartedOverrideClip,
                    Volume = 1f
                }); 
            }

            if (_dirtyDay)
            {
                EventBus.RaiseEvent(new RequestAudioEvent()
                {
                    Signal = _isDayOrNearlyDay ? AudioSignal.Sunrise : AudioSignal.Nightfall,
                    Clip = _isDayOrNearlyDay ? _sunriseOverrideClip: _nightfallOverrideClip,
                    Volume = 1f
                });
            }

            base.UpdateIndicators();

        }

        private void UpdateVisibility()
        {
            if (!_isEditMode && Input.GetKeyDown(KeyCode.Escape) && _closeOnEscape)
            {
                SetVisible(false);
            }
        }

        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);
            _isEditMode &= visible; // never in edit mode when we turn off.
        }


        /// <summary>
        /// If editable, clicking the clock will store the current rotation so we can
        /// update it as we drag based on that.
        /// </summary>
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (_canEdit)
            {
                _rotationAtEditStart = _rotation;
                _isEditMode = true;
                _dragStartVector = (Input.mousePosition - this.transform.position).normalized;
            }
        }

        /// <summary>
        /// On releasing the clock, we save the newly set clock rotation in prefs, along with current real time,
        /// so next time we open the app it will load the delta rotation based on how long has passed.
        /// </summary>
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (_canEdit)
            {
                _isEditMode = false;
                _managedClockData.HasBeenSet = true;
                PlayerPrefs.SetString(_clockSaveKey, DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
                PlayerPrefs.SetFloat(_clockRotateKey, _rotation);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// While editing, as we drag the mouse we update our rotation based on the vector to the mouse,
        /// compared to the vector to where we started the drag.
        /// </summary>
        void UpdateMouseDrag()
        {
            Vector2 mouseVector = (Input.mousePosition - this.transform.position).normalized;
            _rotation = _rotationAtEditStart + Vector2.SignedAngle(mouseVector, _dragStartVector);
        }

        /// <summary>
        /// The clock data read by other clock instances. We use a private class
        /// to restrict other clocks from setting the values.
        /// </summary>
        private class ManagedClockData : IClockData
        {
            public float Multiplier { get; set; }
            public bool HasBeenSet { get; set; }
            public float Rotation { get; set; }
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
