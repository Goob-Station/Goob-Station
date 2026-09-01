// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class SwapOnProjectileHitComponent : Component
{
    [DataField]
    public SoundSpecifier? Sound;

    [DataField]
    public EntProtoId Effect = "SwapSpellEffect";

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public bool DeleteProjectileOnSwap;
}