using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.Examine;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// Handles cinematic systems, including segments, duck audio, and camera pull.
/// </summary>
public sealed class SharedCinematicSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
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
            ApplyTimelineRegistry(uid, comp);
            UpdateSegmentComponents(uid, comp);

            if (_timing.CurTime >= comp.EndTime)
                RemCompDeferred<CinematicComponent>(uid);
        }
    }

    /// <summary>
    /// Applies the addComp registry to the entity.
    /// </summary>
    private void ApplyTimelineRegistry(EntityUid uid, CinematicComponent comp)
    {
        if (comp.RegistryApplied || !_proto.TryIndex(comp.Timeline, out var proto))
            return;

        comp.RegistryApplied = true;

        if (proto.AddComp is { } add)
            EntityManager.AddComponents(uid, add);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var player = _player.LocalEntity;

        var query = EntityQueryEnumerator<CinematicComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            UpdateEngagement((uid, comp), player, frameTime);

            // Keeps all of the cinematic components synced.
            // Just prevents a lot of manual timing for them.
            if (comp.Engaged)
            {
                var ev = new CinematicUpdatedEvent(comp.Strength);
                RaiseLocalEvent(uid, ref ev);

                comp.EyeOffset = comp.Pan + ev.EyeOffset;
            }

            UpdateTimeline((uid, comp), player);
        }
    }

    private void OnShutdown(Entity<CinematicComponent> ent, ref ComponentShutdown args)
    {
        if (_proto.TryIndex(ent.Comp.Timeline, out var proto))
        {
            if (ent.Comp.ActiveSegment >= 0
                && ent.Comp.ActiveSegment < proto.Segments.Count
                && proto.Segments[ent.Comp.ActiveSegment].AddComp is { } registry)
                EntityManager.RemoveComponents(ent.Owner, registry);

            if (proto.AddComp is { } wholeRegistry)
                EntityManager.RemoveComponents(ent.Owner, wholeRegistry);

            ent.Comp.ActiveSegment = -1;
        }

        CleanupTimeline(ent.Comp);
    }

    #region Engagement

    private void UpdateEngagement(Entity<CinematicComponent> cine, EntityUid? player, float frameTime)
    {
        var comp = cine.Comp;
        var target = player == null ? 0f : GetStrength(comp);

        if (target > 0f)
        {
            if (!comp.Engaged)
                comp.Engaged = ShouldEngage(cine, player!.Value);

            if (comp.Engaged)
            {
                var delta = _transform.GetWorldPosition(cine) - _transform.GetWorldPosition(player!.Value);
                if (delta.Length() > comp.MaxPanDistance)
                    delta = Vector2.Normalize(delta) * comp.MaxPanDistance;

                comp.Pan = delta * comp.CameraPull * target;
            }
            else
                target = 0f;
        }

        if (target <= 0f)
            comp.Pan = Vector2.Lerp(comp.Pan, Vector2.Zero, MathF.Min(1f, frameTime * comp.PanReturnRate));

        comp.Strength = MathHelper.Lerp(comp.Strength, target, MathF.Min(1f, frameTime * (target > comp.Strength ? comp.EngageRate : comp.DisengageRate)));
        if (target <= 0f && comp.Strength < 0.005f)
        {
            comp.Strength = 0f;
            comp.Pan = Vector2.Zero;
        }
    }

    private bool ShouldEngage(Entity<CinematicComponent> cine, EntityUid player)
    {
        if (cine.Owner == player)
            return true;

        if (!cine.Comp.PullsOtherCameras)
            return false;

        return _examine.InRangeUnOccluded(player, cine, cine.Comp.ViewerRange);
    }

    private void OnGetEyeOffset(Entity<ContentEyeComponent> ent, ref GetEyeOffsetEvent args)
    {
        if (ent.Owner != _player.LocalEntity)
            return;

        var query = EntityQueryEnumerator<CinematicComponent>();
        while (query.MoveNext(out _, out var comp))
            args.Offset += comp.EyeOffset;
    }

    #endregion

    #region Timeline

    private void UpdateTimeline(Entity<CinematicComponent> cine, EntityUid? local)
    {
        var comp = cine.Comp;

        if (cine.Owner != local
            || !_proto.TryIndex(comp.Timeline, out var timeline)
            || timeline.Segments.Count == 0
            || GetStrength(comp) <= 0f)
        {
            CleanupTimeline(comp);
            return;
        }

        if (timeline.DuckAudio)
            DuckOthers(comp, timeline.DuckDecibels);
    }

    private void EnterSegment(CinematicComponent comp, CinematicPrototype timeline, int index)
    {
        if (index < 0)
            return;

        var segment = timeline.Segments[index];

        if (segment.Sound != null
            && _audio.PlayGlobal(segment.Sound, Filter.Local(), false)?.Entity is { } stream)
        {
            comp.SegmentSounds.Add(stream);
            EnsureComp<CinematicSceneSoundComponent>(stream);
        }
    }

    /// <summary>
    /// Pulls every sound source that isn't the from the scene and turns the volume down.
    /// </summary>
    private void DuckOthers(CinematicComponent comp, float decibels)
    {
        var query = AllEntityQuery<AudioComponent>();
        while (query.MoveNext(out var uid, out var audio))
        {
            if (HasComp<CinematicSceneSoundComponent>(uid) || comp.Ducked.ContainsKey(uid))
                continue;

            comp.Ducked[uid] = audio.Params.Volume;
            _audio.SetVolume(uid, audio.Params.Volume - decibels);
        }
    }

    private void CleanupTimeline(CinematicComponent comp)
    {
        foreach (var stream in comp.SegmentSounds)
            _audio.Stop(stream);

        comp.SegmentSounds.Clear();

        if (comp.Ducked.Count == 0)
            return;

        foreach (var (uid, volume) in comp.Ducked)
            if (HasComp<AudioComponent>(uid))
                _audio.SetVolume(uid, volume);

        comp.Ducked.Clear();
    }

    /// <summary>
    /// Applies and removes segments addComp registries on the focus entity.
    /// </summary>
    private void UpdateSegmentComponents(EntityUid uid, CinematicComponent comp)
    {
        if (!_proto.TryIndex(comp.Timeline, out var proto))
            return;

        var index = GetSegmentIndex(comp);
        if (index == comp.ActiveSegment)
            return;

        if (comp.ActiveSegment >= 0
            && comp.ActiveSegment < proto.Segments.Count
            && proto.Segments[comp.ActiveSegment].AddComp is { } old)
            EntityManager.RemoveComponents(uid, old);

        if (index >= 0 && proto.Segments[index].AddComp is { } add)
            EntityManager.AddComponents(uid, add);

        comp.ActiveSegment = index;

        if (uid == _player.LocalEntity || comp.Engaged)
            EnterSegment(comp, proto, index);
    }

    /// <summary>
    /// The index of the segment running right now, or -1 outside the timeline.
    /// </summary>
    public int GetSegmentIndex(CinematicComponent comp)
    {
        var elapsed = (float) (_timing.CurTime - comp.StartTime).TotalSeconds;
        if (elapsed >= 0f && _proto.TryIndex(comp.Timeline, out var proto))
            for (var i = 0; i < proto.Segments.Count; i++)
            {
                elapsed -= proto.Segments[i].Duration;
                if (elapsed < 0f)
                    return i;
            }

        return -1;
    }

    #endregion

    /// <summary>
    /// Starts (or restarts) a scripted cinematic from a prototype on the focus entity.
    /// </summary>
    public CinematicComponent StartCinematic(EntityUid uid, ProtoId<CinematicPrototype> protoId, string? subject = null, string? station = null)
    {
        var proto = _proto.Index(protoId);

        var total = 0f;
        foreach (var segment in proto.Segments)
            total += segment.Duration;

        var comp = EnsureComp<CinematicComponent>(uid);
        comp.StartTime = _timing.CurTime;
        comp.EndTime = _timing.CurTime + TimeSpan.FromSeconds(total);
        comp.Timeline = protoId;
        comp.ActiveSegment = -1;
        comp.SubjectName = subject;
        comp.StationName = station;
        comp.RegistryApplied = false;
        Dirty(uid, comp);

        return comp;
    }

    public void StopCinematic(EntityUid uid) =>
        RemCompDeferred<CinematicComponent>(uid);

    public float GetStrength(CinematicComponent comp)
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

    public static float SmoothStep(float x)
    {
        return x * x * (3f - 2f * x);
    }
}
