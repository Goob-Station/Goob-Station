using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Server.Chat.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Chat;
using Content.Shared.Polymorph;

namespace Content.Goobstation.Server.MartialArts;

/// <summary>
/// Handles the very few things that we can't handle in shared.
/// </summary>
public sealed partial class ServerMartialArtsSystem : EntitySystem
{
    [Dependency] private PolymorphSystem _polymorph = default!;

    [SubscribeLocalEvent]
    private void OnPolymorphedCPC(Entity<ComboPerformerComponent> ent, ref PolymorphedEvent args)
    {
        if (HasComp<ComboPerformerComponent>(args.NewEntity))
            return;

        _polymorph.CopyPolymorphComponent<ComboPerformerComponent>(ent, args.NewEntity);
    }

    [SubscribeLocalEvent]
    private void OnPolymorphedMAK(Entity<MartialArtsKnowledgeComponent> ent, ref PolymorphedEvent args)
    {
        if (HasComp<MartialArtsKnowledgeComponent>(args.NewEntity))
            return;

        _polymorph.CopyPolymorphComponent<MartialArtsKnowledgeComponent>(ent, args.NewEntity);
    }
}
