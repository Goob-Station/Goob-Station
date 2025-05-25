// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.GameStates;
namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     Randomly spawns gas of a given type.
/// </summary>
[RegisterComponent]
public sealed partial class ExpelGasComponent : Component
{
    public List<Gas> PossibleGases = new()
    {
        Gas.Oxygen,
        Gas.Plasma,
        Gas.Nitrogen,
        Gas.CarbonDioxide,
        Gas.Tritium,
        Gas.Ammonia,
        Gas.NitrousOxide,
        Gas.Frezon,
        Gas.BZ, ///tg/ gases
        Gas.Healium, ///tg/ gases
        Gas.Nitrium, ///tg/ gases
    };
}