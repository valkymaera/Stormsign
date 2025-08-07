using UnityEngine;
using System.Collections.Generic;

namespace LorePath.Stormsign
{
    [CreateAssetMenu(fileName = "FadePatternSet", menuName = "Scriptable Objects/FadePatternSet")]
    public class FadePatternSet : ScriptableObject
    {
        public List<AnimationCurve> Curves = new();
    }
}
