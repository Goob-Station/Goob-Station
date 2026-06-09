using System.Linq;
using Content.Goobstation.Common.NTR.Scan;
using Content.Goobstation.Shared.Lathe;
using Content.Server.Chat.Systems;
using Content.Server.Lathe.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Lathe;
using Content.Shared.Materials;

namespace Content.Server.Lathe;

public sealed partial class LatheSystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;

    private void OnLatheQueueResetMessage(Entity<LatheComponent> ent, ref LatheQueueResetMessage args)
    {
        var (uid, component) = ent;
        if (component.Queue.Count > 0)
        {
            var allMaterials = component.Queue.SelectMany(q => _proto.Index(q).Materials);
            var totalMaterials = new Dictionary<string, int>();

            foreach (var (mat, amount) in allMaterials)
            {
                totalMaterials.TryAdd(mat, 0);
                totalMaterials[mat] += amount;
            }

            if(_materialStorage.CanChangeMaterialAmount(uid, totalMaterials))
            {
                foreach (var (mat, amount) in totalMaterials)
                {
                    _materialStorage.TryChangeMaterialAmount(uid, mat, amount);
                }
                component.Queue.Clear();
            }
            else
            {
                _popup.PopupEntity(Loc.GetString("lathe-queue-reset-material-overflow"), uid);
            }
        }
        UpdateUserInterfaceState(uid, component);
    }

    /// <summary>
    /// Produces 0-time items that output into the storage automatically.
    /// Used in order to prevent stack overflows (of the server) caused by printing a lot of materials at once.
    /// TODO It's still not great and in the future should be replaced with something even more optimized.
    /// </summary>
    private void FinishProducingManyStorage(Entity<LatheComponent, LatheProducingComponent> ent)
    {
        var (uid, comp, prodComp) = ent;

        if (comp.CurrentRecipe != null)
        {
            var count = comp.Queue.Count;
            for (int i = 0; i < count + 1; i++)
            {
                // Modified FinishProducing method
                var currentRecipe = _proto.Index(comp.CurrentRecipe.Value);
                if (currentRecipe.Result is { } resultProto)
                {
                    var prototype = _proto.Index(resultProto);
                    // Storage output is already true if we are in this method
                    if (prototype.TryGetComponent<PhysicalCompositionComponent>(out var composition, _factory))
                    {
                        _materialStorage.TryChangeMaterialAmount(uid, composition.MaterialComposition);
                    }
                    else
                    {
                        // This case should ideally never happen? But whatever
                        var result = Spawn(resultProto, Transform(uid).Coordinates);
                        _stack.TryMergeToContacts(result);
                        if (TryComp<ScannableForPointsComponent>(result, out var scannable)) // Goobstation
                            scannable.Points = 0; // Goobstation, this thing is to prevent ntr duping points via an emagged lathe
                    }
                }

                if (currentRecipe.ResultReagents is { } resultReagents &&
                    comp.ReagentOutputSlotId is { } slotId)
                {
                    var toAdd = new Solution(
                        resultReagents.Select(p => new ReagentQuantity(p.Key.Id, p.Value)));

                    // dispense it in the container if we have it and dump it if we don't
                    if (_container.TryGetContainer(uid, slotId, out var container) &&
                        container.ContainedEntities.Count == 1 &&
                        _solution.TryGetFitsInDispenser(container.ContainedEntities.First(), out var solution, out _))
                    {
                        _solution.AddSolution(solution.Value, toAdd);
                    }
                    else
                    {
                        // No popup because this is a mass-produced case and we don't want 10000 popups
                        //_popup.PopupEntity(Loc.GetString("lathe-reagent-dispense-no-container", ("name", uid)), uid);
                        _puddle.TrySpillAt(uid, toAdd, out _);
                    }
                }

                // Dequeue recipes on a loop
                // We do this after the main code since the first recipe is given outside of this method
                if (!comp.Queue.TryDequeue(out var recipeProto))
                    break;

                var recipe = _proto.Index(recipeProto);
                var time = _reagentSpeed.ApplySpeed(uid, recipe.CompleteTime) * comp.TimeMultiplier;
                if (time != TimeSpan.Zero)
                    break; // Now it should be handled by another method

                comp.CurrentRecipe = recipe;
            }
        }

        comp.CurrentRecipe = null;
        prodComp.StartTime = _timing.CurTime;

        if (!TryStartProducing(uid, comp))
        {
            RemCompDeferred(uid, prodComp);
            UpdateUserInterfaceState(uid, comp);
            UpdateRunningAppearance(uid, false);
        }
    }
}
