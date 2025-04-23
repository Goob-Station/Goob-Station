using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Content.Server._Goobstation.DropPod;
using Content.Server.Camera;
using Content.Server.Decals;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;
using Content.Shared.Body.Components;
using Content.Shared.Database;
using Content.Shared.Decals;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.Tag;
using Content.Shared.Timing;
using Robust.Server.GameObjects;
using Robust.Server.Physics;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Player;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    [Dependency] private readonly GridFixtureSystem _gridFixture = default!;
    [Dependency] private readonly CameraRecoilSystem _recoil = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public static SoundSpecifier CloseSound = new SoundCollectionSpecifier("Explosion");

    public static SoundSpecifier FarSound = new SoundCollectionSpecifier("ExplosionFar");

    public const int CloseDroppodSoundRange = 15;

    public const int FarDroppodSoundRange = 35;

    public const string DroppodSmokeTag = "DroppodSmokePos";

    public void InitializePod()
    {
        SubscribeLocalEvent<DropPodGridComponent, DoFTLArrivingOverrideEvent>(OnFTLOverride);
        SubscribeLocalEvent<DropPodGridComponent, FTLCompletedEvent>(OnFTLCompleted);
    }

    private void OnFTLOverride(Entity<DropPodGridComponent> ent, ref DoFTLArrivingOverrideEvent args)
    {
        if (args.Handled)
            return;

        args.PlaySound = false;
        args.Handled = true;
        if (!ent.Comp.TargetCoords.HasValue)
            return;

        _transform.SetMapCoordinates(ent, _transform.ToMapCoordinates(ent.Comp.TargetCoords.Value));
        _transform.SetWorldRotation(ent, Transform(ent.Comp.TargetCoords.Value.EntityId).WorldRotation);
    }

    private void OnFTLCompleted(Entity<DropPodGridComponent> ent, ref FTLCompletedEvent args)
    {
        if (ent.Comp.TargetCoords.HasValue)
            MergeGridsTargeted(ent.Owner, ent.Comp.TargetCoords.Value.EntityId, ent.Comp.TargetCoords.Value);
    }

    private void MergeGridsTargeted(Entity<MapGridComponent?> from, Entity<MapGridComponent?> to, EntityCoordinates targetCoords)
    {
        if (!Resolve(from.Owner, ref from.Comp) || !Resolve(to.Owner, ref to.Comp))
            return;

        var smoke = Transform(from).ChildEnumerator;
        var smokeList = new List<EntityUid>();
        while (smoke.MoveNext(out var uid))
        {
            if (_tag.HasTag(uid, DroppodSmokeTag))
                smokeList.Add(uid);
        }

        CrashEntities(from, to, targetCoords.Position);
        _gridFixture.Merge(to, from, targetCoords.Position.Floored(), Angle.Zero);

        smokeList.ForEach(x => Spawn("Smoke", Transform(x).Coordinates));
    }

    private void CrashEntities(Entity<MapGridComponent?, FixturesComponent?> from, Entity<MapGridComponent?> grid, Vector2 relative)
    {
        if (!Resolve(from, ref from.Comp1, ref from.Comp2) || !Resolve(grid, ref grid.Comp))
            return;

        var gibList = new List<EntityUid>();
        var delList = new List<EntityUid>();
        var xform = Transform(from);
        var targXform = Transform(grid);

        if (!targXform.MapUid.HasValue)
            return;

        var tilePositions = _mapSystem.GetAllTiles(from.Owner, from.Comp1).Select(x => x.GridIndices + relative.Floored());
        var tiles = _mapSystem.GetAllTiles(grid, grid.Comp).Where(x => tilePositions.Contains(x.GridIndices)).Select(x => x.GridIndices);

        foreach (var ent in _lookup.GetLocalEntitiesIntersecting(grid, tiles))
        {
            if (HasComp<BodyComponent>(ent))
                gibList.Add(ent);
            else
                delList.Add(ent);
        }

        gibList.ForEach(x =>
        {
            _logger.Add(LogType.Gib, LogImpact.Extreme, $"{ToPrettyString(x):player} got gibbed by the shuttle" +
                                                        $" {ToPrettyString(x)} arriving from FTL at {Transform(x).Coordinates:coordinates}");
            _bobby.GibBody(x);
        });

        delList.ForEach(x => QueueDel(x));

        var pos = new EntityCoordinates(grid, relative);

        var filter = Filter.Pvs(pos).AddInRange(_transform.ToMapCoordinates(pos), CloseDroppodSoundRange).AddInGrid(from);
        var audio = _audio.PlayStatic(CloseSound, filter, pos, true);

        if (audio != null)
        {
            audio.Value.Component.Flags |= AudioFlags.NoOcclusion;
            Dirty(audio.Value.Entity, audio.Value.Component);
        }

        var farFilter = Filter.Empty().AddInRange(_transform.ToMapCoordinates(pos), FarDroppodSoundRange).RemoveInRange(_transform.ToMapCoordinates(pos), CloseDroppodSoundRange);
        _audio.PlayGlobal(FarSound, farFilter, true);

        foreach (var player in farFilter.Recipients.Union(filter.Recipients))
        {
            if (!player.AttachedEntity.HasValue)
                continue;

            var uid = player.AttachedEntity.Value;
            var playerXform = Transform(uid);
            if (playerXform.MapUid != Transform(grid).MapUid)
                continue;

            if (!playerXform.Coordinates.TryDistance(EntityManager, pos, out var distance) || distance > FarDroppodSoundRange)
                continue;

            _recoil.KickCamera(uid, Vector2.Clamp((playerXform.Coordinates.Position - relative).Normalized() / (FarDroppodSoundRange / distance), new(-4f), new(4f)));
        }
    }
}
