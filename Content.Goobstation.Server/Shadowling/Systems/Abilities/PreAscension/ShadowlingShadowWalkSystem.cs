using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.Actions;
using Content.Server.Stealth;
using Content.Shared.Movement.Systems;
using Content.Shared.Stealth.Components;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles Shadow Walk!
/// </summary>
public sealed class ShadowlingShadowWalkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StealthSystem _stealth = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private  readonly TransformSystem _transform = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingShadowWalkComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
        SubscribeLocalEvent<ShadowlingShadowWalkComponent, ShadowWalkEvent>(OnShadowWalk);
    }

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
                var effectEnt = Spawn(shadowWalk.ShadowWalkEffectOut, _transform.GetMapCoordinates(uid));
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
        {
            args.ModifySpeed(comp.WalkSpeedModifier, comp.RunSpeedModifier);
        }
        else
        {
            args.ModifySpeed(1f, 1f);
        }
    }

    private void OnShadowWalk(EntityUid uid, ShadowlingShadowWalkComponent comp, ShadowWalkEvent args)
    {
        comp.IsActive = true;
        comp.NextUpdate = comp.TimeUntilDeactivation + _timing.CurTime;

        _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);

        _audio.PlayPvs(comp.ShadowWalkSound, uid, AudioParams.Default.WithVolume(-2f));
        var effectEnt = Spawn(comp.ShadowWalkEffectIn, _transform.GetMapCoordinates(uid));
        _transform.SetParent(effectEnt, uid);

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, 0f, stealth);

        _actions.StartUseDelay(args.Action);
    }
}
