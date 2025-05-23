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
            var transferable = MathF.Min(userFlammable.FireStacks, targetFlammable.MaximumFireStacks - targetFlammable.FireStacks);

            _flammable.AdjustFireStacks(target, transferable, ignite: true);
            _flammable.AdjustFireStacks(user, -transferable, ignite: true);
        }
        else if (targetFlammable.FireStacks > 0 && userFlammable.FireStacks < userFlammable.MaximumFireStacks)
        {
            var transferable = MathF.Min(targetFlammable.FireStacks, userFlammable.MaximumFireStacks - userFlammable.FireStacks);

            _flammable.AdjustFireStacks(user, transferable, ignite: true);
            _flammable.AdjustFireStacks(target, -transferable, ignite: true);
        }
    }
}
