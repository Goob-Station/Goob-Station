// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Content.Shared.Movement.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Shared.Movement.Systems;

public sealed class AutoOrientSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;

    private TimeSpan _delay = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AutoOrientComponent, EntParentChangedMessage>(OnEntParentChanged);

        Subs.CVar(_cfgManager, CCVars.AutoOrientDelay, OnAutoOrient, true);
    }

    private void OnAutoOrient(double obj)
    {
        _delay = TimeSpan.FromSeconds(obj);
    }

    private void OnEntParentChanged(Entity<AutoOrientComponent> ent, ref EntParentChangedMessage args)
    {
        ent.Comp.NextChange = _timing.CurTime + _delay;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AutoOrientComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextChange <= _timing.CurTime)
            {
                comp.NextChange = null;
                Dirty(uid, comp);
                _mover.ResetCamera(uid);
            }
        }
    }
}