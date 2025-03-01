using System.Linq;
using Content.Server.Chat.Systems;
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
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Reflect;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.MartialArts;

public sealed class MartialArtsSystem : EntitySystem
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
    [Dependency] private readonly ChatSystem _chat = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanPerformComboComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<CanPerformComboComponent, ComboAttackPerformedEvent>(OnAttackPerformed);

        // Granting - Subscribes
        SubscribeLocalEvent<GrantCqcComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCqcComponent, ExaminedEvent>(OnGrantCQCExamine);
        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotEquippedEvent>(OnGrantCorporateJudo);
        SubscribeLocalEvent<GrantCorporateJudoComponent, ClothingGotUnequippedEvent>(OnRemoveCorporateJudo);
        SubscribeLocalEvent<GrantSleepingCarpComponent, UseInHandEvent>(OnGrantSleepingCarp);


        // Martial Arts - Subscribes
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, CheckGrabOverridesEvent>(CheckGrabStageOverride);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComboAttackPerformedEvent>(OnCQCAttackPerformed);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, EntInsertedIntoContainerMessage>(OnEntInserted); // Stun baton no no

        //CQC Combos - Subscribes
        SubscribeLocalEvent<CanPerformComboComponent, CQCSlamPerformedEvent>(OnCQCSlam);
        SubscribeLocalEvent<CanPerformComboComponent, CQCKickPerformedEvent>(OnCQCKick);
        SubscribeLocalEvent<CanPerformComboComponent, CQCRestrainPerformedEvent>(OnCQCRestrain);
        SubscribeLocalEvent<CanPerformComboComponent, CQCPressurePerformedEvent>(OnCQCPressure);
        SubscribeLocalEvent<CanPerformComboComponent, CQCConsecutivePerformedEvent>(OnCQCConsecutive);

        // Judo - Subscribes
        // discombubulate
        SubscribeLocalEvent<CanPerformComboComponent, JudoThrowPerformedEvent>(OnJudoThrow);
        SubscribeLocalEvent<CanPerformComboComponent, JudoEyePokePerformedEvent>(OnJudoEyepoke);
        SubscribeLocalEvent<CanPerformComboComponent, JudoArmbarPerformedEvent>(OnJudoArmbar);
        SubscribeLocalEvent<CanPerformComboComponent, JudoGoldenBlastPerformedEvent>(OnJudoGoldenBlast);
        // wheel throw

        #region Sleeping Carp - Subscribes
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpGnashingTeethPerformedEvent>(OnSleepingCarpGnashing);
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpKneeHaulPerformedEvent>(OnSleepingCarpKneeHaul);
        SubscribeLocalEvent<CanPerformComboComponent, SleepingCarpCrashingWavesPerformedEvent>(OnSleepingCarpCrashingWaves);

        #endregion
    }

    #region CanPerformCombo
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
        while (query.MoveNext(out _, out var comp))
        {
            if (_timing.CurTime < comp.ResetTime || comp.LastAttacks.Count <= 0)
                continue;
            comp.LastAttacks.Clear();
            comp.ConsecutiveGnashes = 0;
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
        CheckCombo(uid, component);
    }

    private void CheckCombo(EntityUid uid, CanPerformComboComponent comp)
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
    #endregion

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

        if (!CheckGrant(ent.Comp, args.User))
            return;
        _popupSystem.PopupEntity(Loc.GetString("cqc-success-learned"), args.User, args.User);
        var cqc = EnsureComp<MartialArtsKnowledgeComponent>(args.User);
        LoadPrototype(args.User, cqc, ent.Comp.MartialArtsForm);
        cqc.Blocked = false;
        ent.Comp.Used = true;
    }
    private bool CheckGrant(GrantMartialArtKnowledgeComponent comp, EntityUid user)
    {
        if (HasComp<CanPerformComboComponent>(user))
        {
            if (!TryComp<MartialArtsKnowledgeComponent>(user, out var cqc))
            {
                _popupSystem.PopupEntity(Loc.GetString("cqc-fail-knowanother"), user, user);
                return false;
            }

            if (cqc.Blocked)
            {
                _popupSystem.PopupEntity(Loc.GetString("cqc-success-unblocked"), user, user);
                cqc.Blocked = false;
                comp.Used = true;
                return false;
            }

            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-already"), user, user);
            return false;
        }
        return true;
    }

    private void OnGrantCQCExamine(Entity<GrantCqcComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Used)
            args.PushMarkup(Loc.GetString("cqc-manual-used", ("manual", Identity.Entity(ent, EntityManager))));
    }


    private void OnGrantCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotEquippedEvent args)
    {
        var user = args.Wearer;
        if (!CheckGrant(ent.Comp, user))
            return;
        var martialArts = EnsureComp<MartialArtsKnowledgeComponent>(user);
        LoadPrototype(user, martialArts, ent.Comp.MartialArtsForm);
        martialArts.Blocked = false;
    }

    private void OnGrantSleepingCarp(Entity<GrantSleepingCarpComponent> ent, ref UseInHandEvent args)
    {
        if(ent.Comp.Used)
            return;
        if (ent.Comp.UseAgainTime == TimeSpan.Zero)
        {
            CarpScrollDelay(ent,args.User);
            return;
        }

        if (_timing.CurTime < ent.Comp.UseAgainTime)
        {
            _popupSystem.PopupEntity("The journey of a thousand miles begins with one step, and the path of wisdom is traveled slowly, one lesson at a time.", ent, args.User, PopupType.MediumCaution); // localize
            return;
        }

        switch (ent.Comp.Stage)
        {
            case < 3:
                CarpScrollDelay(ent,args.User);
                break;
            case >= 3:
                if (!CheckGrant(ent.Comp, args.User))
                    return;
                var martialArts = EnsureComp<MartialArtsKnowledgeComponent>(args.User);
                var userReflect = EnsureComp<ReflectComponent>(args.User);
                LoadPrototype(args.User, martialArts, ent.Comp.MartialArtsForm);
                martialArts.Blocked = false;
                userReflect.ReflectProb = 1;
                userReflect.Spread = 75;
                userReflect.OtherTypeReflectProb = 0.25f;
                ent.Comp.Used = true;
                _popupSystem.PopupEntity("You are now a master of the Way of the Sleeping Carp.", ent, args.User, PopupType.LargeCaution); // localize // localize
                return;
        }
    }

    private void CarpScrollDelay(Entity<GrantSleepingCarpComponent> ent, EntityUid user)
    {
        var time = new Random().Next(1, 2); // set to comp variables
        ent.Comp.UseAgainTime = _timing.CurTime + TimeSpan.FromSeconds(time);
        ent.Comp.Stage++;
        _popupSystem.PopupEntity("You have taken one step closer to becoming a master of the Way of the Sleeping Carp.", ent, user,  PopupType.Medium); // localize
    }

    private void OnRemoveCorporateJudo(Entity<GrantCorporateJudoComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        var user = args.Wearer;
        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }

    private void OnEntInserted(EntityUid ent, MartialArtsKnowledgeComponent comp, ref EntInsertedIntoContainerMessage args)
    {
        if (!HasComp<JudoBlockedComponent>(args.Entity))
            return;

        if(comp.MartialArtsForm != MartialArtsForms.CorporateJudo)
            return;

        var item = _hands.GetActiveItem(ent);
        if (item != null)
            _hands.TryDrop(item.Value);
        _popupSystem.PopupEntity("Judo forbids the use of stun batons", ent, PopupType.MediumCaution);
    }

    private void LoadCombos(ProtoId<ComboListPrototype> list, CanPerformComboComponent combo)
    {
        combo.AllowedCombos.Clear();
        if (!_proto.TryIndex(list, out var comboListPrototype))
            return;
        foreach (var item in comboListPrototype.Combos)
        {
            combo.AllowedCombos.Add(_proto.Index(item));
        }
    }

    private void LoadPrototype(EntityUid uid, MartialArtsKnowledgeComponent component, MartialArtsForms name)
    {
        var combo = EnsureComp<CanPerformComboComponent>(uid);
        if (!_proto.TryIndex<MartialArtPrototype>(name.ToString(), out var martialArtsPrototype))
            return;
        component.MartialArtsForm = martialArtsPrototype.MartialArtsForm;
        component.RoundstartCombos = martialArtsPrototype.RoundstartCombos;
        component.MinDamageModifier = martialArtsPrototype.MinDamageModifier;
        component.MaxDamageModifier = martialArtsPrototype.MaxDamageModifier;
        component.RandomDamageModifier = martialArtsPrototype.RandomDamageModifier;
        component.HarmAsStamina = martialArtsPrototype.HarmAsStamina;
        component.RandomSayings = martialArtsPrototype.RandomSayings;
        component.RandomSayingsDowned = martialArtsPrototype.RandomSayingsDowned;
        LoadCombos(martialArtsPrototype.RoundstartCombos, combo);
    }

    private void OnShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        var combo = EnsureComp<CanPerformComboComponent>(ent);
        combo.AllowedCombos.Clear();
    }

    #endregion

    #region Judo

    private void OnJudoThrow(Entity<CanPerformComboComponent> ent, ref JudoThrowPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;
        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(7), false);
        _stamina.TakeStaminaDamage(target, 25f);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);

    }

    private void OnJudoEyepoke(Entity<CanPerformComboComponent> ent, ref JudoEyePokePerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;
        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        if (!TryComp(target, out StatusEffectsComponent? status))
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(target,
            "TemporaryBlindness",
            TimeSpan.FromSeconds(2),
            true,
            status);
        _status.TryAddStatusEffect<BlurryVisionComponent>(target,
            "BlurryVision",
            TimeSpan.FromSeconds(5),
            false,
            status);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    private void OnJudoArmbar(Entity<CanPerformComboComponent> ent, ref JudoArmbarPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (!TryComp<RequireProjectileTargetComponent>(target, out var downed))
            return;

        switch (downed.Active)
        {
            case false:
                var item = _hands.GetActiveItem(target);
                if (item != null)
                    _hands.TryDrop(target, item.Value);
                break;
            case true:
                _stamina.TakeStaminaDamage(target, 45f);
                _stun.TryParalyze(target, TimeSpan.FromSeconds(5), false);
                Log.Info("IN ARM BAR");
                _popupSystem.PopupEntity("You were placed in an arm bar", target);
                break;
        }
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }

    private void OnJudoGoldenBlast(Entity<CanPerformComboComponent> ent, ref JudoGoldenBlastPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;
        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CorporateJudo))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(30), false);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    #endregion

    #region CQC
    private void OnCQCAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledgeComponent) && !knowledgeComponent.Blocked)
            return;

        if (args.Type != ComboAttackType.Disarm)
            return;
        if (!_random.Prob(0.5f)) // random chance to steal items? this
            return;

        var item = _hands.GetActiveItem(args.Target);

        if (item == null)
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
        _grabThrowing.Throw(target, ent, dir, 25f, damage, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
    }

    private void OnCQCRestrain(Entity<CanPerformComboComponent> ent, ref CQCRestrainPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        _stun.TryParalyze(target, TimeSpan.FromSeconds(10), true);
        _stamina.TakeStaminaDamage(target, 30f, source: ent);
    }

    private void OnCQCPressure(Entity<CanPerformComboComponent> ent, ref CQCPressurePerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (!_hands.TryGetActiveItem(target, out var activeItem))
            return;
        _hands.TryDrop(target, activeItem.Value);
        _hands.TryPickupAnyHand(ent, activeItem.Value);
        _stamina.TakeStaminaDamage(target, 65f, source: ent);
    }

    private void OnCQCConsecutive(Entity<CanPerformComboComponent> ent, ref CQCConsecutivePerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.CloseQuartersCombat))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 20);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stamina.TakeStaminaDamage(target, 70, source: ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
    }

    private bool CheckCanUseMartialArt(EntityUid uid, MartialArtsForms form)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(uid, out var knowledgeComponent)
            && !knowledgeComponent.Blocked)
        {
            Log.Debug(form + ":" + knowledgeComponent.MartialArtsForm);
            if(knowledgeComponent.MartialArtsForm == form)
                return true;
        }

        foreach (var ent in _lookup.GetEntitiesInRange(uid, 8f))
        {
            if (TryPrototype(ent, out var proto) && proto.ID == "DefaultStationBeaconKitchen")
                return true;
        }

        return false;
    }

    #endregion

    #region Sleeping Carp

    private void OnSleepingCarpGnashing(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpGnashingTeethPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.SleepingCarp))
            return;

        if(!TryComp<MartialArtsKnowledgeComponent>(ent.Owner, out var knowledgeComponent))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Slash", 20 + ent.Comp.ConsecutiveGnashes * 5) ;
        _damageable.TryChangeDamage(target, damage, origin: ent);
        ent.Comp.ConsecutiveGnashes++;
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
        if (TryComp<RequireProjectileTargetComponent>(target, out var standing)
            && !standing.Active)
        {
            var saying =
                knowledgeComponent.RandomSayings.ElementAt(
                    _random.Next(knowledgeComponent.RandomSayings.Count));
            _chat.TrySendInGameICMessage(ent, Loc.GetString(saying), InGameICChatType.Speak, false);
        }
        else
        {
            var saying =
                knowledgeComponent.RandomSayingsDowned.ElementAt(_random.Next(knowledgeComponent.RandomSayingsDowned.Count));
            _chat.TrySendInGameICMessage(ent, Loc.GetString( saying) , InGameICChatType.Speak, false);
        }
    }

    private void OnSleepingCarpKneeHaul(Entity<CanPerformComboComponent> ent,
        ref SleepingCarpKneeHaulPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.SleepingCarp))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 10);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        _stamina.TakeStaminaDamage(target, 60f);
        _stun.TryParalyze(target, TimeSpan.FromSeconds(6), true);
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
    }
    private void OnSleepingCarpCrashingWaves(Entity<CanPerformComboComponent> ent, ref SleepingCarpCrashingWavesPerformedEvent args)
    {
        if (ent.Comp.CurrentTarget == null)
            return;

        if (!CheckCanUseMartialArt(ent, MartialArtsForms.SleepingCarp))
            return;

        var target = ent.Comp.CurrentTarget.Value;

        if (TryComp<RequireProjectileTargetComponent>(target, out var downed) && downed.Active)
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 5);
        _damageable.TryChangeDamage(target, damage, origin: ent);
        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;
        dir *= 1f / dir.Length();
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);
        _grabThrowing.Throw(target, ent, dir, 25f, damage, damage);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit2.ogg"), target);
    }

    #endregion
}
