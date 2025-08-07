namespace LorePath.Stormsign
{

    using System;
    using UnityEngine;

    //using LorePath;
    //using DG.Tweening;

    public enum StormRisk
    {        
        Low = 0,    // 45 to 60m
        Moderate,   // 15 to 30m.
        High,       // 5 to 20m
        Extreme,    // 0 to 15m. Any time now.
        Extremer,   // < 10m
        Extremest,  // < 5m
        Unknown,
    }

    /// <summary>
    /// Handles the passage of time between storms, and the calculation of its relative and normalized values.
    /// This does NOT manage the game clock, which is about dew and day/night cycles, only storm timer related values.
    /// </summary>
    public class StormTimer : MonoBehaviour
    {
        
        [Tooltip("A multiplier for offset adjustments in the editor. In builds this is ignored.")]
        [SerializeField] private float _debugMultiplier = 1f;

        private TimeData _time = new();
        public long Now => _time.Now;
        public long Start => _time.Start;
        public long Offset => _time.Offset;
        public long OffsetStart => _time.OffsetStart; 

        public bool IsPaused => _time.IsPaused;

        private long _pauseStartTime;
        //private long _pauseOffset;

        /// <summary>
        /// Time passed since the timer started.
        /// </summary>
        public int TotalSeconds => _time.TotalSeconds;
        public long _lastTick = 0;

        /// <summary>
        /// The minutes that have passed
        /// </summary>
        public int Minutes => _time.Minutes;

        /// <summary>
        /// the seconds REMAINING between minutes. 
        /// This is always a value between 0 and 59.
        /// </summary>
        public int Seconds => _time.Seconds;
        public float NormalizedTimeUnclamped { get; private set; }
        public float NormalizedTime => _time.NormalizedTime;


        private Vector2Int _bracket;
        public Vector2Int WindowBracket => _time.RiskBracket;

        private Vector2Int _stormWindow;
        public Vector2Int StormWindow => _time.StormWindow;


        public StormRisk RiskLevel => _time.RiskLevel;

        /// <summary>
        /// An event raised every frame the timer updates.
        /// since the timer only tracks seconds, this is not the recommended event to subscribe to.
        /// </summary>
        private TimerUpdateEvent _updateEvent;

        /// <summary>
        /// An event raised every second the timer ticks. This is the recommended event to subscribe to.
        /// </summary>
        private TimerTickEvent _tickEvent;

        /// <summary>
        /// An event raised when the risk bracket changes, currently hardcoded
        /// </summary>
        private RiskLevelEvent _riskLevelEvent;

        private TimerPauseEvent _pauseEvent;

        private void Awake()
        {
            // we don't need to be fast. could probably even lower it.
            Application.targetFrameRate = 60;
            _riskLevelEvent = new() { Time = _time };
            _updateEvent = new() { Time = _time };
            _tickEvent = new() { Time = _time };
            _pauseEvent = new() { Time = _time };

            _time.Start = Now;
            _time.RiskLevel = StormRisk.Low;

            EventBus.Subscribe<TimerRequestEvent>(OnRequestEvent);
            EventBus.Subscribe<CancelStormEvent>(OnCancelRecentSighting);
            
            // todo: save last sighted time in prefs, and load it.
            // if our next session is within 45 minutes of it, then we can pick up where we left off.
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TimerRequestEvent>(OnRequestEvent);
            EventBus.Unsubscribe<CancelStormEvent>(OnCancelRecentSighting);
        }
         
        
        /// <summary>
        /// Starts the timer over by matching start time to the current time.
        /// </summary>
        public void ResetTimer(bool pause = false)
        {
            _time.Start = Now;
            _time.Offset = 0;
            Update();
            if (pause) SetPaused(pause);
            else ClearPauseStatus();
            EventBus.RaiseEvent(_tickEvent);
        }

        /// <summary>
        /// Pauses the timer and tracks the pause start time,
        /// so as time progresses we can offset it by how long we've been paused,
        /// and still use real time values in our data.
        /// </summary>        
        public void SetPaused(bool paused)
        {
            if (paused != IsPaused)
            {
                _time.IsPaused = paused;
                if (paused)
                {
                    // track the time we started pausing.
                    _pauseStartTime = Now;
                }
                else
                {
                    // use the time we started pausing to update the tracked time.
                    _time.Offset += Now - _pauseStartTime;
                }
                EventBus.RaiseEvent(_pauseEvent);
            }
        }

        /// <summary>
        /// Adjusts the timer by a number of seconds, storing it in the offset data.
        /// </summary>
        public void AddTimerOffset(long seconds)
        {
            // don't offset anythin if we're paused.
            if (IsPaused) return; 

#if UNITY_EDITOR
            // in the editor to quickly debug risk brackets we make big offset jumps.
            _time.Offset += (long)(seconds * _debugMultiplier);
#else
            _time.Offset += seconds;
#endif
            Update();
        }


        private void Update()
        {
            if (!IsPaused)
            {
                UpdateTime();
                UpdateBrackets();
                EventBus.RaiseEvent(_updateEvent);
            }
        }

        /// <summary>
        /// Updates all time and normalized time values, called every frame.
        /// </summary>
        void UpdateTime()
        {
            
            _time.TotalSeconds = (int)(Now - OffsetStart);
            _time.Minutes = TotalSeconds / 60;
            _time.Seconds = TotalSeconds % 60;

            // 0..1 where 1 is an hour. we use this for tracking the fill value of the visual risk meter.
            NormalizedTimeUnclamped = TotalSeconds / 3600f;
            _time.NormalizedTime = Mathf.Clamp01(NormalizedTimeUnclamped);

            if (TotalSeconds != _lastTick)
            {
                // if it's been a second, let everything that cares about time know.
                EventBus.RaiseEvent(_tickEvent);
            }

            _lastTick = TotalSeconds;
        }

        /// <summary>
        /// Updates tracked risk brackets / storm windows for display and events.
        /// </summary>
        private void UpdateBrackets()
        {
            StormRisk prevRisk = RiskLevel;

            // if we don't have a recent storm, we have no idea when our first storm is going to be.
            // So we assume any time between 0 and 60 minutes. Soon we will at least check against 
            // the last sighting saved in prefs, but for now we just don't know.
            if (!Blackboard.Get(out RecentStorm recent) || recent.Data == null)
            {
                _stormWindow[0] = 0;
                _stormWindow[1] = 60 - Minutes;
                _bracket[0] = 0;
                _bracket[1] = 60;
                _time.RiskLevel = StormRisk.Unknown;
            }
            else
            {
                // if we have a storm, then we use our timer to determine where we are in the window to the next one.

                _stormWindow[0] = Mathf.Max(0, Mathf.CeilToInt(45 - Minutes));
                _stormWindow[1] = Mathf.Max(0, Mathf.CeilToInt(60 - Minutes));

                _bracket = Vector2Int.up * 15;
                Vector2Int shift = Vector2Int.one;
                // there's probably a way to do this concisely and mathematically but i'm tired.
                // also brackets and risk could move to a separate script from the raw timer but meh.
                if (_stormWindow[0] > 15)
                {
                    shift *= 45;
                    _time.RiskLevel = StormRisk.Low;
                }
                else if (_stormWindow[0] > 5)
                {
                    shift *= 15;
                    _time.RiskLevel = StormRisk.Moderate;
                }
                else if (_stormWindow[0] > 0)
                {
                    shift *= 5;
                    _time.RiskLevel = StormRisk.High;
                }
                else if (_stormWindow[1] > 10)
                {
                    shift *= 0;
                    _time.RiskLevel = StormRisk.Extreme;
                }
                else if (_stormWindow[1] > 5)
                {
                    shift = Vector2Int.up * -5;
                    _time.RiskLevel = StormRisk.Extremer;
                }
                else if (_stormWindow[1] > 0)
                {
                    shift = Vector2Int.up * -10;
                    _time.RiskLevel = StormRisk.Extremest;
                }
                else
                {
                    // at some point we can probably make a 'grace' window
                    // where if you miss a sighting, it can determine you have 
                    // a minimum of 30 minutes of safety or something.
                    // for now if you run out the clock and didn't sight it,
                    // we have no idea what's going on.
                    _bracket = Vector2Int.up * 60;
                    _time.RiskLevel = StormRisk.Unknown;
                }

                if (_time.RiskLevel != StormRisk.Unknown) _bracket += shift;
            }
            
            _time.RiskBracket = _bracket;
            _time.StormWindow = _stormWindow;
            _time.LastStormRisk = prevRisk;
            if (prevRisk != RiskLevel)
            {
                EventBus.RaiseEvent(_riskLevelEvent);
            }

        }

        /// <summary>
        /// Handles a number of timer request event types, 
        /// providing or setting values in response.
        /// </summary>
        void OnRequestEvent(TimerRequestEvent evt)
        {
            switch (evt)
            {
                case TimerUpdateRequest updateRequest: Update(); break;
                case TimerResetRequest resetRequest: ResetTimer(resetRequest.Pause); break;
                case TimerPauseRequest pauseRequest:
                    if (IsPaused && pauseRequest.Unpause) SetPaused(false);
                    else if (!IsPaused && pauseRequest.Pause) SetPaused(true);
                    break;
                    
                case TimerOffsetRequest adjustRequest: AddTimerOffset(adjustRequest.Offset); break;
                case TimerGetTimeRequest getTimeRequest: getTimeRequest.Time = _time; break;
            }
        }

        
        void OnCancelRecentSighting(CancelStormEvent evt)
        {
            RecentStorm recentStorm;
            if (Blackboard.Get(out recentStorm))
            {
                if (recentStorm.Data != null)
                {
                    // assuming there is a sighting to cancel, 
                    // we restore its start time and offset so the timer can continue as though we didn't reset.
                    _time.Offset = recentStorm.Data.TimerOffset;
                    _time.Start = recentStorm.Data.TimerStart;
                    ClearPauseStatus();
                }
            }
        }


        /// <summary>
        /// Unpauses the timer but also resets the pause time
        /// so there is no offset applied.
        /// </summary>
        private void ClearPauseStatus()
        {
            _pauseStartTime = Now;
            SetPaused(false);
        }

        /// <summary>
        /// a privately managed time data class allowing the timer to set values,
        /// while protecting them through the interface.
        /// </summary>
        private class TimeData : ITimeData
        {
            public bool IsPaused { get; set; }
            public long Now => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            public long Start { get; set; }
            public long Offset { get; set; }
            public long OffsetStart => Start + Offset;
            public int TotalSeconds { get; set; }
            public int Minutes { get; set; }
            public int Seconds { get; set; } 
            public float NormalizedTime { get; set; }
            public StormRisk LastStormRisk { get; set; }
            public StormRisk RiskLevel { get; set; }
            public Vector2Int StormWindow { get; set; }
            public Vector2Int RiskBracket { get; set; }

            public bool Stale { get; set; }
        }
    }


    public interface ITimeData : IDataObject
    {
        bool IsPaused { get; }
        long Now { get; }
        long Start { get; }
        long OffsetStart { get; }
        long Offset { get; }
        int TotalSeconds { get; }
        int Minutes { get; }
        int Seconds { get; }
        float NormalizedTime { get; }
        StormRisk RiskLevel { get; }
        StormRisk LastStormRisk { get; }
        Vector2Int StormWindow { get; }
        Vector2Int RiskBracket { get; }
        bool Stale { get; }
    }

}


// q // cD // d
// Unity 6000.0.41f1
