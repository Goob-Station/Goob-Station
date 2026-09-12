using Content.Goobstation.Common.Knowledge;
using Content.Shared.Silicons.Borgs.Components;

namespace Content.Goobstation.Shared.Silicon.Borgs;

public sealed class GoobBorgSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BorgBrainComponent, KnowledgeContainerRelayEvent>(HandleKnowledge);
    }

    private void HandleKnowledge(Entity<BorgBrainComponent> ent, ref KnowledgeContainerRelayEvent args)
    {
        if (args.Handled)
            return;

        args.Found = ent.Owner;
        // Don't handle it so on the next iteration we will get the actual brain
    }
}
