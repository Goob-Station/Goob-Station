using System.Linq;
using Content.Goobstation.Server.Condemned;
using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Server.Actions;
using Content.Server.Atmos.Components;
using Content.Server.Chat.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Speech;
using Content.Server.Speech.Components;
using Content.Server.Store.Systems;
using Content.Server.Stunnable;
using Content.Server.Zombies;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Dataset;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

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


    private readonly EntProtoId _contractPrototype = "PaperDevilContract";
    private readonly EntProtoId _revivalContractPrototype = "PaperDevilContractRevival";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<DevilComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<DevilComponent, ListenEvent>(OnListen);

        SubscribeAbilities();
    }

    private void OnStartup(EntityUid uid, DevilComponent comp, ComponentStartup args)
    {
        // Remove human components.
        RemComp<CombatModeComponent>(uid);
        RemComp<HungerComponent>(uid);
        RemComp<ThirstComponent>(uid);

        // Add immunity components.
        EnsureComp<ZombieImmuneComponent>(uid);
        EnsureComp<BreathingImmunityComponent>(uid);
        EnsureComp<PressureImmunityComponent>(uid);
        EnsureComp<ActiveListenerComponent>(uid);

        // Add base actions
        foreach (var actionId in comp.BaseDevilActions)
            _actions.AddAction(uid, actionId);

        // Generate true name.
        var nameOptions = _prototype.Index<DatasetPrototype>("names_devil");
        comp.TrueName = _random.Pick(nameOptions.Values);

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

        _stun.TryKnockdown(uid, TimeSpan.FromSeconds(4), false);
        _stun.TryStun(uid, TimeSpan.FromSeconds(4), false);
        var popup = Loc.GetString("devil-true-name-heard", ("speaker", args.Source));
        _popup.PopupPredicted(popup, uid, uid, PopupType.LargeCaution);

    }

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
}
