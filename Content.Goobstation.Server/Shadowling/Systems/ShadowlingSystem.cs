using Content.Goobstation.Shared.Flashbang;
using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Systems;
using Content.Server.Actions;
using Content.Server.Humanoid;
using Content.Server.Objectives.Systems;
using Content.Server.Popups;
using Content.Server.Storage.EntitySystems;
using Content.Server.Stunnable;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles the Shadowling's System
/// </summary>
public sealed partial class ShadowlingSystem : SharedShadowlingSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly CodeConditionSystem _codeCondition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShadowlingComponent, BeforeDamageChangedEvent>(BeforeDamageChanged);
        SubscribeLocalEvent<ShadowlingComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ShadowlingComponent, GetFlashbangedEvent>(OnFlashBanged);
        SubscribeLocalEvent<ShadowlingComponent, DamageModifyEvent>(OnDamageModify);
        SubscribeLocalEvent<ShadowlingComponent, SelfBeforeGunShotEvent>(BeforeGunShot);
        SubscribeLocalEvent<ShadowlingComponent, ExaminedEvent>(OnExamined);

        SubscribeAbilities();
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
        foreach (var (key,_) in args.Damage.DamageDict)
        {
            if (key == "Heat")
                args.Damage += component.HeatDamageProjectileModifier;
        }
    }
    private void OnFlashBanged(EntityUid uid, ShadowlingComponent component, GetFlashbangedEvent args)
    {
        // Shadowling get damaged from flashbangs
        if (!TryComp<DamageableComponent>(uid, out var damageableComp))
            return;

        _damageable.TryChangeDamage(uid, component.HeatDamage, damageable: damageableComp);
    }
    public void OnThrallAdded(EntityUid uid, ShadowlingComponent comp)
    {
        if (!TryComp<LightDetectionDamageComponent>(uid, out var lightDet))
            return;

        lightDet.ResistanceModifier += comp.LightResistanceModifier;
    }

    public void OnThrallRemoved(EntityUid uid, ShadowlingComponent comp)
    {
        if (!TryComp<LightDetectionDamageComponent>(uid, out var lightDet))
            return;

        lightDet.ResistanceModifier -= comp.LightResistanceModifier;
    }

    private void OnInit(EntityUid uid, ShadowlingComponent component, ref ComponentInit args)
    {
        _actions.AddAction(uid, ref component.ActionHatchEntity, component.ActionHatch);
    }

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
                break;
            }
            case ShadowlingPhases.Ascension:
            {
                // Remove all previous actions
                EntityManager.RemoveComponents(uid, defaultAbilities);
                EntityManager.RemoveComponents(uid, _protoMan.Index(component.ObtainableComponents));

                EntityManager.AddComponents(uid, _protoMan.Index(component.PostAscensionComponents));

                var ev = new ShadowlingAscendEvent();
                RaiseLocalEvent(ev);

                _codeCondition.SetCompleted(uid, component.ObjectiveAscend);
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
                break;
            }
        }
    }

    private void BeforeGunShot(Entity<ShadowlingComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        // Slings cant shoot guns
        if (args.Gun.Comp.ClumsyProof)
            return;

        if (!_random.Prob(0.5f))
            return;

        _damageable.TryChangeDamage(ent, ent.Comp.GunShootFailDamage, origin: ent);

        _stun.TryParalyze(ent, ent.Comp.GunShootFailStunTime, false);

        args.Cancel();
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
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-shadowling"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (HasComp<ThrallComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-already-thrall"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-non-humanoid"), uid, uid, PopupType.SmallCaution);
            return false;
        }

        // Target needs to be alive
        if (!TryComp<MobStateComponent>(target, out var mobState)
            || !_mobStateSystem.IsCritical(target, mobState) && !_mobStateSystem.IsCritical(target, mobState))
            return true;

        _popup.PopupEntity(Loc.GetString("shadowling-enthrall-dead"), uid, uid, PopupType.SmallCaution);
        return false;

    }

    public bool CanGlare(EntityUid target)
    {
        return HasComp<MobStateComponent>(target)
               && !HasComp<ShadowlingComponent>(target)
               && !HasComp<ThrallComponent>(target);
    }

    public void DoEnthrall(EntityUid uid, SimpleDoAfterEvent args)
    {
        if (args.Cancelled
            || args.Target == null)
            return;

        var target = args.Target.Value;

        var thrall = EnsureComp<ThrallComponent>(target);

        if (TryComp<ShadowlingComponent>(uid, out var sling))
        {
            sling.Thralls.Add(target);
            thrall.Converter = uid;

            OnThrallAdded(uid, sling);
        }

        _audio.PlayPvs(
            new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg"),
            target,
            AudioParams.Default);
    }
}
