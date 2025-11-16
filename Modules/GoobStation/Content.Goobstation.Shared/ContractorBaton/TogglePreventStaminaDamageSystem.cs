// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Events;
using Content.Shared.Item.ItemToggle;

namespace Content.Goobstation.Shared.ContractorBaton;

public sealed class TogglePreventStaminaDamageSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TogglePreventStaminaDamageComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
    }

    private void OnStaminaHitAttempt(Entity<TogglePreventStaminaDamageComponent> ent,
        ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!_toggle.IsActivated(ent.Owner))
            args.Cancelled = true;
    }
}