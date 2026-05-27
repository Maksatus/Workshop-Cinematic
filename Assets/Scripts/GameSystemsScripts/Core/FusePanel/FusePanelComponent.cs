using System.Collections.Generic;
using UnityEngine;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class FusePanelComponent
    {
        public readonly List<FuseSlotSpec> Slots = new List<FuseSlotSpec>();
        public readonly Dictionary<string, FuseItemSpec> FusesById = new Dictionary<string, FuseItemSpec>();
        public readonly Dictionary<string, FuseSourceContainer> ContainersById = new Dictionary<string, FuseSourceContainer>();

        public FusePanelPhase Phase = FusePanelPhase.Inactive;
        public FusePanelResult LastResult = FusePanelResult.RetryRequired;
        public bool IsCompleted;
        public bool WasActivated;
        public bool IsGenerationReady;
        public bool IsGenerationFailed;
        public int ActivationAttemptId;
        public GeneratedPanelState CurrentGeneratedState;

        public FuseItemContainer DraggedFuse;
        public Transform DragOriginalParent;
        public Vector3 DragOriginalPosition;

        public void ResetDragState()
        {
            DraggedFuse = null;
            DragOriginalParent = null;
            DragOriginalPosition = Vector3.zero;
        }
    }
}
