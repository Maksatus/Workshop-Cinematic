using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Core.Inventory.Scripts
{
    [TrackColor(0.6f, 0.2f, 0.85f)]
    [TrackClipType(typeof(TMPAnimateClip))]
    [TrackBindingType(typeof(TMP_Text))]
    public class TMPAnimateTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<TMPAnimateMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var label = director.GetGenericBinding(this) as TMP_Text;
            if (label != null)
            {
                driver.AddFromName<TMP_Text>(label.gameObject, "m_text");
                driver.AddFromName<TMP_Text>(label.gameObject, "m_fontColor");
            }
            base.GatherProperties(director, driver);
        }
    }
}
