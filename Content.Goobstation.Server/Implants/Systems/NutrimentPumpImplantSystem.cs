using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class NutrimentPumpImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NutrimentPumpImplantComponent, ImplantImplantedEvent>(OnImplant);
        SubscribeLocalEvent<NutrimentPumpImplantComponent, ImplantRemovedFromEvent>(OnUnimplanted);
    }

    private void OnImplant(Entity<NutrimentPumpImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (!args.Implanted.HasValue)
            return;

        var comp = ent.Comp;
        var user = args.Implanted.Value;

        if (HasComp<HungerComponent>(user))
        {
            RemCompDeferred<HungerComponent>(user);
            comp.HadHunger = true;
        }

        if (HasComp<ThirstComponent>(user))
        {
            RemCompDeferred<ThirstComponent>(user);
            comp.HadThirst = true;
        }

    }

    public void OnUnimplanted(Entity<NutrimentPumpImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        var user = args.Implanted;
        var comp = ent.Comp;

        if (comp.HadHunger)
            EnsureComp<HungerComponent>(user);
        if (comp.HadThirst)
            EnsureComp<ThirstComponent>(user);
    }
}
