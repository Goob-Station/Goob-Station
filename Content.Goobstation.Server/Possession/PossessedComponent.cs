// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Possession;


[RegisterComponent]
public sealed partial class PossessedComponent : Component
{
    [DataField]
    public EntityUid OriginalMindId;

    [DataField]
    public EntityUid OriginalEntity;

    [DataField]
    public MindComponent OriginalMindComponent;

    [DataField]
    public EntityUid PossessorMindId;

    [DataField]
    public EntityUid PossessorOriginalEntity;

    [DataField]
    public MindComponent PossessorMindComponent;

    [DataField]
    public TimeSpan PossessionEndTime;

    [DataField]
    public TimeSpan PossessionTimeRemaining;

    [DataField]
    public bool WasPacified;

    [DataField]
    public bool DoPacify;

    [DataField]
    public SoundPathSpecifier PossessionSoundPath = new ("/Audio/_Goobstation/Effects/bone_crack.ogg");
}
