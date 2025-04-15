// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <6192499+Jezithyr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2023 qwerltaz <69696513+qwerltaz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.IgnitionSource;

/// <summary>
/// This is used for creating atmosphere hotspots while ignited to start reactions such as fire.
/// </summary>
[RegisterComponent, Access(typeof(IgnitionSourceSystem))]
public sealed partial class IgnitionSourceComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Ignited;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Temperature = 700;
}