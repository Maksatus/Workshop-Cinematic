using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FuseItemContainer : MonoBehaviour
    {
        [SerializeField] private string fuseId;
        [SerializeField] private int voltage = 12;
        [SerializeField] private string color;
        [SerializeField] private string size;
        [SerializeField] private string sourceContainerId;
        [SerializeField] private bool isBurned;
        [SerializeField] private GameObject normalVisual;
        [SerializeField] private GameObject burnedVisual;

        public string FuseId => fuseId;
        public int Voltage => voltage;
        public string Color => color;
        public string Size => size;
        public string SourceContainerId => sourceContainerId;
        public bool IsBurned => isBurned;

        public void SetFuseId(string value)
        {
            fuseId = value;
        }

        public void SetVoltage(int value)
        {
            voltage = value;
        }

        public void SetBurned(bool burned)
        {
            isBurned = burned;
            if (normalVisual != null)
            {
                normalVisual.SetActive(!burned);
            }

            if (burnedVisual != null)
            {
                burnedVisual.SetActive(burned);
            }
        }
    }
}
