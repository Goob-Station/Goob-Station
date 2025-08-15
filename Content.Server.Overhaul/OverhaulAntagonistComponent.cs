using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Server.Overhaul
{
    [RegisterComponent]
    public sealed class OverhaulAntagonistComponent : Component
    {
        [ViewVariables] public bool HasCorpseBuff { get; set; }
        [ViewVariables] public int CorpseFusionStacks { get; set; } = 0;
        [ViewVariables] public TimeSpan LastAbilityUse { get; set; } = TimeSpan.Zero;
    }
}