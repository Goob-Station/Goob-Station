using Content.Goobstation.Common.Knowledge.Prototypes;
using Content.Goobstation.Common.Knowledge.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.CharacterInfo;

public sealed partial class CharacterInfoSystem
{
    [Dependency] private readonly KnowledgeSystem _knowledge = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    private Dictionary<ProtoId<KnowledgeCategoryPrototype>, List<string>> GetKnowledgeTabInfo(EntityUid entity)
    {
        var knowledge = new Dictionary<ProtoId<KnowledgeCategoryPrototype>, List<string>>();
        if (!_knowledge.TryGetAllKnowledgeUnits(entity, out var found))
            return knowledge;

        foreach (var unit in found)
        {
            if (unit.Comp.Hidden)
                continue;

            var info = _knowledge.GetKnowledgeString(unit);
            if (info == null)
                continue;

            var category = unit.Comp.Category;

            if (!knowledge.ContainsKey(category))
            {
                knowledge[category] = new List<string>();
                knowledge[category].Add(Loc.GetString(_protoMan.Index(category).Description));
            }
            knowledge[category].Add(info);
        }

        return knowledge;
    }
}
