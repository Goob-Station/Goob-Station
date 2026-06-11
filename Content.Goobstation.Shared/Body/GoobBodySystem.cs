using System.Linq;
using Content.Goobstation.Common.Knowledge;
using Content.Goobstation.Common.Knowledge.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Systems;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Goobstation.Shared.Body;

public sealed class GoobBodySystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OrganComponent, KnowledgeContainerRelayEvent>(HandleKnowledge); // Goobstation edit
    }

    private void HandleKnowledge(Entity<OrganComponent> ent, ref KnowledgeContainerRelayEvent args)
    {
        // Look for the first organ that has KnowledgeContainerComponent and is inserted inside as body organ
        if (args.Handled
            || !HasComp<KnowledgeContainerComponent>(ent.Owner))
            return;

        // Check for a brain that is inside MMI because borg system is trash
        if (HasComp<MMIComponent>(Transform(ent.Owner).ParentUid))
        {
            args.Found = ent.Owner;
            args.Handled = true;
            return;
        }

        // Check that the brain is inserted into the right entity
        var organs = _body.GetBodyOrgans(args.Target).Select(x => x.Id).ToList();
        if (!organs.Contains(ent.Owner))
            return;

        // We are in a right place.
        args.Found = ent.Owner;
        args.Handled = true;
    }
}
