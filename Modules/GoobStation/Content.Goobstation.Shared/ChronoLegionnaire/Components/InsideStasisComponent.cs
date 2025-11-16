// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.ChronoLegionnaire.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.ChronoLegionnaire.Components;

/// <summary>
/// Marks an entity that is under a stasis effect at the moment
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedStasisSystem))]
public sealed partial class InsideStasisComponent : Component
{
    public SoundSpecifier StasisSound = new SoundPathSpecifier("/Audio/Effects/Grenades/Supermatter/whitehole_start.ogg");

    public SoundSpecifier StasisEndSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/ChronoLegionnaire/stasis_reversed.ogg");

    /// <summary>
    /// Stasis effect on contanmend player
    /// </summary>
    [DataField("effectProto")]
    public string EffectEntityProto = "EffectStasis";

    public EntityUid Effect = new();
}

/// <summary>
/// Event when someone get inside the stasis
/// </summary>
[ByRefEvent]
public record struct StasisEvent;