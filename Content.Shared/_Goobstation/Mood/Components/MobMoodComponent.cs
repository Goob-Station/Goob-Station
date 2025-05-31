// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Mood.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Mood.Components;

[RegisterComponent, NetworkedComponent(), AutoGenerateComponentState]
public sealed partial class MobMoodComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<MobMoodletPrototype> MobMoods { get; set; } = new();
}
