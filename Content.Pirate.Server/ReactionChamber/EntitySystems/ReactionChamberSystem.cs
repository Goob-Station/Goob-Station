using System;
using System.Collections.Generic;
using Content.Goobstation.Maths.FixedPoint;
using Content.Pirate.Server.ReactionChamber.Components;
using Content.Pirate.Shared.ReactionChamber;
using Content.Pirate.Shared.ReactionChamber.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Temperature;
using Content.Shared.UserInterface;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Server.ReactionChamber.EntitySystems;

public sealed partial class ReactionChamberSystem : EntitySystem
{
    const float ReactionChamberTreshold = 0.005f;
    [Dependency] readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] readonly AppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ReactionChamberComponent, SolutionContainerChangedEvent>(UpdateUiState);
        // SubscribeLocalEvent<ReactionChamberComponent, OnTemperatureChangeEvent>(UpdateUiState);
        SubscribeLocalEvent<ReactionChamberComponent, ComponentInit>(UpdateUiState);
        SubscribeLocalEvent<ReactionChamberComponent, EntInsertedIntoContainerMessage>(UpdateUiState);
        SubscribeLocalEvent<ReactionChamberComponent, EntRemovedFromContainerMessage>(UpdateUiStateNull);
        SubscribeLocalEvent<ReactionChamberComponent, ReactionChamberActiveChangeMessage>(OnActiveChangeMessage);
        SubscribeLocalEvent<ReactionChamberComponent, ReactionChamberTempChangeMessage>(OnTempChangeMessage);
        SubscribeLocalEvent<ReactionChamberComponent, BoundUIOpenedEvent>(UpdateUiState);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<ReactionChamberComponent>();
        while (query.MoveNext(out var uid, out ReactionChamberComponent? comp))
        {
            if (!comp.Active)
                continue;
            comp.IsAllTempRight = false;
            var beaker = _itemSlotsSystem.GetItemOrNull(uid, "beakerSlot");
            if (beaker is null)
                continue;
            if (!TryComp<SolutionContainerManagerComponent>(beaker, out var container))
                continue;
            if (!comp.IsAllTempRight)
                foreach (var (_, soln) in _solutionContainerSystem.EnumerateSolutions(beaker.Value)) // add temp to all solutions
                {
                    var (_, solnComp) = soln;
                    if (comp.SolnHeatCapacity == 0)
                        comp.SolnHeatCapacity = 0.1f;
                    comp.SolnHeatCapacity = solnComp.Solution.GetHeatCapacity(PrototypeManager); // used to get real capacity, and therefore real soln temp
                    var solnTemp = solnComp.Solution.GetThermalEnergy(PrototypeManager) / comp.SolnHeatCapacity;
                    var deltaT = comp.Temp - solnTemp;
                    UpdateDeltaJ(uid, deltaT, frameTime, comp);
                    if (solnTemp != comp.Temp)
                    {
                        _solutionContainerSystem.AddThermalEnergy(soln, (float) comp.DeltaJ);
                        comp.IsAllTempRight = false;
                        var t = new ReactionChamberTempChangeMessage(solnTemp);
                        UpdateUiState(new Entity<ReactionChamberComponent>(uid, comp), ref t);
                        if (Math.Abs(comp.DeltaJ) <= ReactionChamberTreshold)
                        {
                            _solutionContainerSystem.SetTemperature(soln, comp.Temp); // optymizacija included :)
                            comp.IsAllTempRight = true;
                            break;
                        }
                    }
                    else
                    {
                        comp.IsAllTempRight = true;
                        break;
                    }
                }
        }
    }
    // void OnItemInsert<T>(Entity<ReactionChamberComponent> ent, ref T ev)
    // {
    //     UpdateUiState(ent, ref ev);
    // }
    public void UpdateDeltaJ(EntityUid uid, float deltaT, float frameTime, ReactionChamberComponent comp)
    {
        comp.DeltaJ = deltaT * comp.HeatCapacity * frameTime;
    }
    private void OnActiveChangeMessage(Entity<ReactionChamberComponent> ent, ref ReactionChamberActiveChangeMessage args)
    {
        ent.Comp.Active = args.Active;
        UpdateUiState(ent, ref args);
    }
    private void OnTempChangeMessage(Entity<ReactionChamberComponent> ent, ref ReactionChamberTempChangeMessage args)
    {
        ent.Comp.Temp = float.Clamp(args.Temp, ent.Comp.MinTemp, ent.Comp.MaxTemp);
        UpdateUiState(ent, ref args);
    }
    void UpdateSpriteState<T>(Entity<ReactionChamberComponent> ent, ref T ev)
    {
        if (!TryComp<AppearanceComponent>(ent.Owner, out var appearance))
            return;
        var beaker = _itemSlotsSystem.GetItemOrNull(ent, "beakerSlot");
        _appearance.SetData(ent.Owner, ReactionChamberVisuals.HasItem, beaker is not null, appearance);
    }
    void UpdateUiStateNull<T>(Entity<ReactionChamberComponent> ent, ref T ev)
    {
        UpdateSpriteState(ent, ref ev);
        _userInterfaceSystem.SetUiState(ent.Owner, ReactionChamberUiKey.Key, new ReactionChamberBoundUIState());
    }
    void UpdateUiState<T>(Entity<ReactionChamberComponent> ent, ref T ev)
    {
        UpdateSpriteState(ent, ref ev);
        var beakerInfo = new BeakerInfo();
        FixedPoint2? temp = null;
        FixedPoint2? spinBoxTemp = null;
        var beaker = _itemSlotsSystem.GetItemOrNull(ent, "beakerSlot");
        // List<string>? solutions = new List<string>();
        // List<FixedPoint2>? solutionVolumes = new List<FixedPoint2>();
        if (beaker is { Valid: true })
        {
            if (_solutionContainerSystem.TryGetFitsInDispenser(beaker.Value, out var soln, out var solution))
            {
                var heatCap = soln.Value.Comp.Solution.GetHeatCapacity(PrototypeManager);
                var thermalEnergy = soln.Value.Comp.Solution.GetThermalEnergy(PrototypeManager);
                if (heatCap > 0 && !float.IsNaN(thermalEnergy) && !float.IsNaN(heatCap))
                    temp = !float.IsNaN(thermalEnergy / heatCap) ? thermalEnergy / heatCap : null; // temp is null if there is no solution in beaker slot

                beakerInfo = new BeakerInfo
                {
                    Name = solution.Name,
                    Volume = solution.Volume,
                    MaxVolume = solution.MaxVolume,
                    Reagents = solution.Contents,
                    Temp = temp,
                    SpinBoxTemp = spinBoxTemp is null ? ent.Comp.Temp : float.Clamp(spinBoxTemp.Value.Float(), ent.Comp.MinTemp, ent.Comp.MaxTemp),
                };
            }
        }
        if (beaker is not null)
            _userInterfaceSystem.SetUiState(ent.Owner, ReactionChamberUiKey.Key, new ReactionChamberBoundUIState(new NetEntity(beaker.Value.Id), beakerInfo, ent.Comp.Active));
        else
            _userInterfaceSystem.SetUiState(ent.Owner, ReactionChamberUiKey.Key, new ReactionChamberBoundUIState(null, beakerInfo, ent.Comp.Active));

    }
}
