// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.StationEvents;

[RegisterComponent, Access(typeof(JobAddTagsRule))]
public sealed partial class JobAddTagsRuleComponent : Component
{
    [DataField(required: true)]
    public List<ProtoId<JobPrototype>> Affected = default!;

    [DataField(required: true)]
    public List<ProtoId<TagPrototype>> Tags = default!;

    /// <summary>
    /// Message to send in the affected person's chat window.
    /// </summary>
    [DataField]
    public LocId? Message;
}