// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 icekot8 <93311212+icekot8@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Handles sending order requests to cargo. Doesn't handle orders themselves via shuttle or telepads.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CargoOrderConsoleComponent : Component
{
    [DataField("soundError")] public SoundSpecifier ErrorSound =
        new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    [DataField("soundConfirm")]
    public SoundSpecifier ConfirmSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// All of the <see cref="CargoProductPrototype.Group"/>s that are supported.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<string> AllowedGroups = new() { "market" };

    /// <summary>
    /// Radio channel on which order approval announcements are transmitted
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<RadioChannelPrototype> AnnouncementChannel = "Supply";
}
