using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Core.Inventory.Scripts
{
    [TrackColor(0.15f, 0.85f, 0.55f)]
    [TrackClipType(typeof(CursorBezierClip))]
    [TrackBindingType(typeof(RectTransform))]
    public class CursorBezierTrack : TrackAsset
    {
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var rect = director.GetGenericBinding(this) as RectTransform;
            if (rect != null)
            {
                driver.AddFromName<RectTransform>(rect.gameObject, "m_AnchoredPosition");
                driver.AddFromName<RectTransform>(rect.gameObject, "m_LocalPosition");
            }
            base.GatherProperties(director, driver);
        }
    }
}
