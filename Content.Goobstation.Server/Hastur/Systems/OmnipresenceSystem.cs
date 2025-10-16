using Content.Goobstation.Shared.Hastur.Components;
using Content.Goobstation.Shared.Hastur.Events;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Goobstation.Shared.Hastur.Systems;

public sealed class OmnipresenceSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

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
            var clone = Spawn(comp.CloneProto, coords);
            affectedCenters.Add(clone);

            // Schedule clone deletion
            var deleteTime = _timing.CurTime + TimeSpan.FromSeconds(comp.CloneLifetime);
            Timer.Spawn(TimeSpan.FromSeconds(comp.CloneLifetime), () =>
            {
                if (Exists(clone))
                    QueueDel(clone);
            });
        }

        // Perform AOE stun around each source
        foreach (var source in affectedCenters)
        {
            DoAoEStun(source, comp.StunRange, comp.StunDuration);
        }

        _popup.PopupEntity(Loc.GetString("hastur-omnipresence-activate"), uid, uid);
        args.Handled = true;
    }

    private void DoAoEStun(EntityUid center, float range, float duration)
    {
        var nearby = _lookup.GetEntitiesInRange(center, range);

        foreach (var target in nearby)
        {
            if (target == center)
                continue;

            _stun.TryKnockdown(target, TimeSpan.FromSeconds(duration), true);
        }
    }
}
