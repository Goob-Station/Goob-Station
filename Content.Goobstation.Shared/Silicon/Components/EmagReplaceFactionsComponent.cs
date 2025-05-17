// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Silicons.Bots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Silicon.Components;

/// <summary>
/// Replaces the entities' factions when emagged.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MedibotSystem))]
public sealed partial class EmagReplaceFactionsComponent : Component
{
    /// <summary>
    /// How long should the entity be stunned for the emagger to get out of the way? Defaults to five seconds.
    /// </summary>
    [DataField(required: false)]
    public int StunSeconds = 5;

    /// <summary>
    /// Factions to replace from the original set.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public List<string> Factions = [];

    /// <summary>
    /// Sound to play when the entity has been emagged
    /// </summary>
    [DataField]
    public SoundSpecifier SparkSound = new SoundCollectionSpecifier("sparks")
    {
        Params = AudioParams.Default.WithVolume(8f)
    };
}
