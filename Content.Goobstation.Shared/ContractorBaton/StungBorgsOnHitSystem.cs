// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Jittering;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.ContractorBaton;

public sealed class StungBorgsOnHitSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StunBorgsOnHitComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<StunBorgsOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!_toggle.IsActivated(ent.Owner))
            return;

        foreach (var borg in args.HitEntities.Where(HasComp<BorgChassisComponent>))
        {
            _stun.TryParalyze(borg, ent.Comp.ParalyzeDuration, true);
            _jitter.DoJitter(borg, ent.Comp.ParalyzeDuration, true);
        }
    }
}