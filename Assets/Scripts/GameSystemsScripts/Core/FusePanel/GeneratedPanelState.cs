using System;
using System.Collections.Generic;

namespace GameSystemsScripts.Core.FusePanel
{
    public sealed class GeneratedPanelState
    {
        public readonly List<GeneratedSlotState> Slots = new List<GeneratedSlotState>();
        public int ActivationAttemptId;
        public DateTime GeneratedAtUtc;
    }
}
