// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Botany;
using Content.Server.Botany.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Botany;

/// <summary>
/// Ferments the crops sealed in a barrel into drinks, one at a time.
/// </summary>
public sealed partial class FermentingBarrelSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedSolutionContainerSystem _solution = default!;

    private const string CropSolution = "food";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FermentingBarrelComponent>();
        while (query.MoveNext(out var uid, out var barrel))
        {
            if (barrel.Open || _timing.CurTime < barrel.NextFerment)
                continue;

            barrel.NextFerment = _timing.CurTime + barrel.TimePerCrop;
            FermentNext((uid, barrel));
        }
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<FermentingBarrelComponent> ent, ref MapInitEvent args)
    {
        _appearance.SetData(ent, FermentingBarrelVisuals.Open, ent.Comp.Open);
    }

    [SubscribeLocalEvent]
    private void OnInteractHand(Entity<FermentingBarrelComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ent.Comp.Open = !ent.Comp.Open;
        ent.Comp.NextFerment = _timing.CurTime + ent.Comp.TimePerCrop;
        _appearance.SetData(ent, FermentingBarrelVisuals.Open, ent.Comp.Open);
        _popup.PopupEntity(Loc.GetString(ent.Comp.Open ? "fermenting-barrel-opened" : "fermenting-barrel-closed"), ent, args.User);
    }

    [SubscribeLocalEvent]
    private void OnInteractUsing(Entity<FermentingBarrelComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !ent.Comp.Open)
            return;

        if (HasComp<ProduceComponent>(args.Used))
        {
            args.Handled = true;
            var msg = TryAddCrop(ent, args.Used)
                ? Loc.GetString("fermenting-barrel-added", ("count", 1))
                : Loc.GetString("fermenting-barrel-full");
            _popup.PopupEntity(msg, ent, args.User);
            return;
        }

        if (!TryComp<StorageComponent>(args.Used, out var bag))
            return;

        var added = 0;
        foreach (var item in bag.Container.ContainedEntities.ToList())
        {
            if (!HasComp<ProduceComponent>(item))
                continue;

            if (!TryAddCrop(ent, item))
                break;

            added++;
        }

        if (added == 0)
            return;

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("fermenting-barrel-added", ("count", added)), ent, args.User);
    }

    [SubscribeLocalEvent]
    private void OnGetAltVerbs(Entity<FermentingBarrelComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !ent.Comp.Open || Crops(ent).ContainedEntities.Count == 0)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("fermenting-barrel-empty-verb"),
            Act = () =>
            {
                _container.EmptyContainer(Crops(ent), true);
                _popup.PopupEntity(Loc.GetString("fermenting-barrel-emptied"), ent, user);
            },
        });
    }

    [SubscribeLocalEvent]
    private void OnExamined(Entity<FermentingBarrelComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(FermentingBarrelComponent)))
        {
            args.PushMarkup(Loc.GetString(ent.Comp.Open ? "fermenting-barrel-examine-open" : "fermenting-barrel-examine-closed"));
            var waiting = Crops(ent).ContainedEntities.Count;
            if (waiting > 0)
                args.PushMarkup(Loc.GetString("fermenting-barrel-examine-waiting", ("count", waiting)));
        }
    }

    private void FermentNext(Entity<FermentingBarrelComponent> ent)
    {
        var crops = Crops(ent);
        if (crops.ContainedEntities.Count == 0
            || !_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out var barrelSolution, out var barrelContents))
            return;

        var crop = crops.ContainedEntities[0];
        var fermented = new Solution();
        if (_solution.TryGetSolution(crop, CropSolution, out var cropSolution, out var cropContents))
        {
            fermented = _solution.SplitSolution(cropSolution.Value, cropContents.Volume);
            FixedPoint2 drink = 0;
            foreach (var reagent in ent.Comp.Fermentable)
            {
                drink += fermented.RemoveReagent(reagent.Id, fermented.GetTotalPrototypeQuantity(reagent));
            }

            if (drink > 0)
                fermented.AddReagent(DrinkFor(ent, crop).Id, drink);
        }

        // No room yet: put it back and try again once some has been drawn off.
        if (fermented.Volume > barrelContents.AvailableVolume)
        {
            if (cropSolution != null)
                _solution.TryAddSolution(cropSolution.Value, fermented);
            return;
        }

        _solution.TryAddSolution(barrelSolution.Value, fermented);
        QueueDel(crop);
    }

    private ProtoId<ReagentPrototype> DrinkFor(Entity<FermentingBarrelComponent> ent, EntityUid crop)
    {
        if (MetaData(crop).EntityPrototype is { } proto && ent.Comp.Drinks.TryGetValue(proto.ID, out var drink))
            return drink;

        return ent.Comp.DefaultDrink;
    }

    /// <summary>
    /// Puts a crop in to wait, if everything waiting would still fit in the barrel once fermented.
    /// </summary>
    private bool TryAddCrop(Entity<FermentingBarrelComponent> ent, EntityUid crop)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var barrelContents))
            return false;

        var waiting = CropVolume(crop);
        foreach (var other in Crops(ent).ContainedEntities)
        {
            waiting += CropVolume(other);
        }

        if (barrelContents.Volume + waiting > barrelContents.MaxVolume)
            return false;

        return _container.Insert(crop, Crops(ent));
    }

    private FixedPoint2 CropVolume(EntityUid crop)
    {
        return _solution.TryGetSolution(crop, CropSolution, out _, out var contents) ? contents.Volume : FixedPoint2.Zero;
    }

    private Container Crops(Entity<FermentingBarrelComponent> ent)
    {
        return _container.EnsureContainer<Container>(ent, ent.Comp.Container);
    }
}
