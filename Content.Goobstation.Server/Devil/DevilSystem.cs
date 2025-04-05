using Content.Goobstation.Shared.Devil;
using Content.Goobstation.Shared.Devil.Actions;
using Content.Server.Actions;
using Content.Server.Atmos.Components;
using Content.Server.Hands.Systems;
using Content.Server.Polymorph.Systems;
using Content.Server.Store.Systems;
using Content.Server.Zombies;
using Content.Shared._Shitmed.Body.Components;
using Content.Shared.Actions;
using Content.Shared.CombatMode;
using Content.Shared.Dataset;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Nutrition.Components;
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


    private readonly EntProtoId _contractPrototype = "PaperDevilContract";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DevilComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<DevilComponent, ExaminedEvent>(OnExamined);

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
