// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Speech;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Speech;

/// <summary>
/// Marks clothing that change wearer speech sound (for example - human talking like borg when wearing borg head (just example))
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeechSoundsReplacerComponent : Component
{
    /// <summary>
    /// A substitute sound
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public ProtoId<SpeechSoundsPrototype>? SpeechSounds;

    /// <summary>
    /// Previous sound that returns when you unequip clothing
    /// </summary>
    [DataField]
    public ProtoId<SpeechSoundsPrototype>? PreviousSound;
}