// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

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
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Random;
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
        SubscribeLocalEvent<MindContainerComponent, RandomizeSpellsEvent>(OnRandomizeSpells);
    }

    private void OnRandomizeSpells(Entity<MindContainerComponent> ent, ref RandomizeSpellsEvent args)
    {
        var comp = ent.Comp;
        if (comp.Mind == null)
            return;

        var container = EnsureComp<ActionsContainerComponent>(comp.Mind.Value);

        var list = args.SpellsDict.ToList();
        list.Sort((kv1, kv2) =>
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (kv1.Value == null && kv2.Value == null)
                return 0;

            if (kv1.Value == null)
                return 1;

            if (kv2.Value == null)
                return -1;

            return kv1.Value.Value.CompareTo(kv2.Value.Value);
        });

        var totalWeight = args.TotalBalance;
        var ignoredSpells = new List<string>();
        foreach (var (key, value) in list)
        {
            var (weight, chosen) =
                GrantRandomSpells(totalWeight, key, comp.Mind.Value, container, value, ignoredSpells);

            totalWeight = weight;
            if (totalWeight <= 0)
                return;

            ignoredSpells.AddRange(chosen);
        }
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

        comp.TotalWeight = GrantRandomSpells(comp.TotalWeight, comp.RandomActions, args.Mind.Owner, container)
            .remainingWeight;
    }

    public (float remainingWeight, List<string> chosenSpells) GrantRandomSpells(float totalWeight,
        ProtoId<WeightedRandomEntityPrototype>? spells,
        EntityUid mind,
        ActionsContainerComponent container,
        int? maxAmount = null,
        List<string>? ignoredSpells = null)
    {
        List<string> chosenSpells = new();
        if (totalWeight <= 0f || !_proto.TryIndex(spells, out var randomActions))
            return (totalWeight, chosenSpells);

        var weights = FilterDictionary(randomActions.Weights, ignoredSpells);

        while (totalWeight > 0f && weights.Count > 0 && maxAmount is null or > 0)
        {
            if (maxAmount != null)
                maxAmount--;
            var protoId = _random.Pick(weights.Keys);
            chosenSpells.Add(protoId);
            weights.Remove(protoId, out var weight);
            totalWeight -= weight;
            _actionContainer.AddAction(mind, protoId, container);
            weights = FilterDictionary(weights);
        }

        return (totalWeight, chosenSpells);

        Dictionary<string, float> FilterDictionary(Dictionary<string, float> dict, List<string>? ignored = null)
        {
            return ignored == null
                ? dict.Where(w => w.Value <= totalWeight).ToDictionary()
                : dict.Where(w => !ignored.Contains(w.Key) && w.Value <= totalWeight).ToDictionary();
        }
    }
}