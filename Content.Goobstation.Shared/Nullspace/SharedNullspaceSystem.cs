using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Nullspace;

public abstract partial class SharedNullspaceSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private SharedAudioSystem _sharedAudio = default!;

    [SubscribeLocalEvent]
    private void OnPlayerStartup(Entity<NullspacePlayerComponent> ent, ref ComponentStartup args)
    {
        _actionBlocker.UpdateCanMove(ent);
    }

    [SubscribeLocalEvent]
    private void OnCanMove(Entity<NullspacePlayerComponent> ent, ref UpdateCanMoveEvent args)
    {
        if (ent.Comp.LifeStage <= ComponentLifeStage.Running)
            args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnTileFriction(Entity<NullspacePlayerComponent> ent, ref TileFrictionEvent args)
    {
        args.Modifier = 0f;
    }

    public NullspaceMaze GetMaze(NullspaceMapComponent map)
    {
        if (map.Maze == null || map.Maze.Seed != (uint) map.Seed)
            map.Maze = new NullspaceMaze((uint) map.Seed);

        if (map.Maze.AppliedVariantVersion != map.VariantVersion)
        {
            foreach (var (chunk, variant) in map.ChunkVariants)
            {
                map.Maze.SetVariant(chunk, variant);
            }

            map.Maze.AppliedVariantVersion = map.VariantVersion;
        }

        return map.Maze;
    }

    public bool TryGetMap(EntityUid uid, out Entity<NullspaceMapComponent> map)
    {
        map = default;
        if (Transform(uid).MapUid is not { } mapUid || !TryComp<NullspaceMapComponent>(mapUid, out var comp))
            return false;

        map = (mapUid, comp);
        return true;
    }

    public void ApplyReverb((EntityUid Entity, AudioComponent Component)? audio)
    {
        if (audio is not { } playing || !_sharedAudio.Auxiliaries.TryGetValue(NullspaceAudio.Reverb, out var aux))
            return;

        _sharedAudio.SetAuxiliary(playing.Entity, playing.Component, aux);
    }

    public void Unoccluded((EntityUid Entity, AudioComponent Component)? audio)
    {
        if (audio is not { } playing)
            return;

        playing.Component.Flags |= AudioFlags.NoOcclusion;
        Dirty(playing.Entity, playing.Component);
    }

    public float GameTime(NullspaceMapComponent map, TimeSpan now)
    {
        return map.StartTime is { } start ? (float) (now - start).TotalSeconds : 0f;
    }

    public static Vector2i CellOf(System.Numerics.Vector2 tiles)
    {
        return new Vector2i((int) MathF.Floor(tiles.X), (int) MathF.Floor(tiles.Y));
    }

    public static System.Numerics.Vector2 CellCenter(Vector2i cell)
    {
        return new System.Numerics.Vector2(cell.X + 0.5f, cell.Y + 0.5f) * NullspaceParams.Cell;
    }

    public static EntityCoordinates ToCoordinates(EntityUid map, System.Numerics.Vector2 meters)
    {
        return new EntityCoordinates(map, meters / NullspaceParams.Cell);
    }
}
