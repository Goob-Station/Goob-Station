// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Explosion.EntitySystems;
using Content.Shared.IgnitionSource;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.Timing;

namespace Content.Server.Trigger.Systems;

/// <summary>
/// Handles igniting when triggered and stopping ignition after the delay.
/// </summary>
public sealed class IgniteOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedIgnitionSourceSystem _source = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgniteOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    // TODO: move this into ignition source component
    // it already has an update loop
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        var query = EntityQueryEnumerator<IgniteOnTriggerComponent, IgnitionSourceComponent>();
        while (query.MoveNext(out var uid, out var comp, out var source))
        {
            if (!source.Ignited)
                continue;

            if (_timing.CurTime < comp.IgnitedUntil)
                continue;

            _source.SetIgnited((uid, source), false);
        }
    }

    private void OnTrigger(Entity<IgniteOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        _source.SetIgnited(target.Value);
        ent.Comp.IgnitedUntil = _timing.CurTime + ent.Comp.IgnitedTime;
        Dirty(ent);

        args.Handled = true;
    }
}
