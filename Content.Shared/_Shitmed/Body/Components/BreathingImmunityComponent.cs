// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Shitmed.Body.Components;

/// <summary>
///     Disables a mobs need for air when this component is added.
///     It will neither breathe nor take airloss damage.
/// </summary>
[RegisterComponent]
public sealed partial class BreathingImmunityComponent : Component;