
namespace LorePath.Stormsign
{
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// A lock toggle that prevents interaction with the minimap.
    /// This helps to prevent accidentally modifying a storm sighting    
    /// </summary>
    public class MapLock : MonoBehaviour
    {

        [SerializeField] private Image _lockedImage;
        [SerializeField] private Image _unlockedImage;
        [SerializeField] private Slider _qualitySlider;
        public static bool IsLocked { get; private set; }


        private void Awake()
        {
            EventBus.Subscribe<DataChangeRequest>(this.OnDataChange);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DataChangeRequest>(this.OnDataChange);
        }

        /// <summary>
        /// Unlocks the map when a new storm is sighted
        /// </summary>        
        void OnDataChange(DataChangeRequest evt)
        {
            if (IsLocked && evt.Storm != null && evt.Add)
            {
                OnClick();
            }
        }

        public void OnClick()
        {
            IsLocked = !IsLocked;
            _lockedImage.enabled = IsLocked;
            _unlockedImage.enabled = !IsLocked;
            _qualitySlider.interactable = !IsLocked;
        }

    }

}


// q // cD // d
// Unity 6000.0.41f1
