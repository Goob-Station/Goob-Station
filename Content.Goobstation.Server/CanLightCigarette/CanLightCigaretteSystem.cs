// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.IgnitionSource;
using Content.Shared.Temperature;

namespace Content.Goobstation.Server.CanLightCigarette;

/// <summary>
/// Toggling is for losers. This bitch can ALWAYS light something.
/// </summary>
public sealed class CanLightCigaretteSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanLightCigaretteComponent, IsHotEvent>(OnIsHot);
    }

    private void OnIsHot(Entity<CanLightCigaretteComponent> ent, ref IsHotEvent args)
    {
        args.IsHot = true;
    }

}
