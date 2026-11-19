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
    [Dependency] private IPrototypeManager _proto = default!;
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

        var query = EntityQueryEnumerator<ActiveFermentingBarrelComponent, FermentingBarrelComponent>();
        while (query.MoveNext(out var uid, out var active, out var barrel))
        {
            if (_timing.CurTime < active.NextFerment)
                continue;

            active.NextFerment = _timing.CurTime + barrel.TimePerCrop;
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

        if (IsCrop(ent, args.Used))
        {
            args.Handled = true;
            if (TryAddCrop(ent, args.Used, args.User))
                _popup.PopupEntity(Loc.GetString("fermenting-barrel-added", ("count", 1)), ent, args.User);
            return;
        }

        if (!IsBag(ent, args.Used) || !TryComp<StorageComponent>(args.Used, out var bag))
            return;

        args.Handled = true;
        var added = 0;
        foreach (var item in bag.Container.ContainedEntities.ToList())
        {
            if (!IsCrop(ent, item))
                continue;

            if (!TryAddCrop(ent, item, args.User))
                break;

            added++;
        }

        if (added > 0)
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
        EntityManager.RemoveComponents(ent, open ? ent.Comp.ClosedComponents : ent.Comp.OpenComponents);
        EntityManager.AddComponents(ent, open ? ent.Comp.OpenComponents : ent.Comp.ClosedComponents);
        _appearance.SetData(ent, FermentingBarrelVisuals.Open, open);

        if (open)
            StopFermenting(ent);
        else
            StartFermenting(ent);
    }

    private void StartFermenting(Entity<FermentingBarrelComponent> ent)
    {
        if (ent.Comp.Open || IsFull(ent) || Crops(ent).ContainedEntities.Count == 0)
            return;

        EnsureComp<ActiveFermentingBarrelComponent>(ent).NextFerment = _timing.CurTime + ent.Comp.TimePerCrop;
        _ambient.SetAmbience(ent, true);
    }

    private void StopFermenting(Entity<FermentingBarrelComponent> ent)
    {
        RemComp<ActiveFermentingBarrelComponent>(ent);
        _ambient.SetAmbience(ent, false);
    }

    private void FermentNext(Entity<FermentingBarrelComponent> ent)
    {
        var crops = Crops(ent);
        if (ent.Comp.Open || IsFull(ent) || crops.ContainedEntities.Count == 0
            || !_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out var barrelSolution, out var barrelContents))
        {
            StopFermenting(ent);
            return;
        }

        var crop = crops.ContainedEntities[0];
        if (_solution.TryGetSolution(crop, CropSolution, out var cropSolution, out var cropContents))
        {
            var fermented = _solution.SplitSolution(cropSolution.Value, cropContents.Volume);
            FixedPoint2 drink = 0;
            foreach (var reagent in FermentableIn(ent, crop))
            {
                drink += fermented.RemoveReagent(reagent.Id, fermented.GetTotalPrototypeQuantity(reagent));
            }

            if (drink > 0)
                fermented.AddReagent(DrinkFor(ent, crop).Id, drink);

            // Whatever doesn't fit is lost with the crop.
            _solution.TryAddSolution(barrelSolution.Value, fermented.SplitSolution(barrelContents.AvailableVolume));
        }

        _container.Remove(crop, crops);
        QueueDel(crop);

        if (IsFull(ent) || crops.ContainedEntities.Count == 0)
            StopFermenting(ent);
    }

    private IEnumerable<ProtoId<ReagentPrototype>> FermentableIn(Entity<FermentingBarrelComponent> ent, EntityUid crop)
    {
        if (MetaData(crop).EntityPrototype is { } proto && ent.Comp.FermentFrom.TryGetValue(proto.ID, out var reagents))
            return reagents;

        return ent.Comp.Fermentable;
    }

    private ProtoId<ReagentPrototype> DrinkFor(Entity<FermentingBarrelComponent> ent, EntityUid crop)
    {
        if (MetaData(crop).EntityPrototype is { } proto && ent.Comp.Drinks.TryGetValue(proto.ID, out var drink))
            return drink;

        return ent.Comp.DefaultDrink;
    }

    /// <summary>
    /// Puts a crop in to wait, if what's already in the barrel and waiting leaves room for it.
    /// </summary>
    private bool TryAddCrop(Entity<FermentingBarrelComponent> ent, EntityUid crop, EntityUid user)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var barrelContents))
            return false;

        var waiting = FixedPoint2.Zero;
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

    private bool IsFull(Entity<FermentingBarrelComponent> ent)
    {
        return _solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out _, out var contents) && contents.AvailableVolume <= 0;
    }

    /// <summary>
    /// Food crops go in; logs, cotton and the like don't.
    /// </summary>
    private bool IsCrop(Entity<FermentingBarrelComponent> ent, EntityUid uid)
    {
        if (!HasComp<ProduceComponent>(uid) || IsAny(uid, ent.Comp.NotCrops))
            return false;

        return HasComp<EdibleComponent>(uid) || IsAny(uid, ent.Comp.ExtraCrops);
    }

    private bool IsBag(Entity<FermentingBarrelComponent> ent, EntityUid uid)
    {
        return IsAny(uid, ent.Comp.Bags);
    }

    /// <summary>
    /// Whether the entity's prototype, or one it inherits from, is in the list.
    /// </summary>
    private bool IsAny(EntityUid uid, List<EntProtoId> ids)
    {
        if (ids.Count == 0 || MetaData(uid).EntityPrototype is not { } proto)
            return false;

        foreach (var parent in _proto.EnumerateParents<EntityPrototype>(proto, includeSelf: true))
        {
            if (ids.Contains(parent.ID))
                return true;
        }

        return false;
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
