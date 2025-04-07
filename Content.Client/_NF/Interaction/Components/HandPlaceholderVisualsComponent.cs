// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._NF.Interaction.Components;

[RegisterComponent]
// Client-side component of the HandPlaceholder. Creates and tracks a client-side entity for hand blocking visuals
public sealed partial class HandPlaceholderVisualsComponent : Component
{
    [DataField]
    public EntityUid Dummy;
}
