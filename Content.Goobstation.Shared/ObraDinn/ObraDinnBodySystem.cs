using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;


namespace Content.Goobstation.Shared.ObraDinn;

/// <summary>
/// This handles storing the entitys near a player specie  on death
/// </summary>
public sealed class ObraDinnBodySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ObraDinnBodyComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ObraDinnBodyComponent, MobStateChangedEvent>(OnDeath);
    }

    private void OnStartup(Entity<ObraDinnBodyComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Location = Transform(ent.Owner).Coordinates;
    }

    private void OnDeath(Entity<ObraDinnBodyComponent> ent, ref MobStateChangedEvent args)
    {
        ent.Comp.Witnesses.Clear();
        ent.Comp.Map = null;

        if (args.NewMobState != MobState.Dead || TerminatingOrDeleted(ent))
            return;

        ent.Comp.Location = Transform(ent.Owner).Coordinates;
        ent.Comp.Map = Transform(ent.Owner).MapID;

        var list = _lookup.GetEntitiesInRange(ent.Comp.Location.Value, ent.Comp.WitnessRange);

        foreach (var possibleWitness in list)
        {
            if(!TryComp<MobStateComponent>(possibleWitness, out var mobStateComponent) )
                continue;

            ent.Comp.Witnesses.Add( new ObraDinnWitness( possibleWitness,
                Transform(possibleWitness).Coordinates,
                Identity.Name(possibleWitness,EntityManager,ent.Owner),
                mobStateComponent.CurrentState
                ));
        }
    }
}
