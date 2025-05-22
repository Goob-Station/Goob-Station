using System.Linq;
using Content.Goobstation.Common.Xenobiology;
using Content.Goobstation.Common.Xenobiology.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Microsoft.VisualBasic.CompilerServices;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// This handles slime breeding and mutation.
/// </summary>
public sealed class SharedSlimeSystem : EntitySystem
{

    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly RobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeComponent, ComponentInit>(OnCompInit);
    }
    
    // While the component is initializing, we push the fields in the provided mutation prototype onto the Slime.
    public void OnCompInit(Entity<SlimeComponent> ent, ref ComponentInit args)
    {
        var protoId = ent.Comp.Mutation;

        if (_prototypeManager.TryIndex<MutationPrototype>(protoId, out var mutation))
        {
            foreach (var (name, data) in mutation.Components)
            {
                var comp = (Component) _componentFactory.GetComponent(name);

                comp.Owner = ent.Owner;

                var temp = (object) comp;
                _serializationManager.CopyTo(data.Component, ref temp);
                EntityManager.AddComponent(ent.Owner, (Component)temp!, true); //The overwrite parameter should ensure any existing components of the same type are overwritten.
            }
        }
    }

    //This method handles slime mitosis, for each offspring, a mutation is selected from their potential mutations - if mutation is successful, the products of mitosis will have the new mutation.
    public void DoMitosis(Entity<SlimeComponent> ent)
    {
        for (int i = 0; i < ent.Comp.Offspring; i++)
        {
            bool success = _random.NextDouble() < ent.Comp.MutationChance;
            var selectedMutation = ent.Comp.Mutation;

            if (success && ent.Comp.PotentialMutations.Count > 0)
            {
                var randomIndex = _random.Next(0, ent.Comp.PotentialMutations.Count);
                selectedMutation = ent.Comp.PotentialMutations.ToArray()[randomIndex];

                var newEntity = SpawnSlime(ent.Owner, selectedMutation);
            }
            else
            {
                var newEntity = SpawnSlime(ent.Owner, selectedMutation);
            }

        }
    }

    //A helper method which clones slimes and updates the mutation field.
    private EntityUid SpawnSlime(EntityUid original, string mutation)
    {
        var transform = Transform(original);

        var newEntity = EntityManager.SpawnEntity(MetaData(original).EntityPrototype!.ID, transform.Coordinates);

        // Get the slime component of the new entity
        if (TryComp<SlimeComponent>(newEntity, out var slime))
        {
            slime.Mutation = mutation;
        }

        return newEntity;
    }

    //Checks the hunger of slimes, if they've reached the threshhold set in SlimeComponent, the mitosis method is called.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<SlimeComponent>();
        var eligibleSlimes = new List<Entity<SlimeComponent>>();
        while (query.MoveNext(out var uid, out var slime))
        {

            if (!HasComp<HungerComponent>(uid))
                continue;

            if (_mobState.IsDead(uid))
                continue;

            eligibleSlimes.Add((uid, slime)); // Goob - self-spawning
        }

        foreach (var ent in eligibleSlimes)
        {
            if (!TryComp<HungerComponent>(ent, out var hunger))
                continue;

            if (_hunger.GetHunger(hunger) < ent.Comp.MitosisHunger)
                continue;

            _hunger.ModifyHunger(ent, -ent.Comp.MitosisHunger, hunger);
            DoMitosis(ent);
        }
    }

}
