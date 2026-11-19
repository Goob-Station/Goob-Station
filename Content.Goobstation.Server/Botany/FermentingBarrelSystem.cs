// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Botany;
using Content.Server.Botany.Components;
using Content.Shared.Audio;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Storage;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
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
    [Dependency] private SharedAmbientSoundSystem _ambient = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
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
        SetLid(ent, ent.Comp.Open);
    }

    [SubscribeLocalEvent]
    private void OnInteractHand(Entity<FermentingBarrelComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        SetLid(ent, !ent.Comp.Open);
        _audio.PlayPvs(ent.Comp.Open ? ent.Comp.OpenSound : ent.Comp.CloseSound, ent);
        _popup.PopupEntity(Loc.GetString(ent.Comp.Open ? "fermenting-barrel-opened" : "fermenting-barrel-closed"), ent, args.User);
    }

    [SubscribeLocalEvent]
    private void OnInteractUsing(Entity<FermentingBarrelComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !ent.Comp.Open)
            return;

        if (IsCrop(args.Used))
        {
            args.Handled = true;
            if (TryAddCrop(ent, args.Used, args.User))
                _popup.PopupEntity(Loc.GetString("fermenting-barrel-added", ("count", 1)), ent, args.User);
            return;
        }

        if (!TryComp<StorageComponent>(args.Used, out var bag))
            return;

        var added = 0;
        foreach (var item in bag.Container.ContainedEntities.ToList())
        {
            if (!IsCrop(item))
                continue;

            if (!TryAddCrop(ent, item, args.User))
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
            if (ent.Comp.Open && waiting > 0)
                args.PushMarkup(Loc.GetString("fermenting-barrel-examine-waiting", ("count", waiting)));
        }
    }

    /// <summary>
    /// The lid decides which way liquid goes: in while it's off, out of the tap while it's on.
    /// </summary>
    private void SetLid(Entity<FermentingBarrelComponent> ent, bool open)
    {
        ent.Comp.Open = open;
        ent.Comp.NextFerment = _timing.CurTime + ent.Comp.TimePerCrop;
        EntityManager.RemoveComponents(ent, open ? ent.Comp.ClosedComponents : ent.Comp.OpenComponents);
        EntityManager.AddComponents(ent, open ? ent.Comp.OpenComponents : ent.Comp.ClosedComponents);
        _appearance.SetData(ent, FermentingBarrelVisuals.Open, open);
        UpdateBubbling(ent);
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
        }
        else
        {
            _solution.TryAddSolution(barrelSolution.Value, fermented);
            _container.Remove(crop, crops);
            QueueDel(crop);
        }

        UpdateBubbling(ent);
    }

    /// <summary>
    /// Bubbles away while it has crops to work through and room for them.
    /// </summary>
    private void UpdateBubbling(Entity<FermentingBarrelComponent> ent)
    {
        var full = _solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var contents) && contents.AvailableVolume <= 0;
        _ambient.SetAmbience(ent, !ent.Comp.Open && !full && Crops(ent).ContainedEntities.Count > 0);
    }

    private ProtoId<ReagentPrototype> DrinkFor(Entity<FermentingBarrelComponent> ent, EntityUid crop)
    {
        if (MetaData(crop).EntityPrototype is { } proto && ent.Comp.Drinks.TryGetValue(proto.ID, out var drink))
            return drink;

        return ent.Comp.DefaultDrink;
    }

    /// <summary>
    /// Puts a crop in to wait, if it can ferment and everything waiting would still fit once fermented.
    /// </summary>
    private bool TryAddCrop(Entity<FermentingBarrelComponent> ent, EntityUid crop, EntityUid user)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var barrelContents))
            return false;

        var waiting = CropVolume(crop);
        foreach (var other in Crops(ent).ContainedEntities)
        {
            waiting += CropVolume(other);
        }

        if (barrelContents.Volume + waiting > barrelContents.MaxVolume)
        {
            _popup.PopupEntity(Loc.GetString("fermenting-barrel-full"), ent, user);
            return false;
        }

        if (MetaData(crop).EntityPrototype is { } proto && ent.Comp.Unfermentable.Contains(proto.ID))
        {
            _popup.PopupEntity(Loc.GetString("fermenting-barrel-cant-ferment"), ent, user);
            return false;
        }

        return _container.Insert(crop, Crops(ent));
    }

    /// <summary>
    /// Only food crops go in; logs, cotton and nettles don't.
    /// </summary>
    private bool IsCrop(EntityUid uid)
    {
        return HasComp<ProduceComponent>(uid) && HasComp<EdibleComponent>(uid);
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
