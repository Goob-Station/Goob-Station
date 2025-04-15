// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Teleportation;

/// <summary>
///     Entity that will randomly teleport the user when used in hand.
/// </summary>
[RegisterComponent]
public sealed partial class RandomTeleportOnUseComponent : RandomTeleportComponent
{
    /// <summary>
    ///     Whether to consume this item on use; consumes only one if it's a stack
    /// </summary>
    [DataField] public bool ConsumeOnUse = true;
}