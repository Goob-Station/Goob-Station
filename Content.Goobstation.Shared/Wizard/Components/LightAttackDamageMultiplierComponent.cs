// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wizard.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LightAttackDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;

    [DataField]
    public SoundSpecifier? ExtraSound;
}