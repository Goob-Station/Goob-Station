// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 OnsenCapy <101037138+OnsenCapy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 OnsenCapy <lucasgrds166@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpellCardsActionComponent : Component
{
    /// <summary>
    /// How many times the spell can be casted without cooldown resetting
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int UsesLeft = 6;

    /// <summary>
    /// Max uses for this spell before it's cooldown is reset
    /// </summary>
    [DataField]
    public int CastAmount = 6;

    /// <summary>
    /// This determines spell use delay, not action component
    /// </summary>
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(6f);

    /// <summary>
    /// Whether the next spell card burst will be purple or red
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PurpleCard = false;
}