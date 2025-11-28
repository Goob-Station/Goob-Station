using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Goobstation.Shared.Hastur.Systems;

public sealed class OmnipresenceSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly INetManager _net = default!;

    private readonly HashSet<Entity<MobStateComponent>> _mobCache = new();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OmnipresenceComponent, OmnipresenceEvent>(OnOmnipresence);
    }

    private void OnOmnipresence(Entity<OmnipresenceComponent> ent, ref OmnipresenceEvent args)
    {
        if (args.Handled)
            return;

        var uid = ent.Owner;
        var comp = ent.Comp;
        var xform = Transform(uid);

        // Diagonal offsets
        var offsets = new (float x, float y)[]
        {
            (comp.CloneDistance, comp.CloneDistance),
            (-comp.CloneDistance, comp.CloneDistance),
            (comp.CloneDistance, -comp.CloneDistance),
            (-comp.CloneDistance, -comp.CloneDistance)
        };

        var affectedCenters = new List<EntityUid> { uid };

        foreach (var (x, y) in offsets)
        {
            var coords = xform.Coordinates.Offset(new Vector2(x, y));
            if (_net.IsServer)
            {
                var clone = Spawn(comp.CloneProto, coords);
                affectedCenters.Add(clone);
            }
        }

        // Perform AOE stun around each source
        foreach (var source in affectedCenters)
        {
            DoAoEStun(source, comp.StunRange, comp.StunDuration);
        }

        _popup.PopupPredicted(Loc.GetString("hastur-omnipresence-activate"), uid, uid);
        args.Handled = true;
    }

    private void DoAoEStun(EntityUid center, float range, float duration)
    {
        _mobCache.Clear();

        var centerCoords = Transform(center).Coordinates; // get coordinates
        _lookup.GetEntitiesInRange(centerCoords, range, _mobCache);

        foreach (var mob in _mobCache)
        {
            if (mob.Owner == center)
                continue;

            _stun.TryKnockdown(mob.Owner, TimeSpan.FromSeconds(duration), true);
        }
    }
}
