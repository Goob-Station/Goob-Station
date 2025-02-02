using System.Linq;
using Content.Server._Goobstation.Wizard.Components;
using Content.Server._Goobstation.Wizard.Store;
using Content.Server.Antag;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Goobstation.Wizard.Systems;

public sealed class SpellsGrantSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ObjectivesSystem _objectives = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpellsGrantComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<GrantTargetObjectiveOnGhostTakeoverComponent, ItemPurchasedEvent>(OnPurchased);
        SubscribeLocalEvent<GrantTargetObjectiveOnGhostTakeoverComponent, TakeGhostRoleEvent>(OnTakeGhostRole,
            after: new[] { typeof(GhostRoleSystem) });
    }

    private void OnPurchased(Entity<GrantTargetObjectiveOnGhostTakeoverComponent> ent, ref ItemPurchasedEvent args)
    {
        if (_mind.TryGetMind(args.Buyer, out var mind, out _))
            ent.Comp.TargetMind = mind;
    }

    private void OnTakeGhostRole(Entity<GrantTargetObjectiveOnGhostTakeoverComponent> ent, ref TakeGhostRoleEvent args)
    {
        if (!args.TookRole)
            return;

        var comp = ent.Comp;

        if (!Exists(comp.TargetMind) || !HasComp<MindComponent>(comp.TargetMind.Value))
            return;

        if (!_mind.TryGetMind(args.Player, out var ourMind, out var ourMindComp) || ourMind == comp.TargetMind.Value)
            return;

        if (!_objectives.TryCreateObjective((ourMind, ourMindComp), comp.Objective, out var objective))
            return;

        if (!TryComp(objective, out TargetObjectiveComponent? target))
        {
            AddObjective();
            return;
        }

        EnsureComp<DynamicObjectiveTargetMindComponent>(comp.TargetMind.Value);
        _target.SetTarget(objective.Value, comp.TargetMind.Value, target);
        _target.SetName(objective.Value, target);
        AddObjective();

        return;

        void AddObjective()
        {
            _mind.AddObjective(ourMind, ourMindComp, objective.Value);
        }
    }

    private void OnMindAdded(Entity<SpellsGrantComponent> ent, ref MindAddedMessage args)
    {
        var comp = ent.Comp;

        if (comp.Granted)
            return;

        comp.Granted = true;

        if (comp.AntagProfile != null)
            _antag.ForceMakeAntag<SpellsGrantComponent>(args.Mind.Comp.Session, comp.AntagProfile);

        var container = EnsureComp<ActionsContainerComponent>(args.Mind.Owner);

        foreach (var action in comp.GuaranteedActions)
        {
            _actionContainer.AddAction(args.Mind.Owner, action, container);
        }

        if (comp.TotalWeight <= 0f || !_proto.TryIndex(comp.RandomActions, out var randomActions))
            return;

        var weights = randomActions.Weights.Where(w => w.Value <= comp.TotalWeight).ToDictionary();

        while (comp.TotalWeight > 0f && weights.Count > 0)
        {
            var protoId = _random.Pick(weights.Keys);
            weights.Remove(protoId, out var weight);
            comp.TotalWeight -= weight;
            _actionContainer.AddAction(args.Mind.Owner, protoId, container);
            weights = weights.Where(w => w.Value <= comp.TotalWeight).ToDictionary();
        }
    }
}
