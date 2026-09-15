using Content.Shared._White.Other;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

using Content.Shared._Oskarrr.Yautja.Components;

namespace Content.Shared._Oskarrr.Yautja.Systems;

public sealed class YautjaCleanserGelSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<YautjaCleanserGelComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<YautjaCleanserGelComponent, YautjaCleanserGelDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<YautjaDissolvingComponent, GettingPickedUpAttemptEvent>(OnDissolvingPickupAttempt);
        SubscribeLocalEvent<YautjaHealingGelEffectComponent, GettingPickedUpAttemptEvent>(OnEffectPickupAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Only server finishes dissolve — avoids predicting delete of networked entities.
        if (!_net.IsServer)
            return;

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<YautjaDissolvingComponent>();
        while (query.MoveNext(out var uid, out var dissolving))
        {
            if (now < dissolving.DissolveAt)
                continue;

            FinishDissolve(uid, dissolving);
        }
    }

    private void OnAfterInteract(Entity<YautjaCleanserGelComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (target == args.User || target == ent.Owner)
            return;

        if (!CanDissolve(target, args.User, popup: true))
        {
            args.Handled = true;
            return;
        }

        var doArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.ApplyDelay,
            new YautjaCleanserGelDoAfterEvent(),
            ent.Owner,
            target,
            ent.Owner)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnHandChange = true,
            AttemptFrequency = AttemptFrequency.EveryTick,
        };

        if (_doAfter.TryStartDoAfter(doArgs))
            args.Handled = true;
    }

    private void OnDoAfter(Entity<YautjaCleanserGelComponent> ent, ref YautjaCleanserGelDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target)
            return;

        if (!CanDissolve(target, args.User, popup: false))
            return;

        if (HasComp<YautjaDissolvingComponent>(target))
            return;

        args.Handled = true;

        var coords = _transform.GetMoverCoordinates(target);
        _audio.PlayPredicted(ent.Comp.DissolveSound, coords, args.User);

        EntityUid? effect = null;
        if (_net.IsServer)
        {
            effect = Spawn(ent.Comp.DissolveEffect, coords);
            EnsureComp<YautjaHealingGelEffectComponent>(effect.Value);
        }

        var dissolving = EnsureComp<YautjaDissolvingComponent>(target);
        dissolving.AshPrototype = ent.Comp.AshPrototype;
        dissolving.DissolveAt = _timing.CurTime + ent.Comp.DissolveDuration;
        dissolving.EffectEntity = effect;
        Dirty(target, dissolving);

        _popup.PopupClient(Loc.GetString("yautja-cleanser-gel-applied"), target, args.User);

        if (ent.Comp.ConsumeOnUse && !TerminatingOrDeleted(ent.Owner))
        {
            if (_net.IsServer)
                QueueDel(ent.Owner);
            else
                PredictedDel(ent.Owner);
        }
    }

    private void OnDissolvingPickupAttempt(Entity<YautjaDissolvingComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        args.Cancel();
        if (args.ShowPopup)
            _popup.PopupClient(Loc.GetString("yautja-cleanser-gel-cant-pickup"), ent.Owner, args.User);
    }

    private void OnEffectPickupAttempt(Entity<YautjaHealingGelEffectComponent> ent, ref GettingPickedUpAttemptEvent args)
    {
        args.Cancel();
    }

    private void FinishDissolve(EntityUid target, YautjaDissolvingComponent dissolving)
    {
        var coords = _transform.GetMoverCoordinates(target);

        if (dissolving.EffectEntity is { } effect && !TerminatingOrDeleted(effect))
            QueueDel(effect);

        RemComp<YautjaDissolvingComponent>(target);
        QueueDel(target);
        Spawn(dissolving.AshPrototype, coords);
    }

    private bool CanDissolve(EntityUid target, EntityUid user, bool popup)
    {
        if (HasComp<YautjaDissolvingComponent>(target))
        {
            if (popup)
                _popup.PopupClient(Loc.GetString("yautja-cleanser-gel-already"), target, user);
            return false;
        }

        if (HasComp<StructureComponent>(target) || Transform(target).Anchored)
        {
            if (popup)
                _popup.PopupClient(Loc.GetString("yautja-cleanser-gel-structure"), target, user);
            return false;
        }

        if (TryComp<MobStateComponent>(target, out var mobState))
        {
            if (mobState.CurrentState != MobState.Dead)
            {
                if (popup)
                    _popup.PopupClient(Loc.GetString("yautja-cleanser-gel-alive"), target, user);
                return false;
            }

            return true;
        }

        if (!HasComp<ItemComponent>(target))
        {
            if (popup)
                _popup.PopupClient(Loc.GetString("yautja-cleanser-gel-invalid"), target, user);
            return false;
        }

        return true;
    }
}

[Serializable, NetSerializable]
public sealed partial class YautjaCleanserGelDoAfterEvent : SimpleDoAfterEvent;
