// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goidastation.Common.Stunnable;
using Content.Goidastation.Shared.Stunnable;

namespace Content.Goidastation.Shared.Stun;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedGoidaStunSystem : EntitySystem
{
    [Dependency] private readonly ClothingModifyStunTimeSystem _modifySystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GetClothingStunModifierEvent>(HandleGetClothingStunModifier);
    }

    private void HandleGetClothingStunModifier(GetClothingStunModifierEvent ev)
    {
        Log.Info("Handling Get Clothing Stun Modify");
        ev.Modifier = _modifySystem.GetModifier(ev.Target);
    }
}