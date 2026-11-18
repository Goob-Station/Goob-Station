using System.Numerics;
using Content.Goobstation.Shared.MouseSwarm;
using Content.Server.Destructible;
using Content.Server.Popups;
using Content.Shared.Damage.Systems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Analyzers;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.MouseSwarm;

public sealed partial class MouseSwarmSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MobThresholdSystem _thresholds = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private readonly List<(EntProtoId Proto, EntityUid Host, MapCoordinates Coords)> _pendingDrops = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_pendingDrops.Count == 0)
            return;

        foreach (var (proto, host, coords) in _pendingDrops)
        {
            var dropped = Exists(host) ? SpawnNextToOrDrop(proto, host) : Spawn(proto, coords);
            _mobState.ChangeMobState(dropped, MobState.Dead);
        }

        _pendingDrops.Clear();
    }

    [SubscribeLocalEvent(before: new[] { typeof(MobThresholdSystem), typeof(DestructibleSystem) })]
    private void OnDamageChanged(Entity<MouseSwarmComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased
            || ent.Comp.Members.Count == 0
            || !_mobState.IsAlive(ent)
            || !TryComp<MobThresholdsComponent>(ent, out var thresholds))
            return;

        if (!_thresholds.TryGetThresholdForState(ent, MobState.Critical, out var limit, thresholds)
            && !_thresholds.TryGetThresholdForState(ent, MobState.Dead, out limit, thresholds))
            return;

        var vital = _thresholds.CheckVitalDamage(ent, args.Damageable);
        if (vital < limit.Value)
            return;

        var killed = (int) MathF.Floor((vital / limit.Value).Float());
        var overflow = killed > ent.Comp.Members.Count;
        killed = Math.Min(killed, ent.Comp.Members.Count);

        var coords = _transform.GetMapCoordinates(ent);
        for (var i = 0; i < killed; i++)
        {
            var member = ent.Comp.Members[^1];
            ent.Comp.Members.RemoveAt(ent.Comp.Members.Count - 1);

            if (member.Prototype is { } proto)
                _pendingDrops.Add((proto, ent.Owner, coords));
        }
        Dirty(ent);

        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            melee.Damage -= ent.Comp.DamagePerMember * (float) killed;
            Dirty(ent, melee);
        }

        if (!overflow)
            _damageable.SetAllDamage((ent, args.Damageable), 0);

        _popup.PopupEntity(Loc.GetString("mouse-swarm-lost"), ent, ent);
    }

    [SubscribeLocalEvent]
    private void OnSwarm(Entity<MouseSwarmComponent> ent, ref MouseSwarmActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;
        if (target == ent.Owner
            || !TryComp<MouseSwarmComponent>(target, out var targetSwarm)
            || HasComp<ActorComponent>(target)
            || _mind.TryGetMind(target, out _, out _)
            || !_mobState.IsAlive(target)
            || _container.IsEntityInContainer(target))
        {
            _popup.PopupEntity(Loc.GetString("mouse-swarm-invalid"), ent, ent);
            return;
        }

        var added = 1 + targetSwarm.Members.Count;
        if (ent.Comp.Members.Count + added > ent.Comp.MaxMembers)
        {
            _popup.PopupEntity(Loc.GetString("mouse-swarm-full"), ent, ent);
            return;
        }

        args.Handled = true;

        ent.Comp.Members.Add(RandomMember(ent.Comp, targetSwarm.Sprite, targetSwarm.State, MetaData(target).EntityPrototype?.ID));
        foreach (var member in targetSwarm.Members)
            ent.Comp.Members.Add(RandomMember(ent.Comp, member.Sprite, member.State, member.Prototype));
        Dirty(ent);

        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
        {
            for (var i = 0; i < added; i++)
                melee.Damage += ent.Comp.DamagePerMember;
            Dirty(ent, melee);
        }

        _popup.PopupEntity(Loc.GetString("mouse-swarm-absorbed", ("target", target)), ent, ent);
        QueueDel(target);
    }

    private MouseSwarmMember RandomMember(MouseSwarmComponent comp, ResPath sprite, string state, EntProtoId? prototype)
    {
        var offset = new Vector2(
            _random.NextFloat(-comp.MaxOffset, comp.MaxOffset),
            _random.NextFloat(-comp.MaxOffset, comp.MaxOffset));
        return new MouseSwarmMember(sprite, state, offset, prototype);
    }
}
