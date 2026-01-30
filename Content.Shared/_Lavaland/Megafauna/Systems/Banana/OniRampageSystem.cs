using Content.Shared._Lavaland.Megafauna.Components.Banana;
using Content.Shared._Lavaland.Megafauna.Events.Banana;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;

namespace Content.Shared._Lavaland.Megafauna.Systems.Banana;

/// <summary>
/// This system handles jumping towards a target, dealing damage in AoE, spawning a ring of flames and optionally playing a voiceline.
/// </summary>
public sealed class OniRampageSystem : EntitySystem
{
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OniRampageComponent, OniRampageEvent>(OnRampage);
        SubscribeLocalEvent<OniRampageComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<OniRampageComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnRampage(Entity<OniRampageComponent> ent, ref OniRampageEvent args)
    {

        if (args.Handled)
            return;

        var comp = ent.Comp;

        if (comp.IsLeaping)
            return;

        comp.IsLeaping = true;
        Dirty(ent);

        // Clamp jump distance
        var origin = Transform(args.Performer).Coordinates;
        var direction = args.Target.Position - origin.Position;
        direction = direction.Normalized() * comp.JumpDistance;

        var destination = origin.Offset(direction);

        _throwing.TryThrow(
            args.Performer,
            destination,
            comp.JumpThrowSpeed
        );

        if (comp.ShouldSpeak && _net.IsServer)
        {
            _chat.TrySendInGameICMessage(
                args.Performer,
                Loc.GetString(comp.Speech),
                InGameICChatType.Speak,
                hideChat: false
            );
        }

        args.Handled = true;
    }

    private void OnLand(Entity<OniRampageComponent> ent, ref LandEvent args)
    {
        var comp = ent.Comp;
        comp.IsLeaping = false;
        Dirty(ent);

        var uid = ent.Owner;
        var coords = Transform(uid).Coordinates;

        // AoE damage
        var range = comp.LandDamageRange;
        var targets = new HashSet<EntityUid>();

        _lookup.GetEntitiesInRange(coords, range, targets);

        foreach (var target in targets)
        {
            if (target == uid)
                continue;

            if (!HasComp<DamageableComponent>(target))
                continue;

            _damage.TryChangeDamage(target, comp.Damage, true);
        }

        // Fire ring
        SpawnFireRing(uid, comp, range: (int) MathF.Ceiling(range));

        // Sound
        if (comp.ShockwaveSound != null)
            _audio.PlayPvs(comp.ShockwaveSound, uid);
    }

    private void SpawnFireRing(EntityUid center, OniRampageComponent comp, int range)
    {
        var xform = Transform(center);

        if (!xform.GridUid.HasValue)
            return;

        if (!_xform.TryGetGridTilePosition(center, out var tilePos))
            return;

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) xform.GridUid, grid);

        var centerPos = _map.TileCenterToVector(gridEnt, tilePos);
        var outer = new Box2(centerPos, centerPos).Enlarged(range);
        var inner = new Box2(centerPos, centerPos).Enlarged(range - 1);

        var outerTiles = _map.GetLocalTilesIntersecting(center, grid, outer);
        var innerTiles = range > 1
            ? new HashSet<TileRef>(_map.GetLocalTilesIntersecting(center, grid, inner))
            : new HashSet<TileRef>();

        foreach (var tile in outerTiles)
        {
            if (innerTiles.Contains(tile))
                continue;

            Spawn(comp.FirePrototype, _map.GridTileToWorld(gridEnt.Item1, grid, tile.GridIndices));
        }
    }
    private void OnStopThrow(Entity<OniRampageComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.IsLeaping = false;
        Dirty(ent);
    }


}
