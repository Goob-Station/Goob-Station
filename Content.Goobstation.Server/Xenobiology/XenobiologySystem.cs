// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Goobstation.Shared.Xenobiology.Systems;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
///     This handles the server-side of Xenobiology.
/// </summary>
public sealed class XenobiologySystem : SharedXenobiologySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PendingSlimeSpawnComponent, MapInitEvent>(OnPendingSlimeMapInit);
        SubscribeLocalEvent<SlimeComponent, MapInitEvent>(OnSlimeMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateMitosis();
    }

    private void OnPendingSlimeMapInit(Entity<PendingSlimeSpawnComponent> ent, ref MapInitEvent args)
    {
        // it sucks but it works and now y*ml warriors can add more slimes 500% faster
        if(!TrySpawnSlime(ent, ent.Comp.BasePrototype, ent.Comp.Breed,out var slime))
            return;

        var s = slime.Value.Comp;
        // every xenobio slime copy is personalized. feel free to tweak it as you like
        // the rest of the shit such as inheritance is handled by SpawnSlime
        s.MutationChance *= _random.NextFloat(.5f, 1.5f);
        s.MaxOffspring += _random.Next(-1, 2);
        s.ExtractsProduced += _random.Next(0, 2);
        s.MitosisHunger *= _random.NextFloat(.75f, 1.2f);
    }

    private void OnSlimeMapInit(Entity<SlimeComponent> ent, ref MapInitEvent args)
    {
        Subs.CVar(_configuration, GoobCVars.BreedingInterval, val => ent.Comp.UpdateInterval = TimeSpan.FromSeconds(val), true);
        ent.Comp.NextUpdateTime = _gameTiming.CurTime + TimeSpan.FromSeconds(_random.NextDouble(2, ent.Comp.UpdateInterval.TotalSeconds));
    }

    /// <summary>
    ///     Checks slime entity hunger threshold, if the threshold required by SlimeComponent is met -> DoMitosis.
    /// </summary>
    private void UpdateMitosis()
    {
        var eligibleSlimes = new HashSet<Entity<SlimeComponent, HungerComponent>>();
        var query = EntityQueryEnumerator<SlimeComponent, MobGrowthComponent, HungerComponent>();
        while (query.MoveNext(out var uid, out var slime, out var growthComp, out var hungerComp))
        {
            if (_gameTiming.CurTime < slime.NextUpdateTime
                || _mobState.IsDead(uid)
                || growthComp.IsFirstStage)
                continue;
            slime.NextUpdateTime = _gameTiming.CurTime + slime.UpdateInterval;
            if (_hunger.GetHunger(hungerComp) >= slime.MitosisHunger)
                eligibleSlimes.Add((uid,slime,hungerComp));
        }

        foreach (var slime in eligibleSlimes)
        {
            var (_, slimeComp,hungerComp) = slime;
            if (_hunger.GetHunger(hungerComp) > slimeComp.MitosisHunger - slimeComp.JitterDifference)
                _jitter.DoJitter(slime, TimeSpan.FromSeconds(1), true);
            DoMitosis(slime);
        }
    }

    /// <summary>
    ///     Handles slime mitosis.
    ///     For each offspring, a mutation is selected from their potential mutations.
    ///     If mutation is successful, the products of mitosis will have the new mutation.
    /// </summary>
    private void DoMitosis(Entity<SlimeComponent> ent)
    {
        var offspringCount = _random.Next(1, ent.Comp.MaxOffspring + 1);
        _audio.PlayPredicted(ent.Comp.MitosisSound, ent, ent);

        for (var i = 0; i < offspringCount; i++)
        {
            var selectedBreed = ent.Comp.Breed;

            if (_random.Prob(ent.Comp.MutationChance) && ent.Comp.PotentialMutations.Count > 0)
                selectedBreed = _random.Pick(ent.Comp.PotentialMutations);

            if (!TrySpawnSlime(ent, ent.Comp.DefaultSlimeProto, selectedBreed, out var sl))
                continue;
            // carries over generations. type shit.
            var newSlime = sl.Value.Comp;
            newSlime.Tamer = ent.Comp.Tamer;
            newSlime.MutationChance = ent.Comp.MutationChance;
            newSlime.MaxOffspring = ent.Comp.MaxOffspring;
            newSlime.ExtractsProduced = ent.Comp.ExtractsProduced;
        }

        _containerSystem.EmptyContainer(ent.Comp.Stomach);
        QueueDel(ent);
    }
    /// <summary>
    /// Returns the extract associated by the slimes breed.
    /// </summary>
    /// <param name="slime">The slime entity.</param>
    /// <returns>Gray if no breed can be found.</returns>
    public EntProtoId GetProducedExtract(Entity<SlimeComponent> slime)
    {
        return _prototypeManager.TryIndex(slime.Comp.Breed, out var breedPrototype)
            ? breedPrototype.ProducedExtract
            : slime.Comp.DefaultExtract;
    }

    /// <summary>
    ///  Tries to spawn a slime with a given mutation, returns false if unsuccessful.
    /// </summary>
    /// <param name="parent">The original entity.</param>
    /// <param name="newEntityProto">The prototype of the entity being spawned.</param>
    /// <param name="selectedBreed">The selected breed of the entity.</param>
    /// <param name="slime">The slime that was spawned.</param>
    private bool TrySpawnSlime(EntityUid parent, EntProtoId newEntityProto, ProtoId<BreedPrototype> selectedBreed, [NotNullWhen(true)] out Entity<SlimeComponent>? slime)
    {
        slime = null;
        if (Deleted(parent) || !_prototypeManager.TryIndex(selectedBreed, out var newBreed))
            return false;
        var newEntityUid = SpawnNextToOrDrop(newEntityProto, parent, null, newBreed.Components);
        if (!TryComp<SlimeComponent>(newEntityUid, out var newSlime))
            return false;
        if (newSlime is { ShouldHaveShader: true, Shader: not null })
            _appearance.SetData(newEntityUid, XenoSlimeVisuals.Shader, newSlime.Shader);

        _appearance.SetData(newEntityUid, XenoSlimeVisuals.Color, newSlime.SlimeColor);
        _metaData.SetEntityName(newEntityUid, newBreed.BreedName);

        slime = (newEntityUid, newSlime);
        return true;
    }
}
