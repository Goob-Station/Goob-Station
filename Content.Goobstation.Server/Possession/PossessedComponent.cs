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
    public EntityUid OriginalMindId { get; set; }

    [DataField]
    public EntityUid OriginalEntity { get; set; }

    [DataField]
    public MindComponent OriginalMindComponent { get; set; }

    [DataField]
    public EntityUid PossessorMindId { get; set; }
    [DataField]
    public EntityUid PossessorOriginalEntity { get; set; }

    [DataField]
    public MindComponent PossessorMindComponent  { get; set; }

    [DataField]
    public TimeSpan PossessionEndTime { get; set; }

    [DataField]
    public TimeSpan PossessionTimeRemaining;

    [DataField]
    public bool WasPacified;

    [DataField]
    public bool DoPacify = false;

    [DataField]
    public SoundPathSpecifier PossessionSoundPath = new ("/Audio/_Goobstation/Effects/bone_crack.ogg");
}
