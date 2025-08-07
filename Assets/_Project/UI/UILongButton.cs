
namespace LorePath.Stormsign
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// a base class for buttons that should be held for a length of time.
    /// </summary>
    public abstract class IUILongButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("How long to hold the button down before it actually works")]
        [SerializeField] private float _holdDuration = 1f;

        [SerializeField] private Image _holdMeter;

        private bool _isClicked;
        private float _clickTimer;

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (_isClicked && _clickTimer < _holdDuration)
            {
                OnClickCancel();
                OnClickEnd();
            }
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {            
            OnClickStart();
        }

        protected virtual void Update()
        {
            if (_isClicked)
            {
                _clickTimer += Time.deltaTime;
                _holdMeter.fillAmount = _clickTimer / _holdDuration;
                if (_clickTimer >= _holdDuration)
                {
                    OnClickComplete();
                    OnClickEnd();
                }
            }
        }

        public virtual void OnClickStart() 
        {
            _isClicked = true;
            _clickTimer = 0f;
            _holdMeter.fillAmount = 0f;
        }

        /// <summary>
        /// Called when the click ends before it completes.
        /// </summary>
        public virtual void OnClickCancel() { }

        /// <summary>
        /// Called when the click is held long enough to complete.
        /// </summary>
        public abstract void OnClickComplete();

        /// <summary>
        /// Called if the click ends for any reason, whether complete or canceled.
        /// </summary>
        public virtual void OnClickEnd() 
        {
            _isClicked = false;
            _clickTimer = 0f;
            _holdMeter.fillAmount = 0f;
        }

        
        
    }

}


// q // cD // d
// Unity 6000.0.41f1
