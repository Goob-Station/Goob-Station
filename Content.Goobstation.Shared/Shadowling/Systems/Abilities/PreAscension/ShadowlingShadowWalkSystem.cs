using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles Shadow Walk!
/// </summary>
public sealed class ShadowlingShadowWalkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingShadowWalkComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
        SubscribeLocalEvent<ShadowlingShadowWalkComponent, ShadowWalkEvent>(OnShadowWalk);
        SubscribeLocalEvent<ShadowlingShadowWalkComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingShadowWalkComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingShadowWalkComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingShadowWalkComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var shadowWalkQuery = EntityQueryEnumerator<ShadowlingShadowWalkComponent>();
        while (shadowWalkQuery.MoveNext(out var uid, out var shadowWalk))
        {
            if (!shadowWalk.IsActive)
                continue;

            if (_timing.CurTime >= shadowWalk.NextUpdate - shadowWalk.EffectOutTimer && !shadowWalk.EffectActivated)
            {
                var effectEnt = PredictedSpawnAtPosition(shadowWalk.ShadowWalkEffectOut, Transform(uid).Coordinates);
                _transform.SetParent(effectEnt, uid);
                shadowWalk.EffectActivated = true;
            }

            if (_timing.CurTime >= shadowWalk.NextUpdate)
            {
                if (TryComp<StealthComponent>(uid, out var stealth))
                {
                    _stealth.SetVisibility(uid, 1f, stealth);
                    RemComp<StealthComponent>(uid);
                    _audio.PlayPvs(shadowWalk.ShadowWalkSound, uid, AudioParams.Default.WithVolume(-2f).WithPitchScale(2f));
                }
                shadowWalk.IsActive = false;
                shadowWalk.EffectActivated = false;
                _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            }
        }
    }

    private void OnMove(EntityUid uid, ShadowlingShadowWalkComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (comp.IsActive)
            args.ModifySpeed(comp.WalkSpeedModifier, comp.RunSpeedModifier);
        else
            args.ModifySpeed(1f, 1f);
    }

    private void OnShadowWalk(EntityUid uid, ShadowlingShadowWalkComponent comp, ShadowWalkEvent args)
    {
        if (args.Handled)
            return;

        comp.IsActive = true;
        comp.NextUpdate = comp.TimeUntilDeactivation + _timing.CurTime;

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);

        _audio.PlayPredicted(comp.ShadowWalkSound, uid, uid, AudioParams.Default.WithVolume(-2f));
        var effectEnt = PredictedSpawnAtPosition(comp.ShadowWalkEffectIn, Transform(uid).Coordinates);
        _transform.SetParent(effectEnt, uid);

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetThermalsImmune(uid, true, stealth);
        _stealth.SetVisibility(uid, -1.5f, stealth);

        args.Handled = true;
    }
}
