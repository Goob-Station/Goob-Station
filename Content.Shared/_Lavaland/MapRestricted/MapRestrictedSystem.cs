using Content.Shared._Lavaland.ItemUpgrades.Events;
using Content.Shared._Lavaland.MapRestricted.Components;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;

namespace Content.Shared._Lavaland.MapRestricted;

public sealed class MapRestrictedSystem : EntitySystem
{
    [Dependency] private readonly EmagSystem _emag = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MapRestrictedUpgradeComponent, BeforeItemUpgradeRelayEvent>(OnBeforeUpgrade);
        SubscribeLocalEvent<MapRestrictedGunComponent, AttemptShootEvent>(OnAttemptShoot);

        SubscribeLocalEvent<MapRestrictedEmaggableComponent, GotEmaggedEvent>(OnEmagged);
    }

    public bool CheckRestricted(Entity<MapRestrictedComponent?> ent)
    {
        var xform = Transform(ent);
        return Resolve(ent.Owner, ref ent.Comp)
               && (xform.MapUid != null && _whitelist.CheckBoth(xform.MapUid.Value, ent.Comp.MapBlacklist, ent.Comp.MapWhitelist)
                   || HasComp<MapRestrictedEmaggableComponent>(ent.Owner) && HasComp<EmaggedComponent>(ent.Owner));
    }

    private void OnEmagged(Entity<MapRestrictedEmaggableComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction)
            || _emag.CheckFlag(ent, EmagType.Interaction))
            return;

        args.Handled = true;
    }

    private void OnBeforeUpgrade(Entity<MapRestrictedUpgradeComponent> ent, ref BeforeItemUpgradeRelayEvent args)
        => args.Cancelled = CheckRestricted(ent.Owner);

    private void OnAttemptShoot(Entity<MapRestrictedGunComponent> ent, ref AttemptShootEvent args)
    {
        if (CheckRestricted(ent.Owner))
            return;

        args.Cancelled = true;
        if (ent.Comp.PopupOnBlock != null)
            args.Message = Loc.GetString(ent.Comp.PopupOnBlock);
    }
}
