// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._Goobstation.Weapons.UseDelay;

public sealed class UseDelayBlockShootSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UseDelayBlockShootComponent, AttemptShootEvent>(OnShootAttempt);
    }

    private void OnShootAttempt(Entity<UseDelayBlockShootComponent> ent, ref AttemptShootEvent args)
    {
        if (TryComp(ent, out UseDelayComponent? useDelay) && _useDelay.IsDelayed((ent, useDelay)))
            args.Cancelled = true;
    }
}