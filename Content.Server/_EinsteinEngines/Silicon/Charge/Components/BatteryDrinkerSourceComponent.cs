// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Server._EinsteinEngines.Silicon.Charge;

[RegisterComponent]
public sealed partial class BatteryDrinkerSourceComponent : Component
{
    /// <summary>
    ///     The max amount of power this source can provide in one sip.
    ///     No limit if null.
    /// </summary>
    [DataField]
    public int? MaxAmount = null;

    /// <summary>
    ///     The multiplier for the drink speed.
    /// </summary>
    [DataField]
    public float DrinkSpeedMulti = 1f;

    /// <summary>
    ///     The sound to play when the battery gets drunk from.
    /// </summary>
    [DataField]
    public SoundSpecifier? DrinkSound = new SoundCollectionSpecifier("sparks");
}