// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Possession;


[RegisterComponent]
public sealed partial class PossessedComponent : Component
{
    [ViewVariables]
    public EntityUid OriginalMindId;

    [ViewVariables]
    public EntityUid OriginalEntity;

    [ViewVariables]
    public EntityUid PossessorMindId;

    [ViewVariables]
    public EntityUid PossessorOriginalEntity;

    [ViewVariables]
    public TimeSpan PossessionEndTime;

    [ViewVariables]
    public TimeSpan PossessionTimeRemaining;

    [ViewVariables]
    public bool WasPacified;

    [ViewVariables]
    public bool WasWeakToHoly;

    [ViewVariables]
    public Container PossessedContainer;

    [ViewVariables]
    public readonly SoundPathSpecifier PossessionSoundPath = new ("/Audio/_Goobstation/Effects/bone_crack.ogg");
}
