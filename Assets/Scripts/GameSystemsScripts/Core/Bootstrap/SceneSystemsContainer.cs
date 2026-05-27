using GameSystemsScripts.Core.FusePanel;
using UnityEngine;

namespace GameSystemsScripts.Core.Bootstrap
{
    public sealed class SceneSystemsContainer : MonoBehaviour
    {
        [SerializeField] private FusePanelContainer fusePanelContainer;

        public FusePanelContainer FusePanelContainer => fusePanelContainer;
    }
}
