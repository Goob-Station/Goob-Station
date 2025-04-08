// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Trevor Day <tday93@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Dice;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedDiceSystem))]
[AutoGenerateComponentState(true)]
public sealed partial class DiceComponent : Component
{
    [DataField]
    public SoundSpecifier Sound { get; private set; } = new SoundCollectionSpecifier("Dice");

    /// <summary>
    ///     Multiplier for the value  of a die. Applied after the <see cref="Offset"/>.
    /// </summary>
    [DataField]
    public int Multiplier { get; private set; } = 1;

    /// <summary>
    ///     Quantity that is subtracted from the value of a die. Can be used to make dice that start at "0". Applied
    ///     before the <see cref="Multiplier"/>
    /// </summary>
    [DataField]
    public int Offset { get; private set; } = 0;

    [DataField]
    public int Sides { get; private set; } = 20;

    /// <summary>
    ///     The currently displayed value.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public int CurrentValue { get; set; } = 20;

}