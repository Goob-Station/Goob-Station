// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Religion;

[RegisterComponent, NetworkedComponent]
public sealed partial class DivineInterventionComponent : Component
{
    /// <summary>
    /// Which sound to play on spell denial.
    /// </summary>
    [DataField]
    public SoundSpecifier DenialSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

    /// <summary>
    /// Which effect to display.
    /// </summary>
    [DataField]
    public EntProtoId EffectProto = "EffectSpark";

    /// <summary>
    /// Which loc string to display.
    /// </summary>
    [DataField]
    public LocId DenialString = "nullrod-spelldenial-popup";

    /// <summary>
    /// Valid inventory slots for spell denial when equipped
    /// </summary>
    [DataField]
    public SlotFlags ValidSpellDenialSlots = SlotFlags.NONE;
}
