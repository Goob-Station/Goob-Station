// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

/// <summary>
/// This handles slime breeding and mutation.
/// </summary>
public partial class XenobiologySystem
{
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
    private TimeSpan _nextUpdateTime;

    private void InitializeBreeding() =>
        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;

    // Mitosis doesn't need to be checked every frame.
    private void UpdateBreeding()
    {
        if (_nextUpdateTime > _gameTiming.CurTime)
            return;

        _nextUpdateTime = _gameTiming.CurTime + _updateInterval;
        UpdateMitosis();
    }

    // Checks slime entity hunger threshholds, if the threshhold required by SlimeComponent is met -> DoMitosis.
    private void UpdateMitosis()
    {
        var eligibleSlimes = new HashSet<Entity<SlimeComponent, MobGrowthComponent, HungerComponent>>();

        var query = EntityQueryEnumerator<SlimeComponent, MobGrowthComponent, HungerComponent>();
        while (query.MoveNext(out var uid, out var slime, out var growthComp, out var hungerComp))
        {
            if (_mobState.IsDead(uid)
                || growthComp.CurrentStage == growthComp.Stages[0])
                continue;

            eligibleSlimes.Add((uid, slime, growthComp, hungerComp));
        }

        foreach (var ent in eligibleSlimes)
        {
            if (_hunger.GetHunger(ent) > ent.Comp1.MitosisHunger - 25)
                _jitter.DoJitter(ent, TimeSpan.FromSeconds(1), true);

            if (_hunger.GetHunger(ent) < ent.Comp1.MitosisHunger)
                continue;

            DoMitosis(ent);
        }
    }

    #region Helpers

    /// <summary>
    /// Spawns a slime with a given mutation
    /// </summary>
    /// <param name="parent">The original entity.</param>
    /// <param name="newEntityProto">The proto of the entity being spawned.</param>
    /// <param name="selectedBreed">The selected breed of the entity.</param>
    private void DoBreeding(EntityUid parent, EntProtoId newEntityProto, ProtoId<BreedPrototype> selectedBreed)
    {
        if (!_net.IsServer
            || !_prototypeManager.TryIndex(selectedBreed, out var newBreed))
            return;

        var newEntityUid = SpawnNextToOrDrop(newEntityProto, parent, null, newBreed.Components);
        if (!TryComp<SlimeComponent>(newEntityUid, out var slime))
            return;

        if (slime is { ShouldHaveShader: true, Shader: not null })
            _appearance.SetData(newEntityUid, XenoSlimeVisuals.Shader, slime.Shader);

        _appearance.SetData(newEntityUid, XenoSlimeVisuals.Color, slime.SlimeColor);
        _metaData.SetEntityName(newEntityUid, newBreed.BreedName);
    }

    //Handles slime mitosis, for each offspring, a mutation is selected from their potential mutations - if mutation is successful, the products of mitosis will have the new mutation.
    private void DoMitosis(Entity<SlimeComponent> ent)
    {
        if (!_net.IsServer)
            return;

        var offspringCount = _random.Next(1, ent.Comp.MaxOffspring + 1);
        _audio.PlayPredicted(ent.Comp.MitosisSound, ent, ent);

        for (var i = 0; i < offspringCount; i++)
        {
            var selectedBreed = ent.Comp.Breed;

            if (_random.Prob(ent.Comp.MutationChance) && ent.Comp.PotentialMutations.Count > 0)
                selectedBreed = _random.Pick(ent.Comp.PotentialMutations);

            DoBreeding(ent, ent.Comp.DefaultSlimeProto, selectedBreed);
        }

        _containerSystem.EmptyContainer(ent.Comp.Stomach);
        QueueDel(ent);
    }

    #endregion
}
