// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Nutrition.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// This handles slime breeding and mutation.
public partial class XenobiologySystem
{
    private void SubscribeBreeding()
    {
        SubscribeLocalEvent<RandomSlimeChangeComponent, MapInitEvent>(OnPendingSlimeMapInit);
        SubscribeLocalEvent<SlimeComponent, MapInitEvent>(OnSlimeMapInit);
    }

    private void OnPendingSlimeMapInit(Entity<RandomSlimeChangeComponent> ent, ref MapInitEvent args)
    {
        if (!_net.IsServer
            || !TryComp(ent.Owner, out SlimeComponent? slime))
            return;

        // every xenobio slime copy is personalized. feel free to tweak it as you like
        slime.MutationChance *= _random.NextFloat(.5f, 1.5f);
        slime.MaxOffspring += _random.Next(-1, 2);
        slime.ExtractsProduced += _random.Next(0, 2);
        slime.MitosisHunger *= _random.NextFloat(.75f, 1.2f);

        RemComp(ent.Owner, ent.Comp);
    }

    private void OnSlimeMapInit(Entity<SlimeComponent> ent, ref MapInitEvent args)
    {
        var (uid, comp) = ent;

        if (comp.Shader != null)
            _appearance.SetData(uid, XenoSlimeVisuals.Shader, comp.Shader);

        _appearance.SetData(uid, XenoSlimeVisuals.Color, comp.SlimeColor);

        if (!_net.IsServer)
            return;

        Subs.CVar(_configuration, GoobCVars.BreedingInterval, val => ent.Comp.UpdateInterval = TimeSpan.FromSeconds(val), true);
        ent.Comp.NextUpdateTime = _gameTiming.CurTime + ent.Comp.UpdateInterval;
    }

    /// <summary>
    ///     Checks slime entity hunger threshholds, if the threshhold required by SlimeComponent is met -> DoMitosis.
    /// </summary>
    private void UpdateMitosis()
    {
        var query = EntityQueryEnumerator<SlimeComponent, MobGrowthComponent, HungerComponent>();
        while (query.MoveNext(out var uid, out var slime, out var growthComp, out var hungerComp))
        {
            if (_gameTiming.CurTime < slime.NextUpdateTime
                || _mobState.IsDead(uid)
                || growthComp.IsFirstStage)
                continue;

            if (_hunger.GetHunger(hungerComp) > slime.MitosisHunger - slime.JitterDifference)
                _jitter.DoJitter(uid, TimeSpan.FromSeconds(1), true);

            if (_hunger.GetHunger(hungerComp) < slime.MitosisHunger)
                continue;

            DoMitosis((uid, slime));
            slime.NextUpdateTime = _gameTiming.CurTime + slime.UpdateInterval;
        }
    }

    /// <summary>
    ///     Handles slime mitosis.
    ///     For each offspring, a mutation is selected from their potential mutations.
    ///     If mutation is successful, the products of mitosis will have the new mutation.
    /// </summary>
    private void DoMitosis(Entity<SlimeComponent> ent)
    {
        if (_net.IsClient)
            return;

        var offspringCount = _random.Next(1, ent.Comp.MaxOffspring + 1);
        _audio.PlayPredicted(ent.Comp.MitosisSound, ent, ent);

        for (var i = 0; i < offspringCount; i++)
        {
            var selectedBreed = ent.Comp.Breed;

            if (_random.Prob(ent.Comp.MutationChance) && ent.Comp.PotentialMutations.Count > 0)
                selectedBreed = _random.Pick(ent.Comp.PotentialMutations);

            var sl = SpawnNextToOrDrop(selectedBreed, ent.Owner);
            if (TryComp(sl, out SlimeComponent? newComp))
            {
                // carries over generations. type shit.
                newComp.Tamer = ent.Comp.Tamer;
                newComp.MutationChance = ent.Comp.MutationChance;
                newComp.MaxOffspring = ent.Comp.MaxOffspring;
                newComp.ExtractsProduced = ent.Comp.ExtractsProduced;
            }
        }

        _containerSystem.EmptyContainer(ent.Comp.Stomach);
        QueueDel(ent);
    }
}
