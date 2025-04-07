// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Shared._EinsteinEngines.Silicon.DeadStartupButton;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class DeadStartupButtonComponent : Component
{
    [DataField("verbText")]
    public string VerbText = "dead-startup-button-verb";

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/Arcade/newgame.ogg");

    [DataField("buttonSound")]
    public SoundSpecifier ButtonSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");

    [DataField("doAfterInterval"), ViewVariables(VVAccess.ReadWrite)]
    public float DoAfterInterval = 1f;

    [DataField("buzzSound")]
    public SoundSpecifier BuzzSound = new SoundCollectionSpecifier("buzzes");

    [DataField("verbPriority"), ViewVariables(VVAccess.ReadWrite)]
    public int VerbPriority = 1;
}