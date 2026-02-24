using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Whitelist;

namespace Content.Shared._Lavaland.Weapons.Ranged.Restricted;

public sealed class MapRestrictedGunSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MapRestrictedGunComponent, AttemptShootEvent>(OnAttemptShoot);
    }

    private void OnAttemptShoot(Entity<MapRestrictedGunComponent> ent, ref AttemptShootEvent args)
    {
        var xform = Transform(ent);
        if (args.Cancelled
            || xform.MapUid == null
            || _whitelist.IsWhitelistPassOrNull(ent.Comp.PlanetWhitelist, xform.MapUid.Value)
            && _whitelist.IsBlacklistFailOrNull(ent.Comp.PlanetBlacklist, xform.MapUid.Value))
            return;

        args.Cancelled = true;
        if (ent.Comp.PopupOnBlock != null)
            args.Message = Loc.GetString(ent.Comp.PopupOnBlock);
    }
}
