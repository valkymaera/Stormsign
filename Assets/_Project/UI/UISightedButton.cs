
namespace LorePath.Stormsign
{
    using UnityEngine;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Raises the events to update the app when clicking the 'storm sighted' button.
    /// This ultimately resets the timer, adds a new storm to the database, and clears the map
    /// </summary>
    public class UISightedButton : IUILongButton
    {

        private DataChangeRequest _addStormRequest = new();
        private TimerResetRequest _resetRequest = new();
        private TimerGetTimeRequest _getTimeRequest = new();

        public override void OnClickComplete()
        {         
            // when we sight a new storm, we create the new storm data and apply the most up to date time data.
            StormData storm = new();
            EventBus.RaiseEvent(_getTimeRequest);
            storm.SpawnTime = _getTimeRequest.Time.Now;
            storm.TimerStart = _getTimeRequest.Time.Start;
            storm.TimerOffset = _getTimeRequest.Time.Offset;
            storm.Version = Application.version;

            // we add the storm to the database. This will automatically set it as the most recent storm
            _addStormRequest.Storm = storm;
            _addStormRequest.Add = true;
            EventBus.RaiseEvent(_addStormRequest);
            
            // then we reset the timer so it begins ticking toward the next storm window.
            EventBus.RaiseEvent(_resetRequest);

        }
    
    }

}


// q // cD // d
// Unity 6000.0.41f1
