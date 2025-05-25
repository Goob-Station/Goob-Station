// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Wizard.Components;
using Content.Shared.Projectiles;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class ProjectileHitShooterAfterDelaySystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ProjectileHitShooterAfterDelayComponent, ProjectileComponent>();
        while (query.MoveNext(out var uid, out var comp, out var projectile))
        {
            comp.Delay -= frameTime;

            if (comp.Delay > 0)
                continue;

            RemCompDeferred(uid, comp);
            projectile.IgnoreShooter = false;
            Dirty(uid, projectile);
        }
    }
}