
namespace LorePath.Stormsign
{
    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Cancels the currently tracked storm timer, resetting it to zero 
    /// and removing the storm from the database. The previous storm will be loaded,
    /// and the timer will continue from where it left off.
    /// </summary>
    public class UICancelButton : IUILongButton
    {

        public override void OnClickComplete()
        {            
            RecentStorm currentStorm;
            if (Blackboard.Get(out currentStorm) && currentStorm.Data != null)
            {
                // if we have a storm to cancel the sighting for, we remove it from the database.
                // note that we call the cancel event first, so the recent storm data can be used by the timer,
                // only then do we remove it completely.
                EventBus.RaiseEvent(new CancelStormEvent());
                EventBus.RaiseEvent(new DataChangeRequest() { Storm = currentStorm.Data, Add = false });
                
                // since we're canceling a storm sighting, we restore the previous storm as the most recent one.
                EventBus.RaiseEvent(new DataRefreshRecentStormRequest());
            }
        }
    }

}


// q // cD // d
// Unity 6000.0.41f1
