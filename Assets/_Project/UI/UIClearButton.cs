
namespace LorePath.Stormsign
{
    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Resets and pauses the timer without modifying sighted storm data.
    /// useful if the current timer isn't actually known or reliable,
    /// as the analyzer will treat zero time as unknown / unreliable
    /// </summary>
    public class UIClearButton : IUILongButton
    {

        public override void OnClickComplete()
        {

            // since we're just clearing the current wait timer, 
            // we don't do anything to the recent storm sighting. We assume that really was sighted.
            // instead we just reset the timer. We pause it if it's not paused, or unpause it if it's paused.
            TimerGetTimeRequest timeRequest = new();
            EventBus.RaiseEvent(timeRequest);
            bool pause = !timeRequest.Time.IsPaused;
           EventBus.RaiseEvent(new TimerResetRequest() { Pause = pause});
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
