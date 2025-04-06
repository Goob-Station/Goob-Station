using System.Linq;
using Content.Goobstation.Server.Condemned;
using Content.Goobstation.Server.Contract;
using Content.Goobstation.Server.Devil.Objectives.Components;
using Content.Goobstation.Shared.CheatDeath;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Server._Goobstation.Wizard.Teleport;
using Content.Server.Actions;
using Content.Server.Administration.Systems;
using Content.Server.Atmos.Components;
using Content.Server.Bible.Components;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Server.Store.Systems;
using Content.Server.Stunnable;
using Content.Server.Temperature.Components;
using Content.Server.Zombies;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Dataset;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Store.Components;
using Content.Shared.Temperature.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Devil;

public sealed partial class DevilSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
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
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly WizardTeleportSystem _teleport = default!;


    private readonly EntProtoId _contractPrototype = "PaperDevilContract";
    private readonly EntProtoId _revivalContractPrototype = "PaperDevilContractRevival";
    private readonly EntProtoId _suitProto = "ClothingUniformJumpsuitDevil";
    private readonly EntProtoId _bookProto = "GuidebookCodexUmbra";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<DevilComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilComponent, ListenEvent>(OnListen);
        SubscribeLocalEvent<DevilComponent, SoulAmountChangedEvent>(OnSoulAmountChanged);

        InitializeHandshakeSystem();
        SubscribeAbilities();
    }

    #region Startup

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

        // Allow infinite revival
        var revival = EnsureComp<CheatDeathComponent>(uid);
        revival.ReviveAmount = -1;

        // Change damage modifier
        TryComp<DamageableComponent>(uid, out var damageable);
        _damageable.SetDamageModifierSetId(uid, "DevilDealPositive");

        // Add base actions
        foreach (var actionId in comp.BaseDevilActions)
            _actions.AddAction(uid, actionId);

        // Generate true name.
        var nameOptions = _prototype.Index<DatasetPrototype>("names_devil");
        comp.TrueName = _random.Pick(nameOptions.Values);

        // Equip suit
        _inventory.TryGetSlotEntity(uid, "jumpsuit", out var jumpsuit);
        Del(jumpsuit);
        var newSuit = SpawnNextToOrDrop(_suitProto, uid);
        _inventory.TryEquip(uid, newSuit, "jumpsuit", true, true, true);

        // Spawn codex
        _inventory.SpawnItemOnEntity(uid, _bookProto);

    }

    #endregion

    #region Event Listeners

    private void OnSoulAmountChanged(EntityUid uid, DevilComponent comp, SoulAmountChangedEvent args)
    {
        // Add souls to internal souls tracker.
        comp.Souls += args.amount;

        if (!_mind.TryGetMind(args.user, out var mindId, out var mind))
            return;

        // Add souls to objective tracker.
        if (_mind.TryGetObjectiveComp<SignContractConditionComponent>(mindId, out var objectiveComp, mind))
            objectiveComp.ContractsSigned += args.amount;
    }

    private void OnExamined(Entity<DevilComponent> comp, ref ExaminedEvent args)
    {
        if (args.IsInDetailsRange && !_net.IsClient)
        {
            args.PushMarkup(Loc.GetString("devil-component-examined", ("target", Identity.Entity(comp, EntityManager))));
        }
    }



    private void OnListen(EntityUid uid, DevilComponent comp, ListenEvent args)
    {
        // Other Devils and entities without souls have no authority over you.
        if (HasComp<DevilComponent>(args.Source) || HasComp<CondemnedComponent>(args.Source) || HasComp<SiliconComponent>(args.Source))
            return;

        var stopList = new List<string>
        {
            "stop",         // Basic stop
            "cease",        // Formal stop
            "halt",         // Urgent stop
            "desist",       // Legal term
            "terminate",    // Technical stop
            "abort",        // Emergency stop
            "discontinue",  // Formal cancellation
            "refrain",       // Preventive stop
            "fuck off"      // Rude stop. Lol.
        };

        var message = args.Message.ToLowerInvariant();

        var trueNameMatch = message.Contains(comp.TrueName.ToLowerInvariant());

        var stopListMatch = stopList.Any(word =>
            message.Contains(word.ToLowerInvariant()));

        if (!trueNameMatch || !stopListMatch)
            return;

        var curTime = _timing.CurTime;
        if (curTime < comp.LastTriggeredTime + comp.CooldownDuration)
            return;

        comp.LastTriggeredTime = curTime;

        if (!HasComp<BibleUserComponent>(args.Source))
        {
            _stun.TryKnockdown(uid, TimeSpan.FromSeconds(4), false);
            _stun.TryStun(uid, TimeSpan.FromSeconds(4), false);
            var popup = Loc.GetString("devil-true-name-heard", ("speaker", args.Source));
            _popup.PopupPredicted(popup, uid, uid, PopupType.LargeCaution);
        }

        var holyDamage = new DamageSpecifier(_prototype.Index<DamageTypePrototype>("Holy"), 15);
        _damageable.TryChangeDamage(uid, holyDamage, true);
        _stun.TryKnockdown(uid, TimeSpan.FromSeconds(8), false);
        _stun.TryStun(uid, TimeSpan.FromSeconds(8), false);
        var popupHoly = Loc.GetString("devil-true-name-heard-chaplain", ("speaker", args.Source));
        _popup.PopupPredicted(popupHoly, uid, uid, PopupType.LargeCaution);

    }

    #endregion

    #region Helper Methods

    private bool TryUseAbility(DevilComponent comp, BaseActionEvent action)
    {
        if (action.Handled)
            return false;

        if (!TryComp<DevilActionComponent>(action.Action, out var devilAction))
            return false;

        if (devilAction.SoulsRequired > comp.Souls)
            return false;

        action.Handled = true;
        return true;
    }

    private void PlayFwooshSound(EntityUid uid, DevilComponent comp)
    {
        _audio.PlayPvs(comp.FwooshPath, uid, new AudioParams(-2f, 1f, SharedAudioSystem.DefaultSoundRange, 1f, false, 0f));
    }

    #endregion

}
