using Content.Shared.Verbs;
using Content.Goobstation.Shared.Silicon.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Silicon;

public abstract partial class SharedStationAiEarlyLeaveSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiEarlyLeaveComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private EntityUid? GetInsertedAI(Entity<StationAiCoreComponent> ent)
    {
        if (!_containers.TryGetContainer(ent.Owner, StationAiHolderComponent.Container, out var container) 
        || container.ContainedEntities.Count != 1)
            return null;

        return container.ContainedEntities[0];
    }

    private void OnGetVerbs(Entity<StationAiEarlyLeaveComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!TryComp<StationAiCoreComponent>(ent.Owner, out var aiCoreComp))
            return;

        var aiCore = new Entity<StationAiCoreComponent>(ent.Owner, aiCoreComp);

        if (GetInsertedAI(aiCore) is { } insertedAi && insertedAi == args.User)
        {
            args.Verbs.Add(new Verb()
            {
                Text = Loc.GetString("station-ai-earlyleave-button"),
                Act = () => RequestEarlyLeave(aiCore, insertedAi),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/Spare/poweronoff.svg.192dpi.png")),
            });
        }
    }

    protected virtual void RequestEarlyLeave(Entity<StationAiCoreComponent> aiCore, EntityUid insertedAi)
    {

    }
}
