using Content.Goobstation.Common.Knowledge;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Jobs;

/// <summary>
/// Adds knowledge on spawn to the entity
/// </summary>
[UsedImplicitly]
public sealed partial class AddKnowledgeSpecial : JobSpecial
{
    [DataField]
    public List<EntProtoId> Knowledge { get; private set; } = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var knowledgeSystem = entMan.System<CommonKnowledgeSystem>();
        knowledgeSystem.AddKnowledgeUnits(mob, Knowledge);
    }
}
