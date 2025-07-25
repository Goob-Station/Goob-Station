// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Events;
using Content.Goobstation.Shared.Werewolf.Systems;
using Content.Server.Humanoid;
using Content.Server.Polymorph.Systems;

namespace Content.Goobstation.Server.Werewolf.Systems;

/// <summary>
/// This handles the transformation of the werewolf
/// </summary>
public sealed class WerewolfTransformSystem : SharedWerewolfTransformSystem
{
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfTransformComponent, WerewolfTransformEvent>(OnWerewolfTransform);
    }

    private void OnWerewolfTransform(Entity<WerewolfTransformComponent> ent, ref WerewolfTransformEvent args)
    {
        var config = GetPolymorphConfig(ent);
        if (config == null)
            return;

        var newUid = _polymorphSystem.PolymorphEntity(ent.Owner, config);
        if (newUid == null)
            return;

        _humanoid.SetSkinColor(newUid.Value, GetFurColor(ent));
    }
}
