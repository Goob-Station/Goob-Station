// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.Components;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Content.Shared.Inventory;

namespace Content.Server.Heretic.EntitySystems;

public sealed class VoidCloakSystem : SharedVoidCloakSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InventoryComponent, GetPressureProtectionValuesEvent>(OnGetPressureProtectionValues);
    }

    private void OnGetPressureProtectionValues(Entity<InventoryComponent> ent, ref GetPressureProtectionValuesEvent args)
    {
        if (!_inventory.TryGetSlotEntity(ent, "outerClothing", out var entity, ent.Comp))
            return;

        if (!TryComp(entity, out VoidCloakComponent? cloak) || cloak.Transparent)
            return;

        args.LowPressureMultiplier *= 1000f;
    }

    protected override void UpdatePressureProtection(EntityUid cloak, bool enabled)
    {
        base.UpdatePressureProtection(cloak, enabled);

        // This updates pressure protection in barotrauma system
        if (enabled)
            EnsureComp<PressureProtectionComponent>(cloak);
        else
            RemCompDeferred<PressureProtectionComponent>(cloak);
    }
}