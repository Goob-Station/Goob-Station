// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Server.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Security.Cryptography;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// This handles slime breeding and mutation.
public partial class XenobiologySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;
    [Dependency] private readonly SlimeLatchSystem _latch = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    private List<EntityUid> _slimes = new();
    private EntityQuery<BloodstreamComponent> _bloodQuery;
    private List<Entity<SlimeComponent, MobGrowthComponent, HungerComponent>> _splitting = new();

    private void SubscribeBreeding()
    {
        SubscribeLocalEvent<PendingSlimeSpawnComponent, MapInitEvent>(OnPendingSlimeMapInit);
        SubscribeLocalEvent<SlimeComponent, MapInitEvent>(OnSlimeMapInit);

        _bloodQuery = GetEntityQuery<BloodstreamComponent>();
    }

    private void OnPendingSlimeMapInit(Entity<PendingSlimeSpawnComponent> ent, ref MapInitEvent args)
    {
        if (SpawnSlime(ent, ent.Comp.BasePrototype, ent.Comp.Breed) is not { } slime)
            return;

        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));

        var s = slime.Comp;
        // every xenobio slime copy is personalized. feel free to tweak it as you like
        // the rest of the shit such as inheritance is handled by SpawnSlime
        s.MutationChance *= rand.NextFloat(.5f, 1.5f);
        s.MaxOffspring += rand.Next(-1, 1);
        s.ExtractsProduced += rand.Next(0, 2);
        s.MitosisHunger *= rand.NextFloat(.75f, 1.2f);
        Dirty(slime);
    }

    private void OnSlimeMapInit(Entity<SlimeComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextUpdateTime = _timing.CurTime + _updateInterval;
    }

    /// <summary>
    ///     Checks slime entity hunger threshholds, if the threshhold required by SlimeComponent is met -> DoMitosis.
    /// </summary>
    private void UpdateMitosis()
    {
        _splitting.Clear();
        _slimes.Clear();
        var query = EntityQueryEnumerator<SlimeComponent, MobGrowthComponent, HungerComponent>();
        while (query.MoveNext(out var uid, out var slime, out var growthComp, out var hungerComp))
        {
            if (_timing.CurTime < slime.NextUpdateTime
                || _mobState.IsDead(uid)
                || growthComp.IsFirstStage)
                continue;

            _splitting.Add((uid, slime, growthComp, hungerComp));
            slime.NextUpdateTime = _timing.CurTime + _updateInterval;
        }

        foreach (var ent in _splitting)
        {
            if (_hunger.GetHunger(ent) > ent.Comp1.MitosisHunger - ent.Comp1.JitterDifference)
                _jitter.DoJitter(ent, TimeSpan.FromSeconds(1), true);

            if (_hunger.GetHunger(ent) < ent.Comp1.MitosisHunger)
                continue;

            DoMitosis(ent);
        }
    }

    /// <summary>
    ///     Handles slime mitosis.
    ///     For each offspring, a mutation is selected from their potential mutations.
    ///     If mutation is successful, the products of mitosis will have the new mutation.
    /// </summary>
    private void DoMitosis(Entity<SlimeComponent> ent)
    {
        var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
        var offspringCount = rand.Next(1, ent.Comp.MaxOffspring + 1);

        if (_net.IsServer) // no local entity for PlayPredicted and i dont trust this israelgpt slop anyway
            _audio.PlayPvs(ent.Comp.MitosisSound, ent);

        for (var i = 0; i < offspringCount; i++)
        {
            var selectedBreed = ent.Comp.Breed;

            if (rand.Prob(ent.Comp.MutationChance) && ent.Comp.PotentialMutations.Count > 0)
                selectedBreed = rand.Pick(ent.Comp.PotentialMutations);

            if (SpawnSlime(ent, ent.Comp.DefaultSlimeProto, selectedBreed) is { } sl)
            {
                // carries over generations. type shit.
                sl.Comp.Tamer = ent.Comp.Tamer;
                sl.Comp.MutationChance = ent.Comp.MutationChance;
                sl.Comp.MaxOffspring = ent.Comp.MaxOffspring;
                sl.Comp.ExtractsProduced = ent.Comp.ExtractsProduced;
                sl.Comp.Whitelist = ent.Comp.Whitelist;
                Dirty(sl);
            }
        }


        // transfer chem bloodstream and stomach chemicals to children evenly
        var slimeScale = 1 / (float) _slimes.Count;
        var parentStomachList = _body.GetBodyOrganEntityComps<StomachComponent>(ent.Owner);
        var parentStomachSolutionTransfer = new Solution();
        foreach (var stomach in parentStomachList)
        {
            if (_solutionContainer.ResolveSolution(stomach.Owner, StomachSystem.DefaultSolutionName, ref stomach.Comp1.Solution, out var sol))
            {
                parentStomachSolutionTransfer.AddSolution(sol, _proto);
                sol.RemoveAllSolution();
            }
        }
        parentStomachSolutionTransfer.ScaleSolution(slimeScale);

        var parentChemSolutionTransfer = new Solution();
        if (_bloodQuery.TryComp(ent, out var parentBloodstream)
            && _solutionContainer.ResolveSolution(ent.Owner, parentBloodstream.BloodSolutionName, ref parentBloodstream.BloodSolution, out var parentChem))
        {
            parentChemSolutionTransfer.AddSolution(parentChem, _proto);
            parentChem.RemoveAllSolution();
        }
        parentChemSolutionTransfer.ScaleSolution(slimeScale);

        foreach (var s in _slimes)
        {
            if (_bloodQuery.TryComp(s, out var childBloodstream)
                && _solutionContainer.ResolveSolution(s, childBloodstream.BloodSolutionName, ref childBloodstream.BloodSolution, out var childChem))
                childChem.AddSolution(parentChemSolutionTransfer, _proto);

            var childStomachList = _body.GetBodyOrganEntityComps<StomachComponent>(s);
            foreach (var stomach in childStomachList)
            {
                _stomach.TryTransferSolution(stomach.Owner, parentStomachSolutionTransfer, stomach);
            }
        }

        _containerSystem.EmptyContainer(ent.Comp.Stomach);
        _latch.Unlatch(ent);
        PredictedQueueDel(ent);
    }

    // <summary>
    ///     Spawns a slime with a given mutation
    /// </summary>
    /// <param name="parent">The original entity.</param>
    /// <param name="newEntityProto">The proto of the entity being spawned.</param>
    /// <param name="selectedBreed">The selected breed of the entity.</param>
    public Entity<SlimeComponent>? SpawnSlime(EntityUid parent, [ForbidLiteral] EntProtoId newEntityProto, ProtoId<BreedPrototype> selectedBreed)
    {
        if (Deleted(parent) ||
            !_proto.Resolve(selectedBreed, out var newBreed))
            return null;

        var newEntityUid = PredictedSpawnNextToOrDrop(newEntityProto, parent, null, newBreed.Components);

        if (!TryComp<SlimeComponent>(newEntityUid, out var newSlime))
            return null;

        if (newSlime.ShouldHaveShader && newSlime.Shader != null)
            _appearance.SetData(newEntityUid, XenoSlimeVisuals.Shader, newSlime.Shader);

        _appearance.SetData(newEntityUid, XenoSlimeVisuals.Color, newSlime.SlimeColor);
        _meta.SetEntityName(newEntityUid, newBreed.BreedName);

        return (newEntityUid, newSlime);
    }
}
