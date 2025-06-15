// SPDX-FileCopyrightText: 2022 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Disposal.Mailing;
using Robust.Shared.GameStates;

namespace Content.Shared.Disposal.Components;

[Access(typeof(SharedMailingUnitSystem))]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MailingUnitComponent : Component
{
    /// <summary>
    /// List of targets the mailing unit can send to.
    /// Each target is just a disposal routing tag
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<string> TargetList = new();

    /// <summary>
    /// The target that gets attached to the disposal holders tag list on flush
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Target;

    /// <summary>
    /// The tag for this mailing unit
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? Tag;
}
