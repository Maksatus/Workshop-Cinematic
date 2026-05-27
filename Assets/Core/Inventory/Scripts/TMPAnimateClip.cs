using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Core.Inventory.Scripts
{
    [Serializable]
    public class TMPAnimateClip : PlayableAsset, ITimelineClipAsset
    {
        [Header("Текст")]
        [TextArea(2, 6)]
        public string text = "";

        [Header("Цвет / прозрачность")]
        public Color color = Color.white;

        // Blending включён — можно делать плавные переходы перекрыванием клипов
        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TMPAnimateBehaviour>.Create(graph);
            var b = playable.GetBehaviour();
            b.text  = text;
            b.color = color;
            return playable;
        }
    }
}
