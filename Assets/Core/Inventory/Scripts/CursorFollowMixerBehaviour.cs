using UnityEngine;
using UnityEngine.Playables;

public class CursorFollowMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        var follower = playerData as CursorFollower;
        if (follower == null) return;

        follower.target = null;

        int inputCount = playable.GetInputCount();
        for (int i = 0; i < inputCount; i++)
        {
            if (playable.GetInputWeight(i) > 0f)
            {
                var inputPlayable = (ScriptPlayable<CursorFollowBehaviour>)playable.GetInput(i);
                follower.target = inputPlayable.GetBehaviour().target;
                break;
            }
        }
    }
}
