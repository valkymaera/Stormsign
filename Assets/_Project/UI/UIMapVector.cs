
namespace LorePath.Stormsign
{
       
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.AI;
    using TMPro;
    using UnityEngine.EventSystems;

    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;

    //using LorePath;
    //using DG.Tweening;

    /// <summary>
    /// Handles visualizing and saving the storm's directional vector, 
    /// based on interacting with the map image.
    /// </summary>
    public class UIMapVector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _mapRect;
        [SerializeField] private RectTransform _vectorRect;
        [SerializeField] private RectTransform _firstClickMarker;
        [SerializeField] private bool _closeOnEscape;

        private bool _leftDown, _rightDown;


        void Awake()
        {
            EventBus.Subscribe<IRecentStormChangeOrUpdateEvent>(OnStormDataChange);
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe<IRecentStormChangeOrUpdateEvent>(OnStormDataChange);
        }

        void OnStormDataChange(IRecentStormChangeOrUpdateEvent evt)
        {
            // load vectors
            RecentStorm recentStorm;
            if (Blackboard.Get(out recentStorm) && recentStorm.Data != null)
            {
                UpdateVector(recentStorm.Data.StartCoord, recentStorm.Data.EndCoord);
            }
            else UpdateVector(Vector3.zero, Vector3.zero);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (MapLock.IsLocked) { return; }

            if (eventData.button == PointerEventData.InputButton.Left) _leftDown = true;
            if (eventData.button == PointerEventData.InputButton.Right) _rightDown = true;
        }

        public void OnPointerUp(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) _leftDown = false;
            if (data.button == PointerEventData.InputButton.Right) _rightDown = false;
            if (!_leftDown && !_rightDown) EventBus.RaiseEvent(new RecentStormUpdateEvent());
        }

        void Update()
        {
            if (_leftDown) UpdateCoordinate(Input.mousePosition, isStart: true);
            if (_rightDown) UpdateCoordinate(Input.mousePosition, isStart: false);
            if (Input.GetKeyDown(KeyCode.Escape) && _closeOnEscape)
            {
                var group = GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 0f;
                    group.blocksRaycasts = false;
                    group.interactable = false;
                }
            }
        }

        void UpdateCoordinate(Vector3 position, bool isStart)
        {            
            RecentStorm latestStorm;
            if (Blackboard.Get(out latestStorm) && latestStorm.Data != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_mapRect, position, null, out Vector2 point);
                position -= _mapRect.position;
                position.x = (position.x / (_mapRect.rect.width * _mapRect.localScale.x));
                position.y = (position.y / (_mapRect.rect.height * _mapRect.localScale.y));
                
                point.x /= _mapRect.rect.width;
                point.y /= _mapRect.rect.height;

                if (isStart) latestStorm.Data.StartCoord = point;
                else latestStorm.Data.EndCoord = point;
                UpdateVector(latestStorm.Data.StartCoord, latestStorm.Data.EndCoord);
            }
         
        }


        void UpdateVector(Vector3 start, Vector3 end)
        {
            Vector3 vector = end - start;            
            Vector3 midpoint = (start + (vector/2f)) * _mapRect.rect.size; // note: this requires the map pivot to be in the lower left, not center.

            if (end.sqrMagnitude > 0.0001f && start.sqrMagnitude > 0.0001f)
            {
                _vectorRect.gameObject.SetActive(true);
                _firstClickMarker.gameObject.SetActive(false);
                _vectorRect.sizeDelta = new Vector2(vector.magnitude * _mapRect.rect.width, 12f);
                _vectorRect.anchoredPosition = midpoint;
                _vectorRect.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.Cross(Vector3.forward, vector.normalized));
            }
            else
            {
                _vectorRect.gameObject.SetActive(false);
                if (start.sqrMagnitude > 0.0001f)
                {
                    _firstClickMarker.gameObject.SetActive(true);
                    Vector3 clickPoint = start * _mapRect.rect.size;
                    _firstClickMarker.anchoredPosition = clickPoint;
                }

            }
            
        }



    }

}


// q // cD // d
// Unity 6000.0.41f1
