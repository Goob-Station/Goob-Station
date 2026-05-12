// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Coordinates;
using Content.Shared.EntityEffects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Trauma.Shared.Bouncing;

public sealed class BouncingSystem : EntitySystem
{
    [Dependency] private readonly SharedEntityEffectsSystem _effects = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BounceableComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<BounceableComponent> ent, ref StartCollideEvent args)
    {
        if (!HasComp<BounceableComponent>(args.OtherEntity))
            return;

        var curTime = _timing.CurTime;

        if (ent.Comp.NextValidBounceTime > curTime)
            return;

        ent.Comp.NextValidBounceTime = curTime + ent.Comp.GraceTime;
        ent.Comp.TimesBounced++;

        if (ent.Owner.Id < args.OtherEntity.Id) // Only need to do effects and stuff on one chicken + it looks weird when both do it
            return;

        _audio.PlayPredicted(ent.Comp.BounceSound, ent.Owner, ent.Owner);
        _effects.ApplyEffects(ent.Owner, ent.Comp.Effects);

        if (ent.Comp.TimesBounced < ent.Comp.BouncesRequired)
            return;

        ent.Comp.TimesBounced = 0;

        if (_net.IsServer)
            SpawnAtPosition(ent.Comp.EntityToSpawn, ent.Owner.ToCoordinates());
    }
}
