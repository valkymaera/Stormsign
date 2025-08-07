
namespace LorePath.Stormsign
{
       
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.AI;
    using TMPro;

    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;

    //using LorePath;
    //using DG.Tweening;

    public class UIOffsetButton : MonoBehaviour
    {
        [SerializeField] private int _offsetSeconds = 30;
        public void OnClick()
        {
            EventBus.RaiseEvent(new TimerOffsetRequest() { Offset = _offsetSeconds });
        }    
    }

}


// q // cD // d
// Unity 6000.0.41f1
