// SPDX-FileCopyrightText: 2025 ThunderBear2006 <bearthunder06@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Callouts;

[RegisterComponent]
public sealed partial class CalloutComponent : Component
{
    [DataField]
    public EntProtoId CalloutAction;

    [DataField]
    public EntityUid? CalloutActionEntity;

    [DataField(required: true)]
    public SoundSpecifier CalloutSpecifier;

    [DataField(required: true)]
    public string CalloutString;
}

public sealed partial class CalloutActionEvent : InstantActionEvent { }