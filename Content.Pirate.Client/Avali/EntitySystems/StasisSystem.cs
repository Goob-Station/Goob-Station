using Content.Pirate.Shared.Avali.Components;
using Content.Pirate.Shared.Avali.EntitySystems;
using Content.Pirate.Shared.Avali.Events;
using Content.Shared.Popups;
using Robust.Client.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Pirate.Client.Avali.EntitySystems;

/// <summary>
/// Client-side visuals and audio for nanite stasis.
/// </summary>
public sealed class StasisSystem : SharedStasisSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StasisComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeNetworkEvent<StasisAnimationEvent>(OnStasisAnimation);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        CleanupOrphanedEffects();
        CheckVisibilityStates();
        CheckContinuousEffects();
    }

    public override void Shutdown()
    {
        var query = AllEntityQuery<StasisComponent>();
        while (query.MoveNext(out var uid, out var stasis))
        {
            CleanupStasisVisuals(uid, stasis);
        }

        base.Shutdown();
    }

    private void OnComponentShutdown(EntityUid uid, StasisComponent component, ComponentShutdown args)
    {
        CleanupStasisVisuals(uid, component);
    }

    private void CleanupStasisVisuals(EntityUid uid, StasisComponent stasis)
    {
        if (stasis.ClientContinuousEffectEntity is { } continuous && Exists(continuous))
            QueueDel(continuous);

        if (stasis.ClientEnterEffectEntity is { } enter && Exists(enter))
            QueueDel(enter);

        if (TryComp<SpriteComponent>(uid, out var sprite))
            _sprite.SetColor(uid, sprite.Color.WithAlpha(1f));
    }

    private void OnStasisAnimation(StasisAnimationEvent ev)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var uid = GetEntity(ev.Entity);
        if (!TryComp<StasisComponent>(uid, out var stasis))
            return;

        switch (ev.AnimationType)
        {
            case StasisAnimationType.Prepare:
                _popup.PopupEntity(Loc.GetString("stasis-entering"), uid, PopupType.Medium);
                PrepareAnimation(uid, stasis);
                break;
            case StasisAnimationType.Enter:
                EnterAnimation(uid, stasis);
                break;
            case StasisAnimationType.Exit:
                _popup.PopupEntity(Loc.GetString("stasis-exiting"), uid, PopupType.Medium);
                ExitAnimation(uid, stasis);
                break;
        }
    }

    private void PrepareAnimation(EntityUid uid, StasisComponent stasis)
    {
        if (!Exists(uid))
            return;

        EnsureComp<TransformComponent>(uid, out var transform);
        var effect = SpawnAttachedTo(stasis.StasisEnterEffect, transform.Coordinates);
        _transform.SetParent(effect, uid);
        RemComp<TimedDespawnComponent>(effect);
        ConfigureEffectSprite(uid, effect);

        _audio.PlayPvs(stasis.StasisEnterSound, effect);
        stasis.ClientEnterEffectEntity = effect;
        Dirty(uid, stasis);
    }

    private void EnterAnimation(EntityUid uid, StasisComponent stasis)
    {
        if (!Exists(uid))
            return;

        StartContinuousAnimation(uid, stasis);
        EndPrepareAnimation(uid, stasis);

        UpdateEntityVisibility(uid, stasis);
    }

    private void ExitAnimation(EntityUid uid, StasisComponent stasis)
    {
        if (!Exists(uid))
            return;

        EnsureComp<TransformComponent>(uid, out var transform);
        var effect = SpawnAttachedTo(stasis.StasisExitEffect, transform.Coordinates);
        _transform.SetParent(effect, uid);
        EnsureComp<TimedDespawnComponent>(effect, out var despawn);
        despawn.Lifetime = stasis.StasisExitEffectLifetime;
        ConfigureEffectSprite(uid, effect);

        _audio.PlayPvs(stasis.StasisExitSound, effect);
        EndPrepareAnimation(uid, stasis);
        EndContinuousAnimation(uid, stasis);
        UpdateEntityVisibility(uid, stasis);
    }

    private void EndPrepareAnimation(EntityUid uid, StasisComponent stasis)
    {
        if (stasis.ClientEnterEffectEntity is not { } effect)
            return;

        if (Exists(effect))
            QueueDel(effect);

        stasis.ClientEnterEffectEntity = null;
        Dirty(uid, stasis);
    }

    private void StartContinuousAnimation(EntityUid uid, StasisComponent stasis)
    {
        if (!Exists(uid))
            return;

        EndContinuousAnimation(uid, stasis);

        EnsureComp<TransformComponent>(uid, out var transform);
        var effect = SpawnAttachedTo(stasis.StasisContinuousEffect, transform.Coordinates);
        _transform.SetParent(effect, uid);
        RemComp<TimedDespawnComponent>(effect);
        ConfigureEffectSprite(uid, effect);

        stasis.ClientContinuousEffectEntity = effect;
        Dirty(uid, stasis);
    }

    private void EndContinuousAnimation(EntityUid uid, StasisComponent stasis)
    {
        if (stasis.ClientContinuousEffectEntity is not { } effect)
            return;

        if (Exists(effect))
            QueueDel(effect);

        stasis.ClientContinuousEffectEntity = null;
        Dirty(uid, stasis);
    }

    private void ConfigureEffectSprite(EntityUid parent, EntityUid effect)
    {
        if (!TryComp<SpriteComponent>(effect, out var effectSprite))
            return;

        _sprite.SetDrawDepth(effect, (int) DrawDepth.Effects);
        effectSprite.NoRotation = true;

        if (TryComp<SpriteComponent>(parent, out var parentSprite))
        {
            _sprite.SetVisible(effect, parentSprite.Visible);
            _sprite.SetScale(effect, parentSprite.Scale);
        }
        else
        {
            _sprite.SetVisible(effect, false);
        }
    }

    private void CleanupOrphanedEffects()
    {
        var query = AllEntityQuery<StasisComponent>();
        while (query.MoveNext(out var uid, out var stasis))
        {
            if (EntityManager.IsQueuedForDeletion(uid))
                continue;

            if (!stasis.IsInStasis && stasis.ClientContinuousEffectEntity is { } continuous)
            {
                if (Exists(continuous))
                    QueueDel(continuous);

                stasis.ClientContinuousEffectEntity = null;
                Dirty(uid, stasis);
            }
            else if (stasis.ClientContinuousEffectEntity is { } staleContinuous && !Exists(staleContinuous))
            {
                stasis.ClientContinuousEffectEntity = null;
                Dirty(uid, stasis);
            }

            if (stasis.ClientEnterEffectEntity is { } enter && !Exists(enter))
            {
                stasis.ClientEnterEffectEntity = null;
                Dirty(uid, stasis);
            }
        }
    }

    private void CheckVisibilityStates()
    {
        var query = AllEntityQuery<StasisComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var stasis, out _))
        {
            if (!EntityManager.IsQueuedForDeletion(uid))
                UpdateEntityVisibility(uid, stasis);
        }
    }

    private void CheckContinuousEffects()
    {
        var query = AllEntityQuery<StasisComponent>();
        while (query.MoveNext(out var uid, out var stasis))
        {
            if (!EntityManager.IsQueuedForDeletion(uid) &&
                stasis.IsInStasis &&
                stasis.ClientContinuousEffectEntity == null)
            {
                StartContinuousAnimation(uid, stasis);
            }
        }
    }

    private void UpdateEntityVisibility(EntityUid uid, StasisComponent stasis)
    {
        if (!Exists(uid) || !TryComp<SpriteComponent>(uid, out var sprite))
            return;

        // Client-side alpha caches can be lost across PVS resyncs.
        _sprite.SetColor(uid, sprite.Color.WithAlpha(stasis.IsVisible ? 1f : 0f));
    }
}
