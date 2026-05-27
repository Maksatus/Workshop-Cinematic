using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    [System.Serializable]
    public sealed class FusePanelActivationLink
    {
        [SerializeField] private GameObject activationObject;

        public GameObject ActivationObject => activationObject;
    }
}
