using Content.Shared.Heretic;

namespace Content.Server.Heretic.Abilities;

public sealed partial class HereticAbilitySystem : EntitySystem
{
    private void SubscribeBlade()
    {
        SubscribeLocalEvent<HereticComponent, EventHereticRealignment>(OnRealignment);
        SubscribeLocalEvent<HereticComponent, HereticChampionStanceEvent>(OnChampionStance);
        SubscribeLocalEvent<HereticComponent, EventHereticFuriousSteel>(OnFuriousSteel);

        SubscribeLocalEvent<HereticComponent, HereticAscensionBladeEvent>(OnAscensionBlade);
    }

    private void OnRealignment(Entity<HereticComponent> ent, ref EventHereticRealignment args)
    {

    }
    private void OnChampionStance(Entity<HereticComponent> ent, ref HereticChampionStanceEvent args)
    {

    }
    private void OnFuriousSteel(Entity<HereticComponent> ent, ref EventHereticFuriousSteel args)
    {

    }

    private void OnAscensionBlade(Entity<HereticComponent> ent, ref HereticAscensionBladeEvent args)
    {

    }
}
