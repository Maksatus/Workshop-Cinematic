using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Core.Inventory.Scripts
{
    [Serializable]
    public class TMPAnimateBehaviour : PlayableBehaviour
    {
        public string text  = "";
        public Color  color = Color.white;
    }
}
