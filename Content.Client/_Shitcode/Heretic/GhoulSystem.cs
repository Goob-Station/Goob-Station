// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 JohnOakman <sremy2012@hotmail.fr>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 github-actions <github-actions@github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Shitcode.Heretic;

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