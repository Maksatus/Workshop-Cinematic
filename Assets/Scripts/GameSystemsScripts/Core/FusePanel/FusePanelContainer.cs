using System.Collections.Generic;
using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelContainer : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private FusePanelActivationLink activationLink;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private FuseItemContainer fuseItemPrefab;
        [SerializeField] private List<FuseSlotContainer> slotViews = new List<FuseSlotContainer>();
        [SerializeField] private List<FuseItemContainer> fuseViews = new List<FuseItemContainer>();
        [SerializeField] private List<FuseSourceContainer> sourceContainers = new List<FuseSourceContainer>();

        public GameObject PanelRoot => panelRoot;
        public FusePanelActivationLink ActivationLink => activationLink;
        public Camera InputCamera => inputCamera;
        public LayerMask InteractionMask => interactionMask;
        public FuseItemContainer FuseItemPrefab => fuseItemPrefab;
        public IReadOnlyList<FuseSlotContainer> SlotViews => slotViews;
        public IReadOnlyList<FuseItemContainer> FuseViews => fuseViews;
        public IReadOnlyList<FuseSourceContainer> SourceContainers => sourceContainers;
    }
}
