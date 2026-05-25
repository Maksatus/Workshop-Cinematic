using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.9f, 0.55f, 0.1f)]
[TrackClipType(typeof(CursorFollowClip))]
[TrackBindingType(typeof(CursorFollower))]
public class CursorFollowTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<CursorFollowMixerBehaviour>.Create(graph, inputCount);
    }
}
