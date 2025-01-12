using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.Smoking;
using Content.Shared.Smoking.Components;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

/// <summary>
///  Prevents using esword or welder when off, laser when no charges.
/// </summary>
public sealed class SurgeryToolConditionsSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ItemToggleComponent, SurgeryToolUsedEvent>(OnToggleUsed);
        SubscribeLocalEvent<GunComponent, SurgeryToolUsedEvent>(OnGunUsed);
        SubscribeLocalEvent<MatchstickComponent, SurgeryToolUsedEvent>(OnMatchUsed);
    }

    private void OnToggleUsed(Entity<ItemToggleComponent> ent, ref SurgeryToolUsedEvent args)
    {
        if (ent.Comp.Activated)
            return;

        _popup.PopupEntity(Loc.GetString("surgery-tool-turn-on"), ent, args.User);
        args.Cancelled = true;
    }

    private void OnGunUsed(Entity<GunComponent> ent, ref SurgeryToolUsedEvent args)
    {
        var coords = Transform(args.User).Coordinates;
        var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), coords, args.User);
        if (ev.Ammo.Count > 0)
            return;

        _popup.PopupEntity(Loc.GetString("surgery-tool-reload"), ent, args.User);
        args.Cancelled = true;
    }

    private void OnMatchUsed(Entity<MatchstickComponent> ent, ref SurgeryToolUsedEvent args)
    {
        var state = ent.Comp.CurrentState;
        if (state == SmokableState.Lit)
            return;

        var key = "surgery-tool-match-" + (state == SmokableState.Burnt ? "replace" : "light");
        _popup.PopupEntity(Loc.GetString(key), ent, args.User);
        args.Cancelled = true;
    }
}
