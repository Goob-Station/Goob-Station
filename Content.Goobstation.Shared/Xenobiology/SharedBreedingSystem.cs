using System.Linq;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Xenobiology;

/// <summary>
/// This handles slime breeding and mutation.
/// </summary>
public sealed class SharedBreedingSystem : EntitySystem
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

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
        var eligibleSlimes = new HashSet<Entity<SlimeComponent, HungerComponent>>();
        while (query.MoveNext(out var ent, out var slime, out var hunger))
        {
            if (_mobState.IsDead(ent))
                continue;

            eligibleSlimes.Add((ent, slime, hunger));
        }

        foreach (var ent in eligibleSlimes)
        {
            if (_hunger.GetHunger(ent) < ent.Comp1.MitosisHunger)
                continue;

            DoMitosis(ent);
            Logger.Debug("mitosis called");
        }

    }

    #region Helpers

    //Spawns a slime with a given mutation
    private void DoBreeding(EntityUid parent, EntProtoId newEntity, ProtoId<BreedPrototype> selectedBreed)
    {
        if (!_gameTiming.IsFirstTimePredicted)
            return;

        if (!_prototypeManager.TryIndex(selectedBreed, out var newBreed))
            return;

        var ent = SpawnNextToOrDrop(newEntity, parent, null, newBreed.Components);
        Logger.Debug("spawned");

        _metaData.SetEntityName(ent, newBreed.BreedName);
        DirtyEntity(ent);
    }

    //Handles slime mitosis, for each offspring, a mutation is selected from their potential mutations - if mutation is successful, the products of mitosis will have the new mutation.
    public void DoMitosis(Entity<SlimeComponent> ent)
    {
        for (int i = 0; i < ent.Comp.Offspring; i++)
        {
            bool success = _random.NextDouble() < ent.Comp.MutationChance;
            var selectedBreed = ent.Comp.Breed;

            if (success && ent.Comp.PotentialMutations.Count > 0)
            {
                var randomIndex = _random.Next(0, ent.Comp.PotentialMutations.Count);
                selectedBreed = ent.Comp.PotentialMutations.ToArray()[randomIndex];

                DoBreeding(ent, _defaultSlime, selectedBreed);
                Logger.Debug("DoMitosis");
            }
            else
            {
                DoBreeding(ent, _defaultSlime, selectedBreed);
                Logger.Debug("DoMitosis");
            }

        }
        QueueDel(ent);
    }

    #endregion

}
