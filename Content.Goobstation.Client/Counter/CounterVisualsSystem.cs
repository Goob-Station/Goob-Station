using Content.Goobstation.Shared.Counter;
using Content.Shared.Movement.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Counter;

/// <summary>
/// Draws the counters visuals.
/// </summary>
public sealed class CounterVisualsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CounterComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CounterComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CounterComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnStartup(Entity<CounterComponent> ent, ref ComponentStartup args) =>
        EnsureShader(ent);

    private void OnState(Entity<CounterComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        EnsureShader(ent);

        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnShutdown(Entity<CounterComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent.Owner) && TryComp<SpriteComponent>(ent, out var sprite))
            sprite.PostShader = null;
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<CounterComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var counter, out var sprite))
        {
            EnsureShader((uid, counter));

            var start = counter.Active || counter.Triggered ? counter.BuildupEndTime : counter.ArmTime;
            var length = (counter.Active || counter.Triggered ? counter.CounterEndTime : counter.BuildupEndTime) - start;
            var progress = length > TimeSpan.Zero
                ? Math.Clamp((float) ((now - start) / length), 0f, 1f)
                : 1f;

            if (sprite.PostShader is { Mutable: true } shader)
                shader.SetParameter("progress", progress);

            if (counter.Active && !counter.Triggered)
                UpdateHeartbeat(uid, counter, progress);
        }
    }

    private void UpdateHeartbeat(EntityUid uid, CounterComponent counter, float progress)
    {
        var closing = SmoothStep(0.66f, 1f, progress);
        var bpm = 1.6f + 3.4f * closing;
        var beat = (int) Math.Floor(_timing.RealTime.TotalSeconds * bpm);

        if (beat == counter.LastBeat)
            return;

        var first = counter.LastBeat == -1;
        counter.LastBeat = beat;
        if (first)
            return;

        if (counter.BeatSound is not { } beatSound)
            return;

        var audioParams = beatSound.Params.WithPitchScale(1f + 0.6f * closing);
        _audio.PlayLocal(beatSound, uid, null, audioParams);
    }

    private static float SmoothStep(float edge0, float edge1, float x)
    {
        var t = Math.Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    private static int Phase(CounterComponent counter) =>
        counter.Triggered ? 2 : counter.Active ? 1 : 0;

    private static string? ShaderFor(CounterComponent counter, int phase) =>
        phase == 2 ? counter.TriggeredShader : phase == 1 ? counter.ArmedShader : counter.BuildupShader;

    private void EnsureShader(Entity<CounterComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var phase = Phase(ent.Comp);
        var missing = sprite.PostShader == null && ShaderFor(ent.Comp, phase) != null;
        if (phase == ent.Comp.ShownPhase && !missing)
            return;

        ent.Comp.ShownPhase = phase;
        UpdateShader(ent, sprite, phase);
    }

    private void UpdateShader(Entity<CounterComponent> ent, SpriteComponent sprite, int phase)
    {
        var shaderId = ShaderFor(ent.Comp, phase);

        if (shaderId is { } id && _proto.TryIndex<ShaderPrototype>(id, out var proto))
            sprite.PostShader = proto.InstanceUnique();
        else
            sprite.PostShader = null;
    }
}
