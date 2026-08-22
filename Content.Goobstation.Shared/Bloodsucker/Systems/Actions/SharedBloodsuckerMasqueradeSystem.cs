using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public abstract class SharedBloodsuckerMasqueradeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<BloodsuckerMasqueradeComponent, BloodsuckerMasqueradeEvent>(OnMasquerade);
        SubscribeLocalEvent<BloodsuckerMasqueradeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMasquerade(Entity<BloodsuckerMasqueradeComponent> ent, ref BloodsuckerMasqueradeEvent args)
    {
        if (!HasComp<BloodsuckerComponent>(ent.Owner))
            return;

        if (ent.Comp.Active)
            Deactivate(ent);
        else
        {
            if (!TryUseCosts(ent.Owner, ent.Comp))
                return;
            Activate(ent);
        }

        args.Handled = true;
    }

    private void OnShutdown(Entity<BloodsuckerMasqueradeComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Active)
            Deactivate(ent);
    }

    protected virtual void Activate(Entity<BloodsuckerMasqueradeComponent> ent)
    {
        ent.Comp.Active = true;
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        EnsureComp<BloodsuckerMasqueradingComponent>(ent.Owner);

        if (TryComp(ent.Owner, out BloodsuckerComponent? bloodsucker))
        {
            bloodsucker.IsMasquerading = true;
            Dirty(ent.Owner, bloodsucker);
        }

        RemoveBloodsuckerPassives(ent.Owner);

        _audio.PlayPredicted(ent.Comp.ActivateSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-masquerade-on"),
            ent.Owner, ent.Owner, PopupType.Small);

        Dirty(ent);
    }

    protected virtual void Deactivate(Entity<BloodsuckerMasqueradeComponent> ent)
    {
        ent.Comp.Active = false;

        RemCompDeferred<BloodsuckerMasqueradingComponent>(ent.Owner);

        if (TryComp(ent.Owner, out BloodsuckerComponent? bloodsucker))
        {
            bloodsucker.IsMasquerading = false;
            Dirty(ent.Owner, bloodsucker);
        }

        RestoreBloodsuckerPassives(ent.Owner);

        _audio.PlayPredicted(ent.Comp.DeactivateSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-masquerade-off"),
            ent.Owner, ent.Owner, PopupType.Small);

        Dirty(ent);
    }

    protected void CheckMasqueradeValidity(Entity<BloodsuckerMasqueradeComponent> ent)
    {
        if (!ent.Comp.Active)
            return;

        if (_mobStateQuery.TryComp(ent.Owner, out var mobState)
            && mobState.CurrentState == MobState.Dead)
            Deactivate(ent);
    }

    protected virtual void RemoveBloodsuckerPassives(EntityUid uid) { }
    protected virtual void RestoreBloodsuckerPassives(EntityUid uid) { }

    private bool TryUseCosts(EntityUid uid, BloodsuckerMasqueradeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(uid))
            return false;

        if (comp.HumanityCost != 0f && TryComp(uid, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(uid, humanity),
                -comp.HumanityCost);

        return true;
    }
}
