using Content.Goobstation.Shared.MalfAI;
using Content.Server.Store.Systems;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store.Components;

namespace Content.Goobstation.Server.MalfAI;

public sealed partial class MalfAiSystem : SharedMalfAiSystem
{
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MalfAiComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<OpenMalfAiStoreEvent>(OnOpenStore);
    }

    private void OnMapInit(Entity<MalfAiComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<MindComponent>(ent, out var mind) || mind.OwnedEntity is not { } body)
            return;

        _actions.AddAction(body, ref ent.Comp.StoreAction, ent.Comp.StoreActionId, ent.Owner);
        Dirty(ent);
    }

    private void OnOpenStore(OpenMalfAiStoreEvent args)
    {
        if (args.Handled || !HasComp<StationAiHeldComponent>(args.Performer)
            || !_mind.TryGetMind(args.Performer, out var mind, out _)
            || !HasComp<MalfAiComponent>(mind) || !TryComp<StoreComponent>(mind, out var store))
            return;

        _store.ToggleUi(args.Performer, mind, store);
        args.Handled = true;
    }

    private void OnShutdown(Entity<MalfAiComponent> ent, ref ComponentShutdown args)
    {
        _store.CloseUi(ent.Owner);
        _actions.RemoveAction(ent.Comp.StoreAction);
    }
}
