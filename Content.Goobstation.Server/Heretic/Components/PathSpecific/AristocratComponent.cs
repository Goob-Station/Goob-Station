// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Heretic.Components.PathSpecific;

/// <summary>
/// This marks an ascended void heretic.
/// </summary>
[RegisterComponent]
public sealed partial class AristocratComponent : Component
{
    [DataField]
    public SoundSpecifier VoidsEmbrace = new SoundPathSpecifier("/Audio/_Goobstation/Heretic/Ambience/Antag/Heretic/VoidsEmbrace.ogg");

    [DataField]
    public float UpdateDelay = 0.1f;

    [DataField]
    public float Range = 10f;

    [ViewVariables(VVAccess.ReadOnly)]
    public int UpdateStep = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public float UpdateTimer = 0f;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool HasDied = false;

}
