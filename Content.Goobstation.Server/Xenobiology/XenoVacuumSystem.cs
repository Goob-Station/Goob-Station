using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Systems;
using Content.Server.NPC.HTN;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles disabling AI of mobs inside a xenovac.
/// </summary>
public sealed partial class XenoVacuumSystem : SharedXenoVacuumSystem
{
    [Dependency] private HTNSystem _htn = default!;

    // mfw no shared htn system
    protected override void SetHTNEnabled(EntityUid uid, bool enabled, float planCooldown)
    {
        _htn.SetHTNEnabled(uid, enabled, planCooldown);
    }
}
