// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Explosion.EntitySystems;

namespace Content.Server.Explosion.Components;

/// <summary>
/// Deletes entity parent on <see cref="TriggerEvent"/>
/// </summary>
[RegisterComponent]
public sealed partial class DeleteParentOnTriggerComponent : Component
{
}
