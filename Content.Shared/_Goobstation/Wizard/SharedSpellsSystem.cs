using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Goobstation.Wizard.BindSoul;
using Content.Shared._Goobstation.Wizard.Chuuni;
using Content.Shared._Goobstation.Wizard.InstantSummons;
using Content.Shared._Goobstation.Wizard.LesserSummonGuns;
using Content.Shared._Goobstation.Wizard.Mutate;
using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared._Goobstation.Wizard.SanguineStrike;
using Content.Shared._Goobstation.Wizard.SpellCards;
using Content.Shared._Goobstation.Wizard.Swap;
using Content.Shared._Goobstation.Wizard.Teleport;
using Content.Shared._Goobstation.Wizard.TeslaBlast;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Access.Components;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Clumsy;
using Content.Shared.Cluwne;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Ghost;
using Content.Shared.Gibbing.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Jittering;
using Content.Shared.Magic;
using Content.Shared.Magic.Components;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.PDA;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Roles;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard;

public abstract class SharedSpellsSystem : EntitySystem
{
    #region Dependencies

    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] protected readonly IMapManager MapManager = default!;
    [Dependency] protected readonly IPrototypeManager ProtoMan = default!;
    [Dependency] protected readonly SharedTransformSystem TransformSystem = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] protected readonly SharedMapSystem Map = default!;
    [Dependency] protected readonly SharedStunSystem Stun = default!;
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] protected readonly SharedMindSystem Mind = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedHandsSystem Hands = default!;
    [Dependency] protected readonly MetaDataSystem Meta = default!;
    [Dependency] protected readonly SharedBodySystem Body = default!;
    [Dependency] protected readonly NpcFactionSystem Faction = default!;
    [Dependency] protected readonly SharedRoleSystem Role = default!;
    [Dependency] protected readonly DamageableSystem Damageable = default!;
    [Dependency] protected readonly GrammarSystem Grammar = default!;
    [Dependency] private   readonly INetManager _net = default!;
    [Dependency] private   readonly IGameTiming _timing = default!;
    [Dependency] private   readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private   readonly InventorySystem _inventory = default!;
    [Dependency] private   readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private   readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private   readonly SharedMagicSystem _magic = default!;
    [Dependency] private   readonly SharedPopupSystem _popup = default!;
    [Dependency] private   readonly SharedGunSystem _gunSystem = default!;
    [Dependency] private   readonly MobStateSystem _mobState = default!;
    [Dependency] private   readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private   readonly SharedBindSoulSystem _bindSoul = default!;
    [Dependency] private   readonly SharedTeslaBlastSystem _teslaBlast = default!;
    [Dependency] private   readonly SharedActionsSystem _actions = default!;
    [Dependency] private   readonly ExamineSystemShared _examine = default!;
    [Dependency] private   readonly TagSystem _tag = default!;
    [Dependency] private   readonly SharedAudioSystem _audio = default!;
    [Dependency] private   readonly ConfirmableActionSystem _confirmableAction = default!;
    [Dependency] private   readonly SharedWizardTeleportSystem _teleport = default!;
    [Dependency] private   readonly PullingSystem _pulling = default!;

    #endregion

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CluwneCurseEvent>(OnCluwneCurse);
        SubscribeLocalEvent<BananaTouchEvent>(OnBananaTouch);
        SubscribeLocalEvent<MimeMalaiseEvent>(OnMimeMalaise);
        SubscribeLocalEvent<MagicMissileEvent>(OnMagicMissile);
        SubscribeLocalEvent<DisableTechEvent>(OnDisableTech);
        SubscribeLocalEvent<SmokeSpellEvent>(OnSmoke);
        SubscribeLocalEvent<RepulseEvent>(OnRepulse);
        SubscribeLocalEvent<StopTimeEvent>(OnStopTime);
        SubscribeLocalEvent<CorpseExplosionEvent>(OnCorpseExplosion);
        SubscribeLocalEvent<BlindSpellEvent>(OnBlind);
        SubscribeLocalEvent<BindSoulEvent>(OnBindSoul);
        SubscribeLocalEvent<PolymorphSpellEvent>(OnPolymorph);
        SubscribeLocalEvent<MutateSpellEvent>(OnMutate);
        SubscribeLocalEvent<TeslaBlastEvent>(OnTeslaBlast);
        SubscribeLocalEvent<LightningBoltEvent>(OnLightningBolt);
        SubscribeLocalEvent<HomingToolboxEvent>(OnHomingToolbox);
        SubscribeLocalEvent<SpellCardsEvent>(OnSpellCards);
        SubscribeLocalEvent<ArcaneBarrageEvent>(OnArcaneBarrage);
        SubscribeLocalEvent<LesserSummonGunsEvent>(OnLesserSummonGuns);
        SubscribeLocalEvent<BarnyardCurseEvent>(OnBarnyardCurse);
        SubscribeLocalEvent<ScreamForMeEvent>(OnScreamForMe);
        SubscribeLocalEvent<InstantSummonsEvent>(OnInstantSummons);
        SubscribeLocalEvent<WizardTeleportEvent>(OnTeleport);
        SubscribeLocalEvent<TrapsSpellEvent>(OnTraps);
        SubscribeLocalEvent<LesserSummonBeesEvent>(OnBees);
        SubscribeLocalEvent<SummonSimiansEvent>(OnSimians);
        SubscribeLocalEvent<ExsanguinatingStrikeEvent>(OnExsangunatingStrike);
        SubscribeLocalEvent<ChuuniInvocationsEvent>(OnChuuniInvocations);
        SubscribeLocalEvent<SwapSpellEvent>(OnSwap);

        SubscribeAllEvent<SetSwapSecondaryTarget>(OnSwapSecondaryTarget);
    }

    private void OnSwapSecondaryTarget(SetSwapSecondaryTarget ev)
    {
        var action = GetEntity(ev.Action);
        var target = GetEntity(ev.Target);

        if (TryComp(action, out SwapSpellComponent? swap))
            swap.SecondaryTarget = target;
    }

    #region Spells

    private void OnCluwneCurse(CluwneCurseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (TryComp(ev.Target, out StatusEffectsComponent? status))
        {
            Stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);
            _jitter.DoJitter(ev.Target, ev.StutterDuration, true, status: status);
        }

        EnsureComp<CluwneComponent>(ev.Target);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBananaTouch(BananaTouchEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (TryComp(ev.Target, out StatusEffectsComponent? status))
        {
            Stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);
            _jitter.DoJitter(ev.Target, ev.JitterStutterDuration, true, status: status);
            _stutter.DoStutter(ev.Target, ev.JitterStutterDuration, true, status);
        }

        var targetWizard = HasComp<WizardComponent>(ev.Target);

        if (!targetWizard)
            EnsureComp<ClumsyComponent>(ev.Target);

        SetGear(ev.Target, ev.Gear, !targetWizard);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnMimeMalaise(MimeMalaiseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        Stun.TryParalyze(ev.Target, ev.ParalyzeDuration, true, status);

        var targetWizard = HasComp<WizardComponent>(ev.Target);

        SetGear(ev.Target, ev.Gear, !targetWizard);

        if (!targetWizard)
            MakeMime(ev.Target);
        else
            _statusEffects.TryAddStatusEffect<MutedComponent>(ev.Target, "Muted", ev.WizardMuteDuration, true, status);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnMagicMissile(MagicMissileEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var ghostQuery = GetEntityQuery<GhostComponent>();
        var spectralQuery = GetEntityQuery<SpectralComponent>();

        var (coords, mapCoords, spawnCoords, velocity) = GetProjectileData(ev.Performer);

        var targets = Lookup.GetEntitiesInRange<StatusEffectsComponent>(coords, ev.Range, LookupFlags.Dynamic);
        var hasTargets = false;

        foreach (var (target, _) in targets)
        {
            if (target == ev.Performer)
                continue;

            if (ghostQuery.HasComp(target) || spectralQuery.HasComp(target))
                continue;

            hasTargets = true;

            if (_net.IsClient)
                break;

            SpawnHomingProjectile(ev.Proto,
                spawnCoords,
                target,
                ev.Performer,
                mapCoords,
                velocity,
                ev.ProjectileSpeed,
                false);
        }

        if (!hasTargets)
        {
            Popup(ev.Performer, "spell-fail-no-targets");
            return;
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnDisableTech(DisableTechEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        Emp(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnSmoke(SmokeSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        SpawnSmoke(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnRepulse(RepulseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        Repulse(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnStopTime(StopTimeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (_net.IsServer)
        {
            var effect = Spawn(ev.Proto, TransformSystem.GetMapCoordinates(ev.Performer));
            EnsureComp<PreventCollideComponent>(effect).Uid = ev.Performer; // Just in case
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnCorpseExplosion(CorpseExplosionEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<BorgChassisComponent>(ev.Target))
        {
            Popup(ev.Performer, "spell-fail-target-borg");
            return;
        }

        if (!_mobState.IsDead(ev.Target))
        {
            Popup(ev.Performer, "spell-fail-not-dead");
            return;
        }

        var coords = TransformSystem.GetMapCoordinates(ev.Target);

        if (_timing.IsFirstTimePredicted)
            Body.GibBody(ev.Target, contents: GibContentsOption.Gib);

        ExplodeCorpse(ev);

        var targets = Lookup.GetEntitiesInRange<DamageableComponent>(coords, ev.KnockdownRange);
        var ghostQuery = GetEntityQuery<GhostComponent>();
        var spectralQuery = GetEntityQuery<SpectralComponent>();
        var statusQuery = GetEntityQuery<StatusEffectsComponent>();
        var bodyPartQuery = GetEntityQuery<BodyPartComponent>();
        foreach (var (target, damageable) in targets)
        {
            if (target == ev.Performer || target == ev.Target)
                continue;

            if (ghostQuery.HasComp(target) || spectralQuery.HasComp(target) || bodyPartQuery.HasComp(target))
                continue;

            var range = (TransformSystem.GetMapCoordinates(target).Position - coords.Position).Length();

            range = MathF.Max(1f, range);

            Damageable.TryChangeDamage(target,
                ev.Damage / range,
                damageable: damageable,
                origin: ev.Performer,
                targetPart: TargetBodyPart.All);

            if (!statusQuery.TryComp(target, out var status))
                continue;

            if (HasComp<SiliconComponent>(target) || HasComp<BorgChassisComponent>(target))
                Stun.TryParalyze(target, ev.SiliconStunTime / range, true, status);
            else
                Stun.TryKnockdown(target, ev.KnockdownTime / range, true, status);
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBlind(BlindSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<GhostComponent>(ev.Target) || HasComp<SpectralComponent>(ev.Target))
            return;

        if (!TryComp(ev.Target, out StatusEffectsComponent? status))
            return;

        _statusEffects.TryAddStatusEffect<TemporaryBlindnessComponent>(ev.Target,
            "TemporaryBlindness",
            ev.BlindDuration,
            true,
            status);

        _statusEffects.TryAddStatusEffect<BlurryVisionComponent>(ev.Target,
            "BlurryVision",
            ev.BlurDuration,
            true,
            status);

        if (_net.IsServer)
        {
            if (TryComp(ev.Target, out VocalComponent? vocal) && !HasComp<BorgChassisComponent>(ev.Target))
                Emote(ev.Target, vocal.ScreamId);

            if (ev.Effect != null)
                Spawn(ev.Effect.Value, Transform(ev.Target).Coordinates);
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBindSoul(BindSoulEvent ev)
    {
        if (ev.Handled)
            return;

        if (_mobState.IsCritical(ev.Performer))
            return;

        if (!Mind.TryGetMind(ev.Performer, out var mind, out var mindComponent))
            return;

        TryComp<SoulBoundComponent>(mind, out var soulBound);

        if (Mind.IsCharacterDeadIc(mindComponent))
        {
            if (soulBound == null)
            {
                Popup(ev.Performer, "spell-fail-soul-not-bound");
                return;
            }

            if (!HasComp<PhylacteryComponent>(soulBound.Item))
            {
                Popup(ev.Performer, "spell-fail-item-destroyed");
                return;
            }

            if (!TryComp(soulBound.Item, out TransformComponent? xform) || xform.MapUid == null ||
                xform.MapUid != soulBound.MapId)
            {
                Popup(ev.Performer, "spell-fail-item-on-another-plane");
                return;
            }

            _bindSoul.Resurrect(mind, soulBound.Item.Value, mindComponent, soulBound);
            ev.Handled = true;
            return;
        }

        if (HasComp<GhostComponent>(ev.Performer))
            return;

        if (soulBound != null)
        {
            Popup(ev.Performer, "spell-fail-no-soul");
            return;
        }

        if (!_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<SiliconComponent>(ev.Performer) || HasComp<BorgChassisComponent>(ev.Performer))
        {
            Popup(ev.Performer, "spell-fail-bind-soul-silicon");
            return;
        }

        if (!TryComp(ev.Performer, out HandsComponent? hands) || hands.ActiveHandEntity == null)
        {
            Popup(ev.Performer, "spell-fail-no-held-entity");
            return;
        }

        var item = hands.ActiveHandEntity.Value;

        if (HasComp<UnremoveableComponent>(item) || !HasComp<ItemComponent>(item))
        {
            PopupLoc(ev.Performer, Loc.GetString("spell-fail-unremoveable", ("item", item)));
            return;
        }

        if (_whitelist.IsValid(ev.Blacklist, item))
        {
            PopupLoc(ev.Performer, Loc.GetString("spell-fail-soul-item-not-suitable", ("item", item)));
            return;
        }

        BindSoul(ev, item, mind, mindComponent);
        ev.Handled = true;
    }

    private void OnPolymorph(PolymorphSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        ev.Handled = Polymorph(ev);
    }

    private void OnMutate(MutateSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<SiliconComponent>(ev.Performer) || HasComp<BorgChassisComponent>(ev.Performer))
        {
            Popup(ev.Performer, "spell-fail-mutate-silicon");
            return;
        }

        EnsureComp<HulkComponent>(ev.Performer).Duration = ev.Duration;

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnTeslaBlast(TeslaBlastEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (TryComp(ev.Performer, out CastingTeslaBlastComponent? casting))
        {
            _teslaBlast.CancelDoAfter(ev.Performer, casting);

            _magic.Speak(ev);
            ev.Handled = true;
            return;
        }

        _teslaBlast.StartCharging(ev);
    }

    private void OnLightningBolt(LightningBoltEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!_examine.InRangeUnOccluded(ev.Performer, ev.Target, SharedInteractionSystem.MaxRaycastRange))
        {
            Popup(ev.Performer, "spell-fail-lightning-bolt");
            return;
        }

        _teslaBlast.ShootLightning(ev.Performer, ev.Target, ev.Proto, ev.Damage);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnHomingToolbox(HomingToolboxEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!ValidateLockOnAction(ev))
            return;

        if (_net.IsServer)
        {
            var (_, mapCoords, spawnCoords, velocity) = GetProjectileData(ev.Performer);

            SpawnHomingProjectile(ev.Proto,
                spawnCoords,
                ev.Entity,
                ev.Performer,
                mapCoords,
                velocity,
                ev.ProjectileSpeed,
                true,
                ev.Coords == null ? null : TransformSystem.ToMapCoordinates(ev.Coords.Value));
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnSpellCards(SpellCardsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!ValidateLockOnAction(ev))
            return;

        if (!TryComp(ev.Action.Owner, out SpellCardsActionComponent? spellCardsAction))
            return;

        ShootSpellCards(ev, spellCardsAction.PurpleCard ? ev.PurpleProto : ev.RedProto);

        spellCardsAction.PurpleCard = !spellCardsAction.PurpleCard;

        _magic.Speak(ev);
        ev.Handled = true;
        if (_net.IsClient)
            return;
        spellCardsAction.UsesLeft--;
        if (spellCardsAction.UsesLeft > 0)
            _actions.SetUseDelay(ev.Action, TimeSpan.FromSeconds(0.5));
        else
        {
            _actions.SetUseDelay(ev.Action, ev.UseDelay);
            spellCardsAction.UsesLeft = spellCardsAction.CastAmount;
            RaiseNetworkEvent(new StopTargetingEvent(), ev.Performer);
        }
    }

    private void OnArcaneBarrage(ArcaneBarrageEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (SpawnItemInHands(ev.Performer, ev.Proto, ev.Action) == null)
            return;

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnLesserSummonGuns(LesserSummonGunsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        var gun = SpawnItemInHands(ev.Performer, ev.Proto, ev.Action);
        if (gun == null)
            return;

        var comp = EnsureComp<EnchantedBoltActionRifleComponent>(gun.Value);
        comp.Caster = ev.Performer;
        Dirty(gun.Value, comp);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBarnyardCurse(BarnyardCurseEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (ev.Masks.Count == 0)
            return;

        if (!TryComp(ev.Target, out InventoryComponent? inventory))
            return;

        if (!_inventory.HasSlot(ev.Target, "mask", inventory))
        {
            Popup(ev.Performer, "spell-fail-target-cant-wear-mask");
            return;
        }

        if (_inventory.TryGetSlotEntity(ev.Target, "mask", out var ent, inventory) &&
            HasComp<UnremoveableComponent>(ent) && _tag.HasTag(ent.Value, ev.CursedMaskTag))
        {
            Popup(ev.Performer, "spell-fail-target-cursed");
            return;
        }

        if (_net.IsClient)
            return;

        var (maskEnt, sound) = Random.Pick(ev.Masks);

        var gear = new Dictionary<string, EntProtoId>
        {
            { "mask", maskEnt },
        };

        SetGear(ev.Target, gear, inventoryComponent: inventory);

        if (sound != null)
            _audio.PlayEntity(sound, Filter.Pvs(ev.Target), ev.Target, true);

        // This should transform into animal noise
        Speak(ev.Target, "!");

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnScreamForMe(ScreamForMeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (HasComp<BorgChassisComponent>(ev.Target) || HasComp<SiliconComponent>(ev.Target))
        {
            Popup(ev.Performer, "spell-fail-target-silicon");
            return;
        }

        if (!ScreamForMe(ev))
            return;

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnInstantSummons(InstantSummonsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!_timing.IsFirstTimePredicted)
            return;

        if (!TryComp(ev.Action, out InstantSummonsActionComponent? summons))
            return;

        if (!TryComp(ev.Performer, out HandsComponent? hands))
            return;

        var held = hands.ActiveHandEntity;

        if (held != null && held == summons.Entity)
            return;

        bool ItemValid([NotNullWhen(true)] EntityUid? item)
        {
            return HasComp<ItemComponent>(item) && !HasComp<VirtualItemComponent>(item);
        }

        void MarkItem(EntityUid item)
        {
            summons.Entity = item;
            PopupLoc(ev.Performer, Loc.GetString("instant-summons-item-marked", ("item", item)));
            Dirty(ev.Action, summons);
        }

        if (!Exists(summons.Entity) || !TryComp(summons.Entity.Value, out TransformComponent? xform))
        {
            if (ItemValid(held))
                MarkItem(held.Value);
            else
                Popup(ev.Performer, "spell-fail-no-held-entity");

            return;
        }

        if (ItemValid(held))
        {
            if (TryComp(ev.Action, out ConfirmableActionComponent? confirmable))
            {
                // if not primed, prime it and cancel the action
                if (confirmable.NextConfirm is not { } confirm)
                {
                    _confirmableAction.Prime((ev.Action, confirmable), ev.Performer);
                    return;
                }

                // primed but the delay isnt over, cancel the action
                if (_timing.CurTime < confirm)
                    return;

                // primed and delay has passed, let the action go through
                _confirmableAction.Unprime((ev.Action, confirmable));
            }

            MarkItem(held.Value);
            return;
        }

        var item = summons.Entity.Value;

        if (TryGetOuterNonMobContainer(item, xform, out var container))
            item = container.Owner;

        if (_net.IsServer)
            _audio.PlayEntity(ev.SummonSound, Filter.Pvs(item).Merge(Filter.Pvs(ev.Performer)), item, true);

        TransformSystem.SetMapCoordinates(item, TransformSystem.GetMapCoordinates(ev.Performer));
        TransformSystem.AttachToGridOrMap(item);

        if (TryComp(item, out EmbeddableProjectileComponent? embeddable) && embeddable.EmbeddedIntoUid != null)
        {
            Physics.SetBodyType(item, BodyType.Dynamic);
            embeddable.EmbeddedIntoUid = null;
            Dirty(item, embeddable);
        }

        Hands.TryForcePickupAnyHand(ev.Performer, item, handsComp: hands);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnTeleport(WizardTeleportEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        _teleport.OnTeleportSpell(ev.Performer, ev.Action);
    }

    private void OnTraps(TrapsSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (ev.Traps.Count == 0)
            return;

        if (_net.IsClient)
        {
            _magic.Speak(ev);
            ev.Handled = true;
            return;
        }

        if (!Mind.TryGetMind(ev.Performer, out var mind, out _))
            return;

        var range = ev.Range;
        var mapPos = TransformSystem.GetMapCoordinates(ev.Performer);
        var box = Box2.CenteredAround(mapPos.Position, new Vector2(range, range));
        var circle = new Circle(mapPos.Position, range);
        var grids = new List<Entity<MapGridComponent>>();
        MapManager.FindGridsIntersecting(mapPos.MapId, box, ref grids);

        bool IsTileValid((EntityCoordinates, TileRef) data)
        {
            var (coords, tile) = data;

            if (tile.IsSpace())
                return false;

            var trapQuery = GetEntityQuery<WizardTrapComponent>();
            var flags = LookupFlags.Static | LookupFlags.Sundries | LookupFlags.Sensors;
            foreach (var (entity, fix) in Lookup.GetEntitiesInRange<FixturesComponent>(coords, 0.1f, flags))
            {
                if (fix.Fixtures.Any(x =>
                        x.Value.Hard && (x.Value.CollisionLayer & (int) CollisionGroup.LowImpassable) != 0))
                    return false;

                if (trapQuery.HasComp(entity))
                    return false;
            }

            return true;
        }

        var tiles = new List<(EntityCoordinates, TileRef)>();
        foreach (var grid in grids)
        {
            tiles.AddRange(Map.GetTilesIntersecting(grid.Owner, grid.Comp, circle)
                .Select(x => (Map.GridTileToLocal(grid.Owner, grid.Comp, x.GridIndices), x))
                .Where(IsTileValid));
        }

        for (var i = 0; i < Math.Min(tiles.Count, ev.Amount); i++)
        {
            var (coords, _) = Random.PickAndTake(tiles);
            var trap = Spawn(Random.Pick(ev.Traps), coords);
            var trapComp = EnsureComp<WizardTrapComponent>(trap);
            trapComp.IgnoredMinds.Add(mind);
            Dirty(trap, trapComp);
        }

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnBees(LesserSummonBeesEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        SpawnBees(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnSimians(SummonSimiansEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        SpawnMonkeys(ev);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnExsangunatingStrike(ExsanguinatingStrikeEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Performer, out HandsComponent? hands))
            return;

        if (!HasComp<ItemComponent>(hands.ActiveHandEntity))
        {
            Popup(ev.Performer, "spell-fail-sanguine-strike-no-item");
            return;
        }

        var item = hands.ActiveHandEntity.Value;

        if (HasComp<VirtualItemComponent>(item))
            return;

        if (HasComp<SanguineStrikeComponent>(item))
        {
            Popup(ev.Performer, "spell-fail-sanguine-strike-already-empowered");
            return;
        }

        if (!TryComp(item, out MeleeWeaponComponent? weapon) || weapon.Damage.GetTotal() == FixedPoint2.Zero)
        {
            PopupLoc(ev.Performer, Loc.GetString("spell-fail-sanguine-strike-not-weapon", ("item", item)));
            return;
        }

        AddComp<SanguineStrikeComponent>(item);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnChuuniInvocations(ChuuniInvocationsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Performer, out InventoryComponent? inventory))
            return;

        if (!_inventory.HasSlot(ev.Performer, "eyes", inventory))
        {
            Popup(ev.Performer, "spell-fail-cant-wear-eyepatch");
            return;
        }

        if (_inventory.TryGetSlotEntity(ev.Performer, "eyes", out var eyepatch, inventory) &&
            HasComp<ChuuniEyepatchComponent>(eyepatch.Value))
        {
            Popup(ev.Performer, "spell-fail-already-wear-eyepatch");
            return;
        }

        SetGear(ev.Performer, ev.Gear, inventoryComponent: inventory);

        if (_net.IsServer && _inventory.TryGetSlotEntity(ev.Performer, "head", out var hat, inventory) &&
            _tag.HasTag(hat.Value, ev.WizardHatTag))
            QueueDel(hat.Value);

        _magic.Speak(ev);
        ev.Handled = true;
    }

    private void OnSwap(SwapSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (ev.Performer == ev.Target)
            return;

        if (!TryComp(ev.Action, out SwapSpellComponent? swap))
            return;

        var userCoords = TransformSystem.GetMapCoordinates(ev.Performer);
        var targetCoords = TransformSystem.GetMapCoordinates(ev.Target);

        Teleport(ev.Performer, targetCoords);

        if (swap.SecondaryTarget == null ||
            swap.SecondaryTarget.Value == ev.Target || swap.SecondaryTarget.Value == ev.Performer)
            Teleport(ev.Target, userCoords);
        else
        {
            var secondaryTarget = swap.SecondaryTarget.Value;
            var secondaryTargetCoords = TransformSystem.GetMapCoordinates(secondaryTarget);

            if (secondaryTargetCoords.MapId != userCoords.MapId || !secondaryTargetCoords.InRange(userCoords, ev.Range))
                Teleport(ev.Target, userCoords);
            else
            {
                Teleport(ev.Target, secondaryTargetCoords);
                Teleport(secondaryTarget, userCoords);
            }

            swap.SecondaryTarget = null;
            if (_net.IsServer)
                RaiseNetworkEvent(new StopTargetingEvent(), ev.Performer); // Just in case
        }

        _magic.Speak(ev);
        ev.Handled = true;
        return;

        void Teleport(EntityUid uid, MapCoordinates coords)
        {
            _pulling.StopAllPulls(uid);
            _audio.PlayPvs(ev.Sound, uid);
            TransformSystem.SetMapCoordinates(uid, coords);
            Spawn(ev.Effect, coords);
            Physics.WakeBody(uid);
        }
    }

    #endregion

    #region Helpers

    // Copied straight from SharedContainerSystem (and modified).
    private bool TryGetOuterNonMobContainer(EntityUid uid,
        TransformComponent xform,
        [NotNullWhen(true)] out BaseContainer? container)
    {
        container = null;

        if (!uid.IsValid())
            return false;

        var child = uid;
        var parent = xform.ParentUid;

        var managerQuery = GetEntityQuery<ContainerManagerComponent>();
        var xformQuery = GetEntityQuery<TransformComponent>();
        var bodyQuery = GetEntityQuery<BodyComponent>();
        var bodyPartQuery = GetEntityQuery<BodyPartComponent>();
        var inventoryQuery = GetEntityQuery<InventoryComponent>();
        var handsQuery = GetEntityQuery<HandsComponent>();

        while (parent.IsValid() && !bodyQuery.HasComp(parent) && !bodyPartQuery.HasComp(parent) &&
               !inventoryQuery.HasComp(parent) && !handsQuery.HasComp(parent))
        {
            if (((EntityManager.MetaQuery.GetComponent(child).Flags & MetaDataFlags.InContainer) ==
                 MetaDataFlags.InContainer) && managerQuery.TryGetComponent(parent, out var conManager) &&
                Container.TryGetContainingContainer(parent, child, out var parentContainer, conManager))
            {
                container = parentContainer;
            }

            var parentXform = xformQuery.GetComponent(parent);
            child = parent;
            parent = parentXform.ParentUid;
        }

        return container != null;
    }

    private EntityUid? SpawnItemInHands(EntityUid user, EntProtoId proto, EntityUid action)
    {
        if (!TryComp(user, out HandsComponent? hands))
            return null;

        if (!Hands.TryGetEmptyHand(user, out var hand, hands))
        {
            Popup(user, "spell-fail-hands-occupied");
            return null;
        }

        if (_net.IsClient)
            return null;

        var item = Spawn(proto, Transform(user).Coordinates);
        if (Hands.TryPickup(user, item, hand, false, false, hands))
            return item;

        QueueDel(item);
        _actions.SetCooldown(action, TimeSpan.FromSeconds(0.5));
        return null;
    }

    private bool ValidateLockOnAction(EntityWorldTargetActionEvent ev)
    {
        if (ev.Coords == null)
            return false;

        if (!TryComp(ev.Action.Owner, out LockOnMarkActionComponent? lockOnMark))
            return false;

        if (!TryComp(ev.Entity, out TransformComponent? xform))
            return true;

        return TransformSystem.InRange(ev.Coords.Value, xform.Coordinates, lockOnMark.LockOnRadius + 1f);
    }

    private void Popup(EntityUid uid, string message)
    {
        _popup.PopupClient(Loc.GetString(message), uid, uid);
    }

    private void PopupLoc(EntityUid uid, string locMessage)
    {
        _popup.PopupClient(locMessage, uid, uid);
    }

    private void SpawnHomingProjectile(EntProtoId proto,
        EntityCoordinates coords,
        EntityUid? target,
        EntityUid user,
        MapCoordinates mapCoords,
        Vector2 velocity,
        float speed,
        bool checkMobState,
        MapCoordinates? toCoords = null)
    {
        if (target == null && toCoords == null)
            return;

        var targetPos = toCoords?.Position ?? TransformSystem.GetMapCoordinates(target!.Value).Position;

        var direction = targetPos - mapCoords.Position;
        if (direction == Vector2.Zero)
            return;

        var projectile = Spawn(proto, coords);

        _gunSystem.ShootProjectile(projectile, direction, velocity, user, user, speed);

        if (target == null || target == user || checkMobState && !HasComp<MobStateComponent>(target))
            return;

        _gunSystem.SetTarget(projectile, target, out var targeted, false);

        var homing = EnsureComp<HomingProjectileComponent>(projectile);
        homing.Target = target;

        Entity<HomingProjectileComponent, TargetedProjectileComponent> ent = (projectile, homing, targeted);

        Dirty(ent);
    }

    protected (EntityCoordinates coords, MapCoordinates mapCoords, EntityCoordinates spawnCoords, Vector2 velocity)
        GetProjectileData(EntityUid shooter)
    {
        var coords = Transform(shooter).Coordinates;
        var mapCoords = TransformSystem.ToMapCoordinates(coords);

        // If applicable, this ensures the projectile is parented to grid on spawn, instead of the map.
        var spawnCoords = MapManager.TryFindGridAt(mapCoords, out var gridUid, out _)
            ? TransformSystem.WithEntityId(coords, gridUid)
            : new(Map.GetMapOrInvalid(mapCoords.MapId), mapCoords.Position);

        var velocity = Physics.GetMapLinearVelocity(spawnCoords);

        return (coords, mapCoords, spawnCoords, velocity);
    }

    protected void SetGear(EntityUid uid,
        Dictionary<string, EntProtoId> gear,
        bool force = true,
        bool makeUnremoveable = true,
        InventoryComponent? inventoryComponent = null)
    {
        if (_net.IsClient)
            return;

        if (!Resolve(uid, ref inventoryComponent, false))
            return;

        foreach (var (slot, item) in gear)
        {
            _inventory.TryUnequip(uid, slot, true, force, false, inventoryComponent);

            var ent = Spawn(item, Transform(uid).Coordinates);
            if (!_inventory.TryEquip(uid, ent, slot, true, force, false, inventoryComponent))
            {
                Del(ent);
                continue;
            }

            if (slot == "id" &&
                TryComp(ent, out PdaComponent? pdaComponent) &&
                TryComp<IdCardComponent>(pdaComponent.ContainedId, out var id))
                id.FullName = MetaData(uid).EntityName;

            if (makeUnremoveable && HasComp<ClothingComponent>(ent))
                EnsureComp<UnremoveableComponent>(ent);
        }
    }

    #endregion

    #region ServerMethods

    public virtual void SpeakSpell(EntityUid speakerUid, EntityUid casterUid, string speech, MagicSchool school) { }

    protected virtual void MakeMime(EntityUid uid) { }

    protected virtual void Emp(DisableTechEvent ev) { }

    protected virtual void SpawnSmoke(SmokeSpellEvent ev) { }

    protected virtual void Repulse(RepulseEvent ev) { }

    protected virtual void ExplodeCorpse(CorpseExplosionEvent ev) { }

    protected virtual void Emote(EntityUid uid, string emoteId) { }

    protected virtual void BindSoul(BindSoulEvent ev, EntityUid item, EntityUid mind, MindComponent mindComponent) { }

    protected virtual bool Polymorph(PolymorphSpellEvent ev)
    {
        return true;
    }

    protected virtual void ShootSpellCards(SpellCardsEvent ev, EntProtoId proto) { }

    protected virtual void Speak(EntityUid uid, string message) { }

    protected virtual bool ScreamForMe(ScreamForMeEvent ev)
    {
        return true;
    }

    protected virtual void SpawnBees(LesserSummonBeesEvent ev) { }

    protected virtual void SpawnMonkeys(SummonSimiansEvent ev) { }

    #endregion
}

[Serializable, NetSerializable]
public sealed class StopTargetingEvent : EntityEventArgs;

[Serializable, NetSerializable]
public sealed class SetSwapSecondaryTarget(NetEntity action, NetEntity? target) : EntityEventArgs
{
    public NetEntity Action = action;

    public NetEntity? Target = target;
}
