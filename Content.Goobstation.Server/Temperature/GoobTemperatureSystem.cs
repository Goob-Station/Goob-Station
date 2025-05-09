// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;

namespace Content.Goobstation.Server.Temperature;

public sealed partial class GoobTemperatureSystem : EntitySystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;

    /// <summary>
    /// Gives all firestacks from one entity to another
    /// Prioritizes transfering from the user to the target first.
    /// </summary>
    public void TransferFireStacks(EntityUid user, EntityUid target)
    {
        if (!TryComp<FlammableComponent>(user, out var userFlammable) || !TryComp<FlammableComponent>(target, out var targetFlammable))
            return;

        if (userFlammable.FireStacks > 0 && targetFlammable.FireStacks < targetFlammable.MaximumFireStacks)
        {
            _flammable.AdjustFireStacks(target, userFlammable.FireStacks, ignite: true);
            _flammable.AdjustFireStacks(user, -userFlammable.FireStacks, ignite: true);
        }
        else if (targetFlammable.FireStacks > 0 && userFlammable.FireStacks < userFlammable.MaximumFireStacks)
        {
            _flammable.AdjustFireStacks(user, targetFlammable.FireStacks, ignite: true);
            _flammable.AdjustFireStacks(target, -targetFlammable.FireStacks, ignite: true);
        }
    }
}
