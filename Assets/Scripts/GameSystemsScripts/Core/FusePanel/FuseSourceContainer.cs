using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FuseSourceContainer : MonoBehaviour
    {
        [SerializeField] private string containerId;
        [SerializeField] private Transform spawnPoint;

        public string ContainerId => containerId;
        public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;
    }
}
