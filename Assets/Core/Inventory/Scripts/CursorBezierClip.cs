// CursorBezierClip
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Core.Inventory.Scripts
{
    [Serializable]
    public class CursorBezierClip : PlayableAsset, ITimelineClipAsset
    {
        [Header("Конечная точка (anchored position)")]
        public Vector2 targetPosition;

        [Header("Изгиб дуги (смещение в canvas-пикселях)")]
        [Tooltip("(0,0) = прямая линия. (0,200) = дуга вверх. (200,0) = дуга вправо.")]
        public Vector2 bend = new Vector2(0f, 120f);

        [Header("Настройки")]
        public AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CursorBezierBehaviour>.Create(graph);
            var b = playable.GetBehaviour();
            b.targetPosition = targetPosition;
            b.bend           = bend;
            b.easing         = easing;
            return playable;
        }
    }
}
