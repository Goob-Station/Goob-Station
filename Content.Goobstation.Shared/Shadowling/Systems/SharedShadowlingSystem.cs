using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.LightDetection.Systems;
using Content.Goobstation.Shared.Mindcontrol;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared._Starlight.CollectiveMind;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Systems;

public abstract class SharedShadowlingSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly SharedLightDetectionDamageSystem _lightDamage = default!;
    [Dependency] private readonly SharedHumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<ShadowlingComponent, HatchEvent>(OnHatch);
        SubscribeLocalEvent<ShadowlingComponent, BeforeDamageChangedEvent>(BeforeDamageChanged);
        SubscribeLocalEvent<ShadowlingComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ShadowlingComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<ShadowlingComponent, ExaminedEvent>(OnExamined);
    }

    #region Event Handlers

    private void OnMobStateChanged(EntityUid uid, ShadowlingComponent component, MobStateChangedEvent args)
    {
        // Remove all Thralls if shadowling is dead
        if (args.NewMobState is not (MobState.Dead or MobState.Invalid)
            || component.CurrentPhase == ShadowlingPhases.Ascension)
            return;

        foreach (var thrall in component.Thralls)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-dead"), thrall, thrall, PopupType.LargeCaution);
            RemCompDeferred<ThrallComponent>(thrall);
        }

        var ev = new ShadowlingDeathEvent();
        RaiseLocalEvent(ev);
    }

    private void OnDamageModify(EntityUid uid, ShadowlingComponent component, DamageModifyEvent args)
    {
        if (args.Origin is not {} origin
            || !HasComp<ProjectileComponent>(origin))
            return;

        foreach (var (key,_) in args.Damage.DamageDict)
        {
            if (key == "Heat")
                args.Damage += component.HeatDamageProjectileModifier;
        }
    }

    public void OnThrallRemoved(Entity<ShadowlingComponent> ent)
    {
        if (!TryComp<LightDetectionDamageComponent>(ent, out var lightDet))
            return;

        _lightDamage.AddResistance((ent.Owner, lightDet), -ent.Comp.LightResistanceModifier);
    }

    public ProtoId<CollectiveMindPrototype> ShadowMind = "Shadowmind";
    private void OnInit(EntityUid uid, ShadowlingComponent component, ref MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.ActionHatchEntity, component.ActionHatch);

        EnsureComp<CollectiveMindComponent>(uid).Channels.Add(ShadowMind);
    }

    private void OnHatch(Entity<ShadowlingComponent> ent, ref HatchEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionHatchEntity);

        StartHatchingProgress(ent);
    }

    protected virtual void StartHatchingProgress(Entity<ShadowlingComponent> ent) { }

    private void BeforeDamageChanged(EntityUid uid, ShadowlingComponent comp, BeforeDamageChangedEvent args)
    {
        // Can't take damage during hatching
        if (comp.IsHatching)
            args.Cancelled = true;
    }

    public void OnPhaseChanged(EntityUid uid, ShadowlingComponent component, ShadowlingPhases phase)
    {
        var defaultAbilities = _protoMan.Index(component.PostHatchComponents);
        switch (phase)
        {
            case ShadowlingPhases.PostHatch:
            {
                EntityManager.AddComponents(uid, defaultAbilities);
                _actions.RemoveAction(uid, component.ActionHatchEntity);
                break;
            }
            case ShadowlingPhases.Ascension:
            {
                // Remove all previous actions
                EntityManager.RemoveComponents(uid, defaultAbilities);
                EntityManager.RemoveComponents(uid, _protoMan.Index(component.ObtainableComponents));

                EntityManager.AddComponents(uid, _protoMan.Index(component.PostAscensionComponents));

                var ev = new ShadowlingAscendEvent(uid);
                RaiseLocalEvent(ev);
                break;
            }
            case ShadowlingPhases.FailedAscension:
            {
                // git gud bro :sob: :pray:
                EntityManager.RemoveComponents(uid, defaultAbilities);
                EntityManager.RemoveComponents(uid, _protoMan.Index(component.ObtainableComponents));

                // this is such a big L that even the code is losing and all variables are hardcoded.
                EnsureComp<SlowedDownComponent>(uid);
                _appearance.AddMarking(uid, "AbominationTorso");
                _appearance.AddMarking(uid, "AbominationHorns");

                // take another hardcoded variable
                _damageable.SetDamageModifierSetId(uid, "ShadowlingAbomination");
                break;
            }
        }
    }

    private void OnExamined(EntityUid uid, ShadowlingComponent comp, ExaminedEvent args)
    {
        if (args.Examiner != uid
            || !TryComp<LightDetectionDamageComponent>(uid, out var lightDet))
            return;

        args.PushMarkup(Loc.GetString("shadowling-examine-self", ("damage", lightDet.ResistanceModifier * lightDet.DamageToDeal.GetTotal())));
    }

    #endregion

    public bool CanEnthrall(EntityUid uid, EntityUid target)
    {
        if (HasComp<ShadowlingComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-shadowling"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (HasComp<ThrallComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-already-thrall"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (!TryComp<MindControllableComponent>(target, out var mindControllable) || mindControllable.ControlledBySomeone)
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-cant-be-controlled"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (!TryComp<MindContainerComponent>(target, out var mind) || !mind.HasMind)
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-no-mind"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-non-humanoid"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        // Target needs to be alive
        if (!TryComp<MobStateComponent>(target, out var mobState)
            || !_mobStateSystem.IsCritical(target, mobState) && !_mobStateSystem.IsCritical(target, mobState))
            return true;

        _popup.PopupPredicted(Loc.GetString("shadowling-enthrall-dead"), uid, uid, PopupType.SmallCaution);
        return false;
    }

    public bool CanGlare(EntityUid target)
    {
        return HasComp<MobStateComponent>(target)
               && !HasComp<ShadowlingComponent>(target)
               && !HasComp<ThrallComponent>(target)
               && !HasComp<HereticComponent>(target)
               && !HasComp<ChangelingIdentityComponent>(target);
    }

    public void DoEnthrall(EntityUid uid, EntProtoId components, SimpleDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Handled
            || args.Target == null)
            return;

        var target = args.Target.Value;

        var thrall = EnsureComp<ThrallComponent>(target);
        thrall.Converter = uid;
        var comps = _protoMan.Index(components);
        EntityManager.AddComponents(target, comps);

        if (TryComp<ShadowlingComponent>(uid, out var sling))
            sling.Thralls.Add(target);

        _audio.PlayPredicted(
            new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg"),
            target,
            uid,
            AudioParams.Default);

        args.Handled = true;
    }
}
