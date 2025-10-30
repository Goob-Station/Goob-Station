// SPDX-FileCopyrightText: 2024 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2024 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Magic.Components;

// TODO: Rename to MagicActionComponent or MagicRequirementsComponent
[RegisterComponent, NetworkedComponent, Access(typeof(SharedMagicSystem))]
public sealed partial class MagicComponent : Component
{
    // TODO: Split into different components?
    // This could be the MagicRequirementsComp - which just is requirements for the spell
    // Magic comp could be on the actual entities itself
    //  Could handle lifetime, ignore caster, etc?
    // Magic caster comp would be on the caster, used for what I'm not sure

    // TODO: Do After here or in actions

    // TODO: Spell requirements
    //  A list of requirements to cast the spell
    //    Hands
    //    Any item in hand
    //    Spell takes up an inhand slot
    //      May be an action toggle or something

    // TODO: List requirements in action desc
    /// <summary>
    ///     Does this spell require Wizard Robes & Hat?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool RequiresClothes;

    /// <summary>
    ///     Does this spell require the user to speak?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool RequiresSpeech;

    // TODO: FreeHand - should check if toggleable action
    //  Check which hand is free to toggle action in

    // Goobstation
    [DataField]
    public MagicSchool School = MagicSchool.Unset;

    // Goobstation
    [DataField]
    public bool BlockedBySpectral = true;
}

public enum MagicSchool : byte // Goobstation
{
    Unset,
    Holy,
    Psychic,
    Mime,
    Restoration,
    Evocation,
    Explosion,
    Transmutation,
    Translocation,
    Conjuration,
    Necromancy,
    Forbidden,
    Sanguine,
    Chuuni, // Specifically for chuuni invocations spell
}
