using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._White.ListViewSelector;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed class DefileSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DefileComponent, DefileEvent>(OnDefile);
        SubscribeLocalEvent<DefileComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DefileComponent, ListViewItemSelectedMessage>(OnDefileSelected);
    }

    private void OnMapInit(Entity<DefileComponent> ent, ref MapInitEvent args)
    {
        foreach (var reagent in ent.Comp.Reagents)
        {
            var reagentEntry = new ListViewSelectorEntry(reagent.Key.ToString(), reagent.Key.ToString());
            ent.Comp.ReagentsEntryList.Add(reagentEntry);
        }
        Dirty(ent);
    }

    private void OnDefile(Entity<DefileComponent> ent, ref DefileEvent args)
    {
        if (args.Target == ent.Owner)
        {
            _ui.SetUiState(ent.Owner, ListViewSelectorUiKey.Key, new ListViewSelectorState(ent.Comp.ReagentsEntryList));
            _ui.TryToggleUi(ent.Owner, ListViewSelectorUiKey.Key, args.Performer);
        }
        else
        {
            if (!TryInjectReagents(args.Target, ent))
                return;

            _popup.PopupClient(Loc.GetString("wraith-poison-success", ("target", ent.Owner)), ent.Owner, ent.Owner);
            args.Handled = true;
        }
    }

    private void OnDefileSelected(Entity<DefileComponent> ent, ref ListViewItemSelectedMessage args)
    {
        if (!ent.Comp.Reagents.TryGetValue(args.SelectedItem.Id, out var amount))
            return;

        ent.Comp.ReagentSelected = args.SelectedItem.Id;
        ent.Comp.ReagentSelectedAmount = amount;
        Dirty(ent);

        _ui.CloseUi(ent.Owner, ListViewSelectorUiKey.Key);
    }


    #region Helper
    private bool TryInjectReagents(EntityUid target, Entity<DefileComponent> ent)
    {
        if (!ent.Comp.ReagentSelected.HasValue)
            return false;

        var solution = new Solution();
        solution.AddReagent(ent.Comp.ReagentSelected, ent.Comp.ReagentSelectedAmount);

        if (!_solution.TryGetSolution(target, "drink", out var targetSolution) &&
            !_solution.TryGetSolution(target, "food", out targetSolution))
            return false;

        if (!TryComp<SolutionComponent>(targetSolution.Value, out var solComp))
            return false;

        // Ensure capacity is large enough before injecting
        var needed = solComp.Solution.Volume + solution.Volume;
        if (needed > solComp.Solution.MaxVolume)
        {
            solComp.Solution.MaxVolume = needed;
            Dirty(targetSolution.Value, solComp);
        }

        return _solution.TryAddSolution(targetSolution.Value, solution);
    }
    #endregion
}
