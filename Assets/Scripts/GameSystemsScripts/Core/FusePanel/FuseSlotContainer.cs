using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FuseSlotContainer : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private int requiredVoltage = 12;

        public string SlotId => slotId;
        public int RequiredVoltage => requiredVoltage;

        public void SetRequiredVoltage(int value)
        {
            requiredVoltage = value;
        }
    }
}
