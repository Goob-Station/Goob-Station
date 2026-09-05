using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Audio.Systems;
using Content.Goobstation.Shared.Overlays;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public abstract class SharedBloodsuckerCloakSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<BloodsuckerCloakComponent, BloodsuckerCloakEvent>(OnCloak);
        SubscribeLocalEvent<BloodsuckerCloakComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnCloak(Entity<BloodsuckerCloakComponent> ent, ref BloodsuckerCloakEvent args)
    {
        if (!HasComp<BloodsuckerComponent>(ent.Owner))
            return;

        if (ent.Comp.Active)
        {
            Deactivate(ent);
        }
        else
        {
            if (!CanActivate(ent))
                return;

            if (!TryUseCosts(ent.Owner, ent.Comp))
                return;

            Activate(ent);
        }

        args.Handled = true;
    }

    private void OnShutdown(Entity<BloodsuckerCloakComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Active)
            Deactivate(ent);

        if (!ent.Comp.HadLightDetection)
            RemCompDeferred<LightDetectionComponent>(ent);
    }

    private bool CanActivate(Entity<BloodsuckerCloakComponent> ent)
    {
        // Check for witnesses
        foreach (var witness in _lookup.GetEntitiesInRange<MobStateComponent>(
                     Transform(ent.Owner).Coordinates, ent.Comp.WitnessRange))
        {
            if (witness.Owner == ent.Owner)
                continue;

            if (witness.Comp.CurrentState == MobState.Dead)
                continue;

            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-cloak-fail-witnessed"),
                ent.Owner, ent.Owner, PopupType.Small);
            return false;
        }

        return true;
    }

    protected virtual void Activate(Entity<BloodsuckerCloakComponent> ent)
    {
        ent.Comp.Active = true;
        ent.Comp.HadLightDetection = HasComp<LightDetectionComponent>(ent.Owner);
        EnsureComp<LightDetectionComponent>(ent.Owner);

        var nvg = EnsureComp<NightVisionComponent>(ent.Owner);
        nvg.IsActive = true;
        nvg.Color = Color.FromHex("#3a1a5e");
        nvg.ActivateSound = null;
        nvg.DeactivateSound = null;
        Dirty(ent.Owner, nvg);

        ApplyStealth(ent);
        _audio.PlayPredicted(ent.Comp.ActivateSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-cloak-on"),
            ent.Owner, ent.Owner, PopupType.Small);

        Dirty(ent);
    }

    protected virtual void Deactivate(Entity<BloodsuckerCloakComponent> ent)
    {
        ent.Comp.Active = false;

        RemCompDeferred<StealthComponent>(ent.Owner);
        RemCompDeferred<NightVisionComponent>(ent.Owner);

        if (!ent.Comp.HadLightDetection)
            RemCompDeferred<LightDetectionComponent>(ent.Owner);

        _audio.PlayPredicted(ent.Comp.DeactivateSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-cloak-off"),
            ent.Owner, ent.Owner, PopupType.Small);

        Dirty(ent);
    }

    protected void CheckCloakValidity(Entity<BloodsuckerCloakComponent> ent)
    {
        if (!ent.Comp.Active)
            return;

        // Drop cloak on unconsciousness
        if (_mobStateQuery.TryComp(ent.Owner, out var mobState)
            && mobState.CurrentState != MobState.Alive)
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-cloak-fell-off"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            Deactivate(ent);
            return;
        }
    }

    private void ApplyStealth(Entity<BloodsuckerCloakComponent> ent)
    {
        var visibility = MathF.Max(
            ent.Comp.MinVisibility,
            ent.Comp.BaseVisibility - ent.Comp.ActionLevel * ent.Comp.VisibilityPerLevel);

        var stealth = EnsureComp<StealthComponent>(ent.Owner);
        _stealth.SetEnabled(ent.Owner, true, stealth);
        _stealth.SetVisibility(ent.Owner, visibility, stealth);
        _stealth.SetRevealOnAttack((ent.Owner, stealth), false);
        _stealth.SetRevealOnDamage((ent.Owner, stealth), false);
    }

    private bool TryUseCosts(EntityUid uid, BloodsuckerCloakComponent comp)
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
