
namespace LorePath.Stormsign
{
    using UnityEngine;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>    
    /// Sends a file save request on detecting a save-worthy event, if the toggle is on,
    /// otherwise saves when the save button is clicked.
    /// </summary>
    public class UISaveButtonHandler : MonoBehaviour
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _manualImage;
        private bool _isDirty;

        private void Awake()
        {
            EventBus.Subscribe<ISavableEvent>(OnSavableEvent);
            _manualImage.color = Color.green;
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<ISavableEvent>(OnSavableEvent);
        }


        void OnSavableEvent(ISavableEvent evt) 
        { 
            if (_toggle.isOn)
            {
                EventBus.RaiseEvent(new DataSaveRequest());
                _manualImage.color = Color.green;
            }
            else
            {
                _isDirty = true;
                _manualImage.color = Color.yellow;
            }
        }

        public void OnClickSaveButton()
        {
            if (_isDirty)
            {
                EventBus.RaiseEvent(new DataSaveRequest());
                _manualImage.color = Color.green;
                _isDirty = false;
            }
        }

    }

}


// q // cD // d
// Unity 6000.0.41f1
