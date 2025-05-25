using System.Linq;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles slime breeding and mutation.
/// </summary>
public sealed class BreedingSystem : EntitySystem
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private readonly EntProtoId _defaultSlime = "MobXenoSlime";

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdateTime;

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

    // Checks slime entity hunger threshholds, if the threshhold required by SlimeComponent is met -> DoMitosis.
    private void UpdateMitosis()
    {
        var query = EntityQueryEnumerator<SlimeComponent, MobGrowthComponent, HungerComponent>();
        var eligibleSlimes = new HashSet<Entity<SlimeComponent, MobGrowthComponent, HungerComponent>>();
        while (query.MoveNext(out var uid, out var slime, out var growth, out var hunger))
        {
            if (_mobState.IsDead(uid))
                continue;

            if (growth.CurrentStage == growth.Stages[0])
                continue;

            eligibleSlimes.Add((uid, slime, growth, hunger));
        }

        foreach (var ent in eligibleSlimes)
        {
            if (_hunger.GetHunger(ent) < ent.Comp1.MitosisHunger)
                continue;

            DoMitosis(ent);
        }
    }

    #region Helpers

    //Spawns a slime with a given mutation
    private void DoBreeding(EntityUid parent, EntProtoId newEntity, ProtoId<BreedPrototype> selectedBreed)
    {
        if (!_net.IsServer
            && !_gameTiming.IsFirstTimePredicted)
            return;

        if (!_prototypeManager.TryIndex(selectedBreed, out var newBreed))
            return;

        var ent = SpawnNextToOrDrop(newEntity, parent, null, newBreed.Components);

        if (TryComp<SlimeComponent>(ent, out var slime))
            _appearance.SetData(ent, XenoSlimeVisuals.Color, slime.SlimeColor);

        _metaData.SetEntityName(ent, newBreed.BreedName);
    }

    //Handles slime mitosis, for each offspring, a mutation is selected from their potential mutations - if mutation is successful, the products of mitosis will have the new mutation.
    public void DoMitosis(Entity<SlimeComponent> ent)
    {
        for (int i = 0; i < ent.Comp.Offspring; i++)
        {
            var success = _random.NextDouble() < ent.Comp.MutationChance;
            var selectedBreed = ent.Comp.Breed;

            if (success && ent.Comp.PotentialMutations.Count > 0)
            {
                var randomIndex = _random.Next(0, ent.Comp.PotentialMutations.Count);
                selectedBreed = ent.Comp.PotentialMutations.ToArray()[randomIndex];

                DoBreeding(ent, _defaultSlime, selectedBreed);
            }
            else
            {
                DoBreeding(ent, _defaultSlime, selectedBreed);
            }

        }
        var container = _container.GetContainer(ent, "storagebase");

        _container.EmptyContainer(container);
        _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_EinsteinEngines/Voice/Slime/slime_squish.ogg"), ent, ent);
        QueueDel(ent);
    }

    #endregion

}
