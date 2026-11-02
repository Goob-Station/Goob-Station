using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Weapons.MeleeDash;

public sealed partial class DashSlamSystem : EntitySystem
{
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private INetManager _net = default!;

    private readonly HashSet<Entity<MobStateComponent>> _targets = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DashSlamComponent, DashLandedEvent>(OnDashLanded);
    }

    private void OnDashLanded(Entity<DashSlamComponent> ent, ref DashLandedEvent args)
    {
        if (!_net.IsServer)
            return;

        _targets.Clear();
        _lookup.GetEntitiesInRange(Transform(args.User).Coordinates, ent.Comp.Range, _targets);

        foreach (var target in _targets)
        {
            if (target.Owner == args.User)
                continue;

            _stun.TryKnockdown(target.Owner, ent.Comp.KnockdownTime);
        }

        if (ent.Comp.SlamSound is not null)
        {
            _audio.PlayPvs(ent.Comp.SlamSound, args.User);
        }
    }
}
