// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Destructible.Thresholds;
using Content.Shared.Whitelist;

namespace Content.Server._Goobstation.Wizard.Components;

[RegisterComponent]
public sealed partial class RandomTeleportOnProjectileHitComponent : Component
{
    [DataField]
    public EntityWhitelist Whitelist = new();
}