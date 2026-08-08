using Content.Server.Nutrition;
using Content.Shared._Shitmed.StatusEffects;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Medical;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Server._Shitmed.StatusEffects;

public sealed class VomitChemEffectSystem : EntitySystem
{
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    List<ProtoId<ReagentPrototype>>? _allReagents = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VomitChemComponent, ComponentInit>(OnInit);

        _allReagents = _proto.EnumeratePrototypes<ReagentPrototype>()
        .Where(x => !x.Abstract)
        .Select(x => new ProtoId<ReagentPrototype>(x.ID)).ToList();
    }

    private void OnInit(Entity<VomitChemComponent> ent, ref ComponentInit args)
    {
        if (_allReagents == null)
            return;

        var pick = _allReagents[_random.Next(_allReagents.Count)];
        _vomit.Vomit(ent.Owner, reagent: pick);
    }
}
