using Content.Goobstation.Server.Implants.Components;
using Content.Shared.Implants;
using Content.Shared.Nutrition.Components;

namespace Content.Goobstation.Server.Implants.Systems;

public sealed class NutrimentPumpImplantSystem : EntitySystem
{
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
            RemComp<HungerComponent>(user);
            comp.HadHunger = true;
        }

        if (HasComp<ThirstComponent>(user))
        {
            RemComp<ThirstComponent>(user);
            comp.HadThirst = true;
        }


    }

    private void OnUnimplanted(Entity<NutrimentPumpImplantComponent> ent, ref ImplantRemovedFromEvent args)
    {
        var user = args.Implanted;
        var comp = ent.Comp;

        if (comp.HadHunger)
            EnsureComp<HungerComponent>(user);
        if (comp.HadThirst)
            EnsureComp<ThirstComponent>(user);

        if (HasComp<NutrimentPumpImplantComponent>(args.Implant))
            RemComp<NutrimentPumpImplantComponent>(ent);
    }
}
