using Content.Shared.Heretic;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Heretic;

public sealed partial class GhoulSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HereticComponent, GetStatusIconsEvent>(OnHereticMasterIcons);
        SubscribeLocalEvent<GhoulComponent, GetStatusIconsEvent>(OnGhoulIcons);
    }

    /// <summary>
    /// Show to ghouls who their master is
    /// </summary>
    private void OnHereticMasterIcons(Entity<HereticComponent> ent, ref GetStatusIconsEvent args)
    {
        var player = _player.LocalEntity;

        if (!TryComp<GhoulComponent>(player, out var playerGhoul))
            return;

        if (GetNetEntity(ent.Owner) != playerGhoul.BoundHeretic)
            return;

        if (_prototype.TryIndex(playerGhoul.MasterIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    /// <summary>
    /// Show an icon for all ghouls to all ghouls and all heretics.
    /// </summary>
    private void OnGhoulIcons(Entity<GhoulComponent> ent, ref GetStatusIconsEvent args)
    {
        var player = _player.LocalEntity;

        if (_prototype.TryIndex(ent.Comp.GhoulIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

}
