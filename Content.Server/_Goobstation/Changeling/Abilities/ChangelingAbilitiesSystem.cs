using Content.Server.Store.Systems;
using Content.Shared._Goobstation.Changeling;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.EntitySystems;
using Content.Shared.Store.Components;

namespace Content.Server._Goobstation.Changeling.Abilities;

public sealed partial class ChangelingAbilitiesSystem : SharedChangelingAbilitiesSystem
{
    [Dependency] private readonly StoreSystem _storeSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingComponent, OpenEvolutionMenuEvent>(OnOpenEvolutionMenu);
    }

    /// <summary>
    ///     Opens ling's store. Maybe move it to click on biomass/chemicals alerts?
    /// </summary>
    private void OnOpenEvolutionMenu(Entity<ChangelingComponent> changeling, ref OpenEvolutionMenuEvent args)
    {
        if (!TryComp<StoreComponent>(changeling, out var store))
            return;

        _storeSystem.ToggleUi(changeling, changeling, store);
    }
}
