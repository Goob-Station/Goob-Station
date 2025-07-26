// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text.RegularExpressions;
using Content.Goobstation.Server.Devil.Condemned;
using Content.Goobstation.Server.Devil.Contract;
using Content.Goobstation.Server.Devil.Objectives.Components;
using Content.Goobstation.Server.Possession;
using Content.Goobstation.Server.Singularity.EventHorizon;
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Chemistry;
using Content.Goobstation.Shared.CrematorImmune;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Condemned;
using Content.Goobstation.Shared.Exorcism;
using Content.Goobstation.Shared.Religion;
using Content.Goobstation.Shared.Supermatter.Components;
using Content.Server._Shitmed.StatusEffects;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.Antag.Components;
using Content.Server.Atmos.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.Cuffs;
using Content.Server.Destructible;
using Content.Server.Hands.Systems;
using Content.Server.Jittering;
using Content.Server.Mind;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Server.Stunnable;
using Content.Server.Temperature.Components;
using Content.Server.Zombies;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Lavaland.Chasm;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Shuttles.Components;
using Content.Shared.Temperature.Components;
using Robust.Server.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem : EntitySystem
{
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PolymorphSystem _poly = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly DevilContractSystem _contract = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PossessionSystem _possession = default!;
    [Dependency] private readonly CondemnedSystem _condemned = default!;
    [Dependency] private readonly MobStateSystem _state = default!;
    [Dependency] private readonly JitteringSystem _jittering = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ContainerSystem _container = default!;

    private static readonly Regex WhitespaceAndNonWordRegex = new(@"[\s\W]+", RegexOptions.Compiled);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<DevilComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<DevilComponent, SoulAmountChangedEvent>(OnSoulAmountChanged);
        SubscribeLocalEvent<DevilComponent, PowerLevelChangedEvent>(OnPowerLevelChanged);
        SubscribeLocalEvent<DevilComponent, ExorcismDoAfterEvent>(OnExorcismDoAfter);

        SubscribeLocalEvent<IdentityBlockerComponent, InventoryRelayedEvent<IsEyesCoveredCheckEvent>>(OnEyesCoveredCheckEvent);

        InitializeHandshakeSystem();
        SubscribeAbilities();
    }

    #region Startup & Remove

    private void OnStartup(Entity<DevilComponent> devil, ref MapInitEvent args)
    {
        // Remove human components.
        RemComp<CombatModeComponent>(devil);
        RemComp<HungerComponent>(devil);
        RemComp<ThirstComponent>(devil);
        RemComp<TemperatureComponent>(devil);
        RemComp<TemperatureSpeedComponent>(devil);
        RemComp<CondemnedComponent>(devil);
        RemComp<DestructibleComponent>(devil);

        // Adjust stats
        EnsureComp<ZombieImmuneComponent>(devil);
        EnsureComp<BreathingImmunityComponent>(devil);
        EnsureComp<PressureImmunityComponent>(devil);
        EnsureComp<ActiveListenerComponent>(devil);
        EnsureComp<WeakToHolyComponent>(devil).AlwaysTakeHoly = true;
        EnsureComp<CrematoriumImmuneComponent>(devil);
        EnsureComp<AntagImmuneComponent>(devil);
        EnsureComp<SupermatterImmuneComponent>(devil);
        EnsureComp<PreventChasmFallingComponent>(devil).DeleteOnUse = false;
        EnsureComp<FTLSmashImmuneComponent>(devil);

        // Allow infinite revival
        var revival = EnsureComp<CheatDeathComponent>(devil);
        revival.InfiniteRevives = true;
        revival.CanCheatStanding = true;

        // Change damage modifier
        if (TryComp<DamageableComponent>(devil, out var damageableComp))
           _damageable.SetDamageModifierSetId(devil, devil.Comp.DevilDamageModifierSet, damageableComp);

        // No decapitating the devil
        foreach (var part in _body.GetBodyChildren(devil))
        {
            if (!TryComp(part.Id, out WoundableComponent? woundable))
                continue;

            woundable.CanRemove = false;
            Dirty(part.Id, woundable);
        }

        // Add base actions
        foreach (var actionId in devil.Comp.BaseDevilActions)
            _actions.AddAction(devil, actionId);

        // Self Explanatory
        GenerateTrueName(devil);
    }

    #endregion

    #region Event Listeners

    private void OnSoulAmountChanged(Entity<DevilComponent> devil, ref SoulAmountChangedEvent args)
    {
        if (!_mind.TryGetMind(args.User, out var mindId, out var mind))
            return;

        devil.Comp.Souls += args.Amount;
        _popup.PopupEntity(Loc.GetString("contract-soul-added"), args.User, args.User, PopupType.MediumCaution);

        if (devil.Comp.Souls is > 1 and < 7 && devil.Comp.Souls % 2 == 0)
        {
            devil.Comp.PowerLevel = (DevilPowerLevel)(devil.Comp.Souls / 2); // malicious casting to enum

            // Raise event
            var ev = new PowerLevelChangedEvent(args.User, devil.Comp.PowerLevel);
            RaiseLocalEvent(args.User, ref ev);
        }

        if (_mind.TryGetObjectiveComp<SignContractConditionComponent>(mindId, out var objectiveComp, mind))
            objectiveComp.ContractsSigned += args.Amount;
    }

    private void OnPowerLevelChanged(Entity<DevilComponent> devil, ref PowerLevelChangedEvent args)
    {
        var popup = Loc.GetString($"devil-power-level-increase-{args.NewLevel.ToString().ToLowerInvariant()}");
        _popup.PopupEntity(popup, args.User, args.User, PopupType.Large);

        if (!_prototype.TryIndex(devil.Comp.DevilBranchPrototype, out var proto))
            return;

        foreach (var ability in proto.PowerActions)
        {
            if (args.NewLevel != ability.Key)
                continue;

            foreach (var actionId in ability.Value)
                _actions.AddAction(devil, actionId);
        }
    }

    private void OnExamined(Entity<DevilComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange || ent.Comp.PowerLevel < DevilPowerLevel.Weak)
            return;

        var ev = new IsEyesCoveredCheckEvent();
        RaiseLocalEvent(ent, ev);

        if (ev.IsEyesProtected)
            return;

        args.PushMarkup(Loc.GetString("devil-component-examined", ("target", Identity.Entity(ent, EntityManager))));
    }

    private void OnEyesCoveredCheckEvent(Entity<IdentityBlockerComponent> ent, ref InventoryRelayedEvent<IsEyesCoveredCheckEvent> args)
    {
        if (ent.Comp.Enabled)
            args.Args.IsEyesProtected = true;
    }
    private void OnListen(Entity<DevilComponent> devil, ref ListenEvent args)
    {
        // Other Devils and entities without souls have no authority over you.
        if (HasComp<DevilComponent>(args.Source)
        || HasComp<CondemnedComponent>(args.Source)
        || HasComp<SiliconComponent>(args.Source)
        || args.Source == devil.Owner)
            return;

        var message = WhitespaceAndNonWordRegex.Replace(args.Message.ToLowerInvariant(), "");
        var trueName = WhitespaceAndNonWordRegex.Replace(devil.Comp.TrueName.ToLowerInvariant(), "");

        if (!message.Contains(trueName))
            return;

        // hardcoded, but this is just flavor so who cares :godo:
        _jittering.DoJitter(devil, TimeSpan.FromSeconds(4), true);

        if (_timing.CurTime < devil.Comp.LastTriggeredTime + devil.Comp.CooldownDuration)
            return;

        devil.Comp.LastTriggeredTime = _timing.CurTime;

        if (HasComp<BibleUserComponent>(args.Source))
        {
            _damageable.TryChangeDamage(devil, devil.Comp.DamageOnTrueName * devil.Comp.BibleUserDamageMultiplier, true);
            _stun.TryParalyze(devil, devil.Comp.ParalyzeDurationOnTrueName * devil.Comp.BibleUserDamageMultiplier, false);

            var popup = Loc.GetString("devil-true-name-heard-chaplain", ("speaker", args.Source), ("target", devil));
            _popup.PopupEntity(popup, devil, PopupType.LargeCaution);
        }
        else
        {
            _stun.TryParalyze(devil, devil.Comp.ParalyzeDurationOnTrueName, false);
            _damageable.TryChangeDamage(devil, devil.Comp.DamageOnTrueName, true);

            var popup = Loc.GetString("devil-true-name-heard", ("speaker", args.Source), ("target", devil));
            _popup.PopupEntity(popup, devil, PopupType.LargeCaution);
        }
    }

    private void OnExorcismDoAfter(Entity<DevilComponent> devil, ref ExorcismDoAfterEvent args)
    {
        if (args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString("devil-exorcised", ("target", Name(devil))), devil, PopupType.LargeCaution);
        _condemned.StartCondemnation(target, behavior: CondemnedBehavior.Banish, doFlavor: false);

    }

    #endregion

    #region Helper Methods

    private static bool TryUseAbility(BaseActionEvent action)
    {
        if (action.Handled)
            return false;

        action.Handled = true;
        return true;
    }
    private void PlayFwooshSound(EntityUid uid, DevilComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        _audio.PlayPvs(comp.FwooshPath, uid, new AudioParams(-2f, 1f, SharedAudioSystem.DefaultSoundRange, 1f, false, 0f));
    }

    private void DoContractFlavor(EntityUid devil, string name)
    {
        var flavor = Loc.GetString("contract-summon-flavor", ("name", name));
        _popup.PopupEntity(flavor, devil, PopupType.Medium);
    }
    private void GenerateTrueName(DevilComponent comp)
    {
        // Generate true name.
        var firstNameOptions = _prototype.Index(comp.FirstNameTrue);
        var lastNameOptions = _prototype.Index(comp.LastNameTrue);

        comp.TrueName = string.Concat(_random.Pick(firstNameOptions.Values), " ", _random.Pick(lastNameOptions.Values));
    }

    #endregion

}
