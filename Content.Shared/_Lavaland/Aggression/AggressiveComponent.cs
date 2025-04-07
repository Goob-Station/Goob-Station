// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared._Lavaland.Aggression;

/// <summary>
///     Keeps track of whoever attacked our mob, so that it could prioritize or randomize targets.
/// </summary>
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class AggressiveComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadOnly)] public HashSet<EntityUid> Aggressors = new();

    [AutoNetworkedField]
    [DataField] public float ForgiveTime = 10f;

    [AutoNetworkedField]
    [DataField] public float ForgiveRange = 10f;
}