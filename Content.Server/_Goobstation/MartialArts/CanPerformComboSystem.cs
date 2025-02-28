using System.Linq;
using Content.Server.Hands.Systems;
using Content.Server.Popups;
using Content.Server.Stunnable;
using Content.Shared._Goobstation.MartialArts;
using Content.Shared._Goobstation.MartialArts.Events;
using Content.Shared._White.Grab;
using Content.Shared.Bed.Sleep;
using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.MartialArts;

public sealed class ComboSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrowing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly HandsSystem _hands = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanPerformComboComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CanPerformComboComponent, ComboAttackPerformedEvent>(OnAttackPerformed);

        // Granting martial arts
        SubscribeLocalEvent<GrantCqcComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCqcComponent, ExaminedEvent>(OnGrantCQCExamine);
        SubscribeLocalEvent<GrantCorporateJudo, ClothingGotEquippedEvent>(OnGrantCorporateJudo);
        // Here comes CQC!
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CheckGrabOverridesEvent>(CheckGrabStageOverride);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComboAttackPerformedEvent>(OnCQCAttackPerformed);

        SubscribeLocalEvent<CanPerformComboComponent, CQCSlamPerformedEvent>(OnCQCSlam);
        SubscribeLocalEvent<CanPerformComboComponent, CQCKickPerformedEvent>(OnCQCKick);
        SubscribeLocalEvent<CanPerformComboComponent, CQCRestrainPerformedEvent>(OnCQCRestrain);
        SubscribeLocalEvent<CanPerformComboComponent, CQCPressurePerformedEvent>(OnCQCPressure);
        SubscribeLocalEvent<CanPerformComboComponent, CQCConsecutivePerformedEvent>(OnCQCConsecutive);
    }

    private void OnMapInit(EntityUid uid, CanPerformComboComponent component, MapInitEvent args)
    {
        foreach (var item in component.RoundstartCombos)
        {
            component.AllowedCombos.Add(_proto.Index(item));
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CanPerformComboComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.ResetTime && comp.LastAttacks.Count > 0)
                comp.LastAttacks.Clear();
        }
    }

    private void OnAttackPerformed(EntityUid uid, CanPerformComboComponent component, ComboAttackPerformedEvent args)
    {
        if (!HasComp<MobStateComponent>(args.Target))
            return;

        if (component.CurrentTarget != null && args.Target != component.CurrentTarget.Value)
        {
            component.LastAttacks.Clear();
        }

        if (args.Weapon != uid)
        {
            component.LastAttacks.Clear();
            return;
        }

        component.CurrentTarget = args.Target;
        component.ResetTime = _timing.CurTime + TimeSpan.FromSeconds(4);
        component.LastAttacks.Add(args.Type);
        CheckCombo(uid, args.Target, component);
    }

    private void CheckCombo(EntityUid uid, EntityUid target, CanPerformComboComponent comp)
    {
        var success = false;

        foreach (var proto in comp.AllowedCombos)
        {
            if (success)
                break;

            var sum = comp.LastAttacks.Count - proto.AttackTypes.Count;
            if (sum < 0)
                continue;

            var list = comp.LastAttacks.GetRange(sum, proto.AttackTypes.Count).AsEnumerable();
            var attackList = proto.AttackTypes.AsEnumerable();

            if (!list.SequenceEqual(attackList) || proto.ResultEvent == null)
                continue;
            var ev = proto.ResultEvent;
            RaiseLocalEvent(uid, ev);
            comp.LastAttacks.Clear();
        }
    }

    private void CheckGrabStageOverride<T>(EntityUid uid, T component, CheckGrabOverridesEvent args)
        where T : GrabStagesOverrideComponent
    {
        if (args.Stage == GrabStage.Soft)
            args.Stage = component.StartingStage;
    }

    #region Granting

    private void OnGrantCQCUse(Entity<GrantCqcComponent> ent, ref UseInHandEvent args)
    {
        if (ent.Comp.Used)
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-used", ("manual", Identity.Entity(ent, EntityManager))),
                args.User,
                args.User);
            return;
        }

        if (HasComp<CanPerformComboComponent>(args.User))
        {
            if (!TryComp<MartialArtsKnowledgeComponent>(args.User, out var cqc))
            {
                _popupSystem.PopupEntity(Loc.GetString("cqc-success-knowanother"), args.User, args.User);
                return;
            }

            if (cqc.Blocked)
            {
                _popupSystem.PopupEntity(Loc.GetString("cqc-success-unblocked"), args.User, args.User);
                cqc.Blocked = false;
                ent.Comp.Used = true;
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-already"), args.User, args.User);
        }
        else
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-success-learned"), args.User, args.User);
            var cqc = EnsureComp<MartialArtsKnowledgeComponent>(args.User);
            cqc.Blocked = false;
            ent.Comp.Used = true;
        }
    }

    private void OnGrantCQCExamine(Entity<GrantCqcComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Used)
            args.PushMarkup(Loc.GetString("cqc-manual-used", ("manual", Identity.Entity(ent, EntityManager))));
    }


    private void OnGrantCorporateJudo(Entity<GrantCorporateJudo> ent, ref ClothingGotEquippedEvent args)
    {
        var user = args.Wearer;
        var martialArts = EnsureComp<MartialArtsKnowledgeComponent>(user);
        martialArts.RoundstartCombos = "CorporateJudoMoves";
        martialArts.Blocked = false;
    }

    private void OnRemoveCorporateJudo(Entity<GrantCorporateJudo> ent, ref ClothingGotEquippedEvent args)
    {
        var user = args.Wearer;
        var martialArts = RemComp<MartialArtsKnowledgeComponent>(user);
        if(!martialArts)
            Log.Error("Failed to remove corporate judo");
    }

    #endregion

    #region CQC

    private void OnMapInit(Entity<MartialArtsKnowledgeComponent> ent, ref MapInitEvent args)
    {
        var combo = EnsureComp<CanPerformComboComponent>(ent);
        if (!_proto.TryIndex(ent.Comp.RoundstartCombos, out var comboListPrototype))
            return;
        Logger.Debug(ent.Comp.RoundstartCombos.Id);
        foreach (var item in comboListPrototype.Combos)
        {
            combo.AllowedCombos.Add(_proto.Index(item));
        }
    }

    private void OnShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        var combo = EnsureComp<CanPerformComboComponent>(ent);
        if (!_proto.TryIndex(ent.Comp.RoundstartCombos, out var comboListPrototype))
            return;
        foreach (var item in comboListPrototype.Combos)
        {
            combo.AllowedCombos.Remove(_proto.Index(item));
        }
    }

    private void OnCQCAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat))
            return;

        if (args.Type != ComboAttackType.Disarm)
            return;
        if (!_random.Prob(0.5f))
            return;

        var item = _hands.GetActiveItem(args.Target);

        if (item == null)
            return;
        if (!HasComp<MeleeWeaponComponent>(item.Value) && !HasComp<GunComponent>(item.Value))
            return;
        _hands.TryDrop(args.Target, item.Value);
        _hands.TryPickupAnyHand(ent, item.Value);
        _stamina.TakeStaminaDamage(args.Target, 10f);
    }

    private void OnCQCSlam(Entity<CanPerformComboComponent> ent, ref CQCSlamPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stun.TryParalyze(target, TimeSpan.FromSeconds(12), true);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }

    private void OnCQCKick(Entity<CanPerformComboComponent> ent, ref CQCKickPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (!TryComp<RequireProjectileTargetComponent>(target, out var downed) || !downed.Active)
            return;

        if (TryComp<StaminaComponent>(target, out var stamina) && stamina.Critical)
        {
            _status.TryAddStatusEffect<ForcedSleepingComponent>(target, "ForcedSleep", TimeSpan.FromSeconds(10), true);
        }

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stamina.TakeStaminaDamage(target, 55f, source: ent);

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 1f / dir.Length();
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, dir, 120f, damage, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
    }

    private void OnCQCRestrain(EntityUid uid, CanPerformComboComponent component, CQCRestrainPerformedEvent args)
    {
        if (component.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(uid, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = component.CurrentTarget.Value;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(10), true);
        _stamina.TakeStaminaDamage(target, 30f, source: uid);
    }

    private void OnCQCPressure(EntityUid uid, CanPerformComboComponent component, CQCPressurePerformedEvent args)
    {
        if (component.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(uid, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = component.CurrentTarget.Value;

        _stamina.TakeStaminaDamage(target, 65f, source: uid);
    }

    private void OnCQCConsecutive(EntityUid uid, CanPerformComboComponent component, CQCConsecutivePerformedEvent args)
    {
        if (component.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(uid, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = component.CurrentTarget.Value;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 25);
        _damageable.TryChangeDamage(target, damage, origin: uid);
        _stamina.TakeStaminaDamage(target, 55f, source: uid);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
    }

    private bool CheckCanUseMartialArt(EntityUid uid, MartialArtsForms form)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(uid, out var knowledgeComponent)
            && !knowledgeComponent.Blocked
            && knowledgeComponent.MartialArtsForm == form)
            return true;

        foreach (var ent in _lookup.GetEntitiesInRange(uid, 8f))
        {
            if (TryPrototype(ent, out var proto) && proto.ID == "DefaultStationBeaconKitchen")
                return true;
        }

        return false;
    }

    #endregion
}
