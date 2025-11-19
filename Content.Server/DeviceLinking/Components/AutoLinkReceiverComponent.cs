// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Server.DeviceLinking.Components;

/// <summary>
/// This is used for automatic linkage with buttons and other transmitters.
/// </summary>
[RegisterComponent]
public sealed partial class AutoLinkReceiverComponent : Component
{
    [DataField("channel", required: true)]
    public string AutoLinkChannel = default!;
}

