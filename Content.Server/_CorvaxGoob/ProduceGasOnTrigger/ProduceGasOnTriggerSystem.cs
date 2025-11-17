using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Atmos;

namespace Content.Server._CorvaxGoob.ProduceGasOnTrigger;

public sealed class ProduceGasOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ProduceGasOnTriggerComponent, TriggerEvent>(HandleTrigger);
    }

    public void HandleTrigger(Entity<ProduceGasOnTriggerComponent> entity, ref TriggerEvent args)
    {
        var mixture = _atmos.GetTileMixture(entity.Owner);

        if (mixture is null)
            return;

        if (entity.Comp.Gases is null)
            return;

        foreach (KeyValuePair<Gas, float> gas in entity.Comp.Gases)
        {
            mixture.AdjustMoles(gas.Key, gas.Value);
        }

        mixture.Temperature = entity.Comp.MixtureTemparature;
    }
}
