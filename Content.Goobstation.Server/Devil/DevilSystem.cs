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
using Content.Goobstation.Shared.Bible;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.CrematorImmune;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Condemned;
using Content.Goobstation.Shared.Exorcism;
using Content.Goobstation.Shared.Religion;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Systems;
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
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Temperature.Components;
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
    [Dependency] private readonly INetManager _net = default!;
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

    private static readonly Regex WhitespaceAndNonWordRegex = new(@"[\s\W]+", RegexOptions.Compiled);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<DevilComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<DevilComponent, SoulAmountChangedEvent>(OnSoulAmountChanged);
        SubscribeLocalEvent<DevilComponent, PowerLevelChangedEvent>(OnPowerLevelChanged);
        SubscribeLocalEvent<DevilComponent, ExorcismDoAfterEvent>(OnExorcismDoAfter);

        InitializeHandshakeSystem();
        SubscribeAbilities();
    }

    #region Startup & Remove

    private void OnStartup(EntityUid uid, DevilComponent comp, ComponentStartup args)
    {

        // Remove human components.
        RemComp<CombatModeComponent>(uid);
        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);
        RemComp<TemperatureComponent>(uid);
        RemComp<TemperatureSpeedComponent>(uid);
        RemComp<CondemnedComponent>(uid);

        // Adjust stats
        EnsureComp<ZombieImmuneComponent>(uid);
        EnsureComp<BreathingImmunityComponent>(uid);
        EnsureComp<PressureImmunityComponent>(uid);
        EnsureComp<ActiveListenerComponent>(uid);
        EnsureComp<WeakToHolyComponent>(uid).AlwaysTakeHoly = true;
        EnsureComp<CrematoriumImmuneComponent>(uid);

        // Allow infinite revival
        var revival = EnsureComp<CheatDeathComponent>(uid);
        revival.ReviveAmount = -1;
        revival.CanCheatStanding = true;

        // Change damage modifier
        if (TryComp<DamageableComponent>(uid, out var damageableComp))
            _damageable.SetDamageModifierSetId(uid, comp.DevilDamageModifierSet, damageableComp);

        // Add base actions
        foreach (var actionId in comp.BaseDevilActions)
            _actions.AddAction(uid, actionId);

        // Self Explanatory
        GenerateTrueName(comp);
    }

    #endregion

    #region Event Listeners

    private void OnSoulAmountChanged(EntityUid uid, DevilComponent comp, ref SoulAmountChangedEvent args)
    {
        if (!_mind.TryGetMind(args.User, out var mindId, out var mind))
            return;

        comp.Souls += args.Amount;
        _popup.PopupEntity(Loc.GetString("contract-soul-added"), args.User, args.User, PopupType.MediumCaution);

        if (comp.Souls is > 1 and < 7 && comp.Souls % 2 == 0)
        {
            comp.PowerLevel = (DevilPowerLevel)(comp.Souls / 2); // malicious casting to enum

            // Raise event
            var ev = new PowerLevelChangedEvent(args.User, comp.PowerLevel);
            RaiseLocalEvent(args.User, ref ev);
        }

        if (_mind.TryGetObjectiveComp<SignContractConditionComponent>(mindId, out var objectiveComp, mind))
            objectiveComp.ContractsSigned += args.Amount;
    }

    private void OnPowerLevelChanged(EntityUid uid, DevilComponent comp, ref PowerLevelChangedEvent args)
    {
        var popup = Loc.GetString($"devil-power-level-increase-{args.NewLevel.ToString().ToLowerInvariant()}");
        _popup.PopupEntity(popup, args.User, args.User, PopupType.Large);

        if (!_prototype.TryIndex(comp.DevilBranchPrototype, out var proto))
            return;

        foreach (var ability in proto.PowerActions)
        {
            if (args.NewLevel != ability.Key)
                continue;

            foreach (var actionId in ability.Value)
            {
                EntityUid? actionEnt = null;
                _actions.AddAction(uid, ref actionEnt, actionId);

                if (actionEnt != null)
                    comp.ActionEntities.Add(actionEnt.Value);
            }
        }
    }

    private void OnExamined(Entity<DevilComponent> comp, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient && comp.Comp.PowerLevel >= DevilPowerLevel.Weak)
            args.PushMarkup(Loc.GetString("devil-component-examined", ("target", Identity.Entity(comp, EntityManager))));
    }
    private void OnListen(EntityUid uid, DevilComponent comp, ListenEvent args)
    {
        // Other Devils and entities without souls have no authority over you.
        if (HasComp<DevilComponent>(args.Source)
        || HasComp<CondemnedComponent>(args.Source)
        || HasComp<SiliconComponent>(args.Source)
        || args.Source == uid)
            return;

        var message = WhitespaceAndNonWordRegex.Replace(args.Message.ToLowerInvariant(), "");
        var trueName = WhitespaceAndNonWordRegex.Replace(comp.TrueName.ToLowerInvariant(), "");

        if (!message.Contains(trueName))
            return;

        // hardcoded, but this is just flavor so who cares :godo:
        _jittering.DoJitter(uid, TimeSpan.FromSeconds(4), true);

        if (_timing.CurTime < comp.LastTriggeredTime + comp.CooldownDuration)
            return;

        comp.LastTriggeredTime = _timing.CurTime;

        if (HasComp<BibleUserComponent>(args.Source))
        {
            _damageable.TryChangeDamage(uid, comp.DamageOnTrueName * comp.BibleUserDamageMultiplier, true);
            _stun.TryParalyze(uid, comp.ParalyzeDurationOnTrueName * comp.BibleUserDamageMultiplier, false);

            var popup = Loc.GetString("devil-true-name-heard-chaplain", ("speaker", args.Source), ("target", uid));
            _popup.PopupEntity(popup, uid, PopupType.LargeCaution);
        }
        else
        {
            _stun.TryParalyze(uid, comp.ParalyzeDurationOnTrueName, false);
            _damageable.TryChangeDamage(uid, comp.DamageOnTrueName, true);

            var popup = Loc.GetString("devil-true-name-heard", ("speaker", args.Source), ("target", uid));
            _popup.PopupEntity(popup, uid, PopupType.LargeCaution);
        }
    }

    private void OnExorcismDoAfter(Entity<DevilComponent> devil, ref ExorcismDoAfterEvent args)
    {
        if (args.Target is not { } target || args.Cancelled || args.Handled)
            return;

        _popup.PopupEntity(Loc.GetString("devil-exorcised", ("target", devil.Comp.TrueName)), devil, PopupType.LargeCaution);
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

    private static ProtoId<PolymorphPrototype> GetJauntEntity(DevilComponent comp)
    {
        return comp.PowerLevelToJauntPrototypeMap.TryGetValue(comp.PowerLevel, out var value)
            ? value
            : new ProtoId<PolymorphPrototype>("ShadowJaunt30");
    }

    private void PlayFwooshSound(EntityUid uid, DevilComponent comp)
    {
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
