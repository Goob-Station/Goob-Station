using System.Diagnostics.CodeAnalysis;
using Content.Shared._EinsteinEngines.Power.Components;
using Content.Shared._EinsteinEngines.Power.Systems;
using Content.Shared._EinsteinEngines.Silicon.Charge;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.PowerCell.Components;
using Content.Shared.Verbs;
using Content.Shared.Whitelist;
using Robust.Shared.Utility;

namespace Content.Client._EinsteinEngines.Power.Systems;

// Goobstation - Energycrit
/// <summary>
///     Client-side prediction for BatteryDrinkerSystem.
/// </summary>
/// <remarks>
///     For some reason, the battery drinking system has a feature letting you drink from anything
///     with a BatteryComponent, this means that all the logic for figuring out what you can drink
///     or not has to be entirely serverside. The feature isn't even used anywhere. It was briefly
///     going to be used in energycrit, but I very quickly realized that it was incredibly broken
///     and shouldn't have existed in the first place. Because of this, the logic has to be copied
///     and shoved into the client too!
/// </remarks>
public sealed class BatteryDrinkerSystem : SharedBatteryDrinkerSystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BatteryDrinkerSourceComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<PowerCellSlotComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    /// <summary>
    ///     *Good enough* prediction for the battery drinking verb.
    /// </summary>
    private void OnGetVerbs<TComp>(Entity<TComp> ent, ref GetVerbsEvent<AlternativeVerb> args)
        where TComp : Component
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<BatteryDrinkerComponent>(args.User, out var drinker) ||
            _whitelist.IsBlacklistPass(drinker.Blacklist, ent) ||
            !SearchForDrinker(args.User, out _) ||
            !SearchForSource(ent, out _))
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("battery-drinker-verb-drink"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/smite.svg.192dpi.png")),
            Priority = -5
        };

        args.Verbs.Add(verb);
    }

    private bool SearchForSource(EntityUid ent, [NotNullWhen(true)] out EntityUid? source)
    {
        // Are we a battery drinker source
        if (HasComp<BatteryDrinkerSourceComponent>(ent))
        {
            source = ent;
            return true;
        }

        // Do we contain a source?
        if (TryComp<PowerCellSlotComponent>(ent, out var slotId) &&
            HasComp<ItemSlotsComponent>(ent) &&
            _itemSlots.TryGetSlot(ent, slotId.CellSlotId, out var slot) &&
            slot.HasItem && HasComp<BatteryDrinkerSourceComponent>(slot.Item))
        {
            source = slot.Item;
            return true;
        }

        // We found nothing
        source = null;
        return false;
    }

    private bool SearchForDrinker(EntityUid ent, [NotNullWhen(true)] out EntityUid? drinker)
    {
        drinker = null;

        // Do we have a power cell slot
        if (TryComp<PowerCellSlotComponent>(ent, out var slotId))
        {
            // Do we have a battery to charge?
            if (HasComp<ItemSlotsComponent>(ent) &&
                _itemSlots.TryGetSlot(ent, slotId.CellSlotId, out var slot) &&
                slot.HasItem && HasComp<BatteryDrinkerSourceComponent>(slot.Item))
            {
                drinker = slot.Item;
                return true;
            }

            // We don't have a battery to charge
            return false;
        }

        // We don't have a power cell slot, assume it's inside us
        drinker = ent;
        return true;
    }
}
