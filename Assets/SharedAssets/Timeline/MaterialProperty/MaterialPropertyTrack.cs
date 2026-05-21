using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Timeline.Samples
{
    [TrackColor(0.855f, 0.455f, 0.157f)]
    [TrackClipType(typeof(MaterialPropertyClip))]
    [TrackBindingType(typeof(Image))]
    public class MaterialPropertyTrack : TrackAsset
    {
        [Tooltip("Shader property name to animate (e.g. _Alpha, _Dissolve)")]
        public string propertyName = "_Alpha";

        [Tooltip("Float animates a float property, Color animates a color property")]
        public MaterialPropertyType propertyType = MaterialPropertyType.Float;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var scriptPlayable = ScriptPlayable<MaterialPropertyMixerBehaviour>.Create(graph, inputCount);
            var mixer = scriptPlayable.GetBehaviour();
            mixer.propertyName = propertyName;
            mixer.propertyType = propertyType;
            return scriptPlayable;
        }
    }
}
