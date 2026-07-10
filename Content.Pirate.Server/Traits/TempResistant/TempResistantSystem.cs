using Content.Shared.Temperature.Components;

namespace Content.Pirate.Server.Traits.HeatResistant;

public sealed class TempResistantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TempResistantComponent, MapInitEvent>(OnInit);
    }

    private void OnInit(Entity<TempResistantComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<TemperatureDamageComponent>(ent.Owner, out var temperatureDamage))
            return;

        temperatureDamage.HeatDamageThreshold *= ent.Comp.HeatModifier;
        temperatureDamage.ColdDamageThreshold *= ent.Comp.ColdModifier;
    }
}
