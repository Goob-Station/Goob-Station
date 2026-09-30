// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.NPC.HTN;
using Content.Goobstation.Shared.Xenobiology.Systems;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// Primarily handle anything that need to be in serverside like HTN
/// </summary>
public sealed partial class XenoVacuumSystem : SharedXenoVacuumSystem
{
    [Dependency] private readonly HTNSystem _htn = default!;

    protected override void SetHTNEnabled(EntityUid uid, bool enabled, float planCooldown)
    {
        _htn.SetHTNEnabled(uid, enabled, planCooldown);
    }
}
