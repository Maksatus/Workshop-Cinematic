using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FuseSlotSpec
    {
        public string SlotId;
        public int RequiredVoltage;
        public Transform SlotTransform;
        public FuseItemContainer CurrentFuseView;
    }
}
