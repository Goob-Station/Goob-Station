// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Wizard;
using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Heretic.Systems;

public sealed class HereticClothingSystem : EntitySystem
{
    [Dependency] private readonly SharedHereticSystem _heretic = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticClothingComponent, BeingEquippedAttemptEvent>(OnEquipAttempt);
    }

    private void OnEquipAttempt(Entity<HereticClothingComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (IsTargetValid(args.EquipTarget) && (args.EquipTarget == args.Equipee || IsTargetValid(args.Equipee)))
            return;

        args.Cancel();
        args.Reason = Loc.GetString("heretic-clothing-component-fail");
    }

    private bool IsTargetValid(EntityUid target)
    {
        return _heretic.IsHereticOrGhoul(target) || HasComp<WizardComponent>(target) ||
               HasComp<ApprenticeComponent>(target);
    }
}
