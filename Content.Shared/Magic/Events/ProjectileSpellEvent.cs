// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Magic.Events;

[Virtual]
public partial class ProjectileSpellEvent : WorldTargetActionEvent // Goob edit
{
    /// <summary>
    /// What entity should be spawned.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype;

    // Goobstation
    [DataField]
    public float Speed = 40f;
}
