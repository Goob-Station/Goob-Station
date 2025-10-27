// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;

namespace Content.Goobstation.Server.Weapons;

/// <summary>
/// Gib this Person
/// </summary>
public sealed class GibThisGuySystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GibThisGuyComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public void OnMeleeHit(EntityUid uid, GibThisGuyComponent component, MeleeHitEvent args)
    {
        if (component.RequireBoth)
        {
            foreach (var hit in args.HitEntities)
                if (component.IcNames.Contains(Name(hit)) &&
                    TryComp<ActorComponent>(hit, out var actor) &&
                    component.OcNames.Contains(actor.PlayerSession.Name))
                    _bodySystem.GibBody(hit);
            return;
        }
        foreach (var hit in args.HitEntities)
        {
            if (component.IcNames.Contains(Name(hit)))
                _bodySystem.GibBody(hit);

            if (TryComp<ActorComponent>(hit, out var actor) &&
                component.OcNames.Contains(actor.PlayerSession.Name))
                _bodySystem.GibBody(hit);
        }
    }
}