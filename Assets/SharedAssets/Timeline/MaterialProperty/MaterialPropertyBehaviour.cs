using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Timeline.Samples
{
    [Serializable]
    public class MaterialPropertyBehaviour : PlayableBehaviour
    {
        [Tooltip("Float value to set on the material property")]
        public float floatValue = 1f;

        [Tooltip("Color value to set on the material property")]
        public Color colorValue = Color.white;
    }
}
