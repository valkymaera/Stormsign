
namespace LorePath.Stormsign
{

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    public class SharedAudioPlayer : MonoBehaviour
    {
        
        [SerializeField] private AudioSource _source;
        [SerializeField] private Image _muteImage;
        [SerializeField] private Image _unmutedImage;
        [SerializeField] private Slider _slider;
        [SerializeField] private AudioClip _unknownClip;
        [SerializeField] private AudioClipLibrary _defaultLibrary;

        [Tooltip("How long to wait between audio clips when multiple are queued. " +
            "for simplicity we use a flat timer instead of checking play and length.")]
        [SerializeField] private float _delayDuration = 3f;

        private Queue<RequestAudioEvent> _queue = new();
        
        private float _playDelay = 0f;
        private bool _isMuted;
        private float _mainVolume;
        private bool _isDirty;
        private long _dirtyTime = 0;

        private void Awake()
        {
            EventBus.Subscribe<RiskLevelEvent>(OnRiskLevelChange);
            EventBus.Subscribe<RequestAudioEvent>(OnRequestAudio);
        }

        private void Start()
        {
            _mainVolume = PlayerPrefs.GetFloat("volume", 0.3f);
            _isMuted = PlayerPrefs.GetFloat("mute", 0f) > 0.5f;

            var audioSettings = new AudioSettings();
            audioSettings.LoadFromPrefs();
            Blackboard.Set(audioSettings);

            CommitVolume();
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<RiskLevelEvent>(OnRiskLevelChange);
            EventBus.Unsubscribe<RequestAudioEvent>(OnRequestAudio);
        }
         
        public void OnSliderChange(float sliderValue)
        {
            _mainVolume = sliderValue;
            _isDirty = true;

            // we wait half a second after an edit was detected before trying to apply the volume changes,
            // to allow scrubbing the volume slider without weirdness or perf damage.
            _dirtyTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 500;
        }
         

        void OnRequestAudio(RequestAudioEvent evt)
        {
            // if the request had no clip, we'll try to get one from our library.
            // if we can't, then we don't play it.
            if (evt.Clip == null && !_defaultLibrary.TryGetClip(evt.Signal, out evt.Clip)) return;
            
            if (_queue.Count == 0 && _playDelay <= 0f) 
            {
                // if other's arent waiting in line and we don't need to wait,
                // we can play the clip immediately.
                Play(evt);
            }
            else
            {
                // if there's a line or a wait timer, we queue up.
                _queue.Enqueue(evt);
            }
        }

        private void Update()
        {
            if (_isDirty && _dirtyTime < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            {
                // if our volume has changed and we're passed the grace period of committing,
                // then commit.
                _isDirty = false;
                CommitVolume();
            }
            if (_playDelay > 0f)
            {
                // tick down the play delay timer, then play something if there's anything to play.
                _playDelay -= Time.deltaTime;
                if (_playDelay <= 0f && _queue.Count > 0)
                {
                    var playable = _queue.Dequeue();
                    Play(playable);
                }
            }
        }

        /// <summary>
        /// Plays an audio request clip (if it has a clip)
        /// </summary>
        private void Play(RequestAudioEvent evt)
        {
            float signalVolume = 1f;
            if (Blackboard.Get<AudioSettings>(out var settings))
            {
                // make sure we get the volume level of the specific audio signal.
                signalVolume = settings.GetVolume(evt.Signal);
            }

            // we have  4 ways to change the volume. Per event, per signal, main volume, and mute button.
            float volume = evt.Volume * _mainVolume * (_isMuted ? 0f : 1f) * signalVolume;
            
            if (evt.Clip != null && volume > 0f)
            {
                _source.PlayOneShot(evt.Clip, volume);
                _playDelay = _delayDuration;
            }
            else
            {
                // if we didn't have a clip to play, or if the clip was muted,
                // we don't really force a wait for the next one.
                _playDelay = 0.1f;
            }
        }

        public void OnToggleAudio()
        {
            _isMuted = !_isMuted;
            CommitVolume();
        }

        /// <summary>
        /// Applies all pending volume changes and saves them in prefs, updating the display to match.
        /// </summary>
        private void CommitVolume()
        {
            PlayerPrefs.SetFloat("mute", _isMuted ? 1f : 0f);
            PlayerPrefs.SetFloat("volume", _mainVolume);
            PlayerPrefs.Save();
            _unmutedImage.enabled = _mainVolume > 0.01f && !_isMuted;
            _muteImage.enabled = !_unmutedImage.enabled;
            _slider.SetValueWithoutNotify(_mainVolume * (_isMuted ? 0f : 1f));
            _slider.interactable = !_isMuted;
        }
        
        /// <summary>
        /// Sends a risk audio signal when the risk theshold changes.        
        /// </summary>
        /// <remarks>
        /// This is leftover from when this script was focused on risk brackets.
        /// It should be moved probably to some risk component, and this script
        /// should handle general shared audio management behavior.
        /// </remarks>
        void OnRiskLevelChange(RiskLevelEvent evt)
        {
            
            AudioSignal signal = AudioSignal.None;
            
            // we have 3 extreme levels. I don't think we need to alert at every single one of them.
            // so we'll only alert if we are going to or from a non-extreme value.
            // TODO: consider adding mute toggles in audio settings for the other stages for extra alerts.
            bool isNotOrWasNotExtreme = (int)evt.Time.RiskLevel < (int)StormRisk.Extreme || (int)evt.Time.LastStormRisk < (int)StormRisk.Extreme;

            if (!Blackboard.Get(out RecentStorm recent) || recent.Data == null || evt.Time.RiskLevel == StormRisk.Unknown)
            {
                // if we didn't have a recent storm or we're actively unknown, we use the unknown signal.
                signal = AudioSignal.Storm_Risk_Unknown;
            }
            else if (isNotOrWasNotExtreme)
            {
                // we know the risk signals are generally sequential, but we don't know what value they start in,
                // since there could be enum entries before Storm_Risk_Low.
                // since the low signal is risk level 0, we can use addition to calibrate what signal should be sent.
                
                // we do have more risk levels than audio signals, so we clamp it to the highest risk-related one.
                signal = (AudioSignal)(Math.Min((int)evt.Time.RiskLevel + ((int)AudioSignal.Storm_Risk_Low), (int)AudioSignal.Storm_Risk_Extreme));
                
            }

            EventBus.RaiseEvent(new RequestAudioEvent(signal));
        }

    }
     
}


// q // cD // d
// Unity 6000.0.41f1
