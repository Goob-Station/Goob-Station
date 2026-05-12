// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.AnimalAgeing.Components;

/// <summary>
/// Entities with this component will cancel the age up event and will not age
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AgelessComponent : Component;
