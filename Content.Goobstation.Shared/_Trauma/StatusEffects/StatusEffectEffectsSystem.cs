// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared._Trauma.StatusEffects;

public sealed partial class StatusEffectEffectsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private SharedEntityEffectsSystem _effects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectEffectsComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StatusEffectEffectsComponent, StatusEffectComponent>();
        var me = _player.LocalEntity;
        var now = _timing.CurTime;
        while (query.MoveNext(out var uid, out var comp, out var status))
        {
            if (status.AppliedTo is not {} target ||
                // don't predict other clients' effects
                _net.IsClient && target != me ||
                now < comp.NextUpdate)
                continue;

            comp.NextUpdate = now + comp.UpdateDelay;
            Dirty(uid, comp);

            _effects.ApplyEffects(target, comp.Effects, user: target);
        }
    }

    private void OnMapInit(Entity<StatusEffectEffectsComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdate = _timing.CurTime + ent.Comp.UpdateDelay;
        Dirty(ent);
    }
}
