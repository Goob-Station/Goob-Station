using Content.Goobstation.Common.Knowledge.Systems;
using Content.Shared.Construction.Components;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction;

public abstract partial class SharedConstructionSystem
{
    [Dependency] private readonly KnowledgeSystem _knowledge = default!;

    /// <summary>
    /// Goobstation
    /// Returns all available construction groups for that entity.
    /// </summary>
    public IEnumerable<ProtoId<ConstructionGroupPrototype>> AvailableConstructionGroups(EntityUid user)
    {
        if (!_knowledge.TryGetKnowledgeWithComp<ConstructionKnowledgeComponent>(user, out var knowledge))
            yield break;

        // Not doing any evil LINQ today. I am honest with my shitcode.
        foreach (var (_, construction, _) in knowledge)
        {
            foreach (var protoId in construction.Groups)
            {
                yield return protoId;
            }
        }
    }
}
