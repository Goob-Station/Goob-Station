using Content.Server.Actions;
using Content.Shared._Goobstation.Fishing.Components;
using Content.Shared._Goobstation.Fishing.Systems;
using Content.Shared.Actions;

namespace Content.Server._Goobstation.Fishing;

public sealed class FishingSystem : SharedFishingSystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FishingRodComponent, MapInitEvent>(OnFishingRodInit);
        SubscribeLocalEvent<FishingRodComponent, GetItemActionsEvent>(OnGetActions);
    }

    private void OnFishingRodInit(Entity<FishingRodComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId);
    }

    private void OnGetActions(Entity<FishingRodComponent> ent, ref GetItemActionsEvent args)
    {
        args.AddAction(ref ent.Comp.ThrowLureActionEntity, ent.Comp.ThrowLureActionId);
    }
}
