namespace LorePath.Stormsign
{
    //using LorePath;
    //using DG.Tweening;


    // ideally we'd make the Time value readonly but I don't feel like writing a bunch of constructors right now
    public class TimerEvent : IEventData
    {
        public ITimeData Time;
    }

    /// <summary>
    /// implemented by events that should trigger a potential autosave.
    /// </summary>
    public interface ISavableEvent : IEventData { }

    public class TimerUpdateEvent : TimerEvent { }
    public class TimerPauseEvent : TimerEvent { }
    public class TimerResetEvent : TimerEvent { }
    public class TimerTickEvent : TimerEvent { }

    public class RiskLevelEvent : TimerEvent { } // probably shouldn't be in timer.

    
    // --- requests 

    public class TimerRequestEvent : IEventData { }

    /// <summary>
    /// A special request that populates itself with the current time data on receipt.
    /// </summary>
    public class TimerGetTimeRequest : TimerRequestEvent
    {
        public ITimeData Time;
    }

    public class TimerUpdateRequest : TimerRequestEvent { }

    public class TimerResetRequest : TimerRequestEvent 
    {
        public bool Pause;
    }

    public class TimerPauseRequest : TimerRequestEvent 
    {
        public bool Pause;
        public bool Unpause;
    }

    public class TimerOffsetRequest : TimerRequestEvent 
    {
        public long Offset;
    }

    /// <summary>
    /// Raised when we want to resume tracking the most recent storm timer, unsighting
    /// </summary>
    public class TimerRestoreRecentTimer : TimerRequestEvent { }


    // --- data
    
    public interface IRecentStormEvent : IEventData { }
    public interface IRecentStormChangeOrUpdateEvent : ISavableEvent, IRecentStormEvent { }

    public class DatabaseEvent : IEventData{ }
   
    public class  DataSaveEvent : DatabaseEvent
    {
        public readonly bool Success;
        public DataSaveEvent(bool success)
        {
            Success = success;
        }
    }


    /// <summary>
    /// Raised when the Latest Storm data changes,
    /// either because the storm data itself is different,
    /// OR its members have updated values.
    /// useful for guaranteeing most up-to-date values are displayed.
    /// </summary>
    public class RecentStormUpdateEvent : IRecentStormChangeOrUpdateEvent { }


    /// <summary>
    /// Raised when specifically the current storm reference changes.
    /// </summary>
    public class RecentStormChangeEvent : DataChangeEvent, IRecentStormChangeOrUpdateEvent { }


    public class CancelStormEvent : IRecentStormEvent { }



    /// <summary>
    /// Raised when the storm database changes due to a storm
    /// being added or removed. This doesn't get raised for 
    /// editing the values of existing storms.
    /// </summary>
    public class  DataChangeEvent : DatabaseEvent { }



    // --- requests
    public class DatabaseRequestEvent : IEventData
    {
        
    }

    /// <summary>
    /// Raised when we want to either add or remove a storm from the database.
    /// </summary>
    public class DataChangeRequest : DatabaseRequestEvent
    {
        public StormData Storm;
        public bool Add;
    }

    /// <summary>
    /// Raised when we want the database to load the most recent storm as current.
    /// By default, if the current storm is nullified, the database doesn't load another to change, for data safety.
    /// This event is used to load the most recent storm as current when we are canceling a sighting.
    /// </summary>
    public class DataRefreshRecentStormRequest : DatabaseRequestEvent { }

    /// <summary>
    /// Raised when the database should be written to file.
    /// </summary>
    public class DataSaveRequest : DatabaseRequestEvent { }


    /// <summary>
    /// Raised when an audio clip should be played based on the signal being sent
    /// </summary>
    public class RequestAudioEvent : IEventData
    {
        /// <summary>
        ///  The signal to use, defines default clip and volume
        /// </summary>
        public AudioSignal Signal = AudioSignal.None;

        /// <summary>
        /// An optional clip. if null, the default library will be used
        /// </summary>
        public UnityEngine.AudioClip Clip = null;

        /// <summary>
        /// a volume multiplier for the event, this is applied
        /// separately from mute, main volume, and signal volume.
        /// </summary>
        public float Volume = 1f;

        public RequestAudioEvent() { }
        public RequestAudioEvent(AudioSignal signal) { Signal = signal; }
    }



}


// q // cD // d
// Unity 6000.0.41f1
