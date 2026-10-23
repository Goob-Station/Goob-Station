// SPDX-License-Identifier: AGPL-3.0-or-later

// <Trauma>
using Robust.Shared.Map;
using System.Numerics;
// </Trauma>
using Content.Shared.Actions;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class ActionGunSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionGunComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ActionGunComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ActionGunComponent, ActionGunShootEvent>(OnShoot);
    }

    private void OnMapInit(Entity<ActionGunComponent> ent, ref MapInitEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.Action))
            return;

        // <Trauma> - parent the gun to the action so it gets networked and shooting can be predicted
        if (!_actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action))
            return;

        var coords = new EntityCoordinates(ent.Comp.ActionEntity.Value, Vector2.Zero);
        ent.Comp.Gun = Spawn(ent.Comp.GunProto, coords);
        Dirty(ent);
        // </Trauma>
    }

    private void OnShutdown(Entity<ActionGunComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Gun is { } gun)
            PredictedQueueDel(gun); // Trauma - predicted
        _actions.RemoveAction(ent.Comp.ActionEntity); // Trauma - don't leave the action behind
    }

    private void OnShoot(Entity<ActionGunComponent> ent, ref ActionGunShootEvent args)
    {
        if (TryComp<GunComponent>(ent.Comp.Gun, out var gun))
            _gun.AttemptShoot(ent, (ent.Comp.Gun.Value, gun), args.Target);
    }
}
