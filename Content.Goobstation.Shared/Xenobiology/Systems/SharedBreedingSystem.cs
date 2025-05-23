using System.Linq;
using Content.Goobstation.Common.Xenobiology;
using Content.Goobstation.Common.Xenobiology.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// This handles slime breeding and mutation.
/// </summary>
public sealed class SharedBreedingSystem : EntitySystem
{

    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly RobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly EntProtoId _defaultSlime = "MobXenoSlime";
    private readonly ProtoId<BreedPrototype> _defaultSlimeMutation = "GreyMutation";

    private TimeSpan _nextUpdateTime;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();

        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
    }

    //Mitosis doesn't need to be checked for every frame.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextUpdateTime > _gameTiming.CurTime)
            return;

        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
        UpdateMitosis();
    }

    //Checks the hunger of slimes, if they've reached the threshhold set in SlimeComponent, the mitosis method is called.
    private void UpdateMitosis()
    {
        var query = EntityQueryEnumerator<SlimeComponent, HungerComponent>();
        var eligibleSlimes = new List<Entity<SlimeComponent, HungerComponent>>();
        while (query.MoveNext(out var uid, out var slime, out var hunger))
        {

            if (_mobState.IsDead(uid))
                continue;

            eligibleSlimes.Add((uid, slime, hunger));
        }

        foreach (var ent in eligibleSlimes)
        {
            if (_hunger.GetHunger(ent) < ent.Comp1.MitosisHunger)
                continue;

            _hunger.ModifyHunger(ent, -ent.Comp1.MitosisHunger, ent.Comp2);
            DoMitosis(ent);
        }
    }

    #region Helpers

    //Mutates slimes based on the selected mutation.
    private void DoBreeding(EntityUid ent, ProtoId<BreedPrototype> selectedBreed)
    {
        if (!HasComp<SlimeComponent>(ent)
            || !_prototypeManager.TryIndex<EntityPrototype>(_defaultSlimeMutation, out var defaultBreed)
            || !_prototypeManager.TryIndex<BreedPrototype>(selectedBreed.Id, out var newBreed))
            return;

        var baseCompReg = defaultBreed.Components;
        var newCompReg = newBreed.Components;

        //Remove all comps not present on default slime.
        var toRemove = EntityManager.GetComponents(ent)
            .Where(c => !baseCompReg.ContainsKey(c.GetType().Name))
            .ToList();

        //Add comps from selected breed.
        var toAdd = newCompReg
            .Select(kvp =>
            {
                var comp = (Component)_componentFactory.GetComponent(kvp.Key);
                comp.Owner = ent;

                var temp = (object)comp;
                _serializationManager.CopyTo(kvp.Value.Component, ref temp);
                return (Component)temp!;
            })
            .ToList();

        foreach (var component in toRemove)
        {
            EntityManager.RemoveComponent(ent, component.GetType());
        }

        foreach (var component in toAdd)
        {
            EntityManager.AddComponent(ent, component, true);
        }

        DirtyEntity(ent);
    }

    //Handles slime mitosis, for each offspring, a mutation is selected from their potential mutations - if mutation is successful, the products of mitosis will have the new mutation.
    public void DoMitosis(Entity<SlimeComponent> ent)
    {
        for (int i = 0; i < ent.Comp.Offspring; i++)
        {
            bool success = _random.NextDouble() < ent.Comp.MutationChance;
            var selectedMutation = ent.Comp.Breed;

            if (success && ent.Comp.PotentialMutations.Count > 0)
            {
                var randomIndex = _random.Next(0, ent.Comp.PotentialMutations.Count);
                selectedMutation = ent.Comp.PotentialMutations.ToArray()[randomIndex];

                var newEntity = Spawn(_defaultSlime);
                DoBreeding(newEntity, selectedMutation);
            }
            else
            {
                var newEntity = Spawn(_defaultSlime);
                DoBreeding(newEntity, selectedMutation);
            }

        }
        QueueDel(ent);
    }

    #endregion

}
