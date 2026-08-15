// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Effects.Step;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryDamageChangeEffectComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public float SleepModifier = 0.5f;

    [DataField]
    public bool IsConsumable;

    [DataField]
    public bool AffectAll;
}