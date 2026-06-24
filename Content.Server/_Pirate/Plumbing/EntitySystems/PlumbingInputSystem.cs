using Content.Server.Popups;
using Content.Shared._Pirate.Plumbing.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Interaction;
using JetBrains.Annotations;

namespace Content.Server._Pirate.Plumbing.EntitySystems;

/// <summary>
///     Handles plumbing input machine behavior: A small tank (with no inlets) that players can pour reagents into.
/// </summary>
[UsedImplicitly]
public sealed partial class PlumbingInputSystem : EntitySystem
{
    [Dependency] private SharedSolutionContainerSystem _solutionSystem = default!;
    [Dependency] private PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlumbingInputComponent, InteractUsingEvent>(OnInputInteractUsing);
    }

    private void OnInputInteractUsing(Entity<PlumbingInputComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_solutionSystem.TryGetDrainableSolution(args.Used, out var drainableSolutionEnt, out var drainableSolution))
            return;

        if (!_solutionSystem.TryGetSolution(ent.Owner, ent.Comp.SolutionName, out var inputSolutionEnt, out var inputSolution))
            return;

        var transferAmount = drainableSolution.Volume;
        if (TryComp<SolutionTransferComponent>(args.Used, out var transferComp))
            transferAmount = FixedPoint2.Min(transferAmount, transferComp.TransferAmount);

        var space = inputSolution.AvailableVolume;
        var toTransfer = FixedPoint2.Min(transferAmount, space);

        if (toTransfer <= 0)
        {
            _popup.PopupEntity(Loc.GetString("plumbing-input-full"), ent.Owner, args.User);
            return;
        }

        var split = _solutionSystem.SplitSolution(drainableSolutionEnt.Value, toTransfer);
        _solutionSystem.TryAddSolution(inputSolutionEnt.Value, split);

        _popup.PopupEntity(Loc.GetString("plumbing-input-poured", ("amount", toTransfer)), ent.Owner, args.User);

        args.Handled = true;
    }
}
