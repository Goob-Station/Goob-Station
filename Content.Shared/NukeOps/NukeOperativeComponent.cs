// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 IntegerTempest <30300017+IntegerTempest@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.NukeOps;

/// <summary>
/// This is used for tagging a mob as a nuke operative.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NukeOperativeComponent : Component
{

    /// <summary>
    ///
    /// </summary>
    [DataField("syndStatusIcon", customTypeSerializer: typeof(PrototypeIdSerializer<FactionIconPrototype>))]
    public string SyndStatusIcon = "SyndicateFaction";
}
