using Content.Goobstation.Shared.Terror.Components;
using Content.Goobstation.Shared.Terror.Events;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using YamlDotNet.Core.Tokens;

namespace Content.Goobstation.Shared.Terror.Systems;

/// <summary>
/// Check component summary.
/// </summary>
public sealed class TerrorPounceSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TerrorPounceComponent, TerrorPounceEvent>(OnPounce);
        SubscribeLocalEvent<TerrorPounceComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<TerrorPounceComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnPounce(Entity<TerrorPounceComponent> ent, ref TerrorPounceEvent args)
    {
        _popup.PopupPredicted(Loc.GetString("terror-pounce-leap"), ent.Owner, ent.Owner, PopupType.Medium);

        ent.Comp.IsLeaping = true;
        Dirty(ent);

        var selfMap = _transform.GetMapCoordinates(ent.Owner);
        var targetMap = args.Target.ToMap(EntityManager, _transform);
        var direction = targetMap.Position - selfMap.Position;

        if (direction.Length() > ent.Comp.JumpDistance)
        {
            direction = direction.Normalized() * ent.Comp.JumpDistance;
        }

        _throwing.TryThrow(ent.Owner, direction, ent.Comp.JumpThrowSpeed);

        _audio.PlayPredicted(ent.Comp.JumpSound, ent.Owner, ent.Owner);

        args.Handled = true;
    }

    private void OnCollide(Entity<TerrorPounceComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.IsLeaping)
        {
            return;
        }

        if (_tag.HasTag(args.OtherEntity, "Wall"))
        {
            _popup.PopupPredicted(Loc.GetString("terror-pounce-leap-fail"), ent.Owner, ent.Owner, PopupType.MediumCaution);
            _stun.TryAddStunDuration(ent.Owner, ent.Comp.SelfStun);

            ent.Comp.IsLeaping = false;
            Dirty(ent);

            return;
        }

        if (HasComp<MobStateComponent>(args.OtherEntity))
        {
            HitLivingTarget(ent, args.OtherEntity);
        }

        ent.Comp.IsLeaping = false;
        Dirty(ent);
    }

    private void OnStopThrow(Entity<TerrorPounceComponent> ent, ref StopThrowEvent args)
    {
        ent.Comp.IsLeaping = false;
        Dirty(ent);
    }

    private void HitLivingTarget(Entity<TerrorPounceComponent> ent, EntityUid target)
    {
        _stun.TryAddStunDuration(target, ent.Comp.TargetStun);
        _stun.TryKnockdown(target, ent.Comp.TargetKnockdown, true);

        if (ent.Comp.InjectReagent is not { } reagent)
        {
            return;
        }

        if (!_solution.TryGetSolution(target, "chemicals", out var targetSolution))
        {
            return;
        }

        _solution.TryAddReagent(targetSolution.Value, reagent, ent.Comp.InjectAmount, out _);
    }
}
