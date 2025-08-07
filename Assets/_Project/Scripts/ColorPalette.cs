
namespace LorePath.Stormsign
{
    using System.Collections.Generic;
    using UnityEngine;


    [CreateAssetMenu(fileName = "ColorPalette", menuName = "Scriptable Objects/ColorPalette")]
    public class ColorPalette : ScriptableObject
    {
        [field: SerializeField] public List<Color> Colors { get; private set; }
    }
}
