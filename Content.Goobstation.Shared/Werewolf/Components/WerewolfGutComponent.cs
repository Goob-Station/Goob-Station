// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Werewolf.Components;

[RegisterComponent]
public sealed partial class WerewolfGutComponent : Component
{
    /// <summary>
    ///  Whitelist of allowed organs to remove from the body.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist;

    /// <summary>
    ///  Detects if an entity is an animal, so the victim's organs don't provide fury unless
    /// they are a humanoid species.
    /// </summary>
    [ViewVariables]
    public ProtoId<TagPrototype> VimPilotTag = "VimPilot";
}
