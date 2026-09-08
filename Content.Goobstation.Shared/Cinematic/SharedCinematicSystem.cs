using Content.Shared.Camera;
using Content.Shared.Examine;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Plays a scripted <see cref="CinematicPrototype"/> on an entity.
/// Split across files:
/// <list type="bullet">
/// <item>Segments: loading everything.</item>
/// <item>Camera: camera pulling.</item>
/// <item>Audio: audio ducking.</item>
/// </list>
/// </summary>
public sealed partial class SharedCinematicSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CinematicComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ContentEyeComponent, GetEyeOffsetEvent>(OnGetEyeOffset);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<CinematicComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!_proto.TryIndex(comp.Timeline, out var timeline))
                continue;

            LoadTimeline((uid, comp), timeline);
            HandleSegments((uid, comp), timeline);

            if (_timing.CurTime >= comp.EndTime)
                StopCinematic(uid);
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var viewer = _player.LocalEntity;

        var query = EntityQueryEnumerator<CinematicComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateCamera((uid, comp), viewer, frameTime);
            UpdateAudio((uid, comp), viewer);
        }
    }

    private void OnShutdown(Entity<CinematicComponent> ent, ref ComponentShutdown args)
    {
        if (_proto.TryIndex(ent.Comp.Timeline, out var timeline))
        {
            UnloadSegment(ent, timeline);
            UnloadTimeline(ent, timeline);
        }

        StopSegmentSounds(ent);
        RestoreDucked(ent);
    }

    /// <summary>
    /// Starts a scripted cinematic on the focus entity.
    /// Fails if one is already playing.
    /// </summary>
    public bool TryStartCinematic(EntityUid uid, ProtoId<CinematicPrototype> protoId, string? subject = null, string? station = null)
    {
        if (TryComp<CinematicComponent>(uid, out var existing))
        {
            Log.Warning($"Tried to start cinematic {protoId} on {ToPrettyString(uid)} but {existing.Timeline} is already playing on it.");
            return false;
        }

        var timeline = _proto.Index(protoId);

        var comp = AddComp<CinematicComponent>(uid);
        comp.StartTime = _timing.CurTime;
        comp.EndTime = _timing.CurTime + GetDuration(timeline);
        comp.Timeline = protoId;
        comp.ActiveSegment = -1;
        comp.SubjectName = subject;
        comp.StationName = station;
        comp.RegistryApplied = false;
        Dirty(uid, comp);

        return true;
    }

    public void StopCinematic(EntityUid uid) =>
        RemCompDeferred<CinematicComponent>(uid);

    #region Timing

    private static TimeSpan GetDuration(CinematicPrototype timeline)
    {
        var total = 0f;
        foreach (var segment in timeline.Segments)
            total += segment.Duration;

        return TimeSpan.FromSeconds(total);
    }

    private float GetStrength(CinematicComponent comp)
    {
        var now = _timing.CurTime;
        if (now < comp.StartTime || now >= comp.EndTime)
            return 0f;

        var intro = comp.IntroTime <= 0f
            ? 1f
            : (float) (now - comp.StartTime).TotalSeconds / comp.IntroTime;
        var outro = comp.OutroTime <= 0f
            ? 1f
            : (float) (comp.EndTime - now).TotalSeconds / comp.OutroTime;

        return SmoothStep(Math.Clamp(Math.Min(intro, outro), 0f, 1f));
    }

    private static float SmoothStep(float x) =>
        x * x * (3f - 2f * x);

    #endregion
}
