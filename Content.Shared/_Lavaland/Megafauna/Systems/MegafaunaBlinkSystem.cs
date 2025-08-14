using Content.Shared._Lavaland.Megafauna.Components;
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

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MegafaunaBlinkComponent, MegafaunaBlinkActionEvent>(OnBlinkAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blinkQuery = EntityQueryEnumerator<MegafaunaBlinkComponent>();
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
        if (args.Handled)
            return;

        Blink(ent, args.Target, args.Duration, args.Sound);

        if (args.SpawnOnUsed != null)
            PredictedSpawnAtPosition(args.SpawnOnUsed.Value, Transform(ent).Coordinates);

        if (args.SpawnOnTarget != null)
            PredictedSpawnAtPosition(args.SpawnOnTarget.Value, args.Target);

        args.Handled = true;
    }

    public void Blink(
        EntityUid ent,
        EntityCoordinates coords,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
    {
        var blinkComp = EnsureComp<MegafaunaBlinkComponent>(ent);
        blinkComp.BlinkTime = _timing.CurTime + duration ?? blinkComp.DefaultDelay;
        blinkComp.Coordinates = coords;
        blinkComp.Sound = sound;
        Dirty(ent, blinkComp);
    }

    public void Blink(
        EntityUid ent,
        EntityUid target,
        TimeSpan? duration = null,
        SoundSpecifier? sound = null)
        => Blink(ent, Transform(target).Coordinates, duration, sound);
}
