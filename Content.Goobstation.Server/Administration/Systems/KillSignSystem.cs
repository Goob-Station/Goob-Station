// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Goobstation.Common.Administration.Components;
using Content.Shared.Movement.Components;

namespace Content.Goobstation.Server.Administration.Systems;

public sealed class KillSignSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<SignOnHitComponent, MeleeHitEvent>(OnSignMeleeHit);
    }

    // Goobstation Change
    private void OnSignMeleeHit(EntityUid uid, SignOnHitComponent component, MeleeHitEvent args)
    {
        if (args.HitEntities.Count < 1)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<InputMoverComponent>(hit)
                || HasComp<KillSignComponent>(hit))
                continue;

            var sign = new KillSignComponent
            {
                SignSprite = component.SignSprite,
            };

            AddComp(hit, sign);
        }
    }
}