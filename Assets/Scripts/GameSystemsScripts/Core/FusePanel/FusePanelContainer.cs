using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelContainer : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject boxRoot;
        [SerializeField] private FusePanelActivationLink activationLink;
        [SerializeField] private Camera inputCamera;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private FuseItemContainer fuseItemPrefab;
        [SerializeField] private SplineAnimate splineAnimation;
        [SerializeField] private Vector3 fuseDragEulerAngles = new Vector3(0f, 0f, 90f);
        [SerializeField] private List<FuseSlotContainer> slotViews = new List<FuseSlotContainer>();
        [SerializeField] private List<FuseItemContainer> fuseViews = new List<FuseItemContainer>();
        [SerializeField] private List<FuseSourceContainer> sourceContainers = new List<FuseSourceContainer>();

        public GameObject PanelRoot => panelRoot;
        public GameObject BoxRoot => boxRoot;
        public FusePanelActivationLink ActivationLink => activationLink;
        public Camera InputCamera => inputCamera;
        public LayerMask InteractionMask => interactionMask;
        public FuseItemContainer FuseItemPrefab => fuseItemPrefab;
        public SplineAnimate SplineAnimation => splineAnimation;
        public Quaternion FuseDragRotation => Quaternion.Euler(fuseDragEulerAngles);
        public IReadOnlyList<FuseSlotContainer> SlotViews => slotViews;
        public IReadOnlyList<FuseItemContainer> FuseViews => fuseViews;
        public IReadOnlyList<FuseSourceContainer> SourceContainers => sourceContainers;
    }
}
