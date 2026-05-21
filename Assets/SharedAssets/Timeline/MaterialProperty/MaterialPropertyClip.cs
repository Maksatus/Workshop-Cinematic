using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Timeline.Samples
{
    [Serializable]
    public class MaterialPropertyClip : PlayableAsset, ITimelineClipAsset
    {
        public MaterialPropertyBehaviour template = new MaterialPropertyBehaviour();

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<MaterialPropertyBehaviour>.Create(graph, template);
        }
    }
}
