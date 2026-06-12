using Content.Goobstation.Common.Knowledge;
using Content.Shared.Construction.Components;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction;

public abstract partial class SharedConstructionSystem
{
    [Dependency] private readonly IKnowledgeSystem _knowledge = default!;

    /// <summary>
    /// Goobstation
    /// Returns all available construction groups for that entity.
    /// </summary>
    public HashSet<ProtoId<ConstructionPrototype>> AvailableConstructionRecipes(EntityUid user)
    {
        var set = new HashSet<ProtoId<ConstructionPrototype>>();

        if (!_knowledge.TryGetKnowledgeWithComp<ConstructionKnowledgeComponent>(user, out var knowledge))
            return set;

        foreach (var (_, construction, _) in knowledge)
        {
            foreach (var protoId in construction.Packs)
            {
                var pack = PrototypeManager.Index(protoId);
                foreach (var recipe in pack.Recipes)
                {
                    set.Add(recipe);
                }
            }
        }

        return set;
    }
}
