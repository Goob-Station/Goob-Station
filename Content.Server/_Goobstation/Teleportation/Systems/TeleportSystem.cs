using System.Numerics;
using Content.Server.Administration.Logs;
using Content.Server.Stack;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics;
using Robust.Shared.Random;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Maps;
using Content.Shared.Tiles;
using Robust.Shared.Map.Components;
using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Server.Teleportation;

public sealed class TeleportSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly IAdminLogManager _alog = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomTeleportOnUseComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(EntityUid uid, RandomTeleportOnUseComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        var sound = _random.Pick(_prot.Index<SoundCollectionPrototype>(component.TeleportSounds).PickFiles);

        _audio.PlayPvs(new SoundPathSpecifier(sound), Transform(uid).Coordinates, AudioParams.Default);

        RandomTeleport(args.User, component);

        // play before and after teleport
        // TODO: replace spark sounds with an actual spark system that makes spark particles go off.
        _audio.PlayPvs(new SoundPathSpecifier(sound), Transform(uid).Coordinates, AudioParams.Default);

        if (component.ConsumeOnUse)
        {
            if (TryComp<StackComponent>(uid, out var stack))
            {
                _stack.SetCount(uid, stack.Count - 1, stack);
                return;
            }

            // It's consumed on use and it's not a stack so delete it
            QueueDel(uid);
        }

        _alog.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):actor} randomly teleported with {ToPrettyString(uid)}");
    }

    public void RandomTeleport(EntityUid uid, RandomTeleportComponent component)
    {
        RandomTeleport(uid, component.Radius);
    }

    public void RandomTeleport(EntityUid uid, MinMax radius)
    {
        var xform = Transform(uid);
        var localpos = xform.Coordinates.Position;

        // if we teleport the pulled entity goes with us
        EntityUid? pullableEntity = null;
        if (TryComp<PullerComponent>(uid, out var puller))
            pullableEntity = puller.Pulling;

        // break any active pulls e.g. secoff pulling you with cuffs
        if (TryComp<PullableComponent>(uid, out var pullable) && _pullingSystem.IsPulled(uid, pullable))
            _pullingSystem.TryStopPull(uid, pullable);

        var tiles = new List<TileRef>();
        // find a random good** position for teleportation (which isn't space or a wall)
        if (TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            var max = new Vector2(radius.Max, radius.Max);
            var box = new Box2(localpos - max, localpos + max);

            tiles = _map.GetLocalTilesIntersecting(uid, grid, box)
                .Where(t => !t.Tile.IsEmpty) // NO SPACE
                .Where(t => t.GetEntitiesInTile(LookupFlags.Static, _lookup).ToList().Count == 0) // prevent teleporting into a wall
                .Where(t => (t.GridIndices - localpos).Length() >= radius.Min) // no less than minimal radius
                .ToList();
        }

        if (tiles.Count() == 0)
        {
            // just teleport randomly
            var distance = _random.Next(radius.Min, radius.Max);
            var targetCoords = _xform.ToMapCoordinates(xform.Coordinates).Offset(_random.NextAngle().ToVec() * distance);
            _xform.SetWorldPosition(uid, targetCoords.Position);
        }
        else
        {
            var tile = _random.Pick(tiles.ToList());
            _xform.SetLocalPosition(uid, tile.GridIndices, xform);
        }

        // pulled entity goes with us
        if (pullableEntity != null)
        {
            _xform.SetWorldPosition((EntityUid) pullableEntity, _xform.GetWorldPosition(uid));
        }
    }
}
