using Content.Server.Construction.Components;
using Content.Goobstation.Server.NewCooking.NewCookingComponent;
using Content.Server.Construction;
using Robust.Shared.Player;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using Content.Goobstation.Prototypes;

public sealed class CookingMachinesSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ConstructionSystem _constructionSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingMachinesComponent, AfterInteractEvent>(OnOvenAfterInteract);
    }

    private void OnOvenAfterInteract(EntityUid uid, CookingMachinesComponent comp, AfterInteractEvent args)
    {
        if (!comp.IsOn || comp.ContainsFood is not { } food)
            return;

        if (!comp.IsOven)
            return;

        var protoId = _entMan.GetComponent<MetaDataComponent>(food).EntityPrototype?.ID;
        if (protoId == null)
            return;

        foreach (var recipe in _prototype.EnumeratePrototypes<CookingRecipePrototype>())
        {
            if (!recipe.Input.Contains(protoId))
                continue;

            // Check machine type matches
            if (!recipe.RequiredMachine.Equals("Oven", StringComparison.OrdinalIgnoreCase))
                continue;

            QueueDel(food);

            var result = Spawn(recipe.Output, Transform(uid).Coordinates);

            comp.ContainsFood = null;

            break;
        }
    }
}