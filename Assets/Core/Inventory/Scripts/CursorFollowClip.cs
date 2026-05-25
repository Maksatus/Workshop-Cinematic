using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CursorFollowClip : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<Transform> target;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CursorFollowBehaviour>.Create(graph);
        playable.GetBehaviour().target = target.Resolve(graph.GetResolver());
        return playable;
    }
}
