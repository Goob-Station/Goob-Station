// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class NinjutsuSneakAttackComponent : Component
{
    [DataField]
    public float Multiplier = 2f;

    [DataField]
    public float AssassinateModifier = 180f;

    [DataField]
    public float AssassinateUnarmedModifier = 115f;

    [DataField]
    public float AssassinateArmorPierce = -2.5f;

    [DataField]
    public float TakedownSlowdownTime = 5f;

    [DataField]
    public float TakedownMuteTime = 10f;

    [DataField]
    public float TakedownSpeedModifier = 0.2f;

    [DataField]
    public float TakedownBackstabMultiplier = 1.5f;

    [DataField]
    public SoundSpecifier AssassinateSoundUnarmed = new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg");

    [DataField]
    public SoundSpecifier AssassinateSoundArmed =
            new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/guillotine.ogg");

    // This should be LocId but combos names don't use locale anyway
    [DataField]
    public string AssassinateComboName = "Assassinate";

    [DataField]
    public string TakedownComboName = "Ninjutsu Takedown";

    [DataField]
    public ProtoId<AlertPrototype> Alert = "SneakAttack";
}
