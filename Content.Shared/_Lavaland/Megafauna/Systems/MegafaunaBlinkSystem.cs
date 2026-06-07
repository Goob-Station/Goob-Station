using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared.Coordinates.Helpers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class MegafaunaBlinkSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<MegafaunaBlinkComponent> _blinkQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaBlinkComponent, MegafaunaBlinkActionEvent>(OnBlinkAction);

        SubscribeLocalEvent<MegafaunaBlinkInactiveComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MegafaunaBlinkInactiveComponent, MegafaunaStartupEvent>(OnStartup);
        SubscribeLocalEvent<MegafaunaBlinkInactiveComponent, MegafaunaShutdownEvent>(OnShutdown);
        SubscribeLocalEvent<MegafaunaBlinkInactiveComponent, EntityTerminatingEvent>(OnDelete);

        _blinkQuery = GetEntityQuery<MegafaunaBlinkComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blinkQuery = EntityQueryEnumerator<MegafaunaActiveBlinkComponent>();
        while (blinkQuery.MoveNext(out var uid, out var blink))
        {
            if (blink.BlinkTime == null
                || _timing.CurTime < blink.BlinkTime)
                continue;

            if (!blink.Coordinates.IsValid(EntityManager))
            {
                RemComp(uid, blink);
                continue;
            }

            _xform.SetCoordinates(uid, blink.Coordinates.SnapToGrid(EntityManager, _mapMan));
            _audio.PlayPredicted(blink.Sound, blink.Coordinates, uid);
            RemComp(uid, blink);
        }
    }

    private void OnBlinkAction(Entity<MegafaunaBlinkComponent> ent, ref MegafaunaBlinkActionEvent args)
    {
        if (args.Handled
            || !args.Target.IsValid(EntityManager))
            return;

        var comp = ent.Comp;
        Blink(ent, args.Target, comp.Delay, comp.Sound);

        if (comp.SpawnOnUsed != null)
            PredictedSpawnAtPosition(comp.SpawnOnUsed.Value, Transform(ent).Coordinates);

        if (comp.SpawnOnTarget != null)
            PredictedSpawnAtPosition(comp.SpawnOnTarget.Value, args.Target);

        args.Handled = true;
    }

    public void Blink(
        EntityUid ent,
        EntityCoordinates coords,
        TimeSpan duration,
        SoundSpecifier? sound = null)
    {
        var blinkComp = EnsureComp<MegafaunaActiveBlinkComponent>(ent);
        blinkComp.BlinkTime = _timing.CurTime + duration;
        blinkComp.Coordinates = coords;
        blinkComp.Sound = sound;
        Dirty(ent, blinkComp);
    }

    public void Blink(
        EntityUid ent,
        EntityUid target,
        TimeSpan duration,
        SoundSpecifier? sound = null)
        => Blink(ent, Transform(target).Coordinates, duration, sound);

    private void OnMapInit(Entity<MegafaunaBlinkInactiveComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.FixedPosition)
            ent.Comp.Marker = PredictedSpawnAtPosition(ent.Comp.MarkerId, Transform(ent).Coordinates);
    }

    private void OnStartup(Entity<MegafaunaBlinkInactiveComponent> ent, ref MegafaunaStartupEvent args)
    {
        if (!ent.Comp.FixedPosition
            && ent.Comp.Marker == null)
            ent.Comp.Marker = PredictedSpawnAtPosition(ent.Comp.MarkerId, Transform(ent).Coordinates);
    }

    private void OnShutdown(Entity<MegafaunaBlinkInactiveComponent> ent, ref MegafaunaShutdownEvent args)
    {
        if (ent.Comp.Marker == null
            || !_blinkQuery.TryComp(ent, out var blinkComp))
            return;

        Blink(ent.Owner, ent.Comp.Marker.Value, blinkComp.Delay, blinkComp.Sound);
        PredictedQueueDel(ent.Comp.Marker);
        ent.Comp.Marker = null;
    }

    private void OnDelete(Entity<MegafaunaBlinkInactiveComponent> ent, ref EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(ent.Comp.Marker))
            return;

        PredictedQueueDel(ent.Comp.Marker);
        ent.Comp.Marker = null;
    }
}
