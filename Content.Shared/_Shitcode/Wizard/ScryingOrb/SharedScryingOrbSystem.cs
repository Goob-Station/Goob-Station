// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Wizard.ScryingOrb;

public abstract class SharedScryingOrbSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    public bool IsScryingOrbEquipped(EntityUid uid)
    {
        var scryingOrbQuery = GetEntityQuery<ScryingOrbComponent>();
        if (_hands.EnumerateHeld(uid).Any(held => scryingOrbQuery.HasComponent(held)))
            return true;

        var enumerator = _inventory.GetSlotEnumerator(uid);
        while (enumerator.MoveNext(out var container))
        {
            if (scryingOrbQuery.HasComp(container.ContainedEntity))
                return true;
        }

        return false;
    }
}