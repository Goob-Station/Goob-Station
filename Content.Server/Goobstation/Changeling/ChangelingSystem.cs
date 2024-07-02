using Content.Server.Store.Systems;
using Content.Shared.Changeling;
using Content.Shared.Store.Components;

namespace Content.Server.Changeling;

public sealed partial class ChangelingSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);

    }
    private void OnOpenEvolutionMenu(EntityUid uid, ChangelingComponent comp, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;
        _store.ToggleUi(uid, uid, store);
    }
}
